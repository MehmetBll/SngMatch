using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText; // Combo UI için
    public int score;

    [Header("Combo Settings")]
    public float comboTimeout = 2.0f; // Combo sıfırlanma süresi (saniye)
    private int comboCount = 0;
    private float comboTimer = 0f;
    private bool comboActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (comboActive)
        {
          //combo aktifse zamanlayıcı artar ve belli sürede combo sıfrlanır
            comboTimer += Time.deltaTime;
            if (comboTimer > comboTimeout)
            {
                ResetCombo();
            }
        }
    }

    public void AddScore(int value, bool isCombo = false)
    {
        if (isCombo)
        {
            comboCount++;
            comboActive = true;
            comboTimer = 0f;
        }
        else
        {
            ResetCombo();
        }

        int multiplier = Mathf.Max(1, comboCount);
        score += value * multiplier;
        scoreText.text = score.ToString();
        UpdateComboText();
    }

    public void ResetCombo()
    {
        comboCount = 0;
        comboActive = false;
        comboTimer = 0f;
        UpdateComboText();
    }

    private void UpdateComboText()
    {
        if (comboText != null)
        {
            if (comboCount > 1)
                comboText.text = $"Combo: {comboCount}x";
            else
                comboText.text = "";
        }
    }

    // Eski AddScore ile uyumluluk için
    public void AddScore(int value)
    {
        AddScore(value, false);
    }
}