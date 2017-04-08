using System.Linq;

namespace Cetera.Hash
{
    public class SimpleHash
    {
        public static uint Create(string input, uint magic, uint hashCount)
        {
            uint hash = 0;

            for (int i = 0; i < input.Count(); i++)
            {
                hash *= magic;
                hash += input[i];
            }

            return hash % hashCount;
        }
    }
}
