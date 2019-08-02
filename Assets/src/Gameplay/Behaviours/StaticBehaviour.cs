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
    public class StaticBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int _position;
        [SerializeField]
        private Vector2Int _size;
        [SerializeField]
        private int _pixelPerUnit = 100;

        private Solid _solid;

        private void Start()
        {
            var pos  = transform.position;
            var size = transform.lossyScale;
            _solid = new Solid(new int2((int)pos.x, (int)pos.y), new int2((int)size.x, (int)size.y));
            Scene.Current.Add(_solid);
        }

        private void OnDrawGizmos()
        {
            Vector2Int p;
            Vector2Int s;
            if (_solid == null)
            {
                p = _position;
                s = _size;
            }
            else
            {
                p = new Vector2Int(_solid.Bounds.Position.x, _solid.Bounds.Position.y);
                s = new Vector2Int(_solid.Bounds.Size.x, _solid.Bounds.Size.y);
            }

            var position = new Vector2(p.x / (float)_pixelPerUnit, p.y / (float)_pixelPerUnit);
            var size = new Vector2(s.x / (float)_pixelPerUnit, s.y / (float)_pixelPerUnit);
            var halfSize = size * 0.5f;
            var center = position + halfSize;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
