using System.Collections.Generic;
using UnityEngine;

public class QueenChecker : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new();

        // Normal movement
        AddNormalMoves(moves, board, tileCountX, tileCountY);

        return moves;
    }

    public override HashSet<SpecialMove> GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> movelist, ref List<Vector2Int> avaiableMoves)
    {
        HashSet<SpecialMove> r = new();
        int tileCountX = 10;
        int tileCountY = 10;

        // Define all possible jump directions (x, y)
        Vector2Int[] jumpDirections = new Vector2Int[]
        {
            new (1,0), new (-1,0), new (0,1), new (0,-1),
            new (1,1), new (-1,1), new (1,-1), new (-1,-1),
        };

        foreach (var jumpDir in jumpDirections)
        {
            Vector2Int jumpOver = new(currentX + jumpDir.x, currentY + jumpDir.y);
            Vector2Int jumpTo = new(currentX + jumpDir.x * 2, currentY + jumpDir.y * 2);

            // Check if the jump-to tile is on the board and empty
            if (jumpTo.x >= 0 && jumpTo.x < tileCountX && jumpTo.y >= 0 && jumpTo.y < tileCountY && board[jumpTo.x, jumpTo.y] == null && board[jumpOver.x, jumpOver.y] != null && board[jumpOver.x, jumpOver.y].team != team)
            {
                avaiableMoves.Add(jumpTo);
                r.Add(SpecialMove.JumpCapture);
            }
        }

        return r;
    }
    
    private void AddNormalMoves(List<Vector2Int> moves, ChessPiece[,] board, int tileCountX, int tileCountY)
    {
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
                if (board[nextX, nextY] == null)
                    moves.Add(new Vector2Int(nextX, nextY));
        }
    }

    private bool IsOnBoard(Vector2Int position, int tileCountX, int tileCountY, int edge = 1)
    {
        return position.x >= edge && position.x < tileCountX - edge && position.y >= edge && position.y < tileCountY - edge;
    }
}
