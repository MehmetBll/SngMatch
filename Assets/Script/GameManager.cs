using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Oyun süresi, kazanma/kaybetme, devam etme ve panel akışını yönetir.</summary>
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
   [Tooltip("Bir oyuncunun sahip oldugu ekstra kullanma hak sayisi (adet)")]
   public int extraUses = 1;
   private int _extraRemaining = 0;
   [Tooltip("Inspector'da ekstra butonu buraya ata (opsiyonel). Buton, haklar bittiginde devre disi birakilir.")]
   public GameObject extraButtonObject;
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

   /// <summary>Oyun başlangıcında süreyi, panelleri, zemin referanslarını ve ekstra hakkı hazırlar.</summary>
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

      _extraRemaining = Mathf.Max(0, extraUses);
      if (extraButtonObject != null)
         extraButtonObject.SetActive(_extraRemaining > 0);
   }

   /// <summary>Inspector değişikliklerinde değerleri güvenli aralıkta tutar ve referansları tazeler.</summary>
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

   /// <summary>Devam butonundaki ücret yazısını günceller.</summary>
   private void UpdateContinueCostUI()
   {
      if (continueCostText != null)
      {
         continueCostText.text = "Devam Et (" + continueCost.ToString() + "$)";
      }
   }

   /// <summary>Her frame süreyi azaltır; süre biterse kaybetme durumunu başlatır.</summary>
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

   /// <summary>Süre değerini ekrandaki timer yazısına basar.</summary>
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

   /// <summary>Aktif sahneyi yeniden yükleyerek oyunu sıfırlar.</summary>
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

   /// <summary>Yakalanan obje sayısını artırır ve toplam hedefe ulaşınca oyunu kazandırır.</summary>
   public void ObjectCaught()
   {
      caughtObjects++;
      if (caughtObjects >= totalObjects)
      {
         GameWon();
      }
   }

   /// <summary>Oyunu kazanıldı olarak bitirir ve kazanma panelini açar.</summary>
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

   /// <summary>Oyunu kaybedildi olarak bitirir ve kaybetme panelini açar.</summary>
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

   /// <summary>Yeterli para varsa kaybetme ekranından oyuna süre ekleyerek devam eder.</summary>
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

   /// <summary>Devam butonları için ContinueFromLost metodunu çağıran kısa yoldur.</summary>
   public void TryContinue()
   {
      ContinueFromLost();
   }

   /// <summary>Kısa süreli bilgilendirme mesajını gösterip gizler.</summary>
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

   /// <summary>Eşleşip yok edilen obje sayısını artırır; hedef tamamlanınca oyunu kazandırır.</summary>
   public void CaughtDestroy()
   {
      destroyOb += 2;
      if (destroyOb >= totalObjects && !_gameEnd)
      {
         GameWon();
      }
   }

   /// <summary>Ayar panelini açıksa kapatır, kapalıysa açar.</summary>
   public void ToggleSettingsPanel()
   {
      if (settingsPanel == null) return;
      if (settingsPanel.activeSelf) CloseSettingsPanel(); else OpenSettingsPanel();
   }

   /// <summary>Ayarlar butonundan çağrılır ve ayar panelini açar.</summary>
   public void OnSettingsButtonPressed()
   {
      OpenSettingsPanel();
   }

   /// <summary>Ayar panelindeki çıkış/kapat butonundan çağrılır.</summary>
   public void OnSettingsExitButtonPressed()
   {
      CloseSettingsPanel();
   }

   /// <summary>Ayar panelini açar, zamanı önce ayarlanan değere çeker sonra dondurur.</summary>
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

   /// <summary>Ayar panelini kapatır ve oyun zamanını eski haline getirir.</summary>
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

   /// <summary>Ayar paneli açık kalırsa kısa bekleme sonrası zamanı tamamen durdurur.</summary>
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

   /// <summary>Çıkış onay panelini açar veya kapatır.</summary>
   public void ToggleExitPanel()
   {
      if (exitPanel == null) return;
      exitPanel.SetActive(!exitPanel.activeSelf);
   }

   /// <summary>Çıkış onay panelini açar.</summary>
   public void OpenExitPanel()
   {
      if (exitPanel == null) return;
      exitPanel.SetActive(true);
   }

   /// <summary>Çıkış onay panelini kapatır.</summary>
   public void CloseExitPanel()
   {
      if (exitPanel == null) return;
      exitPanel.SetActive(false);
   }

   /// <summary>Editörde play modunu durdurur, build içinde uygulamadan çıkar.</summary>
   public void ExitGame()
   {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
   }

   /// <summary>Aktif combo süresini belirlenen süre kadar dondurur.</summary>
   public void UseComboFreeze()
   {
      if (ScoreManager.Instance == null) return;
      ScoreManager.Instance.PauseCombo(comboFreezeDuration);
      Debug.Log("Combo Freeze kullanildi: " + comboFreezeDuration + "s");
   }

   /// <summary>Hak varsa oyuna ekstra süre ekler ve hak bitince butonu kapatır.</summary>
   public void UseExtraTimeOnce()
   {
      if (_gameEnd) return;
      if (_extraRemaining <= 0)
      {
         StartCoroutine(ShowTempMessage("Ekstra hak kalmadi"));
         return;
      }
      _extraRemaining--;

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

      if (extraButtonObject != null && _extraRemaining <= 0)
      {
         extraButtonObject.SetActive(false);
      }

      string remText = _extraRemaining > 0 ? $" ({_extraRemaining} hak kaldı)" : "";
      StartCoroutine(ShowTempMessage("+" + Mathf.CeilToInt(extraTimeAmount) + " saniye eklendi" + remText));
      Debug.Log("Extra time used: " + extraTimeAmount + "s, remaining: " + _extraRemaining);
   }

   /// <summary>Verilen index ile bgImages listesinden zemin görseli seçer.</summary>
   public void SetFloorSpriteByIndex(int index)
   {
      if (bgImages == null) return;
      if (index < 0 || index >= bgImages.Length) return;

      Sprite s = bgImages[index]?.sprite;
      if (s == null) return;

      ApplyFloorSprite(s, true);
   }

   /// <summary>Verilen Image üzerindeki sprite'ı zemin görseli olarak uygular.</summary>
   public void SetFloorFromImage(Image img)
   {
      if (img == null || img.sprite == null) return;
      ApplyFloorSprite(img.sprite, true);
   }

   /// <summary>Seçilen sprite'ı UI zeminine ve varsa 3D floor materyaline uygular.</summary>
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
               catch { }
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

   /// <summary>Birinci zemin görselini seçer.</summary>
   public void SetBG0() { SetFloorSpriteByIndex(0); }
   /// <summary>İkinci zemin görselini seçer.</summary>
   public void SetBG1() { SetFloorSpriteByIndex(1); }
   /// <summary>Üçüncü zemin görselini seçer.</summary>
   public void SetBG2() { SetFloorSpriteByIndex(2); }
   /// <summary>Dördüncü zemin görselini seçer.</summary>
   public void SetBG3() { SetFloorSpriteByIndex(3); }
}
