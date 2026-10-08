using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public float time = 60;
    public TMP_Text timerText;
    public GameObject timesUpPanel;

    bool running = true;

    public bool IsRunning => running && time > 0;

    public bool TryAddTime(float seconds)
    {
        if (!IsRunning || Time.timeScale <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds <= 0)
            return false;
        time = Mathf.Min(time + seconds, 3600f);
        ShowTime();
        return true;
    }

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
