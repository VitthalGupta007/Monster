using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class SafeAreaManager : MonoBehaviour
    {
        [SerializeField] List<RectTransform> screens;

        public void Awake()
        {
            var rect = Screen.safeArea;

            rect.x = 0;
            rect.width = Screen.width;

            if (Screen.width > 0 && Screen.height > 0)
            {
                var anchorMin = rect.position;
                var anchorMax = rect.position + rect.size;

                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0)
                {
                    for (int i = 0; i < screens.Count; i++)
                    {
                        if (screens[i] != null)
                        {
                            screens[i].anchorMin = anchorMin;
                            screens[i].anchorMax = anchorMax;
                        }
                        else
                        {
                            screens.RemoveAt(i);

                            i--;
                        }
                    }
                }
            }
        }
    }
}