﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Common;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Joints;
using eae_coolkatz.Images;
using FarseerPhysics.Collision;
using FarseerPhysics;
using FarseerPhysics.DebugView;
using eae_koolcatz;
using FarseerPhysics.Factories;
using System.Xml.Serialization;

namespace eae_coolkatz.Screens
{
    public class GameplayScreen : GameScreen
    {
        private World world;

        Camera2D camera;
        DebugViewXNA debug;

        private Body floor;
        private Body wallLeft;
        private Body wallRight;

        public Image background;

        //Angel Truck Stuff
        private Body truckAngelCollisionBox;

        public Image truckAngel;
        public Image tireAngel;

        Body _wheelBackAngel;
        Body _wheelFrontAngel;
        WheelJoint _springBackAngel;
        WheelJoint _springFrontAngel;

        bool rearInTheAirAngel = false;
        bool frontInTheAirAngel = false;

        //Demon Truck Stuff
        private Body truckDemonCollisionBox;

        public Image truckDemon;
        public Image tireDemon;

        Body _wheelBackDemon;
        Body _wheelFrontDemon;
        WheelJoint _springBackDemon;
        WheelJoint _springFrontDemon;

        bool rearInTheAirDemon = false;
        bool frontInTheAirDemon = false;

        const float MaxSpeed = 20.0f;
        private float _accelerationAngel;
        private float _accelerationDemon;

        public override void LoadContent()
        {
            base.LoadContent();
            background.LoadContent();

            truckAngel.LoadContent();
            tireAngel.LoadContent();

            truckDemon.LoadContent();
            tireDemon.LoadContent();

            camera = new Camera2D(ScreenManager.Instance.GraphicsDevice);

            if(world == null)
            {
                world = new World(Vector2.UnitY*10);
            }
            else
            {
                world.Clear();
            }

            if(debug == null)
            {
                debug = new DebugViewXNA(world);
                debug.AppendFlags(DebugViewFlags.Shape);
                debug.AppendFlags(DebugViewFlags.PolygonPoints);
                debug.LoadContent(ScreenManager.Instance.GraphicsDevice, ScreenManager.Instance.Content);
            }

            //floor = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(19200f), ConvertUnits.ToSimUnits(70f), 10f);
            //floor.Position = ConvertUnits.ToSimUnits(960, 1080-35);
            //floor.IsStatic = true;
            //floor.Restitution = 0.2f;
            //floor.Friction = 0.2f;

            floor = new Body(world);

            {
                Vertices terrain = new Vertices();
                terrain.Add(ConvertUnits.ToSimUnits(0, 1010));
                //terrain.Add(ConvertUnits.ToSimUnits(960, 880));
                terrain.Add(ConvertUnits.ToSimUnits(1920, 1010));

                for (int i = 0; i < terrain.Count - 1; ++i)
                {
                    FixtureFactory.AttachEdge(terrain[i], terrain[i + 1], floor);
                }

                floor.Friction = 0.6f;
            }

            wallLeft = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(2f), ConvertUnits.ToSimUnits(1080f), 10f);
            wallLeft.Position = ConvertUnits.ToSimUnits(0, 540);
            wallLeft.IsStatic = true;
            wallLeft.Restitution = 0.2f;
            wallLeft.Friction = 0.2f;

            wallRight = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(2f), ConvertUnits.ToSimUnits(1080f), 10f);
            wallRight.Position = ConvertUnits.ToSimUnits(1920, 540);
            wallRight.IsStatic = true;
            wallRight.Restitution = 0.2f;
            wallRight.Friction = 0.2f;

            CircleShape wheelShape = new CircleShape(0.2f, 0.8f);

            Vertices chassisShapeDemon = new Vertices(4);

            chassisShapeDemon.Add(ConvertUnits.ToSimUnits(-80, -15));
            chassisShapeDemon.Add(ConvertUnits.ToSimUnits(-70, 20));
            chassisShapeDemon.Add(ConvertUnits.ToSimUnits(65, 10));
            chassisShapeDemon.Add(ConvertUnits.ToSimUnits(65, 0));

            PolygonShape chassisDemon = new PolygonShape(chassisShapeDemon, 2);

            truckDemonCollisionBox = new Body(world);
            truckDemonCollisionBox.BodyType = BodyType.Dynamic;
            truckDemonCollisionBox.Position = new Vector2(1.1f, -1.0f);
            truckDemonCollisionBox.CreateFixture(chassisDemon);

            _wheelBackDemon = new Body(world);
            _wheelBackDemon.BodyType = BodyType.Dynamic;
            _wheelBackDemon.Position = new Vector2(0.6f, -0.65f);
            _wheelBackDemon.CreateFixture(wheelShape);
            _wheelBackDemon.Friction = 1.8f;

