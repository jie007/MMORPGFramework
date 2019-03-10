using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class DeterministicRandom
    {
        private static MD5 md5 = new MD5Cng();

        public static float Get(string rnd, float min, float max)
        {
            var hash = md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(rnd));
            uint val = BitConverter.ToUInt32(hash, hash.Length - 5);
            float percentage = val / (float)uint.MaxValue;
            float range = max - min;

            float result = min + range * percentage;
            return result;
        }
    }
}
