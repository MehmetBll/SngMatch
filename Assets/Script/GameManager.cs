using System.Threading;
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

         void Start()
         {
            _timer = gameTime;
            UpdateTimerUI();
         }
         void Update()
         {
            if (_gameEnd) return;
            _timer -= Time.deltaTime;
            UpdateTimerUI();

            if (_timer <= 0f)
        {
            GameLost();
        }
         }
         void UpdateTimerUI()
         {
            timerText.text = Mathf.Ceil(_timer).ToString();
            _timer = Mathf.Max(_timer, 0f);
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
            _gameEnd = true;
            Debug.Log("Game Lost!");
         }
}
