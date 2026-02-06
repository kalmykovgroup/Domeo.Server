using Domeo.Shared.Domain;
using Modules.Domain.Entities.Shared;

namespace Modules.Domain.Entities;

/// <summary>
/// Деталь сборки — связь между <see cref="Assembly"/> и <see cref="Component"/>
/// с указанием роли, параметрических размеров и позиции внутри корпуса.
/// </summary>
public sealed class AssemblyPart : Entity<Guid>
{
    /// <summary>Идентификатор родительской сборки.</summary>
    public Guid AssemblyId { get; private set; }

    /// <summary>Идентификатор используемого компонента (материал или 3D-модель).</summary>
    public Guid ComponentId { get; private set; }

    /// <summary>Конструктивная роль детали в сборке (стенка, полка, фасад и т.п.).</summary>
    public PartRole Role { get; private set; }

    /// <summary>Параметрическая длина детали. <c>null</c> — определяется компонентом.</summary>
    public DynamicSize? Length { get; private set; }

    /// <summary>Параметрическая ширина детали. <c>null</c> — определяется компонентом.</summary>
    public DynamicSize? Width { get; private set; }

    /// <summary>3D-позиция и ориентация детали внутри сборки.</summary>
    public Placement Placement { get; private set; } = null!;

    /// <summary>Произвольный 2D-контур детали (для нестандартных форм). <c>null</c> — прямоугольник.</summary>
    public List<ShapeSegment>? Shape { get; private set; }

    /// <summary>Количество одинаковых деталей (по умолчанию 1).</summary>
    public int Quantity { get; private set; } = 1;

    /// <summary>Формула для вычисления количества (например, зависимость от высоты). <c>null</c> — используется <see cref="Quantity"/>.</summary>
    public string? QuantityFormula { get; private set; }

    /// <summary>Порядок сортировки при отображении деталей сборки.</summary>
    public int SortOrder { get; private set; }

    private AssemblyPart() { }

    public static AssemblyPart Create(
        Guid assemblyId,
        Guid componentId,
        PartRole role,
        Placement placement,
        DynamicSize? length = null,
        DynamicSize? width = null,
        int quantity = 1,
        string? quantityFormula = null,
        int sortOrder = 0,
        List<ShapeSegment>? shape = null)
    {
        return new AssemblyPart
        {
            Id = Guid.NewGuid(),
            AssemblyId = assemblyId,
            ComponentId = componentId,
            Role = role,
            Placement = placement,
            Length = length,
            Width = width,
            Shape = shape,
            Quantity = quantity,
            QuantityFormula = quantityFormula,
            SortOrder = sortOrder
        };
    }

    public void Update(
        Guid componentId,
        PartRole role,
        Placement placement,
        DynamicSize? length,
        DynamicSize? width,
        int quantity,
        string? quantityFormula,
        int sortOrder,
        List<ShapeSegment>? shape = null)
    {
        ComponentId = componentId;
        Role = role;
        Placement = placement;
        Length = length;
        Width = width;
        Shape = shape;
        Quantity = quantity;
        QuantityFormula = quantityFormula;
        SortOrder = sortOrder;
    }
}
