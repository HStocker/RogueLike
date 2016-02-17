using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace RogueLikeGame
{
    class TitleCard : GameState
    {
        SpriteBatch spriteBatch;
        RogueLike ingame;
        Texture2D overlay;
        SpriteFont output128;
        Vector2 textLocation = new Vector2(384, 100);
        KeyboardState state;
        int titleTimer = 0;
        public TitleCard(SpriteBatch spriteBatch, RogueLike ingame)
        {

            this.spriteBatch = spriteBatch;
            this.ingame = ingame;
            overlay = ingame.Content.Load<Texture2D>("overlay.png");
            output128 = ingame.Content.Load<SpriteFont>("Output128pt");
        }
        public void update(GameTime gameTime)
        {
            titleTimer += gameTime.ElapsedGameTime.Milliseconds;

            state = Keyboard.GetState();

            if (textLocation.Y < 360) { textLocation.Y += (float)(.008) * (390 - textLocation.Y); }
            if (textLocation.Y > 360) { textLocation.Y = 360; }
            if (state.IsKeyDown(Keys.Escape)) { ingame.changeGameState("playing"); }
            else if (titleTimer >= 4000) { ingame.changeGameState("playing"); }
        }
        public void draw()
        {
            spriteBatch.DrawString(output128, "Wizard Game", textLocation, Playing.getColor("White"));
            spriteBatch.Draw(overlay, new Rectangle(0, 0, 1360, 960), Color.White);
        }
        public void entering()
        {

        }
        public void leaving()
        {

        }
        public string getTag() { return "title"; }
    }
}
