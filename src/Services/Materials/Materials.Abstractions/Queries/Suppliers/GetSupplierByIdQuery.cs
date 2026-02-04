using Materials.Abstractions.DTOs;
using MediatR;

namespace Materials.Abstractions.Queries.Suppliers;

public sealed record GetSupplierByIdQuery(string Id) : IRequest<SupplierDto>;
