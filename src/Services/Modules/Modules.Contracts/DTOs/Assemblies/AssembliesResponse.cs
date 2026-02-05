namespace Modules.Contracts.DTOs.Assemblies;

public sealed record AssembliesResponse(
    List<AssemblyDto> Items,
    int? Total = null,
    int? Page = null,
    int? Limit = null)
{
    public bool IsPaginated => Total.HasValue;
}
