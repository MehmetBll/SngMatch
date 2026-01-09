using System.Threading;
using JetBrains.Annotations;
using TMPro;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float gameTime = 30f;
    private float _timer;
    public int totalObjects;
    public int caughtObjects;
    public TextMeshProUGUI timerText;
    private bool _gameEnd = false;
    private int destroyOb = 0;
         void Start()
         {
            _timer = gameTime;
            UpdateTimerUI();
         }
         void Update()
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
         void UpdateTimerUI()
         {
            timerText.text = Mathf.Ceil(_timer).ToString();
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
            _gameEnd = true;
            Debug.Log("Game Won!");
         }
         void GameLost()
         {
            if(_gameEnd) 
               return;
            _gameEnd = true;
            Debug.Log("Game Lost!");
         }
      public void CaughtDestroy()
   {
      destroyOb +=2;
      if (destroyOb >= totalObjects && !_gameEnd)
      {
          GameWon();
          //Time.timeScale = 0f;
      }
   }
}
