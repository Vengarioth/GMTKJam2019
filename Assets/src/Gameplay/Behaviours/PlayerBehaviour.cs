using Debugging;
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

        [SerializeField]
        private float _jumpHeight;
        [SerializeField]
        private float _horizontalDistanceToJumpPeak;
        [SerializeField]
        private float _maxHorizontalSpeed;
        [SerializeField]
        private int _maxStamina = 100;


        [SerializeField]
        private Vector2 _debugAnchor;

        private Actor _actor;

        private float2 _velocity;
        private float2 _acceleration;
        private bool _wasGrounded;
        private int _stamina;
        private bool _jumpReleased;
        private int _paintTime;
        [SerializeField]
        private int _paintTollerance = 5;

        private void Start()
        {
            _actor = new Actor(new int2(_position.x, _position.y), new int2(_size.x, _size.y), OnSquish, OnMove);
            Scene.Current.Add(_actor);
            _wasGrounded = _actor.IsGrounded();

            _stamina = _maxStamina;
        }

        private void OnMove()
        {
            var isGrounded = _actor.IsGrounded();
            var pixelBuffer = Scene.Current.GetPixelBuffer();
            var box = _actor.Bounds;

            if (!isGrounded)
            {
                if(_paintTime > _paintTollerance && pixelBuffer.Overlaps(box, 1, (byte)(_paintTime - _paintTollerance)))
                {
                    _actor.Squish();
                }

                pixelBuffer.FillBox(box.FromShrink(12), (byte)_paintTime);
            }
        }

        private void OnSquish()
        {
            Debug.Log("Squish");
            _actor.Teleport(new int2(_position.x, _position.y), _actor.Squish);
            _velocity = new float2(0f, 0f);
            Scene.Current.GetPixelBuffer().Clear();
            _paintTime = 0;
        }

        public void Replenish()
        {
            Debug.Log("Replenish");
            _stamina = _maxStamina;
        }

        private void Update()
        {
            var isGrounded = _actor.IsGrounded();
            var horizontalInput = Input.GetAxis("Horizontal");
            var doJump = Input.GetButton("Jump");

            var dt = Time.deltaTime;
            var velocity = _velocity;
            var acceleration = _acceleration;

            var xh = _horizontalDistanceToJumpPeak;
            var vx = _maxHorizontalSpeed;
            var h = _jumpHeight;

            var g = (-2f * h * vx * vx) / (xh * xh);
            var v = (2f * h * vx) / xh;

            // refresh stamina & clear buffer
            if(!_wasGrounded && isGrounded)
            {
                _stamina = _maxStamina;
                Scene.Current.GetPixelBuffer().Clear();
                _paintTime = 0;
            }
            if(!isGrounded)
            {
                _paintTime += 1;
                Shader.SetGlobalFloat("_PaintTime", ((float)_paintTime - (float)_paintTollerance) / (float)byte.MaxValue);
            }

            if(doJump && isGrounded)
            {
                Debug.Log("Jump");
                velocity.y = v;
                _jumpReleased = false;
            }
            else if(isGrounded)
            {
                velocity.y = 0;
            }

            if(!doJump)
            {
                _jumpReleased = true;
            }

            if(doJump && !isGrounded && _jumpReleased && _stamina >= 20)
            {
                _stamina -= 20;
                _jumpReleased = false;
                Debug.Log("Dash");
                velocity.y = v;
            }

            if(_actor.IsFloored())
            {
                velocity.y = 0;
            }

            acceleration.y = g;
            
            var vertical = velocity.y * dt + (acceleration.y * 0.5f) * dt * dt;
            velocity.y += acceleration.y * dt;

            var horizontal = horizontalInput * _maxHorizontalSpeed * dt;
            
            _velocity = velocity;
            _acceleration = acceleration;

            _actor.MoveY(vertical, null);
            _actor.MoveX(horizontal * 10f, null);

            _wasGrounded = isGrounded;

            var position = new Vector2(_actor.Bounds.Position.x / (float)_pixelPerUnit, _actor.Bounds.Position.y / (float)_pixelPerUnit);
            var size = new Vector2(_actor.Bounds.Size.x / (float)_pixelPerUnit, _actor.Bounds.Size.y / (float)_pixelPerUnit);
            transform.position = position + (size * 0.5f);

            DebugRenderer.Add(new HollowOpenCircle(
                _debugAnchor,
                0.3f,
                0.6f,
                (float)_stamina / (float)_maxStamina,
                Color.green
            ));
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
