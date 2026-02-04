namespace MockSupplier.API.Models;

public sealed class Supplier
{
    public string Id { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public double Rating { get; set; } = 4.0;
    public bool IsActive { get; set; } = true;
}
