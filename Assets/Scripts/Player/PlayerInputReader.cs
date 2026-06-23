using UnityEngine;

namespace DiaToMas.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        private Vector2 _moveInput;
        private bool _isJumpPressed;
        private bool _isInteractPressed;
        private bool _isRunHeld;

        public Vector2 MoveInput => _moveInput;
        public bool IsJumpPressed => _isJumpPressed;
        public bool IsInteractPressed => _isInteractPressed;
        public bool IsRunHeld => _isRunHeld;

        private void Update()
        {
            ReadMoveInput();
            ReadActionInput();
        }

        private void LateUpdate()
        {
            _isJumpPressed = false;
            _isInteractPressed = false;
        }

        private void ReadMoveInput()
        {
            _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            _moveInput = Vector2.ClampMagnitude(_moveInput, 1f);
        }

        private void ReadActionInput()
        {
            _isJumpPressed = Input.GetButtonDown("Jump");
            _isInteractPressed = Input.GetKeyDown(KeyCode.E);
            _isRunHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
    }
}
