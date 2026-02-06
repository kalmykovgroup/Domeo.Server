using System.Text.Json.Serialization;

namespace Modules.Domain.Entities.Shared;

[JsonConverter(typeof(JsonStringEnumConverter<DimensionSource>))]
public enum DimensionSource
{
    ParentWidth,
    ParentDepth,
    ParentHeight,
    Fixed
}

[JsonConverter(typeof(JsonStringEnumConverter<AnchorOrigin>))]
public enum AnchorOrigin
{
    Start,
    Center,
    End
}

[JsonConverter(typeof(JsonStringEnumConverter<PartRole>))]
public enum PartRole
{
    Left,
    Right,
    Top,
    Bottom,
    Back,
    Shelf,
    Divider,
    Facade,
    Hinge,
    Handle,
    Leg,
    DrawerSlide
}

[JsonConverter(typeof(JsonStringEnumConverter<CutoutAnchor>))]
public enum CutoutAnchor
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}
