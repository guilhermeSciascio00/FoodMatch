using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public enum Directions
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}

public enum BoardState
{
    Idle = 0,
    Swapping = 1,
    Destroying = 2,
    Falling = 3,
    Refilling = 4,
    Checking = 5
}

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] int boardHeight;
    [SerializeField] int boardWidth;
    [SerializeField] Tile boardTile;
    [SerializeField] Piece basePiece;

    [Header("References")]
    [SerializeField] GameObject tilesParent;
    [SerializeField] GameObject objectPoolParent;

    [Header("Debug State")]
    [SerializeField] BoardState _boardCurrentState;
    [SerializeField] BoardState _boardLastState;

    //Animations variables
    private int _prePiecesPool = 16;
    private float _spawnAboveOffset = 1.5f;
    private float _swapDuration = 0.2f;
    private float _fallDuration = 0.25f;
    private float _shrinkDuration = 0.2f;
    private Vector3 _defaultLocalScale;

    //Tiles list and pieces pool
    private Tile[,] _tiles;
    private List<Tile> _emptyTopRowTiles;
    private Queue<Piece> _piecesPool;

    //Safe guard variable, so the coroutine isn't called multiple times inside the DestroyMatchFunction

    private bool _isStablizing = false;

    private void Start()
    {
        _boardCurrentState = BoardState.Idle;
        _boardLastState = _boardCurrentState;

        //Arrays, Collections
        _tiles = new Tile[boardHeight, boardWidth];
        _piecesPool = new Queue<Piece>();
        _emptyTopRowTiles = new List<Tile>();

        DOTween.Init();
        DOTween.SetTweensCapacity(800, 500);

        CreateGameBoard();
        CreateAllPieces();
        FillObjectPool();
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
        Tile _tile = Instantiate(boardTile, new Vector3(tilePosition.x, tilePosition.y), Quaternion.identity, tilesParent.transform);

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

        _defaultLocalScale = _piece.transform.localScale;

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

                if(neigbourLeftLeft != currentPiece) { neigbourLeftLeft.SetPiece(); }
                
            }
        }

        if(CheckPieceMatch(currentPiece, neighbourBottomPiece))
        {
            if (CheckPieceMatch(neighbourBottomPiece, CheckNeighbourPiece(neighbourBottomPiece, Directions.Down)))
            {
                Piece neighbourBottomBottom = CheckNeighbourPiece(neighbourBottomPiece, Directions.Down);

                if(neighbourBottomBottom != null){neighbourBottomBottom.SetPiece();}
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

        //Last -> Idle, current -> Swapping
        _boardLastState = _boardCurrentState;
        _boardCurrentState = BoardState.Swapping;

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
            //Safety Guard
            if (currentPiece == null || targetPiece == null)
            {
                _boardCurrentState = BoardState.Idle;
                return;
            }

            targetPiece.transform.parent = currentPiece.transform.parent;
            targetPiece.transform.localPosition = Vector3.zero;

            //changing the current piece tile
            currentPiece.transform.parent = tempParent;
            currentPiece.transform.localPosition = Vector3.zero;


            //Only do this once, when the player swap the pieces
            if (isReversing == false)
            {
                //Last -> Swapping, current -> Checking
                _boardLastState = _boardCurrentState;
                _boardCurrentState = BoardState.Checking;

                horizontalMatches.Add(currentPiece);

                verticalMatches.Add(currentPiece);

                horizontalMatches.AddRange(CheckNeighboursInLine(currentPiece, Directions.Left, Directions.Right));

                verticalMatches.AddRange(CheckNeighboursInLine(currentPiece, Directions.Up, Directions.Down));
                

                if (horizontalMatches.Count >= 3)
                {
                    //Debug.Log("We have a horizontal match");
                    MatchDestroySequence(horizontalMatches);
                }
                else if(verticalMatches.Count >= 3)
                {
                    //Debug.Log("We have a vertical match");
                    MatchDestroySequence(verticalMatches);
                }
                else
                {
                    //Debug.Log("We didn't find any matches");
                    SwapPieces(targetPiece, currentPiece, isReversing:true);
                }
            }
            else
            {
                //LastState -> Swapping, current -> Idle
                _boardLastState = _boardCurrentState;
                _boardCurrentState = BoardState.Idle;
            }

            moveSequence.Kill();
        });
    }


    //Checks all the matches in-line, vertically and horizontally, exclude the center piece. While a match is found, the loop keeps going, until it gets in the border of board or when it doesn't find any matches.
    private List<Piece> CheckNeighboursInLine(Piece centerPiece, Directions directionA, Directions directionB)
    {
        List<Piece> matches = new List<Piece>();
        if (centerPiece == null) return matches; // safe guard

        //The swaped pieces before the match verification run, we will use it to reset the value of the center piece for each direction
        Piece firstCenterPiece = centerPiece;

        while (CheckPieceMatch(centerPiece, CheckNeighbourPiece(centerPiece, directionA)))
        {
            centerPiece = CheckNeighbourPiece(centerPiece, directionA);
            if (centerPiece == null) break;
            matches.Add(centerPiece);
        }
        //Watch out this one
        centerPiece = firstCenterPiece;
        
        while(CheckPieceMatch(centerPiece, CheckNeighbourPiece(centerPiece, directionB)))
        {
            centerPiece = CheckNeighbourPiece(centerPiece, directionB);
            if(centerPiece == null) break;
            matches.Add(centerPiece);
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

    // CASCADE, REFILL, BOARD_STABILIZER()

    private void CascadeManagerV2()
    {
        _emptyTopRowTiles.Clear();

        for (int x = 0; x < boardWidth; x++)
        {
            bool columnChanged = true;

            while (columnChanged)
            {
                columnChanged = false;

                for (int y = 0; y < boardHeight - 1; y++)
                {
                    Tile currentTile = _tiles[x, y];

                    if(currentTile.TileState == TileState.Empty)
                    {
                        int searchY = y + 1;

                        while(searchY < boardHeight && _tiles[x, searchY].TileState == TileState.Empty) 
                        {
                            searchY++;
                        }

                        if (searchY < boardHeight && _tiles[x, searchY].TileState == TileState.HoldingAPiece)
                        {
                            Piece movedPiece = MovePieceDownLogically(_tiles[x, searchY]);
                            if(movedPiece != null)
                            {
                                CascadeEffectVisually(movedPiece);
                                columnChanged = true;
                                //Break is imporant here to re-scan the column from bottom to top, so we don't skip recently created empty tiles in the same loop
                                break;
                            }
                        }
                    }
                }
            }
            Tile tileTop = _tiles[x, boardHeight - 1];
            if (tileTop != null && tileTop.TileState == TileState.Empty)
            {
                if (!_emptyTopRowTiles.Contains(tileTop))
                {
                    _emptyTopRowTiles.Add(tileTop);
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

        if (currentPieceTile == null || currentPieceTile.PieceReference == null) return null;

        int xPos = currentPieceTile.TilePosition.x;
        int yPos = currentPieceTile.TilePosition.y;

        int targetY = yPos - 1;

        while (targetY >= 0 && _tiles[xPos, targetY].TileState == TileState.Empty)
        {
            targetY--;
        }

        targetY = Mathf.Max(targetY + 1, 0);
        Tile downwardTile = _tiles[xPos, targetY];


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

        if(pieceToAnimate == null || pieceToAnimate.CurrentTile == null) return;

        Vector3 targetPos = pieceToAnimate.CurrentTile.transform.position;

        pieceToAnimate.transform.parent = tilesParent.transform;

        Tween t = pieceToAnimate.transform.DOMove(targetPos, _fallDuration).SetEase(Ease.OutQuad);

        t.OnComplete(() => 
        {
            if (pieceToAnimate != null && pieceToAnimate.CurrentTile != null)
            {
                pieceToAnimate.transform.parent = pieceToAnimate.CurrentTile.transform;
                pieceToAnimate.transform.localPosition = Vector3.zero;
            }
            t.Kill();
        });
    }

    private void MatchDestroySequence(List<Piece> matches)
    {
        _boardCurrentState = BoardState.Destroying;

        //Filtering null entries up front and remove duplicates
        List<Piece> cleanMatches = new List<Piece>();
        HashSet<Piece> hasSeen = new HashSet<Piece>();

        foreach(Piece p in matches)
        {
            if (p == null) continue;
            if (p.CurrentTile == null) continue;
            if (hasSeen.Add(p)) cleanMatches.Add(p);
        }

        if (cleanMatches.Count == 0)
        {
            //Nothing to do, but it needs to run to avoid deadlock
            StartCoroutine(StabilizerBoard());
            return;
        }

        Sequence destructionSequence = DOTween.Sequence();
        Vector3 scaleUpOffset = new Vector3(0.2f, 0.2f);

        foreach (Piece piece in cleanMatches)
        {

            Sequence pieceSeq = DOTween.Sequence();

            pieceSeq.Append(piece.transform.DOScale(transform.localScale + scaleUpOffset, _shrinkDuration));

            pieceSeq.Append(piece.transform.DOScale(Vector3.zero, _shrinkDuration));

            destructionSequence.Join(pieceSeq);
        }
       
        destructionSequence.OnComplete(() =>
        { 
            
            //Only do it after all animations are finished
            foreach (Piece piece in cleanMatches)
            {
                if (piece == null) continue;

                if (piece.CurrentTile != null)
                {
                    piece.CurrentTile.PieceReference = null;
                    piece.CurrentTile.TileState = TileState.Empty;
                    piece.CurrentTile = null;
                }

                //Moving destoyed pieces to the pool
                piece.transform.parent = objectPoolParent.transform;
                piece.transform.localPosition = Vector3.zero;

                piece.SetPiece(isPooling: true);
                piece.gameObject.SetActive(false);
                DOTween.Kill(piece);
                _piecesPool.Enqueue(piece);
            }
            destructionSequence.Kill();
            if(!_isStablizing) 
            {
                //Start the board stabilization after the destruction process has ended.
                StartCoroutine(StabilizerBoard());
            }

        });
    }

    private void RefillTheColumn(int columnPosition)
    {

        int topRowIndex = boardHeight - 1;
        Tile topCurrentTile = _tiles[columnPosition, topRowIndex];

        //If the topTile isn't empty neither null, we don't do anything.
        if(topCurrentTile == null || topCurrentTile.TileState != TileState.Empty) return;

        //Creating the refill sequence
        Sequence refillSeq = DOTween.Sequence();

        Tile spawnTile = topCurrentTile;

        while (spawnTile != null && spawnTile.TileState == TileState.Empty)
        {

            if(_piecesPool.Count == 0)
            {
                //shouldn't happen as much, I'll use a debug.log to check it's frequency
                //But in case it happens, we don't get a null error
                Debug.Log("Spawning a new piece, because the pool was empty");
                Piece newP = Instantiate(basePiece, objectPoolParent.transform);
                _piecesPool.Enqueue(newP);
                newP.gameObject.SetActive(false);
            }

            Piece piece = _piecesPool.Dequeue();
            piece.gameObject.SetActive(true);

            //Assigning current tile first, so other systems can access it
            piece.CurrentTile = spawnTile;
            spawnTile.PieceReference = piece;
            spawnTile.TileState = TileState.HoldingAPiece;

            float worldX = spawnTile.TilePosition.x;
            float worldY = (boardHeight - 1) + _spawnAboveOffset;
            Vector3 spawnWordPos = new Vector3(worldX, worldY, 0);

            piece.transform.parent = tilesParent.transform;
            piece.transform.position = spawnWordPos;
            piece.transform.localScale = _defaultLocalScale;

            //Tween down to the tile's world position

            Vector3 targetWordPos = spawnTile.transform.position;
            Tween t = piece.transform.DOMove(targetWordPos, _fallDuration).SetEase(Ease.OutQuad);

            //On complete re-parent to tile
            t.OnComplete(() =>
            {
                if (piece == null || piece.CurrentTile == null) return;
                piece.transform.parent = piece.CurrentTile.transform;
                piece.transform.localPosition = Vector3.zero;
            });

            refillSeq.Join(t);

            //Walk to the tile below to check if it's empty(since we only spawn at the top, gravity->the cascade, should hande the rest
            //But to protect from infite loop, break if spawnTile is already in the bottom
            int spawnY = spawnTile.TilePosition.y;
            if (spawnY - 1 < 0) break;
            spawnTile = _tiles[columnPosition, spawnY - 1];
        }
        //Keep an eye on here
        refillSeq.OnComplete(() => refillSeq.Kill());
    }

    private IEnumerator StabilizerBoard()
    {
        if (_boardCurrentState == BoardState.Falling || _boardCurrentState == BoardState.Refilling || _isStablizing) yield break;

        bool boardStable = false;
        _isStablizing = true;

        while(!boardStable)
        {
            //Cascade(Gravity)
            _emptyTopRowTiles.Clear();
            CascadeManagerV2();

            //Wait Until all animations are finished
            yield return new WaitUntil(() => !DOTween.IsTweening(tilesParent));

            //Refill
            if(_emptyTopRowTiles.Count > 0)
            {
                _boardLastState = _boardCurrentState;
                _boardCurrentState = BoardState.Refilling;

                //Iterate a snapshot of the list
                List<Tile> toRefill = new List<Tile>(_emptyTopRowTiles);
                foreach (Tile emptyTile in toRefill)
                {
                    RefillTheColumn(emptyTile.TilePosition.x);
                }

                //Wait until the Refill tween finish
                yield return new WaitUntil(() => !DOTween.IsTweening(tilesParent));
            }

            //Combo checking
            List<Piece> matches = CheckEntireBoardForMatches();

            if (matches.Count > 0)
            {
                MatchDestroySequence(matches);
                yield return new WaitUntil(() => !DOTween.IsTweening(tilesParent));
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                boardStable = true;
            }
        }

        _boardLastState = _boardCurrentState;
        _boardCurrentState = BoardState.Idle;
        _emptyTopRowTiles.Clear();
        _isStablizing = false;
    }

    private List<Piece> CheckEntireBoardForMatches()
    {
        List<Piece> foundMatches = new List<Piece>();
        HashSet<Piece> uniquePieces = new HashSet<Piece>(); 

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                Piece piece = _tiles[x, y].PieceReference;
                if (piece == null) continue;

                List<Piece> horizontalMatches = CheckNeighboursInLine(piece, Directions.Left, Directions.Right);

                List<Piece> verticalMatches = CheckNeighboursInLine(piece, Directions.Up, Directions.Down);

                if (horizontalMatches.Count >= 2)
                {
                    horizontalMatches.Add(piece);
                    foreach (Piece p in  horizontalMatches)
                    {
                        uniquePieces.Add(p);
                    }
                }

                if (verticalMatches.Count >= 2)
                {
                    verticalMatches.Add(piece);
                    foreach(Piece p in verticalMatches)
                    {
                        uniquePieces.Add(p);
                    }
                }
            }
        }

        foundMatches.AddRange(uniquePieces);
        return foundMatches;
    }

    private void FillObjectPool()
    {
        for (int i = 0; i < _prePiecesPool; i++)
        {
            Piece piece = Instantiate(basePiece, objectPoolParent.transform);
            piece.SetPiece(isPooling: true);
            _piecesPool.Enqueue(piece);
            piece.gameObject.SetActive(false);
        }

        Debug.Log($"Pool prefilled with {_piecesPool.Count} pieces");
    }

    public BoardState GetBoardCurrentState() => _boardCurrentState;
}
