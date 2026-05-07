using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Devam butonu ve mesaj yazısı eksikse bunları oyun başında otomatik oluşturur.</summary>
public class ContinueUIBuilder : MonoBehaviour
{
    private GameObject buttonObject;
    private TextMeshProUGUI messageText;
    private GameManager gm;

    /// <summary>Canvas, devam mesajı ve devam butonunu hazırlar; butonu GameManager'a bağlar.</summary>
    private void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
        if (gm == null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        if (gm.continueMessageText == null)
        {
            GameObject msgGO = new GameObject("ContinueMessage");
            msgGO.transform.SetParent(canvas.transform, false);
            messageText = msgGO.AddComponent<TextMeshProUGUI>();
            messageText.raycastTarget = false;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.fontSize = 36;
            RectTransform r = messageText.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.5f);
            r.anchorMax = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(500, 80);
            r.anchoredPosition = new Vector2(0, -100);
            messageText.text = "";
            msgGO.SetActive(false);
            gm.continueMessageText = messageText;
        }
        else
        {
            messageText = gm.continueMessageText;
        }

        if (GameObject.Find("ContinueButton") == null)
        {
            buttonObject = new GameObject("ContinueButton");
            if (gm.gameLost != null)
                buttonObject.transform.SetParent(gm.gameLost.transform, false);
            else
                buttonObject.transform.SetParent(canvas.transform, false);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            var btn = buttonObject.AddComponent<Button>();
            RectTransform br = buttonObject.GetComponent<RectTransform>();

            if (gm.gameLost != null)
            {
                br.anchorMin = new Vector2(0.5f, 0.5f);
                br.anchorMax = new Vector2(0.5f, 0.5f);
                br.sizeDelta = new Vector2(220, 56);
                br.anchoredPosition = new Vector2(0, -40);
            }
            else
            {
                br.anchorMin = new Vector2(0.5f, 0f);
                br.anchorMax = new Vector2(0.5f, 0f);
                br.sizeDelta = new Vector2(180, 56);
                br.anchoredPosition = new Vector2(0, 80);
            }

            GameObject label = new GameObject("Text");
            label.transform.SetParent(buttonObject.transform, false);
            var labelTMP = label.AddComponent<TextMeshProUGUI>();
            labelTMP.text = $"Devam ({gm.continueCost} altin)";
            labelTMP.alignment = TextAlignmentOptions.Center;
            labelTMP.fontSize = 22;
            RectTransform lr = labelTMP.GetComponent<RectTransform>();
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.sizeDelta = Vector2.zero;
            labelTMP.color = Color.black;

            btn.onClick.AddListener(() => { gm.TryContinue(); });
            if (gm.gameLost == null) buttonObject.SetActive(false);
        }
        else
        {
            buttonObject = GameObject.Find("ContinueButton");
            if (gm.gameLost == null) buttonObject.SetActive(false);
        }
    }

    /// <summary>Devam butonunun görünürlüğünü kaybetme panelinin durumuna göre günceller.</summary>
    private void Update()
    {
        if (gm == null) return;
        if (buttonObject != null && gm.gameLost != null && buttonObject.transform.parent != gm.gameLost.transform)
        {
            buttonObject.SetActive(gm.gameLost.activeSelf);
        }
    }
}
