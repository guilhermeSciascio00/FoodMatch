using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum Directions
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

    private bool _isSwapping = false;
    private float _swapDuration = 0.2f;
    private float _shrinkDuration = 0.2f;

    [SerializeField] bool checkEmptyFlag = false;

    private void Start()
    {
        _tiles = new Tile[boardHeight, boardWidth];
        DOTween.Init();
        CreateGameBoard();
        CreateAllPieces();
    }

    private void Update()
    {
        if (checkEmptyFlag)
        {
            CascadeManagerV2();
            checkEmptyFlag = false;
        }

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
        //WARNING : There's a small change of the pieces already form in thet beginning, proabably, because I'm only checking left and down and not all direction, I'll fix this later, since isn't so hard and won't break anything at all
        //At the moment, for ten board rolls, 2 or 1 might appear with a 3 match
        Piece neighbourLeftPiece = CheckNeighbourPiece(currentPiece, Directions.Left);

        Piece neighbourBottomPiece = CheckNeighbourPiece(currentPiece, Directions.Down);

        if(CheckPieceMatch(currentPiece, neighbourLeftPiece))
        {
            if(CheckPieceMatch(neighbourLeftPiece, CheckNeighbourPiece(neighbourLeftPiece, Directions.Left)))
            {
                Piece neigbourLeftLeft = CheckNeighbourPiece(neighbourLeftPiece, Directions.Left);
                neigbourLeftLeft.SetPiece();
            }
        }

        if(CheckPieceMatch(currentPiece, neighbourBottomPiece))
        {
            if (CheckPieceMatch(neighbourBottomPiece, CheckNeighbourPiece(neighbourBottomPiece, Directions.Down)))
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
    private bool CheckPieceMatch(Piece piece1, Piece piece2)
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
    public Piece CheckNeighbourPiece(Piece pieceToCheck, Directions directionToCheck)
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

    public void SwapPieces(Piece currentPiece, Piece targetPiece, bool isReversing)
    {
        SwapPiecesLogically(currentPiece, targetPiece);
        SwapPiecesVisually(currentPiece, targetPiece, isReversing);
    }

    private void SwapPiecesLogically(Piece currentPiece, Piece targetPiece)
    {
        if (currentPiece == null || targetPiece == null) return;

        Vector2Int currentPiecePos = (Vector2Int)currentPiece.CurrentTile.TilePosition;

        Vector2Int targetPiecePos = (Vector2Int)targetPiece.CurrentTile.TilePosition;

        //Target Piece temp
        Piece tempPiece = _tiles[targetPiecePos.x, targetPiecePos.y].PieceReference;

        //Target Tile temp
        Tile tempTile = targetPiece.CurrentTile;

        //Changing the target piece Logically
        _tiles[targetPiecePos.x, targetPiecePos.y].PieceReference = _tiles[currentPiecePos.x, currentPiecePos.y].PieceReference;

        //Changing the target tile logically
        targetPiece.CurrentTile = currentPiece.CurrentTile;

        //Changing the current piece logically
        _tiles[currentPiecePos.x, currentPiecePos.y].PieceReference = tempPiece;

        //changing the current piece tile
        currentPiece.CurrentTile = tempTile;
    }

    private void SwapPiecesVisually(Piece currentPiece, Piece targetPiece, bool isReversing)
    {
        if (currentPiece == null || targetPiece == null) return;

        //W = World
        Vector2 startCurrentPieceWPos = currentPiece.transform.position;

        Vector2 startTargetWPiece = targetPiece.transform.position;

        List<Piece> horizontalMatches = new List<Piece>();
        List<Piece> verticalMatches = new List<Piece>();

        Transform tempParent = targetPiece.transform.parent;

        Sequence moveSequence = DOTween.Sequence();

        moveSequence.Append(currentPiece.transform.DOMove(startTargetWPiece, _swapDuration));
        moveSequence.Join(targetPiece.transform.DOMove(startCurrentPieceWPos, _swapDuration));

        moveSequence.OnComplete(() => 
        {
            targetPiece.transform.parent = currentPiece.transform.parent;
            targetPiece.transform.localPosition = Vector3.zero;

            //changing the current piece tile
            currentPiece.transform.parent = tempParent;
            currentPiece.transform.localPosition = Vector3.zero;


            //Only do this once, when the player swap the pieces
            if (isReversing == false)
            {
                horizontalMatches.Add(currentPiece);

                verticalMatches.Add(currentPiece);

                horizontalMatches.AddRange(CheckNeighboursInLine(currentPiece, Directions.Left, Directions.Right));

                verticalMatches.AddRange(CheckNeighboursInLine(currentPiece, Directions.Up, Directions.Down));
                

                if (horizontalMatches.Count >= 3)
                {
                    Debug.Log("We have a horizontal match");
                    MatchDestroySequence(horizontalMatches);
                }
                else if(verticalMatches.Count >= 3)
                {
                    Debug.Log("We have a vertical match");
                    MatchDestroySequence(verticalMatches);
                }
                else
                {
                    Debug.Log("We didn't find any matches");
                    SwapPieces(targetPiece, currentPiece, isReversing:true);
                }
            }
            else
            {
                _isSwapping = false;
            }
        });
    }


    //Checks all the matches in-line, vertically and horizontally, exclude the center piece. While a match is found, the loop keeps going, until it gets in the border of board or when it doesn't find any matches.
    private List<Piece> CheckNeighboursInLine(Piece centerPiece, Directions directionA, Directions directionB)
    {
        List<Piece> matches = new List<Piece>();
        //The swaped pieces before the match verification run, we will use it to reset the value of the center piece for each direction
        Piece firstCenterPiece = centerPiece;

        while (CheckPieceMatch(centerPiece, CheckNeighbourPiece(centerPiece, directionA)))
        {
            centerPiece = CheckNeighbourPiece(centerPiece, directionA);
            matches.Add(centerPiece);
        }

        centerPiece = firstCenterPiece;
        
        while(CheckPieceMatch(centerPiece, CheckNeighbourPiece(centerPiece, directionB)))
        {
            centerPiece = CheckNeighbourPiece(centerPiece, directionB);
            matches.Add(centerPiece);
        }

        //Debug Purposes
        foreach (Piece match in matches)
        {
            Debug.Log(match.CurrentTile);
        }
        return matches;
    }

    /// <summary>
    /// Checks to see if the Piece position is in bouds, otherwise it returns null
    /// </summary>
    /// <param name="piece"></param>
    /// <returns></returns>
    private bool CheckIfTileExists(Vector2Int tilePos)
    {
        if (tilePos.x < 0 || tilePos.y < 0) { return false; }

        if (tilePos.x >= boardWidth || tilePos.y >= boardHeight) { return false; }

        return true;
    }

    private void CascadeManagerV2()
    {
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if(_tiles[x,y].TileState == TileState.Empty)
                {
                    int searchY = y + 1;

                    while(searchY < boardHeight && _tiles[x, searchY].TileState == TileState.Empty)
                    {
                        searchY++;
                    }

                    if (searchY < boardHeight && _tiles[x, searchY].TileState == TileState.HoldingAPiece)
                    {
                        CascadeEffectVisually(MovePieceDownLogically(_tiles[x, searchY]));
                    }

                    else if(searchY >= boardHeight)
                    {
                        Debug.Log($"A refill is needed in the column {x}");
                    }
                }
            }
        }
    }


    /// <summary>
    /// Moves the Piece Down logically, and returns the moved piece, so it's easy to animate 
    /// </summary>
    /// <param name="currentPieceTile"></param>
    private Piece MovePieceDownLogically(Tile currentPieceTile)
    {

        int searchY = currentPieceTile.TilePosition.y - 1;

        

        while (searchY >= 0 && _tiles[currentPieceTile.TilePosition.x, searchY].TileState == TileState.Empty)
        {
            searchY--;
        }

        int targetY = Mathf.Max(searchY + 1, 0);
        Tile downwardTile = _tiles[currentPieceTile.TilePosition.x, targetY];

        //if(searchY > 0 && _tiles[currentPieceTile.TilePosition.x, searchY].TileState == TileState.HoldingAPiece)
        //{
        //    downwardTile = CheckIfTileExists(new Vector2Int(currentPieceTile.TilePosition.x, searchY + 1)) ? _tiles[currentPieceTile.TilePosition.x, searchY + 1] : null;
        //}


        if (downwardTile != null && downwardTile.TileState == TileState.Empty)
        {
            Piece pieceToMove = currentPieceTile.PieceReference;

            pieceToMove.CurrentTile = downwardTile;

            downwardTile.PieceReference = pieceToMove;
            downwardTile.TileState = TileState.HoldingAPiece;

            currentPieceTile.PieceReference = null;
            currentPieceTile.TileState = TileState.Empty;

            return downwardTile.PieceReference;
        }
        return null;
    }

    private void CascadeEffectVisually(Piece pieceToAnimate)
    {
        Vector2 targetPos = pieceToAnimate.CurrentTile.transform.position;

        Sequence moveSequence = DOTween.Sequence();

        moveSequence.Join(pieceToAnimate.transform.DOMove(targetPos, .2f));

        moveSequence.OnComplete(() => 
        {
            pieceToAnimate.transform.parent = pieceToAnimate.CurrentTile.transform;
            pieceToAnimate.transform.localPosition = Vector3.zero;
        });
    }

    private void MatchDestroySequence(List<Piece> matches)
    {
        Sequence destructionSequence = DOTween.Sequence();
        Vector3 scaleUpOffset = new Vector3(0.2f, 0.2f);

        foreach (Piece piece in matches)
        {
            Sequence pieceSeq = DOTween.Sequence();

            pieceSeq.Append(piece.transform.DOScale(transform.localScale + scaleUpOffset, _shrinkDuration));

            pieceSeq.Append(piece.transform.DOScale(Vector3.zero, _shrinkDuration));

            destructionSequence.Join(pieceSeq);
        }
       
        destructionSequence.OnComplete(() =>
        { foreach (Piece piece in matches)
            {
                
                piece.CurrentTile.PieceReference = null;

                piece.CurrentTile.TileState = TileState.Empty;

                piece.CurrentTile = null;

                piece.gameObject.SetActive(false);

                _isSwapping = false;
            }
        }
        );
    }

    public bool GetIsSwapping() => _isSwapping;
    public void SetIsSwapping() => _isSwapping = true;
}
