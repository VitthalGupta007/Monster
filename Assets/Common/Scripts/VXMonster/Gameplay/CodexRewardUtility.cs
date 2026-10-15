using VXMonster.Core;
using VXMonster.UI;

namespace VXMonster.Gameplay
{
    public static class CodexRewardUtility
    {
        private const int RelicDiscoveryGold = 25;
        private const int EvolutionDiscoveryGold = 40;
        private const int FirstComboGold = 15;
        private const int ScholarBonusGold = 15;

        public static void OnRelicDiscovered(string relicId)
        {
            var total = RelicDiscoveryGold;
            GrantGold(RelicDiscoveryGold);
            if (TalentTreeIds.ScholarsEyeUnlocked())
            {
                GrantGold(ScholarBonusGold);
                total += ScholarBonusGold;
            }

            var name = RelicsManager.Instance?.GetRelicById(relicId)?.DisplayName ?? "Relic";
            VxToastBehavior.Show($"Discovered {name} — +{total} gold");
        }

        public static void OnEvolutionDiscovered(string evolutionId)
        {
            GrantGold(EvolutionDiscoveryGold);
            VxToastBehavior.Show($"Evolution discovered — +{EvolutionDiscoveryGold} gold");
        }

        public static void OnFirstComboDiscovered()
        {
            GrantGold(FirstComboGold);
            VxToastBehavior.Show($"First combo! — +{FirstComboGold} gold");
        }

        private static void GrantGold(int amount)
        {
            if (amount <= 0 || GameController.CurrenciesManager == null) return;

            var gold = GameController.CurrenciesManager.GetDefaultCurrency(false);
            gold?.Deposit(amount);
            GameController.SaveManager?.Save(false);
        }
    }
}

