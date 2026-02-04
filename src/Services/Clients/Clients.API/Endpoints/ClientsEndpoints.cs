using Clients.API.Contracts;
using Clients.API.Entities;
using Clients.API.Persistence;
using Domeo.Shared.Auth;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clients.API.Endpoints;

public static class ClientsEndpoints
{
    public static void MapClientsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/clients").WithTags("Clients");

        // Read endpoints
        group.MapGet("/", GetClients).RequireAuthorization("Permission:clients:read");
        group.MapGet("/{id:guid}", GetClient).RequireAuthorization("Permission:clients:read");

        // Write endpoints
        group.MapPost("/", CreateClient).RequireAuthorization("Permission:clients:write");
        group.MapPut("/{id:guid}", UpdateClient).RequireAuthorization("Permission:clients:write");

        // Restore endpoint
        group.MapPost("/{id:guid}/restore", RestoreClient).RequireAuthorization("Permission:clients:write");

        // Delete endpoints (soft delete)
        group.MapDelete("/{id:guid}", DeleteClient).RequireAuthorization("Permission:clients:delete");
    }

    private static async Task<IResult> GetClients(
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        ICurrentUserAccessor currentUserAccessor,
        ClientsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse<PaginatedResponse<ClientDto>>.Fail("Unauthorized"));

        var query = dbContext.Clients
            .Where(c => c.UserId == userId && c.DeletedAt == null);

        // Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchLower) ||
                (c.Phone != null && c.Phone.Contains(search)) ||
                (c.Email != null && c.Email.ToLower().Contains(searchLower)));
        }

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "createdat" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        // Pagination
        var currentPage = page ?? 1;
        var currentPageSize = pageSize ?? 20;
        var total = await query.CountAsync(cancellationToken);

        var clients = await query
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(c => new ClientDto(
                c.Id,
                c.Name,
                c.Phone,
                c.Email,
                c.Address,
                c.Notes,
                c.UserId,
                c.CreatedAt,
                c.DeletedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<PaginatedResponse<ClientDto>>.Ok(
            new PaginatedResponse<ClientDto>(total, currentPage, currentPageSize, clients)));
    }

    private static async Task<IResult> GetClient(
        Guid id,
        ICurrentUserAccessor currentUserAccessor,
        ClientsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse<ClientDto>.Fail("Unauthorized"));

        var client = await dbContext.Clients.FindAsync([id], cancellationToken);
        if (client is null)
            return Results.Ok(ApiResponse<ClientDto>.Fail("Client not found"));

        if (client.UserId != userId)
            return Results.Ok(ApiResponse<ClientDto>.Fail("Access denied"));

        return Results.Ok(ApiResponse<ClientDto>.Ok(new ClientDto(
            client.Id,
            client.Name,
            client.Phone,
            client.Email,
            client.Address,
            client.Notes,
            client.UserId,
            client.CreatedAt,
            client.DeletedAt)));
    }

    private static async Task<IResult> CreateClient(
        [FromBody] CreateClientRequest request,
        ICurrentUserAccessor currentUserAccessor,
        ClientsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse<ClientDto>.Fail("Unauthorized"));

        var client = Client.Create(
            request.Name,
            userId.Value,
            request.Phone,
            request.Email,
            request.Address,
            request.Notes);

        dbContext.Clients.Add(client);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<ClientDto>.Ok(new ClientDto(
            client.Id,
            client.Name,
            client.Phone,
            client.Email,
            client.Address,
            client.Notes,
            client.UserId,
            client.CreatedAt,
            client.DeletedAt), "Client created successfully"));
    }

    private static async Task<IResult> UpdateClient(
        Guid id,
        [FromBody] UpdateClientRequest request,
        ICurrentUserAccessor currentUserAccessor,
        ClientsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse<ClientDto>.Fail("Unauthorized"));

        var client = await dbContext.Clients.FindAsync([id], cancellationToken);
        if (client is null)
            return Results.Ok(ApiResponse<ClientDto>.Fail("Client not found"));

        if (client.UserId != userId)
            return Results.Ok(ApiResponse<ClientDto>.Fail("Access denied"));

        client.Update(request.Name, request.Phone, request.Email, request.Address, request.Notes);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<ClientDto>.Ok(new ClientDto(
            client.Id,
            client.Name,
            client.Phone,
            client.Email,
            client.Address,
            client.Notes,
            client.UserId,
            client.CreatedAt,
            client.DeletedAt), "Client updated successfully"));
    }

    private static async Task<IResult> DeleteClient(
        Guid id,
        ICurrentUserAccessor currentUserAccessor,
        ClientsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse.Fail("Unauthorized"));

        var client = await dbContext.Clients.FindAsync([id], cancellationToken);
        if (client is null)
            return Results.Ok(ApiResponse.Fail("Client not found"));

        if (client.UserId != userId)
            return Results.Ok(ApiResponse.Fail("Access denied"));

        client.SoftDelete();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Client deleted successfully"));
    }

    private static async Task<IResult> RestoreClient(
        Guid id,
        ICurrentUserAccessor currentUserAccessor,
        ClientsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.User?.Id;
        if (userId is null)
            return Results.Ok(ApiResponse<ClientDto>.Fail("Unauthorized"));

        var client = await dbContext.Clients.FindAsync([id], cancellationToken);
        if (client is null)
            return Results.Ok(ApiResponse<ClientDto>.Fail("Client not found"));

        if (client.UserId != userId)
            return Results.Ok(ApiResponse<ClientDto>.Fail("Access denied"));

        if (!client.IsDeleted)
            return Results.Ok(ApiResponse<ClientDto>.Fail("Client is not deleted"));

        client.Restore();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<ClientDto>.Ok(new ClientDto(
            client.Id,
            client.Name,
            client.Phone,
            client.Email,
            client.Address,
            client.Notes,
            client.UserId,
            client.CreatedAt,
            client.DeletedAt), "Client restored successfully"));
    }
}
