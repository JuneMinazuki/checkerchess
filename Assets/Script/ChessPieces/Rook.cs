using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new();

        // Move up
        for (int i = currentY + 1; i <= tileCountY - 2; i++)
        {
            if (board[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i));
            else
            {
                if (board[currentX, i].team != team)
                    r.Add(new Vector2Int(currentX, i));
                break;
            }
        }

        // Move down
        for (int i = currentY - 1; i >= 1; i--)
            if (board[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i));
            else
            {
                if (board[currentX, i].team != team)
                    r.Add(new Vector2Int(currentX, i));
                break;
            }

        // Move left
        for (int i = currentX - 1; i >= 1; i--)
            if (board[i, currentY] == null)
                r.Add(new Vector2Int(i, currentY));
            else
            {
                if (board[i, currentY].team != team)
                    r.Add(new Vector2Int(i, currentY));
                break;
            }

        // Move right
        for (int i = currentX + 1; i <= tileCountX - 2; i++)
            if (board[i, currentY] == null)
                r.Add(new Vector2Int(i, currentY));
            else
            {
                if (board[i, currentY].team != team)
                    r.Add(new Vector2Int(i, currentY));
                break;
            }

        return r;
    }
}
