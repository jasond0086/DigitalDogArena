using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NarratorManager : MonoBehaviour
{
    private const string NarratorLogSaveKey = "NARRATOR_LOG_SAVE";

    [Header("Narrator UI")]
    public TextMeshProUGUI narratorText;
    public TextMeshProUGUI narratorLogText;
    public ScrollRect narratorScrollRect;

    [Header("Narrator Settings")]
    public int maxLogEntries = 100;

    [Header("Current Message")]
    public string currentMessage;

    private List<string> narrationLog = new List<string>();

    [System.Serializable]
    private class NarratorLogSaveData
    {
        public List<string> entries = new List<string>();
    }

    void Awake()
    {
        LoadNarratorLog();
    }

    void Start()
    {
        if (narrationLog.Count == 0)
        {
            ShowDefaultMessage();
            return;
        }

        currentMessage = narrationLog[narrationLog.Count - 1];
        RefreshNarratorLogUI();
    }

    public void SetNarration(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        currentMessage = message;
        narrationLog.Add(currentMessage);
        TrimNarratorLog();
        RefreshNarratorLogUI();
        SaveNarratorLog();

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

    public void SaveNarratorLog()
    {
        NarratorLogSaveData saveData = new NarratorLogSaveData
        {
            entries = new List<string>(narrationLog)
        };

        string json = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(NarratorLogSaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadNarratorLog()
    {
        narrationLog.Clear();

        if (!PlayerPrefs.HasKey(NarratorLogSaveKey))
        {
            return;
        }

        string json = PlayerPrefs.GetString(NarratorLogSaveKey);
        NarratorLogSaveData saveData = JsonUtility.FromJson<NarratorLogSaveData>(json);

        if (saveData == null || saveData.entries == null)
        {
            return;
        }

        narrationLog = new List<string>(saveData.entries);
        TrimNarratorLog();
    }

    public void RefreshNarratorLogUI()
    {
        if (narratorText != null)
        {
            narratorText.text = currentMessage;
        }

        if (narratorLogText != null)
        {
            narratorLogText.text = string.Join("\n", narrationLog.ToArray());
        }

        ScrollToBottom();
    }

    public void ClearNarratorLog()
    {
        narrationLog.Clear();
        currentMessage = "";
        PlayerPrefs.DeleteKey(NarratorLogSaveKey);
        PlayerPrefs.Save();
        RefreshNarratorLogUI();
        ShowDefaultMessage();
    }

    void TrimNarratorLog()
    {
        int entryLimit = Mathf.Max(1, maxLogEntries);

        while (narrationLog.Count > entryLimit)
        {
            narrationLog.RemoveAt(0);
        }
    }

    void ScrollToBottom()
    {
        if (narratorScrollRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        narratorScrollRect.verticalNormalizedPosition = 0f;
    }
}
