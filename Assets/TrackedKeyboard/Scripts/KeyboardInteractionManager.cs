// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

namespace Meta.XR.TrackedKeyboardSample
{
    /// <summary>
    /// Manages interactions between hands and a tracked keyboard. This class handles proximity detection, hand visibility, pass through opacity and hover events.
    /// It provides an interface between the user's hands and the tracked keyboard, adjusting visuals and triggering hover events based on hand proximity.
    /// </summary>
    public class KeyboardInteractionManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Event triggered when a hand starts hovering over the keyboard.")]
        public UnityEvent OnHoverStart;
        [SerializeField, Tooltip("Event triggered when a hand stops hovering over the keyboard.")]
        public UnityEvent OnHoverEnd;
        [SerializeField, Range(0.2f, 0.6f), Tooltip("Hand penetration distance for the keyboard collider. The normalized opacity value of the hands and keyboard is calculated relative to this distance for a fade effect. Lower values will result in an abrupt fade")]
        private float _maxDistance = 0.4f;

        private const float FadeMultiplier = 5f;
        private const float HandOpacity = 0.8f;
        private const float HandOutlineOpacity = 0.4f;
        private const int LeftHandIndex = 0;
        private const int RightHandIndex = 1;

        private Collider _keyboardCollider;
        private bool _isHovering;
        private bool _isBeingDisabled;
        private readonly HandState[] _handStates = new HandState[2];
        private static readonly int OutlineOpacity = Shader.PropertyToID("_OutlineOpacity");
        private static readonly int Opacity = Shader.PropertyToID("_Opacity");

        /// <summary>
        /// Visual representation of the left hand.
        /// </summary>
        public HandVisual LeftHandVisual { get; set; }
        /// <summary>
        /// Visual representation of the right hand.
        /// </summary>
        public HandVisual RightHandVisual { get; set; }
        /// <summary>
        /// Left hand's ray interactor.
        /// </summary>
        public RayInteractor LeftRayInteractor { get; set; }
        /// <summary>
        /// Right hand's ray interactor.
        /// </summary>
        public RayInteractor RightRayInteractor { get; set; }
        /// <summary>
        /// Pass-through layer for rendering the keyboard.
        /// </summary>
        public OVRPassthroughLayer PassthroughOverlay { get; set; }
        /// <summary>
        /// Left hand's material property block editor.
        /// </summary>
        public MaterialPropertyBlockEditor LeftHandPropertyBlock { get; set; }
        /// <summary>
        /// Right hand's material property block editor.
        /// </summary>
        public MaterialPropertyBlockEditor RightHandPropertyBlock { get; set; }

        private void Awake()
        {
            _keyboardCollider = GetComponent<Collider>();
            InitializeHandStates();
        }

        private void OnDisable()
        {
            _isBeingDisabled = true;
            SetHandAlpha(LeftHandPropertyBlock, 0);
            SetHandAlpha(RightHandPropertyBlock, 0);
            SetRayInteractorState(0, true);
            SetRayInteractorState(1, true);
            InitializeHandStates();
        }

        private void OnTriggerEnter(Collider other) => HandleTriggerEvent(other, true);
        private void OnTriggerExit(Collider other) => HandleTriggerEvent(other, false);
        private void OnTriggerStay(Collider other) => UpdateHandProximity(other);
        /// <summary>
        /// Gets the index for a given hand.
        /// </summary>
        /// <param name="hand">The OVRHand to get the index for.</param>
        /// <returns>0 for left hand, 1 for right hand.</returns>
        private int GetHandIndex(OVRHand hand) => hand.GetHand() == OVRPlugin.Hand.HandLeft ? 0 : 1;

        /// <summary>
        /// Handles trigger events between the hands and keyboard.
        /// </summary>
        /// <param name="other">Collider of the object entering or exiting the trigger zone.</param>
        /// <param name="isEntering">True if a hand is entering, false if existing.</param>
        private void HandleTriggerEvent(Collider other, bool isEntering)
        {
            OVRHand ovrHand = other.GetComponentInChildren<OVRHand>();
            if (!ovrHand)
            {
                return;
            }

            int handIndex = GetHandIndex(ovrHand);
            if (isEntering)
            {
                UpdateHandProximity(other);
            }
            else
            {
                ResetHandState(handIndex);
            }

            UpdateHoverState();

        }

        /// <summary>
        /// Updates the proximity state of a hand to the keyboard.
        /// </summary>
        /// <param name="handCollider">Collider of the hand.</param>
        private void UpdateHandProximity(Collider handCollider)
        {
            OVRHand ovrHand = handCollider.GetComponentInChildren<OVRHand>();
            if (!ovrHand)
            {
                return;
            }

            int handIndex = GetHandIndex(ovrHand);
            float penetrationDistance = CalculatePenetrationDistance(handCollider);
            _handStates[handIndex].IsHovering = true;
            _handStates[handIndex].Distance = penetrationDistance;
            SetRayInteractorState(handIndex, false);
        }

