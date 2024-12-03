// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using UnityEngine;
using Meta.XR.MRUtilityKit;

namespace Meta.XR.TrackedKeyboardSample
{
    /// <summary>
    /// Abstract base class for boundary visualization implementations.
    /// </summary>
    public abstract class BoundaryVisual : ScriptableObject
    {
        /// <summary>
        /// Initializes the boundary visual with the provided parameters.
        /// </summary>
        /// <param name="visualizer">The Bounded3DVisualizer instance.</param>
        /// <param name="passthroughLayer">The passthrough layer.</param>
        /// <param name="trackable">The associated MRUKTrackable.</param>
        public abstract void Initialize(Bounded3DVisualizer visualizer, OVRPassthroughLayer passthroughLayer, MRUKTrackable trackable);

        /// <summary>
        /// Updates the visualization based on the trackable's state.
        /// </summary>
        /// <param name="visualizer">The Bounded3DVisualizer instance.</param>
        public abstract void UpdateVisual(Bounded3DVisualizer visualizer);

        /// <summary>
        /// Updates the visualization's visibility.
        /// </summary>
        /// <param name="enable">The visibility state boolean.</param>
        public abstract void UpdateVisibility(Bounded3DVisualizer visualizer, bool enable);
    }
}
