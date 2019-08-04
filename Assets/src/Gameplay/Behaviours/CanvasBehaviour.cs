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

        private PixelBuffer _frontBuffer;
        private Texture2D _frontBufferTexture;

        private PixelBuffer _backBuffer;
        private Texture2D _backBufferTexture;

        private void Start()
        {
            _frontBuffer = new PixelBuffer(_size.x, _size.y);
            Scene.Current.Add(_frontBuffer);
            _frontBufferTexture = new Texture2D(_size.x, _size.y, TextureFormat.R8, false);


            _backBuffer = new PixelBuffer(_size.x, _size.y);
            Scene.Current.Add(_backBuffer);
            _backBufferTexture = new Texture2D(_size.x, _size.y, TextureFormat.R8, false);
        }

        private void Update()
        {
            _frontBuffer.FillTexture(_frontBufferTexture);
            _backBuffer.FillTexture(_backBufferTexture);
            GetComponent<Renderer>().material.SetTexture("_MainTex", _frontBufferTexture);
            GetComponent<Renderer>().material.SetTexture("_BackTex", _backBufferTexture);
        }

        private void OnDestroy()
        {
            _frontBuffer.Dispose();
            _backBuffer.Dispose();
        }
    }
}
