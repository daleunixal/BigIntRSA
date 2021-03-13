﻿using System;
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
            Assert.Throws<ArithmeticException>(() =>
            {
                var q = new BigInt("fas231");
            });
        }

        [Test]
        public void Subtract()
        {
            var q = new BigInt("123");
            var w = new BigInt("143");
            Assert.AreEqual(new BigInt(-20), (q-w));
        }

        [Test]
        public void GCDTest()
        {
            var q = new BigInt(543);
            var w = new BigInt(12);
            Assert.AreEqual(new BigInt(3), q.gcd(w));
        }

        [Test]
        public void Multiply()
        {
            Assert.AreEqual(new BigInt(64), new BigInt(8) * new BigInt(8));
        }

        [Test]
        public void MultiplyWithNegative()
        {
            Assert.AreEqual(new BigInt(-25), new BigInt(5) * new BigInt(-5));
        }

        [Test]
        public void MultiplyWithBothNegative()
        {
            Assert.AreEqual(new BigInt(3990), new BigInt(-42) * new BigInt(-95));
        }

        [Test]
        public void SimpleDivide()
        {
            var veryBigNumber =
                new BigInt("543312312312543534643524352423425345312312435435346346534634534543534534534534525");
            var justALittleNum = new BigInt(5);
            var gcd = veryBigNumber.gcd(justALittleNum);
            Assert.AreEqual(new BigInt("108662462462508706928704870484685069062462487087069269306926906908706906906906905"),
                veryBigNumber / gcd);
        }

        [Test]
        public void DivideWithOneNegative()
        {
            var neg = new BigInt(-41242);
            var pos = new BigInt(48);
            var gcd = neg.gcd(pos);
            Assert.AreEqual(new BigInt(-20621), neg / gcd);
        }

        [Test]
        public void InverseModuleMultiplication()
        {
            var first = new BigInt(432423);
            var modulus = first % new BigInt(31);
            Assert.AreEqual(new BigInt(1), (first * first.modInverse(modulus))% modulus);
        }

        [Test]
        public void InverseModuleMultiplicationWithOneDigit()
        {
            var first = new BigInt(3);
            var modulus = new BigInt(7);
            Assert.AreEqual(new BigInt(1), (first * first.modInverse(modulus))% modulus);
        }
    }
}