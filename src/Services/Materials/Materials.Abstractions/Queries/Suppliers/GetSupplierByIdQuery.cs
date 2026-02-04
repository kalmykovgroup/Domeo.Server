using Domeo.Shared.Kernel.Application.Abstractions;
using Materials.Abstractions.DTOs;

namespace Materials.Abstractions.Queries.Suppliers;

public sealed record GetSupplierByIdQuery(string Id) : IQuery<SupplierDto>;
