using UnityEngine;

public class FightPresentationManager : MonoBehaviour
{
    private const string ArenaRootName = "ArenaRoot";

    private static GameObject sharedArenaRoot;

    private GameObject arenaRoot;
    private bool arenaObjectsCreated;

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
        CreateFighterPlaceholder("FighterA_Imprint", new Vector3(-2f, 0.6f, 0f), Color.cyan);
        CreateFighterPlaceholder("FighterB_Imprint", new Vector3(2f, 0.6f, 0f), Color.magenta);
        CreateMarker("CenterMarker", new Vector3(0f, 0.05f, 0f), Color.white);
        CreateWall("BackGridWall", new Vector3(0f, 1.4f, 2.6f), new Vector3(5.5f, 2.5f, 0.08f), new Color(0.1f, 0.25f, 0.35f, 0.7f));
        CreateGridLines();

        arenaObjectsCreated = true;
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

    void CreateFighterPlaceholder(string objectName, Vector3 position, Color color)
    {
        GameObject fighter = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        fighter.name = objectName;
        fighter.transform.SetParent(arenaRoot.transform);
        fighter.transform.localPosition = position;
        fighter.transform.localScale = new Vector3(0.7f, 1.2f, 0.7f);
        SetObjectColor(fighter, color);
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
