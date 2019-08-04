using Debugging;
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
    public enum Mode
    {
        Default,
        Wall,
        Dash,
    };

    public class PlayerBehaviour : MonoBehaviour
    {
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
        private float _horizontalTweak = 0.7f;
        [SerializeField]
        private Vector2 _debugAnchor;

        private float _wallJumpHorizontalSpeed = 4f;

        private int2 _spawn;
        private Actor _actor;

        private float2 _velocity;
        private float2 _acceleration;

        private bool _isGrounded;
        private bool _wasGrounded;

        private bool _doJump;
        private bool _didJump;
        private float _horizontalInput;
        private float _verticalInput;

        private int _stamina;
        private Mode _mode;



        float dt = 0.016f;

        private bool _jumpReleased;

        private int _amtOfFramesSinceJumpWasReleased = 0;

        private int _framesSinceLastGrounded;
        private int _maxFramesToJumpAfterLeavingGround = 5;

        [SerializeField]
        private int _paintTollerance = 50;
        private int _paintTime;

        private Box TransformAsBox()
        {
            var spriteSize = GetComponent<SpriteRenderer>().sprite.rect.size;
            var position = transform.position;
            var size = transform.lossyScale;
            
            return ConversionUtil.GetObjectBox(spriteSize, position, size, _pixelPerUnit);
        }

        private void Start()
        {
            _actor = new Actor(TransformAsBox(), OnSquish, OnMove);
            Scene.Current.Add(_actor);
            _wasGrounded = _actor.IsGrounded();

            _spawn = _actor.Bounds.Position;

            _stamina = _maxStamina;
            _mode = Mode.Default;
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

                pixelBuffer.FillBox(box.FromShrink(6), (byte)_paintTime);
            }
        }

        private void OnSquish()
        {
            Debug.Log("Squish");
            _actor.Teleport(_spawn, _actor.Squish);
            _velocity = new float2(0f, 0f);
            _acceleration = new float2(0f, 0f);
            Scene.Current.GetPixelBuffer().Clear();
            _paintTime = 0;

            _didJump = true;
            _didDash = true;
        }

        public void Replenish()
        {
            Debug.Log("Replenish");
            _stamina = _maxStamina;
        }

        private void UpdatePaint()
        {
            if(!_wasGrounded && _isGrounded)
            {
                Scene.Current.GetPixelBuffer().Clear();
                _paintTime = 0;
            }

            if(!_isGrounded)
            {
                _paintTime += 1;
                Shader.SetGlobalFloat("_PaintTime", ((float)_paintTime - (float)_paintTollerance) / (float)byte.MaxValue);
            }
        }

        private void DefaultUpdate()
        {
            var epsilon = 0.1f;
            var speed = abs(_velocity.x);
            var absHorizontalInput = abs(_horizontalInput);
            var speedSign = speed <= epsilon ? sign(_horizontalInput) : sign(_velocity.x);
            var maxSpeed = 3f;
            var horizontalAcceleration = 0.3f;
            var horizontalDeceleration = 0.3f;

            var accelerating = (_velocity.x > 0f && _horizontalInput > 0f) || (_velocity.x < 0f && _horizontalInput < 0f) || (speed <= epsilon && absHorizontalInput > epsilon);
            var decelerating = (_velocity.x > 0f && _horizontalInput < 0f) || (_velocity.x < 0f && _horizontalInput > 0f);
            var stopping = absHorizontalInput <= epsilon;

            if (speed < maxSpeed && accelerating)
            {
                var delta = horizontalAcceleration * absHorizontalInput;
                delta = min(maxSpeed - speed, delta);
                speed += delta;
            }
            else if (speed > -maxSpeed && decelerating)
            {
                var delta = horizontalDeceleration * absHorizontalInput;
                delta = min(abs(-maxSpeed - speed), delta);
                speed -= delta;
            }
            else if (stopping)
            {
                var delta = horizontalDeceleration;
                delta = min(speed, delta);
                speed -= delta;
            }

            _velocity.x = speed * speedSign;

            if (_actor.CollidesLeft() && _velocity.x < 0f)
            {
                _velocity.x = 0f;
            }
            if (_actor.CollidesRight() && _velocity.x > 0f)
            {
                _velocity.x = 0f;
            }

            var xh = _horizontalDistanceToJumpPeak;
            var vx = _maxHorizontalSpeed;
            var h = _jumpHeight;

            var g = (2f * h * vx * vx) / (xh * xh);
            var v = (2f * h * vx) / xh;

            _acceleration.y = -g;

            var vertical = _velocity.y * dt + (_acceleration.y * 0.5f) * dt * dt;
            _velocity.y += _acceleration.y * dt;

            if (!_didJump && _doJump && _jumpReleased && _framesSinceLastGrounded < _maxFramesToJumpAfterLeavingGround)
            {
                _didJump = true;
                _velocity.y = v;
                _jumpReleased = false;
                Debug.Log("Jump");
            }
            else if (_isGrounded && _velocity.y < 0f)
            {
                _velocity.y = 0f;
            }

            _actor.MoveY(vertical, null);
            _actor.MoveX(_velocity.x, null);

            // Transition to Wall
            if(_velocity.y < 0f && (_actor.CollidesLeft() || _actor.CollidesRight()))
            {
                _velocity.y = 0f;
                _didJump = false;
                _mode = Mode.Wall;
            }
            else if((_horizontalInput > 0f && _actor.CollidesRight()) || (_horizontalInput < 0f && _actor.CollidesLeft()))
            {
                _velocity.y = 0f;
                _didJump = false;
                _mode = Mode.Wall;
            }
            // Transition to Dash
            else if(!_didDash && !_isGrounded && _jumpReleased && _doJump)
            {
                if(_horizontalInput > epsilon && _verticalInput > epsilon)
                {
                    _dashDirection = normalizesafe(new float2(1f, 1f));
                }
                else if(_horizontalInput > epsilon && _verticalInput < -epsilon)
                {
                    _dashDirection = normalizesafe(new float2(1f, -1f));
                }
                else if (_horizontalInput < -epsilon && _verticalInput > epsilon)
                {
                    _dashDirection = normalizesafe(new float2(-1f, 1f));
                }
                else if (_horizontalInput < -epsilon && _verticalInput < -epsilon)
                {
                    _dashDirection = normalizesafe(new float2(-1f, -1f));
                }
                else if(_horizontalInput > epsilon)
                {
                    _dashDirection = new float2(1f, 0f);
                }
                else if (_verticalInput > epsilon)
                {
                    _dashDirection = new float2(0f, 1f);
                }
                else if (_horizontalInput < -epsilon)
                {
                    _dashDirection = new float2(-1f, 0f);
                }
                else if (_verticalInput < -epsilon)
                {
                    _dashDirection = new float2(0f, -1f);
                }
                else
                {
                    return;
                }

                _velocity.y = 0f;
                _didDash = true;
                _dashFramesLeft = _dashFrames;
                _mode = Mode.Dash;
                _jumpReleased = false;
            }
        }

        private void WallUpdate()
        {
            var xh = _horizontalDistanceToJumpPeak;
            var vx = _maxHorizontalSpeed;
            var h = _jumpHeight;

            var g = (2f * h * vx * vx) / (xh * xh);
            var v = (2f * h * vx) / xh;

            _acceleration.y = -g * 0.08f;

            if(_doJump && !_didJump && _jumpReleased)
            {
                _didJump = true;
                _jumpReleased = false;

                if (_actor.CollidesLeft())
                {
                    _velocity.y = v;
                    _velocity.x = _wallJumpHorizontalSpeed;
                }
                else
                {
                    _velocity.y = v;
                    _velocity.x = -_wallJumpHorizontalSpeed;
                }
            }

            var vertical = _velocity.y * dt + (_acceleration.y * 0.5f) * dt * dt;
            _velocity.y += _acceleration.y * dt;

            _actor.MoveY(vertical, null);
            _actor.MoveX(_velocity.x, null);

            // Transition to Default
            if (_actor.IsGrounded() || !(_actor.CollidesLeft() || _actor.CollidesRight()))
            {
                _mode = Mode.Default;
            }
            if((_actor.CollidesLeft() && _horizontalInput > 0f) || (_actor.CollidesRight() && _horizontalInput < 0f))
            {
                _mode = Mode.Default;
            }
        }

        private void EndDash()
        {
            _dashFramesLeft = 0;
            _mode = Mode.Default;
            _didJump = true;
            _didDash = true;
        }

        private int _dashFramesLeft;
        private int _dashFrames = 5;
        private bool _didDash;
        private float2 _dashDirection;
        private void DashUpdate()
        {
            _dashFramesLeft -= 1;

            var length = 10f;
            var d = _dashDirection * length;
            _actor.MoveY(d.y, EndDash);
            _actor.MoveX(d.x, EndDash);

            if(_dashFramesLeft <= 0)
            {
                EndDash();
            }
        }

        private void SampleInput()
        {
            _doJump = Input.GetButton("Jump");
            _horizontalInput = Input.GetAxis("Horizontal");
            _verticalInput = Input.GetAxis("Vertical");
        }

        private void Update()
        {
            _wasGrounded = _isGrounded;
            _isGrounded = _actor.IsGrounded();

            SampleInput();
            UpdatePaint();

            if (_isGrounded)
            {
                _didJump = false;
                _didDash = false;
                _framesSinceLastGrounded = 0;
            }
            else
            {
                _framesSinceLastGrounded += 1;
            }

            switch(_mode)
            {
                case Mode.Default:
                    DefaultUpdate();
                    break;
                case Mode.Wall:
                    WallUpdate();
                    break;
                case Mode.Dash:
                    DashUpdate();
                    break;
            }

            /*
            var isGrounded = _actor.IsGrounded();
            _isGrounded = isGrounded;

            var horizontalInput = Input.GetAxis("Horizontal");
            UpdatePaint(isGrounded);

            var doJump = Input.GetButton("Jump");

            var wallTouchL = _actor.CollidesLeft();
            var wallTouchR = _actor.CollidesRight();
            var wallTouch = wallTouchL || wallTouchR;

            int wallTouchDir = 0;
            if (wallTouchL)
                wallTouchDir += -1;
            if (wallTouchR)
                wallTouchDir += +1;

            bool horizontalInputIsAgainstWall = (int)sign(horizontalInput) == wallTouchDir;
            
            var dt = Time.deltaTime;
            var velocity = _velocity;
            var acceleration = _acceleration;

            var xh = _horizontalDistanceToJumpPeak;
            var vx = _maxHorizontalSpeed;
            var h = _jumpHeight;

            var g = (-2f * h * vx * vx) / (xh * xh);
            var v = (2f * h * vx) / xh;

            // reset
            if (isGrounded)
            {
                _stamina = _maxStamina;
                _amtOfFramesSinceJumpWasReleased = 0; //reset jump frames because we're back on ground
                _framesSinceLastGrounded = 0;
                _didJump = false;
            }
            else
            {
                _framesSinceLastGrounded += 1;
            }

            if (!doJump)
            {
                ++_amtOfFramesSinceJumpWasReleased;
            }

            if (doJump && !_didJump && _jumpReleased && _framesSinceLastGrounded < _maxFramesToJumpAfterLeavingGround)
            {
                _didJump = true;
                Debug.Log("Jump");
                velocity.y = v;
                _jumpReleased = false;
            }
            else if(isGrounded && !doJump)
            {
                velocity.y = 0;
            }

            if(!doJump)
            {
                _jumpReleased = true;
            }

            if (doJump && !isGrounded && _jumpReleased && _stamina >= 20)
            {
                _stamina -= 20;
                _jumpReleased = false;
                _dashTimeout = _dashDuration;
                _dashDirection = sign(horizontalInput);
                velocity.x = _dashSpeed * _dashDirection;
                Debug.Log("Dash");
            }

            if (!isGrounded && _jumpReleased && _amtOfFramesSinceJumpWasReleased >= 3) {
                //g *= 1 + 0.1f * (_amtOfFramesSinceJumpWasReleased - 3f);
                g = 1.8f * g;
            }

            if (_actor.IsFloored() && velocity.y > 0) //if touches ceiling
            {
                velocity.y = 0; //prevent sticking to ceiling when hitting it early in the jump
            }

            acceleration.y = g;
            var vertical = 0f;
            if(_dashTimeout > 0)
            {
                acceleration.y = 0;
                _dashTimeout -= 1;
                velocity.y = 0;
                horizontalInput = _dashDirection;
            }
            else
            {
                vertical = velocity.y * dt + (acceleration.y * 0.5f) * dt * dt;
                velocity.y += acceleration.y * dt;
            }


            const float fallspeed_cap = 40 * -10f;
            if (velocity.y < fallspeed_cap) //limit falling speed
                velocity.y = fallspeed_cap;

            //Horizontal movement stuff in this scope
            {
                var epsilon = 0.1f;
                var speed = abs(velocity.x);
                var absHorizontalInput = abs(horizontalInput);
                var speedSign = speed <= epsilon ? sign(horizontalInput) : sign(velocity.x);
                var maxSpeed = 3f;
                var horizontalAcceleration = 0.3f;
                var horizontalDeceleration = 0.3f;

                var accelerating = (velocity.x > 0f && horizontalInput > 0f) || (velocity.x < 0f && horizontalInput < 0f) || (speed <= epsilon && absHorizontalInput > epsilon);
                var decelerating = (velocity.x > 0f && horizontalInput < 0f) || (velocity.x < 0f && horizontalInput > 0f);
                var stopping = absHorizontalInput <= epsilon;

                if(speed < maxSpeed && accelerating)
                {
                    var delta = horizontalAcceleration * absHorizontalInput;
                    delta = min(maxSpeed - speed, delta);
                    speed += delta;
                    //speed = min(speed + (horizontalAcceleration * absHorizontalInput), maxSpeed);
                }
                else if(speed > -maxSpeed && decelerating)
                {
                    // speed = max(-maxSpeed, speed - (horizontalDeceleration * absHorizontalInput));
                    var delta = horizontalDeceleration * absHorizontalInput;
                    delta = min(abs(-maxSpeed - speed), delta);
                    speed -= delta;
                }
                else if(stopping)
                {
                    var delta = horizontalDeceleration;
                    delta = min(speed, delta);
                    speed -= delta;
                    // speed = max(0f, speed - horizontalDeceleration);
                }

                velocity.x = speed * speedSign;

                if(wallTouchL && velocity.x < 0f)
                {
                    velocity.x = 0f;
                }
                if (wallTouchR && velocity.x > 0f)
                {
                    velocity.x = 0f;
                }
            }

            _velocity = velocity;
            _acceleration = acceleration;

            _actor.MoveY(vertical, null);
            _actor.MoveX(velocity.x, null);

            _wasGrounded = isGrounded;

            var position = new Vector2(_actor.Bounds.Position.x / (float)_pixelPerUnit, _actor.Bounds.Position.y / (float)_pixelPerUnit);
            var size = new Vector2(_actor.Bounds.Size.x / (float)_pixelPerUnit, _actor.Bounds.Size.y / (float)_pixelPerUnit);
            transform.position = position + (size * 0.5f);

            DebugRenderer.Add(new HollowOpenCircle(
                _debugAnchor,
                0.2f,
                0.3f,
                (float)_stamina / (float)_maxStamina,
                Color.green
            ));
            */

            if (_isGrounded)
            {
                _didJump = false;
            }

            if (!_doJump)
            {
                _jumpReleased = true;
            }

            var position = new Vector2(_actor.Bounds.Position.x / (float)_pixelPerUnit, _actor.Bounds.Position.y / (float)_pixelPerUnit);
            var size = new Vector2(_actor.Bounds.Size.x / (float)_pixelPerUnit, _actor.Bounds.Size.y / (float)_pixelPerUnit);
            transform.position = position + (size * 0.5f);
        }

        private void OnDrawGizmos()
        {
            Vector2Int p;
            Vector2Int s;
            if (_actor == null)
            {
                var box = TransformAsBox();
                p = new Vector2Int(box.Position.x, box.Position.y);
                s = new Vector2Int(box.Size.x, box.Size.y);
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
