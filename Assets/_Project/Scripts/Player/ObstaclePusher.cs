using System.Collections.Generic;
using UnityEngine;

namespace ArenaSurvival.Player
{
    /// <summary>
    /// Передает ограниченный импульс Rigidbody-объектам,
    /// с которыми сталкивается CharacterController.
    ///
    /// Логика вынесена из PlayerMovement, потому что
    /// толкание препятствий является отдельной ответственностью.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class ObstaclePusher : MonoBehaviour
    {
        [Header("Push Settings")]
        [SerializeField, Min(0f)]
        private float pushImpulsePerSpeedUnit = 0.35f;

        [SerializeField, Min(0f)]
        private float maximumImpulse = 3f;

        [SerializeField, Min(0f)]
        private float maximumPushableMass = 10f;

        [SerializeField, Min(0f)]
        private float minimumPlayerSpeed = 0.5f;

        [SerializeField, Min(0f)]
        private float repeatPushDelay = 0.15f;

        private PlayerMovement playerMovement;

        // Храним время последнего импульса для каждого Rigidbody.
        // Это защищает объект от множества импульсов подряд,
        // пока Player остается с ним в контакте.
        private readonly Dictionary<int, float>
            lastPushTimeByBody = new();

        private void Awake()
        {
            playerMovement =
                GetComponent<PlayerMovement>();
        }

        private void OnControllerColliderHit(
            ControllerColliderHit hit)
        {
            Rigidbody hitBody =
                hit.collider.attachedRigidbody;

            // Статические Collider не имеют Rigidbody.
            // Kinematic Rigidbody не должен реагировать на силу.
            if (hitBody == null || hitBody.isKinematic)
            {
                return;
            }

            if (hitBody.mass > maximumPushableMass)
            {
                return;
            }

            float playerSpeed =
                playerMovement.CurrentHorizontalSpeed;

            if (playerSpeed < minimumPlayerSpeed)
            {
                return;
            }

            // Убираем вертикальную составляющую,
            // чтобы Player не подбрасывал объект вверх.
            Vector3 pushDirection =
                Vector3.ProjectOnPlane(
                    hit.moveDirection,
                    Vector3.up);

            if (pushDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            int bodyId =
                hitBody.GetInstanceID();

            if (lastPushTimeByBody.TryGetValue(
                    bodyId,
                    out float lastPushTime) &&
                Time.time - lastPushTime <
                repeatPushDelay)
            {
                return;
            }

            float impulse = Mathf.Min(
                playerSpeed * pushImpulsePerSpeedUnit,
                maximumImpulse);

            hitBody.AddForceAtPosition(
                pushDirection.normalized * impulse,
                hit.point,
                ForceMode.Impulse);

            lastPushTimeByBody[bodyId] =
                Time.time;
        }
    }
}
