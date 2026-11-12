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
        private readonly System.Collections.Generic.List<GameObject> blocks = new System.Collections.Generic.List<GameObject>();
        private PlayerController controller;

        public override void TearDown()
        {
            if (player != null) Object.Destroy(player);
            if (floor != null) Object.Destroy(floor);
            foreach (var block in blocks) Object.Destroy(block);
            blocks.Clear();
            base.TearDown();
        }

        private void CreateFloor()
        {
            floor = new GameObject("Floor") { layer = GroundLayer };
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.AddComponent<BoxCollider2D>().size = new Vector2(100, 1);
        }

        private GameObject CreateBlock(string name, Vector2 center, Vector2 size)
        {
            var block = new GameObject(name) { layer = GroundLayer };
            block.transform.position = center;
            block.AddComponent<BoxCollider2D>().size = size;
            blocks.Add(block);
            return block;
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
            controller.wallSlidingSpeed = 1;
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

        private string State()
        {
            return $"pos={player.transform.position} vel={controller.rigBody.linearVelocity} " +
                   $"grounded={controller.IsGrounded} wall={controller.HasWall} input={controller.HorizontalInput}";
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

        [UnityTest]
        public IEnumerator WallSlideLimitsFallSpeed()
        {
            // Wall face at x = 0.55, touching the right wall checker.
            CreateBlock("Wall", new Vector2(1.05f, 10f), new Vector2(1f, 40f));
            CreatePlayer(new Vector2(0, 20f));

            PressKey(keyboard.dKey);
            yield return new WaitForSeconds(1f);

            Assert.IsTrue(controller.IsWallSliding, State());
            // Clamped to -wallSlidingSpeed (-1) in FixedUpdate; the physics step then adds
            // one step of gravity (~-0.2). Free fall would be around -20 by now.
            Assert.Greater(controller.rigBody.linearVelocity.y, -1.5f, State());
            ReleaseKey(keyboard.dKey);
        }

        [UnityTest]
        public IEnumerator WallJumpPushesAwayFromWall()
        {
            CreateBlock("Wall", new Vector2(1.05f, 10f), new Vector2(1f, 40f));
            CreatePlayer(new Vector2(0, 20f));
            int jumps = 0;
            controller.Jumped += () => jumps++;

            PressKey(keyboard.dKey);
            yield return new WaitForSeconds(0.5f);
            Assert.IsTrue(controller.IsWallSliding);

            PressKey(keyboard.spaceKey);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.AreEqual(1, jumps);
            Assert.Less(controller.rigBody.linearVelocity.x, 0f, "should move away from the right-hand wall");
            Assert.Greater(controller.rigBody.linearVelocity.y, 0f);
            ReleaseKey(keyboard.spaceKey);
            ReleaseKey(keyboard.dKey);
        }

        [UnityTest]
        public IEnumerator StaysCrouchedUnderLowCeiling()
        {
            CreateFloor();
            CreatePlayer(new Vector2(0, 1.4f));
            yield return WaitUntilGrounded();

            PressKey(keyboard.sKey);
            yield return null;
            Assert.IsTrue(controller.IsCrouching);

            // Covers the top checker (about 2.6 above the floor) but clears the crouched collider.
            CreateBlock("Ceiling", new Vector2(0, 2.6f), new Vector2(4f, 0.4f));
            yield return new WaitForFixedUpdate();

            ReleaseKey(keyboard.sKey);
            yield return null;
            yield return null;
            Assert.IsTrue(controller.IsCrouching, "should not stand up into the ceiling");
        }
    }
}
