using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Catalog.API.Entities;

public sealed class ModuleType : Entity<int>
{
    public string CategoryId { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    // Width dimensions
    public int WidthDefault { get; private set; }
    public int WidthMin { get; private set; }
    public int WidthMax { get; private set; }

    // Height dimensions
    public int HeightDefault { get; private set; }
    public int HeightMin { get; private set; }
    public int HeightMax { get; private set; }

    // Depth dimensions
    public int DepthDefault { get; private set; }
    public int DepthMin { get; private set; }
    public int DepthMax { get; private set; }

    // Panel thicknesses
    public int PanelThickness { get; private set; } = 16;
    public int BackPanelThickness { get; private set; } = 4;
    public int FacadeThickness { get; private set; } = 18;

    // Gaps and offsets
    public int FacadeGap { get; private set; } = 2;
    public int FacadeOffset { get; private set; }
    public int ShelfSideGap { get; private set; } = 2;
    public int ShelfRearInset { get; private set; } = 20;
    public int ShelfFrontInset { get; private set; } = 10;

    public bool IsActive { get; private set; } = true;

    private ModuleType() { }

    public static ModuleType Create(
        int id,
        string categoryId,
        string type,
        string name,
        int widthDefault, int widthMin, int widthMax,
        int heightDefault, int heightMin, int heightMax,
        int depthDefault, int depthMin, int depthMax,
        int panelThickness = 16,
        int backPanelThickness = 4,
        int facadeThickness = 18,
        int facadeGap = 2,
        int facadeOffset = 0,
        int shelfSideGap = 2,
        int shelfRearInset = 20,
        int shelfFrontInset = 10)
    {
        return new ModuleType
        {
            Id = id,
            CategoryId = categoryId,
            Type = type,
            Name = name,
            WidthDefault = widthDefault,
            WidthMin = widthMin,
            WidthMax = widthMax,
            HeightDefault = heightDefault,
            HeightMin = heightMin,
            HeightMax = heightMax,
            DepthDefault = depthDefault,
            DepthMin = depthMin,
            DepthMax = depthMax,
            PanelThickness = panelThickness,
            BackPanelThickness = backPanelThickness,
            FacadeThickness = facadeThickness,
            FacadeGap = facadeGap,
            FacadeOffset = facadeOffset,
            ShelfSideGap = shelfSideGap,
            ShelfRearInset = shelfRearInset,
            ShelfFrontInset = shelfFrontInset,
            IsActive = true
        };
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
