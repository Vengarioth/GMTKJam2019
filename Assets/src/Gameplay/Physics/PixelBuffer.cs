using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Gameplay.Physics
{
    public class PixelBuffer : IDisposable
    {
        private NativeArray<byte> _data;
        private int _width;
        private int _height;

        public PixelBuffer(int width, int height)
        {
            _width = width;
            _height = height;
            _data = new NativeArray<byte>(width * height, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

        private int PositionToIndex(int2 position)
        {
            return (_width * position.y) + position.x;
        }

        private int2 IndexToPosition(int index)
        {
            int x = index % _width;
            int y = (index - x) / _width;
            return new int2(x, y);
        }

        public void Clear()
        {
            Debug.Log("Clear Pixel Buffer");
            _data.Dispose();
            _data = new NativeArray<byte>(_width * _height, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

        public bool Overlaps(Box box, byte minValue = byte.MinValue, byte maxValue = byte.MaxValue)
        {
            if (box.IsEmpty())
            {
                Debug.Log("box is empty");
            }

            int x = 0;
            int y = 0;
            int index = 0;

            var from = box.Position;
            var to = box.Position + box.Size;

            for (x = from.x; x < to.x; x++)
            {
                for (y = from.y; y < to.y; y++)
                {
                    index = PositionToIndex(new int2(x, y));
                    var value = _data[index];
                    if (value >= minValue && value <= maxValue)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void FillBox(Box box, byte value)
        {
            int x = 0;
            int y = 0;
            int index = 0;

            var from = box.Position;
            var to = box.Position + box.Size;

            for (x = from.x; x < to.x; x++)
            {
                if (x < 0 || x > _width)
                    continue;

                for (y = from.y; y < to.y; y++)
                {
                    if (y < 0 || y > _height - 1)
                        continue;

                    index = PositionToIndex(new int2(x, y));
                    _data[index] = value;
                }
            }
        }

        public void FillTexture(Texture2D tex)
        {
            tex.LoadRawTextureData(_data);
            tex.Apply();
        }

        public void Dispose()
        {
            _data.Dispose();
        }
    }
}
