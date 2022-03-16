using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AutoTorrentInspection.Util
{
    // ref: https://github.com/dsoprea/RijndaelPbkdf/blob/master/pprp/crypto.py 
    public static class Crypto
    {
        public static readonly int[,,] shifts =
        {
            {{0, 0}, {1, 3}, {2, 2}, {3, 1}},
            {{0, 0}, {1, 5}, {2, 4}, {3, 3}},
            {{0, 0}, {1, 7}, {3, 5}, {4, 4}}
        };
        // [keysize][block_size]
        public static readonly Dictionary<int, Dictionary<int, int>> num_rounds = new Dictionary<int, Dictionary<int, int>>
        {
            [16] = new Dictionary<int, int>
            {
                [16] = 10,
                [24] = 12,
                [32] = 14,
            },
            [24] = new Dictionary<int, int>
            {
                [16] = 12,
                [24] = 12,
                [32] = 14,
            },
            [32] = new Dictionary<int, int>
            {
                [16] = 14,
                [24] = 14,
                [32] = 14,
            },
        };

        public static int[] S = new int[256];
        public static int[] Si = new int[256];

        public static uint[] T1 = new uint[256];
        public static uint[] T2 = new uint[256];
        public static uint[] T3 = new uint[256];
        public static uint[] T4 = new uint[256];
        public static uint[] T5 = new uint[256];
        public static uint[] T6 = new uint[256];
        public static uint[] T7 = new uint[256];
        public static uint[] T8 = new uint[256];
        public static uint[] U1 = new uint[256];
        public static uint[] U2 = new uint[256];
        public static uint[] U3 = new uint[256];
        public static uint[] U4 = new uint[256];

        public static int[] rcon = new int[30];

        private static int[] alog = new int[256];
        private static int[] log = new int[256];

        static Crypto()
        {
            // produce log and alog tables, needed for multiplying in the
            // field GF(2^m) (generator = 3)
            alog[0] = 1;
            for (var i = 0; i < 255; ++i)
            {
                var j = (alog[i] << 1) ^ alog[i];
                if ((j & 0x100) != 0)
                {
                    j ^= 0x11b;
                }
                alog[i + 1] = j;
            }
            for (var i = 1; i < 255; ++i)
            {
                log[alog[i]] = i;
            }

            // substitution box based on F^{-1}(x)
            var box = new int[256, 8];
            box[1, 7] = 1;
            for (var i = 2; i < 256; ++i)
            {
                var j = alog[255 - log[i]];
                for (var t = 0; t < 8; ++t)
                {
                    box[i, t] = (j >> (7 - t)) & 0x01;
                }
            }

            int[,] A =
            {
                {1, 1, 1, 1, 1, 0, 0, 0},
                {0, 1, 1, 1, 1, 1, 0, 0},
                {0, 0, 1, 1, 1, 1, 1, 0},
                {0, 0, 0, 1, 1, 1, 1, 1},
                {1, 0, 0, 0, 1, 1, 1, 1},
                {1, 1, 0, 0, 0, 1, 1, 1},
                {1, 1, 1, 0, 0, 0, 1, 1},
                {1, 1, 1, 1, 0, 0, 0, 1}
            };
            var B = new int[] { 0, 1, 1, 0, 0, 0, 1, 1 };

            var cox = new int[256, 8];
            for (var i = 0; i < 256; ++i)
            {
                for (var t = 0; t < 8; ++t)
                {
                    cox[i, t] = B[t];
                    for (var j = 0; j < 8; ++j)
                    {
                        cox[i, t] ^= A[t, j] * box[i, j];
                    }
                }
            }

            for (var i = 0; i < 256; ++i)
            {
                S[i] = cox[i, 0] << 7;
                for (var t = 1; t < 8; ++t)
                {
                    S[i] ^= cox[i, t] << (7 - t);
                }
                Si[S[i] & 0xff] = i;
            }

            var G = new[]
            {
                new[] {2, 1, 1, 3},
                new[] {3, 2, 1, 1},
                new[] {1, 3, 2, 1},
                new[] {1, 1, 3, 2}
            };


            var AA = new int[4, 8];
            for (var i = 0; i < 4; ++i)
            {
                for (var j = 0; j < 4; ++j)
                {
                    AA[i, j] = G[i][j];
                    AA[i, i + 4] = 1;
                }
            }


            for (var i = 0; i < 4; ++i)
            {
                var pivot = AA[i, i];
                if (pivot == 0)
                {
                    var t = i + 1;
                    while (AA[t, i] == 0 && t < 4)
                    {
                        t += 1;
                        Debug.Assert(t != 4, "G matrix must be invertible");
                        for (var j = 0; j < 8; ++j)
                        {
                            (AA[i, j], AA[t, j]) = (AA[t, j], AA[i, j]);
                        }
                        pivot = AA[i, i];
                    }
                }
                for (var j = 0; j < 8; ++j)
                {
                    if (AA[i, j] != 0)
                    {
                        AA[i, j] = alog[(255 + log[AA[i, j] & 0xFF] - log[pivot & 0xFF]) % 255];
                    }
                }
                for (var t = 0; t < 4; ++t)
                {
                    if (i != t)
                    {
                        for (var j = i + 1; j < 8; ++j)
                        {
                            AA[t, j] ^= mul(AA[i, j], AA[t, i]);
                        }
                        AA[t, i] = 0;
                    }
                }
            }

            var iG = new[]
            {
                new int[4], new int[4], new int[4], new int[4]
            };
            for (var i = 0; i < 4; ++i)
            {
                for (var j = 0; j < 4; ++j)
                {
                    iG[i][j] = AA[i, j + 4];
                }
            }


            for (var t = 0; t < 256; ++t)
            {
                var s = S[t];
                T1[t] = mul4(s, G[0]);
                T2[t] = mul4(s, G[1]);
                T3[t] = mul4(s, G[2]);
                T4[t] = mul4(s, G[3]);

                s = Si[t];
                T5[t] = mul4(s, iG[0]);
                T6[t] = mul4(s, iG[1]);
                T7[t] = mul4(s, iG[2]);
                T8[t] = mul4(s, iG[3]);

                U1[t] = mul4(t, iG[0]);
                U2[t] = mul4(t, iG[1]);
                U3[t] = mul4(t, iG[2]);
                U4[t] = mul4(t, iG[3]);
            }

            rcon[0] = 1;
            var r = 1;
            for (var t = 1; t < 30; ++t)
            {
                r = mul(2, r);
                rcon[t] = r;
            }
        }


        // multiply two elements of GF(2^m)
        private static int mul(int a, int b)
        {
            if (a == 0 || b == 0)
            {
                return 0;
            }
            return alog[(log[a & 0xFF] + log[b & 0xFF]) % 255];
        }

        private static uint mul4(int a, int[] bs)
        {
            if (a == 0)
            {
                return 0;
            }
            uint r = 0;
            foreach (var b in bs)
            {
                r <<= 8;
                if (b != 0)
                {
                    r = r | (uint)mul(a, b);
                }
            }
            return r;
        }


    }

    class Rijndael
    {
        private int block_size;
        private uint[,] Ke;
        private uint[,] Kd;

        public Rijndael(byte[] key, int block_size = 16)
        {
            if (block_size != 16 && block_size != 24 && block_size != 32)
            {
                throw new ArgumentException($"Invalid block size: {block_size}");
            }
            if (key.Length != 16 && key.Length != 24 && key.Length != 32)
            {
                throw new ArgumentException($"Invalid key size: {key}");
            }
            this.block_size = block_size;

            var ROUNDS = Crypto.num_rounds[key.Length][block_size];
            var BC = block_size / 4;
            // encryption round keys
            var Ke = new uint[ROUNDS + 1, BC];
            // decryption round keys
            var Kd = new uint[ROUNDS + 1, BC];
            var ROUND_KEY_COUNT = (ROUNDS + 1) * BC;
            var KC = key.Length / 4;

            // copy user material bytes into temporary ints
            var tk = new uint[KC];
            for (var i = 0; i < KC; ++i)
            {
                tk[i] = (uint)((key[i * 4] << 24) | (key[i * 4 + 1] << 16) |
                                (key[i * 4 + 2] << 8) | key[i * 4 + 3]);
            }

            // copy values into round key arrays
            var t = 0;
            var j = 0;
            while (j < KC && t < ROUND_KEY_COUNT)
            {
                Ke[t / BC, t % BC] = tk[j];
                Kd[ROUNDS - t / BC, t % BC] = tk[j];
                j += 1;
                t += 1;
            }
            uint tt;
            var rconpointer = 0;
            while (t < ROUND_KEY_COUNT)
            {
                // extrapolate using phi (the round key evolution function)
                tt = tk[KC - 1];
                tk[0] ^= (uint)((Crypto.S[(tt >> 16) & 0xFF] & 0xFF) << 24 ^
                                 (Crypto.S[(tt >> 8) & 0xFF] & 0xFF) << 16 ^
                                 (Crypto.S[tt & 0xFF] & 0xFF) << 8 ^
                                 (Crypto.S[(tt >> 24) & 0xFF] & 0xFF) ^
                                 (Crypto.rcon[rconpointer] & 0xFF) << 24);
                rconpointer += 1;
                if (KC != 8)
                {
                    for (var i = 1; i < KC; ++i)
                    {
                        tk[i] ^= tk[i - 1];
                    }
                }
                else
                {
                    for (var i = 1; i < KC / 2; ++i)
                    {
                        tk[i] ^= tk[i - 1];
                    }
                    tt = tk[KC / 2 - 1];
                    tk[KC / 2] ^= (uint)((Crypto.S[tt & 0xFF] & 0xFF) ^
                                          (Crypto.S[(tt >> 8) & 0xFF] & 0xFF) << 8 ^
                                          (Crypto.S[(tt >> 16) & 0xFF] & 0xFF) << 16 ^
                                          (Crypto.S[(tt >> 24) & 0xFF] & 0xFF) << 24);
                    for (var i = KC / 2 + 1; i < KC; ++i)
                    {
                        tk[i] ^= tk[i - 1];
                    }
                    // copy values into round key arrays
                    j = 0;
                    while (j < KC && t < ROUND_KEY_COUNT)
                    {
                        Ke[t / BC, t % BC] = tk[j];
                        Kd[ROUNDS - t / BC, t % BC] = tk[j];
                        j += 1;
                        t += 1;
                    }


                }
            }
            // inverse MixColumn where needed
            for (var r = 1; r < ROUNDS; ++r)
            {
                for (j = 0; j < BC; ++j)
                {
                    tt = Kd[r, j];
                    Kd[r, j] = Crypto.U1[(tt >> 24) & 0xFF] ^
                               Crypto.U2[(tt >> 16) & 0xFF] ^
                               Crypto.U3[(tt >> 8) & 0xFF] ^
                               Crypto.U4[tt & 0xFF];
                }
            }
            this.Ke = Ke;
            this.Kd = Kd;
        }

        public byte[] encrypt(byte[] plaintext)
        {
            if (plaintext.Length != this.block_size)
            {
                throw new ArgumentException($"wrong block length, expected {this.block_size} got {plaintext.Length}");
            }
            var Ke = this.Ke;

            var BC = block_size / 4;
            var ROUNDS = Ke.GetLength(0) - 1;
            var SC = BC == 4 ? 0 : BC == 6 ? 1 : 2;
            var s1 = Crypto.shifts[SC, 1, 0];
            var s2 = Crypto.shifts[SC, 2, 0];
            var s3 = Crypto.shifts[SC, 3, 0];
            var a = new uint[BC];

            // temporary work array
            var t = new uint[BC];
            // plaintext to ints + key
            for (var i = 0; i < BC; ++i)
            {
                t[i] = (uint)((plaintext[i * 4] << 24 |
                                plaintext[i * 4 + 1] << 16 |
                                plaintext[i * 4 + 2] << 8 |
                                plaintext[i * 4 + 3]) ^ Ke[0, i]);
            }
            // apply round transforms
            for (var r = 1; r < ROUNDS; ++r)
            {
                for (var i = 0; i < BC; ++i)
                {
                    a[i] = (uint)(Crypto.T1[(t[i] >> 24) & 0xFF] ^
                                   Crypto.T2[(t[(i + s1) % BC] >> 16) & 0xFF] ^
                                   Crypto.T3[(t[(i + s2) % BC] >> 8) & 0xFF] ^
                                   Crypto.T4[t[(i + s3) % BC] & 0xFF] ^ Ke[r, i]);
                }
                Array.Copy(a, t, BC);
            }
            // last round is special
            var result = new byte[BC * 4];
            for (var i = 0; i < BC; ++i)
            {
                var tt = Ke[ROUNDS, i];
                result[i * 4 + 0] = (byte)((Crypto.S[(t[i] >> 24) & 0xFF] ^ (tt >> 24)) & 0xFF);
                result[i * 4 + 1] = (byte)((Crypto.S[(t[(i + s1) % BC] >> 16) & 0xFF] ^ (tt >> 16)) & 0xFF);
                result[i * 4 + 2] = (byte)((Crypto.S[(t[(i + s2) % BC] >> 8) & 0xFF] ^ (tt >> 8)) & 0xFF);
                result[i * 4 + 3] = (byte)((Crypto.S[t[(i + s3) % BC] & 0xFF] ^ tt) & 0xFF);
            }
            return result;
        }

        public byte[] decrypt(byte[] ciphertext)
        {
            if (ciphertext.Length != this.block_size)
            {
                throw new ArgumentException($"wrong block length, expected {this.block_size} got {ciphertext.Length}");
            }
            var Kd = this.Kd;

            var BC = block_size / 4;
            var ROUNDS = Kd.GetLength(0) - 1;
            var SC = BC == 4 ? 0 : BC == 6 ? 1 : 2;
            var s1 = Crypto.shifts[SC, 1, 1];
            var s2 = Crypto.shifts[SC, 2, 1];
            var s3 = Crypto.shifts[SC, 3, 1];
            var a = new uint[BC];

            // temporary work array
            var t = new uint[BC];
            // ciphertext to ints + key
            for (var i = 0; i < BC; ++i)
            {
                t[i] = (uint)((ciphertext[i * 4] << 24 |
                                ciphertext[i * 4 + 1] << 16 |
                                ciphertext[i * 4 + 2] << 8 |
                                ciphertext[i * 4 + 3]) ^ Kd[0, i]);
            }
            // apply round transforms
            for (var r = 1; r < ROUNDS; ++r)
            {
                for (var i = 0; i < BC; ++i)
                {
                    a[i] = (uint)(Crypto.T5[(t[i] >> 24) & 0xFF] ^
                                   Crypto.T6[(t[(i + s1) % BC] >> 16) & 0xFF] ^
                                   Crypto.T7[(t[(i + s2) % BC] >> 8) & 0xFF] ^
                                   Crypto.T8[t[(i + s3) % BC] & 0xFF] ^ Kd[r, i]);
                }
                Array.Copy(a, t, BC);
            }
            // last round is special
            var result = new byte[BC * 4];
            for (var i = 0; i < BC; ++i)
            {
                var tt = Kd[ROUNDS, i];
                result[i * 4 + 0] = (byte)((Crypto.Si[(t[i] >> 24) & 0xFF] ^ (tt >> 24)) & 0xFF);
                result[i * 4 + 1] = (byte)((Crypto.Si[(t[(i + s1) % BC] >> 16) & 0xFF] ^ (tt >> 16)) & 0xFF);
                result[i * 4 + 2] = (byte)((Crypto.Si[(t[(i + s2) % BC] >> 8) & 0xFF] ^ (tt >> 8)) & 0xFF);
                result[i * 4 + 3] = (byte)((Crypto.Si[t[(i + s3) % BC] & 0xFF] ^ tt) & 0xFF);
            }
            return result;
        }
    }
}
