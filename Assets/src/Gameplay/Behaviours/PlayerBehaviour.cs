﻿using Debugging;
using Gameplay.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

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
        private float _horizontalVel = 0;
        private bool _wasGrounded;
        private int _stamina;
        private bool _jumpReleased;

        private void Start()
        {
            _actor = new Actor(new int2(_position.x, _position.y), new int2(_size.x, _size.y), OnSquish);
            Scene.Current.Add(_actor);
            _wasGrounded = _actor.IsGrounded();

            _stamina = _maxStamina;
        }

        private void OnSquish()
        {
            _actor.Teleport(new int2(_position.x, _position.y), _actor.Squish);
            _velocity = new float2(0f, 0f);
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

            // refresh stamina
            if(isGrounded)
            {
                _stamina = _maxStamina;
            }

            if(doJump && isGrounded)
            {
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

            if(_actor.IsFloored()) //if touches ceiling
            {
                velocity.y = 0; //prevent sticking to ceiling when hitting it early in the jump
            }

            acceleration.y = g;



            var vertical = velocity.y * dt + (acceleration.y * 0.5f) * dt * dt;
            velocity.y += acceleration.y * dt;

            //Horizontal movement
            if (true) {
                var spdMax = 0.8f;
                var hv = _horizontalVel;
                var tv = horizontalInput * spdMax; //tv = target velocity

                var normalizer = sign(hv);
                if (abs(normalizer) < 0.01) normalizer = sign(tv);

                //normalize: positive == in current direction, negative == in opposite direction
                hv *= normalizer;
                tv *= normalizer;

                var prevHv = hv; //hv of previous frame

                if (tv > hv) { //if we want to move faster in the same direction than we currently do
                    //accelerate!
                    //Debug.Log("accelerate " + tv + " >  " + hv);
                    hv += 0.1f;
                }
                else {//slow down!
                    //Debug.Log("decellerate " + tv + " <= " + hv);
                    hv -= 0.2f;

                    if (hv < 0f) //but don't start moving backwards!
                        hv = 0f;
                }

                bool wallBonk = _actor.CollidesLeft() || _actor.CollidesRight();
                bool speedIsTiny = abs(prevHv) < 0.05f;
                bool slowingDown = abs(hv) < abs(prevHv);

                if ((slowingDown && speedIsTiny)) {
                    hv = 0f;
                }

                if (wallBonk && !speedIsTiny) { //don't reset if speed is tiny! otherwise you perma-stick to the wall
                    //vertical += 10f;
                    if (_actor.IsGrounded()) {
                        hv *= -1; //if on ground, bonk backwards!
                        vertical += 10; //and bonk upward a little? remove if annoying
                    }
                    else {
                        hv = 0; // if airbound, just slide on the wall
                    }
                }

                hv *= normalizer; //de-normalize

                hv = clamp(hv, -spdMax, spdMax);
                _horizontalVel = hv;
            }

            _velocity = velocity;
            _acceleration = acceleration;

            _actor.MoveY(vertical, null);
            _actor.MoveX(_horizontalVel * 10f, null);

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
