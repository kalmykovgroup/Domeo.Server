using System.Text.Json.Serialization;

namespace Modules.Domain.Entities.Shared;

/// <summary>
/// Полиморфный сегмент 2D-контура детали (аналог SVG path).
/// JSON-дискриминатор <c>"type"</c> определяет подтип: <c>"line"</c> или <c>"arc"</c>.
/// Контур задаётся последовательностью сегментов, начиная от точки (0, 0).
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(LineSegment), "line")]
[JsonDerivedType(typeof(ArcSegment), "arc")]
public abstract record ShapeSegment;

/// <summary>
/// Прямолинейный сегмент контура — отрезок до указанной точки.
/// </summary>
/// <param name="X">Координата конечной точки по оси X в мм.</param>
/// <param name="Y">Координата конечной точки по оси Y в мм.</param>
public sealed record LineSegment(double X, double Y) : ShapeSegment;

/// <summary>
/// Дуговой сегмент контура — дуга окружности до указанной точки.
/// </summary>
/// <param name="X">Координата конечной точки по оси X в мм.</param>
/// <param name="Y">Координата конечной точки по оси Y в мм.</param>
/// <param name="Radius">Радиус дуги в мм.</param>
/// <param name="LargeArc">Если <c>true</c> — выбирается большая дуга (> 180°), иначе — малая.</param>
/// <param name="Clockwise">Если <c>true</c> — дуга идёт по часовой стрелке, иначе — против.</param>
public sealed record ArcSegment(double X, double Y, double Radius, bool LargeArc, bool Clockwise) : ShapeSegment;
