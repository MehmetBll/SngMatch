using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Oyun suresi, kazanma/kaybetme, devam etme ve panel akisini yonetir.</summary>
public class GameManager : MonoBehaviour
{
    [Header("Genel Ayarlar")]
    [Tooltip("Oyun suresi (saniye)")]
    public float gameTime = 30f;
    private float _timer;
    [Tooltip("Toplam sahnedeki hedef obje sayisi")]
    public int totalObjects;
    [Tooltip("Kactane yakalandigini izlemek icin (read-only runtime)")]
    public int caughtObjects;
    [Tooltip("Oyun kazanildi paneli (opsiyonel)")]
    public GameObject gameWon;
    [Tooltip("Oyun kaybedildi paneli (opsiyonel)")]
    public GameObject gameLost;

    [Header("UI Panelleri")]
    [Tooltip("Ayar paneli GameObject'i (opsiyonel)")]
    public GameObject settingsPanel;
    [Tooltip("Cikis onay paneli (opsiyonel)")]
    public GameObject exitPanel;

    [Header("Sahne")]
    [Tooltip("Menu butonuna basildiginda yuklenecek sahne adi")]
    public string menuSceneName = "menu";

    [Tooltip("Ayar paneli acildiginda Time.timeScale olarak atanacak deger. 0 dogrudan atamak UI etkilesimlerini engelliyorsa kucuk bir deger kullanin.")]
    public float pauseTimeScale = 1f;
    [Tooltip("Settings butonuna bastiktan sonra oyunun tamamen donmadan once gececek gercek-sure (saniye)")]
    public float freezeDelay = 0.5f;

    [Tooltip("Ana zaman gostergesi (TextMeshPro)")]
    public TextMeshProUGUI timerText;

    [Header("Guclendirmeler")]
    [Tooltip("Combo dondurucu suresi (saniye)")]
    public float comboFreezeDuration = 2f;
    [Tooltip("Inspector'da freeze butonu buraya ata (opsiyonel). Haklar bittiginde devre disi birakilir.")]
    public GameObject freezeButtonObject;
    [Tooltip("Freeze hak sayisini gosteren TMP (opsiyonel)")]
    public TextMeshProUGUI freezeUsesText;
    private int _freezeRemaining = 0;

    [Header("Devam Ayarlari")]
    [Tooltip("Bir devam icin gereken para miktari")]
    public int continueCost = 20;
    [Tooltip("Devam etmede eklenecek sure (saniye)")]
    public float continueTimeBonus = 20f;
    [Tooltip("Devam icin gecerli gecici mesaj Text (TMP)")]
    public TextMeshProUGUI continueMessageText;
    [Tooltip("Devam butonundaki ucret Text (TMP)")]
    public TextMeshProUGUI continueCostText;

    [Header("Extra Haklari")]
    [Tooltip("Eski ayar: Extra haklari artik shop satin alimi ile CurrencyWallet'tan gelir.")]
    public int extraUses = 1;
    private int _extraRemaining = 0;
    [Tooltip("Inspector'da ekstra butonu buraya ata (opsiyonel). Buton, haklar bittiginde devre disi birakilir.")]
    public GameObject extraButtonObject;
    [Tooltip("Extra hak sayisini gosteren TMP (opsiyonel)")]
    public TextMeshProUGUI extraUsesText;
    [Tooltip("Her kullanista eklenecek ekstra sure (saniye)")]
    public float extraTimeAmount = 10f;

    [Header("Extra UI")]
    [Tooltip("Opsiyonel: ekstra zamanin yazilacagi TMP Text. Atanmazsa ana timer guncellenir.")]
    public TextMeshProUGUI extraTargetTimerText;

