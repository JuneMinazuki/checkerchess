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
        List<Vector2Int> r = new();

        return r;
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
}
