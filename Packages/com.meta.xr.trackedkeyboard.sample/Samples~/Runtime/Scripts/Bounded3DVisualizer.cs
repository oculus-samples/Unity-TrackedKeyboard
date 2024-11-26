// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.XR.TrackedKeyboardSample
{
    public enum BoundaryVisualizationMode
    {
        TwoD,
        ThreeD
    }
    /// <summary>
    /// Visualizes the bounded 3D area for the tracked keyboard.
    /// </summary>
    public class Bounded3DVisualizer : MonoBehaviour
    {
        [SerializeField]
        private BoundaryVisualizationMode _visualizationMode = BoundaryVisualizationMode.TwoD;
        [SerializeField]
        private Area2DBoundaryVisual _2DVisual;
        [SerializeField]
        private Area3DBoundaryVisual _3DVisual;

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
        public LineRenderer LineRenderer => _lineRenderer;
        public BoundaryVisualizationMode CurrentMode => _visualizationMode;
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
            _2DVisual?.UpdateVisibility(this, _visualizationMode == BoundaryVisualizationMode.TwoD && !_isHoverActive && _isBoundaryVisualEnabled);
            _3DVisual?.Initialize(this, _passthroughLayer, _trackable);
            _3DVisual?.UpdateVisibility(this, _visualizationMode == BoundaryVisualizationMode.ThreeD && !_isHoverActive && _isBoundaryVisualEnabled);

            if (_boxTransform != null)
            {
                _boxTransform.localScale = box.size;

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
        /// Sets the current visualization mode for the boundary visual.
        /// </summary>
        /// <param name="mode">The mode to set.</param>
        public void SetVisualizationMode(BoundaryVisualizationMode mode)
        {
            if (_visualizationMode == mode)
            {
                return;
            }

            _visualizationMode = mode;
            bool shouldShow = !_isHoverActive && _isBoundaryVisualEnabled;

            _2DVisual?.UpdateVisibility(this, mode == BoundaryVisualizationMode.TwoD && shouldShow);
            _3DVisual?.UpdateVisibility(this, mode == BoundaryVisualizationMode.ThreeD && shouldShow);
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

        private BoundaryVisual GetActiveVisual()
        {
            return _visualizationMode == BoundaryVisualizationMode.TwoD ? _2DVisual : _3DVisual;
        }

        /// <summary>
        /// Show the boundary visual if button is enabled, and user is not hovering.
        /// </summary>
        private void UpdateVisibility()
        {
            bool shouldShow = _isBoundaryVisualEnabled && !_isHoverActive;

            if (_visualizationMode == BoundaryVisualizationMode.TwoD)
            {
                _2DVisual?.UpdateVisibility(this, shouldShow);
                _3DVisual?.UpdateVisibility(this, false);
            }
            else
            {
                _3DVisual?.UpdateVisibility(this, shouldShow);
                _2DVisual?.UpdateVisibility(this, false);
            }
        }
    }
}
