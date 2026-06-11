using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Ana menude Play, Settings ve Shop butonlarinin davranislarini yonetir.</summary>
public class MenuManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        [Tooltip("Kayit icin benzersiz urun id'si")]
        public string itemId;
        [Tooltip("Urun fiyati")]
        public int price = 50;
        [Tooltip("Urunu satin alacak buton")]
        public Button buyButton;
        [Tooltip("Fiyat veya alindi bilgisini gosteren TMP")]
        public TextMeshProUGUI priceText;
        [Tooltip("Satin alindiginda aktif olacak isaret/obje (opsiyonel)")]
        public GameObject purchasedIndicator;
    }

    [Header("Butonlar")]
    [Tooltip("Oyunu baslatan ve InGame sahnesine gecen buton")]
    public Button playButton;
    [Tooltip("Ayarlar panelini acip kapatan buton")]
    public Button settingsButton;
    [Tooltip("Shop panelini acip kapatan buton")]
    public Button shopButton;

    [Header("Paneller")]
    [Tooltip("Settings butonuna basildiginda acilip kapanacak panel")]
    public GameObject settingsPanel;
    [Tooltip("Shop butonuna basildiginda acilip kapanacak panel")]
    public GameObject shopPanel;

    [Header("Para UI")]
    [Tooltip("Menude toplam para bakiyesini gosterecek TMP")]
    public TextMeshProUGUI moneyText;

    [Header("Shop")]
    [Tooltip("Shop panelindeki satin alinabilir ogeler")]
    public ShopItem[] shopItems;
    [Tooltip("1 adet freeze hakki fiyati")]
    public int freezeUsePrice = 50;
    [Tooltip("1 adet extra sure hakki fiyati")]
    public int extraUsePrice = 50;
    [Tooltip("Freeze hakki satin alma butonu")]
    public Button buyFreezeButton;
    [Tooltip("Extra sure hakki satin alma butonu")]
    public Button buyExtraButton;
    [Tooltip("Freeze fiyati TMP")]
    public TextMeshProUGUI freezePriceText;
    [Tooltip("Extra fiyati TMP")]
    public TextMeshProUGUI extraPriceText;
    [Tooltip("Sahip olunan freeze hakki TMP")]
    public TextMeshProUGUI freezeOwnedText;
    [Tooltip("Sahip olunan extra hakki TMP")]
    public TextMeshProUGUI extraOwnedText;

    [Header("Sahne")]
    [Tooltip("Play butonuna basildiginda yuklenecek sahne adi")]
    public string inGameSceneName = "InGame";

    private UnityAction[] shopBuyActions;

    /// <summary>Buton tiklamalarini baglar ve menu panellerini baslangicta kapatir.</summary>
    private void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(ToggleSettingsPanel);
        if (shopButton != null) shopButton.onClick.AddListener(ToggleShopPanel);
        if (buyFreezeButton != null) buyFreezeButton.onClick.AddListener(BuyFreezeUse);
        if (buyExtraButton != null) buyExtraButton.onClick.AddListener(BuyExtraUse);

        BindShopButtons();
        UpdateMoneyUI();
        UpdateShopUI();

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    /// <summary>Menu tekrar aktif oldugunda kayitli para UI'ini tazeler.</summary>
    private void OnEnable()
    {
        UpdateMoneyUI();
        UpdateShopUI();
    }

    /// <summary>Script kapanirken buton dinleyicilerini temizler.</summary>
    private void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveListener(PlayGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(ToggleSettingsPanel);
        if (shopButton != null) shopButton.onClick.RemoveListener(ToggleShopPanel);
        if (buyFreezeButton != null) buyFreezeButton.onClick.RemoveListener(BuyFreezeUse);
        if (buyExtraButton != null) buyExtraButton.onClick.RemoveListener(BuyExtraUse);
        UnbindShopButtons();
    }

    /// <summary>Oyunu baslatir ve InGame sahnesini yukler.</summary>
    public void PlayGame()
    {
        Time.timeScale = 1f;
        ClearEditorVolumeSelection();
        SceneManager.LoadScene(inGameSceneName);
    }

    private static void ClearEditorVolumeSelection()
    {
#if UNITY_EDITOR
        UnityEngine.Object activeObject = UnityEditor.Selection.activeObject;
        if (activeObject == null)
            return;

        if (activeObject is UnityEngine.Rendering.Volume ||
            activeObject is GameObject gameObject && gameObject.GetComponent<UnityEngine.Rendering.Volume>() != null)
        {
            UnityEditor.Selection.activeObject = null;
        }
#endif
    }

    /// <summary>Settings panelini aciksa kapatir, kapaliysa acar.</summary>
    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(!settingsPanel.activeSelf);

        if (settingsPanel.activeSelf && shopPanel != null)
            shopPanel.SetActive(false);
    }

    /// <summary>Shop panelini aciksa kapatir, kapaliysa acar.</summary>
    public void ToggleShopPanel()
    {
        if (shopPanel == null) return;
        shopPanel.SetActive(!shopPanel.activeSelf);
        UpdateMoneyUI();
        UpdateShopUI();

        if (shopPanel.activeSelf && settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>Settings panelini kapatir.</summary>
    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>Shop panelini kapatir.</summary>
    public void CloseShopPanel()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    /// <summary>Shop butonlarini listedeki urun indexlerine baglar.</summary>
    private void BindShopButtons()
    {
        if (shopItems == null || shopItems.Length == 0)
            return;

        shopBuyActions = new UnityAction[shopItems.Length];
        for (int i = 0; i < shopItems.Length; i++)
        {
            int index = i;
            shopBuyActions[i] = () => TryBuyShopItem(index);

            if (shopItems[i] != null && shopItems[i].buyButton != null)
                shopItems[i].buyButton.onClick.AddListener(shopBuyActions[i]);
        }
    }

    /// <summary>Shop butonu dinleyicilerini temizler.</summary>
    private void UnbindShopButtons()
    {
        if (shopItems == null || shopBuyActions == null)
            return;

        for (int i = 0; i < shopItems.Length && i < shopBuyActions.Length; i++)
        {
            if (shopItems[i] != null && shopItems[i].buyButton != null && shopBuyActions[i] != null)
                shopItems[i].buyButton.onClick.RemoveListener(shopBuyActions[i]);
        }
    }

    /// <summary>Menudeki toplam para TMP'sini gunceller.</summary>
    public void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = CurrencyWallet.Balance.ToString();
    }

    /// <summary>Para yeterliyse 1 freeze hakki satin alir.</summary>
    public void BuyFreezeUse()
    {
        if (!CurrencyWallet.TrySpend(Mathf.Max(0, freezeUsePrice)))
        {
            UpdateMoneyUI();
            UpdateShopUI();
            return;
        }

        CurrencyWallet.AddFreezeUses(1);
        UpdateMoneyUI();
        UpdateShopUI();
    }

    /// <summary>Para yeterliyse 1 extra sure hakki satin alir.</summary>
    public void BuyExtraUse()
    {
        if (!CurrencyWallet.TrySpend(Mathf.Max(0, extraUsePrice)))
        {
            UpdateMoneyUI();
            UpdateShopUI();
            return;
        }

        CurrencyWallet.AddExtraUses(1);
        UpdateMoneyUI();
        UpdateShopUI();
    }

    /// <summary>Shop ogesini satin alir, basariliysa kayda isler.</summary>
    public bool TryBuyShopItem(int itemIndex)
    {
        if (shopItems == null || itemIndex < 0 || itemIndex >= shopItems.Length)
            return false;

        ShopItem item = shopItems[itemIndex];
        if (item == null)
            return false;

        string itemId = GetShopItemId(item, itemIndex);
        if (CurrencyWallet.IsPurchased(itemId))
        {
            UpdateMoneyUI();
            UpdateShopUI();
            return true;
        }

        if (!CurrencyWallet.TrySpend(Mathf.Max(0, item.price)))
        {
            UpdateMoneyUI();
            UpdateShopUI();
            return false;
        }

        CurrencyWallet.MarkPurchased(itemId);
        UpdateMoneyUI();
        UpdateShopUI();
        return true;
    }

    /// <summary>Verilen shop indexinin daha once satin alinip alinmadigini dondurur.</summary>
    public bool IsShopItemPurchased(int itemIndex)
    {
        if (shopItems == null || itemIndex < 0 || itemIndex >= shopItems.Length)
            return false;

        return CurrencyWallet.IsPurchased(GetShopItemId(shopItems[itemIndex], itemIndex));
    }

    /// <summary>Shop fiyatlarini, buton durumlarini ve satin alindi isaretlerini tazeler.</summary>
    public void UpdateShopUI()
    {
        UpdatePowerupShopUI();

        if (shopItems == null)
            return;

        int balance = CurrencyWallet.Balance;
        for (int i = 0; i < shopItems.Length; i++)
        {
            ShopItem item = shopItems[i];
            if (item == null)
                continue;

            int price = Mathf.Max(0, item.price);
            bool purchased = CurrencyWallet.IsPurchased(GetShopItemId(item, i));

            if (item.priceText != null)
                item.priceText.text = purchased ? "Alindi" : price + "$";

            if (item.buyButton != null)
                item.buyButton.interactable = !purchased && balance >= price;

            if (item.purchasedIndicator != null)
                item.purchasedIndicator.SetActive(purchased);
        }
    }

    private string GetShopItemId(ShopItem item, int itemIndex)
    {
        if (item != null && !string.IsNullOrEmpty(item.itemId))
            return item.itemId;

        return "Item_" + itemIndex;
    }

    private void UpdatePowerupShopUI()
    {
        int balance = CurrencyWallet.Balance;
        int freezePrice = Mathf.Max(0, freezeUsePrice);
        int extraPrice = Mathf.Max(0, extraUsePrice);

        if (freezePriceText != null)
            freezePriceText.text = freezePrice + "$";

        if (extraPriceText != null)
            extraPriceText.text = extraPrice + "$";

        if (freezeOwnedText != null)
            freezeOwnedText.text = CurrencyWallet.FreezeUses.ToString();

        if (extraOwnedText != null)
            extraOwnedText.text = CurrencyWallet.ExtraUses.ToString();

        if (buyFreezeButton != null)
            buyFreezeButton.interactable = balance >= freezePrice;

        if (buyExtraButton != null)
            buyExtraButton.interactable = balance >= extraPrice;
    }
}
