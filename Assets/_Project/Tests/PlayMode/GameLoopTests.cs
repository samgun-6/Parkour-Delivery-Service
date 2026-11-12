using System.Collections;
using NUnit.Framework;
using ParkourDelivery.Core;
using ParkourDelivery.Gameplay;
using UnityEngine;
using UnityEngine.TestTools;

namespace ParkourDelivery.Tests
{
    /// <summary>
    /// The level loop: the player runs right into packages, hazards and the
    /// delivery point, and GameManager tracks the outcome.
    /// </summary>
    public class GameLoopTests : PlayerFixture
    {
        private GameManager manager;

        public override void TearDown()
        {
            if (manager != null) Object.Destroy(manager.gameObject);
            base.TearDown();
        }

        private void AddToRight<T>(string name, float x) where T : Component
        {
            CreateTrigger(name, new Vector2(x, 1.3f), new Vector2(1f, 2f)).AddComponent<T>();
        }

        // Call after every Package/Hazard/DeliveryPoint exists: GameManager counts packages in Awake.
        private IEnumerator StartRun(int lives = 3)
        {
            manager = new GameObject("GameManager").AddComponent<GameManager>();
            manager.Configure(controller, lives, countdownStep: 0.01f, respawnDelay: 0.1f);
            float deadline = Time.time + 3f;
            while (manager.State != GameState.Playing && Time.time < deadline) yield return null;
            Assert.AreEqual(GameState.Playing, manager.State, "countdown never finished");
        }

        private IEnumerator RunRight(float seconds)
        {
            PressKey(keyboard.dKey);
            yield return new WaitForSeconds(seconds);
            ReleaseKey(keyboard.dKey);
        }

        [UnityTest]
        public IEnumerator RunningThroughPackageCollectsIt()
        {
            CreateFloor();
            CreatePlayer(new Vector2(0, 1.4f));
            AddToRight<Package>("Package", 3f);
            yield return StartRun();
            Assert.AreEqual(1, manager.Stats.PackagesTotal);

            yield return RunRight(1f);

            Assert.AreEqual(1, manager.Stats.PackagesCollected);
        }

        [UnityTest]
        public IEnumerator HazardCostsLifeAndRespawnsAtCheckpoint()
        {
            CreateFloor();
            CreatePlayer(new Vector2(0, 1.4f));
            AddToRight<Hazard>("Hazard", 3f);
            yield return StartRun();
            Vector3 start = player.transform.position;

            PressKey(keyboard.dKey);
            float deadline = Time.time + 2f;
            while (manager.Stats.Lives == 3 && Time.time < deadline) yield return null;
            ReleaseKey(keyboard.dKey);
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(2, manager.Stats.Lives);
            Assert.AreEqual(GameState.Playing, manager.State);
            Assert.Less(Mathf.Abs(player.transform.position.x - start.x), 0.5f, "respawned at the start");
        }

        [UnityTest]
        public IEnumerator LosingLastLifeIsGameOver()
        {
            CreateFloor();
            CreatePlayer(new Vector2(0, 1.4f));
            AddToRight<Hazard>("Hazard", 3f);
            yield return StartRun(lives: 1);

            yield return RunRight(1f);
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(GameState.GameOver, manager.State);
        }

        [UnityTest]
        public IEnumerator ReachingDeliveryPointCompletesLevelWithScore()
        {
            CreateFloor();
            CreatePlayer(new Vector2(0, 1.4f));
            AddToRight<Package>("Package", 2f);
            AddToRight<DeliveryPoint>("Delivery", 5f);
            yield return StartRun();

            yield return RunRight(1.5f);

            Assert.AreEqual(GameState.LevelComplete, manager.State);
            Assert.GreaterOrEqual(manager.FinalScore, ScoreCalculator.PointsPerPackage);
        }
    }
}
