using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public TextMeshProUGUI scoreText;
    public int score;

         private void Awake()
         {
                  if (Instance == null)
                  {
                        Instance = this;
                  }
                  else
                        Destroy(gameObject);
         }

    public void AddScore(int value)
    {
         score += value;
         scoreText.text = score.ToString();
    }
}