using UnityEngine;

public class PlayerBoardInteraction : MonoBehaviour
{
    [Header("Piece References for debugging")]
    [SerializeField] Piece _currentHoldingPiece;
    [SerializeField] Piece _targetSwapPiece;
    [SerializeField] LayerMask _pieceLayerMask;

    [Header("Objects References")]
    [SerializeField] InputManager _inputManager;
    [SerializeField] Camera _mainCamera;
    [SerializeField] BoardManager _boardManager;

    //Touch Position
    private Vector2 _currentTouchPos = Vector2.zero;

    private Vector2 _startingDraggingPos = Vector2.zero;
    private Vector2 _endDragginPos = Vector2.zero;
    private bool _isDragging = false;
    private float _swapTreshold = .9f;

    private void Update()
    {
        TouchInteraction();
        DragInteraction();
    }

    private void TouchInteraction()
    {
        
        ConvertTouchPosition(_inputManager.GetTouchPosition());

        if (_currentHoldingPiece != null) { return; }

        RaycastHit2D hit = Physics2D.Raycast(_currentTouchPos, Vector2.zero, 0f, _pieceLayerMask);

        if(_inputManager.IsTouchPressed() && hit)
        {
            SetHoldingPieces(hit.transform.GetComponent<Piece>());
            _isDragging = true;
            _startingDraggingPos = _currentTouchPos;
        }

    }

    private void DragInteraction()
    {
        if (_isDragging && _inputManager.WasTouchReleased())
        {
            _endDragginPos = _currentTouchPos;

            _isDragging = false;

            Vector2 dragDifference = _endDragginPos - _startingDraggingPos;

            float dragDistance = Vector2.Distance(_startingDraggingPos, _endDragginPos);

            if (dragDistance > _swapTreshold)
            {
                SetTargetSwapPieceBasedOnDrag(dragDifference.normalized);
                //Invalid Swap
                if(_targetSwapPiece == null)
                {
                    _currentHoldingPiece = null;
                }
            }
            //Invalid Swap
            else
            {
                _currentHoldingPiece = null;
            }
        }
    }

    private void SetTargetSwapPieceBasedOnDrag(Vector2 dragDifference)
    {

        if(Mathf.Abs(dragDifference.x) > Mathf.Abs(dragDifference.y))
        {

            if (dragDifference.x >= _swapTreshold)
            {
                _targetSwapPiece = _boardManager.CheckNeighbourPiece(_currentHoldingPiece, Directions.Right);
            }
            else if (dragDifference.x < -_swapTreshold)
            {
                _targetSwapPiece = _boardManager.CheckNeighbourPiece(_currentHoldingPiece, Directions.Left);
            }

        }
        else
        {

            if (dragDifference.y > _swapTreshold)
            {
                _targetSwapPiece = _boardManager.CheckNeighbourPiece(_currentHoldingPiece, Directions.Up);
            }
            else if (dragDifference.y < -_swapTreshold)
            {
                _targetSwapPiece = _boardManager.CheckNeighbourPiece(_currentHoldingPiece, Directions.Down);
            }

        }

        if(_currentHoldingPiece != null && _targetSwapPiece != null) 
        {
            _boardManager.SwapPieces(_currentHoldingPiece, _targetSwapPiece, isReversing:false);
            _currentHoldingPiece = null;
            _targetSwapPiece = null;
        }

    }



    private void ConvertTouchPosition(Vector2 touchPos)
    {
       _currentTouchPos = _mainCamera.ScreenToWorldPoint(touchPos);
    }

    private void SetHoldingPieces(Piece piece)
    {
        if(_currentHoldingPiece == null)
        {
            _currentHoldingPiece = piece;
        }
    }

}
