using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public ScoreManager scoreManager;
    public TMP_Text finalScoreText;
    public TMP_InputField nameInput;
    public GameObject leaderboardPanel;

    bool submitted;

    void OnEnable()
    {
        submitted = false;

        if (scoreManager != null && finalScoreText != null)
            finalScoreText.text = "Final Score: " + scoreManager.score;
    }

    public void Submit()
    {
        if (submitted || scoreManager == null || nameInput == null)
            return;

        string playerName = nameInput.text.Trim();
        if (playerName == "") return;

        submitted = true;

        Leaderboard.AddToLeaderboard(playerName, scoreManager.score);

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);

        gameObject.SetActive(false);
    }
}