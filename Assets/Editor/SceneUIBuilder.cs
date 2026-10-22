using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;
using Image = UnityEngine.UI.Image;

public class SceneUIBuilder
{
    private static readonly Color ColorBgPrimary = new Color(0.973f, 0.965f, 0.945f, 1f);
    private static readonly Color ColorBgSurface = new Color(1f, 1f, 1f, 0.98f);
    private static readonly Color ColorTextPrimary = new Color(0.141f, 0.141f, 0.141f, 1f);
    private static readonly Color ColorTextSecondary = new Color(0.522f, 0.522f, 0.522f, 1f);
    private static readonly Color ColorAccentTeal = new Color(0.224f, 0.714f, 0.714f, 1f);
    private static readonly Color ColorAccentActive = new Color(0.92f, 0.98f, 0.98f, 1f);
    private static readonly Color ColorBorderSubtle = new Color(0.88f, 0.87f, 0.84f, 1f);
    private static readonly Color ColorHandle = new Color(0.80f, 0.79f, 0.76f, 1f);

    [MenuItem("Furnish AR/Bake UI To Scene")]
    public static void BakeSceneUI()
    {
        EditorPrefs.SetBool("SelectableEditor.ShowNavigation", false);

        string scenePath = "Assets/Scenes/SampleScene.unity";
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != scenePath)
        {
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        GameObject xrOrigin = GameObject.Find("XR Origin");
        if (xrOrigin != null) Object.DestroyImmediate(xrOrigin);

        GameObject arSession = GameObject.Find("AR Session");
        if (arSession != null) Object.DestroyImmediate(arSession);

        GameObject oldMainCam = GameObject.Find("Main Camera");
        if (oldMainCam != null) Object.DestroyImmediate(oldMainCam);

        GameObject oldPlane = GameObject.Find("AR Default Plane");
        if (oldPlane != null) Object.DestroyImmediate(oldPlane);

        GameObject arCamObj = GameObject.Find("ARCamera");
        if (arCamObj == null)
        {
            arCamObj = new GameObject("ARCamera");
        }
        arCamObj.tag = "MainCamera";
        arCamObj.transform.position = Vector3.zero;
        arCamObj.transform.rotation = Quaternion.identity;

        Camera cam = arCamObj.GetComponent<Camera>();
        if (cam == null) cam = arCamObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 20f;
        cam.allowMSAA = false;
        cam.allowHDR = false;

        AudioListener listener = arCamObj.GetComponent<AudioListener>();
        if (listener == null) arCamObj.AddComponent<AudioListener>();

        VuforiaBehaviour vb = arCamObj.GetComponent<VuforiaBehaviour>();
        if (vb == null) vb = arCamObj.AddComponent<VuforiaBehaviour>();

        GameObject planeFinderObj = GameObject.Find("Plane Finder");
        if (planeFinderObj == null)
        {
            planeFinderObj = new GameObject("Plane Finder");
        }
        planeFinderObj.transform.position = Vector3.zero;
        planeFinderObj.transform.rotation = Quaternion.identity;

        PlaneFinderBehaviour pf = planeFinderObj.GetComponent<PlaneFinderBehaviour>();
        if (pf == null) pf = planeFinderObj.AddComponent<PlaneFinderBehaviour>();
        pf.Height = 1.4f;
        pf.HitTestMode = HitTestMode.AUTOMATIC;

        GameObject indicatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.ptc.vuforia.engine/Vuforia/Prefabs/DefaultPlaneIndicator.prefab");
        if (indicatorPrefab != null)
        {
            pf.PlaneIndicator = indicatorPrefab;
        }

        GameObject pmObj = GameObject.Find("PlacementManager");
        if (pmObj == null)
        {
            pmObj = new GameObject("PlacementManager");
        }
        pmObj.transform.position = Vector3.zero;
        pmObj.transform.rotation = Quaternion.identity;

        furniturePlacementManager pm = pmObj.GetComponent<furniturePlacementManager>();
        if (pm == null) pm = pmObj.AddComponent<furniturePlacementManager>();
        pm.planeFinder = pf;
        pm.whiteSofaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Assets/Furnitures/White Sofa/White Sofa.prefab");
        pm.brownSofaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Assets/Furnitures/Brown Sofa/Brown Sofa.prefab");
        pm.lightBrownSofaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Assets/Furnitures/Light Brown Sofa/Light Brown Sofa.prefab");

        Font appFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (appFont == null) appFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        GameObject canvasObj;
        if (canvas == null)
        {
            canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        canvasObj.layer = 5;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2400);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GraphicRaycaster gr = canvasObj.GetComponent<GraphicRaycaster>();
        if (gr == null) canvasObj.AddComponent<GraphicRaycaster>();

        for (int i = canvasObj.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvasObj.transform.GetChild(i);
            Object.DestroyImmediate(child.gameObject);
        }

        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Textures/Backgrounds/living_room_bg.png");
        Sprite whiteSofaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Textures/Furniture/sofa_white_card.png");
        Sprite brownSofaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Textures/Furniture/sofa_brown_card.png");
        Sprite lightBrownSofaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Textures/Furniture/sofa_light_brown_card.png");

        GameObject overlay = CreateUIObject("AROverlayPanel", canvasObj.transform);
        SetStretch(overlay.GetComponent<RectTransform>());

        GameObject bgObj = CreateUIObject("FallbackRoomBackground", overlay.transform);
        SetStretch(bgObj.GetComponent<RectTransform>());
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = bgSprite;
        bgImg.preserveAspect = false;
        bgImg.raycastTarget = false;

        GameObject topBar = CreateUIObject("TopFloatingHeader", overlay.transform);
        RectTransform tbRect = topBar.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0.5f, 1f);
        tbRect.anchorMax = new Vector2(0.5f, 1f);
        tbRect.pivot = new Vector2(0.5f, 1f);
        tbRect.anchoredPosition = new Vector2(0, -130);
        tbRect.sizeDelta = new Vector2(980, 76);
        Image tbBg = topBar.AddComponent<Image>();
        tbBg.color = ColorBgSurface;