            wheelShape.Density = 1;
            _wheelFrontDemon = new Body(world);
            _wheelFrontDemon.BodyType = BodyType.Dynamic;
            _wheelFrontDemon.Position = new Vector2(1.2f, -0.65f);
            _wheelFrontDemon.CreateFixture(wheelShape);
            _wheelFrontDemon.Friction = 1.8f;

            Vector2 axisDemon = new Vector2(0.0f, -1.2f);
            _springBackDemon = new WheelJoint(truckDemonCollisionBox, _wheelBackDemon, _wheelBackDemon.Position, axisDemon, true);
            _springBackDemon.MotorSpeed = 0.0f;
            _springBackDemon.MaxMotorTorque = 5.0f;
            _springBackDemon.MotorEnabled = true;
            _springBackDemon.Frequency = 4.0f;
            _springBackDemon.DampingRatio = 0.7f;
            world.AddJoint(_springBackDemon);

            _springFrontDemon = new WheelJoint(truckDemonCollisionBox, _wheelFrontDemon, _wheelFrontDemon.Position, axisDemon, true);
            _springFrontDemon.MotorSpeed = 0.0f;
            _springFrontDemon.MaxMotorTorque = 5.0f;
            _springFrontDemon.MotorEnabled = true;
            _springFrontDemon.Frequency = 4.0f;
            _springFrontDemon.DampingRatio = 0.7f;
            world.AddJoint(_springFrontDemon);

            Vertices chassisShapeAngel = new Vertices(4);

            chassisShapeAngel.Add(ConvertUnits.ToSimUnits(-70, 0));
            chassisShapeAngel.Add(ConvertUnits.ToSimUnits(-70, 10));
            chassisShapeAngel.Add(ConvertUnits.ToSimUnits(65, 15));
            chassisShapeAngel.Add(ConvertUnits.ToSimUnits(75, -15));

            PolygonShape chassisAngel = new PolygonShape(chassisShapeAngel, 2);

            truckAngelCollisionBox = new Body(world);
            truckAngelCollisionBox.BodyType = BodyType.Dynamic;
            truckAngelCollisionBox.Position = new Vector2(18.0f, -1.0f);
            truckAngelCollisionBox.CreateFixture(chassisAngel);

            _wheelBackAngel = new Body(world);
            _wheelBackAngel.BodyType = BodyType.Dynamic;
            _wheelBackAngel.Position = new Vector2(17.90f, -0.65f);
            _wheelBackAngel.CreateFixture(wheelShape);
            _wheelBackAngel.Friction = 1.8f;

            wheelShape.Density = 1;
            _wheelFrontAngel = new Body(world);
            _wheelFrontAngel.BodyType = BodyType.Dynamic;
            _wheelFrontAngel.Position = new Vector2(18.50f, -0.65f);
            _wheelFrontAngel.CreateFixture(wheelShape);
            _wheelFrontAngel.Friction = 1.8f;

            Vector2 axisAngel = new Vector2(0.0f, -1.2f);
            _springBackAngel = new WheelJoint(truckAngelCollisionBox, _wheelBackAngel, _wheelBackAngel.Position, axisAngel, true);
            _springBackAngel.MotorSpeed = 0.0f;
            _springBackAngel.MaxMotorTorque = 5.0f;
            _springBackAngel.MotorEnabled = true;
            _springBackAngel.Frequency = 4.0f;
            _springBackAngel.DampingRatio = 0.7f;
            world.AddJoint(_springBackAngel);

            _springFrontAngel = new WheelJoint(truckAngelCollisionBox, _wheelFrontAngel, _wheelFrontAngel.Position, axisAngel, true);
            _springFrontAngel.MotorSpeed = 0.0f;
            _springFrontAngel.MaxMotorTorque = 5.0f;
            _springFrontAngel.MotorEnabled = true;
            _springFrontAngel.Frequency = 4.0f;
            _springFrontAngel.DampingRatio = 0.7f;
            world.AddJoint(_springFrontAngel);

            //camera.TrackingBody = body;
            //camera.EnableTracking = true;
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
            background.UnloadContent();

            truckAngel.UnloadContent();
            tireAngel.UnloadContent();

