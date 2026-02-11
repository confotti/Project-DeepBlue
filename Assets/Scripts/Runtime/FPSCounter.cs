using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        float fps = 1.0f / deltaTime;
        float ms = deltaTime * 1000.0f;

        fpsText.text = $"FPS: {Mathf.Ceil(fps)}\n{ms:0.0} ms";

        if (fps >= 60)
            fpsText.color = Color.green;
        else if (fps >= 30)
            fpsText.color = Color.yellow;
        else
            fpsText.color = Color.red;
    }
}