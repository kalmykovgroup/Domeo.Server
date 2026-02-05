using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MockSupplier.API.Infrastructure.Persistence;
using MockSupplier.API.Models;

namespace MockSupplier.API.Services;

public sealed class DataStore
{
    private readonly List<Category> _categories;
    private readonly List<Material> _materials;
    private readonly List<Supplier> _suppliers;
    private readonly List<Offer> _offers;
    private readonly List<Brand> _brands;
    private readonly List<CategoryAttribute> _categoryAttributes;
    private readonly List<MaterialAttributeValue> _materialAttributes;

    public DataStore(SupplierDbContext db)
    {
        var brands = db.Brands.AsNoTracking().Select(b => new Brand
        {
            Id = b.Id,
            Name = b.Name,
            LogoUrl = b.LogoUrl,
            IsActive = b.IsActive
        }).ToList();

        var brandDict = brands.ToDictionary(b => b.Id);

        _brands = brands;

        _categories = db.Categories.AsNoTracking().Select(c => new Category
        {
            Id = c.Id,
            ParentId = c.ParentId,
            Name = c.Name,
            Level = c.Level,
            OrderIndex = c.OrderIndex,
            IsActive = c.IsActive
        }).ToList();

        _materials = db.Materials.AsNoTracking().ToList().Select(m => new Material
        {
            Id = m.Id,
            CategoryId = m.CategoryId,
            BrandId = m.BrandId,
            BrandName = m.BrandId != null && brandDict.TryGetValue(m.BrandId, out var brand) ? brand.Name : null,
            Name = m.Name,
            Description = m.Description,
            Unit = m.Unit,
            Color = m.Color,
            TextureUrl = m.TextureUrl,
            IsActive = m.IsActive
        }).ToList();

        _suppliers = db.Suppliers.AsNoTracking().Select(s => new Supplier
        {
            Id = s.Id,
            Company = s.Company,
            ContactName = s.ContactName,
            Email = s.Email,
            Phone = s.Phone,
            IsActive = s.IsActive
        }).ToList();

        _offers = db.Offers.AsNoTracking().Select(o => new Offer
        {
            Id = o.Id,
            MaterialId = o.MaterialId,
            SupplierId = o.SupplierId,
            Price = o.Price,
            Currency = o.Currency,
            MinOrderQty = o.MinOrderQty,
            LeadTimeDays = o.LeadTimeDays,
            InStock = o.InStock,
            Sku = o.Sku,
            Notes = o.Notes
        }).ToList();

        _categoryAttributes = db.CategoryAttributes.AsNoTracking().ToList().Select(a => new CategoryAttribute
        {
            Id = a.Id,
            CategoryId = a.CategoryId,
            Name = a.Name,
            Type = a.Type,
            Unit = a.Unit,
            EnumValues = !string.IsNullOrEmpty(a.EnumValuesJson)
                ? JsonSerializer.Deserialize<List<string>>(a.EnumValuesJson)
                : null
        }).ToList();

        _materialAttributes = db.MaterialAttributeValues.AsNoTracking().Select(a => new MaterialAttributeValue
        {
            MaterialId = a.MaterialId,
            AttributeId = a.AttributeId,
            Value = a.Value
        }).ToList();

        Console.WriteLine($"[DataStore] Loaded: {_categories.Count} categories, {_materials.Count} materials, " +
                          $"{_suppliers.Count} suppliers, {_offers.Count} offers, {_brands.Count} brands, " +
                          $"{_categoryAttributes.Count} attributes, {_materialAttributes.Count} attr values");
    }

    // ========================================================================
    // CATEGORIES
    // ========================================================================

    public IReadOnlyList<Category> GetCategories(bool activeOnly = true)
    {
        return activeOnly
            ? _categories.Where(c => c.IsActive).ToList()
            : _categories;
    }