            truckDemon.UnloadContent();
            tireDemon.UnloadContent();
        }

        void Rear_OnSeperationAngel(Fixture a, Fixture b)
        {
            rearInTheAirAngel = true;
        }

        void Front_OnSeperationAngel(Fixture a, Fixture b)
        {
            frontInTheAirAngel = true;
        }

        bool Rear_OnCollisionAngel(Fixture a, Fixture b, Contact contact)
        {
            rearInTheAirAngel = false;

            return true;
        }

        bool Front_OnCollisionAngel(Fixture a, Fixture b, Contact contact)
        {
            frontInTheAirAngel = false;

            return true;
        }

        void Rear_OnSeperationDemon(Fixture a, Fixture b)
        {
            rearInTheAirDemon = true;
        }

        void Front_OnSeperationDemon(Fixture a, Fixture b)
        {
            frontInTheAirDemon = true;
        }

        bool Rear_OnCollisionDemon(Fixture a, Fixture b, Contact contact)
        {
            rearInTheAirDemon = false;
            return true;
        }

        bool Front_OnCollisionDemon(Fixture a, Fixture b, Contact contact)
        {
            frontInTheAirDemon = false;
            return true;
        }

        public override void Update(GameTime gameTime)
        {
            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));

            _wheelBackAngel.OnCollision += new OnCollisionEventHandler(Rear_OnCollisionAngel);
            _wheelFrontAngel.OnCollision += new OnCollisionEventHandler(Front_OnCollisionAngel);
            _wheelBackAngel.OnSeparation += new OnSeparationEventHandler(Rear_OnSeperationAngel);
            _wheelFrontAngel.OnSeparation += new OnSeparationEventHandler(Front_OnSeperationAngel);

            _springBackAngel.MotorSpeed = Math.Sign(_accelerationAngel) * MathHelper.SmoothStep(0f, MaxSpeed, Math.Abs(_accelerationAngel));
            _springFrontAngel.MotorSpeed = Math.Sign(_accelerationAngel) * MathHelper.SmoothStep(0f, MaxSpeed, Math.Abs(_accelerationAngel));
            if (Math.Abs(_springBackAngel.MotorSpeed) < MaxSpeed * 0.06f || (Math.Abs(_springFrontAngel.MotorSpeed) < MaxSpeed * 0.06f))
            {
                _springBackAngel.MotorEnabled = false;
                _springFrontAngel.MotorEnabled = false;
            }
            else
            {
                _springBackAngel.MotorEnabled = true;
                _springFrontAngel.MotorEnabled = true;
            }

            // Move our sprite based on arrow keys being pressed:
            if (InputManager.Instance.KeyDown(Keys.Right))
            {
                if (frontInTheAirAngel && rearInTheAirAngel)
                {
                    truckAngelCollisionBox.ApplyAngularImpulse(0.01f);
                }
                else
                {
                    _accelerationAngel = Math.Min(_accelerationAngel + (float)(5.0 * gameTime.ElapsedGameTime.TotalSeconds), 1f);
                    _springFrontAngel.MotorEnabled = false;
                    _springBackAngel.MotorEnabled = true;
                }
            }
            else if (InputManager.Instance.KeyDown(Keys.Left))
            {
                if (frontInTheAirAngel && rearInTheAirAngel)
                {
                    truckAngelCollisionBox.ApplyAngularImpulse(-0.01f);
                }
                else
                {
                    _accelerationAngel = Math.Max(_accelerationAngel - (float)(5.0 * gameTime.ElapsedGameTime.TotalSeconds), -1f);
                    _springBackAngel.MotorEnabled = false;
                    _springFrontAngel.MotorEnabled = true;
                }
            }
            else if (InputManager.Instance.KeyPressed(Keys.Down))
                _accelerationAngel = 0f;
            else
                _accelerationAngel -= Math.Sign(_accelerationAngel) * (float)(5.0 * gameTime.ElapsedGameTime.TotalSeconds);

            if (InputManager.Instance.KeyPressed(Keys.Up))
            {
                if (!rearInTheAirAngel && !frontInTheAirAngel)
                    truckAngelCollisionBox.ApplyForce(new Vector2(0, -250), truckAngelCollisionBox.Position + new Vector2(0.14f, 0f));
            }

            _wheelBackDemon.OnCollision += new OnCollisionEventHandler(Rear_OnCollisionDemon);
            _wheelFrontDemon.OnCollision += new OnCollisionEventHandler(Front_OnCollisionDemon);
            _wheelBackDemon.OnSeparation += new OnSeparationEventHandler(Rear_OnSeperationDemon);
            _wheelFrontDemon.OnSeparation += new OnSeparationEventHandler(Front_OnSeperationDemon);

            _springBackDemon.MotorSpeed = Math.Sign(_accelerationDemon) * MathHelper.SmoothStep(0f, MaxSpeed, Math.Abs(_accelerationDemon));
            _springFrontDemon.MotorSpeed = Math.Sign(_accelerationDemon) * MathHelper.SmoothStep(0f, MaxSpeed, Math.Abs(_accelerationDemon));
            if (Math.Abs(_springBackDemon.MotorSpeed) < MaxSpeed * 0.06f || (Math.Abs(_springFrontDemon.MotorSpeed) < MaxSpeed * 0.06f))
            {
                _springBackDemon.MotorEnabled = false;
                _springFrontDemon.MotorEnabled = false;
            }
            else
            {
                _springBackDemon.MotorEnabled = true;
                _springFrontDemon.MotorEnabled = true;
            }

            // Move our sprite based on arrow keys being pressed:
            if (InputManager.Instance.KeyDown(Keys.D))
            {
                if (frontInTheAirDemon && rearInTheAirDemon)
                {
                    truckDemonCollisionBox.ApplyAngularImpulse(0.01f);
                }
                else
                {
                    _accelerationDemon = Math.Min(_accelerationDemon + (float)(5.0 * gameTime.ElapsedGameTime.TotalSeconds), 1f);
                    _springFrontDemon.MotorEnabled = false;
                    _springBackDemon.MotorEnabled = true;
                }
            }
            else if (InputManager.Instance.KeyDown(Keys.A))
            {
                if (frontInTheAirDemon && rearInTheAirDemon)
                {
                    truckDemonCollisionBox.ApplyAngularImpulse(-0.01f);
                }
                else
                {
                    _accelerationDemon = Math.Max(_accelerationDemon - (float)(5.0 * gameTime.ElapsedGameTime.TotalSeconds), -1f);
                    _springBackDemon.MotorEnabled = false;
                    _springFrontDemon.MotorEnabled = true;
                }
            }
            else if (InputManager.Instance.KeyPressed(Keys.S))
                _accelerationDemon = 0f;
            else
                _accelerationDemon -= Math.Sign(_accelerationDemon) * (float)(5.0 * gameTime.ElapsedGameTime.TotalSeconds);

            if (InputManager.Instance.KeyPressed(Keys.W))
            {
                if (!rearInTheAirDemon && !frontInTheAirDemon)
                    truckDemonCollisionBox.ApplyForce(new Vector2(0, -250), truckDemonCollisionBox.Position + new Vector2(-0.175f, 0f));
            }

            background.Update(gameTime);
            truckAngel.Update(gameTime);
            tireAngel.Update(gameTime);
            truckDemon.Update(gameTime);
            tireDemon.Update(gameTime);
            camera.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            background.Draw(spriteBatch);

            truckAngel.Draw(spriteBatch, ConvertUnits.ToDisplayUnits(truckAngelCollisionBox.Position), truckAngelCollisionBox.Rotation, true);
            tireAngel.Draw(spriteBatch, ConvertUnits.ToDisplayUnits(_wheelBackAngel.Position), _wheelBackAngel.Rotation, false);
            tireAngel.Draw(spriteBatch, ConvertUnits.ToDisplayUnits(_wheelFrontAngel.Position), _wheelFrontAngel.Rotation, false);

            truckDemon.Draw(spriteBatch, ConvertUnits.ToDisplayUnits(truckDemonCollisionBox.Position), truckDemonCollisionBox.Rotation, false);
            tireDemon.Draw(spriteBatch, ConvertUnits.ToDisplayUnits(_wheelBackDemon.Position), _wheelBackDemon.Rotation, false);
            tireDemon.Draw(spriteBatch, ConvertUnits.ToDisplayUnits(_wheelFrontDemon.Position), _wheelFrontDemon.Rotation, false);

            spriteBatch.End();

            spriteBatch.Begin();
            //debug.RenderDebugData(ref camera.SimProjection, ref camera.SimView);
            base.Draw(spriteBatch);
        }

        public static Vector2 CalculateOrigin(Body b)
        {
            Vector2 lBound = new Vector2(float.MaxValue);
            Transform trans;
            b.GetTransform(out trans);

            for (int i = 0; i < b.FixtureList.Count; ++i)
            {
                for (int j = 0; j < b.FixtureList[i].Shape.ChildCount; ++j)
                {
                    AABB bounds;
                    b.FixtureList[i].Shape.ComputeAABB(out bounds, ref trans, j);
                    Vector2.Min(ref lBound, ref bounds.LowerBound, out lBound);
                }
            }

            // calculate body offset from its center and add a 1 pixel border
            // because we generate the textures a little bigger than the actual body's fixtures
            return ConvertUnits.ToDisplayUnits(b.Position - lBound) + new Vector2(1f);
        }
    }
}
