using DiaToMas.Managers;
using UnityEngine;

namespace DiaToMas.CameraSystem
{
    public class ThirdPersonCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _targetOffset = new(0f, 1.45f, 0f);
        [SerializeField] private float _distance = 6f;
        [SerializeField] private float _minDistance = 3.2f;
        [SerializeField] private float _maxDistance = 8f;
        [SerializeField] private float _followSpeed = 12f;
        [SerializeField] private float _lookSpeed = 14f;
        [SerializeField] private float _mouseSensitivity = 2.4f;
        [SerializeField] private float _zoomSensitivity = 1.4f;
        [SerializeField] private float _minPitch = -8f;
        [SerializeField] private float _maxPitch = 48f;
        [SerializeField] private float _collisionRadius = 0.28f;
        [SerializeField] private LayerMask _obstacleLayerMask = ~0;

        private float _yaw;
        private float _pitch = 24f;

        public void SetTarget(Transform target)
        {
            _target = target;

            if (_target != null)
            {
                _yaw = _target.eulerAngles.y;
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            if (GameManager.Inst == null || !GameManager.Inst.IsPlayerInputLocked)
            {
                RotateFromInput();
                ZoomFromInput();
            }

            FollowTarget();
        }

        private void RotateFromInput()
        {
            _yaw += Input.GetAxis("Mouse X") * _mouseSensitivity;
            _pitch -= Input.GetAxis("Mouse Y") * _mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        }

        private void ZoomFromInput()
        {
            float scrollValue = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollValue) <= 0.001f)
            {
                return;
            }

            _distance = Mathf.Clamp(_distance - scrollValue * _zoomSensitivity, _minDistance, _maxDistance);
        }

        private void FollowTarget()
        {
            Vector3 lookPoint = _target.position + _targetOffset;
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 cameraDirection = rotation * Vector3.back;
            float resolvedDistance = ResolveCameraDistance(lookPoint, cameraDirection);
            Vector3 targetPosition = lookPoint + cameraDirection * resolvedDistance;

            transform.position = Vector3.Lerp(transform.position, targetPosition, _followSpeed * Time.deltaTime);

            Quaternion lookRotation = Quaternion.LookRotation(lookPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _lookSpeed * Time.deltaTime);
        }

        private float ResolveCameraDistance(Vector3 lookPoint, Vector3 cameraDirection)
        {
            if (Physics.SphereCast(
                    lookPoint,
                    _collisionRadius,
                    cameraDirection,
                    out RaycastHit hit,
                    _distance,
                    _obstacleLayerMask,
                    QueryTriggerInteraction.Ignore))
            {
                return Mathf.Clamp(hit.distance - _collisionRadius, _minDistance, _distance);
            }

            return _distance;
        }
    }
}
