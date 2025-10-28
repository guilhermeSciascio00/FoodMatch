using UnityEngine;

enum Directions
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] int boardHeight;
    [SerializeField] int boardWidth;
    [SerializeField] Tile boardTile;
    [SerializeField] Piece basePiece;

    private Tile[,] _tiles;

    private void Start()
    {
        _tiles = new Tile[boardHeight, boardWidth];
        CreateGameBoard();
        CreateAllPieces();
    }

    private void CreateGameBoard()
    {
        for(int x = 0; x < boardWidth; x++)
        {
            for(int y = 0; y < boardHeight; y++)
            {
                CreateSingleTile(new Vector2Int(x, y));
            }
        }
    }


    private Tile CreateSingleTile(Vector2Int tilePosition)
    {
        Tile _tile = Instantiate(boardTile, new Vector3(tilePosition.x, tilePosition.y), Quaternion.identity, this.transform);

        _tiles[tilePosition.x, tilePosition.y] = _tile;

        _tiles[tilePosition.x, tilePosition.y].TilePosition = new Vector3Int(tilePosition.x, tilePosition.y);

        return _tile;
    }

    private Piece CreateSinglePiece(Vector2Int piecePosition)
    {
        //Create the piece and adds its current tile
        Piece _piece = Instantiate(basePiece, new Vector3(piecePosition.x, piecePosition.y), Quaternion.identity, _tiles[piecePosition.x, piecePosition.y].transform);
        _piece.CurrentTile = _tiles[piecePosition.x, piecePosition.y];

        _piece.SetPiece();

        //Here we update the tile state and its piece reference
        _tiles[piecePosition.x, piecePosition.y].PieceReference = _piece;
        _tiles[piecePosition.x, piecePosition.y].TileState = TileState.HoldingAPiece;

        return _piece;
    }

    private void CreateAllPieces()
    {
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                SpawnCheckGuard(CreateSinglePiece(new Vector2Int(x, y)));
            }
        }
    }

    /* Checks the left and doww neighbors. if it's not the same, just return, if it is the same, we check the neighbor of this neighbor to see if we have a three match..(Recursive function might be useful here.. */

    //Avoids free matches in the spawn phase.
    private void SpawnCheckGuard(Piece currentPiece)
    {

        Piece neighbourLeftPiece = CheckNeighbourPiece(currentPiece, Directions.Left);

        Piece neighbourBottomPiece = CheckNeighbourPiece(currentPiece, Directions.Down);

        if(CheckPiecesMatch(currentPiece, neighbourLeftPiece))
        {
            if(CheckPiecesMatch(neighbourLeftPiece, CheckNeighbourPiece(neighbourLeftPiece, Directions.Left)))
            {
                Piece neigbourLeftLeft = CheckNeighbourPiece(neighbourLeftPiece, Directions.Left);
                neigbourLeftLeft.SetPiece();
            }
        }

        if(CheckPiecesMatch(currentPiece, neighbourBottomPiece))
        {
            if (CheckPiecesMatch(neighbourBottomPiece, CheckNeighbourPiece(neighbourBottomPiece, Directions.Down)))
            {
                Piece neighbourBottomBottom = CheckNeighbourPiece(neighbourBottomPiece, Directions.Down);
                neighbourBottomBottom.SetPiece();
            }
        }

    }

    /// <summary>
    /// The piece1 is the currentPiece and the piece2 is the target Piece
    /// </summary>
    /// <param name="piece1"></param>
    /// <param name="piece2"></param>
    /// <returns></returns>
    private bool CheckPiecesMatch(Piece piece1, Piece piece2)
    {
        if(piece1 == null || piece2 == null) { return false; }

        if(piece1.PieceType == piece2.PieceType && piece1.PieceSprite == piece2.PieceSprite) 
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// This method checks for the neighbor piece, if it exits, returns it, it's also necessary to pass the direction you want to check
    /// </summary>
    /// <param name="pieceToCheck"></param>
    /// <param name="directionToCheck"></param>
    /// <returns></returns>
    private Piece CheckNeighbourPiece(Piece pieceToCheck, Directions directionToCheck)
    {

        Vector2Int currentPiecePos = (Vector2Int)pieceToCheck.CurrentTile.TilePosition;

        Vector2Int directionOffset = Vector2Int.zero;

        switch (directionToCheck)
        {
            case Directions.Up:
                directionOffset = Vector2Int.up;
                break;
            case Directions.Down:
                directionOffset = Vector2Int.down;
                break;
            case Directions.Left:
                directionOffset = Vector2Int.left;
                break;
            case Directions.Right:
                directionOffset = Vector2Int.right;
                break;
        }

        Vector2Int neighborPiecePos = currentPiecePos + directionOffset;

        if(CheckIfTileExists(neighborPiecePos))
        {
            return _tiles[neighborPiecePos.x, neighborPiecePos.y].PieceReference;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Checks to see if the Piece position is in bouds, otherwise it returns null
    /// </summary>
    /// <param name="piece"></param>
    /// <returns></returns>
    private bool CheckIfTileExists(Vector2 tilePos)
    {
        if (tilePos.x < 0 || tilePos.y < 0) { return false; }

        if (tilePos.x >= boardWidth || tilePos.y >= boardHeight) { return false; }

        return true;
    }

}
