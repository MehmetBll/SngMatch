using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <remarks>Skor, combo ve para yonetimi.</remarks>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Skor UI")]
    [Tooltip("Ana skor Text (TMP)")]
    public TextMeshProUGUI scoreText;
    [Tooltip("Combo metni (TMP)")]
    public TextMeshProUGUI comboText; // Combo UI icin
    [Tooltip("Combo suresi gostergesi (TMP)")]
    public TextMeshProUGUI comboTimerText; // Combo suresi gostergesi
    public int score;

    [Header("Para Sistemi")]
    [Tooltip("Mevcut para miktari (runtime)")]
    public int money = 0;
    [Tooltip("Para Text (TMP)")]
    public TextMeshProUGUI moneyText;
    [Tooltip("Para kazanimi carpan (skor -> para)")]
    public float moneyPerScore = 0.1f;

    [Header("Combo Ayarlari")]
    [Tooltip("Combo sifirlanma suresi (saniye)")]
    public float comboTimeout = 2.0f; // Combo sifirlanma suresi (saniye)
    private int comboCount = 0;
    private float comboTimer = 0f;
    private bool comboActive = false;
    private bool comboPaused = false;

    /// <remarks>Singleton ve baslangic para metnini ayarlar.</remarks>
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

    /// <remarks>Combo zamanlayicisini her frame gunceller.</remarks>
    private void Update()
    {
        if (comboActive)
        {
            // combo aktifse zamanlayici artar ve belli surede combo sifirlanir
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
            // combo kapaliyssa zaman gostergesini temizle
            if (comboTimerText != null)
                comboTimerText.text = "";
        }
    }

    /// <remarks>Skoru ekler; isCombo true ise combo sayisini arttirir.</remarks>
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
        // her comboda carpan ekler
        int multiplier = Mathf.Max(1, comboCount);
        score += value * multiplier;
        scoreText.text = score.ToString();
        // Para kazanci hesapla: skor * combonun carpani * para basina skor carpani
        int moneyGain = Mathf.CeilToInt(value * multiplier * moneyPerScore);
        if (moneyGain > 0)
        {
            AddMoney(moneyGain);
        }
        UpdateComboText();
    }

    /// <remarks>Combo bilgisini sifirlar.</remarks>
    public void ResetCombo()
    {
        comboCount = 0;
        comboActive = false;
        comboTimer = 0f;
        UpdateComboText();
    }

    /// <remarks>Para miktarini arttirir ve UI gunceller.</remarks>
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        money += amount;
        UpdateMoneyText();
    }

    // para varsa harcar ve oldugu yerden devam eder yoksa harcamaz ve false d oner
    /// <remarks>Para harcar; yetersizse false doner.</remarks>
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

    /// <remarks>Para metnini UI'da gunceller.</remarks>
    private void UpdateMoneyText()
    {
        if (moneyText == null) return;
        moneyText.text = money.ToString();
    }

    /// <remarks>Combo metnini ve timer metnini gunceller.</remarks>
    private void UpdateComboText()
    {
        if (comboText != null)
        {
            if (comboCount > 1)
                // combo olursa combo ui sini gosterir ve yazar
                comboText.text = $"Combo: {comboCount}x";
            else
                // combo yoksa ui yi gizler ve sifirlar
                comboText.text = "";
        }
        UpdateComboTimerText();
    }

    // Belirtilen sure boyunca combo zamanlayicisini dondurur (orn: guc-up)
    /// <remarks>Combo zamanlayicisini verilen sure kadar durdurur.</remarks>
    public void PauseCombo(float duration)
    {
        if (!comboActive || comboPaused || duration <= 0f) return;
        StartCoroutine(PauseComboCoroutine(duration));
    }

    /// <remarks>Combo durdurma coroutine'i.</remarks>
    private IEnumerator PauseComboCoroutine(float duration)
    {
        comboPaused = true;
        UpdateComboTimerText();
        yield return new WaitForSeconds(duration);
        comboPaused = false;
        UpdateComboTimerText();
    }

    /// <remarks>Combo suresi UI metnini gunceller.</remarks>
    private void UpdateComboTimerText()
    {
        if (comboTimerText == null)
            return;

        if (comboActive)
        {
            // combo olursa combo suresini aktif eder
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

    // Eski AddScore ile uyumluluk icin
    /// <remarks>Eski sinifa uyumlu AddScore overload.</remarks>
    public void AddScore(int value)
    {
        AddScore(value, false);
    }
}