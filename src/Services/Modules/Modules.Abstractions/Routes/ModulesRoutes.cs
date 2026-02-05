namespace Modules.Abstractions.Routes;

public static class ModulesRoutes
{
    public const string ServiceName = "modules";

    public static class Controller
    {
        public const string Categories = "categories";
        public const string Assemblies = "assemblies";
        public const string Components = "components";

        public const string Tree = "tree";
        public const string Count = "count";
        public const string AssemblyById = "{id:guid}";
        public const string AssemblyParts = "{id:guid}/parts";
        public const string ComponentById = "{id:guid}";
    }

    public static class Gateway
    {
        public const string Prefix = "/api/modules";
        public const string Assemblies = "/api/assemblies";
        public const string ModuleCategories = "/api/module-categories";
    }
}
