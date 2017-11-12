using System.Collections;

namespace Cetera.Image.PVRTC
{
    public struct Vector3Int
    {

        public int x;
        public int y;
        public int z;

        public Vector3Int(int xx, int yy, int zz)
        {
            this.x = xx;
            this.y = yy;
            this.z = zz;
        }

        public static Vector3Int operator *(Vector3Int a, int b)
        {
            return new Vector3Int(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3Int operator +(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3Int operator -(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static int operator %(Vector3Int a, Vector3Int b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }
    }
}