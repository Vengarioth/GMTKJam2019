using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Debugging
{
    public interface IDebugShape
    {
        void Render(Camera camera, CommandBuffer buffer, Material material);
    }
}
