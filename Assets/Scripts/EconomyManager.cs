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
