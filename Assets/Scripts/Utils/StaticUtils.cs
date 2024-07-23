using System;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Random = UnityEngine.Random;

public static class StaticUtils
{
    [BurstCompile]
    public static float Remap(float input, float inputMin, float inputMax, float targetMin, float targetMax) => targetMin + (input - inputMin) * (targetMax - targetMin) / (inputMax - inputMin);

    [BurstCompile]
    public static int Array3DTo1D(int x, int y, int z, int xMax, int yMax) => (z * xMax * yMax) + (y * xMax) + x;

    [BurstCompile]
    public static Vector3Int Array1Dto3D(int idx, int xMax, int yMax)
    {
        int z = idx / (xMax * yMax);
        idx -= (z * xMax * yMax);
        int y = idx / xMax;
        int x = idx % xMax;
        return new Vector3Int(x, y, z);
    }

    [BurstCompile]
    public static int Array2dTo1d(int x, int y, int width) => y * width + x;

    [BurstCompile]
    public static Vector2Int Array1dTo2d(int i, int width) => new Vector2Int { x = i % width, y = i / width };

    [BurstCompile]
    public static bool IsPointInsideSphere(Vector3 Ppoint, Vector3 Ccenter, float Cradius) => (Vector3.Distance(Ppoint, Ccenter) <= Cradius);

    [BurstCompile]
    public static float Rounder(float x, float g = 16) => Mathf.Floor((x + g / 2) / g) * g;

    [BurstCompile]
    public static int Rounder(int x, int g = 16) => Mathf.FloorToInt((x + g / 2) / g) * g;

    [BurstCompile]
    public static bool IsTooSteep(Vector3 normal, Vector3 direction, float steepness) => Mathf.Abs(Vector3.Dot(normal, direction)) < steepness;

    [BurstCompile]
    public static Vector3 Snap(Vector3 pos, int v)
    {
        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        x = Mathf.FloorToInt(x / v) * v;
        y = Mathf.FloorToInt(y / v) * v;
        z = Mathf.FloorToInt(z / v) * v;
        return new Vector3(x, y, z);
    }

    //Returns local coords!
    [BurstCompile]
    public static List<Vector2> GeneratePoissonPoints(float minDistance, Vector2 bounds, int maxAttempts)
    {
        List<Vector2> points = new List<Vector2>();
        float cellSize = minDistance / Mathf.Sqrt(2);
        int[,] grid = new int[Mathf.CeilToInt(bounds.x / cellSize), Mathf.CeilToInt(bounds.y / cellSize)];
        List<Vector2> activeList = new List<Vector2>();

        Vector2 firstPoint = new Vector2(Random.Range(0, bounds.x), Random.Range(0, bounds.y));
        points.Add(firstPoint);
        activeList.Add(firstPoint);
        grid[Mathf.FloorToInt(firstPoint.x / cellSize), Mathf.FloorToInt(firstPoint.y / cellSize)] = points.Count;

        while (activeList.Count > 0)
        {
            int randomIndex = Random.Range(0, activeList.Count);
            Vector2 point = activeList[randomIndex];
            bool foundPoint = false;

            for (int i = 0; i < maxAttempts; i++)
            {
                float angle = Random.Range(0, Mathf.PI * 2);
                float radius = Random.Range(minDistance, minDistance * 2);
                Vector2 newPoint = new Vector2(point.x + radius * Mathf.Cos(angle), point.y + radius * Mathf.Sin(angle));

                if (newPoint.x >= 0 && newPoint.x < bounds.x && newPoint.y >= 0 && newPoint.y < bounds.y)
                {
                    int cellX = Mathf.FloorToInt(newPoint.x / cellSize);
                    int cellY = Mathf.FloorToInt(newPoint.y / cellSize);
                    bool canPlace = true;

                    for (int x = Mathf.Max(0, cellX - 2); x < Mathf.Min(grid.GetLength(0), cellX + 3); x++)
                    {
                        for (int y = Mathf.Max(0, cellY - 2); y < Mathf.Min(grid.GetLength(1), cellY + 3); y++)
                        {
                            if (grid[x, y] > 0 && Vector2.Distance(points[grid[x, y] - 1], newPoint) < minDistance)
                            {
                                canPlace = false;
                                break;
                            }
                        }
                    }

                    if (canPlace)
                    {
                        points.Add(newPoint);
                        activeList.Add(newPoint);
                        grid[cellX, cellY] = points.Count;
                        foundPoint = true;
                    }
                }
            }

            if (!foundPoint)
            {
                activeList.RemoveAt(randomIndex);
            }
        }

        return points;
    }

