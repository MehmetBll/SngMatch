using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Ana menüde Play, Settings ve Shop butonlarının davranışlarını yönetir.</summary>
public class MenuManager : MonoBehaviour
{
    [Header("Butonlar")]
    [Tooltip("Oyunu başlatan ve InGame sahnesine geçen buton")]
    public Button playButton;
    [Tooltip("Ayarlar panelini açıp kapatan buton")]
    public Button settingsButton;
    [Tooltip("Shop panelini açıp kapatan buton")]
    public Button shopButton;

    [Header("Paneller")]
    [Tooltip("Settings butonuna basıldığında açılıp kapanacak panel")]
    public GameObject settingsPanel;
    [Tooltip("Shop butonuna basıldığında açılıp kapanacak panel")]
    public GameObject shopPanel;

    [Header("Sahne")]
    [Tooltip("Play butonuna basıldığında yüklenecek sahne adı")]
    public string inGameSceneName = "InGame";

    /// <summary>Buton tıklamalarını bağlar ve menü panellerini başlangıçta kapatır.</summary>
    private void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(ToggleSettingsPanel);
        if (shopButton != null) shopButton.onClick.AddListener(ToggleShopPanel);

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    /// <summary>Script kapanırken buton dinleyicilerini temizler.</summary>
    private void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveListener(PlayGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(ToggleSettingsPanel);
        if (shopButton != null) shopButton.onClick.RemoveListener(ToggleShopPanel);
    }

    /// <summary>Oyunu başlatır ve InGame sahnesini yükler.</summary>
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(inGameSceneName);
    }

    /// <summary>Settings panelini açıksa kapatır, kapalıysa açar.</summary>
    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(!settingsPanel.activeSelf);

        if (settingsPanel.activeSelf && shopPanel != null)
            shopPanel.SetActive(false);
    }

    /// <summary>Shop panelini açıksa kapatır, kapalıysa açar.</summary>
    public void ToggleShopPanel()
    {
        if (shopPanel == null) return;
        shopPanel.SetActive(!shopPanel.activeSelf);

        if (shopPanel.activeSelf && settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>Settings panelini kapatır.</summary>
    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>Shop panelini kapatır.</summary>
    public void CloseShopPanel()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
}
