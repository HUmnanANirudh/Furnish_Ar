using System.Collections.Generic;
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
    private static readonly Color ColorBgPrimary = new Color(0.96f, 0.95f, 0.93f, 1f);
    private static readonly Color ColorBgSurface = new Color(1f, 1f, 1f, 0.96f);
    private static readonly Color ColorTextPrimary = new Color(0.10f, 0.10f, 0.12f, 1f);
    private static readonly Color ColorTextSecondary = new Color(0.42f, 0.42f, 0.45f, 1f);
    private static readonly Color ColorAccentTeal = new Color(0.16f, 0.62f, 0.56f, 1f);
    private static readonly Color ColorAccentActive = new Color(0.88f, 0.96f, 0.95f, 1f);
    private static readonly Color ColorBorderSubtle = new Color(0.86f, 0.85f, 0.82f, 1f);
    private static readonly Color ColorHandle = new Color(0.78f, 0.77f, 0.74f, 1f);
    private static readonly Color ColorScrim = new Color(0.08f, 0.08f, 0.10f, 0.76f);
    private static readonly Color ColorDanger = new Color(0.85f, 0.28f, 0.25f, 1f);

    public struct ItemDef
    {
        public string id;
        public string displayName;
        public string prefabPath;
        public string thumbPath;

        public ItemDef(string id, string displayName, string prefabPath, string thumbPath)
        {
            this.id = id;
            this.displayName = displayName;
            this.prefabPath = prefabPath;
            this.thumbPath = thumbPath;
        }
    }

    public struct CategoryDef
    {
        public string id;
        public string displayName;
        public List<ItemDef> items;

        public CategoryDef(string id, string displayName)
        {
            this.id = id;
            this.displayName = displayName;
            this.items = new List<ItemDef>();
        }
    }

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

        GameObject oldShowcase = GameObject.Find("InitialShowcaseFurniture");
        if (oldShowcase != null) Object.DestroyImmediate(oldShowcase);

        GameObject dirLightObj = GameObject.Find("Directional Light");
        if (dirLightObj == null)
        {
            dirLightObj = new GameObject("Directional Light");
        }
        dirLightObj.transform.position = new Vector3(0f, 3f, 0f);
        dirLightObj.transform.rotation = Quaternion.Euler(45f, 35f, 0f);
        Light dirLight = dirLightObj.GetComponent<Light>();
        if (dirLight == null) dirLight = dirLightObj.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = new Color(1f, 0.98f, 0.94f);
        dirLight.intensity = 1.15f;
        dirLight.shadows = LightShadows.Soft;

        GameObject arCamObj = GameObject.Find("ARCamera");
        if (arCamObj == null)
        {
            arCamObj = new GameObject("ARCamera");
        }
        arCamObj.tag = "MainCamera";
        arCamObj.transform.position = new Vector3(0f, 1.4f, -0.5f);
        arCamObj.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

        Camera cam = arCamObj.GetComponent<Camera>();
        if (cam == null) cam = arCamObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 1f);
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 50f;
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
        pm.currentSelectedPrefab = pm.whiteSofaPrefab;

        GameObject showcaseFurniture = null;
        if (pm.whiteSofaPrefab != null)
        {
            showcaseFurniture = Object.Instantiate(pm.whiteSofaPrefab);
            showcaseFurniture.name = "InitialShowcaseFurniture";
            showcaseFurniture.transform.position = new Vector3(0f, 0f, 2.2f);
            showcaseFurniture.transform.rotation = Quaternion.Euler(0f, 195f, 0f);

            FurnitureLockBadge badge = showcaseFurniture.GetComponent<FurnitureLockBadge>();
            if (badge == null) badge = showcaseFurniture.AddComponent<FurnitureLockBadge>();
            badge.Setup(false);

            pm.activeFurnitureInstance = showcaseFurniture;
        }

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
        scaler.dynamicPixelsPerUnit = 2.5f;

        GraphicRaycaster gr = canvasObj.GetComponent<GraphicRaycaster>();
        if (gr == null) canvasObj.AddComponent<GraphicRaycaster>();

        for (int i = canvasObj.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvasObj.transform.GetChild(i);
            Object.DestroyImmediate(child.gameObject);
        }

        List<CategoryDef> catalogCategories = BuildCatalogDefinitions();

        ARFurnitureAppManager appManager = canvasObj.GetComponent<ARFurnitureAppManager>();
        if (appManager == null) appManager = canvasObj.AddComponent<ARFurnitureAppManager>();
        appManager.placementManager = pm;
        appManager.categories.Clear();

        foreach (CategoryDef cd in catalogCategories)
        {
            FurnitureCategory fc = new FurnitureCategory();
            fc.id = cd.id;
            fc.displayName = cd.displayName;

            foreach (ItemDef item in cd.items)
            {
                FurnitureItem fi = new FurnitureItem();
                fi.id = item.id;
                fi.displayName = item.displayName;
                fi.category = cd.id;
                fi.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(item.prefabPath);
                fi.thumbnail = AssetDatabase.LoadAssetAtPath<Sprite>(item.thumbPath);
                fc.items.Add(fi);
            }

            appManager.categories.Add(fc);
        }

        if (appManager.categories.Count > 0 && appManager.categories[0].items.Count > 0)
        {
            appManager.activeItem = appManager.categories[0].items[0];
            appManager.activeItemId = appManager.activeItem.id;
        }

        GameObject splashPanel = CreateUIObject("SplashScreenPanel", canvasObj.transform);
        SetStretch(splashPanel.GetComponent<RectTransform>());
        Image splashBg = splashPanel.AddComponent<Image>();
        splashBg.color = new Color(0.08f, 0.08f, 0.10f, 0.85f);
        CanvasGroup splashCg = splashPanel.AddComponent<CanvasGroup>();
        appManager.splashPanel = splashPanel;
        appManager.splashCanvasGroup = splashCg;

        GameObject splashCard = CreateUIObject("SplashCard", splashPanel.transform);
        RectTransform scRect = splashCard.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0.5f, 0.5f);
        scRect.anchorMax = new Vector2(0.5f, 0.5f);
        scRect.pivot = new Vector2(0.5f, 0.5f);
        scRect.anchoredPosition = new Vector2(0, 30);
        scRect.sizeDelta = new Vector2(880, 1060);
        Image scBg = splashCard.AddComponent<Image>();
        scBg.color = ColorBgSurface;

        GameObject scBorder = CreateUIObject("Border", splashCard.transform);
        SetStretch(scBorder.GetComponent<RectTransform>());
        Image scbImg = scBorder.AddComponent<Image>();
        scbImg.color = ColorBorderSubtle;
        scbImg.raycastTarget = false;

        GameObject logoEmblem = CreateUIObject("LogoEmblem", splashCard.transform);
        RectTransform leRect = logoEmblem.GetComponent<RectTransform>();
        leRect.anchorMin = new Vector2(0.5f, 0.82f);
        leRect.anchorMax = new Vector2(0.5f, 0.82f);
        leRect.pivot = new Vector2(0.5f, 0.5f);
        leRect.sizeDelta = new Vector2(120, 120);
        Image leImg = logoEmblem.AddComponent<Image>();
        leImg.color = ColorAccentTeal;

        GameObject logoIconText = CreateUIObject("Label", logoEmblem.transform);
        SetStretch(logoIconText.GetComponent<RectTransform>());
        Text lit = logoIconText.AddComponent<Text>();
        lit.font = appFont;
        lit.fontSize = 46;
        lit.fontStyle = FontStyle.Bold;
        lit.color = Color.white;
        lit.alignment = TextAnchor.MiddleCenter;
        lit.text = "AR";

        GameObject titleObj = CreateUIObject("AppTitle", splashCard.transform);
        RectTransform titRect = titleObj.GetComponent<RectTransform>();
        titRect.anchorMin = new Vector2(0.05f, 0.65f);
        titRect.anchorMax = new Vector2(0.95f, 0.75f);
        titRect.offsetMin = Vector2.zero;
        titRect.offsetMax = Vector2.zero;
        Text titText = titleObj.AddComponent<Text>();
        titText.font = appFont;
        titText.fontSize = 44;
        titText.fontStyle = FontStyle.Bold;
        titText.color = ColorTextPrimary;
        titText.alignment = TextAnchor.MiddleCenter;
        titText.text = "Furnish AR";

        GameObject subObj = CreateUIObject("AppSubtitle", splashCard.transform);
        RectTransform subRect = subObj.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.05f, 0.57f);
        subRect.anchorMax = new Vector2(0.95f, 0.64f);
        subRect.offsetMin = Vector2.zero;
        subRect.offsetMax = Vector2.zero;
        Text subText = subObj.AddComponent<Text>();
        subText.font = appFont;
        subText.fontSize = 22;
        subText.color = ColorTextSecondary;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.text = "Augmented Reality Interior Design";

        GameObject featuresObj = CreateUIObject("FeaturesTag", splashCard.transform);
        RectTransform featRect = featuresObj.GetComponent<RectTransform>();
        featRect.anchorMin = new Vector2(0.08f, 0.28f);
        featRect.anchorMax = new Vector2(0.92f, 0.52f);
        featRect.offsetMin = Vector2.zero;
        featRect.offsetMax = Vector2.zero;
        Text featText = featuresObj.AddComponent<Text>();
        featText.font = appFont;
        featText.fontSize = 22;
        featText.fontStyle = FontStyle.Bold;
        featText.lineSpacing = 1.35f;
        featText.color = ColorAccentTeal;
        featText.alignment = TextAnchor.MiddleCenter;
        featText.text = "• TRUE-TO-SCALE 3D PREVIEW\n• INSTANT FLOOR PLANE TRACKING\n• 75 CURATED DESIGNER PIECES";

        GameObject startBtn = CreateUIObject("StartBtn", splashCard.transform);
        RectTransform sbtnRect = startBtn.GetComponent<RectTransform>();
        sbtnRect.anchorMin = new Vector2(0.5f, 0.12f);
        sbtnRect.anchorMax = new Vector2(0.5f, 0.12f);
        sbtnRect.pivot = new Vector2(0.5f, 0.5f);
        sbtnRect.sizeDelta = new Vector2(680, 88);
        Image sbtnBg = startBtn.AddComponent<Image>();
        sbtnBg.color = ColorAccentTeal;
        AddButton(startBtn);

        GameObject sbtnLabel = CreateUIObject("Label", startBtn.transform);
        SetStretch(sbtnLabel.GetComponent<RectTransform>());
        Text sbtnText = sbtnLabel.AddComponent<Text>();
        sbtnText.font = appFont;
        sbtnText.fontSize = 26;
        sbtnText.fontStyle = FontStyle.Bold;
        sbtnText.color = Color.white;
        sbtnText.alignment = TextAnchor.MiddleCenter;
        sbtnText.text = "START AR EXPERIENCE →";

        splashPanel.SetActive(false);

        GameObject overlay = CreateUIObject("AROverlayPanel", canvasObj.transform);
        SetStretch(overlay.GetComponent<RectTransform>());
        appManager.arOverlayPanel = overlay;
        overlay.SetActive(true);

        GameObject topBar = CreateUIObject("TopFloatingHeader", overlay.transform);
        RectTransform tbRect = topBar.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0.5f, 1f);
        tbRect.anchorMax = new Vector2(0.5f, 1f);
        tbRect.pivot = new Vector2(0.5f, 1f);
        tbRect.anchoredPosition = new Vector2(0, -120);
        tbRect.sizeDelta = new Vector2(980, 84);
        Image tbBg = topBar.AddComponent<Image>();
        tbBg.color = ColorBgSurface;

        GameObject tbBorder = CreateUIObject("Border", topBar.transform);
        SetStretch(tbBorder.GetComponent<RectTransform>());
        Image tbbImg = tbBorder.AddComponent<Image>();
        tbbImg.color = ColorBorderSubtle;
        tbbImg.raycastTarget = false;

        GameObject resetBtn = CreateUIObject("ResetBtn", topBar.transform);
        RectTransform rbRect = resetBtn.GetComponent<RectTransform>();
        rbRect.anchorMin = new Vector2(0f, 0.5f);
        rbRect.anchorMax = new Vector2(0f, 0.5f);
        rbRect.pivot = new Vector2(0f, 0.5f);
        rbRect.anchoredPosition = new Vector2(16, 0);
        rbRect.sizeDelta = new Vector2(140, 56);
        Image rbBg = resetBtn.AddComponent<Image>();
        rbBg.color = ColorBgPrimary;
        AddButton(resetBtn);
        AddText(resetBtn, "RESET", 20, FontStyle.Bold, ColorTextSecondary, appFont);

        GameObject statusObj = CreateUIObject("StatusLabel", topBar.transform);
        RectTransform stRect = statusObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 0f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.offsetMin = new Vector2(174, 0);
        stRect.offsetMax = new Vector2(-20, 0);
        Text stText = statusObj.AddComponent<Text>();
        stText.font = appFont;
        stText.fontSize = 24;
        stText.fontStyle = FontStyle.Bold;
        stText.color = ColorTextPrimary;
        stText.alignment = TextAnchor.MiddleLeft;
        stText.horizontalOverflow = HorizontalWrapMode.Overflow;
        stText.verticalOverflow = VerticalWrapMode.Overflow;
        stText.text = "Tap floor to place White Sofa";
        appManager.mainStatusText = stText;

        GameObject bottomPanel = CreateUIObject("UnifiedBottomPanel", overlay.transform);
        RectTransform bpRect = bottomPanel.GetComponent<RectTransform>();
        bpRect.anchorMin = new Vector2(0.5f, 0f);
        bpRect.anchorMax = new Vector2(0.5f, 0f);
        bpRect.pivot = new Vector2(0.5f, 0.5f);
        bpRect.anchoredPosition = new Vector2(0, 190);
        bpRect.sizeDelta = new Vector2(1000, 316);
        Image bpBg = bottomPanel.AddComponent<Image>();
        bpBg.color = ColorBgSurface;

        GameObject bpBorder = CreateUIObject("Border", bottomPanel.transform);
        SetStretch(bpBorder.GetComponent<RectTransform>());
        Image bpbImg = bpBorder.AddComponent<Image>();
        bpbImg.color = ColorBorderSubtle;
        bpbImg.raycastTarget = false;

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
        crRect.anchoredPosition = new Vector2(0, -22);
        crRect.sizeDelta = new Vector2(0, 84);

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
        leL.preferredWidth = 160f;
        leL.preferredHeight = 74f;
        leL.minWidth = 140f;
        leL.minHeight = 65f;
        Image bgL = rotL.AddComponent<Image>();
        bgL.color = ColorBgPrimary;
        AddButton(rotL);
        AddText(rotL, "ROTATE L\n-15°", 18, FontStyle.Bold, ColorTextPrimary, appFont);

        GameObject lockBtn = CreateUIObject("LockBtn", ctrlRow.transform);
        LayoutElement leLock = lockBtn.AddComponent<LayoutElement>();
        leLock.preferredWidth = 510f;
        leLock.preferredHeight = 74f;
        leLock.minWidth = 400f;
        leLock.minHeight = 65f;
        Image bgLock = lockBtn.AddComponent<Image>();
        bgLock.color = ColorAccentTeal;
        AddButton(lockBtn);
        AddText(lockBtn, "LOCK POSITION", 26, FontStyle.Bold, Color.white, appFont);

        GameObject rotR = CreateUIObject("RotRBtn", ctrlRow.transform);
        LayoutElement leR = rotR.AddComponent<LayoutElement>();
        leR.preferredWidth = 160f;
        leR.preferredHeight = 74f;
        leR.minWidth = 140f;
        leR.minHeight = 65f;
        Image bgR = rotR.AddComponent<Image>();
        bgR.color = ColorBgPrimary;
        AddButton(rotR);
        AddText(rotR, "ROTATE R\n+15°", 18, FontStyle.Bold, ColorTextPrimary, appFont);

        GameObject delBtn = CreateUIObject("DelBtn", ctrlRow.transform);
        LayoutElement leDel = delBtn.AddComponent<LayoutElement>();
        leDel.preferredWidth = 100f;
        leDel.preferredHeight = 74f;
        leDel.minWidth = 80f;
        leDel.minHeight = 65f;
        Image bgDel = delBtn.AddComponent<Image>();
        bgDel.color = ColorBgPrimary;
        AddButton(delBtn);
        AddText(delBtn, "DEL", 20, FontStyle.Bold, ColorDanger, appFont);

        GameObject launcherRow = CreateUIObject("CatalogLauncherRow", bottomPanel.transform);
        RectTransform lrRect = launcherRow.GetComponent<RectTransform>();
        lrRect.anchorMin = new Vector2(0f, 0f);
        lrRect.anchorMax = new Vector2(1f, 0f);
        lrRect.pivot = new Vector2(0.5f, 0f);
        lrRect.anchoredPosition = new Vector2(0, 16);
        lrRect.sizeDelta = new Vector2(0, 172);

        GameObject activeCard = CreateUIObject("ActiveItemCard", launcherRow.transform);
        RectTransform acRect = activeCard.GetComponent<RectTransform>();
        acRect.anchorMin = new Vector2(0f, 0.5f);
        acRect.anchorMax = new Vector2(0f, 0.5f);
        acRect.pivot = new Vector2(0f, 0.5f);
        acRect.anchoredPosition = new Vector2(16, 0);
        acRect.sizeDelta = new Vector2(286, 156);
        Image acBg = activeCard.AddComponent<Image>();
        acBg.color = ColorBgPrimary;
        AddButton(activeCard);

        GameObject acBorder = CreateUIObject("Border", activeCard.transform);
        SetStretch(acBorder.GetComponent<RectTransform>());
        Image acbImg = acBorder.AddComponent<Image>();
        acbImg.color = ColorBorderSubtle;
        acbImg.raycastTarget = false;

        GameObject acTag = CreateUIObject("Tag", activeCard.transform);
        RectTransform actRect = acTag.GetComponent<RectTransform>();
        actRect.anchorMin = new Vector2(0.5f, 0.88f);
        actRect.anchorMax = new Vector2(0.5f, 0.88f);
        actRect.pivot = new Vector2(0.5f, 0.5f);
        actRect.sizeDelta = new Vector2(260, 24);
        Text actText = acTag.AddComponent<Text>();
        actText.font = appFont;
        actText.fontSize = 13;
        actText.fontStyle = FontStyle.Bold;
        actText.color = ColorTextSecondary;
        actText.alignment = TextAnchor.MiddleCenter;
        actText.text = "CURRENT PIECE";

        GameObject acThumb = CreateUIObject("Thumbnail", activeCard.transform);
        RectTransform acthRect = acThumb.GetComponent<RectTransform>();
        acthRect.anchorMin = new Vector2(0.5f, 0.52f);
        acthRect.anchorMax = new Vector2(0.5f, 0.52f);
        acthRect.pivot = new Vector2(0.5f, 0.5f);
        acthRect.sizeDelta = new Vector2(88, 88);
        Image acthImg = acThumb.AddComponent<Image>();
        acthImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Textures/Catalog/Sofas/WhiteSofa.png");
        acthImg.preserveAspect = true;
        appManager.activeItemThumbImg = acthImg;

        GameObject acName = CreateUIObject("ItemName", activeCard.transform);
        RectTransform acnRect = acName.GetComponent<RectTransform>();
        acnRect.anchorMin = new Vector2(0.5f, 0.16f);
        acnRect.anchorMax = new Vector2(0.5f, 0.16f);
        acnRect.pivot = new Vector2(0.5f, 0.5f);
        acnRect.sizeDelta = new Vector2(270, 28);
        Text acnText = acName.AddComponent<Text>();
        acnText.font = appFont;
        acnText.fontSize = 20;
        acnText.fontStyle = FontStyle.Bold;
        acnText.color = ColorTextPrimary;
        acnText.alignment = TextAnchor.MiddleCenter;
        acnText.text = "White Sofa";
        appManager.activeItemNameText = acnText;

        GameObject openCatalogBtn = CreateUIObject("OpenCatalogBtn", launcherRow.transform);
        RectTransform ocRect = openCatalogBtn.GetComponent<RectTransform>();
        ocRect.anchorMin = new Vector2(1f, 0.5f);
        ocRect.anchorMax = new Vector2(1f, 0.5f);
        ocRect.pivot = new Vector2(1f, 0.5f);
        ocRect.anchoredPosition = new Vector2(-16, 0);
        ocRect.sizeDelta = new Vector2(666, 156);
        Image ocBg = openCatalogBtn.AddComponent<Image>();
        ocBg.color = ColorAccentTeal;
        AddButton(openCatalogBtn);

        GameObject ocTitle = CreateUIObject("BtnTitle", openCatalogBtn.transform);
        RectTransform octRect = ocTitle.GetComponent<RectTransform>();
        octRect.anchorMin = new Vector2(0.5f, 0.76f);
        octRect.anchorMax = new Vector2(0.5f, 0.76f);
        octRect.pivot = new Vector2(0.5f, 0.5f);
        octRect.sizeDelta = new Vector2(640, 36);
        Text octText = ocTitle.AddComponent<Text>();
        octText.font = appFont;
        octText.fontSize = 30;
        octText.fontStyle = FontStyle.Bold;
        octText.color = Color.white;
        octText.alignment = TextAnchor.MiddleCenter;
        octText.text = "FURNITURE CATALOG";

        GameObject ocSub = CreateUIObject("BtnSubtitle", openCatalogBtn.transform);
        RectTransform ocsRect = ocSub.GetComponent<RectTransform>();
        ocsRect.anchorMin = new Vector2(0.5f, 0.50f);
        ocsRect.anchorMax = new Vector2(0.5f, 0.50f);
        ocsRect.pivot = new Vector2(0.5f, 0.5f);
        ocsRect.sizeDelta = new Vector2(640, 26);
        Text ocsText = ocSub.AddComponent<Text>();
        ocsText.font = appFont;
        ocsText.fontSize = 18;
        ocsText.color = new Color(1f, 1f, 1f, 0.92f);
        ocsText.alignment = TextAnchor.MiddleCenter;
        ocsText.text = "9 Categories · 75 Curated Pieces";

        GameObject ocPill = CreateUIObject("BtnPill", openCatalogBtn.transform);
        RectTransform ocpRect = ocPill.GetComponent<RectTransform>();
        ocpRect.anchorMin = new Vector2(0.5f, 0.20f);
        ocpRect.anchorMax = new Vector2(0.5f, 0.20f);
        ocpRect.pivot = new Vector2(0.5f, 0.5f);
        ocpRect.sizeDelta = new Vector2(520, 44);
        Image ocpBg = ocPill.AddComponent<Image>();
        ocpBg.color = Color.white;
        GameObject ocpLabel = CreateUIObject("Label", ocPill.transform);
        SetStretch(ocpLabel.GetComponent<RectTransform>());
        Text ocpText = ocpLabel.AddComponent<Text>();
        ocpText.font = appFont;
        ocpText.fontSize = 18;
        ocpText.fontStyle = FontStyle.Bold;
        ocpText.color = ColorTextPrimary;
        ocpText.alignment = TextAnchor.MiddleCenter;
        ocpText.text = "EXPLORE ALL FURNITURE →";

        GameObject modalPanel = CreateUIObject("CatalogModalPanel", canvasObj.transform);
        SetStretch(modalPanel.GetComponent<RectTransform>());
        appManager.catalogModalPanel = modalPanel;

        GameObject scrim = CreateUIObject("ModalScrim", modalPanel.transform);
        SetStretch(scrim.GetComponent<RectTransform>());
        Image scrimImg = scrim.AddComponent<Image>();
        scrimImg.color = ColorScrim;
        scrimImg.raycastTarget = true;
        AddButton(scrim);

        GameObject modalWindow = CreateUIObject("ModalWindow", modalPanel.transform);
        RectTransform mwRect = modalWindow.GetComponent<RectTransform>();
        mwRect.anchorMin = new Vector2(0.5f, 0.5f);
        mwRect.anchorMax = new Vector2(0.5f, 0.5f);
        mwRect.pivot = new Vector2(0.5f, 0.5f);
        mwRect.anchoredPosition = new Vector2(0, 10);
        mwRect.sizeDelta = new Vector2(1020, 2050);
        Image mwBg = modalWindow.AddComponent<Image>();
        mwBg.color = ColorBgSurface;

        GameObject mwBorder = CreateUIObject("Border", modalWindow.transform);
        SetStretch(mwBorder.GetComponent<RectTransform>());
        Image mwbImg = mwBorder.AddComponent<Image>();
        mwbImg.color = ColorBorderSubtle;
        mwbImg.raycastTarget = false;

        GameObject modalHeader = CreateUIObject("ModalHeader", modalWindow.transform);
        RectTransform mhRect = modalHeader.GetComponent<RectTransform>();
        mhRect.anchorMin = new Vector2(0f, 1f);
        mhRect.anchorMax = new Vector2(1f, 1f);
        mhRect.pivot = new Vector2(0.5f, 1f);
        mhRect.anchoredPosition = Vector2.zero;
        mhRect.sizeDelta = new Vector2(0, 120);
        Image mhBg = modalHeader.AddComponent<Image>();
        mhBg.color = ColorBgPrimary;

        GameObject mhTitle = CreateUIObject("Title", modalHeader.transform);
        RectTransform mhtRect = mhTitle.GetComponent<RectTransform>();
        mhtRect.anchorMin = new Vector2(0f, 0.48f);
        mhtRect.anchorMax = new Vector2(0.8f, 0.92f);
        mhtRect.offsetMin = new Vector2(24, 0);
        mhtRect.offsetMax = Vector2.zero;
        Text mht = mhTitle.AddComponent<Text>();
        mht.font = appFont;
        mht.fontSize = 32;
        mht.fontStyle = FontStyle.Bold;
        mht.color = ColorTextPrimary;
        mht.alignment = TextAnchor.MiddleLeft;
        mht.text = "FURNITURE CATALOG";

        GameObject mhSub = CreateUIObject("Subtitle", modalHeader.transform);
        RectTransform mhsRect = mhSub.GetComponent<RectTransform>();
        mhsRect.anchorMin = new Vector2(0f, 0.10f);
        mhsRect.anchorMax = new Vector2(0.8f, 0.46f);
        mhsRect.offsetMin = new Vector2(24, 0);
        mhsRect.offsetMax = Vector2.zero;
        Text mhs = mhSub.AddComponent<Text>();
        mhs.font = appFont;
        mhs.fontSize = 18;
        mhs.color = ColorTextSecondary;
        mhs.alignment = TextAnchor.MiddleLeft;
        mhs.text = "Select category on left - Tap any piece to place in your room";

        GameObject closeBtn = CreateUIObject("CloseBtn", modalHeader.transform);
        RectTransform cbRect = closeBtn.GetComponent<RectTransform>();
        cbRect.anchorMin = new Vector2(1f, 0.5f);
        cbRect.anchorMax = new Vector2(1f, 0.5f);
        cbRect.pivot = new Vector2(1f, 0.5f);
        cbRect.anchoredPosition = new Vector2(-20, 0);
        cbRect.sizeDelta = new Vector2(140, 60);
        Image cbBg = closeBtn.AddComponent<Image>();
        cbBg.color = ColorBgSurface;
        AddButton(closeBtn);
        AddText(closeBtn, "X CLOSE", 22, FontStyle.Bold, ColorTextPrimary, appFont);

        GameObject splitBody = CreateUIObject("SplitBody", modalWindow.transform);
        RectTransform sbRect = splitBody.GetComponent<RectTransform>();
        sbRect.anchorMin = Vector2.zero;
        sbRect.anchorMax = Vector2.one;
        sbRect.offsetMin = Vector2.zero;
        sbRect.offsetMax = new Vector2(0, -120);

        GameObject sidebar = CreateUIObject("CategorySidebar", splitBody.transform);
        RectTransform sbSideRect = sidebar.GetComponent<RectTransform>();
        sbSideRect.anchorMin = new Vector2(0f, 0f);
        sbSideRect.anchorMax = new Vector2(0f, 1f);
        sbSideRect.pivot = new Vector2(0f, 0.5f);
        sbSideRect.anchoredPosition = Vector2.zero;
        sbSideRect.sizeDelta = new Vector2(320, 0);
        Image sbSideBg = sidebar.AddComponent<Image>();
        sbSideBg.color = ColorBgPrimary;

        GameObject sideHeader = CreateUIObject("SideHeader", sidebar.transform);
        RectTransform shRect = sideHeader.GetComponent<RectTransform>();
        shRect.anchorMin = new Vector2(0f, 1f);
        shRect.anchorMax = new Vector2(1f, 1f);
        shRect.pivot = new Vector2(0.5f, 1f);
        shRect.anchoredPosition = Vector2.zero;
        shRect.sizeDelta = new Vector2(0, 48);
        Text shText = sideHeader.AddComponent<Text>();
        shText.font = appFont;
        shText.fontSize = 18;
        shText.fontStyle = FontStyle.Bold;
        shText.color = ColorTextSecondary;
        shText.alignment = TextAnchor.MiddleCenter;
        shText.text = "CATEGORIES (9)";

        GameObject sideScroll = CreateUIObject("ScrollRect", sidebar.transform);
        RectTransform ssRect = sideScroll.GetComponent<RectTransform>();
        ssRect.anchorMin = Vector2.zero;
        ssRect.anchorMax = Vector2.one;
        ssRect.offsetMin = Vector2.zero;
        ssRect.offsetMax = new Vector2(0, -48);

        GameObject sideViewport = CreateUIObject("Viewport", sideScroll.transform);
        SetStretch(sideViewport.GetComponent<RectTransform>());
        Image svImg = sideViewport.AddComponent<Image>();
        svImg.color = Color.white;
        sideViewport.AddComponent<RectMask2D>();

        GameObject sideContent = CreateUIObject("Content", sideViewport.transform);
        RectTransform sideContentRect = sideContent.GetComponent<RectTransform>();
        sideContentRect.anchorMin = new Vector2(0f, 1f);
        sideContentRect.anchorMax = new Vector2(1f, 1f);
        sideContentRect.pivot = new Vector2(0.5f, 1f);
        sideContentRect.anchoredPosition = Vector2.zero;
        sideContentRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup svlg = sideContent.AddComponent<VerticalLayoutGroup>();
        svlg.padding = new RectOffset(10, 10, 10, 20);
        svlg.spacing = 10;
        svlg.childControlWidth = true;
        svlg.childControlHeight = false;
        svlg.childForceExpandWidth = true;
        svlg.childForceExpandHeight = false;

        ContentSizeFitter scsf = sideContent.AddComponent<ContentSizeFitter>();
        scsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sideSr = sideScroll.AddComponent<ScrollRect>();
        sideSr.content = sideContentRect;
        sideSr.viewport = sideViewport.GetComponent<RectTransform>();
        sideSr.horizontal = false;
        sideSr.vertical = true;
        sideSr.movementType = ScrollRect.MovementType.Clamped;
        sideSr.scrollSensitivity = 25f;

        appManager.categorySidebarContainer = sideContent.transform;

        for (int i = 0; i < catalogCategories.Count; i++)
        {
            CategoryDef cd = catalogCategories[i];
            bool isFirst = (i == 0);

            GameObject catBtn = CreateUIObject("CatBtn_" + cd.id, sideContent.transform);
            LayoutElement cle = catBtn.AddComponent<LayoutElement>();
            cle.preferredHeight = 92f;
            cle.minHeight = 84f;

            Image cbi = catBtn.AddComponent<Image>();
            cbi.color = isFirst ? ColorAccentTeal : ColorBgSurface;
            AddButton(catBtn);

            GameObject cbBorder = CreateUIObject("Border", catBtn.transform);
            SetStretch(cbBorder.GetComponent<RectTransform>());
            Image cbbImg = cbBorder.AddComponent<Image>();
            cbbImg.color = ColorBorderSubtle;
            cbbImg.raycastTarget = false;

            GameObject cbLabel = CreateUIObject("Label", catBtn.transform);
            SetStretch(cbLabel.GetComponent<RectTransform>());
            Text cbt = cbLabel.AddComponent<Text>();
            cbt.font = appFont;
            cbt.fontSize = 20;
            cbt.fontStyle = isFirst ? FontStyle.Bold : FontStyle.Normal;
            cbt.color = isFirst ? Color.white : ColorTextPrimary;
            cbt.alignment = TextAnchor.MiddleCenter;
            cbt.text = cd.displayName.ToUpper() + "\n(" + cd.items.Count + ")";

            GameObject cbInd = CreateUIObject("ActiveIndicator", catBtn.transform);
            RectTransform cbiRect = cbInd.GetComponent<RectTransform>();
            cbiRect.anchorMin = new Vector2(1f, 0f);
            cbiRect.anchorMax = new Vector2(1f, 1f);
            cbiRect.pivot = new Vector2(1f, 0.5f);
            cbiRect.sizeDelta = new Vector2(6, 0);
            Image cbii = cbInd.AddComponent<Image>();
            cbii.color = Color.white;
            cbInd.SetActive(isFirst);
        }

        GameObject divider = CreateUIObject("DividerLine", splitBody.transform);
        RectTransform divRect = divider.GetComponent<RectTransform>();
        divRect.anchorMin = new Vector2(0f, 0f);
        divRect.anchorMax = new Vector2(0f, 1f);
        divRect.pivot = new Vector2(0.5f, 0.5f);
        divRect.anchoredPosition = new Vector2(320, 0);
        divRect.sizeDelta = new Vector2(2, 0);
        Image divImg = divider.AddComponent<Image>();
        divImg.color = ColorBorderSubtle;

        GameObject gridArea = CreateUIObject("FurnitureGridArea", splitBody.transform);
        RectTransform gaRect = gridArea.GetComponent<RectTransform>();
        gaRect.anchorMin = new Vector2(0f, 0f);
        gaRect.anchorMax = new Vector2(1f, 1f);
        gaRect.offsetMin = new Vector2(322, 0);
        gaRect.offsetMax = Vector2.zero;

        GameObject gridHeader = CreateUIObject("GridHeader", gridArea.transform);
        RectTransform ghRect = gridHeader.GetComponent<RectTransform>();
        ghRect.anchorMin = new Vector2(0f, 1f);
        ghRect.anchorMax = new Vector2(1f, 1f);
        ghRect.pivot = new Vector2(0.5f, 1f);
        ghRect.anchoredPosition = Vector2.zero;
        ghRect.sizeDelta = new Vector2(0, 56);

        GameObject ghTitle = CreateUIObject("CategoryTitle", gridHeader.transform);
        RectTransform ghtRect = ghTitle.GetComponent<RectTransform>();
        ghtRect.anchorMin = new Vector2(0f, 0f);
        ghtRect.anchorMax = new Vector2(0.6f, 1f);
        ghtRect.offsetMin = new Vector2(20, 0);
        ghtRect.offsetMax = Vector2.zero;
        Text ghtText = ghTitle.AddComponent<Text>();
        ghtText.font = appFont;
        ghtText.fontSize = 24;
        ghtText.fontStyle = FontStyle.Bold;
        ghtText.color = ColorTextPrimary;
        ghtText.alignment = TextAnchor.MiddleLeft;
        ghtText.text = "SOFAS (11 ITEMS)";
        appManager.categoryHeaderTitle = ghtText;

        GameObject ghHint = CreateUIObject("CategoryHint", gridHeader.transform);
        RectTransform ghhRect = ghHint.GetComponent<RectTransform>();
        ghhRect.anchorMin = new Vector2(0.6f, 0f);
        ghhRect.anchorMax = new Vector2(1f, 1f);
        ghhRect.offsetMin = Vector2.zero;
        ghhRect.offsetMax = new Vector2(-20, 0);
        Text ghhText = ghHint.AddComponent<Text>();
        ghhText.font = appFont;
        ghhText.fontSize = 18;
        ghhText.color = ColorTextSecondary;
        ghhText.alignment = TextAnchor.MiddleRight;
        ghhText.text = "Tap piece to place";

        GameObject gridScroll = CreateUIObject("ScrollRect", gridArea.transform);
        RectTransform gsRect = gridScroll.GetComponent<RectTransform>();
        gsRect.anchorMin = Vector2.zero;
        gsRect.anchorMax = Vector2.one;
        gsRect.offsetMin = Vector2.zero;
        gsRect.offsetMax = new Vector2(0, -56);

        GameObject gridViewport = CreateUIObject("Viewport", gridScroll.transform);
        SetStretch(gridViewport.GetComponent<RectTransform>());
        Image gvImg = gridViewport.AddComponent<Image>();
        gvImg.color = Color.white;
        gridViewport.AddComponent<RectMask2D>();

        GameObject gridContent = CreateUIObject("Content", gridViewport.transform);
        RectTransform gcRect = gridContent.GetComponent<RectTransform>();
        gcRect.anchorMin = new Vector2(0f, 1f);
        gcRect.anchorMax = new Vector2(1f, 1f);
        gcRect.pivot = new Vector2(0.5f, 1f);
        gcRect.anchoredPosition = Vector2.zero;
        gcRect.sizeDelta = new Vector2(0, 0);

        ScrollRect mainSr = gridScroll.AddComponent<ScrollRect>();
        mainSr.content = gcRect;
        mainSr.viewport = gridViewport.GetComponent<RectTransform>();
        mainSr.horizontal = false;
        mainSr.vertical = true;
        mainSr.movementType = ScrollRect.MovementType.Clamped;
        mainSr.scrollSensitivity = 35f;

        appManager.furnitureGridContainer = gridContent.transform;

        for (int c = 0; c < catalogCategories.Count; c++)
        {
            CategoryDef cd = catalogCategories[c];
            bool isFirstCat = (c == 0);

            GameObject catView = CreateUIObject("CategoryView_" + cd.id, gridContent.transform);
            RectTransform cvRect = catView.GetComponent<RectTransform>();
            cvRect.anchorMin = new Vector2(0f, 1f);
            cvRect.anchorMax = new Vector2(1f, 1f);
            cvRect.pivot = new Vector2(0.5f, 1f);
            cvRect.anchoredPosition = Vector2.zero;
            cvRect.sizeDelta = new Vector2(0, 0);

            GridLayoutGroup glg = catView.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(320, 340);
            glg.spacing = new Vector2(16, 18);
            glg.padding = new RectOffset(18, 18, 16, 30);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;

            ContentSizeFitter cvCsf = catView.AddComponent<ContentSizeFitter>();
            cvCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < cd.items.Count; i++)
            {
                ItemDef item = cd.items[i];
                bool isSelectedItem = (c == 0 && i == 0);

                GameObject card = CreateUIObject("Card_" + item.id, catView.transform);
                Image cardBg = card.AddComponent<Image>();
                cardBg.color = isSelectedItem ? ColorAccentActive : ColorBgSurface;
                AddButton(card);

                GameObject cardBorder = CreateUIObject("Border", card.transform);
                SetStretch(cardBorder.GetComponent<RectTransform>());
                Image cbImg = cardBorder.AddComponent<Image>();
                cbImg.color = isSelectedItem ? ColorAccentTeal : ColorBorderSubtle;
                cbImg.raycastTarget = false;

                GameObject thumbObj = CreateUIObject("Thumbnail", card.transform);
                RectTransform tRect = thumbObj.GetComponent<RectTransform>();
                tRect.anchorMin = new Vector2(0.06f, 0.28f);
                tRect.anchorMax = new Vector2(0.94f, 0.94f);
                tRect.offsetMin = Vector2.zero;
                tRect.offsetMax = Vector2.zero;
                Image thumbImg = thumbObj.AddComponent<Image>();
                thumbImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(item.thumbPath);
                thumbImg.preserveAspect = true;

                GameObject labelObj = CreateUIObject("Label", card.transform);
                RectTransform lRect = labelObj.GetComponent<RectTransform>();
                lRect.anchorMin = new Vector2(0.04f, 0.12f);
                lRect.anchorMax = new Vector2(0.96f, 0.28f);
                lRect.offsetMin = Vector2.zero;
                lRect.offsetMax = Vector2.zero;
                Text labelText = labelObj.AddComponent<Text>();
                labelText.font = appFont;
                labelText.fontSize = 22;
                labelText.fontStyle = isSelectedItem ? FontStyle.Bold : FontStyle.Normal;
                labelText.color = ColorTextPrimary;
                labelText.alignment = TextAnchor.MiddleCenter;
                labelText.text = item.displayName;

                GameObject hintObj = CreateUIObject("Hint", card.transform);
                RectTransform hntRect = hintObj.GetComponent<RectTransform>();
                hntRect.anchorMin = new Vector2(0.04f, 0.02f);
                hntRect.anchorMax = new Vector2(0.96f, 0.12f);
                hntRect.offsetMin = Vector2.zero;
                hntRect.offsetMax = Vector2.zero;
                Text hintText = hintObj.AddComponent<Text>();
                hintText.font = appFont;
                hintText.fontSize = 16;
                hintText.fontStyle = FontStyle.Bold;
                hintText.color = ColorAccentTeal;
                hintText.alignment = TextAnchor.MiddleCenter;
                hintText.text = "SELECT & PLACE";

                GameObject badgeObj = CreateUIObject("ActiveBadge", card.transform);
                RectTransform bRect = badgeObj.GetComponent<RectTransform>();
                bRect.anchorMin = new Vector2(1f, 1f);
                bRect.anchorMax = new Vector2(1f, 1f);
                bRect.pivot = new Vector2(1f, 1f);
                bRect.sizeDelta = new Vector2(86, 30);
                Image bImg = badgeObj.AddComponent<Image>();
                bImg.color = ColorAccentTeal;

                GameObject btObj = CreateUIObject("Text", badgeObj.transform);
                SetStretch(btObj.GetComponent<RectTransform>());
                Text bt = btObj.AddComponent<Text>();
                bt.font = appFont;
                bt.fontSize = 15;
                bt.fontStyle = FontStyle.Bold;
                bt.color = Color.white;
                bt.alignment = TextAnchor.MiddleCenter;
                bt.text = "ACTIVE";

                badgeObj.SetActive(isSelectedItem);
            }

            catView.SetActive(isFirstCat);
        }

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
        tt.text = "";
        toastObj.SetActive(false);
        appManager.toastObj = toastObj;
        appManager.toastText = tt;

        modalPanel.SetActive(false);

        SetLayerRecursively(canvasObj, 5);

        EditorUtility.SetDirty(canvasObj);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Scene UI successfully baked with Splash, AR, and Catalog screens, and initial 3D showcase furniture");
    }

    private static List<CategoryDef> BuildCatalogDefinitions()
    {
        List<CategoryDef> list = new List<CategoryDef>();

        CategoryDef sofas = new CategoryDef("Sofas", "Sofas");
        sofas.items.Add(new ItemDef("WhiteSofa", "White Sofa", "Assets/Assets/Furnitures/White Sofa/White Sofa.prefab", "Assets/Resources/Textures/Catalog/Sofas/WhiteSofa.png"));
        sofas.items.Add(new ItemDef("BrownSofa", "Brown Sofa", "Assets/Assets/Furnitures/Brown Sofa/Brown Sofa.prefab", "Assets/Resources/Textures/Catalog/Sofas/BrownSofa.png"));
        sofas.items.Add(new ItemDef("LightBrownSofa", "Light Brown Sofa", "Assets/Assets/Furnitures/Light Brown Sofa/Light Brown Sofa.prefab", "Assets/Resources/Textures/Catalog/Sofas/LightBrownSofa.png"));
        for (int i = 1; i <= 8; i++)
        {
            string numStr = i < 10 ? "0" + i : i.ToString();
            sofas.items.Add(new ItemDef("Sofa" + numStr, "Modern Sofa " + numStr, "Assets/Furniture Mega Pack/Prefabs/Sofas/Sofa" + numStr + ".prefab", "Assets/Resources/Textures/Catalog/Sofas/Sofa" + numStr + ".png"));
        }
        list.Add(sofas);

        CategoryDef chairs = new CategoryDef("Chairs", "Chairs");
        for (int i = 1; i <= 8; i++)
        {
            string numStr = i < 10 ? "0" + i : i.ToString();
            chairs.items.Add(new ItemDef("Chair" + numStr, "Armchair " + numStr, "Assets/Furniture Mega Pack/Prefabs/Chairs/Chair" + numStr + ".prefab", "Assets/Resources/Textures/Catalog/Chairs/Chair" + numStr + ".png"));
        }
        list.Add(chairs);

        CategoryDef beds = new CategoryDef("Beds", "Beds");
        for (int i = 1; i <= 8; i++)
        {
            string numStr = i < 10 ? "0" + i : i.ToString();
            beds.items.Add(new ItemDef("Bed" + numStr, "Comfort Bed " + numStr, "Assets/Furniture Mega Pack/Prefabs/Beds/Bed" + numStr + ".prefab", "Assets/Resources/Textures/Catalog/Beds/Bed" + numStr + ".png"));
        }
        list.Add(beds);

        CategoryDef tables = new CategoryDef("Tables", "Tables");
        for (int i = 1; i <= 8; i++)
        {
            string numStr = i < 10 ? "0" + i : i.ToString();
            tables.items.Add(new ItemDef("Table" + numStr, "Dining Table " + numStr, "Assets/Furniture Mega Pack/Prefabs/Tables/Table" + numStr + ".prefab", "Assets/Resources/Textures/Catalog/Tables/Table" + numStr + ".png"));
        }
        list.Add(tables);

        CategoryDef closets = new CategoryDef("Closets", "Closets");
        for (int i = 1; i <= 8; i++)
        {
            string numStr = i < 10 ? "0" + i : i.ToString();
            closets.items.Add(new ItemDef("Closet" + numStr, "Wardrobe " + numStr, "Assets/Furniture Mega Pack/Prefabs/Closets/Closet" + numStr + ".prefab", "Assets/Resources/Textures/Catalog/Closets/Closet" + numStr + ".png"));
        }
        list.Add(closets);

        CategoryDef drawers = new CategoryDef("Drawers", "Drawers");
        for (int i = 1; i <= 8; i++)
        {
            string numStr = i < 10 ? "0" + i : i.ToString();
            drawers.items.Add(new ItemDef("Drawer" + numStr, "Storage Drawer " + numStr, "Assets/Furniture Mega Pack/Prefabs/Drawers/Drawer" + numStr + ".prefab", "Assets/Resources/Textures/Catalog/Drawers/Drawer" + numStr + ".png"));
        }
        list.Add(drawers);

        CategoryDef bathroom = new CategoryDef("Bathroom", "Bathroom");
        for (int i = 1; i <= 7; i++)
        {
            string numStr = i < 10 ? "0" + i : i.ToString();
            bathroom.items.Add(new ItemDef("BathroomVanity" + numStr, "Vanity Unit " + numStr, "Assets/Furniture Mega Pack/Prefabs/Bathroom/BathroomVanity" + numStr + ".prefab", "Assets/Resources/Textures/Catalog/Bathroom/BathroomVanity" + numStr + ".png"));
        }
        bathroom.items.Add(new ItemDef("BathTub01", "Soaking Tub 01", "Assets/Furniture Mega Pack/Prefabs/Bathroom/BathTub01.prefab", "Assets/Resources/Textures/Catalog/Bathroom/BathTub01.png"));
        list.Add(bathroom);

        CategoryDef kitchen = new CategoryDef("Kitchen", "Kitchen");
        for (int i = 1; i <= 7; i++)
        {
            string numStr = i < 10 ? "0" + i : i.ToString();
            kitchen.items.Add(new ItemDef("CabinetA" + numStr, "Kitchen Cabinet " + numStr, "Assets/Furniture Mega Pack/Prefabs/Kitchen/CabinetA" + numStr + ".prefab", "Assets/Resources/Textures/Catalog/Kitchen/CabinetA" + numStr + ".png"));
        }
        kitchen.items.Add(new ItemDef("CabinetA_Sink", "Kitchen Sink Unit", "Assets/Furniture Mega Pack/Prefabs/Kitchen/CabinetA_Sink.prefab", "Assets/Resources/Textures/Catalog/Kitchen/CabinetA_Sink.png"));
        list.Add(kitchen);

        CategoryDef cushions = new CategoryDef("Cushions", "Cushions");
        for (int i = 1; i <= 8; i++)
        {
            string numStr = i < 10 ? "0" + i : i.ToString();
            cushions.items.Add(new ItemDef("Cushion" + numStr, "Accent Cushion " + numStr, "Assets/Furniture Mega Pack/Prefabs/Cushioins/Cushion" + numStr + ".prefab", "Assets/Resources/Textures/Catalog/Cushions/Cushion" + numStr + ".png"));
        }
        list.Add(cushions);

        return list;
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
