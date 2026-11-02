using UnityEngine;

namespace ArenaSurvival.Player
{
    /// <summary>
    /// Перемещает Player через CharacterController.
    ///
    /// Компонент отвечает за ходьбу, ускорение,
    /// прыжок и гравитацию, но не вращает камеру.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float walkSpeed = 5f;

        [SerializeField, Min(0f)]
        private float sprintSpeed = 8f;

        [SerializeField, Min(0f)]
        private float acceleration = 20f;

        [Header("Vertical Movement")]
        [SerializeField, Min(0f)]
        private float jumpHeight = 1.5f;

        [SerializeField]
        private float gravity = -20f;

        [SerializeField]
        private float groundedVerticalVelocity = -2f;

        private CharacterController characterController;
        private PlayerInputReader inputReader;

        private float currentHorizontalSpeed;
        private float verticalVelocity;

        /// <summary>
        /// Текущая горизонтальная скорость нужна,
        /// например, для расчета силы толкания.
        /// </summary>
        public float CurrentHorizontalSpeed =>
            currentHorizontalSpeed;

        private void Awake()
        {
            characterController =
                GetComponent<CharacterController>();

            inputReader =
                GetComponent<PlayerInputReader>();
        }

        public void ResetMotion()
        {
            currentHorizontalSpeed = 0f;
            verticalVelocity = 0f;
        }

        private void Update()
        {
            Vector2 movementInput =
                inputReader.ReadMovement();

            UpdateHorizontalSpeed(movementInput);
            UpdateVerticalVelocity();

            Vector3 horizontalDirection =
                transform.right * movementInput.x +
                transform.forward * movementInput.y;

            Vector3 horizontalVelocity =
                horizontalDirection * currentHorizontalSpeed;

            Vector3 totalVelocity =
                horizontalVelocity +
                Vector3.up * verticalVelocity;

            // Move ожидает перемещение за текущий кадр,
            // поэтому скорость умножается на Time.deltaTime.
            characterController.Move(
                totalVelocity * Time.deltaTime);
        }

        private void UpdateHorizontalSpeed(
            Vector2 movementInput)
        {
            bool hasMovementInput =
                movementInput.sqrMagnitude > 0.001f;

            float targetSpeed = 0f;

            if (hasMovementInput)
            {
                targetSpeed = inputReader.IsSprintHeld()
                    ? sprintSpeed
                    : walkSpeed;
            }

            // MoveTowards создает плавный разгон и остановку.
            currentHorizontalSpeed = Mathf.MoveTowards(
                currentHorizontalSpeed,
                targetSpeed,
                acceleration * Time.deltaTime);
        }

        private void UpdateVerticalVelocity()
        {
            bool isGrounded =
                characterController.isGrounded;

            if (isGrounded && verticalVelocity < 0f)
            {
                // Небольшая отрицательная скорость помогает
                // CharacterController сохранять контакт с полом.
                verticalVelocity =
                    groundedVerticalVelocity;
            }

            if (isGrounded &&
                inputReader.WasJumpPressed())
            {
                // Формула получает стартовую скорость,
                // необходимую для прыжка на заданную высоту.
                verticalVelocity = Mathf.Sqrt(
                    jumpHeight * -2f * gravity);
            }

            // CharacterController не применяет гравитацию сам.
            verticalVelocity +=
                gravity * Time.deltaTime;
        }
    }
}