    public Category? GetCategory(string id)
    {
        return _categories.FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Returns a set containing the given categoryId plus all its descendant category IDs.
    /// </summary>
    private HashSet<string> GetCategoryWithDescendants(string categoryId)
    {
        var result = new HashSet<string> { categoryId };
        var queue = new Queue<string>();
        queue.Enqueue(categoryId);

        while (queue.Count > 0)
        {
            var parentId = queue.Dequeue();
            foreach (var child in _categories.Where(c => c.ParentId == parentId))
            {
                if (result.Add(child.Id))
                    queue.Enqueue(child.Id);
            }
        }

        return result;
    }

    // ========================================================================
    // BRANDS
    // ========================================================================

    public IReadOnlyList<Brand> GetBrands(bool activeOnly = true)
    {
        return activeOnly
            ? _brands.Where(b => b.IsActive).ToList()
            : _brands;
    }

    // ========================================================================
    // CATEGORY ATTRIBUTES
    // ========================================================================

    public IReadOnlyList<CategoryAttribute> GetCategoryAttributes(string categoryId)
    {
        var directAttrs = _categoryAttributes.Where(a => a.CategoryId == categoryId).ToList();
        if (directAttrs.Count > 0)
            return directAttrs;

        // For parent categories: aggregate unique attributes from all descendants
        var categoryIds = GetCategoryWithDescendants(categoryId);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return _categoryAttributes
            .Where(a => categoryIds.Contains(a.CategoryId))
            .Where(a => seen.Add(a.Name)) // deduplicate by attribute name
            .ToList();
    }

    // ========================================================================
    // MATERIAL ATTRIBUTES
    // ========================================================================

    public IReadOnlyList<MaterialAttributeValue> GetMaterialAttributes(string materialId)
    {
        return _materialAttributes.Where(a => a.MaterialId == materialId).ToList();
    }

    // ========================================================================
    // MATERIALS (with extended filtering)
    // ========================================================================

    public IReadOnlyList<Material> GetMaterials(
        string? categoryId = null,
        bool activeOnly = true,
        string? brandId = null,
        string? supplierId = null,
        Dictionary<string, string>? attributes = null)
    {
        var query = _materials.AsEnumerable();

        if (activeOnly)
            query = query.Where(m => m.IsActive);

        if (!string.IsNullOrEmpty(categoryId))
        {
            var categoryIds = GetCategoryWithDescendants(categoryId);
            query = query.Where(m => categoryIds.Contains(m.CategoryId));
        }

        if (!string.IsNullOrEmpty(brandId))
        {
            var brandIds = brandId.Split(',', StringSplitOptions.RemoveEmptyEntries);
            query = query.Where(m => m.BrandId != null && brandIds.Contains(m.BrandId));
        }

        if (!string.IsNullOrEmpty(supplierId))
        {
            var supplierIds = supplierId.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            var materialIdsWithSupplier = _offers
                .Where(o => supplierIds.Contains(o.SupplierId))
                .Select(o => o.MaterialId)
                .ToHashSet();
            query = query.Where(m => materialIdsWithSupplier.Contains(m.Id));
        }

        // Filter by attributes: attr_{name}={value}
        if (attributes is { Count: > 0 })
        {
            foreach (var (attrName, attrValue) in attributes)
            {
                var matchingAttrDefs = _categoryAttributes
                    .Where(a => a.Name.Equals(attrName, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.Id)
                    .ToHashSet();

                if (matchingAttrDefs.Count == 0)
                    continue;

                var attrValues = attrValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

                var materialIdsWithAttr = _materialAttributes
                    .Where(ma => matchingAttrDefs.Contains(ma.AttributeId) &&
                                 attrValues.Contains(ma.Value, StringComparer.OrdinalIgnoreCase))
                    .Select(ma => ma.MaterialId)
                    .ToHashSet();

                query = query.Where(m => materialIdsWithAttr.Contains(m.Id));
            }
        }

        return query.ToList();
    }

    public Material? GetMaterial(string id)
    {
        return _materials.FirstOrDefault(m => m.Id == id);
    }

    // ========================================================================
    // SUPPLIERS
    // ========================================================================

    public IReadOnlyList<Supplier> GetSuppliers(bool activeOnly = true)
    {
        return activeOnly
            ? _suppliers.Where(s => s.IsActive).ToList()
            : _suppliers;
    }

    public Supplier? GetSupplier(string id)
    {
        return _suppliers.FirstOrDefault(s => s.Id == id);
    }

    // ========================================================================
    // OFFERS
    // ========================================================================

    public IReadOnlyList<OfferWithSupplier> GetOffersByMaterial(string materialId)
    {
        var offers = _offers.Where(o => o.MaterialId == materialId).ToList();
        var result = new List<OfferWithSupplier>();

        foreach (var offer in offers)
        {
            var supplier = _suppliers.FirstOrDefault(s => s.Id == offer.SupplierId);
            if (supplier == null || !supplier.IsActive)
                continue;

            result.Add(new OfferWithSupplier
            {
                OfferId = offer.Id,
                MaterialId = offer.MaterialId,
                Price = offer.Price,
                Currency = offer.Currency,
                MinOrderQty = offer.MinOrderQty,
                LeadTimeDays = offer.LeadTimeDays,
                InStock = offer.InStock,
                Sku = offer.Sku,
                Notes = offer.Notes,
                UpdatedAt = offer.UpdatedAt,
                Supplier = new SupplierInfo
                {
                    Id = supplier.Id,
                    Company = supplier.Company,
                    ContactName = supplier.ContactName,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    Rating = supplier.Rating
                }
            });
        }

        return result.OrderBy(o => o.Price).ToList();
    }

    public IReadOnlyList<Offer> GetOffersBySupplier(string supplierId)
    {
        return _offers.Where(o => o.SupplierId == supplierId).ToList();
    }

    // ========================================================================
    // CATEGORY TREE
    // ========================================================================

    public IReadOnlyList<CategoryTreeNode> GetCategoriesTree(bool activeOnly = true)
    {
        var categories = activeOnly
            ? _categories.Where(c => c.IsActive).ToList()
            : _categories;

        // Собираем supplierIds для каждой категории через материалы и офферы
        var categorySuppliers = new Dictionary<string, HashSet<string>>();

        foreach (var category in categories)
        {
            categorySuppliers[category.Id] = [];
        }

        // Находим поставщиков для каждой категории через материалы
        foreach (var material in _materials.Where(m => !activeOnly || m.IsActive))
        {
            if (!categorySuppliers.ContainsKey(material.CategoryId))
                continue;

            var materialOffers = _offers.Where(o => o.MaterialId == material.Id);
            foreach (var offer in materialOffers)
            {
                var supplier = _suppliers.FirstOrDefault(s => s.Id == offer.SupplierId);
                if (supplier != null && (!activeOnly || supplier.IsActive))
                {
                    categorySuppliers[material.CategoryId].Add(supplier.Id);
                }
            }
        }

        // Создаём узлы дерева
        var nodeDict = categories
            .OrderBy(c => c.Level)
            .ThenBy(c => c.OrderIndex)
            .ToDictionary(
                c => c.Id,
                c => new CategoryTreeNode
                {
                    Id = c.Id,
                    ParentId = c.ParentId,
                    Name = c.Name,
                    Level = c.Level,
                    OrderIndex = c.OrderIndex,
                    IsActive = c.IsActive,
                    SupplierIds = categorySuppliers.GetValueOrDefault(c.Id, []).ToList()
                });

        var rootNodes = new List<CategoryTreeNode>();

        foreach (var category in categories.OrderBy(c => c.Level).ThenBy(c => c.OrderIndex))
        {
            var node = nodeDict[category.Id];

            if (!string.IsNullOrEmpty(category.ParentId) && nodeDict.TryGetValue(category.ParentId, out var parent))
            {
                parent.Children.Add(node);
                // Propagate supplier IDs to parent
                foreach (var supplierId in node.SupplierIds)
                {
                    if (!parent.SupplierIds.Contains(supplierId))
                        parent.SupplierIds.Add(supplierId);
                }
            }
            else
            {
                rootNodes.Add(node);
            }
        }

        return rootNodes;
    }

    // ========================================================================
    // SEARCH SUGGESTIONS
    // ========================================================================

    public IReadOnlyList<SearchSuggestion> GetSearchSuggestions(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        query = query.Trim();
        var results = new List<SearchSuggestion>();

        // 1. Materials — по name (active only)
        foreach (var m in _materials.Where(m => m.IsActive))
        {
            var score = CalculateScore(m.Name, query);
            if (score <= 0) continue;

            results.Add(new SearchSuggestion
            {
                Type = "material",
                Text = m.Name,
                Score = score * 1.5,
                Metadata = new Dictionary<string, object?>
                {
                    ["materialId"] = m.Id,
                    ["categoryId"] = m.CategoryId,
                    ["brandName"] = m.BrandName,
                    ["textureUrl"] = m.TextureUrl,
                }
            });
        }

        // 2. Brands — по name (active only)
        foreach (var b in _brands.Where(b => b.IsActive))
        {
            var score = CalculateScore(b.Name, query);
            if (score <= 0) continue;

            results.Add(new SearchSuggestion
            {
                Type = "brand",
                Text = b.Name,
                Score = score * 1.3,
                Metadata = new Dictionary<string, object?>
                {
                    ["brandId"] = b.Id,
                    ["logoUrl"] = b.LogoUrl,
                }
            });
        }

        // 3. Categories — по name (active only)
        foreach (var c in _categories.Where(c => c.IsActive))
        {
            var score = CalculateScore(c.Name, query);
            if (score <= 0) continue;

            results.Add(new SearchSuggestion
            {
                Type = "category",
                Text = c.Name,
                Score = score * 1.2,
                Metadata = new Dictionary<string, object?>
                {
                    ["categoryId"] = c.Id,
                    ["parentId"] = c.ParentId,
                    ["level"] = c.Level,
                    ["categoryPath"] = BuildCategoryPath(c),
                }
            });
        }

        // 4. Attributes — по value из materialAttributes (дедупликация по attributeName+value)
        var attrDict = _categoryAttributes.ToDictionary(a => a.Id);
        var seenAttrs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var ma in _materialAttributes)
        {
            if (!attrDict.TryGetValue(ma.AttributeId, out var attrDef))
                continue;

            var key = $"{attrDef.Name}:{ma.Value}";
            if (!seenAttrs.Add(key))
                continue;

            var score = CalculateScore(ma.Value, query);
            if (score <= 0) continue;

            // Find the category for categoryPath
            var category = _categories.FirstOrDefault(c => c.Id == attrDef.CategoryId);

            results.Add(new SearchSuggestion
            {
                Type = "attribute",
                Text = $"{attrDef.Name}: {ma.Value}",
                Score = score * 1.0,
                Metadata = new Dictionary<string, object?>
                {
                    ["attributeName"] = attrDef.Name,
                    ["attributeValue"] = ma.Value,
                    ["categoryId"] = attrDef.CategoryId,
                    ["categoryPath"] = category != null ? BuildCategoryPath(category) : null,
                }
            });
        }

        // Дедупликация по (type, text), сортировка по score desc, limit
        return results
            .GroupBy(s => (s.Type, s.Text.ToLowerInvariant()))
            .Select(g => g.OrderByDescending(s => s.Score).First())
            .OrderByDescending(s => s.Score)
            .Take(limit)
            .ToList();
    }

    private static double CalculateScore(string text, string query)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        var comparison = StringComparison.OrdinalIgnoreCase;

        // StartsWith → 100
        if (text.StartsWith(query, comparison))
            return 100;

        // Слово начинается с query → 50
        var words = text.Split([' ', '-', '/', ',', '.'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            if (word.StartsWith(query, comparison))
                return 50;
        }

        // Contains → 25
        if (text.Contains(query, comparison))
            return 25;

        return 0;
    }

    private string BuildCategoryPath(Category category)
    {
        var parts = new List<string> { category.Name };
        var current = category;

        while (!string.IsNullOrEmpty(current.ParentId))
        {
            var parent = _categories.FirstOrDefault(c => c.Id == current.ParentId);
            if (parent == null) break;
            parts.Insert(0, parent.Name);
            current = parent;
        }

        return string.Join(" > ", parts);
    }
}
