using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ParkourDelivery.Tests
{
    /// <summary>
    /// Drives PlayerController through simulated keyboard input on a flat floor.
    /// </summary>
    public class PlayerControllerTests : PlayerFixture
    {
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
