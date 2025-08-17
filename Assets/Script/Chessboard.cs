using System;
using NUnit.Framework.Constraints;
using Unity.Collections;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;

public class Chessboard : MonoBehaviour
{
    [Header("Art And Sprite")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;

    //LOGIC
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private const int TILE_COUNT_X = 10;
    private const int TILE_COUNT_Y = 10;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;

    private void Awake()
    {
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

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover")))
        {
            // Get index of tile that the mouse is touching
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            // If hovering a tile after not hovering one
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If changing the tile hovering
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // Mouse Pressed
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                //To Select Chess Piece
                if (currentlyDragging == null)
                {
                    if (chessPieces[hitPosition.x, hitPosition.y] != null)
                    {
                        // Check If It Is Our Turn (Currently Always True)
                        if (true)
                        {
                            currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                        }
                    }
                }
                //To Select Where Piece Move To
                else if (currentlyDragging != null)
                {
                    //Vector2Int previousPostion = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                    bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                    currentlyDragging = null;
                    if (!validMove)
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                    }
                }
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
        }
    }

    // Board Generation
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.z;
        bounds = new Vector3(tileCountX / 2 * tileSize, 0, tileCountX / 2 * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, y * tileSize, yOffset) - bounds;
        vertices[1] = new Vector3(x * tileSize, (y + 1) * tileSize, yOffset) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, y * tileSize, yOffset) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, (y + 1) * tileSize, yOffset) - bounds;

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
        chessPieces[4, 1] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 1] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
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
        return new Vector3(x * tileSize, y * tileSize, yOffset) - bounds + new Vector3(tileSize / 2, tileSize / 2, 0);
    }

    // Operation
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        // Check If Target Position Is Occupied By Another Piece
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];

            if (cp.team == ocp.team)
            {
                return false;
            }
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

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