using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new();

        // Directions: diagonally
        Vector2Int[] directions = {new(1,1), new(1, -1), new(-1, 1), new(-1, -1)};

        foreach (Vector2Int dir in directions)
        {
            for (int i = 1; i < 8; i++)
            {
                int nextX = currentX + dir.x * i;
                int nextY = currentY + dir.y * i;

                // Check if the tile is within the board boundaries
                if (nextX < 1 || nextX >= 9 || nextY < 1 || nextY >= 9)
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
