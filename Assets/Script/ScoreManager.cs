using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText; // Combo UI için
    public TextMeshProUGUI comboTimerText; // Combo süresi göstergesi
    public int score;

    // Para sistemi
    public int money = 0;
    public TextMeshProUGUI moneyText;
    [Tooltip("Kaç para verileceğini belirlemek için her bir score puanı başına çarpan (ör: 0.1 = her 10 skor için 1 para)")]
    public float moneyPerScore = 0.1f;

    [Header("Combo Settings")]
    public float comboTimeout = 2.0f; // Combo sıfırlanma süresi (saniye)
    private int comboCount = 0;
    private float comboTimer = 0f;
    private bool comboActive = false;
    private bool comboPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);
        UpdateMoneyText();
    }

    private void Update()
    {
        if (comboActive)
        {
            //combo aktifse zamanlayıcı artar ve belli sürede combo sıfrlanır
            if (!comboPaused)
            {
                comboTimer += Time.deltaTime;
                if (comboTimer > comboTimeout)
                {
                    ResetCombo();
                }
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
        // Para kazancı hesapla: skor * combonun çarpanı * para başına skor çarpanı
        int moneyGain = Mathf.CeilToInt(value * multiplier * moneyPerScore);
        if (moneyGain > 0)
        {
            AddMoney(moneyGain);
        }
        UpdateComboText();
    }

    public void ResetCombo()
    {
        comboCount = 0;
        comboActive = false;
        comboTimer = 0f;
        UpdateComboText();
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        money += amount;
        UpdateMoneyText();
    }

    // para varsa harcar ve olduğu yerden devam eder yoksa harcamaz ve false döner
    public bool TrySpendMoney(int amount)
    {
        if (amount <= 0) return true;
        if (money >= amount)
        {
            money -= amount;
            UpdateMoneyText();
            return true;
        }
        return false;
    }

    private void UpdateMoneyText()
    {
        if (moneyText == null) return;
        moneyText.text = money.ToString();
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

    // Belirtilen süre boyunca combo zamanlayıcısını dondurur (ör: güç-up)
    public void PauseCombo(float duration)
    {
        if (!comboActive || comboPaused || duration <= 0f) return;
        StartCoroutine(PauseComboCoroutine(duration));
    }

    private IEnumerator PauseComboCoroutine(float duration)
    {
        comboPaused = true;
        UpdateComboTimerText();
        yield return new WaitForSeconds(duration);
        comboPaused = false;
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
            if (comboPaused)
                comboTimerText.text = $"{remaining:0.00}s (Donduruldu)";
            else
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