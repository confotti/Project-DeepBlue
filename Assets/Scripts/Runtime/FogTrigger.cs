using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FogTrigger : MonoBehaviour
{
    [Header("Fog Settings")]
    public float dayFogStart = 0f;
    public float dayFogEnd = 150f;
    public float nightFogStart = 0f;
    public float nightFogEnd = 50f;
    public Color dayFogColor = Color.gray;
    public Color nightFogColor = Color.black;

    [Header("Background Settings")]
    public Color dayBackgroundColor = new Color(0.5f, 0.8f, 1f);
    public Color nightBackgroundColor = new Color(0f, 0f, 0.1f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FogManager.Instance?.RegisterTrigger(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FogManager.Instance?.UnregisterTrigger(this);
        }
    }

    // Returns the target fog settings based on the current day/night factor
    public void GetTargetSettings(float dayFactor, out float fogStart, out float fogEnd, out Color fogColor, out Color backgroundColor)
    {
        fogStart = Mathf.Lerp(nightFogStart, dayFogStart, dayFactor);
        fogEnd = Mathf.Lerp(nightFogEnd, dayFogEnd, dayFactor);
        fogColor = Color.Lerp(nightFogColor, dayFogColor, dayFactor);
        backgroundColor = Color.Lerp(nightBackgroundColor, dayBackgroundColor, dayFactor);
    }
}