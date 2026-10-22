using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Image = UnityEngine.UI.Image;

public class ARFurnitureAppManager : MonoBehaviour
{
    public static ARFurnitureAppManager Instance { get; private set; }

    [Header("Dependencies")]
    public furniturePlacementManager placementManager;

    [Header("Real 3 Furniture Prefabs")]
    public GameObject whiteSofaPrefab;
    public GameObject brownSofaPrefab;
    public GameObject lightBrownSofaPrefab;

    public static readonly Color ColorBgPrimary = new Color(0.973f, 0.965f, 0.945f, 1f);
    public static readonly Color ColorBgSurface = new Color(1f, 1f, 1f, 0.98f);
    public static readonly Color ColorTextPrimary = new Color(0.141f, 0.141f, 0.141f, 1f);
    public static readonly Color ColorTextSecondary = new Color(0.522f, 0.522f, 0.522f, 1f);
    public static readonly Color ColorAccentTeal = new Color(0.224f, 0.714f, 0.714f, 1f);
    public static readonly Color ColorAccentActive = new Color(0.92f, 0.98f, 0.98f, 1f);
    public static readonly Color BorderSubtle = new Color(0.88f, 0.87f, 0.84f, 1f);
    public static readonly Color HandleColor = new Color(0.80f, 0.79f, 0.76f, 1f);

    public struct SofaChoice
    {
        public string id;
        public string name;
        public string thumbnailPath;
        public GameObject prefab;
    }

    private List<SofaChoice> sofaChoices = new List<SofaChoice>();
    private int selectedSofaIndex = 0;

    private Canvas mainCanvas;
    private GameObject splashPanel;
    private CanvasGroup splashCanvasGroup;
    private GameObject arOverlayPanel;

    private Text mainStatusText;

    private Image[] sofaCardBorderImgs;
    private Image[] sofaCardBgImgs;
    private Text[] sofaCardTitleTexts;
    private GameObject[] sofaCardActiveBadges;

