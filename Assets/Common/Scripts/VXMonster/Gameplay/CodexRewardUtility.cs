using VXMonster.Core;
using VXMonster.Core.Currency;
using VXMonster.Save;

namespace VXMonster.Gameplay
{
    public static class CodexRewardUtility
    {
        private const int RelicDiscoveryGold = 25;
        private const int EvolutionDiscoveryGold = 40;
        private const int FirstComboGold = 15;

        public static void OnRelicDiscovered(string relicId)
        {
            GrantGold(RelicDiscoveryGold, "codex_relic");
            if (TalentTreeIds.ScholarsEyeUnlocked())
            {
                GrantGold(15, "codex_scholar_bonus");
            }
        }

        public static void OnEvolutionDiscovered(string evolutionId)
        {
            GrantGold(EvolutionDiscoveryGold, "codex_evolution");
        }

        public static void OnFirstComboDiscovered()
        {
            GrantGold(FirstComboGold, "codex_combo");
        }

        private static void GrantGold(int amount, string reason)
        {
            if (amount <= 0 || GameController.CurrenciesManager == null) return;

            var gold = GameController.CurrenciesManager.GetDefaultCurrency(false);
            gold?.Deposit(amount);
            GameController.SaveManager?.Save(false);
        }
    }
}
