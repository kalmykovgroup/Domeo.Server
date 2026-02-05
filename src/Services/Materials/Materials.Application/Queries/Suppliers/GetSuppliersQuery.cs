using Materials.Contracts.DTOs;
using MediatR;

namespace Materials.Application.Queries.Suppliers;

public sealed record GetSuppliersQuery(bool? ActiveOnly) : IRequest<List<SupplierDto>>;
