using System.Collections.Generic;
using UnityEngine;

public class Checker : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new();

        int direction = (team == 0) ? 1 : -1;

        // Normal movement (1 in front and diagonally)
        AddNormalMoves(moves, board, tileCountX, tileCountY, direction);

        return moves;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> movelist, ref List<Vector2Int> avaiableMoves)
    {
        bool haveSpecialMove = false;
        int direction = (team == 0) ? 1 : -1;
        int tileCountX = 10;
        int tileCountY = 10;

        // Define all possible jump directions (x, y)
        Vector2Int[] jumpDirections = new Vector2Int[] { new(0, direction), new(-1, direction), new(1, direction), new(-1, 0), new(1, 0) };

        foreach (var jumpDir in jumpDirections)
        {
            Vector2Int jumpOver = new(currentX + jumpDir.x, currentY + jumpDir.y);
            Vector2Int jumpTo = new(currentX + jumpDir.x * 2, currentY + jumpDir.y * 2);

            // Check if the jump-to tile is on the board and empty
            if (IsOnBoard(jumpTo, tileCountX, tileCountY, 0) && board[jumpTo.x, jumpTo.y] == null && board[jumpOver.x, jumpOver.y] != null && board[jumpOver.x, jumpOver.y].team != team)
            {
                avaiableMoves.Add(jumpTo);
                haveSpecialMove = true;
            }
        }

        if (haveSpecialMove)
            return SpecialMove.JumpCapture;

        return SpecialMove.None;
    }
    
    private void AddNormalMoves(List<Vector2Int> moves, ChessPiece[,] board, int tileCountX, int tileCountY, int direction)
    {
        // Check forward move
        Vector2Int forwardMove = new(currentX, currentY + direction);
        if (IsOnBoard(forwardMove, tileCountX, tileCountY) && board[forwardMove.x, forwardMove.y] == null)
            moves.Add(forwardMove);

        // Check diagonal moves
        int[] diagonalX = { -1, 1 };
        foreach (int x in diagonalX)
        {
            Vector2Int diagonalMove = new(currentX + x, currentY + direction);
            if (IsOnBoard(diagonalMove, tileCountX, tileCountY) && board[diagonalMove.x, diagonalMove.y] == null)
                moves.Add(diagonalMove);
        }
    }

    private bool IsOnBoard(Vector2Int position, int tileCountX, int tileCountY, int edge = 1)
    {
        return position.x >= edge && position.x < tileCountX - edge && position.y >= edge && position.y < tileCountY - edge;
    }
}
