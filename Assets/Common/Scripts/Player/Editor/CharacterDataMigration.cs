using VXMonster.Core.Currency;
using VXMonster.Core.StageCreator;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace VXMonster.Core
{
    [InitializeOnLoad]
    public static class CharacterDataMigration
    {
        private static readonly string MIGRATED_KEY_1_4_0 = "CharacterDataMigration Migrated v1.4.0";

        static CharacterDataMigration()
        {
           EditorApplication.delayCall += () => Migrate(true);
        }

        [MenuItem("Tools/VX Monster/Migration/Convert Character Data to Scriptable Objects", true)]
        private static bool RunCheckMenuValidation()
        {
            return !Application.isPlaying;
        }

        [MenuItem("Tools/VX Monster/Migration/Convert Character Data to Scriptable Objects")]
        private static void RunCheckMenu()
        {
            Migrate(false);
        }

        private static void Migrate(bool checkPrefKey = false)
        {
            if (checkPrefKey && EditorPrefs.GetBool(MIGRATED_KEY_1_4_0, false)) return;

            var guids = AssetDatabase.FindAssets("t:CharactersDatabase");

            var currencies = CurrencyCache.GetIds();
            var currency = currencies != null ? currencies[0] : "gold";

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                MigrateDatabase(path, currency);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Character Data Migration] Finished Migration");

            EditorPrefs.SetBool(MIGRATED_KEY_1_4_0, true);
        }

        private static void MigrateDatabase(string path, string currencyid)
        {
            var database = AssetDatabase.LoadAssetAtPath<CharactersDatabase>(path);

            if (database == null) return;

            var folderPath = Path.GetDirectoryName(path);
            var dataFolder = Path.Combine(folderPath, "Characters");

            var databaseObject = new SerializedObject(database);

            var charactersProperty = databaseObject.FindProperty("characters");
            var characterDatasProperty = databaseObject.FindProperty("characterDatas");

            var created = false;

            for (int i = 0; i < charactersProperty.arraySize; i++)
            {
                var characterProperty = charactersProperty.GetArrayElementAtIndex(i);

                var newAsset = ScriptableObject.CreateInstance<CharacterDataSO>();
                newAsset.CloneFromSerializedProperty(characterProperty, currencyid);

                var fileName = MakeValidFileName($"{newAsset.Name} Character Data.asset");
                var fullPath = Path.Combine(dataFolder, fileName);
                var uniquePath = AssetDatabase.GenerateUniqueAssetPath(fullPath);

                AssetDatabase.CreateAsset(newAsset, uniquePath);

                characterDatasProperty.arraySize++;
                characterDatasProperty.GetLastArrayElement().objectReferenceValue = newAsset;

                created = true;
            }

            charactersProperty.arraySize = 0;
            databaseObject.ApplyModifiedPropertiesWithoutUndo();

            if (created)
            {
                Debug.Log($"[Character Data Migration] Migrated Character data from '{database.name}' to Scriptable Objects");
            }
            else
            {
                Debug.Log($"[Character Data Migration] Couldn't migrate Character data from '{database.name}' to Scriptable Objects: There are no characters in the database");
            }
        }

        private static string MakeValidFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            return name;
        }
    }
}