using System;

public static class MatrixTraversal<T>
{
    /// <summary>
    /// Traverse a 2D matrix using a cubic square pattern.
    /// </summary>
    /// <param name="matrix">The 2D matrix to traverse.</param>
    /// <param name="centerX">The x-coordinate of the center. Use -1 to set to the center of the matrix.</param>
    /// <param name="centerY">The y-coordinate of the center. Use -1 to set to the center of the matrix.</param>
    /// <param name="maxDistance">The maximum traversal distance from the center.</param>
    /// <returns>An array of indexes representing the traversed elements.</returns>
    public static (int x, int y)[] TraverseMatrixCubic(T[,] matrix, int centerX, int centerY, int maxDistance)
    {
        if (centerX == -1) centerX = matrix.GetLength(0) / 2;
        if (centerY == -1) centerY = matrix.GetLength(1) / 2;

        int matrixMaximumDistance = Math.Max(centerX, centerY);

        if (maxDistance == -1 || maxDistance > matrixMaximumDistance) maxDistance = matrixMaximumDistance;

        int maxElements = (maxDistance + 1) * (maxDistance * 4 + 1) / 2;
        (int x, int y)[] indexes = new (int x, int y)[maxElements];

        int index = 0;

        for (int distance = 0; distance <= maxDistance; distance++)
        {
            for (int i = -distance; i <= distance; i++)
            {
                int x = centerX + i;
                int y = centerY + distance;
                if (IsValidIndex(x, y)) indexes[index++] = (x, y);

                y = centerY - distance;
                if (IsValidIndex(x, y)) indexes[index++] = (x, y);
            }

            for (int j = -distance + 1; j < distance; j++)
            {
                int x = centerX + distance;
                int y = centerY + j;
                if (IsValidIndex(x, y)) indexes[index++] = (x, y);

                x = centerX - distance;
                if (IsValidIndex(x, y)) indexes[index++] = (x, y);
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
    /// <returns>An array of indexes representing the traversed elements.</returns>
    public static (int x, int y)[] TraverseMatrixSpiral(T[,] matrix, int centerX, int centerY, int maxDistance)
    {
        if (centerX == -1) centerX = matrix.GetLength(0) / 2;
        if (centerY == -1) centerY = matrix.GetLength(1) / 2;

        int maxElements = (maxDistance * 2 + 1) * (maxDistance * 2 + 1);
        (int x, int y)[] indexes = new (int x, int y)[maxElements];

        int x = centerX;
        int y = centerY;
        int dx = 1;
        int dy = 0;
        int distance = 0;
        int stepsInCurrentDirection = 0;
        int stepsBeforeDirectionChange = 1;
        int index = 0;

        while (distance <= maxDistance)
        {
            if (IsValidIndex(x, y))
            {
                indexes[index++] = (x, y);
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

        // Trim the array to remove excess unused elements
        Array.Resize(ref indexes, index);

        return indexes;

        bool IsValidIndex(int x, int y)
        {
            return x >= 0 && x < matrix.GetLength(0) && y >= 0 && y < matrix.GetLength(1);
        }
    }
}