        GameObject resetBtn = CreateUIObject("ResetBtn", topBar.transform);
        RectTransform rbRect = resetBtn.GetComponent<RectTransform>();
        rbRect.anchorMin = new Vector2(0f, 0.5f);
        rbRect.anchorMax = new Vector2(0f, 0.5f);
        rbRect.pivot = new Vector2(0f, 0.5f);
        rbRect.anchoredPosition = new Vector2(16, 0);
        rbRect.sizeDelta = new Vector2(130, 52);
        Image rbBg = resetBtn.AddComponent<Image>();
        rbBg.color = ColorBgPrimary;
        AddButton(resetBtn);
        AddText(resetBtn, "RESET", 17, FontStyle.Bold, ColorTextSecondary, appFont);

        GameObject statusObj = CreateUIObject("StatusLabel", topBar.transform);
        RectTransform stRect = statusObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 0f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.offsetMin = new Vector2(160, 0);
        stRect.offsetMax = new Vector2(-160, 0);
        Text stText = statusObj.AddComponent<Text>();
        stText.font = appFont;
        stText.fontSize = 20;
        stText.fontStyle = FontStyle.Bold;
        stText.color = ColorTextPrimary;
        stText.alignment = TextAnchor.MiddleCenter;
        stText.horizontalOverflow = HorizontalWrapMode.Overflow;
        stText.verticalOverflow = VerticalWrapMode.Overflow;
        stText.text = "Tap floor to place White Sofa";

