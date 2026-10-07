using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int score;
    public float comboTime = 3;
    public TMP_Text scoreText;
    public TMP_Text comboText;
    public Slider comboBar;
    int combo;
    float timeLeft;

    void Start()
    {
        if (comboBar != null)
        {
            comboBar.minValue = 0;
            comboBar.maxValue = 1;
            comboBar.value = 0;
            comboBar.interactable = false;
        }

        ShowScore();
    }

    void Update()
    {
        if (combo == 0) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0)
        {
            timeLeft = 0;
            combo = 0;
            ShowScore();
        }

        ShowBar();
    }

    public void AddScore()
    {
        combo++;
        timeLeft = comboTime;

        score += 1;
        score += combo - 1;

        ShowScore();
        ShowBar();
    }

    void ShowBar()
    {
        if (comboBar != null)
            comboBar.value = comboTime > 0 ? timeLeft / comboTime : 0;
    }

    void ShowScore()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (comboText != null)
            comboText.text = combo > 1 ? "Combo: " + combo : "";
    }
}