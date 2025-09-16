using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new();

        Vector2Int[] possibleMoves = new Vector2Int[]
        {
            new (1,0), new (-1,0), new (0,1), new (0,-1),
            new (1,1), new (-1,1), new (1,-1), new (-1,-1),
        };

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

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> movelist, ref List<Vector2Int> avaiableMoves)
    {
        SpecialMove r = SpecialMove.None;

        var kingMove = movelist.Find(m => m[0].x == 5 && m[0].y == ((team == 0) ? 1 : 8));
        var leftRook = movelist.Find(m => m[0].x == 1 && m[0].y == ((team == 0) ? 1 : 8));
        var rightRook = movelist.Find(m => m[0].x == 8 && m[0].y == ((team == 0) ? 1 : 8));

        if (kingMove == null)
        {
            int row = (team == 0) ? 1 : 8;

            if (leftRook == null && board[4, row] == null && board[3, row] == null && board[2, row] == null)
            {
                avaiableMoves.Add(new Vector2Int(3, row));
                r = SpecialMove.Castling;
            }

            if (rightRook == null && board[6, row] == null && board[7, row] == null)
            {
                avaiableMoves.Add(new Vector2Int(7, row));
                r = SpecialMove.Castling;
            }
        }

        return r;
    }
}