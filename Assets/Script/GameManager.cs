using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
   public float gameTime = 30f;
   private float _timer;
   public int totalObjects;
   public int caughtObjects;
   public GameObject gameWon;
   public GameObject gameLost;
   [Header("UI Panels")]
   public GameObject settingsPanel;
   public GameObject exitPanel; // onay paneli veya çıkış penceresi
   public TextMeshProUGUI timerText;
   [Header("Powerups")]
   public float comboFreezeDuration = 2f; // saniye
   [Header("Devam Ayarları")]
   public int continueCost = 20;
   public float continueTimeBonus = 20f;
   public TextMeshProUGUI continueMessageText; // yetersiz bakiye için geçici mesaj
   public TextMeshProUGUI continueCostText; // buton üzerinde veya UI'da gösterilecek ücret
      [Header("Floor Selection")]
      public SpriteRenderer floorSpriteRenderer;
      public Image floorUIImage;
      public Image[] bgImages;
      public GameObject floorObject; // floor prefab veya floor GameObject (cube child içerir)
   private bool _gameEnd = false;
   private int destroyOb = 0;
   void Start()
   {
      _timer = gameTime;
      UpdateTimerUI();
      if (gameWon != null) gameWon.SetActive(false);
      else Debug.LogWarning("GameManager.gameWon is not assigned in the inspector.");
      if (gameLost != null) gameLost.SetActive(false);
      else Debug.LogWarning("GameManager.gameLost is not assigned in the inspector.");
      if (settingsPanel != null) settingsPanel.SetActive(false);
      if (exitPanel != null) exitPanel.SetActive(false);
      UpdateContinueCostUI();
      // Eğer inspector'da atanmadıysa, floorObject üzerinden child SpriteRenderer veya Image bağla
      if (floorSpriteRenderer == null && floorObject != null)
      {
         floorSpriteRenderer = floorObject.GetComponentInChildren<SpriteRenderer>();
         if (floorSpriteRenderer == null)
            Debug.LogWarning("GameManager: floorObject içinde SpriteRenderer bulunamadı.");
      }
      if (floorUIImage == null && floorObject != null)
      {
         floorUIImage = floorObject.GetComponentInChildren<Image>();
      }
   }

   private void OnValidate()
   {
      UpdateContinueCostUI();
   }

   private void UpdateContinueCostUI()
   {
      if (continueCostText != null)
      {
         continueCostText.text = "Devam Et (" + continueCost.ToString() + "$)";
      }
   }
   void Update()
   {
      if (_gameEnd) return;
      //zamanlayıcının zamanınını düşürür
      _timer -= Time.deltaTime;
      _timer = Mathf.Max(_timer, 0f);
      UpdateTimerUI();

      if (_timer <= 0f)
      {
         GameLost();
      }


   }
   void UpdateTimerUI()
   {
      //zamnalayıcının ekranda gözükmesi için
      if (timerText != null)
      {
         timerText.text = Mathf.Ceil(_timer).ToString();
      }
      else
      {
         Debug.LogWarning("GameManager: 'timerText' inspector'da atanmamış. Timer UI güncellenemiyor.");
      }
   }
   public void ResetGame()
   {
      //oyunu resetler 
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
   }
   public void ObjectCaught()
   {
      caughtObjects++;
      if (caughtObjects >= totalObjects)
      {
         GameWon();
      }
   }
   void GameWon()
   {
      if (_gameEnd)
         return;
      _gameEnd = true;
      Debug.Log("Game Won!");
      if (gameWon != null)
      {
         gameWon.SetActive(true);
      }
      else
      {
         Debug.LogWarning("GameManager.gameWon is not assigned in the inspector.");
      }
   }
   void GameLost()
   {
      if (_gameEnd)
         return;
      _gameEnd = true;
      Debug.Log("Game Lost!");
      if (gameLost != null)
      {
         gameLost.SetActive(true);
      }
      else
      {
         Debug.LogWarning("GameManager.gameLost is not assigned in the inspector.");
      }
   }

   // Yeniden başlatmadan devam: para harcanarak oyuna kaldığı yerden devam et
   public void ContinueFromLost()
   {
      if (ScoreManager.Instance == null)
      {
         StartCoroutine(ShowTempMessage("Para yetmiyor"));
         return;
      }

      if (ScoreManager.Instance.TrySpendMoney(continueCost))
      {
         // Para başarıyla harcandı: sahneyi yeniden yüklemeden oyuna devam et
         _gameEnd = false;
         _timer += continueTimeBonus;
         if (gameLost != null)
         {
            gameLost.SetActive(false);
         }
         // ücret iki katına çıkar (sonraki devam için)
         continueCost = Mathf.Max(1, continueCost * 2);
         UpdateContinueCostUI();
         // oyun kaldığı yerden devam eder (timeScale ile oynanmaz)
         Debug.Log("Para harcayarak devam edildi (yeniden başlatma yok). Yeni devam ücreti: " + continueCost);
      }
      else
      {
         // Yetersiz para: geçici mesaj göster
         StartCoroutine(ShowTempMessage("Para yetmiyor"));
      }
   }

   // Uyumluluk için eski adı koru; şimdi ContinueFromLost()'a yönlendirir
   public void TryContinue()
   {
      ContinueFromLost();
   }

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

   public void CaughtDestroy()
   {
      destroyOb += 2;
      //tüm objeleri yakaladımı diye kontrol eder yakaladıysa GameWon olur
      if (destroyOb >= totalObjects && !_gameEnd)
      {
         GameWon();
         //Time.timeScale = 0f;
      }
   }

   // Settings panel toggle
   public void ToggleSettingsPanel()
   {
      if (settingsPanel == null) return;
      settingsPanel.SetActive(!settingsPanel.activeSelf);
   }

   // Button handler: open the settings panel
   public void OnSettingsButtonPressed()
   {
      OpenSettingsPanel();
   }

   // Button handler: close the settings panel (e.g. Exit button inside panel)
   public void OnSettingsExitButtonPressed()
   {
      CloseSettingsPanel();
   }

   public void OpenSettingsPanel()
   {
      if (settingsPanel == null) return;
      settingsPanel.SetActive(true);
   }

   public void CloseSettingsPanel()
   {
      if (settingsPanel == null) return;
      settingsPanel.SetActive(false);
   }

   // Exit panel (confirmation) toggle
   public void ToggleExitPanel()
   {
      if (exitPanel == null) return;
      exitPanel.SetActive(!exitPanel.activeSelf);
   }

   public void OpenExitPanel()
   {
      if (exitPanel == null) return;
      exitPanel.SetActive(true);
   }

   public void CloseExitPanel()
   {
      if (exitPanel == null) return;
      exitPanel.SetActive(false);
   }

   // UI'deki Çıkış butonuna bağlayın
   public void ExitGame()
   {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
   }
   // UI butonuna bağlanacak: combo zamanlayıcısını belirtilen süre dondurur
   public void UseComboFreeze()
   {
      if (ScoreManager.Instance == null) return;
      ScoreManager.Instance.PauseCombo(comboFreezeDuration);
      Debug.Log("Combo dondurucu kullanıldı: " + comboFreezeDuration + "s");
   }

    // UI üzerinden floor sprite'ını BG görsellerinden ayarlamak için
    public void SetFloorSpriteByIndex(int index)
    {
       if (bgImages == null) return;
       if (index < 0 || index >= bgImages.Length) return;

       Sprite s = bgImages[index]?.sprite;
       if (s == null) return;

       if (floorUIImage != null) floorUIImage.sprite = s;
       if (floorSpriteRenderer != null) floorSpriteRenderer.sprite = s;
    }

    // Doğrudan bir Image referansından floor'u ayarlamak için
    public void SetFloorFromImage(Image img)
    {
       if (img == null || img.sprite == null) return;
       if (floorUIImage != null) floorUIImage.sprite = img.sprite;
       if (floorSpriteRenderer != null) floorSpriteRenderer.sprite = img.sprite;
    }

    // Kolay bağlama için index bazlı kısa metodlar (Inspector'da doğrudan seçmek için)
    public void SetBG0() { SetFloorSpriteByIndex(0); }
    public void SetBG1() { SetFloorSpriteByIndex(1); }
    public void SetBG2() { SetFloorSpriteByIndex(2); }
    public void SetBG3() { SetFloorSpriteByIndex(3); }
}
