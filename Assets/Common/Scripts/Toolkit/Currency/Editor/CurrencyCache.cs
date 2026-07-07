using System;
using UnityEngine;
using UnityEditor;

namespace OctoberStudio.Currency
{
    [InitializeOnLoad]
    public static class CurrencyCache
    {
        private static CurrenciesDatabase database;
        public static CurrenciesDatabase Database 
        { 
            get 
            {
                if (!IsInitialized || database == null) Initialize();

                return database;
            } 
        }

        private static string[] ids;
        private static string[] names;

        private static int count;

        private static bool IsInitialized
        {
            get => EditorPrefs.GetBool("CurrencyCache initialized", false);
            set => EditorPrefs.SetBool("CurrencyCache initialized", value);
        }

        static CurrencyCache()
        {
            IsInitialized = false;
        }

        private static void Initialize()
        {
            var guids = AssetDatabase.FindAssets("t:CurrenciesDatabase");

            if (guids != null || guids.Length > 0) 
            {
                var guid = guids[0];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                database = AssetDatabase.LoadAssetAtPath<CurrenciesDatabase>(path);
            }

            if (database == null)
            {
                Debug.Log("[CurrencyCache] Could not initialize, cannot find Currencies Database in the project.");
                return;
            }

            count = database.Count;
            ids = new string[count];
            names = new string[count];

            for(int i = 0; i < database.Count; i++)
            {
                var data = database.GetCurrency(i);

                ids[i] = data.ID;
                names[i] = data.Name;
            }

            IsInitialized = true;
        }

        public static string GetName(string id)
        {
            if (!IsInitialized || names == null || ids == null) Initialize();

            if(names != null || ids != null)
            {
                var index = Array.IndexOf(ids, id);

                if (index >= 0)
                {
                    return names[index];
                }
            }

            return "None";
        }

        public static string[] GetNames()
        {
            if(!IsInitialized || names == null) Initialize();

            return names;
        }

        public static string[] GetIds()
        {
            if (!IsInitialized || ids == null) Initialize();

            return ids;
        }

        public static int Count
        {
            get
            {
                if (!IsInitialized) Initialize();

                return count;
            }
        }

        public static void Invalidate()
        {
            IsInitialized = false;
        }
    }
}