using System;

public class BigInt
{
    // Максимальная длинна хранения
    private const int maxLength = 42000;

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


            if (posValue >= radix) throw new ArithmeticException("Invalid string in constructor.");

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


    public static implicit operator BigInt(long value)
    {
        return new BigInt(value);
    }


    public static implicit operator BigInt(ulong value)
    {
        return new BigInt(value);
    }


    public static implicit operator BigInt(int value)
    {
        return new BigInt((long) value);
    }


    public static implicit operator BigInt(uint value)
    {
        return new BigInt((ulong) value);
    }


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
                    var val = first.data[i] * second.data[j] +
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

            if (result.dataLength == 1) return result;

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


    public static BigInt operator -(BigInt bi1)
    {
        if (bi1.dataLength == 1 && bi1.data[0] == 0)
            return new BigInt();

        var result = new BigInt(bi1);


        for (var i = 0; i < maxLength; i++)
            result.data[i] = (uint) ~bi1.data[i];

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


    public static bool operator ==(BigInt bi1, BigInt bi2)
    {
        return bi1.Equals(bi2);
    }


    public static bool operator !=(BigInt bi1, BigInt bi2)
    {
        return !bi1.Equals(bi2);
    }


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


    public static bool operator >=(BigInt bi1, BigInt bi2)
    {
        return bi1 == bi2 || bi1 > bi2;
    }


    public static bool operator <=(BigInt bi1, BigInt bi2)
    {
        return bi1 == bi2 || bi1 < bi2;
    }


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


    public BigInt max(BigInt bi)
    {
        return this > bi ? new BigInt(this) : new BigInt(bi);
    }


    public BigInt min(BigInt bi)
    {
        return this < bi ? new BigInt(this) : new BigInt(bi);
    }


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

    private int bitCount()
    {
        while (dataLength > 1 && data[dataLength - 1] == 0)
            dataLength--;

        var value = data[dataLength - 1];
        var mask = 0x80000000;
        var bits = 32;

        while (bits > 0 && (value & mask) == 0)
        {
            bits--;
            mask >>= 1;
        }

        bits += (dataLength - 1) << 5;

        return bits == 0 ? 1 : bits;
    }


    public BigInt modPow(BigInt exp, BigInt n)
    {
        if ((exp.data[maxLength - 1] & 0x80000000) != 0)
            throw new ArithmeticException("Positive exponents only.");

        BigInt resultNum = 1;
        BigInt tempNum;
        var thisNegative = false;

        if ((data[maxLength - 1] & 0x80000000) != 0) // negative this
        {
            tempNum = -this % n;
            thisNegative = true;
        }
        else
        {
            tempNum = this % n; // ensures (tempNum * tempNum) < b^(2k)
        }

        if ((n.data[maxLength - 1] & 0x80000000) != 0) // negative n
            n = -n;

        // calculate constant = b^(2k) / m
        var constant = new BigInt();

        var i = n.dataLength << 1;
        constant.data[i] = 0x00000001;
        constant.dataLength = i + 1;

        constant = constant / n;
        var totalBits = exp.bitCount();
        var count = 0;

        // perform squaring and multiply exponentiation
        for (var pos = 0; pos < exp.dataLength; pos++)
        {
            uint mask = 0x01;

            for (var index = 0; index < 32; index++)
            {
                if ((exp.data[pos] & mask) != 0)
                    resultNum = BarrettReduction(resultNum * tempNum, n, constant);

                mask <<= 1;

                tempNum = BarrettReduction(tempNum * tempNum, n, constant);


                if (tempNum.dataLength == 1 && tempNum.data[0] == 1)
                {
                    if (thisNegative && (exp.data[0] & 0x1) != 0) //odd exp
                        return -resultNum;
                    return resultNum;
                }

                count++;
                if (count == totalBits)
                    break;
            }
        }

        if (thisNegative && (exp.data[0] & 0x1) != 0) //odd exp
            return -resultNum;

        return resultNum;
    }

    private BigInt BarrettReduction(BigInt x, BigInt n, BigInt constant)
    {
        int k = n.dataLength,
            kPlusOne = k + 1,
            kMinusOne = k - 1;

        var q1 = new BigInt();


        for (int i = kMinusOne, j = 0; i < x.dataLength; i++, j++)
            q1.data[j] = x.data[i];
        q1.dataLength = x.dataLength - kMinusOne;
        if (q1.dataLength <= 0)
            q1.dataLength = 1;


        var q2 = q1 * constant;
        var q3 = new BigInt();


        for (int i = kPlusOne, j = 0; i < q2.dataLength; i++, j++)
            q3.data[j] = q2.data[i];
        q3.dataLength = q2.dataLength - kPlusOne;
        if (q3.dataLength <= 0)
            q3.dataLength = 1;


        var r1 = new BigInt();
        var lengthToCopy = x.dataLength > kPlusOne ? kPlusOne : x.dataLength;
        for (var i = 0; i < lengthToCopy; i++)
            r1.data[i] = x.data[i];
        r1.dataLength = lengthToCopy;


        // r2 = (q3 * n) mod b^(k+1)
        // partial multiplication of q3 and n

        var r2 = new BigInt();
        for (var i = 0; i < q3.dataLength; i++)
        {
            if (q3.data[i] == 0) continue;

            ulong mcarry = 0;
            var t = i;
            for (var j = 0; j < n.dataLength && t < kPlusOne; j++, t++)
            {
                // t = i + j
                var val = (ulong) q3.data[i] * (ulong) n.data[j] +
                          (ulong) r2.data[t] + mcarry;

                r2.data[t] = (uint) (val & 0xFFFFFFFF);
                mcarry = val >> 32;
            }

            if (t < kPlusOne)
                r2.data[t] = (uint) mcarry;
        }

        r2.dataLength = kPlusOne;
        while (r2.dataLength > 1 && r2.data[r2.dataLength - 1] == 0)
            r2.dataLength--;

        r1 -= r2;
        if ((r1.data[maxLength - 1] & 0x80000000) != 0) // negative
        {
            var val = new BigInt();
            val.data[kPlusOne] = 0x00000001;
            val.dataLength = kPlusOne + 1;
            r1 += val;
        }

        while (r1 >= n)
            r1 -= n;

        return r1;
    }
}