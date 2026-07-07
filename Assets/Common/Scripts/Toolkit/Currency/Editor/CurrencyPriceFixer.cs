using VXMonster.Core.Upgrades;
using UnityEditor;
using UnityEngine;

namespace VXMonster.Core.Currency
{
    [InitializeOnLoad]
    public static class CurrencyPriceFixer 
    {
        private static readonly string MIGRATED_KEY_1_4_0 = "CurrencyPriceFixer Fixed v1.4.0";

        static CurrencyPriceFixer()
        {
            EditorApplication.delayCall += () => RunCheck(true);
        }

        [MenuItem("Tools/VX Monster/Migration/Migrate from Cost to Price", true)]
        private static bool RunCheckMenuValidation()
        {
            return !Application.isPlaying;
        }

        [MenuItem("Tools/VX Monster/Migration/Migrate from Cost to Price")]
        private static void RunCheckMenu()
        {
            RunCheck(false);
        }

        private static void RunCheck(bool checkPrefKey)
        {
            if (checkPrefKey && EditorPrefs.GetBool(MIGRATED_KEY_1_4_0, false)) return;

            var defaultCurrencyId = CurrencyCache.Database == null || CurrencyCache.Database.Count == 0 ? "gold" : CurrencyCache.Database.GetCurrency(0).ID;

            var database = CurrencyCache.Database;
            if (database != null)
            {
                defaultCurrencyId = database.GetCurrency(0).ID;
            }

            var changed = false;

            try
            {
                if (TryFixAllUpgradeData(defaultCurrencyId))
                {
                    changed = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("[Currency Price Fixer] Could not migrate from cost to price. Most likely the Upgrade Data structure was modified");
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
                Debug.Log("[Currency Price Fixer] Migrated to Price");
            } else
            {
                Debug.Log("[Currency Price Fixer] There were no assets that require migrating to Price");
            }

            EditorPrefs.SetBool(MIGRATED_KEY_1_4_0, true);
        }

        private static bool TryFixAllUpgradeData(string defaultCurrencyId)
        {
            var guids = AssetDatabase.FindAssets("t:UpgradeData");

            var changed = false;

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                UpgradeData upgrade = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);

                if (upgrade == null || upgrade.DataVersion == 1) continue;

                if (TryFixUpgradeData(upgrade, defaultCurrencyId))
                {
                    changed = true;
                }
            }

            return changed;
        }

        private static bool TryFixUpgradeData(UpgradeData upgradeData, string defaultCurrencyId)
        {
            var serializedObject = new SerializedObject(upgradeData);

            var levelsProperty = serializedObject.FindProperty("levels");

            var changed = false;

            for (int i = 0; i < levelsProperty.arraySize; i++)
            {
                var levelProperty = levelsProperty.GetArrayElementAtIndex(i);
                var priceProperty = levelProperty.FindPropertyRelative("price");

                var currencyId = priceProperty.FindPropertyRelative("currencyId");

                if (string.IsNullOrEmpty(currencyId.stringValue))
                {
                    currencyId.stringValue = defaultCurrencyId;

                    var costProperty = levelProperty.FindPropertyRelative("cost");
                    var amountProperty = priceProperty.FindPropertyRelative("amount");

                    amountProperty.intValue = costProperty.intValue;

                    changed = true;
                }
            }

            var dataVersionProperty = serializedObject.FindProperty("dataVersion");

            if (dataVersionProperty.intValue == 0)
            {
                dataVersionProperty.intValue = 1;
                changed = true;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(upgradeData);
            }

            return changed;
        }
    }
}