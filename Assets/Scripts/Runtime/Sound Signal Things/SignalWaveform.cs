using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class SignalWaveform : MaskableGraphic
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField, Range(32, 1024)]
    private int audioSamples = 256;

    [Header("Waveform")]
    [SerializeField, Range(32, 1024)]
    private int waveformPoints = 256;

    [SerializeField]
    private float amplitude = 40f;

    [SerializeField]
    private float thickness = 2f;

    [SerializeField, Range(0f, 1f)]
    private float smoothing = 0.75f;

    [Header("Scrolling")]
    [SerializeField]
    private float scrollSpeed = 1f;

    [SerializeField]
    private bool animate = true;

    [Header("Test Waveform")]
    [SerializeField]
    private bool useTestWaveform;

    [SerializeField]
    private float testFrequency = 4f;

    [SerializeField]
    private float testHarmonic = 0.25f;

    private float[] audioBuffer;
    private float[] waveformBuffer;
    private float[] smoothedBuffer;

    private float animationOffset;

    protected override void Awake()
    {
        base.Awake();

        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        audioBuffer = new float[audioSamples];
        waveformBuffer = new float[waveformPoints];
        smoothedBuffer = new float[waveformPoints];
    }

    private void Update()
    {
        if (animate)
        {
            animationOffset +=
                Time.deltaTime * scrollSpeed;
        }

        if (useTestWaveform)
            GenerateTestWaveform();
        else
            UpdateAudioWaveform();

        SetVerticesDirty();
    }

    // =========================================================
    // WAVEFORM GENERATION
    // =========================================================

    private void GenerateTestWaveform()
    {
        for (int i = 0; i < waveformPoints; i++)
        {
            float x =
                (float)i / (waveformPoints - 1);

            // Fixed waveform.
            float wave =
                Mathf.Sin(
                    x *
                    Mathf.PI *
                    2f *
                    testFrequency
                );

            wave +=
                Mathf.Sin(
                    x *
                    Mathf.PI *
                    2f *
                    testFrequency *
                    2.7f
                ) * testHarmonic;

            waveformBuffer[i] = wave;
        }

        ScrollAndSmoothWaveform();
    }

    private void UpdateAudioWaveform()
    {
        if (audioSource == null ||
            !audioSource.isPlaying)
        {
            FadeWaveform();
            return;
        }

        audioSource.GetOutputData(
            audioBuffer,
            0
        );

        for (int i = 0; i < waveformPoints; i++)
        {
            float normalized =
                (float)i /
                (waveformPoints - 1);

            int sample =
                Mathf.FloorToInt(
                    normalized *
                    (audioSamples - 1)
                );

            waveformBuffer[i] =
                audioBuffer[sample];
        }

        ScrollAndSmoothWaveform();
    }

    private void ScrollAndSmoothWaveform()
    {
        float offset =
            animationOffset *
            (waveformPoints - 1);

        for (int i = 0; i < waveformPoints; i++)
        {
            float samplePosition =
                i + offset;

            samplePosition =
                Mathf.Repeat(
                    samplePosition,
                    waveformPoints
                );

            int index0 =
                Mathf.FloorToInt(samplePosition);

            int index1 =
                (index0 + 1) %
                waveformPoints;

            float interpolation =
                samplePosition - index0;

            float value =
                Mathf.Lerp(
                    waveformBuffer[index0],
                    waveformBuffer[index1],
                    interpolation
                );

            smoothedBuffer[i] =
                Mathf.Lerp(
                    smoothedBuffer[i],
                    value,
                    1f - smoothing
                );
        }
    }

    private void FadeWaveform()
    {
        for (int i = 0; i < waveformPoints; i++)
        {
            smoothedBuffer[i] =
                Mathf.Lerp(
                    smoothedBuffer[i],
                    0f,
                    Time.deltaTime * 5f
                );
        }
    }

    // =========================================================
    // MESH
    // =========================================================

    protected override void OnPopulateMesh(
    VertexHelper vh)
    {
        vh.Clear();

        if (smoothedBuffer == null ||
            smoothedBuffer.Length < 2)
            return;

        Rect rect = rectTransform.rect;

        int count = smoothedBuffer.Length;

        Vector2[] points =
            new Vector2[count];

        Vector2[] normals =
            new Vector2[count];

        // ---------------------------------------------------------
        // Create points
        // ---------------------------------------------------------

        for (int i = 0; i < count; i++)
        {
            float t =
                (float)i /
                (count - 1);

            float x =
                Mathf.Lerp(
                    rect.xMin,
                    rect.xMax,
                    t
                );

            float y =
                smoothedBuffer[i] *
                amplitude;

            points[i] =
                new Vector2(x, y);
        }

        // ---------------------------------------------------------
        // Calculate normals
        // ---------------------------------------------------------

        for (int i = 0; i < count; i++)
        {
            Vector2 tangent;

            if (i == 0)
            {
                tangent =
                    points[1] -
                    points[0];
            }
            else if (i == count - 1)
            {
                tangent =
                    points[i] -
                    points[i - 1];
            }
            else
            {
                tangent =
                    points[i + 1] -
                    points[i - 1];
            }

            tangent.Normalize();

            normals[i] =
                new Vector2(
                    -tangent.y,
                    tangent.x
                );
        }

        // ---------------------------------------------------------
        // Vertices
        // ---------------------------------------------------------

        float halfThickness =
            thickness * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector2 offset =
                normals[i] *
                halfThickness;

            AddVertex(
                vh,
                points[i] + offset
            );

            AddVertex(
                vh,
                points[i] - offset
            );
        }

        // ---------------------------------------------------------
        // Triangles
        // ---------------------------------------------------------

        for (int i = 0; i < count - 1; i++)
        {
            int index =
                i * 2;

            vh.AddTriangle(
                index,
                index + 1,
                index + 2
            );

            vh.AddTriangle(
                index + 2,
                index + 1,
                index + 3
            );
        }
    }

    private void AddVertex(
        VertexHelper vh,
        Vector2 position)
    {
        UIVertex vertex =
            UIVertex.simpleVert;

        vertex.color = color;
        vertex.position = position;

        vh.AddVert(vertex);
    }


    // =========================================================
    // PUBLIC API
    // =========================================================

    public void SetAudioSource(
        AudioSource source)
    {
        audioSource = source;
    }

    public void SetSignalStrength(
        float strength)
    {
        strength =
            Mathf.Clamp01(strength);

        Color c = color;

        c.a = strength;

        color = c;

        SetVerticesDirty();
    }

    public void SetAmplitude(
        float value)
    {
        amplitude = value;

        SetVerticesDirty();
    }

    public void SetScrollSpeed(
        float value)
    {
        scrollSpeed = value;
    }
}