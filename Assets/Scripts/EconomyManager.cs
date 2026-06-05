using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    private const string EconomySaveKey = "ECONOMY_STATE_SAVE";

    [Header("Economy")]
    public int credits = 500;

    [Header("Optional UI")]
    public TextMeshProUGUI creditsText;

    void Awake()
    {
        LoadEconomy();
        RefreshCreditsUI();
    }

    void Start()
    {
        RefreshCreditsUI();
    }

    public void AddCredits(int amount, string reason = "")
    {
        credits += amount;
        SaveEconomy();
        RefreshCreditsUI();

        if (!string.IsNullOrEmpty(reason))
        {
            Debug.Log($"Credits +{amount}: {reason}. Balance: {credits}");
        }
    }

    public bool CanAfford(int amount)
    {
        return credits >= amount;
    }

    public bool SpendCredits(int amount, string reason = "")
    {
        if (!CanAfford(amount))
        {
            return false;
        }

        credits -= amount;
        SaveEconomy();
        RefreshCreditsUI();

        if (!string.IsNullOrEmpty(reason))
        {
            Debug.Log($"Credits -{amount}: {reason}. Balance: {credits}");
        }

        return true;
    }

    public int CalculateWeeklyUpkeep(List<Dog> dogs)
    {
        int activeDogCount = 0;

        if (dogs != null)
        {
            foreach (Dog dog in dogs)
            {
                if (dog != null && !dog.isDead && !dog.isRetired)
                {
                    activeDogCount++;
                }
            }
        }

        return 50 + (activeDogCount * 10);
    }

    public bool PayWeeklyUpkeep(List<Dog> dogs)
    {
        int upkeepCost = CalculateWeeklyUpkeep(dogs);

        if (SpendCredits(upkeepCost, "Weekly kennel upkeep"))
        {
            Debug.Log($"Paid {upkeepCost} credits in weekly upkeep.");
            return true;
        }

        credits = 0;
        RefreshCreditsUI();
        SaveEconomy();
        Debug.Log("Could not afford full upkeep. Credits dropped to 0.");

        return false;
    }

    public void SaveEconomy()
    {
        PlayerPrefs.SetInt(EconomySaveKey, credits);
        PlayerPrefs.Save();
    }

    public void LoadEconomy()
    {
        credits = PlayerPrefs.GetInt(EconomySaveKey, credits);
    }

    public void RefreshCreditsUI()
    {
        if (creditsText != null)
        {
            creditsText.text = GetCreditsSummaryText();
        }
    }

    public string GetCreditsSummaryText()
    {
        return $"Credits: {credits}";
    }
}
