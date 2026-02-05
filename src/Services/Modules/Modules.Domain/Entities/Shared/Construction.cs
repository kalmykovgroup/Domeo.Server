namespace Modules.Domain.Entities.Shared;

public sealed record Construction(
    double PanelThickness,
    double BackPanelThickness,
    double FacadeThickness,
    double FacadeGap,
    double FacadeOffset,
    double ShelfSideGap,
    double ShelfRearInset,
    double ShelfFrontInset);
