using UnityEngine;
using UnityEditor;

namespace VXMonster.Core.Save
{
    public static class SaveActionsMenu
    {
        [MenuItem("Tools/VX Monster/Delete Save File", priority = 3)]
        private static void DeleteSaveFile()
        {
            PlayerPrefs.DeleteAll();
            SaveManager.DeleteSaveFile();
        }

        [MenuItem("Tools/VX Monster/Delete Save File", true)]
        private static bool DeleteSaveFileValidation()
        {
            return !Application.isPlaying;
        }

        [MenuItem("Tools/VX Monster/Open All Stages", priority = 2)]
        private static void OpenAllStages()
        {
            var stageSave = GameController.SaveManager.GetSave<StageSave>("Stage");

            string[] guiID = AssetDatabase.FindAssets("t:StagesDatabase");

            if (guiID != null)
            {
                var database = AssetDatabase.LoadAssetAtPath<StagesDatabase>(AssetDatabase.GUIDToAssetPath(guiID[0]));

                if (database != null)
                {
                    for (int i = 0; i < database.StagesCount; i++)
                    {
                        var stageData = database.GetStage(i);

                        stageSave.UnlockStage(stageData);
                    }

                    EditorApplication.isPlaying = false;
                }
            }
        }

        [MenuItem("Tools/VX Monster/Open All Stages", true)]
        private static bool OpenAllStagesValidation()
        {
            return Application.isPlaying;
        }

        [MenuItem("Tools/VX Monster/Get Currencies (1K)", priority = 1)]
        private static void GetGold()
        {
            var currencies = GameController.CurrenciesManager.GetAllCurrencies(StageController.IsLoaded);
            for (int i = 0; i < currencies.Count; i++)
            {
                currencies[i].Deposit(1000);
            }
        }

        [MenuItem("Tools/VX Monster/Get Currencies (1K)", true)]
        private static bool GetGoldValidation()
        {
            return Application.isPlaying;
        }

        [MenuItem("Tools/VX Monster/Get Currencies (10K)", priority = 1)]
        private static void GetGoldBig()
        {
            var currencies = GameController.CurrenciesManager.GetAllCurrencies(StageController.IsLoaded);
            for (int i = 0; i < currencies.Count; i++)
            {
                currencies[i].Deposit(10000);
            }
        }

        [MenuItem("Tools/VX Monster/Get Currencies (10K)", true)]
        private static bool GetGoldBigValidation()
        {
            return Application.isPlaying;
        }
    }
}