// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using TMPro;
using UnityEngine;
using UnityEngine.Android;

namespace Meta.XR.TrackedKeyboardSample
{
    public class StatusTextManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _supportText;
        [SerializeField] private TrackedKeyboardManager _keyboardManager;

        private void Start()
        {
            if (_keyboardManager == null)
            {
                _keyboardManager = GetComponent<TrackedKeyboardManager>();
            }
        }

        private void Update()
        {
            if (_supportText)
            {
                _supportText.text =
                    $"{GetKeyboardTrackingStatus()}\n{GetTrackingState()}\n{GetScenePermissionStatus()}";
            }
        }

        private string GetKeyboardTrackingStatus()
        {
            bool isSupported = OVRAnchor.TrackerConfiguration.KeyboardTrackingSupported;
            return FormatStatus("Keyboard Tracking: ", isSupported, "supported", "not supported");
        }

        private string GetTrackingState()
        {
            bool isEnabled = _keyboardManager && _keyboardManager.Trackable;
            return FormatStatus("Tracking state: ", isEnabled, "enabled", "disabled");
        }

        private string GetScenePermissionStatus()
        {
            bool isGranted = Permission.HasUserAuthorizedPermission(OVRPermissionsRequester.ScenePermission);
            return FormatStatus("Scene permission: ", isGranted, "granted", "not granted");
        }

        private string FormatStatus(string label, bool condition, string positiveText, string negativeText)
        {
            string status = condition ? positiveText : negativeText;
            string color = condition ? "#00FF00" : "#FF0000";
            return $"{label} <color={color}><b>{status}</b></color>";
        }
    }
}
