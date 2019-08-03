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

        private List<Actor> _residentActors;
        private Action<Actor> _onEnter;
        private Action<Actor> _onLeave;

        public Trigger(int2 position, int2 size, Action<Actor> onEnter, Action<Actor> onLeave)
        {
            _bounds = new Box(position, size);
            Enabled = true;

            _residentActors = new List<Actor>();

            _onEnter = onEnter;
            _onLeave = onLeave;

        }

        public Trigger(Box box, Action<Actor> onEnter, Action<Actor> onLeave)
        {
            _bounds = box;
            Enabled = true;

            _residentActors = new List<Actor>();

            _onEnter = onEnter;
            _onLeave = onLeave;
        }

        public void Check(Actor actor)
        {
            if(Bounds.Overlaps(actor.Bounds))
            {
                if (_residentActors.Contains(actor))
                    return;

                _residentActors.Add(actor);
                _onEnter?.Invoke(actor);
            }
            else
            {
                if (!_residentActors.Contains(actor))
                    return;

                _residentActors.Remove(actor);
                _onLeave?.Invoke(actor);
            }
        }

        public bool Overlaps(Box box)
        {
            return _bounds.Overlaps(box);
        }
    }
}
