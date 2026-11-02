using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaSurvival.Player
{
    /// <summary>
    /// Предоставляет остальным компонентам данные клавиатуры и мыши.
    ///
    /// Компонент не перемещает Player и не вращает камеру.
    /// Его единственная ответственность - чтение ввода.
    /// </summary>
    public sealed class PlayerInputReader : MonoBehaviour
    {
        /// <summary>
        /// Возвращает направление движения.
        ///
        /// X: движение влево и вправо.
        /// Y: движение назад и вперед.
        /// </summary>
        public Vector2 ReadMovement()
        {
            Keyboard keyboard = Keyboard.current;

            // На устройстве может не быть физической клавиатуры.
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.aKey.isPressed)
            {
                horizontal -= 1f;
            }

            if (keyboard.dKey.isPressed)
            {
                horizontal += 1f;
            }

            if (keyboard.sKey.isPressed)
            {
                vertical -= 1f;
            }

            if (keyboard.wKey.isPressed)
            {
                vertical += 1f;
            }

            Vector2 input = new(horizontal, vertical);

            // Ограничиваем длину, чтобы диагональное движение
            // не было быстрее движения только вперед.
            return Vector2.ClampMagnitude(input, 1f);
        }

        /// <summary>
        /// Возвращает смещение мыши с предыдущего кадра.
        /// </summary>
        public Vector2 ReadLookDelta()
        {
            Mouse mouse = Mouse.current;

            return mouse == null
                ? Vector2.zero
                : mouse.delta.ReadValue();
        }

        /// <summary>
        /// Возвращает true в кадре нажатия Space.
        /// </summary>
        public bool WasJumpPressed()
        {
            Keyboard keyboard = Keyboard.current;

            return keyboard != null &&
                   keyboard.spaceKey.wasPressedThisFrame;
        }

        /// <summary>
        /// Возвращает true, пока удерживается Left Shift.
        /// </summary>
        public bool IsSprintHeld()
        {
            Keyboard keyboard = Keyboard.current;

            return keyboard != null &&
                   keyboard.leftShiftKey.isPressed;
        }

        /// <summary>
        /// Возвращает true в кадре нажатия Escape.
        /// </summary>
        public bool WasCursorReleasePressed()
        {
            Keyboard keyboard = Keyboard.current;

            return keyboard != null &&
                   keyboard.escapeKey.wasPressedThisFrame;
        }

        /// <summary>
        /// Возвращает true в кадре нажатия левой кнопки мыши.
        /// </summary>
        public bool WasPrimaryButtonPressed()
        {
            Mouse mouse = Mouse.current;

            return mouse != null &&
                   mouse.leftButton.wasPressedThisFrame;
        }
    }
}
