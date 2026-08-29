using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FightManager : MonoBehaviour
{
    [Header("References")]
    public DogManager dogManager;
    public LeagueManager leagueManager;
    public RivalManager rivalManager;
    public StoryManager storyManager;
    public EconomyManager economyManager;
    public NarratorManager narratorManager;
    public FightPresentationManager fightPresentationManager;
    public TextMeshProUGUI fightLog;
    public ScrollRect fightLogScrollRect;
    public RectTransform fightLogPanel;
    public RectTransform fightLogViewport;

    [Header("Fight Controls")]
    public Button startFightButton;
    public Button nextRoundButton;
    public Button endFightButton;
    public TextMeshProUGUI roundStatusText;

    [Header("Fight Preview UI")]
    public RectTransform playerFightPreviewPanel;
    public RectTransform opponentFightPreviewPanel;
    public TextMeshProUGUI playerFightPreviewText;
    public TextMeshProUGUI opponentFightPreviewText;
    public Image playerFightDogImage;
    public Image opponentFightDogImage;

    [Header("Fight Settings")]
    public int maxRounds = 6;
    public int healthMultiplier = 2;
    public int damageVariance = 15;

    private const float DecisionDrawThreshold = 0.03f;
    private const float GlancingDodgeDamageMultiplier = 0.30f;
    private const float SecondWindHealingCapPercent = 0.65f;
    public const string StoryFightStep0Id = "story_step_0_rust_yard";

    [Header("Active Fight State")]
    public bool fightInProgress;
    public int currentRound;
    public Dog activeFighter1;
    public Dog activeFighter2;
    public int currentFighter1Health;
    public int currentFighter2Health;
    [TextArea] public string runningFightLog = "";

    private int startingFighter1Health;
    private int startingFighter2Health;
    private bool fighter1WasBehind;
    private bool fighter2WasBehind;
    private bool activeFightIsRival;
    private RivalHandlerData activeRival;
    [Header("Story Fight State")]
    public bool pendingStoryFight;
    [SerializeField] string pendingStoryFightId;
    Dog pendingStoryOpponent;
    bool activeFightIsStory;
    string activeStoryFightId;
    private int fighter1WinsBeforeFight;
    private int fighter2WinsBeforeFight;
    private bool fightIntroInProgress;
    private bool dogPoundTokensAwardedForActiveFight;
    private bool fightResultResolved;
    private Dog lastPreviewPlayerDog;
    private Dog lastPreviewOpponentDog;
    private FightStrategy lastPreviewPlayerStrategy;
    private FightStrategy lastPreviewOpponentStrategy;
    private bool playerFightPreviewTextIsRuntimeFallback;
    private bool opponentFightPreviewTextIsRuntimeFallback;

    void Awake()
    {
        ConfigureScrollableFightLog();

        if (dogManager == null)
        {
            dogManager = GetComponent<DogManager>();
        }

        if (leagueManager == null)
        {
            leagueManager = GetComponent<LeagueManager>();
        }

        if (rivalManager == null)
        {
            rivalManager = GetComponent<RivalManager>();
        }

        if (storyManager == null)
        {
            storyManager = GetComponent<StoryManager>();
        }

        if (economyManager == null)
        {
            economyManager = GetComponent<EconomyManager>();
        }

        if (narratorManager == null)
        {
            narratorManager = GetComponent<NarratorManager>();
        }

        if (narratorManager == null)
        {
            narratorManager = FindFirstObjectByType<NarratorManager>();
        }

        if (fightPresentationManager == null)
        {
            fightPresentationManager = FindObjectOfType<FightPresentationManager>();
        }

        if (fightPresentationManager == null)
        {
            GameObject presentationManagerObject = new GameObject("FightPresentationManager");
            fightPresentationManager = presentationManagerObject.AddComponent<FightPresentationManager>();
        }

        FindFightPreviewReferencesIfMissing();
        ConfigureFightPreviewText(playerFightPreviewText, playerFightPreviewTextIsRuntimeFallback);
        ConfigureFightPreviewText(opponentFightPreviewText, opponentFightPreviewTextIsRuntimeFallback);
        FindEndFightButtonIfMissing();
        SetEndFightButtonVisible(false);
    }

    void Start()
    {
        UpdateFightUIState("Choose fighters and start a fight.");
        RefreshFightPreviews(true);
    }

    void Update()
    {
        RefreshFightPreviews(false);
    }

    public void StartFight()
    {
        if (fightInProgress)
        {
            SetLog("Finish the current fight before starting another one.");
            return;
        }

        if (pendingStoryFight)
        {
            StartPendingStoryFight();
            return;
        }

        if (dogManager == null)
        {
            SetLog("DogManager is missing!");
            return;
        }

        Dog dog1 = dogManager.selectedFighter1;
        Dog dog2 = dogManager.selectedFighter2;

        if (dog1 == null || dog2 == null)
        {
            SetLog("Select 2 fighters first!");
            return;
        }

        if (dog1 == dog2)
        {
            SetLog("A fighter cannot fight itself.");
            return;
        }

        InitializeFight(dog1, dog2, false, null);
        BeginFightIntroThenFirstRound();
    }

    public bool QueueStoryFight(string storyFightId)
    {
        if (fightInProgress)
        {
            ReportStoryFightStartFailure("Finish the current fight before starting a story fight.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(storyFightId))
        {
            ReportStoryFightStartFailure("Story fight data missing.");
            return false;
        }

        Dog storyOpponent = CreateStoryFightOpponent(storyFightId);

        if (storyOpponent == null)
        {
            ReportStoryFightStartFailure("No opponent available.");
            return false;
        }

        pendingStoryFight = true;
        pendingStoryFightId = storyFightId;
        pendingStoryOpponent = storyOpponent;
        RefreshFightPreviews(true);

        string message = "Story fight queued: Rust Yard versus CHAINSAW. Select any owned dog, then press Start Fight.";
        SetLog(message);
        SetFightNarration("Story fight queued.", message);
        UpdateFightUIState("Story fight pending - select one dog, then start the fight.");
        Debug.Log($"Story fight queued: {pendingStoryFightId} against {pendingStoryOpponent.dogName}.");
        return true;
    }

    public void ClearStoryFightState()
    {
        bool hadPendingStoryFight = pendingStoryFight;
        bool hadActiveStoryFight = activeFightIsStory;

        pendingStoryFight = false;
        pendingStoryFightId = string.Empty;
        pendingStoryOpponent = null;
        activeFightIsStory = false;
        activeStoryFightId = string.Empty;

        if (!fightInProgress)
        {
            UpdateFightUIState();
        }

        Debug.Log($"Story fight state cleared. Pending: {hadPendingStoryFight}. Active: {hadActiveStoryFight}.");
    }

    public void EndCurrentFight()
    {
        if (fightInProgress || fightIntroInProgress)
        {
            Debug.LogWarning("End fight cleanup was requested before the fight result resolved.");
            return;
        }

        if (!fightResultResolved)
        {
            Debug.LogWarning("End fight cleanup was requested, but there is no resolved fight to clear.");
            return;
        }

        Debug.Log("End fight cleanup started.");

        if (fightPresentationManager != null)
        {
            fightPresentationManager.ClearFightPresentation();
        }
        else
        {
            Debug.LogWarning("Fight presentation cleanup was skipped because FightPresentationManager is missing.");
        }

        activeFighter1 = null;
        activeFighter2 = null;
        activeRival = null;
        activeFightIsRival = false;
        activeFightIsStory = false;
        activeStoryFightId = string.Empty;
        currentRound = 0;
        currentFighter1Health = 0;
        currentFighter2Health = 0;
        startingFighter1Health = 0;
        startingFighter2Health = 0;
        fighter1WasBehind = false;
        fighter2WasBehind = false;
        dogPoundTokensAwardedForActiveFight = false;
        fightResultResolved = false;

        SetEndFightButtonVisible(false);
        SetLog("Ready for a new fight.");
        UpdateFightUIState("Choose fighters and start a fight.");
        RefreshFightPreviews(true);
        Debug.Log("Fight returned to the ready state.");
    }

    public void StartRivalFight()
    {
        if (fightInProgress)
        {
            SetLog("Finish the current fight before starting another one.");
            return;
        }

        if (dogManager == null)
        {
            SetLog("DogManager is missing!");
            return;
        }

        Dog playerDog = dogManager.selectedFighter1;

        if (playerDog == null)
        {
            SetLog("Select Fighter 1 before challenging a rival.");
            return;
        }

        if (!playerDog.CanFight())
        {
            SetLog($"{playerDog.dogName} cannot enter the arena.");
            return;
        }

        if (rivalManager == null)
        {
            SetLog("RivalManager is missing!");
            return;
        }

        RivalHandlerData rival = rivalManager.GetCurrentRival();

        if (rival == null || rival.rivalDog == null)
        {
            SetLog("No undefeated rival is currently available.");
            return;
        }

        InitializeFight(playerDog, rival.rivalDog, true, rival);
        BeginFightIntroThenFirstRound();
    }

    void StartPendingStoryFight()
    {
        if (string.IsNullOrWhiteSpace(pendingStoryFightId))
        {
            ReportStoryFightStartFailure("Story fight data missing.");
            return;
        }

        if (dogManager == null)
        {
            ReportStoryFightStartFailure("Story fight data missing.");
            return;
        }

        Dog playerDog = dogManager.selectedFighter1;

        if (playerDog == null)
        {
            ReportStoryFightStartFailure("Select a dog first.");
            return;
        }

        if (!playerDog.CanFight())
        {
            ReportStoryFightStartFailure($"{playerDog.dogName} cannot enter the arena.");
            return;
        }

        if (pendingStoryOpponent == null)
        {
            ReportStoryFightStartFailure("No opponent available.");
            return;
        }

        string storyFightId = pendingStoryFightId;
        Dog storyOpponent = pendingStoryOpponent;
        pendingStoryFight = false;
        pendingStoryFightId = string.Empty;
        pendingStoryOpponent = null;

        InitializeFight(playerDog, storyOpponent, false, null);
        activeFightIsStory = true;
        activeStoryFightId = storyFightId;
        runningFightLog = $"<b>STORY FIGHT: RUST YARD</b>\n" + runningFightLog;
        SetLog(runningFightLog);
        BeginFightIntroThenFirstRound();
    }

    void InitializeFight(Dog fighter1, Dog fighter2, bool isRivalFight, RivalHandlerData rival)
    {
        activeFighter1 = fighter1;
        activeFighter2 = fighter2;

        activeFightIsRival = isRivalFight;
        activeRival = rival;
        activeFightIsStory = false;
        activeStoryFightId = string.Empty;
        fightResultResolved = false;
        SetEndFightButtonVisible(false);
        currentRound = 0;
        startingFighter1Health = CalculateStartingHealth(activeFighter1);
        startingFighter2Health = CalculateStartingHealth(activeFighter2);
        currentFighter1Health = startingFighter1Health;
        currentFighter2Health = startingFighter2Health;
        fighter1WasBehind = false;
        fighter2WasBehind = false;
        fighter1WinsBeforeFight = activeFighter1.wins;
        fighter2WinsBeforeFight = activeFighter2.wins;
        dogPoundTokensAwardedForActiveFight = false;

        FightStrategy strategy1 = dogManager.fighter1Strategy;
        FightStrategy strategy2 = dogManager.fighter2Strategy;

        runningFightLog = $"<b>{activeFighter1.dogName} vs {activeFighter2.dogName}</b>\n\n";
        runningFightLog += $"{activeFighter1.dogName} Style: {activeFighter1.fightStyle} | Starting Strategy: {strategy1}\n";
        runningFightLog += $"{activeFighter2.dogName} Style: {activeFighter2.fightStyle} | Starting Strategy: {strategy2}\n\n";
        runningFightLog += $"{activeFighter1.dogName} Traits: {activeFighter1.GetTraitSummary()}\n";
        runningFightLog += $"{activeFighter2.dogName} Traits: {activeFighter2.GetTraitSummary()}\n\n";
        runningFightLog += $"{activeFighter1.dogName} Starting HP: {currentFighter1Health}\n";
        runningFightLog += $"{activeFighter2.dogName} Starting HP: {currentFighter2Health}\n\n";

        fightInProgress = true;
        UpdateFightUIState();
        SetLog(runningFightLog);
        SetFightNarration(
            $"Fight Started: {activeFighter1.dogName} vs {activeFighter2.dogName}",
            runningFightLog
        );
    }

    void BeginFightIntroThenFirstRound()
    {
        fightIntroInProgress = true;
        UpdateFightUIState("DNA scan sequence in progress...");

        if (fightPresentationManager == null)
        {
            CompleteFightIntroAndStartFirstRound();
            return;
        }

        fightPresentationManager.PlayFightIntroSequence(
            activeFighter1,
            activeFighter2,
            CompleteFightIntroAndStartFirstRound
        );
    }

    void CompleteFightIntroAndStartFirstRound()
    {
        if (!fightInProgress || activeFighter1 == null || activeFighter2 == null)
        {
            fightIntroInProgress = false;
            UpdateFightUIState();
            return;
        }

        fightIntroInProgress = false;
        PlayNextRound();
    }

    public void PlayNextRound()
    {
        if (!fightInProgress || activeFighter1 == null || activeFighter2 == null)
        {
            return;
        }

        if (fightIntroInProgress)
        {
            return;
        }

        currentRound++;

        FightStrategy strategy1 = dogManager.fighter1Strategy;
        FightStrategy strategy2 = dogManager.fighter2Strategy;

        float baseDamage1 = CalculateBaseDamage(activeFighter1);
        float baseDamage2 = CalculateBaseDamage(activeFighter2);

        bool fighter2GlancedHit;
        bool fighter1GlancedHit;
        int dmg1 = CalculateFinalDamage(activeFighter1, activeFighter2, baseDamage1, currentRound, strategy1, strategy2, out fighter2GlancedHit);
        int dmg2 = CalculateFinalDamage(activeFighter2, activeFighter1, baseDamage2, currentRound, strategy2, strategy1, out fighter1GlancedHit);

        currentFighter2Health = Mathf.Max(0, currentFighter2Health - dmg1);
        currentFighter1Health = Mathf.Max(0, currentFighter1Health - dmg2);

        int fighter1Healing = 0;
        int fighter2Healing = 0;

        currentFighter1Health = ApplySecondWindHealing(
            activeFighter1,
            strategy1,
            currentFighter1Health,
            startingFighter1Health,
            out fighter1Healing
        );

        currentFighter2Health = ApplySecondWindHealing(
            activeFighter2,
            strategy2,
            currentFighter2Health,
            startingFighter2Health,
            out fighter2Healing
        );

        if (fightPresentationManager != null)
        {
            fightPresentationManager.PresentRoundAction(
                currentRound,
                activeFighter1,
                activeFighter2,
                currentFighter1Health,
                currentFighter2Health,
                dmg1,
                dmg2,
                strategy1,
                strategy2,
                activeFighter1.fightStyle,
                activeFighter2.fightStyle
            );
        }

        if (currentFighter1Health < currentFighter2Health)
        {
            fighter1WasBehind = true;
        }

        if (currentFighter2Health < currentFighter1Health)
        {
            fighter2WasBehind = true;
        }

        string roundFlavor = GetRoundFlavor(
            activeFighter1,
            activeFighter2,
            dmg1,
            dmg2,
            currentRound,
            strategy1,
            strategy2
        );

        runningFightLog += $"<b>Round {currentRound}</b>: {roundFlavor}\n";
        runningFightLog += $"Strategies: {activeFighter1.dogName} {strategy1} | {activeFighter2.dogName} {strategy2}\n";
        string strategyFeedback = GetStrategyMatchupFeedback(activeFighter1, activeFighter2, strategy1, strategy2, currentRound);
        if (!string.IsNullOrEmpty(strategyFeedback))
        {
            runningFightLog += $"Strategy read: {strategyFeedback}\n";
        }

        runningFightLog += $"{activeFighter1.dogName} impact: {dmg1} | {activeFighter2.dogName} impact: {dmg2}\n";
        if (fighter2GlancedHit)
        {
            runningFightLog += $"GLANCING HIT: {activeFighter2.dogName} dodged most of {activeFighter1.dogName}'s attack (-70% damage).\n";
        }

        if (fighter1GlancedHit)
        {
            runningFightLog += $"GLANCING HIT: {activeFighter1.dogName} dodged most of {activeFighter2.dogName}'s attack (-70% damage).\n";
        }

        if (fighter1Healing > 0)
        {
            runningFightLog += $"SECOND WIND +{fighter1Healing} HP: {activeFighter1.dogName} recovered but stayed under the 65% cap.\n";
        }

        if (fighter2Healing > 0)
        {
            runningFightLog += $"SECOND WIND +{fighter2Healing} HP: {activeFighter2.dogName} recovered but stayed under the 65% cap.\n";
        }

        runningFightLog += $"{activeFighter1.dogName} HP: {currentFighter1Health} | {activeFighter2.dogName} HP: {currentFighter2Health}\n\n";

        SetLog(runningFightLog);

        bool fightShouldEnd = currentRound >= maxRounds ||
                              currentFighter1Health <= 0 ||
                              currentFighter2Health <= 0;

        if (fightShouldEnd)
        {
            FinishActiveFight();
            return;
        }

        SetFightNarration(
            $"Round {currentRound}: {activeFighter1.dogName} {currentFighter1Health} HP - {activeFighter2.dogName} {currentFighter2Health} HP",
            runningFightLog
        );
        UpdateFightUIState();
    }

    void ApplyRivalWinReward(string leagueName)
    {
        if (storyManager == null)
        {
            return;
        }

        switch (leagueName)
        {
            case "Street League":
                storyManager.AddReputation(2);
                break;

            case "Local Circuit":
                storyManager.AddReputation(3);
                break;

            case "Underground Circuit":
                storyManager.AddReputation(2);
                storyManager.AddUndergroundReputation(2);
                break;

            case "Elite Circuit":
                storyManager.AddReputation(5);
                break;

            case "Apex League":
                storyManager.AddReputation(8);
                break;
        }
    }

    void ApplyRivalCreditReward(string leagueName)
    {
        if (economyManager == null)
        {
            return;
        }

        switch (leagueName)
        {
            case "Street League":
                economyManager.AddCredits(150, "Street League rival defeated");
                break;

            case "Local Circuit":
                economyManager.AddCredits(250, "Local Circuit rival defeated");
                break;

            case "Underground Circuit":
                economyManager.AddCredits(400, "Underground Circuit rival defeated");
                break;

            case "Elite Circuit":
                economyManager.AddCredits(650, "Elite Circuit rival defeated");
                break;

            case "Apex League":
                economyManager.AddCredits(1000, "Apex League rival defeated");
                break;
        }
    }

    int GetRivalCreditReward(string leagueName)
    {
        switch (leagueName)
        {
            case "Street League": return 150;
            case "Local Circuit": return 250;
            case "Underground Circuit": return 400;
            case "Elite Circuit": return 650;
            case "Apex League": return 1000;
            default: return 0;
        }
    }

    void FinishActiveFight()
    {
        ResolveFightResult(
            activeFighter1,
            activeFighter2,
            currentFighter1Health,
            currentFighter2Health,
            startingFighter1Health,
            startingFighter2Health,
            fighter1WasBehind,
            fighter2WasBehind,
            ref runningFightLog
        );

        AwardDogPoundTokensForFightResult();

        int presentationHealth1;
        int presentationHealth2;
        GetResultPresentationHealth(out presentationHealth1, out presentationHealth2);

        if (fightPresentationManager != null)
        {
            fightPresentationManager.PresentFightResult(
                activeFighter1,
                activeFighter2,
                presentationHealth1,
                presentationHealth2
            );
        }

        fightInProgress = false;
        fightIntroInProgress = false;
        string finalStatus = GetFinalFightStatus();

        if (activeFightIsStory)
        {
            FinishStoryFight();
        }
        else if (activeFightIsRival)
        {
            FinishRivalFight();
        }
        else
        {
            FinishNormalFight();
        }

        activeFightIsStory = false;
        activeStoryFightId = string.Empty;

        SetLog(runningFightLog);

        if (dogManager != null)
        {
            dogManager.DisplayDogs();
            dogManager.SaveStable();
        }

        if (leagueManager != null)
        {
            leagueManager.RefreshLeagueProgress();
        }

        UpdateFightUIState(finalStatus);
        fightResultResolved = true;
        SetEndFightButtonVisible(true);
        Debug.Log("Fight resolved. EndFightButton is now available.");
    }

    string GetFinalFightStatus()
    {
        if (activeFighter1.wins > fighter1WinsBeforeFight)
        {
            return $"Fight complete - {activeFighter1.dogName} defeated {activeFighter2.dogName}.";
        }

        if (activeFighter2.wins > fighter2WinsBeforeFight)
        {
            return $"Fight complete - {activeFighter2.dogName} defeated {activeFighter1.dogName}.";
        }

        return $"Fight complete - {activeFighter1.dogName} and {activeFighter2.dogName} fought to a draw.";
    }

    void AwardDogPoundTokensForFightResult()
    {
        if (dogPoundTokensAwardedForActiveFight || activeFighter1 == null || activeFighter2 == null)
        {
            return;
        }

        bool fighter1Won = activeFighter1.wins > fighter1WinsBeforeFight;
        bool fighter2Won = activeFighter2.wins > fighter2WinsBeforeFight;
        int tokenAward = 1;

        if (fighter1Won || (!activeFightIsRival && !activeFightIsStory && fighter2Won))
        {
            tokenAward = 2;
        }

        DogPoundSettings.AddTokens(tokenAward);
        dogPoundTokensAwardedForActiveFight = true;
        Debug.Log($"Dog Pound Tokens awarded: +{tokenAward}.");
    }

    void GetResultPresentationHealth(out int presentationHealth1, out int presentationHealth2)
    {
        presentationHealth1 = currentFighter1Health;
        presentationHealth2 = currentFighter2Health;

        bool fighter1Won = activeFighter1.wins > fighter1WinsBeforeFight;
        bool fighter2Won = activeFighter2.wins > fighter2WinsBeforeFight;

        if (fighter1Won && presentationHealth1 <= presentationHealth2)
        {
            presentationHealth1 = presentationHealth2 + 1;
        }
        else if (fighter2Won && presentationHealth2 <= presentationHealth1)
        {
            presentationHealth2 = presentationHealth1 + 1;
        }
        else if (!fighter1Won && !fighter2Won)
        {
            int drawPresentationHealth = Mathf.Max(presentationHealth1, presentationHealth2);
            presentationHealth1 = drawPresentationHealth;
            presentationHealth2 = drawPresentationHealth;
        }
    }

    void UpdateFightUIState(string statusOverride = "")
    {
        if (startFightButton != null)
        {
            startFightButton.interactable = !fightInProgress && !fightIntroInProgress;
        }

        if (nextRoundButton != null)
        {
            nextRoundButton.interactable = fightInProgress && !fightIntroInProgress;
        }

        if (roundStatusText == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(statusOverride))
        {
            roundStatusText.text = statusOverride;
            return;
        }

        if (fightIntroInProgress)
        {
            roundStatusText.text = "DNA scan sequence in progress...";
        }
        else if (fightInProgress)
        {
            int nextRound = Mathf.Min(currentRound + 1, maxRounds);
            roundStatusText.text = $"Round {nextRound} / {maxRounds} — adjust strategy, then play next round.";
        }
        else
        {
            roundStatusText.text = "Choose fighters and start a fight.";
        }
    }

    void FinishNormalFight()
    {
        if (economyManager != null)
        {
            if (activeFighter1.wins > fighter1WinsBeforeFight)
            {
                economyManager.AddCredits(50, $"Normal fight win by {activeFighter1.dogName}");
            }
            else if (activeFighter2.wins > fighter2WinsBeforeFight)
            {
                economyManager.AddCredits(50, $"Normal fight win by {activeFighter2.dogName}");
            }
        }

        if (activeFighter1.wins > fighter1WinsBeforeFight)
        {
            SetFightNarration(
                $"Fight Result: {activeFighter1.dogName} defeated {activeFighter2.dogName} - 50 credits",
                runningFightLog
            );
        }
        else if (activeFighter2.wins > fighter2WinsBeforeFight)
        {
            SetFightNarration(
                $"Fight Result: {activeFighter2.dogName} defeated {activeFighter1.dogName} - 50 credits",
                runningFightLog
            );
        }
        else
        {
            SetFightNarration(
                $"Fight Result: {activeFighter1.dogName} and {activeFighter2.dogName} fought to a draw",
                runningFightLog
            );
        }
    }

    void FinishRivalFight()
    {
        if (activeRival == null)
        {
            FinishNormalFight();
            return;
        }

        if (activeFighter1.wins > fighter1WinsBeforeFight)
        {
            rivalManager.MarkRivalDefeated(activeRival.rivalId);
            ApplyRivalWinReward(activeRival.leagueName);
            ApplyRivalCreditReward(activeRival.leagueName);
            SetFightNarration(
                $"Fight Result: {activeFighter1.dogName} defeated {activeFighter2.dogName} - {activeRival.leagueName} - {GetRivalCreditReward(activeRival.leagueName)} credits",
                runningFightLog
            );
        }
        else
        {
            if (storyManager != null)
            {
                storyManager.AddRiskModifier(0.02f);
            }

            if (activeFighter2.wins > fighter2WinsBeforeFight)
            {
                SetFightNarration(
                    $"Fight Result: {activeFighter2.dogName} defeated {activeFighter1.dogName} - {activeRival.leagueName}",
                    runningFightLog
                );
            }
            else
            {
                SetFightNarration(
                    $"Fight Result: {activeFighter1.dogName} and {activeFighter2.dogName} fought to a draw - {activeRival.leagueName}",
                    runningFightLog
                );
            }
        }

        rivalManager.RefreshRivalStatusUI();
    }

    void FinishStoryFight()
    {
        bool playerWon = activeFighter1.wins > fighter1WinsBeforeFight;
        bool kennelReputationAwarded = false;

        if (storyManager != null)
        {
            bool storyFightCompleted = storyManager.CompleteCurrentStoryFight();

            if (storyFightCompleted)
            {
                kennelReputationAwarded = storyManager.TryAwardStoryFightKennelReputation(activeStoryFightId);
            }

            if (playerWon)
            {
                storyManager.AddReputation(10);
            }
        }

        string reputationRewardText = kennelReputationAwarded ? " +10 Kennel Reputation." : string.Empty;
        string resultText = playerWon
            ? $"Story Fight complete: {activeFighter1.dogName} defeated CHAINSAW.{reputationRewardText}"
            : $"Story Fight complete: CHAINSAW held the Rust Yard. Story Step 1 unlocked.{reputationRewardText}";

        runningFightLog += $"\n{resultText}\n";
        SetFightNarration(resultText, runningFightLog);
        Debug.Log($"Story fight resolved: {activeStoryFightId}. Player won: {playerWon}. Kennel Reputation awarded: {kennelReputationAwarded}.");
    }

    Dog CreateStoryFightOpponent(string storyFightId)
    {
        if (storyFightId != StoryFightStep0Id)
        {
            return null;
        }

        Dog opponent = ScriptableObject.CreateInstance<Dog>();
        opponent.dogId = "story_rust_yard_chainsaw";
        opponent.dogName = "CHAINSAW";
        opponent.breed = "Bully Striker";
        opponent.gender = DogGender.Male;
        opponent.strength = 24;
        opponent.agility = 20;
        opponent.stamina = 24;
        opponent.intelligence = 18;
        opponent.strengthPotential = 60;
        opponent.agilityPotential = 56;
        opponent.staminaPotential = 60;
        opponent.intelligencePotential = 52;
        opponent.fightStyle = FightStyle.Rushdown;
        opponent.primaryTrait = DogTrait.Aggressive;
        opponent.secondaryTrait = DogTrait.None;
        opponent.level = 1;
        opponent.NormalizeLegacyStats();
        return opponent;
    }

    void ReportStoryFightStartFailure(string message)
    {
        SetLog(message);
        SetFightNarration("Story fight unavailable.", message);
        UpdateFightUIState(message);
        Debug.LogWarning($"Story fight could not start: {message}");
    }

    int CalculateStartingHealth(Dog dog)
    {
        float health = CalculateMaxHealth(dog);

        if (HasTrait(dog, DogTrait.Durable))
        {
            health *= 1.12f;
        }

        if (HasTrait(dog, DogTrait.GlassCannon))
        {
            health *= 0.90f;
        }

        return Mathf.Max(1, Mathf.RoundToInt(health));
    }

    float CalculateMaxHealth(Dog dog)
    {
        if (dog == null)
        {
            return 1f;
        }

        return 45f + (dog.stamina * 3f) + (dog.GetIntelligence() * 0.75f);
    }

    float CalculateBaseDamage(Dog attacker)
    {
        if (attacker == null)
        {
            return 1f;
        }

        return 3f +
               (attacker.strength * 0.85f) +
               (attacker.agility * 0.35f) +
               (attacker.GetIntelligence() * 0.15f);
    }

    int CalculateFinalDamage(
        Dog attacker,
        Dog defender,
        float baseDamage,
        int round,
        FightStrategy attackerStrategy,
        FightStrategy defenderStrategy,
        out bool glancingHit
    )
    {
        glancingHit = false;
        float damage = Mathf.Max(1f, baseDamage);

        damage = ApplyStyleModifier(attacker, defender, damage, round);
        damage = ApplyAttackerStrategy(attackerStrategy, defenderStrategy, damage, round);
        damage = ApplyDefenderStrategy(attackerStrategy, defenderStrategy, damage, round);
        damage = ApplyDefenderAgilityMitigation(defender, damage);
        damage = ApplyTraitModifiers(attacker, defender, damage, round);
        damage = ApplyRandomDamageVariance(damage);
        damage = TryApplyGlancingDodge(defender, defenderStrategy, damage, out glancingHit);

        return ClampFinalDamage(baseDamage, damage);
    }

    float GetBaseDodgeChance(Dog defender)
    {
        if (defender == null)
        {
            return 0.05f;
        }

        float dodgeChance = (defender.agility * 0.01f) + (defender.GetIntelligence() * 0.0025f);
        return Mathf.Clamp(dodgeChance, 0.05f, 0.30f);
    }

    float GetStrategyDodgeBonus(FightStrategy defenderStrategy)
    {
        switch (defenderStrategy)
        {
            case FightStrategy.RushEarly:
                return -0.03f;

            case FightStrategy.CounterPlan:
                return 0.08f;

            case FightStrategy.WearDown:
                return 0.03f;

            case FightStrategy.DefensiveShell:
                return 0.05f;

            case FightStrategy.AllIn:
                return -0.08f;

            case FightStrategy.SecondWind:
                return 0.12f;

            case FightStrategy.Balanced:
            default:
                return 0f;
        }
    }

    float TryApplyGlancingDodge(Dog defender, FightStrategy defenderStrategy, float incomingDamage, out bool glancingHit)
    {
        glancingHit = false;
        float dodgeChance = Mathf.Clamp(
            GetBaseDodgeChance(defender) + GetStrategyDodgeBonus(defenderStrategy),
            0.05f,
            0.40f
        );

        if (Random.value <= dodgeChance)
        {
            glancingHit = true;
            return incomingDamage * GlancingDodgeDamageMultiplier;
        }

        return incomingDamage;
    }

    float ApplyTraitModifiers(Dog attacker, Dog defender, float damage, int round)
    {
        if (HasTrait(attacker, DogTrait.Aggressive))
        {
            damage *= 1.08f;
        }

        if (HasTrait(attacker, DogTrait.GlassCannon))
        {
            damage *= 1.12f;
        }

        if (HasTrait(attacker, DogTrait.Clutch) && round >= maxRounds - 1)
        {
            damage *= 1.15f;
        }

        if (HasTrait(defender, DogTrait.Durable))
        {
            damage *= 0.90f;
        }

        if (HasTrait(defender, DogTrait.GlassCannon))
        {
            damage *= 1.08f;
        }

        return damage;
    }

    float ApplyStyleModifier(Dog attacker, Dog defender, float damage, int round)
    {
        switch (attacker.fightStyle)
        {
            case FightStyle.Rushdown:
                damage *= round <= 2 ? 1.12f : 0.94f;
                break;

            case FightStyle.Counter:
                if (defender.fightStyle == FightStyle.Rushdown)
                {
                    damage *= 1.15f;
                }
                else if (defender.strength > attacker.strength)
                {
                    damage *= 1.08f;
                }
                break;

            case FightStyle.Tank:
                damage *= 0.94f;
                break;

            case FightStyle.Wildcard:
                damage *= Random.Range(0.92f, 1.13f);
                break;

            case FightStyle.Balanced:
            default:
                break;
        }

        return damage;
    }

    float ApplyAttackerStrategy(
        FightStrategy attackerStrategy,
        FightStrategy defenderStrategy,
        float damage,
        int round
    )
    {
        switch (attackerStrategy)
        {
            case FightStrategy.RushEarly:
                damage *= round <= 2 ? 1.15f : 0.92f;
                break;

            case FightStrategy.CounterPlan:
                if (defenderStrategy == FightStrategy.RushEarly || defenderStrategy == FightStrategy.AllIn)
                {
                    damage *= 1.18f;
                }
                else
                {
                    damage *= 0.95f;
                }
                break;

            case FightStrategy.WearDown:
                damage *= round >= 4 ? 1.16f : 0.92f;
                break;

            case FightStrategy.DefensiveShell:
                damage *= 0.88f;
                break;

            case FightStrategy.AllIn:
                damage *= Random.Range(1.12f, 1.24f);
                break;

            case FightStrategy.SecondWind:
                damage *= 0.75f;

                if (defenderStrategy == FightStrategy.WearDown)
                {
                    damage *= 1.12f;
                }
                else if (defenderStrategy == FightStrategy.CounterPlan)
                {
                    damage *= 1.08f;
                }
                else if (defenderStrategy == FightStrategy.AllIn ||
                         (defenderStrategy == FightStrategy.RushEarly && round <= 2))
                {
                    damage *= 0.88f;
                }

                break;

            case FightStrategy.Balanced:
            default:
                break;
        }

        return damage;
    }

    float ApplyDefenderStrategy(
        FightStrategy attackerStrategy,
        FightStrategy defenderStrategy,
        float incomingDamage,
        int round
    )
    {
        switch (defenderStrategy)
        {
            case FightStrategy.DefensiveShell:
                incomingDamage *= 0.80f;
                break;

            case FightStrategy.WearDown:
                if (round >= 4)
                {
                    incomingDamage *= 0.92f;
                }
                break;

            case FightStrategy.AllIn:
                incomingDamage *= 1.10f;
                break;

            case FightStrategy.SecondWind:
                if (attackerStrategy == FightStrategy.WearDown)
                {
                    incomingDamage *= 0.88f;
                }
                else if (attackerStrategy == FightStrategy.CounterPlan)
                {
                    incomingDamage *= 0.92f;
                }
                else if (attackerStrategy == FightStrategy.AllIn ||
                         (attackerStrategy == FightStrategy.RushEarly && round <= 2))
                {
                    incomingDamage *= 1.12f;
                }

                break;

            case FightStrategy.RushEarly:
            case FightStrategy.CounterPlan:
            case FightStrategy.Balanced:
            default:
                break;
        }

        return incomingDamage;
    }

    float ApplyDefenderAgilityMitigation(Dog defender, float incomingDamage)
    {
        if (defender == null)
        {
            return incomingDamage;
        }

        float agilityMitigation = Mathf.Clamp(1f - (defender.agility * 0.0018f), 0.82f, 0.96f);
        return incomingDamage * agilityMitigation;
    }

    float ApplyRandomDamageVariance(float damage)
    {
        return damage * Random.Range(0.90f, 1.10f);
    }

    int ClampFinalDamage(float baseDamage, float finalDamage)
    {
        float safeBaseDamage = Mathf.Max(1f, baseDamage);
        float clampedDamage = Mathf.Clamp(finalDamage, 1f, safeBaseDamage * 1.75f);
        return Mathf.Max(1, Mathf.RoundToInt(clampedDamage));
    }

    int ApplySecondWindHealing(
        Dog dog,
        FightStrategy strategy,
        int currentHealth,
        int maxHealth,
        out int healingApplied
    )
    {
        healingApplied = 0;

        if (dog == null || strategy != FightStrategy.SecondWind || currentHealth <= 0)
        {
            return currentHealth;
        }

        int healingCap = Mathf.Max(1, Mathf.RoundToInt(maxHealth * SecondWindHealingCapPercent));

        if (currentHealth >= healingCap)
        {
            return currentHealth;
        }

        int healAmount = Mathf.Max(1, Mathf.RoundToInt(
            4f +
            (dog.stamina * 0.25f) +
            (dog.GetIntelligence() * 0.10f)
        ));
        int healedHealth = Mathf.Min(healingCap, currentHealth + healAmount);
        healingApplied = healedHealth - currentHealth;

        return healedHealth;
    }

    string GetStrategyMatchupFeedback(
        Dog d1,
        Dog d2,
        FightStrategy strategy1,
        FightStrategy strategy2,
        int round
    )
    {
        string fighter1Name = d1 != null ? d1.dogName : "Fighter 1";
        string fighter2Name = d2 != null ? d2.dogName : "Fighter 2";

        if (strategy1 == FightStrategy.SecondWind)
        {
            string feedback = GetSecondWindMatchupFeedback(fighter1Name, fighter2Name, strategy2, round);
            if (!string.IsNullOrEmpty(feedback))
            {
                return feedback;
            }
        }

        if (strategy2 == FightStrategy.SecondWind)
        {
            return GetSecondWindMatchupFeedback(fighter2Name, fighter1Name, strategy1, round);
        }

        return string.Empty;
    }

    string GetSecondWindMatchupFeedback(
        string secondWindName,
        string opponentName,
        FightStrategy opponentStrategy,
        int round
    )
    {
        switch (opponentStrategy)
        {
            case FightStrategy.RushEarly:
                if (round <= 2)
                {
                    return $"{opponentName}'s RushEarly pressures {secondWindName}'s SecondWind before recovery settles.";
                }

                break;

            case FightStrategy.AllIn:
                return $"{opponentName}'s AllIn threatens {secondWindName}'s SecondWind with burst damage.";

            case FightStrategy.WearDown:
                return $"{secondWindName}'s SecondWind resists {opponentName}'s WearDown pressure.";

            case FightStrategy.CounterPlan:
                return $"{secondWindName}'s SecondWind stays patient against {opponentName}'s CounterPlan.";

            case FightStrategy.DefensiveShell:
                return "SecondWind and DefensiveShell both slow the fight into a cautious exchange.";

            case FightStrategy.Balanced:
                return $"{secondWindName}'s SecondWind looks for safe recovery windows against Balanced pressure.";
        }

        return string.Empty;
    }

    string GetRoundFlavor(
        Dog d1,
        Dog d2,
        int dmg1,
        int dmg2,
        int round,
        FightStrategy strategy1,
        FightStrategy strategy2
    )
    {
        if (round == 1)
        {
            if (strategy1 == FightStrategy.RushEarly && dmg1 > dmg2)
            {
                return $"{d1.dogName} explodes out fast and takes early control.";
            }

            if (strategy2 == FightStrategy.RushEarly && dmg2 > dmg1)
            {
                return $"{d2.dogName} storms forward and wins the opening exchange.";
            }

            if (strategy1 == FightStrategy.DefensiveShell || strategy2 == FightStrategy.DefensiveShell)
            {
                return "Both fighters test distance while one side shells up early.";
            }

            if (strategy1 == FightStrategy.SecondWind || strategy2 == FightStrategy.SecondWind)
            {
                return "One fighter opens carefully, looking to slip damage and recover rhythm.";
            }

            return "Both fighters circle and feel out the opening rhythm.";
        }

        if (strategy1 == FightStrategy.CounterPlan && dmg1 > dmg2)
        {
            return $"{d1.dogName} reads the attack and fires back with a clean counter.";
        }

        if (strategy2 == FightStrategy.CounterPlan && dmg2 > dmg1)
        {
            return $"{d2.dogName} times the pressure and answers with a sharp counter.";
        }

        if (strategy1 == FightStrategy.WearDown && round >= 4 && dmg1 > dmg2)
        {
            return $"{d1.dogName}'s wear-down plan starts paying off late.";
        }

        if (strategy2 == FightStrategy.WearDown && round >= 4 && dmg2 > dmg1)
        {
            return $"{d2.dogName}'s patience starts breaking the fight open.";
        }

        if (strategy1 == FightStrategy.SecondWind && dmg2 <= dmg1)
        {
            return $"{d1.dogName} slips enough pressure to keep the recovery plan alive.";
        }

        if (strategy2 == FightStrategy.SecondWind && dmg1 <= dmg2)
        {
            return $"{d2.dogName} evades the worst of the exchange and steadies up.";
        }

        if (strategy1 == FightStrategy.AllIn && dmg1 > dmg2 + 8)
        {
            return $"{d1.dogName} goes all-in and lands a heavy momentum swing.";
        }

        if (strategy2 == FightStrategy.AllIn && dmg2 > dmg1 + 8)
        {
            return $"{d2.dogName} commits hard and turns the round violent.";
        }

        if (dmg1 > dmg2 + 10)
        {
            return $"{d1.dogName} dominates the exchange and forces {d2.dogName} backward.";
        }

        if (dmg2 > dmg1 + 10)
        {
            return $"{d2.dogName} takes over the round and pushes {d1.dogName} off rhythm.";
        }

        if (Mathf.Abs(dmg1 - dmg2) <= 3)
        {
            return "The round is nearly even, both fighters trading without a clear edge.";
        }

        if (dmg1 > dmg2)
        {
            return $"{d1.dogName} edges the round with cleaner pressure.";
        }

        if (dmg2 > dmg1)
        {
            return $"{d2.dogName} edges the round with better timing.";
        }

        return "Both fighters reset after a dead-even exchange.";
    }

    void ResolveFightResult(
        Dog d1,
        Dog d2,
        int health1,
        int health2,
        int startingHealth1,
        int startingHealth2,
        bool d1WasBehind,
        bool d2WasBehind,
        ref string log
    )
    {
        d1.totalFights++;
        d2.totalFights++;

        int outcome = GetFightDecisionOutcome(health1, health2, startingHealth1, startingHealth2);
        bool wentToDecision = health1 > 0 && health2 > 0;

        if (outcome == 1)
        {
            AwardFightWinner(
                d1,
                d2,
                health1,
                health2,
                startingHealth1,
                startingHealth2,
                d1WasBehind,
                wentToDecision,
                ref log
            );
        }
        else if (outcome == 2)
        {
            AwardFightWinner(
                d2,
                d1,
                health2,
                health1,
                startingHealth2,
                startingHealth1,
                d2WasBehind,
                wentToDecision,
                ref log
            );
        }
        else
        {
            AwardFightDraw(d1, d2, health1, health2, startingHealth1, startingHealth2, wentToDecision, ref log);
        }
    }

    int GetFightDecisionOutcome(int health1, int health2, int startingHealth1, int startingHealth2)
    {
        bool fighter1Out = health1 <= 0;
        bool fighter2Out = health2 <= 0;

        if (fighter1Out && fighter2Out)
        {
            return 0;
        }

        if (fighter2Out)
        {
            return 1;
        }

        if (fighter1Out)
        {
            return 2;
        }

        float fighter1HealthPercent = GetRemainingHealthPercent(health1, startingHealth1);
        float fighter2HealthPercent = GetRemainingHealthPercent(health2, startingHealth2);
        float decisionMargin = Mathf.Abs(fighter1HealthPercent - fighter2HealthPercent);

        if (decisionMargin <= DecisionDrawThreshold)
        {
            return 0;
        }

        return fighter1HealthPercent > fighter2HealthPercent ? 1 : 2;
    }

    float GetRemainingHealthPercent(int health, int startingHealth)
    {
        return Mathf.Clamp01((float)Mathf.Max(0, health) / Mathf.Max(1, startingHealth));
    }

    void AwardFightWinner(
        Dog winner,
        Dog loser,
        int winnerHealth,
        int loserHealth,
        int winnerStartingHealth,
        int loserStartingHealth,
        bool winnerWasBehind,
        bool wentToDecision,
        ref string log
    )
    {
        winner.wins++;
        loser.losses++;

        string resultLabel = GetResultLabel(
            winner,
            loser,
            winnerHealth,
            loserHealth,
            winnerStartingHealth,
            winnerWasBehind
        );

        AwardFightXP(winner, true, resultLabel, ref log);
        AwardFightXP(loser, false, resultLabel, ref log);

        if (wentToDecision)
        {
            float winnerHealthPercent = GetRemainingHealthPercent(winnerHealth, winnerStartingHealth);
            float loserHealthPercent = GetRemainingHealthPercent(loserHealth, loserStartingHealth);
            log += $"<b>WINS BY DECISION: {winner.dogName}</b>\n";
            log += $"Decision: {winnerHealthPercent:P0} HP vs {loserHealthPercent:P0} HP.\n";
        }

        log += $"<b>WINNER: {winner.dogName}</b>\n";
        log += $"<b>Result Type: {resultLabel}</b>";
    }

    void AwardFightDraw(
        Dog d1,
        Dog d2,
        int health1,
        int health2,
        int startingHealth1,
        int startingHealth2,
        bool wentToDecision,
        ref string log
    )
    {
        AwardFightXP(d1, false, "Draw", ref log);
        AwardFightXP(d2, false, "Draw", ref log);

        if (wentToDecision)
        {
            float healthPercent1 = GetRemainingHealthPercent(health1, startingHealth1);
            float healthPercent2 = GetRemainingHealthPercent(health2, startingHealth2);
            log += $"<b>TRUE DRAW</b>: Too close to call ({healthPercent1:P0} HP vs {healthPercent2:P0} HP).\n";
        }

        log += "<b>DRAW!</b>\n";
        log += "<b>Result Type: Dead Even</b>";
    }

    string GetResultLabel(
        Dog winner,
        Dog loser,
        int winnerHealth,
        int loserHealth,
        int winnerStartingHealth,
        bool winnerWasBehind
    )
    {
        int healthMargin = winnerHealth - loserHealth;
        float winnerHealthPercent = (float)winnerHealth / winnerStartingHealth;

        if (winnerWasBehind && winnerHealthPercent <= 0.35f)
        {
            return "Comeback Win";
        }

        if (winnerWasBehind)
        {
            return "Momentum Swing Victory";
        }

        if (loserHealth <= 0 && winnerHealthPercent >= 0.65f)
        {
            return "Dominant Finish";
        }

        if (healthMargin <= 10)
        {
            return "Close Decision";
        }

        if (winnerHealthPercent >= 0.75f)
        {
            return "Dominant Win";
        }

        if (winner.fightStyle == FightStyle.Wildcard)
        {
            return "Unstable Upset";
        }

        if (winner.fightStyle == FightStyle.Tank && winnerHealthPercent >= 0.5f)
        {
            return "Control Win";
        }

        return "Standard Victory";
    }

    void AwardFightXP(Dog dog, bool won, string resultLabel, ref string log)
    {
        int xpGain = won ? 35 : 15;

        if (HasTrait(dog, DogTrait.Prodigy))
        {
            xpGain += 5;
        }

        switch (resultLabel)
        {
            case "Dominant Finish":
                xpGain += won ? 20 : 0;
                break;

            case "Dominant Win":
                xpGain += won ? 15 : 0;
                break;

            case "Comeback Win":
                xpGain += won ? 25 : 0;
                break;

            case "Momentum Swing Victory":
                xpGain += won ? 18 : 0;
                break;

            case "Close Decision":
                xpGain += 8;
                break;

            case "Unstable Upset":
                xpGain += won ? 20 : 0;
                break;

            case "Control Win":
                xpGain += won ? 12 : 0;
                break;

            case "Draw":
                xpGain = 20;
                break;
        }

        dog.xp += xpGain;

        log += $"{dog.dogName} gained {xpGain} XP.\n";

        while (dog.xp >= dog.xpToNextLevel)
        {
            dog.xp -= dog.xpToNextLevel;
            dog.level++;

            dog.xpToNextLevel = CalculateXPToNextLevel(dog.level);

            int strengthGain = Mathf.Max(1, Mathf.RoundToInt(Random.Range(1, 4) * dog.growthRate));
            int agilityGain = Mathf.Max(1, Mathf.RoundToInt(Random.Range(1, 4) * dog.growthRate));
            int staminaGain = Mathf.Max(1, Mathf.RoundToInt(Random.Range(1, 4) * dog.growthRate));
            int intelligenceGain = Mathf.Max(1, Mathf.RoundToInt(Random.Range(1, 3) * dog.growthRate));

            if (HasTrait(dog, DogTrait.LateBloomer) && dog.level >= 5)
            {
                strengthGain++;
                agilityGain++;
                staminaGain++;
                intelligenceGain++;
            }

            dog.strength = Mathf.Min(dog.strength + strengthGain, Mathf.Min(100, dog.strengthPotential));
            dog.agility = Mathf.Min(dog.agility + agilityGain, Mathf.Min(100, dog.agilityPotential));
            dog.stamina = Mathf.Min(dog.stamina + staminaGain, Mathf.Min(100, dog.staminaPotential));
            dog.intelligence = Mathf.Min(
                dog.GetIntelligence() + intelligenceGain,
                Mathf.Min(100, dog.GetIntelligencePotential())
            );

            log += $"<b>{dog.dogName} leveled up to Level {dog.level}!</b>\n";
            log += $"{dog.dogName} gained +{strengthGain} STR, +{agilityGain} AGI, +{staminaGain} STA, +{intelligenceGain} INT.\n";
        }
    }

    bool HasTrait(Dog dog, DogTrait trait)
    {
        return dog.primaryTrait == trait || dog.secondaryTrait == trait;
    }

    int CalculateXPToNextLevel(int level)
    {
        return 100 + ((level - 1) * 50);
    }

    void SetLog(string message)
    {
        if (fightLog != null)
        {
            fightLog.text = message;
        }

        Canvas.ForceUpdateCanvases();

        if (fightLogScrollRect != null)
        {
            fightLogScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    void ConfigureScrollableFightLog()
    {
        if (fightLog == null)
        {
            return;
        }

        RectTransform fightLogRect = fightLog.rectTransform;
        RectTransform panelRect = fightLogPanel != null
            ? fightLogPanel
            : FindSceneComponentByName<RectTransform>("FightLogPanel");

        if (panelRect == null)
        {
            panelRect = fightLogRect.parent as RectTransform;
        }

        if (panelRect == null)
        {
            Debug.LogWarning("Fight log could not be constrained because FightLogPanel is missing.");
            return;
        }

        fightLogPanel = panelRect;
        RectTransform viewportRect = fightLogViewport != null ? fightLogViewport : panelRect;

        if (viewportRect.GetComponent<RectMask2D>() == null)
        {
            viewportRect.gameObject.AddComponent<RectMask2D>();
        }
    }

    void FindFightPreviewReferencesIfMissing()
    {
        if (playerFightPreviewPanel == null)
        {
            playerFightPreviewPanel = FindSceneComponentByName<RectTransform>("PlayerFightPreviewPanel") ??
                                      FindSceneComponentByName<RectTransform>("Fighter1Panel");
        }

        if (opponentFightPreviewPanel == null)
        {
            opponentFightPreviewPanel = FindSceneComponentByName<RectTransform>("OpponentFightPreviewPanel") ??
                                        FindSceneComponentByName<RectTransform>("Fighter2Panel");
        }

        if (playerFightPreviewText == null)
        {
            playerFightPreviewText = FindSceneComponentByName<TextMeshProUGUI>("PlayerFightPreviewText");
        }

        if (opponentFightPreviewText == null)
        {
            opponentFightPreviewText = FindSceneComponentByName<TextMeshProUGUI>("OpponentFightPreviewText");
        }

        if (playerFightPreviewText == null)
        {
            playerFightPreviewText = CreateFightPreviewTextFallback("PlayerFightPreviewText", playerFightPreviewPanel, "Fighter1Title");
            playerFightPreviewTextIsRuntimeFallback = playerFightPreviewText != null;
        }

        if (opponentFightPreviewText == null)
        {
            opponentFightPreviewText = CreateFightPreviewTextFallback("OpponentFightPreviewText", opponentFightPreviewPanel, "Fighter2Title");
            opponentFightPreviewTextIsRuntimeFallback = opponentFightPreviewText != null;
        }

        if (playerFightDogImage == null)
        {
            playerFightDogImage = FindSceneComponentByName<Image>("PlayerFightDogImage");
        }

        if (opponentFightDogImage == null)
        {
            opponentFightDogImage = FindSceneComponentByName<Image>("OpponentFightDogImage");
        }
    }

    TextMeshProUGUI CreateFightPreviewTextFallback(string objectName, RectTransform panelRect, string titleObjectName)
    {
        if (panelRect == null)
        {
            Debug.LogWarning($"{objectName} could not be created because its fight preview panel is missing.");
            return null;
        }

        GameObject previewObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        previewObject.transform.SetParent(panelRect, false);

        RectTransform previewRect = previewObject.GetComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0f, 0f);
        previewRect.anchorMax = new Vector2(1f, 0f);
        previewRect.pivot = new Vector2(0.5f, 0f);
        previewRect.anchoredPosition = new Vector2(0f, 12f);
        previewRect.sizeDelta = new Vector2(-28f, 135f);

        TextMeshProUGUI previewText = previewObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI titleText = FindSceneComponentByName<TextMeshProUGUI>(titleObjectName);

        if (titleText != null)
        {
            previewText.font = titleText.font;
            previewText.fontSharedMaterial = titleText.fontSharedMaterial;
        }

        return previewText;
    }

    void ConfigureFightPreviewText(TextMeshProUGUI previewText, bool isRuntimeFallback)
    {
        if (previewText == null || !isRuntimeFallback)
        {
            return;
        }

        previewText.raycastTarget = false;
        previewText.alignment = TextAlignmentOptions.Center;
        previewText.textWrappingMode = TextWrappingModes.Normal;
        previewText.overflowMode = TextOverflowModes.Truncate;
        previewText.fontSize = 12f;
        previewText.margin = Vector4.zero;
        previewText.color = Color.white;
    }

    void FindEndFightButtonIfMissing()
    {
        if (endFightButton == null)
        {
            endFightButton = FindSceneComponentByName<Button>("EndFightButton");
        }
    }

    void SetEndFightButtonVisible(bool isVisible)
    {
        if (endFightButton == null)
        {
            if (isVisible)
            {
                Debug.LogWarning("Fight resolved, but EndFightButton is not assigned. Create a scene button named EndFightButton and wire PageManager.EndCurrentFight().");
            }

            return;
        }

        endFightButton.gameObject.SetActive(isVisible);

        if (isVisible)
        {
            Debug.Log("EndFightButton shown.");
        }
    }

    public void RefreshFightPreviews(bool forceRefresh = false)
    {
        Dog playerDog = activeFighter1 != null ? activeFighter1 : dogManager != null ? dogManager.selectedFighter1 : null;
        Dog opponentDog = activeFighter2 != null
            ? activeFighter2
            : pendingStoryFight && pendingStoryOpponent != null
                ? pendingStoryOpponent
                : dogManager != null ? dogManager.selectedFighter2 : null;

        FightStrategy playerStrategy = dogManager != null ? dogManager.fighter1Strategy : FightStrategy.Balanced;
        FightStrategy opponentStrategy = dogManager != null ? dogManager.fighter2Strategy : FightStrategy.Balanced;

        if (!forceRefresh &&
            playerDog == lastPreviewPlayerDog &&
            opponentDog == lastPreviewOpponentDog &&
            playerStrategy == lastPreviewPlayerStrategy &&
            opponentStrategy == lastPreviewOpponentStrategy)
        {
            return;
        }

        lastPreviewPlayerDog = playerDog;
        lastPreviewOpponentDog = opponentDog;
        lastPreviewPlayerStrategy = playerStrategy;
        lastPreviewOpponentStrategy = opponentStrategy;

        SetFightPreviewText(playerFightPreviewText, playerDog, playerStrategy, "PLAYER FIGHTER");
        SetFightPreviewText(opponentFightPreviewText, opponentDog, opponentStrategy, "OPPONENT FIGHTER");
        SetFightPreviewDogImage(playerFightDogImage, playerDog);
        SetFightPreviewDogImage(opponentFightDogImage, opponentDog);
    }

    void SetFightPreviewText(TextMeshProUGUI previewText, Dog dog, FightStrategy strategy, string heading)
    {
        if (previewText == null)
        {
            return;
        }

        if (dog == null)
        {
            previewText.text = $"<b>{heading}</b>\nSelect a dog.";
            return;
        }

        string dogName = string.IsNullOrWhiteSpace(dog.dogName) ? "Unnamed Dog" : dog.dogName.Trim();
        string breed = string.IsNullOrWhiteSpace(dog.breed) ? "Unknown Breed" : dog.breed.Trim();

        previewText.text =
            $"<b>{heading}</b>\n" +
            $"<color=#7CF6FF>{dogName}</color>\n" +
            $"{dog.gender} | {breed}\n" +
            $"{DogPreviewTextFormatter.GetCompactStats(dog)}\n" +
            $"Style: {dog.fightStyle} | Strategy: {strategy}\n" +
            $"{DogPreviewTextFormatter.GetGenerationLabel(dog)} | Traits: {DogPreviewTextFormatter.GetShortTraitSummary(dog)}";
    }

    void SetFightPreviewDogImage(Image image, Dog dog)
    {
        if (image == null)
        {
            return;
        }

        Sprite portraitSprite = dog == null
            ? null
            : DogPortraitLibrary.ChooseStableCardPortrait(dog, dog.dogSprite, null);

        image.sprite = portraitSprite;
        image.enabled = portraitSprite != null;
    }

    T FindSceneComponentByName<T>(string objectName) where T : Component
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        T[] components = Resources.FindObjectsOfTypeAll<T>();

        for (int i = 0; i < components.Length; i++)
        {
            T component = components[i];

            if (component != null && component.gameObject != null &&
                component.gameObject.scene.IsValid() && component.gameObject.name == objectName)
            {
                return component;
            }
        }

        return null;
    }

    void SetFightNarration(string headline, string details)
    {
        if (narratorManager == null)
        {
            narratorManager = GetComponent<NarratorManager>();
        }

        if (narratorManager == null)
        {
            narratorManager = FindFirstObjectByType<NarratorManager>();
        }

        if (narratorManager != null)
        {
            narratorManager.SetNarration(headline, details);
        }
    }
}
