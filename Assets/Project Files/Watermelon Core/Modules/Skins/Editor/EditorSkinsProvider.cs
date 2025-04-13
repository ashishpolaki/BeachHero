using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Watermelon
{
    [InitializeOnLoad]
    public static class EditorSkinsProvider
    {
        private static List<AbstractSkinsDatabase> skinsDatabases;
        public static List<AbstractSkinsDatabase> SkinsDatabases => skinsDatabases;

        private static IEnumerable<Type> registeredTypes;

        static EditorSkinsProvider()
        {
            skinsDatabases = new List<AbstractSkinsDatabase>();

            registeredTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => !p.IsAbstract && p.IsSubclassOf(typeof(AbstractSkinsDatabase)));

            foreach(Type type in registeredTypes)
            {
                Object database = EditorUtils.GetAsset(type);
                if(database != null)
                {
                    skinsDatabases.Add((AbstractSkinsDatabase)database);
                }
            }
        }

        public static void AddDatabase(AbstractSkinsDatabase database)
        {
            if (HasSkinsProvider(database)) return;

            skinsDatabases.Add(database);
        }

        public static AbstractSkinsDatabase GetSkinsProvider(Type providerType)
        {
            if (!skinsDatabases.IsNullOrEmpty())
            {
                foreach (AbstractSkinsDatabase database in skinsDatabases)
                {
                    if (database.GetType() == providerType)
                        return database;
                }
            }

            return null;
        }

        public static bool HasSkinsProvider(AbstractSkinsDatabase provider)
        {
            if (!skinsDatabases.IsNullOrEmpty())
            {
                foreach (AbstractSkinsDatabase database in skinsDatabases)
                {
                    if (database == provider)
                        return true;
                }
            }

            return false;
        }
    }
}
