using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using UdpServer;
namespace Shooter
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        Servidor s;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Player player;
        Player player2;

        //Inputs del teclado al juego
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        //Inputs del control al juego
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        //Velocidad del jugador
        float playerMoveSpeed;

        // Image used to display the static background
        Texture2D mainBackground;

        // Parallaxing Layers
        ParallaxingBackground bgLayer1;
        ParallaxingBackground bgLayer2;

        // Enemies
        Texture2D enemyTexture;
        List<Enemy> enemies;

        // The rate at which the enemies appear
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        // A random number generator
        Random random;

        Texture2D projectileTexture;
        List<Projectile> projectiles;
        List<Projectile> projectiles2;

        // The rate of fire of the player laser
        TimeSpan fireTime;
        TimeSpan previousFireTime;
        TimeSpan previousFireTime2;

        Texture2D explosionTexture;
        List<Animation> explosions;

        // The sound that is played when a laser is fired
        SoundEffect laserSound;

        // The sound used when the player or an enemy dies
        SoundEffect explosionSound;

        // The music played during gameplay
        Song gameplayMusic;

        //Number that holds the player score
        int score;
        int score2;
        // The font used to display UI elements
        SpriteFont font;

        string parser;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            player = new Player();
            player2 = new Player();
            playerMoveSpeed = 8.0f;
            s = new Servidor();
            s.start();
            // Initialize the enemies list
            enemies = new List<Enemy>();

            // Set the time keepers to zero
            previousSpawnTime = TimeSpan.Zero;

            // Used to determine how fast enemy respawns
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            // Initialize our random number generator
            random = new Random();

            TouchPanel.EnabledGestures = GestureType.FreeDrag;

            bgLayer1 = new ParallaxingBackground();
            bgLayer2 = new ParallaxingBackground();

            projectiles = new List<Projectile>();
            projectiles2 = new List<Projectile>();

            // Set the laser to fire every quarter second
            fireTime = TimeSpan.FromSeconds(.15f);

            explosions = new List<Animation>();

            score = 0;
            score2 = 0;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load the player resources
            
            Animation playerAnimation = new Animation();
            Animation player2Animation = new Animation();
            Texture2D playerTexture = Content.Load<Texture2D>("shipAnimation");
            Texture2D player2Texture = Content.Load<Texture2D>("shipAnimation2");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);
            
            enemyTexture = Content.Load<Texture2D>("mineAnimation");

            player2Animation.Initialize(player2Texture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);

            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y
            + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            Vector2 player2Position = new Vector2(GraphicsDevice.Viewport.Width + (50), GraphicsDevice.Viewport.TitleSafeArea.Y
            + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            player.Initialize(playerAnimation, playerPosition);
            player2.Initialize(player2Animation,player2Position);
            // Load the parallaxing background
            bgLayer1.Initialize(Content, "bgLayer1", GraphicsDevice.Viewport.Width, -1);
            bgLayer2.Initialize(Content, "bgLayer2", GraphicsDevice.Viewport.Width, -2);

            mainBackground = Content.Load<Texture2D>("mainbackground");
            spriteBatch = new SpriteBatch(GraphicsDevice);

            projectileTexture = Content.Load<Texture2D>("laser");

            explosionTexture = Content.Load<Texture2D>("explosion");

            // Load the music
            gameplayMusic = Content.Load<Song>("sound/gameMusic");

            // Load the laser and explosion sound effect
            laserSound = Content.Load<SoundEffect>("sound/laserFire");
            explosionSound = Content.Load<SoundEffect>("sound/explosion");

            font = Content.Load<SpriteFont>("gameFont");

            // Start the music right away
            PlayMusic(gameplayMusic);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || currentKeyboardState.IsKeyDown(Keys.Escape))
            {
               // MediaPlayer.Stop();
                //s.Close();
                this.Exit();
            }

            // Save the previous state of the keyboard and game pad so we can determinesingle key/button presses
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;

            // Read the current state of the keyboard and gamepad and store it
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);


            //Update the player
            UpdatePlayer(gameTime);

            UpdatePlayer2(gameTime);

            actualizaInstruccion(gameTime);

            UpdateCollision();

            UpdateProjectiles();

            UpdateExplosions(gameTime);

            
            // Update the parallaxing background
            bgLayer1.Update();
            bgLayer2.Update();

            UpdateParser();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

            // Draw the moving background
            bgLayer1.Draw(spriteBatch);
            bgLayer2.Draw(spriteBatch);

            // Draw the explosions
            for (int i = 0; i < explosions.Count; i++)
            {
                explosions[i].Draw(spriteBatch);
            }

            // Draw the Projectiles
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Draw(spriteBatch);
            }
            for (int i = 0; i < projectiles2.Count; i++)
            {
                projectiles2[i].Draw(spriteBatch);
            }

            if(player.Active){
                player.Draw(spriteBatch);
            }
            if (player2.Active)
            {
                player2.Draw(spriteBatch);
            }
            //spriteBatch.DrawString(font, parser, new Vector2(0, GraphicsDevice.Viewport.Height / 2), Color.White);
            if(!player.Active && !player2.Active){
                spriteBatch.DrawString(font, "DRAW!!", new Vector2(GraphicsDevice.Viewport.Width / 2 - 100, GraphicsDevice.Viewport.Height / 2), Color.White);
            }
            else if(!player.Active){
                spriteBatch.DrawString(font,"PLAYER 2 WINS!!",new Vector2(GraphicsDevice.Viewport.Width/2-100,GraphicsDevice.Viewport.Height/2),Color.White);
            }
            else if (!player2.Active)
            {
                spriteBatch.DrawString(font, "PLAYER 1 WINS!!", new Vector2(GraphicsDevice.Viewport.Width / 2 - 100, GraphicsDevice.Viewport.Height / 2), Color.White);
            }
            // Draw the score
            spriteBatch.DrawString(font, "Score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
            // Draw the player health
            spriteBatch.DrawString(font, "Health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);
            spriteBatch.DrawString(font, "Score: " + score2, new Vector2(GraphicsDevice.Viewport.Width - (player.Width+50), GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
            // Draw the player health
            spriteBatch.DrawString(font, "Health: " + player2.Health, new Vector2(GraphicsDevice.Viewport.Width - (player.Width + 50), GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
        
        private void UpdatePlayer(GameTime gameTime)
        {
            
            player.Update(gameTime);
            // Windows Phone Controls
            if (player.Active)
            {
                while (TouchPanel.IsGestureAvailable)
                {
                    GestureSample gesture = TouchPanel.ReadGesture();
                    if (gesture.GestureType == GestureType.FreeDrag)
                    {
                        player.Position += gesture.Delta;
                    }
                }

                // Get Thumbstick Controls
                player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
                player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

                // Use the Keyboard / Dpad
                if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentGamePadState.DPad.Left == ButtonState.Pressed)
                {
                    player.Position.X -= playerMoveSpeed;
                }
                if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentGamePadState.DPad.Right == ButtonState.Pressed)
                {
                    player.Position.X += playerMoveSpeed;
                }
                if (currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentGamePadState.DPad.Up == ButtonState.Pressed)
                {
                    player.Position.Y -= playerMoveSpeed;
                }
                if (currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentGamePadState.DPad.Down == ButtonState.Pressed)
                {
                    player.Position.Y += playerMoveSpeed;
                }
                if (currentKeyboardState.IsKeyDown(Keys.Space))
                {
                    laserSound.Play();
                    // Fire only every interval we set as the fireTime
                    if (gameTime.TotalGameTime - previousFireTime2 > fireTime)
                    {
                        // Reset our current time
                        previousFireTime2 = gameTime.TotalGameTime;

                        // Add the projectile, but add it to the front and center of the player
                        AddProjectile(player.Position + new Vector2(player.Width / 2,0));
                    }
                }

                // Make sure that the player does not go out of bounds
                player.Position.X = MathHelper.Clamp(player.Position.X, 40, (GraphicsDevice.Viewport.Width / 2 - player.Width));
                player.Position.Y = MathHelper.Clamp(player.Position.Y, 35, GraphicsDevice.Viewport.Height - player.Height + 35);

                // reset score if player health goes to zero
            }
        }

        private void UpdateParser()
        {
            string player1Pos = "*"+player.Position.X+","+player.Position.Y;
            string player2Pos = "*"+player2.Position.X + "," + player2.Position.Y;
            string balas = "";
            int cont = 0;
            for (int i = projectiles.Count - 1; i >= 0; i--,cont++)
            {
                if (i == 0)
                {
                    string proyectil = projectiles[i].Position.X + "," + projectiles[i].Position.Y;
                    balas += proyectil;
                }
                else
                {
                    string proyectil = projectiles[i].Position.X + "," + projectiles[i].Position.Y+"|";
                    balas += proyectil;
                }
            }
            parser = balas + player1Pos + "%"+player.Health+"@";
            balas = "";
            for (int i = projectiles2.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    string proyectil = projectiles2[i].Position.X + "," + projectiles2[i].Position.Y;
                    balas += proyectil;
                }
                else
                {
                    string proyectil = projectiles2[i].Position.X + "," + projectiles2[i].Position.Y + "|";
                    balas += proyectil;

                }
            }
            parser += balas + player2Pos+"%"+player2.Health;
            s.message = parser;
        }

        private void UpdatePlayer2(GameTime gameTime)
        {
            player2.Update(gameTime);
            
                
                // Make sure that the player does not go out of bounds
            player2.Position.X = MathHelper.Clamp(player2.Position.X, GraphicsDevice.Viewport.Width / 2 + player2.Width, GraphicsDevice.Viewport.Width - 50);
            player2.Position.Y = MathHelper.Clamp(player2.Position.Y, 35, GraphicsDevice.Viewport.Height - player2.Height + 35);
            
            // reset score if player health goes to zero
            if (player2.Health <= 0)
            {
                player2.Active = false;
            }
            else {
                player2.Active = true;
            }


        }
        private void UpdateCollision()
        {
            Rectangle rectangle1;
            Rectangle rectangle2;
            Rectangle rectangle3;

            // Only create the rectangle once for the player
            rectangle1 = new Rectangle((int)player.Position.X,
            (int)player.Position.Y,
            player.Width,
            player.Height);

            rectangle2 = new Rectangle((int)player2.Position.X,
            (int)player2.Position.Y,
            player2.Width,
            player2.Height);

            // Projectile vs Enemy Collision
            for (int i = 0; i < projectiles.Count; i++)
            {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle3 = new Rectangle((int)projectiles[i].Position.X -
                    projectiles[i].Width / 2, (int)projectiles[i].Position.Y -
                    projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);

                    // Determine if the two objects collided with each other
                    if (rectangle3.Intersects(rectangle2) && player2.Active)
                    {
                        player2.Health -= 10;
                        score += 10;
                        projectiles[i].Active = false;
                        if(player2.Health <= 0){
                            AddExplosion(player2.Position);
                            player2.Active = false;
                            
                        }
                    }

            }
            for (int i = 0; i < projectiles2.Count; i++)
            {
                // Create the rectangles we need to determine if we collided with each other
                rectangle3 = new Rectangle((int)projectiles2[i].Position.X -
                projectiles2[i].Width / 2, (int)projectiles2[i].Position.Y -
                projectiles2[i].Height / 2, projectiles2[i].Width, projectiles2[i].Height);

                // Determine if the two objects collided with each other
                if (rectangle3.Intersects(rectangle1) && player.Active)
                {
                    player.Health -= 10;
                    score2 += 10;
                    projectiles2[i].Active = false;
                    if (player.Health <= 0)
                    {
                        AddExplosion(player.Position);
                        player.Active = false;

                    }
                }

            }
        }
        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile();
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position,1);
            projectiles.Add(projectile);
        }
        private void AddProjectile2(Vector2 position)
        {
            Projectile projectile2 = new Projectile();
            projectile2.Initialize(GraphicsDevice.Viewport, projectileTexture, position,2);
            projectiles2.Add(projectile2);
        }
        private void UpdateProjectiles()
        {
            // Update the Projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update();

                if (projectiles[i].Active == false)
                {
                    projectiles.RemoveAt(i);
                }
            }
            for (int i = projectiles2.Count - 1; i >= 0; i--)
            {
                projectiles2[i].Update();

                if (projectiles2[i].Active == false)
                {
                    projectiles2.RemoveAt(i);
                }
            }
        }
        private void AddExplosion(Vector2 position)
        {
            Animation explosion = new Animation();
            explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
            explosions.Add(explosion);
            explosionSound.Play();
        }
        private void UpdateExplosions(GameTime gameTime)
        {
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                explosions[i].Update(gameTime);
                if (explosions[i].Active == false)
                {
                    explosions.RemoveAt(i);
                }
            }
        }

        private void UpdatePorParser(string parsa)
        {
            if (!string.IsNullOrEmpty(parsa))
            {
                string[] arr = parsa.Split('@');
                string p1 = arr[0];
                string p2 = arr[1];
                if (!string.IsNullOrEmpty(p1))
                {
                    arr = p1.Split('*');
                    string balas1 = arr[0];
                    string dp1 = arr[1];
                    arr = dp1.Split('%');
                    player.Health = Convert.ToInt32(arr[1]);
                    string[] pos = arr[0].Split(',');
                    player.Position.X = Convert.ToInt32(pos[0]);
                    player.Position.Y = Convert.ToInt32(pos[1]);
                    arr = balas1.Split('|');
                    List<Projectile> lista = new List<Projectile>();
                    for(int i = 0;i<arr.Length;i++){
                        string []tmp = arr[i].Split(',');
                        Projectile projectile = new Projectile();
                        projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, new Vector2(Convert.ToInt32(arr[0]), Convert.ToInt32(arr[1])), 1);
                        lista.Add(projectile);
                    }
                    projectiles = lista;
                }
                if (!string.IsNullOrEmpty(p2))
                {
                    arr = p2.Split('*');
                    string balas2 = arr[0];
                    string dp2 = arr[1];
                    arr = dp2.Split('%');
                    player2.Health = Convert.ToInt32(arr[1]);
                    string[] pos = arr[0].Split(',');
                    player2.Position.X = Convert.ToInt32(pos[0]);
                    player2.Position.Y = Convert.ToInt32(pos[1]);
                    arr = balas2.Split('|');
                    List<Projectile> lista = new List<Projectile>();
                    for (int i = 0; i < arr.Length; i++)
                    {
                        string[] tmp = arr[i].Split(',');
                        Projectile projectile = new Projectile();
                        projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, new Vector2(Convert.ToInt32(arr[0]), Convert.ToInt32(arr[1])), 1);
                        lista.Add(projectile);
                    }
                    projectiles2 = lista;
                }
            }
        }
        private void actualizaInstruccion(GameTime gameTime)
        {
            string linea = s.GetNextInstruction();            
                switch(linea){
                    case "ARRIBA": player2.Position.Y -= playerMoveSpeed; break;
                    case "ABAJO": player2.Position.Y += playerMoveSpeed; break;
                    case "DERECHA": player2.Position.X += playerMoveSpeed; break;
                    case "IZQUIERDA": player2.Position.X -= playerMoveSpeed; break;
                    case "DISPARO": laserSound.Play();
                        // Fire only every interval we set as the fireTime
                        if (gameTime.TotalGameTime - previousFireTime > fireTime)
                        {
                            // Reset our current time
                            previousFireTime = gameTime.TotalGameTime;

                            // Add the projectile, but add it to the front and center of the player
                            AddProjectile2(player2.Position);
                        } break;
            }
        }
        protected override void OnExiting(Object sender, EventArgs args)
        {
            
            s.Close();
            MediaPlayer.Stop();
            //this.Exit();
            base.OnExiting(sender, args);
            // Stop the threads
        }

        private void PlayMusic(Song song)
        {
            // Due to the way the MediaPlayer plays music,
            // we have to catch the exception. Music will play when the game is not tethered
            try
            {
                // Play the music
                MediaPlayer.Play(song);

                // Loop the currently playing song
                MediaPlayer.IsRepeating = true;
            }
            catch { }
        }
    }
}
