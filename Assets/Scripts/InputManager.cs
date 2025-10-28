using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private GameInputAction _gameInput;

    private InputAction _touchInteraction;
    private InputAction _touchPos;

    private void Awake()
    {
        _gameInput = new GameInputAction();

        _touchInteraction = _gameInput.PlayerInput.TouchInteraction;
        _touchPos = _gameInput.PlayerInput.TouchPosition;
    }

    private void OnEnable()
    {
        _gameInput.Enable();
    }

    private void OnDisable()
    {
        _gameInput.Disable();
    }

    public bool IsTouchPressed()
    {
        return _touchInteraction.IsPressed();
    }

    public Vector2 GetTouchPosition()
    {
        return _touchPos.ReadValue<Vector2>();
    }
}
