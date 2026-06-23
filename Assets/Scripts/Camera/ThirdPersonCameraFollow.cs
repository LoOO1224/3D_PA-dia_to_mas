using UnityEngine;

namespace DiaToMas.CameraSystem
{
    public class ThirdPersonCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new(0f, 4f, -6f);
        [SerializeField] private float _followSpeed = 9f;
        [SerializeField] private float _lookSpeed = 9f;
        [SerializeField] private float _mouseSensitivity = 2.2f;

        private float _yaw;

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            RotateFromInput();
            FollowTarget();
        }

        private void RotateFromInput()
        {
            _yaw += Input.GetAxis("Mouse X") * _mouseSensitivity;
        }

        private void FollowTarget()
        {
            Quaternion rotation = Quaternion.Euler(0f, _yaw, 0f);
            Vector3 targetPosition = _target.position + rotation * _offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, _followSpeed * Time.deltaTime);

            Quaternion lookRotation = Quaternion.LookRotation(_target.position + Vector3.up * 1.4f - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _lookSpeed * Time.deltaTime);
        }
    }
}
