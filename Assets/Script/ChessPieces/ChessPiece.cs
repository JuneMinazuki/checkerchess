using System;
using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6,
    Checker = 7,
    QueenChecker = 8
}

public class ChessPiece : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    private Vector3 desiredPostiton;
    private Vector3 desiredScale = new(2.25f, 2.25f, 2.25f);

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPostiton, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new();

        return moves;
    }

    public virtual SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> movelist, ref List<Vector2Int> avaiableMoves)
    {
        return SpecialMove.None;
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPostiton = position;
        if (force)
        {
            transform.position = desiredPostiton;
        }
    }

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }

    public SpecialMove CheckJumpMoves(List<Vector2Int> moves, ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        bool haveSpecialMove = false;

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
                moves.Add(jumpTo);
                haveSpecialMove = true;
        }

        if (haveSpecialMove)
            return SpecialMove.JumpCapture;

        return SpecialMove.None;        
    }
}
