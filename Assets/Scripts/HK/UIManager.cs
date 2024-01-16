using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject canvas;

    public GameObject perlinNoisePanel;
    public GameObject midPointDisplacementPanel;

    //type
    public TMP_Dropdown TerrainTypeDropdown;

    public TMP_InputField Scale;
    public Button ScaleSetlButton;

    //PerlinNoise
    public Slider NoiseScaleSlider;
    public Slider OctavesSlider;
    public Slider PersistanceSlider;
    public Slider LacunaritySlider;
    public Slider HeightSlider1;
    public Slider SeedSlider1;
    public Slider OffsetXSlider;
    public Slider OffsetYSlider;

    //MidPointDisplacement
    public Button ScaleAddButton;
    public Button ScaleSubButton;
    public TMP_Text ScaleText;
    public Slider SeedSlider2;
    public Slider RoughnessSlider;
    public Slider HeightSlider2;


    

    private bool isHide = true;

    void Start()
    {
        TerrainTypeDropdown.value = 0;
        perlinNoisePanel.SetActive(true);
        midPointDisplacementPanel.SetActive(false);

        //PerlinNoise
        NoiseScaleSlider.value = NoisMapGenerator.instance.noiseScale;
        OctavesSlider.value = NoisMapGenerator.instance.octaves;
        PersistanceSlider.value = NoisMapGenerator.instance.persistance;
        LacunaritySlider.value = NoisMapGenerator.instance.lacunarity;
        HeightSlider1.value = NoisMapGenerator.instance.meshHeightMultiplier;
        SeedSlider1.value = NoisMapGenerator.instance.seed;
        OffsetXSlider.value = NoisMapGenerator.instance.offset.x;
        OffsetYSlider.value = NoisMapGenerator.instance.offset.y;

        //MidPointDisplacement
        SeedSlider2.value = NoisMapGenerator.instance.seed;
        RoughnessSlider.value = NoisMapGenerator.instance.roughness;
        HeightSlider2.value = NoisMapGenerator.instance.meshHeightMultiplier2;

        //type
        TerrainTypeDropdown.onValueChanged.AddListener(OnTerrianTypeValueChanged);

        //PerlinNoise
        ScaleSetlButton.onClick.AddListener(GetScaleValue);

        NoiseScaleSlider.onValueChanged.AddListener(OnNoiseScaleValueChanged);
        OctavesSlider.onValueChanged.AddListener(OnOctavesValueChanged);
        PersistanceSlider.onValueChanged.AddListener(OnPersistanceValueChanged);
        LacunaritySlider.onValueChanged.AddListener(OnLacunarityValueChanged);
        HeightSlider1.onValueChanged.AddListener(OnHeightValueChanged);
        SeedSlider1.onValueChanged.AddListener(OnSeedValueChanged);
        OffsetXSlider.onValueChanged.AddListener(OnOffsetXValueChanged);
        OffsetYSlider.onValueChanged.AddListener(OnOffsetYValueChanged);

        //MidPointDisplacement
        SeedSlider2.onValueChanged.AddListener(OnSeedValueChanged);
        RoughnessSlider.onValueChanged.AddListener(OnRoughnessValueChanged);
        HeightSlider2.onValueChanged.AddListener(OnHeightValue2Changed);

        ScaleAddButton.onClick.AddListener(MidPointScaleAdd);
        ScaleSubButton.onClick.AddListener(MidPointScaleSub);


        //TODO: change text

        

    }
    void Update()
    {
        Debug.Log("NoisMapGenerator.instance.size");

        if(NoisMapGenerator.instance.size >= 256)
        {
            ScaleAddButton.interactable = false;
        }
        else
        {
            ScaleAddButton.interactable = true;
        }
        if(NoisMapGenerator.instance.size <= 2)
        {
            ScaleSubButton.interactable = false;
        }
        else
        {
            ScaleSubButton.interactable = true;
        }
    }

    public void OnTerrianTypeValueChanged(int value)
    {
        //Debug.Log("OnTerrianTypeValueChanged");
        //NoisMapGenerator.instance.terrainType = (NoisMapGenerator.TerrainType)TerrainTypeDropdown.value;
        switch(value)
        {
            case 0:
                perlinNoisePanel.SetActive(true);
                midPointDisplacementPanel.SetActive(false);
                //set default value
                NoisMapGenerator.instance.SetDefaltValueForPerlinNoise();
                NoisMapGenerator.instance.terrainType = NoisMapGenerator.TerrainType.PerlinNoise;
                break;
            case 1:
                perlinNoisePanel.SetActive(false);
                midPointDisplacementPanel.SetActive(true);
                //set default value
                NoisMapGenerator.instance.SetDefaltValueForMidPointDisplacement();
                NoisMapGenerator.instance.terrainType = NoisMapGenerator.TerrainType.MidPointDisplacement;
                break;
        }
        NoisMapGenerator.instance.GenerateMap();
    }

    public void GetScaleValue()
    {
        string scale = Scale.text;
        if (int.TryParse(scale, out int intValue) && intValue>0 && intValue < 256)
        {
            Debug.Log("int" + intValue);

            NoisMapGenerator.instance.mapWidth = intValue;
            NoisMapGenerator.instance.mapHeight = intValue;
            NoisMapGenerator.instance.GenerateMap();
        }
        else
        {
            Debug.LogWarning("not int");
        }
    }

    public void OnNoiseScaleValueChanged(float value)
    {
        Debug.Log("OnNoiseScaleValueChanged");
        NoisMapGenerator.instance.noiseScale = value;
        NoisMapGenerator.instance.GenerateMap();
    }

    public void OnOctavesValueChanged(float value)
    {
        NoisMapGenerator.instance.octaves = (int)value;
        NoisMapGenerator.instance.GenerateMap();
    }

    public void OnPersistanceValueChanged(float value)
    {
        NoisMapGenerator.instance.persistance = value;
        NoisMapGenerator.instance.GenerateMap();
    }

    public void OnLacunarityValueChanged(float value)
    {
        NoisMapGenerator.instance.lacunarity = value;
        NoisMapGenerator.instance.GenerateMap();
    }

    public void OnHeightValueChanged(float value)
    {
        NoisMapGenerator.instance.meshHeightMultiplier = value;
        NoisMapGenerator.instance.GenerateMap();
    }

    public void OnSeedValueChanged(float value)
    {
        NoisMapGenerator.instance.seed = (int)value;
        NoisMapGenerator.instance.GenerateMap();
    }

    public void OnOffsetXValueChanged(float value)
    {
        NoisMapGenerator.instance.offset.x = value;
        NoisMapGenerator.instance.GenerateMap();

    }

    public void OnOffsetYValueChanged(float value)
    {
        NoisMapGenerator.instance.offset.y = value;
        NoisMapGenerator.instance.GenerateMap();
    }

    public void OnRoughnessValueChanged(float value)
    {
        NoisMapGenerator.instance.roughness = value;
        NoisMapGenerator.instance.GenerateMap();
    }

    public void MidPointScaleAdd()
    {
        Debug.Log("NoisMapGenerator.instance.size");
        if(NoisMapGenerator.instance.size >= 128)
        {
            ScaleAddButton.interactable = false;
        }
        else
        {
            ScaleAddButton.interactable = true;
        }
        ScaleAddButton.interactable = true;
        NoisMapGenerator.instance.size *= 2;
        ScaleText.text = NoisMapGenerator.instance.size.ToString();
        NoisMapGenerator.instance.GenerateMap();

    }
    public void MidPointScaleSub()
    {
        if (NoisMapGenerator.instance.size == 2)
        {
            ScaleSubButton.interactable = false;
        }
        else
        {
            ScaleSubButton.interactable = true;
        }

        ScaleSubButton.interactable = true;
        NoisMapGenerator.instance.size /= 2;
        ScaleText.text = NoisMapGenerator.instance.size.ToString();
        NoisMapGenerator.instance.GenerateMap();
    }

    public void OnHeightValue2Changed(float value)
    {
        NoisMapGenerator.instance.meshHeightMultiplier2 = value;
        NoisMapGenerator.instance.GenerateMap();
    }



}
