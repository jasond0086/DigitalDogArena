using UnityEngine;
using TMPro;

public class FightManager : MonoBehaviour
{
    [Header("References")]
    public DogManager dogManager;
    public LeagueManager leagueManager;
    public RivalManager rivalManager;
    public StoryManager storyManager;
    public TextMeshProUGUI fightLog;

    [Header("Fight Settings")]
    public int maxRounds = 6;
    public int healthMultiplier = 2;
    public int damageVariance = 15;

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
    }

    public void StartFight()
    {
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

        SimulateFight(dog1, dog2);
    }

    public void StartRivalFight()
    {
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

        int playerWinsBeforeFight = playerDog.wins;

        SimulateFight(playerDog, rival.rivalDog);

        if (playerDog.wins > playerWinsBeforeFight)
        {
            rivalManager.MarkRivalDefeated(rival.rivalId);
            ApplyRivalWinReward(rival.leagueName);
        }
        else if (storyManager != null)
        {
            storyManager.AddRiskModifier(0.02f);
        }

        rivalManager.RefreshRivalStatusUI();

        if (leagueManager != null)
        {
            leagueManager.RefreshLeagueProgress();
        }

        dogManager.SaveStable();
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

    void SimulateFight(Dog d1, Dog d2)
    {
        FightStrategy strategy1 = dogManager.fighter1Strategy;
        FightStrategy strategy2 = dogManager.fighter2Strategy;

        string log = $"<b>{d1.dogName} vs {d2.dogName}</b>\n\n";

        log += $"{d1.dogName} Style: {d1.fightStyle} | Strategy: {strategy1}\n";
        log += $"{d2.dogName} Style: {d2.fightStyle} | Strategy: {strategy2}\n\n";
        log += $"{d1.dogName} Traits: {d1.GetTraitSummary()}\n";
        log += $"{d2.dogName} Traits: {d2.GetTraitSummary()}\n\n";

        int health1 = CalculateStartingHealth(d1);
        int health2 = CalculateStartingHealth(d2);

        int startingHealth1 = health1;
        int startingHealth2 = health2;

        bool d1WasBehind = false;
        bool d2WasBehind = false;

        log += $"{d1.dogName} Starting HP: {health1}\n";
        log += $"{d2.dogName} Starting HP: {health2}\n\n";

        for (int round = 1; round <= maxRounds; round++)
        {
            int rawDmg1 = Random.Range(
                d1.strength - damageVariance,
                d1.strength + damageVariance + 1
            ) - (d2.agility / 6);

            int rawDmg2 = Random.Range(
                d2.strength - damageVariance,
                d2.strength + damageVariance + 1
            ) - (d1.agility / 6);

            int dmg1 = CalculateFinalDamage(d1, d2, rawDmg1, round, strategy1, strategy2);
            int dmg2 = CalculateFinalDamage(d2, d1, rawDmg2, round, strategy2, strategy1);

            health2 = Mathf.Max(0, health2 - dmg1);
            health1 = Mathf.Max(0, health1 - dmg2);

            if (health1 < health2)
            {
                d1WasBehind = true;
            }

            if (health2 < health1)
            {
                d2WasBehind = true;
            }

            string roundFlavor = GetRoundFlavor(d1, d2, dmg1, dmg2, round, strategy1, strategy2);

            log += $"<b>Round {round}</b>: {roundFlavor}\n";
            log += $"{d1.dogName} impact: {dmg1} | ";
            log += $"{d2.dogName} impact: {dmg2}\n";

            log += $"{d1.dogName} HP: {health1} | ";
            log += $"{d2.dogName} HP: {health2}\n\n";

            if (health1 <= 0 || health2 <= 0)
            {
                break;
            }
        }

        ResolveFightResult(
            d1,
            d2,
            health1,
            health2,
            startingHealth1,
            startingHealth2,
            d1WasBehind,
            d2WasBehind,
            ref log
        );

        SetLog(log);

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
}
