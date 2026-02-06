using System.Text.Json.Serialization;

namespace Modules.Domain.Entities.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SegmentType
{
    Line,
    Arc
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(LineSegment), "line")]
[JsonDerivedType(typeof(ArcSegment), "arc")]
public abstract record ShapeSegment;

public sealed record LineSegment(string X, string Y) : ShapeSegment;

public sealed record ArcSegment(string X, string Y, string Radius, bool LargeArc, bool Clockwise) : ShapeSegment;
