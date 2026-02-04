using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.DTOs;
using Projects.API.Entities;
using Projects.API.Infrastructure.Persistence;

namespace Projects.API.Endpoints;

public static class CabinetHardwareOverrideEndpoints
{
    public static void MapCabinetHardwareOverrideEndpoints(this IEndpointRouteBuilder app)
    {
        // Nested under /cabinets/{cabinetId}/hardware-overrides
        var nested = app.MapGroup("/cabinets/{cabinetId:guid}/hardware-overrides")
            .WithTags("Cabinet Hardware Overrides");

        nested.MapGet("/", GetOverridesByCabinet).RequireAuthorization("Permission:cabinets:read");
        nested.MapPost("/", CreateOverride).RequireAuthorization("Permission:cabinets:write");

        // Direct access /cabinet-hardware-overrides/{id}
        var direct = app.MapGroup("/cabinet-hardware-overrides")
            .WithTags("Cabinet Hardware Overrides");

        direct.MapGet("/{id:guid}", GetOverride).RequireAuthorization("Permission:cabinets:read");
        direct.MapPut("/{id:guid}", UpdateOverride).RequireAuthorization("Permission:cabinets:write");
        direct.MapDelete("/{id:guid}", DeleteOverride).RequireAuthorization("Permission:cabinets:delete");
    }

    private static async Task<IResult> GetOverridesByCabinet(
        Guid cabinetId,
        ProjectsDbContext dbContext,
        CancellationToken ct)
    {
        // Check cabinet exists
        var cabinetExists = await dbContext.Cabinets.AnyAsync(c => c.Id == cabinetId, ct);
        if (!cabinetExists)
            return Results.Ok(ApiResponse<List<CabinetHardwareOverrideDto>>.Fail("Cabinet not found"));

        var overrides = await dbContext.CabinetHardwareOverrides
            .Where(o => o.CabinetId == cabinetId)
            .Select(o => new CabinetHardwareOverrideDto(
                o.Id,
                o.CabinetId,
                o.ModuleHardwareId,
                o.HardwareId,
                o.Role,
                o.QuantityFormula,
                o.PositionXFormula,
                o.PositionYFormula,
                o.PositionZFormula,
                o.IsEnabled,
                o.MaterialId,
                o.CreatedAt,
                o.UpdatedAt ?? o.CreatedAt))
            .ToListAsync(ct);

        return Results.Ok(ApiResponse<List<CabinetHardwareOverrideDto>>.Ok(overrides));
    }

    private static async Task<IResult> GetOverride(
        Guid id,
        ProjectsDbContext dbContext,
        CancellationToken ct)
    {
        var entity = await dbContext.CabinetHardwareOverrides.FindAsync([id], ct);
        if (entity is null)
            return Results.Ok(ApiResponse<CabinetHardwareOverrideDto>.Fail("Override not found"));

        return Results.Ok(ApiResponse<CabinetHardwareOverrideDto>.Ok(new CabinetHardwareOverrideDto(
            entity.Id,
            entity.CabinetId,
            entity.ModuleHardwareId,
            entity.HardwareId,
            entity.Role,
            entity.QuantityFormula,
            entity.PositionXFormula,
            entity.PositionYFormula,
            entity.PositionZFormula,
            entity.IsEnabled,
            entity.MaterialId,
            entity.CreatedAt,
            entity.UpdatedAt ?? entity.CreatedAt)));
    }

    private static async Task<IResult> CreateOverride(
        Guid cabinetId,
        [FromBody] CreateCabinetHardwareOverrideRequest request,
        ProjectsDbContext dbContext,
        CancellationToken ct)
    {
        // Check cabinet exists
        var cabinetExists = await dbContext.Cabinets.AnyAsync(c => c.Id == cabinetId, ct);
        if (!cabinetExists)
            return Results.Ok(ApiResponse<IdResponse>.Fail("Cabinet not found"));

        // Check if override already exists for this cabinet + module_hardware pair
        var existingOverride = await dbContext.CabinetHardwareOverrides
            .AnyAsync(o => o.CabinetId == cabinetId && o.ModuleHardwareId == request.ModuleHardwareId, ct);

        if (existingOverride)
            return Results.Ok(ApiResponse<IdResponse>.Fail(
                $"Override for cabinet {cabinetId} and module_hardware {request.ModuleHardwareId} already exists"));

        var entity = CabinetHardwareOverride.Create(
            cabinetId,
            request.ModuleHardwareId,
            request.IsEnabled,
            request.HardwareId,
            request.Role,
            request.QuantityFormula,
            request.PositionXFormula,
            request.PositionYFormula,
            request.PositionZFormula,
            request.MaterialId);

        dbContext.CabinetHardwareOverrides.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        return Results.Ok(ApiResponse<IdResponse>.Ok(
            new IdResponse(entity.Id),
            "Hardware override created successfully"));
    }

    private static async Task<IResult> UpdateOverride(
        Guid id,
        [FromBody] UpdateCabinetHardwareOverrideRequest request,
        ProjectsDbContext dbContext,
        CancellationToken ct)
    {
        var entity = await dbContext.CabinetHardwareOverrides.FindAsync([id], ct);
        if (entity is null)
            return Results.Ok(ApiResponse.Fail("Override not found"));

        entity.Update(
            request.IsEnabled,
            request.HardwareId,
            request.Role,
            request.QuantityFormula,
            request.PositionXFormula,
            request.PositionYFormula,
            request.PositionZFormula,
            request.MaterialId);

        await dbContext.SaveChangesAsync(ct);

        return Results.Ok(ApiResponse.Ok("Hardware override updated successfully"));
    }

    private static async Task<IResult> DeleteOverride(
        Guid id,
        ProjectsDbContext dbContext,
        CancellationToken ct)
    {
        var entity = await dbContext.CabinetHardwareOverrides.FindAsync([id], ct);
        if (entity is null)
            return Results.Ok(ApiResponse.Fail("Override not found"));

        dbContext.CabinetHardwareOverrides.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return Results.Ok(ApiResponse.Ok("Hardware override deleted successfully"));
    }
}
