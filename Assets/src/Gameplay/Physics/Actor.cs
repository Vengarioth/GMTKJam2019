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

        public Actor(int2 position, int2 size)
        {
            _bounds = new Box(position, size);
        }

        public void MoveX(float amount, Action onCollide)
        {
            _xRemainder += amount;
            int move = Mathf.RoundToInt(_xRemainder);

            if(move != 0)
            {
                _xRemainder -= move;
                int sign = Math.Sign(move);

                while(move != 0)
                {
                    if(!Scene.Current.CollidesSolid(_bounds.FromOffset(new int2(sign, 0))))
                    {
                        _bounds.Position.x += sign;
                        move -= sign;
                    }
                    else
                    {
                        onCollide?.Invoke();
                        break;
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
                int sign = Math.Sign(move);

                while(move != 0)
                {
                    if(!Scene.Current.CollidesSolid(_bounds.FromOffset(new int2(0, sign))))
                    {
                        _bounds.Position.y += sign;
                        move -= sign;
                    }
                    else
                    {
                        onCollide?.Invoke();
                        break;
                    }
                }
            }
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

        }
    }
}
