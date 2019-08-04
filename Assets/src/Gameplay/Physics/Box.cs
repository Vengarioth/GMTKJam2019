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

        public Box FromResize(int2 size)
        {
            return new Box(Position, size);
        }

        public Box RowRight(int thickness = 1)
        {
            return new Box(
                new int2(Position.x + Size.y + thickness, Position.y),
                new int2(thickness, Size.y)
            );
        }

        public Box RowBottom(int thickness = 1)
        {
            return new Box(
                new int2(Position.x, Position.y - thickness),
                new int2(Size.x, thickness)
            );
        }

        public Box RowTop(int thickness = 1)
        {
            return new Box(
                new int2(Position.x, Position.y + Size.y),
                new int2(Size.x, thickness)
            );
        }

        public Box RowLeft(int thickness = 1)
        {
            return new Box(
                new int2(Position.x - thickness, Position.y),
                new int2(thickness, Size.y)
            );
        }

        public bool IsEmpty()
        {
            return Position.x == 0 && Position.y == 0 && Size.x == 0 && Size.y == 0;
        }

        public Box FromShrink(int amount)
        {
            return new Box(
                new int2(Position.x + amount, Position.y + amount),
                new int2(Size.x - amount - amount, Size.y - amount - amount)
            );
        }
    }
}