        GameObject photoBtn = CreateUIObject("PhotoBtn", topBar.transform);
        RectTransform pbRect = photoBtn.GetComponent<RectTransform>();
        pbRect.anchorMin = new Vector2(1f, 0.5f);
        pbRect.anchorMax = new Vector2(1f, 0.5f);
        pbRect.pivot = new Vector2(1f, 0.5f);
        pbRect.anchoredPosition = new Vector2(-16, 0);
        pbRect.sizeDelta = new Vector2(130, 52);
        Image pbBg = photoBtn.AddComponent<Image>();
        pbBg.color = ColorBgPrimary;
        AddButton(photoBtn);
        AddText(photoBtn, "PHOTO", 17, FontStyle.Bold, ColorTextPrimary, appFont);

        GameObject bottomPanel = CreateUIObject("UnifiedBottomPanel", overlay.transform);
        RectTransform bpRect = bottomPanel.GetComponent<RectTransform>();
        bpRect.anchorMin = new Vector2(0.5f, 0f);
        bpRect.anchorMax = new Vector2(0.5f, 0f);
        bpRect.pivot = new Vector2(0.5f, 0f);
        bpRect.anchoredPosition = new Vector2(0, 40);
        bpRect.sizeDelta = new Vector2(1020, 360);
        Image bpBg = bottomPanel.AddComponent<Image>();
        bpBg.color = ColorBgSurface;

