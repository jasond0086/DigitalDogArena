using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FightPresentationManager : MonoBehaviour
{
    private enum BreedVisualArchetype
    {
        ShepherdSentinel,
        BullyStriker,
        GuardMastiff,
        IronRott,
        SpitzWarden,
        VelocityHound,
        HybridVariant,
        Unknown
    }

    private const string ArenaRootName = "ArenaRoot";
    private const string ScanChamberRootName = "ScanChamberRoot";
    private const string MonitorTransitionRootName = "MonitorTransitionRoot";
    private const string PresentationCameraName = "PresentationCamera";
    private const string FightPresentationViewportName = "FightPresentationViewport";
    private const string DogImprintResourcePath = "FightPresentation/DogImprint";
    private const string BreedArchetypeResourceFolder = "FightPresentation/BreedArchetypes/";
    private const bool ShowDogPortraitPlaceholders = false;
    private const float DogArtGroundY = 0.16f;
    private const float DogArtGroundPadding = 0.02f;
    private const float ContactShadowY = DogArtGroundY + 0.008f;
    private const int ContactShadowSegmentCount = 40;
    private const float DogImprintFallbackVerticalOffset = -0.48f;
    private const float BreedArchetypeArtForwardOffset = -0.42f;
    private const float ContactShadowForwardOffset = BreedArchetypeArtForwardOffset * 0.45f;
    private const float BreedArchetypeArtBaseScale = 0.9f;
    private const float BreedArchetypeSpriteTargetHeight = 1.56f;
    private const float BreedArchetypeSpriteMaxWidth = 2f;
    private const float ScanIntroDelaySeconds = 1.5f;
    private const float MonitorTransitionDelaySeconds = 1f;
    private const float CameraMoveDurationSeconds = 0.75f;
    private const float RoundActionDurationSeconds = 1.6f;
    private const float CinematicHitFreezeMinSeconds = 0.018f;
    private const float CinematicHitFreezeMaxSeconds = 0.07f;
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
    private GameObject dogImprintPrefab;
    private GameObject fighterADogImprintArt;
    private GameObject fighterBDogImprintArt;
    private GameObject fighterABreedArchetypeArt;
    private GameObject fighterBBreedArchetypeArt;
    private GameObject fighterAContactShadow;
    private GameObject fighterBContactShadow;
    private Dog currentDogImprintA;
    private Dog currentDogImprintB;
    private bool attemptedDogImprintLoad;
    private bool warnedMissingDogImprintPrefab;
    private HashSet<string> warnedMissingBreedArtKeys = new HashSet<string>();
    private GameObject impactSparkA;
    private GameObject impactSparkB;
    private GameObject corruptionNodeA;
    private GameObject corruptionNodeB;
    private GameObject impactRingA;
    private GameObject impactRingB;
    private GameObject strategyEffectA;
    private GameObject strategyEffectB;
    private GameObject defensiveShellA;
    private GameObject defensiveShellB;
    private GameObject styleEffectA;
    private GameObject styleEffectB;
    private GameObject clashTextObject;
    private GameObject[] imprintCorruptionNodesA;
    private GameObject[] imprintCorruptionNodesB;
    private GameObject healthBarBackgroundA;
    private GameObject healthBarFillA;
    private GameObject healthBarBackgroundB;
    private GameObject healthBarFillB;
    private GameObject roundStatusBannerObject;
    private GameObject fighterAPortraitBillboard;
    private GameObject fighterBPortraitBillboard;
    private GameObject fighterAPortraitFrame;
    private GameObject fighterBPortraitFrame;
    private bool warnedMissingDogSpriteA;
    private bool warnedMissingDogSpriteB;
    private Dog[] cachedDogPortraitResourceDogs;
    private Material portraitSpriteMaterial;
    private Material contactShadowMaterial;
    private Mesh contactShadowMesh;
    private Coroutine scanIntroCoroutine;
    private Coroutine cameraMoveCoroutine;
    private Coroutine roundAnimationCoroutine;
    private Coroutine delayedResultPresentationCoroutine;
    private Coroutine cinematicCameraCoroutine;
    private Coroutine cameraBeatCoroutine;
    private Coroutine arenaPulseCoroutine;

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
        ReleaseContactShadowResources();
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

        SetCurrentDogImprintIdentity(dogA, dogB);
        ResetVisualHealthTracking();
        StopCameraBeatIfRunning();
        StopCinematicCameraIfRunning();
        StopArenaPulseIfRunning();
        CreateArenaObjectsIfNeeded();
        CreateDogImprintArtIfNeeded();
        CreateArenaImpactEffectsIfNeeded();
        CreateCorruptionNodesIfNeeded();
        CreateHealthBarsIfNeeded();
        CreateRoundStatusBannerIfNeeded();
        CreateDogPortraitBillboardsIfNeeded();
        CreateContactShadowsIfNeeded();
        HideImpactEffects();
        HideStrategyEffects();
        HideRoundStatusBanner();
        HideClashText();
        UpdateImprintCorruptionVisuals(0, 0);
        UpdateDogImprintArtPositions();
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
        StopDelayedResultPresentationIfRunning();
        StopCameraBeatIfRunning();
        StopCinematicCameraIfRunning();
        StopArenaPulseIfRunning();
        HideRoundStatusBanner();
        HideClashText();
        HideStrategyEffects();
        SetDogImprintArtVisible(false);
        SetBreedArchetypeArtVisible(false);
        SetContactShadowVisible(false);
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

        SetCurrentDogImprintIdentity(dogA, dogB);
        CreateArenaObjectsIfNeeded();
        CreateDogImprintArtIfNeeded();
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
            fighterATransform.localScale = new Vector3(0.46f * pulse, 0.84f * pulse, 0.46f * pulse);
        }

        if (fighterBTransform != null)
        {
            fighterBTransform.localPosition = new Vector3(1.75f - roundStep, 0.6f, 0f);
            fighterBTransform.localScale = new Vector3(0.46f * pulse, 0.84f * pulse, 0.46f * pulse);
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
        PresentRoundAction(
            roundNumber,
            dogA,
            dogB,
            dogAHealth,
            dogBHealth,
            dogAImpact,
            dogBImpact,
            FightStrategy.Balanced,
            FightStrategy.Balanced,
            dogA != null ? dogA.fightStyle : FightStyle.Balanced,
            dogB != null ? dogB.fightStyle : FightStyle.Balanced
        );
    }

    public void PresentRoundAction(
        int roundNumber,
        Dog dogA,
        Dog dogB,
        int dogAHealth,
        int dogBHealth,
        int dogAImpact,
        int dogBImpact,
        FightStrategy dogAStrategy,
        FightStrategy dogBStrategy
    )
    {
        PresentRoundAction(
            roundNumber,
            dogA,
            dogB,
            dogAHealth,
            dogBHealth,
            dogAImpact,
            dogBImpact,
            dogAStrategy,
            dogBStrategy,
            dogA != null ? dogA.fightStyle : FightStyle.Balanced,
            dogB != null ? dogB.fightStyle : FightStyle.Balanced
        );
    }

    public void PresentRoundAction(
        int roundNumber,
        Dog dogA,
        Dog dogB,
        int dogAHealth,
        int dogBHealth,
        int dogAImpact,
        int dogBImpact,
        FightStrategy dogAStrategy,
        FightStrategy dogBStrategy,
        FightStyle dogAStyle,
        FightStyle dogBStyle
    )
    {
        if (dogA == null || dogB == null)
        {
            Debug.LogWarning("FightPresentationManager could not present round action because one or both dogs were missing.");
            return;
        }

        SetCurrentDogImprintIdentity(dogA, dogB);
        EnsureArenaRoot();

        if (arenaRoot == null)
        {
            Debug.LogWarning("FightPresentationManager could not present round action because ArenaRoot was missing.");
            return;
        }

        CreateArenaObjectsIfNeeded();
        CreateDogImprintArtIfNeeded();
        CreateArenaImpactEffectsIfNeeded();
        CreateCorruptionNodesIfNeeded();
        CreateHealthBarsIfNeeded();
        CreateRoundStatusBannerIfNeeded();
        CreateDogPortraitBillboardsIfNeeded();
        CreateContactShadowsIfNeeded();
        arenaRoot.SetActive(true);
        SetFightPresentationViewportVisible(true);
        StopRoundAnimationIfRunning();
        StopDelayedResultPresentationIfRunning();
        ResetFighterArenaPositions();
        HideStrategyEffects();
        UpdateMovingFighterVisualPositions(dogA, dogB, dogAHealth, dogBHealth);
        UpdateRoundStatusBanner(roundNumber, dogAHealth, dogBHealth, dogAImpact, dogBImpact, false, dogAStrategy, dogBStrategy, dogAStyle, dogBStyle);
        FrameArena();

        roundAnimationCoroutine = StartCoroutine(AnimateRoundExchange(
            roundNumber,
            dogA,
            dogB,
            dogAHealth,
            dogBHealth,
            dogAImpact,
            dogBImpact,
            dogAStrategy,
            dogBStrategy,
            dogAStyle,
            dogBStyle
        ));
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

        if (roundAnimationCoroutine != null)
        {
            StopDelayedResultPresentationIfRunning();
            delayedResultPresentationCoroutine = StartCoroutine(PresentFightResultAfterRoundAnimation(dogA, dogB, dogAHealth, dogBHealth));
            return;
        }

        SetCurrentDogImprintIdentity(dogA, dogB);
        CreateArenaObjectsIfNeeded();
        CreateArenaImpactEffectsIfNeeded();
        CreateCorruptionNodesIfNeeded();
        CreateHealthBarsIfNeeded();
        CreateRoundStatusBannerIfNeeded();
        CreateDogPortraitBillboardsIfNeeded();
        arenaRoot.SetActive(true);
        FrameArena();
        StopRoundAnimationIfRunning();
        UpdateDogImprintArtPositions();
        UpdateDogPortraitBillboards(dogA, dogB);

        if (dogAHealth > dogBHealth)
        {
            string resultBannerText = GetResultBannerText(dogAHealth, dogBHealth, false);
            MarkWinner(fighterATransform);
            MarkLoser(fighterBTransform);
            UpdateDogPortraitBillboards(dogA, dogB);
            UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
            ApplyPortraitResultVisual(fighterAPortraitBillboard, fighterAPortraitFrame, true, false, Color.cyan);
            ApplyPortraitResultVisual(fighterBPortraitBillboard, fighterBPortraitFrame, false, false, Color.magenta);
            ApplyDogImprintResultVisual(fighterADogImprintArt, true, false, Color.cyan);
            ApplyDogImprintResultVisual(fighterBDogImprintArt, false, false, Color.magenta);
            ApplyBreedArchetypeResultVisual(fighterABreedArchetypeArt, true, false, Color.cyan);
            ApplyBreedArchetypeResultVisual(fighterBBreedArchetypeArt, false, false, Color.magenta);
            UpdateRoundStatusBanner(0, dogAHealth, dogBHealth, 0, 0, true);
            SetRoundStatusBannerText(resultBannerText);
            UpdateArenaResultLabels(dogA, dogB, "WINNER", "DEFEATED", new Color(0.1f, 1f, 0.35f), new Color(0.65f, 0.25f, 0.8f));
            PlayResultCinematic(fighterATransform, false, resultBannerText);
            Debug.Log($"Digital arena result: {dogA.dogName} imprint wins. {dogB.dogName} imprint falls back.");
            return;
        }

        if (dogBHealth > dogAHealth)
        {
            string resultBannerText = GetResultBannerText(dogAHealth, dogBHealth, false);
            MarkWinner(fighterBTransform);
            MarkLoser(fighterATransform);
            UpdateDogPortraitBillboards(dogA, dogB);
            UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
            ApplyPortraitResultVisual(fighterAPortraitBillboard, fighterAPortraitFrame, false, false, Color.cyan);
            ApplyPortraitResultVisual(fighterBPortraitBillboard, fighterBPortraitFrame, true, false, Color.magenta);
            ApplyDogImprintResultVisual(fighterADogImprintArt, false, false, Color.cyan);
            ApplyDogImprintResultVisual(fighterBDogImprintArt, true, false, Color.magenta);
            ApplyBreedArchetypeResultVisual(fighterABreedArchetypeArt, false, false, Color.cyan);
            ApplyBreedArchetypeResultVisual(fighterBBreedArchetypeArt, true, false, Color.magenta);
            UpdateRoundStatusBanner(0, dogAHealth, dogBHealth, 0, 0, true);
            SetRoundStatusBannerText(resultBannerText);
            UpdateArenaResultLabels(dogA, dogB, "DEFEATED", "WINNER", new Color(0.65f, 0.25f, 0.8f), new Color(0.1f, 1f, 0.35f));
            PlayResultCinematic(fighterBTransform, false, resultBannerText);
            Debug.Log($"Digital arena result: {dogB.dogName} imprint wins. {dogA.dogName} imprint falls back.");
            return;
        }

        string drawBannerText = GetResultBannerText(dogAHealth, dogBHealth, true);
        MarkDraw(fighterATransform);
        MarkDraw(fighterBTransform);
        UpdateDogPortraitBillboards(dogA, dogB);
        UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
        ApplyPortraitResultVisual(fighterAPortraitBillboard, fighterAPortraitFrame, false, true, Color.cyan);
        ApplyPortraitResultVisual(fighterBPortraitBillboard, fighterBPortraitFrame, false, true, Color.magenta);
        ApplyDogImprintResultVisual(fighterADogImprintArt, false, true, Color.cyan);
        ApplyDogImprintResultVisual(fighterBDogImprintArt, false, true, Color.magenta);
        ApplyBreedArchetypeResultVisual(fighterABreedArchetypeArt, false, true, Color.cyan);
        ApplyBreedArchetypeResultVisual(fighterBBreedArchetypeArt, false, true, Color.magenta);
        UpdateRoundStatusBanner(0, dogAHealth, dogBHealth, 0, 0, true);
        SetRoundStatusBannerText(drawBannerText);
        UpdateArenaResultLabels(dogA, dogB, "DRAW", "DRAW", new Color(1f, 0.85f, 0.2f), new Color(1f, 0.85f, 0.2f));
        PlayResultCinematic(null, true, drawBannerText);
        Debug.Log($"Digital arena result: {dogA.dogName} and {dogB.dogName} imprints end in a draw.");
    }

    IEnumerator PresentFightResultAfterRoundAnimation(Dog dogA, Dog dogB, int dogAHealth, int dogBHealth)
    {
        while (roundAnimationCoroutine != null)
        {
            yield return null;
        }

        delayedResultPresentationCoroutine = null;
        PresentFightResult(dogA, dogB, dogAHealth, dogBHealth);
    }

    void StopDelayedResultPresentationIfRunning()
    {
        if (delayedResultPresentationCoroutine == null)
        {
            return;
        }

        StopCoroutine(delayedResultPresentationCoroutine);
        delayedResultPresentationCoroutine = null;
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

    void ReleaseContactShadowResources()
    {
        if (contactShadowMaterial != null)
        {
            if (Application.isPlaying)
            {
                Destroy(contactShadowMaterial);
            }
            else
            {
                DestroyImmediate(contactShadowMaterial);
            }

            contactShadowMaterial = null;
        }

        if (contactShadowMesh != null)
        {
            if (Application.isPlaying)
            {
                Destroy(contactShadowMesh);
            }
            else
            {
                DestroyImmediate(contactShadowMesh);
            }

            contactShadowMesh = null;
        }
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
        FrameFightWide();
    }

    void FrameFightWide()
    {
        SetFightPresentationViewportVisible(true);
        StopCameraBeatIfRunning();
        MovePresentationCameraTo(GetArenaCameraPosition(), GetArenaLookAtPosition(), CameraMoveDurationSeconds);
    }

    void ReturnToFightWide()
    {
        SetFightPresentationViewportVisible(true);
        MovePresentationCameraTo(GetArenaCameraPosition(), GetArenaLookAtPosition(), 0.24f);
    }

    Vector3 GetArenaCameraPosition()
    {
        return new Vector3(0f, 4.2f, -7.2f);
    }

    Vector3 GetArenaLookAtPosition()
    {
        return new Vector3(0f, 0.75f, 0.25f);
    }

    Vector3 GetArenaWorldPoint(Vector3 localPoint)
    {
        return arenaRoot != null ? arenaRoot.transform.TransformPoint(localPoint) : localPoint;
    }

    void FrameAttackerBeat(int dogAImpact, int dogBImpact, Vector3 fighterALocalPosition, Vector3 fighterBLocalPosition)
    {
        if (dogAImpact <= 0 && dogBImpact <= 0)
        {
            return;
        }

        if (dogAImpact > 0 && dogBImpact > 0)
        {
            FrameClashPoint((fighterALocalPosition + fighterBLocalPosition) * 0.5f);
            return;
        }

        bool dogAIsAttacker = dogAImpact >= dogBImpact;
        Vector3 attackerLocalPosition = dogAIsAttacker ? fighterALocalPosition : fighterBLocalPosition;
        Vector3 targetLocalPosition = dogAIsAttacker ? fighterBLocalPosition : fighterALocalPosition;
        Vector3 focusLocalPosition = Vector3.Lerp(attackerLocalPosition, targetLocalPosition, 0.42f);
        Vector3 baseCameraPosition = GetArenaCameraPosition();
        Vector3 targetCameraPosition = baseCameraPosition + new Vector3(Mathf.Clamp(focusLocalPosition.x * 0.16f, -0.42f, 0.42f), -0.08f, 0.36f);
        Vector3 lookAtPosition = GetArenaWorldPoint(new Vector3(Mathf.Clamp(focusLocalPosition.x * 0.55f, -0.9f, 0.9f), 0.82f, focusLocalPosition.z + 0.08f));

        StartCameraBeat(targetCameraPosition, lookAtPosition, 0.16f, 0.12f, 0.22f);
    }

    void FrameClashPoint(Vector3 clashLocalPosition)
    {
        Vector3 baseCameraPosition = GetArenaCameraPosition();
        Vector3 targetCameraPosition = baseCameraPosition + new Vector3(Mathf.Clamp(clashLocalPosition.x * 0.12f, -0.25f, 0.25f), -0.12f, 0.5f);
        Vector3 lookAtPosition = GetArenaWorldPoint(new Vector3(Mathf.Clamp(clashLocalPosition.x * 0.35f, -0.55f, 0.55f), 0.84f, clashLocalPosition.z + 0.05f));

        StartCameraBeat(targetCameraPosition, lookAtPosition, 0.14f, 0.14f, 0.24f);
    }

    void FrameWinner(Transform winnerTransform, bool isDraw, string bannerText)
    {
        SetRoundStatusBannerText(bannerText);
        StopCameraBeatIfRunning();
        StopCinematicCameraIfRunning();
        cinematicCameraCoroutine = StartCoroutine(ResultCinematicRoutine(winnerTransform, isDraw));
    }

    void StartCameraBeat(Vector3 targetPosition, Vector3 lookAtPosition, float moveInDuration, float holdDuration, float returnDuration)
    {
        EnsurePresentationCamera();

        if (presentationCamera == null || presentationCameraObject == null)
        {
            return;
        }

        StopCameraBeatIfRunning();
        StopCameraMoveIfRunning();
        cameraBeatCoroutine = StartCoroutine(CameraBeatRoutine(targetPosition, lookAtPosition, moveInDuration, holdDuration, returnDuration));
    }

    IEnumerator CameraBeatRoutine(Vector3 targetPosition, Vector3 lookAtPosition, float moveInDuration, float holdDuration, float returnDuration)
    {
        Vector3 startPosition = presentationCameraObject.transform.position;

        yield return MoveCameraBetweenPoints(startPosition, targetPosition, lookAtPosition, moveInDuration);

        if (holdDuration > 0f)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        yield return MoveCameraBetweenPoints(targetPosition, GetArenaCameraPosition(), GetArenaLookAtPosition(), returnDuration);
        cameraBeatCoroutine = null;
    }

    IEnumerator MoveCameraBetweenPoints(Vector3 startPosition, Vector3 targetPosition, Vector3 lookAtPosition, float duration)
    {
        if (duration <= 0f)
        {
            presentationCameraObject.transform.position = targetPosition;
            presentationCameraObject.transform.LookAt(lookAtPosition);
            FacePortraitsTowardPresentationCamera();
            yield break;
        }

        float elapsedTime = 0f;

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
    }

    void StopCameraBeatIfRunning()
    {
        if (cameraBeatCoroutine == null)
        {
            return;
        }

        StopCoroutine(cameraBeatCoroutine);
        cameraBeatCoroutine = null;
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

    void PlayCameraPunchAndShake(int severity)
    {
        if (severity < 2)
        {
            return;
        }

        StopCameraBeatIfRunning();
        StopCinematicCameraIfRunning();
        cinematicCameraCoroutine = StartCoroutine(CameraPunchAndShakeRoutine(severity));
    }

    IEnumerator CameraPunchAndShakeRoutine(int severity)
    {
        EnsurePresentationCamera();

        if (presentationCamera == null || presentationCameraObject == null)
        {
            cinematicCameraCoroutine = null;
            yield break;
        }

        StopCameraMoveIfRunning();

        Vector3 basePosition = GetArenaCameraPosition();
        Vector3 lookAtPosition = GetArenaLookAtPosition();
        Vector3 forward = (lookAtPosition - basePosition).normalized;
        float severityPercent = Mathf.InverseLerp(2f, 3f, severity);
        float punchDistance = Mathf.Lerp(0.08f, 0.22f, severityPercent);
        float shakeDistance = Mathf.Lerp(0.006f, 0.026f, severityPercent);
        float duration = Mathf.Lerp(0.12f, 0.2f, severityPercent);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float punchCurve = Mathf.Sin(progress * Mathf.PI);
            float fade = 1f - progress;
            Vector3 shakeOffset = new Vector3(
                Mathf.Sin(elapsedTime * 38f) * shakeDistance * fade,
                Mathf.Cos(elapsedTime * 31f) * shakeDistance * fade,
                0f
            );

            presentationCameraObject.transform.position = basePosition + (forward * punchDistance * punchCurve) + shakeOffset;
            presentationCameraObject.transform.LookAt(lookAtPosition);
            FacePortraitsTowardPresentationCamera();
            yield return null;
        }

        cinematicCameraCoroutine = null;
        SetPresentationCameraInstant(basePosition, lookAtPosition);
    }

    void PlayResultCinematic(Transform focusTransform, bool isDraw, string bannerText)
    {
        FrameWinner(focusTransform, isDraw, bannerText);
    }

    IEnumerator ResultCinematicRoutine(Transform focusTransform, bool isDraw)
    {
        EnsurePresentationCamera();

        if (presentationCamera == null || presentationCameraObject == null)
        {
            cinematicCameraCoroutine = null;
            yield break;
        }

        StopCameraMoveIfRunning();

        Vector3 startPosition = presentationCameraObject.transform.position;
        Vector3 focusPosition = isDraw || focusTransform == null
            ? GetArenaLookAtPosition()
            : focusTransform.position + new Vector3(0f, 0.7f, 0f);
        Vector3 targetPosition = new Vector3(Mathf.Clamp(focusPosition.x * 0.28f, -0.7f, 0.7f), 3.45f, -6.15f);
        float duration = 0.28f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float smoothProgress = progress * progress * (3f - (2f * progress));

            presentationCameraObject.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);
            presentationCameraObject.transform.LookAt(focusPosition);
            FacePortraitsTowardPresentationCamera();
            yield return null;
        }

        yield return new WaitForSeconds(0.22f);

        cinematicCameraCoroutine = null;
        ReturnToFightWide();
    }

    void StopCinematicCameraIfRunning()
    {
        if (cinematicCameraCoroutine == null)
        {
            return;
        }

        StopCoroutine(cinematicCameraCoroutine);
        cinematicCameraCoroutine = null;
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
            StopCameraBeatIfRunning();
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
            fighterATransform = CreateFighterPlaceholder("FighterA_Imprint", new Vector3(-1.75f, 0.6f, 0f), new Color(0f, 0.58f, 0.78f)).transform;
        }

        if (fighterBTransform == null)
        {
            fighterBTransform = CreateFighterPlaceholder("FighterB_Imprint", new Vector3(1.75f, 0.6f, 0f), new Color(0.78f, 0.1f, 0.68f)).transform;
        }

        CreateDogImprintArtIfNeeded();
        CreateContactShadowsIfNeeded();
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

        Transform dogImprintArtA = arenaRoot.transform.Find("FighterA_DogImprintArt");
        Transform dogImprintArtB = arenaRoot.transform.Find("FighterB_DogImprintArt");
        Transform breedArchetypeArtA = arenaRoot.transform.Find("FighterA_BreedArchetypeArt");
        Transform breedArchetypeArtB = arenaRoot.transform.Find("FighterB_BreedArchetypeArt");

        if (dogImprintArtA != null)
        {
            fighterADogImprintArt = dogImprintArtA.gameObject;
        }

        if (dogImprintArtB != null)
        {
            fighterBDogImprintArt = dogImprintArtB.gameObject;
        }

        if (breedArchetypeArtA != null)
        {
            fighterABreedArchetypeArt = breedArchetypeArtA.gameObject;
        }

        if (breedArchetypeArtB != null)
        {
            fighterBBreedArchetypeArt = breedArchetypeArtB.gameObject;
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

    void SetCurrentDogImprintIdentity(Dog dogA, Dog dogB)
    {
        currentDogImprintA = dogA;
        currentDogImprintB = dogB;
    }

    void LoadDogImprintPrefabIfNeeded()
    {
        if (attemptedDogImprintLoad)
        {
            return;
        }

        attemptedDogImprintLoad = true;
        dogImprintPrefab = Resources.Load<GameObject>(DogImprintResourcePath);

        if (dogImprintPrefab == null && !warnedMissingDogImprintPrefab)
        {
            Debug.LogWarning($"FightPresentationManager could not load Resources/{DogImprintResourcePath}. Capsules will remain as fallback fighters.");
            warnedMissingDogImprintPrefab = true;
        }
    }

    void CreateDogImprintArtIfNeeded()
    {
        if (arenaRoot == null)
        {
            return;
        }

        LoadDogImprintPrefabIfNeeded();
        CreateBreedArchetypeArtIfNeeded();

        if (dogImprintPrefab == null)
        {
            SetCapsuleFallbackVisible(true);
            UpdateBreedArchetypeArtPositions();
            return;
        }

        if (fighterADogImprintArt == null)
        {
            fighterADogImprintArt = CreateSingleDogImprintArt("FighterA_DogImprintArt", true);
        }

        if (fighterBDogImprintArt == null)
        {
            fighterBDogImprintArt = CreateSingleDogImprintArt("FighterB_DogImprintArt", false);
        }

        SetCapsuleFallbackVisible(false);
        SetDogImprintArtVisible(true);
        UpdateDogImprintArtPositions();
    }

    void CreateBreedArchetypeArtIfNeeded()
    {
        if (arenaRoot == null)
        {
            return;
        }

        if (fighterABreedArchetypeArt == null)
        {
            fighterABreedArchetypeArt = CreateSingleBreedArchetypeArt("FighterA_BreedArchetypeArt");
        }

        if (fighterBBreedArchetypeArt == null)
        {
            fighterBBreedArchetypeArt = CreateSingleBreedArchetypeArt("FighterB_BreedArchetypeArt");
        }

        UpdateBreedArchetypeArtPositions();
        UpdateFighterContactShadows();
    }

    GameObject CreateSingleBreedArchetypeArt(string objectName)
    {
        Transform existingArt = arenaRoot.transform.Find(objectName);
        GameObject artObject;

        if (existingArt != null)
        {
            artObject = existingArt.gameObject;
        }
        else
        {
            artObject = new GameObject(objectName);
            artObject.transform.SetParent(arenaRoot.transform);
        }

        artObject.hideFlags = HideFlags.DontSave;
        artObject.transform.localRotation = Quaternion.identity;
        artObject.transform.localScale = Vector3.one;
        GetBreedArchetypeSpriteRenderer(artObject);
        GetBreedArchetypeTextureQuad(artObject);
        artObject.SetActive(false);
        return artObject;
    }

    GameObject CreateSingleDogImprintArt(string objectName, bool isFighterA)
    {
        Transform existingArt = arenaRoot.transform.Find(objectName);
        GameObject artObject;

        if (existingArt != null)
        {
            artObject = existingArt.gameObject;
        }
        else
        {
            artObject = Instantiate(dogImprintPrefab);
            artObject.name = objectName;
            artObject.transform.SetParent(arenaRoot.transform);
        }

        PrepareDogImprintArtObject(artObject);
        UpdateSingleDogImprintArtPosition(artObject, isFighterA ? fighterATransform : fighterBTransform, isFighterA);
        return artObject;
    }

    void PrepareDogImprintArtObject(GameObject artObject)
    {
        if (artObject == null)
        {
            return;
        }

        artObject.hideFlags = HideFlags.DontSave;

        foreach (Transform child in artObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.hideFlags = HideFlags.DontSave;
        }

        foreach (Collider artCollider in artObject.GetComponentsInChildren<Collider>(true))
        {
            artCollider.enabled = false;
        }
    }

    void UpdateDogImprintArtPositions()
    {
        if (dogImprintPrefab != null)
        {
            UpdateSingleDogImprintArtPosition(fighterADogImprintArt, fighterATransform, true);
            UpdateSingleDogImprintArtPosition(fighterBDogImprintArt, fighterBTransform, false);
        }

        UpdateBreedArchetypeArtPositions();
        UpdateFighterContactShadows();
    }

    void UpdateSingleDogImprintArtPosition(GameObject artObject, Transform fighterTransform, bool isFighterA)
    {
        if (artObject == null || fighterTransform == null)
        {
            return;
        }

        Dog dog = isFighterA ? currentDogImprintA : currentDogImprintB;
        BreedVisualArchetype archetype = ResolveBreedVisualArchetype(dog);

        artObject.transform.localPosition = fighterTransform.localPosition + GetDogImprintArtOffset(archetype);
        artObject.transform.localRotation = Quaternion.Euler(0f, isFighterA ? 90f : -90f, 0f);
        artObject.transform.localScale = Vector3.Scale(GetDogImprintBaseScale(), GetBreedArchetypeScaleModifier(archetype));
        SetDogArtFacing(artObject, !isFighterA);
        artObject.SetActive(true);
        GroundAlignDogArt(artObject, DogArtGroundY);
    }

    Vector3 GetDogImprintArtOffset(BreedVisualArchetype archetype)
    {
        return new Vector3(0f, DogImprintFallbackVerticalOffset + GetDogArtVerticalOffset(archetype), 0f);
    }

    Vector3 GetDogImprintBaseScale()
    {
        return new Vector3(0.82f, 0.82f, 0.82f);
    }

    void SetCapsuleFallbackVisible(bool visible)
    {
        SetFighterCapsuleVisible(fighterATransform, visible);
        SetFighterCapsuleVisible(fighterBTransform, visible);
    }

    void SetFighterCapsuleVisible(Transform fighterTransform, bool visible)
    {
        if (fighterTransform != null)
        {
            fighterTransform.gameObject.SetActive(visible);
        }
    }

    void SetDogImprintArtVisible(bool visible)
    {
        if (fighterADogImprintArt != null)
        {
            fighterADogImprintArt.SetActive(visible);
        }

        if (fighterBDogImprintArt != null)
        {
            fighterBDogImprintArt.SetActive(visible);
        }
    }

    void SetBreedArchetypeArtVisible(bool visible)
    {
        SetBreedArchetypeArtVisible(fighterABreedArchetypeArt, visible);
        SetBreedArchetypeArtVisible(fighterBBreedArchetypeArt, visible);
    }

    void SetBreedArchetypeArtVisible(GameObject artObject, bool visible)
    {
        if (artObject != null)
        {
            artObject.SetActive(visible);
        }
    }

    void UpdateBreedArchetypeArtPositions()
    {
        UpdateSingleBreedArchetypeArtPosition(fighterABreedArchetypeArt, fighterATransform, currentDogImprintA);
        UpdateSingleBreedArchetypeArtPosition(fighterBBreedArchetypeArt, fighterBTransform, currentDogImprintB);
        FaceBreedArchetypeArtsTowardPresentationCamera();
        UpdateFighterFacingDirections();
    }

    void UpdateSingleBreedArchetypeArtPosition(GameObject artObject, Transform fighterTransform, Dog dog)
    {
        if (artObject == null || fighterTransform == null)
        {
            return;
        }

        BreedVisualArchetype archetype = ResolveBreedVisualArchetype(dog);
        SetDogArtLocalPresentationTransform(artObject.transform, fighterTransform, archetype);
    }

    Vector3 GetBreedArchetypeArtOffset(BreedVisualArchetype archetype)
    {
        return new Vector3(0f, GetDogArtVerticalOffset(archetype), BreedArchetypeArtForwardOffset);
    }

    Vector3 GetBreedArchetypeArtRootScale(BreedVisualArchetype archetype)
    {
        return Vector3.Scale(Vector3.one * BreedArchetypeArtBaseScale, GetBreedArchetypeScaleModifier(archetype));
    }

    void SetDogArtLocalPresentationTransform(Transform artTransform, Transform fighterTransform, BreedVisualArchetype archetype)
    {
        if (artTransform == null || fighterTransform == null)
        {
            return;
        }

        Vector3 localPosition = fighterTransform.localPosition + GetBreedArchetypeArtOffset(archetype);
        localPosition.y = DogArtGroundY;

        artTransform.localPosition = localPosition;
        artTransform.localRotation = Quaternion.identity;
        artTransform.localScale = GetBreedArchetypeArtRootScale(archetype);
    }

    void ApplyDogArtPresentationTuning(GameObject artObject, Transform visualTransform, BreedVisualArchetype archetype)
    {
        if (artObject == null || visualTransform == null)
        {
            return;
        }

        visualTransform.localPosition += new Vector3(0f, GetDogArtVerticalOffset(archetype), 0f);
    }

    float GetDogArtVerticalOffset(BreedVisualArchetype archetype)
    {
        switch (archetype)
        {
            case BreedVisualArchetype.GuardMastiff:
                return -0.01f;

            case BreedVisualArchetype.IronRott:
                return -0.005f;

            case BreedVisualArchetype.SpitzWarden:
                return 0.025f;

            case BreedVisualArchetype.VelocityHound:
                return 0.02f;

            case BreedVisualArchetype.BullyStriker:
                return -0.015f;

            case BreedVisualArchetype.HybridVariant:
            case BreedVisualArchetype.ShepherdSentinel:
            case BreedVisualArchetype.Unknown:
            default:
                return 0f;
        }
    }

    Vector3 GetBreedArchetypeScaleModifier(BreedVisualArchetype archetype)
    {
        switch (archetype)
        {
            case BreedVisualArchetype.GuardMastiff:
                return new Vector3(1.12f, 1.08f, 1.08f);

            case BreedVisualArchetype.IronRott:
                return new Vector3(1.08f, 1.02f, 1.04f);

            case BreedVisualArchetype.BullyStriker:
                return new Vector3(1.07f, 0.98f, 1.04f);

            case BreedVisualArchetype.SpitzWarden:
                return new Vector3(0.96f, 1.06f, 0.98f);

            case BreedVisualArchetype.VelocityHound:
                return new Vector3(0.86f, 1.06f, 0.9f);

            case BreedVisualArchetype.HybridVariant:
                return new Vector3(1.02f, 1.02f, 1.02f);

            case BreedVisualArchetype.ShepherdSentinel:
            case BreedVisualArchetype.Unknown:
            default:
                return Vector3.one;
        }
    }

    void CreateContactShadowsIfNeeded()
    {
        if (arenaRoot == null)
        {
            return;
        }

        if (fighterAContactShadow == null)
        {
            fighterAContactShadow = CreateFighterContactShadow("FighterA_ContactShadow");
        }

        if (fighterBContactShadow == null)
        {
            fighterBContactShadow = CreateFighterContactShadow("FighterB_ContactShadow");
        }
    }

    GameObject CreateFighterContactShadow(string objectName)
    {
        Transform existingShadow = arenaRoot.transform.Find(objectName);
        GameObject shadowObject;

        if (existingShadow != null)
        {
            shadowObject = existingShadow.gameObject;
        }
        else
        {
            shadowObject = new GameObject(objectName);
            shadowObject.transform.SetParent(arenaRoot.transform);
        }

        shadowObject.hideFlags = HideFlags.DontSave;

        MeshFilter shadowFilter = shadowObject.GetComponent<MeshFilter>();

        if (shadowFilter == null)
        {
            shadowFilter = shadowObject.AddComponent<MeshFilter>();
        }

        shadowFilter.sharedMesh = EnsureContactShadowMesh();

        MeshRenderer shadowRenderer = shadowObject.GetComponent<MeshRenderer>();

        if (shadowRenderer == null)
        {
            shadowRenderer = shadowObject.AddComponent<MeshRenderer>();
        }

        shadowRenderer.sharedMaterial = EnsureContactShadowMaterial();
        shadowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        shadowRenderer.receiveShadows = false;
        shadowRenderer.allowOcclusionWhenDynamic = false;

        Collider shadowCollider = shadowObject.GetComponent<Collider>();

        if (shadowCollider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(shadowCollider);
            }
            else
            {
                DestroyImmediate(shadowCollider);
            }
        }

        shadowObject.transform.localRotation = Quaternion.identity;
        shadowObject.SetActive(false);
        return shadowObject;
    }

    Mesh EnsureContactShadowMesh()
    {
        if (contactShadowMesh != null)
        {
            return contactShadowMesh;
        }

        int segmentCount = Mathf.Max(12, ContactShadowSegmentCount);
        Vector3[] vertices = new Vector3[segmentCount + 1];
        Vector2[] uvs = new Vector2[segmentCount + 1];
        int[] triangles = new int[segmentCount * 3];

        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < segmentCount; i++)
        {
            float angle = (Mathf.PI * 2f * i) / segmentCount;
            float x = Mathf.Cos(angle) * 0.5f;
            float z = Mathf.Sin(angle) * 0.5f;

            vertices[i + 1] = new Vector3(x, 0f, z);
            uvs[i + 1] = new Vector2(0.5f + x, 0.5f + z);

            int nextVertex = i == segmentCount - 1 ? 1 : i + 2;
            int triangleIndex = i * 3;

            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = nextVertex;
            triangles[triangleIndex + 2] = i + 1;
        }

        contactShadowMesh = new Mesh();
        contactShadowMesh.name = "RuntimeContactShadowEllipse";
        contactShadowMesh.hideFlags = HideFlags.DontSave;
        contactShadowMesh.vertices = vertices;
        contactShadowMesh.uv = uvs;
        contactShadowMesh.triangles = triangles;
        contactShadowMesh.RecalculateNormals();
        contactShadowMesh.RecalculateBounds();
        return contactShadowMesh;
    }

    Material EnsureContactShadowMaterial()
    {
        if (contactShadowMaterial != null)
        {
            return contactShadowMaterial;
        }

        Shader shadowShader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shadowShader == null)
        {
            shadowShader = Shader.Find("Unlit/Transparent");
        }

        if (shadowShader == null)
        {
            shadowShader = Shader.Find("Sprites/Default");
        }

        if (shadowShader == null)
        {
            shadowShader = Shader.Find("Unlit/Color");
        }

        if (shadowShader == null)
        {
            shadowShader = Shader.Find("Standard");
        }

        if (shadowShader == null)
        {
            return null;
        }

        contactShadowMaterial = new Material(shadowShader);

        contactShadowMaterial.name = "RuntimeContactShadowMaterial";
        contactShadowMaterial.hideFlags = HideFlags.DontSave;
        Color shadowColor = new Color(0.06f, 0.18f, 0.22f, 0.5f);

        contactShadowMaterial.color = shadowColor;

        if (contactShadowMaterial.HasProperty("_BaseColor"))
        {
            contactShadowMaterial.SetColor("_BaseColor", shadowColor);
        }

        if (contactShadowMaterial.HasProperty("_Color"))
        {
            contactShadowMaterial.SetColor("_Color", shadowColor);
        }

        ConfigureContactShadowMaterialForTransparency(contactShadowMaterial);
        return contactShadowMaterial;
    }

    void ConfigureContactShadowMaterialForTransparency(Material shadowMaterial)
    {
        if (shadowMaterial == null)
        {
            return;
        }

        if (shadowMaterial.HasProperty("_Surface"))
        {
            shadowMaterial.SetFloat("_Surface", 1f);
        }

        if (shadowMaterial.HasProperty("_Blend"))
        {
            shadowMaterial.SetFloat("_Blend", 0f);
        }

        if (shadowMaterial.HasProperty("_AlphaClip"))
        {
            shadowMaterial.SetFloat("_AlphaClip", 0f);
        }

        if (shadowMaterial.HasProperty("_SrcBlend"))
        {
            shadowMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        }

        if (shadowMaterial.HasProperty("_DstBlend"))
        {
            shadowMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }

        if (shadowMaterial.HasProperty("_ZWrite"))
        {
            shadowMaterial.SetFloat("_ZWrite", 0f);
        }

        shadowMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        shadowMaterial.EnableKeyword("_ALPHABLEND_ON");
        shadowMaterial.DisableKeyword("_ALPHATEST_ON");
        shadowMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    void UpdateFighterContactShadows()
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreateContactShadowsIfNeeded();
        UpdateFighterContactShadow(fighterAContactShadow, fighterATransform, currentDogImprintA);
        UpdateFighterContactShadow(fighterBContactShadow, fighterBTransform, currentDogImprintB);
    }

    void UpdateFighterContactShadow(GameObject shadowObject, Transform fighterTransform, Dog dog)
    {
        if (shadowObject == null || fighterTransform == null)
        {
            return;
        }

        BreedVisualArchetype archetype = ResolveBreedVisualArchetype(dog);
        Vector3 fighterPosition = fighterTransform.localPosition;
        shadowObject.transform.localPosition = new Vector3(
            fighterPosition.x,
            ContactShadowY,
            fighterPosition.z + GetContactShadowForwardOffset(archetype)
        );
        shadowObject.transform.localRotation = Quaternion.identity;
        shadowObject.transform.localScale = GetContactShadowScaleForArchetype(archetype);
        shadowObject.SetActive(true);
    }

    float GetContactShadowForwardOffset(BreedVisualArchetype archetype)
    {
        switch (archetype)
        {
            case BreedVisualArchetype.GuardMastiff:
            case BreedVisualArchetype.IronRott:
                return ContactShadowForwardOffset - 0.02f;

            case BreedVisualArchetype.VelocityHound:
                return ContactShadowForwardOffset - 0.04f;

            case BreedVisualArchetype.BullyStriker:
            case BreedVisualArchetype.SpitzWarden:
            case BreedVisualArchetype.HybridVariant:
            case BreedVisualArchetype.ShepherdSentinel:
            case BreedVisualArchetype.Unknown:
            default:
                return ContactShadowForwardOffset;
        }
    }

    Vector3 GetContactShadowScaleForArchetype(BreedVisualArchetype archetype)
    {
        switch (archetype)
        {
            case BreedVisualArchetype.GuardMastiff:
                return new Vector3(1.46f, 1f, 0.66f);

            case BreedVisualArchetype.IronRott:
                return new Vector3(1.34f, 1f, 0.58f);

            case BreedVisualArchetype.VelocityHound:
                return new Vector3(1.42f, 1f, 0.42f);

            case BreedVisualArchetype.BullyStriker:
                return new Vector3(1.28f, 1f, 0.54f);

            case BreedVisualArchetype.SpitzWarden:
                return new Vector3(1.12f, 1f, 0.5f);

            case BreedVisualArchetype.HybridVariant:
                return new Vector3(1.24f, 1f, 0.52f);

            case BreedVisualArchetype.ShepherdSentinel:
                return new Vector3(1.2f, 1f, 0.52f);

            case BreedVisualArchetype.Unknown:
            default:
                return new Vector3(1.15f, 1f, 0.5f);
        }
    }

    void SetContactShadowVisible(bool visible)
    {
        if (fighterAContactShadow != null)
        {
            fighterAContactShadow.SetActive(visible);
        }

        if (fighterBContactShadow != null)
        {
            fighterBContactShadow.SetActive(visible);
        }
    }

    void ApplyBreedArchetypeArtToFighter(GameObject artObject, Dog dog, bool isFighterA, int currentHealth, int maxLikelyHealth)
    {
        if (artObject == null || dog == null)
        {
            SetBreedArchetypeArtVisible(artObject, false);
            return;
        }

        UpdateSingleBreedArchetypeArtPosition(artObject, isFighterA ? fighterATransform : fighterBTransform, dog);

        List<string> resourceNames = GetAvailableBreedArtResourceNames(dog);

        foreach (string resourceName in resourceNames)
        {
            if (TryLoadBreedArchetypeSpriteOrTexture(resourceName, out Sprite sprite, out Texture2D texture))
            {
                if (sprite != null)
                {
                    ConfigureBreedArchetypeSpriteArt(artObject, sprite, dog, isFighterA, currentHealth, maxLikelyHealth);
                }
                else
                {
                    ConfigureBreedArchetypeTextureArt(artObject, texture, dog, isFighterA, currentHealth, maxLikelyHealth);
                }

                SetBreedArchetypeArtVisible(artObject, true);
                FaceBreedArchetypeArtTowardPresentationCamera(artObject);
                SetDogArtFacing(artObject, !isFighterA);
                return;
            }
        }

        SetBreedArchetypeArtVisible(artObject, false);
        WarnMissingBreedArtIfNeeded(dog, resourceNames);
    }

    List<string> GetAvailableBreedArtResourceNames(Dog dog)
    {
        List<string> resourceNames = new List<string>();
        string breedText = GetDogBreedText(dog);
        bool isHybridBreed = IsHybridBreedText(breedText);
        BreedVisualArchetype resolvedArchetype = ResolveBreedVisualArchetype(dog);
        BreedVisualArchetype breedFamilyArchetype = GetBreedArtFamilyArchetype(breedText);

        if (IsShepherdBullyHybridText(breedText))
        {
            AddBreedArtResourceName(resourceNames, "dog_imprint_shepherd_hybrid_variant_01");
            AddBreedArtResourceName(resourceNames, "dog_imprint_bully_hybrid_variant_01");
            AddBreedArtResourceName(resourceNames, "dog_imprint_hybrid_variant_01");
            return resourceNames;
        }

        if (isHybridBreed)
        {
            AddBreedArtResourceName(resourceNames, GetHybridSpecificResourceName(breedText));

            if (breedFamilyArchetype == BreedVisualArchetype.BullyStriker ||
                breedFamilyArchetype == BreedVisualArchetype.ShepherdSentinel)
            {
                AddBreedArtVariantNames(resourceNames, breedFamilyArchetype, dog);
                return resourceNames;
            }

            if (breedFamilyArchetype != BreedVisualArchetype.Unknown &&
                breedFamilyArchetype != BreedVisualArchetype.HybridVariant)
            {
                AddBreedArtVariantNames(resourceNames, breedFamilyArchetype, dog);
                return resourceNames;
            }

            AddBreedArtResourceName(resourceNames, "dog_imprint_hybrid_variant_01");
            return resourceNames;
        }

        if (resolvedArchetype == BreedVisualArchetype.HybridVariant)
        {
            AddBreedArtResourceName(resourceNames, "dog_imprint_hybrid_variant_01");
            return resourceNames;
        }

        AddBreedArtVariantNames(resourceNames, resolvedArchetype, dog);
        return resourceNames;
    }

    void AddBreedArtVariantNames(List<string> resourceNames, BreedVisualArchetype archetype, Dog dog)
    {
        string baseName = GetBreedArchetypeResourceBaseName(archetype);

        if (string.IsNullOrEmpty(baseName))
        {
            return;
        }

        int firstVariant = GetDeterministicVariantIndex(dog);
        int secondVariant = firstVariant == 1 ? 2 : 1;
        AddBreedArtResourceName(resourceNames, $"{baseName}_{firstVariant:00}");
        AddBreedArtResourceName(resourceNames, $"{baseName}_{secondVariant:00}");
    }

    void AddBreedArtResourceName(List<string> resourceNames, string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName) || resourceNames.Contains(resourceName))
        {
            return;
        }

        resourceNames.Add(resourceName);
    }

    string GetBreedArchetypeResourceBaseName(BreedVisualArchetype archetype)
    {
        switch (archetype)
        {
            case BreedVisualArchetype.ShepherdSentinel:
                return "dog_imprint_shepherd_sentinel";

            case BreedVisualArchetype.BullyStriker:
                return "dog_imprint_bully_striker";

            case BreedVisualArchetype.GuardMastiff:
                return "dog_imprint_guard_mastiff";

            case BreedVisualArchetype.IronRott:
                return "dog_imprint_iron_rott";

            case BreedVisualArchetype.SpitzWarden:
                return "dog_imprint_spitz_warden";

            case BreedVisualArchetype.VelocityHound:
                return "dog_imprint_velocity_hound";

            case BreedVisualArchetype.HybridVariant:
                return "dog_imprint_hybrid_variant";

            case BreedVisualArchetype.Unknown:
            default:
                return string.Empty;
        }
    }

    string GetHybridSpecificResourceName(string breedText)
    {
        if (!IsHybridBreedText(breedText))
        {
            return string.Empty;
        }

        if (IsShepherdBullyHybridText(breedText))
        {
            return "dog_imprint_shepherd_hybrid_variant_01";
        }

        if (ContainsBullyBreedText(breedText))
        {
            return "dog_imprint_bully_hybrid_variant_01";
        }

        if (ContainsShepherdBreedText(breedText))
        {
            return "dog_imprint_shepherd_hybrid_variant_01";
        }

        return string.Empty;
    }

    BreedVisualArchetype GetBreedArtFamilyArchetype(string breedText)
    {
        if (ContainsBullyBreedText(breedText))
        {
            return BreedVisualArchetype.BullyStriker;
        }

        if (ContainsShepherdBreedText(breedText))
        {
            return BreedVisualArchetype.ShepherdSentinel;
        }

        return ResolveBreedVisualArchetypeFromBreedText(breedText, false);
    }

    bool ContainsBullyBreedText(string breedText)
    {
        if (string.IsNullOrWhiteSpace(breedText))
        {
            return false;
        }

        string rawBreed = GetRawNormalizedBreedText(breedText);
        string separatorNormalizedBreed = GetSeparatorNormalizedBreedText(breedText);
        string compactBreed = GetCompactBreedText(breedText);

        return separatorNormalizedBreed.Contains("pit bull") ||
               compactBreed.Contains("pitbull") ||
               rawBreed.Contains("boxer") ||
               compactBreed.Contains("boxer") ||
               rawBreed.Contains("bully") ||
               compactBreed.Contains("bully");
    }

    bool ContainsShepherdBreedText(string breedText)
    {
        if (string.IsNullOrWhiteSpace(breedText))
        {
            return false;
        }

        string rawBreed = GetRawNormalizedBreedText(breedText);
        string separatorNormalizedBreed = GetSeparatorNormalizedBreedText(breedText);
        string compactBreed = GetCompactBreedText(breedText);

        return separatorNormalizedBreed.Contains("german shepherd") ||
               separatorNormalizedBreed.Contains("german shepard") ||
               separatorNormalizedBreed.Contains("belgian malinois") ||
               rawBreed.Contains("shepherd") ||
               rawBreed.Contains("shepard") ||
               compactBreed.Contains("shepherd") ||
               compactBreed.Contains("shepard") ||
               rawBreed.Contains("malinois") ||
               compactBreed.Contains("malinois") ||
               compactBreed.Contains("german");
    }

    bool IsShepherdBullyHybridText(string breedText)
    {
        if (string.IsNullOrWhiteSpace(breedText))
        {
            return false;
        }

        string compactBreed = GetCompactBreedText(breedText);

        if (string.IsNullOrEmpty(compactBreed))
        {
            return false;
        }

        return compactBreed.Contains("germanbull") ||
               compactBreed.Contains("germanbully") ||
               compactBreed.Contains("shepherdbull") ||
               compactBreed.Contains("shepherdbully") ||
               compactBreed.Contains("pitgerman") ||
               compactBreed.Contains("pitshepherd") ||
               compactBreed.Contains("bullshepherd") ||
               compactBreed.Contains("bullyshepherd") ||
               (ContainsShepherdBreedText(breedText) && ContainsBullyBreedText(breedText) && IsHybridBreedText(breedText));
    }

    int GetDeterministicVariantIndex(Dog dog)
    {
        string key = $"{GetDogDisplayName(dog, "dog")}|{GetDogBreedText(dog)}";
        return GetStableNameHash01(key) < 0.5f ? 1 : 2;
    }

    bool TryLoadBreedArchetypeSpriteOrTexture(string resourceName, out Sprite sprite, out Texture2D texture)
    {
        sprite = null;
        texture = null;

        if (string.IsNullOrEmpty(resourceName))
        {
            return false;
        }

        string resourcePath = BreedArchetypeResourceFolder + resourceName;
        sprite = Resources.Load<Sprite>(resourcePath);

        if (sprite != null)
        {
            return true;
        }

        texture = Resources.Load<Texture2D>(resourcePath);
        return texture != null;
    }

    void WarnMissingBreedArtIfNeeded(Dog dog, List<string> attemptedResourceNames)
    {
        string dogName = GetDogDisplayName(dog, "dog");
        string rawBreedText = GetDogBreedText(dog);
        string compactBreedText = GetCompactBreedText(rawBreedText);
        string warningKey = $"{dogName}|{rawBreedText}|{compactBreedText}";

        if (warnedMissingBreedArtKeys.Contains(warningKey))
        {
            return;
        }

        warnedMissingBreedArtKeys.Add(warningKey);

        string attemptedResources = attemptedResourceNames != null && attemptedResourceNames.Count > 0
            ? string.Join(", ", attemptedResourceNames)
            : "none";

        Debug.LogWarning($"FightPresentationManager could not resolve breed art for {dogName}. Raw breed: '{rawBreedText}'. Compact breed: '{compactBreedText}'. Attempted resources: {attemptedResources}.");
    }

    void ConfigureBreedArchetypeSpriteArt(GameObject artObject, Sprite sprite, Dog dog, bool isFighterA, int currentHealth, int maxLikelyHealth)
    {
        SpriteRenderer spriteRenderer = GetBreedArchetypeSpriteRenderer(artObject);
        GameObject textureQuad = GetBreedArchetypeTextureQuad(artObject);

        if (spriteRenderer == null)
        {
            return;
        }

        if (textureQuad != null)
        {
            textureQuad.SetActive(false);
        }

        spriteRenderer.enabled = true;
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = GetBreedArchetypeArtTint(dog, isFighterA, currentHealth, maxLikelyHealth);
        SetSpriteFacing(spriteRenderer, !isFighterA);
        spriteRenderer.sortingOrder = 460;
        spriteRenderer.transform.localPosition = Vector3.zero;
        spriteRenderer.transform.localRotation = Quaternion.identity;
        spriteRenderer.transform.localScale = GetBreedArchetypeSpriteScale(sprite);
        GroundAlignDogArt(spriteRenderer);
        ApplyDogArtPresentationTuning(artObject, spriteRenderer.transform, ResolveBreedVisualArchetype(dog));

        Material runtimeMaterial = GetPortraitSpriteMaterial();

        if (runtimeMaterial != null)
        {
            spriteRenderer.material = runtimeMaterial;
        }
    }

    void ConfigureBreedArchetypeTextureArt(GameObject artObject, Texture2D texture, Dog dog, bool isFighterA, int currentHealth, int maxLikelyHealth)
    {
        SpriteRenderer spriteRenderer = GetBreedArchetypeSpriteRenderer(artObject);
        GameObject textureQuad = GetBreedArchetypeTextureQuad(artObject);

        if (textureQuad == null || texture == null)
        {
            return;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        textureQuad.SetActive(true);
        textureQuad.transform.localPosition = Vector3.zero;
        textureQuad.transform.localRotation = Quaternion.identity;
        textureQuad.transform.localScale = GetBreedArchetypeTextureQuadScale(texture);
        GroundAlignDogArt(textureQuad.transform, Mathf.Abs(textureQuad.transform.localScale.y));
        ApplyDogArtPresentationTuning(artObject, textureQuad.transform, ResolveBreedVisualArchetype(dog));
        SetQuadFacing(textureQuad.transform, !isFighterA);

        Renderer quadRenderer = textureQuad.GetComponent<Renderer>();

        if (quadRenderer == null)
        {
            return;
        }

        Material runtimeMaterial = GetBreedArchetypeTextureMaterial(quadRenderer);
        Color tintColor = GetBreedArchetypeArtTint(dog, isFighterA, currentHealth, maxLikelyHealth);

        runtimeMaterial.mainTexture = texture;
        SetMaterialColor(runtimeMaterial, tintColor);
        quadRenderer.material = runtimeMaterial;
    }

    SpriteRenderer GetBreedArchetypeSpriteRenderer(GameObject artObject)
    {
        if (artObject == null)
        {
            return null;
        }

        Transform spriteTransform = artObject.transform.Find("BreedArchetypeSprite");
        GameObject spriteObject;

        if (spriteTransform != null)
        {
            spriteObject = spriteTransform.gameObject;
        }
        else
        {
            spriteObject = new GameObject("BreedArchetypeSprite");
            spriteObject.transform.SetParent(artObject.transform);
        }

        spriteObject.hideFlags = HideFlags.DontSave;
        spriteObject.transform.localPosition = Vector3.zero;
        spriteObject.transform.localRotation = Quaternion.identity;

        SpriteRenderer spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        spriteRenderer.receiveShadows = false;
        return spriteRenderer;
    }

    GameObject GetBreedArchetypeTextureQuad(GameObject artObject)
    {
        if (artObject == null)
        {
            return null;
        }

        Transform quadTransform = artObject.transform.Find("BreedArchetypeTextureQuad");
        GameObject quadObject;
        bool createdQuad = false;

        if (quadTransform != null)
        {
            quadObject = quadTransform.gameObject;
        }
        else
        {
            quadObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quadObject.name = "BreedArchetypeTextureQuad";
            quadObject.transform.SetParent(artObject.transform);
            createdQuad = true;
        }

        quadObject.hideFlags = HideFlags.DontSave;
        quadObject.transform.localPosition = Vector3.zero;
        quadObject.transform.localRotation = Quaternion.identity;

        Collider quadCollider = quadObject.GetComponent<Collider>();

        if (quadCollider != null)
        {
            quadCollider.enabled = false;
        }

        Renderer quadRenderer = quadObject.GetComponent<Renderer>();

        if (quadRenderer != null)
        {
            quadRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            quadRenderer.receiveShadows = false;
        }

        if (createdQuad)
        {
            quadObject.SetActive(false);
        }

        return quadObject;
    }

    Material GetBreedArchetypeTextureMaterial(Renderer objectRenderer)
    {
        Material runtimeMaterial = objectRenderer.material;

        if (runtimeMaterial != null && runtimeMaterial.name.StartsWith("RuntimeBreedArchetypeTextureMaterial"))
        {
            return runtimeMaterial;
        }

        Shader textureShader = Shader.Find("Unlit/Transparent");

        if (textureShader == null)
        {
            textureShader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (textureShader == null)
        {
            textureShader = Shader.Find("Unlit/Texture");
        }

        runtimeMaterial = textureShader != null
            ? new Material(textureShader)
            : new Material(objectRenderer.sharedMaterial);
        runtimeMaterial.name = "RuntimeBreedArchetypeTextureMaterial";
        runtimeMaterial.hideFlags = HideFlags.DontSave;
        runtimeMaterial.renderQueue = 3000;
        return runtimeMaterial;
    }

    Vector3 GetBreedArchetypeSpriteScale(Sprite sprite)
    {
        if (sprite == null || sprite.bounds.size.y <= 0f)
        {
            return Vector3.one;
        }

        float targetHeight = BreedArchetypeSpriteTargetHeight;
        float scale = targetHeight / sprite.bounds.size.y;
        float scaledWidth = sprite.bounds.size.x * scale;

        if (scaledWidth > BreedArchetypeSpriteMaxWidth && scaledWidth > 0f)
        {
            scale *= BreedArchetypeSpriteMaxWidth / scaledWidth;
        }

        return new Vector3(scale, scale, scale);
    }

    Vector3 GetBreedArchetypeTextureQuadScale(Texture2D texture)
    {
        if (texture == null || texture.height <= 0)
        {
            return Vector3.one;
        }

        float targetHeight = BreedArchetypeSpriteTargetHeight;
        float aspectRatio = Mathf.Clamp((float)texture.width / texture.height, 0.35f, 1.55f);
        return new Vector3(targetHeight * aspectRatio, targetHeight, 1f);
    }

    void GroundAlignDogArt(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        Vector3 localPosition = spriteRenderer.transform.localPosition;
        float visualBottom = spriteRenderer.sprite.bounds.min.y * Mathf.Abs(spriteRenderer.transform.localScale.y);
        localPosition.y = DogArtGroundPadding - visualBottom;
        spriteRenderer.transform.localPosition = localPosition;
    }

    void GroundAlignDogArt(Transform visualTransform, float visualHeight)
    {
        if (visualTransform == null)
        {
            return;
        }

        Vector3 localPosition = visualTransform.localPosition;
        localPosition.y = DogArtGroundPadding + (Mathf.Abs(visualHeight) * 0.5f);
        visualTransform.localPosition = localPosition;
    }

    void GroundAlignDogArt(GameObject artObject, float groundY)
    {
        if (artObject == null || artObject.transform.parent == null)
        {
            return;
        }

        Renderer[] renderers = GetRendererList(artObject);

        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        bool hasBounds = false;
        Bounds combinedBounds = new Bounds();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer artRenderer = renderers[i];

            if (artRenderer == null || !artRenderer.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = artRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(artRenderer.bounds);
            }
        }

        if (!hasBounds)
        {
            return;
        }

        Vector3 worldBottom = new Vector3(combinedBounds.center.x, combinedBounds.min.y, combinedBounds.center.z);
        float localBottomY = artObject.transform.parent.InverseTransformPoint(worldBottom).y;
        Vector3 localPosition = artObject.transform.localPosition;
        localPosition.y += groundY - localBottomY;
        artObject.transform.localPosition = localPosition;
    }

    Color GetBreedArchetypeArtTint(Dog dog, bool isFighterA, int currentHealth, int maxLikelyHealth)
    {
        float healthPercent = GetVisualHealthPercent(currentHealth, maxLikelyHealth);
        Color identityColor = GetDogIdentityColor(dog, isFighterA);
        Color finalColor = ApplyHealthCorruptionVisual(identityColor, healthPercent);
        return Color.Lerp(Color.white, finalColor, 0.34f);
    }

    void SetMaterialColor(Material runtimeMaterial, Color color)
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        runtimeMaterial.color = color;

        if (runtimeMaterial.HasProperty("_BaseColor"))
        {
            runtimeMaterial.SetColor("_BaseColor", color);
        }

        if (runtimeMaterial.HasProperty("_Color"))
        {
            runtimeMaterial.SetColor("_Color", color);
        }
    }

    void TintBreedArchetypeArt(GameObject artObject, Color color)
    {
        if (artObject == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = GetBreedArchetypeSpriteRenderer(artObject);

        if (spriteRenderer != null && spriteRenderer.enabled)
        {
            spriteRenderer.color = color;
        }

        GameObject textureQuad = GetBreedArchetypeTextureQuad(artObject);

        if (textureQuad == null || !textureQuad.activeSelf)
        {
            return;
        }

        Renderer quadRenderer = textureQuad.GetComponent<Renderer>();

        if (quadRenderer != null && quadRenderer.material != null)
        {
            SetMaterialColor(quadRenderer.material, color);
        }
    }

    void FaceBreedArchetypeArtsTowardPresentationCamera()
    {
        FaceBreedArchetypeArtTowardPresentationCamera(fighterABreedArchetypeArt);
        FaceBreedArchetypeArtTowardPresentationCamera(fighterBBreedArchetypeArt);
    }

    void FaceBreedArchetypeArtTowardPresentationCamera(GameObject artObject)
    {
        if (artObject == null || !artObject.activeSelf || presentationCamera == null)
        {
            return;
        }

        Vector3 cameraToArt = artObject.transform.position - presentationCamera.transform.position;

        if (cameraToArt.sqrMagnitude < 0.0001f)
        {
            artObject.transform.rotation = presentationCamera.transform.rotation;
            return;
        }

        artObject.transform.rotation = Quaternion.LookRotation(cameraToArt.normalized, Vector3.up);
    }

    void UpdateFighterFacingDirections()
    {
        // The current source dog art naturally faces left. FighterA must be mirrored
        // to face center, while FighterB can keep the source direction.
        SetDogArtFacing(fighterADogImprintArt, false);
        SetDogArtFacing(fighterBDogImprintArt, true);
        SetDogArtFacing(fighterABreedArchetypeArt, false);
        SetDogArtFacing(fighterBBreedArchetypeArt, true);
    }

    void SetDogArtFacing(GameObject artObject, bool faceRight)
    {
        if (artObject == null)
        {
            return;
        }

        bool isDogImprintPrefabArt = artObject == fighterADogImprintArt || artObject == fighterBDogImprintArt;

        if (isDogImprintPrefabArt)
        {
            SetQuadFacing(artObject.transform, faceRight);
            return;
        }

        foreach (SpriteRenderer spriteRenderer in artObject.GetComponentsInChildren<SpriteRenderer>(true))
        {
            SetSpriteFacing(spriteRenderer, faceRight);
        }

        Transform textureQuadTransform = artObject.transform.Find("BreedArchetypeTextureQuad");

        if (textureQuadTransform != null)
        {
            SetQuadFacing(textureQuadTransform, faceRight);
        }
    }

    void SetSpriteFacing(SpriteRenderer spriteRenderer, bool faceRight)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.flipX = !faceRight;
    }

    void SetQuadFacing(Transform artTransform, bool faceRight)
    {
        if (artTransform == null)
        {
            return;
        }

        Vector3 scale = artTransform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
        artTransform.localScale = scale;
    }

    bool ShouldDogArtFaceRight(GameObject artObject)
    {
        return artObject == fighterBDogImprintArt || artObject == fighterBBreedArchetypeArt;
    }

    void ApplyBreedArchetypeResultVisual(GameObject artObject, bool isWinner, bool isDraw, Color accentColor)
    {
        if (artObject == null || !artObject.activeSelf)
        {
            return;
        }

        bool faceRight = ShouldDogArtFaceRight(artObject);

        if (isWinner)
        {
            artObject.transform.localPosition += new Vector3(0f, 0.22f, -0.05f);
            artObject.transform.localScale *= 1.12f;
            TintBreedArchetypeArt(artObject, Color.Lerp(Color.white, accentColor, 0.18f));
            SetDogArtFacing(artObject, faceRight);
            return;
        }

        if (isDraw)
        {
            artObject.transform.localPosition += new Vector3(0f, 0.08f, 0f);
            artObject.transform.localScale *= 1.04f;
            TintBreedArchetypeArt(artObject, new Color(1f, 0.92f, 0.45f));
            SetDogArtFacing(artObject, faceRight);
            return;
        }

        artObject.transform.localPosition += new Vector3(0f, -0.22f, 0.06f);
        artObject.transform.localScale *= 0.82f;
        TintBreedArchetypeArt(artObject, new Color(0.55f, 0.35f, 0.66f, 0.95f));
        SetDogArtFacing(artObject, faceRight);
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
        CreateOrUpdateLabel(arenaRoot, "FighterALabel", $"{GetDogDisplayName(dogA, "DOG A")}\nIMPRINT", GetArenaFighterLabelPosition(fighterATransform, new Vector3(-1.75f, 2.55f, 0f)), Color.cyan, 0.085f);
        CreateOrUpdateLabel(arenaRoot, "FighterBLabel", $"{GetDogDisplayName(dogB, "DOG B")}\nIMPRINT", GetArenaFighterLabelPosition(fighterBTransform, new Vector3(1.75f, 2.55f, 0f)), Color.magenta, 0.085f);
    }

    void UpdateArenaResultLabels(Dog dogA, Dog dogB, string dogAStatus, string dogBStatus, Color dogAColor, Color dogBColor)
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreateOrUpdateLabel(arenaRoot, "ArenaTitleLabel", "DIGITAL ARENA", new Vector3(0f, 3.12f, 0.2f), Color.white, 0.145f);
        CreateOrUpdateLabel(arenaRoot, "FighterALabel", $"{GetDogDisplayName(dogA, "DOG A")}\n{dogAStatus}", GetArenaFighterLabelPosition(fighterATransform, new Vector3(-1.75f, 2.55f, 0f)), dogAColor, 0.09f);
        CreateOrUpdateLabel(arenaRoot, "FighterBLabel", $"{GetDogDisplayName(dogB, "DOG B")}\n{dogBStatus}", GetArenaFighterLabelPosition(fighterBTransform, new Vector3(1.75f, 2.55f, 0f)), dogBColor, 0.09f);
    }

    void CreateDogPortraitBillboardsIfNeeded()
    {
        if (arenaRoot == null)
        {
            return;
        }

        if (!ShowDogPortraitPlaceholders)
        {
            HideDogPortraitBillboards();
            return;
        }

        if (fighterAPortraitBillboard == null)
        {
            fighterAPortraitBillboard = CreateDogPortraitBillboard("FighterA_PortraitBillboard", "FighterA_PortraitFrame", Color.cyan);
        }

        if (fighterBPortraitBillboard == null)
        {
            fighterBPortraitBillboard = CreateDogPortraitBillboard("FighterB_PortraitBillboard", "FighterB_PortraitFrame", Color.magenta);
        }

        fighterAPortraitFrame = GetPortraitFrameObject(fighterAPortraitBillboard, "FighterA_PortraitFrame");
        fighterBPortraitFrame = GetPortraitFrameObject(fighterBPortraitBillboard, "FighterB_PortraitFrame");
    }

    GameObject CreateDogPortraitBillboard(string objectName, string frameName, Color accentColor)
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

        CreatePortraitFramesIfNeeded(billboardObject.transform, frameName, accentColor);
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

        if (!ShowDogPortraitPlaceholders)
        {
            HideDogPortraitBillboards();
            return;
        }

        CreateDogPortraitBillboardsIfNeeded();
        ConfigurePortraitBillboard(fighterAPortraitBillboard, dogA, fighterATransform, ref warnedMissingDogSpriteA);
        ConfigurePortraitBillboard(fighterBPortraitBillboard, dogB, fighterBTransform, ref warnedMissingDogSpriteB);
        UpdatePortraitBillboardPositions();
        FacePortraitsTowardPresentationCamera();
    }

    void ConfigurePortraitBillboard(GameObject billboardObject, Dog dog, Transform fighterTransform, ref bool warnedMissingSprite)
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

        UpdateSinglePortraitBillboardPosition(billboardObject, fighterTransform);
        billboardObject.transform.localScale = GetPortraitBillboardBaseScale();
        spriteRenderer.transform.localScale = GetPortraitSpriteScale(portraitSprite);
        SetPortraitBillboardActive(billboardObject, true);
    }

    void UpdatePortraitBillboardPositions()
    {
        UpdateSinglePortraitBillboardPosition(fighterAPortraitBillboard, fighterATransform);
        UpdateSinglePortraitBillboardPosition(fighterBPortraitBillboard, fighterBTransform);
    }

    void UpdateSinglePortraitBillboardPosition(GameObject billboardObject, Transform fighterTransform)
    {
        if (billboardObject == null || fighterTransform == null)
        {
            return;
        }

        billboardObject.transform.localPosition = fighterTransform.localPosition + GetPortraitBillboardOffset();
    }

    Vector3 GetPortraitBillboardOffset()
    {
        if (dogImprintPrefab != null)
        {
            return new Vector3(0f, 1.24f, -0.95f);
        }

        return new Vector3(0f, 1.08f, -0.72f);
    }

    Vector3 GetPortraitBillboardBaseScale()
    {
        if (dogImprintPrefab != null)
        {
            return new Vector3(0.72f, 0.72f, 0.72f);
        }

        return new Vector3(1.08f, 1.08f, 1.08f);
    }

    void CreatePortraitFramesIfNeeded(Transform billboardTransform, string frameName, Color accentColor)
    {
        if (billboardTransform == null)
        {
            return;
        }

        CreatePortraitCardPart(billboardTransform, frameName, new Vector3(0f, 0f, 0.055f), new Vector3(1.48f, 1.18f, 0.04f), new Color(0.012f, 0.018f, 0.03f));
        CreatePortraitCardPart(billboardTransform, $"{frameName}_Top", new Vector3(0f, 0.63f, -0.015f), new Vector3(1.56f, 0.07f, 0.055f), accentColor);
        CreatePortraitCardPart(billboardTransform, $"{frameName}_Bottom", new Vector3(0f, -0.63f, -0.015f), new Vector3(1.56f, 0.07f, 0.055f), accentColor);
        CreatePortraitCardPart(billboardTransform, $"{frameName}_Left", new Vector3(-0.78f, 0f, -0.015f), new Vector3(0.07f, 1.24f, 0.055f), accentColor);
        CreatePortraitCardPart(billboardTransform, $"{frameName}_Right", new Vector3(0.78f, 0f, -0.015f), new Vector3(0.07f, 1.24f, 0.055f), accentColor);
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

    GameObject GetPortraitFrameObject(GameObject billboardObject, string frameName)
    {
        if (billboardObject == null)
        {
            return null;
        }

        Transform frameTransform = billboardObject.transform.Find(frameName);
        return frameTransform != null ? frameTransform.gameObject : null;
    }

    void UpdatePortraitFrameVisuals(int dogAHealth, int dogBHealth)
    {
        UpdateSinglePortraitFrameVisual(
            fighterAPortraitBillboard,
            fighterAPortraitFrame,
            dogAHealth,
            visualMaxHealthA,
            Color.cyan
        );
        UpdateSinglePortraitFrameVisual(
            fighterBPortraitBillboard,
            fighterBPortraitFrame,
            dogBHealth,
            visualMaxHealthB,
            Color.magenta
        );
    }

    void UpdateSinglePortraitFrameVisual(GameObject billboardObject, GameObject frameObject, int currentHealth, int maxLikelyHealth, Color cleanAccentColor)
    {
        if (billboardObject == null || frameObject == null || !billboardObject.activeSelf)
        {
            return;
        }

        float healthPercent = GetPortraitHealthPercent(currentHealth, maxLikelyHealth);
        float corruptionStrength = 1f - healthPercent;
        Color corruptedAccent = Color.Lerp(new Color(0.55f, 0.05f, 0.85f), new Color(1f, 0.05f, 0.02f), corruptionStrength);
        Color frameAccent = Color.Lerp(corruptedAccent, cleanAccentColor, healthPercent);
        Color frameBack = Color.Lerp(new Color(0.08f, 0.01f, 0.12f), new Color(0.012f, 0.018f, 0.03f), healthPercent);

        TintPortraitFrame(billboardObject, frameObject.name, frameBack, frameAccent);

        SpriteRenderer spriteRenderer = GetPortraitSpriteRenderer(billboardObject);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(new Color(0.62f, 0.35f, 0.85f), Color.white, Mathf.Max(0.35f, healthPercent));
        }
    }

    float GetPortraitHealthPercent(int currentHealth, int maxLikelyHealth)
    {
        if (maxLikelyHealth <= 1 && currentHealth <= 0)
        {
            return 1f;
        }

        return GetHealthPercent(currentHealth, maxLikelyHealth);
    }

    void TintPortraitFrame(GameObject billboardObject, string frameName, Color backColor, Color accentColor)
    {
        if (billboardObject == null || string.IsNullOrEmpty(frameName))
        {
            return;
        }

        SetObjectUnlitColor(GetPortraitFrameObject(billboardObject, frameName), backColor);
        SetObjectUnlitColor(GetPortraitFrameObject(billboardObject, $"{frameName}_Top"), accentColor);
        SetObjectUnlitColor(GetPortraitFrameObject(billboardObject, $"{frameName}_Bottom"), accentColor);
        SetObjectUnlitColor(GetPortraitFrameObject(billboardObject, $"{frameName}_Left"), accentColor);
        SetObjectUnlitColor(GetPortraitFrameObject(billboardObject, $"{frameName}_Right"), accentColor);
    }

    void ApplyPortraitResultVisual(GameObject billboardObject, GameObject frameObject, bool isWinner, bool isDraw, Color accentColor)
    {
        if (billboardObject == null || frameObject == null || !billboardObject.activeSelf)
        {
            return;
        }

        SpriteRenderer spriteRenderer = GetPortraitSpriteRenderer(billboardObject);
        string frameName = frameObject.name;

        if (isWinner)
        {
            billboardObject.transform.localPosition += new Vector3(0f, 0.25f, -0.05f);
            billboardObject.transform.localScale = GetPortraitBillboardBaseScale() * 1.15f;
            TintPortraitFrame(billboardObject, frameName, new Color(0.02f, 0.07f, 0.04f), new Color(0.1f, 1f, 0.35f));

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }

            return;
        }

        if (isDraw)
        {
            billboardObject.transform.localPosition += new Vector3(0f, 0.1f, 0f);
            billboardObject.transform.localScale = GetPortraitBillboardBaseScale() * 1.05f;
            TintPortraitFrame(billboardObject, frameName, new Color(0.07f, 0.06f, 0.025f), new Color(1f, 0.85f, 0.2f));

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }

            return;
        }

        billboardObject.transform.localPosition += new Vector3(0f, -0.28f, 0.08f);
        billboardObject.transform.localScale = GetPortraitBillboardBaseScale() * 0.82f;
        TintPortraitFrame(billboardObject, frameName, new Color(0.035f, 0.02f, 0.045f), new Color(0.55f, 0.1f, 0.75f));

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.48f, 0.42f, 0.55f, 0.9f);
        }
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
        FaceBreedArchetypeArtsTowardPresentationCamera();
    }

    void FacePortraitTowardPresentationCamera(GameObject billboardObject)
    {
        if (billboardObject == null || !billboardObject.activeSelf || presentationCamera == null)
        {
            return;
        }

        Vector3 cameraToBillboard = billboardObject.transform.position - presentationCamera.transform.position;

        if (cameraToBillboard.sqrMagnitude < 0.0001f)
        {
            billboardObject.transform.rotation = presentationCamera.transform.rotation;
            return;
        }

        // The sprite child sits on local -Z, so local +Z points away from the camera.
        // That keeps the portrait readable without flipping it backwards.
        billboardObject.transform.rotation = Quaternion.LookRotation(cameraToBillboard.normalized, Vector3.up);
    }

    void HideDogPortraitBillboards()
    {
        SetPortraitBillboardActive(fighterAPortraitBillboard, false);
        SetPortraitBillboardActive(fighterBPortraitBillboard, false);
        SetArenaChildActive("FighterA_PortraitBillboard", false);
        SetArenaChildActive("FighterB_PortraitBillboard", false);
    }

    void SetPortraitBillboardActive(GameObject billboardObject, bool isActive)
    {
        if (billboardObject != null)
        {
            billboardObject.SetActive(isActive);
        }
    }

    void SetArenaChildActive(string childName, bool isActive)
    {
        if (arenaRoot == null || string.IsNullOrEmpty(childName))
        {
            return;
        }

        Transform childTransform = arenaRoot.transform.Find(childName);

        if (childTransform != null)
        {
            childTransform.gameObject.SetActive(isActive);
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

    void UpdateRoundStatusBanner(
        int roundNumber,
        int dogAHealth,
        int dogBHealth,
        int dogAImpact,
        int dogBImpact,
        bool isResult,
        FightStrategy dogAStrategy = FightStrategy.Balanced,
        FightStrategy dogBStrategy = FightStrategy.Balanced,
        FightStyle dogAStyle = FightStyle.Balanced,
        FightStyle dogBStyle = FightStyle.Balanced
    )
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

        string message = GetRoundStatusMessage(
            roundNumber,
            dogAHealth,
            dogBHealth,
            dogAImpact,
            dogBImpact,
            isResult,
            dogAStrategy,
            dogBStrategy,
            dogAStyle,
            dogBStyle
        );

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

    void SetRoundStatusBannerText(string message)
    {
        CreateRoundStatusBannerIfNeeded();

        if (roundStatusBannerObject == null || string.IsNullOrEmpty(message))
        {
            return;
        }

        TextMesh banner = roundStatusBannerObject.GetComponent<TextMesh>();

        if (banner == null)
        {
            banner = roundStatusBannerObject.AddComponent<TextMesh>();
        }

        SetLabelText(banner, message);
        banner.color = GetRoundStatusColor(message);
        roundStatusBannerObject.SetActive(true);
    }

    string GetResultBannerText(int dogAHealth, int dogBHealth, bool isDraw)
    {
        if (isDraw)
        {
            return "DRAW";
        }

        return Mathf.Min(dogAHealth, dogBHealth) <= 0 ? "FINISH" : "WINNER";
    }

    string GetRoundStatusMessage(
        int roundNumber,
        int dogAHealth,
        int dogBHealth,
        int dogAImpact,
        int dogBImpact,
        bool isResult,
        FightStrategy dogAStrategy,
        FightStrategy dogBStrategy,
        FightStyle dogAStyle,
        FightStyle dogBStyle
    )
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

        string styleStatus = GetStyleStatusText(dogAStyle, dogBStyle, dogAImpact, dogBImpact);
        string strategyStatus = string.IsNullOrEmpty(styleStatus)
            ? GetStrategyStatusText(dogAStrategy, dogBStrategy, dogAImpact, dogBImpact, roundNumber)
            : styleStatus;

        if (!hasImpact)
        {
            return strategyStatus;
        }

        if (highestImpact >= corruptionSpikeImpactValue && anyImprintDamaged)
        {
            return "GLITCH";
        }

        if (impactDifference <= evenExchangeDifference)
        {
            return strategyStatus == "EXCHANGE" ? "EVEN TRADE" : strategyStatus;
        }

        if (highestImpact >= heavyImpactValue || impactDifference >= heavyImpactDifference)
        {
            return strategyStatus == "EXCHANGE" ? "HEAVY HIT" : strategyStatus;
        }

        return strategyStatus;
    }

    string GetStrategyStatusText(FightStrategy dogAStrategy, FightStrategy dogBStrategy, int dogAImpact, int dogBImpact, int roundNumber)
    {
        if (dogAStrategy == dogBStrategy)
        {
            return GetSingleStrategyStatusText(dogAStrategy);
        }

        if (dogAImpact > dogBImpact + 3)
        {
            return GetSingleStrategyStatusText(dogAStrategy);
        }

        if (dogBImpact > dogAImpact + 3)
        {
            return GetSingleStrategyStatusText(dogBStrategy);
        }

        if (dogAStrategy == FightStrategy.AllIn || dogBStrategy == FightStrategy.AllIn)
        {
            return "ALL IN";
        }

        if (dogAStrategy == FightStrategy.DefensiveShell || dogBStrategy == FightStrategy.DefensiveShell)
        {
            return "SHELL";
        }

        if (dogAStrategy == FightStrategy.CounterPlan || dogBStrategy == FightStrategy.CounterPlan)
        {
            return "COUNTER";
        }

        if ((dogAStrategy == FightStrategy.RushEarly || dogBStrategy == FightStrategy.RushEarly) && roundNumber <= 2)
        {
            return "RUSH";
        }

        if ((dogAStrategy == FightStrategy.WearDown || dogBStrategy == FightStrategy.WearDown) && roundNumber >= 4)
        {
            return "PRESSURE";
        }

        return "EXCHANGE";
    }

    string GetSingleStrategyStatusText(FightStrategy strategy)
    {
        switch (strategy)
        {
            case FightStrategy.RushEarly:
                return "RUSH";

            case FightStrategy.CounterPlan:
                return "COUNTER";

            case FightStrategy.WearDown:
                return "PRESSURE";

            case FightStrategy.DefensiveShell:
                return "SHELL";

            case FightStrategy.AllIn:
                return "ALL IN";

            case FightStrategy.Balanced:
            default:
                return "EXCHANGE";
        }
    }

    string GetStyleStatusText(FightStyle dogAStyle, FightStyle dogBStyle, int dogAImpact, int dogBImpact)
    {
        if (dogAStyle == FightStyle.Wildcard || dogBStyle == FightStyle.Wildcard)
        {
            return "GLITCH";
        }

        if (dogAStyle == FightStyle.Tank || dogBStyle == FightStyle.Tank)
        {
            return "TANK";
        }

        if (dogAStyle == FightStyle.Counter && dogAImpact >= dogBImpact + 3)
        {
            return "COUNTER";
        }

        if (dogBStyle == FightStyle.Counter && dogBImpact >= dogAImpact + 3)
        {
            return "COUNTER";
        }

        if (dogAStyle == FightStyle.Rushdown && dogAImpact >= dogBImpact)
        {
            return "RUSH";
        }

        if (dogBStyle == FightStyle.Rushdown && dogBImpact >= dogAImpact)
        {
            return "RUSH";
        }

        return string.Empty;
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

        if (message.StartsWith("WINNER"))
        {
            return new Color(0.1f, 1f, 0.35f);
        }

        if (message.StartsWith("FINISH"))
        {
            return new Color(1f, 0.45f, 0.05f);
        }

        if (message.StartsWith("DRAW"))
        {
            return new Color(1f, 0.85f, 0.2f);
        }

        if (message.StartsWith("CLASH") || message.StartsWith("TRADE") || message.StartsWith("HIT"))
        {
            return new Color(0.72f, 0.95f, 1f);
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

        if (message.StartsWith("RUSH"))
        {
            return new Color(1f, 0.35f, 0.05f);
        }

        if (message.StartsWith("COUNTER"))
        {
            return new Color(0.35f, 0.8f, 1f);
        }

        if (message.StartsWith("PRESSURE"))
        {
            return new Color(1f, 0.75f, 0.15f);
        }

        if (message.StartsWith("SHELL"))
        {
            return new Color(0.25f, 1f, 1f);
        }

        if (message.StartsWith("ALL IN"))
        {
            return new Color(1f, 0.08f, 0.08f);
        }

        if (message.StartsWith("TANK"))
        {
            return new Color(0.35f, 1f, 0.85f);
        }

        if (message.StartsWith("EXCHANGE"))
        {
            return new Color(0.65f, 0.9f, 1f);
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

    void CreateClashTextIfNeeded()
    {
        if (clashTextObject != null || arenaRoot == null)
        {
            return;
        }

        TextMesh clashText = CreateOrUpdateLabel(arenaRoot, "CinematicClashText", "", new Vector3(0f, 1.92f, 0.22f), new Color(0.72f, 0.95f, 1f), 0.075f);

        if (clashText == null)
        {
            return;
        }

        clashText.fontSize = 52;
        clashText.alignment = TextAlignment.Center;
        clashText.anchor = TextAnchor.MiddleCenter;
        clashTextObject = clashText.gameObject;
        clashTextObject.SetActive(false);
    }

    void ShowClashText(string message, Color color, float scaleMultiplier)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        CreateClashTextIfNeeded();

        if (clashTextObject == null)
        {
            return;
        }

        TextMesh clashText = clashTextObject.GetComponent<TextMesh>();

        if (clashText != null)
        {
            SetLabelText(clashText, message);
            clashText.color = color;
            clashText.characterSize = 0.075f;
        }

        clashTextObject.transform.localPosition = new Vector3(0f, 1.92f, 0.22f);
        clashTextObject.transform.localRotation = Quaternion.identity;
        clashTextObject.transform.localScale = Vector3.one * Mathf.Clamp(scaleMultiplier, 0.75f, 1.05f);
        clashTextObject.SetActive(true);
    }

    void HideClashText()
    {
        if (clashTextObject != null)
        {
            clashTextObject.SetActive(false);
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

    Vector3 GetArenaFighterLabelPosition(Transform targetTransform, Vector3 fallbackPosition)
    {
        if (targetTransform == null)
        {
            return fallbackPosition;
        }

        return targetTransform.localPosition + new Vector3(0f, 1.95f, 0.02f);
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

    void UpdateImprintCorruptionVisuals(int dogAHealth, int dogBHealth, bool refreshBreedArt = true)
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreateCorruptionNodesIfNeeded();
        visualMaxHealthA = Mathf.Max(visualMaxHealthA, Mathf.Max(1, dogAHealth));
        visualMaxHealthB = Mathf.Max(visualMaxHealthB, Mathf.Max(1, dogBHealth));

        ApplyImprintCorruption(fighterATransform, dogAHealth, visualMaxHealthA, imprintCorruptionNodesA, new Color(0f, 0.58f, 0.78f));
        ApplyImprintCorruption(fighterBTransform, dogBHealth, visualMaxHealthB, imprintCorruptionNodesB, new Color(0.78f, 0.1f, 0.68f));
        UpdateDogImprintArtPositions();
        ApplyDogIdentityVisuals(fighterADogImprintArt, currentDogImprintA, dogAHealth, visualMaxHealthA, true);
        ApplyDogIdentityVisuals(fighterBDogImprintArt, currentDogImprintB, dogBHealth, visualMaxHealthB, false);
        if (refreshBreedArt)
        {
            CreateBreedArchetypeArtIfNeeded();
            ApplyBreedArchetypeArtToFighter(fighterABreedArchetypeArt, currentDogImprintA, true, dogAHealth, visualMaxHealthA);
            ApplyBreedArchetypeArtToFighter(fighterBBreedArchetypeArt, currentDogImprintB, false, dogBHealth, visualMaxHealthB);
        }
        UpdateHealthBars(dogAHealth, dogBHealth);
        UpdatePortraitFrameVisuals(dogAHealth, dogBHealth);
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

    void ApplyDogIdentityVisuals(GameObject artObject, Dog dog, int currentHealth, int maxLikelyHealth, bool isFighterA)
    {
        if (artObject == null || dogImprintPrefab == null)
        {
            return;
        }

        float healthPercent = GetVisualHealthPercent(currentHealth, maxLikelyHealth);
        Color identityColor = GetDogIdentityColor(dog, isFighterA);
        Vector3 identityScale = GetDogImprintBaseScale();

        ApplyBreedArchetypeVisuals(dog, ref identityColor, ref identityScale);
        ApplyTraitVisualAccents(dog, healthPercent, ref identityColor, ref identityScale);

        Color finalColor = ApplyHealthCorruptionVisual(identityColor, healthPercent);
        TintDogImprintArt(artObject, finalColor);
        artObject.transform.localScale = identityScale * Mathf.Lerp(1f, 0.86f, 1f - healthPercent);
        SetDogArtFacing(artObject, !isFighterA);
        GroundAlignDogArt(artObject, DogArtGroundY);
    }

    float GetVisualHealthPercent(int currentHealth, int maxLikelyHealth)
    {
        int safeMaxHealth = Mathf.Max(1, maxLikelyHealth);

        if (safeMaxHealth <= 1 && currentHealth <= 0)
        {
            return 1f;
        }

        return Mathf.Clamp01((float)Mathf.Max(0, currentHealth) / safeMaxHealth);
    }

    Color GetDogIdentityColor(Dog dog, bool isFighterA)
    {
        FightStyle style = dog != null ? dog.fightStyle : FightStyle.Balanced;
        Color styleColor = GetFightStyleAccentColor(style);
        BreedVisualArchetype archetype = ResolveBreedVisualArchetype(dog);
        Color archetypeColor = GetBreedArchetypeAccentColor(archetype);
        Color blendedColor = Color.Lerp(styleColor, archetypeColor, 0.28f);
        return GetDogNameColorVariation(dog, blendedColor, isFighterA);
    }

    Color GetFightStyleAccentColor(FightStyle style)
    {
        switch (style)
        {
            case FightStyle.Rushdown:
                return new Color(1f, 0.34f, 0.08f);

            case FightStyle.Counter:
                return new Color(0.18f, 0.78f, 1f);

            case FightStyle.Tank:
                return new Color(0.36f, 0.88f, 0.78f);

            case FightStyle.Wildcard:
                return new Color(0.82f, 0.16f, 1f);

            case FightStyle.Balanced:
            default:
                return new Color(0.82f, 1f, 1f);
        }
    }

    Color GetDogNameColorVariation(Dog dog, Color baseColor, bool isFighterA)
    {
        string nameKey = dog != null && !string.IsNullOrEmpty(dog.dogName)
            ? dog.dogName
            : (isFighterA ? "fighter_a" : "fighter_b");
        float variation = GetStableNameHash01(nameKey);

        Color.RGBToHSV(baseColor, out float hue, out float saturation, out float value);
        hue = Mathf.Repeat(hue + Mathf.Lerp(-0.035f, 0.035f, variation), 1f);
        saturation = Mathf.Clamp01(saturation * Mathf.Lerp(0.88f, 1.12f, variation));
        value = Mathf.Clamp01(value * Mathf.Lerp(0.94f, 1.08f, 1f - variation));

        return Color.HSVToRGB(hue, saturation, value);
    }

    float GetStableNameHash01(string value)
    {
        unchecked
        {
            int hash = 17;

            for (int i = 0; i < value.Length; i++)
            {
                hash = (hash * 31) + char.ToUpperInvariant(value[i]);
            }

            return (hash & 0x7fffffff) / (float)int.MaxValue;
        }
    }

    BreedVisualArchetype ResolveBreedVisualArchetype(Dog dog)
    {
        return ResolveBreedVisualArchetypeFromBreedText(GetDogBreedText(dog), true);
    }

    BreedVisualArchetype ResolveBreedVisualArchetypeFromBreedText(string breedText, bool preferHybrid)
    {
        if (string.IsNullOrWhiteSpace(breedText))
        {
            return BreedVisualArchetype.Unknown;
        }

        string rawBreed = GetRawNormalizedBreedText(breedText);
        string separatorNormalizedBreed = GetSeparatorNormalizedBreedText(breedText);
        string compactBreed = GetCompactBreedText(breedText);

        if (preferHybrid && (IsHybridBreedText(breedText) || IsShepherdBullyHybridText(breedText)))
        {
            return BreedVisualArchetype.HybridVariant;
        }

        if (separatorNormalizedBreed.Contains("german shepherd") ||
            separatorNormalizedBreed.Contains("german shepard") ||
            separatorNormalizedBreed.Contains("belgian malinois") ||
            rawBreed.Contains("shepherd") ||
            rawBreed.Contains("shepard") ||
            compactBreed.Contains("shepherd") ||
            compactBreed.Contains("shepard") ||
            rawBreed.Contains("malinois") ||
            compactBreed.Contains("malinois"))
        {
            return BreedVisualArchetype.ShepherdSentinel;
        }

        if (separatorNormalizedBreed.Contains("pit bull") ||
            compactBreed.Contains("pitbull") ||
            rawBreed.Contains("boxer") ||
            compactBreed.Contains("boxer") ||
            rawBreed.Contains("bully") ||
            compactBreed.Contains("bully"))
        {
            return BreedVisualArchetype.BullyStriker;
        }

        if (rawBreed.Contains("mastiff") ||
            compactBreed.Contains("mastiff") ||
            separatorNormalizedBreed.Contains("cane corso") ||
            compactBreed.Contains("canecorso") ||
            rawBreed.Contains("presa") ||
            compactBreed.Contains("presa") ||
            separatorNormalizedBreed.Contains("dogo argentino") ||
            compactBreed.Contains("dogoargentino"))
        {
            return BreedVisualArchetype.GuardMastiff;
        }

        if (rawBreed.Contains("rottweiler") ||
            compactBreed.Contains("rottweiler") ||
            rawBreed.Contains("doberman") ||
            compactBreed.Contains("doberman"))
        {
            return BreedVisualArchetype.IronRott;
        }

        if (rawBreed.Contains("akita") ||
            compactBreed.Contains("akita") ||
            rawBreed.Contains("spitz") ||
            compactBreed.Contains("spitz") ||
            rawBreed.Contains("husky") ||
            compactBreed.Contains("husky"))
        {
            return BreedVisualArchetype.SpitzWarden;
        }

        if (rawBreed.Contains("greyhound") ||
            compactBreed.Contains("greyhound") ||
            rawBreed.Contains("hound") ||
            compactBreed.Contains("hound"))
        {
            return BreedVisualArchetype.VelocityHound;
        }

        return IsHybridBreedText(breedText)
            ? BreedVisualArchetype.HybridVariant
            : BreedVisualArchetype.Unknown;
    }

    string GetDogBreedText(Dog dog)
    {
        return dog != null ? dog.breed : string.Empty;
    }

    string GetRawNormalizedBreedText(string breedText)
    {
        return string.IsNullOrWhiteSpace(breedText)
            ? string.Empty
            : breedText.Trim().ToLowerInvariant();
    }

    string GetSeparatorNormalizedBreedText(string breedText)
    {
        string rawBreed = GetRawNormalizedBreedText(breedText);

        if (string.IsNullOrEmpty(rawBreed))
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(rawBreed.Length);
        bool previousWasSpace = false;

        for (int i = 0; i < rawBreed.Length; i++)
        {
            char breedCharacter = rawBreed[i];
            bool isSeparator = char.IsWhiteSpace(breedCharacter) ||
                               breedCharacter == '_' ||
                               breedCharacter == '-' ||
                               breedCharacter == '/' ||
                               breedCharacter == '\\' ||
                               breedCharacter == '\'';

            if (isSeparator)
            {
                if (!previousWasSpace && builder.Length > 0)
                {
                    builder.Append(' ');
                    previousWasSpace = true;
                }

                continue;
            }

            builder.Append(breedCharacter);
            previousWasSpace = false;
        }

        return builder.ToString().Trim();
    }

    string GetCompactBreedText(string breedText)
    {
        string rawBreed = GetRawNormalizedBreedText(breedText);

        if (string.IsNullOrEmpty(rawBreed))
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(rawBreed.Length);

        for (int i = 0; i < rawBreed.Length; i++)
        {
            char breedCharacter = rawBreed[i];

            if (char.IsWhiteSpace(breedCharacter) ||
                breedCharacter == '_' ||
                breedCharacter == '-' ||
                breedCharacter == '/' ||
                breedCharacter == '\\' ||
                breedCharacter == '\'')
            {
                continue;
            }

            builder.Append(breedCharacter);
        }

        return builder.ToString();
    }

    bool IsHybridBreedText(string breedText)
    {
        if (string.IsNullOrWhiteSpace(breedText))
        {
            return false;
        }

        string rawBreed = GetRawNormalizedBreedText(breedText);
        string separatorNormalizedBreed = GetSeparatorNormalizedBreedText(breedText);
        string compactBreed = GetCompactBreedText(breedText);

        return rawBreed.Contains("hybrid") ||
               rawBreed.Contains("mix") ||
               rawBreed.Contains("mixed") ||
               rawBreed.Contains("cross") ||
               separatorNormalizedBreed.Contains(" x ") ||
               compactBreed.Contains("hybrid") ||
               compactBreed.Contains("mixed") ||
               compactBreed.Contains("cross");
    }

    void ApplyBreedArchetypeVisuals(Dog dog, ref Color color, ref Vector3 scale)
    {
        BreedVisualArchetype archetype = ResolveBreedVisualArchetype(dog);
        color = Color.Lerp(color, GetBreedArchetypeAccentColor(archetype), 0.16f);
        scale = Vector3.Scale(scale, GetBreedArchetypeScaleModifier(archetype));
    }

    Color GetBreedArchetypeAccentColor(BreedVisualArchetype archetype)
    {
        switch (archetype)
        {
            case BreedVisualArchetype.ShepherdSentinel:
                return new Color(0.55f, 0.95f, 1f);

            case BreedVisualArchetype.BullyStriker:
                return new Color(1f, 0.36f, 0.08f);

            case BreedVisualArchetype.GuardMastiff:
                return new Color(0.38f, 0.82f, 0.78f);

            case BreedVisualArchetype.IronRott:
                return new Color(0.34f, 0.62f, 0.72f);

            case BreedVisualArchetype.SpitzWarden:
                return new Color(0.7f, 0.9f, 1f);

            case BreedVisualArchetype.VelocityHound:
                return new Color(0.86f, 1f, 1f);

            case BreedVisualArchetype.HybridVariant:
                return new Color(0.78f, 0.42f, 1f);

            case BreedVisualArchetype.Unknown:
            default:
                return new Color(0.82f, 1f, 1f);
        }
    }

    Vector3 GetBreedArchetypeScale(BreedVisualArchetype archetype)
    {
        switch (archetype)
        {
            case BreedVisualArchetype.BullyStriker:
                return new Vector3(1.13f, 0.9f, 1.08f);

            case BreedVisualArchetype.GuardMastiff:
                return new Vector3(1.18f, 1.08f, 1.16f);

            case BreedVisualArchetype.IronRott:
                return new Vector3(1.08f, 0.96f, 1.12f);

            case BreedVisualArchetype.SpitzWarden:
                return new Vector3(0.96f, 1.12f, 0.98f);

            case BreedVisualArchetype.VelocityHound:
                return new Vector3(0.82f, 1.18f, 0.82f);

            case BreedVisualArchetype.HybridVariant:
                return new Vector3(1.02f, 1.02f, 1.02f);

            case BreedVisualArchetype.ShepherdSentinel:
            case BreedVisualArchetype.Unknown:
            default:
                return Vector3.one;
        }
    }

    Vector3 GetBreedArchetypeOffset(BreedVisualArchetype archetype)
    {
        switch (archetype)
        {
            case BreedVisualArchetype.BullyStriker:
                return new Vector3(0f, -0.04f, 0f);

            case BreedVisualArchetype.GuardMastiff:
                return new Vector3(0f, -0.02f, 0f);

            case BreedVisualArchetype.SpitzWarden:
                return new Vector3(0f, 0.05f, 0f);

            case BreedVisualArchetype.VelocityHound:
                return new Vector3(0f, 0.06f, 0f);

            case BreedVisualArchetype.IronRott:
            case BreedVisualArchetype.HybridVariant:
            case BreedVisualArchetype.ShepherdSentinel:
            case BreedVisualArchetype.Unknown:
            default:
                return Vector3.zero;
        }
    }

    string GetBreedArchetypeStatusText(BreedVisualArchetype archetype)
    {
        switch (archetype)
        {
            case BreedVisualArchetype.ShepherdSentinel:
                return "Shepherd Sentinel";

            case BreedVisualArchetype.BullyStriker:
                return "Bully Striker";

            case BreedVisualArchetype.GuardMastiff:
                return "Guard Mastiff";

            case BreedVisualArchetype.IronRott:
                return "Iron Rott";

            case BreedVisualArchetype.SpitzWarden:
                return "Spitz Warden";

            case BreedVisualArchetype.VelocityHound:
                return "Velocity Hound";

            case BreedVisualArchetype.HybridVariant:
                return "Hybrid Variant";

            case BreedVisualArchetype.Unknown:
            default:
                return "Unknown";
        }
    }

    void ApplyTraitVisualAccents(Dog dog, float healthPercent, ref Color color, ref Vector3 scale)
    {
        if (dog == null)
        {
            return;
        }

        if (dog.HasTrait(DogTrait.Aggressive))
        {
            color = Color.Lerp(color, new Color(1f, 0.24f, 0.04f), 0.28f);
        }

        if (dog.HasTrait(DogTrait.Durable))
        {
            color = Color.Lerp(color, new Color(0.35f, 0.95f, 0.85f), 0.25f);
            scale = new Vector3(scale.x * 1.05f, scale.y * 1.03f, scale.z * 1.05f);
        }

        if (dog.HasTrait(DogTrait.GlassCannon))
        {
            color = Color.Lerp(color, Color.white, 0.32f);
            scale = new Vector3(scale.x * 0.92f, scale.y * 1.04f, scale.z * 0.92f);
        }

        if (dog.HasTrait(DogTrait.Clutch) && healthPercent <= 0.35f)
        {
            float pulse = Mathf.PingPong(Time.time * 2.8f, 1f);
            color = Color.Lerp(color, new Color(1f, 0.86f, 0.25f), Mathf.Lerp(0.25f, 0.55f, pulse));
            scale *= Mathf.Lerp(1f, 1.06f, pulse);
        }

        if (dog.HasTrait(DogTrait.LateBloomer))
        {
            color = Color.Lerp(color, new Color(0.34f, 0.85f, 0.72f), 0.18f);
        }

        if (dog.HasTrait(DogTrait.Prodigy))
        {
            color = Color.Lerp(color, new Color(0.85f, 1f, 1f), 0.34f);
            scale *= 1.04f;
        }
    }

    Color ApplyHealthCorruptionVisual(Color cleanColor, float healthPercent)
    {
        float corruptionStrength = 1f - Mathf.Clamp01(healthPercent);

        if (healthPercent <= 0f)
        {
            return Color.Lerp(new Color(0.12f, 0.04f, 0.16f), new Color(0.42f, 0.05f, 0.08f), 0.45f);
        }

        if (healthPercent <= 0.25f)
        {
            return Color.Lerp(cleanColor, new Color(0.7f, 0.08f, 0.22f), Mathf.Lerp(0.45f, 0.72f, corruptionStrength));
        }

        if (healthPercent <= 0.55f)
        {
            return Color.Lerp(cleanColor, new Color(0.75f, 0.16f, 1f), 0.22f);
        }

        return Color.Lerp(cleanColor, Color.white, 0.08f);
    }

    void ApplyDogImprintResultVisual(GameObject artObject, bool isWinner, bool isDraw, Color accentColor)
    {
        if (artObject == null || dogImprintPrefab == null)
        {
            return;
        }

        bool faceRight = ShouldDogArtFaceRight(artObject);

        if (isWinner)
        {
            artObject.transform.localPosition += new Vector3(0f, 0.24f, -0.04f);
            artObject.transform.localScale = GetDogImprintBaseScale() * 1.12f;
            TintDogImprintArt(artObject, Color.Lerp(Color.white, accentColor, 0.25f));
            SetDogArtFacing(artObject, faceRight);
            return;
        }

        if (isDraw)
        {
            artObject.transform.localPosition += new Vector3(0f, 0.08f, 0f);
            artObject.transform.localScale = GetDogImprintBaseScale();
            TintDogImprintArt(artObject, new Color(1f, 0.9f, 0.45f));
            SetDogArtFacing(artObject, faceRight);
            return;
        }

        artObject.transform.localPosition += new Vector3(0f, -0.18f, 0.06f);
        artObject.transform.localScale = GetDogImprintBaseScale() * 0.78f;
        TintDogImprintArt(artObject, new Color(0.38f, 0.22f, 0.48f));
        SetDogArtFacing(artObject, faceRight);
    }

    void TintDogImprintArt(GameObject artObject, Color color)
    {
        if (artObject == null)
        {
            return;
        }

        foreach (Renderer artRenderer in GetRendererList(artObject))
        {
            if (artRenderer == null)
            {
                continue;
            }

            Material runtimeMaterial = artRenderer.material;

            if (runtimeMaterial == null)
            {
                continue;
            }

            if (runtimeMaterial.HasProperty("_BaseColor"))
            {
                runtimeMaterial.SetColor("_BaseColor", color);
            }

            if (runtimeMaterial.HasProperty("_Color"))
            {
                runtimeMaterial.SetColor("_Color", color);
            }
        }
    }

    Renderer[] GetRendererList(GameObject artObject)
    {
        return artObject != null
            ? artObject.GetComponentsInChildren<Renderer>(true)
            : new Renderer[0];
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
        float horizontalScale = Mathf.Lerp(0.46f, 0.64f, corruptionStrength);
        float verticalScale = Mathf.Lerp(0.84f, 0.68f, corruptionStrength);
        float depthScale = Mathf.Lerp(0.46f, 0.36f, corruptionStrength);

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

        return fighterTransform.localPosition + new Vector3(0f, 1.82f, 0f);
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
        fighter.transform.localScale = new Vector3(0.46f, 0.84f, 0.46f);
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

        impactSparkA = CreateArenaImpactEffectObject("ImpactSparkA", PrimitiveType.Sphere, new Vector3(0.32f, 0.32f, 0.32f), Color.red);
        impactSparkB = CreateArenaImpactEffectObject("ImpactSparkB", PrimitiveType.Sphere, new Vector3(0.32f, 0.32f, 0.32f), Color.red);
        corruptionNodeA = CreateArenaImpactEffectObject("CorruptionNodeA", PrimitiveType.Cube, new Vector3(0.24f, 0.24f, 0.24f), new Color(0.75f, 0.1f, 1f));
        corruptionNodeB = CreateArenaImpactEffectObject("CorruptionNodeB", PrimitiveType.Cube, new Vector3(0.24f, 0.24f, 0.24f), new Color(0.75f, 0.1f, 1f));
        impactRingA = CreateArenaImpactEffectObject("ImpactRingA", PrimitiveType.Cylinder, new Vector3(0.62f, 0.025f, 0.62f), new Color(1f, 0.45f, 0.05f));
        impactRingB = CreateArenaImpactEffectObject("ImpactRingB", PrimitiveType.Cylinder, new Vector3(0.62f, 0.025f, 0.62f), new Color(1f, 0.45f, 0.05f));

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
        float scaleMultiplier = Mathf.Lerp(0.75f, 1.35f, intensity);
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

    void CreateStrategyEffectsIfNeeded()
    {
        if (arenaRoot == null)
        {
            return;
        }

        if (strategyEffectA == null)
        {
            strategyEffectA = CreateArenaImpactEffectObject("StrategyEffectA", PrimitiveType.Cube, new Vector3(0.35f, 0.35f, 0.35f), Color.cyan);
        }

        if (strategyEffectB == null)
        {
            strategyEffectB = CreateArenaImpactEffectObject("StrategyEffectB", PrimitiveType.Cube, new Vector3(0.35f, 0.35f, 0.35f), Color.magenta);
        }

        if (defensiveShellA == null)
        {
            defensiveShellA = CreateArenaImpactEffectObject("DefensiveShellA", PrimitiveType.Cylinder, new Vector3(1.1f, 0.08f, 1.1f), new Color(0.2f, 1f, 1f));
        }

        if (defensiveShellB == null)
        {
            defensiveShellB = CreateArenaImpactEffectObject("DefensiveShellB", PrimitiveType.Cylinder, new Vector3(1.1f, 0.08f, 1.1f), new Color(1f, 0.2f, 1f));
        }

        if (styleEffectA == null)
        {
            styleEffectA = CreateArenaImpactEffectObject("StyleEffectA", PrimitiveType.Cube, new Vector3(0.32f, 0.32f, 0.32f), Color.cyan);
        }

        if (styleEffectB == null)
        {
            styleEffectB = CreateArenaImpactEffectObject("StyleEffectB", PrimitiveType.Cube, new Vector3(0.32f, 0.32f, 0.32f), Color.magenta);
        }
    }

    void ShowStrategyEffect(Transform fighterTransform, FightStrategy strategy, string effectName, int roundNumber)
    {
        if (fighterTransform == null)
        {
            return;
        }

        CreateStrategyEffectsIfNeeded();

        if (strategy == FightStrategy.DefensiveShell)
        {
            ShowDefensiveShellEffect(fighterTransform, effectName);
            return;
        }

        GameObject strategyEffect = effectName == "A" ? strategyEffectA : strategyEffectB;

        if (strategyEffect == null)
        {
            return;
        }

        strategyEffect.SetActive(strategy != FightStrategy.Balanced);

        if (!strategyEffect.activeSelf)
        {
            return;
        }

        strategyEffect.transform.localPosition = fighterTransform.localPosition + new Vector3(0f, 1f, -0.08f);
        strategyEffect.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        strategyEffect.transform.localScale = GetStrategyEffectScale(strategy, roundNumber);
        SetObjectUnlitColor(strategyEffect, GetStrategyEffectColor(strategy));
    }

    void ShowDefensiveShellEffect(Transform fighterTransform, string effectName)
    {
        GameObject shellEffect = effectName == "A" ? defensiveShellA : defensiveShellB;

        if (shellEffect == null || fighterTransform == null)
        {
            return;
        }

        shellEffect.SetActive(true);
        shellEffect.transform.localPosition = fighterTransform.localPosition + new Vector3(0f, 0.48f, 0f);
        shellEffect.transform.localRotation = Quaternion.identity;
        shellEffect.transform.localScale = new Vector3(0.98f, 0.055f, 0.98f);
        SetObjectUnlitColor(shellEffect, effectName == "A" ? new Color(0.2f, 1f, 1f) : new Color(1f, 0.2f, 1f));
    }

    void HideStrategyEffects()
    {
        SetImpactEffectActive(strategyEffectA, false);
        SetImpactEffectActive(strategyEffectB, false);
        SetImpactEffectActive(defensiveShellA, false);
        SetImpactEffectActive(defensiveShellB, false);
        SetImpactEffectActive(styleEffectA, false);
        SetImpactEffectActive(styleEffectB, false);
    }

    void UpdateStrategyEffectPositions()
    {
        UpdateSingleStrategyEffectPosition(strategyEffectA, fighterATransform, new Vector3(0f, 1f, -0.08f));
        UpdateSingleStrategyEffectPosition(strategyEffectB, fighterBTransform, new Vector3(0f, 1f, -0.08f));
        UpdateSingleStrategyEffectPosition(defensiveShellA, fighterATransform, new Vector3(0f, 0.48f, 0f));
        UpdateSingleStrategyEffectPosition(defensiveShellB, fighterBTransform, new Vector3(0f, 0.48f, 0f));
        UpdateSingleStrategyEffectPosition(styleEffectA, fighterATransform, new Vector3(0f, 1.18f, -0.14f));
        UpdateSingleStrategyEffectPosition(styleEffectB, fighterBTransform, new Vector3(0f, 1.18f, -0.14f));
    }

    void UpdateSingleStrategyEffectPosition(GameObject effectObject, Transform fighterTransform, Vector3 offset)
    {
        if (effectObject == null || fighterTransform == null || !effectObject.activeSelf)
        {
            return;
        }

        effectObject.transform.localPosition = fighterTransform.localPosition + offset;
    }

    void UpdateStyleEffectVisuals(FightStyle dogAStyle, FightStyle dogBStyle, int roundNumber, float elapsedTime)
    {
        UpdateSingleStyleEffectVisual(styleEffectA, dogAStyle, roundNumber, elapsedTime, 1f);
        UpdateSingleStyleEffectVisual(styleEffectB, dogBStyle, roundNumber, elapsedTime, -1f);
    }

    void UpdateSingleStyleEffectVisual(GameObject effectObject, FightStyle style, int roundNumber, float elapsedTime, float sideDirection)
    {
        if (effectObject == null || !effectObject.activeSelf || style != FightStyle.Wildcard)
        {
            return;
        }

        float flicker = Mathf.Abs(Mathf.Sin((elapsedTime * 24f) + (roundNumber * 1.7f) + sideDirection));
        float jitter = Mathf.Sin((elapsedTime * 18f) + sideDirection) * 0.08f;
        effectObject.transform.localPosition += new Vector3(jitter, 0f, -jitter * 0.5f);
        effectObject.transform.localScale = Vector3.Lerp(new Vector3(0.26f, 0.26f, 0.26f), new Vector3(0.46f, 0.46f, 0.46f), flicker);
        SetObjectUnlitColor(effectObject, Color.Lerp(new Color(0.2f, 0.8f, 1f), GetStyleAccentColor(FightStyle.Wildcard), flicker));
    }

    Color GetStrategyEffectColor(FightStrategy strategy)
    {
        switch (strategy)
        {
            case FightStrategy.RushEarly:
                return new Color(1f, 0.35f, 0.05f);

            case FightStrategy.CounterPlan:
                return new Color(0.2f, 0.75f, 1f);

            case FightStrategy.WearDown:
                return new Color(1f, 0.72f, 0.12f);

            case FightStrategy.AllIn:
                return new Color(1f, 0.05f, 0.02f);

            case FightStrategy.DefensiveShell:
                return new Color(0.2f, 1f, 1f);

            case FightStrategy.Balanced:
            default:
                return new Color(0.65f, 0.9f, 1f);
        }
    }

    Vector3 GetStrategyEffectScale(FightStrategy strategy, int roundNumber)
    {
        switch (strategy)
        {
            case FightStrategy.RushEarly:
                return roundNumber <= 2 ? new Vector3(0.7f, 0.7f, 0.7f) : new Vector3(0.35f, 0.35f, 0.35f);

            case FightStrategy.CounterPlan:
                return new Vector3(0.42f, 0.42f, 0.42f);

            case FightStrategy.WearDown:
                return roundNumber >= 4 ? new Vector3(0.62f, 0.62f, 0.62f) : new Vector3(0.28f, 0.28f, 0.28f);

            case FightStrategy.AllIn:
                return new Vector3(0.9f, 0.9f, 0.9f);

            case FightStrategy.Balanced:
            default:
                return new Vector3(0.3f, 0.3f, 0.3f);
        }
    }

    int GetStrategyImpactVisualIntensity(FightStrategy strategy, int impact, int roundNumber)
    {
        if (impact <= 0)
        {
            return 0;
        }

        switch (strategy)
        {
            case FightStrategy.RushEarly:
                return roundNumber <= 2 ? Mathf.RoundToInt((impact * 1.25f) + 5f) : Mathf.RoundToInt(impact * 0.8f);

            case FightStrategy.CounterPlan:
                return impact >= 5 ? impact + 4 : impact;

            case FightStrategy.WearDown:
                return roundNumber >= 4 ? Mathf.RoundToInt((impact * 1.2f) + 4f) : Mathf.RoundToInt(impact * 0.75f);

            case FightStrategy.DefensiveShell:
                return Mathf.RoundToInt(impact * 0.75f);

            case FightStrategy.AllIn:
                return Mathf.RoundToInt((impact * 1.35f) + 8f);

            case FightStrategy.Balanced:
            default:
                return impact;
        }
    }

    float GetStyleSpeedMultiplier(FightStyle style)
    {
        switch (style)
        {
            case FightStyle.Rushdown:
                return 1.25f;

            case FightStyle.Counter:
                return 1.15f;

            case FightStyle.Tank:
                return 0.8f;

            case FightStyle.Wildcard:
                return 1.1f;

            case FightStyle.Balanced:
            default:
                return 1f;
        }
    }

    float GetStyleLungeMultiplier(FightStyle style)
    {
        switch (style)
        {
            case FightStyle.Rushdown:
                return 1.22f;

            case FightStyle.Counter:
                return 1.05f;

            case FightStyle.Tank:
                return 0.78f;

            case FightStyle.Wildcard:
                return 1.12f;

            case FightStyle.Balanced:
            default:
                return 1f;
        }
    }

    float GetStyleRecoilMultiplier(FightStyle style)
    {
        switch (style)
        {
            case FightStyle.Tank:
                return 0.45f;

            case FightStyle.Counter:
                return 0.72f;

            case FightStyle.Rushdown:
                return 1.08f;

            case FightStyle.Wildcard:
                return 1.2f;

            case FightStyle.Balanced:
            default:
                return 1f;
        }
    }

    float GetStyleSideOffset(FightStyle style, int roundNumber, float sideDirection)
    {
        switch (style)
        {
            case FightStyle.Rushdown:
                return -0.08f;

            case FightStyle.Counter:
                return 0.12f * sideDirection;

            case FightStyle.Wildcard:
                return Mathf.Sin((roundNumber * 2.17f) + sideDirection) * 0.28f;

            case FightStyle.Tank:
            case FightStyle.Balanced:
            default:
                return 0f;
        }
    }

    Vector3 GetStyleWindupOffset(FightStyle style, float forwardDirection)
    {
        switch (style)
        {
            case FightStyle.Counter:
                return new Vector3(-forwardDirection * 0.28f, 0f, 0.1f * forwardDirection);

            case FightStyle.Tank:
                return new Vector3(-forwardDirection * 0.08f, 0f, 0f);

            case FightStyle.Wildcard:
                return new Vector3(-forwardDirection * 0.14f, 0f, 0.24f * forwardDirection);

            case FightStyle.Rushdown:
            case FightStyle.Balanced:
            default:
                return Vector3.zero;
        }
    }

    int GetStyleImpactVisualIntensity(FightStyle style, int impact, int roundNumber)
    {
        if (impact <= 0)
        {
            return 0;
        }

        switch (style)
        {
            case FightStyle.Rushdown:
                return Mathf.RoundToInt((impact * 1.2f) + 3f);

            case FightStyle.Counter:
                return impact + 4;

            case FightStyle.Tank:
                return Mathf.RoundToInt(impact * 0.85f);

            case FightStyle.Wildcard:
                return Mathf.RoundToInt((impact * 1.15f) + (roundNumber % 2 == 0 ? 8f : 2f));

            case FightStyle.Balanced:
            default:
                return impact;
        }
    }

    void ApplyStyleVisualModifier(Transform fighterTransform, FightStyle style, string effectName, int roundNumber, int impact)
    {
        if (fighterTransform == null || style == FightStyle.Balanced)
        {
            return;
        }

        switch (style)
        {
            case FightStyle.Rushdown:
                ShowRushdownStyleEffect(fighterTransform, effectName, impact);
                break;

            case FightStyle.Counter:
                ShowCounterDodgeEffect(fighterTransform, effectName, roundNumber);
                break;

            case FightStyle.Tank:
                ShowTankAbsorbEffect(fighterTransform, effectName);
                break;

            case FightStyle.Wildcard:
                ShowWildcardGlitchEffect(fighterTransform, effectName, roundNumber);
                break;
        }
    }

    void ShowRushdownStyleEffect(Transform fighterTransform, string effectName, int impact)
    {
        GameObject styleEffect = effectName == "A" ? styleEffectA : styleEffectB;

        if (styleEffect == null)
        {
            return;
        }

        float intensity = Mathf.InverseLerp(1f, 35f, Mathf.Max(1, impact));
        styleEffect.SetActive(true);
        styleEffect.transform.localPosition = fighterTransform.localPosition + new Vector3(0f, 1.18f, -0.14f);
        styleEffect.transform.localRotation = Quaternion.Euler(0f, 45f, -18f);
        styleEffect.transform.localScale = Vector3.Lerp(new Vector3(0.22f, 0.22f, 0.22f), new Vector3(0.46f, 0.46f, 0.46f), intensity);
        SetObjectUnlitColor(styleEffect, GetStyleAccentColor(FightStyle.Rushdown));
    }

    void ShowCounterDodgeEffect(Transform fighterTransform, string effectName, int roundNumber)
    {
        GameObject styleEffect = effectName == "A" ? styleEffectA : styleEffectB;

        if (styleEffect == null)
        {
            return;
        }

        float side = effectName == "A" ? 1f : -1f;
        styleEffect.SetActive(true);
        styleEffect.transform.localPosition = fighterTransform.localPosition + new Vector3(0.13f * side, 1.1f, -0.12f);
        styleEffect.transform.localRotation = Quaternion.Euler(0f, 60f + (roundNumber * 12f), 0f);
        styleEffect.transform.localScale = new Vector3(0.32f, 0.1f, 0.32f);
        SetObjectUnlitColor(styleEffect, GetStyleAccentColor(FightStyle.Counter));
    }

    void ShowTankAbsorbEffect(Transform fighterTransform, string effectName)
    {
        ShowDefensiveShellEffect(fighterTransform, effectName);

        GameObject styleEffect = effectName == "A" ? styleEffectA : styleEffectB;

        if (styleEffect == null)
        {
            return;
        }

        styleEffect.SetActive(true);
        styleEffect.transform.localPosition = fighterTransform.localPosition + new Vector3(0f, 0.85f, -0.05f);
        styleEffect.transform.localRotation = Quaternion.identity;
        styleEffect.transform.localScale = new Vector3(0.58f, 0.13f, 0.58f);
        SetObjectUnlitColor(styleEffect, GetStyleAccentColor(FightStyle.Tank));
    }

    void ShowWildcardGlitchEffect(Transform fighterTransform, string effectName, int roundNumber)
    {
        GameObject styleEffect = effectName == "A" ? styleEffectA : styleEffectB;

        if (styleEffect == null)
        {
            return;
        }

        float side = effectName == "A" ? 1f : -1f;
        float jitter = Mathf.Sin((roundNumber * 3.1f) + side) * 0.08f;
        styleEffect.SetActive(true);
        styleEffect.transform.localPosition = fighterTransform.localPosition + new Vector3(jitter, 1.13f, -0.14f);
        styleEffect.transform.localRotation = Quaternion.Euler(0f, 35f + (roundNumber * 37f), 22f * side);
        styleEffect.transform.localScale = new Vector3(0.32f + Mathf.Abs(jitter), 0.32f, 0.32f + Mathf.Abs(jitter));
        SetObjectUnlitColor(styleEffect, roundNumber % 2 == 0 ? GetStyleAccentColor(FightStyle.Wildcard) : new Color(0.2f, 0.8f, 1f));
    }

    IEnumerator PlayCinematicHitBeat(int dogAImpact, int dogBImpact, FightStyle dogAStyle, FightStyle dogBStyle)
    {
        int severity = GetImpactSeverity(dogAImpact, dogBImpact);

        if (severity <= 0)
        {
            yield break;
        }

        Color accentColor = GetDominantStyleAccentColor(dogAImpact, dogBImpact, dogAStyle, dogBStyle);
        string clashMessage = GetClashMessage(dogAImpact, dogBImpact, severity);

        if (severity >= 2)
        {
            PulseArena(severity, accentColor);
        }

        if (severity >= 3)
        {
            PlayCameraPunchAndShake(severity);
        }

        if (severity >= 2 && !string.IsNullOrEmpty(clashMessage))
        {
            ShowClashText(clashMessage, Color.Lerp(accentColor, Color.white, 0.25f), 0.8f + (severity * 0.06f));
        }

        float freezeDuration = Mathf.Lerp(CinematicHitFreezeMinSeconds, CinematicHitFreezeMaxSeconds, Mathf.InverseLerp(1f, 3f, severity));
        yield return new WaitForSeconds(freezeDuration);
        HideClashText();
    }

    int GetImpactSeverity(int dogAImpact, int dogBImpact)
    {
        int highestImpact = Mathf.Max(dogAImpact, dogBImpact);
        int impactDifference = Mathf.Abs(dogAImpact - dogBImpact);

        if (highestImpact <= 0)
        {
            return 0;
        }

        if (highestImpact >= 26 || impactDifference >= 16)
        {
            return 3;
        }

        if (highestImpact >= 17 || impactDifference >= 10)
        {
            return 2;
        }

        if (highestImpact >= 8 || (dogAImpact > 0 && dogBImpact > 0))
        {
            return 1;
        }

        return 0;
    }

    string GetClashMessage(int dogAImpact, int dogBImpact, int severity)
    {
        if (dogAImpact > 0 && dogBImpact > 0)
        {
            return Mathf.Abs(dogAImpact - dogBImpact) <= 3 ? "TRADE" : "CLASH";
        }

        return severity >= 3 ? "HIT" : string.Empty;
    }

    Color GetDominantStyleAccentColor(int dogAImpact, int dogBImpact, FightStyle dogAStyle, FightStyle dogBStyle)
    {
        if (dogAImpact > 0 && dogBImpact > 0 && Mathf.Abs(dogAImpact - dogBImpact) <= 3)
        {
            return Color.Lerp(GetStyleAccentColor(dogAStyle), GetStyleAccentColor(dogBStyle), 0.5f);
        }

        return dogAImpact >= dogBImpact ? GetStyleAccentColor(dogAStyle) : GetStyleAccentColor(dogBStyle);
    }

    Color GetStyleAccentColor(FightStyle style)
    {
        return GetFightStyleAccentColor(style);
    }

    IEnumerator AnimateRoundExchange(
        int roundNumber,
        Dog dogA,
        Dog dogB,
        int dogAHealth,
        int dogBHealth,
        int dogAImpact,
        int dogBImpact,
        FightStrategy dogAStrategy,
        FightStrategy dogBStrategy,
        FightStyle dogAStyle,
        FightStyle dogBStyle
    )
    {
        if (fighterATransform == null || fighterBTransform == null)
        {
            roundAnimationCoroutine = null;
            yield break;
        }

        Vector3 fighterAHome = new Vector3(-1.75f, 0.6f, 0f);
        Vector3 fighterBHome = new Vector3(1.75f, 0.6f, 0f);

        float dogALunge = GetStrategyLungeDistance(dogAStrategy, dogAImpact, roundNumber) * GetStyleLungeMultiplier(dogAStyle);
        float dogBLunge = GetStrategyLungeDistance(dogBStrategy, dogBImpact, roundNumber) * GetStyleLungeMultiplier(dogBStyle);
        float dogARecoil = GetStrategyRecoilDistance(dogAStrategy, dogBImpact, dogAImpact, roundNumber) * GetStyleRecoilMultiplier(dogAStyle);
        float dogBRecoil = GetStrategyRecoilDistance(dogBStrategy, dogAImpact, dogBImpact, roundNumber) * GetStyleRecoilMultiplier(dogBStyle);
        float dogASideOffset = GetStyleSideOffset(dogAStyle, roundNumber, 1f);
        float dogBSideOffset = GetStyleSideOffset(dogBStyle, roundNumber, -1f);

        Vector3 fighterAImpactPosition = fighterAHome + new Vector3(dogALunge - dogARecoil, 0f, dogASideOffset);
        Vector3 fighterBImpactPosition = fighterBHome + new Vector3(-dogBLunge + dogBRecoil, 0f, dogBSideOffset);
        Vector3 fighterAWindupPosition = GetStrategyWindupPosition(fighterAHome, dogAStrategy, 1f) + GetStyleWindupOffset(dogAStyle, 1f);
        Vector3 fighterBWindupPosition = GetStrategyWindupPosition(fighterBHome, dogBStrategy, -1f) + GetStyleWindupOffset(dogBStyle, -1f);

        fighterAImpactPosition.x = Mathf.Clamp(fighterAImpactPosition.x, -2.4f, -0.35f);
        fighterBImpactPosition.x = Mathf.Clamp(fighterBImpactPosition.x, 0.35f, 2.4f);

        CreateArenaImpactEffectsIfNeeded();
        CreateStrategyEffectsIfNeeded();
        HideImpactEffects();
        HideStrategyEffects();

        bool hasWindup = Vector3.Distance(fighterAHome, fighterAWindupPosition) > 0.01f ||
                         Vector3.Distance(fighterBHome, fighterBWindupPosition) > 0.01f;

        if (!hasWindup && (dogAImpact > 0 || dogBImpact > 0))
        {
            fighterAWindupPosition = GetDefaultReadableWindupPosition(fighterAHome, 1f, dogAImpact);
            fighterBWindupPosition = GetDefaultReadableWindupPosition(fighterBHome, -1f, dogBImpact);
            hasWindup = true;
        }

        float adjustedRoundDuration = RoundActionDurationSeconds;
        float windupDuration = hasWindup ? adjustedRoundDuration * 0.3f : 0f;
        float strikeDuration = adjustedRoundDuration * (hasWindup ? 0.5f : 0.58f);
        float resetDuration = adjustedRoundDuration - windupDuration - strikeDuration;

        if (hasWindup)
        {
            yield return AnimateFightersToPositions(dogA, dogB, dogAHealth, dogBHealth, fighterAHome, fighterBHome, fighterAWindupPosition, fighterBWindupPosition, windupDuration, dogAStyle, dogBStyle, roundNumber);
        }

        yield return AnimateFightersToPositions(dogA, dogB, dogAHealth, dogBHealth, fighterAWindupPosition, fighterBWindupPosition, fighterAImpactPosition, fighterBImpactPosition, strikeDuration, dogAStyle, dogBStyle, roundNumber);
        yield return HoldFightersAtPositions(dogA, dogB, dogAHealth, dogBHealth, fighterAImpactPosition, fighterBImpactPosition, 0.16f, dogAStyle, dogBStyle, roundNumber);
        ShowStrategyEffect(fighterATransform, dogAStrategy, "A", roundNumber);
        ShowStrategyEffect(fighterBTransform, dogBStrategy, "B", roundNumber);
        ApplyStyleVisualModifier(fighterATransform, dogAStyle, "A", roundNumber, dogAImpact);
        ApplyStyleVisualModifier(fighterBTransform, dogBStyle, "B", roundNumber, dogBImpact);
        ShowRoundImpactEffects(dogAImpact, dogBImpact, dogAStrategy, dogBStrategy, dogAStyle, dogBStyle, roundNumber);
        yield return PlayCinematicHitBeat(dogAImpact, dogBImpact, dogAStyle, dogBStyle);
        yield return AnimateFightersToPositions(dogA, dogB, dogAHealth, dogBHealth, fighterAImpactPosition, fighterBImpactPosition, fighterAHome, fighterBHome, Mathf.Max(0.05f, resetDuration), dogAStyle, dogBStyle, roundNumber);

        HideImpactEffects();
        HideStrategyEffects();
        roundAnimationCoroutine = null;
        ResetFighterArenaPositions();
        UpdateDogPortraitBillboards(dogA, dogB);
        UpdateImprintCorruptionVisuals(dogAHealth, dogBHealth);
        UpdateArenaLabels(dogA, dogB);
        UpdateRoundStatusBanner(roundNumber, dogAHealth, dogBHealth, dogAImpact, dogBImpact, false, dogAStrategy, dogBStrategy, dogAStyle, dogBStyle);
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
        float duration,
        FightStyle dogAStyle,
        FightStyle dogBStyle,
        int roundNumber
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

            UpdateMovingFighterVisualPositions(dogA, dogB, dogAHealth, dogBHealth);
            UpdateStrategyEffectPositions();
            UpdateStyleEffectVisuals(dogAStyle, dogBStyle, roundNumber, elapsedTime);
            yield return null;
        }
    }

    IEnumerator HoldFightersAtPositions(
        Dog dogA,
        Dog dogB,
        int dogAHealth,
        int dogBHealth,
        Vector3 fighterAPosition,
        Vector3 fighterBPosition,
        float duration,
        FightStyle dogAStyle,
        FightStyle dogBStyle,
        int roundNumber
    )
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            if (fighterATransform != null)
            {
                fighterATransform.localPosition = fighterAPosition;
            }

            if (fighterBTransform != null)
            {
                fighterBTransform.localPosition = fighterBPosition;
            }

            UpdateMovingFighterVisualPositions(dogA, dogB, dogAHealth, dogBHealth);
            UpdateStrategyEffectPositions();
            UpdateStyleEffectVisuals(dogAStyle, dogBStyle, roundNumber, elapsedTime);
            yield return null;
        }
    }

    void UpdateMovingFighterVisualPositions(Dog dogA, Dog dogB, int dogAHealth, int dogBHealth)
    {
        if (dogImprintPrefab != null)
        {
            UpdateSingleDogImprintArtPosition(fighterADogImprintArt, fighterATransform, true);
            UpdateSingleDogImprintArtPosition(fighterBDogImprintArt, fighterBTransform, false);
        }

        UpdateSingleBreedArchetypeArtPosition(fighterABreedArchetypeArt, fighterATransform, currentDogImprintA);
        UpdateSingleBreedArchetypeArtPosition(fighterBBreedArchetypeArt, fighterBTransform, currentDogImprintB);
        FaceBreedArchetypeArtsTowardPresentationCamera();
        UpdateFighterFacingDirections();
        UpdateFighterContactShadows();
        UpdatePortraitBillboardPositions();
        FacePortraitsTowardPresentationCamera();
        UpdateArenaLabels(dogA, dogB);
        UpdateHealthBarPositionsOnly();
    }

    void UpdateHealthBarPositionsOnly()
    {
        UpdateSingleHealthBarPositionOnly(healthBarBackgroundA, healthBarFillA, fighterATransform, -1);
        UpdateSingleHealthBarPositionOnly(healthBarBackgroundB, healthBarFillB, fighterBTransform, 1);
    }

    void UpdateSingleHealthBarPositionOnly(GameObject backgroundBar, GameObject fillBar, Transform fighterTransform, int fillDirection)
    {
        if (backgroundBar == null || fillBar == null || fighterTransform == null)
        {
            return;
        }

        Vector3 basePosition = PositionHealthBarAboveFighter(fighterTransform);
        float fullWidth = Mathf.Max(0.05f, Mathf.Abs(backgroundBar.transform.localScale.x));
        float fillWidth = Mathf.Max(0.05f, Mathf.Abs(fillBar.transform.localScale.x));
        float fillOffset = ((fullWidth - fillWidth) * 0.5f) * fillDirection;

        backgroundBar.transform.localPosition = basePosition;
        fillBar.transform.localPosition = basePosition + new Vector3(fillOffset, 0.02f, -0.02f);
    }

    void StopRoundAnimationIfRunning()
    {
        if (roundAnimationCoroutine == null)
        {
            HideImpactEffects();
            HideStrategyEffects();
            HideClashText();
            StopCinematicCameraIfRunning();
            StopArenaPulseIfRunning();
            return;
        }

        StopCoroutine(roundAnimationCoroutine);
        roundAnimationCoroutine = null;
        HideImpactEffects();
        HideStrategyEffects();
        HideClashText();
        StopCinematicCameraIfRunning();
        StopArenaPulseIfRunning();
    }

    void ResetFighterArenaPositions()
    {
        if (fighterATransform != null)
        {
            fighterATransform.localPosition = new Vector3(-1.75f, 0.6f, 0f);
            fighterATransform.localScale = new Vector3(0.46f, 0.84f, 0.46f);
            SetObjectUnlitColor(fighterATransform.gameObject, new Color(0f, 0.58f, 0.78f));
        }

        if (fighterBTransform != null)
        {
            fighterBTransform.localPosition = new Vector3(1.75f, 0.6f, 0f);
            fighterBTransform.localScale = new Vector3(0.46f, 0.84f, 0.46f);
            SetObjectUnlitColor(fighterBTransform.gameObject, new Color(0.78f, 0.1f, 0.68f));
        }

        SetCapsuleFallbackVisible(!HasDogImprintArt());
        UpdateDogImprintArtPositions();
    }

    bool HasDogImprintArt()
    {
        return dogImprintPrefab != null && fighterADogImprintArt != null && fighterBDogImprintArt != null;
    }

    float GetStrategyLungeDistance(FightStrategy strategy, int impact, int roundNumber)
    {
        float baseLunge = GetImpactLungeDistance(impact);

        switch (strategy)
        {
            case FightStrategy.RushEarly:
                return roundNumber <= 2
                    ? Mathf.Clamp(Mathf.Max(baseLunge, 0.4f) + 0.25f, 0f, 1.05f)
                    : Mathf.Clamp(baseLunge * 0.65f, 0f, 0.55f);

            case FightStrategy.CounterPlan:
                return impact >= 5 ? Mathf.Clamp((baseLunge * 1.15f) + 0.1f, 0f, 0.85f) : 0.05f;

            case FightStrategy.WearDown:
                return roundNumber >= 4
                    ? Mathf.Clamp((baseLunge * 1.35f) + 0.18f, 0f, 0.95f)
                    : Mathf.Clamp(baseLunge * 0.55f, 0f, 0.4f);

            case FightStrategy.DefensiveShell:
                return Mathf.Clamp(baseLunge * 0.25f, 0f, 0.18f);

            case FightStrategy.AllIn:
                return Mathf.Clamp(Mathf.Max(baseLunge * 1.55f, 0.4f) + 0.25f, 0f, 1.15f);

            case FightStrategy.Balanced:
            default:
                return baseLunge;
        }
    }

    float GetStrategyRecoilDistance(FightStrategy strategy, int incomingImpact, int ownImpact, int roundNumber)
    {
        float baseRecoil = GetImpactRecoilDistance(incomingImpact, ownImpact);

        switch (strategy)
        {
            case FightStrategy.DefensiveShell:
                return baseRecoil * 0.35f;

            case FightStrategy.CounterPlan:
                return baseRecoil * 0.75f;

            case FightStrategy.WearDown:
                return roundNumber >= 4 ? baseRecoil * 0.8f : baseRecoil;

            case FightStrategy.AllIn:
                return incomingImpact > ownImpact ? baseRecoil * 1.55f : baseRecoil * 1.1f;

            case FightStrategy.RushEarly:
                return roundNumber <= 2 ? baseRecoil * 0.9f : baseRecoil * 1.1f;

            case FightStrategy.Balanced:
            default:
                return baseRecoil;
        }
    }

    Vector3 GetStrategyWindupPosition(Vector3 homePosition, FightStrategy strategy, float forwardDirection)
    {
        switch (strategy)
        {
            case FightStrategy.CounterPlan:
                return homePosition + new Vector3(-forwardDirection * 0.42f, 0f, 0f);

            case FightStrategy.AllIn:
                return homePosition + new Vector3(-forwardDirection * 0.22f, 0f, 0f);

            case FightStrategy.DefensiveShell:
                return homePosition + new Vector3(-forwardDirection * 0.08f, 0f, 0f);

            case FightStrategy.RushEarly:
            case FightStrategy.WearDown:
            case FightStrategy.Balanced:
            default:
                return homePosition;
        }
    }

    Vector3 GetDefaultReadableWindupPosition(Vector3 homePosition, float forwardDirection, int impact)
    {
        if (impact <= 0)
        {
            return homePosition;
        }

        float pullbackDistance = Mathf.Clamp(0.16f + (impact * 0.004f), 0.16f, 0.28f);
        return homePosition + new Vector3(-forwardDirection * pullbackDistance, 0f, 0f);
    }

    void ShowRoundImpactEffects(
        int dogAImpact,
        int dogBImpact,
        FightStrategy dogAStrategy,
        FightStrategy dogBStrategy,
        FightStyle dogAStyle,
        FightStyle dogBStyle,
        int roundNumber
    )
    {
        CreateArenaImpactEffectsIfNeeded();

        if (dogAImpact > 0)
        {
            int visualImpact = GetStyleImpactVisualIntensity(
                dogAStyle,
                GetStrategyImpactVisualIntensity(dogAStrategy, dogAImpact, roundNumber),
                roundNumber
            );
            ShowImpactEffect(fighterBTransform, visualImpact, "B");
        }

        if (dogBImpact > 0)
        {
            int visualImpact = GetStyleImpactVisualIntensity(
                dogBStyle,
                GetStrategyImpactVisualIntensity(dogBStrategy, dogBImpact, roundNumber),
                roundNumber
            );
            ShowImpactEffect(fighterATransform, visualImpact, "A");
        }
    }

    float GetImpactLungeDistance(int impact)
    {
        if (impact <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp(0.42f + (impact * 0.022f), 0.42f, 1.05f);
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
        fighterTransform.localScale = new Vector3(0.65f, 1f, 0.65f);
        SetObjectUnlitColor(fighterTransform.gameObject, new Color(0.1f, 1f, 0.35f));
    }

    void MarkLoser(Transform fighterTransform)
    {
        if (fighterTransform == null)
        {
            return;
        }

        fighterTransform.localPosition = new Vector3(fighterTransform.localPosition.x, 0.35f, fighterTransform.localPosition.z);
        fighterTransform.localScale = new Vector3(0.34f, 0.5f, 0.34f);
        SetObjectUnlitColor(fighterTransform.gameObject, new Color(0.25f, 0.25f, 0.3f));
    }

    void MarkDraw(Transform fighterTransform)
    {
        if (fighterTransform == null)
        {
            return;
        }

        fighterTransform.localPosition = new Vector3(fighterTransform.localPosition.x, 0.8f, fighterTransform.localPosition.z);
        fighterTransform.localScale = new Vector3(0.55f, 0.9f, 0.55f);
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

    void PulseArena(int severity, Color accentColor)
    {
        if (arenaRoot == null || severity <= 0)
        {
            return;
        }

        StopArenaPulseIfRunning();
        arenaPulseCoroutine = StartCoroutine(PulseArenaRoutine(severity, accentColor));
    }

    IEnumerator PulseArenaRoutine(int severity, Color accentColor)
    {
        float duration = Mathf.Lerp(0.1f, 0.2f, Mathf.InverseLerp(1f, 3f, severity));
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float pulse = Mathf.Sin(progress * Mathf.PI);
            SetArenaPulseVisual(pulse, severity, accentColor);
            yield return null;
        }

        arenaPulseCoroutine = null;
        ResetArenaPulseVisual();
    }

    void StopArenaPulseIfRunning()
    {
        if (arenaPulseCoroutine == null)
        {
            return;
        }

        StopCoroutine(arenaPulseCoroutine);
        arenaPulseCoroutine = null;
        ResetArenaPulseVisual();
    }

    void SetArenaPulseVisual(float pulse, int severity, Color accentColor)
    {
        Color platformBaseColor = new Color(0.01f, 0.014f, 0.022f);
        Color gridBaseColor = new Color(0f, 0.78f, 1f);
        Color borderBaseColor = new Color(0.25f, 1f, 1f);
        float strength = Mathf.Lerp(0.08f, 0.22f, Mathf.InverseLerp(1f, 3f, severity)) * pulse;

        GameObject platform = GetArenaChildObject("DigitalArenaPlatform");

        if (platform != null)
        {
            platform.transform.localScale = Vector3.Lerp(new Vector3(5.8f, 0.08f, 3.55f), new Vector3(5.88f, 0.082f, 3.6f), strength);
            SetObjectUnlitColor(platform, Color.Lerp(platformBaseColor, accentColor, strength * 0.28f));
        }

        for (int i = -3; i <= 3; i++)
        {
            ApplyArenaLinePulse($"GridLine_X_{i}", new Vector3(0.065f, 0.035f, 3.65f), Color.Lerp(gridBaseColor, accentColor, strength));
        }

        for (int i = -2; i <= 2; i++)
        {
            ApplyArenaLinePulse($"GridLine_Z_{i}", new Vector3(5.95f, 0.035f, 0.065f), Color.Lerp(gridBaseColor, accentColor, strength));
        }

        ApplyArenaLinePulse("ArenaBorder_North", new Vector3(6.15f, 0.075f, 0.11f), Color.Lerp(borderBaseColor, accentColor, strength));
        ApplyArenaLinePulse("ArenaBorder_South", new Vector3(6.15f, 0.075f, 0.11f), Color.Lerp(borderBaseColor, accentColor, strength));
        ApplyArenaLinePulse("ArenaBorder_East", new Vector3(0.11f, 0.075f, 3.75f), Color.Lerp(borderBaseColor, accentColor, strength));
        ApplyArenaLinePulse("ArenaBorder_West", new Vector3(0.11f, 0.075f, 3.75f), Color.Lerp(borderBaseColor, accentColor, strength));
    }

    void ApplyArenaLinePulse(string objectName, Vector3 baseScale, Color color)
    {
        GameObject lineObject = GetArenaChildObject(objectName);

        if (lineObject == null)
        {
            return;
        }

        lineObject.transform.localScale = new Vector3(baseScale.x * 1.01f, baseScale.y * 1.12f, baseScale.z * 1.01f);
        SetObjectUnlitColor(lineObject, color);
    }

    void ResetArenaPulseVisual()
    {
        if (arenaRoot == null)
        {
            return;
        }

        CreatePlatform();
        CreateGridLines();
    }

    GameObject GetArenaChildObject(string objectName)
    {
        if (arenaRoot == null || string.IsNullOrEmpty(objectName))
        {
            return null;
        }

        Transform childTransform = arenaRoot.transform.Find(objectName);
        return childTransform != null ? childTransform.gameObject : null;
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
