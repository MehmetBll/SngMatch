using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float gameTime = 30f;
    private float _timer;
    public int totalObjects;
    public int caughtObjects;
    public GameObject gameWon;
    public GameObject gameLost;
    public TextMeshProUGUI timerText;
   [Header("Devam Ayarları")] 
   public int continueCost = 20;
   public float continueTimeBonus = 20f;
   public TextMeshProUGUI continueMessageText; // yetersiz bakiye için geçici mesaj
   public TextMeshProUGUI continueCostText; // buton üzerinde veya UI'da gösterilecek ücret
    private bool _gameEnd = false;
    private int destroyOb = 0;
         void Start()
         {
            _timer = gameTime;
            UpdateTimerUI();
            gameWon.SetActive(false);
            gameLost.SetActive(false);
            UpdateContinueCostUI();
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
            timerText.text = Mathf.Ceil(_timer).ToString();
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
            if(_gameEnd) 
               return;
            _gameEnd = true;
            Debug.Log("Game Won!");
            gameWon.SetActive(true);
         }
         void GameLost()
         {
            if(_gameEnd) 
               return;
            _gameEnd = true;
            Debug.Log("Game Lost!");
            gameLost.SetActive(true);
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
      destroyOb +=2;
      //tüm objeleri yakaladımı diye kontrol eder yakaladıysa GameWon olur
      if (destroyOb >= totalObjects && !_gameEnd)
      {
          GameWon();
          //Time.timeScale = 0f;
      }
   }
}
