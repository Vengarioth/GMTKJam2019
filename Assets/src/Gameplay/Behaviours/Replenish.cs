﻿using Gameplay.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Behaviours
{
    public class Replenish : MonoBehaviour
    {
        [SerializeField]
        private int _pixelPerUnit = 100;

        [SerializeField]
        private bool _full;
        [SerializeField]
        private Sprite _fullSprite;
        [SerializeField]
        private Sprite _emptySprite;

        private Trigger _trigger;

        private Box TransformAsBox()
        {
            var spriteSize = GetComponent<SpriteRenderer>().sprite.rect.size;
            var position = transform.position;
            var size = transform.lossyScale;

            return ConversionUtil.GetObjectBox(spriteSize, position, size, _pixelPerUnit);
        }

        private void Start()
        {
            _trigger = new Trigger(TransformAsBox(), OnActorEnter, OnActorLeave);
            Scene.Current.Add(_trigger);

            if(_full)
            {
                GetComponent<SpriteRenderer>().sprite = _fullSprite;
            }
            else
            {
                GetComponent<SpriteRenderer>().sprite = _emptySprite;
            }
        }

        private void OnActorEnter(Actor actor)
        {
            if (!_full)
                return;

            _full = false;
            GetComponent<SpriteRenderer>().sprite = _emptySprite;

            FindObjectOfType<PlayerBehaviour>().Replenish();
            StartCoroutine(ReplenishCoroutine());
        }

        private void OnActorLeave(Actor actor)
        {

        }

        private IEnumerator ReplenishCoroutine()
        {
            yield return new WaitForSeconds(1.8f);

            _full = true;
            GetComponent<SpriteRenderer>().sprite = _fullSprite;
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

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
