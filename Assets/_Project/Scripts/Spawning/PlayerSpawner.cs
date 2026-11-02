using UnityEngine;

namespace ArenaSurvival.Spawning
{
    /// <summary>
    /// Создает Player в заданной точке появления.
    ///
    /// Компонент не управляет Player после создания.
    /// Его ответственность заканчивается после Instantiate.
    /// </summary>
    public sealed class PlayerSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GameObject playerPrefab;

        [SerializeField]
        private Transform spawnPoint;

        private GameObject spawnedPlayer;

        private void Start()
        {
            SpawnPlayer();
        }

        [ContextMenu("Spawn Player")]
        public void SpawnPlayer()
        {
            if (spawnedPlayer != null)
            {
                Debug.LogWarning(
                    "PlayerSpawner: Player уже создан.",
                    this);

                return;
            }

            if (playerPrefab == null)
            {
                Debug.LogError(
                    "PlayerSpawner: Player Prefab не назначен.",
                    this);

                return;
            }

            if (spawnPoint == null)
            {
                Debug.LogError(
                    "PlayerSpawner: Spawn Point не назначен.",
                    this);

                return;
            }

            spawnedPlayer = Instantiate(
                playerPrefab,
                spawnPoint.position,
                spawnPoint.rotation);

            spawnedPlayer.name =
                playerPrefab.name;
        }
    }
}
