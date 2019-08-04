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
    public class Spikes : MonoBehaviour
    {
        [SerializeField]
        private int _pixelPerUnit = 100;

        [SerializeField]
        private Vector2Int _hurtBox = new Vector2Int(16, 16);

        [SerializeField]
        private Vector2Int _offset = new Vector2Int(0, 0);

        private Trigger _trigger;

        private Box TransformAsBox()
        {
            var spriteSize = GetComponent<SpriteRenderer>().sprite.rect.size;
            var position = transform.position;
            var size = transform.lossyScale;
            var box = ConversionUtil.GetObjectBox(spriteSize, position, size, _pixelPerUnit);
            return box.FromOffset(new int2(_offset.x, _offset.y)).FromResize(new int2(_hurtBox.x, _hurtBox.y));
        }

        private void Start()
        {
            _trigger = new Trigger(TransformAsBox(), OnActorEnter, OnActorLeave);
            Scene.Current.Add(_trigger);
        }

        private void OnActorEnter(Actor actor)
        {
            actor.Squish();
        }

        private void OnActorLeave(Actor actor)
        {
        }

        private void OnDrawGizmos()
        {
            Vector2Int p;
            Vector2Int s;
            if (_trigger == null)
            {
                var box = TransformAsBox();
                p = new Vector2Int(box.Position.x, box.Position.y);
                s = new Vector2Int(box.Size.x, box.Size.y);
            }
            else
            {
                p = new Vector2Int(_trigger.Bounds.Position.x, _trigger.Bounds.Position.y);
                s = new Vector2Int(_trigger.Bounds.Size.x, _trigger.Bounds.Size.y);
            }

            var position = new Vector2(p.x / (float)_pixelPerUnit, p.y / (float)_pixelPerUnit);
            var size = new Vector2(s.x / (float)_pixelPerUnit, s.y / (float)_pixelPerUnit);
            var halfSize = size * 0.5f;
            var center = position + halfSize;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
