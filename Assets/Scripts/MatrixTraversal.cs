using System;
using System.Collections.Generic;

public static class MatrixTraversal<T>
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum QuarterDirection
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// Traverse a 2D matrix using a cubic square pattern.
    /// </summary>
    /// <param name="matrix">The 2D matrix to traverse.</param>
    /// <param name="centerX">The x-coordinate of the center. Use -1 to set to the center of the matrix.</param>
    /// <param name="centerY">The y-coordinate of the center. Use -1 to set to the center of the matrix.</param>
    /// <param name="maxDistance">The maximum traversal distance from the center.</param>
    /// <returns>A list of indexes representing the traversed elements.</returns>
    public static List<(int x, int y)> TraverseMatrixCubic(T[,] matrix, int centerX, int centerY, int maxDistance)
    {
        if (centerX == -1) centerX = matrix.GetLength(0) / 2;
        if (centerY == -1) centerY = matrix.GetLength(1) / 2;

        int matrixMaximumDistance = Math.Max(centerX, centerY);

        if (maxDistance == -1 || maxDistance > matrixMaximumDistance) maxDistance = matrixMaximumDistance;

        List<(int x, int y)> indexes = new List<(int x, int y)>();

        for (int distance = 0; distance <= maxDistance; distance++)
        {
            for (int i = -distance; i <= distance; i++)
            {
                int x = centerX + i;
                int y = centerY + distance;
                if (IsValidIndex(x, y)) indexes.Add((x, y));

                y = centerY - distance;
                if (IsValidIndex(x, y)) indexes.Add((x, y));
            }

            for (int j = -distance + 1; j < distance; j++)
            {
                int x = centerX + distance;
                int y = centerY + j;
                if (IsValidIndex(x, y)) indexes.Add((x, y));

                x = centerX - distance;
                if (IsValidIndex(x, y)) indexes.Add((x, y));
            }
        }

        return indexes;

        bool IsValidIndex(int x, int y)
        {
            return x >= 0 && x < matrix.GetLength(0) && y >= 0 && y < matrix.GetLength(1);
        }
    }

    /// <summary>
    /// Traverse a 2D matrix using a spiral pattern.
    /// </summary>
    /// <param name="matrix">The 2D matrix to traverse.</param>
    /// <param name="centerX">The x-coordinate of the center. Use -1 to set to the center of the matrix.</param>
    /// <param name="centerY">The y-coordinate of the center. Use -1 to set to the center of the matrix.</param>
    /// <param name="maxDistance">The maximum traversal distance from the center.</param>
    /// <returns>A list of indexes representing the traversed elements.</returns>
    public static List<(int x, int y)> TraverseMatrixSpiral(T[,] matrix, int centerX, int centerY, int maxDistance)
    {
        if (centerX == -1) centerX = matrix.GetLength(0) / 2;
        if (centerY == -1) centerY = matrix.GetLength(1) / 2;

        List<(int x, int y)> indexes = new List<(int x, int y)>();

        int x = centerX;
        int y = centerY;
        int dx = 1;
        int dy = 0;
        int distance = 0;
        int stepsInCurrentDirection = 0;
        int stepsBeforeDirectionChange = 1;

        while (distance <= maxDistance)
        {
            if (IsValidIndex(x, y))
            {
                indexes.Add((x, y));
            }

            x += dx;
            y += dy;

            stepsInCurrentDirection++;
            if (stepsInCurrentDirection == stepsBeforeDirectionChange)
            {
                stepsInCurrentDirection = 0;
                int temp = dx;
                dx = -dy;
                dy = temp;
                if (dy == 0) // Horizontal movement
                    stepsBeforeDirectionChange++;
            }

            distance = Math.Max(Math.Abs(centerX - x), Math.Abs(centerY - y));
        }

        return indexes;

        bool IsValidIndex(int x, int y)
        {
            return x >= 0 && x < matrix.GetLength(0) && y >= 0 && y < matrix.GetLength(1);
        }
    }

    /// <summary>
    /// Traverses a 2D matrix from a specified starting point towards a side within a given range.
    /// </summary>
    /// <typeparam name="T">The type of elements in the matrix.</typeparam>
    /// <param name="matrix">The 2D matrix to traverse.</param>
    /// <param name="startX">The starting X-coordinate within the matrix.</param>
    /// <param name="startY">The starting Y-coordinate within the matrix.</param>
    /// <param name="direction">The direction towards which to traverse.</param>
    /// <param name="range">The range of traversal from the starting point.</param>
    /// <returns>A list of coordinates representing the traversal path.</returns>
    public static List<(int x, int y)> TraverseToSide(T[,] matrix, int startX, int startY, Direction direction, int range)
    {
        List<(int x, int y)> traversalPath = new List<(int x, int y)>();

        int xStep = 0;
        int yStep = 0;

        // Determine step direction based on the specified direction
        switch (direction)
        {
            case Direction.Up:
                yStep = -1;
                break;
            case Direction.Down:
                yStep = 1;
                break;
            case Direction.Left:
                xStep = -1;
                break;
            case Direction.Right:
                xStep = 1;
                break;
        }

        int currentX = startX;
        int currentY = startY;

        // Traverse until reaching the boundary of the specified range
        for (int i = 0; i < range; i++)
        {
            // Check if the current position is within the matrix bounds
            if (currentX >= 0 && currentX < matrix.GetLength(0) &&
                currentY >= 0 && currentY < matrix.GetLength(1))
            {
                traversalPath.Add((currentX, currentY));
            }
            else
            {
                // If the current position is out of bounds, stop traversal
                break;
            }

            // Move to the next position
            currentX += xStep;
            currentY += yStep;
        }

        return traversalPath;
    }

    /// <summary>
    /// Traverses a quarter of a 2D matrix within specified ranges on the X and Y axes.
    /// </summary>
    /// <typeparam name="T">The type of elements in the matrix.</typeparam>
    /// <param name="matrix">The 2D matrix to traverse.</param>
    /// <param name="startX">The starting X-coordinate within the matrix.</param>
    /// <param name="startY">The starting Y-coordinate within the matrix.</param>
    /// <param name="xRange">The range of traversal on the X-axis.</param>
    /// <param name="yRange">The range of traversal on the Y-axis.</param>
    /// <param name="direction">The direction of traversal.</param>
    /// <returns>A list of coordinates representing the traversal path.</returns>
    public static List<(int x, int y)> TraverseQuarter<T>(T[,] matrix, int startX, int startY, int xRange, int yRange, QuarterDirection direction)
    {
        List<(int x, int y)> traversalPath = new List<(int x, int y)>();

        // Determine the ending coordinates based on the ranges and direction
        int endX, endY;
        switch (direction)
        {
            case QuarterDirection.TopLeft:
                endX = Math.Min(startX + xRange, matrix.GetLength(0) - 1);
                endY = Math.Min(startY + yRange, matrix.GetLength(1) - 1);
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        traversalPath.Add((x, y));
                    }
                }
                break;
            case QuarterDirection.TopRight:
                endX = Math.Max(startX - xRange, 0);
                endY = Math.Min(startY + yRange, matrix.GetLength(1) - 1);
                for (int x = startX; x >= endX; x--)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        traversalPath.Add((x, y));
                    }
                }
                break;
            case QuarterDirection.BottomLeft:
                endX = Math.Min(startX + xRange, matrix.GetLength(0) - 1);
                endY = Math.Max(startY - yRange, 0);
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y >= endY; y--)
                    {
                        traversalPath.Add((x, y));
                    }
                }
                break;
            case QuarterDirection.BottomRight:
                endX = Math.Max(startX - xRange, 0);
                endY = Math.Max(startY - yRange, 0);
                for (int x = startX; x >= endX; x--)
                {
                    for (int y = startY; y >= endY; y--)
                    {
                        traversalPath.Add((x, y));
                    }
                }
                break;
        }

        return traversalPath;
    }
}