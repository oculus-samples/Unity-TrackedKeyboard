// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace Meta.XR.TrackedKeyboardSample
{
    /// <summary>
    /// Visualizes the bounded 3D area for the tracked keyboard.
    /// </summary>
    public class Bounded2DVisualizer : MonoBehaviour
    {

        [SerializeField]
        private Area2DBoundaryVisual _2DVisual;
        [SerializeField, Tooltip("Boundary visual implementation.")]
        private BoundaryVisual _boundaryVisual;
        [SerializeField, Tooltip("Transform to apply the keyboard's position and scale.")]
        private Transform _boxTransform;
        [SerializeField, Tooltip("Line renderer for visualizing boundaries.")]
        private LineRenderer _lineRenderer;
        [SerializeField, Range(1f, 1.5f), Tooltip("Scaling factor for the trackable box colliders X axis. This defines the hand detection range and does not need to be changed in most cases.")]
        private float _colliderScaleX = 1.2f;
        [SerializeField, Range(2f, 4f), Tooltip("Scaling factor for the trackable box colliders Z axis. This defines the hand detection range and does not need to be changed in most cases.")]
        private float _colliderScaleZ = 3f;

        [SerializeField, Range(1f, 1.3f), Tooltip("Scaling factor for the passthrough cutout width (X axis)")]
        private float _passthroughScaleX = 1.05f;
        [SerializeField, Range(1f, 1.3f), Tooltip("Scaling factor for the passthrough cutout height (Y axis)")]
        private float _passthroughScaleY = 1.05f;
        [SerializeField, Range(1f, 1.2f), Tooltip("Scaling factor for the passthrough cutout depth (Z axis)")]
        private float _passthroughScaleZ = 1.05f;

        public LineRenderer LineRenderer => _lineRenderer;
        public BoxCollider BoxCollider => _boxCollider;

        private MRUKTrackable _trackable;
        private OVRPassthroughLayer _passthroughLayer;
        private readonly HashSet<string> _logOnce = new HashSet<string>();
        private bool _isBoundaryVisualEnabled = true;
        private bool _isHoverActive = false;
        private BoxCollider _boxCollider;

        /// <summary>
        /// Logs a message only once.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        private void LogOnce(string msg)
        {
            if (_logOnce.Add(msg))
            {
                Debug.Log(msg);
            }
        }

        private void Update()
        {
            if (_passthroughLayer)
            {
                if (_boxTransform)
                {
                    // Only draw the box when using surface projected passthrough
                    _boxTransform.gameObject.SetActive(_passthroughLayer.isActiveAndEnabled);
                }
            }

            // Update the visual if necessary
            _boundaryVisual?.UpdateVisual(this);
        }

        /// <summary>
        /// Initializes the visualizer with the given passthrough layer and trackable.
        /// </summary>
        /// <param name="passthroughLayer">The passthrough layer to use.</param>
        /// <param name="trackable">The MRUKTrackable to visualize.</param>
        /// <param name="boundaryVisual">The boundary visual implementation.</param>
        public void Initialize(OVRPassthroughLayer passthroughLayer, MRUKTrackable trackable, BoundaryVisual boundaryVisual)
        {
            if (trackable == null)
                throw new ArgumentNullException(nameof(trackable));

            _passthroughLayer = passthroughLayer;
            _trackable = trackable;
            _boundaryVisual = boundaryVisual;

            if (!_trackable.VolumeBounds.HasValue)
            {
                LogOnce($"Trackable {_trackable} has no Bounded3D component. Ignoring.");
                return;
            }

            var box = _trackable.VolumeBounds.Value;
            LogOnce($"Bounded3D volume: {box}");

            _boxCollider = GetComponent<BoxCollider>();
            if (_boxCollider == null)
            {
                Debug.LogWarning("BoxCollider component is missing on Bounded3DVisualizer.");
                return;
            }

            _boxCollider.size = new Vector3(box.size.x * _colliderScaleX, box.size.y, box.size.z * _colliderScaleZ);

            _2DVisual?.Initialize(this, _passthroughLayer, _trackable);
            _2DVisual?.UpdateVisibility(this, !_isHoverActive && _isBoundaryVisualEnabled);

            if (_boxTransform != null)
            {
                Vector3 passthroughScale = new Vector3(
                    box.size.x * (_passthroughScaleX),
                    box.size.y * (_passthroughScaleY),
                    box.size.z * (_passthroughScaleZ)
                );

                _boxTransform.localScale = passthroughScale;

                var meshFilter = _boxTransform.GetComponentInChildren<MeshFilter>();
                if (meshFilter)
                {
                    _passthroughLayer.AddSurfaceGeometry(meshFilter.gameObject, true);
                }
                else
                {
                    Debug.LogWarning($"BoxTransform '{_boxTransform.name}' has no MeshFilter. Ignoring passthrough layer.");
                }
            }
            else
            {
                Debug.LogWarning("BoxTransform is not set; ignoring passthrough layer.");
            }
        }

        /// <summary>
        /// Called by UI button to explicitly enable/disable the boundary.
        /// </summary>
        /// <param name="enable"></param>
        public void SetUserEnabled(bool enable)
        {
            _isBoundaryVisualEnabled = enable;
            UpdateVisibility();
        }

        /// <summary>
        /// Called by hover detection events to show/hide based on hand proximity.
        /// Show the boundary when not hovering and button is enabled.
        /// </summary>
        /// <param name="isHovering"></param>
        public void SetHoverState(bool isHovering)
        {
            _isHoverActive = isHovering;

            if (_isBoundaryVisualEnabled)
            {
                UpdateVisibility();
            }
        }

        /// <summary>
        /// Show the boundary visual if button is enabled, and user is not hovering.
        /// </summary>
        private void UpdateVisibility()
        {
            bool shouldShow = _isBoundaryVisualEnabled && !_isHoverActive;
            _2DVisual?.UpdateVisibility(this, shouldShow);
        }
    }
}
