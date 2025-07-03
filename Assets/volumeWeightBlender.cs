using UnityEngine;
using UnityEngine.Rendering; 

public class VolumeWeightBlender : MonoBehaviour
{
    public Volume volume;
    public float transitionSpeed = 1f;
    public float targetWeight = 1f;

    private void Update()
    {
        if (volume != null)
        {
            volume.weight = Mathf.Lerp(volume.weight, targetWeight, transitionSpeed * Time.deltaTime);
        }
    }

    public void SetTargetWeight(float newWeight)
    {
        targetWeight = Mathf.Clamp01(newWeight);
    }
} 
