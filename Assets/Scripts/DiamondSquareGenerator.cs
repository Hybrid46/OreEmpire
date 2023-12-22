using UnityEngine;

public class DiamondSquareGenerator
{
    private int terrainPoints;
    private float roughness;
    private float cornersInitialValue;

    public DiamondSquareGenerator(int terrainPoints, float roughness, float cornersInitialValue)
    {
        this.terrainPoints = terrainPoints;
        this.roughness = roughness;
        this.cornersInitialValue = cornersInitialValue;
    }

    public float[,] GenerateTerrain()
    {
        int terrainSize = terrainPoints + 1;
        float[,] terrainData = new float[terrainSize, terrainSize];

        // Set seed value to corners
        terrainData[0, 0] = terrainData[0, terrainSize - 1] = terrainData[terrainSize - 1, 0] = terrainData[terrainSize - 1, terrainSize - 1] = cornersInitialValue;

        // Terrain generation using Diamond-Square algorithm
        float range = roughness;
        for (int sideLength = terrainSize - 1; sideLength >= 2; sideLength /= 2, range *= 0.5f)
        {
            GenerateSquares(terrainData, sideLength, range);
            GenerateDiamonds(terrainData, sideLength, range);
        }

        return terrainData;
    }

    private void GenerateSquares(float[,] terrainData, int sideLength, float range)
    {
        int halfSide = sideLength / 2;

        for (int x = 0; x < terrainData.GetLength(0) - 1; x += sideLength)
        {
            for (int y = 0; y < terrainData.GetLength(1) - 1; y += sideLength)
            {
                float average = (terrainData[x, y] + terrainData[x + sideLength, y] +
                                 terrainData[x, y + sideLength] + terrainData[x + sideLength, y + sideLength]) / 4.0f;

                terrainData[x + halfSide, y + halfSide] = average + (Random.Range(0.0f, 1.0f) * 2 * range) - range;
            }
        }
    }

    private void GenerateDiamonds(float[,] terrainData, int sideLength, float range)
    {
        int halfSide = sideLength / 2;

        for (int x = 0; x < terrainData.GetLength(0) - 1; x += halfSide)
        {
            for (int y = (x + halfSide) % sideLength; y < terrainData.GetLength(1) - 1; y += sideLength)
            {
                float average = (terrainData[(x - halfSide + terrainData.GetLength(0)) % terrainData.GetLength(0), y] +
                                 terrainData[(x + halfSide) % terrainData.GetLength(0), y] +
                                 terrainData[x, (y + halfSide) % terrainData.GetLength(1)] +
                                 terrainData[x, (y - halfSide + terrainData.GetLength(1)) % terrainData.GetLength(1)]) / 4.0f;

                average += (Random.Range(0.0f, 1.0f) * 2 * range) - range;
                terrainData[x, y] = average;

                if (x == 0) terrainData[terrainData.GetLength(0) - 1, y] = average;
                if (y == 0) terrainData[x, terrainData.GetLength(1) - 1] = average;
            }
        }
    }
}
