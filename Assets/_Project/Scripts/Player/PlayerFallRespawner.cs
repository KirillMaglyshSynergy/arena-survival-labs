using UnityEngine;

namespace ArenaSurvival.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerFallRespawner : MonoBehaviour
    {
        [SerializeField]
        private float minimumHeight = -8f;

        private CharacterController characterController;
        private PlayerMovement movement;
        private PlayerLook look;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            movement = GetComponent<PlayerMovement>();
            look = GetComponent<PlayerLook>();

            // Instantiate уже установил позицию и поворот из PlayerSpawner
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        private void LateUpdate()
        {
            if (transform.position.y < minimumHeight)
            {
                Respawn();
            }
        }

        public void Respawn()
        {
            bool wasEnabled = characterController.enabled;
            characterController.enabled = false;
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            movement.ResetMotion();

            if (look != null)
            {
                look.ResetLook();
            }

            characterController.enabled = wasEnabled;
        }
    }
}
