using System;
using System.Collections.Generic;
using System.Linq;

namespace BigIntRSA
{
    public class BigInt
    {
        private static char[] sign = {'+', '-'};
        
        private readonly List<int> _store;
        private readonly bool _isNegative = false; // If false = '+' else '-'


        public BigInt()
        {
            _store = new List<int>() {0};
        }
        
        public BigInt(string number)
        {
            if (char.IsDigit(number[0]))
            {
                _store = ParseString(number);
                return;
            }

            if (number[0] == '+' || number[0] == '-')
            {
                if (number[0] == '-')
                {
                    _isNegative = true;
                }
                _store = ParseString(number.Trim(sign));
            }
            else
            {
                throw new ArgumentException($"Uncorrect input: {number}");
            }
        }

        public BigInt(BigInt ds, bool? sign)
        {
            _store = ds._store;
            _isNegative = sign ?? ds._isNegative;
        }

        public BigInt SwapSign()
        {
            var signToSwap = !_isNegative;
            return new BigInt(this, signToSwap);
        }

        private List<int> ParseString(string number)
        {
            return number.ToCharArray().Select(x => Int32.Parse(x.ToString())).ToList();
        }

        public override string ToString()
        {
            return _isNegative ? '-' + string.Join("", _store) : String.Join("", _store);
        }

        public static BigInt operator %(BigInt firstInt, BigInt secondInt)
        {
            return a - b * a / b;
        }

        public static BigInt operator -(BigInt firstInt, BigInt secondInt)
        { 
            return firstInt + secondInt.SwapSign();
        }

        public static BigInt operator +(BigInt firstInt, BigInt secondInt)
        {
            
        }
    }
}