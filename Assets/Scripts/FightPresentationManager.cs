using System.Collections;
using UnityEngine;

public class FightPresentationManager : MonoBehaviour
{
    private const string ArenaRootName = "ArenaRoot";
    private const string ScanChamberRootName = "ScanChamberRoot";
    private const string MonitorTransitionRootName = "MonitorTransitionRoot";
    private const string PresentationCameraName = "PresentationCamera";
    private const float ScanIntroDelaySeconds = 1.5f;
    private const float MonitorTransitionDelaySeconds = 1f;
    private const float CameraMoveDurationSeconds = 0.75f;
    private const float RoundActionDurationSeconds = 0.8f;

    private static GameObject sharedArenaRoot;
    private static GameObject sharedScanChamberRoot;
    private static GameObject sharedMonitorTransitionRoot;
    private static GameObject sharedPresentationCameraObject;

    private GameObject arenaRoot;
    private GameObject scanChamberRoot;
    private GameObject monitorTransitionRoot;
    private GameObject presentationCameraObject;
    private Camera presentationCamera;
    private bool arenaObjectsCreated;
    private bool scanChamberObjectsCreated;
    private bool monitorTransitionObjectsCreated;
    private bool arenaImpactEffectsCreated;
    private bool imprintCorruptionNodesCreated;
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
        HideImpactEffects();
        UpdateImprintCorruptionVisuals(0, 0);
        UpdateArenaLabels(dogA, dogB);
        arenaRoot.SetActive(true);
        FrameArena();

