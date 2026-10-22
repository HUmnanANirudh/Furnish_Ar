using UnityEngine;
using UnityEngine.UI;

public class FurnitureLockBadge : MonoBehaviour
{
    private GameObject canvasObj;
    private Text badgeText;
    private Image badgeBg;
    private bool isLocked = false;
    private Camera targetCamera;
    private Font appFont;

    private void Awake()
    {
        appFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (appFont == null) appFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (appFont == null) appFont = Font.CreateDynamicFontFromOSFont("sans-serif", 14);

        targetCamera = Camera.main;
    }

    public void Setup(bool locked)
    {
        if (canvasObj == null)
        {
            BuildWorldCanvas();
        }
        SetLocked(locked);
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;

        if (badgeText != null)
        {
            badgeText.text = isLocked ? "LOCKED" : "UNLOCKED";
        }

        if (badgeBg != null)
        {
            badgeBg.color = isLocked
                ? new Color(0.06f, 0.68f, 0.44f, 0.94f)
                : new Color(0.92f, 0.55f, 0.12f, 0.94f);
        }
    }

    private void BuildWorldCanvas()
    {
        float highestY = 0.8f;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.enabled && r.bounds.max.y > highestY)
            {
                highestY = r.bounds.max.y;
            }
        }
        float localY = highestY - transform.position.y + 0.25f;

        canvasObj = new GameObject("LockBadgeCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0f, localY, 0f);
        canvasObj.transform.localScale = Vector3.one * 0.0035f;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform cRect = canvasObj.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(300, 80);

        GameObject bgObj = new GameObject("BadgeBg", typeof(RectTransform));
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        badgeBg = bgObj.AddComponent<Image>();
        badgeBg.color = new Color(0.92f, 0.55f, 0.12f, 0.94f);

        GameObject textObj = new GameObject("BadgeText", typeof(RectTransform));
        textObj.transform.SetParent(bgObj.transform, false);
        RectTransform tRect = textObj.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;

        badgeText = textObj.AddComponent<Text>();
        badgeText.font = appFont;
        badgeText.fontSize = 36;
        badgeText.fontStyle = FontStyle.Bold;
        badgeText.color = Color.white;
        badgeText.alignment = TextAnchor.MiddleCenter;
        badgeText.text = "UNLOCKED";
    }

    private void LateUpdate()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        if (targetCamera != null && canvasObj != null)
        {
            canvasObj.transform.rotation = targetCamera.transform.rotation;
        }
    }
}
