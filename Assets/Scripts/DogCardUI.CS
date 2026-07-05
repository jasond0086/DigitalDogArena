using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DogCardUI : MonoBehaviour
{
    [Header("UI Text")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI strengthValueText;
    public TextMeshProUGUI agilityValueText;
    public TextMeshProUGUI staminaValueText;
    public TextMeshProUGUI intelligenceValueText;
    public TextMeshProUGUI winsLossesValueText;
    public TextMeshProUGUI levelValueText;
    public TextMeshProUGUI bloodlineText;

    [Header("Optional Image")]
    public Image dogImage;
    public Sprite defaultDogSprite;

    [Header("Rename UI")]
    public TMP_InputField renameInputField;
    public TextMeshProUGUI renameMessageText;

    private Dog dog;
    private DogManager dogManager;

    public void Setup(Dog dogToDisplay, DogManager manager)
    {
        dog = dogToDisplay;
        dogManager = manager;

        if (dog == null)
        {
            Debug.LogError("DogCardUI received a null dog.");
            return;
        }

        if (nameText != null)
        {
            nameText.text = dog.dogName;

        }

        ConfigureRenameUI();

        if (statsText != null)
        {
            string breedingStatus = GetBreedingStatusText();

            statsText.text =
                $"Breed: {GetDisplayBreedName(dog.breed)}\n" +
                $"Gender: {dog.gender}\n" +
                $"Age {dog.age} | {dog.GetLifeStage()}\n" +
                $"{dog.GetStatusText()} | {breedingStatus}\n" +
                $"{dog.GetPotentialTier()} | {dog.GetPotentialTitle()}\n" +
                $"Style: {dog.fightStyle}\n" +
                $"Traits: {dog.GetTraitSummary()}";
        }

        SetText(strengthValueText, $"STR {dog.strength}/{dog.strengthPotential}");
        SetText(agilityValueText, $"AGI {dog.agility}/{dog.agilityPotential}");
        SetText(staminaValueText, $"STA {dog.stamina}/{dog.staminaPotential}");
        if (intelligenceValueText != null)
        {
            SetText(intelligenceValueText, $"INT {dog.GetIntelligence()}/{dog.GetIntelligencePotential()}");
        }
        else
        {
            SetText(staminaValueText, $"STA {dog.stamina}/{dog.staminaPotential}  INT {dog.GetIntelligence()}/{dog.GetIntelligencePotential()}");
        }

        SetText(winsLossesValueText, $"Record {dog.wins}W - {dog.losses}L");
        SetText(levelValueText, $"Level {dog.level}");
        SetText(bloodlineText, GetBloodlineText());

        if (dogImage != null)
        {
            dogImage.sprite = DogPortraitLibrary.ChooseStableCardPortrait(dog, dog.dogSprite, defaultDogSprite);
            dogImage.color = Color.white;
            dogImage.enabled = dogImage.sprite != null;
        }
    }

    void ConfigureRenameUI()
    {
        EnsureRenameUI();

        if (renameInputField == null)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.enabled = false;
        }

        renameInputField.onEndEdit.RemoveListener(HandleRenameSubmitted);
        renameInputField.onSubmit.RemoveListener(HandleRenameSubmitted);
        renameInputField.onSelect.RemoveListener(HandleRenameSelected);
        renameInputField.SetTextWithoutNotify(dog.dogName);
        renameInputField.onEndEdit.AddListener(HandleRenameSubmitted);
        renameInputField.onSubmit.AddListener(HandleRenameSubmitted);
        renameInputField.onSelect.AddListener(HandleRenameSelected);

        SetRenameMessage(string.Empty);
    }

    void EnsureRenameUI()
    {
        if (renameInputField == null)
        {
            renameInputField = CreateRuntimeRenameInputField();
        }

        if (renameMessageText == null)
        {
            renameMessageText = CreateRuntimeRenameMessageText();
        }
    }

    TMP_InputField CreateRuntimeRenameInputField()
    {
        GameObject inputObject = new GameObject("DogNameRenameInput", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
        inputObject.transform.SetParent(transform, false);

        RectTransform inputRect = inputObject.GetComponent<RectTransform>();
        ConfigureRenameInputRect(inputRect);

        Image inputBackground = inputObject.GetComponent<Image>();
        inputBackground.color = new Color(1f, 1f, 1f, 0.12f);
        inputBackground.raycastTarget = true;

        TMP_InputField inputField = inputObject.GetComponent<TMP_InputField>();
        inputField.characterLimit = 18;
        inputField.lineType = TMP_InputField.LineType.SingleLine;
        inputField.targetGraphic = inputBackground;
        inputField.selectionColor = new Color(0.3f, 0.8f, 1f, 0.45f);

        RectTransform textAreaRect = CreateInputChildRect(inputObject.transform, "Text Area", Vector2.zero, Vector2.one, new Vector2(8f, 1f), new Vector2(-8f, -1f));
        textAreaRect.gameObject.AddComponent<RectMask2D>();

        TextMeshProUGUI placeholderText = CreateInputText(textAreaRect.transform, "Placeholder", "Rename");
        placeholderText.color = new Color(0f, 0f, 0f, 0.35f);

        TextMeshProUGUI inputText = CreateInputText(textAreaRect.transform, "Text", string.Empty);
        inputText.color = new Color(0.05f, 0.05f, 0.05f, 1f);

        inputField.textViewport = textAreaRect;
        inputField.placeholder = placeholderText;
        inputField.textComponent = inputText;

        return inputField;
    }

    void ConfigureRenameInputRect(RectTransform inputRect)
    {
        if (inputRect == null)
        {
            return;
        }

        if (nameText != null)
        {
            RectTransform nameRect = nameText.rectTransform;
            inputRect.anchorMin = nameRect.anchorMin;
            inputRect.anchorMax = nameRect.anchorMax;
            inputRect.pivot = nameRect.pivot;
            inputRect.anchoredPosition = nameRect.anchoredPosition;
            inputRect.sizeDelta = nameRect.sizeDelta;
            return;
        }

        inputRect.anchorMin = new Vector2(0f, 1f);
        inputRect.anchorMax = new Vector2(0f, 1f);
        inputRect.pivot = new Vector2(0.5f, 0.5f);
        inputRect.anchoredPosition = new Vector2(205f, -26f);
        inputRect.sizeDelta = new Vector2(340f, 30f);
    }

    RectTransform CreateInputChildRect(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        GameObject childObject = new GameObject(objectName, typeof(RectTransform));
        childObject.transform.SetParent(parent, false);

        RectTransform childRect = childObject.GetComponent<RectTransform>();
        childRect.anchorMin = anchorMin;
        childRect.anchorMax = anchorMax;
        childRect.offsetMin = offsetMin;
        childRect.offsetMax = offsetMax;

        return childRect;
    }

    TextMeshProUGUI CreateInputText(Transform parent, string objectName, string text)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = nameText != null ? Mathf.Max(12f, nameText.fontSize - 2f) : 16f;
        textComponent.enableWordWrapping = false;
        textComponent.raycastTarget = false;

        if (nameText != null && nameText.font != null)
        {
            textComponent.font = nameText.font;
            textComponent.fontSharedMaterial = nameText.fontSharedMaterial;
        }

        return textComponent;
    }

    TextMeshProUGUI CreateRuntimeRenameMessageText()
    {
        GameObject messageObject = new GameObject("DogNameRenameMessage", typeof(RectTransform), typeof(TextMeshProUGUI));
        messageObject.transform.SetParent(transform, false);

        RectTransform messageRect = messageObject.GetComponent<RectTransform>();

        if (renameInputField != null)
        {
            RectTransform inputRect = renameInputField.GetComponent<RectTransform>();
            messageRect.anchorMin = inputRect.anchorMin;
            messageRect.anchorMax = inputRect.anchorMax;
            messageRect.pivot = inputRect.pivot;
            messageRect.anchoredPosition = inputRect.anchoredPosition + new Vector2(0f, -22f);
            messageRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, 18f);
        }
        else
        {
            messageRect.anchorMin = new Vector2(0f, 1f);
            messageRect.anchorMax = new Vector2(0f, 1f);
            messageRect.pivot = new Vector2(0.5f, 0.5f);
            messageRect.anchoredPosition = new Vector2(205f, -48f);
            messageRect.sizeDelta = new Vector2(340f, 18f);
        }

        TextMeshProUGUI messageText = messageObject.GetComponent<TextMeshProUGUI>();
        messageText.text = string.Empty;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontSize = 10f;
        messageText.color = new Color(0.55f, 0.05f, 0.05f, 1f);
        messageText.enableWordWrapping = false;
        messageText.raycastTarget = false;

        if (nameText != null && nameText.font != null)
        {
            messageText.font = nameText.font;
            messageText.fontSharedMaterial = nameText.fontSharedMaterial;
        }

        return messageText;
    }

    void HandleRenameSelected(string value)
    {
        SetRenameMessage(string.Empty);
    }

    void HandleRenameSubmitted(string requestedName)
    {
        if (dog == null || dogManager == null)
        {
            ResetRenameInputText();
            return;
        }

        if (dogManager.TryRenameDog(dog, requestedName, out string cleanedName, out string message))
        {
            if (renameInputField != null)
            {
                renameInputField.SetTextWithoutNotify(cleanedName);
            }

            SetRenameMessage(string.Empty);
            return;
        }

        ResetRenameInputText();
        SetRenameMessage(string.IsNullOrEmpty(message) ? DogNameValidator.RealDogNameMessage : message);
    }

    void ResetRenameInputText()
    {
        if (renameInputField != null && dog != null)
        {
            renameInputField.SetTextWithoutNotify(dog.dogName);
        }
    }

    void SetRenameMessage(string message)
    {
        if (renameMessageText != null)
        {
            renameMessageText.text = message;
        }
    }

    string GetBreedingStatusText()
    {
        if (dog == null || dogManager == null)
        {
            return "Last Bred Week: Unknown";
        }

        if (dog.CanBreedInWeek(dogManager.currentWeek))
        {
            return "Can Breed This Week";
        }

        return $"Already Bred This Week | Last Bred Week: {dog.lastBredWeek}";
    }

    void SetText(TextMeshProUGUI textField, string value)
    {
        if (textField != null)
        {
            textField.text = value;
        }
    }

    string GetBloodlineText()
    {
        bool hasBloodline = !string.IsNullOrWhiteSpace(dog.bloodlineName);
        bool hasAncestorSummary = !string.IsNullOrWhiteSpace(dog.ancestorBonusSummary);

        if (!hasBloodline && !dog.isBloodlineCarrier)
        {
            return "Bloodline: None";
        }

        string bloodlineName = hasBloodline ? dog.bloodlineName : "Unknown";
        string text = $"Bloodline: {bloodlineName}";

        if (hasAncestorSummary)
        {
            text += $"\nAncestor: {dog.ancestorBonusSummary}";
        }

        if (dog.isBloodlineCarrier)
        {
            text += "\nCarrier";
        }

        return text;
    }

    string GetDisplayBreedName(string breedName)
    {
        string compactBreed = GetCompactBreedName(breedName);

        switch (compactBreed)
        {
            case "germanbull":
                return "German Bull Hybrid";

            case "germanbully":
                return "German Bully Hybrid";

            case "shepherdbull":
                return "Shepherd Bull Hybrid";

            case "shepherdbully":
                return "Shepherd Bully Hybrid";

            case "pitgerman":
                return "Pit German Hybrid";

            case "pitshepherd":
                return "Pit Shepherd Hybrid";

            case "bullshepherd":
                return "Bull Shepherd Hybrid";

            case "bullyshepherd":
                return "Bully Shepherd Hybrid";

            default:
                return string.IsNullOrWhiteSpace(breedName) ? "Unknown" : breedName.Trim();
        }
    }

    string GetCompactBreedName(string breedName)
    {
        if (string.IsNullOrWhiteSpace(breedName))
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(breedName.Length);

        for (int i = 0; i < breedName.Length; i++)
        {
            char breedCharacter = char.ToLowerInvariant(breedName[i]);

            if (char.IsWhiteSpace(breedCharacter) ||
                breedCharacter == '_' ||
                breedCharacter == '-' ||
                breedCharacter == '/' ||
                breedCharacter == '\\' ||
                breedCharacter == '\'')
            {
                continue;
            }

            builder.Append(breedCharacter);
        }

        return builder.ToString();
    }
}
