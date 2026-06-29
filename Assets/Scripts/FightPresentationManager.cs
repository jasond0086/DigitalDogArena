using System.Collections;
using UnityEngine;

public class FightPresentationManager : MonoBehaviour
{
    private const string ArenaRootName = "ArenaRoot";
    private const string ScanChamberRootName = "ScanChamberRoot";
    private const float ScanIntroDelaySeconds = 1.5f;

    private static GameObject sharedArenaRoot;
    private static GameObject sharedScanChamberRoot;

    private GameObject arenaRoot;
    private GameObject scanChamberRoot;
    private bool arenaObjectsCreated;
    private bool scanChamberObjectsCreated;
    private Transform fighterATransform;
    private Transform fighterBTransform;
    private Transform scanDogATransform;
    private Transform scanDogBTransform;
    private Coroutine scanIntroCoroutine;

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

        CreateArenaObjectsIfNeeded();
        arenaRoot.SetActive(true);

        Debug.Log($"Digital arena ready: {dogA.dogName} imprint vs {dogB.dogName} imprint.");
    }

    public void HideArena()
    {
        EnsureArenaRoot();

        if (arenaRoot != null)
        {
            arenaRoot.SetActive(false);
        }
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
        EnsureScanChamberRoot();

        if (scanChamberRoot == null)
        {
            Debug.LogWarning("FightPresentationManager could not create ScanChamberRoot.");
            return;
        }

        CreateScanChamberObjectsIfNeeded();
        PositionScanSubjects();
        scanChamberRoot.SetActive(true);

        Debug.Log($"DNA scan started for {dogA.dogName} and {dogB.dogName}. Real dogs remain safe. Digital imprints are being copied.");

        scanIntroCoroutine = StartCoroutine(ScanIntroRoutine(dogA, dogB));
    }

    IEnumerator ScanIntroRoutine(Dog dogA, Dog dogB)
    {
        // This short pause lets the scan chamber read as an intro before the digital arena appears.
        yield return new WaitForSeconds(ScanIntroDelaySeconds);

        HideScanChamber();
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
        arenaRoot.SetActive(true);

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

        Debug.Log($"Digital arena round {roundNumber}: {dogA.dogName} HP {dogAHealth} vs {dogB.dogName} HP {dogBHealth}.");
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
        arenaRoot.SetActive(true);

        if (dogAHealth > dogBHealth)
        {
            MarkWinner(fighterATransform);
            MarkLoser(fighterBTransform);
            Debug.Log($"Digital arena result: {dogA.dogName} imprint wins. {dogB.dogName} imprint falls back.");
            return;
        }

        if (dogBHealth > dogAHealth)
        {
            MarkWinner(fighterBTransform);
            MarkLoser(fighterATransform);
            Debug.Log($"Digital arena result: {dogB.dogName} imprint wins. {dogA.dogName} imprint falls back.");
            return;
        }

        MarkDraw(fighterATransform);
        MarkDraw(fighterBTransform);
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

    void CreateArenaObjectsIfNeeded()
    {
        if (arenaObjectsCreated)
        {
            return;
        }

        if (arenaRoot.transform.childCount > 0)
        {
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
