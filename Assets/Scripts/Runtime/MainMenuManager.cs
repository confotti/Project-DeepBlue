using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _quitButton;

    void Awake()
    {
        if(_newGameButton) _newGameButton.onClick.AddListener(NewGamePressed);
        if(_optionsButton) _optionsButton.onClick.AddListener(OptionsPressed);
        if(_quitButton) _quitButton.onClick.AddListener(QuitPressed);
    }

    void OnDestroy()
    {
        if(_newGameButton) _newGameButton.onClick.RemoveAllListeners();
        if(_optionsButton) _optionsButton.onClick.RemoveAllListeners();
        if(_quitButton) _quitButton.onClick.RemoveAllListeners();
    }

    private void NewGamePressed()
    {
        
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
