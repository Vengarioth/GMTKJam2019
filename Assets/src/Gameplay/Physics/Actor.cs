using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Gameplay.Physics
{
    public class Actor
    {
        public Box Bounds => _bounds;
        private Box _bounds;

        private float _xRemainder;
        private float _yRemainder;

        private Action _onSquish;

        public Actor(int2 position, int2 size, Action onSquish)
        {
            _bounds = new Box(position, size);
            _onSquish = onSquish;
        }

        public Actor(Box box, Action onSquish)
        {
            _bounds = box;
            _onSquish = onSquish;
        }

        public void Teleport(int2 position, Action onCollide)
        {
            _bounds.Position = position;
            if(Scene.Current.CollidesSolid(_bounds))
            {
                onCollide?.Invoke();
            }
        }

        public void MoveX(float amount, Action onCollide)
        {
            _xRemainder += amount;
            int move = Mathf.RoundToInt(_xRemainder);

            if(move != 0)
            {
                _xRemainder -= move;

                if(move > 0)
                {
                    // Move Right
                    while(move != 0)
                    {
                        if (!Scene.Current.CollidesSolid(_bounds.FromOffset(new int2(1, 0))))
                        {
                            _bounds.Position.x += 1;
                            move -= 1;

                            Scene.Current.UpdateTriggers(this);

                            Scene.Current.GetPixelBuffer().FillBox(Bounds, byte.MaxValue);
                        }
                        else
                        {
                            onCollide?.Invoke();
                            break;
                        }
                    }
                }
                else
                {
                    // Move Left
                    while (move != 0)
                    {
                        if (!Scene.Current.CollidesSolid(_bounds.FromOffset(new int2(-1, 0))))
                        {
                            _bounds.Position.x -= 1;
                            move += 1;

                            Scene.Current.UpdateTriggers(this);

                            Scene.Current.GetPixelBuffer().FillBox(Bounds, byte.MaxValue);
                        }
                        else
                        {
                            onCollide?.Invoke();
                            break;
                        }
                    }
                }
            }
        }

        public void MoveY(float amount, Action onCollide)
        {
            _yRemainder += amount;
            int move = Mathf.RoundToInt(_yRemainder);

            if(move != 0)
            {
                _yRemainder -= move;

                if(move > 0)
                {
                    // Move Up
                    while (move != 0)
                    {
                        if (!Scene.Current.CollidesSolid(_bounds.FromOffset(new int2(0, 1))))
                        {
                            _bounds.Position.y += 1;
                            move -= 1;

                            Scene.Current.UpdateTriggers(this);

                            Scene.Current.GetPixelBuffer().FillBox(Bounds, byte.MaxValue);
                        }
                        else
                        {
                            onCollide?.Invoke();
                            break;
                        }
                    }
                }
                else
                {
                    // Move Down
                    while (move != 0)
                    {
                        if (!Scene.Current.CollidesSolid(_bounds.FromOffset(new int2(0, -1))))
                        {
                            _bounds.Position.y -= 1;
                            move += 1;

                            Scene.Current.UpdateTriggers(this);

                            Scene.Current.GetPixelBuffer().FillBox(Bounds, byte.MaxValue);
                        }
                        else
                        {
                            onCollide?.Invoke();
                            break;
                        }
                    }
                }
            }
        }

        public bool IsFloored()
        {
            return Scene.Current.CollidesSolid(_bounds.RowTop());
        }

        public bool IsGrounded()
        {
            return Scene.Current.CollidesSolid(_bounds.RowBottom());
        }

        public bool CollidesLeft()
        {
            return Scene.Current.CollidesSolid(_bounds.RowLeft());
        }

        public bool CollidesRight()
        {
            return Scene.Current.CollidesSolid(_bounds.RowRight());
        }

        public bool Overlaps(Box box)
        {
            return _bounds.Overlaps(box);
        }

        public bool Rides(Box box)
        {
            return false;
        }

        public void Squish()
        {
            _onSquish?.Invoke();
        }
    }
}
