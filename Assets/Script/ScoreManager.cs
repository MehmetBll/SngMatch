using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText; // Combo UI için
    public TextMeshProUGUI comboTimerText; // Combo süresi göstergesi
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
            UpdateComboTimerText();
        }
        else
        {
            // combo kapalıysa zaman göstergesini temizle
            if (comboTimerText != null)
                comboTimerText.text = "";
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
        //her comboda çarpan ekler
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
            //combo olursa combu ui ını gösterir ve yazar
                comboText.text = $"Combo: {comboCount}x";
            else
            //combo yoksa ui ı gizler v sıfırlar
                comboText.text = "";
        }
        UpdateComboTimerText();
    }


    private void UpdateComboTimerText()
    {
        if (comboTimerText == null)
            return;

        if (comboActive)
        {
            //combo olursa combo süresini aktif eder 
            float remaining = Mathf.Max(0f, comboTimeout - comboTimer);
            comboTimerText.text = $"{remaining:0.00}s";
        }
        else
        {
            comboTimerText.text = "";
        }
    }

    // Eski AddScore ile uyumluluk için
    public void AddScore(int value)
    {
        AddScore(value, false);
    }
}