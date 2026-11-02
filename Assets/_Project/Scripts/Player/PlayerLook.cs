using UnityEngine;

namespace ArenaSurvival.Player
{
    /// <summary>
    /// Управляет обзором от первого лица.
    ///
    /// Player вращается только вокруг вертикальной оси Y.
    /// Камера вращается только вверх и вниз по оси X.
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform cameraTransform;

        [Header("Look Settings")]
        [SerializeField, Min(0f)]
        private float mouseSensitivity = 0.08f;

        [SerializeField, Range(0f, 89f)]
        private float verticalLookLimit = 85f;

        private PlayerInputReader inputReader;
        private float cameraPitch;
        private bool isCursorLocked;

        private void Awake()
        {
            inputReader =
                GetComponent<PlayerInputReader>();

            if (cameraTransform == null)
            {
                Camera childCamera =
                    GetComponentInChildren<Camera>();

                if (childCamera != null)
                {
                    cameraTransform =
                        childCamera.transform;
                }
            }

            if (cameraTransform == null)
            {
                Debug.LogError(
                    "PlayerLook: камера Player не назначена.",
                    this);

                enabled = false;
                return;
            }

            LockCursor();
        }

        public void ResetLook()
        {
            cameraPitch = 0f;
            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.identity;
            }
        }

        private void Update()
        {
            UpdateCursorState();

            if (!isCursorLocked)
            {
                return;
            }

            Vector2 lookDelta =
                inputReader.ReadLookDelta();

            float yaw =
                lookDelta.x * mouseSensitivity;

            float pitchDelta =
                lookDelta.y * mouseSensitivity;

            // Все тело поворачивается только влево и вправо.
            transform.Rotate(
                Vector3.up * yaw,
                Space.Self);

            // Камера поворачивается вверх и вниз.
            cameraPitch -= pitchDelta;

            cameraPitch = Mathf.Clamp(
                cameraPitch,
                -verticalLookLimit,
                verticalLookLimit);

            cameraTransform.localRotation =
                Quaternion.Euler(
                    cameraPitch,
                    0f,
                    0f);
        }

        private void UpdateCursorState()
        {
            if (inputReader.WasCursorReleasePressed())
            {
                UnlockCursor();
                return;
            }

            if (!isCursorLocked &&
                inputReader.WasPrimaryButtonPressed())
            {
                LockCursor();
            }
        }

        private void LockCursor()
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible = false;
            isCursorLocked = true;
        }

        private void UnlockCursor()
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible = true;
            isCursorLocked = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                UnlockCursor();
            }
        }

        private void OnDisable()
        {
            UnlockCursor();
        }
    }
}
