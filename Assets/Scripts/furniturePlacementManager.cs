using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Vuforia;

public class furniturePlacementManager : MonoBehaviour
{
    public static furniturePlacementManager Instance { get; private set; }

    [Header("Vuforia Components")]
    public PlaneFinderBehaviour planeFinder;

    [Header("Available Prefabs")]
    public GameObject whiteSofaPrefab;
    public GameObject brownSofaPrefab;
    public GameObject lightBrownSofaPrefab;

    [Header("Current State")]
    public GameObject currentSelectedPrefab;
    public GameObject activeFurnitureInstance = null;
    public List<GameObject> lockedFurnitureList = new List<GameObject>();

    private float lastGroundY = 0f;
    private bool isTouchOverUI = false;

    public event Action<GameObject> OnActiveFurnitureSpawned;
    public event Action<GameObject> OnActiveFurnitureMoved;
    public event Action<GameObject> OnFurnitureLocked;
    public event Action OnActiveFurnitureDeleted;
    public event Action OnLockedFurnitureRemoved;
    public event Action OnAllCleared;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }

    private void Start()
    {
        if (planeFinder == null) planeFinder = FindObjectOfType<PlaneFinderBehaviour>();

        if (currentSelectedPrefab == null && whiteSofaPrefab != null)
        {
            currentSelectedPrefab = whiteSofaPrefab;
        }

        if (activeFurnitureInstance == null)
        {
            GameObject showcase = GameObject.Find("InitialShowcaseFurniture");
            if (showcase != null)
            {
                activeFurnitureInstance = showcase;
                FurnitureLockBadge badge = activeFurnitureInstance.GetComponent<FurnitureLockBadge>();
                if (badge == null)
                {
                    badge = activeFurnitureInstance.AddComponent<FurnitureLockBadge>();
                    badge.Setup(false);
                }
            }
        }
    }

    private void Update()
    {
        Vector2 screenPosition = Vector2.zero;
        bool isPointerDown = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            screenPosition = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                isTouchOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                if (!isTouchOverUI) isPointerDown = true;
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (!isTouchOverUI) isPointerDown = true;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isTouchOverUI = false;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            screenPosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                isTouchOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                if (!isTouchOverUI) isPointerDown = true;
            }
            else if (!isTouchOverUI)
            {
                isPointerDown = true;
            }
        }
        else
        {
            isTouchOverUI = false;
        }

        if (!isPointerDown) return;

        if (TryGetGroundHit(screenPosition, out Pose hitPose))
        {
            lastGroundY = hitPose.position.y;

            if (activeFurnitureInstance == null)
            {
                if (currentSelectedPrefab != null)
                {
                    Camera cam = Camera.main;
                    float camYaw = cam != null ? cam.transform.eulerAngles.y : hitPose.rotation.eulerAngles.y;
                    Quaternion uprightRotation = Quaternion.Euler(0f, camYaw + 180f, 0f);

                    activeFurnitureInstance = Instantiate(currentSelectedPrefab, hitPose.position, uprightRotation);
                    SnapToGround(activeFurnitureInstance, lastGroundY);

                    FurnitureLockBadge badge = activeFurnitureInstance.AddComponent<FurnitureLockBadge>();
                    badge.Setup(false);

                    OnActiveFurnitureSpawned?.Invoke(activeFurnitureInstance);
                }
            }
            else
            {
                activeFurnitureInstance.transform.position = hitPose.position;
                SnapToGround(activeFurnitureInstance, lastGroundY);
                OnActiveFurnitureMoved?.Invoke(activeFurnitureInstance);
            }
        }
    }

    public void SwitchSelectedFurniture(GameObject newPrefab)
    {
        if (newPrefab == null) return;
        currentSelectedPrefab = newPrefab;

        if (activeFurnitureInstance != null)
        {
            Vector3 pos = activeFurnitureInstance.transform.position;
            Quaternion rot = activeFurnitureInstance.transform.rotation;
            Vector3 scale = activeFurnitureInstance.transform.localScale;

            Destroy(activeFurnitureInstance);
            activeFurnitureInstance = Instantiate(newPrefab, pos, rot);
            activeFurnitureInstance.transform.localScale = scale;
            SnapToGround(activeFurnitureInstance, lastGroundY);

            FurnitureLockBadge badge = activeFurnitureInstance.AddComponent<FurnitureLockBadge>();
            badge.Setup(false);

            OnActiveFurnitureSpawned?.Invoke(activeFurnitureInstance);
        }
    }

    public bool LockActiveFurniture()
    {
        if (activeFurnitureInstance == null) return false;

        SnapToGround(activeFurnitureInstance, lastGroundY);

        FurnitureLockBadge badge = activeFurnitureInstance.GetComponent<FurnitureLockBadge>();
        if (badge != null) badge.SetLocked(true);

        lockedFurnitureList.Add(activeFurnitureInstance);

        GameObject lockedObj = activeFurnitureInstance;
        activeFurnitureInstance = null;

        OnFurnitureLocked?.Invoke(lockedObj);
        return true;
    }

    public void RotateActiveFurniture(float angleDelta)
    {
        if (activeFurnitureInstance == null) return;
        activeFurnitureInstance.transform.Rotate(Vector3.up, angleDelta, Space.World);
        OnActiveFurnitureMoved?.Invoke(activeFurnitureInstance);
    }

    public void DeleteCurrentOrLast()
    {
        if (activeFurnitureInstance != null)
        {
            Destroy(activeFurnitureInstance);
            activeFurnitureInstance = null;
            OnActiveFurnitureDeleted?.Invoke();
        }
        else if (lockedFurnitureList.Count > 0)
        {
            int lastIndex = lockedFurnitureList.Count - 1;
            GameObject lastObj = lockedFurnitureList[lastIndex];
            lockedFurnitureList.RemoveAt(lastIndex);
            if (lastObj != null) Destroy(lastObj);
            OnLockedFurnitureRemoved?.Invoke();
        }
    }

    public void ClearAllFurniture()
    {
        if (activeFurnitureInstance != null)
        {
            Destroy(activeFurnitureInstance);
            activeFurnitureInstance = null;
        }

        for (int i = 0; i < lockedFurnitureList.Count; i++)
        {
            if (lockedFurnitureList[i] != null)
            {
                Destroy(lockedFurnitureList[i]);
            }
        }
        lockedFurnitureList.Clear();
        OnAllCleared?.Invoke();
    }

    private bool TryGetGroundHit(Vector2 screenPosition, out Pose hitPose)
    {
        hitPose = Pose.identity;

        if (VuforiaBehaviour.Instance != null && VuforiaBehaviour.Instance.World != null)
        {
            if (VuforiaBehaviour.Instance.World.HitTest(screenPosition, 1.4f, out HitTestResult hitResult))
            {
                hitPose = new Pose(hitResult.Position, hitResult.Rotation);
                return true;
            }
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray ray = cam.ScreenPointToRay(screenPosition);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, lastGroundY, 0f));
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 worldPoint = ray.GetPoint(enter);
                hitPose = new Pose(worldPoint, Quaternion.identity);
                return true;
            }
        }

        return false;
    }

    private void SnapToGround(GameObject obj, float groundY)
    {
        if (obj == null) return;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Vector3 p = obj.transform.position;
            p.y = groundY;
            obj.transform.position = p;
            return;
        }

        float minY = float.MaxValue;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].enabled || !renderers[i].gameObject.activeInHierarchy) continue;
            float rMinY = renderers[i].bounds.min.y;
            if (rMinY < minY) minY = rMinY;
        }

        if (minY == float.MaxValue) return;

        float offset = groundY - minY;
        obj.transform.position += new Vector3(0f, offset, 0f);
    }
}
