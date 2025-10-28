using UnityEngine;

public class PlayerBoardInteraction : MonoBehaviour
{
    [Header("Piece References for debugging")]
    [SerializeField] Piece _currentHoldingPiece;
    [SerializeField] Piece _targetSwapPiece;

    [Header("Objects References")]
    [SerializeField] InputManager _inputManager;
    [SerializeField] Camera _mainCamera;

    //Touch Position
    private Vector2 _currentTouchPos = Vector2.zero;

    private void Update()
    {
        TouchInteraction();
    }

    private void TouchInteraction()
    {
        if (_inputManager.IsTouchPressed())
        {
            ConvertTouchPosition(_inputManager.GetTouchPosition());
            
            RaycastHit2D hit = Physics2D.Raycast(_currentTouchPos, Vector2.zero);

            Debug.Log(hit);
        }
    }

    private void ConvertTouchPosition(Vector2 touchPos)
    {
       _currentTouchPos = _mainCamera.ScreenToWorldPoint(touchPos);
    }

}
