using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Image = UnityEngine.UI.Image;

[System.Serializable]
public class FurnitureItem
{
    public string id;
    public string displayName;
    public string category;
    public GameObject prefab;
    public Sprite thumbnail;
}

[System.Serializable]
public class FurnitureCategory
{
    public string id;
    public string displayName;
    public List<FurnitureItem> items = new List<FurnitureItem>();
}

public class ARFurnitureAppManager : MonoBehaviour
{
    public static ARFurnitureAppManager Instance { get; private set; }

    [Header("Dependencies")]
    public furniturePlacementManager placementManager;

    [Header("Catalog Suite Data")]
    public List<FurnitureCategory> categories = new List<FurnitureCategory>();
    public int selectedCategoryIndex = 0;
    public string activeItemId = "WhiteSofa";
    public FurnitureItem activeItem;

    [Header("Color Palette Tokens")]
    public static readonly Color ColorBgPrimary = new Color(0.973f, 0.965f, 0.945f, 1f);
    public static readonly Color ColorBgSurface = new Color(1f, 1f, 1f, 0.98f);
    public static readonly Color ColorTextPrimary = new Color(0.141f, 0.141f, 0.141f, 1f);
    public static readonly Color ColorTextSecondary = new Color(0.45f, 0.45f, 0.45f, 1f);
    public static readonly Color ColorAccentTeal = new Color(0.18f, 0.68f, 0.68f, 1f);
    public static readonly Color ColorAccentActive = new Color(0.92f, 0.98f, 0.98f, 1f);
    public static readonly Color BorderSubtle = new Color(0.88f, 0.87f, 0.84f, 1f);
    public static readonly Color HandleColor = new Color(0.80f, 0.79f, 0.76f, 1f);

    [Header("AR Overlay References")]
    public GameObject arOverlayPanel;
    public Text mainStatusText;
    public Image activeItemThumbImg;
    public Text activeItemNameText;

    [Header("Catalog Modal References")]
    public GameObject catalogModalPanel;
    public Text categoryHeaderTitle;
    public Transform categorySidebarContainer;
    public Transform furnitureGridContainer;

    [Header("System Overlays")]
    public GameObject snapshotFlash;
    public CanvasGroup snapshotCanvasGroup;
    public GameObject toastObj;
    public Text toastText;
    public GameObject splashPanel;
    public CanvasGroup splashCanvasGroup;

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

