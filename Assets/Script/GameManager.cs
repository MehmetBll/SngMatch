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
