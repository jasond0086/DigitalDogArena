using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DogDetailPanelUI : MonoBehaviour
{
    Image portraitImage;
    TextMeshProUGUI titleText;
    TextMeshProUGUI mainInfoText;
    TextMeshProUGUI statsText;
    TextMeshProUGUI familyHeaderText;
    TMP_Text familyTreeText;
    Button closeButton;

    Dog currentDog;
    DogManager dogManager;
    bool uiBuilt;

    void Awake()
    {
        BuildRuntimeUI();
    }

    public void Show(Dog dog, DogManager manager)
    {
        if (dog == null)
        {
            Close();
            return;
        }

        currentDog = dog;
        dogManager = manager;

        BuildRuntimeUI();
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        Refresh();
    }

    public void RefreshIfShowing(Dog dog)
    {
        if (dog == null || currentDog != dog || !gameObject.activeSelf)
        {
            return;
        }

        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void BuildRuntimeUI()
    {
        if (uiBuilt)
        {
            return;
        }

        uiBuilt = true;

        RectTransform rootRect = GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image overlay = gameObject.GetComponent<Image>();
        if (overlay == null)
        {
            overlay = gameObject.AddComponent<Image>();
        }

        overlay.color = new Color(0f, 0f, 0f, 0.72f);
        overlay.raycastTarget = true;

        GameObject panel = CreateUIObject("DogDetailCard", transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.055f, 0.06f, 0.07f, 0.96f);
        panelImage.raycastTarget = true;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(920f, 650f);

        titleText = CreateText(panel.transform, "DogDetailTitle", 28f, TextAlignmentOptions.Left);
        ConfigureRect(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(28f, -62f), new Vector2(-88f, -18f));

        closeButton = CreateCloseButton(panel.transform);

        portraitImage = CreateImage(panel.transform, "DogDetailPortrait");
        ConfigureRect(portraitImage.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -312f), new Vector2(294f, -92f));

        mainInfoText = CreateText(panel.transform, "DogDetailMainInfo", 17f, TextAlignmentOptions.TopLeft);
        ConfigureRect(mainInfoText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(320f, -314f), new Vector2(-32f, -92f));

        statsText = CreateText(panel.transform, "DogDetailStats", 16f, TextAlignmentOptions.TopLeft);
        ConfigureRect(statsText.rectTransform, new Vector2(0f, 0f), new Vector2(0.48f, 1f), new Vector2(34f, 32f), new Vector2(-20f, -338f));

        familyHeaderText = CreateText(panel.transform, "DogDetailFamilyTreeHeader", 18f, TextAlignmentOptions.Left);
        familyHeaderText.text = "Family Tree";
        ConfigureRect(familyHeaderText.rectTransform, new Vector2(0.48f, 0f), new Vector2(1f, 1f), new Vector2(20f, 282f), new Vector2(-32f, -338f));

        familyTreeText = CreateText(panel.transform, "DogDetailFamilyTreeBody", 14f, TextAlignmentOptions.TopLeft);
        familyTreeText.overflowMode = TextOverflowModes.Overflow;
        ConfigureRect(familyTreeText.rectTransform, new Vector2(0.48f, 0f), new Vector2(1f, 1f), new Vector2(20f, 32f), new Vector2(-32f, -374f));
    }

    Button CreateCloseButton(Transform parent)
    {
        GameObject buttonObject = CreateUIObject("DogDetailCloseButton", parent);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.22f, 0.07f, 0.07f, 0.95f);
        buttonImage.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(Close);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        ConfigureRect(buttonRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-78f, -60f), new Vector2(-24f, -18f));

        TextMeshProUGUI buttonText = CreateText(buttonObject.transform, "Text", 22f, TextAlignmentOptions.Center);
        buttonText.text = "X";
        buttonText.color = Color.white;
        ConfigureRect(buttonText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        return button;
    }

    GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    TextMeshProUGUI CreateText(Transform parent, string objectName, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = new Color(0.92f, 0.94f, 0.95f, 1f);
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        return text;
    }

    Image CreateImage(Transform parent, string objectName)
    {
        GameObject imageObject = CreateUIObject(objectName, parent);
        Image image = imageObject.AddComponent<Image>();
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    void ConfigureRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    void Refresh()
    {
        if (currentDog == null)
        {
            Close();
            return;
        }

        titleText.text = currentDog.dogName;
        mainInfoText.text = BuildMainInfoText();
        statsText.text = BuildStatsText();
        familyHeaderText.text = "Family Tree";
        familyTreeText.text = BuildFamilyTreeBodyText();
        RefreshPortrait();
    }

    void RefreshPortrait()
    {
        if (portraitImage == null)
        {
            return;
        }

        Sprite portrait = DogPortraitLibrary.ChooseStableCardPortrait(currentDog, currentDog.dogSprite, null);
        portraitImage.sprite = portrait;
        portraitImage.enabled = portrait != null;
    }

    string BuildMainInfoText()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Sex: {currentDog.gender}");
        builder.AppendLine($"Breed: {GetDisplayBreedName(currentDog.breed)}");
        builder.AppendLine($"Generation: {GetGenerationText(currentDog)}");
        builder.AppendLine($"Age: {currentDog.age} ({currentDog.GetLifeStage()})");
        builder.AppendLine($"Status: {currentDog.GetStatusText()}");
        builder.AppendLine($"Record: {currentDog.wins}W - {currentDog.losses}L ({currentDog.totalFights} fights)");
        builder.AppendLine($"Fight Style: {currentDog.fightStyle}");
        builder.AppendLine($"Strategy: {GetStrategyText()}");
        builder.AppendLine($"Breeding: {GetBreedingStatusText()}");
        return builder.ToString();
    }

    string BuildStatsText()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Stats");
        builder.AppendLine($"STR: {currentDog.strength} / {currentDog.strengthPotential}");
        builder.AppendLine($"AGI: {currentDog.agility} / {currentDog.agilityPotential}");
        builder.AppendLine($"STA: {currentDog.stamina} / {currentDog.staminaPotential}");
        builder.AppendLine($"INT: {currentDog.GetIntelligence()} / {currentDog.GetIntelligencePotential()}");
        builder.AppendLine();
        builder.AppendLine($"Potential: {currentDog.GetPotentialTier()} - {currentDog.GetPotentialTitle()}");
        builder.AppendLine($"Traits: {currentDog.GetTraitSummary()}");

        if (!string.IsNullOrWhiteSpace(currentDog.bloodlineName) ||
            !string.IsNullOrWhiteSpace(currentDog.ancestorBonusSummary) ||
            currentDog.isBloodlineCarrier)
        {
            builder.AppendLine();
            builder.AppendLine($"Bloodline: {GetSafeText(currentDog.bloodlineName, "Unknown")}");

            if (!string.IsNullOrWhiteSpace(currentDog.ancestorBonusSummary))
            {
                builder.AppendLine($"Ancestor: {currentDog.ancestorBonusSummary}");
            }

            if (currentDog.isBloodlineCarrier)
            {
                builder.AppendLine("Dormant Carrier: Yes");
            }
        }

        return builder.ToString();
    }

    string BuildFamilyTreeBodyText()
    {
        StringBuilder builder = new StringBuilder();

        if (IsFoundationDog(currentDog))
        {
            builder.AppendLine("Generation: Foundation");
            builder.AppendLine("Parents: Unknown");
            return builder.ToString();
        }

        builder.AppendLine($"Generation: {GetGenerationText(currentDog)}");
        builder.AppendLine(BuildParentLine("Parent A", GetParentAId(currentDog), currentDog.parentAName, currentDog.parentABreed, currentDog.parentASex));
        builder.AppendLine(BuildParentLine("Parent B", GetParentBId(currentDog), currentDog.parentBName, currentDog.parentBBreed, currentDog.parentBSex));
        builder.AppendLine($"Lineage: {GetParentBreedForLineage(GetParentAId(currentDog), currentDog.parentABreed)} x {GetParentBreedForLineage(GetParentBId(currentDog), currentDog.parentBBreed)}");

        string grandparents = BuildGrandparentText();

        if (!string.IsNullOrWhiteSpace(grandparents))
        {
            builder.AppendLine();
            builder.Append(grandparents);
        }

        return builder.ToString();
    }

    string BuildParentLine(string label, string parentId, string storedName, string storedBreed, string storedSex)
    {
        Dog parent = dogManager != null ? dogManager.FindOwnedDogById(parentId) : null;
        string name = parent != null ? parent.dogName : storedName;
        string breed = parent != null ? parent.breed : storedBreed;
        string sex = parent != null ? parent.gender.ToString() : storedSex;
        string role = GetParentRole(sex, label);
        string generation = parent != null ? $" - {GetShortGenerationText(parent)}" : "";

        return $"{role}: {GetSafeText(name, "Unknown")} - {GetSafeText(sex, "Unknown")} - {GetDisplayBreedName(breed)}{generation}";
    }

    string GetParentBreedForLineage(string parentId, string storedBreed)
    {
        Dog parent = dogManager != null ? dogManager.FindOwnedDogById(parentId) : null;
        string breed = parent != null ? parent.breed : storedBreed;
        return GetDisplayBreedName(breed);
    }

    string GetParentRole(string sex, string fallbackLabel)
    {
        if (string.Equals(sex, DogGender.Male.ToString(), System.StringComparison.OrdinalIgnoreCase))
        {
            return "Sire";
        }

        if (string.Equals(sex, DogGender.Female.ToString(), System.StringComparison.OrdinalIgnoreCase))
        {
            return "Dam";
        }

        return fallbackLabel;
    }

    string BuildGrandparentText()
    {
        StringBuilder builder = new StringBuilder();
        AppendGrandparentLine(builder, "Sire side", dogManager != null ? dogManager.FindOwnedDogById(GetParentAId(currentDog)) : null);
        AppendGrandparentLine(builder, "Dam side", dogManager != null ? dogManager.FindOwnedDogById(GetParentBId(currentDog)) : null);
        return builder.ToString();
    }

    void AppendGrandparentLine(StringBuilder builder, string label, Dog parent)
    {
        if (builder == null || parent == null || IsFoundationDog(parent))
        {
            return;
        }

        string parentA = GetSafeText(parent.parentAName, "Unknown");
        string parentB = GetSafeText(parent.parentBName, "Unknown");
        builder.AppendLine($"{label}: {parentA} / {parentB}");
    }

    bool IsFoundationDog(Dog dog)
    {
        if (dog == null)
        {
            return true;
        }

        bool hasParentIds = !string.IsNullOrWhiteSpace(GetParentAId(dog)) || !string.IsNullOrWhiteSpace(GetParentBId(dog));
        bool hasParentSnapshots = !string.IsNullOrWhiteSpace(dog.parentAName) || !string.IsNullOrWhiteSpace(dog.parentBName);
        return !hasParentIds && !hasParentSnapshots;
    }

    string GetParentAId(Dog dog)
    {
        if (dog == null)
        {
            return string.Empty;
        }

        return !string.IsNullOrWhiteSpace(dog.parentAId) ? dog.parentAId : dog.parent1Id;
    }

    string GetParentBId(Dog dog)
    {
        if (dog == null)
        {
            return string.Empty;
        }

        return !string.IsNullOrWhiteSpace(dog.parentBId) ? dog.parentBId : dog.parent2Id;
    }

    string GetGenerationText(Dog dog)
    {
        if (dog == null || dog.generation <= 0)
        {
            return "Foundation (G1)";
        }

        return $"G{dog.generation + 1}";
    }

    string GetShortGenerationText(Dog dog)
    {
        if (dog == null || dog.generation <= 0)
        {
            return "G1";
        }

        return $"G{dog.generation + 1}";
    }

    string GetStrategyText()
    {
        return dogManager != null
            ? dogManager.GetCurrentStrategyTextForDog(currentDog)
            : "Unknown";
    }

    string GetBreedingStatusText()
    {
        if (dogManager == null)
        {
            return "Unknown";
        }

        if (currentDog.CanBreedInWeek(dogManager.currentWeek))
        {
            return "Can breed this week";
        }

        return $"Cooldown active (last bred week {currentDog.lastBredWeek})";
    }

    string GetSafeText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
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

        StringBuilder builder = new StringBuilder(breedName.Length);

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
