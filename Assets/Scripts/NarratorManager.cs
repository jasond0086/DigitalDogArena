using TMPro;
using UnityEngine;

public class NarratorManager : MonoBehaviour
{
    [Header("Narrator UI")]
    public TextMeshProUGUI narratorText;

    [Header("Current Message")]
    public string currentMessage;

    void Start()
    {
        if (string.IsNullOrEmpty(currentMessage))
        {
            ShowDefaultMessage();
        }
        else if (narratorText != null)
        {
            narratorText.text = currentMessage;
        }
    }

    public void SetNarration(string message)
    {
        currentMessage = message;

        if (narratorText != null)
        {
            narratorText.text = currentMessage;
        }

        Debug.Log($"Narrator: {currentMessage}");
    }

    public void ClearNarration()
    {
        currentMessage = "";

        if (narratorText != null)
        {
            narratorText.text = "";
        }
    }

    public void ShowDefaultMessage()
    {
        SetNarration("Welcome back to the kennel. Choose your next move.");
    }
}
