// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using UnityEngine;

namespace Meta.XR.TrackedKeyboardSample
{
    public class CameraFollower : MonoBehaviour
    {
        [SerializeField] Camera _camera;

        [SerializeField] float _distance = 1;

        [SerializeField]
        [Tooltip("How quickly this object will move to the target position.")]
        [Range(0, 1)]
        float _stiffness = .1f;

        void Update()
        {
            var targetPosition = _camera.transform.position + _camera.transform.forward * _distance;
            var targetRotation = Quaternion.LookRotation(_camera.transform.forward);

            transform.SetPositionAndRotation(
                position: Vector3.Lerp(transform.position, targetPosition, _stiffness),
                rotation: Quaternion.Slerp(transform.rotation, targetRotation, _stiffness));
        }
    }
}
