using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class WavesGenerator : MonoBehaviour
{
    // Cascades for different scales of wave simulation
    public WavesCascade cascade0;
    public WavesCascade cascade1;
    public WavesCascade cascade2;

    // Size of the textures (power of 2 for FFT compatibility)
    [SerializeField]
    int size = 512;

    // Settings for wave generation
    [SerializeField]
    WavesSettings wavesSettings;

    // Length scales for each cascade, affecting wave size
    [SerializeField]
    float lengthScale0 = 250;
    [SerializeField]
    float lengthScale1 = 17;
    [SerializeField]
    float lengthScale2 = 5;

    // Compute shaders for FFT and wave spectrum calculations
    [SerializeField]
    ComputeShader fftShader;
    [SerializeField]
    ComputeShader initialSpectrumShader;
    [SerializeField]
    ComputeShader timeDependentSpectrumShader;
    [SerializeField]
    ComputeShader texturesMergerShader;

    // Gaussian noise texture for wave randomness
    Texture2D gaussianNoise;
    FastFourierTransform fft;
    Texture2D physicsReadback;

    private void Awake()
    {
        // Setting frame rate to unlimited
        Application.targetFrameRate = -1;

        // Initialize FFT and Gaussian noise texture
        fft = new FastFourierTransform(size, fftShader);
        gaussianNoise = GetNoiseTexture(size);

        // Initialize cascades for different scale waves
        cascade0 = new WavesCascade(size, initialSpectrumShader, timeDependentSpectrumShader, texturesMergerShader, fft, gaussianNoise);
        cascade1 = new WavesCascade(size, initialSpectrumShader, timeDependentSpectrumShader, texturesMergerShader, fft, gaussianNoise);
        cascade2 = new WavesCascade(size, initialSpectrumShader, timeDependentSpectrumShader, texturesMergerShader, fft, gaussianNoise);
        // Initialize the cascades with appropriate settings
        InitialiseCascades();

        // Create a texture to read back physics data from the GPU
        physicsReadback = new Texture2D(size, size, TextureFormat.RGBAFloat, false);
    }

    private void Update()
    {
        // Re-initialize cascades on each frame (can be optimized if not needed every frame)
        InitialiseCascades();

        // Calculate wave displacements based on current time for each cascade
        cascade0.CalculateWavesAtTime(Time.time);
        cascade1.CalculateWavesAtTime(Time.time);
        cascade2.CalculateWavesAtTime(Time.time);

        // Request data readback from the GPU for displacement texture
        RequestReadbacks();
    }

    void InitialiseCascades()
    {
        // Define boundaries for different wave cascades based on length scales
        float boundary1 = 2 * Mathf.PI / lengthScale1 * 6f;
        float boundary2 = 2 * Mathf.PI / lengthScale2 * 6f;

        // Initialize each wave cascade with its respective settings
        cascade0.CalculateInitials(wavesSettings, lengthScale0, 0.0001f, boundary1);
        cascade1.CalculateInitials(wavesSettings, lengthScale1, boundary1, boundary2);
        cascade2.CalculateInitials(wavesSettings, lengthScale2, boundary2, 9999);

        // Set global shader parameters for length scales
        Shader.SetGlobalFloat("LengthScale0", lengthScale0);
        Shader.SetGlobalFloat("LengthScale1", lengthScale1);
        Shader.SetGlobalFloat("LengthScale2", lengthScale2);
    }

    Texture2D GetNoiseTexture(int size)
    {
        // Load or generate Gaussian noise texture based on size
        string filename = "GaussianNoiseTexture" + size.ToString() + "x" + size.ToString();
        Texture2D noise = Resources.Load<Texture2D>("GaussianNoiseTextures/" + filename);
        return noise ? noise : GenerateNoiseTexture(size, true);
    }

    Texture2D GenerateNoiseTexture(int size, bool saveIntoAssetFile)
    {
        // Generate a Gaussian noise texture
        Texture2D noise = new Texture2D(size, size, TextureFormat.RGFloat, false, true);
        noise.filterMode = FilterMode.Point;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                // Fill each pixel with Gaussian random values
                noise.SetPixel(i, j, new Vector4(NormalRandom(), NormalRandom()));
            }
        }
        noise.Apply();
        // If in Unity Editor, save the generated noise texture as an asset
#if UNITY_EDITOR
        if (saveIntoAssetFile)
        {
            string filename = "GaussianNoiseTexture" + size.ToString() + "x" + size.ToString();
            string path = "Assets/Resources/GaussianNoiseTextures/";
            AssetDatabase.CreateAsset(noise, path + filename + ".asset");
        }
#endif
        return noise;
    }
    // Generate a Gaussian random number
    float NormalRandom()
    {
        return Mathf.Cos(2 * Mathf.PI * Random.value) * Mathf.Sqrt(-2 * Mathf.Log(Random.value));
    }

    private void OnDestroy()
    {
        // Properly dispose of resources when object is destroyed
        cascade0.Dispose();
        cascade1.Dispose();
        cascade2.Dispose();
    }

    void RequestReadbacks()
    {
        // Request asynchronous GPU readback of the displacement texture
        AsyncGPUReadback.Request(cascade0.Displacement, 0, TextureFormat.RGBAFloat, OnCompleteReadback);
    }

    public float GetWaterHeight(Vector3 position)
    {
        // Calculate the water height at a given position by repeatedly refining displacement
        Vector3 displacement = GetWaterDisplacement(position);
        displacement = GetWaterDisplacement(position - displacement);
        displacement = GetWaterDisplacement(position - displacement);

        return GetWaterDisplacement(position - displacement).y;
    }

    public Vector3 GetWaterDisplacement(Vector3 position)
    {
        // Get water displacement from the physics readback texture at a given position
        Color c = physicsReadback.GetPixelBilinear(position.x / lengthScale0, position.z / lengthScale0);
        return new Vector3(c.r, c.g, c.b);
    }

    void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        OnCompleteReadback(request, physicsReadback);
    }

    void OnCompleteReadback(AsyncGPUReadbackRequest request, Texture2D result)
    {
        // Apply the readback data to the texture when GPU readback is complete
        if (result != null)
        {
            result.LoadRawTextureData(request.GetData<Color>());
            result.Apply();
        }
    }
}

