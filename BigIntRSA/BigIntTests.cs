using System;
using NUnit.Framework;

namespace BigIntRSA
{
    [TestFixture]
    public class BigIntTests
    {
        private BigInt _myBigInt;

        [SetUp]
        public void SetUp()
        {
            _myBigInt = new BigInt();
        }

        [Test]
        public void ZeroInt()
        {
            Assert.AreEqual("0", _myBigInt.ToString());
        }

        [Test]
        public void NegativeString()
        {
            var q = new BigInt("-123");
            Assert.AreEqual("-123", q.ToString());
        }

        [Test]
        public void PositiveString()
        {
            var q = new BigInt("3342");
            Assert.AreEqual("3342", q.ToString());
        }

        [Test]
        public void UncorrectStringThrows()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var q = new BigInt("fas231");
            });
        }

        [Test]
        public void Subtract()
        {
            var q = new BigInt("123");
            var w = new BigInt("143");
            Assert.AreEqual("31", w-q);
        }
    }
}