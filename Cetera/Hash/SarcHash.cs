using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cetera.Hash
{
    public class SarcHash
    {
        public static uint Create(String name, int hashMultiplier)
        {
            uint result = 0;

            for (int i = 0; i < name.Length; i++)
            {
                result = (uint)(name[i] + (result * hashMultiplier));
            }

            return result;
        }
    }
}
