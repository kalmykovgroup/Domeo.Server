using Users.API.Entities;
using Users.API.Persistence;

namespace Users.API.Services;

public sealed class UsersSeeder
{
    private readonly UsersDbContext _dbContext;
    private readonly ILogger<UsersSeeder> _logger;

    // Default admin password hash (password: "Admin123!")
    // BCrypt hash generated with cost factor 12
    private const string DefaultAdminPasswordHash = "$2a$12$Q4iDvucF3yq.W2WonIRkHOTF/q.FENx1EhJpIwgV3VEIdo7IRwHBm";

    // Default user password hash (password: "User123!")
    private const string DefaultUserPasswordHash = "$2a$12$YeViwkuPIzY1H39WeyLXm.6RozUDPGpTNS0egXXsFpFAuDRyWXQK2";

    // Fixed GUIDs for cross-service references
    public static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid DesignerUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ManagerUserId = Guid.Parse("11111111-1111-1111-1111-111111111112");
    public static readonly Guid Client1Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Client2Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public UsersSeeder(UsersDbContext dbContext, ILogger<UsersSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding users database...");

        // Admin user
        var admin = CreateUserWithId(AdminUserId, "admin@domeo.ru", DefaultAdminPasswordHash, "Admin", "System", "admin");
        _dbContext.Users.Add(admin);

        // Demo designer user
        var designer = CreateUserWithId(DesignerUserId, "designer@domeo.ru", DefaultUserPasswordHash, "Иван", "Дизайнеров", "designer");
        _dbContext.Users.Add(designer);

        // Demo manager user
        var manager = CreateUserWithId(ManagerUserId, "manager@domeo.ru", DefaultUserPasswordHash, "Мария", "Менеджерова", "manager");
        _dbContext.Users.Add(manager);

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} users", 3);

        // Clients for designer
        var clients = new[]
        {
            CreateClientWithId(Client1Id, "Петров Алексей Иванович", designer.Id, "+7 (999) 123-45-67", "petrov@mail.ru", "г. Москва, ул. Пушкина, д. 10", "VIP клиент"),
            CreateClientWithId(Client2Id, "Сидорова Елена Петровна", designer.Id, "+7 (999) 234-56-78", "sidorova@gmail.com", "г. Санкт-Петербург, Невский пр., д. 100"),
            CreateClientWithId(Guid.Parse("33333333-3333-3333-3333-333333333334"), "Козлов Дмитрий Сергеевич", designer.Id, "+7 (999) 345-67-89", "kozlov@yandex.ru", "г. Казань, ул. Баумана, д. 50", "Предпочитает современный стиль"),
            CreateClientWithId(Guid.Parse("33333333-3333-3333-3333-333333333335"), "Новикова Анна Михайловна", manager.Id, "+7 (999) 456-78-90", "novikova@mail.ru", "г. Екатеринбург, ул. Ленина, д. 25"),
            CreateClientWithId(Guid.Parse("33333333-3333-3333-3333-333333333336"), "Морозов Сергей Владимирович", manager.Id, "+7 (999) 567-89-01", "morozov@gmail.com", "г. Новосибирск, Красный пр., д. 75")
        };

        _dbContext.Clients.AddRange(clients);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} clients", clients.Length);

        _logger.LogInformation("Users database seeding completed");
    }

    private static User CreateUserWithId(Guid id, string email, string passwordHash, string firstName, string lastName, string role)
    {
        var user = User.Create(email, passwordHash, firstName, lastName, role);
        typeof(User).GetProperty("Id")!.SetValue(user, id);
        return user;
    }

    private static Client CreateClientWithId(Guid id, string name, Guid userId, string? phone = null, string? email = null, string? address = null, string? notes = null)
    {
        var client = Client.Create(name, userId, phone, email, address, notes);
        typeof(Client).GetProperty("Id")!.SetValue(client, id);
        return client;
    }
}
