using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OctoberStudio
{
    public static class StageUnlockConditionTypeCache
    {
        private static List<(Type type, UnlockConditionAttribute meta)> cachedTypes;

        public static List<(Type, UnlockConditionAttribute)> GetTypes()
        {
            if (cachedTypes != null)
                return cachedTypes;

            cachedTypes = new List<(Type, UnlockConditionAttribute)>();

            var baseType = typeof(StageUnlockCondition);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type.IsAbstract || !baseType.IsAssignableFrom(type))
                        continue;

                    var attr = type.GetCustomAttribute<UnlockConditionAttribute>();
                    if (attr == null)
                        continue;

                    cachedTypes.Add((type, attr));
                }
            }

            cachedTypes = cachedTypes
                .OrderBy(t => t.meta.Order)
                .ThenBy(t => t.meta.MenuName)
                .ToList();

            return cachedTypes;
        }
    }
}