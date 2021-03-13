using System;

public class BigInt
{
    // Максимальная длинна хранения
    private const int maxLength = 70;

    private uint[] data = null; // stores bytes from the Big Integer
    public int dataLength; // number of actual chars used

    /// <summary>
    /// Базовый 0
    /// </summary>
    public BigInt()
    {
        data = new uint[maxLength];
        dataLength = 1;
    }

    
    public BigInt(long value)
    {
        data = new uint[maxLength];
        var tempVal = value;

        // copy bytes from long to BigInteger without any assumption of
        // the length of the long datatype
        dataLength = 0;
        while (value != 0 && dataLength < maxLength)
        {
            data[dataLength] = (uint) (value & 0xFFFFFFFF);
            value >>= 32;
            dataLength++;
        }

        if (tempVal > 0) // overflow check for +ve value
        {
            if (value != 0 || (data[maxLength - 1] & 0x80000000) != 0)
                throw new ArithmeticException("Positive overflow in constructor.");
        }
        else if (tempVal < 0) // underflow check for -ve value
        {
            if (value != -1 || (data[dataLength - 1] & 0x80000000) == 0)
                throw new ArithmeticException("Negative underflow in constructor.");
        }

        if (dataLength == 0)
            dataLength = 1;
    }

    
    public BigInt(ulong value)
    {
        data = new uint[maxLength];

        // copy bytes from ulong to BigInteger without any assumption of
        // the length of the ulong datatype
        dataLength = 0;
        while (value != 0 && dataLength < maxLength)
        {
            data[dataLength] = (uint) (value & 0xFFFFFFFF);
            value >>= 32;
            dataLength++;
        }

        if (value != 0 || (data[maxLength - 1] & 0x80000000) != 0)
            throw new ArithmeticException("Positive overflow in constructor.");

        if (dataLength == 0)
            dataLength = 1;
    }

    
    public BigInt(BigInt bi)
    {
        data = new uint[maxLength];

        dataLength = bi.dataLength;

        for (var i = 0; i < dataLength; i++)
            data[i] = bi.data[i];
    }


    /// <summary>
    /// Конструктор-парсер. Принимает строку и СИ строки
    /// </summary>
    /// 
    /// <param name="value">String value in the format of [sign][magnitude]</param>
    /// <param name="radix">The base of value</param>
    public BigInt(string value, int radix = 10)
    {
        var multiplier = new BigInt(1);
        var result = new BigInt();
        value = value.ToUpper().Trim();
        var limit = 0;

        if (value[0] == '-')
            limit = 1;

        for (var i = value.Length - 1; i >= limit; i--)
        {
            var posValue = (int) value[i];

            if (posValue >= '0' && posValue <= '9')
                posValue -= '0';
            else if (posValue >= 'A' && posValue <= 'Z')
                posValue = posValue - 'A' + 10;
            else
                posValue = 9999999; // произвольное сверх-большое


            if (posValue >= radix)
            {
                throw new ArithmeticException("Invalid string in constructor.");
            }

            if (value[0] == '-')
                posValue = -posValue;

            result = result + multiplier * posValue;

            if (i - 1 >= limit)
                multiplier = multiplier * radix;
        }

        if (value[0] == '-') // negative values
        {
            if ((result.data[maxLength - 1] & 0x80000000) == 0)
                throw new ArithmeticException("Negative underflow in constructor.");
        }
        else // positive values
        {
            if ((result.data[maxLength - 1] & 0x80000000) != 0)
                throw new ArithmeticException("Positive overflow in constructor.");
        }

        data = new uint[maxLength];
        for (var i = 0; i < result.dataLength; i++)
            data[i] = result.data[i];

        dataLength = result.dataLength;
    }

    /// <summary>
    ///  Constructor (Default value provided by an array of unsigned integers)
    /// </summary>
    /// <param name="inData">Array of unsigned integer</param>
    public BigInt(uint[] inData)
    {
        dataLength = inData.Length;

        if (dataLength > maxLength)
            throw new ArithmeticException("Byte overflow in constructor.");

        data = new uint[maxLength];

        for (int i = dataLength - 1, j = 0; i >= 0; i--, j++)
            data[j] = inData[i];

        while (dataLength > 1 && data[dataLength - 1] == 0)
            dataLength--;
    }


