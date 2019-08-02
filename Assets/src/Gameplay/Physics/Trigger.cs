using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace Gameplay.Physics
{
    public class Trigger
    {
        public bool Enabled { get; private set; }
        public Box Bounds => _bounds;
        private Box _bounds;

        private float _xRemainder;
        private float _yRemainder;

        public Trigger(int2 position, int2 size)
        {
            _bounds = new Box(position, size);
        }

        public bool Overlaps(Box box)
        {
            return _bounds.Overlaps(box);
        }
    }
}