    [Header("Zemin Secimi")]
    private Renderer floorRenderer;
    [Tooltip("Sahnedeki veya prefab olarak atanmis zemin UI Image objesi")]
    public GameObject floorUIImageObject;
    private Image _floorUIImageComp;
    [Tooltip("True ise zemin sadece UI butonlari uzerinden degistirilir")]
    public bool changeOnlyFromButtons = true;
    [Tooltip("Zemin secimi icin kullanilacak arka plan Image referanslari")]
    public Image[] bgImages;
    [Tooltip("3D floor prefab'i veya sahnedeki floor GameObject'i")]
    public GameObject floorObject;

    private bool _gameEnd = false;
    private int destroyOb = 0;
    private float _prevTimeScale = 1f;
    private bool _pausedBySettings = false;
    private Coroutine _freezeCoroutine = null;
    private bool _isFrozenBySettings = false;

    /// <summary>Oyun baslangicinda sureyi, panelleri, zemin referanslarini ve ekstra hakki hazirlar.</summary>
    private void Start()
    {
        _timer = gameTime;
        UpdateTimerUI();
        if (gameWon != null) gameWon.SetActive(false);
        else Debug.LogWarning("GameManager.gameWon inspector'da atanmis degil.");
        if (gameLost != null) gameLost.SetActive(false);
        else Debug.LogWarning("GameManager.gameLost inspector'da atanmis degil.");
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (exitPanel != null) exitPanel.SetActive(false);
        UpdateContinueCostUI();

        if (floorRenderer == null && floorObject != null)
        {
            floorRenderer = floorObject.GetComponentInChildren<Renderer>();
            if (floorRenderer == null)
                Debug.LogWarning("GameManager: floorObject icinde Renderer bulunamadi.");
        }

        if (_floorUIImageComp == null)
        {
            if (floorUIImageObject != null)
            {
                if (Application.isPlaying && !floorUIImageObject.scene.IsValid())
                {
                    Canvas c = FindFirstObjectByType<Canvas>();
                    GameObject parent = c != null ? c.gameObject : null;
                    GameObject inst = parent != null ? Instantiate(floorUIImageObject, parent.transform) : Instantiate(floorUIImageObject);
                    inst.name = floorUIImageObject.name + "_inst";
                    _floorUIImageComp = inst.GetComponentInChildren<Image>();
                }
                else
                {
                    _floorUIImageComp = floorUIImageObject.GetComponentInChildren<Image>();
                }
            }

            if (_floorUIImageComp == null && floorObject != null)
            {
                _floorUIImageComp = floorObject.GetComponentInChildren<Image>();
            }
        }

        RefreshPowerupUsesFromWallet();
        UpdatePowerupUI();
    }

    /// <summary>Inspector degisikliklerinde degerleri guvenli aralikta tutar ve referanslari tazeler.</summary>
    private void OnValidate()
    {
        UpdateContinueCostUI();
        extraUses = Mathf.Max(0, extraUses);
        if (floorRenderer == null && floorObject != null)
        {
            floorRenderer = floorObject.GetComponentInChildren<Renderer>();
            if (floorRenderer == null)
                Debug.LogWarning("GameManager: floorObject icinde Renderer bulunamadi.");
        }
        if (_floorUIImageComp == null)
        {
            if (floorUIImageObject != null)
            {
                _floorUIImageComp = floorUIImageObject.GetComponentInChildren<Image>();
            }
            if (_floorUIImageComp == null && floorObject != null)
            {
                _floorUIImageComp = floorObject.GetComponentInChildren<Image>();
            }
        }
    }

    /// <summary>Devam butonundaki ucret yazisini gunceller.</summary>
    private void UpdateContinueCostUI()
    {
        if (continueCostText != null)
        {
            continueCostText.text = "Devam Et (" + continueCost.ToString() + "$)";
        }
    }

    /// <summary>Her frame sureyi azaltir; sure biterse kaybetme durumunu baslatir.</summary>
    private void Update()
    {
        if (_gameEnd) return;

        _timer -= Time.deltaTime;
        _timer = Mathf.Max(_timer, 0f);
        UpdateTimerUI();

        if (_timer <= 0f)
        {
            GameLost();
        }
    }

