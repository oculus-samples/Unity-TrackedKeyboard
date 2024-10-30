// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections;
using UnityEngine;

namespace Meta.XR.TrackedKeyboardSample
{
    /// <summary>
    /// Manages the visibility of the Android soft keyboard.
    /// </summary>
    public class KeyboardVisibility : MonoBehaviour
    {
        private Coroutine _hideKeyboardCoroutine;
        private bool _isTrackingSupported;

        private void Start()
        {
            _isTrackingSupported = OVRAnchor.TrackerConfiguration.KeyboardTrackingSupported;
        }

        private void OnDisable()
        {
            if (_hideKeyboardCoroutine != null)
            {
                StopCoroutine(_hideKeyboardCoroutine);
                _hideKeyboardCoroutine = null;
            }
        }

        private void LateUpdate()
        {
            if (_isTrackingSupported && TouchScreenKeyboard.visible && _hideKeyboardCoroutine == null)
            {
                _hideKeyboardCoroutine = StartCoroutine(HideKeyboardAfterLateUpdate());
            }
        }

        /// <summary>
        /// Coroutine to hide the keyboard after the current frame.
        /// </summary>
        /// <returns>IEnumerator for the coroutine.</returns>
        private IEnumerator HideKeyboardAfterLateUpdate()
        {
            yield return new WaitForEndOfFrame();

            if (!isActiveAndEnabled)
                yield break;

            HideKeyboard();
            _hideKeyboardCoroutine = null;
        }

        /// <summary>
        /// Hides the Android soft keyboard.
        /// </summary>
        private void HideKeyboard()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaObject window = currentActivity.Call<AndroidJavaObject>("getWindow");
                    AndroidJavaObject decorView = window.Call<AndroidJavaObject>("getDecorView");
                    AndroidJavaObject inputMethodManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "input_method");
                    AndroidJavaObject windowToken = decorView.Call<AndroidJavaObject>("getWindowToken");

                    bool success = inputMethodManager.Call<bool>("hideSoftInputFromWindow", windowToken, 0);
                    if (success)
                    {
                        Debug.Log("Soft keyboard hidden successfully.");
                    }
                    else
                    {
                        Debug.LogWarning("Soft keyboard could not be hidden.");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to hide keyboard: {e.Message}");
            }
#endif
        }
    }
}
