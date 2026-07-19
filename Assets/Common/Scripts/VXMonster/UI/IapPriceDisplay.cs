namespace VXMonster.UI
{
    /// <summary>
    /// Formats Play localized prices for TMP fonts that lack the rupee glyph.
    /// </summary>
    public static class IapPriceDisplay
    {
        public static string Format(string localizedPriceString)
        {
            if (string.IsNullOrEmpty(localizedPriceString) || localizedPriceString == "—")
            {
                return localizedPriceString ?? "—";
            }

            return localizedPriceString
                .Replace("\u20B9", "Rs.")
                .Replace("₹", "Rs.");
        }
    }
}
