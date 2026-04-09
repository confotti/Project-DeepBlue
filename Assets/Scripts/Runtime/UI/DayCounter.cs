using System.Collections;
using UnityEngine;
using TMPro;

public class DayCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float stayDuration = 2f;

    private void Start()
    {
        TimeManager.Instance.OnDayChanged += ShowDay;

        int startDay = TimeManager.Instance.GetGameTimeStamp().Day;
        ShowDay(startDay);
    }

    private void OnDisable()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnDayChanged -= ShowDay;
    } 

    private void ShowDay(int day)
    {
        StopAllCoroutines();
        StartCoroutine(ShowRoutine(day));
    }

    private IEnumerator ShowRoutine(int day)
    {
        canvasGroup.alpha = 0f; // <-- FIX

        dayText.text = "Day " + day;

        yield return Fade(0, 1);
        yield return new WaitForSeconds(stayDuration);
        yield return Fade(1, 0);
    }

    private IEnumerator Fade(float start, float end)
    {
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = end;
    }
} 