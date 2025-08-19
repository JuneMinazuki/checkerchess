using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new();

        int direction = (team == 0) ? 1 : -1;

        // Move 1 in front
        if (board[currentX, currentY + direction] == null)
            r.Add(new Vector2Int(currentX, currentY + direction));

        // Move 2 in front
        if (board[currentX, currentY + direction] == null)
        {
            // White team
            if (team == 0 && currentY == 2 && board[currentX, currentY + direction * 2] == null)
                r.Add(new Vector2Int(currentX, currentY + direction * 2));
            
            // Black team
            if (team == 1 && currentY == 2 && board[currentX, currentY + direction * 2] == null)
                r.Add(new Vector2Int(currentX, currentY + direction * 2));
        }

        // Capture sideway
        if (currentX != tileCountX - 2 && board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
            r.Add(new Vector2Int(currentX + 1, currentY + direction));
        if (currentX != 1 && board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
            r.Add(new Vector2Int(currentX - 1, currentY + direction));

        return r;
    }
}
