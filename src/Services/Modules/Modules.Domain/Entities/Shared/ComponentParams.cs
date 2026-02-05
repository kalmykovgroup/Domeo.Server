using System.Text.Json.Serialization;

namespace Modules.Domain.Entities.Shared;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PanelParams), "panel")]
[JsonDerivedType(typeof(GlbParams), "glb")]
public abstract record ComponentParams;

public sealed record PanelParams(double Thickness) : ComponentParams;

public sealed record GlbParams(string GlbUrl, double Scale) : ComponentParams;
