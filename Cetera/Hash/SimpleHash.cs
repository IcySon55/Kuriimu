using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cetera.Hash
{
    public class SimpleHash
    {
        public static uint Create(string label, uint magic, uint hashCount)
        {
            uint hash = 0;

            for (int i = 0; i < label.Count(); i++)
            {
                hash *= magic;
                hash += label[i];
            }

            return hash % hashCount;
        }
    }
}
