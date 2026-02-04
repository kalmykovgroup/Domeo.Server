using MockSupplier.API.Models;

namespace MockSupplier.API.Services;

public sealed class DataStore
{
    private readonly List<Category> _categories;
    private readonly List<Material> _materials;
    private readonly List<Supplier> _suppliers;
    private readonly List<Offer> _offers;

    public DataStore(IConfiguration configuration)
    {
        _categories = configuration.GetSection("Data:Categories").Get<List<Category>>() ?? [];
        _materials = configuration.GetSection("Data:Materials").Get<List<Material>>() ?? [];
        _suppliers = configuration.GetSection("Data:Suppliers").Get<List<Supplier>>() ?? [];
        _offers = configuration.GetSection("Data:Offers").Get<List<Offer>>() ?? [];
    }

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

    public IReadOnlyList<Material> GetMaterials(string? categoryId = null, bool activeOnly = true)
    {
        var query = _materials.AsEnumerable();

        if (activeOnly)
            query = query.Where(m => m.IsActive);

        if (!string.IsNullOrEmpty(categoryId))
            query = query.Where(m => m.CategoryId == categoryId);

        return query.ToList();
    }

    public Material? GetMaterial(string id)
    {
        return _materials.FirstOrDefault(m => m.Id == id);
    }

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
}
