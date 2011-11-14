using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KinectLibrary.Movement;
using Microsoft.Research.Kinect.Nui;
using KinectLibrary.Movement.EventsArgs;

namespace KinectAMole.Entities
{
    class Hammer : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;

        private Texture2D _texture;

        private SpriteFont _uiFont;

        private Vector2 _rotationPoint;

        private MovementTracker _movementTracker;

        #region State

        private Rectangle _drawDestination;

        #region SmashingAnimationState

        private bool _startSmashingAnimation;

        private TimeSpan _animationTime;

        private float _rotation;

        #endregion

        #endregion
        
        public Hammer(KinectAMoleGame game, MovementTracker movementTracker)
            : base(game)
        {
            _spriteBatch = game.SpriteBatch;
            _movementTracker = movementTracker;
        }

        public override void Initialize()
        {
            _movementTracker.OnSkeletonDataReceived += OnHandMovement;
            
            base.Initialize();
        }

        private Vector3 _lastPoint;
        private bool _startedSmashGesture;

        private void OnHandMovement(object state, SkeletonDataReadyEventArgs args)
        {
            Vector coordinates = args.SkeletonData.Joints[JointID.HandRight].Position;

            Vector2 point = GetSmoothPoint(coordinates);

            _drawDestination = new Rectangle((int)point.X, (int)point.Y, _texture.Width, _texture.Height);

            if (_lastPoint != null)
            {
                if (!_startedSmashGesture && _lastPoint.Y > coordinates.Y + 0.02f && _lastPoint.Z < coordinates.Z + 0.02f)
                {
                    _startedSmashGesture = true;
                }
                else if (_startedSmashGesture && _lastPoint.Y < coordinates.Y + 0.02f && _lastPoint.Z > coordinates.Z + 0.02f)
                {
                    StartSmashingAnimation();
                    _startedSmashGesture = false;
                }
            }

            _lastPoint = new Vector3(coordinates.X, coordinates.Y, coordinates.Z);
        }

        private Vector[] _lastPoints = new Vector[10];
        private int _lastPointsCount = 0;
        private const float movementScale = 1000.0f;
        private const float xAdjust = 0, yAdjust = 200;
        private int _currPos;
        
        private Vector2 GetSmoothPoint(Vector point)
        {
            if (_lastPointsCount < _lastPoints.Length)
            {
                _lastPoints[_lastPointsCount] = point;
                _lastPointsCount++;
            }
            else
            {
                _lastPoints[_currPos++] = point;

                _currPos = (_currPos == _lastPoints.Length) ? 0 : _currPos; 
            }

            float averageX = _lastPoints.Take(_lastPointsCount).Select(p => p.X).Sum() / _lastPointsCount;
            float averageY = _lastPoints.Take(_lastPointsCount).Select(p => p.Y).Sum() / _lastPointsCount;

            return ConvertToScreenCoords(averageX, averageY);
        }

        private Vector2 ConvertToScreenCoords(float X, float Y)
        {
            return new Vector2(((Game.GraphicsDevice.Viewport.Width / 2) + (X * movementScale) + xAdjust), ((Game.GraphicsDevice.Viewport.Height / 2) + (Y * movementScale * -1) + yAdjust));
        }

        protected override void LoadContent()
        {
            _texture = Game.Content.Load<Texture2D>("Hammer");
            _uiFont = Game.Content.Load<SpriteFont>("UiFont");
            _rotationPoint = new Vector2(_texture.Width, _texture.Height);

 	        base.LoadContent();
        }

        public void StartSmashingAnimation()
        {
            _startSmashingAnimation = true;
        }

        public override void  Update(GameTime gameTime)
        {
            if (_startSmashingAnimation)
            {
                if (_animationTime > TimeSpan.FromMilliseconds(250))
                {
                    _animationTime = TimeSpan.MinValue;
                    _startSmashingAnimation = false;
                    _rotation = 0;
                }

                if (_animationTime == TimeSpan.MinValue)
                {
                    _animationTime = TimeSpan.Zero;
                }
                else
                {
                    _animationTime = _animationTime.Add(gameTime.ElapsedGameTime);

                    _rotation = (float) _animationTime.TotalMilliseconds * -0.004f;                    
                }

            }
 	        
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            _spriteBatch.DrawString(_uiFont, String.Format("X: {0} Y: {1}", _drawDestination.X, _drawDestination.Y), Vector2.UnitX, Color.Red);

            _spriteBatch.Draw(_texture, _drawDestination, null, Color.White, _rotation, _rotationPoint, SpriteEffects.None, 0.1f);

            _spriteBatch.End();
        }
    }
}
