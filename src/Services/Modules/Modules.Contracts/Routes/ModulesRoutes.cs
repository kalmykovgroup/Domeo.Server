namespace Modules.Contracts.Routes;

public static class ModulesRoutes
{
    public const string ServiceName = "modules";

    public static class Controller
    {
        public const string Categories = "categories";
        public const string Assemblies = "assemblies";
        public const string Components = "components";

    }

    public static class Gateway
    {
        public const string Prefix = "/api/modules";
        public const string Assemblies = "/api/assemblies";
        public const string ModuleCategories = "/api/module-categories";
    }
}
