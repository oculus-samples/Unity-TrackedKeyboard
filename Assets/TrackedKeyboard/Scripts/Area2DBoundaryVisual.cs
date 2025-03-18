// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using UnityEngine;
using Meta.XR.MRUtilityKit;

namespace Meta.XR.TrackedKeyboardSample
{
    [CreateAssetMenu(fileName = "Area2DVisual", menuName = "Meta/BoundaryVisual", order = 1)]
    public class Area2DBoundaryVisual : BoundaryVisual
    {
        [SerializeField] private GameObject _quadPrefab;
        [SerializeField] private Texture2D _boundaryTexture;
        [SerializeField] private Color _tintColor = Color.white;

        private GameObject _visualInstance;
        private Material _material;

        public override void Initialize(Bounded2DVisualizer visualizer, OVRPassthroughLayer passthroughLayer, MRUKTrackable trackable)
        {
            if (_quadPrefab == null)
            {
                Debug.LogError("Quad prefab not assigned!");
                return;
            }

            CleanupVisual();

            CreateVisualFromPrefab(visualizer.transform, trackable);

            Debug.Log($"TextureBoundaryVisual initialized. Instance active: {_visualInstance.activeSelf}, " +
                      $"Position: {_visualInstance.transform.localPosition}, " +
                      $"Scale: {_visualInstance.transform.localScale}");
        }

        public override void UpdateVisual(Bounded2DVisualizer visualizer)
        {
            if (_material != null)
            {
                _material.SetColor("_Color", new Color(_tintColor.r, _tintColor.g, _tintColor.b, _tintColor.a));
            }
        }

        public override void UpdateVisibility(Bounded2DVisualizer visualizer, bool enable)
        {
            if (_visualInstance != null)
            {
                _visualInstance.SetActive(enable);
            }
        }

        private void CreateVisualFromPrefab(Transform parent, MRUKTrackable trackable)
        {
            var bounds = trackable.VolumeBounds.Value;

            _visualInstance = Instantiate(_quadPrefab, parent);
            _visualInstance.name = "BoundaryVisualInstance";

            _visualInstance.transform.localScale = new Vector3(
                bounds.size.x,
                -0.01f, // small offset to avoid z-fighting
                bounds.size.y
            );

            _visualInstance.transform.localPosition = Vector3.zero;
            _visualInstance.transform.localRotation = _quadPrefab.transform.rotation;

            // Cache the material if needed for color updates
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
