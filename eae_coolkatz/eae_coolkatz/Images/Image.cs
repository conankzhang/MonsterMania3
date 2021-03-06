﻿using eae_coolkatz.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace eae_coolkatz.Images
{
    public class Image
    {
        public float Alpha;
        public string Text, FontName, Path;

        public Vector2 Position, Scale;
        public Rectangle SourceRect;
        [XmlIgnore]
        public Texture2D Texture;
        public Vector2 Origin;
        ContentManager content;
        RenderTarget2D renderTarget;
        SpriteFont font;

        Dictionary<string, ImageEffect> effectList;
        public string Effects;

        public FadeEffect FadeEffect;
        public SpriteSheetEffect SpriteSheetEffect;

        public bool IsActive;
        public bool flipHorizontally;

        public Image()
        {
            Path = Text = Effects = string.Empty;
            FontName = "Fonts/Arial";
            Position = Vector2.Zero;
            Origin = new Vector2(SourceRect.Width / 2, SourceRect.Height / 2);
            Scale = Vector2.One;
            Alpha = 1.0f;
            SourceRect = Rectangle.Empty;
            effectList = new Dictionary<string, ImageEffect>();
        }

        public void StoreEffects()
        {
            Effects = string.Empty;
            foreach(var effect in effectList)
            {
                if(effect.Value.IsActive)
                {
                    Effects += effect.Key + ":";
                }
            }

            if(Effects != string.Empty)
            {
                Effects.Remove(Effects.Length - 1);
            }
        }

        public void RestoreEffects()
        {
            foreach(var effect in effectList)
            {
                DeactivateEffect(effect.Key);
            }
            string[] split = Effects.Split(':');
            foreach(string s in split)
            {
                ActivateEffect(s);
            }

        }

        void SetEffect<T>(ref T effect)
        {

            if(effect == null)
            {
                effect = (T)Activator.CreateInstance(typeof(T));
            }
            else
            {
                (effect as ImageEffect).IsActive = true;
                var obj = this;
                (effect as ImageEffect).LoadContent(ref obj);
            }

            effectList.Add(effect.GetType().ToString().Replace("eae_coolkatz.Images.", ""), (effect as ImageEffect));
        }

        public void ActivateEffect(string effect)
        {
            if(effectList.ContainsKey(effect))
            {
                effectList[effect].IsActive = true;
                var obj = this;
                effectList[effect].LoadContent(ref obj);
            }
        }

        public void DeactivateEffect(string effect)
        {
            if(effectList.ContainsKey(effect))
            {
                effectList[effect].IsActive = false;
                effectList[effect].UnloadContent();
            }

        }

        public void LoadContent()
        {
            content = new ContentManager(ScreenManager.Instance.Content.ServiceProvider, "Content");
            if(Path != string.Empty)
            {
                Texture = content.Load<Texture2D>(Path);
            }

            font = content.Load<SpriteFont>(FontName);
            
            Vector2 dimensions = Vector2.Zero;

            if(Texture != null)
            {
                dimensions.X += Texture.Width;
            }
            dimensions.X += font.MeasureString(Text).X;

            if(Texture != null)
            {
                dimensions.Y = Math.Max(Texture.Height, font.MeasureString(Text).Y);
            }
            else
            {
                dimensions.Y = font.MeasureString(Text).Y;
            }

            if(SourceRect == Rectangle.Empty)
            {
                SourceRect = new Rectangle(0, 0, (int)dimensions.X, (int)dimensions.Y);
            }

            renderTarget = new RenderTarget2D(ScreenManager.Instance.GraphicsDevice, (int)dimensions.X, (int)dimensions.Y);
            ScreenManager.Instance.GraphicsDevice.SetRenderTarget(renderTarget);
            ScreenManager.Instance.GraphicsDevice.Clear(Color.Transparent);
            ScreenManager.Instance.SpriteBatch.Begin();
            if(Texture != null)
            {
                ScreenManager.Instance.SpriteBatch.Draw(Texture, Vector2.Zero, Color.White);
            }
            ScreenManager.Instance.SpriteBatch.DrawString(font, Text, Vector2.Zero, Color.White);
            ScreenManager.Instance.SpriteBatch.End();

            Texture = renderTarget;

            ScreenManager.Instance.GraphicsDevice.SetRenderTarget(null);

            SetEffect<FadeEffect>(ref FadeEffect);
            SetEffect<SpriteSheetEffect>(ref SpriteSheetEffect);
            
            if(Effects != string.Empty)
            {
                string[] split = Effects.Split(':');
                foreach(string item in split)
                {
                    ActivateEffect(item);
                }
            }
        }

        public void UnloadContent()
        {
            content.Unload();
            foreach(var effect in effectList)
            {
                DeactivateEffect(effect.Key);
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach(var effect in effectList)
            {
                if(effect.Value.IsActive)
                {
                    effect.Value.Update(gameTime);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(flipHorizontally)
            {
                spriteBatch.Draw(Texture, Position + Origin, SourceRect, Color.White * Alpha, 0.0f, Origin, Scale, SpriteEffects.FlipHorizontally, 0.0f);
            }
            else
            {
                spriteBatch.Draw(Texture, Position + Origin, SourceRect, Color.White * Alpha, 0.0f, Origin, Scale, SpriteEffects.None, 0.0f);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 _origin)
        {
            if(flipHorizontally)
            {
                spriteBatch.Draw(Texture, Position + Origin, SourceRect, Color.White * Alpha, 0.0f, _origin, Scale, SpriteEffects.FlipHorizontally, 0.0f);
            }
            else
            {
                spriteBatch.Draw(Texture, Position + Origin, SourceRect, Color.White * Alpha, 0.0f, _origin, Scale, SpriteEffects.None, 0.0f);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation, bool flip, float scale)
        {
            Vector2 test = new Vector2(Texture.Width/2, Texture.Height/2);

            if(!flip)
                spriteBatch.Draw(Texture, position + Origin, SourceRect, Color.White * Alpha, rotation, test, scale, SpriteEffects.None, 0.0f);

            else
                spriteBatch.Draw(Texture, position + Origin, SourceRect, Color.White * Alpha, rotation, test, scale, SpriteEffects.FlipHorizontally, 0.0f);

        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation, bool flip, float scale, Rectangle source, float frameWidth, float frameHeight)
        {
            Vector2 test = new Vector2(frameWidth / 2, frameHeight / 2);

            if (!flip)
                spriteBatch.Draw(Texture, position + Origin, source, Color.White * Alpha, rotation, test, scale, SpriteEffects.None, 0.0f);

            else
                spriteBatch.Draw(Texture, position + Origin, source, Color.White * Alpha, rotation, test, scale, SpriteEffects.FlipHorizontally, 0.0f);

        }
    }
}
