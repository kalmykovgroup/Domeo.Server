using Domeo.Shared.Kernel.Application.Abstractions;
using Materials.Abstractions.DTOs;

namespace Materials.Abstractions.Queries.Suppliers;

public sealed record GetSuppliersQuery(bool? ActiveOnly) : IQuery<List<SupplierDto>>;
