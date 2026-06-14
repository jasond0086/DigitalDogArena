using UnityEngine;
using TMPro;

public class FightManager : MonoBehaviour
{
    [Header("References")]
    public DogManager dogManager;
    public LeagueManager leagueManager;
    public RivalManager rivalManager;
    public StoryManager storyManager;
    public EconomyManager economyManager;
    public NarratorManager narratorManager;
    public TextMeshProUGUI fightLog;

    [Header("Fight Settings")]
    public int maxRounds = 6;
    public int healthMultiplier = 2;
    public int damageVariance = 15;

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
    private int fighter1WinsBeforeFight;
    private int fighter2WinsBeforeFight;

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
    }

    public void StartFight()
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
    }

    void InitializeFight(Dog fighter1, Dog fighter2, bool isRivalFight, RivalHandlerData rival)
    {
        activeFighter1 = fighter1;
        activeFighter2 = fighter2;
        activeFightIsRival = isRivalFight;
        activeRival = rival;
        currentRound = 0;
        startingFighter1Health = CalculateStartingHealth(activeFighter1);
        startingFighter2Health = CalculateStartingHealth(activeFighter2);
        currentFighter1Health = startingFighter1Health;
        currentFighter2Health = startingFighter2Health;
        fighter1WasBehind = false;
        fighter2WasBehind = false;
        fighter1WinsBeforeFight = activeFighter1.wins;
        fighter2WinsBeforeFight = activeFighter2.wins;

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
        SetLog(runningFightLog);
        SetFightNarration(
            $"Fight Started: {activeFighter1.dogName} vs {activeFighter2.dogName}",
            runningFightLog
        );
    }

    public void PlayNextRound()
    {
        if (!fightInProgress || activeFighter1 == null || activeFighter2 == null)
        {
            return;
        }

        currentRound++;

        FightStrategy strategy1 = dogManager.fighter1Strategy;
        FightStrategy strategy2 = dogManager.fighter2Strategy;

        int rawDmg1 = Random.Range(
            activeFighter1.strength - damageVariance,
            activeFighter1.strength + damageVariance + 1
        ) - (activeFighter2.agility / 6);

        int rawDmg2 = Random.Range(
            activeFighter2.strength - damageVariance,
            activeFighter2.strength + damageVariance + 1
        ) - (activeFighter1.agility / 6);

        int dmg1 = CalculateFinalDamage(activeFighter1, activeFighter2, rawDmg1, currentRound, strategy1, strategy2);
        int dmg2 = CalculateFinalDamage(activeFighter2, activeFighter1, rawDmg2, currentRound, strategy2, strategy1);

        currentFighter2Health = Mathf.Max(0, currentFighter2Health - dmg1);
        currentFighter1Health = Mathf.Max(0, currentFighter1Health - dmg2);

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
        runningFightLog += $"{activeFighter1.dogName} impact: {dmg1} | {activeFighter2.dogName} impact: {dmg2}\n";
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

        fightInProgress = false;

        if (activeFightIsRival)
        {
            FinishRivalFight();
        }
        else
        {
            FinishNormalFight();
        }

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

    int CalculateStartingHealth(Dog dog)
    {
        int health = dog.stamina * healthMultiplier;

        if (HasTrait(dog, DogTrait.Durable))
        {
            health += Mathf.RoundToInt(dog.stamina * 0.25f);
        }

        if (HasTrait(dog, DogTrait.GlassCannon))
        {
            health -= Mathf.RoundToInt(dog.stamina * 0.15f);
        }

        return Mathf.Max(1, health);
    }

        int CalculateFinalDamage(
        Dog attacker,
        Dog defender,
        int baseDamage,
        int round,
        FightStrategy attackerStrategy,
        FightStrategy defenderStrategy
    )
    {
        int damage = baseDamage;

        damage = ApplyStyleModifier(attacker, defender, damage, round);
        damage = ApplyAttackerStrategy(attackerStrategy, defenderStrategy, damage, round);
        damage = ApplyDefenderStrategy(defenderStrategy, damage, round);
        damage = ApplyTraitModifiers(attacker, defender, damage, round);

        return Mathf.Max(0, damage);
    }

    int ApplyTraitModifiers(Dog attacker, Dog defender, int damage, int round)
    {
        if (HasTrait(attacker, DogTrait.Aggressive))
        {
            damage += 5;
        }

        if (HasTrait(attacker, DogTrait.GlassCannon))
        {
            damage += 8;
        }

        if (HasTrait(attacker, DogTrait.Clutch) && round >= maxRounds - 1)
        {
            damage += 10;
        }

        if (HasTrait(defender, DogTrait.Durable))
        {
            damage -= 4;
        }

        if (HasTrait(defender, DogTrait.GlassCannon))
        {
            damage += 4;
        }

        return damage;
    }

    int ApplyStyleModifier(Dog attacker, Dog defender, int damage, int round)
    {
        switch (attacker.fightStyle)
        {
            case FightStyle.Rushdown:
                damage += round <= 2 ? 10 : -5;
                break;

            case FightStyle.Counter:
                if (defender.fightStyle == FightStyle.Rushdown)
                {
                    damage += 10;
                }
                else if (defender.strength > attacker.strength)
                {
                    damage += 6;
                }
                break;

            case FightStyle.Tank:
                damage -= 3;
                damage = Mathf.Max(damage, attacker.strength / 3);
                break;

            case FightStyle.Wildcard:
                damage += Random.Range(-15, 21);
                break;

            case FightStyle.Balanced:
            default:
                break;
        }

        return damage;
    }

    int ApplyAttackerStrategy(
        FightStrategy attackerStrategy,
        FightStrategy defenderStrategy,
        int damage,
        int round
    )
    {
        switch (attackerStrategy)
        {
            case FightStrategy.RushEarly:
                damage += round <= 2 ? 8 : -4;
                break;

            case FightStrategy.CounterPlan:
                if (defenderStrategy == FightStrategy.RushEarly || defenderStrategy == FightStrategy.AllIn)
                {
                    damage += 8;
                }
                else
                {
                    damage -= 2;
                }
                break;

            case FightStrategy.WearDown:
                damage += round >= 4 ? 10 : -4;
                break;

            case FightStrategy.DefensiveShell:
                damage -= 5;
                break;

            case FightStrategy.AllIn:
                damage += Random.Range(6, 16);
                break;

            case FightStrategy.Balanced:
            default:
                break;
        }

        return damage;
    }

    int ApplyDefenderStrategy(FightStrategy defenderStrategy, int incomingDamage, int round)
    {
        switch (defenderStrategy)
        {
            case FightStrategy.DefensiveShell:
                incomingDamage -= 7;
                break;

            case FightStrategy.WearDown:
                if (round >= 4)
                {
                    incomingDamage -= 4;
                }
                break;

            case FightStrategy.AllIn:
                incomingDamage += 5;
                break;

            case FightStrategy.RushEarly:
            case FightStrategy.CounterPlan:
            case FightStrategy.Balanced:
            default:
                break;
        }

        return incomingDamage;
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

        if (health1 > health2)
        {
            d1.wins++;
            d2.losses++;

            string resultLabel = GetResultLabel(
                d1,
                d2,
                health1,
                health2,
                startingHealth1,
                d1WasBehind
            );

            AwardFightXP(d1, true, resultLabel, ref log);
            AwardFightXP(d2, false, resultLabel, ref log);

            log += $"<b>WINNER: {d1.dogName}</b>\n";
            log += $"<b>Result Type: {resultLabel}</b>";
        }
        else if (health2 > health1)
        {
            d2.wins++;
            d1.losses++;

            string resultLabel = GetResultLabel(
                d2,
                d1,
                health2,
                health1,
                startingHealth2,
                d2WasBehind
            );

            AwardFightXP(d2, true, resultLabel, ref log);
            AwardFightXP(d1, false, resultLabel, ref log);

            log += $"<b>WINNER: {d2.dogName}</b>\n";
            log += $"<b>Result Type: {resultLabel}</b>";
        }
        else
        {
            AwardFightXP(d1, false, "Draw", ref log);
            AwardFightXP(d2, false, "Draw", ref log);

            log += "<b>DRAW!</b>\n";
            log += "<b>Result Type: Dead Even</b>";
        }
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

            if (HasTrait(dog, DogTrait.LateBloomer) && dog.level >= 5)
            {
                strengthGain++;
                agilityGain++;
                staminaGain++;
            }

            dog.strength = Mathf.Min(dog.strength + strengthGain, Mathf.Min(100, dog.strengthPotential));
            dog.agility = Mathf.Min(dog.agility + agilityGain, Mathf.Min(100, dog.agilityPotential));
            dog.stamina = Mathf.Min(dog.stamina + staminaGain, Mathf.Min(100, dog.staminaPotential));

            log += $"<b>{dog.dogName} leveled up to Level {dog.level}!</b>\n";
            log += $"{dog.dogName} gained +{strengthGain} STR, +{agilityGain} AGI, +{staminaGain} STA.\n";
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
