using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction selectAction;
    public Button[] buttons;
    public GameObject[] selectArrows;
    private int currentHover = 1;
    private Vector2 previousInputDir;

    public GameObject howToMenu, leaderboardMenu, creditsMenu;

    public Transform[] leaderboardEntries;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        selectAction = InputSystem.actions.FindAction("Jump");

        previousInputDir = Vector2.zero;
        currentHover = 1;

        UpdateHover();
        HowToButton();
    }

    void Update()
    {
        // movement input
        // x = left, right
        // y = up, down
        Vector2 inputDir = moveAction.ReadValue<Vector2>();

        if(previousInputDir.y == 0f)
        {
            if(inputDir.y > 0f)
            {
                // move up
                currentHover = Mathf.Clamp(currentHover - 1, 0, buttons.Length - 1);
                UpdateHover();
            }

            if(inputDir.y < 0f)
            {
                // move down
                currentHover = Mathf.Clamp(currentHover + 1, 0, buttons.Length - 1);
                UpdateHover();
            }
        }

        previousInputDir = inputDir;

        // selection input
        if (selectAction.WasPressedThisFrame())
        {
            Select();
        }
    }

    private void UpdateHover()
    {
        foreach(GameObject arrow in selectArrows)
        {
            arrow.SetActive(false);
        }

        selectArrows[currentHover].SetActive(true);
    }

    private void Select()
    {
        switch (currentHover)
        {
            case 0:
                PlayButton(); break;
            case 1:
                HowToButton(); break;
            case 2:
                LeaderboardButton(); break;
            case 3:
                CreditsButton(); break;
            case 4:
                QuitButton(); break;
            default:
                Debug.Log("Tried to select a button that doesn't exist."); break;
        }
    }

    public void PlayButton()
    {
        Debug.Log("PLAY");
        SceneManager.LoadScene(1);
    }

    public void HowToButton()
    {
        Debug.Log("HOW TO");
        howToMenu.SetActive(true);

        leaderboardMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

    public void LeaderboardButton()
    {
        Debug.Log("LEADERBOARD");
        leaderboardMenu.SetActive(true);

        howToMenu.SetActive(false);
        creditsMenu.SetActive(false);

        // set up leaderboard
        SetupLeaderboard();
    }

    public void CreditsButton()
    {
        Debug.Log("CREDITS");
        creditsMenu.SetActive(true);

        leaderboardMenu.SetActive(false);
        howToMenu.SetActive(false);
    }

    public void QuitButton()
    {
        Debug.Log("QUIT");
        Application.Quit();
    }

    private void SetupLeaderboard()
    {
        List<LeaderEntry> topSix = Leaderboard.GetTopSixEntries();
        for(int i = 0; i < 6; i++)
        {
            TMP_Text name = leaderboardEntries[i].GetChild(1).GetComponent<TMP_Text>();
            TMP_Text score = leaderboardEntries[i].GetChild(2).GetComponent<TMP_Text>();
            if (topSix.Count > i)
            {
                name.text = topSix[i].name;
                score.text = topSix[i].score.ToString();
            }
            else
            {
                name.text = "---";
                score.text = "---";
            }
        }
    }
}
