using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace ParkourDelivery.Tests
{
    /// <summary>
    /// Drives PlayerController through simulated keyboard input on a flat floor.
    /// </summary>
    public class PlayerControllerTests : InputTestFixture
    {
        private const int GroundLayer = 9;

        private Keyboard keyboard;
        private GameObject floor;
        private GameObject player;
        private PlayerController controller;

        public override void TearDown()
        {
            if (player != null) Object.Destroy(player);
            if (floor != null) Object.Destroy(floor);
            base.TearDown();
        }

        private void CreateFloor()
        {
            floor = new GameObject("Floor") { layer = GroundLayer };
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.AddComponent<BoxCollider2D>().size = new Vector2(100, 1);
        }

        private void CreatePlayer(Vector2 position)
        {
            keyboard = InputSystem.AddDevice<Keyboard>();
            player = new GameObject("Player");
            player.SetActive(false);
            player.transform.position = position;

            var body = player.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            var capsule = player.AddComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(1f, 2.56f);

            controller = player.AddComponent<PlayerController>();
            controller.rigBody = body;
            controller.speed = 7;
            controller.sprintSpeed = 14;
            controller.elasticity = 13;
            controller.groundLayer = 1 << GroundLayer;
            controller.isGroundedCheck = Child("GroundChecker", new Vector2(0, -1.3f));
            controller.topObstacleCheck = Child("TopChecker", new Vector2(0, 1.3f));
            controller.rightWallCheck = Child("RightWall", new Vector2(0.55f, 0));
            controller.leftWallCheck = Child("LeftWall", new Vector2(-0.55f, 0));

            player.SetActive(true);
        }

        private Transform Child(string name, Vector2 localPosition)
        {
            var child = new GameObject(name).transform;
            child.SetParent(player.transform, false);
            child.localPosition = localPosition;
            return child;
        }

        // Batch play mode interleaves editor updates, which leave the editor
        // state buffers active; run a player update first so the simulated
        // keyboard has state to write into.
        private void PressKey(UnityEngine.InputSystem.Controls.KeyControl key)
        {
            InputSystem.Update();
            Press(key);
        }

        private void ReleaseKey(UnityEngine.InputSystem.Controls.KeyControl key)
        {
            InputSystem.Update();
            Release(key);
        }

        private IEnumerator WaitUntilGrounded()
        {
            for (int i = 0; i < 200 && !controller.IsGrounded; i++) yield return new WaitForFixedUpdate();
            Assert.IsTrue(controller.IsGrounded, "player never landed on the floor");
        }

        [UnityTest]
        public IEnumerator MovesRightWhileRightKeyHeld()
        {
            CreateFloor();
            CreatePlayer(new Vector2(0, 1.4f));
            yield return WaitUntilGrounded();
            float startX = player.transform.position.x;

            PressKey(keyboard.dKey);
            yield return new WaitForSeconds(0.3f);
            ReleaseKey(keyboard.dKey);

            Assert.Greater(player.transform.position.x, startX + 0.5f);
        }

        [UnityTest]
        public IEnumerator JumpsWhenGroundedAndJumpPressed()
        {
            CreateFloor();
            CreatePlayer(new Vector2(0, 1.4f));
            yield return WaitUntilGrounded();
            int jumps = 0;
            controller.Jumped += () => jumps++;
            float startY = player.transform.position.y;

            PressKey(keyboard.spaceKey);
            yield return new WaitForSeconds(0.2f);
            ReleaseKey(keyboard.spaceKey);

            Assert.AreEqual(1, jumps);
            Assert.Greater(player.transform.position.y, startY + 0.5f);
        }

        [UnityTest]
        public IEnumerator DoesNotJumpWhileInAir()
        {
            CreatePlayer(new Vector2(0, 50f));
            int jumps = 0;
            controller.Jumped += () => jumps++;
            yield return new WaitForFixedUpdate();

            PressKey(keyboard.spaceKey);
            yield return new WaitForSeconds(0.2f);
            ReleaseKey(keyboard.spaceKey);

            Assert.AreEqual(0, jumps);
        }

        [UnityTest]
        public IEnumerator CrouchShrinksColliderUntilReleased()
        {
            CreateFloor();
            CreatePlayer(new Vector2(0, 1.4f));
            yield return WaitUntilGrounded();
            var capsule = player.GetComponent<CapsuleCollider2D>();
            float standing = capsule.size.y;

            PressKey(keyboard.sKey);
            yield return null;
            Assert.IsTrue(controller.IsCrouching);
            Assert.Less(capsule.size.y, standing);

            ReleaseKey(keyboard.sKey);
            yield return null;
            Assert.IsFalse(controller.IsCrouching);
            Assert.AreEqual(standing, capsule.size.y, 0.001f);
        }
    }
}
