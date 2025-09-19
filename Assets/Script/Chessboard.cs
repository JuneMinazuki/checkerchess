using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SpecialMove
{
    Castling,
    Promotion,
    JumpCapture
}

public class Chessboard : MonoBehaviour
{
    [Header("Art And Sprite")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float zOffset = -0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 1.75f;
    [SerializeField] private float deathSpacing = 0.37f;

    [Header("UI and Canvas")]
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject midGameUI;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;

    //LOGIC
    private ChessPiece[,] chessPieces;
    private readonly List<ChessPiece> deadWhites = new();
    private readonly List<ChessPiece> deadBlacks = new();
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new();
    private const int TILE_COUNT_X = 10;
    private const int TILE_COUNT_Y = 10;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private Vector2Int currentSelecting = -Vector2Int.one;
    private bool isWhiteTurn;
    private List<Vector2Int[]> moveList = new();
    private HashSet<SpecialMove> specialMoves;
    private bool isJumpCapture = false;

    private void Awake()
    {
        isWhiteTurn = true;

        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllPieces();
        PositionAllPieces();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        // If a mouse is present and being used
        if (Mouse.current != null)
            HandleInput(Mouse.current.position.ReadValue(), Mouse.current.leftButton.wasPressedThisFrame);


        // If a touch is present and being used
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            HandleInput(touch.position, touch.phase == UnityEngine.TouchPhase.Began);
        }
    }

    // Input Handle
    private void HandleInput(Vector2 screenPosition, bool wasPressedThisFrame)
    {
        Ray ray = currentCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight", "Selecting")))
        {
            // Get index of tile that the cursor/finger is touching
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            // Hover logic remains the same, but now applies to either mouse or touch
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            else if (currentHover != hitPosition)
            {
                if (ContainsValidMove(ref availableMoves, currentHover))
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Highlight");
                else if (currentHover == currentSelecting)
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Selecting");
                else
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");

                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // Handle a "click" or "touch began" event
            if (wasPressedThisFrame)
            {
                //To Select Chess Piece
                if (currentlyDragging == null)
                {
                    if (chessPieces[hitPosition.x, hitPosition.y] != null)
                    {
                        // Check If It Is Our Turn
                        if (((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn)) && !isJumpCapture)
                        {
                            currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                            tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Selecting");
                            currentSelecting = new(hitPosition.x, hitPosition.y);

                            // Get list of available moves, highlight those tiles
                            availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                            // Get list of special moves
                            specialMoves = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);

                            PreventCheck();

                            HighlightTiles();
                        }
                    }
                }
                //To Select Where Piece Move To
                else if (currentlyDragging != null)
                {
                    bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);

                    if (!isJumpCapture)
                        RemoveHighlightTiles();

                    if (!validMove)
                    {
                        if (chessPieces[hitPosition.x, hitPosition.y] != null && currentlyDragging.team == chessPieces[hitPosition.x, hitPosition.y].team && !isJumpCapture)
                        {
                            currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                            tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Selecting");
                            currentSelecting = new(hitPosition.x, hitPosition.y);

                            // Get list of available moves, highlight those tiles
                            availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                            HighlightTiles();
                        }
                        else if (!isJumpCapture)
                            currentlyDragging = null;
                    }
                    else if (!isJumpCapture)
                        currentlyDragging = null;
                }
            }
        }
        else
        {
            // Logic for when the cursor/finger is not over a tile
            if (currentHover != -Vector2Int.one)
            {
                if (ContainsValidMove(ref availableMoves, currentHover))
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Highlight");
                else if (currentHover == currentSelecting)
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Selecting");
                else
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
        }
    }