        Debug.Log($"Digital arena ready: {dogA.dogName} imprint vs {dogB.dogName} imprint.");
    }

    public void HideArena()
    {
        EnsureArenaRoot();
        StopRoundAnimationIfRunning();

        if (arenaRoot != null)
        {
            arenaRoot.SetActive(false);
        }

        SetPresentationCameraEnabled(false);
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
        arenaRoot.SetActive(true);
        FrameArena();

        float roundStep = Mathf.Clamp(roundNumber, 1, 6) * 0.08f;
        float pulse = roundNumber % 2 == 0 ? 1.12f : 0.95f;

        if (fighterATransform != null)
        {
            fighterATransform.localPosition = new Vector3(-2f + roundStep, 0.6f, 0f);
            fighterATransform.localScale = new Vector3(0.7f * pulse, 1.2f * pulse, 0.7f * pulse);
        }

        if (fighterBTransform != null)
        {
            fighterBTransform.localPosition = new Vector3(2f - roundStep, 0.6f, 0f);
            fighterBTransform.localScale = new Vector3(0.7f * pulse, 1.2f * pulse, 0.7f * pulse);
        }

        UpdateArenaLabels(dogA, dogB);
        UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);

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
        UpdateArenaLabels(dogA, dogB);
        UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
        FrameArena();

        roundAnimationCoroutine = StartCoroutine(AnimateRoundExchange(dogA, dogB, dogAHealth, dogBHealth, dogAImpact, dogBImpact));
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
        arenaRoot.SetActive(true);
        FrameArena();
        StopRoundAnimationIfRunning();

        if (dogAHealth > dogBHealth)
        {
            MarkWinner(fighterATransform);
            MarkLoser(fighterBTransform);
            UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
            UpdateArenaResultLabels(dogA, dogB, "WINNER", "CORRUPTED / DEFEATED", new Color(0.1f, 1f, 0.35f), new Color(0.6f, 0.6f, 0.65f));
            Debug.Log($"Digital arena result: {dogA.dogName} imprint wins. {dogB.dogName} imprint falls back.");
            return;
        }

        if (dogBHealth > dogAHealth)
        {
            MarkWinner(fighterBTransform);
            MarkLoser(fighterATransform);
            UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
            UpdateArenaResultLabels(dogA, dogB, "CORRUPTED / DEFEATED", "WINNER", new Color(0.6f, 0.6f, 0.65f), new Color(0.1f, 1f, 0.35f));
            Debug.Log($"Digital arena result: {dogB.dogName} imprint wins. {dogA.dogName} imprint falls back.");
            return;
        }

        MarkDraw(fighterATransform);
        MarkDraw(fighterBTransform);
        UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
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
    }

    void FrameScanChamber()
    {
        SetPresentationCameraInstant(new Vector3(0f, 3.5f, -7f), new Vector3(0f, 1.2f, 0f));
    }

    void FrameMonitorTransition()
    {
        MovePresentationCameraTo(new Vector3(0f, 3f, -6f), new Vector3(0f, 1.2f, 0f), CameraMoveDurationSeconds);
    }

    void FrameArena()
    {
        MovePresentationCameraTo(new Vector3(0f, 5f, -8f), new Vector3(0f, 0.8f, 0f), CameraMoveDurationSeconds);
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

        if (arenaRoot.transform.childCount > 0)
        {
            AssignExistingArenaTransforms();
            arenaObjectsCreated = true;
            return;
        }

        CreatePlatform();
        fighterATransform = CreateFighterPlaceholder("FighterA_Imprint", new Vector3(-2f, 0.6f, 0f), Color.cyan).transform;
        fighterBTransform = CreateFighterPlaceholder("FighterB_Imprint", new Vector3(2f, 0.6f, 0f), Color.magenta).transform;
        CreateMarker("CenterMarker", new Vector3(0f, 0.05f, 0f), Color.white);
        CreateWall("BackGridWall", new Vector3(0f, 1.4f, 2.6f), new Vector3(5.5f, 2.5f, 0.08f), new Color(0.1f, 0.25f, 0.35f, 0.7f));
        CreateGridLines();

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

        CreateOrUpdateLabel(scanChamberRoot, "ScanChamberTitleLabel", "SEALED DNA SCAN CHAMBER", new Vector3(0f, 3f, 0f), Color.white, 0.18f);
        CreateOrUpdateLabel(scanChamberRoot, "ScanChamberSafetyLabel", "REAL DOGS SAFE - COPYING DIGITAL IMPRINTS", new Vector3(0f, 2.65f, 0f), new Color(0.45f, 1f, 0.75f), 0.12f);
        CreateOrUpdateLabel(scanChamberRoot, "SafeDogALabel", GetDogDisplayName(dogA, "DOG A"), GetLabelPosition(scanDogATransform, new Vector3(-1.5f, 1.8f, 0f)), Color.cyan, 0.14f);
        CreateOrUpdateLabel(scanChamberRoot, "SafeDogBLabel", GetDogDisplayName(dogB, "DOG B"), GetLabelPosition(scanDogBTransform, new Vector3(1.5f, 1.8f, 0f)), Color.magenta, 0.14f);
    }

    void UpdateMonitorTransitionLabels()
    {
        if (monitorTransitionRoot == null)
        {
            return;
        }

        CreateOrUpdateLabel(monitorTransitionRoot, "MonitorTransitionTitleLabel", "DIGITAL IMPRINT TRANSFER", new Vector3(0f, 3.15f, 0f), Color.white, 0.18f);
        CreateOrUpdateLabel(monitorTransitionRoot, "MonitorTransitionStatusLabel", "IMPRINTS ENTERING MONITOR GRID", new Vector3(0f, 2.8f, 0f), new Color(0.45f, 1f, 0.75f), 0.13f);
    }

    void UpdateArenaLabels(Dog dogA, Dog dogB)
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreateOrUpdateLabel(arenaRoot, "ArenaTitleLabel", "DIGITAL ARENA", new Vector3(0f, 3f, 0f), Color.white, 0.2f);
        CreateOrUpdateLabel(arenaRoot, "FighterALabel", $"{GetDogDisplayName(dogA, "DOG A")} IMPRINT", GetLabelPosition(fighterATransform, new Vector3(-2f, 1.85f, 0f)), Color.cyan, 0.14f);
        CreateOrUpdateLabel(arenaRoot, "FighterBLabel", $"{GetDogDisplayName(dogB, "DOG B")} IMPRINT", GetLabelPosition(fighterBTransform, new Vector3(2f, 1.85f, 0f)), Color.magenta, 0.14f);
    }

    void UpdateArenaResultLabels(Dog dogA, Dog dogB, string dogAStatus, string dogBStatus, Color dogAColor, Color dogBColor)
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreateOrUpdateLabel(arenaRoot, "ArenaTitleLabel", "DIGITAL ARENA", new Vector3(0f, 3f, 0f), Color.white, 0.2f);
        CreateOrUpdateLabel(arenaRoot, "FighterALabel", $"{GetDogDisplayName(dogA, "DOG A")} IMPRINT\n{dogAStatus}", GetLabelPosition(fighterATransform, new Vector3(-2f, 1.85f, 0f)), dogAColor, 0.14f);
        CreateOrUpdateLabel(arenaRoot, "FighterBLabel", $"{GetDogDisplayName(dogB, "DOG B")} IMPRINT\n{dogBStatus}", GetLabelPosition(fighterBTransform, new Vector3(2f, 1.85f, 0f)), dogBColor, 0.14f);
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

        return targetTransform.localPosition + new Vector3(0f, 1.2f, 0f);
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

        SetObjectColor(fighterTransform.gameObject, GetCorruptionColor(cleanColor, corruptionStrength));
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
            CreateImprintCorruptionNode("ImprintCorruptionA_3", PrimitiveType.Cube)
        };

        imprintCorruptionNodesB = new GameObject[]
        {
            CreateImprintCorruptionNode("ImprintCorruptionB_1", PrimitiveType.Cube),
            CreateImprintCorruptionNode("ImprintCorruptionB_2", PrimitiveType.Sphere),
            CreateImprintCorruptionNode("ImprintCorruptionB_3", PrimitiveType.Cube)
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
        SetObjectColor(nodeObject, new Color(0.8f, 0.1f, 1f));
        nodeObject.SetActive(false);

        return nodeObject;
    }

    void UpdateCorruptionNodes(GameObject[] corruptionNodes, Transform fighterTransform, float corruptionStrength)
    {
        if (corruptionNodes == null)
        {
            return;
        }

        bool shouldShowNodes = fighterTransform != null && corruptionStrength > 0.15f;
        Vector3[] offsets = new Vector3[]
        {
            new Vector3(-0.42f, 0.75f, 0.05f),
            new Vector3(0.32f, 1.05f, -0.08f),
            new Vector3(0.2f, 0.42f, 0.12f)
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

            float scale = Mathf.Lerp(0.08f, 0.38f, corruptionStrength);
            node.transform.localPosition = fighterTransform.localPosition + offsets[i % offsets.Length] * Mathf.Lerp(0.8f, 1.35f, corruptionStrength);
            node.transform.localRotation = Quaternion.Euler(0f, 45f + (corruptionStrength * 90f), 0f);
            node.transform.localScale = new Vector3(scale, scale, scale);
            SetObjectColor(node, GetCorruptionColor(new Color(0.75f, 0.1f, 1f), corruptionStrength));
        }
    }

    Color GetCorruptionColor(Color cleanColor, float corruptionStrength)
    {
        Color damagedColor = Color.Lerp(new Color(0.75f, 0.1f, 1f), new Color(0.18f, 0.03f, 0.28f), corruptionStrength);
        return Color.Lerp(cleanColor, damagedColor, Mathf.Clamp01(corruptionStrength));
    }

    Vector3 GetCorruptionScale(float corruptionStrength)
    {
        float horizontalScale = Mathf.Lerp(0.7f, 0.82f, corruptionStrength);
        float verticalScale = Mathf.Lerp(1.2f, 1.05f, corruptionStrength);
        float depthScale = Mathf.Lerp(0.7f, 0.55f, corruptionStrength);

        return new Vector3(horizontalScale, verticalScale, depthScale);
    }

    void CreatePlatform()
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "DigitalArenaPlatform";
        platform.transform.SetParent(arenaRoot.transform);
        platform.transform.localPosition = Vector3.zero;
        platform.transform.localScale = new Vector3(6f, 0.1f, 4f);
        SetObjectColor(platform, new Color(0.04f, 0.08f, 0.1f));
    }

    GameObject CreateFighterPlaceholder(string objectName, Vector3 position, Color color)
    {
        GameObject fighter = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        fighter.name = objectName;
        fighter.transform.SetParent(arenaRoot.transform);
        fighter.transform.localPosition = position;
        fighter.transform.localScale = new Vector3(0.7f, 1.2f, 0.7f);
        SetObjectColor(fighter, color);
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

        impactSparkA = CreateArenaImpactEffectObject("ImpactSparkA", PrimitiveType.Sphere, new Vector3(0.55f, 0.55f, 0.55f), Color.red);
        impactSparkB = CreateArenaImpactEffectObject("ImpactSparkB", PrimitiveType.Sphere, new Vector3(0.55f, 0.55f, 0.55f), Color.red);
        corruptionNodeA = CreateArenaImpactEffectObject("CorruptionNodeA", PrimitiveType.Cube, new Vector3(0.4f, 0.4f, 0.4f), new Color(0.75f, 0.1f, 1f));
        corruptionNodeB = CreateArenaImpactEffectObject("CorruptionNodeB", PrimitiveType.Cube, new Vector3(0.4f, 0.4f, 0.4f), new Color(0.75f, 0.1f, 1f));
        impactRingA = CreateArenaImpactEffectObject("ImpactRingA", PrimitiveType.Cylinder, new Vector3(0.9f, 0.03f, 0.9f), new Color(1f, 0.45f, 0.05f));
        impactRingB = CreateArenaImpactEffectObject("ImpactRingB", PrimitiveType.Cylinder, new Vector3(0.9f, 0.03f, 0.9f), new Color(1f, 0.45f, 0.05f));

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
        SetObjectColor(effectObject, color);
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

        SetImpactEffectScale(spark, impact, new Vector3(0.55f, 0.55f, 0.55f));
        SetImpactEffectScale(corruptionNode, impact, new Vector3(0.4f, 0.4f, 0.4f));
        SetImpactEffectScale(impactRing, impact, new Vector3(0.9f, 0.03f, 0.9f));

        SetObjectColor(spark, GetImpactEffectColor(impact, new Color(1f, 0.35f, 0.05f), new Color(1f, 0.05f, 0.02f)));
        SetObjectColor(corruptionNode, GetImpactEffectColor(impact, new Color(0.65f, 0.1f, 1f), new Color(1f, 0.1f, 1f)));
        SetObjectColor(impactRing, GetImpactEffectColor(impact, new Color(0f, 0.75f, 1f), new Color(1f, 0.45f, 0.05f)));

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

        float intensity = Mathf.InverseLerp(1f, 55f, impact);
        float scaleMultiplier = Mathf.Lerp(0.9f, 1.85f, intensity);
        effectObject.transform.localScale = baseScale * scaleMultiplier;
    }

    Color GetImpactEffectColor(int impact, Color lowImpactColor, Color highImpactColor)
    {
        float intensity = Mathf.InverseLerp(1f, 55f, impact);
        return Color.Lerp(lowImpactColor, highImpactColor, intensity);
    }

    void PositionImpactEffectNearTarget(GameObject effectObject, Transform target, int impact, Vector3 localOffset)
    {
        if (effectObject == null || target == null)
        {
            return;
        }

        float intensity = Mathf.InverseLerp(1f, 55f, impact);
        float sideDistance = Mathf.Lerp(0.55f, 0.25f, intensity);
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

    IEnumerator AnimateRoundExchange(Dog dogA, Dog dogB, int dogAHealth, int dogBHealth, int dogAImpact, int dogBImpact)
    {
        if (fighterATransform == null || fighterBTransform == null)
        {
            roundAnimationCoroutine = null;
            yield break;
        }

        Vector3 fighterAHome = new Vector3(-2f, 0.6f, 0f);
        Vector3 fighterBHome = new Vector3(2f, 0.6f, 0f);

        float dogALunge = GetImpactLungeDistance(dogAImpact);
        float dogBLunge = GetImpactLungeDistance(dogBImpact);
        float dogARecoil = GetImpactRecoilDistance(dogBImpact, dogAImpact);
        float dogBRecoil = GetImpactRecoilDistance(dogAImpact, dogBImpact);

        Vector3 fighterAImpactPosition = fighterAHome + new Vector3(dogALunge - dogARecoil, 0f, 0f);
        Vector3 fighterBImpactPosition = fighterBHome + new Vector3(-dogBLunge + dogBRecoil, 0f, 0f);

        fighterAImpactPosition.x = Mathf.Clamp(fighterAImpactPosition.x, -2.7f, -0.45f);
        fighterBImpactPosition.x = Mathf.Clamp(fighterBImpactPosition.x, 0.45f, 2.7f);

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
        UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
        UpdateArenaLabels(dogA, dogB);
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
            fighterATransform.localPosition = new Vector3(-2f, 0.6f, 0f);
            fighterATransform.localScale = new Vector3(0.7f, 1.2f, 0.7f);
            SetObjectColor(fighterATransform.gameObject, Color.cyan);
        }

        if (fighterBTransform != null)
        {
            fighterBTransform.localPosition = new Vector3(2f, 0.6f, 0f);
            fighterBTransform.localScale = new Vector3(0.7f, 1.2f, 0.7f);
            SetObjectColor(fighterBTransform.gameObject, Color.magenta);
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
        SetObjectColor(fighterTransform.gameObject, new Color(0.1f, 1f, 0.35f));
    }

    void MarkLoser(Transform fighterTransform)
    {
        if (fighterTransform == null)
        {
            return;
        }

        fighterTransform.localPosition = new Vector3(fighterTransform.localPosition.x, 0.35f, fighterTransform.localPosition.z);
        fighterTransform.localScale = new Vector3(0.55f, 0.75f, 0.55f);
        SetObjectColor(fighterTransform.gameObject, new Color(0.25f, 0.25f, 0.3f));
    }

    void MarkDraw(Transform fighterTransform)
    {
        if (fighterTransform == null)
        {
            return;
        }

        fighterTransform.localPosition = new Vector3(fighterTransform.localPosition.x, 0.8f, fighterTransform.localPosition.z);
        fighterTransform.localScale = new Vector3(0.82f, 1.35f, 0.82f);
        SetObjectColor(fighterTransform.gameObject, new Color(1f, 0.85f, 0.2f));
    }

    void CreateMarker(string objectName, Vector3 position, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = objectName;
        marker.transform.SetParent(arenaRoot.transform);
        marker.transform.localPosition = position;
        marker.transform.localScale = new Vector3(0.25f, 0.03f, 0.25f);
        SetObjectColor(marker, color);
    }

    void CreateWall(string objectName, Vector3 position, Vector3 scale, Color color)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = objectName;
        wall.transform.SetParent(arenaRoot.transform);
        wall.transform.localPosition = position;
        wall.transform.localScale = scale;
        SetObjectColor(wall, color);
    }

    void CreateGridLines()
    {
        for (int i = -3; i <= 3; i++)
        {
            CreateWall(
                $"GridLine_X_{i}",
                new Vector3(i, 0.08f, 0f),
                new Vector3(0.03f, 0.03f, 4f),
                new Color(0f, 0.75f, 1f, 0.8f)
            );
        }

        for (int i = -2; i <= 2; i++)
        {
            CreateWall(
                $"GridLine_Z_{i}",
                new Vector3(0f, 0.09f, i),
                new Vector3(6f, 0.03f, 0.03f),
                new Color(0f, 0.75f, 1f, 0.8f)
            );
        }
    }

    void SetObjectColor(GameObject targetObject, Color color)
    {
        if (targetObject == null)
        {
            return;
        }

        Renderer objectRenderer = targetObject.GetComponent<Renderer>();

        if (objectRenderer == null)
        {
            return;
        }

        Material runtimeMaterial = new Material(objectRenderer.sharedMaterial);
        runtimeMaterial.color = color;
        objectRenderer.material = runtimeMaterial;
    }
}
