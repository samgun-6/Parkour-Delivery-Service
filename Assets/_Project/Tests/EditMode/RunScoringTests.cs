using NUnit.Framework;
using ParkourDelivery.Core;

namespace ParkourDelivery.Tests
{
    public class RunScoringTests
    {
        [Test]
        public void ScoreAddsTimeBonusWhenUnderPar()
        {
            // 3 packages * 1000 + (120 - 60) s * 20
            Assert.AreEqual(4200, ScoreCalculator.Calculate(3, 60f, 120f));
        }

        [Test]
        public void ScoreHasNoTimeBonusOverPar()
        {
            Assert.AreEqual(2000, ScoreCalculator.Calculate(2, 200f, 120f));
        }

        [Test]
        public void LosingLastLifeEndsRun()
        {
            var stats = new RunStats(lives: 2, packagesTotal: 5);

            Assert.IsFalse(stats.LoseLife());
            Assert.IsTrue(stats.LoseLife());
            Assert.AreEqual(0, stats.Lives);
            Assert.IsTrue(stats.LoseLife(), "stays at zero");
        }

        [Test]
        public void PackagesNeverExceedTotal()
        {
            var stats = new RunStats(lives: 3, packagesTotal: 2);

            stats.CollectPackage();
            stats.CollectPackage();
            stats.CollectPackage();

            Assert.AreEqual(2, stats.PackagesCollected);
        }
    }
}
