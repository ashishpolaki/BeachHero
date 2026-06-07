namespace BeachHero
{
    public static class CoreSettings
    {
        private const string DefaultInitSceneName = "Init";

        public static string InitSceneName => DefaultInitSceneName;
        
        public static bool UseCustomInspector { get; private set; } = true;
        public static bool UseHierarchyIcons { get; private set; } = true;
    }
}