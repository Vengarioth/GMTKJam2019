using Gameplay.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Gameplay.Behaviours
{
    public static class ConversionUtil
    {
        public static Box GetObjectBox(Vector2 spriteSize, Vector2 position, Vector2 size, int pixelPerUnit)
        {
            position *= pixelPerUnit;

            size.x *= spriteSize.x; //apply sprite size
            size.y *= spriteSize.y;

            position -= size * .5f;

            var ipos = new int2((int)Math.Round(position.x), (int)Math.Round(position.y));
            var isize = new int2((int)Math.Round(size.x), (int)Math.Round(size.y));
            return new Box(ipos, isize);
        }
    }
}