        GameObject handle = CreateUIObject("HandlePill", bottomPanel.transform);
        RectTransform hRect = handle.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0.5f, 1f);
        hRect.anchorMax = new Vector2(0.5f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.anchoredPosition = new Vector2(0, -10);
        hRect.sizeDelta = new Vector2(64, 6);
        Image hImg = handle.AddComponent<Image>();
        hImg.color = ColorHandle;

        GameObject ctrlRow = CreateUIObject("ControlRow", bottomPanel.transform);
        RectTransform crRect = ctrlRow.GetComponent<RectTransform>();
        crRect.anchorMin = new Vector2(0f, 1f);
        crRect.anchorMax = new Vector2(1f, 1f);
        crRect.pivot = new Vector2(0.5f, 1f);
        crRect.anchoredPosition = new Vector2(0, -28);
        crRect.sizeDelta = new Vector2(0, 78);

        HorizontalLayoutGroup crLayout = ctrlRow.AddComponent<HorizontalLayoutGroup>();
        crLayout.padding = new RectOffset(16, 16, 0, 0);
        crLayout.spacing = 14;
        crLayout.childAlignment = TextAnchor.MiddleCenter;
        crLayout.childControlWidth = true;
        crLayout.childControlHeight = true;
        crLayout.childForceExpandWidth = false;
        crLayout.childForceExpandHeight = false;

        GameObject rotL = CreateUIObject("RotLBtn", ctrlRow.transform);
        LayoutElement leL = rotL.AddComponent<LayoutElement>();
        leL.preferredWidth = 170f;
        leL.preferredHeight = 72f;
        leL.minWidth = 150f;
        leL.minHeight = 65f;
        Image bgL = rotL.AddComponent<Image>();
        bgL.color = ColorBgPrimary;
        AddButton(rotL);
        AddText(rotL, "ROTATE L\n(-15°)", 16, FontStyle.Bold, ColorTextPrimary, appFont);

        GameObject lockBtn = CreateUIObject("LockBtn", ctrlRow.transform);
        LayoutElement leLock = lockBtn.AddComponent<LayoutElement>();
        leLock.preferredWidth = 470f;
        leLock.preferredHeight = 72f;
        leLock.minWidth = 380f;
        leLock.minHeight = 65f;
        Image bgLock = lockBtn.AddComponent<Image>();
        bgLock.color = ColorAccentTeal;
        AddButton(lockBtn);
        AddText(lockBtn, "LOCK POSITION", 24, FontStyle.Bold, Color.white, appFont);

        GameObject rotR = CreateUIObject("RotRBtn", ctrlRow.transform);
        LayoutElement leR = rotR.AddComponent<LayoutElement>();
        leR.preferredWidth = 170f;
        leR.preferredHeight = 72f;
        leR.minWidth = 150f;
        leR.minHeight = 65f;
        Image bgR = rotR.AddComponent<Image>();
        bgR.color = ColorBgPrimary;
        AddButton(rotR);
        AddText(rotR, "ROTATE R\n(+15°)", 16, FontStyle.Bold, ColorTextPrimary, appFont);

        GameObject delBtn = CreateUIObject("DelBtn", ctrlRow.transform);
        LayoutElement leDel = delBtn.AddComponent<LayoutElement>();
        leDel.preferredWidth = 84f;
        leDel.preferredHeight = 72f;
        leDel.minWidth = 70f;
        leDel.minHeight = 65f;
        Image bgDel = delBtn.AddComponent<Image>();
        bgDel.color = ColorBgPrimary;
        AddButton(delBtn);
        AddText(delBtn, "DEL", 18, FontStyle.Bold, ColorTextPrimary, appFont);

        GameObject sofaRow = CreateUIObject("SofaSelectionRow", bottomPanel.transform);
        RectTransform srRect = sofaRow.GetComponent<RectTransform>();
        srRect.anchorMin = new Vector2(0f, 0f);
        srRect.anchorMax = new Vector2(1f, 0f);
        srRect.pivot = new Vector2(0.5f, 0f);
        srRect.anchoredPosition = new Vector2(0, 16);
        srRect.sizeDelta = new Vector2(0, 220);

        HorizontalLayoutGroup srLayout = sofaRow.AddComponent<HorizontalLayoutGroup>();
        srLayout.padding = new RectOffset(16, 16, 0, 0);
        srLayout.spacing = 16;
        srLayout.childAlignment = TextAnchor.MiddleCenter;
        srLayout.childControlWidth = true;
        srLayout.childControlHeight = true;
        srLayout.childForceExpandWidth = true;
        srLayout.childForceExpandHeight = true;

        CreateSofaCard(sofaRow.transform, "Card_white_sofa", "White Sofa", whiteSofaSprite, true, appFont);
        CreateSofaCard(sofaRow.transform, "Card_brown_sofa", "Brown Sofa", brownSofaSprite, false, appFont);
        CreateSofaCard(sofaRow.transform, "Card_light_brown_sofa", "Light Brown Sofa", lightBrownSofaSprite, false, appFont);

        GameObject toastObj = CreateUIObject("ToastNotification", overlay.transform);
        RectTransform toastRect = toastObj.GetComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0.5f, 0.44f);
        toastRect.anchorMax = new Vector2(0.5f, 0.44f);
        toastRect.pivot = new Vector2(0.5f, 0.5f);
        toastRect.sizeDelta = new Vector2(680, 76);
        Image toastBg = toastObj.AddComponent<Image>();
        toastBg.color = new Color(0.14f, 0.14f, 0.14f, 0.94f);
        GameObject toastTextObj = CreateUIObject("Text", toastObj.transform);
        SetStretch(toastTextObj.GetComponent<RectTransform>());
        Text tt = toastTextObj.AddComponent<Text>();
        tt.font = appFont;
        tt.fontSize = 22;
        tt.color = Color.white;
        tt.alignment = TextAnchor.MiddleCenter;
        tt.horizontalOverflow = HorizontalWrapMode.Overflow;
        tt.verticalOverflow = VerticalWrapMode.Overflow;
        tt.text = "";
        toastObj.SetActive(false);

        GameObject flashObj = CreateUIObject("SnapshotFlashOverlay", overlay.transform);
        SetStretch(flashObj.GetComponent<RectTransform>());
        Image flashImg = flashObj.AddComponent<Image>();
        flashImg.color = Color.white;
        CanvasGroup flashCg = flashObj.AddComponent<CanvasGroup>();
        flashCg.alpha = 0f;
        flashObj.SetActive(false);

        SetLayerRecursively(canvasObj, 5);

        ARFurnitureAppManager appManager = canvasObj.GetComponent<ARFurnitureAppManager>();
        if (appManager == null) appManager = canvasObj.AddComponent<ARFurnitureAppManager>();

        appManager.whiteSofaPrefab = pm.whiteSofaPrefab;
        appManager.brownSofaPrefab = pm.brownSofaPrefab;
        appManager.lightBrownSofaPrefab = pm.lightBrownSofaPrefab;
        appManager.placementManager = pm;

        EditorUtility.SetDirty(canvasObj);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Scene UI successfully baked into Assets/Scenes/SampleScene.unity with Vuforia");
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private static Button AddButton(GameObject go)
    {
        Button b = go.AddComponent<Button>();
        Navigation nav = b.navigation;
        nav.mode = Navigation.Mode.None;
        b.navigation = nav;
        return b;
    }

    private static void CreateSofaCard(Transform parent, string cardName, string sofaName, Sprite thumbSprite, bool isActive, Font font)
    {
        GameObject card = CreateUIObject(cardName, parent);
        LayoutElement le = card.AddComponent<LayoutElement>();
        le.preferredWidth = 310f;
        le.preferredHeight = 210f;
        le.minWidth = 260f;
        le.minHeight = 180f;

        Image cBg = card.AddComponent<Image>();
        cBg.color = isActive ? ColorAccentActive : ColorBgPrimary;
        AddButton(card);

        GameObject borderObj = CreateUIObject("Border", card.transform);
        SetStretch(borderObj.GetComponent<RectTransform>());
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = isActive ? ColorAccentTeal : ColorBorderSubtle;
        borderImg.raycastTarget = false;

        GameObject thumbObj = CreateUIObject("Thumbnail", card.transform);
        RectTransform tRect = thumbObj.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.08f, 0.28f);
        tRect.anchorMax = new Vector2(0.92f, 0.94f);
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;
        Image thumbImg = thumbObj.AddComponent<Image>();
        thumbImg.sprite = thumbSprite;
        thumbImg.preserveAspect = true;

        GameObject labelObj = CreateUIObject("Label", card.transform);
        RectTransform lRect = labelObj.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0.04f, 0.04f);
        lRect.anchorMax = new Vector2(0.96f, 0.28f);
        lRect.offsetMin = Vector2.zero;
        lRect.offsetMax = Vector2.zero;
        Text labelText = labelObj.AddComponent<Text>();
        labelText.font = font;
        labelText.fontSize = 20;
        labelText.fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;
        labelText.color = isActive ? ColorAccentTeal : ColorTextPrimary;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
        labelText.verticalOverflow = VerticalWrapMode.Overflow;
        labelText.text = sofaName;

        GameObject badgeObj = CreateUIObject("ActiveBadge", card.transform);
        RectTransform bRect = badgeObj.GetComponent<RectTransform>();
        bRect.anchorMin = new Vector2(1f, 1f);
        bRect.anchorMax = new Vector2(1f, 1f);
        bRect.pivot = new Vector2(1f, 1f);
        bRect.sizeDelta = new Vector2(80, 28);
        Image bImg = badgeObj.AddComponent<Image>();
        bImg.color = ColorAccentTeal;

        GameObject btObj = CreateUIObject("Text", badgeObj.transform);
        SetStretch(btObj.GetComponent<RectTransform>());
        Text bt = btObj.AddComponent<Text>();
        bt.font = font;
        bt.fontSize = 14;
        bt.fontStyle = FontStyle.Bold;
        bt.color = Color.white;
        bt.alignment = TextAnchor.MiddleCenter;
        bt.horizontalOverflow = HorizontalWrapMode.Overflow;
        bt.verticalOverflow = VerticalWrapMode.Overflow;
        bt.text = "ACTIVE";

        badgeObj.SetActive(isActive);
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void SetStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void AddText(GameObject parent, string text, int fontSize, FontStyle style, Color color, Font font)
    {
        GameObject tObj = CreateUIObject("Label", parent.transform);
        SetStretch(tObj.GetComponent<RectTransform>());
        Text t = tObj.AddComponent<Text>();
        t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.text = text;
    }
}
