using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Debugging
{
    [RequireComponent(typeof(Camera))]
    public class DebugRenderer : MonoBehaviour
    {
        [SerializeField]
        private Material _debugMaterial;

        private Camera _camera;
        private CommandBuffer _buffer;
        private List<IDebugShape> _shapes;
        private static DebugRenderer _instance;

        public static void Add(IDebugShape shape)
        {
            _instance._shapes.Add(shape);
        }

        private void Start()
        {
            _shapes = new List<IDebugShape>();
            _instance = this;
        }

        private void Initialize()
        {
            _camera = GetComponent<Camera>();
            _buffer = new CommandBuffer();
            _buffer.name = "DebugRenderer";
            _camera.AddCommandBuffer(CameraEvent.AfterImageEffects, _buffer);
        }

        private void Clear()
        {
            _camera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, _buffer);
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            Clear();
        }

        private void LateUpdate()
        {
            _buffer.Clear();
            foreach (var shape in _shapes)
            {
                _buffer.BeginSample("DebugRenderer");
                shape.Render(_camera, _buffer, _debugMaterial);
                _buffer.EndSample("DebugRenderer");
            }
            _shapes.Clear();
        }
    }
}
