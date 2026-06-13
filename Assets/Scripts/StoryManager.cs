using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class StoryEventData
{
    public string eventId;
    public string title;
    [TextArea] public string bodyText;
    public string requiredLeagueName;
    public int requiredTotalWins;
    public int requiredUndergroundReputation;
    public string requiredSeenEventId;
    public bool hasBeenSeen;
    public List<StoryChoiceData> choices = new List<StoryChoiceData>();
}

[System.Serializable]
public class StoryChoiceData
{
    public string choiceText;
    [TextArea] public string resultText;
    public int reputationChange;
    public int undergroundReputationChange;
    public float riskModifierChange;
    public string unlockEventId;
}

[System.Serializable]
public class StorySaveData
{
    public int reputation;
    public int undergroundReputation;
    public float riskModifier;
    public List<string> seenEventIds = new List<string>();
}

public class StoryManager : MonoBehaviour
{
    private const string StorySaveKey = "STORY_STATE_SAVE";

    [Header("References")]
    public DogManager dogManager;
    public LeagueManager leagueManager;
    public NarratorManager narratorManager;

    [Header("Story UI")]
    public TextMeshProUGUI storyText;
    public Button choice1Button;
    public Button choice2Button;
    public Button choice3Button;
    public TextMeshProUGUI choice1Text;
    public TextMeshProUGUI choice2Text;
    public TextMeshProUGUI choice3Text;

    [Header("Story State")]
    public int reputation = 0;
    public int undergroundReputation = 0;
    public float riskModifier = 0f;

    [Header("Story Events")]
    public List<StoryEventData> storyEvents = new List<StoryEventData>();

    private StoryEventData activeStoryEvent;

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

        if (narratorManager == null)
        {
            narratorManager = GetComponent<NarratorManager>();
        }

