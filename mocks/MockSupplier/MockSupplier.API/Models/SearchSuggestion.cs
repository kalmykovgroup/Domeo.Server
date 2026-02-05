namespace MockSupplier.API.Models;

public sealed class SearchSuggestion
{
    public string Type { get; set; } = "";      // "material" | "brand" | "category" | "attribute"
    public string Text { get; set; } = "";      // Отображаемый текст
    public double Score { get; set; }            // Релевантность (выше = лучше)
    public Dictionary<string, object?> Metadata { get; set; } = new();
}
