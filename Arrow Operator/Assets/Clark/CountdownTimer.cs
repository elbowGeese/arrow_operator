using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public float time = 60;
    public TMP_Text timerText;
    public GameObject timesUpPanel;

    bool running = true;

    void Start()
    {
        Time.timeScale = 1;

        if (timesUpPanel != null)
            timesUpPanel.SetActive(false);

        ShowTime();
    }

    void Update()
    {
        if (!running) return;

        time -= Time.deltaTime;

        if (time <= 0)
        {
            time = 0;
            running = false;

            if (timesUpPanel != null)
                timesUpPanel.SetActive(true);

            Time.timeScale = 0;
        }

        ShowTime();
    }

    void ShowTime()
    {
        if (timerText != null)
            timerText.text = "Time Remaining: " + Mathf.CeilToInt(time);
    }
}