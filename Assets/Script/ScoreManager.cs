using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>Skor, kalici toplam skor, para ve combo sistemini yonetir.</summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private const string SessionScoreLabel = "El skoru: ";
    private const string TotalScoreLabel = "Toplam skor: ";

    [Header("Skor UI")]
    [Tooltip("Ek session skor Text'i (TMP)")]
    public TextMeshProUGUI MainScoreText;
    [Tooltip("Combo metni (TMP)")]
    public TextMeshProUGUI comboText;
    [Tooltip("Combo suresi gostergesi (TMP)")]
    public TextMeshProUGUI comboTimerText;
    [Tooltip("Kalici toplam skor (runtime PlayerPrefs ile guncellenir)")]
    public int score;

    private int sessionScore = 0;

    [Header("End Game UI")]
    [Tooltip("Win paneli: bu oturumun puani (TMP)")]
    public TextMeshProUGUI winSessionScoreText;
    [Tooltip("Win paneli: toplam kaydedilmis puan (TMP)")]
    public TextMeshProUGUI winTotalScoreText;
    [Tooltip("Lose paneli: bu oturumun puani (TMP)")]
    public TextMeshProUGUI loseSessionScoreText;
    [Tooltip("Lose paneli: toplam kaydedilmis puan (TMP)")]
    public TextMeshProUGUI loseTotalScoreText;

    [Header("Para Sistemi")]
    [Tooltip("Kalici toplam para bakiyesi")]
    public int money = 0;
    [Tooltip("Para Text (TMP)")]
    public TextMeshProUGUI moneyText;
    [Tooltip("Para kazanimi carpan (skor -> para)")]
    public float moneyPerScore = 0.1f;

    [Header("Combo Ayarlari")]
    [Tooltip("Combo sifirlanma suresi (saniye)")]
    public float comboTimeout = 2.0f;

    private int comboCount = 0;
    private float comboTimer = 0f;
    private bool comboActive = false;
    private bool comboPaused = false;

    /// <summary>Tek ScoreManager ornegini kurar, kayitli toplam skoru yukler ve UI'i gunceller.</summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        score = PlayerPrefs.GetInt("TotalScore", 0);
        money = CurrencyWallet.Balance;
        UpdateScoreText();
        UpdateMoneyText();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Combo aktifken combo suresini takip eder; sure biterse combo'yu sifirlar.</summary>
    private void Update()
    {
        if (comboActive)
        {
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
            if (comboTimerText != null)
                comboTimerText.text = "";
        }
    }

    /// <summary>Skor ekler, combo carpanini uygular, toplam skoru kaydeder ve para kazandirir.</summary>
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
        int gained = value * multiplier;
        sessionScore += gained;
        score += gained;
        PlayerPrefs.SetInt("TotalScore", score);
        PlayerPrefs.Save();
        UpdateScoreText();

        int moneyGain = Mathf.CeilToInt(value * multiplier * moneyPerScore);
        if (moneyGain > 0)
        {
            AddMoney(moneyGain);
        }
        UpdateComboText();
    }

    /// <summary>Bu elde kazanilan skoru ana skor yazisina basar.</summary>
    private void UpdateScoreText()
    {
        if (MainScoreText != null)
            MainScoreText.text = SessionScoreLabel + sessionScore;
    }

    /// <summary>Kazanma ve kaybetme panellerindeki skor yazilarini gunceller.</summary>
    public void UpdateEndGameTexts()
    {
        if (winSessionScoreText != null)
            winSessionScoreText.text = SessionScoreLabel + sessionScore;
        if (winTotalScoreText != null)
            winTotalScoreText.text = TotalScoreLabel + score;
        if (loseSessionScoreText != null)
            loseSessionScoreText.text = SessionScoreLabel + sessionScore;
        if (loseTotalScoreText != null)
            loseTotalScoreText.text = TotalScoreLabel + score;
    }

    /// <summary>Kayitli toplam skoru sifirlar ve UI'i gunceller.</summary>
    public void ResetPersistentScore()
    {
        PlayerPrefs.DeleteKey("TotalScore");
        PlayerPrefs.Save();
        score = 0;
        UpdateScoreText();
        UpdateEndGameTexts();
    }

    /// <summary>Combo sayacini, combo durumunu ve combo suresini sifirlar.</summary>
    public void ResetCombo()
    {
        comboCount = 0;
        comboActive = false;
        comboTimer = 0f;
        UpdateComboText();
    }

    /// <summary>Para ekler ve para yazisini gunceller.</summary>
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        money = CurrencyWallet.Add(amount);
        UpdateMoneyText();
    }

    /// <summary>Yeterli para varsa harcar ve true doner; yetmezse false doner.</summary>
    public bool TrySpendMoney(int amount)
    {
        if (amount <= 0) return true;
        if (CurrencyWallet.TrySpend(amount))
        {
            money = CurrencyWallet.Balance;
            UpdateMoneyText();
            return true;
        }

        UpdateMoneyText();
        return false;
    }

    /// <summary>Mevcut para miktarini UI yazisina basar.</summary>
    private void UpdateMoneyText()
    {
        money = CurrencyWallet.Balance;
        if (moneyText == null) return;
        moneyText.text = money.ToString();
    }

    /// <summary>Combo sayisini ve combo suresi yazisini gunceller.</summary>
    private void UpdateComboText()
    {
        if (comboText != null)
        {
            if (comboCount > 1)
                comboText.text = $"Combo: {comboCount}x";
            else
                comboText.text = "";
        }
        UpdateComboTimerText();
    }

    /// <summary>Aktif combo zamanlayicisini verilen sure kadar dondurur.</summary>
    public bool TryPauseCombo(float duration)
    {
        if (!comboActive || comboPaused || duration <= 0f) return false;
        StartCoroutine(PauseComboCoroutine(duration));
        return true;
    }

    /// <summary>Eski buton baglantilari icin uyumluluk metodudur.</summary>
    public void PauseCombo(float duration)
    {
        TryPauseCombo(duration);
    }

    /// <summary>Combo dondurma durumunu baslatir, bekler ve tekrar acar.</summary>
    private IEnumerator PauseComboCoroutine(float duration)
    {
        comboPaused = true;
        UpdateComboTimerText();
        yield return new WaitForSeconds(duration);
        comboPaused = false;
        UpdateComboTimerText();
    }

    /// <summary>Combo suresinin kalan zamanini veya donduruldu bilgisini UI'a yazar.</summary>
    private void UpdateComboTimerText()
    {
        if (comboTimerText == null)
            return;

        if (comboActive)
        {
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

    /// <summary>Combo kullanmadan skor ekleyen uyumluluk metodudur.</summary>
    public void AddScore(int value)
    {
        AddScore(value, false);
    }
}
