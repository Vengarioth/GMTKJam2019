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
    public class PlayerBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int _position;
        [SerializeField]
        private Vector2Int _size;
        [SerializeField]
        private int _pixelPerUnit = 100;

        private Actor _actor;

        private void Start()
        {
            _actor = new Actor(new int2(_position.x, _position.y), new int2(_size.x, _size.y));
            Scene.Current.Add(_actor);
        }

        private void Update()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            _actor.MoveY(vertical * 10f, null);
            _actor.MoveX(horizontal * 10f, null);

            FindObjectOfType<CanvasBehaviour>().Draw(_actor.Bounds, byte.MaxValue);

            var position = new Vector2(_actor.Bounds.Position.x / (float)_pixelPerUnit, _actor.Bounds.Position.y / (float)_pixelPerUnit);
            var size = new Vector2(_actor.Bounds.Size.x / (float)_pixelPerUnit, _actor.Bounds.Size.y / (float)_pixelPerUnit);
            transform.position = position + (size * 0.5f);
        }

        private void OnDrawGizmos()
        {
            Vector2Int p;
            Vector2Int s;
            if(_actor == null)
            {
                p = _position;
                s = _size;
            }
            else
            {
                p = new Vector2Int(_actor.Bounds.Position.x, _actor.Bounds.Position.y);
                s = new Vector2Int(_actor.Bounds.Size.x, _actor.Bounds.Size.y);
            }

            var position = new Vector2(p.x / (float)_pixelPerUnit, p.y / (float)_pixelPerUnit);
            var size = new Vector2(s.x / (float)_pixelPerUnit, s.y / (float)_pixelPerUnit);
            var halfSize = size * 0.5f;
            var center = position + halfSize;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
