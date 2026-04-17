using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Creates a Continue button and a TMP message at runtime.
// Button is visible only when GameManager.gameLost is active.
public class ContinueUIBuilder : MonoBehaviour
{
    private GameObject btnGO;
    private TextMeshProUGUI messageText;
    private GameManager gm;

    void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
        if (gm == null) return;

        // Ensure there's a Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Create message TMP if none assigned
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

        // Create a Continue button (prefer parent under gameLost panel if available)
        if (GameObject.Find("ContinueButton") == null)
        {
            btnGO = new GameObject("ContinueButton");
            // Parent under gameLost panel if provided, otherwise canvas
            if (gm.gameLost != null)
                btnGO.transform.SetParent(gm.gameLost.transform, false);
            else
                btnGO.transform.SetParent(canvas.transform, false);
            var image = btnGO.AddComponent<Image>();
            image.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            var btn = btnGO.AddComponent<Button>();
            RectTransform br = btnGO.GetComponent<RectTransform>();
            // If parent is gameLost panel, position near center-bottom; otherwise place above bottom center of screen
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
            label.transform.SetParent(btnGO.transform, false);
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
            // If parented under gameLost, button visibility is controlled by that panel.
            if (gm.gameLost == null) btnGO.SetActive(false);
        }
        else
        {
            btnGO = GameObject.Find("ContinueButton");
            if (gm.gameLost == null) btnGO.SetActive(false);
        }
    }

    void Update()
    {
        if (gm == null) return;
        // If button is not parented under gameLost, toggle it manually based on gameLost state
        if (btnGO != null && gm.gameLost != null && btnGO.transform.parent != gm.gameLost.transform)
        {
            btnGO.SetActive(gm.gameLost.activeSelf);
        }
    }
}
