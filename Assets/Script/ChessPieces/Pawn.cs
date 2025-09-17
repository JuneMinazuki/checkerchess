using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new();

        int direction = (team == 0) ? 1 : -1;
        int startRow = (team == 0) ? 2 : tileCountY - 3;

        // Move 1 in front
        if (board[currentX, currentY + direction] == null)
        {
            moves.Add(new Vector2Int(currentX, currentY + direction));

            // Move 2 in front
            if (currentY == startRow && board[currentX, currentY + direction * 2] == null)
                moves.Add(new Vector2Int(currentX, currentY + direction * 2));
        }

        // Capture sideway
        int[] captureOffsets = { -1, 1 };

        foreach (int xOffset in captureOffsets)
        {
            int nextX = currentX + xOffset;
            int nextY = currentY + direction;

            if (nextX >= 0 && nextX < tileCountX && nextY >= 0 && nextY < tileCountY)
                if (board[nextX, nextY] != null && board[nextX, nextY].team != team)
                    moves.Add(new Vector2Int(nextX, nextY));
        }

        return moves;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> movelist, ref List<Vector2Int> avaiableMoves)
    {
        // Promotion
        if ((team == 0 && currentY == 7) || (team == 1 && currentY == 2))
            return SpecialMove.Promotion;

        return SpecialMove.None;
    }
}
