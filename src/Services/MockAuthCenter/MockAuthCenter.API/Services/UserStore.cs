namespace MockAuthCenter.API.Services;

public class UserStore
{
    private readonly List<TestUser> _users;

    public UserStore(IConfiguration configuration)
    {
        _users = configuration.GetSection("TestUsers").Get<List<TestUser>>() ?? GetDefaultUsers();
    }

    public TestUser? FindByEmail(string email)
    {
        return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public TestUser? FindById(string id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public bool ValidateCredentials(string email, string password, out TestUser? user)
    {
        user = FindByEmail(email);
        return user != null && user.Password == password;
    }

    public string? GetRoleForProgram(TestUser user, string programId)
    {
        return user.Roles.TryGetValue(programId, out var role) ? role : null;
    }

    private static List<TestUser> GetDefaultUsers() =>
    [
        new TestUser
        {
            Id = "11111111-1111-1111-1111-111111111111",
            Email = "admin@test.com",
            Password = "admin123",
            Name = "Test Admin",
            Roles = new Dictionary<string, string> { { "domeo", "admin" } }
        },
        new TestUser
        {
            Id = "22222222-2222-2222-2222-222222222222",
            Email = "manager@test.com",
            Password = "manager123",
            Name = "Test Manager",
            Roles = new Dictionary<string, string> { { "domeo", "manager" } }
        },
        new TestUser
        {
            Id = "33333333-3333-3333-3333-333333333333",
            Email = "viewer@test.com",
            Password = "viewer123",
            Name = "Test Viewer",
            Roles = new Dictionary<string, string> { { "domeo", "viewer" } }
        }
    ];
}

public class TestUser
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Roles { get; set; } = new();
}
