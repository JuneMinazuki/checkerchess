using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
     public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new();

        // Directions: Up, Down, Left, Right, Diagonally
        Vector2Int[] directions = {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new(1,1), new(1, -1), new(-1, 1), new(-1, -1)};

        foreach (Vector2Int dir in directions)
        {
            for (int i = 1; i < Mathf.Max(tileCountX, tileCountY); i++)
            {
                int nextX = currentX + dir.x * i;
                int nextY = currentY + dir.y * i;

                // Check if the tile is within the board boundaries
                if (nextX < 1 || nextX >= tileCountX - 1 || nextY < 1 || nextY >= tileCountY - 1)
                    break;

                // If the tile is empty
                if (board[nextX, nextY] == null)
                    r.Add(new Vector2Int(nextX, nextY));
                // If the tile has a piece
                else
                {
                    if (board[nextX, nextY].team != team)
                        r.Add(new Vector2Int(nextX, nextY));
                    break;
                }
            }
        }

        return r;
    }
}
