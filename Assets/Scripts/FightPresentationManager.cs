using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FightPresentationManager : MonoBehaviour
{
    private const string ArenaRootName = "ArenaRoot";
    private const string ScanChamberRootName = "ScanChamberRoot";
    private const string MonitorTransitionRootName = "MonitorTransitionRoot";
    private const string PresentationCameraName = "PresentationCamera";
    private const string FightPresentationViewportName = "FightPresentationViewport";
    private const float ScanIntroDelaySeconds = 1.5f;
    private const float MonitorTransitionDelaySeconds = 1f;
    private const float CameraMoveDurationSeconds = 0.75f;
    private const float RoundActionDurationSeconds = 0.8f;
    private const int PresentationRenderTextureWidth = 1280;
    private const int PresentationRenderTextureHeight = 720;

    private static GameObject sharedArenaRoot;
    private static GameObject sharedScanChamberRoot;
    private static GameObject sharedMonitorTransitionRoot;
    private static GameObject sharedPresentationCameraObject;

    private GameObject arenaRoot;
    private GameObject scanChamberRoot;
    private GameObject monitorTransitionRoot;
    private GameObject presentationCameraObject;
    private GameObject fightPresentationViewportObject;
    private Camera presentationCamera;
    private RawImage fightPresentationViewportImage;
    private RenderTexture presentationRenderTexture;
    private bool arenaObjectsCreated;
    private bool scanChamberObjectsCreated;
    private bool monitorTransitionObjectsCreated;
    private bool arenaImpactEffectsCreated;
    private bool imprintCorruptionNodesCreated;
    private bool healthBarsCreated;
    private int visualMaxHealthA;
    private int visualMaxHealthB;
    private Transform fighterATransform;
    private Transform fighterBTransform;
    private Transform scanDogATransform;
    private Transform scanDogBTransform;
    private GameObject impactSparkA;
    private GameObject impactSparkB;
    private GameObject corruptionNodeA;
    private GameObject corruptionNodeB;
    private GameObject impactRingA;
    private GameObject impactRingB;
    private GameObject[] imprintCorruptionNodesA;
    private GameObject[] imprintCorruptionNodesB;
    private GameObject healthBarBackgroundA;
    private GameObject healthBarFillA;
    private GameObject healthBarBackgroundB;
    private GameObject healthBarFillB;
    private GameObject roundStatusBannerObject;
    private GameObject fighterAPortraitBillboard;
    private GameObject fighterBPortraitBillboard;
    private bool warnedMissingDogSpriteA;
    private bool warnedMissingDogSpriteB;
    private Dog[] cachedDogPortraitResourceDogs;
    private Material portraitSpriteMaterial;
    private Coroutine scanIntroCoroutine;
    private Coroutine cameraMoveCoroutine;
    private Coroutine roundAnimationCoroutine;

    void Awake()
    {
        EnsureArenaRoot();

        if (arenaRoot != null)
        {
            arenaRoot.SetActive(false);
        }
    }

    void OnDestroy()
    {
        ReleasePresentationRenderTexture();
        ReleasePortraitSpriteMaterial();
    }

    public void ShowPlaceholderArena(Dog dogA, Dog dogB)
    {
        if (dogA == null || dogB == null)
        {
            Debug.LogWarning("FightPresentationManager could not show arena because one or both dogs were missing.");
            return;
        }

        EnsureArenaRoot();

        if (arenaRoot == null)
        {
            Debug.LogWarning("FightPresentationManager could not create ArenaRoot.");
            return;
        }

        ResetVisualHealthTracking();
        CreateArenaObjectsIfNeeded();
        CreateArenaImpactEffectsIfNeeded();
        CreateCorruptionNodesIfNeeded();
        CreateHealthBarsIfNeeded();
        CreateRoundStatusBannerIfNeeded();
        CreateDogPortraitBillboardsIfNeeded();
        HideImpactEffects();
        HideRoundStatusBanner();
        UpdateImprintCorruptionVisuals(0, 0);
        UpdateDogPortraitBillboards(dogA, dogB);
        UpdateArenaLabels(dogA, dogB);
        arenaRoot.SetActive(true);
        FrameArena();

        Debug.Log($"Digital arena ready: {dogA.dogName} imprint vs {dogB.dogName} imprint.");
    }

    public void HideArena()
    {
        EnsureArenaRoot();
        StopRoundAnimationIfRunning();
        HideRoundStatusBanner();
        HideDogPortraitBillboards();

        if (arenaRoot != null)
        {
            arenaRoot.SetActive(false);
        }

        SetPresentationCameraEnabled(false);
        SetFightPresentationViewportVisible(false);
    }

    public void PlayScanIntroThenShowArena(Dog dogA, Dog dogB)
    {
        if (dogA == null || dogB == null)
        {
            Debug.LogWarning("FightPresentationManager could not start DNA scan because one or both dogs were missing.");
            return;
        }

        if (scanIntroCoroutine != null)
        {
            StopCoroutine(scanIntroCoroutine);
            scanIntroCoroutine = null;
        }

        HideExistingArenaRootForScan();
        HideMonitorTransition();
        ResetVisualHealthTracking();
        EnsureScanChamberRoot();

        if (scanChamberRoot == null)
        {
            Debug.LogWarning("FightPresentationManager could not create ScanChamberRoot.");
            return;
        }

        CreateScanChamberObjectsIfNeeded();
        PositionScanSubjects();
        UpdateScanChamberLabels(dogA, dogB);
        scanChamberRoot.SetActive(true);
        SetFightPresentationViewportVisible(true);
        FrameScanChamber();

        Debug.Log($"DNA scan started for {dogA.dogName} and {dogB.dogName}. Real dogs remain safe. Digital imprints are being copied.");

        scanIntroCoroutine = StartCoroutine(ScanIntroRoutine(dogA, dogB));
    }

    IEnumerator ScanIntroRoutine(Dog dogA, Dog dogB)
    {
        // This short pause lets the scan chamber read as an intro before the digital arena appears.
        yield return new WaitForSeconds(ScanIntroDelaySeconds);

        HideScanChamber();

        ShowMonitorTransition();
        Debug.Log($"Digital imprints for {dogA.dogName} and {dogB.dogName} are entering the monitor grid.");

        yield return new WaitForSeconds(MonitorTransitionDelaySeconds);

        HideMonitorTransition();
        scanIntroCoroutine = null;
        ShowPlaceholderArena(dogA, dogB);
    }

    public void PresentRound(int roundNumber, Dog dogA, Dog dogB, int dogAHealth, int dogBHealth)
    {
        if (dogA == null || dogB == null)
        {
            Debug.LogWarning("FightPresentationManager could not present round because one or both dogs were missing.");
            return;
        }

        EnsureArenaRoot();

        if (arenaRoot == null)
        {
            Debug.LogWarning("FightPresentationManager could not present round because ArenaRoot was missing.");
            return;
        }

        CreateArenaObjectsIfNeeded();
        CreateArenaImpactEffectsIfNeeded();
        CreateCorruptionNodesIfNeeded();
        CreateHealthBarsIfNeeded();
        CreateRoundStatusBannerIfNeeded();
        CreateDogPortraitBillboardsIfNeeded();
        arenaRoot.SetActive(true);
        FrameArena();

        float roundStep = Mathf.Clamp(roundNumber, 1, 6) * 0.08f;
        float pulse = roundNumber % 2 == 0 ? 1.12f : 0.95f;

        if (fighterATransform != null)
        {
            fighterATransform.localPosition = new Vector3(-1.75f + roundStep, 0.6f, 0f);
            fighterATransform.localScale = new Vector3(0.62f * pulse, 1.12f * pulse, 0.62f * pulse);
        }

        if (fighterBTransform != null)
        {
            fighterBTransform.localPosition = new Vector3(1.75f - roundStep, 0.6f, 0f);
            fighterBTransform.localScale = new Vector3(0.62f * pulse, 1.12f * pulse, 0.62f * pulse);
        }

        UpdateDogPortraitBillboards(dogA, dogB);
        UpdateArenaLabels(dogA, dogB);
        UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
        UpdateRoundStatusBanner(roundNumber, dogAHealth, dogBHealth, 0, 0, false);

        Debug.Log($"Digital arena round {roundNumber}: {dogA.dogName} HP {dogAHealth} vs {dogB.dogName} HP {dogBHealth}.");
    }

    public void PresentRoundAction(
        int roundNumber,
        Dog dogA,
        Dog dogB,
        int dogAHealth,
        int dogBHealth,
        int dogAImpact,
        int dogBImpact
    )
    {
        if (dogA == null || dogB == null)
        {
            Debug.LogWarning("FightPresentationManager could not present round action because one or both dogs were missing.");
            return;
        }

        PresentRound(roundNumber, dogA, dogB, dogAHealth, dogBHealth);
        StopRoundAnimationIfRunning();
        ResetFighterArenaPositions();
        UpdateDogPortraitBillboards(dogA, dogB);
        UpdateArenaLabels(dogA, dogB);
        UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
        UpdateRoundStatusBanner(roundNumber, dogAHealth, dogBHealth, dogAImpact, dogBImpact, false);
        FrameArena();

        roundAnimationCoroutine = StartCoroutine(AnimateRoundExchange(roundNumber, dogA, dogB, dogAHealth, dogBHealth, dogAImpact, dogBImpact));
    }

    public void PresentFightResult(Dog dogA, Dog dogB, int dogAHealth, int dogBHealth)
    {
        if (dogA == null || dogB == null)
        {
            Debug.LogWarning("FightPresentationManager could not present result because one or both dogs were missing.");
            return;
        }

        EnsureArenaRoot();

        if (arenaRoot == null)
        {
            Debug.LogWarning("FightPresentationManager could not present result because ArenaRoot was missing.");
            return;
        }

        CreateArenaObjectsIfNeeded();
        CreateArenaImpactEffectsIfNeeded();
        CreateCorruptionNodesIfNeeded();
        CreateHealthBarsIfNeeded();
        CreateRoundStatusBannerIfNeeded();
        CreateDogPortraitBillboardsIfNeeded();
        arenaRoot.SetActive(true);
        FrameArena();
        StopRoundAnimationIfRunning();
        UpdateDogPortraitBillboards(dogA, dogB);

        if (dogAHealth > dogBHealth)
        {
            MarkWinner(fighterATransform);
            MarkLoser(fighterBTransform);
            UpdateDogPortraitBillboards(dogA, dogB);
            UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
            UpdateRoundStatusBanner(0, dogAHealth, dogBHealth, 0, 0, true);
            UpdateArenaResultLabels(dogA, dogB, "WINNER", "DEFEATED", new Color(0.1f, 1f, 0.35f), new Color(0.65f, 0.25f, 0.8f));
            Debug.Log($"Digital arena result: {dogA.dogName} imprint wins. {dogB.dogName} imprint falls back.");
            return;
        }

        if (dogBHealth > dogAHealth)
        {
            MarkWinner(fighterBTransform);
            MarkLoser(fighterATransform);
            UpdateDogPortraitBillboards(dogA, dogB);
            UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
            UpdateRoundStatusBanner(0, dogAHealth, dogBHealth, 0, 0, true);
            UpdateArenaResultLabels(dogA, dogB, "DEFEATED", "WINNER", new Color(0.65f, 0.25f, 0.8f), new Color(0.1f, 1f, 0.35f));
            Debug.Log($"Digital arena result: {dogB.dogName} imprint wins. {dogA.dogName} imprint falls back.");
            return;
        }

        MarkDraw(fighterATransform);
        MarkDraw(fighterBTransform);
        UpdateDogPortraitBillboards(dogA, dogB);
        UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
        UpdateRoundStatusBanner(0, dogAHealth, dogBHealth, 0, 0, true);
        UpdateArenaResultLabels(dogA, dogB, "DRAW", "DRAW", new Color(1f, 0.85f, 0.2f), new Color(1f, 0.85f, 0.2f));
        Debug.Log($"Digital arena result: {dogA.dogName} and {dogB.dogName} imprints end in a draw.");
    }

    void EnsureArenaRoot()
    {
        if (arenaRoot != null)
        {
            return;
        }

        arenaRoot = FindExistingArenaRoot();

        if (arenaRoot == null)
        {
            arenaRoot = new GameObject(ArenaRootName);
        }

        sharedArenaRoot = arenaRoot;
        arenaRoot.SetActive(false);
    }

    GameObject FindExistingArenaRoot()
    {
        if (sharedArenaRoot != null)
        {
            return sharedArenaRoot;
        }

        GameObject activeArenaRoot = GameObject.Find(ArenaRootName);

        if (activeArenaRoot != null)
        {
            return activeArenaRoot;
        }

        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject candidate in allGameObjects)
        {
            if (candidate.name == ArenaRootName && candidate.scene.IsValid())
            {
                return candidate;
            }
        }

        return null;
    }

    void EnsureScanChamberRoot()
    {
        if (scanChamberRoot != null)
        {
            return;
        }

        scanChamberRoot = FindExistingScanChamberRoot();

        if (scanChamberRoot == null)
        {
            scanChamberRoot = new GameObject(ScanChamberRootName);
        }

        scanChamberRoot.hideFlags = HideFlags.DontSave;
        sharedScanChamberRoot = scanChamberRoot;
        scanChamberRoot.SetActive(false);
    }

    GameObject FindExistingScanChamberRoot()
    {
        if (sharedScanChamberRoot != null)
        {
            return sharedScanChamberRoot;
        }

        GameObject activeScanChamberRoot = GameObject.Find(ScanChamberRootName);

        if (activeScanChamberRoot != null)
        {
            return activeScanChamberRoot;
        }

        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject candidate in allGameObjects)
        {
            if (candidate.name == ScanChamberRootName && candidate.scene.IsValid())
            {
                return candidate;
            }
        }

        return null;
    }

    void EnsureMonitorTransitionRoot()
    {
        if (monitorTransitionRoot != null)
        {
            return;
        }

        monitorTransitionRoot = FindExistingMonitorTransitionRoot();

        if (monitorTransitionRoot == null)
        {
            monitorTransitionRoot = new GameObject(MonitorTransitionRootName);
        }

        monitorTransitionRoot.hideFlags = HideFlags.DontSave;
        sharedMonitorTransitionRoot = monitorTransitionRoot;
        monitorTransitionRoot.SetActive(false);
    }

    GameObject FindExistingMonitorTransitionRoot()
    {
        if (sharedMonitorTransitionRoot != null)
        {
            return sharedMonitorTransitionRoot;
        }

        GameObject activeMonitorRoot = GameObject.Find(MonitorTransitionRootName);

        if (activeMonitorRoot != null)
        {
            return activeMonitorRoot;
        }

        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject candidate in allGameObjects)
        {
            if (candidate.name == MonitorTransitionRootName && candidate.scene.IsValid())
            {
                return candidate;
            }
        }

        return null;
    }

    void HideExistingArenaRootForScan()
    {
        GameObject existingArenaRoot = FindExistingArenaRoot();

        if (existingArenaRoot == null)
        {
            return;
        }

        arenaRoot = existingArenaRoot;
        arenaRoot.SetActive(false);
    }

    void HideScanChamber()
    {
        EnsureScanChamberRoot();

        if (scanChamberRoot != null)
        {
            scanChamberRoot.SetActive(false);
        }
    }

    void ShowMonitorTransition()
    {
        EnsureMonitorTransitionRoot();

        if (monitorTransitionRoot == null)
        {
            Debug.LogWarning("FightPresentationManager could not create MonitorTransitionRoot.");
            return;
        }

        CreateMonitorTransitionObjectsIfNeeded();
        UpdateMonitorTransitionLabels();
        monitorTransitionRoot.SetActive(true);
        SetFightPresentationViewportVisible(true);
        FrameMonitorTransition();
    }

    void HideMonitorTransition()
    {
        EnsureMonitorTransitionRoot();

        if (monitorTransitionRoot != null)
        {
            monitorTransitionRoot.SetActive(false);
        }
    }

    void EnsurePresentationCamera()
    {
        if (presentationCameraObject == null)
        {
            presentationCameraObject = FindExistingPresentationCameraObject();

            if (presentationCameraObject == null)
            {
                presentationCameraObject = new GameObject(PresentationCameraName);
            }
        }

        presentationCameraObject.hideFlags = HideFlags.DontSave;
        sharedPresentationCameraObject = presentationCameraObject;

        if (presentationCamera == null)
        {
            presentationCamera = presentationCameraObject.GetComponent<Camera>();

            if (presentationCamera == null)
            {
                presentationCamera = presentationCameraObject.AddComponent<Camera>();
            }
        }

        ConfigurePresentationCamera();
        presentationCamera.enabled = false;
    }

    GameObject FindExistingPresentationCameraObject()
    {
        if (sharedPresentationCameraObject != null)
        {
            return sharedPresentationCameraObject;
        }

        GameObject activeCameraObject = GameObject.Find(PresentationCameraName);

        if (activeCameraObject != null)
        {
            return activeCameraObject;
        }

        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject candidate in allGameObjects)
        {
            if (candidate.name == PresentationCameraName && candidate.scene.IsValid())
            {
                return candidate;
            }
        }

        return null;
    }

    void EnsureFightPresentationViewport()
    {
        Transform viewportParent = FindViewportParent();

        if (viewportParent == null)
        {
            return;
        }

        if (fightPresentationViewportObject == null)
        {
            fightPresentationViewportObject = FindExistingViewportObject();
        }

        if (fightPresentationViewportObject == null)
        {
            fightPresentationViewportObject = new GameObject(
                FightPresentationViewportName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage)
            );
        }

        fightPresentationViewportObject.hideFlags = HideFlags.DontSave;
        fightPresentationViewportObject.transform.SetParent(viewportParent, false);

        fightPresentationViewportImage = fightPresentationViewportObject.GetComponent<RawImage>();

        if (fightPresentationViewportImage == null)
        {
            fightPresentationViewportImage = fightPresentationViewportObject.AddComponent<RawImage>();
        }

        ConfigureFightPresentationViewport();
        EnsurePresentationRenderTexture();

        fightPresentationViewportImage.texture = presentationRenderTexture;
        fightPresentationViewportImage.color = Color.white;
        fightPresentationViewportImage.raycastTarget = false;

        if (presentationCamera != null)
        {
            presentationCamera.targetTexture = presentationRenderTexture;
        }
    }

    GameObject FindExistingViewportObject()
    {
        GameObject activeViewport = GameObject.Find(FightPresentationViewportName);

        if (activeViewport != null)
        {
            return activeViewport;
        }

        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject candidate in allGameObjects)
        {
            if (candidate.name == FightPresentationViewportName && candidate.scene.IsValid())
            {
                return candidate;
            }
        }

        return null;
    }

    Transform FindViewportParent()
    {
        GameObject fightPage = GameObject.Find("FightPage");

        if (fightPage != null)
        {
            return fightPage.transform;
        }

        GameObject mainCanvas = GameObject.Find("MainCanvas");

        if (mainCanvas != null)
        {
            return mainCanvas.transform;
        }

        Canvas firstCanvas = FindFirstObjectByType<Canvas>();

        if (firstCanvas != null)
        {
            return firstCanvas.transform;
        }

        return null;
    }

    void ConfigureFightPresentationViewport()
    {
        if (fightPresentationViewportObject == null)
        {
            return;
        }

        RectTransform viewportRect = fightPresentationViewportObject.GetComponent<RectTransform>();

        if (viewportRect == null)
        {
            viewportRect = fightPresentationViewportObject.AddComponent<RectTransform>();
        }

        viewportRect.anchorMin = new Vector2(0.29f, 0.43f);
        viewportRect.anchorMax = new Vector2(0.71f, 0.78f);
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportRect.localScale = Vector3.one;
        viewportRect.localRotation = Quaternion.identity;

        if (fightPresentationViewportObject.transform.parent != null &&
            fightPresentationViewportObject.transform.parent.name == "FightPage")
        {
            fightPresentationViewportObject.transform.SetAsFirstSibling();
            return;
        }

        fightPresentationViewportObject.transform.SetAsLastSibling();
    }

    void EnsurePresentationRenderTexture()
    {
        if (presentationRenderTexture != null &&
            presentationRenderTexture.width == PresentationRenderTextureWidth &&
            presentationRenderTexture.height == PresentationRenderTextureHeight)
        {
            return;
        }

        ReleasePresentationRenderTexture();

        presentationRenderTexture = new RenderTexture(
            PresentationRenderTextureWidth,
            PresentationRenderTextureHeight,
            24,
            RenderTextureFormat.ARGB32
        );
        presentationRenderTexture.name = "FightPresentationRenderTexture";
        presentationRenderTexture.antiAliasing = 2;
        presentationRenderTexture.filterMode = FilterMode.Bilinear;
        presentationRenderTexture.Create();

        if (presentationCamera != null)
        {
            presentationCamera.targetTexture = presentationRenderTexture;
        }

        if (fightPresentationViewportImage != null)
        {
            fightPresentationViewportImage.texture = presentationRenderTexture;
        }
    }

    void SetFightPresentationViewportVisible(bool isVisible)
    {
        if (isVisible)
        {
            EnsureFightPresentationViewport();
        }

        if (fightPresentationViewportObject != null)
        {
            fightPresentationViewportObject.SetActive(isVisible);
        }
    }

    void ReleasePresentationRenderTexture()
    {
        if (presentationCamera != null && presentationCamera.targetTexture == presentationRenderTexture)
        {
            presentationCamera.targetTexture = null;
        }

        if (fightPresentationViewportImage != null && fightPresentationViewportImage.texture == presentationRenderTexture)
        {
            fightPresentationViewportImage.texture = null;
        }

        if (presentationRenderTexture == null)
        {
            return;
        }

        presentationRenderTexture.Release();

        if (Application.isPlaying)
        {
            Destroy(presentationRenderTexture);
        }
        else
        {
            DestroyImmediate(presentationRenderTexture);
        }

        presentationRenderTexture = null;
    }

    void ReleasePortraitSpriteMaterial()
    {
        if (portraitSpriteMaterial == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(portraitSpriteMaterial);
        }
        else
        {
            DestroyImmediate(portraitSpriteMaterial);
        }

        portraitSpriteMaterial = null;
    }

    void ConfigurePresentationCamera()
    {
        if (presentationCamera == null)
        {
            return;
        }

        presentationCamera.clearFlags = CameraClearFlags.SolidColor;
        presentationCamera.backgroundColor = new Color(0.01f, 0.015f, 0.025f);
        presentationCamera.fieldOfView = 50f;
        presentationCamera.nearClipPlane = 0.1f;
        presentationCamera.farClipPlane = 100f;
        presentationCamera.depth = 5f;
        presentationCamera.rect = new Rect(0f, 0f, 1f, 1f);
        EnsurePresentationRenderTexture();
        presentationCamera.targetTexture = presentationRenderTexture;
    }

    void FrameScanChamber()
    {
        SetFightPresentationViewportVisible(true);
        SetPresentationCameraInstant(new Vector3(0f, 3.5f, -7f), new Vector3(0f, 1.2f, 0f));
    }

    void FrameMonitorTransition()
    {
        SetFightPresentationViewportVisible(true);
        MovePresentationCameraTo(new Vector3(0f, 3f, -6f), new Vector3(0f, 1.2f, 0f), CameraMoveDurationSeconds);
    }

    void FrameArena()
    {
        SetFightPresentationViewportVisible(true);
        MovePresentationCameraTo(new Vector3(0f, 4.2f, -7.2f), new Vector3(0f, 0.75f, 0.25f), CameraMoveDurationSeconds);
    }

    void MovePresentationCameraTo(Vector3 targetPosition, Vector3 lookAtPosition, float duration)
    {
        EnsurePresentationCamera();

        if (presentationCamera == null)
        {
            return;
        }

        if (duration <= 0f || Vector3.Distance(presentationCameraObject.transform.position, targetPosition) < 0.01f)
        {
            SetPresentationCameraInstant(targetPosition, lookAtPosition);
            return;
        }

        StopCameraMoveIfRunning();
        presentationCamera.enabled = true;
        cameraMoveCoroutine = StartCoroutine(MovePresentationCameraRoutine(targetPosition, lookAtPosition, duration));
    }

    IEnumerator MovePresentationCameraRoutine(Vector3 targetPosition, Vector3 lookAtPosition, float duration)
    {
        Vector3 startPosition = presentationCameraObject.transform.position;
        float elapsedTime = 0f;

        // SmoothStep gives the move a tiny ease-in/ease-out without needing an animation controller.
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float smoothProgress = progress * progress * (3f - (2f * progress));

            presentationCameraObject.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);
            presentationCameraObject.transform.LookAt(lookAtPosition);
            FacePortraitsTowardPresentationCamera();
            yield return null;
        }

        cameraMoveCoroutine = null;
        SetPresentationCameraInstant(targetPosition, lookAtPosition);
    }

    void StopCameraMoveIfRunning()
    {
        if (cameraMoveCoroutine == null)
        {
            return;
        }

        StopCoroutine(cameraMoveCoroutine);
        cameraMoveCoroutine = null;
    }

    void SetPresentationCameraInstant(Vector3 cameraPosition, Vector3 lookAtPosition)
    {
        EnsurePresentationCamera();

        if (presentationCamera == null)
        {
            return;
        }

        StopCameraMoveIfRunning();
        presentationCameraObject.transform.position = cameraPosition;
        presentationCameraObject.transform.LookAt(lookAtPosition);
        presentationCamera.enabled = true;
        FacePortraitsTowardPresentationCamera();
    }

    void SetPresentationCameraEnabled(bool isEnabled)
    {
        if (presentationCamera == null && !isEnabled)
        {
            return;
        }

        if (!isEnabled)
        {
            StopCameraMoveIfRunning();
        }

        EnsurePresentationCamera();

        if (presentationCamera != null)
        {
            presentationCamera.enabled = isEnabled;
        }
    }

    void CreateArenaObjectsIfNeeded()
    {
        if (arenaObjectsCreated)
        {
            return;
        }

        CreateArenaSurfaceVisuals();
        AssignExistingArenaTransforms();

        if (fighterATransform == null)
        {
            fighterATransform = CreateFighterPlaceholder("FighterA_Imprint", new Vector3(-1.75f, 0.6f, 0f), Color.cyan).transform;
        }

        if (fighterBTransform == null)
        {
            fighterBTransform = CreateFighterPlaceholder("FighterB_Imprint", new Vector3(1.75f, 0.6f, 0f), Color.magenta).transform;
        }

        CreateMarker("CenterMarker", new Vector3(0f, 0.08f, 0f), new Color(0.75f, 1f, 1f));

        arenaObjectsCreated = true;
    }

    void CreateScanChamberObjectsIfNeeded()
    {
        if (scanChamberObjectsCreated)
        {
            return;
        }

        if (scanChamberRoot.transform.childCount > 0)
        {
            AssignExistingScanTransforms();
            scanChamberObjectsCreated = true;
            return;
        }

        CreateScanChamberBase();
        scanDogATransform = CreateSafeDogPlaceholder("SafeChamberDogA", new Vector3(-1.5f, 0.6f, 0f), Color.cyan).transform;
        scanDogBTransform = CreateSafeDogPlaceholder("SafeChamberDogB", new Vector3(1.5f, 0.6f, 0f), Color.magenta).transform;
        CreateScanBeam("ScanBeamA", new Vector3(-1.5f, 1.45f, 0f), new Color(0.1f, 0.8f, 1f));
        CreateScanBeam("ScanBeamB", new Vector3(1.5f, 1.45f, 0f), new Color(1f, 0.2f, 0.9f));
        CreateCopyCore();

        scanChamberObjectsCreated = true;
    }

    void CreateMonitorTransitionObjectsIfNeeded()
    {
        if (monitorTransitionObjectsCreated)
        {
            return;
        }

        if (monitorTransitionRoot.transform.childCount > 0)
        {
            monitorTransitionObjectsCreated = true;
            return;
        }

        CreateMonitorScreen();
        CreateMonitorFrame();
        CreateMonitorGridMarkers();
        CreateImprintStream();

        monitorTransitionObjectsCreated = true;
    }

    void AssignExistingArenaTransforms()
    {
        Transform fighterA = arenaRoot.transform.Find("FighterA_Imprint");
        Transform fighterB = arenaRoot.transform.Find("FighterB_Imprint");

        if (fighterA != null)
        {
            fighterATransform = fighterA;
        }

        if (fighterB != null)
        {
            fighterBTransform = fighterB;
        }
    }

    void AssignExistingScanTransforms()
    {
        Transform dogA = scanChamberRoot.transform.Find("SafeChamberDogA");
        Transform dogB = scanChamberRoot.transform.Find("SafeChamberDogB");

        if (dogA != null)
        {
            scanDogATransform = dogA;
        }

        if (dogB != null)
        {
            scanDogBTransform = dogB;
        }
    }

    void PositionScanSubjects()
    {
        if (scanDogATransform != null)
        {
            scanDogATransform.localPosition = new Vector3(-1.5f, 0.6f, 0f);
            scanDogATransform.localScale = new Vector3(0.65f, 1.05f, 0.65f);
        }

        if (scanDogBTransform != null)
        {
            scanDogBTransform.localPosition = new Vector3(1.5f, 0.6f, 0f);
            scanDogBTransform.localScale = new Vector3(0.65f, 1.05f, 0.65f);
        }
    }

    void UpdateScanChamberLabels(Dog dogA, Dog dogB)
    {
        if (scanChamberRoot == null)
        {
            return;
        }

        CreateOrUpdateLabel(scanChamberRoot, "ScanChamberTitleLabel", "DNA SCAN", new Vector3(0f, 3f, 0f), Color.white, 0.17f);
        CreateOrUpdateLabel(scanChamberRoot, "ScanChamberSafetyLabel", "REAL DOGS SAFE\nCOPYING IMPRINTS", new Vector3(0f, 2.62f, 0f), new Color(0.45f, 1f, 0.75f), 0.095f);
        CreateOrUpdateLabel(scanChamberRoot, "SafeDogALabel", GetDogDisplayName(dogA, "DOG A"), GetLabelPosition(scanDogATransform, new Vector3(-1.5f, 1.95f, 0f)), Color.cyan, 0.115f);
        CreateOrUpdateLabel(scanChamberRoot, "SafeDogBLabel", GetDogDisplayName(dogB, "DOG B"), GetLabelPosition(scanDogBTransform, new Vector3(1.5f, 1.95f, 0f)), Color.magenta, 0.115f);
    }

    void UpdateMonitorTransitionLabels()
    {
        if (monitorTransitionRoot == null)
        {
            return;
        }

        CreateOrUpdateLabel(monitorTransitionRoot, "MonitorTransitionTitleLabel", "IMPRINT TRANSFER", new Vector3(0f, 3.1f, 0f), Color.white, 0.16f);
        CreateOrUpdateLabel(monitorTransitionRoot, "MonitorTransitionStatusLabel", "ENTERING GRID", new Vector3(0f, 2.78f, 0f), new Color(0.45f, 1f, 0.75f), 0.105f);
    }

    void UpdateArenaLabels(Dog dogA, Dog dogB)
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreateOrUpdateLabel(arenaRoot, "ArenaTitleLabel", "DIGITAL ARENA", new Vector3(0f, 3.12f, 0.2f), Color.white, 0.145f);
        CreateOrUpdateLabel(arenaRoot, "FighterALabel", $"{GetDogDisplayName(dogA, "DOG A")}\nIMPRINT", GetLabelPosition(fighterATransform, new Vector3(-1.75f, 2.25f, 0f)), Color.cyan, 0.09f);
        CreateOrUpdateLabel(arenaRoot, "FighterBLabel", $"{GetDogDisplayName(dogB, "DOG B")}\nIMPRINT", GetLabelPosition(fighterBTransform, new Vector3(1.75f, 2.25f, 0f)), Color.magenta, 0.09f);
    }

    void UpdateArenaResultLabels(Dog dogA, Dog dogB, string dogAStatus, string dogBStatus, Color dogAColor, Color dogBColor)
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreateOrUpdateLabel(arenaRoot, "ArenaTitleLabel", "DIGITAL ARENA", new Vector3(0f, 3.12f, 0.2f), Color.white, 0.145f);
        CreateOrUpdateLabel(arenaRoot, "FighterALabel", $"{GetDogDisplayName(dogA, "DOG A")}\n{dogAStatus}", GetLabelPosition(fighterATransform, new Vector3(-1.75f, 2.25f, 0f)), dogAColor, 0.095f);
        CreateOrUpdateLabel(arenaRoot, "FighterBLabel", $"{GetDogDisplayName(dogB, "DOG B")}\n{dogBStatus}", GetLabelPosition(fighterBTransform, new Vector3(1.75f, 2.25f, 0f)), dogBColor, 0.095f);
    }

    void CreateDogPortraitBillboardsIfNeeded()
    {
        if (arenaRoot == null)
        {
            return;
        }

        if (fighterAPortraitBillboard == null)
        {
            fighterAPortraitBillboard = CreateDogPortraitBillboard("FighterA_PortraitBillboard", Color.cyan);
        }

        if (fighterBPortraitBillboard == null)
        {
            fighterBPortraitBillboard = CreateDogPortraitBillboard("FighterB_PortraitBillboard", Color.magenta);
        }
    }

    GameObject CreateDogPortraitBillboard(string objectName, Color accentColor)
    {
        Transform existingBillboard = arenaRoot.transform.Find(objectName);
        GameObject billboardObject;

        if (existingBillboard != null)
        {
            billboardObject = existingBillboard.gameObject;
        }
        else
        {
            billboardObject = new GameObject(objectName);
            billboardObject.transform.SetParent(arenaRoot.transform);
        }

        billboardObject.hideFlags = HideFlags.DontSave;
        billboardObject.transform.localRotation = Quaternion.identity;
        billboardObject.transform.localScale = Vector3.one;

        SpriteRenderer oldRootRenderer = billboardObject.GetComponent<SpriteRenderer>();

        if (oldRootRenderer != null)
        {
            oldRootRenderer.enabled = false;
        }

        CreatePortraitCardFrameIfNeeded(billboardObject.transform, accentColor);
        ConfigurePortraitSpriteRenderer(GetPortraitSpriteRenderer(billboardObject));

        billboardObject.SetActive(false);
        return billboardObject;
    }

    void UpdateDogPortraitBillboards(Dog dogA, Dog dogB)
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreateDogPortraitBillboardsIfNeeded();
        ConfigurePortraitBillboard(fighterAPortraitBillboard, dogA, fighterATransform, new Vector3(-0.62f, 1.05f, -0.55f), ref warnedMissingDogSpriteA);
        ConfigurePortraitBillboard(fighterBPortraitBillboard, dogB, fighterBTransform, new Vector3(0.62f, 1.05f, -0.55f), ref warnedMissingDogSpriteB);
        FacePortraitsTowardPresentationCamera();
    }

    void ConfigurePortraitBillboard(GameObject billboardObject, Dog dog, Transform fighterTransform, Vector3 localOffset, ref bool warnedMissingSprite)
    {
        if (billboardObject == null || dog == null || fighterTransform == null)
        {
            SetPortraitBillboardActive(billboardObject, false);
            return;
        }

        SpriteRenderer spriteRenderer = GetPortraitSpriteRenderer(billboardObject);

        if (spriteRenderer == null)
        {
            SetPortraitBillboardActive(billboardObject, false);
            return;
        }

        Sprite portraitSprite = ResolveDogPortraitSprite(dog);

        if (portraitSprite == null)
        {
            spriteRenderer.sprite = null;
            SetPortraitBillboardActive(billboardObject, false);

            if (!warnedMissingSprite)
            {
                Debug.Log($"FightPresentationManager found no dogSprite for {GetDogDisplayName(dog, "dog")}; hiding portrait billboard.");
                warnedMissingSprite = true;
            }

            return;
        }

        warnedMissingSprite = false;
        spriteRenderer.sprite = portraitSprite;
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 500;
        ConfigurePortraitSpriteRenderer(spriteRenderer);

        billboardObject.transform.localPosition = fighterTransform.localPosition + localOffset;
        billboardObject.transform.localScale = Vector3.one;
        spriteRenderer.transform.localScale = GetPortraitSpriteScale(portraitSprite);
        SetPortraitBillboardActive(billboardObject, true);
    }

    void CreatePortraitCardFrameIfNeeded(Transform billboardTransform, Color accentColor)
    {
        if (billboardTransform == null)
        {
            return;
        }

        CreatePortraitCardPart(billboardTransform, "PortraitCardBack", new Vector3(0f, 0f, 0.045f), new Vector3(1.35f, 1.05f, 0.035f), new Color(0.015f, 0.02f, 0.03f));
        CreatePortraitCardPart(billboardTransform, "PortraitCardTop", new Vector3(0f, 0.55f, -0.015f), new Vector3(1.42f, 0.055f, 0.045f), accentColor);
        CreatePortraitCardPart(billboardTransform, "PortraitCardBottom", new Vector3(0f, -0.55f, -0.015f), new Vector3(1.42f, 0.055f, 0.045f), accentColor);
        CreatePortraitCardPart(billboardTransform, "PortraitCardLeft", new Vector3(-0.71f, 0f, -0.015f), new Vector3(0.055f, 1.08f, 0.045f), accentColor);
        CreatePortraitCardPart(billboardTransform, "PortraitCardRight", new Vector3(0.71f, 0f, -0.015f), new Vector3(0.055f, 1.08f, 0.045f), accentColor);
    }

    void CreatePortraitCardPart(Transform parentTransform, string objectName, Vector3 localPosition, Vector3 localScale, Color color)
    {
        Transform existingPart = parentTransform.Find(objectName);
        GameObject partObject;

        if (existingPart != null)
        {
            partObject = existingPart.gameObject;
        }
        else
        {
            partObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            partObject.name = objectName;
            partObject.transform.SetParent(parentTransform);
        }

        partObject.hideFlags = HideFlags.DontSave;
        partObject.transform.localPosition = localPosition;
        partObject.transform.localRotation = Quaternion.identity;
        partObject.transform.localScale = localScale;
        SetObjectUnlitColor(partObject, color);
    }

    SpriteRenderer GetPortraitSpriteRenderer(GameObject billboardObject)
    {
        if (billboardObject == null)
        {
            return null;
        }

        Transform existingSpriteTransform = billboardObject.transform.Find("PortraitSprite");
        GameObject spriteObject;

        if (existingSpriteTransform != null)
        {
            spriteObject = existingSpriteTransform.gameObject;
        }
        else
        {
            spriteObject = new GameObject("PortraitSprite");
            spriteObject.transform.SetParent(billboardObject.transform);
        }

        spriteObject.hideFlags = HideFlags.DontSave;
        spriteObject.transform.localPosition = new Vector3(0f, 0f, -0.08f);
        spriteObject.transform.localRotation = Quaternion.identity;

        SpriteRenderer spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
        }

        return spriteRenderer;
    }

    void ConfigurePortraitSpriteRenderer(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.enabled = true;
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 500;
        spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        spriteRenderer.receiveShadows = false;

        Material runtimeMaterial = GetPortraitSpriteMaterial();

        if (runtimeMaterial != null)
        {
            spriteRenderer.material = runtimeMaterial;
        }
    }

    Material GetPortraitSpriteMaterial()
    {
        if (portraitSpriteMaterial != null)
        {
            return portraitSpriteMaterial;
        }

        Shader spriteShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");

        if (spriteShader == null)
        {
            spriteShader = Shader.Find("Sprites/Default");
        }

        if (spriteShader == null)
        {
            spriteShader = Shader.Find("Unlit/Transparent");
        }

        if (spriteShader == null)
        {
            return null;
        }

        portraitSpriteMaterial = new Material(spriteShader);
        portraitSpriteMaterial.name = "RuntimePortraitBillboardMaterial";
        portraitSpriteMaterial.hideFlags = HideFlags.DontSave;
        portraitSpriteMaterial.renderQueue = 3000;
        return portraitSpriteMaterial;
    }

    Sprite ResolveDogPortraitSprite(Dog dog)
    {
        if (dog == null)
        {
            return null;
        }

        if (dog.dogSprite != null)
        {
            return dog.dogSprite;
        }

        return FindResourceDogSprite(dog);
    }

    Sprite FindResourceDogSprite(Dog dog)
    {
        Dog[] resourceDogs = GetCachedDogPortraitResourceDogs();

        if (resourceDogs == null || resourceDogs.Length == 0 || dog == null)
        {
            return null;
        }

        string dogIdKey = NormalizeDogPortraitKey(dog.dogId);
        string dogNameKey = NormalizeDogPortraitKey(dog.dogName);

        foreach (Dog resourceDog in resourceDogs)
        {
            if (resourceDog == null || resourceDog.dogSprite == null)
            {
                continue;
            }

            string resourceIdKey = NormalizeDogPortraitKey(resourceDog.dogId);
            string resourceNameKey = NormalizeDogPortraitKey(resourceDog.dogName);

            if ((!string.IsNullOrEmpty(dogIdKey) && dogIdKey == resourceIdKey) ||
                (!string.IsNullOrEmpty(dogNameKey) && dogNameKey == resourceNameKey))
            {
                return resourceDog.dogSprite;
            }
        }

        return null;
    }

    Dog[] GetCachedDogPortraitResourceDogs()
    {
        if (cachedDogPortraitResourceDogs == null)
        {
            cachedDogPortraitResourceDogs = Resources.LoadAll<Dog>(string.Empty);
        }

        return cachedDogPortraitResourceDogs;
    }

    string NormalizeDogPortraitKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Trim()
            .ToLowerInvariant()
            .Replace("(clone)", string.Empty)
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty);
    }

    Vector3 GetPortraitSpriteScale(Sprite sprite)
    {
        if (sprite == null || sprite.bounds.size.y <= 0f)
        {
            return new Vector3(0.8f, 0.8f, 0.8f);
        }

        float targetHeight = 0.86f;
        float scale = targetHeight / sprite.bounds.size.y;
        float scaledWidth = sprite.bounds.size.x * scale;

        if (scaledWidth > 1.2f && scaledWidth > 0f)
        {
            scale *= 1.2f / scaledWidth;
        }

        return new Vector3(scale, scale, scale);
    }

    void FacePortraitsTowardPresentationCamera()
    {
        if (presentationCamera == null)
        {
            return;
        }

        FacePortraitTowardPresentationCamera(fighterAPortraitBillboard);
        FacePortraitTowardPresentationCamera(fighterBPortraitBillboard);
    }

    void FacePortraitTowardPresentationCamera(GameObject billboardObject)
    {
        if (billboardObject == null || !billboardObject.activeSelf || presentationCamera == null)
        {
            return;
        }

        billboardObject.transform.rotation = presentationCamera.transform.rotation;
    }

    void HideDogPortraitBillboards()
    {
        SetPortraitBillboardActive(fighterAPortraitBillboard, false);
        SetPortraitBillboardActive(fighterBPortraitBillboard, false);
    }

    void SetPortraitBillboardActive(GameObject billboardObject, bool isActive)
    {
        if (billboardObject != null)
        {
            billboardObject.SetActive(isActive);
        }
    }

    void CreateRoundStatusBannerIfNeeded()
    {
        if (roundStatusBannerObject != null)
        {
            return;
        }

        if (arenaRoot == null)
        {
            return;
        }

        TextMesh banner = CreateOrUpdateLabel(arenaRoot, "RoundStatusBanner", "", new Vector3(0f, 2.28f, 1.55f), Color.white, 0.1f);

        if (banner == null)
        {
            return;
        }

        roundStatusBannerObject = banner.gameObject;
        roundStatusBannerObject.SetActive(false);
    }

    void UpdateRoundStatusBanner(int roundNumber, int dogAHealth, int dogBHealth, int dogAImpact, int dogBImpact, bool isResult)
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreateRoundStatusBannerIfNeeded();

        if (roundStatusBannerObject == null)
        {
            return;
        }

        string message = GetRoundStatusMessage(roundNumber, dogAHealth, dogBHealth, dogAImpact, dogBImpact, isResult);

        TextMesh banner = roundStatusBannerObject.GetComponent<TextMesh>();

        if (banner == null)
        {
            banner = roundStatusBannerObject.AddComponent<TextMesh>();
        }

        SetLabelText(banner, message);
        banner.color = GetRoundStatusColor(message);
        banner.fontSize = 72;
        banner.characterSize = 0.1f;
        banner.alignment = TextAlignment.Center;
        banner.anchor = TextAnchor.MiddleCenter;

        roundStatusBannerObject.transform.localPosition = new Vector3(0f, 2.28f, 1.55f);
        roundStatusBannerObject.transform.localRotation = Quaternion.identity;
        roundStatusBannerObject.transform.localScale = Vector3.one;
        roundStatusBannerObject.SetActive(true);
    }

    string GetRoundStatusMessage(int roundNumber, int dogAHealth, int dogBHealth, int dogAImpact, int dogBImpact, bool isResult)
    {
        if (isResult)
        {
            return "FINAL";
        }

        float dogAHealthPercent = GetHealthPercent(dogAHealth, visualMaxHealthA);
        float dogBHealthPercent = GetHealthPercent(dogBHealth, visualMaxHealthB);
        bool anyImprintCritical = dogAHealthPercent <= 0.2f || dogBHealthPercent <= 0.2f;
        bool anyImprintDamaged = dogAHealthPercent <= 0.45f || dogBHealthPercent <= 0.45f;
        int highestImpact = Mathf.Max(dogAImpact, dogBImpact);
        int impactDifference = Mathf.Abs(dogAImpact - dogBImpact);
        bool hasImpact = dogAImpact > 0 || dogBImpact > 0;
        const int evenExchangeDifference = 3;
        const int heavyImpactValue = 15;
        const int heavyImpactDifference = 8;
        const int corruptionSpikeImpactValue = 18;

        if (anyImprintCritical)
        {
            return "CRITICAL";
        }

        if (!hasImpact)
        {
            if (roundNumber <= 1)
            {
                return "ROUND 1";
            }

            return $"ROUND {roundNumber}";
        }

        if (highestImpact >= corruptionSpikeImpactValue && anyImprintDamaged)
        {
            return "GLITCH";
        }

        if (impactDifference <= evenExchangeDifference)
        {
            return "EVEN TRADE";
        }

        if (highestImpact >= heavyImpactValue || impactDifference >= heavyImpactDifference)
        {
            return "HEAVY HIT";
        }

        return $"ROUND {roundNumber}";
    }

    Color GetRoundStatusColor(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return new Color(0.65f, 0.9f, 1f);
        }

        if (message.StartsWith("FINAL"))
        {
            return Color.white;
        }

        if (message.StartsWith("CRITICAL"))
        {
            return new Color(1f, 0.05f, 0.1f);
        }

        if (message.StartsWith("GLITCH"))
        {
            return new Color(0.9f, 0.1f, 1f);
        }

        if (message.StartsWith("HEAVY HIT"))
        {
            return new Color(1f, 0.45f, 0.05f);
        }

        if (message.StartsWith("EVEN TRADE"))
        {
            return new Color(0.45f, 1f, 0.75f);
        }

        return new Color(0.65f, 0.9f, 1f);
    }

    void HideRoundStatusBanner()
    {
        if (roundStatusBannerObject != null)
        {
            roundStatusBannerObject.SetActive(false);
        }
    }

    TextMesh CreateOrUpdateLabel(GameObject rootObject, string objectName, string text, Vector3 localPosition, Color color, float characterSize)
    {
        if (rootObject == null)
        {
            return null;
        }

        Transform labelTransform = rootObject.transform.Find(objectName);
        GameObject labelObject;

        if (labelTransform == null)
        {
            labelObject = new GameObject(objectName);
            labelObject.transform.SetParent(rootObject.transform);
        }
        else
        {
            labelObject = labelTransform.gameObject;
        }

        labelObject.hideFlags = HideFlags.DontSave;
        labelObject.transform.localPosition = localPosition;
        labelObject.transform.localRotation = Quaternion.identity;
        labelObject.transform.localScale = Vector3.one;

        TextMesh label = labelObject.GetComponent<TextMesh>();

        if (label == null)
        {
            label = labelObject.AddComponent<TextMesh>();
        }

        SetLabelText(label, text);
        label.color = color;
        label.fontSize = 80;
        label.characterSize = characterSize;
        label.alignment = TextAlignment.Center;
        label.anchor = TextAnchor.MiddleCenter;

        MeshRenderer labelRenderer = labelObject.GetComponent<MeshRenderer>();

        if (labelRenderer != null)
        {
            labelRenderer.sortingOrder = 10;
        }

        return label;
    }

    void SetLabelText(TextMesh label, string text)
    {
        if (label == null)
        {
            return;
        }

        label.text = text;
    }

    Vector3 GetLabelPosition(Transform targetTransform, Vector3 fallbackPosition)
    {
        if (targetTransform == null)
        {
            return fallbackPosition;
        }

        return targetTransform.localPosition + new Vector3(0f, 1.45f, 0f);
    }

    string GetDogDisplayName(Dog dog, string fallbackName)
    {
        if (dog == null || string.IsNullOrEmpty(dog.dogName))
        {
            return fallbackName;
        }

        return dog.dogName;
    }

    void ResetVisualHealthTracking()
    {
        visualMaxHealthA = 0;
        visualMaxHealthB = 0;
    }

    void UpdateImprintCorruptionVisuals(int dogAHealth, int dogBHealth)
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreateCorruptionNodesIfNeeded();
        visualMaxHealthA = Mathf.Max(visualMaxHealthA, Mathf.Max(1, dogAHealth));
        visualMaxHealthB = Mathf.Max(visualMaxHealthB, Mathf.Max(1, dogBHealth));

        ApplyImprintCorruption(fighterATransform, dogAHealth, visualMaxHealthA, imprintCorruptionNodesA, Color.cyan);
        ApplyImprintCorruption(fighterBTransform, dogBHealth, visualMaxHealthB, imprintCorruptionNodesB, Color.magenta);
        UpdateHealthBars(dogAHealth, dogBHealth);
    }

    void ApplyImprintCorruption(Transform fighterTransform, int currentHealth, int maxLikelyHealth, GameObject[] corruptionNodes, Color cleanColor)
    {
        if (fighterTransform == null)
        {
            UpdateCorruptionNodes(corruptionNodes, null, 0f);
            return;
        }

        int safeMaxHealth = Mathf.Max(1, maxLikelyHealth);
        float healthPercent = Mathf.Clamp01((float)Mathf.Max(0, currentHealth) / safeMaxHealth);
        float corruptionStrength = 1f - healthPercent;

        SetObjectUnlitColor(fighterTransform.gameObject, GetCorruptionColor(cleanColor, corruptionStrength));
        fighterTransform.localScale = GetCorruptionScale(corruptionStrength);
        UpdateCorruptionNodes(corruptionNodes, fighterTransform, corruptionStrength);
    }

    void CreateCorruptionNodesIfNeeded()
    {
        if (imprintCorruptionNodesCreated)
        {
            return;
        }

        if (arenaRoot == null)
        {
            return;
        }

        imprintCorruptionNodesA = new GameObject[]
        {
            CreateImprintCorruptionNode("ImprintCorruptionA_1", PrimitiveType.Cube),
            CreateImprintCorruptionNode("ImprintCorruptionA_2", PrimitiveType.Sphere),
            CreateImprintCorruptionNode("ImprintCorruptionA_3", PrimitiveType.Cube),
            CreateImprintCorruptionNode("ImprintCorruptionA_4", PrimitiveType.Sphere),
            CreateImprintCorruptionNode("ImprintCorruptionA_5", PrimitiveType.Cube)
        };

        imprintCorruptionNodesB = new GameObject[]
        {
            CreateImprintCorruptionNode("ImprintCorruptionB_1", PrimitiveType.Cube),
            CreateImprintCorruptionNode("ImprintCorruptionB_2", PrimitiveType.Sphere),
            CreateImprintCorruptionNode("ImprintCorruptionB_3", PrimitiveType.Cube),
            CreateImprintCorruptionNode("ImprintCorruptionB_4", PrimitiveType.Sphere),
            CreateImprintCorruptionNode("ImprintCorruptionB_5", PrimitiveType.Cube)
        };

        imprintCorruptionNodesCreated = true;
        UpdateCorruptionNodes(imprintCorruptionNodesA, null, 0f);
        UpdateCorruptionNodes(imprintCorruptionNodesB, null, 0f);
    }

    GameObject CreateImprintCorruptionNode(string objectName, PrimitiveType primitiveType)
    {
        Transform existingNode = arenaRoot.transform.Find(objectName);
        GameObject nodeObject;

        if (existingNode != null)
        {
            nodeObject = existingNode.gameObject;
        }
        else
        {
            nodeObject = GameObject.CreatePrimitive(primitiveType);
            nodeObject.name = objectName;
            nodeObject.transform.SetParent(arenaRoot.transform);
        }

        nodeObject.hideFlags = HideFlags.DontSave;
        nodeObject.transform.localPosition = Vector3.zero;
        nodeObject.transform.localRotation = Quaternion.identity;
        nodeObject.transform.localScale = Vector3.zero;
        SetObjectUnlitColor(nodeObject, new Color(0.8f, 0.1f, 1f));
        nodeObject.SetActive(false);

        return nodeObject;
    }

    void UpdateCorruptionNodes(GameObject[] corruptionNodes, Transform fighterTransform, float corruptionStrength)
    {
        if (corruptionNodes == null)
        {
            return;
        }

        bool shouldShowNodes = fighterTransform != null && corruptionStrength > 0.1f;
        Vector3[] offsets = new Vector3[]
        {
            new Vector3(-0.42f, 0.75f, 0.05f),
            new Vector3(0.32f, 1.05f, -0.08f),
            new Vector3(0.2f, 0.42f, 0.12f),
            new Vector3(-0.12f, 1.28f, 0.14f),
            new Vector3(0.46f, 0.62f, -0.16f)
        };

        for (int i = 0; i < corruptionNodes.Length; i++)
        {
            GameObject node = corruptionNodes[i];

            if (node == null)
            {
                continue;
            }

            node.SetActive(shouldShowNodes);

            if (!shouldShowNodes)
            {
                continue;
            }

            float scale = Mathf.Lerp(0.06f, 0.46f, corruptionStrength);
            node.transform.localPosition = fighterTransform.localPosition + offsets[i % offsets.Length] * Mathf.Lerp(0.8f, 1.35f, corruptionStrength);
            node.transform.localRotation = Quaternion.Euler(0f, 45f + (corruptionStrength * 90f), 0f);
            node.transform.localScale = new Vector3(scale, scale, scale);
            SetObjectUnlitColor(node, GetCorruptionColor(new Color(0.75f, 0.1f, 1f), corruptionStrength));
        }
    }

    Color GetCorruptionColor(Color cleanColor, float corruptionStrength)
    {
        Color damagedColor = Color.Lerp(new Color(0.75f, 0.1f, 1f), new Color(0.18f, 0.03f, 0.28f), corruptionStrength);
        return Color.Lerp(cleanColor, damagedColor, Mathf.Clamp01(corruptionStrength));
    }

    Vector3 GetCorruptionScale(float corruptionStrength)
    {
        float horizontalScale = Mathf.Lerp(0.62f, 0.86f, corruptionStrength);
        float verticalScale = Mathf.Lerp(1.12f, 0.9f, corruptionStrength);
        float depthScale = Mathf.Lerp(0.62f, 0.5f, corruptionStrength);

        return new Vector3(horizontalScale, verticalScale, depthScale);
    }

    void CreateHealthBarsIfNeeded()
    {
        if (healthBarsCreated)
        {
            return;
        }

        if (arenaRoot == null)
        {
            return;
        }

        healthBarBackgroundA = CreateHealthBarPart("HealthBarBackgroundA", new Color(0.08f, 0.08f, 0.08f));
        healthBarFillA = CreateHealthBarPart("HealthBarFillA", Color.cyan);
        healthBarBackgroundB = CreateHealthBarPart("HealthBarBackgroundB", new Color(0.08f, 0.08f, 0.08f));
        healthBarFillB = CreateHealthBarPart("HealthBarFillB", Color.magenta);

        healthBarsCreated = true;
        SetHealthBarActive(healthBarBackgroundA, false);
        SetHealthBarActive(healthBarFillA, false);
        SetHealthBarActive(healthBarBackgroundB, false);
        SetHealthBarActive(healthBarFillB, false);
    }

    GameObject CreateHealthBarPart(string objectName, Color color)
    {
        Transform existingPart = arenaRoot.transform.Find(objectName);
        GameObject barPart;

        if (existingPart != null)
        {
            barPart = existingPart.gameObject;
        }
        else
        {
            barPart = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barPart.name = objectName;
            barPart.transform.SetParent(arenaRoot.transform);
        }

        barPart.hideFlags = HideFlags.DontSave;
        barPart.transform.localPosition = Vector3.zero;
        barPart.transform.localRotation = Quaternion.identity;
        barPart.transform.localScale = new Vector3(1.2f, 0.08f, 0.08f);
        SetObjectUnlitColor(barPart, color);

        return barPart;
    }

    void UpdateHealthBars(int dogAHealth, int dogBHealth)
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreateHealthBarsIfNeeded();
        UpdateSingleHealthBar(healthBarBackgroundA, healthBarFillA, fighterATransform, dogAHealth, visualMaxHealthA, -1);
        UpdateSingleHealthBar(healthBarBackgroundB, healthBarFillB, fighterBTransform, dogBHealth, visualMaxHealthB, 1);
    }

    void UpdateSingleHealthBar(GameObject backgroundBar, GameObject fillBar, Transform fighterTransform, int currentHealth, int maxLikelyHealth, int fillDirection)
    {
        if (backgroundBar == null || fillBar == null || fighterTransform == null)
        {
            SetHealthBarActive(backgroundBar, false);
            SetHealthBarActive(fillBar, false);
            return;
        }

        float healthPercent = GetHealthPercent(currentHealth, maxLikelyHealth);
        Vector3 basePosition = PositionHealthBarAboveFighter(fighterTransform);
        float fullWidth = 1.25f;
        float fillWidth = Mathf.Max(0.05f, fullWidth * healthPercent);

        SetHealthBarActive(backgroundBar, true);
        SetHealthBarActive(fillBar, true);

        backgroundBar.transform.localPosition = basePosition;
        backgroundBar.transform.localRotation = Quaternion.identity;
        backgroundBar.transform.localScale = new Vector3(fullWidth, 0.08f, 0.08f);

        float fillOffset = ((fullWidth - fillWidth) * 0.5f) * fillDirection;
        fillBar.transform.localPosition = basePosition + new Vector3(fillOffset, 0.02f, -0.02f);
        fillBar.transform.localRotation = Quaternion.identity;
        fillBar.transform.localScale = new Vector3(fillWidth, 0.1f, 0.1f);

        SetObjectUnlitColor(backgroundBar, new Color(0.04f, 0.045f, 0.055f));
        SetObjectUnlitColor(fillBar, GetHealthBarColor(healthPercent));
    }

    float GetHealthPercent(int currentHealth, int maxLikelyHealth)
    {
        int safeMaxHealth = Mathf.Max(1, maxLikelyHealth);
        return Mathf.Clamp01((float)Mathf.Max(0, currentHealth) / safeMaxHealth);
    }

    Color GetHealthBarColor(float healthPercent)
    {
        if (healthPercent > 0.6f)
        {
            return Color.Lerp(new Color(0.1f, 1f, 0.35f), Color.cyan, Mathf.InverseLerp(0.6f, 1f, healthPercent));
        }

        if (healthPercent > 0.3f)
        {
            return Color.Lerp(new Color(1f, 0.45f, 0.05f), new Color(1f, 0.9f, 0.1f), Mathf.InverseLerp(0.3f, 0.6f, healthPercent));
        }

        return Color.Lerp(new Color(0.45f, 0.05f, 0.75f), new Color(1f, 0.05f, 0.02f), Mathf.InverseLerp(0f, 0.3f, healthPercent));
    }

    Vector3 PositionHealthBarAboveFighter(Transform fighterTransform)
    {
        if (fighterTransform == null)
        {
            return Vector3.zero;
        }

        return fighterTransform.localPosition + new Vector3(0f, 1.65f, 0f);
    }

    void SetHealthBarActive(GameObject barPart, bool isActive)
    {
        if (barPart != null)
        {
            barPart.SetActive(isActive);
        }
    }

    void CreateArenaSurfaceVisuals()
    {
        CreatePresentationBackdrop();
        CreatePlatform();
        CreateWall("BackGridWall", new Vector3(0f, 1.35f, 2.65f), new Vector3(5.8f, 2.25f, 0.08f), new Color(0.015f, 0.035f, 0.055f));
        CreateGridLines();
    }

    void CreatePresentationBackdrop()
    {
        GameObject backdrop = GetOrCreateArenaCube("PresentationBackdrop");
        backdrop.transform.localPosition = new Vector3(0f, 1.45f, 3.05f);
        backdrop.transform.localRotation = Quaternion.identity;
        backdrop.transform.localScale = new Vector3(7.6f, 4.2f, 0.12f);
        SetObjectUnlitColor(backdrop, new Color(0.005f, 0.008f, 0.014f));
    }

    void CreatePlatform()
    {
        GameObject platform = GetOrCreateArenaCube("DigitalArenaPlatform");
        platform.transform.localPosition = new Vector3(0f, -0.04f, 0f);
        platform.transform.localRotation = Quaternion.identity;
        platform.transform.localScale = new Vector3(5.8f, 0.08f, 3.55f);
        SetObjectUnlitColor(platform, new Color(0.01f, 0.014f, 0.022f));
    }

    GameObject CreateFighterPlaceholder(string objectName, Vector3 position, Color color)
    {
        GameObject fighter = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        fighter.name = objectName;
        fighter.transform.SetParent(arenaRoot.transform);
        fighter.transform.localPosition = position;
        fighter.transform.localScale = new Vector3(0.62f, 1.12f, 0.62f);
        SetObjectUnlitColor(fighter, color);
        return fighter;
    }

    void CreateMonitorScreen()
    {
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "MonitorScreen";
        screen.transform.SetParent(monitorTransitionRoot.transform);
        screen.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        screen.transform.localScale = new Vector3(4.2f, 2.4f, 0.08f);
        SetObjectColor(screen, new Color(0.02f, 0.12f, 0.18f));
    }

    void CreateMonitorFrame()
    {
        CreateMonitorPart("MonitorFrameTop", new Vector3(0f, 2.55f, 0f), new Vector3(4.8f, 0.18f, 0.18f), new Color(0.02f, 0.02f, 0.025f));
        CreateMonitorPart("MonitorFrameBottom", new Vector3(0f, -0.05f, 0f), new Vector3(4.8f, 0.18f, 0.18f), new Color(0.02f, 0.02f, 0.025f));
        CreateMonitorPart("MonitorFrameLeft", new Vector3(-2.35f, 1.25f, 0f), new Vector3(0.18f, 2.7f, 0.18f), new Color(0.02f, 0.02f, 0.025f));
        CreateMonitorPart("MonitorFrameRight", new Vector3(2.35f, 1.25f, 0f), new Vector3(0.18f, 2.7f, 0.18f), new Color(0.02f, 0.02f, 0.025f));
        CreateMonitorPart("MonitorStand", new Vector3(0f, -0.7f, 0f), new Vector3(0.28f, 1.2f, 0.18f), new Color(0.02f, 0.02f, 0.025f));
        CreateMonitorPart("MonitorBase", new Vector3(0f, -1.35f, 0f), new Vector3(1.6f, 0.18f, 0.8f), new Color(0.02f, 0.02f, 0.025f));
    }

    void CreateMonitorGridMarkers()
    {
        for (int i = -2; i <= 2; i++)
        {
            CreateMonitorPart(
                $"MonitorGridVertical_{i}",
                new Vector3(i * 0.75f, 1.25f, -0.08f),
                new Vector3(0.03f, 2.15f, 0.03f),
                new Color(0f, 0.7f, 1f)
            );
        }

        for (int i = -1; i <= 1; i++)
        {
            CreateMonitorPart(
                $"MonitorGridHorizontal_{i}",
                new Vector3(0f, 1.25f + (i * 0.65f), -0.09f),
                new Vector3(3.8f, 0.03f, 0.03f),
                new Color(0f, 0.7f, 1f)
            );
        }
    }

    void CreateImprintStream()
    {
        CreateImprintStreamNode("ImprintStreamA_Outer", new Vector3(-1.45f, -0.85f, -0.35f), Color.cyan, 0.2f);
        CreateImprintStreamNode("ImprintStreamA_Inner", new Vector3(-0.65f, 0.15f, -0.25f), Color.cyan, 0.16f);
        CreateImprintStreamNode("ImprintStreamB_Outer", new Vector3(1.45f, -0.85f, -0.35f), Color.magenta, 0.2f);
        CreateImprintStreamNode("ImprintStreamB_Inner", new Vector3(0.65f, 0.15f, -0.25f), Color.magenta, 0.16f);
        CreateImprintStreamNode("ImprintStreamCore", new Vector3(0f, 1.25f, -0.3f), new Color(0.45f, 1f, 0.75f), 0.28f);
    }

    void CreateMonitorPart(string objectName, Vector3 position, Vector3 scale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = objectName;
        part.transform.SetParent(monitorTransitionRoot.transform);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        SetObjectColor(part, color);
    }

    void CreateImprintStreamNode(string objectName, Vector3 position, Color color, float size)
    {
        GameObject streamNode = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        streamNode.name = objectName;
        streamNode.transform.SetParent(monitorTransitionRoot.transform);
        streamNode.transform.localPosition = position;
        streamNode.transform.localScale = new Vector3(size, size, size);
        SetObjectColor(streamNode, color);
    }

    void CreateScanChamberBase()
    {
        GameObject basePlatform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        basePlatform.name = "SealedScanChamberBase";
        basePlatform.transform.SetParent(scanChamberRoot.transform);
        basePlatform.transform.localPosition = Vector3.zero;
        basePlatform.transform.localScale = new Vector3(5f, 0.15f, 3f);
        SetObjectColor(basePlatform, new Color(0.08f, 0.08f, 0.1f));
    }

    GameObject CreateSafeDogPlaceholder(string objectName, Vector3 position, Color color)
    {
        GameObject safeDog = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        safeDog.name = objectName;
        safeDog.transform.SetParent(scanChamberRoot.transform);
        safeDog.transform.localPosition = position;
        safeDog.transform.localScale = new Vector3(0.65f, 1.05f, 0.65f);
        SetObjectColor(safeDog, color);
        return safeDog;
    }

    void CreateScanBeam(string objectName, Vector3 position, Color color)
    {
        GameObject scanBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        scanBeam.name = objectName;
        scanBeam.transform.SetParent(scanChamberRoot.transform);
        scanBeam.transform.localPosition = position;
        scanBeam.transform.localScale = new Vector3(0.18f, 1.4f, 0.18f);
        SetObjectColor(scanBeam, color);
    }

    void CreateCopyCore()
    {
        GameObject copyCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        copyCore.name = "DNACopyCore";
        copyCore.transform.SetParent(scanChamberRoot.transform);
        copyCore.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        copyCore.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
        SetObjectColor(copyCore, new Color(0.45f, 1f, 0.75f));
    }

    void CreateArenaImpactEffectsIfNeeded()
    {
        if (arenaImpactEffectsCreated)
        {
            return;
        }

        if (arenaRoot == null)
        {
            return;
        }

        impactSparkA = CreateArenaImpactEffectObject("ImpactSparkA", PrimitiveType.Sphere, new Vector3(0.7f, 0.7f, 0.7f), Color.red);
        impactSparkB = CreateArenaImpactEffectObject("ImpactSparkB", PrimitiveType.Sphere, new Vector3(0.7f, 0.7f, 0.7f), Color.red);
        corruptionNodeA = CreateArenaImpactEffectObject("CorruptionNodeA", PrimitiveType.Cube, new Vector3(0.5f, 0.5f, 0.5f), new Color(0.75f, 0.1f, 1f));
        corruptionNodeB = CreateArenaImpactEffectObject("CorruptionNodeB", PrimitiveType.Cube, new Vector3(0.5f, 0.5f, 0.5f), new Color(0.75f, 0.1f, 1f));
        impactRingA = CreateArenaImpactEffectObject("ImpactRingA", PrimitiveType.Cylinder, new Vector3(1.15f, 0.045f, 1.15f), new Color(1f, 0.45f, 0.05f));
        impactRingB = CreateArenaImpactEffectObject("ImpactRingB", PrimitiveType.Cylinder, new Vector3(1.15f, 0.045f, 1.15f), new Color(1f, 0.45f, 0.05f));

        arenaImpactEffectsCreated = true;
        HideImpactEffects();
    }

    GameObject CreateArenaImpactEffectObject(string objectName, PrimitiveType primitiveType, Vector3 baseScale, Color color)
    {
        Transform existingEffect = arenaRoot.transform.Find(objectName);
        GameObject effectObject;

        if (existingEffect != null)
        {
            effectObject = existingEffect.gameObject;
        }
        else
        {
            effectObject = GameObject.CreatePrimitive(primitiveType);
            effectObject.name = objectName;
            effectObject.transform.SetParent(arenaRoot.transform);
        }

        effectObject.hideFlags = HideFlags.DontSave;
        effectObject.transform.localPosition = Vector3.zero;
        effectObject.transform.localRotation = Quaternion.identity;
        effectObject.transform.localScale = baseScale;
        SetObjectUnlitColor(effectObject, color);
        effectObject.SetActive(false);

        return effectObject;
    }

    void ShowImpactEffect(Transform target, int impact, string effectName)
    {
        if (target == null || impact <= 0)
        {
            return;
        }

        CreateArenaImpactEffectsIfNeeded();

        GameObject spark = effectName == "A" ? impactSparkA : impactSparkB;
        GameObject corruptionNode = effectName == "A" ? corruptionNodeA : corruptionNodeB;
        GameObject impactRing = effectName == "A" ? impactRingA : impactRingB;

        SetImpactEffectScale(spark, impact, new Vector3(0.7f, 0.7f, 0.7f));
        SetImpactEffectScale(corruptionNode, impact, new Vector3(0.5f, 0.5f, 0.5f));
        SetImpactEffectScale(impactRing, impact, new Vector3(1.15f, 0.045f, 1.15f));

        SetObjectUnlitColor(spark, GetImpactEffectColor(impact, new Color(1f, 0.35f, 0.05f), new Color(1f, 0.05f, 0.02f)));
        SetObjectUnlitColor(corruptionNode, GetImpactEffectColor(impact, new Color(0.65f, 0.1f, 1f), new Color(1f, 0.1f, 1f)));
        SetObjectUnlitColor(impactRing, GetImpactEffectColor(impact, new Color(0f, 0.75f, 1f), new Color(1f, 0.45f, 0.05f)));

        PositionImpactEffectNearTarget(spark, target, impact, new Vector3(0f, 0.45f, -0.1f));
        PositionImpactEffectNearTarget(corruptionNode, target, impact, new Vector3(0f, 0.75f, 0.08f));
        PositionImpactEffectNearTarget(impactRing, target, impact, new Vector3(0f, -0.48f, 0f));

        SetImpactEffectActive(spark, true);
        SetImpactEffectActive(corruptionNode, true);
        SetImpactEffectActive(impactRing, true);
    }

    void HideImpactEffects()
    {
        SetImpactEffectActive(impactSparkA, false);
        SetImpactEffectActive(impactSparkB, false);
        SetImpactEffectActive(corruptionNodeA, false);
        SetImpactEffectActive(corruptionNodeB, false);
        SetImpactEffectActive(impactRingA, false);
        SetImpactEffectActive(impactRingB, false);
    }

    void SetImpactEffectScale(GameObject effectObject, int impact, Vector3 baseScale)
    {
        if (effectObject == null)
        {
            return;
        }

        float intensity = Mathf.InverseLerp(1f, 35f, impact);
        float scaleMultiplier = Mathf.Lerp(1.05f, 2.35f, intensity);
        effectObject.transform.localScale = baseScale * scaleMultiplier;
    }

    Color GetImpactEffectColor(int impact, Color lowImpactColor, Color highImpactColor)
    {
        float intensity = Mathf.InverseLerp(1f, 35f, impact);
        return Color.Lerp(lowImpactColor, highImpactColor, intensity);
    }

    void PositionImpactEffectNearTarget(GameObject effectObject, Transform target, int impact, Vector3 localOffset)
    {
        if (effectObject == null || target == null)
        {
            return;
        }

        float intensity = Mathf.InverseLerp(1f, 35f, impact);
        float sideDistance = Mathf.Lerp(0.5f, 0.18f, intensity);
        float sideDirection = target == fighterATransform ? 1f : -1f;

        effectObject.transform.localPosition = target.localPosition + localOffset + new Vector3(sideDirection * sideDistance, 0f, 0f);
    }

    void SetImpactEffectActive(GameObject effectObject, bool isActive)
    {
        if (effectObject != null)
        {
            effectObject.SetActive(isActive);
        }
    }

    IEnumerator AnimateRoundExchange(int roundNumber, Dog dogA, Dog dogB, int dogAHealth, int dogBHealth, int dogAImpact, int dogBImpact)
    {
        if (fighterATransform == null || fighterBTransform == null)
        {
            roundAnimationCoroutine = null;
            yield break;
        }

        Vector3 fighterAHome = new Vector3(-1.75f, 0.6f, 0f);
        Vector3 fighterBHome = new Vector3(1.75f, 0.6f, 0f);

        float dogALunge = GetImpactLungeDistance(dogAImpact);
        float dogBLunge = GetImpactLungeDistance(dogBImpact);
        float dogARecoil = GetImpactRecoilDistance(dogBImpact, dogAImpact);
        float dogBRecoil = GetImpactRecoilDistance(dogAImpact, dogBImpact);

        Vector3 fighterAImpactPosition = fighterAHome + new Vector3(dogALunge - dogARecoil, 0f, 0f);
        Vector3 fighterBImpactPosition = fighterBHome + new Vector3(-dogBLunge + dogBRecoil, 0f, 0f);

        fighterAImpactPosition.x = Mathf.Clamp(fighterAImpactPosition.x, -2.4f, -0.35f);
        fighterBImpactPosition.x = Mathf.Clamp(fighterBImpactPosition.x, 0.35f, 2.4f);

        CreateArenaImpactEffectsIfNeeded();
        HideImpactEffects();

        if (dogAImpact > 0)
        {
            ShowImpactEffect(fighterBTransform, dogAImpact, "B");
        }

        if (dogBImpact > 0)
        {
            ShowImpactEffect(fighterATransform, dogBImpact, "A");
        }

        float halfDuration = RoundActionDurationSeconds * 0.5f;

        yield return AnimateFightersToPositions(dogA, dogB, dogAHealth, dogBHealth, fighterAHome, fighterBHome, fighterAImpactPosition, fighterBImpactPosition, halfDuration);
        yield return AnimateFightersToPositions(dogA, dogB, dogAHealth, dogBHealth, fighterAImpactPosition, fighterBImpactPosition, fighterAHome, fighterBHome, halfDuration);

        HideImpactEffects();
        roundAnimationCoroutine = null;
        ResetFighterArenaPositions();
        UpdateDogPortraitBillboards(dogA, dogB);
        UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
        UpdateArenaLabels(dogA, dogB);
        UpdateRoundStatusBanner(roundNumber, dogAHealth, dogBHealth, dogAImpact, dogBImpact, false);
    }

    IEnumerator AnimateFightersToPositions(
        Dog dogA,
        Dog dogB,
        int dogAHealth,
        int dogBHealth,
        Vector3 fighterAStart,
        Vector3 fighterBStart,
        Vector3 fighterATarget,
        Vector3 fighterBTarget,
        float duration
    )
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float smoothProgress = progress * progress * (3f - (2f * progress));

            if (fighterATransform != null)
            {
                fighterATransform.localPosition = Vector3.Lerp(fighterAStart, fighterATarget, smoothProgress);
            }

            if (fighterBTransform != null)
            {
                fighterBTransform.localPosition = Vector3.Lerp(fighterBStart, fighterBTarget, smoothProgress);
            }

            UpdateArenaLabels(dogA, dogB);
            UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
            UpdateDogPortraitBillboards(dogA, dogB);
            yield return null;
        }
    }

    void StopRoundAnimationIfRunning()
    {
        if (roundAnimationCoroutine == null)
        {
            HideImpactEffects();
            return;
        }

        StopCoroutine(roundAnimationCoroutine);
        roundAnimationCoroutine = null;
        HideImpactEffects();
    }

    void ResetFighterArenaPositions()
    {
        if (fighterATransform != null)
        {
            fighterATransform.localPosition = new Vector3(-1.75f, 0.6f, 0f);
            fighterATransform.localScale = new Vector3(0.62f, 1.12f, 0.62f);
            SetObjectUnlitColor(fighterATransform.gameObject, Color.cyan);
        }

        if (fighterBTransform != null)
        {
            fighterBTransform.localPosition = new Vector3(1.75f, 0.6f, 0f);
            fighterBTransform.localScale = new Vector3(0.62f, 1.12f, 0.62f);
            SetObjectUnlitColor(fighterBTransform.gameObject, Color.magenta);
        }
    }

    float GetImpactLungeDistance(int impact)
    {
        if (impact <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp(0.25f + (impact * 0.015f), 0.25f, 0.7f);
    }

    float GetImpactRecoilDistance(int incomingImpact, int ownImpact)
    {
        int impactGap = incomingImpact - ownImpact;

        if (impactGap <= 4)
        {
            return 0f;
        }

        return Mathf.Clamp(impactGap * 0.025f, 0.12f, 0.6f);
    }

    void MarkWinner(Transform fighterTransform)
    {
        if (fighterTransform == null)
        {
            return;
        }

        fighterTransform.localPosition = new Vector3(fighterTransform.localPosition.x, 0.9f, fighterTransform.localPosition.z);
        fighterTransform.localScale = new Vector3(0.95f, 1.55f, 0.95f);
        SetObjectUnlitColor(fighterTransform.gameObject, new Color(0.1f, 1f, 0.35f));
    }

    void MarkLoser(Transform fighterTransform)
    {
        if (fighterTransform == null)
        {
            return;
        }

        fighterTransform.localPosition = new Vector3(fighterTransform.localPosition.x, 0.35f, fighterTransform.localPosition.z);
        fighterTransform.localScale = new Vector3(0.55f, 0.75f, 0.55f);
        SetObjectUnlitColor(fighterTransform.gameObject, new Color(0.25f, 0.25f, 0.3f));
    }

    void MarkDraw(Transform fighterTransform)
    {
        if (fighterTransform == null)
        {
            return;
        }

        fighterTransform.localPosition = new Vector3(fighterTransform.localPosition.x, 0.8f, fighterTransform.localPosition.z);
        fighterTransform.localScale = new Vector3(0.82f, 1.35f, 0.82f);
        SetObjectUnlitColor(fighterTransform.gameObject, new Color(1f, 0.85f, 0.2f));
    }

    void CreateMarker(string objectName, Vector3 position, Color color)
    {
        Transform existingMarker = arenaRoot.transform.Find(objectName);
        GameObject marker;

        if (existingMarker != null)
        {
            marker = existingMarker.gameObject;
        }
        else
        {
            marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = objectName;
            marker.transform.SetParent(arenaRoot.transform);
        }

        marker.hideFlags = HideFlags.DontSave;
        marker.transform.localPosition = position;
        marker.transform.localRotation = Quaternion.identity;
        marker.transform.localScale = new Vector3(0.18f, 0.025f, 0.18f);
        SetObjectUnlitColor(marker, color);
    }

    GameObject CreateWall(string objectName, Vector3 position, Vector3 scale, Color color)
    {
        GameObject wall = GetOrCreateArenaCube(objectName);
        wall.transform.localPosition = position;
        wall.transform.localRotation = Quaternion.identity;
        wall.transform.localScale = scale;
        SetObjectUnlitColor(wall, color);
        return wall;
    }

    void CreateGridLines()
    {
        Color gridColor = new Color(0f, 0.78f, 1f);
        Color borderColor = new Color(0.25f, 1f, 1f);
        float gridHeight = 0.09f;
        float gridThickness = 0.065f;

        for (int i = -3; i <= 3; i++)
        {
            CreateBrightWall(
                $"GridLine_X_{i}",
                new Vector3(i, gridHeight, 0f),
                new Vector3(gridThickness, 0.035f, 3.65f),
                gridColor
            );
        }

        for (int i = -2; i <= 2; i++)
        {
            CreateBrightWall(
                $"GridLine_Z_{i}",
                new Vector3(0f, gridHeight, i),
                new Vector3(5.95f, 0.035f, gridThickness),
                gridColor
            );
        }

        CreateBrightWall("ArenaBorder_North", new Vector3(0f, gridHeight + 0.025f, 1.85f), new Vector3(6.15f, 0.075f, 0.11f), borderColor);
        CreateBrightWall("ArenaBorder_South", new Vector3(0f, gridHeight + 0.025f, -1.85f), new Vector3(6.15f, 0.075f, 0.11f), borderColor);
        CreateBrightWall("ArenaBorder_East", new Vector3(3f, gridHeight + 0.025f, 0f), new Vector3(0.11f, 0.075f, 3.75f), borderColor);
        CreateBrightWall("ArenaBorder_West", new Vector3(-3f, gridHeight + 0.025f, 0f), new Vector3(0.11f, 0.075f, 3.75f), borderColor);
    }

    void CreateBrightWall(string objectName, Vector3 position, Vector3 scale, Color color)
    {
        GameObject wall = CreateWall(objectName, position, scale, color);
        SetObjectUnlitColor(wall, color);
    }

    void SetObjectColor(GameObject targetObject, Color color)
    {
        if (targetObject == null)
        {
            return;
        }

        PrepareRuntimePrimitive(targetObject);

        Renderer objectRenderer = targetObject.GetComponent<Renderer>();

        if (objectRenderer == null)
        {
            return;
        }

        Material runtimeMaterial = new Material(objectRenderer.sharedMaterial);
        runtimeMaterial.color = color;
        objectRenderer.material = runtimeMaterial;
    }

    GameObject GetOrCreateArenaCube(string objectName)
    {
        Transform existingObject = arenaRoot.transform.Find(objectName);

        if (existingObject != null)
        {
            existingObject.gameObject.hideFlags = HideFlags.DontSave;
            return existingObject.gameObject;
        }

        GameObject createdObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        createdObject.name = objectName;
        createdObject.transform.SetParent(arenaRoot.transform);
        createdObject.hideFlags = HideFlags.DontSave;
        return createdObject;
    }

    void SetObjectUnlitColor(GameObject targetObject, Color color)
    {
        if (targetObject == null)
        {
            return;
        }

        PrepareRuntimePrimitive(targetObject);

        Renderer objectRenderer = targetObject.GetComponent<Renderer>();

        if (objectRenderer == null)
        {
            return;
        }

        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");

        if (unlitShader == null)
        {
            unlitShader = Shader.Find("Unlit/Color");
        }

        Material runtimeMaterial = unlitShader != null
            ? new Material(unlitShader)
            : new Material(objectRenderer.sharedMaterial);

        runtimeMaterial.color = color;

        if (runtimeMaterial.HasProperty("_BaseColor"))
        {
            runtimeMaterial.SetColor("_BaseColor", color);
        }

        if (runtimeMaterial.HasProperty("_Color"))
        {
            runtimeMaterial.SetColor("_Color", color);
        }

        objectRenderer.material = runtimeMaterial;
    }

    void PrepareRuntimePrimitive(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return;
        }

        targetObject.hideFlags = HideFlags.DontSave;

        Collider objectCollider = targetObject.GetComponent<Collider>();

        if (objectCollider == null)
        {
            return;
        }

        // These presentation primitives are visual only. Removing their colliders also keeps
        // Game view Gizmos from drawing a noisy collider-wireframe over the arena.
        if (Application.isPlaying)
        {
            Destroy(objectCollider);
        }
        else
        {
            DestroyImmediate(objectCollider);
        }
    }
}