    //Returns Local coords!
    [BurstCompile]
    public static List<Vector3> GeneratePoissonPoints(float minDistance, Bounds bounds, int maxAttempts)
    {
        List<Vector3> points = new List<Vector3>();
        float cellSize = minDistance / Mathf.Sqrt(2);
        int[,] grid = new int[Mathf.CeilToInt(bounds.size.x / cellSize), Mathf.CeilToInt(bounds.size.z / cellSize)];
        List<Vector3> activeList = new List<Vector3>();

        Vector3 firstPoint = new Vector3(Random.Range(0, bounds.size.x), 0.0f, Random.Range(0, bounds.size.z));
        points.Add(firstPoint);
        activeList.Add(firstPoint);
        grid[Mathf.FloorToInt(firstPoint.x / cellSize), Mathf.FloorToInt(firstPoint.z / cellSize)] = points.Count;

        while (activeList.Count > 0)
        {
            int randomIndex = Random.Range(0, activeList.Count);
            Vector3 point = activeList[randomIndex];
            bool foundPoint = false;

            for (int i = 0; i < maxAttempts; i++)
            {
                float angle = Random.Range(0, Mathf.PI * 2);
                float radius = Random.Range(minDistance, minDistance * 2);
                Vector3 newPoint = new Vector3(point.x + radius * Mathf.Cos(angle), 0.0f, point.z + radius * Mathf.Sin(angle));

                if (newPoint.x >= 0 && newPoint.x < bounds.size.x && newPoint.z >= 0 && newPoint.z < bounds.size.z)
                {
                    int cellX = Mathf.FloorToInt(newPoint.x / cellSize);
                    int cellY = Mathf.FloorToInt(newPoint.z / cellSize);
                    bool canPlace = true;

                    for (int x = Mathf.Max(0, cellX - 2); x < Mathf.Min(grid.GetLength(0), cellX + 3); x++)
                    {
                        for (int y = Mathf.Max(0, cellY - 2); y < Mathf.Min(grid.GetLength(1), cellY + 3); y++)
                        {
                            if (grid[x, y] > 0 && Vector3.Distance(points[grid[x, y] - 1], newPoint) < minDistance)
                            {
                                canPlace = false;
                                break;
                            }
                        }
                    }

                    if (canPlace)
                    {
                        points.Add(newPoint);
                        activeList.Add(newPoint);
                        grid[cellX, cellY] = points.Count;
                        foundPoint = true;
                    }
                }
            }

            if (!foundPoint)
            {
                activeList.RemoveAt(randomIndex);
            }
        }

        points.TrimExcess();
        return points;
    }

    public static Texture2D GradientToTexture(Gradient gradient, int width)
    {
        Texture2D texture = new Texture2D(width, 1, TextureFormat.RGB24, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < texture.width; i++)
        {
            float t = i / (float)(texture.width - 1);
            Color color = gradient.Evaluate(t);
            texture.SetPixel(i, 0, color);
        }

        texture.Apply();

        return texture;
    }

    public static void AddLayerToCameraCullingMask(Camera camera, string layerName) => camera.cullingMask |= (1 << LayerMask.NameToLayer(layerName));
    public static void RemoveLayerFromCameraCullingMask(Camera camera, string layerName) => camera.cullingMask &= ~(1 << LayerMask.NameToLayer(layerName));

