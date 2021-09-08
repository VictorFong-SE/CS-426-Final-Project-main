using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    [SerializeField]
    GameObject buttons, controlsScreen, storyScreen, creditsScreen, loadingScreen, storyContinueText, storyExitText, tutorialScreen;

    bool inOther, toLoad, loading, inTutorial;

    int tutorialPage;
    [SerializeField]
    GameObject[] tutorialPages;

    // Start is called before the first frame update
    void Start()
    {
        buttons.SetActive(true);
        controlsScreen.SetActive(false);
        storyScreen.SetActive(false);
        creditsScreen.SetActive(false);
        tutorialPages[0].SetActive(true);
        for (var i = 1; i < tutorialPages.Length; i++)
        {
            tutorialPages[i].SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (inTutorial)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (tutorialPage == 0)
                {
                    inTutorial = false;
                    buttons.SetActive(true);
                    tutorialScreen.SetActive(false);
                }
                else
                {
                    tutorialPages[tutorialPage].SetActive(false);
                    tutorialPage--;
                    tutorialPages[tutorialPage].SetActive(true);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (tutorialPage < tutorialPages.Length - 1)
                {
                    tutorialPages[tutorialPage].SetActive(false);
                    tutorialPage++;
                    tutorialPages[tutorialPage].SetActive(true);
                }
            }
        }
        else if (!loading && Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (inOther)
            {
                inOther = false;
                buttons.SetActive(true);
                controlsScreen.SetActive(false);
                storyScreen.SetActive(false);
                creditsScreen.SetActive(false);
            }
            else if (toLoad)
            {
                loading = true;
                loadingScreen.SetActive(true);
                SceneManager.LoadSceneAsync("Level");
            }
        }
    }

    public void ButtonStartGame()
    {
        toLoad = true;
        buttons.SetActive(false);
        storyScreen.SetActive(true);
        storyContinueText.SetActive(true);
        storyExitText.SetActive(false);
    }

    public void ButtonControls()
    {
        inOther = true;
        buttons.SetActive(false);
        controlsScreen.SetActive(true);
    }

    public void ButtonTutorial()
    {
        inTutorial = true;
        buttons.SetActive(false);
        tutorialScreen.SetActive(true);
        tutorialPage = 0;
    }

    public void ButtonStory()
    {
        inOther = true;
        buttons.SetActive(false);
        storyScreen.SetActive(true);
    }

    public void ButtonCredits()
    {
        inOther = true;
        buttons.SetActive(false);
        creditsScreen.SetActive(true);
        FindObjectOfType<Credits>().Reset();
    }

    public void ButtonQuit()
    {
        Application.Quit();
    }
}
