using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace Gameplay.Physics
{
    public struct Box
    {
        public int Right => Position.x + Size.x;
        public int Left => Position.x;
        public int Top => Position.y + Size.y;
        public int Bottom => Position.y;

        public int2 Position;
        public int2 Size;

        public Box(int2 position, int2 size)
        {
            Position = position;
            Size = size;
        }

        public bool Overlaps(Box other)
        {
            return !(other.Position.x + other.Size.x <= Position.x ||
                other.Position.x >= Position.x + Size.x ||
                other.Position.y + other.Size.y <= Position.y ||
                other.Position.y >= Position.y + Size.y);
        }

        public Box FromOffset(int2 delta)
        {
            return new Box(Position + delta, Size);
        }
    }
}
