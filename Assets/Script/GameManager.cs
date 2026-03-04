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
   [Header("Continue Settings")] 
   public int continueCost = 20;
   public float continueTimeBonus = 20f;
   public TextMeshProUGUI continueMessageText; // temporary message for insufficient funds
    private bool _gameEnd = false;
    private int destroyOb = 0;
         void Start()
         {
            _timer = gameTime;
            UpdateTimerUI();
            gameWon.SetActive(false);
            gameLost.SetActive(false);
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

      // Non-restarting continue: resume gameplay from the current state by spending money
      public void ContinueFromLost()
      {
         if (ScoreManager.Instance == null)
         {
            StartCoroutine(ShowTempMessage("Para yetmiyor"));
            return;
         }

         if (ScoreManager.Instance.TrySpendMoney(continueCost))
         {
            // Spend successful: resume the game without reloading the scene
            _gameEnd = false;
            _timer += continueTimeBonus;
            if (gameLost != null)
               gameLost.SetActive(false);
            Time.timeScale = 1f; // ensure game is unpaused
            Debug.Log("Continued game by spending money (no restart).");
         }
         else
         {
            // Not enough money: show temporary message
            StartCoroutine(ShowTempMessage("Para yetmiyor"));
         }
      }

      // Keep the old name for compatibility; it now forwards to ContinueFromLost
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
