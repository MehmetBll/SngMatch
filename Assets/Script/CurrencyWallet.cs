using UnityEngine;

/// <summary>Oyuncunun kalici para bakiyesini ve shop satin alimlarini PlayerPrefs ile saklar.</summary>
public static class CurrencyWallet
{
    private const string MoneyKey = "TotalMoney";
    private const string ExtraUsesKey = "ExtraUses";
    private const string FreezeUsesKey = "FreezeUses";
    private const string ShopItemPrefix = "ShopItem_";

    public static int Balance => PlayerPrefs.GetInt(MoneyKey, 0);
    public static int ExtraUses => PlayerPrefs.GetInt(ExtraUsesKey, 0);
    public static int FreezeUses => PlayerPrefs.GetInt(FreezeUsesKey, 0);

    public static int Add(int amount)
    {
        if (amount <= 0)
            return Balance;

        int balance = Balance + amount;
        PlayerPrefs.SetInt(MoneyKey, balance);
        PlayerPrefs.Save();
        return balance;
    }

    public static bool TrySpend(int amount)
    {
        if (amount <= 0)
            return true;

        int balance = Balance;
        if (balance < amount)
            return false;

        PlayerPrefs.SetInt(MoneyKey, balance - amount);
        PlayerPrefs.Save();
        return true;
    }

    public static int AddExtraUses(int amount)
    {
        return AddUses(ExtraUsesKey, amount);
    }

    public static int AddFreezeUses(int amount)
    {
        return AddUses(FreezeUsesKey, amount);
    }

    public static bool TryUseExtra()
    {
        return TryUse(ExtraUsesKey);
    }

    public static bool TryUseFreeze()
    {
        return TryUse(FreezeUsesKey);
    }

    public static bool IsPurchased(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return false;

        return PlayerPrefs.GetInt(GetShopItemKey(itemId), 0) == 1;
    }

    public static void MarkPurchased(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        PlayerPrefs.SetInt(GetShopItemKey(itemId), 1);
        PlayerPrefs.Save();
    }

    public static void ResetMoney()
    {
        PlayerPrefs.DeleteKey(MoneyKey);
        PlayerPrefs.Save();
    }

    private static int AddUses(string key, int amount)
    {
        if (amount <= 0)
            return PlayerPrefs.GetInt(key, 0);

        int uses = PlayerPrefs.GetInt(key, 0) + amount;
        PlayerPrefs.SetInt(key, uses);
        PlayerPrefs.Save();
        return uses;
    }

    private static bool TryUse(string key)
    {
        int uses = PlayerPrefs.GetInt(key, 0);
        if (uses <= 0)
            return false;

        PlayerPrefs.SetInt(key, uses - 1);
        PlayerPrefs.Save();
        return true;
    }

    private static string GetShopItemKey(string itemId)
    {
        return ShopItemPrefix + itemId;
    }
}
