using Gameplay.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Behaviours
{
    public class CanvasBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int _size;

        private PixelBuffer _pixelBuffer;
        private Texture2D _texture;

        private void Start()
        {
            _pixelBuffer = new PixelBuffer(_size.x, _size.y);
            _texture = new Texture2D(_size.x, _size.y, TextureFormat.R8, false);
        }

        public void Draw(Box box, byte value)
        {
            _pixelBuffer.FillBox(box, value);
        }

        private void Update()
        {
            _pixelBuffer.Decay(15);
            _pixelBuffer.FillTexture(_texture);
            GetComponent<Renderer>().material.mainTexture = _texture;
        }

        private void OnDestroy()
        {
            _pixelBuffer.Dispose();
        }
    }
}
