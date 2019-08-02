using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace Gameplay.Physics
{
    public class Canvas : IDisposable
    {
        private NativeArray<byte> _data;

        public Canvas(int width, int height)
        {
            _data = new NativeArray<byte>(width * height, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

        public void Dispose()
        {
            _data.Dispose();
        }
    }
}
