using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidPointDisplacement : MonoBehaviour
{
    public static MidPointDisplacement instance;
    void Awake()
    {
        instance = this;
    }
    public float[,] MidpointDisplacement(int size, float roughness, float noiseScale)
    {
        //size = size + 1;
        float[,] heights = new float[size, size];

        // Initialize corner heights
        heights[0, 0] = Random.Range(0f, 1f);
        heights[0, size - 1] = Random.Range(0f, 1f);
        heights[size - 1, 0] = Random.Range(0f, 1f);
        heights[size - 1, size - 1] = Random.Range(0f, 1f);

        int step = size - 1;

        // Midpoint Displacement algorithm
        for (int sideLength = size - 1; sideLength >= 2; sideLength /= 2, roughness /= 2)
        {
            int half = sideLength / 2;

            // Diamond step
            for (int x = 0; x < size - 1; x += sideLength)
            {
                for (int y = 0; y < size - 1; y += sideLength)
                {
                    float avg = heights[x, y] + heights[x + sideLength, y] +
                                heights[x, y + sideLength] + heights[x + sideLength, y + sideLength];
                    avg /= 4.0f;

                    heights[x + half, y + half] = avg + Random.Range(-roughness, roughness);
                }
            }

            // Square step
            for (int x = 0; x < size - 1; x += half)
            {
                for (int y = (x + half) % sideLength; y < size - 1; y += sideLength)
                {
                    float avg = heights[(x - half + size - 1) % (size - 1), y] +
                                heights[(x + half) % size, y] +
                                heights[x, (y + half) % size] +
                                heights[x, (y - half + size - 1) % (size - 1)];
                    avg /= 4.0f;

                    heights[x, y] = avg + Random.Range(-roughness, roughness);
                }
            }
        }
        Debug.Log(heights.Length);
        return heights;
    }
}