        if (catalogModalPanel != null)
        {
            catalogModalPanel.SetActive(false);
        }
    }

    private void Start()
    {
        if (placementManager == null)
            placementManager = FindObjectOfType<furniturePlacementManager>();

        if (placementManager != null)
        {
            placementManager.OnActiveFurnitureSpawned += OnActiveFurnitureSpawnedHandler;
            placementManager.OnActiveFurnitureMoved += OnActiveFurnitureMovedHandler;
            placementManager.OnFurnitureLocked += OnFurnitureLockedHandler;
            placementManager.OnActiveFurnitureDeleted += OnActiveFurnitureDeletedHandler;
            placementManager.OnLockedFurnitureRemoved += OnLockedFurnitureRemovedHandler;
            placementManager.OnAllCleared += OnAllClearedHandler;
        }

        BindExistingSceneUI();

        if (activeItem == null)
        {
            FindAndSetActiveItem(activeItemId);
        }

        if (activeItem != null && placementManager != null)
        {
            placementManager.currentSelectedPrefab = activeItem.prefab;
        }

        UpdateActiveItemUI();
        UpdateStatusText();

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

            Transform launcherRow = bottomPanel.Find("CatalogLauncherRow");
            if (launcherRow != null)
            {
                Transform activeCard = launcherRow.Find("ActiveItemCard");
                if (activeCard != null)
                {
                    activeItemThumbImg = activeCard.Find("Thumbnail")?.GetComponent<Image>();
                    activeItemNameText = activeCard.Find("ItemName")?.GetComponent<Text>();

                    Button cardBtn = activeCard.GetComponent<Button>();
                    if (cardBtn != null)
                    {
                        cardBtn.onClick.RemoveAllListeners();
                        cardBtn.onClick.AddListener(OpenCatalogModal);
                    }
                }

                Transform openBtn = launcherRow.Find("OpenCatalogBtn");
                if (openBtn != null)
                {
                    Button b = openBtn.GetComponent<Button>();
                    if (b != null)
                    {
                        b.onClick.RemoveAllListeners();
                        b.onClick.AddListener(OpenCatalogModal);
                    }
                }
            }
        }

        Transform modal = overlay.Find("CatalogModalPanel");
        if (modal != null)
        {
            catalogModalPanel = modal.gameObject;

            Transform scrim = modal.Find("ModalScrim");
            if (scrim != null)
            {
                Button scrimBtn = scrim.GetComponent<Button>();
                if (scrimBtn != null)
                {
                    scrimBtn.onClick.RemoveAllListeners();
                    scrimBtn.onClick.AddListener(CloseCatalogModal);
                }
            }

            Transform header = modal.Find("ModalWindow/ModalHeader");
            if (header != null)
            {
                Button closeBtn = header.Find("CloseBtn")?.GetComponent<Button>();
                if (closeBtn != null)
                {
                    closeBtn.onClick.RemoveAllListeners();
                    closeBtn.onClick.AddListener(CloseCatalogModal);
                }
            }

            Transform split = modal.Find("ModalWindow/SplitBody");
            if (split != null)
            {
                categorySidebarContainer = split.Find("CategorySidebar/ScrollRect/Viewport/Content");
                if (categorySidebarContainer == null)
                    categorySidebarContainer = split.Find("CategorySidebar/Content");

                categoryHeaderTitle = split.Find("FurnitureGridArea/GridHeader/CategoryTitle")?.GetComponent<Text>();

                furnitureGridContainer = split.Find("FurnitureGridArea/ScrollRect/Viewport/Content");
                if (furnitureGridContainer == null)
                    furnitureGridContainer = split.Find("FurnitureGridArea/Content");

                BindCategoryButtons();
                BindFurnitureCardButtons();
            }

            catalogModalPanel.SetActive(false);
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

        return true;
    }

    private void BindCategoryButtons()
    {
        if (categorySidebarContainer == null) return;

        for (int i = 0; i < categories.Count; i++)
        {
            int catIdx = i;
            string catId = categories[i].id;
            Transform btnTransform = categorySidebarContainer.Find("CatBtn_" + catId);
            if (btnTransform != null)
            {
                Button btn = btnTransform.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => SelectCategory(catIdx));
                }
            }
        }
    }

    private void BindFurnitureCardButtons()
    {
        if (furnitureGridContainer == null) return;

        for (int c = 0; c < categories.Count; c++)
        {
            FurnitureCategory cat = categories[c];
            Transform catView = furnitureGridContainer.Find("CategoryView_" + cat.id);
            if (catView == null) continue;

            for (int i = 0; i < cat.items.Count; i++)
            {
                FurnitureItem item = cat.items[i];
                Transform card = catView.Find("Card_" + item.id);
                if (card != null)
                {
                    Button btn = card.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => SelectFurnitureItem(item));
                    }
                }
            }
        }
    }

    public void OpenCatalogModal()
    {
        if (catalogModalPanel != null)
        {
            catalogModalPanel.SetActive(true);
            SelectCategory(selectedCategoryIndex);
        }
    }

    public void CloseCatalogModal()
    {
        if (catalogModalPanel != null)
        {
            catalogModalPanel.SetActive(false);
        }
    }

    public void SelectCategory(int categoryIndex)
    {
        if (categoryIndex < 0 || categoryIndex >= categories.Count) return;

        selectedCategoryIndex = categoryIndex;
        FurnitureCategory cat = categories[selectedCategoryIndex];

        if (categoryHeaderTitle != null)
        {
            categoryHeaderTitle.text = cat.displayName.ToUpper() + " (" + cat.items.Count + " ITEMS)";
        }

        if (categorySidebarContainer != null)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                Transform btnTransform = categorySidebarContainer.Find("CatBtn_" + categories[i].id);
                if (btnTransform != null)
                {
                    Image bg = btnTransform.GetComponent<Image>();
                    Text label = btnTransform.Find("Label")?.GetComponent<Text>();
                    Image indicator = btnTransform.Find("ActiveIndicator")?.GetComponent<Image>();

                    bool isSelected = (i == selectedCategoryIndex);

                    if (bg != null)
                    {
                        bg.color = isSelected ? ColorAccentTeal : ColorBgSurface;
                    }

                    if (label != null)
                    {
                        label.color = isSelected ? Color.white : ColorTextPrimary;
                        label.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
                    }

                    if (indicator != null)
                    {
                        indicator.gameObject.SetActive(isSelected);
                    }
                }
            }
        }

        if (furnitureGridContainer != null)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                Transform catView = furnitureGridContainer.Find("CategoryView_" + categories[i].id);
                if (catView != null)
                {
                    catView.gameObject.SetActive(i == selectedCategoryIndex);
                }
            }
        }

        RefreshCardActiveStates();
    }

    public void SelectFurnitureItem(FurnitureItem item)
    {
        if (item == null) return;

        activeItem = item;
        activeItemId = item.id;

        UpdateActiveItemUI();

        if (placementManager != null)
        {
            placementManager.SwitchSelectedFurniture(item.prefab);
        }

        UpdateStatusText();
        CloseCatalogModal();

        ShowToast("Selected " + item.displayName + " - Tap floor to place");
    }

    public void SelectFurnitureItemById(string itemId)
    {
        FindAndSetActiveItem(itemId);
        if (activeItem != null)
        {
            SelectFurnitureItem(activeItem);
        }
    }

    private void FindAndSetActiveItem(string itemId)
    {
        foreach (var cat in categories)
        {
            foreach (var item in cat.items)
            {
                if (item.id == itemId)
                {
                    activeItem = item;
                    activeItemId = item.id;
                    return;
                }
            }
        }

        if (categories.Count > 0 && categories[0].items.Count > 0)
        {
            activeItem = categories[0].items[0];
            activeItemId = activeItem.id;
        }
    }

    private void UpdateActiveItemUI()
    {
        if (activeItem == null) return;

        if (activeItemThumbImg != null && activeItem.thumbnail != null)
        {
            activeItemThumbImg.sprite = activeItem.thumbnail;
        }

        if (activeItemNameText != null)
        {
            activeItemNameText.text = activeItem.displayName;
        }

        RefreshCardActiveStates();
    }

    private void RefreshCardActiveStates()
    {
        if (furnitureGridContainer == null || activeItem == null) return;

        for (int c = 0; c < categories.Count; c++)
        {
            FurnitureCategory cat = categories[c];
            Transform catView = furnitureGridContainer.Find("CategoryView_" + cat.id);
            if (catView == null) continue;

            for (int i = 0; i < cat.items.Count; i++)
            {
                FurnitureItem item = cat.items[i];
                Transform card = catView.Find("Card_" + item.id);
                if (card != null)
                {
                    bool isActive = (item.id == activeItemId);

                    Image cardBg = card.GetComponent<Image>();
                    if (cardBg != null)
                    {
                        cardBg.color = isActive ? ColorAccentActive : ColorBgSurface;
                    }

                    Image border = card.Find("Border")?.GetComponent<Image>();
                    if (border != null)
                    {
                        border.color = isActive ? ColorAccentTeal : BorderSubtle;
                    }

                    GameObject badge = card.Find("ActiveBadge")?.gameObject;
                    if (badge != null)
                    {
                        badge.SetActive(isActive);
                    }
                }
            }
        }
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
            ShowToast("Tap the floor first to place the furniture before locking!");
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
        string currentName = activeItem != null ? activeItem.displayName : "Furniture";

        if (hasActive)
        {
            mainStatusText.text = "POSITIONING " + currentName.ToUpper() + " - Tap LOCK to fix";
            mainStatusText.color = new Color(0.85f, 0.45f, 0.05f);
        }
        else if (lockedCount > 0)
        {
            mainStatusText.text = lockedCount + " PIECE(S) LOCKED - Tap floor to place another";
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
        ShowToast("Position piece on floor, then tap Lock");
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
        ShowToast("Removed last piece (" + totalLocked + " remaining)");
    }

    private void OnAllClearedHandler()
    {
        UpdateStatusText();
        ShowToast("All furniture cleared. Tap floor to place.");
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
}
