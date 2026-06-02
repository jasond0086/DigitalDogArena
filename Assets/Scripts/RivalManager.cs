using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[System.Serializable]
public class RivalHandlerData
{
    public string rivalId;
    public string handlerName;
    public string leagueName;
    [TextArea] public string introText;
    [TextArea] public string winText;
    [TextArea] public string lossText;
    public bool isDefeated;
    public Dog rivalDog;
}

[System.Serializable]
public class RivalSaveData
{
    public List<string> defeatedRivalIds = new List<string>();
}

public class RivalManager : MonoBehaviour
{
    private const string RivalSaveKey = "RIVAL_STATE_SAVE";

    [Header("References")]
    public DogManager dogManager;
    public LeagueManager leagueManager;
    public TextMeshProUGUI rivalStatusText;

    [Header("Rival Handlers")]
    public List<RivalHandlerData> rivals = new List<RivalHandlerData>();

    void Awake()
    {
        if (dogManager == null)
        {
            dogManager = GetComponent<DogManager>();
        }

        if (leagueManager == null)
        {
            leagueManager = GetComponent<LeagueManager>();
        }

        InitializeDefaultRivals();
        LoadRivalState();
        RefreshRivalStatusUI();
    }

    void Start()
    {
        RefreshRivalStatusUI();
    }

    public List<RivalHandlerData> GetAvailableRivals()
    {
        InitializeDefaultRivals();

        List<RivalHandlerData> availableRivals = new List<RivalHandlerData>();

        foreach (RivalHandlerData rival in rivals)
        {
            if (rival == null)
            {
                continue;
            }

            if (leagueManager != null && leagueManager.IsLeagueUnlocked(rival.leagueName))
            {
                availableRivals.Add(rival);
            }
        }

        return availableRivals;
    }

    public RivalHandlerData GetCurrentRival()
    {
        foreach (RivalHandlerData rival in GetAvailableRivals())
        {
            if (!rival.isDefeated)
            {
                return rival;
            }
        }

        return null;
    }

    public string GetRivalSummaryText()
    {
        StringBuilder summary = new StringBuilder();
        List<RivalHandlerData> availableRivals = GetAvailableRivals();
        RivalHandlerData currentRival = GetCurrentRival();

        summary.AppendLine("RIVAL HANDLERS");
        summary.AppendLine();

        if (currentRival != null)
        {
            summary.AppendLine($"CURRENT RIVAL: {currentRival.handlerName}");
            summary.AppendLine($"LEAGUE: {currentRival.leagueName}");
            summary.AppendLine($"DOG: {currentRival.rivalDog.dogName}");
            summary.AppendLine(currentRival.introText);
            summary.AppendLine();
        }
        else
        {
            summary.AppendLine("No undefeated rival is currently available.");
            summary.AppendLine();
        }

        foreach (RivalHandlerData rival in availableRivals)
        {
            string status = rival.isDefeated ? "DEFEATED" : "ACTIVE";
            summary.AppendLine($"{rival.leagueName}: {rival.handlerName} - {status}");
            summary.AppendLine($"Dog: {rival.rivalDog.dogName} | Style: {rival.rivalDog.fightStyle}");
        }

        return summary.ToString();
    }

    public void MarkRivalDefeated(string rivalId)
    {
        RivalHandlerData rival = FindRival(rivalId);

        if (rival == null)
        {
            return;
        }

        rival.isDefeated = true;
        RefreshCompletedLeagues();
        SaveRivalState();
        RefreshRivalStatusUI();
    }