        InitializeDefaultEvents();
        LoadStoryState();
        RefreshStoryUI();
    }

    void Start()
    {
        RefreshStoryUI();
    }

    public void InitializeDefaultEvents()
    {
        if (storyEvents.Count > 0)
        {
            EnsureUndergroundPathEvent();
            return;
        }

        storyEvents.Add(CreateStoryEvent(
            "first_offer",
            "The First Offer",
            "A message flickers through the arena feed. Someone has noticed your stable before your name has earned weight.",
            "Street League",
            0,
            new List<StoryChoiceData>
            {
                CreateChoice("Stay clean and build slow.", "You keep the stable above board. The bloodline grows without shortcuts.", 2, 0, 0f, ""),
                CreateChoice("Take the underground invitation.", "You step through a side door in the circuit. The risk rises, but so does your shadow reputation.", 0, 2, 0.05f, "choose_underground_path"),
                CreateChoice("Ignore both and focus on breeding.", "You turn away from the noise and study the bloodline records.", 1, 0, 0f, "")
            }
        ));

        EnsureUndergroundPathEvent();

        storyEvents.Add(CreateStoryEvent(
            "handler_watches",
            "A Handler Watches",
            "A rival handler watches from the edge of the feed. They do not speak at first. They only count your wins.",
            "Street League",
            2,
            new List<StoryChoiceData>
            {
                CreateChoice("Accept the rival's challenge.", "You answer in public. The stable gains respect for taking pressure head-on.", 2, 0, 0f, ""),
                CreateChoice("Delay and strengthen the stable.", "You refuse to rush the bloodline. The patient move earns quiet respect.", 1, 0, 0f, ""),
                CreateChoice("Take the backdoor arena route.", "You avoid the official spotlight and build influence where records blur.", 0, 2, 0.05f, "")
            }
        ));

        storyEvents.Add(CreateStoryEvent(
            "local_circuit_pressure",
            "Local Circuit Pressure",
            "The Local Circuit opens, but every open gate has a watcher. Your stable can enter clean, sideways, or not yet.",
            "Local Circuit",
            3,
            new List<StoryChoiceData>
            {
                CreateChoice("Enter officially.", "You step into the Local Circuit under your own name.", 3, 0, 0f, ""),
                CreateChoice("Enter underground first.", "You let the underground test the stable before the official circuit sees it.", 0, 3, 0.10f, ""),
                CreateChoice("Focus on bloodline development.", "You hold back from the spotlight and invest in the next generation.", 1, 0, 0f, "")
            }
        ));
    }

    void EnsureUndergroundPathEvent()
    {
        StoryEventData firstOffer = FindStoryEvent("first_offer");

        if (firstOffer != null && firstOffer.choices != null && firstOffer.choices.Count > 1)
        {
            firstOffer.choices[1].unlockEventId = "choose_underground_path";
        }

        if (FindStoryEvent("choose_underground_path") != null)
        {
            return;
        }

        StoryEventData undergroundPathEvent = CreateStoryEvent(
            "choose_underground_path",
            "Choose Your Underground Path",
            "The invitation leads below the official circuit, where every opportunity carries a different price. Decide what kind of foothold your stable will build.",
            "Street League",
            0,
            new List<StoryChoiceData>
            {
                CreateChoice("Chase fast money", "You take the quickest-paying matches. Credits may come fast, but attention follows the noise.", 0, 2, 0.10f, ""),
                CreateChoice("Build underground reputation", "You choose opponents and allies carefully, building a name that carries weight below the official circuit.", 1, 4, 0.05f, ""),
                CreateChoice("Stay cautious and gather intel", "You keep the stable out of the brightest danger while learning who controls the hidden arenas.", 1, 1, -0.02f, "")
            }
        );
        undergroundPathEvent.requiredUndergroundReputation = 2;
        undergroundPathEvent.requiredSeenEventId = "first_offer";
        storyEvents.Add(undergroundPathEvent);
    }

    public List<StoryEventData> GetAvailableStoryEvents()
    {
        InitializeDefaultEvents();

        List<StoryEventData> availableEvents = new List<StoryEventData>();
        int totalStableWins = leagueManager != null ? leagueManager.GetTotalStableWins() : 0;

        foreach (StoryEventData storyEvent in storyEvents)
        {
            if (storyEvent == null || storyEvent.hasBeenSeen)
            {
                continue;
            }

            if (totalStableWins < storyEvent.requiredTotalWins)
            {
                continue;
            }

            if (undergroundReputation < storyEvent.requiredUndergroundReputation)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(storyEvent.requiredSeenEventId))
            {
                StoryEventData requiredEvent = FindStoryEvent(storyEvent.requiredSeenEventId);

                if (requiredEvent == null || !requiredEvent.hasBeenSeen)
                {
                    continue;
                }
            }

            if (!string.IsNullOrEmpty(storyEvent.requiredLeagueName) &&
                (leagueManager == null || !leagueManager.IsLeagueUnlocked(storyEvent.requiredLeagueName)))
            {
                continue;
            }

            availableEvents.Add(storyEvent);
        }

        return availableEvents;
    }

    public void ShowNextAvailableEvent()
    {
        List<StoryEventData> availableEvents = GetAvailableStoryEvents();

        if (availableEvents.Count == 0)
        {
            activeStoryEvent = null;
            RefreshStoryUI();
            return;
        }

        ShowStoryEvent(availableEvents[0]);
    }

    public void ShowStoryEvent(StoryEventData storyEvent)
    {
        activeStoryEvent = storyEvent;
        RefreshStoryUI();
    }

    public void TriggerStoryEvent(string eventId)
    {
        StoryEventData storyEvent = FindStoryEvent(eventId);

        if (storyEvent == null || storyEvent.hasBeenSeen)
        {
            RefreshStoryUI();
            return;
        }

        ShowStoryEvent(storyEvent);
    }

    public void ChooseStoryOption(int choiceIndex)
    {
        EnsureUndergroundPathEvent();

        if (activeStoryEvent == null ||
            activeStoryEvent.choices == null ||
            choiceIndex < 0 ||
            choiceIndex >= activeStoryEvent.choices.Count)
        {
            return;
        }

        string activeEventId = activeStoryEvent.eventId;
        StoryChoiceData choice = activeStoryEvent.choices[choiceIndex];
        string nextEventId = choice.unlockEventId;

        if (activeEventId == "first_offer" && choiceIndex == 1)
        {
            nextEventId = "choose_underground_path";
        }

        reputation += choice.reputationChange;
        undergroundReputation += choice.undergroundReputationChange;
        riskModifier += choice.riskModifierChange;
        activeStoryEvent.hasBeenSeen = true;

        StoryEventData unlockedEvent = FindStoryEvent(nextEventId);
        if (unlockedEvent != null)
        {
            unlockedEvent.hasBeenSeen = false;
        }

        SaveStoryState();
        SetNarration($"Story choice made: {choice.choiceText}");

        if (unlockedEvent != null)
        {
            ShowStoryEvent(unlockedEvent);
            return;
        }

        ShowChoiceResult(choice);
    }

    public string GetStorySummaryText()
    {
        StringBuilder summary = new StringBuilder();

        summary.AppendLine("STORY STATUS");
        summary.AppendLine($"Reputation: {reputation}");
        summary.AppendLine($"Underground Reputation: {undergroundReputation}");
        summary.AppendLine($"Risk Modifier: {riskModifier:+0.00;-0.00;0.00}");
        summary.AppendLine();

        List<StoryEventData> availableEvents = GetAvailableStoryEvents();

        if (availableEvents.Count > 0)
        {
            summary.AppendLine("Available Event:");
            summary.AppendLine(availableEvents[0].title);
            summary.AppendLine();
            summary.AppendLine("A new story choice is ready.");
        }
        else
        {
            summary.AppendLine("No new story events are available right now.");
            summary.AppendLine("Win more fights or unlock another league to reveal the next thread.");
        }

        return summary.ToString();
    }

    public void SaveStoryState()
    {
        StorySaveData saveData = new StorySaveData
        {
            reputation = reputation,
            undergroundReputation = undergroundReputation,
            riskModifier = riskModifier
        };

        foreach (StoryEventData storyEvent in storyEvents)
        {
            if (storyEvent != null && storyEvent.hasBeenSeen)
            {
                saveData.seenEventIds.Add(storyEvent.eventId);
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(StorySaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadStoryState()
    {
        if (!PlayerPrefs.HasKey(StorySaveKey))
        {
            return;
        }

        string json = PlayerPrefs.GetString(StorySaveKey);
        StorySaveData saveData = JsonUtility.FromJson<StorySaveData>(json);

        if (saveData == null)
        {
            return;
        }

        reputation = saveData.reputation;
        undergroundReputation = saveData.undergroundReputation;
        riskModifier = saveData.riskModifier;

        foreach (StoryEventData storyEvent in storyEvents)
        {
            if (storyEvent == null) continue;

            storyEvent.hasBeenSeen = saveData.seenEventIds != null &&
                                     saveData.seenEventIds.Contains(storyEvent.eventId);
        }
    }

    public void ClearStorySave()
    {
        PlayerPrefs.DeleteKey(StorySaveKey);

        reputation = 0;
        undergroundReputation = 0;
        riskModifier = 0f;
        activeStoryEvent = null;

        InitializeDefaultEvents();

        foreach (StoryEventData storyEvent in storyEvents)
        {
            if (storyEvent != null)
            {
                storyEvent.hasBeenSeen = false;
            }
        }

        SaveStoryState();
        ShowNextAvailableEvent();
    }

    public void AddReputation(int amount)
    {
        reputation += amount;
        SaveStoryState();
        RefreshStoryUI();
    }

    public void AddUndergroundReputation(int amount)
    {
        undergroundReputation += amount;
        SaveStoryState();
        RefreshStoryUI();
    }

    public void AddRiskModifier(float amount)
    {
        riskModifier += amount;
        SaveStoryState();
        RefreshStoryUI();
    }

    public void RefreshStoryUI()
    {
        if (activeStoryEvent == null)
        {
            List<StoryEventData> availableEvents = GetAvailableStoryEvents();

            if (availableEvents.Count > 0)
            {
                activeStoryEvent = availableEvents[0];
            }
        }

        if (activeStoryEvent == null)
        {
            SetNarration("No new story events are available right now.");
            SetStoryText(GetStorySummaryText());
            ConfigureChoiceButton(choice1Button, choice1Text, null, -1);
            ConfigureChoiceButton(choice2Button, choice2Text, null, -1);
            ConfigureChoiceButton(choice3Button, choice3Text, null, -1);
            return;
        }

        SetStoryText(
            $"<b>{activeStoryEvent.title}</b>\n\n" +
            $"{activeStoryEvent.bodyText}\n\n" +
            $"Reputation: {reputation}\n" +
            $"Underground Reputation: {undergroundReputation}\n" +
            $"Risk Modifier: {riskModifier:+0.00;-0.00;0.00}"
        );

        ConfigureChoiceButton(choice1Button, choice1Text, GetChoiceAtIndex(0), 0);
        ConfigureChoiceButton(choice2Button, choice2Text, GetChoiceAtIndex(1), 1);
        ConfigureChoiceButton(choice3Button, choice3Text, GetChoiceAtIndex(2), 2);
    }

    void ShowChoiceResult(StoryChoiceData choice)
    {
        activeStoryEvent = null;

        List<StoryEventData> availableEvents = GetAvailableStoryEvents();

        if (availableEvents.Count > 0)
        {
            activeStoryEvent = availableEvents[0];
            RefreshStoryUI();
            return;
        }

        SetStoryText(
            $"{choice.resultText}\n\n" +
            GetStorySummaryText()
        );

        ConfigureChoiceButton(choice1Button, choice1Text, null, -1);
        ConfigureChoiceButton(choice2Button, choice2Text, null, -1);
        ConfigureChoiceButton(choice3Button, choice3Text, null, -1);
    }

    void SetStoryText(string text)
    {
        if (storyText != null)
        {
            storyText.text = text;
        }
    }

    void ConfigureChoiceButton(Button button, TextMeshProUGUI buttonText, StoryChoiceData choice, int choiceIndex)
    {
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(choice != null);

        if (buttonText != null)
        {
            buttonText.text = choice != null ? choice.choiceText : "";
        }
    }

    StoryEventData FindStoryEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return null;
        }

        foreach (StoryEventData storyEvent in storyEvents)
        {
            if (storyEvent != null && storyEvent.eventId == eventId)
            {
                return storyEvent;
            }
        }

        return null;
    }

    StoryChoiceData GetChoiceAtIndex(int index)
    {
        if (activeStoryEvent == null ||
            activeStoryEvent.choices == null ||
            index < 0 ||
            index >= activeStoryEvent.choices.Count)
        {
            return null;
        }

        return activeStoryEvent.choices[index];
    }

    StoryEventData CreateStoryEvent(
        string eventId,
        string title,
        string bodyText,
        string requiredLeagueName,
        int requiredTotalWins,
        List<StoryChoiceData> choices
    )
    {
        return new StoryEventData
        {
            eventId = eventId,
            title = title,
            bodyText = bodyText,
            requiredLeagueName = requiredLeagueName,
            requiredTotalWins = requiredTotalWins,
            requiredUndergroundReputation = 0,
            requiredSeenEventId = "",
            hasBeenSeen = false,
            choices = choices
        };
    }

    StoryChoiceData CreateChoice(
        string choiceText,
        string resultText,
        int reputationChange,
        int undergroundReputationChange,
        float riskModifierChange,
        string unlockEventId
    )
    {
        return new StoryChoiceData
        {
            choiceText = choiceText,
            resultText = resultText,
            reputationChange = reputationChange,
            undergroundReputationChange = undergroundReputationChange,
            riskModifierChange = riskModifierChange,
            unlockEventId = unlockEventId
        };
    }

    void SetNarration(string message)
    {
        if (narratorManager != null)
        {
            narratorManager.SetNarration(message);
        }
    }
}
