using Materials.Contracts.DTOs;
using MediatR;

namespace Materials.Application.Queries.Suppliers;

public sealed record GetSupplierByIdQuery(string Id) : IRequest<SupplierDto>;
