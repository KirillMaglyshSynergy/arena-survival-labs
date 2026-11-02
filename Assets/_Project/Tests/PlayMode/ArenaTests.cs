using System.Collections;
using ArenaSurvival.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ArenaSurvival.Tests
{
    public sealed class ArenaTests
    {
        private Keyboard keyboard;
        private GameObject player;
        private PlayerFallRespawner respawner;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync("Arena_01");
            yield return null;
            yield return new WaitForSeconds(0.3f);
            player = Object.FindFirstObjectByType<PlayerMovement>().gameObject;
            respawner = player.GetComponent<PlayerFallRespawner>();
        }

        [TearDown]
        public void TearDown()
        {
            if (keyboard != null && keyboard.added)
            {
                InputSystem.RemoveDevice(keyboard);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        [UnityTest]
        public IEnumerator SpawnerCreatesExactlyOnePlayer()
        {
            Assert.That(Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(player.GetComponent<Rigidbody>(), Is.Null);
            Assert.That(player.GetComponent<CapsuleCollider>(), Is.Null);
            Assert.That(player.GetComponent<CharacterController>(), Is.Not.Null);
            Assert.That(player.transform.position.z, Is.EqualTo(-7).Within(0.05f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator WalkAndSprintHaveDifferentSpeeds()
        {
            float startX = player.transform.position.x;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.A));
            yield return new WaitForSeconds(0.8f);
            float walkDistance = Mathf.Abs(player.transform.position.x - startX);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            respawner.Respawn();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.A, Key.LeftShift));
            yield return new WaitForSeconds(0.8f);
            float sprintDistance = Mathf.Abs(player.transform.position.x - startX);

            Assert.That(walkDistance, Is.GreaterThan(2));
            Assert.That(sprintDistance, Is.GreaterThan(walkDistance * 1.2f));
        }

        [UnityTest]
        public IEnumerator DiagonalInputIsNormalized()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.D));
            yield return null;
            Vector2 movement = player.GetComponent<PlayerInputReader>().ReadMovement();
            Assert.That(movement.magnitude, Is.EqualTo(1).Within(0.001f));
            Assert.That(movement.x, Is.GreaterThan(0));
            Assert.That(movement.y, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator JumpLandsOnFloor()
        {
            float initialY = player.transform.position.y;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return new WaitForSeconds(0.2f);
            Assert.That(player.transform.position.y, Is.GreaterThan(initialY + 0.5f));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSeconds(1.1f);
            Assert.That(player.transform.position.y, Is.EqualTo(initialY).Within(0.12f));
        }

        [UnityTest]
        public IEnumerator WallBlocksPlayer()
        {
            Teleport(new Vector3(0, 0.15f, 8));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.LeftShift));
            yield return new WaitForSeconds(1);
            Assert.That(player.transform.position.z, Is.InRange(9.2f, 9.6f));
        }

        [UnityTest]
        public IEnumerator LightCrateMovesWhenPushed()
        {
            Rigidbody crate = GameObject.Find("Crate_Light_01").GetComponent<Rigidbody>();
            float initialZ = crate.position.z;
            Teleport(new Vector3(0, 0.15f, -4.5f));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            yield return new WaitForSeconds(0.9f);
            Assert.That(crate.position.z, Is.GreaterThan(initialZ + 0.15f));
        }

        [UnityTest]
        public IEnumerator HeavyAndKinematicCratesStayInPlace()
        {
            foreach (string name in new[] { "Crate_Heavy", "Crate_Kinematic" })
            {
                Rigidbody crate = GameObject.Find(name).GetComponent<Rigidbody>();
                Vector3 initial = crate.position;
                respawner.Respawn();
                Teleport(new Vector3(initial.x, 0.15f, initial.z - 1.5f));
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
                yield return new WaitForSeconds(0.7f);
                Assert.That(crate.position.z, Is.EqualTo(initial.z).Within(0.05f), name);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator FallingReturnsPlayerToSpawn()
        {
            Teleport(new Vector3(30, -10, 30));
            yield return null;
            yield return new WaitForSeconds(0.3f);
            Assert.That(player.transform.position.x, Is.EqualTo(0).Within(0.05f));
            Assert.That(player.transform.position.z, Is.EqualTo(-7).Within(0.05f));
            Assert.That(player.transform.position.y, Is.InRange(-0.12f, 0.2f));
            Assert.That(player.GetComponent<CharacterController>().enabled, Is.True);
        }

        private void Teleport(Vector3 position)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.SetPositionAndRotation(position, Quaternion.identity);
            controller.enabled = true;
            player.GetComponent<PlayerMovement>().ResetMotion();
        }
    }
}
