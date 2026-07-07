using OctoberStudio.Easing;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace OctoberStudio.Save
{
    [DefaultExecutionOrder(-100)]
    public class SaveManager : MonoBehaviour, ISaveManager
    {
        public static readonly string SAVE_FILE_NAME = "game_save";

        private static SaveManager instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            instance = null;
        }

        [SerializeField] SaveType saveType = SaveType.SaveFile;

        [Space]
        [SerializeField] bool clearSave;

        [Space]
        [SerializeField] bool autoSaveEnabled;
        [SerializeField] float autoSaveDelay;

        private SaveDatabase SaveDatabase { get; set; }

        public bool IsSaveLoaded { get; private set; }

        public event UnityAction OnSaveLoaded;

        private Coroutine saveCoroutine;

        protected bool isSaving = false;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);

                return;
            }

            instance = this;

            DontDestroyOnLoad(gameObject);

            if (clearSave)
            {
                InitClear();
            }
            else
            {
                Load();
            }

            if (autoSaveEnabled)
            {
                StartCoroutine(AutoSaveCoroutine());
            }

            GameController.RegisterSaveManager(this);
        }

        /// <summary>
        /// Returns an instance from the save database, or creates a new one
        /// </summary>
        /// <typeparam name="T">Should implement ISave interface</typeparam>
        /// <param name="uniqueName">The unique identifier of the object you want to retrieve</param>
        /// <returns></returns>
        public T GetSave<T>(int hash) where T : ISave, new()
        {
            if (!IsSaveLoaded)
            {
                Debug.LogError("Save file has not been loaded yet");
                return default;
            }

            return SaveDatabase.GetSave<T>(hash);
        }

        /// <summary>
        /// Returns an instance from the save database, or creates a new one
        /// </summary>
        /// <typeparam name="T">Should implement ISave interface</typeparam>
        /// <param name="uniqueName">The unique identifier of the object you want to retrieve</param>
        /// <returns></returns>
        public T GetSave<T>(string uniqueName) where T : ISave, new()
        {
            return GetSave<T>(uniqueName.GetHashCode());
        }

        private void InitClear()
        {
            SaveDatabase = new SaveDatabase();
            SaveDatabase.Init();

            Debug.Log("New save is created");

            IsSaveLoaded = true;
        }

        private void Load()
        {
            if (IsSaveLoaded)
                return;

            if(saveType == SaveType.SaveFile)
            {
                // Try to read and deserialize file or create new one
                SaveDatabase = LoadSave();

                SaveDatabase.Init();

                Debug.Log("Save file is loaded");
            } else
            {
                var json = PlayerPrefs.GetString("save");
                SaveDatabase = JsonUtility.FromJson<SaveDatabase>(json);
                if (SaveDatabase == null) SaveDatabase = new SaveDatabase();

                SaveDatabase.Init();

                Debug.Log("Loaded SaveDatabase from PlayerPrefs");
            }

            IsSaveLoaded = true;

            OnSaveLoaded?.Invoke();
        }

        private SaveDatabase LoadSave()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string jsonObject = load(SAVE_FILE_NAME);
            if(!string.IsNullOrEmpty(jsonObject))
            {
                try
                {
                    SaveDatabase deserializedObject = JsonUtility.FromJson<SaveDatabase>(jsonObject);

                    return deserializedObject;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }

            return new SaveDatabase();
#else
            return SerializationHelper.DeserializePersistent<SaveDatabase>(SAVE_FILE_NAME, useLogs: false);
#endif
        }

        private IEnumerator SaveCoroutine(bool multithreading = false)
        {
            while (isSaving)
            {
                yield return null;
            }

            isSaving = true;

            if (multithreading)
            {
                var completed = false;
                Exception threadException = null;

                Thread saveThread = new Thread(() =>
                {
                    try
                    {
                        SerializationHelper.SerializePersistent(SaveDatabase, SAVE_FILE_NAME);
                    }
                    catch (Exception e)
                    {
                        threadException = e;
                    }
                    finally
                    {
                        completed = true;
                    }
                });

                saveThread.Start();

                yield return new WaitUntil(() => completed);

                if (threadException != null)
                {
                    Debug.LogException(threadException);
                }
            }
            else
            {
                try
                {
                    SerializationHelper.SerializePersistent(SaveDatabase, SAVE_FILE_NAME);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Debug.Log("Save file is updated");

            isSaving = false;
            saveCoroutine = null;
        }

        private void ForceSave()
        {
            if (SaveDatabase == null) return;
            SaveDatabase.Flush();

            if(saveType == SaveType.PlayerPrefs)
            {
                PlayerPrefs.SetString("save", JsonUtility.ToJson(SaveDatabase));
                PlayerPrefs.Save();

                Debug.Log("Save Database is sent to PlayerPrefs");
            } else
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                WebGLSave(SaveDatabase, SAVE_FILE_NAME);
                Debug.Log("Save file is updated");
#else
                if (!SerializationHelper.IsFileLocked(SAVE_FILE_NAME))
                {
                    SerializationHelper.SerializePersistent(SaveDatabase, SAVE_FILE_NAME);

                    Debug.Log("Save file is updated");
                }
#endif
            }
        }

        /// <summary>
        /// Saves the current state of the game to the file system
        /// </summary>
        /// <param name="multithreading"> if true, saves the file in another thread. Do not use multitherading in OnDestroy</param>
        public void Save(bool multithreading = false)
        {
            if (SaveDatabase == null) return;
            SaveDatabase.Flush();

            if (saveType == SaveType.PlayerPrefs)
            {
                PlayerPrefs.SetString("save", JsonUtility.ToJson(SaveDatabase));
                PlayerPrefs.Save();

                Debug.Log("Save Database is sent to PlayerPrefs");
            } else
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                WebGLSave(SaveDatabase, SAVE_FILE_NAME);
                Debug.Log("Save file is updated");
#else
                if (saveCoroutine == null) saveCoroutine = StartCoroutine(SaveCoroutine(multithreading));
#endif
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private void WebGLSave(SaveDatabase saveDatabase, string fileName)
        {
            string jsonObject = JsonUtility.ToJson(saveDatabase);

            save(fileName, jsonObject);
        }
#endif

        private IEnumerator AutoSaveCoroutine()
        {
            var wait = new WaitForSecondsRealtime(autoSaveDelay);

            while (true)
            {
                yield return wait;

                Save(true);
            }
        }

        public static void DeleteSaveFile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            deleteItem(SAVE_FILE_NAME);
#else
            SerializationHelper.DeletePersistent(SAVE_FILE_NAME);
#endif

            PlayerPrefs.DeleteAll();

            Debug.Log("Save file is deleted!");
        }

        private void OnDestroy()
        {
            ForceSave();
        }

        private void OnDisable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ForceSave();
#endif
        }

        /// <summary>
        /// Android and IOS Phones minimize applications instead of destroying them 
        /// </summary>
        /// <param name="focus"></param>
        private void OnApplicationFocus(bool focus)
        {
#if !UNITY_EDITOR
            if(!focus) ForceSave();
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string load(string keyName);

        [DllImport("__Internal")]
        private static extern void save(string keyName, string data);

        [DllImport("__Internal")]
        private static extern void deleteItem(string keyName);
#endif
    }

    public enum SaveType
    {
        SaveFile,
        PlayerPrefs,
    }
}