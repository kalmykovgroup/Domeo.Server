using Materials.Abstractions.DTOs;
using MediatR;

namespace Materials.Abstractions.Queries.Suppliers;

public sealed record GetSuppliersQuery(bool? ActiveOnly) : IRequest<List<SupplierDto>>;