        /// <summary>
        /// Calculates the penetration distance between the hand and the keyboard collider.
        /// </summary>
        /// <param name="other">Collider of the hand.</param>
        /// <returns>The penetration distance.</returns>
        private float CalculatePenetrationDistance(Collider other)
        {
            return Physics.ComputePenetration(_keyboardCollider, transform.position, transform.rotation,
                other, other.transform.position, other.transform.rotation,
                out _, out float distance) ? distance : 0f;
        }

        private void Update()
        {
            if (_isBeingDisabled)
            {
                return;
            }

            UpdateHandVisibility(LeftHandVisual, _handStates[LeftHandIndex].Distance, LeftHandPropertyBlock);
            UpdateHandVisibility(RightHandVisual, _handStates[RightHandIndex].Distance, RightHandPropertyBlock);
            UpdatePassthroughAlpha();
        }

        /// <summary>
        /// Updates the visibility of the hand based on proximity to the keyboard.
        /// </summary>
        /// <param name="hand">Visual component of the hand.</param>
        /// <param name="distance">The penetration distance between the hand and the keyboard collider.</param>
        /// <param name="propertyBlock">Material property block editor for the hand.</param>
        private void UpdateHandVisibility(HandVisual hand, float distance, MaterialPropertyBlockEditor propertyBlock)
        {
            if (!hand || !propertyBlock)
            {
                return;
            }

            float alpha = CalculateAlpha(distance);
            SetHandAlpha(propertyBlock, alpha);
        }

        /// <summary>
        /// Calculates the alpha value based on the hands' penetration distance within the keyboard collider.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        private float CalculateAlpha(float distance)
        {
            if (Mathf.Approximately(distance, float.MaxValue))
            {
                return 0f;
            }

            return (Mathf.Clamp(distance, 0f, _maxDistance) / _maxDistance) * FadeMultiplier;
        }

        /// <summary>
        /// Updates the opacity of the pass-through overlay based on hand proximity.
        /// </summary>
        private void UpdatePassthroughAlpha()
        {
            if (PassthroughOverlay)
            {
                float alpha = Mathf.Max(
                    CalculateAlpha(_handStates[LeftHandIndex].Distance),
                    CalculateAlpha(_handStates[RightHandIndex].Distance)
                );
                PassthroughOverlay.textureOpacity = alpha;
            }
        }

        /// <summary>
        /// Updates the overall hover state and triggers appropriate events.
        /// </summary>
        private void UpdateHoverState()
        {
            bool shouldHover = _handStates[LeftHandIndex].IsHovering || _handStates[RightHandIndex].IsHovering;

            if (shouldHover != _isHovering)
            {
                _isHovering = shouldHover;
                if (_isHovering)
                    OnHoverStart?.Invoke();
                else
                    OnHoverEnd?.Invoke();
            }
        }

        /// <summary>
        /// Sets the alpha values for a hand's material properties.
        /// </summary>
        /// <param name="propertyBlockEditor">Material property block editor for the hand.</param>
        /// <param name="alpha">Alpha value to set.</param>
        private void SetHandAlpha(MaterialPropertyBlockEditor propertyBlockEditor, float alpha)
        {
            var propertyBlock = propertyBlockEditor.MaterialPropertyBlock;
            propertyBlock.SetFloat(Opacity, HandOpacity - alpha);
            propertyBlock.SetFloat(OutlineOpacity, HandOutlineOpacity - alpha);
        }

        /// <summary>
        /// Initializes the state of both hands.
        /// </summary>
        private void InitializeHandStates()
        {
            _handStates[LeftHandIndex] = new HandState();
            _handStates[RightHandIndex] = new HandState();
        }

        /// <summary>
        /// Resets the state of a single hand.
        /// </summary>
        /// <param name="handIndex"></param>
        private void ResetHandState(int handIndex)
        {
            _handStates[handIndex].Reset();
            SetRayInteractorState(handIndex, true);
        }

        /// <summary>
        /// Sets the enabled state of a hand's ray interactor.
        /// </summary>
        /// <param name="handIndex">Index of the hand.</param>
        /// <param name="enabled">Whether the ray interactor should be enabled.</param>
        private void SetRayInteractorState(int handIndex, bool enabled)
        {
            if (handIndex == LeftHandIndex)
                LeftRayInteractor.enabled = enabled;
            else
                RightRayInteractor.enabled = enabled;
        }

        /// <summary>
        /// Represents the state of a hand in relation to the keyboard.
        /// </summary>
        private class HandState
        {
            /// <summary>
            /// Indicates whether the hand is currently hovering over the keyboard.
            /// </summary>
            public bool IsHovering { get; set; }
            /// <summary>
            /// Penetration distance between the hand and the keyboard collider.
            /// </summary>
            public float Distance { get; set; } = float.MaxValue;
            /// <summary>
            /// Resets the hand state to its default values.
            /// </summary>
            public void Reset()
            {
                IsHovering = false;
                Distance = float.MaxValue;
            }
        }
    }
}
