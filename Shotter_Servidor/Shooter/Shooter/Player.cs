using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Shooter
{
    class Player
    {
        public Vector2 Position;
        public bool Active;
        public int Health;
        public Animation playerAnimation;
        public int Width
        {
            get { return this.playerAnimation.FrameWidth; }
        }
        public int Height
        {
            get { return this.playerAnimation.FrameHeight; }
        }


        public void Initialize(Animation animation, Vector2 position)
            {
                this.playerAnimation = animation;
                this.Position = position;
                this.Active = true;
                this.Health = 100;
            }

        public void Update(GameTime gameTime)
            {
                this.playerAnimation.Position = Position;
                this.playerAnimation.Update(gameTime);
            }

        public void Draw(SpriteBatch spriteBatch)
            {
                    this.playerAnimation.Draw(spriteBatch);
            }
    }
}
