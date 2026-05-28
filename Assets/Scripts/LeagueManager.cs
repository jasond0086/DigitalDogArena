using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class LeagueManager : MonoBehaviour
{
    [System.Serializable]
    public class LeagueData
    {
        public string leagueName;
        public int requiredWinsToUnlock;
        [TextArea] public string description;
        public bool isUnlocked;
        public bool isCompleted;
        public string rewardText;
    }

    [Header("References")]
    public DogManager dogManager;
    public TextMeshProUGUI leagueStatusText;

    [Header("League Progress")]
    public List<LeagueData> leagues = new List<LeagueData>();

    void Awake()
    {
        if (dogManager == null)
        {
            dogManager = GetComponent<DogManager>();
        }

        EnsureDefaultLeagues();
        RefreshLeagueProgress();
    }

    void Start()
    {
        RefreshLeagueProgress();
    }

    public void RefreshLeagueProgress()
    {
        EnsureDefaultLeagues();

        int totalStableWins = GetTotalStableWins();

        foreach (LeagueData league in leagues)
        {
            league.isUnlocked = totalStableWins >= league.requiredWinsToUnlock;
            league.isCompleted = IsLeagueCompleted(league, totalStableWins);
        }

        UpdateLeagueStatusUI();
    }

    public string GetCurrentLeagueName()
    {
        LeagueData currentLeague = GetCurrentLeague();

        if (currentLeague == null)
        {
            return "No League";
        }

        return currentLeague.leagueName;
    }

    public string GetLeagueSummaryText()
    {
        int totalStableWins = GetTotalStableWins();
        StringBuilder summary = new StringBuilder();

        summary.AppendLine($"Total Stable Wins: {totalStableWins}");
        summary.AppendLine($"Current League: {GetCurrentLeagueName()}");
        summary.AppendLine();

        foreach (LeagueData league in leagues)
        {
            string lockState = league.isUnlocked ? "Unlocked" : $"Locked ({totalStableWins}/{league.requiredWinsToUnlock} wins)";
            string completionState = league.isCompleted ? "Completed" : "In Progress";

            summary.AppendLine($"{league.leagueName}: {lockState} | {completionState}");
            summary.AppendLine(league.description);
            summary.AppendLine($"Reward: {league.rewardText}");
            summary.AppendLine();
        }

        return summary.ToString();
    }

    public void UpdateLeagueStatusUI()
    {
        if (leagueStatusText == null)
        {
            return;
        }

        leagueStatusText.text =
            $"<b>Total Stable Wins:</b> {GetTotalStableWins()}\n" +
            $"<b>Current League:</b> {GetCurrentLeagueName()}\n\n" +
            GetLeagueSummaryText();
    }

    public int GetTotalStableWins()
    {
        if (dogManager == null || dogManager.ownedDogs == null)
        {
            return 0;
        }

        int totalWins = 0;

        foreach (Dog dog in dogManager.ownedDogs)
        {
            if (dog == null) continue;

            totalWins += dog.wins;
        }

        return totalWins;
    }

    LeagueData GetCurrentLeague()
    {
        LeagueData currentLeague = null;

        foreach (LeagueData league in leagues)
        {
            if (!league.isUnlocked)
            {
                continue;
            }

            currentLeague = league;

            if (!league.isCompleted)
            {
                break;
            }
        }

        return currentLeague;
    }

    bool IsLeagueCompleted(LeagueData league, int totalStableWins)
    {
        int leagueIndex = leagues.IndexOf(league);

        if (leagueIndex < 0 || leagueIndex >= leagues.Count - 1)
        {
            return false;
        }

        LeagueData nextLeague = leagues[leagueIndex + 1];
        return totalStableWins >= nextLeague.requiredWinsToUnlock;
    }

    void EnsureDefaultLeagues()
    {
        if (leagues.Count > 0)
        {
            return;
        }

        leagues.Add(CreateLeague(
            "Street League",
            0,
            "The first gates of the arena. Raw instincts, fresh bloodlines, and names still being written.",
            "Access to the Local Circuit at 3 total stable wins."
        ));

        leagues.Add(CreateLeague(
            "Local Circuit",
            3,
            "The kennel starts drawing attention. Local handlers test discipline, stamina, and early legacy.",
            "Access to the Underground Circuit at 8 total stable wins."
        ));

        leagues.Add(CreateLeague(
            "Underground Circuit",
            8,
            "The lights get colder here. Bloodlines with weak code collapse under pressure.",
            "Access to the Elite Circuit at 15 total stable wins."
        ));

        leagues.Add(CreateLeague(
            "Elite Circuit",
            15,
            "Only proven records reach this tier. Every fight feels like a career turning point.",
            "Access to the Apex League at 25 total stable wins."
        ));

        leagues.Add(CreateLeague(
            "Apex League",
            25,
            "The top of the digital arena. Legacy, instinct, and command meet under the final lights.",
            "Apex status unlocked. PvP foundations can build from here later."
        ));

        RefreshLeagueProgress();
    }

    LeagueData CreateLeague(string leagueName, int requiredWinsToUnlock, string description, string rewardText)
    {
        return new LeagueData
        {
            leagueName = leagueName,
            requiredWinsToUnlock = requiredWinsToUnlock,
            description = description,
            rewardText = rewardText,
            isUnlocked = requiredWinsToUnlock == 0,
            isCompleted = false
        };
    }
}
