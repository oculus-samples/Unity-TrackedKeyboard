// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using UnityEngine;
using Meta.XR.MRUtilityKit;

namespace Meta.XR.TrackedKeyboardSample
{
    [CreateAssetMenu(fileName = "Area3DVisual", menuName = "Meta/BoundaryVisual", order = 2)]
    public class Area3DBoundaryVisual : BoundaryVisual
    {
        [SerializeField] private GameObject _roundedAreaPrefab;
        [SerializeField] private Color _tintColor = Color.white;

        private GameObject _visualInstance;
        private Vector3 _originalLocalScale;
        private Material _material;
        private float _paddingFactorZ = 1.15f;

        public override void Initialize(Bounded3DVisualizer visualizer, OVRPassthroughLayer passthroughLayer,
            MRUKTrackable trackable)
        {
            if (_roundedAreaPrefab == null)
            {
                Debug.LogError("Rounded area prefab not assigned!");
                return;
            }

            CleanupVisual();

            CreateVisualFromPrefab(visualizer.transform, trackable);
        }

        public override void UpdateVisual(Bounded3DVisualizer visualizer)
        {
            if (_material != null)
            {
                _material.SetColor("_Color", new Color(_tintColor.r, _tintColor.g, _tintColor.b, _tintColor.a));
            }
        }

        public override void UpdateVisibility(Bounded3DVisualizer visualizer, bool enable)
        {
            if (_visualInstance != null)
            {
                _visualInstance.SetActive(enable);
            }
        }

        private void CreateVisualFromPrefab(Transform parent, MRUKTrackable trackable)
        {
            var bounds = trackable.VolumeBounds.Value;

            _visualInstance = Instantiate(_roundedAreaPrefab, parent);
            _visualInstance.name = "BoundaryVisualInstance";
            _originalLocalScale = _roundedAreaPrefab.transform.localScale;

            Mesh mesh = _visualInstance.GetComponent<MeshFilter>().mesh;
            Bounds originalBounds = mesh.bounds;

            float scaleX = bounds.size.x / (originalBounds.size.x * _originalLocalScale.x);
            float scaleZ = bounds.size.y / (originalBounds.size.z * _originalLocalScale.z);

            _visualInstance.transform.localScale = new Vector3(_originalLocalScale.x * scaleX,
                _originalLocalScale.y, _originalLocalScale.z * scaleZ * _paddingFactorZ);

            _visualInstance.transform.localPosition = Vector3.zero;
            _visualInstance.transform.localRotation = _roundedAreaPrefab.transform.rotation;

            var renderer = _visualInstance.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                _material = renderer.material;
                UpdateVisual(null);
            }

            _visualInstance.SetActive(true);
        }

        private void CleanupVisual()
        {
            if (_visualInstance != null)
            {
                if (Application.isPlaying)
                    Destroy(_visualInstance);
                else
                    DestroyImmediate(_visualInstance);
            }
        }

        private void OnDestroy()
        {
            CleanupVisual();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_material != null)
            {
                _material.SetColor("_Color", _tintColor);
            }
        }
#endif
    }
}
