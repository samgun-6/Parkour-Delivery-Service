using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ParkourDelivery.Tests
{
    /// <summary>
    /// Shared setup for play mode tests: a floor, a player built like the one in
    /// Level1, simulated keyboard input, and cleanup of everything created.
    /// </summary>
    public abstract class PlayerFixture : InputTestFixture
    {
        protected const int GroundLayer = 9;

        protected Keyboard keyboard;
        protected GameObject floor;
        protected GameObject player;
        protected readonly List<GameObject> blocks = new List<GameObject>();
        protected PlayerController controller;

        public override void TearDown()
        {
            if (player != null) Object.Destroy(player);
            if (floor != null) Object.Destroy(floor);
            foreach (var block in blocks) if (block != null) Object.Destroy(block);
            blocks.Clear();
            base.TearDown();
        }

        protected void CreateFloor()
        {
            floor = new GameObject("Floor") { layer = GroundLayer };
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.AddComponent<BoxCollider2D>().size = new Vector2(100, 1);
        }

        protected GameObject CreateBlock(string name, Vector2 center, Vector2 size)
        {
            var block = new GameObject(name) { layer = GroundLayer };
            block.transform.position = center;
            block.AddComponent<BoxCollider2D>().size = size;
            blocks.Add(block);
            return block;
        }

        /// <summary>A non-solid trigger box (not on the ground layer), destroyed in TearDown.</summary>
        protected GameObject CreateTrigger(string name, Vector2 center, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.position = center;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.isTrigger = true;
            blocks.Add(go);
            return go;
        }

        protected void CreatePlayer(Vector2 position)
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

        protected Transform Child(string name, Vector2 localPosition)
        {
            var child = new GameObject(name).transform;
            child.SetParent(player.transform, false);
            child.localPosition = localPosition;
            return child;
        }

        // Batch play mode interleaves editor updates, which leave the editor
        // state buffers active; run a player update first so the simulated
        // keyboard has state to write into.
        protected void PressKey(UnityEngine.InputSystem.Controls.KeyControl key)
        {
            InputSystem.Update();
            Press(key);
        }

        protected void ReleaseKey(UnityEngine.InputSystem.Controls.KeyControl key)
        {
            InputSystem.Update();
            Release(key);
        }

        protected string State()
        {
            return $"pos={player.transform.position} vel={controller.rigBody.linearVelocity} " +
                   $"grounded={controller.IsGrounded} wall={controller.HasWall} input={controller.HorizontalInput}";
        }

        protected IEnumerator WaitUntilGrounded()
        {
            for (int i = 0; i < 200 && !controller.IsGrounded; i++) yield return new WaitForFixedUpdate();
            Assert.IsTrue(controller.IsGrounded, "player never landed on the floor");
        }
    }
}