    public void SaveRivalState()
    {
        RivalSaveData saveData = new RivalSaveData();

        foreach (RivalHandlerData rival in rivals)
        {
            if (rival != null && rival.isDefeated)
            {
                saveData.defeatedRivalIds.Add(rival.rivalId);
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(RivalSaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadRivalState()
    {
        if (!PlayerPrefs.HasKey(RivalSaveKey))
        {
            return;
        }

        string json = PlayerPrefs.GetString(RivalSaveKey);
        RivalSaveData saveData = JsonUtility.FromJson<RivalSaveData>(json);

        if (saveData == null)
        {
            return;
        }

        foreach (RivalHandlerData rival in rivals)
        {
            rival.isDefeated = saveData.defeatedRivalIds != null &&
                               saveData.defeatedRivalIds.Contains(rival.rivalId);
        }

        RefreshCompletedLeagues();
    }

    public void ClearRivalSave()
    {
        PlayerPrefs.DeleteKey(RivalSaveKey);

        foreach (RivalHandlerData rival in rivals)
        {
            if (rival != null)
            {
                rival.isDefeated = false;
            }
        }

        if (leagueManager != null)
        {
            leagueManager.ClearRivalLeagueCompletions();
        }

        SaveRivalState();
        RefreshRivalStatusUI();
    }

    public void RefreshRivalStatusUI()
    {
        string summaryText = GetRivalSummaryText();
        Debug.Log($"Rival Status UI:\n{summaryText}");

        if (rivalStatusText != null)
        {
            rivalStatusText.text = summaryText;
        }
    }

    void InitializeDefaultRivals()
    {
        if (rivals.Count > 0)
        {
            return;
        }

        rivals.Add(CreateRival(
            "street_chainlink",
            "Mason \"Chainlink\" Crowe",
            "Street League",
            "Chainlink waits at the first gate with a steady fighter and a patient stare. He wants to know whether your stable has discipline.",
            "Chainlink nods once. Your name has weight in the Street League now.",
            "Chainlink leaves the feed running just long enough for the loss to settle.",
            CreateRivalDog("chainlink_dog", "Lockjaw", 58, 55, 62, 70, 68, 72, FightStyle.Balanced, DogTrait.Durable)
        ));

        rivals.Add(CreateRival(
            "local_vera_knox",
            "Vera Knox",
            "Local Circuit",
            "Vera Knox studies every opening. Her fighter moves like it already knows where your command will break.",
            "Vera accepts the record without excuses. The Local Circuit takes notice.",
            "Vera closes the match with cold timing and a cleaner plan.",
            CreateRivalDog("vera_dog", "Switchblade", 60, 78, 64, 74, 92, 76, FightStyle.Counter, DogTrait.Clutch)
        ));

        rivals.Add(CreateRival(
            "underground_bishop_vale",
            "Bishop Vale",
            "Underground Circuit",
            "Bishop Vale arrives through a side channel with a fighter built for pressure. Every command sounds like a wager.",
            "Bishop disappears from the feed. The underground record keeps your answer.",
            "Bishop's fighter turns risk into momentum and leaves your stable with a lesson.",
            CreateRivalDog("bishop_dog", "Blackout", 86, 76, 60, 100, 92, 72, FightStyle.Rushdown, DogTrait.Aggressive)
        ));

        rivals.Add(CreateRival(
            "elite_sable_cross",
            "Sable Cross",
            "Elite Circuit",
            "Sable Cross brings a fighter with old patience and brutal control of the pace. The Elite Circuit watches in silence.",
            "Sable studies the result, then steps aside. Your bloodline belongs in the higher rooms.",
            "Sable's fighter absorbs the pressure and closes the gate one exchange at a time.",
            CreateRivalDog("sable_dog", "Bastion", 84, 66, 94, 104, 82, 112, FightStyle.Tank, DogTrait.Durable)
        ));

        rivals.Add(CreateRival(
            "apex_orion_black",
            "Orion Black",
            "Apex League",
            "Orion Black stands at the final gate with a hybrid fighter shaped by every lesson the arena can teach.",
            "Orion gives a quiet signal. The Apex record has a new name written into it.",
            "Orion's fighter answers every shift in the match. The final gate remains closed for now.",
            CreateRivalDog("orion_dog", "Event Horizon", 98, 98, 98, 118, 118, 118, FightStyle.Wildcard, DogTrait.Prodigy)
        ));
    }

    RivalHandlerData CreateRival(
        string rivalId,
        string handlerName,
        string leagueName,
        string introText,
        string winText,
        string lossText,
        Dog rivalDog
    )
    {
        return new RivalHandlerData
        {
            rivalId = rivalId,
            handlerName = handlerName,
            leagueName = leagueName,
            introText = introText,
            winText = winText,
            lossText = lossText,
            isDefeated = false,
            rivalDog = rivalDog
        };
    }

    Dog CreateRivalDog(
        string dogId,
        string dogName,
        int strength,
        int agility,
        int stamina,
        int strengthPotential,
        int agilityPotential,
        int staminaPotential,
        FightStyle fightStyle,
        DogTrait primaryTrait
    )
    {
        Dog dog = ScriptableObject.CreateInstance<Dog>();
        dog.dogId = dogId;
        dog.dogName = dogName;
        dog.breed = "Rival Bloodline";
        dog.strength = strength;
        dog.agility = agility;
        dog.stamina = stamina;
        dog.strengthPotential = strengthPotential;
        dog.agilityPotential = agilityPotential;
        dog.staminaPotential = staminaPotential;
        dog.fightStyle = fightStyle;
        dog.primaryTrait = primaryTrait;
        dog.secondaryTrait = DogTrait.None;

        return dog;
    }

    RivalHandlerData FindRival(string rivalId)
    {
        foreach (RivalHandlerData rival in rivals)
        {
            if (rival != null && rival.rivalId == rivalId)
            {
                return rival;
            }
        }

        return null;
    }

    public void RefreshCompletedLeagues()
    {
        if (leagueManager == null)
        {
            return;
        }

        HashSet<string> leagueNames = new HashSet<string>();

        foreach (RivalHandlerData rival in rivals)
        {
            if (rival != null)
            {
                leagueNames.Add(rival.leagueName);
            }
        }

        foreach (string leagueName in leagueNames)
        {
            if (AreAllRivalsInLeagueDefeated(leagueName))
            {
                leagueManager.MarkLeagueCompleted(leagueName);
            }
        }
    }

    bool AreAllRivalsInLeagueDefeated(string leagueName)
    {
        bool foundRival = false;

        foreach (RivalHandlerData rival in rivals)
        {
            if (rival == null || rival.leagueName != leagueName)
            {
                continue;
            }

            foundRival = true;

            if (!rival.isDefeated)
            {
                return false;
            }
        }

        return foundRival;
    }
}
