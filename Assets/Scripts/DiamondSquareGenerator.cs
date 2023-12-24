using UnityEngine;

//It's a very fast diamond square algorithm which uses Perlin noise with offsets to get randomlike numbers with seeds(offset velues for perlin noise)

public class DiamondSquareGenerator
{
    private int terrainPoints;
    private float roughness;
    private float cornersInitialValue;
    private float perlinScale;
    private Vector2[] offsets;

    public DiamondSquareGenerator(int terrainPoints, float roughness, float cornersInitialValue)
    {
        this.terrainPoints = terrainPoints;
        this.roughness = roughness;
        this.cornersInitialValue = cornersInitialValue;
        this.perlinScale = 10f;
    }

    public DiamondSquareGenerator(int terrainPoints, float roughness, float cornersInitialValue, float perlinScale) : this(terrainPoints, roughness, cornersInitialValue)
    {
        this.perlinScale = perlinScale;
    }

    public DiamondSquareGenerator(int terrainPoints, float roughness, float cornersInitialValue, float perlinScale, Vector2[] offsets) : this(terrainPoints, roughness, cornersInitialValue, perlinScale)
    {
        this.offsets = offsets;        
    }

    public float[,] GenerateTerrain(int iterations)
    {
        int terrainSize = terrainPoints + 1;
        float[,] terrainData = new float[terrainSize, terrainSize];

        // Initialize the terrain data with the initial corners
        terrainData[0, 0] = terrainData[0, terrainSize - 1] = terrainData[terrainSize - 1, 0] = terrainData[terrainSize - 1, terrainSize - 1] = cornersInitialValue;

        for (int iteration = 0; iteration < iterations; iteration++)
        {
            float range = roughness;
            Vector2 offset;

            if (offsets == null || offsets.Length == 0)
            {
                offset = new Vector2(Random.Range(0f, 999999f), Random.Range(0f, 999999f));
            }
            else
            {
                if (offsets.Length != iterations)
                {
                    Debug.LogError("Iteration count and offset count are different!");
                    return null;
                }

                offset = offsets[iteration];
            }

            // Terrain generation using Diamond-Square algorithm
            for (int sideLength = terrainSize - 1; sideLength >= 2; sideLength /= 2, range *= 0.5f)
            {
                GenerateSquares(terrainData, sideLength, range, offset);
                GenerateDiamonds(terrainData, sideLength, range, offset);
            }
        }

        return terrainData;
    }

    private void GenerateSquares(float[,] terrainData, int sideLength, float range, Vector2 offset)
    {
        int halfSide = sideLength / 2;

        for (int x = 0; x < terrainData.GetLength(0) - 1; x += sideLength)
        {
            for (int y = 0; y < terrainData.GetLength(1) - 1; y += sideLength)
            {
                float average = (terrainData[x, y] + terrainData[x + sideLength, y] +
                                 terrainData[x, y + sideLength] + terrainData[x + sideLength, y + sideLength]) * 0.25f;

                int posX = x + halfSide;
                int posY = y + halfSide;

                terrainData[posX, posY] = Mathf.Clamp01(average + (GetPerlin(posX, posY, terrainData.GetLength(0), perlinScale, offset) * 2.0f * range) - range);
            }
        }
    }

    private void GenerateDiamonds(float[,] terrainData, int sideLength, float range, Vector2 offset)
    {
        int halfSide = sideLength / 2;

        for (int x = 0; x < terrainData.GetLength(0) - 1; x += halfSide)
        {
            for (int y = (x + halfSide) % sideLength; y < terrainData.GetLength(1) - 1; y += sideLength)
            {
                float average = (terrainData[(x - halfSide + terrainData.GetLength(0)) % terrainData.GetLength(0), y] +
                                 terrainData[(x + halfSide) % terrainData.GetLength(0), y] +
                                 terrainData[x, (y + halfSide) % terrainData.GetLength(1)] +
                                 terrainData[x, (y - halfSide + terrainData.GetLength(1)) % terrainData.GetLength(1)]) * 0.25f;

                average += (GetPerlin(x, y, terrainData.GetLength(0), perlinScale, offset) * 2 * range) - range;
                average = Mathf.Clamp01(average);

                terrainData[x, y] = average;

                if (x == 0) terrainData[terrainData.GetLength(0) - 1, y] = average;
                if (y == 0) terrainData[x, terrainData.GetLength(1) - 1] = average;
            }
        }
    }

    private float GetPerlin(float x, float y, float mapSize, float scale, Vector2 offset)
    {
        float xCoord = x / mapSize * scale + offset.x;
        float zCoord = y / mapSize * scale + offset.y;

        return Mathf.PerlinNoise(xCoord, zCoord);
    }
}
