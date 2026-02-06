using Domeo.Shared.Domain;
using Modules.Domain.Entities.Shared;

namespace Modules.Domain.Entities;

/// <summary>
/// Компонент — элементарный строительный блок мебели (материал или 3D-модель фурнитуры).
/// Компоненты переиспользуются между сборками через <see cref="AssemblyPart"/>.
/// </summary>
public sealed class Component : Entity<Guid>
{
    /// <summary>Отображаемое название компонента (например, «ЛДСП Белый 16мм», «Ручка-скоба 128мм»).</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Типоспецифичные параметры: <see cref="PanelParams"/> для панелей, <see cref="GlbParams"/> для 3D-моделей.</summary>
    public ComponentParams? Params { get; private set; }

    /// <summary>Теги для фильтрации и поиска (например, «белый», «ЛДСП», «16мм»).</summary>
    public List<string> Tags { get; private set; } = [];

    /// <summary>Флаг активности. Неактивные компоненты скрыты из каталога.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Дата и время создания (UTC).</summary>
    public DateTime CreatedAt { get; private set; }

    private Component() { }

    public static Component Create(
        string name,
        ComponentParams? @params = null,
        List<string>? tags = null)
    {
        return new Component
        {
            Id = Guid.NewGuid(),
            Name = name,
            Params = @params,
            Tags = tags ?? [],
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, ComponentParams? @params, List<string>? tags)
    {
        Name = name;
        Params = @params;
        if (tags is not null) Tags = tags;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
