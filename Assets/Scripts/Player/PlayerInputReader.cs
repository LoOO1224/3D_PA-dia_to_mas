using DiaToMas.Managers;
using UnityEngine;

namespace DiaToMas.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        private Vector2 _moveInput;
        private bool _isInteractPressed;
        private bool _isRunHeld;

        public Vector2 MoveInput => _moveInput;
        public bool IsInteractPressed => _isInteractPressed;
        public bool IsRunHeld => _isRunHeld;

        private void Update()
        {
            if (GameManager.Inst != null && GameManager.Inst.IsPlayerInputLocked)
            {
                ClearInput();
                return;
            }

            ReadMoveInput();
            ReadActionInput();
        }

        private void ReadMoveInput()
        {
            _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            _moveInput = Vector2.ClampMagnitude(_moveInput, 1f);
        }

        private void ReadActionInput()
        {
            _isInteractPressed = Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
            _isRunHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private void ClearInput()
        {
            _moveInput = Vector2.zero;
            _isInteractPressed = false;
            _isRunHeld = false;
        }

        public bool ConsumeInteract()
        {
            if (!_isInteractPressed)
            {
                return false;
            }

            _isInteractPressed = false;
            return true;
        }
    }
}
