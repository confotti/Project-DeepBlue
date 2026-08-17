using UnityEngine;
using System.Collections; 
using UnityEngine.UI;
using System.IO;

public class PhotoCapture : MonoBehaviour
{
    [SerializeField] private float sizeMultiplier = 1.5f;

    [Header("Save as file?")]
    [SerializeField] private bool SaveAsFile = false;
    [SerializeField] private string path = "Test/test_image_save.png";

    [Header("Photo Taker")]
    [SerializeField] private Image photoDisplayArea;
    [SerializeField] private GameObject photoFrame;
    [SerializeField] private GameObject cameraUI; 

    [Header("Flash Effect")]
    [SerializeField] private GameObject cameraFlash;
    [SerializeField] private float flashTime;

    [Header("Flash Fader Effect")]
    [SerializeField] private Animator fadingAnimation; 

    private Texture2D screenCapture;
    private bool viewingPhoto; 

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (!viewingPhoto)
            {
                StartCoroutine(CapturePhoto());
            }
            else
            {
                RemovePhoto(); 
            }
        }
    }

    void MakeBlackAndWhite()
    {
        Color[] pixels = screenCapture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];

            float gray = pixel.r * 0.299f +
                         pixel.g * 0.587f +
                         pixel.b * 0.114f;

            pixels[i] = new Color(gray, gray, gray, pixel.a);
        }

        screenCapture.SetPixels(pixels);
        screenCapture.Apply();
    }

    IEnumerator CapturePhoto()
    {
        screenCapture = new Texture2D((int)(Screen.width * 460 / 1920 * sizeMultiplier), (int)(Screen.height * 480 / 1080 * sizeMultiplier), TextureFormat.RGB24, false);

        cameraUI.SetActive(false); 
        viewingPhoto = true;

        yield return new WaitForEndOfFrame();

        Rect regionToRead = new Rect(Screen.width / 2 - screenCapture.width / 2, Screen.height / 2 - screenCapture.height / 2, Screen.width / 2 + screenCapture.width / 2, Screen.height / 2 + screenCapture.height / 2);

        screenCapture.ReadPixels(regionToRead, 0, 0, false);
        screenCapture.Apply();
        MakeBlackAndWhite(); 
        ShowPhoto();

        if (SaveAsFile) SaveImageAsFile();
    }

    void ShowPhoto()
    {
        Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0.0f, 0.0f, screenCapture.width, screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);
        photoDisplayArea.sprite = photoSprite;

        photoFrame.SetActive(true);
        StartCoroutine(CameraFlashEffect());
        fadingAnimation.Play("PhotoFade"); 
    }

    IEnumerator CameraFlashEffect()
    {
        cameraFlash.SetActive(true);
        yield return new WaitForSeconds(flashTime);
        cameraFlash.SetActive(false); 
    }

    void RemovePhoto()
    {
        viewingPhoto = false; 
        photoFrame.SetActive(false);
        cameraUI.SetActive(true); 
    }

    private void SaveImageAsFile()
    {
        byte[] bytes = screenCapture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/" + path, bytes);
    }
}
