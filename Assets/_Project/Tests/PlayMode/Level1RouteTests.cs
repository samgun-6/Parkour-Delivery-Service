using System.Collections;
using NUnit.Framework;
using ParkourDelivery.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ParkourDelivery.Tests
{
    /// <summary>
    /// Plays the hard passages of the real Level1 scene with simulated input, to check
    /// that the layout can be completed with the moves it is designed around, and not
    /// without them. Coordinates refer to the layout in Level1Builder.
    /// </summary>
    public class Level1RouteTests : PlayerFixture
    {
        private GameManager manager;

        private IEnumerator LoadLevel()
        {
            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync("Level1");
            manager = GameManager.Instance;
            controller = manager.Player;
            float deadline = Time.time + 6f;
            while (manager.State != GameState.Playing && Time.time < deadline) yield return null;
            Assert.AreEqual(GameState.Playing, manager.State, "countdown never finished");
        }

        private IEnumerator Teleport(Vector2 position)
        {
            controller.transform.position = position;
            controller.rigBody.linearVelocity = Vector2.zero;
            for (int i = 0; i < 100 && !controller.IsGrounded; i++) yield return new WaitForFixedUpdate();
            Assert.IsTrue(controller.IsGrounded, $"not standing after teleport to {position}");
        }

        private void Hold(int direction)
        {
            ReleaseKey(keyboard.aKey);
            ReleaseKey(keyboard.dKey);
            if (direction > 0) PressKey(keyboard.dKey);
            if (direction < 0) PressKey(keyboard.aKey);
        }

        private Vector3 Position => controller.transform.position;

        [UnityTest]
        public IEnumerator ShaftCanBeClimbedWithWallJumps()
        {
            yield return LoadLevel();
            yield return Teleport(new Vector2(91f, 10.5f)); // shaft floor, roof at 9

            int direction = 1;
            Hold(direction);
            PressKey(keyboard.spaceKey);
            float lastJump = Time.time;
            float deadline = Time.time + 10f;
            while (Time.time < deadline)
            {
                if (controller.IsGrounded && Position.y > 33f) break;
                if (Position.y > 35f)
                {
                    // Above both walls: steer onto the tower roof.
                    if (direction != 1) Hold(direction = 1);
                }
                else if (controller.HasWall && !controller.IsGrounded && Time.time - lastJump > 0.25f)
                {
                    ReleaseKey(keyboard.spaceKey);
                    PressKey(keyboard.spaceKey);
                    lastJump = Time.time;
                    Hold(direction = -direction);
                }
                yield return new WaitForFixedUpdate();
            }
            Hold(0);
            ReleaseKey(keyboard.spaceKey);

            Assert.IsTrue(controller.IsGrounded && Position.y > 33f, $"did not reach the roof (y={Position.y:0.0})");
        }

        [UnityTest]
        public IEnumerator TunnelBlocksStandingButNotCrouching()
        {
            yield return LoadLevel();
            yield return Teleport(new Vector2(150f, 19.5f)); // B7 roof at 18, tunnel from x 156

            Hold(1);
            yield return new WaitForSeconds(2f);
            Assert.Less(Position.x, 155f, "walked into the tunnel standing up");

            PressKey(keyboard.sKey);
            yield return new WaitForSeconds(3.5f);
            Hold(0);
            ReleaseKey(keyboard.sKey);

            Assert.Greater(Position.x, 171f, "crouching should get through the tunnel");
        }

        private IEnumerator RunAndJumpOffB8(bool sprint)
        {
            yield return LoadLevel();
            yield return Teleport(new Vector2(184f, 15.5f)); // B8 roof at 14, edge at x 196
            if (sprint) PressKey(keyboard.jKey);
            Hold(1);
            float deadline = Time.time + 3f;
            while (Position.x < 194.3f && Time.time < deadline) yield return new WaitForFixedUpdate();
            PressKey(keyboard.spaceKey);
            // Until it lands on B9 or falls into the kill zone below the street.
            float end = Time.time + 8f;
            while (Time.time < end && manager.Stats.Lives == 3 && !(controller.IsGrounded && Position.x > 210f))
            {
                yield return new WaitForFixedUpdate();
            }
            Hold(0);
            ReleaseKey(keyboard.spaceKey);
            if (sprint) ReleaseKey(keyboard.jKey);
        }

        [UnityTest]
        public IEnumerator SprintJumpClearsTheBigGap()
        {
            yield return RunAndJumpOffB8(sprint: true);

            Assert.AreEqual(3, manager.Stats.Lives, "fell into the gap");
            Assert.Greater(Position.x, 216f, "landed on B9");
        }

        [UnityTest]
        public IEnumerator RunningJumpFallsShortOfTheBigGap()
        {
            yield return RunAndJumpOffB8(sprint: false);

            Assert.AreEqual(2, manager.Stats.Lives, "the gap should need a sprint");
        }
    }
}
