using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace RogueLikeGame
{
    public class RogueLike : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private bool active;
        GameState state;
        Playing playing;
        Initializer init;
        TitleCard title;

        public RogueLike()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1380;
            graphics.PreferredBackBufferHeight = 880;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }
        protected override void OnDeactivated(object sender, EventArgs args)
        {
            active = false;
            base.OnDeactivated(sender, args);
        }
        protected override void OnActivated(object sender, EventArgs args)
        {
            active = true;
            base.OnActivated(sender, args);
        }
        public bool isActive() { return active; }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            //AspectRatio = GraphicsDevice.Viewport.AspectRatio;
            //OldWindowSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            init = new Initializer(spriteBatch, this);
            state = init;
        }

        public void changeGameState(string newState)
        {
            switch (newState)
            {
                case "playing":
                    {
                        playing = new Playing(spriteBatch, this);
                        state.leaving();
                        state = playing;
                        state.entering();
                        break;
                    }
                case "initialize":
                    {
                        init = new Initializer(spriteBatch, this);
                        state.leaving();
                        state = init;
                        state.entering();
                        break;
                    }
                case "title":
                    {
                        title = new TitleCard(spriteBatch, this);
                        state.leaving();
                        state = title;
                        state.entering();
                        break;
                    }
            }

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            //

            if (active)
            {
                state.update(gameTime);
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(20,12,28));
            spriteBatch.Begin();

            state.draw();

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