    //Inverse distance weighted interpolation
    //https://gisgeography.com/inverse-distance-weighting-idw-interpolation/
    [BurstCompile]
    public static float InverseDistanceWeightedInterpolation(Func<Vector3, float> getValueMethod, Vector3 position, Vector3[] pattern)
    {
        float value = 0;
        float inverseDistance = 0;

        for (int p = 0; p < pattern.Length; p++)
        {
            Vector3 curentPos = position + pattern[p];
            float currentValue = getValueMethod(curentPos);

            if (float.IsNaN(currentValue)) continue;

            float distance = Vector3.Distance(curentPos, position);

            if (distance == 0f) return currentValue;

            float weight = 1.0f / distance;
            value += currentValue * weight;
            inverseDistance += weight;
        }

        return value / inverseDistance;
    }

    [BurstCompile]
    public static float Averaging(Vector3 position, Vector3[] pattern, Func<Vector3, float> getValueMethod)
    {
            float value = 0f;
            float sampleCount = 0f;

            for (int p = 0; p < pattern.Length; p++)
            {
                Vector3 currentPos = position + pattern[p];

                value += getValueMethod(currentPos);
                sampleCount++;
            }

            return value / sampleCount;
    }

    //Builds 2D MxM matrix pattern, distance based, circle
    [BurstCompile]
    public static List<Vector3> GetPatternCirlce(float stepSize, float range, bool includeCenter = true)
    {
        int matrixSize = Mathf.CeilToInt(range / stepSize);
        List<Vector3> pattern = new List<Vector3>(matrixSize * matrixSize);

        for (float y = -range; y <= range; y += stepSize)
        {
            for (float x = -range; x <= range; x += stepSize)
            {
                Vector3 currentPos = new Vector3(x, 0.0f, y);

                if (currentPos == Vector3.zero && !includeCenter) continue;

                if (Vector3.Distance(Vector3.zero, currentPos) <= range) pattern.Add(currentPos);
            }
        }

        pattern.TrimExcess();

        return pattern;
    }

    //Builds 2D MxM matrix pattern, cube
    [BurstCompile]
    public static List<Vector3> GetPatternCube(float stepSize, float range, bool includeCenter = true)
    {
        int matrixSize = Mathf.CeilToInt(range / stepSize);
        List<Vector3> pattern = new List<Vector3>(matrixSize * matrixSize);

        for (float y = -range; y <= range; y += stepSize)
        {
            for (float x = -range; x <= range; x += stepSize)
            {
                Vector3 currentPos = new Vector3(x, 0.0f, y);

                if (currentPos == Vector3.zero && !includeCenter) continue;

                pattern.Add(currentPos);
            }
        }

        pattern.TrimExcess();

        return pattern;
    }

    [BurstCompile]
    public static void Copy2DArray(float[,] source, float[,] destination, int startX, int startY, int lengthX, int lengthY)
    {
        // Check the bounds to ensure we don't go out of the array limits
        if (startX < 0 || startY < 0 || lengthX <= 0 || lengthY <= 0 ||
            startX + lengthX > source.GetLength(0) || startY + lengthY > source.GetLength(1) ||
            lengthX > destination.GetLength(0) || lengthY > destination.GetLength(1))
        {
            throw new ArgumentException("Invalid dimensions for copying array");
        }

        for (int i = 0; i < lengthX; i++)
        {
            for (int j = 0; j < lengthY; j++)
            {
                destination[i, j] = source[startX + i, startY + j];
            }
        }
    }

    [BurstCompile]
    public static void Copy2DArrayFast(float[,] source, float[,] destination, int startX, int startY, int lengthX, int lengthY)
    {
        // Check the bounds to ensure we don't go out of the array limits
        if (startX < 0 || startY < 0 || lengthX <= 0 || lengthY <= 0 ||
            startX + lengthX > source.GetLength(0) || startY + lengthY > source.GetLength(1) ||
            lengthX > destination.GetLength(0) || lengthY > destination.GetLength(1))
        {
            throw new ArgumentException("Invalid dimensions for copying array");
        }

        for (int i = 0; i < lengthX; i++)
        {
            // Calculate the starting index in the source array and destination array for each row
            int sourceIndex = (startX + i) * source.GetLength(1) + startY;
            int destinationIndex = i * destination.GetLength(1);

            // Copy the row from the source to the destination
            Array.Copy(source, sourceIndex, destination, destinationIndex, lengthY);
        }
    }
}