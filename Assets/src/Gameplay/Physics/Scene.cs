using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gameplay.Physics
{
    public class Scene
    {
        public static Scene Current = new Scene();

        private List<Actor> _actors;
        private List<Solid> _solids;
        private List<Trigger> _trigger;
        private List<PixelBuffer> _pixelBuffer;

        public Scene()
        {
            _actors = new List<Actor>();
            _solids = new List<Solid>();
            _trigger = new List<Trigger>();
            _pixelBuffer = new List<PixelBuffer>();
        }

        public void Add(Actor actor)
        {
            _actors.Add(actor);
        }

        public void Remove(Actor actor)
        {
            _actors.Remove(actor);
        }

        public void Add(Solid solid)
        {
            _solids.Add(solid);
        }

        public void Remove(Solid solid)
        {
            _solids.Remove(solid);
        }

        public void Add(Trigger trigger)
        {
            _trigger.Add(trigger);
        }

        public void Remove(Trigger trigger)
        {
            _trigger.Remove(trigger);
        }

        public void Add(PixelBuffer pixelBuffer)
        {
            _pixelBuffer.Add(pixelBuffer);
        }

        public PixelBuffer GetPixelBuffer()
        {
            return _pixelBuffer[0];
        }

        public void Remove(PixelBuffer pixelBuffer)
        {
            _pixelBuffer.Remove(pixelBuffer);
        }

        public bool CollidesActor(Box box)
        {
            for (int i = 0; i < _actors.Count; i++)
            {
                var actor = _actors[i];
                if (actor.Overlaps(box))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CollidesSolid(Box box)
        {
            for (int i = 0; i < _solids.Count; i++)
            {
                var solid = _solids[i];
                if (solid.Collidable && solid.Overlaps(box))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CollidesTrigger(Box box)
        {
            for(int i = 0; i < _trigger.Count; i++)
            {
                var trigger = _trigger[i];
                if(trigger.Enabled && trigger.Overlaps(box))
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateTriggers(Actor actor)
        {
            for(int i = 0; i < _trigger.Count; i++)
            {
                var trigger = _trigger[i];
                trigger.Check(actor);
            }
        }

        public Actor[] GetActors()
        {
            return _actors.ToArray();
        }

        public Actor[] GetActorsRiding(Box box)
        {
            var actors = new List<Actor>();
            for (int i = 0; i < _actors.Count; i++)
            {
                var actor = _actors[i];
                if (actor.Rides(box))
                    actors.Add(actor);
            }
            return actors.ToArray();
        }
    }
}
