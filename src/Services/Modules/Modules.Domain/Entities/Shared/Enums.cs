using System.Text.Json.Serialization;

namespace Modules.Domain.Entities.Shared;

/// <summary>
/// Источник размера для <see cref="DynamicSize"/>: определяет, от какого измерения
/// родительской сборки вычисляется параметрический размер детали.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DimensionSource>))]
public enum DimensionSource
{
    /// <summary>Размер вычисляется от ширины родительской сборки.</summary>
    ParentWidth,

    /// <summary>Размер вычисляется от глубины родительской сборки.</summary>
    ParentDepth,

    /// <summary>Размер вычисляется от высоты родительской сборки.</summary>
    ParentHeight,

    /// <summary>Фиксированное значение; используется <see cref="DynamicSize.FixedValue"/>.</summary>
    Fixed
}

/// <summary>
/// Точка привязки по одной оси в <see cref="Placement"/>.
/// Определяет процентное положение внутри родительской сборки.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AnchorOrigin>))]
public enum AnchorOrigin
{
    /// <summary>Начало оси (0%).</summary>
    Start,

    /// <summary>Центр оси (50%).</summary>
    Center,

    /// <summary>Конец оси (100%).</summary>
    End
}

/// <summary>
/// Конструктивная роль детали (<see cref="AssemblyPart"/>) в сборке.
/// Определяет функциональное назначение детали в корпусной мебели.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PartRole>))]
public enum PartRole
{
    /// <summary>Левая боковая стенка корпуса.</summary>
    Left,

    /// <summary>Правая боковая стенка корпуса.</summary>
    Right,

    /// <summary>Верхняя панель (крышка) корпуса.</summary>
    Top,

    /// <summary>Нижняя панель (дно) корпуса.</summary>
    Bottom,

    /// <summary>Задняя стенка корпуса.</summary>
    Back,

    /// <summary>Полка (горизонтальная перегородка).</summary>
    Shelf,

    /// <summary>Вертикальная перегородка (разделитель секций).</summary>
    Divider,

    /// <summary>Фасад (дверца).</summary>
    Facade,

    /// <summary>Мебельная петля (крепление фасада к корпусу).</summary>
    Hinge,

    /// <summary>Ручка фасада.</summary>
    Handle,

    /// <summary>Опора (ножка) корпуса.</summary>
    Leg,

    /// <summary>Направляющая для выдвижного ящика.</summary>
    DrawerSlide
}
