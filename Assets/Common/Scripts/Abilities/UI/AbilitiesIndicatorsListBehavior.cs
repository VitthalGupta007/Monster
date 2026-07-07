using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities.UI
{
    public class AbilitiesIndicatorsListBehavior : MonoBehaviour
    {
        [SerializeField] bool isActiveaAbilities;
        [SerializeField] GameObject abilityIndicatorPrefab;
        private List<AbilityIndicatorBehavior> indicators;

        [Header("Refs")]
        [SerializeField] CanvasGroup canvasGroup;

        private IEasingCoroutine alphaCoroutine;

        private void Awake()
        {
            canvasGroup.alpha = 0;

            Init();
        }

        private void Init(){
            var capacity = isActiveaAbilities ? 
                StageController.AbilityManager.ActiveAbilitiesCapacity : 
                StageController.AbilityManager.PassiveAbilitiesCapacity;
            
            indicators = new List<AbilityIndicatorBehavior>(capacity);
            
            for(int i = 0 ; i < capacity; i++)
            {
                var indicator = Instantiate(abilityIndicatorPrefab).GetComponent<AbilityIndicatorBehavior>();
                indicator.transform.SetParent(transform);
                indicator.transform.ResetLocal();

                indicators.Add(indicator);
            }
        }

        public void Refresh()
        {
            var abilityTypes = StageController.AbilityManager.GetAquiredAbilityTypes();

            int counter = 0;
            for(int i = 0; i < abilityTypes.Count; i++)
            {
                var abilityType = abilityTypes[i];
                var abilityData = StageController.AbilityManager.GetAbilityData(abilityType);

                if (abilityData.IsActiveAbility == isActiveaAbilities)
                {
                    indicators[counter++].Show(abilityData.Icon, StageController.AbilityManager.GetAbilityLevel(abilityType), !abilityData.IsEvolution);
                }
            }

            var capacity = isActiveaAbilities ? StageController.AbilityManager.ActiveAbilitiesCapacity : StageController.AbilityManager.PassiveAbilitiesCapacity;

            for (int i = counter; i < capacity; i++)
            {
                indicators[i].Show();
            }

            for(int i = capacity; i < indicators.Count; i++)
            {
                indicators[i].Hide();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            alphaCoroutine.StopIfExists();
            alphaCoroutine = canvasGroup.DoAlpha(1f, 0.2f).SetUnscaledTime(true);
        }

        public void Hide()
        {
            alphaCoroutine.StopIfExists();
            alphaCoroutine = canvasGroup.DoAlpha(0, 0.2f).SetOnFinish(() => gameObject.SetActive(false)).SetUnscaledTime(true);
        }
    }
}