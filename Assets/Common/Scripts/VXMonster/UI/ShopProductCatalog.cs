using VXMonster.Platform.IAP;

namespace VXMonster.UI
{
    public readonly struct ShopProductInfo
    {
        public ShopProductInfo(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public string Title { get; }
        public string Description { get; }
    }

    public static class ShopProductCatalog
    {
        public static ShopProductInfo Get(string productId)
        {
            switch (productId)
            {
                case IAPProductIds.RemoveAds:
                    return new ShopProductInfo(
                        "Remove Ads",
                        "Permanently disables banner ads.");

                case IAPProductIds.StarterBundle:
                    return new ShopProductInfo(
                        "Starter Bundle",
                        "Remove ads plus 1,000 gold (one-time).");

                case IAPProductIds.GoldSmall:
                    return new ShopProductInfo(
                        "Gold Pack (Small)",
                        $"Instantly adds {PurchaseFulfillment.GoldSmallAmount:N0} gold.");

                case IAPProductIds.GoldMedium:
                    return new ShopProductInfo(
                        "Gold Pack (Medium)",
                        $"Instantly adds {PurchaseFulfillment.GoldMediumAmount:N0} gold.");

                case IAPProductIds.GoldLarge:
                    return new ShopProductInfo(
                        "Gold Pack (Large)",
                        $"Instantly adds {PurchaseFulfillment.GoldLargeAmount:N0} gold.");

                default:
                    return new ShopProductInfo("Product", string.Empty);
            }
        }
    }
}
