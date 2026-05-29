using UnityEngine;

public class PageManager : MonoBehaviour
{
    [Header("Pages")]
    public GameObject stablePage;
    public GameObject fightPage;
    public GameObject breedPage;
    public GameObject leaguePage;

    void Start()
    {
        ShowStablePage();
    }

    public void ShowStablePage()
    {
        SetPage(stablePage);
    }

    public void ShowFightPage()
    {
        SetPage(fightPage);
    }

    public void ShowBreedPage()
    {
        SetPage(breedPage);
    }

    public void ShowLeaguePage()
    {
        SetPage(leaguePage);
    }

    void SetPage(GameObject pageToShow)
    {
        if (stablePage != null)
        {
            stablePage.SetActive(pageToShow == stablePage);
        }

        if (fightPage != null)
        {
            fightPage.SetActive(pageToShow == fightPage);
        }

        if (breedPage != null)
        {
            breedPage.SetActive(pageToShow == breedPage);
        }

        if (leaguePage != null)
        {
            leaguePage.SetActive(pageToShow == leaguePage);
        }
    }
}
