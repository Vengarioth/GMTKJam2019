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

        private struct DecayJob : IJobParallelFor
        {
            public NativeArray<byte> Data;
            public byte Amount;

            public void Execute(int index)
            {
                byte d = Data[index];
                if(d >= Amount)
                {
                    Data[index] = (byte)(d - Amount);
                }
                else
                {
                    Data[index] = 0;
                }
            }
        }

        public void Decay(byte amount)
        {
            new DecayJob
            {
                Data = _data,
                Amount = amount
            }.Schedule(_data.Length, 32).Complete();
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