    private GameObject snapshotFlash;
    private CanvasGroup snapshotCanvasGroup;
    private GameObject toastObj;
    private Text toastText;
    private Coroutine toastCoroutine;

    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private Font appFont;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);

        Camera[] cams = FindObjectsOfType<Camera>();
        foreach (Camera c in cams)
        {
            c.allowMSAA = false;
            c.allowHDR = false;
        }

        InitFont();
        InitSofaChoices();
    }

    private void Start()
    {
        if (placementManager == null)
            placementManager = FindObjectOfType<furniturePlacementManager>();

        if (placementManager != null)
        {
            if (whiteSofaPrefab != null) placementManager.whiteSofaPrefab = whiteSofaPrefab;
            if (brownSofaPrefab != null) placementManager.brownSofaPrefab = brownSofaPrefab;
            if (lightBrownSofaPrefab != null) placementManager.lightBrownSofaPrefab = lightBrownSofaPrefab;

            placementManager.currentSelectedPrefab = sofaChoices[selectedSofaIndex].prefab;

            placementManager.OnActiveFurnitureSpawned += OnActiveFurnitureSpawnedHandler;
            placementManager.OnActiveFurnitureMoved += OnActiveFurnitureMovedHandler;
            placementManager.OnFurnitureLocked += OnFurnitureLockedHandler;
            placementManager.OnActiveFurnitureDeleted += OnActiveFurnitureDeletedHandler;
            placementManager.OnLockedFurnitureRemoved += OnLockedFurnitureRemovedHandler;
            placementManager.OnAllCleared += OnAllClearedHandler;
        }

        if (!BindExistingSceneUI())
        {
            BuildUI();
        }

        if (splashPanel != null && splashPanel.activeInHierarchy)
        {
            StartCoroutine(SplashScreenSequence());
        }
    }

    private void InitFont()
    {
        appFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (appFont == null) appFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (appFont == null) appFont = Font.CreateDynamicFontFromOSFont("sans-serif", 14);
    }

    private void InitSofaChoices()
    {
        sofaChoices.Clear();

        sofaChoices.Add(new SofaChoice
        {
            id = "white_sofa",
            name = "White Sofa",
            thumbnailPath = "Textures/Furniture/sofa_white_card",
            prefab = whiteSofaPrefab
        });

        sofaChoices.Add(new SofaChoice
        {
            id = "brown_sofa",
            name = "Brown Sofa",
            thumbnailPath = "Textures/Furniture/sofa_brown_card",
            prefab = brownSofaPrefab
        });

        sofaChoices.Add(new SofaChoice
        {
            id = "light_brown_sofa",
            name = "Light Brown Sofa",
            thumbnailPath = "Textures/Furniture/sofa_light_brown_card",
            prefab = lightBrownSofaPrefab
        });
    }

    public Sprite GetSprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        if (spriteCache.TryGetValue(path, out Sprite cached) && cached != null)
            return cached;

        Sprite s = Resources.Load<Sprite>(path);
        if (s == null)
        {
            Texture2D tex = Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }

        if (s == null)
        {
            string fullPath = Path.Combine(Application.dataPath, path + ".png");
            if (File.Exists(fullPath))
            {
                byte[] bytes = File.ReadAllBytes(fullPath);
                Texture2D directTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (directTex.LoadImage(bytes))
                {
                    s = Sprite.Create(directTex, new Rect(0, 0, directTex.width, directTex.height), new Vector2(0.5f, 0.5f));
                }
            }
        }

        if (s != null) spriteCache[path] = s;
        return s;
    }

    private bool BindExistingSceneUI()
    {
        Transform canvasTransform = transform;
        if (canvasTransform.GetComponent<Canvas>() == null)
        {
            Canvas c = FindObjectOfType<Canvas>();
            if (c != null) canvasTransform = c.transform;
        }

        Transform overlay = canvasTransform.Find("AROverlayPanel");
        if (overlay == null)
        {
            return false;
        }

        arOverlayPanel = overlay.gameObject;

        Button[] allButtons = canvasTransform.GetComponentsInChildren<Button>(true);
        foreach (Button b in allButtons)
        {
            Navigation nav = b.navigation;
            nav.mode = Navigation.Mode.None;
            b.navigation = nav;
        }

        Transform fallbackBg = overlay.Find("FallbackRoomBackground");
        if (fallbackBg != null)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                fallbackBg.gameObject.SetActive(false);
            }
            else
            {
                fallbackBg.gameObject.SetActive(true);
            }
        }

        Transform topBar = overlay.Find("TopFloatingHeader");
        if (topBar != null)
        {
            Button resetBtn = topBar.Find("ResetBtn")?.GetComponent<Button>();
            if (resetBtn != null)
            {
                resetBtn.onClick.RemoveAllListeners();
                resetBtn.onClick.AddListener(() => {
                    if (placementManager != null) placementManager.ClearAllFurniture();
                });
            }

            Button photoBtn = topBar.Find("PhotoBtn")?.GetComponent<Button>();
            if (photoBtn != null)
            {
                photoBtn.onClick.RemoveAllListeners();
                photoBtn.onClick.AddListener(TriggerSnapshot);
            }

            mainStatusText = topBar.Find("StatusLabel")?.GetComponent<Text>();
        }

        Transform bottomPanel = overlay.Find("UnifiedBottomPanel");
        if (bottomPanel != null)
        {
            Transform ctrlRow = bottomPanel.Find("ControlRow");
            if (ctrlRow != null)
            {
                Button rotL = ctrlRow.Find("RotLBtn")?.GetComponent<Button>();
                if (rotL != null)
                {
                    rotL.onClick.RemoveAllListeners();
                    rotL.onClick.AddListener(() => {
                        if (placementManager != null) placementManager.RotateActiveFurniture(-15f);
                    });
                }

                Button lockBtn = ctrlRow.Find("LockBtn")?.GetComponent<Button>();
                if (lockBtn != null)
                {
                    lockBtn.onClick.RemoveAllListeners();
                    lockBtn.onClick.AddListener(OnLockClicked);
                }

                Button rotR = ctrlRow.Find("RotRBtn")?.GetComponent<Button>();
                if (rotR != null)
                {
                    rotR.onClick.RemoveAllListeners();
                    rotR.onClick.AddListener(() => {
                        if (placementManager != null) placementManager.RotateActiveFurniture(15f);
                    });
                }

                Button delBtn = ctrlRow.Find("DelBtn")?.GetComponent<Button>();
                if (delBtn != null)
                {
                    delBtn.onClick.RemoveAllListeners();
                    delBtn.onClick.AddListener(() => {
                        if (placementManager != null) placementManager.DeleteCurrentOrLast();
                    });
                }
            }

            Transform sofaRow = bottomPanel.Find("SofaSelectionRow");
            if (sofaRow != null)
            {
                sofaCardBorderImgs = new Image[sofaChoices.Count];
                sofaCardBgImgs = new Image[sofaChoices.Count];
                sofaCardTitleTexts = new Text[sofaChoices.Count];
                sofaCardActiveBadges = new GameObject[sofaChoices.Count];

                for (int i = 0; i < sofaChoices.Count; i++)
                {
                    int index = i;
                    SofaChoice choice = sofaChoices[i];
                    Transform card = sofaRow.Find("Card_" + choice.id);
                    if (card != null)
                    {
                        Button b = card.GetComponent<Button>();
                        if (b != null)
                        {
                            b.onClick.RemoveAllListeners();
                            b.onClick.AddListener(() => OnSofaSelected(index));
                        }

                        sofaCardBgImgs[i] = card.GetComponent<Image>();
                        sofaCardBorderImgs[i] = card.Find("Border")?.GetComponent<Image>();
                        sofaCardTitleTexts[i] = card.Find("Label")?.GetComponent<Text>();
                        sofaCardActiveBadges[i] = card.Find("ActiveBadge")?.gameObject;
                    }
                }
            }
        }

        Transform toast = overlay.Find("ToastNotification");
        if (toast != null)
        {
            toastObj = toast.gameObject;
            toastText = toast.Find("Text")?.GetComponent<Text>();
            toastObj.SetActive(false);
        }

        Transform flash = overlay.Find("SnapshotFlashOverlay");
        if (flash != null)
        {
            snapshotFlash = flash.gameObject;
            snapshotCanvasGroup = flash.GetComponent<CanvasGroup>();
            snapshotFlash.SetActive(false);
        }

        Transform splash = canvasTransform.Find("SplashScreenPanel");
        if (splash != null)
        {
            splashPanel = splash.gameObject;
            splashCanvasGroup = splash.GetComponent<CanvasGroup>();
        }

        OnSofaSelected(selectedSofaIndex);
        UpdateStatusText();

        return true;
    }

    private void BuildUI()
    {
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            GameObject canvasObj = new GameObject("ARAppCanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2400);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        BuildAROverlay(mainCanvas.transform);
        BuildSnapshotFlash(mainCanvas.transform);
        BuildToast(mainCanvas.transform);
    }

    private void BuildAROverlay(Transform parent)
    {
        arOverlayPanel = CreateUIObject("AROverlayPanel", parent);
        SetStretch(arOverlayPanel.GetComponent<RectTransform>());

        GameObject bgObj = CreateUIObject("FallbackRoomBackground", arOverlayPanel.transform);
        SetStretch(bgObj.GetComponent<RectTransform>());
        Image fallbackRoomBg = bgObj.AddComponent<Image>();
        fallbackRoomBg.sprite = GetSprite("Textures/Backgrounds/living_room_bg");
        fallbackRoomBg.preserveAspect = false;

        if (Application.platform == RuntimePlatform.Android)
        {
            fallbackRoomBg.gameObject.SetActive(false);
        }

        BuildTopFloatingHeader(arOverlayPanel.transform);
        BuildUnifiedBottomPanel(arOverlayPanel.transform);
    }

    private void BuildTopFloatingHeader(Transform parent)
    {
        GameObject topBar = CreateUIObject("TopFloatingHeader", parent);
        RectTransform r = topBar.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 1f);
        r.anchorMax = new Vector2(0.5f, 1f);
        r.pivot = new Vector2(0.5f, 1f);
        r.anchoredPosition = new Vector2(0, -130);
        r.sizeDelta = new Vector2(980, 76);

        Image bg = topBar.AddComponent<Image>();
        bg.color = ColorBgSurface;

        GameObject resetBtn = CreateUIObject("ResetBtn", topBar.transform);
        RectTransform rbRect = resetBtn.GetComponent<RectTransform>();
        rbRect.anchorMin = new Vector2(0f, 0.5f);
        rbRect.anchorMax = new Vector2(0f, 0.5f);
        rbRect.pivot = new Vector2(0f, 0.5f);
        rbRect.anchoredPosition = new Vector2(16, 0);
        rbRect.sizeDelta = new Vector2(130, 52);

        Image rbBg = resetBtn.AddComponent<Image>();
        rbBg.color = ColorBgPrimary;
        Button rBtn = AddButton(resetBtn);
        rBtn.onClick.AddListener(() => {
            if (placementManager != null) placementManager.ClearAllFurniture();
        });
        AddRobustText(resetBtn, "RESET", 17, FontStyle.Bold, ColorTextSecondary);

        GameObject statusObj = CreateUIObject("StatusLabel", topBar.transform);
        RectTransform stRect = statusObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 0f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.offsetMin = new Vector2(160, 0);
        stRect.offsetMax = new Vector2(-160, 0);

        mainStatusText = statusObj.AddComponent<Text>();
        mainStatusText.font = appFont;
        mainStatusText.fontSize = 20;
        mainStatusText.fontStyle = FontStyle.Bold;
        mainStatusText.color = ColorTextPrimary;
        mainStatusText.alignment = TextAnchor.MiddleCenter;
        mainStatusText.horizontalOverflow = HorizontalWrapMode.Overflow;
        mainStatusText.verticalOverflow = VerticalWrapMode.Overflow;
        mainStatusText.text = "Tap floor to place White Sofa";

        GameObject photoBtn = CreateUIObject("PhotoBtn", topBar.transform);
        RectTransform pbRect = photoBtn.GetComponent<RectTransform>();
        pbRect.anchorMin = new Vector2(1f, 0.5f);
        pbRect.anchorMax = new Vector2(1f, 0.5f);
        pbRect.pivot = new Vector2(1f, 0.5f);
        pbRect.anchoredPosition = new Vector2(-16, 0);
        pbRect.sizeDelta = new Vector2(130, 52);

        Image pbBg = photoBtn.AddComponent<Image>();
        pbBg.color = ColorBgPrimary;
        Button pBtn = AddButton(photoBtn);
        pBtn.onClick.AddListener(TriggerSnapshot);
        AddRobustText(photoBtn, "PHOTO", 17, FontStyle.Bold, ColorTextPrimary);
    }

    private void BuildUnifiedBottomPanel(Transform parent)
    {
        GameObject bottomPanel = CreateUIObject("UnifiedBottomPanel", parent);
        RectTransform r = bottomPanel.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 0f);
        r.anchorMax = new Vector2(0.5f, 0f);
        r.pivot = new Vector2(0.5f, 0f);
        r.anchoredPosition = new Vector2(0, 40);
        r.sizeDelta = new Vector2(1020, 360);

        Image bg = bottomPanel.AddComponent<Image>();
        bg.color = ColorBgSurface;

        GameObject handle = CreateUIObject("HandlePill", bottomPanel.transform);
        RectTransform hRect = handle.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0.5f, 1f);
        hRect.anchorMax = new Vector2(0.5f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.anchoredPosition = new Vector2(0, -10);
        hRect.sizeDelta = new Vector2(64, 6);
        Image hImg = handle.AddComponent<Image>();
        hImg.color = HandleColor;

        BuildControlRowInsideSheet(bottomPanel.transform);
        BuildSofaSelectionRowInsideSheet(bottomPanel.transform);
    }

    private void BuildControlRowInsideSheet(Transform parent)
    {
        GameObject ctrlRow = CreateUIObject("ControlRow", parent);
        RectTransform r = ctrlRow.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 1f);
        r.anchorMax = new Vector2(1f, 1f);
        r.pivot = new Vector2(0.5f, 1f);
        r.anchoredPosition = new Vector2(0, -28);
        r.sizeDelta = new Vector2(0, 78);

        HorizontalLayoutGroup hlg = ctrlRow.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(16, 16, 0, 0);
        hlg.spacing = 14;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        GameObject rotL = CreateUIObject("RotLBtn", ctrlRow.transform);
        LayoutElement leL = rotL.AddComponent<LayoutElement>();
        leL.preferredWidth = 170f;
        leL.preferredHeight = 72f;
        leL.minWidth = 150f;
        leL.minHeight = 65f;
        Image bgL = rotL.AddComponent<Image>();
        bgL.color = ColorBgPrimary;
        Button bL = AddButton(rotL);
        bL.onClick.AddListener(() => {
            if (placementManager != null) placementManager.RotateActiveFurniture(-15f);
        });
        AddRobustText(rotL, "ROTATE L\n(-15°)", 16, FontStyle.Bold, ColorTextPrimary);

        GameObject lockBtn = CreateUIObject("LockBtn", ctrlRow.transform);
        LayoutElement leLock = lockBtn.AddComponent<LayoutElement>();
        leLock.preferredWidth = 470f;
        leLock.preferredHeight = 72f;
        leLock.minWidth = 380f;
        leLock.minHeight = 65f;
        Image bgLock = lockBtn.AddComponent<Image>();
        bgLock.color = ColorAccentTeal;
        Button bLock = AddButton(lockBtn);
        bLock.onClick.AddListener(OnLockClicked);
        AddRobustText(lockBtn, "LOCK POSITION", 24, FontStyle.Bold, Color.white);

        GameObject rotR = CreateUIObject("RotRBtn", ctrlRow.transform);
        LayoutElement leR = rotR.AddComponent<LayoutElement>();
        leR.preferredWidth = 170f;
        leR.preferredHeight = 72f;
        leR.minWidth = 150f;
        leR.minHeight = 65f;
        Image bgR = rotR.AddComponent<Image>();
        bgR.color = ColorBgPrimary;
        Button bR = AddButton(rotR);
        bR.onClick.AddListener(() => {
            if (placementManager != null) placementManager.RotateActiveFurniture(15f);
        });
        AddRobustText(rotR, "ROTATE R\n(+15°)", 16, FontStyle.Bold, ColorTextPrimary);

        GameObject delBtn = CreateUIObject("DelBtn", ctrlRow.transform);
        LayoutElement leDel = delBtn.AddComponent<LayoutElement>();
        leDel.preferredWidth = 84f;
        leDel.preferredHeight = 72f;
        leDel.minWidth = 70f;
        leDel.minHeight = 65f;
        Image bgDel = delBtn.AddComponent<Image>();
        bgDel.color = ColorBgPrimary;
        Button bDel = AddButton(delBtn);
        bDel.onClick.AddListener(() => {
            if (placementManager != null) placementManager.DeleteCurrentOrLast();
        });
        AddRobustText(delBtn, "DEL", 18, FontStyle.Bold, ColorTextPrimary);
    }

    private void BuildSofaSelectionRowInsideSheet(Transform parent)
    {
        GameObject sofaRow = CreateUIObject("SofaSelectionRow", parent);
        RectTransform r = sofaRow.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 0f);
        r.anchorMax = new Vector2(1f, 0f);
        r.pivot = new Vector2(0.5f, 0f);
        r.anchoredPosition = new Vector2(0, 16);
        r.sizeDelta = new Vector2(0, 220);

        HorizontalLayoutGroup hlg = sofaRow.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(16, 16, 0, 0);
        hlg.spacing = 16;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        sofaCardBorderImgs = new Image[sofaChoices.Count];
        sofaCardBgImgs = new Image[sofaChoices.Count];
        sofaCardTitleTexts = new Text[sofaChoices.Count];
        sofaCardActiveBadges = new GameObject[sofaChoices.Count];

        for (int i = 0; i < sofaChoices.Count; i++)
        {
            int index = i;
            SofaChoice choice = sofaChoices[i];
            bool isSel = (i == selectedSofaIndex);

            GameObject card = CreateUIObject("Card_" + choice.id, sofaRow.transform);
            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredWidth = 310f;
            le.preferredHeight = 210f;
            le.minWidth = 260f;
            le.minHeight = 180f;

            Image cBg = card.AddComponent<Image>();
            cBg.color = isSel ? ColorAccentActive : ColorBgPrimary;
            sofaCardBgImgs[i] = cBg;

            GameObject borderObj = CreateUIObject("Border", card.transform);
            SetStretch(borderObj.GetComponent<RectTransform>());
            Image borderImg = borderObj.AddComponent<Image>();
            borderImg.color = isSel ? ColorAccentTeal : BorderSubtle;
            borderImg.raycastTarget = false;
            sofaCardBorderImgs[i] = borderImg;

            Button btn = AddButton(card);
            btn.onClick.AddListener(() => OnSofaSelected(index));

            GameObject thumbObj = CreateUIObject("Thumbnail", card.transform);
            RectTransform tRect = thumbObj.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.08f, 0.28f);
            tRect.anchorMax = new Vector2(0.92f, 0.94f);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            Image thumbImg = thumbObj.AddComponent<Image>();
            thumbImg.sprite = GetSprite(choice.thumbnailPath);
            thumbImg.preserveAspect = true;

            GameObject labelObj = CreateUIObject("Label", card.transform);
            RectTransform lRect = labelObj.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0.04f, 0.04f);
            lRect.anchorMax = new Vector2(0.96f, 0.28f);
            lRect.offsetMin = Vector2.zero;
            lRect.offsetMax = Vector2.zero;
            Text labelText = labelObj.AddComponent<Text>();
            labelText.font = appFont;
            labelText.fontSize = 20;
            labelText.fontStyle = isSel ? FontStyle.Bold : FontStyle.Normal;
            labelText.color = isSel ? ColorAccentTeal : ColorTextPrimary;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
            labelText.verticalOverflow = VerticalWrapMode.Overflow;
            labelText.text = choice.name;
            sofaCardTitleTexts[i] = labelText;

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
            bt.font = appFont;
            bt.fontSize = 14;
            bt.fontStyle = FontStyle.Bold;
            bt.color = Color.white;
            bt.alignment = TextAnchor.MiddleCenter;
            bt.horizontalOverflow = HorizontalWrapMode.Overflow;
            bt.verticalOverflow = VerticalWrapMode.Overflow;
            bt.text = "ACTIVE";

            sofaCardActiveBadges[i] = badgeObj;
            badgeObj.SetActive(isSel);
        }
    }

    private static Button AddButton(GameObject parent)
    {
        Button b = parent.AddComponent<Button>();
        Navigation nav = b.navigation;
        nav.mode = Navigation.Mode.None;
        b.navigation = nav;
        return b;
    }

    private void BuildSnapshotFlash(Transform parent)
    {
        snapshotFlash = CreateUIObject("SnapshotFlashOverlay", parent);
        SetStretch(snapshotFlash.GetComponent<RectTransform>());

        Image img = snapshotFlash.AddComponent<Image>();
        img.color = Color.white;

        snapshotCanvasGroup = snapshotFlash.AddComponent<CanvasGroup>();
        snapshotCanvasGroup.alpha = 0f;
        snapshotFlash.SetActive(false);
    }

    private void BuildToast(Transform parent)
    {
        toastObj = CreateUIObject("ToastNotification", parent);
        RectTransform r = toastObj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 0.44f);
        r.anchorMax = new Vector2(0.5f, 0.44f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(680, 76);

        Image bg = toastObj.AddComponent<Image>();
        bg.color = new Color(0.14f, 0.14f, 0.14f, 0.94f);

        GameObject textObj = CreateUIObject("Text", toastObj.transform);
        SetStretch(textObj.GetComponent<RectTransform>());
        toastText = textObj.AddComponent<Text>();
        toastText.font = appFont;
        toastText.fontSize = 22;
        toastText.color = Color.white;
        toastText.alignment = TextAnchor.MiddleCenter;
        toastText.horizontalOverflow = HorizontalWrapMode.Overflow;
        toastText.verticalOverflow = VerticalWrapMode.Overflow;
        toastText.text = "";

        toastObj.SetActive(false);
    }

    private IEnumerator SplashScreenSequence()
    {
        yield return new WaitForSeconds(1.8f);

        float elapsed = 0f;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (splashCanvasGroup != null)
                splashCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        if (splashPanel != null) splashPanel.SetActive(false);
    }

    public void OnSofaSelected(int index)
    {
        selectedSofaIndex = index;
        SofaChoice choice = sofaChoices[index];

        for (int i = 0; i < sofaChoices.Count; i++)
        {
            bool isCurrent = (i == index);
            if (sofaCardBorderImgs != null && i < sofaCardBorderImgs.Length && sofaCardBorderImgs[i] != null)
            {
                sofaCardBorderImgs[i].color = isCurrent ? ColorAccentTeal : BorderSubtle;
            }
            if (sofaCardBgImgs != null && i < sofaCardBgImgs.Length && sofaCardBgImgs[i] != null)
            {
                sofaCardBgImgs[i].color = isCurrent ? ColorAccentActive : ColorBgPrimary;
            }
            if (sofaCardTitleTexts != null && i < sofaCardTitleTexts.Length && sofaCardTitleTexts[i] != null)
            {
                sofaCardTitleTexts[i].color = isCurrent ? ColorAccentTeal : ColorTextPrimary;
                sofaCardTitleTexts[i].fontStyle = isCurrent ? FontStyle.Bold : FontStyle.Normal;
            }
            if (sofaCardActiveBadges != null && i < sofaCardActiveBadges.Length && sofaCardActiveBadges[i] != null)
            {
                sofaCardActiveBadges[i].SetActive(isCurrent);
            }
        }

        if (placementManager != null)
        {
            placementManager.SwitchSelectedFurniture(choice.prefab);
        }

        UpdateStatusText();
    }

    public void OnLockClicked()
    {
        if (placementManager == null) return;

        bool success = placementManager.LockActiveFurniture();
        if (success)
        {
            UpdateStatusText();
        }
        else
        {
            ShowToast("Tap the floor first to place the sofa before locking!");
        }
    }

    public void TriggerSnapshot()
    {
        StartCoroutine(CaptureSnapshotRoutine());
    }

    private IEnumerator CaptureSnapshotRoutine()
    {
        if (arOverlayPanel != null) arOverlayPanel.SetActive(false);

        yield return new WaitForEndOfFrame();

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = "FurnishAR_" + timestamp + ".png";
        string savePath;

        if (Application.platform == RuntimePlatform.Android)
        {
            savePath = Path.Combine(Application.persistentDataPath, filename);
        }
        else
        {
            savePath = Path.Combine(Application.dataPath, "../" + filename);
        }

        ScreenCapture.CaptureScreenshot(filename);

        yield return new WaitForSeconds(0.1f);

        if (arOverlayPanel != null) arOverlayPanel.SetActive(true);

        if (snapshotFlash != null && snapshotCanvasGroup != null)
        {
            snapshotFlash.SetActive(true);
            snapshotCanvasGroup.alpha = 1f;

            float elapsed = 0f;
            float dur = 0.35f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                snapshotCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / dur);
                yield return null;
            }
            snapshotFlash.SetActive(false);
        }

        ShowToast("Photo saved to gallery!");
    }

    public void ShowToast(string message)
    {
        if (toastObj == null || toastText == null) return;

        if (toastCoroutine != null) StopCoroutine(toastCoroutine);
        toastCoroutine = StartCoroutine(ToastRoutine(message));
    }

    private IEnumerator ToastRoutine(string message)
    {
        toastText.text = message;
        toastObj.SetActive(true);
        yield return new WaitForSeconds(2.2f);
        toastObj.SetActive(false);
    }

    private void UpdateStatusText()
    {
        if (mainStatusText == null) return;

        bool hasActive = placementManager != null && placementManager.activeFurnitureInstance != null;
        int lockedCount = placementManager != null ? placementManager.lockedFurnitureList.Count : 0;
        string currentName = sofaChoices[selectedSofaIndex].name;

        if (hasActive)
        {
            mainStatusText.text = "POSITIONING " + currentName.ToUpper() + " - Tap LOCK to fix";
            mainStatusText.color = new Color(0.85f, 0.45f, 0.05f);
        }
        else if (lockedCount > 0)
        {
            mainStatusText.text = lockedCount + " SOFA(S) LOCKED - Tap floor to place another";
            mainStatusText.color = ColorAccentTeal;
        }
        else
        {
            mainStatusText.text = "Tap floor to place " + currentName;
            mainStatusText.color = ColorTextPrimary;
        }
    }

    private void OnActiveFurnitureSpawnedHandler(GameObject obj)
    {
        UpdateStatusText();
        ShowToast("Position sofa on floor, then tap Lock");
    }

    private void OnActiveFurnitureMovedHandler(GameObject obj)
    {
        UpdateStatusText();
    }

    private void OnFurnitureLockedHandler(GameObject obj)
    {
        UpdateStatusText();
        int totalLocked = placementManager != null ? placementManager.lockedFurnitureList.Count : 1;
        ShowToast("Position Locked (" + totalLocked + " fixed in room). Tap floor to add another!");
    }

    private void OnActiveFurnitureDeletedHandler()
    {
        UpdateStatusText();
        ShowToast("Preview removed");
    }

    private void OnLockedFurnitureRemovedHandler()
    {
        UpdateStatusText();
        int totalLocked = placementManager != null ? placementManager.lockedFurnitureList.Count : 0;
        ShowToast("Removed last sofa (" + totalLocked + " remaining)");
    }

    private void OnAllClearedHandler()
    {
        UpdateStatusText();
        ShowToast("All sofas cleared. Tap floor to place.");
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

    private void AddRobustText(GameObject parent, string text, int fontSize, FontStyle style, Color color)
    {
        GameObject tObj = CreateUIObject("Label", parent.transform);
        SetStretch(tObj.GetComponent<RectTransform>());
        Text t = tObj.AddComponent<Text>();
        t.font = appFont;
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.text = text;
    }
}
