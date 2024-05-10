using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerupTimer : MonoBehaviour
{
    private Image timerMask;
    private TextMeshProUGUI text;

    public float StartingTime;
    public float TimeRemaining;
    private bool isTimerRunning;

    // Start is called before the first frame update
    void Start()
    {
        timerMask = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.gameObject.SetActive(false);
        timerMask.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isTimerRunning)
        {
            if (TimeRemaining > 0)
            {
                TimeRemaining -= Time.deltaTime;
                timerMask.fillAmount = TimeRemaining / StartingTime;
            }
            else
            {
                Debug.Log("Powerup time has run out!");
                TimeRemaining = 0;
                isTimerRunning = false;
                timerMask.fillAmount = 0;
                text.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Restarts the powerup timer
    /// </summary>
    /// <param name="durationSeconds">New time to set in seconds</param>
    public void RestartTimer(float durationSeconds)
    {
        isTimerRunning = true;
        StartingTime = durationSeconds;
        TimeRemaining = StartingTime;
        text.gameObject.SetActive(true);
    }
}
