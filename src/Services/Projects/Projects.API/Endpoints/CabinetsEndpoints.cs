using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.DTOs;
using Projects.API.Entities;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Endpoints;

public static class CabinetsEndpoints
{
    public static void MapCabinetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cabinets").WithTags("Cabinets");

        // Read endpoints
        group.MapGet("/room/{roomId:guid}", GetCabinetsByRoom).RequireAuthorization("Permission:cabinets:read");
        group.MapGet("/{id:guid}", GetCabinet).RequireAuthorization("Permission:cabinets:read");

        // Write endpoints
        group.MapPost("/", CreateCabinet).RequireAuthorization("Permission:cabinets:write");
        group.MapPut("/{id:guid}", UpdateCabinet).RequireAuthorization("Permission:cabinets:write");

        // Delete endpoints
        group.MapDelete("/{id:guid}", DeleteCabinet).RequireAuthorization("Permission:cabinets:delete");
    }

    private static async Task<IResult> GetCabinetsByRoom(
        Guid roomId,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var cabinets = await dbContext.Cabinets
            .Where(c => c.RoomId == roomId)
            .Select(c => new CabinetDto(
                c.Id,
                c.RoomId,
                c.EdgeId,
                c.ZoneId,
                c.ModuleTypeId,
                c.Name,
                c.PlacementType,
                c.FacadeType,
                c.PositionX,
                c.PositionY,
                c.Rotation,
                c.Width,
                c.Height,
                c.Depth,
                c.CalculatedPrice,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<CabinetDto>>.Ok(cabinets));
    }

    private static async Task<IResult> GetCabinet(
        Guid id,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var cabinet = await dbContext.Cabinets.FindAsync([id], cancellationToken);
        if (cabinet is null)
            return Results.Ok(ApiResponse<CabinetDto>.Fail("Cabinet not found"));

        return Results.Ok(ApiResponse<CabinetDto>.Ok(new CabinetDto(
            cabinet.Id,
            cabinet.RoomId,
            cabinet.EdgeId,
            cabinet.ZoneId,
            cabinet.ModuleTypeId,
            cabinet.Name,
            cabinet.PlacementType,
            cabinet.FacadeType,
            cabinet.PositionX,
            cabinet.PositionY,
            cabinet.Rotation,
            cabinet.Width,
            cabinet.Height,
            cabinet.Depth,
            cabinet.CalculatedPrice,
            cabinet.CreatedAt)));
    }

    private static async Task<IResult> CreateCabinet(
        [FromBody] CreateCabinetRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var cabinet = Cabinet.Create(
            request.RoomId,
            request.PlacementType,
            request.PositionX,
            request.PositionY,
            request.Width,
            request.Height,
            request.Depth,
            request.Name);

        if (request.EdgeId.HasValue)
            cabinet.SetEdge(request.EdgeId);
        if (request.ZoneId.HasValue)
            cabinet.SetZone(request.ZoneId);
        if (request.ModuleTypeId.HasValue)
            cabinet.SetModuleType(request.ModuleTypeId);
        if (request.FacadeType is not null)
            cabinet.SetFacadeType(request.FacadeType);
        if (request.Rotation != 0)
            cabinet.UpdatePosition(request.PositionX, request.PositionY, request.Rotation);

        dbContext.Cabinets.Add(cabinet);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<IdResponse>.Ok(new IdResponse(cabinet.Id), "Cabinet created successfully"));
    }

    private static async Task<IResult> UpdateCabinet(
        Guid id,
        [FromBody] UpdateCabinetRequest request,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var cabinet = await dbContext.Cabinets.FindAsync([id], cancellationToken);
        if (cabinet is null)
            return Results.Ok(ApiResponse.Fail("Cabinet not found"));

        cabinet.UpdatePosition(request.PositionX, request.PositionY, request.Rotation);
        cabinet.UpdateDimensions(request.Width, request.Height, request.Depth);
        cabinet.SetName(request.Name);
        cabinet.SetEdge(request.EdgeId);
        cabinet.SetZone(request.ZoneId);
        cabinet.SetModuleType(request.ModuleTypeId);
        cabinet.SetFacadeType(request.FacadeType);
        cabinet.SetCalculatedPrice(request.CalculatedPrice);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Cabinet updated successfully"));
    }

    private static async Task<IResult> DeleteCabinet(
        Guid id,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var cabinet = await dbContext.Cabinets.FindAsync([id], cancellationToken);
        if (cabinet is null)
            return Results.Ok(ApiResponse.Fail("Cabinet not found"));

        dbContext.Cabinets.Remove(cabinet);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Cabinet deleted successfully"));
    }
}
