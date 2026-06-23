using DiaToMas.Data;
using DiaToMas.Managers;
using UnityEngine;

namespace DiaToMas.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private LayerMask _groundLayerMask = ~0;

        private bool _isGrounded;
        private Vector3 _moveDirection;

        public bool IsGrounded => _isGrounded;
        public bool IsRunning => _inputReader != null && _inputReader.IsRunHeld && _moveDirection.sqrMagnitude > 0.001f;
        public float VerticalSpeed => _rigidbody != null ? _rigidbody.linearVelocity.y : 0f;
        public float MoveAmount => _moveDirection.magnitude;

        private void Awake()
        {
            _rigidbody = _rigidbody != null ? _rigidbody : GetComponent<Rigidbody>();
            _inputReader = _inputReader != null ? _inputReader : GetComponent<PlayerInputReader>();
            _cameraTransform = _cameraTransform != null && _cameraTransform != transform ? _cameraTransform : Camera.main?.transform;
            _groundCheckPoint = _groundCheckPoint != null ? _groundCheckPoint : transform;
            _rigidbody.freezeRotation = true;
        }

        private void FixedUpdate()
        {
            CheckGrounded();

            if (GameManager.Inst != null && GameManager.Inst.IsPlayerInputLocked)
            {
                StopHorizontalMovement();
                return;
            }

            Move();
        }

        public void SetCameraTransform(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        private void Move()
        {
            PlayerMovementData movementData = GetMovementData();
            Vector3 cameraForward = GetPlanarCameraForward();
            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);
            _moveDirection = cameraForward * _inputReader.MoveInput.y + cameraRight * _inputReader.MoveInput.x;
            _moveDirection = Vector3.ClampMagnitude(_moveDirection, 1f);

            float speed = movementData.moveSpeed * (_inputReader.IsRunHeld ? movementData.runMultiplier : 1f);
            Vector3 velocity = _moveDirection * speed;
            velocity.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = velocity;

            if (_moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 12f * Time.fixedDeltaTime);
            }
        }

        private void StopHorizontalMovement()
        {
            _moveDirection = Vector3.zero;
            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.x = 0f;
            velocity.z = 0f;
            _rigidbody.linearVelocity = velocity;
        }

        private void CheckGrounded()
        {
            PlayerMovementData movementData = GetMovementData();
            _isGrounded = Physics.CheckSphere(
                _groundCheckPoint.position,
                movementData.groundCheckRadius,
                _groundLayerMask,
                QueryTriggerInteraction.Ignore);
        }

        private PlayerMovementData GetMovementData()
        {
            return GameManager.Inst != null && GameManager.Inst.GameDataManager.PlayerMovementData != null
                ? GameManager.Inst.GameDataManager.PlayerMovementData
                : new PlayerMovementData
                {
                    moveSpeed = 4.5f,
                    runMultiplier = 1.5f,
                    groundCheckRadius = 0.28f
                };
        }

        private Vector3 GetPlanarCameraForward()
        {
            if (_cameraTransform == null)
            {
                return Vector3.forward;
            }

            Vector3 forward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up);
            return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
        }
    }
}
