using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new();

        Vector2Int[] possibleMoves = new Vector2Int[]{new(2, 1), new(2, -1), new (-2, 1), new (-2, -1), new (1, 2), new (-1, 2), new (1, -2), new (-1, -2)};

        foreach (Vector2Int move in possibleMoves)
        {
            int nextX = currentX + move.x;
            int nextY = currentY + move.y;

            // Check if the new position is within the board boundaries.
            if (nextX >= 1 && nextX < tileCountX - 1 && nextY >= 1 && nextY < tileCountY - 1)
                // Add the move if the tile is empty or contains an opponent's piece.
                if (board[nextX, nextY] == null || board[nextX, nextY].team != team)
                    moves.Add(new Vector2Int(nextX, nextY));
        }    

        return moves;
    }
}
