using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protean.Tools
{
    using System;
    using System.Diagnostics;
    public partial class Number
    { 
    public class Random
    {
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908B0DFU;
        private const uint UPPER_MASK = 0x80000000U;
        private const uint LOWER_MASK = 0x7FFFFFFFU;

        private uint[] mt = new uint[624];
        private int mti = N + 1;
        /// <summary>
        /// Create a new Mersenne Twister random number generator.
        /// </summary>
        public Random() : this(System.Convert.ToUInt32(DateTime.Now.Millisecond))
        {
        }

        /// <summary>
        /// Create a new Mersenne Twister random number generator with a
        /// particular seed.
        /// </summary>
        /// <param name="seed">The seed for the generator.</param>
        [CLSCompliant(false)]
        public Random(uint seed)
        {
            mt[0] = seed;
            for (mti = 1; mti <= N - 1; mti++)
                mt[mti] = System.Convert.ToUInt32((1812433253UL * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + System.Convert.ToUInt32(mti)) & 0xFFFFFFFFUL);
        }

        /// <summary>
        /// Create a new Mersenne Twister random number generator with a
        /// particular initial key.
        /// </summary>
        /// <param name="initialKey">The initial key.</param>
        [CLSCompliant(false)]
        public Random(uint[] initialKey) : this(19650218U)
        {
            int i, j, k;
            i = 1; j = 0;
            k = System.Convert.ToInt32(N > initialKey.Length? N: initialKey.Length);
            for (k = k; k >= 1; k += -1)
            {
                mt[i] = System.Convert.ToUInt32(((mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525UL)) + initialKey[j] + System.Convert.ToUInt32(j)) & 0xFFFFFFFFU);
                i += 1; j += 1;
                if (i >= N)
                {
                    mt[0] = mt[N - 1]; i = 1;
                }
                if (j >= initialKey.Length)
                    j = 0;
            }
            for (k = N - 1; k >= 1; k += -1)
            {
                mt[i] = System.Convert.ToUInt32(((mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1566083941UL)) - System.Convert.ToUInt32(i)) & 0xFFFFFFFFU);
                i += 1;
                if (i >= N)
                {
                    mt[0] = mt[N - 1]; i = 1;
                }
            }
            mt[0] = 0x80000000U;
        }

        /// <summary>
        /// Generates a random number between 0 and System.UInt32.MaxValue.
        /// </summary>
        [CLSCompliant(false)]
        public uint NextUInt32()
        {
            uint y;

            //UInteger mag01() As UInteger = { &H0UI, MATRIX_A };

            uint[] mag01 = { 0x0U, MATRIX_A };

            if (mti >= N)
            {
                int kk;
                Debug.Assert(mti != N + 1, "Failed initialization");
                for (kk = 0; kk <= N - M - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[System.Convert.ToInt32(y & 0x1)];
                }
                for (kk = kk; kk <= N - 2; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[System.Convert.ToInt32(y & 0x1)];
                }
                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[System.Convert.ToInt32(y & 0x1)];
                mti = 0;
            }
            y = mt[mti];
            mti += 1;
            // Tempering
            y = y ^ (y >> 11);
            y = y ^ ((y << 7) & 0x9D2C5680U);
            y = y ^ ((y << 15) & 0xEFC60000U);
            y = y ^ (y >> 18);
            return y;
        }

        /// <summary>
        /// Generates a random integer between 0 and System.Int32.MaxValue.
        /// </summary>
        public int Next()
        {
            return System.Convert.ToInt32(NextUInt32() >> 1);
        }

        /// <summary>
        /// Generates a random integer between 0 and maxValue.
        /// </summary>
        /// <param name="maxValue">The maximum value. Must be greater than zero.</param>
        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        /// <summary>
        /// Generates a random integer between minValue and maxValue.
        /// </summary>
        /// <param name="maxValue">The lower bound.</param>
        /// <param name="minValue">The upper bound.</param>
        public int Next(int minValue, int maxValue)
        {
            return System.Convert.ToInt32(Math.Floor((maxValue - minValue + 1) * NextDouble() + minValue));
        }

        /// <summary>
        /// Generates a random floating point number between 0 and 1.
        /// </summary>
        public double NextDouble()
        {
            return NextUInt32() * (1.0 / 4294967295.0);
        }
    }
    }
}
