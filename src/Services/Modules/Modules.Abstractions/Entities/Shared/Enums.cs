namespace Modules.Abstractions.Entities.Shared;

public enum DimensionSource
{
    ParentWidth,
    ParentDepth,
    ParentHeight,
    Fixed
}

public enum AnchorOrigin
{
    Start,
    Center,
    End
}

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
