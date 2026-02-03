using Catalog.API.Contracts;
using Catalog.API.Entities;
using Catalog.API.Persistence;
using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Endpoints;

public static class SuppliersEndpoints
{
    public static void MapSuppliersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/suppliers").WithTags("Suppliers");

        // Read endpoints
        group.MapGet("/", GetSuppliers).RequireAuthorization("Permission:suppliers:read");
        group.MapGet("/{id:guid}", GetSupplier).RequireAuthorization("Permission:suppliers:read");

        // Write endpoints
        group.MapPost("/", CreateSupplier).RequireAuthorization("Permission:suppliers:write");
        group.MapPut("/{id:guid}", UpdateSupplier).RequireAuthorization("Permission:suppliers:write");

        // Delete endpoints
        group.MapDelete("/{id:guid}", DeleteSupplier).RequireAuthorization("Permission:suppliers:delete");
    }

    private static async Task<IResult> GetSuppliers(
        [FromQuery] bool? activeOnly,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Suppliers.AsQueryable();

        if (activeOnly == true)
            query = query.Where(s => s.IsActive);

        var suppliers = await query
            .Select(s => new SupplierDto(
                s.Id,
                s.Company,
                s.ContactFirstName,
                s.ContactLastName,
                s.Email,
                s.Phone,
                s.Address,
                s.IsActive))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<List<SupplierDto>>.Ok(suppliers));
    }

    private static async Task<IResult> GetSupplier(
        Guid id,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers.FindAsync([id], cancellationToken);
        if (supplier is null)
            return Results.Ok(ApiResponse<SupplierDto>.Fail("Supplier not found"));

        return Results.Ok(ApiResponse<SupplierDto>.Ok(new SupplierDto(
            supplier.Id,
            supplier.Company,
            supplier.ContactFirstName,
            supplier.ContactLastName,
            supplier.Email,
            supplier.Phone,
            supplier.Address,
            supplier.IsActive)));
    }

    private static async Task<IResult> CreateSupplier(
        [FromBody] CreateSupplierRequest request,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(),
            request.Company,
            request.ContactFirstName,
            request.ContactLastName,
            request.Email,
            request.Phone,
            request.Address);

        dbContext.Suppliers.Add(supplier);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<IdResponse>.Ok(new IdResponse(supplier.Id), "Supplier created successfully"));
    }

    private static async Task<IResult> UpdateSupplier(
        Guid id,
        [FromBody] UpdateSupplierRequest request,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers.FindAsync([id], cancellationToken);
        if (supplier is null)
            return Results.Ok(ApiResponse.Fail("Supplier not found"));

        supplier.Update(
            request.Company,
            request.ContactFirstName,
            request.ContactLastName,
            request.Email,
            request.Phone,
            request.Address);

        if (request.IsActive)
            supplier.Activate();
        else
            supplier.Deactivate();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Supplier updated successfully"));
    }

    private static async Task<IResult> DeleteSupplier(
        Guid id,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers.FindAsync([id], cancellationToken);
        if (supplier is null)
            return Results.Ok(ApiResponse.Fail("Supplier not found"));

        dbContext.Suppliers.Remove(supplier);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse.Ok("Supplier deleted successfully"));
    }
}
