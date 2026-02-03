using Domeo.Shared.Auth;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projects.API.Contracts;
using Projects.API.Entities;
using Projects.API.Persistence;

namespace Projects.API.Endpoints;

public static class ProjectsEndpoints
{
    public static void MapProjectsEndpoints(this IEndpointRouteBuilder app)
    {
        // Projects
        var projects = app.MapGroup("/projects").WithTags("Projects");
        projects.MapGet("/", GetProjects).RequireAuthorization("Permission:projects:read");
        projects.MapGet("/{id:guid}", GetProject).RequireAuthorization("Permission:projects:read");
        projects.MapPost("/", CreateProject).RequireAuthorization("Permission:projects:write");
        projects.MapPut("/{id:guid}", UpdateProject).RequireAuthorization("Permission:projects:write");
        projects.MapPut("/{id:guid}/status", UpdateProjectStatus).RequireAuthorization("Permission:projects:write");
        projects.MapPut("/{id:guid}/questionnaire", UpdateQuestionnaire).RequireAuthorization("Permission:projects:write");
        projects.MapDelete("/{id:guid}", DeleteProject).RequireAuthorization("Permission:projects:delete");

        // Rooms
        var rooms = app.MapGroup("/projects/{projectId:guid}/rooms").WithTags("Rooms");
        rooms.MapGet("/", GetRooms).RequireAuthorization("Permission:projects:read");
        rooms.MapGet("/{roomId:guid}", GetRoom).RequireAuthorization("Permission:projects:read");
        rooms.MapPost("/", CreateRoom).RequireAuthorization("Permission:projects:write");
        rooms.MapPut("/{roomId:guid}", UpdateRoom).RequireAuthorization("Permission:projects:write");
        rooms.MapDelete("/{roomId:guid}", DeleteRoom).RequireAuthorization("Permission:projects:delete");

        // Room Vertices
        var vertices = app.MapGroup("/rooms/{roomId:guid}/vertices").WithTags("Room Vertices");
        vertices.MapGet("/", GetRoomVertices).RequireAuthorization("Permission:projects:read");
        vertices.MapPost("/", CreateRoomVertex).RequireAuthorization("Permission:projects:write");
        vertices.MapDelete("/{vertexId:guid}", DeleteRoomVertex).RequireAuthorization("Permission:projects:delete");

        // Room Edges
        var edges = app.MapGroup("/rooms/{roomId:guid}/edges").WithTags("Room Edges");
        edges.MapGet("/", GetRoomEdges).RequireAuthorization("Permission:projects:read");
        edges.MapPost("/", CreateRoomEdge).RequireAuthorization("Permission:projects:write");
        edges.MapPut("/{edgeId:guid}", UpdateRoomEdge).RequireAuthorization("Permission:projects:write");
        edges.MapDelete("/{edgeId:guid}", DeleteRoomEdge).RequireAuthorization("Permission:projects:delete");

        // Zones
        var zones = app.MapGroup("/edges/{edgeId:guid}/zones").WithTags("Zones");
        zones.MapGet("/", GetZones).RequireAuthorization("Permission:projects:read");
        zones.MapPost("/", CreateZone).RequireAuthorization("Permission:projects:write");
        zones.MapPut("/{zoneId:guid}", UpdateZone).RequireAuthorization("Permission:projects:write");
        zones.MapDelete("/{zoneId:guid}", DeleteZone).RequireAuthorization("Permission:projects:delete");
    }

    // Projects
    private static async Task<IResult> GetProjects(
        [FromQuery] Guid? clientId,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        ICurrentUserAccessor currentUserAccessor,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse<PaginatedResponse<ProjectDto>>.Fail("Unauthorized"));

        var query = dbContext.Projects
            .Where(p => p.UserId == userId && p.DeletedAt == null);

        // Client filter
        if (clientId.HasValue)
            query = query.Where(p => p.ClientId == clientId.Value);

        // Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower));
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ProjectStatus>(status, true, out var statusEnum))
            query = query.Where(p => p.Status == statusEnum);

        // Type filter
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(p => p.Type == type);

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            "createdat" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            "updatedat" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(p => p.UpdatedAt)
                : query.OrderBy(p => p.UpdatedAt),
            "status" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(p => p.Status)
                : query.OrderBy(p => p.Status),
            _ => query.OrderByDescending(p => p.UpdatedAt)
        };

        // Pagination
        var currentPage = page ?? 1;
        var currentPageSize = pageSize ?? 20;
        var total = await query.CountAsync(cancellationToken);

        var projects = await query
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Type,
                p.Status.ToString(),
                p.ClientId,
                p.UserId,
                p.Notes,
                p.QuestionnaireData,
                p.CreatedAt,
                p.UpdatedAt,
                p.DeletedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<PaginatedResponse<ProjectDto>>.Ok(
            new PaginatedResponse<ProjectDto>(total, currentPage, currentPageSize, projects)));
    }

    private static async Task<IResult> GetProject(
        Guid id,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects.FindAsync([id], cancellationToken);
        if (project is null)
            return Results.Ok(ApiResponse<ProjectDto>.Fail("Project not found"));

        return Results.Ok(ApiResponse<ProjectDto>.Ok(new ProjectDto(
            project.Id,
            project.Name,
            project.Type,
            project.Status.ToString(),
            project.ClientId,
            project.UserId,
            project.Notes,
            project.QuestionnaireData,
            project.CreatedAt,
            project.UpdatedAt,
            project.DeletedAt)));
    }

    private static async Task<IResult> CreateProject(
        [FromBody] CreateProjectRequest request,
        ICurrentUserAccessor currentUserAccessor,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse<IdResponse>.Fail("Unauthorized"));

        var project = Project.Create(request.Name, request.Type, request.ClientId, userId.Value, request.Notes);

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<IdResponse>.Ok(new IdResponse(project.Id), "Project created successfully"));
    }

    private static async Task<IResult> UpdateProject(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects.FindAsync([id], cancellationToken);
        if (project is null)
            return Results.Ok(ApiResponse.Fail("Project not found"));

        project.Update(request.Name, request.Notes);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Project updated successfully"));
    }

    private static async Task<IResult> UpdateProjectStatus(
        Guid id,
        [FromBody] UpdateStatusRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects.FindAsync([id], cancellationToken);
        if (project is null)
            return Results.Ok(ApiResponse.Fail("Project not found"));

        if (!Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
            return Results.Ok(ApiResponse.Fail("Invalid status"));

        project.UpdateStatus(status);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Status updated successfully"));
    }

    private static async Task<IResult> UpdateQuestionnaire(
        Guid id,
        [FromBody] UpdateQuestionnaireRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects.FindAsync([id], cancellationToken);
        if (project is null)
            return Results.Ok(ApiResponse.Fail("Project not found"));

        project.SetQuestionnaireData(request.QuestionnaireData);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Questionnaire updated successfully"));
    }

    private static async Task<IResult> DeleteProject(
        Guid id,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects.FindAsync([id], cancellationToken);
        if (project is null)
            return Results.Ok(ApiResponse.Fail("Project not found"));

        project.SoftDelete();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Project deleted successfully"));
    }

    // Rooms
    private static async Task<IResult> GetRooms(
        Guid projectId,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var rooms = await dbContext.Rooms
            .Where(r => r.ProjectId == projectId)
            .OrderBy(r => r.OrderIndex)
            .Select(r => new RoomDto(
                r.Id,
                r.ProjectId,
                r.Name,
                r.CeilingHeight,
                r.OrderIndex,
                r.CreatedAt,
                r.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<RoomDto>>.Ok(rooms));
    }

    private static async Task<IResult> GetRoom(
        Guid projectId,
        Guid roomId,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && r.ProjectId == projectId, cancellationToken);

        if (room is null)
            return Results.Ok(ApiResponse<RoomDto>.Fail("Room not found"));

        return Results.Ok(ApiResponse<RoomDto>.Ok(new RoomDto(
            room.Id,
            room.ProjectId,
            room.Name,
            room.CeilingHeight,
            room.OrderIndex,
            room.CreatedAt,
            room.UpdatedAt)));
    }

    private static async Task<IResult> CreateRoom(
        Guid projectId,
        [FromBody] CreateRoomRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var room = Room.Create(projectId, request.Name, request.CeilingHeight, request.OrderIndex);

        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<IdResponse>.Ok(new IdResponse(room.Id), "Room created successfully"));
    }

    private static async Task<IResult> UpdateRoom(
        Guid projectId,
        Guid roomId,
        [FromBody] UpdateRoomRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && r.ProjectId == projectId, cancellationToken);

        if (room is null)
            return Results.Ok(ApiResponse.Fail("Room not found"));

        room.Update(request.Name, request.CeilingHeight, request.OrderIndex);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Room updated successfully"));
    }

    private static async Task<IResult> DeleteRoom(
        Guid projectId,
        Guid roomId,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && r.ProjectId == projectId, cancellationToken);

        if (room is null)
            return Results.Ok(ApiResponse.Fail("Room not found"));

        dbContext.Rooms.Remove(room);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Room deleted successfully"));
    }

    // Room Vertices
    private static async Task<IResult> GetRoomVertices(
        Guid roomId,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var vertices = await dbContext.RoomVertices
            .Where(v => v.RoomId == roomId)
            .OrderBy(v => v.OrderIndex)
            .Select(v => new RoomVertexDto(v.Id, v.RoomId, v.X, v.Y, v.OrderIndex))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<RoomVertexDto>>.Ok(vertices));
    }

    private static async Task<IResult> CreateRoomVertex(
        Guid roomId,
        [FromBody] CreateRoomVertexRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var vertex = RoomVertex.Create(roomId, request.X, request.Y, request.OrderIndex);

        dbContext.RoomVertices.Add(vertex);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<IdResponse>.Ok(new IdResponse(vertex.Id), "Vertex created successfully"));
    }

    private static async Task<IResult> DeleteRoomVertex(
        Guid roomId,
        Guid vertexId,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var vertex = await dbContext.RoomVertices
            .FirstOrDefaultAsync(v => v.Id == vertexId && v.RoomId == roomId, cancellationToken);

        if (vertex is null)
            return Results.Ok(ApiResponse.Fail("Vertex not found"));

        dbContext.RoomVertices.Remove(vertex);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Vertex deleted successfully"));
    }

    // Room Edges
    private static async Task<IResult> GetRoomEdges(
        Guid roomId,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var edges = await dbContext.RoomEdges
            .Where(e => e.RoomId == roomId)
            .OrderBy(e => e.OrderIndex)
            .Select(e => new RoomEdgeDto(
                e.Id,
                e.RoomId,
                e.StartVertexId,
                e.EndVertexId,
                e.WallHeight,
                e.HasWindow,
                e.HasDoor,
                e.OrderIndex))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<RoomEdgeDto>>.Ok(edges));
    }

    private static async Task<IResult> CreateRoomEdge(
        Guid roomId,
        [FromBody] CreateRoomEdgeRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var edge = RoomEdge.Create(
            roomId,
            request.StartVertexId,
            request.EndVertexId,
            request.WallHeight,
            request.HasWindow,
            request.HasDoor,
            request.OrderIndex);

        dbContext.RoomEdges.Add(edge);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<IdResponse>.Ok(new IdResponse(edge.Id), "Edge created successfully"));
    }

    private static async Task<IResult> UpdateRoomEdge(
        Guid roomId,
        Guid edgeId,
        [FromBody] UpdateRoomEdgeRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var edge = await dbContext.RoomEdges
            .FirstOrDefaultAsync(e => e.Id == edgeId && e.RoomId == roomId, cancellationToken);

        if (edge is null)
            return Results.Ok(ApiResponse.Fail("Edge not found"));

        edge.Update(request.WallHeight, request.HasWindow, request.HasDoor, request.OrderIndex);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Edge updated successfully"));
    }

    private static async Task<IResult> DeleteRoomEdge(
        Guid roomId,
        Guid edgeId,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var edge = await dbContext.RoomEdges
            .FirstOrDefaultAsync(e => e.Id == edgeId && e.RoomId == roomId, cancellationToken);

        if (edge is null)
            return Results.Ok(ApiResponse.Fail("Edge not found"));

        dbContext.RoomEdges.Remove(edge);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Edge deleted successfully"));
    }

    // Zones
    private static async Task<IResult> GetZones(
        Guid edgeId,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var zones = await dbContext.Zones
            .Where(z => z.EdgeId == edgeId)
            .Select(z => new ZoneDto(
                z.Id,
                z.EdgeId,
                z.Name,
                z.Type.ToString(),
                z.StartX,
                z.EndX))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<ZoneDto>>.Ok(zones));
    }

    private static async Task<IResult> CreateZone(
        Guid edgeId,
        [FromBody] CreateZoneRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ZoneType>(request.Type, true, out var zoneType))
            return Results.Ok(ApiResponse<IdResponse>.Fail("Invalid zone type"));

        var zone = Zone.Create(edgeId, zoneType, request.StartX, request.EndX, request.Name);

        dbContext.Zones.Add(zone);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<IdResponse>.Ok(new IdResponse(zone.Id), "Zone created successfully"));
    }

    private static async Task<IResult> UpdateZone(
        Guid edgeId,
        Guid zoneId,
        [FromBody] UpdateZoneRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var zone = await dbContext.Zones
            .FirstOrDefaultAsync(z => z.Id == zoneId && z.EdgeId == edgeId, cancellationToken);

        if (zone is null)
            return Results.Ok(ApiResponse.Fail("Zone not found"));

        zone.Update(request.Name, request.StartX, request.EndX);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Zone updated successfully"));
    }

    private static async Task<IResult> DeleteZone(
        Guid edgeId,
        Guid zoneId,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var zone = await dbContext.Zones
            .FirstOrDefaultAsync(z => z.Id == zoneId && z.EdgeId == edgeId, cancellationToken);

        if (zone is null)
            return Results.Ok(ApiResponse.Fail("Zone not found"));

        dbContext.Zones.Remove(zone);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Zone deleted successfully"));
    }
}
