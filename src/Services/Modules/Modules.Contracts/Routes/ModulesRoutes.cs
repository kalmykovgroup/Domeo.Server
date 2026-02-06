namespace Modules.Contracts.Routes;

public static class ModulesRoutes
{
    public const string ServiceName = "modules";
    public const string ById = "{id:guid}";

    public static class Controller
    {
        public const string Categories = "categories";
        public const string Assemblies = "assemblies";
        public const string Components = "components";
        public const string Parts = "parts";
        public const string Storage = "storage";
    }

    public static class Gateway
    {
        public const string Prefix = "/api/modules";
        public const string Assemblies = "/api/assemblies";
        public const string ModuleCategories = "/api/module-categories";
    }
}