    /// <summary>
    /// Cast a type long value to type BigInteger value
    /// </summary>
    /// <param name="value">A long value</param>
    public static implicit operator BigInt(long value)
    {
        return new BigInt(value);
    }


    /// <summary>
    /// Cast a type ulong value to type BigInteger value
    /// </summary>
    /// <param name="value">An unsigned long value</param>
    public static implicit operator BigInt(ulong value)
    {
        return new BigInt(value);
    }


    /// <summary>
    /// Cast a type int value to type BigInteger value
    /// </summary>
    /// <param name="value">An int value</param>
    public static implicit operator BigInt(int value)
    {
        return new BigInt((long) value);
    }


    /// <summary>
    /// Cast a type uint value to type BigInteger value
    /// </summary>
    /// <param name="value">An unsigned int value</param>
    public static implicit operator BigInt(uint value)
    {
        return new BigInt((ulong) value);
    }


    /// <summary>
    /// Overloading of addition operator
    /// </summary>
    /// <param name="bi1">First BigInteger</param>
    /// <param name="bi2">Second BigInteger</param>
    /// <returns>Result of the addition of 2 BigIntegers</returns>
    public static BigInt operator +(BigInt bi1, BigInt bi2)
    {
        var result = new BigInt()
        {
            dataLength = bi1.dataLength > bi2.dataLength ? bi1.dataLength : bi2.dataLength
        };

        long carry = 0;
        for (var i = 0; i < result.dataLength; i++)
        {
            var sum = (long) bi1.data[i] + (long) bi2.data[i] + carry;
            carry = sum >> 32;
            result.data[i] = (uint) (sum & 0xFFFFFFFF);
        }

        if (carry != 0 && result.dataLength < maxLength)
        {
            result.data[result.dataLength] = (uint) carry;
            result.dataLength++;
        }

        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;


        // overflow проверка
        var lastPos = maxLength - 1;
        if ((bi1.data[lastPos] & 0x80000000) == (bi2.data[lastPos] & 0x80000000) &&
            (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
            throw new ArithmeticException();

        return result;
    }


    /// <summary>
    /// Overloading of the unary ++ operator, which increments BigInteger by 1
    /// </summary>
    /// <param name="bi1">A BigInteger</param>
    /// <returns>Incremented BigInteger</returns>
    public static BigInt operator ++(BigInt bi1)
    {
        var result = new BigInt(bi1);

        long val, carry = 1;
        var index = 0;

        while (carry != 0 && index < maxLength)
        {
            val = (long) result.data[index];
            val++;

            result.data[index] = (uint) (val & 0xFFFFFFFF);
            carry = val >> 32;

            index++;
        }

        if (index > result.dataLength)
            result.dataLength = index;
        else
            while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
                result.dataLength--;

        // overflow check
        var lastPos = maxLength - 1;

        // overflow if initial value was +ve but ++ caused a sign
        // change to negative.

        if ((bi1.data[lastPos] & 0x80000000) == 0 &&
            (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
            throw new ArithmeticException("Overflow in ++.");
        return result;
    }


    /// <summary>
    /// Overloading of subtraction operator
    /// </summary>
    /// <param name="bi1">First BigInteger</param>
    /// <param name="bi2">Second BigInteger</param>
    /// <returns>Result of the subtraction of 2 BigIntegers</returns>
    public static BigInt operator -(BigInt bi1, BigInt bi2)
    {
        var result = new BigInt()
        {
            dataLength = bi1.dataLength > bi2.dataLength ? bi1.dataLength : bi2.dataLength
        };

        long carryIn = 0;
        for (var i = 0; i < result.dataLength; i++)
        {
            long diff;

            diff = (long) bi1.data[i] - (long) bi2.data[i] - carryIn;
            result.data[i] = (uint) (diff & 0xFFFFFFFF);

            carryIn = diff < 0 ? 1 : 0;
        }

        // roll over to negative
        if (carryIn != 0)
        {
            for (var i = result.dataLength; i < maxLength; i++)
                result.data[i] = 0xFFFFFFFF;
            result.dataLength = maxLength;
        }

        // fixed in v1.03 to give correct datalength for a - (-b)
        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;

        // overflow check

        var lastPos = maxLength - 1;
        if ((bi1.data[lastPos] & 0x80000000) != (bi2.data[lastPos] & 0x80000000) &&
            (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
            throw new ArithmeticException();

        return result;
    }


    /// <summary>
    /// Overloading of the unary -- operator, decrements BigInteger by 1
    /// </summary>
    /// <param name="bi1">A BigInteger</param>
    /// <returns>Decremented BigInteger</returns>
    public static BigInt operator --(BigInt bi1)
    {
        var result = new BigInt(bi1);

        var carryIn = true;
        var index = 0;

        while (carryIn && index < maxLength)
        {
            var val = (long) result.data[index];
            val--;

            result.data[index] = (uint) (val & 0xFFFFFFFF);

            if (val >= 0)
                carryIn = false;

            index++;
        }

        if (index > result.dataLength)
            result.dataLength = index;

        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;

        // overflow check
        var lastPos = maxLength - 1;

        // overflow if initial value was -ve but -- caused a sign
        // change to positive.

        if ((bi1.data[lastPos] & 0x80000000) != 0 &&
            (result.data[lastPos] & 0x80000000) != (bi1.data[lastPos] & 0x80000000))
            throw new ArithmeticException("Underflow in --.");

        return result;
    }


    /// <summary>
    /// Перегрузка бинарного оператора умножения
    /// </summary>
    /// <param name="first">Первый BigInteger</param>
    /// <param name="second">Второй BigInteger</param>
    /// <returns>Произведение</returns>
    public static BigInt operator *(BigInt first, BigInt second)
    {
        var lastPos = maxLength - 1;
        bool bi1Neg = false, bi2Neg = false;

        // 0x8 AND Sign проверка
        try
        {
            if ((first.data[lastPos] & 0x80000000) != 0) // first negative
            {
                bi1Neg = true;
                first = -first;
            }

            if ((second.data[lastPos] & 0x80000000) != 0) // second negative
            {
                bi2Neg = true;
                second = -second;
            }
        }
        catch (Exception)
        {
            // ignored
        }

        var result = new BigInt();

        // Умножение абсолютной велечины переменных
        try
        {
            for (var i = 0; i < first.dataLength; i++)
            {
                if (first.data[i] == 0) continue;

                ulong mcarry = 0;
                for (int j = 0, k = i; j < second.dataLength; j++, k++)
                {
                    var val =  first.data[i] *  second.data[j] +
                               result.data[k] + mcarry;

                    result.data[k] = (uint) (val & 0xFFFFFFFF);
                    mcarry = val >> 32;
                }

                if (mcarry != 0)
                    result.data[i + second.dataLength] = (uint) mcarry;
            }
        }
        catch (Exception)
        {
            throw new ArithmeticException("Multiplication overflow.");
        }


        result.dataLength = first.dataLength + second.dataLength;
        if (result.dataLength > maxLength)
            result.dataLength = maxLength;

        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;

        // overflow check (result is -ve)
        if ((result.data[lastPos] & 0x80000000) != 0)
        {
            if (bi1Neg == bi2Neg || result.data[lastPos] != 0x80000000)
                throw new ArithmeticException("Multiplication overflow.");

            if (result.dataLength == 1)
            {
                return result;
            }

            var isMaxNeg = true;
            for (var i = 0; i < result.dataLength - 1 && isMaxNeg; i++)
                if (result.data[i] != 0)
                    isMaxNeg = false;

            if (isMaxNeg)
                return result;

            throw new ArithmeticException("Multiplication overflow.");
        }

        // Определение итогового знака
        if (bi1Neg != bi2Neg)
            return -result;

        return result;
    }


    /// <summary>
    /// Overloading of the unary &lt;&lt; operator (left shift)
    /// </summary>
    /// <remarks>
    /// Shifting by a negative number is an undefined behaviour (UB).
    /// </remarks>
    /// <param name="bi1">A BigInteger</param>
    /// <param name="shiftVal">Left shift by shiftVal bit</param>
    /// <returns>Left-shifted BigInteger</returns>
    public static BigInt operator <<(BigInt bi1, int shiftVal)
    {
        var result = new BigInt(bi1);
        result.dataLength = shiftLeft(result.data, shiftVal);

        return result;
    }

    // Младший байт
    private static int shiftLeft(uint[] buffer, int shiftVal)
    {
        var shiftAmount = 32;
        var bufLen = buffer.Length;

        while (bufLen > 1 && buffer[bufLen - 1] == 0)
            bufLen--;

        for (var count = shiftVal; count > 0;)
        {
            if (count < shiftAmount)
                shiftAmount = count;

            ulong carry = 0;
            for (var i = 0; i < bufLen; i++)
            {
                var val = (ulong) buffer[i] << shiftAmount;
                val |= carry;

                buffer[i] = (uint) (val & 0xFFFFFFFF);
                carry = val >> 32;
            }

            if (carry != 0)
                if (bufLen + 1 <= buffer.Length)
                {
                    buffer[bufLen] = (uint) carry;
                    bufLen++;
                }

            count -= shiftAmount;
        }

        return bufLen;
    }


    /// <summary>
    /// Overloading of the unary &gt;&gt; operator (right shift)
    /// </summary>
    /// <remarks>
    /// Shifting by a negative number is an undefined behaviour (UB).
    /// </remarks>
    /// <param name="bi1">A BigInteger</param>
    /// <param name="shiftVal">Right shift by shiftVal bit</param>
    /// <returns>Right-shifted BigInteger</returns>
    public static BigInt operator >>(BigInt bi1, int shiftVal)
    {
        var result = new BigInt(bi1);
        result.dataLength = shiftRight(result.data, shiftVal);


        if ((bi1.data[maxLength - 1] & 0x80000000) == 0) return result;
        for (var i = maxLength - 1; i >= result.dataLength; i--)
            result.data[i] = 0xFFFFFFFF;

        var mask = 0x80000000;
        for (var i = 0; i < 32; i++)
        {
            if ((result.data[result.dataLength - 1] & mask) != 0)
                break;

            result.data[result.dataLength - 1] |= mask;
            mask >>= 1;
        }

        result.dataLength = maxLength;

        return result;
    }

    //Старший байт
    private static int shiftRight(uint[] buffer, int shiftVal)
    {
        var shiftAmount = 32;
        var invShift = 0;
        var bufLen = buffer.Length;

        while (bufLen > 1 && buffer[bufLen - 1] == 0)
            bufLen--;

        for (var count = shiftVal; count > 0;)
        {
            if (count < shiftAmount)
            {
                shiftAmount = count;
                invShift = 32 - shiftAmount;
            }

            ulong carry = 0;
            for (var i = bufLen - 1; i >= 0; i--)
            {
                var val = (ulong) buffer[i] >> shiftAmount;
                val |= carry;

                carry = ((ulong) buffer[i] << invShift) & 0xFFFFFFFF;
                buffer[i] = (uint) val;
            }

            count -= shiftAmount;
        }

        while (bufLen > 1 && buffer[bufLen - 1] == 0)
            bufLen--;

        return bufLen;
    }


    /// <summary>
    /// Overloading of the NEGATE operator (2's complement)
    /// </summary>
    /// <param name="bi1">A BigInteger</param>
    /// <returns>Negated BigInteger or default BigInteger value if bi1 is 0</returns>
    public static BigInt operator -(BigInt bi1)
    {
        // handle neg of zero separately since it'll cause an overflow
        // if we proceed.

        if (bi1.dataLength == 1 && bi1.data[0] == 0)
            return new BigInt();

        var result = new BigInt(bi1);

        // 1's complement
        for (var i = 0; i < maxLength; i++)
            result.data[i] = (uint) ~bi1.data[i];

        // add one to result of 1's complement
        long val, carry = 1;
        var index = 0;

        while (carry != 0 && index < maxLength)
        {
            val = (long) result.data[index];
            val++;

            result.data[index] = (uint) (val & 0xFFFFFFFF);
            carry = val >> 32;

            index++;
        }

        if ((bi1.data[maxLength - 1] & 0x80000000) == (result.data[maxLength - 1] & 0x80000000))
            throw new ArithmeticException("Overflow in negation.\n");

        result.dataLength = maxLength;

        while (result.dataLength > 1 && result.data[result.dataLength - 1] == 0)
            result.dataLength--;
        return result;
    }


    /// <summary>
    /// Overloading of equality operator, allows comparing 2 BigIntegers with == operator
    /// </summary>
    /// <param name="bi1">First BigInteger</param>
    /// <param name="bi2">Second BigInteger</param>
    /// <returns>Boolean result of the comparison</returns>
    public static bool operator ==(BigInt bi1, BigInt bi2)
    {
        return bi1.Equals(bi2);
    }


    /// <summary>
    /// Overloading of not equal operator, allows comparing 2 BigIntegers with != operator
    /// </summary>
    /// <param name="bi1">First BigInteger</param>
    /// <param name="bi2">Second BigInteger</param>
    /// <returns>Boolean result of the comparison</returns>
    public static bool operator !=(BigInt bi1, BigInt bi2)
    {
        return !bi1.Equals(bi2);
    }


    /// <summary>
    /// Overriding of Equals method, allows comparing BigInteger with an arbitary object
    /// </summary>
    /// <param name="o">Input object, to be casted into BigInteger type for comparison</param>
    /// <returns>Boolean result of the comparison</returns>
    public override bool Equals(object o)
    {
        var bi = (BigInt) o;

        if (dataLength != bi.dataLength)
            return false;

        for (var i = 0; i < dataLength; i++)
            if (data[i] != bi.data[i])
                return false;
        return true;
    }


    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }


    /// <summary>
    /// Overloading of greater than operator, allows comparing 2 BigIntegers with &gt; operator
    /// </summary>
    /// <param name="bi1">First BigInteger</param>
    /// <param name="bi2">Second BigInteger</param>
    /// <returns>Boolean result of the comparison</returns>
    public static bool operator >(BigInt bi1, BigInt bi2)
    {
        var pos = maxLength - 1;

        // bi1 is negative, bi2 is positive
        if ((bi1.data[pos] & 0x80000000) != 0 && (bi2.data[pos] & 0x80000000) == 0)
            return false;

        // bi1 is positive, bi2 is negative
        else if ((bi1.data[pos] & 0x80000000) == 0 && (bi2.data[pos] & 0x80000000) != 0)
            return true;

        // same sign
        var len = bi1.dataLength > bi2.dataLength ? bi1.dataLength : bi2.dataLength;
        for (pos = len - 1; pos >= 0 && bi1.data[pos] == bi2.data[pos]; pos--) ;

        if (!(pos >= 0)) return false;
        return bi1.data[pos] > bi2.data[pos];
    }


    /// <summary>
    /// Overloading of greater than operator, allows comparing 2 BigIntegers with &lt; operator
    /// </summary>
    /// <param name="bi1">First BigInteger</param>
    /// <param name="bi2">Second BigInteger</param>
    /// <returns>Boolean result of the comparison</returns>
    public static bool operator <(BigInt bi1, BigInt bi2)
    {
        var pos = maxLength - 1;

        // bi1 is negative, bi2 is positive
        if ((bi1.data[pos] & 0x80000000) != 0 && (bi2.data[pos] & 0x80000000) == 0)
            return true;

        // bi1 is positive, bi2 is negative
        else if ((bi1.data[pos] & 0x80000000) == 0 && (bi2.data[pos] & 0x80000000) != 0)
            return false;

        // same sign
        var len = bi1.dataLength > bi2.dataLength ? bi1.dataLength : bi2.dataLength;
        for (pos = len - 1; pos >= 0 && bi1.data[pos] == bi2.data[pos]; pos--) ;

        if (!(pos >= 0)) return false;
        return bi1.data[pos] < bi2.data[pos];
    }


    /// <summary>
    /// Overloading of greater than or equal to operator, allows comparing 2 BigIntegers with &gt;= operator
    /// </summary>
    /// <param name="bi1">First BigInteger</param>
    /// <param name="bi2">Second BigInteger</param>
    /// <returns>Boolean result of the comparison</returns>
    public static bool operator >=(BigInt bi1, BigInt bi2)
    {
        return bi1 == bi2 || bi1 > bi2;
    }


    /// <summary>
    /// Overloading of less than or equal to operator, allows comparing 2 BigIntegers with &lt;= operator
    /// </summary>
    /// <param name="bi1">First BigInteger</param>
    /// <param name="bi2">Second BigInteger</param>
    /// <returns>Boolean result of the comparison</returns>
    public static bool operator <=(BigInt bi1, BigInt bi2)
    {
        return bi1 == bi2 || bi1 < bi2;
    }


    /// <summary>
    /// Returns the modulo inverse of this
    /// </summary>
    /// <remarks>
    /// Throws ArithmeticException if the inverse does not exist.  (i.e. gcd(this, modulus) != 1)
    /// </remarks>
    /// <param name="modulus"></param>
    /// <returns>Modulo inverse of this</returns>
    public BigInt modInverse(BigInt modulus)
    {
        BigInt[] p = {0, 1};
        var q = new BigInt[2]; // quotients
        BigInt[] r = {0, 0}; // remainders

        var step = 0;

        var a = modulus;
        var b = this;

        while (b.dataLength > 1 || b.dataLength == 1 && b.data[0] != 0)
        {
            var quotient = new BigInt();
            var remainder = new BigInt();

            if (step > 1)
            {
                var pval = (p[0] - p[1] * q[0]) % modulus;
                p[0] = p[1];
                p[1] = pval;
            }

            if (b.dataLength == 1)
                singleByteDivide(a, b, quotient, remainder);
            else
                multiByteDivide(a, b, quotient, remainder);

            q[0] = q[1];
            r[0] = r[1];
            q[1] = quotient;
            r[1] = remainder;

            a = b;
            b = remainder;

            step++;
        }

        if (r[0].dataLength > 1 || r[0].dataLength == 1 && r[0].data[0] != 1)
            throw new ArithmeticException("No inverse!");

        var result = (p[0] - p[1] * q[0]) % modulus;

        if ((result.data[maxLength - 1] & 0x80000000) != 0)
            result += modulus; // get the least positive modulus

        return result;
    }

    //***********************************************************************
    // Private function that supports the division of two numbers with
    // a divisor that has more than 1 digit.
    //
    // Algorithm taken from [1]
    //***********************************************************************
    private static void multiByteDivide(BigInt bi1, BigInt bi2,
        BigInt outQuotient, BigInt outRemainder)
    {
        var result = new uint[maxLength];

        var remainderLen = bi1.dataLength + 1;
        var remainder = new uint[remainderLen];

        var mask = 0x80000000;
        var val = bi2.data[bi2.dataLength - 1];
        int shift = 0, resultPos = 0;

        while (mask != 0 && (val & mask) == 0)
        {
            shift++;
            mask >>= 1;
        }

        for (var i = 0; i < bi1.dataLength; i++)
            remainder[i] = bi1.data[i];
        shiftLeft(remainder, shift);
        bi2 <<= shift;

        var j = remainderLen - bi2.dataLength;
        var pos = remainderLen - 1;

        ulong firstDivisorByte = bi2.data[bi2.dataLength - 1];
        ulong secondDivisorByte = bi2.data[bi2.dataLength - 2];

        var divisorLen = bi2.dataLength + 1;
        var dividendPart = new uint[divisorLen];

        while (j > 0)
        {
            var dividend = ((ulong) remainder[pos] << 32) + (ulong) remainder[pos - 1];

            var q_hat = dividend / firstDivisorByte;
            var r_hat = dividend % firstDivisorByte;

            var done = false;
            while (!done)
            {
                done = true;

                if (q_hat != 0x100000000 && q_hat * secondDivisorByte <= (r_hat << 32) + remainder[pos - 2]) continue;
                q_hat--;
                r_hat += firstDivisorByte;

                if (r_hat < 0x100000000)
                    done = false;
            }

            for (var h = 0; h < divisorLen; h++)
                dividendPart[h] = remainder[pos - h];

            var kk = new BigInt(dividendPart);
            var ss = bi2 * (long) q_hat;

            while (ss > kk)
            {
                q_hat--;
                ss -= bi2;
            }

            var yy = kk - ss;

            for (var h = 0; h < divisorLen; h++)
                remainder[pos - h] = yy.data[bi2.dataLength - h];

            result[resultPos++] = (uint) q_hat;

            pos--;
            j--;
        }

        outQuotient.dataLength = resultPos;
        var y = 0;
        for (var x = outQuotient.dataLength - 1; x >= 0; x--, y++)
            outQuotient.data[y] = result[x];
        for (; y < maxLength; y++)
            outQuotient.data[y] = 0;

        while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)
            outQuotient.dataLength--;

        if (outQuotient.dataLength == 0)
            outQuotient.dataLength = 1;

        outRemainder.dataLength = shiftRight(remainder, shift);

        for (y = 0; y < outRemainder.dataLength; y++)
            outRemainder.data[y] = remainder[y];
        for (; y < maxLength; y++)
            outRemainder.data[y] = 0;
    }


    //***********************************************************************
    // Private function that supports the division of two numbers with
    // a divisor that has only 1 digit.
    //***********************************************************************
    private static void singleByteDivide(BigInt bi1, BigInt bi2,
        BigInt outQuotient, BigInt outRemainder)
    {
        var result = new uint[maxLength];
        var resultPos = 0;

        // copy dividend to reminder
        for (var i = 0; i < maxLength; i++)
            outRemainder.data[i] = bi1.data[i];
        outRemainder.dataLength = bi1.dataLength;

        while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)
            outRemainder.dataLength--;

        var divisor = (ulong) bi2.data[0];
        var pos = outRemainder.dataLength - 1;
        var dividend = (ulong) outRemainder.data[pos];

        if (dividend >= divisor)
        {
            var quotient = dividend / divisor;
            result[resultPos++] = (uint) quotient;

            outRemainder.data[pos] = (uint) (dividend % divisor);
        }

        pos--;

        while (pos >= 0)
        {
            dividend = ((ulong) outRemainder.data[pos + 1] << 32) + (ulong) outRemainder.data[pos];
            var quotient = dividend / divisor;
            result[resultPos++] = (uint) quotient;

            outRemainder.data[pos + 1] = 0;
            outRemainder.data[pos--] = (uint) (dividend % divisor);
        }

        outQuotient.dataLength = resultPos;
        var j = 0;
        for (var i = outQuotient.dataLength - 1; i >= 0; i--, j++)
            outQuotient.data[j] = result[i];
        for (; j < maxLength; j++)
            outQuotient.data[j] = 0;

        while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)
            outQuotient.dataLength--;

        if (outQuotient.dataLength == 0)
            outQuotient.dataLength = 1;

        while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)
            outRemainder.dataLength--;
    }


    /// <summary>
    /// Overloading of division operator
    /// </summary>
    /// <remarks>The dataLength of the divisor's absolute value must be less than maxLength</remarks>
    /// <param name="bi1">Dividend</param>
    /// <param name="bi2">Divisor</param>
    /// <returns>Quotient of the division</returns>
    public static BigInt operator /(BigInt bi1, BigInt bi2)
    {
        var quotient = new BigInt();
        var remainder = new BigInt();

        var lastPos = maxLength - 1;
        bool divisorNeg = false, dividendNeg = false;

        if ((bi1.data[lastPos] & 0x80000000) != 0) // bi1 negative
        {
            bi1 = -bi1;
            dividendNeg = true;
        }

        if ((bi2.data[lastPos] & 0x80000000) != 0) // bi2 negative
        {
            bi2 = -bi2;
            divisorNeg = true;
        }

        if (bi1 < bi2) return quotient;

        if (bi2.dataLength == 1)
            singleByteDivide(bi1, bi2, quotient, remainder);
        else
            multiByteDivide(bi1, bi2, quotient, remainder);

        if (dividendNeg != divisorNeg)
            return -quotient;

        return quotient;
    }


    /// <summary>
    /// Overloading of modulus operator
    /// </summary>
    /// <remarks>The dataLength of the divisor's absolute value must be less than maxLength</remarks>
    /// <param name="bi1">Dividend</param>
    /// <param name="bi2">Divisor</param>
    /// <returns>Remainder of the division</returns>
    public static BigInt operator %(BigInt bi1, BigInt bi2)
    {
        var quotient = new BigInt();
        var remainder = new BigInt(bi1);

        var lastPos = maxLength - 1;
        var dividendNeg = false;

        if ((bi1.data[lastPos] & 0x80000000) != 0) // bi1 negative
        {
            bi1 = -bi1;
            dividendNeg = true;
        }

        if ((bi2.data[lastPos] & 0x80000000) != 0) // bi2 negative
            bi2 = -bi2;

        if (bi1 < bi2) return remainder;

        if (bi2.dataLength == 1)
            singleByteDivide(bi1, bi2, quotient, remainder);
        else
            multiByteDivide(bi1, bi2, quotient, remainder);

        if (dividendNeg)
            return -remainder;

        return remainder;
    }


    /// <summary>
    /// Compare this and a BigInteger and find the maximum one
    /// </summary>
    /// <param name="bi">BigInteger to be compared with this</param>
    /// <returns>The bigger value of this and bi</returns>
    public BigInt max(BigInt bi)
    {
        return this > bi ? new BigInt(this) : new BigInt(bi);
    }


    /// <summary>
    /// Сравнивает два числа и возвращает наименьшее
    /// </summary>
    /// <param name="bi">BigInt для сравнения</param>
    /// <returns>Наименьшее</returns>
    public BigInt min(BigInt bi)
    {
        return this < bi ? new BigInt(this) : new BigInt(bi);
    }


    /// <summary>
    /// Возвращает модуль числа
    /// </summary>
    /// <returns>Модуль числа</returns>
    public BigInt abs()
    {
        if ((data[maxLength - 1] & 0x80000000) != 0)
            return -this;
        return new BigInt(this);
    }


    /// <summary>
    /// Возвращает строковое представление числа в 10-ной СИ
    /// </summary>
    /// <returns>string representation of the BigInteger</returns>
    public override string ToString()
    {
        return ToString(10);
    }


    /// <summary>
    /// Возвращает строковое представление числа в выбранной системе исчисления
    /// </summary>
    /// <param name="radix">СИ</param>
    /// <returns>string representation of the BigInteger in [sign][magnitude] format</returns>
    public string ToString(int radix)
    {
        if (radix < 2 || radix > 36)
            throw new ArgumentException("Radix must be >= 2 and <= 36");

        var charSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var result = "";

        var a = this;

        var negative = false;
        if ((a.data[maxLength - 1] & 0x80000000) != 0)
        {
            negative = true;
            try
            {
                a = -a;
            }
            catch (Exception)
            {
            }
        }

        var quotient = new BigInt();
        var remainder = new BigInt();
        var biRadix = new BigInt(radix);

        if (a.dataLength == 1 && a.data[0] == 0)
        {
            result = "0";
        }
        else
        {
            while (a.dataLength > 1 || a.dataLength == 1 && a.data[0] != 0)
            {
                singleByteDivide(a, biRadix, quotient, remainder);

                if (remainder.data[0] < 10)
                    result = remainder.data[0] + result;
                else
                    result = charSet[(int) remainder.data[0] - 10] + result;

                a = quotient;
            }

            if (negative)
                result = "-" + result;
        }

        return result;
    }


    /// <summary>
    /// Возвращает наибольший общий делитель(this, bi)
    /// </summary>
    /// <param name="bi"></param>
    /// <returns>Возвращает наибольший общий делитель текущего числа и bi</returns>
    public BigInt gcd(BigInt bi)
    {
        BigInt x;
        BigInt y;

        if ((data[maxLength - 1] & 0x80000000) != 0) // negative
            x = -this;
        else
            x = this;

        if ((bi.data[maxLength - 1] & 0x80000000) != 0) // negative
            y = -bi;
        else
            y = bi;

        var g = y;

        while (x.dataLength > 1 || x.dataLength == 1 && x.data[0] != 0)
        {
            g = x;
            x = y % x;
            y = g;
        }

        return g;
    }

    /// <summary>
    /// Вычисляет символ Якоби для 2 BigInt a и b
    /// </summary>
    /// <param name="a">Любой BigInt</param>
    /// <param name="b">Нечетный BigInt</param>
    /// <returns>Символ Якоби</returns>
    public static int Jacobi(BigInt a, BigInt b)
    {
        while (true)
        {
            // Символ Якоби существует только для нечетных целых
            if ((b.data[0] & 0x1) == 0) throw new ArgumentException("Jacobi defined only for odd integers.");

            if (a >= b) a %= b;
            switch (a.dataLength)
            {
                case 1 when a.data[0] == 0:
                    return 0; // a == 0
                case 1 when a.data[0] == 1:
                    return 1; // a == 1
            }

            if (a < 0)
            {
                if (((b - 1).data[0] & 0x2) != 0) return -Jacobi(-a, b);
                a = -a;
                continue;
            }

            var e = 0;
            for (var index = 0; index < a.dataLength; index++)
            {
                uint mask = 0x01;

                for (var i = 0; i < 32; i++)
                {
                    if ((a.data[index] & mask) != 0)
                    {
                        index = a.dataLength; // to break the outer loop
                        break;
                    }

                    mask <<= 1;
                    e++;
                }
            }

            var a1 = a >> e;

            var s = 1;
            if ((e & 0x1) != 0 && ((b.data[0] & 0x7) == 3 || (b.data[0] & 0x7) == 5)) s = -1;

            if ((b.data[0] & 0x3) == 3 && (a1.data[0] & 0x3) == 3) s = -s;

            if (a1.dataLength == 1 && a1.data[0] == 1) return s;
            return s * Jacobi(b % a1, a1);
        }
    }
}