using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NarratorManager : MonoBehaviour
{
    private const string NarratorLogSaveKey = "NARRATOR_LOG_SAVE";

    private enum NarratorTab
    {
        Feed,
        Details
    }

    [Header("Narrator UI")]
    public TextMeshProUGUI narratorText;
    public TextMeshProUGUI narratorLogText;
    public ScrollRect narratorScrollRect;

    [Header("Narrator Tabs")]
    public GameObject feedPanel;
    public GameObject detailsPanel;
    public TextMeshProUGUI feedText;
    public TextMeshProUGUI detailsText;
    public ScrollRect detailsScrollRect;

    [Header("Narrator Settings")]
    public int maxLogEntries = 100;

    [Header("Current Message")]
    public string currentMessage;
    public string currentHeadline;
    public string currentDetails;

    private List<string> narrationLog = new List<string>();
    private NarratorTab currentNarratorTab = NarratorTab.Feed;

    [System.Serializable]
    private class NarratorLogSaveData
    {
        public List<string> entries = new List<string>();
    }

    void Awake()
    {
        ConfigureScrollableFeed();
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
        currentHeadline = GetVisibleNarratorMessage(currentMessage);
        currentDetails = currentMessage;
        RefreshNarratorLogUI();
    }

    public void SetNarration(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        currentMessage = message;
        currentHeadline = GetVisibleNarratorMessage(message);
        currentDetails = message;
        narrationLog.Add(currentMessage);
        TrimNarratorLog();
        RefreshNarratorLogUI();
        SaveNarratorLog();
    }

    public void SetNarration(string headline, string details)
    {
        if (string.IsNullOrEmpty(headline) && string.IsNullOrEmpty(details))
        {
            return;
        }

        currentHeadline = GetVisibleNarratorMessage(string.IsNullOrEmpty(headline) ? details : headline);
        currentDetails = string.IsNullOrEmpty(details) ? headline : details;
        currentMessage = currentDetails;
        narrationLog.Add(currentHeadline);
        TrimNarratorLog();
        RefreshNarratorLogUI();
        SaveNarratorLog();
    }

    public void ClearNarration()
    {
        currentMessage = "";
        currentHeadline = "";
        currentDetails = "";

        if (narratorText != null)
        {
            narratorText.text = "";
        }

        if (narratorLogText != null)
        {
            narratorLogText.text = "";
        }

        if (feedText != null)
        {
            feedText.text = "";
        }

        if (detailsText != null)
        {
            detailsText.text = "";
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

        if (string.IsNullOrEmpty(currentMessage) && narrationLog.Count > 0)
        {
            currentMessage = narrationLog[narrationLog.Count - 1];
        }

        if (string.IsNullOrEmpty(currentDetails))
        {
            currentDetails = currentMessage;
        }

        if (string.IsNullOrEmpty(currentHeadline))
        {
            currentHeadline = GetVisibleNarratorMessage(currentMessage);
        }
    }

    public void RefreshNarratorLogUI()
    {
        if (currentNarratorTab == NarratorTab.Details)
        {
            UpdateDetailsText();
            return;
        }

        UpdateFeedText();
    }

    public void ShowFeedTab()
    {
        currentNarratorTab = NarratorTab.Feed;
        SetPanelActive(feedPanel, true);
        SetPanelActive(detailsPanel, false);
        RefreshNarratorLogUI();
    }

    public void ShowDetailsTab()
    {
        currentNarratorTab = NarratorTab.Details;
        SetPanelActive(feedPanel, false);
        SetPanelActive(detailsPanel, true);
        RefreshNarratorLogUI();
    }

    public void ButtonShowFeedTab()
    {
        ShowFeedTab();
    }

    public void ButtonShowDetailsTab()
    {
        ShowDetailsTab();
    }

    public void ClearNarratorLog()
    {
        narrationLog.Clear();
        currentMessage = "";
        currentHeadline = "";
        currentDetails = "";
        PlayerPrefs.DeleteKey(NarratorLogSaveKey);
        PlayerPrefs.Save();
        RefreshNarratorLogUI();
        ShowDefaultMessage();
    }

    void TrimNarratorLog()
    {
        int entryLimit = Mathf.Max(1000, maxLogEntries);

        while (narrationLog.Count > entryLimit)
        {
            narrationLog.RemoveAt(0);
        }
    }

    private string GetVisibleNarratorMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "";
        }

        string cleaned = message.Replace("\r", " ").Replace("\n", " ").Trim();

        const int maxLength = 110;

        if (cleaned.Length > maxLength)
        {
            cleaned = cleaned.Substring(0, maxLength) + "...";
        }

        return cleaned;
    }

    void UpdateFeedText()
    {
        string fullFeed = GetFullFeedText();

        if (feedText != null)
        {
            feedText.text = fullFeed;
        }

        if (narratorText != null && narratorText != feedText)
        {
            narratorText.text = fullFeed;
        }

        if (narratorLogText != null && narratorLogText != feedText && narratorLogText != narratorText)
        {
            narratorLogText.text = fullFeed;
        }

        Canvas.ForceUpdateCanvases();

        if (narratorScrollRect != null)
        {
            narratorScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    string GetFullFeedText()
    {
        StringBuilder feedBuilder = new StringBuilder();

        foreach (string entry in narrationLog)
        {
            string visibleEntry = GetVisibleNarratorMessage(entry);

            if (string.IsNullOrEmpty(visibleEntry))
            {
                continue;
            }

            if (feedBuilder.Length > 0)
            {
                feedBuilder.AppendLine();
                feedBuilder.AppendLine();
            }

            feedBuilder.Append(visibleEntry);
        }

        return feedBuilder.ToString();
    }

    void ConfigureScrollableFeed()
    {
        if (feedPanel == null || feedText == null)
        {
            return;
        }

        narratorScrollRect = feedPanel.GetComponent<ScrollRect>();

        if (narratorScrollRect == null)
        {
            narratorScrollRect = feedPanel.AddComponent<ScrollRect>();
        }

        RectTransform feedPanelRect = feedPanel.GetComponent<RectTransform>();
        RectTransform feedTextRect = feedText.rectTransform;

        narratorScrollRect.content = feedTextRect;
        narratorScrollRect.viewport = feedPanelRect;
        narratorScrollRect.horizontal = false;
        narratorScrollRect.vertical = true;
        narratorScrollRect.movementType = ScrollRect.MovementType.Clamped;
        narratorScrollRect.scrollSensitivity = 20f;

        if (feedPanel.GetComponent<RectMask2D>() == null)
        {
            feedPanel.AddComponent<RectMask2D>();
        }

        ContentSizeFitter sizeFitter = feedText.GetComponent<ContentSizeFitter>();

        if (sizeFitter == null)
        {
            sizeFitter = feedText.gameObject.AddComponent<ContentSizeFitter>();
        }

        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        feedTextRect.anchorMin = new Vector2(0f, 1f);
        feedTextRect.anchorMax = new Vector2(1f, 1f);
        feedTextRect.pivot = new Vector2(0.5f, 1f);
        feedTextRect.anchoredPosition = Vector2.zero;
        feedTextRect.sizeDelta = Vector2.zero;
        feedText.alignment = TextAlignmentOptions.TopLeft;
    }

    void UpdateDetailsText()
    {
        if (detailsText != null)
        {
            detailsText.text = currentDetails;
        }

        Canvas.ForceUpdateCanvases();

        if (detailsScrollRect != null)
        {
            detailsScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }
}
