using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UIController : MonoBehaviour
{
    public TerrainGenerator terrainGenerator;
    public Slider heightMultiplierSlider;
    public Slider smoothnessSlider;
    public TextMeshProUGUI heightMultiplierText; 
    public TextMeshProUGUI smoothnessText; 
    public Button generateButton;

    private void Start()
    {
        heightMultiplierSlider.value = terrainGenerator.heightMultiplier;
        smoothnessSlider.value = terrainGenerator.smoothness;
        
        UpdateHeightMultiplierText();
        UpdateSmoothnessText();
        
        heightMultiplierSlider.onValueChanged.AddListener(delegate { HeightMultiplierChanged(); });
        smoothnessSlider.onValueChanged.AddListener(delegate { SmoothnessChanged(); });
        
        generateButton.onClick.AddListener(delegate { GenerateButtonPressed(); });
    }

    public void GenerateButtonPressed()
    {
        terrainGenerator.GenerateTerrain();
    }

    public void HeightMultiplierChanged()
    {
        terrainGenerator.heightMultiplier = heightMultiplierSlider.value;
        UpdateHeightMultiplierText();
    }

    public void SmoothnessChanged()
    {
        terrainGenerator.smoothness = smoothnessSlider.value;
        UpdateSmoothnessText();
    }

    void UpdateHeightMultiplierText()
    {
        heightMultiplierText.text = "Height Multiplier: " + heightMultiplierSlider.value.ToString("F2");
    }

    void UpdateSmoothnessText()
    {
        smoothnessText.text = "Smoothness: " + smoothnessSlider.value.ToString("F2");
    }
}