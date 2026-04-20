using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <remarks>Oyun yonetimi: zaman, paneller, devam ve zemin islemleri.</remarks>
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
   public GameObject exitPanel; // onay paneli veya cikis penceresi

   [Tooltip("Ayar paneli acildiginda Time.timeScale olarak atanacak deger. 0 dogrudan atamak UI etkilesimlerini engelliyorsa kucuk bir deger kullanin.")]
   public float pauseTimeScale = 1f;
   [Tooltip("Settings butonuna bastiktan sonra oyunun tamamen donmadan once gececek gercek-sure (saniye)")]
   public float freezeDelay = 0.5f;

   [Tooltip("Ana zaman gostergesi (TextMeshPro)")]
   public TextMeshProUGUI timerText;

   [Header("Guclendirmeler")]
   [Tooltip("Combo dondurucu suresi (saniye)")]
   public float comboFreezeDuration = 2f; // saniye

   [Header("Devam Ayarlari")]
   [Tooltip("Bir devam icin gereken para miktari")]
   public int continueCost = 20;
   [Tooltip("Devam etmede eklenecek sure (saniye)")]
   public float continueTimeBonus = 20f;
   [Tooltip("Devam icin gecerli gecici mesaj Text (TMP)")]
   public TextMeshProUGUI continueMessageText; // yetersiz bakiye icin gecici mesaj
   [Tooltip("Devam butonundaki ucret Text (TMP)")]
   public TextMeshProUGUI continueCostText; // buton uzerinde veya UI'da gosterilecek ucret

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
   // artik inspector'da gorunmesin; sahnedeki floorObject'tan veya prefab'tan runtime'ta bulunur
   // 3D floor prefab icindeki cube gibi obje icin Renderer kullaniyoruz (MeshRenderer/SkinnedMeshRenderer)
   private Renderer floorRenderer;
   // Atanabilecek: sahnedeki Image iceren GameObject veya prefab (Image iceren)
   public GameObject floorUIImageObject;
   private Image _floorUIImageComp;
   // Eger true ise sadece buton tiklamasiyla floor degistirilebilir
   public bool changeOnlyFromButtons = true;
   public Image[] bgImages;
   public GameObject floorObject; // floor prefab veya floor GameObject (cube child icerir)

   private bool _gameEnd = false;
   private int destroyOb = 0;
   // zaman olcegini geri almak icin saklanan deger
   private float _prevTimeScale = 1f;
   private bool _pausedBySettings = false;
   private Coroutine _freezeCoroutine = null;
   private bool _isFrozenBySettings = false;

   /// <remarks>Baslangicta timer ve referanslari ayarlar.</remarks>
   void Start()
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

      // Eger inspector'da atanmadiysa, floorObject uzerinden child SpriteRenderer veya Image bagla
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
            // Eger inspector'a bir prefab (asset) atadiysaniz, prefab.asset'in scene'i gecerli olmayacaktir.
            // Bu durumda runtime'da prefab'in bir ornegini olusturup Image bilesenini ondan aliyoruz.
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

         // extra haklarini baslangicta ayarla
         _extraRemaining = Mathf.Max(0, extraUses);
         if (extraButtonObject != null)
            extraButtonObject.SetActive(_extraRemaining > 0);
      }
   }

   /// <remarks>Inspector degisikliklerinde edit-time atamalari gunceller.</remarks>
   private void OnValidate()
   {
      UpdateContinueCostUI();
      // Edit-time automatic assignment so the inspector field won't stay empty
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

   /// <remarks>Devam butonu ucret metnini gunceller.</remarks>
   private void UpdateContinueCostUI()
   {
      if (continueCostText != null)
      {
         continueCostText.text = "Devam Et (" + continueCost.ToString() + "$)";
      }
   }
   /// <remarks>Her frame oyun zamanlayicisini gunceller ve kayip kontrolleri yapar.</remarks>
   void Update()
   {
      if (_gameEnd) return;
      // zamanlayicinin zamanini dusurur
      _timer -= Time.deltaTime;
      _timer = Mathf.Max(_timer, 0f);
      UpdateTimerUI();

      if (_timer <= 0f)
      {
         GameLost();
      }

   }
   /// <remarks>Timer metnini ekranda gunceller.</remarks>
   void UpdateTimerUI()
   {
      // zamanlayicinin ekranda gorunmesi icin
      if (timerText != null)
      {
         timerText.text = Mathf.Ceil(_timer).ToString();
      }
      else
      {
         Debug.LogWarning("GameManager: 'timerText' inspector'da atanmamis. Timer UI guncellenemiyor.");
      }
   }
   /// <remarks>Oyunu yeniden yukleyerek sifirlar.</remarks>
   public void ResetGame()
   {
      // oyunu resetler
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
   }
   /// <remarks>Bir obje yakalandiginda sayaci artirir.</remarks>
   public void ObjectCaught()
   {
      caughtObjects++;
      if (caughtObjects >= totalObjects)
      {
         GameWon();
      }
   }
   /// <remarks>Oyun kazanma durumunu ayarlar.</remarks>
   void GameWon()
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
   /// <remarks>Oyun kaybetme durumunu ayarlar.</remarks>
   void GameLost()
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

   // Yeniden baslatmadan devam: para harcanarak oyuna kaldigi yerden devam et
   /// <remarks>Para harcanarak oyuna kaldigi yerden devam eder.</remarks>
   public void ContinueFromLost()
   {
      if (ScoreManager.Instance == null)
      {
         StartCoroutine(ShowTempMessage("Para yetmiyor"));
         return;
      }

      if (ScoreManager.Instance.TrySpendMoney(continueCost))
      {
         // Para basariyla harcandi: sahneyi yeniden yuklemeden oyuna devam et
         _gameEnd = false;
         _timer += continueTimeBonus;
         if (gameLost != null)
         {
            gameLost.SetActive(false);
         }
         // ucret iki katina cikar (sonraki devam icin)
         continueCost = Mathf.Max(1, continueCost * 2);
         UpdateContinueCostUI();
         // oyun kaldigi yerden devam eder (timeScale ile oynanmaz)
         Debug.Log("Para harcayarak devam edildi (yeniden baslatma yok). Yeni devam ucreti: " + continueCost);
      }
      else
      {
         // Yetersiz para: gecici mesaj goster
         StartCoroutine(ShowTempMessage("Para yetmiyor"));
      }
   }

   // Uyumluluk icin eski adi koru; simdi ContinueFromLost()'a yonlendirir
   /// <remarks>Uyumluluk icin devam metodunu cagiran sarmalayıcı.</remarks>
   public void TryContinue()
   {
      ContinueFromLost();
   }

   /// <remarks>Gecici mesaj gosterir (TMP) ve sonradan gizler.</remarks>
   private System.Collections.IEnumerator ShowTempMessage(string msg)
   {
      if (continueMessageText == null)
         yield break;

      continueMessageText.text = msg;
      continueMessageText.gameObject.SetActive(true);
      yield return new WaitForSeconds(2f);
      continueMessageText.text = "";
      continueMessageText.gameObject.SetActive(false);
   }

   /// <remarks>Yakalanan nesne sayisini arttirir ve galibiyeti kontrol eder.</remarks>
   public void CaughtDestroy()
   {
      destroyOb += 2;
      // tum objeleri yakaladi mi diye kontrol eder, yakaladiysa GameWon olur
      if (destroyOb >= totalObjects && !_gameEnd)
      {
         GameWon();
         // Time.timeScale = 0f;
      }
   }

   // Settings panel toggle
   /// <remarks>Settings panelini acar/kapatir.</remarks>
   public void ToggleSettingsPanel()
   {
      if (settingsPanel == null) return;
      if (settingsPanel.activeSelf) CloseSettingsPanel(); else OpenSettingsPanel();
   }

   // Button handler: open the settings panel
   /// <remarks>Settings acma butonunun Unity event handler'i.</remarks>
   public void OnSettingsButtonPressed()
   {
      OpenSettingsPanel();
   }

   // Button handler: close the settings panel (e.g. Exit button inside panel)
   /// <remarks>Settings paneli icindeki cikis butonunun handler'i.</remarks>
   public void OnSettingsExitButtonPressed()
   {
      CloseSettingsPanel();
   }

   /// <remarks>Settings panelini acar, zamani yavaslatir ve freeze coroutine baslatir.</remarks>
   public void OpenSettingsPanel()
   {
      if (settingsPanel == null) return;
      // yalnizca oyun devam ediyorsa zamani yavaslat (inspectordan ayarlanabilir deger kullan)
      if (!_gameEnd && Time.timeScale != pauseTimeScale)
      {
         _prevTimeScale = Time.timeScale;
         Time.timeScale = pauseTimeScale;
         _pausedBySettings = true;
      }
      settingsPanel.SetActive(true);
      // belirlenen gecikme sonra tamamen dondurmak icin coroutine baslat
      if (_freezeCoroutine != null) StopCoroutine(_freezeCoroutine);
      _freezeCoroutine = StartCoroutine(FreezeAfterDelay());
   }

   /// <remarks>Settings panelini kapatir ve zamani eski haline getirir.</remarks>
   public void CloseSettingsPanel()
   {
      if (settingsPanel == null) return;
      settingsPanel.SetActive(false);
      // eger hala gecikmeli donma coroutine'i calisiyorsa iptal et
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

   /// <remarks>Gercek-sure bekleyip zamani tamamen dondurur (Time.timeScale=0).</remarks>
   private System.Collections.IEnumerator FreezeAfterDelay()
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

   // Exit panel (confirmation) toggle
   /// <remarks>Exit onay panelini acar/kapatir.</remarks>
   public void ToggleExitPanel()
   {
      if (exitPanel == null) return;
      exitPanel.SetActive(!exitPanel.activeSelf);
   }

   /// <remarks>Exit onay panelini acar.</remarks>
   public void OpenExitPanel()
   {
      if (exitPanel == null) return;
      exitPanel.SetActive(true);
   }

   /// <remarks>Exit onay panelini kapatir.</remarks>
   public void CloseExitPanel()
   {
      if (exitPanel == null) return;
      exitPanel.SetActive(false);
   }

   // UI'deki Cikis butonuna baglayin
   /// <remarks>Editor veya build icin oyunu kapatma handler'i.</remarks>
   public void ExitGame()
   {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
   }
   // UI butonuna baglanacak: combo zamanlayicisini belirtilen sure dondurur
   /// <remarks>Combo zamanlayicisini belirtilen sure dondurur.</remarks>
   public void UseComboFreeze()
   {
      if (ScoreManager.Instance == null) return;
      ScoreManager.Instance.PauseCombo(comboFreezeDuration);
      Debug.Log("Combo Freeze kullanildi: " + comboFreezeDuration + "s");
   }

   // UI butonuna baglanacak: ekstra zaman haklarini kullanir
   /// <remarks>Bir kereye mahsus ekstra zaman hakki kullanir.</remarks>
   public void UseExtraTimeOnce()
   {
      if (_gameEnd) return;
      if (_extraRemaining <= 0)
      {
         StartCoroutine(ShowTempMessage("Ekstra hak kalmadi"));
         return;
      }
      _extraRemaining--;
      // Hedef text atanmissa, onunkine ekle; degilse ana timer'a ekle
      float newTimeValue = _timer;
      if (extraTargetTimerText != null)
      {
         if (!float.TryParse(extraTargetTimerText.text, out newTimeValue))
         {
            newTimeValue = _timer;
         }
      }
      newTimeValue += extraTimeAmount;
      // Uygula ana timer'a da yaz
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

   // UI uzerinden floor sprite'ini BG gorsellerinden ayarlamak icin
   /// <remarks>Index ile bgImages dizisinden sprite secip zemine uygular.</remarks>
   public void SetFloorSpriteByIndex(int index)
   {
      if (bgImages == null) return;
      if (index < 0 || index >= bgImages.Length) return;

      Sprite s = bgImages[index]?.sprite;
      if (s == null) return;

      ApplyFloorSprite(s, true);
   }

   // Dogrudan bir Image referansindan floor'u ayarlamak icin
   /// <remarks>Verilen Image bileşenindeki sprite'i zemine uygular.</remarks>
   public void SetFloorFromImage(Image img)
   {
      if (img == null || img.sprite == null) return;
      ApplyFloorSprite(img.sprite, true);
   }

   // Merkezi islem: sadece buton tiklamasiyla degisim izni verilebilir
   /// <remarks>Sprite'tan texture alir ve 3D floor materyaline atar (URP/STD uyumlu).</remarks>
   private void ApplyFloorSprite(Sprite s, bool fromButton)
   {
      if (s == null) return;
      if (changeOnlyFromButtons && !fromButton) return;

      if (_floorUIImageComp != null) _floorUIImageComp.sprite = s;
      // 3D floor icin sprite'tan texture alip floor materyaline uygula (varsa)
      if (floorRenderer != null && s.texture != null)
      {
         Material[] mats = floorRenderer.materials; // renderer.materials returns instances
         bool applied = false;
         for (int i = 0; i < mats.Length; i++)
         {
            Material mat = mats[i];
            if (mat == null) continue;
            // URP/HDRP ve yeni shader'larda bazen '_BaseMap' kullaniliyor
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
            // yeniden ata ki instance materyaller guncellensin
            floorRenderer.materials = mats;
         }
         else
         {
            Debug.LogWarning("GameManager: floorRenderer materyallerinde uygun texture property bulunamadi (orn. _BaseMap/_MainTex).");
         }
      }
   }

   // Kolay baglama icin index bazli kisa metodlar (Inspector'da dogrudan secmek icin)
   /// <remarks>Kisa BG atama yardimci metodlari.</remarks>
   public void SetBG0() { SetFloorSpriteByIndex(0); }
   public void SetBG1() { SetFloorSpriteByIndex(1); }
   public void SetBG2() { SetFloorSpriteByIndex(2); }
   public void SetBG3() { SetFloorSpriteByIndex(3); }
}
