using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Gameplay.Physics
{
    public class Solid
    {
        public bool Collidable { get; private set; }
        public Box Bounds => _bounds;
        private Box _bounds;

        private float _xRemainder;
        private float _yRemainder;

        public Solid(int2 position, int2 size)
        {
            _bounds = new Box(position, size);
            Collidable = true;
        }

        public Solid(Box bounds) 
        {
            _bounds = bounds;
            Collidable = true;
        }

        public void Move(float2 delta)
        {
            Move(delta.x, delta.y);
        }

        public void Move(float x, float y)
        {
            _xRemainder += x;
            _yRemainder += y;

            int moveX = Mathf.RoundToInt(_xRemainder);
            int moveY = Mathf.RoundToInt(_yRemainder);

            if(moveX != 0 || moveY != 0)
            {
                var riding = Scene.Current.GetActorsRiding(_bounds);
                var actors = Scene.Current.GetActors();
                Collidable = false;

                if(moveX != 0)
                {
                    _xRemainder -= moveX;
                    _bounds.Position.x += moveX;

                    if(moveX > 0)
                    {
                        foreach(var actor in actors)
                        {
                            if(actor.Overlaps(_bounds))
                            {
                                actor.MoveX(_bounds.Right - actor.Bounds.Left, actor.Squish);
                            }
                            else if(riding.Contains(actor))
                            {
                                actor.MoveX(moveX, null);
                            }
                        }
                    }
                    else
                    {
                        foreach(var actor in actors)
                        {
                            if(actor.Overlaps(_bounds))
                            {
                                actor.MoveX(_bounds.Left - actor.Bounds.Right, actor.Squish);
                            }
                            else if(riding.Contains(actor))
                            {
                                actor.MoveX(moveX, null);
                            }
                        }
                    }
                }

                if(moveY != 0)
                {
                    _yRemainder -= moveY;
                    _bounds.Position.y += moveY;

                    if(moveY > 0)
                    {
                        foreach(var actor in Scene.Current.GetActors())
                        {
                            if(actor.Overlaps(_bounds))
                            {
                                actor.MoveY(_bounds.Top - actor.Bounds.Bottom, actor.Squish);
                            }
                            else if(riding.Contains(actor))
                            {
                                actor.MoveY(moveY, null);
                            }
                        }
                    }
                    else
                    {
                        foreach(var actor in Scene.Current.GetActors())
                        {
                            if(actor.Overlaps(_bounds))
                            {
                                actor.MoveY(_bounds.Bottom - actor.Bounds.Top, actor.Squish);
                            }
                            else if(riding.Contains(actor))
                            {
                                actor.MoveY(moveY, null);
                            }
                        }
                    }
                }
            }
        }

        public bool Overlaps(Box box)
        {
            return _bounds.Overlaps(box);
        }
    }
}
