using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace OctoberStudio.Abilities.Editor
{
    [Overlay(typeof(SceneView), "Abilities Overlay", true, defaultDockPosition = DockPosition.Top, defaultDockZone = DockZone.BottomToolbar)]
    [Icon("Assets/Common/Sprites/Editor/editor_ab_icon.png")]
    public class AbilitiesOverlay : Overlay
    {
        protected string filterText = "";
        protected Vector2 scrollPosition;
        protected int selectedAbilityId = -1;

        protected GUIStyle selectedLabelStyle;
        protected GUIStyle selectedButtonStyle;

        IMGUIContainer root;

        protected Filter selectedTab = 0;

        public override VisualElement CreatePanelContent()
        {
            root = new IMGUIContainer(CreateGUI);
            root.style.minWidth = 500;

            selectedLabelStyle = new GUIStyle(GUI.skin.label);
            selectedLabelStyle.normal.textColor = Color.white;

            selectedButtonStyle = new GUIStyle(GUI.skin.button);
            selectedButtonStyle.normal.background = MakeSelectedButtonBackgroundTexture(1, 1, Color.white);
            selectedButtonStyle.normal.textColor = Color.black;

            return root;
        }

        protected virtual void CreateGUI()
        {
            if (!Application.isPlaying)
            {
                DrawPlaymodeNotActiveGUI();
                return;
            }

            var manager = Object.FindAnyObjectByType<AbilityManager>();

            if (manager == null)
            {
                DrawManagerIsNotPresentGUI();
                return;
            }

            var abilities = manager.GetAllAbilitiesDev();

            if (Time.timeScale == 0)
            {
                if (GUILayout.Button($"Resume time"))
                {
                    Time.timeScale = 1;
                }
            }
            else
            {
                if (GUILayout.Button($"Pause time"))
                {
                    Time.timeScale = 0;
                }
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button($"All", selectedTab == Filter.All ? selectedButtonStyle : GUI.skin.button))
            {
                selectedTab = Filter.All;
            }

            if (GUILayout.Button($"Acquired", selectedTab == Filter.Acquired ? selectedButtonStyle : GUI.skin.button))
            {
                selectedTab = Filter.Acquired;
            }

            if (GUILayout.Button($"Not Acquired", selectedTab == Filter.NotAcquired ? selectedButtonStyle : GUI.skin.button))
            {
                selectedTab = Filter.NotAcquired;
            }

            EditorGUILayout.EndHorizontal();

            filterText = EditorGUILayout.TextField("Search:", filterText);

            EditorGUILayout.BeginHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(250), GUILayout.Height(250));
            scrollPosition.x = 0;

            int counter = 0;
            for (int i = 0; i < abilities.Count; i++)
            {
                var ability = abilities[i];

                if (ability.Title.ToLower().Contains(filterText.ToLower()))
                {
                    if(selectedTab != Filter.All)
                    {
                        int level = manager.GetAbilityLevelDev(ability.AbilityType);

                        if (selectedTab == Filter.Acquired && level == -1) continue;
                        if(selectedTab == Filter.NotAcquired && level != -1) continue;
                    }

                    var rect = EditorGUILayout.GetControlRect();
                    if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                    {
                        selectedAbilityId = i;
                    }

                    if (selectedAbilityId == i)
                    {
                        EditorGUI.DrawRect(rect, Color.grey);

                    }

                    GUI.Label(rect, $"{counter + 1}. {ability.Title}", GUI.skin.label);
                    counter++;
                }
            }

            EditorGUILayout.EndScrollView();

            if (selectedAbilityId >= 0 && selectedAbilityId < abilities.Count)
            {
                var selectedAbility = abilities[selectedAbilityId];

                EditorGUILayout.BeginVertical();

                int level = manager.GetAbilityLevelDev(selectedAbility.AbilityType);

                if (level == -1)
                {
                    EditorGUILayout.LabelField($"Status: Disabled");

                    if (GUILayout.Button($"Activate {selectedAbility.Title} ability"))
                    {
                        manager.AddAbility(selectedAbility, 0);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField($"Status: Active, Level: {level + 1} out of {selectedAbility.LevelsCount}");

                    if (GUILayout.Button($"Disable {selectedAbility.Title} ability"))
                    {
                        manager.RemoveAbilityDev(selectedAbility);
                    }

                    if (level > 0)
                    {
                        if (GUILayout.Button($"Decrease ability level"))
                        {
                            manager.DecreaseAbilityLevelDev(selectedAbility);
                        }
                    }

                    if (level < selectedAbility.LevelsCount - 1)
                    {
                        if (GUILayout.Button($"Increase ability level"))
                        {
                            manager.IncreaseAbilityLevelDev(selectedAbility);
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawPlaymodeNotActiveGUI()
        {
            EditorGUILayout.HelpBox("This overlay only works in playmode", MessageType.Warning);
        }

        protected virtual void DrawManagerIsNotPresentGUI()
        {
            EditorGUILayout.HelpBox("There are no AbilitiesManager Component in the Hierarchy", MessageType.Warning);
        }

        protected virtual Texture2D MakeSelectedButtonBackgroundTexture(int width, int height, Color color)
        {
            var pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            var backgroundTexture = new Texture2D(width, height);

            backgroundTexture.SetPixels(pixels);
            backgroundTexture.Apply();

            return backgroundTexture;
        }

        protected enum Filter
        {
            All,
            Acquired,
            NotAcquired
        }
    }
}
