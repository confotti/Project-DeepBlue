using System;
using UnityEngine;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour
{
    public static Action OnContinuePressed;

    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _quitButton;

    void Awake()
    {
        if(_continueButton) _continueButton.onClick.AddListener(ContinuePressed);
        if(_optionsButton) _optionsButton.onClick.AddListener(OptionsPressed);
        if(_quitButton) _quitButton.onClick.AddListener(QuitPressed);
    }

    void OnDestroy()
    {
        if(_continueButton) _continueButton.onClick.RemoveAllListeners();
        if(_optionsButton) _optionsButton.onClick.RemoveAllListeners();
        if(_quitButton) _quitButton.onClick.RemoveAllListeners();
    }

    private void ContinuePressed()
    {
        OnContinuePressed?.Invoke();
    }
    
    private void OptionsPressed()
    {
        
    }

    private void QuitPressed()
    {
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
