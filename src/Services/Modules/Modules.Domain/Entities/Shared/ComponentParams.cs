using System.Text.Json.Serialization;

namespace Modules.Domain.Entities.Shared;

/// <summary>
/// Полиморфный базовый тип параметров компонента.
/// JSON-дискриминатор <c>"type"</c> определяет конкретный подтип: <c>"panel"</c> или <c>"glb"</c>.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PanelParams), "panel")]
[JsonDerivedType(typeof(GlbParams), "glb")]
public abstract record ComponentParams;

/// <summary>
/// Параметры панельного компонента (ЛДСП, МДФ и т.п.).
/// </summary>
/// <param name="Thickness">Толщина материала панели в мм (например, 16 или 18).</param>
public sealed record PanelParams(double Thickness) : ComponentParams;

/// <summary>
/// Параметры 3D-модели компонента (фурнитура, ручки, петли и т.п.).
/// </summary>
/// <param name="GlbUrl">Относительный URL файла <c>.glb</c> модели.</param>
/// <param name="Scale">Масштаб модели (1.0 = оригинальный размер).</param>
public sealed record GlbParams(string GlbUrl, double Scale) : ComponentParams;
