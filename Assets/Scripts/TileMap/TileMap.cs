using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public Tile[,] tiles;
    public Mesh mesh;

    public void RenderTiles(int centerX, int centerY, int rangeX, int rangeY)
    {
        List<(int x, int y)> renderTiles = new List<(int x, int y)>(rangeX * rangeY * 4);

        renderTiles.AddRange(MatrixTraversal<Tile>.TraverseQuarter(tiles, centerX, centerY, 10, 10, MatrixTraversal<Tile>.QuarterDirection.TopLeft));
        renderTiles.AddRange(MatrixTraversal<Tile>.TraverseQuarter(tiles, centerX, centerY, 10, 10, MatrixTraversal<Tile>.QuarterDirection.TopRight));
        renderTiles.AddRange(MatrixTraversal<Tile>.TraverseQuarter(tiles, centerX, centerY, 10, 10, MatrixTraversal<Tile>.QuarterDirection.BottomLeft));
        renderTiles.AddRange(MatrixTraversal<Tile>.TraverseQuarter(tiles, centerX, centerY, 10, 10, MatrixTraversal<Tile>.QuarterDirection.BottomRight));

        renderTiles.ForEach(tile =>
        {
            Vector3 position = new Vector3(tile.x, 0f, tile.y);
            Quaternion rotation = RotationToQuaternion(tiles[tile.x, tile.y].rotationDirection);

            GraphicDrawer.instance.AddInstance(tiles[tile.x, tile.y].tileData.tileMaterial, position, rotation, Vector3.one * 1.2f);
        });
    }

    private Quaternion RotationToQuaternion(Tile.RotationDirection rotationDirection)
    {
        switch (rotationDirection)
        {
            case Tile.RotationDirection.Up:
                return Quaternion.identity;
            case Tile.RotationDirection.Down:
                return Quaternion.Euler(0f, 0f, 180f);
            case Tile.RotationDirection.Right:
                return Quaternion.Euler(0f, 0f, -90f);
            case Tile.RotationDirection.Left:
                return Quaternion.Euler(0f, 0f, 90f);
        }

        return Quaternion.identity;
    }
}
