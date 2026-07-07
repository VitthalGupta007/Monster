using System;
using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Platform.Ads
{
    /// <summary>
    /// Marshals AdMob SDK callbacks onto Unity's main thread.
    /// </summary>
    public sealed class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> Queue = new();
        private static MainThreadDispatcher instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureExists()
        {
            if (instance != null) return;

            var go = new GameObject("VX Main Thread Dispatcher");
            instance = go.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }

        public static void Run(Action action)
        {
            if (action == null) return;

            if (instance == null)
            {
                EnsureExists();
            }

            lock (Queue)
            {
                Queue.Enqueue(action);
            }
        }

        private void Update()
        {
            while (true)
            {
                Action action;
                lock (Queue)
                {
                    if (Queue.Count == 0) return;
                    action = Queue.Dequeue();
                }

                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}
