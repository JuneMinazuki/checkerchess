using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new();

        // Directions: diagonally
        Vector2Int[] directions = {new(1,1), new(1, -1), new(-1, 1), new(-1, -1)};

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
                    moves.Add(new Vector2Int(nextX, nextY));
                // If the tile has a piece
                else
                {
                    if (board[nextX, nextY].team != team)
                        moves.Add(new Vector2Int(nextX, nextY));
                    break;
                }
            }
        }

        return moves;
    }
}
