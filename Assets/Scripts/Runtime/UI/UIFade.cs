using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIFade : MonoBehaviour
{
    [SerializeField] private UIPort _uiPort;
    private Image _image;

    private Coroutine fadeCo;

    void Awake()
    {
        _image = GetComponent<Image>();
    }

    void OnEnable()
    {
        _uiPort.OnStartScreenFade += StartScreenFade;
    }

    void OnDisable()
    {
        _uiPort.OnStartScreenFade -= StartScreenFade;
    }

    private void StartScreenFade(bool toBlack, float time, UIPort.ScreenFadeDone callback)
    {
        if(fadeCo != null) return;
        if(toBlack) fadeCo = StartCoroutine(Fade(0, 1, time, callback));
        else fadeCo = StartCoroutine(Fade(1, 0, time, callback));
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float time = 1f, UIPort.ScreenFadeDone callback = null)
    {
        Color c = Color.black;
        c.a = startAlpha;
        _image.color = c;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            c.a = Mathf.Lerp(startAlpha, endAlpha, t);
            _image.color = c;
            yield return null;
        }

        fadeCo = null;
        
        c.a = endAlpha;
        _image.color = c;
        callback?.Invoke();
    }

/*
    IEnumerator TunnelvisionFade(float timeIn, float timeOut, UIPort.ScreenFadeDone callbackBlack = null, UIPort.ScreenFadeDone callbackDone = null)
    {
        
    }
    */
}
