using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New UI Port", menuName = "Ports/UI Port")]
public class UIPort : ScriptableObject
{
    //Screen Fade
    public delegate void ScreenFadeDone();
    public event Action<bool, float, ScreenFadeDone> OnStartScreenFade;
    public void StartScreenFade(bool toBlack, float time = 1, ScreenFadeDone callback = null) => OnStartScreenFade?.Invoke(toBlack, time, callback);


    //Player stats UI and oxygen blur. 
    public Action<int> OnSetMaxOxygen;
    public Action<float> UpdateOxygenUI;
    public Action<float> UpdateHealthBar;
    public Action<float> UpdateDrownTime;
}