    /// <summary>Sure degerini ekrandaki timer yazisina basar.</summary>
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(_timer).ToString();
        }
        else
        {
            Debug.LogWarning("GameManager: 'timerText' inspector'da atanmamis. Timer UI guncellenemiyor.");
        }
    }

    /// <summary>Aktif sahneyi yeniden yukleyerek oyunu sifirlar.</summary>
    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Menu butonundan cagrilir ve menu sahnesini yukler.</summary>
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    /// <summary>Yakalanan obje sayisini artirir ve toplam hedefe ulasinca oyunu kazandirir.</summary>
    public void ObjectCaught()
    {
        caughtObjects++;
        if (caughtObjects >= totalObjects)
        {
            GameWon();
        }
    }

    /// <summary>Oyunu kazanildi olarak bitirir ve kazanma panelini acar.</summary>
    private void GameWon()
    {
        if (_gameEnd)
            return;
        _gameEnd = true;
        Debug.Log("Oyun Kazanildi!");
        if (gameWon != null)
        {
            gameWon.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameManager.gameWon inspector'da atanmis degil.");
        }
    }

    /// <summary>Oyunu kaybedildi olarak bitirir ve kaybetme panelini acar.</summary>
    private void GameLost()
    {
        if (_gameEnd)
            return;
        _gameEnd = true;
        Debug.Log("Oyun Kaybedildi!");
        if (gameLost != null)
        {
            gameLost.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameManager.gameLost inspector'da atanmis degil.");
        }
    }

    /// <summary>Yeterli para varsa kaybetme ekranindan oyuna sure ekleyerek devam eder.</summary>
    public void ContinueFromLost()
    {
        if (ScoreManager.Instance == null)
        {
            StartCoroutine(ShowTempMessage("Para yetmiyor"));
            return;
        }

        if (ScoreManager.Instance.TrySpendMoney(continueCost))
        {
            _gameEnd = false;
            _timer += continueTimeBonus;
            if (gameLost != null)
            {
                gameLost.SetActive(false);
            }

            continueCost = Mathf.Max(1, continueCost * 2);
            UpdateContinueCostUI();
            Debug.Log("Para harcayarak devam edildi (yeniden baslatma yok). Yeni devam ucreti: " + continueCost);
        }
        else
        {
            StartCoroutine(ShowTempMessage("Para yetmiyor"));
        }
    }

    /// <summary>Devam butonlari icin ContinueFromLost metodunu cagiran kisa yoldur.</summary>
    public void TryContinue()
    {
        ContinueFromLost();
    }

    /// <summary>Kisa sureli bilgilendirme mesajini gosterip gizler.</summary>
    private IEnumerator ShowTempMessage(string msg)
    {
        if (continueMessageText == null)
            yield break;

        continueMessageText.text = msg;
        continueMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        continueMessageText.text = "";
        continueMessageText.gameObject.SetActive(false);
    }

    /// <summary>Eslesip yok edilen obje sayisini artirir; hedef tamamlaninca oyunu kazandirir.</summary>
    public void CaughtDestroy()
    {
        destroyOb += 2;
        if (destroyOb >= totalObjects && !_gameEnd)
        {
            GameWon();
        }
    }

    /// <summary>Ayar panelini aciksa kapatir, kapaliysa acar.</summary>
    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;
        if (settingsPanel.activeSelf) CloseSettingsPanel(); else OpenSettingsPanel();
    }

    /// <summary>Ayarlar butonundan cagrilir ve ayar panelini acar.</summary>
    public void OnSettingsButtonPressed()
    {
        OpenSettingsPanel();
    }

    /// <summary>Ayar panelindeki cikis/kapat butonundan cagrilir.</summary>
    public void OnSettingsExitButtonPressed()
    {
        CloseSettingsPanel();
    }

    /// <summary>Ayar panelini acar, zamani once ayarlanan degere ceker sonra dondurur.</summary>
    public void OpenSettingsPanel()
    {
        if (settingsPanel == null) return;

        if (!_gameEnd && Time.timeScale != pauseTimeScale)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = pauseTimeScale;
            _pausedBySettings = true;
        }
        settingsPanel.SetActive(true);

        if (_freezeCoroutine != null) StopCoroutine(_freezeCoroutine);
        _freezeCoroutine = StartCoroutine(FreezeAfterDelay());
    }

    /// <summary>Ayar panelini kapatir ve oyun zamanini eski haline getirir.</summary>
    public void CloseSettingsPanel()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(false);

        if (_freezeCoroutine != null)
        {
            StopCoroutine(_freezeCoroutine);
            _freezeCoroutine = null;
        }
        if (_isFrozenBySettings || _pausedBySettings)
        {
            Time.timeScale = _prevTimeScale;
            _pausedBySettings = false;
            _isFrozenBySettings = false;
        }
    }

    /// <summary>Ayar paneli acik kalirsa kisa bekleme sonrasi zamani tamamen durdurur.</summary>
    private IEnumerator FreezeAfterDelay()
    {
        yield return new WaitForSecondsRealtime(freezeDelay);
        if (settingsPanel == null || !settingsPanel.activeSelf)
        {
            _freezeCoroutine = null;
            yield break;
        }
        Time.timeScale = 0f;
        _isFrozenBySettings = true;
        _freezeCoroutine = null;
    }

    /// <summary>Cikis onay panelini acar veya kapatir.</summary>
    public void ToggleExitPanel()
    {
        if (exitPanel == null) return;
        exitPanel.SetActive(!exitPanel.activeSelf);
    }

    /// <summary>Cikis onay panelini acar.</summary>
    public void OpenExitPanel()
    {
        if (exitPanel == null) return;
        exitPanel.SetActive(true);
    }

    /// <summary>Cikis onay panelini kapatir.</summary>
    public void CloseExitPanel()
    {
        if (exitPanel == null) return;
        exitPanel.SetActive(false);
    }

    /// <summary>Editorde play modunu durdurur, build icinde uygulamadan cikar.</summary>
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
    }

    /// <summary>Aktif combo suresini belirlenen sure kadar dondurur.</summary>
    public void UseComboFreeze()
    {
        if (ScoreManager.Instance == null) return;

        RefreshPowerupUsesFromWallet();
        if (_freezeRemaining <= 0)
        {
            StartCoroutine(ShowTempMessage("Freeze hakki yok"));
            UpdatePowerupUI();
            return;
        }

        if (!ScoreManager.Instance.TryPauseCombo(comboFreezeDuration))
        {
            StartCoroutine(ShowTempMessage("Aktif combo yok"));
            return;
        }

        CurrencyWallet.TryUseFreeze();
        RefreshPowerupUsesFromWallet();
        UpdatePowerupUI();
        Debug.Log("Combo Freeze kullanildi: " + comboFreezeDuration + "s, remaining: " + _freezeRemaining);
    }

    /// <summary>Hak varsa oyuna ekstra sure ekler ve hak bitince butonu kapatir.</summary>
    public void UseExtraTimeOnce()
    {
        if (_gameEnd) return;
        RefreshPowerupUsesFromWallet();
        if (_extraRemaining <= 0)
        {
            StartCoroutine(ShowTempMessage("Ekstra hak kalmadi"));
            UpdatePowerupUI();
            return;
        }
        CurrencyWallet.TryUseExtra();
        RefreshPowerupUsesFromWallet();

        float newTimeValue = _timer;
        if (extraTargetTimerText != null)
        {
            if (!float.TryParse(extraTargetTimerText.text, out newTimeValue))
            {
                newTimeValue = _timer;
            }
        }
        newTimeValue += extraTimeAmount;

        _timer = newTimeValue;
        if (timerText != null)
            timerText.text = Mathf.Ceil(_timer).ToString();

        if (extraTargetTimerText != null)
            extraTargetTimerText.text = Mathf.CeilToInt(newTimeValue).ToString();

        UpdatePowerupUI();

        string remText = _extraRemaining > 0 ? $" ({_extraRemaining} hak kaldi)" : "";
        StartCoroutine(ShowTempMessage("+" + Mathf.CeilToInt(extraTimeAmount) + " saniye eklendi" + remText));
        Debug.Log("Extra time used: " + extraTimeAmount + "s, remaining: " + _extraRemaining);
    }

    /// <summary>Kalici kayittaki guclendirme haklarini runtime sayaclara ceker.</summary>
    private void RefreshPowerupUsesFromWallet()
    {
        _extraRemaining = CurrencyWallet.ExtraUses;
        _freezeRemaining = CurrencyWallet.FreezeUses;
    }

    /// <summary>Guclendirme butonlarini ve hak sayisi yazilarini gunceller.</summary>
    private void UpdatePowerupUI()
    {
        if (extraButtonObject != null)
        {
            Button button = extraButtonObject.GetComponent<Button>();
            if (button != null)
                button.interactable = _extraRemaining > 0;
            else
                extraButtonObject.SetActive(_extraRemaining > 0);
        }

        if (freezeButtonObject != null)
        {
            Button button = freezeButtonObject.GetComponent<Button>();
            if (button != null)
                button.interactable = _freezeRemaining > 0;
            else
                freezeButtonObject.SetActive(_freezeRemaining > 0);
        }

        if (extraUsesText != null)
            extraUsesText.text = _extraRemaining.ToString();

        if (freezeUsesText != null)
            freezeUsesText.text = _freezeRemaining.ToString();
    }

    /// <summary>Verilen index ile bgImages listesinden zemin gorseli secer.</summary>
    public void SetFloorSpriteByIndex(int index)
    {
        if (bgImages == null) return;
        if (index < 0 || index >= bgImages.Length) return;

        Sprite s = bgImages[index]?.sprite;
        if (s == null) return;

        ApplyFloorSprite(s, true);
    }

    /// <summary>Verilen Image uzerindeki sprite'i zemin gorseli olarak uygular.</summary>
    public void SetFloorFromImage(Image img)
    {
        if (img == null || img.sprite == null) return;
        ApplyFloorSprite(img.sprite, true);
    }

    /// <summary>Secilen sprite'i UI zeminine ve varsa 3D floor materyaline uygular.</summary>
    private void ApplyFloorSprite(Sprite s, bool fromButton)
    {
        if (s == null) return;
        if (changeOnlyFromButtons && !fromButton) return;

        if (_floorUIImageComp != null) _floorUIImageComp.sprite = s;

        if (floorRenderer != null && s.texture != null)
        {
            Material[] mats = floorRenderer.materials;
            bool applied = false;
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                if (mat == null) continue;

                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", s.texture);
                    applied = true;
                }
                else if (mat.HasProperty("_MainTex"))
                {
                    mat.SetTexture("_MainTex", s.texture);
                    applied = true;
                }
                else
                {
                    try
                    {
                        mat.mainTexture = s.texture;
                        applied = true;
                    }
                    catch (System.Exception exception)
                    {
                        Debug.LogWarning("GameManager: floor texture uygulanamadi: " + exception.Message);
                    }
                }
            }
            if (applied)
            {
                floorRenderer.materials = mats;
            }
            else
            {
                Debug.LogWarning("GameManager: floorRenderer materyallerinde uygun texture property bulunamadi (orn. _BaseMap/_MainTex).");
            }
        }
    }

    /// <summary>Birinci zemin gorselini secer.</summary>
    public void SetBG0() { SetFloorSpriteByIndex(0); }
    /// <summary>Ikinci zemin gorselini secer.</summary>
    public void SetBG1() { SetFloorSpriteByIndex(1); }
    /// <summary>Ucuncu zemin gorselini secer.</summary>
    public void SetBG2() { SetFloorSpriteByIndex(2); }
    /// <summary>Dorduncu zemin gorselini secer.</summary>
    public void SetBG3() { SetFloorSpriteByIndex(3); }
}