    // Board Generation
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        zOffset += transform.position.z;
        bounds = new Vector3(tileCountX / 2 * tileSize, 0, tileCountX / 2 * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, y * tileSize, zOffset) - bounds;
        vertices[1] = new Vector3(x * tileSize, (y + 1) * tileSize, zOffset) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, y * tileSize, zOffset) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, (y + 1) * tileSize, zOffset) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // Pieces Spawning
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0, blackTeam = 1;

        //White Team
        chessPieces[1, 1] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[2, 1] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[3, 1] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[4, 1] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[5, 1] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[6, 1] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[7, 1] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[8, 1] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 1; i < TILE_COUNT_X - 1; i++)
            chessPieces[i, 2] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);

        //White Team
        for (int i = 1; i < TILE_COUNT_X - 1; i += 2)
            chessPieces[i, 8] = SpawnSinglePiece(ChessPieceType.QueenChecker, blackTeam);
        for (int i = 2; i < TILE_COUNT_X - 1; i += 2)
            chessPieces[i, 7] = SpawnSinglePiece(ChessPieceType.Checker, blackTeam);
        for (int i = 1; i < TILE_COUNT_X - 1; i += 2)
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Checker, blackTeam);
    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;

        return cp;
    }

    // Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    PositionSinglePiece(x, y, true);
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, y * tileSize, zOffset) - bounds + new Vector3(tileSize / 2, tileSize / 2, 0);
    }

    // Highlight Tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        availableMoves.Clear();

        tiles[currentSelecting.x, currentSelecting.y].layer = LayerMask.NameToLayer("Tile");

        if (isJumpCapture)
        {
            tiles[currentlyDragging.currentX, currentlyDragging.currentY].layer = LayerMask.NameToLayer("Selecting");
            currentSelecting = new(currentlyDragging.currentX, currentlyDragging.currentY);
        }
        else
            currentSelecting = -Vector2Int.one;
    }

    // Checkmate
    private void CheckMate(int team)
    {
        DisplayVictory(team);
    }

    private void DisplayVictory(int winningTeam)
    {
        midGameUI.SetActive(false);

        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    public void OnResetButton()
    {
        // Hide UI
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        // Show Mid-Game UI
        midGameUI.SetActive(true);

        // Field Reset
        currentlyDragging = null;
        availableMoves.Clear();
        moveList.Clear();
        currentSelecting = -Vector2Int.one;
        isJumpCapture = false;

        // Clean Up
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                    Destroy(chessPieces[x, y].gameObject);

                chessPieces[x, y] = null;
                tiles[x, y].layer = LayerMask.NameToLayer("Tile");
            }
        }

        for (int i = 0; i < deadWhites.Count; i++)
            Destroy(deadWhites[i].gameObject);
        for (int i = 0; i < deadBlacks.Count; i++)
            Destroy(deadBlacks[i].gameObject);

        deadWhites.Clear();
        deadBlacks.Clear();

        // Spawn Pieces
        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
    }

    public void OnEndTurnButton()
    {
        isJumpCapture = false;
        RemoveHighlightTiles();

        isWhiteTurn = !isWhiteTurn;
        midGameUI.transform.GetChild(0).gameObject.SetActive(false);

        currentlyDragging = null;
    }

    //Special Moves
    private void ProcessSpecialMove()
    {
        if (specialMoves.Contains(SpecialMove.JumpCapture))
        {
            var jumpingCheckerMove = moveList[^1];

            // Check if jump capture
            if (Math.Abs(jumpingCheckerMove[0].x - jumpingCheckerMove[1].x) == 2 || Math.Abs(jumpingCheckerMove[0].y - jumpingCheckerMove[1].y) == 2)
            {
                // Get piece to be captured
                int capturedPieceX = (jumpingCheckerMove[0].x + jumpingCheckerMove[1].x) / 2;
                int capturedPieceY = (jumpingCheckerMove[0].y + jumpingCheckerMove[1].y) / 2;

                ChessPiece ocp = chessPieces[capturedPieceX, capturedPieceY];
                chessPieces[capturedPieceX, capturedPieceY] = null;

                // Capture enemy piece
                if (ocp.team == 0)
                {
                    if (ocp.type == ChessPieceType.King)
                        CheckMate(1);

                    deadWhites.Add(ocp);
                    ocp.SetScale(Vector3.one * deathSize);
                    if (deadWhites.Count <= 8)
                        ocp.SetPosition(new Vector3(-7.92f, -0.88f, zOffset) + deathSpacing * (deadWhites.Count - 1) * Vector3.right);
                    else
                        ocp.SetPosition(new Vector3(-7.92f, -1.38f, zOffset) + deathSpacing * (deadWhites.Count - 9) * Vector3.right);
                }
                else
                {
                    if (ocp.type == ChessPieceType.King)
                        CheckMate(0);

                    deadBlacks.Add(ocp);
                    ocp.SetScale(Vector3.one * deathSize);
                    if (deadBlacks.Count <= 8)
                        ocp.SetPosition(new Vector3(-7.92f, 0.88f, zOffset) + deathSpacing * (deadBlacks.Count - 1) * Vector3.right);
                    else
                        ocp.SetPosition(new Vector3(-7.92f, 0.38f, zOffset) + deathSpacing * (deadBlacks.Count - 9) * Vector3.right);
                }

                isJumpCapture = true;
            }
        }

        if (specialMoves.Contains(SpecialMove.Castling))
        {
            Vector2Int[] lastMove = moveList[^1];

            // Left Rook
            if (lastMove[1].x == 3)
            {
                if (lastMove[1].y == 1)
                {
                    chessPieces[4, 1] = chessPieces[1, 1];
                    PositionSinglePiece(4, 1);
                    chessPieces[1, 1] = null;
                }
                else if (lastMove[1].y == 8)
                {
                    chessPieces[4, 8] = chessPieces[1, 8];
                    PositionSinglePiece(4, 8);
                    chessPieces[1, 8] = null;
                }
            }
            // Right Rook
            else if (lastMove[1].x == 7)
            {
                if (lastMove[1].y == 1)
                {
                    chessPieces[6, 1] = chessPieces[8, 1];
                    PositionSinglePiece(6, 1);
                    chessPieces[8, 1] = null;
                }
                else if (lastMove[1].y == 8)
                {
                    chessPieces[6, 8] = chessPieces[8, 8];
                    PositionSinglePiece(6, 8);
                    chessPieces[8, 8] = null;
                }
            }
        }

        if (specialMoves.Contains(SpecialMove.Promotion))
        {
            Vector2Int[] lastMove = moveList[^1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if (targetPawn.type == ChessPieceType.Pawn)
            {
                if (targetPawn.team == 0 && lastMove[1].y == 8)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
                else if (targetPawn.team == 1 && lastMove[1].y == 1)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }

            if (targetPawn.type == ChessPieceType.Checker)
            {
                if (targetPawn.team == 0 && lastMove[1].y == 8)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.QueenChecker, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
                else if (targetPawn.team == 1 && lastMove[1].y == 1)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.QueenChecker, 1);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }

        specialMoves.Clear();
    }

    private void PreventCheck()
    {
        ChessPiece targetKing = null;

        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y].type == ChessPieceType.King && chessPieces[x, y].team == currentlyDragging.team)
                {
                    targetKing = chessPieces[x, y];
                    break;
                }

        //Delete move going into check
        if (targetKing != null)
            SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves, targetKing);
    }

    private void SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        // Save current values, reset after method
        int actualX = cp.currentX;
        int actualY = cp.currentY;
        List<Vector2Int> movesToRemove = new();

        // Going through every moves, simulate and check if legal move

        // Remove from move list
        foreach (Vector2Int move in movesToRemove)
            moves.Remove(move);
    }

    // Operation
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;

        return false;
    }

    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
            return false;

        Vector2Int previousPosition = new(cp.currentX, cp.currentY);

        // Check If Target Position Is Occupied By Another Piece
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];

            // Capture enemy piece
            if (ocp.team == 0)
            {
                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                if (deadWhites.Count <= 8)
                    ocp.SetPosition(new Vector3(-7.92f, -0.88f, zOffset) + deathSpacing * (deadWhites.Count - 1) * Vector3.right);
                else
                    ocp.SetPosition(new Vector3(-7.92f, -1.38f, zOffset) + deathSpacing * (deadWhites.Count - 9) * Vector3.right);

                if (deadWhites.Count >= 12)
                    CheckMate(1);
            }
            else
            {
                deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                if (deadBlacks.Count <= 8)
                    ocp.SetPosition(new Vector3(-7.92f, 0.88f, zOffset) + deathSpacing * (deadBlacks.Count - 1) * Vector3.right);
                else
                    ocp.SetPosition(new Vector3(-7.92f, 0.38f, zOffset) + deathSpacing * (deadBlacks.Count - 9) * Vector3.right);

                if (deadBlacks.Count >= 12)
                    CheckMate(0);
            }
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        moveList.Add(new Vector2Int[]{previousPosition, new(x,y)});
        PositionSinglePiece(x, y);

        ProcessSpecialMove();

        if (isJumpCapture)
        {
            currentlyDragging = chessPieces[x, y];

            List<Vector2Int> newJumps = new();
            specialMoves = currentlyDragging.CheckJumpMoves(newJumps, chessPieces, TILE_COUNT_X, TILE_COUNT_X);

            if (newJumps.Count > 0)
            {
                RemoveHighlightTiles();

                availableMoves = newJumps;
                HighlightTiles();

                midGameUI.transform.GetChild(0).gameObject.SetActive(true);

                return true;
            }
        }

        isWhiteTurn = !isWhiteTurn;
        midGameUI.transform.GetChild(0).gameObject.SetActive(false);
        isJumpCapture = false;

        return true;
    }

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

        return -Vector2Int.one; //Invalid return
    }
}