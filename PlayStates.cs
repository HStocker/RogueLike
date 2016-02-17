using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace RogueLikeGame
{
    class Unpaused : GameState
    {
        private string tag = "Unpaused";
        KeyboardState state;
        Player player;
        Playing playing;
        List<Enemy> enemies;
        List<Item> items;
        List<Particle> particles;
        List<Projectile> projectiles;
        List<Box> boxes;
        List<StaticObject> staticObjects;
        Scene scene;
        TextBox textBox;

        Random rand = new Random();
        Color[] magicColors = { Playing.getColor("Yellow"), Playing.getColor("LightBlue"), Playing.getColor("Purple"), Playing.getColor("Purple"), Playing.getColor("Red"), Playing.getColor("Red") };
        
        //timers
        private int moveUpdate = 100;
        private int pauseUpdate = 128;
        private int escapeUpdate = 0;
        private int attackUpdate = 0;
        private int hitUpdate = 0;
        private int selectSwitchUpdate = 0;

        private int bossTimerA = 0;
        private int bossTimerB = 0;
        private int bossTimerC = 0;

        public Unpaused(Playing playing, Scene scene, Player player, List<Enemy> enemies, List<Item> items, List<Particle> particles, List<Projectile> projectiles, TextBox textBox, List<Box> boxes, List<StaticObject> staticObjects)
        {
            this.playing = playing;
            this.player = player;
            this.enemies = enemies;
            this.projectiles = projectiles;
            this.items = items;
            this.particles = particles;
            this.scene = scene;
            this.textBox = textBox;
            this.boxes = boxes;
            this.staticObjects = staticObjects;
            this.tag = "Unpaused";
        }

        public void update(GameTime gameTime)
        {
            //if (player.health <= 0) { playing.changeState("Paused"); }
            state = Keyboard.GetState();

            hitUpdate += gameTime.ElapsedGameTime.Milliseconds;
            moveUpdate += gameTime.ElapsedGameTime.Milliseconds;
            pauseUpdate += gameTime.ElapsedGameTime.Milliseconds;
            escapeUpdate += gameTime.ElapsedGameTime.Milliseconds;
            attackUpdate += gameTime.ElapsedGameTime.Milliseconds;
            selectSwitchUpdate += gameTime.ElapsedGameTime.Milliseconds;

            player.update(gameTime);
            textBox.update(gameTime);   

            if (scene.trigger(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] })) { scene.getEvent(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] }).trigger(); }
            if (player.onFire) { playing.addParticle(new Particle(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Red"))); }

            foreach (Particle particle in particles) { particle.update(gameTime); }
            foreach (StaticObject staticObject in staticObjects) { staticObject.update(gameTime); if (staticObject.particleTimer > staticObject.frequency && staticObject.frequency > 0) { playing.addParticle(new Particle(staticObject.coords, new int[] { 0, 0 }, staticObject.particleLife, staticObject.particles, Playing.getColor("White"))); staticObject.particleTimer = 0; } }
            foreach (Projectile projectile in projectiles)
            {
                switch (projectile.type)
                {
                    case "copper":
                        {

                            //!!! BUG: when shooting on edge tile, it throws index out of bounds
                            //this is because the animation travels backwars a small bit, causing 
                            //using math.round has helped a bit, but needs to be thought through more
                            //when you're not tired

                            if (projectile.velocity[0] * projectile.direction[0] < 0 || projectile.velocity[1] * projectile.direction[1] < 0
                               || scene.collides(new int[] { projectile.direction[0] + projectile.coords[0] + (int)Math.Round(projectile.pixMod[0] / 26.0) - playing.currentCorner[0], projectile.direction[1] + projectile.coords[1] + (int)Math.Round(projectile.pixMod[1] / 44.0) - playing.currentCorner[1] }))
                            {
                                for (int i = 0; i < 50; i++) { playing.addParticle(new Particle(new int[] { projectile.direction[0] + projectile.coords[0] + projectile.pixMod[0] / 26, projectile.direction[1] + projectile.coords[1] + projectile.pixMod[1] / 44 }, new int[] { rand.Next(-1, 2), rand.Next(-1, 2) }, 120, "magic", magicColors[rand.Next(0, 6)])); }
                                Enemy hit = null;
                                hit = enemies.Find(a => a.coords[0] - playing.currentCorner[0] == projectile.direction[0] + projectile.coords[0] + projectile.pixMod[0] / 26 - playing.currentCorner[0] && a.coords[1] - playing.currentCorner[1] == projectile.direction[1] + projectile.coords[1] + projectile.pixMod[1] / 44 - playing.currentCorner[1]);
                                if (hit != null) { hit.hit(Projectile.getDamage(projectile.type), new int[] { 0, 0 }); playing.addParticle(new Particle(hit.coords, player.facing, 240, string.Format("-{0}", Projectile.getDamage(projectile.type)), Playing.getColor("Red"))); } hit = null;
                                hit = enemies.Find(a => a.coords[0] - playing.currentCorner[0] == projectile.direction[0] + projectile.coords[0] + projectile.pixMod[0] / 26 - playing.currentCorner[0] + 1 && a.coords[1] - playing.currentCorner[1] == projectile.direction[1] + projectile.coords[1] + projectile.pixMod[1] / 44 - playing.currentCorner[1]);
                                if (hit != null) { hit.hit(Projectile.getDamage(projectile.type), new int[] { 1, 0 }); playing.addParticle(new Particle(hit.coords, player.facing, 240, string.Format("-{0}", Projectile.getDamage(projectile.type)), Playing.getColor("Red"))); } hit = null;
                                hit = enemies.Find(a => a.coords[0] - playing.currentCorner[0] == projectile.direction[0] + projectile.coords[0] + projectile.pixMod[0] / 26 - playing.currentCorner[0] - 1 && a.coords[1] - playing.currentCorner[1] == projectile.direction[1] + projectile.coords[1] + projectile.pixMod[1] / 44 - playing.currentCorner[1]);
                                if (hit != null) { hit.hit(Projectile.getDamage(projectile.type), new int[] { -1, 0 }); playing.addParticle(new Particle(hit.coords, player.facing, 240, string.Format("-{0}", Projectile.getDamage(projectile.type)), Playing.getColor("Red"))); } hit = null;
                                hit = enemies.Find(a => a.coords[0] - playing.currentCorner[0] == projectile.direction[0] + projectile.coords[0] + projectile.pixMod[0] / 26 - playing.currentCorner[0] && a.coords[1] - playing.currentCorner[1] == projectile.direction[1] + projectile.coords[1] + projectile.pixMod[1] / 44 - playing.currentCorner[1] + 1);
                                if (hit != null) { hit.hit(Projectile.getDamage(projectile.type), new int[] { 0, 1 }); playing.addParticle(new Particle(hit.coords, player.facing, 240, string.Format("-{0}", Projectile.getDamage(projectile.type)), Playing.getColor("Red"))); } hit = null;
                                hit = enemies.Find(a => a.coords[0] - playing.currentCorner[0] == projectile.direction[0] + projectile.coords[0] + projectile.pixMod[0] / 26 - playing.currentCorner[0] && a.coords[1] - playing.currentCorner[1] == projectile.direction[1] + projectile.coords[1] + projectile.pixMod[1] / 44 - playing.currentCorner[1] - 1);
                                if (hit != null) { hit.hit(Projectile.getDamage(projectile.type), new int[] { 0, -1 }); playing.addParticle(new Particle(hit.coords, player.facing, 240, string.Format("-{0}", Projectile.getDamage(projectile.type)), Playing.getColor("Red"))); }

                                projectile.toBeDeleted = true;
                            }
                            else if (projectile.time % 80 < 5) { playing.addParticle(new Particle(new int[] { projectile.direction[0] + projectile.coords[0] + projectile.pixMod[0] / 26, projectile.direction[1] + projectile.coords[1] + projectile.pixMod[1] / 44 }, new int[] { (-1) * projectile.direction[0], (-1) * projectile.direction[1] }, 120, "magic", Playing.getColor("Salmon"))); }

                            projectile.update(gameTime);

                            break;
                        }
                    case "arrow":
                        {
                            if (scene.collidesPlayer(new int[] { projectile.coords[0] + (int)Math.Round(projectile.pixMod[0] / 26.0) - playing.currentCorner[0], projectile.coords[1] + (int)Math.Round(projectile.pixMod[1] / 44.0) - playing.currentCorner[1] }))
                            { player.hit(1, projectile.direction); projectile.toBeDeleted = true; }
                            if (scene.collidesEnemy(new int[] { projectile.coords[0] + (int)Math.Round(projectile.pixMod[0] / 26.0), projectile.coords[1] + (int)Math.Round(projectile.pixMod[1] / 44.0) }))
                            { 
                                enemies.Find(a => a.coords[0] == projectile.coords[0] + (int)Math.Round(projectile.pixMod[0] / 26.0) && a.coords[1] == projectile.coords[1] + (int)Math.Round(projectile.pixMod[1] / 44.0)).hit(2, projectile.direction);
                                projectile.toBeDeleted = true;
                                playing.addParticle(new Particle(new int[] { projectile.coords[0] + (int)Math.Round(projectile.pixMod[0] / 26.0), projectile.coords[1] + (int)Math.Round(projectile.pixMod[1] / 44.0) }, projectile.direction, 120, "fire", Playing.getColor("Red")));
                                playing.addParticle(new Particle(new int[] { projectile.coords[0] + (int)Math.Round(projectile.pixMod[0] / 26.0), projectile.coords[1] + (int)Math.Round(projectile.pixMod[1] / 44.0) }, projectile.direction, 240, string.Format("-{0}", Projectile.getDamage(projectile.type)), Playing.getColor("Red")));
                            }

                            if (scene.collides(new int[] { projectile.coords[0] + (int)Math.Round(projectile.pixMod[0] / 26.0) - playing.currentCorner[0], projectile.coords[1] + (int)Math.Round(projectile.pixMod[1] / 44.0) - playing.currentCorner[1] })) { 
                                projectile.toBeDeleted = true; 
                                scene.breakObject(new int[] { projectile.coords[0] + (int)Math.Round(projectile.pixMod[0] / 26.0) - playing.currentCorner[0], projectile.coords[1] + (int)Math.Round(projectile.pixMod[1] / 44.0) - playing.currentCorner[1] }); 
                                playing.addParticle(new Particle(new int[] { projectile.coords[0] + projectile.pixMod[0] / 26, projectile.coords[1] + projectile.pixMod[1] / 44 }, projectile.direction, 120, "\u2737", Playing.getColor("LightGray"))); 
                            }
                            else { projectile.update(gameTime); }

                            break;
                        }
                    case "fireball":
                        {
                            if (scene.collidesPlayer(new int[] { projectile.coords[0] + (int)Math.Round(projectile.pixMod[0] / 26.0) - playing.currentCorner[0], projectile.coords[1] + (int)Math.Round(projectile.pixMod[1] / 44.0) - playing.currentCorner[1] }))
                            { player.hit(2, projectile.direction); projectile.toBeDeleted = true; }
                            if (scene.collides(new int[] { projectile.coords[0] + (int)Math.Round(projectile.pixMod[0] / 26.0) - playing.currentCorner[0], projectile.coords[1] + (int)Math.Round(projectile.pixMod[1] / 44.0) - playing.currentCorner[1] }))
                            {
                                projectile.toBeDeleted = true;
                                scene.breakObject(new int[] { projectile.coords[0] + (int)Math.Round(projectile.pixMod[0] / 26.0) - playing.currentCorner[0], projectile.coords[1] + (int)Math.Round(projectile.pixMod[1] / 44.0) - playing.currentCorner[1] });
                                playing.addParticle(new Particle(new int[] { projectile.coords[0] + projectile.pixMod[0] / 26, projectile.coords[1] + projectile.pixMod[1] / 44 }, projectile.direction, 140, "fire", Playing.getColor("Red")));
                                playing.addParticle(new Particle(new int[] { projectile.coords[0] + projectile.pixMod[0] / 26, projectile.coords[1] + projectile.pixMod[1] / 44 }, projectile.direction, 140, "fire", Playing.getColor("Sienna")));
                                playing.addParticle(new Particle(new int[] { projectile.coords[0] + projectile.pixMod[0] / 26, projectile.coords[1] + projectile.pixMod[1] / 44 }, projectile.direction, 140, "fire", Playing.getColor("Red")));
                                playing.addParticle(new Particle(new int[] { projectile.coords[0] + projectile.pixMod[0] / 26, projectile.coords[1] + projectile.pixMod[1] / 44 }, projectile.direction, 140, "fire", Playing.getColor("Yellow")));
                                playing.addParticle(new Particle(new int[] { projectile.coords[0] + projectile.pixMod[0] / 26, projectile.coords[1] + projectile.pixMod[1] / 44 }, projectile.direction, 140, "fire", Playing.getColor("Red")));
                                playing.addParticle(new Particle(new int[] { projectile.coords[0] + projectile.pixMod[0] / 26, projectile.coords[1] + projectile.pixMod[1] / 44 }, projectile.direction, 140, "fire", Playing.getColor("Red")));
                            }
                            else { projectile.update(gameTime); }

                            break;
                        }
                }
            }

            if (moveUpdate >= player.speed)
            {
                moveUpdate = 0;
                if (state.IsKeyDown(Keys.W)) { if (player.moveUp(state)) { playing.currentCorner[1]--; } }
                else if (state.IsKeyDown(Keys.A)) { if (player.moveLeft(state)) { playing.currentCorner[0]--; } }
                else if (state.IsKeyDown(Keys.D)) { if (player.moveRight(state)) { playing.currentCorner[0]++; } }
                else if (state.IsKeyDown(Keys.S)) { if (player.moveDown(state)) { playing.currentCorner[1]++; } }
            }
            player.getFacing(state);
            
            Item pickup = null;
            pickup = items.Find(item => item.coords[0] - playing.currentCorner[0] == player.coords[0] && item.coords[1] - playing.currentCorner[1] == player.coords[1]);
            if (pickup != null && player.inventory.Count < 8) { player.process(pickup); items.Remove(pickup); }
            if (state.IsKeyDown(Keys.Escape) && escapeUpdate > 128)
            {
                escapeUpdate = 0;
                playing.changeState("Paused");
            }

            foreach (Enemy enemy in enemies)
            {
                enemy.update(gameTime);
                if (enemy.aggressive && playing.isSeen(enemy.coords))
                {
                    switch (enemy.getTag())
                    {

                        case "sword":
                            {
                                if (!enemy.attacking && !enemy.swinging && !enemy.dying) 
                                {
                                    //Debug.Print(Convert.ToString((player.coords[0] + playing.currentCorner[0])+","+ (player.coords[1] + playing.currentCorner[1])));
                                    enemy.setDestination(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] });
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] + 1 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1]
                                        || player.coords[0] + playing.currentCorner[0] == enemy.coords[0] && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] + 1
                                        || player.coords[0] + playing.currentCorner[0] == enemy.coords[0] -1 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1]
                                        || player.coords[0] + playing.currentCorner[0] == enemy.coords[0]  && player.coords[1] + playing.currentCorner[1] == enemy.coords[1]-1)
                                    {
                                        enemy.attack(gameTime, new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] });
                                    }
                                }
                                if (enemy.attacking) { if (!enemy.hasHit && player.coords[0] + playing.currentCorner[0] == enemy.coords[0] + enemy.direction[0] && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] + enemy.direction[1])
                                { player.hit(2, enemy.direction); enemy.hasHit = true; }
                                }
                                break;
                            }
                        case "bow":
                            {
                                if (enemy.coords[0] - player.coords[0] + playing.currentCorner[0] != 0 && enemy.coords[1] - player.coords[1] + playing.currentCorner[1] != 0) { enemy.setDestination(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] }); }
                                else if (Playing.getTilesInbetween( player.coords, new int[] { enemy.coords[0] - playing.currentCorner[0], enemy.coords[1] - playing.currentCorner[1]}).Find(a => scene.isSolid(a)) != null) { enemy.setDestination(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] }); }
                                if (Math.Abs(enemy.coords[0] - player.coords[0] + playing.currentCorner[0]) + Math.Abs(enemy.coords[1] - player.coords[1] + playing.currentCorner[1]) < 6) { enemy.path.Clear(); }
                                
                                if (!enemy.dying && !enemy.swinging && (enemy.coords[0] - player.coords[0] - playing.currentCorner[0] == 0 || enemy.coords[1] - player.coords[1] - playing.currentCorner[1] == 0))
                                { enemy.attack(gameTime, new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] });}
                                if (!enemy.hasHit) { enemy.hasHit = true; playing.addProjectile(new Projectile(new int[] { enemy.coords[0] + enemy.direction[0], enemy.coords[1] + enemy.direction[1] }, enemy.direction, new int[] {0,0}, "arrow", scene)); }
                                break;
                            }
                        case "galvanized tome":
                            {
                                if (enemy.hasHit) { bossTimerA += gameTime.ElapsedGameTime.Milliseconds; if (bossTimerA > 1500) { bossTimerA = 0; enemy.hasHit = false; } }
                                if (!enemy.swinging && !enemy.hasHit && Math.Abs(player.coords[0] + playing.currentCorner[0] - enemy.coords[0]) + Math.Abs(player.coords[1] + playing.currentCorner[1] - enemy.coords[1]) < 4)
                                { enemy.attack(gameTime, new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] }); }
                                else if (!enemy.swinging) { enemy.setDestination(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] }); }

                                bossTimerC += gameTime.ElapsedGameTime.Milliseconds;
                                if (!enemy.swinging && !enemy.attacking && bossTimerC > 650 && Math.Abs(player.coords[0] + playing.currentCorner[0] - enemy.coords[0]) + Math.Abs(player.coords[1] + playing.currentCorner[1] - enemy.coords[1]) > 3 && (enemy.coords[0] - player.coords[0] - playing.currentCorner[0] == 0 || enemy.coords[1] - player.coords[1] - playing.currentCorner[1] == 0)) 
                                { 
                                    bossTimerC = 0;
                                    playing.addProjectile(new Projectile(new int[] { enemy.coords[0] + enemy.getDirection(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] })[0], enemy.coords[1] + enemy.getDirection(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] })[1] }, enemy.getDirection(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] }), new int[] { 0, 0 }, "fireball", scene)); 
                                }

                                if (enemy.swinging && !enemy.attacking) 
                                {
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] + 1, enemy.coords[1] }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] + 1, enemy.coords[1] + 1 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] + 1, enemy.coords[1] - 1 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] - 1, enemy.coords[1] }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] - 1, enemy.coords[1] + 1 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] - 1, enemy.coords[1] - 1 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0], enemy.coords[1] + 1 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0], enemy.coords[1] - 1 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] + 2, enemy.coords[1] }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] - 2, enemy.coords[1] }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0], enemy.coords[1] + 2 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0], enemy.coords[1] - 2 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] + 2, enemy.coords[1] + 2 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] + 2, enemy.coords[1] - 2 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] - 2, enemy.coords[1] + 2 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                    playing.addParticle(new Particle(new int[] { enemy.coords[0] - 2, enemy.coords[1] - 2 }, new int[] { 0, 0 }, 10, "boss1prefire", Playing.getColor("Yellow")));
                                }
                                if (enemy.attacking)
                                {
                                    if (!enemy.hasHit)
                                    {
                                        if(scene.includesTile(new int[] { enemy.coords[0] + 1, enemy.coords[1] })){playing.addParticle(new Particle(new int[] { enemy.coords[0] + 1, enemy.coords[1] }, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow")));}
                                        if (scene.includesTile(new int[] { enemy.coords[0] + 1, enemy.coords[1] + 1 })) { playing.addParticle(new Particle(new int[] { enemy.coords[0] + 1, enemy.coords[1] + 1 }, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] + 1, enemy.coords[1] - 1 })) { playing.addParticle(new Particle(new int[] { enemy.coords[0] + 1, enemy.coords[1] - 1 }, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] - 1, enemy.coords[1] })) { playing.addParticle(new Particle(new int[] { enemy.coords[0] - 1, enemy.coords[1] }, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] - 1, enemy.coords[1] + 1})) { playing.addParticle(new Particle(new int[] { enemy.coords[0] - 1, enemy.coords[1] + 1 }, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] - 1, enemy.coords[1] - 1})) { playing.addParticle(new Particle(new int[] { enemy.coords[0] - 1, enemy.coords[1] - 1}, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] , enemy.coords[1] + 1})) { playing.addParticle(new Particle(new int[] { enemy.coords[0] , enemy.coords[1] +1}, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] , enemy.coords[1] - 1})) { playing.addParticle(new Particle(new int[] { enemy.coords[0] , enemy.coords[1] -1}, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] + 2, enemy.coords[1] })) { playing.addParticle(new Particle(new int[] { enemy.coords[0] + 2, enemy.coords[1] }, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] + 2, enemy.coords[1] + 2})) { playing.addParticle(new Particle(new int[] { enemy.coords[0] + 2, enemy.coords[1] + 2 }, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] + 2, enemy.coords[1] - 2})) { playing.addParticle(new Particle(new int[] { enemy.coords[0] + 2, enemy.coords[1] -2}, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] - 2, enemy.coords[1] })) { playing.addParticle(new Particle(new int[] { enemy.coords[0] - 2, enemy.coords[1] }, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] - 2, enemy.coords[1] + 2})) { playing.addParticle(new Particle(new int[] { enemy.coords[0] - 2, enemy.coords[1] +2}, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0] - 2, enemy.coords[1] - 2})) { playing.addParticle(new Particle(new int[] { enemy.coords[0] - 2, enemy.coords[1] -2}, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0], enemy.coords[1] + 2})) { playing.addParticle(new Particle(new int[] { enemy.coords[0], enemy.coords[1] +2}, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        if (scene.includesTile(new int[] { enemy.coords[0], enemy.coords[1] - 2})) { playing.addParticle(new Particle(new int[] { enemy.coords[0], enemy.coords[1] -2}, new int[] { 0, 0 }, 600, "boss1fire", Playing.getColor("Yellow"))); }
                                        enemy.hasHit = true;
                                    }

                                    bossTimerB += gameTime.ElapsedGameTime.Milliseconds;
                                    if (bossTimerB > 30)
                                    {
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] + 1, enemy.coords[1] }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] + 1, enemy.coords[1] + 1 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] + 1, enemy.coords[1] - 1 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] - 1, enemy.coords[1] }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] - 1, enemy.coords[1] + 1 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] - 1, enemy.coords[1] - 1 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0], enemy.coords[1] + 1 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0], enemy.coords[1] - 1 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] + 2, enemy.coords[1] }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] - 2, enemy.coords[1] }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0], enemy.coords[1] + 2 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0], enemy.coords[1] - 2 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] + 2, enemy.coords[1] + 2 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] + 2, enemy.coords[1] - 2 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] - 2, enemy.coords[1] + 2 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        playing.addParticle(new Particle(new int[] { enemy.coords[0] - 2, enemy.coords[1] - 2 }, new int[] { 0, 0 }, 140, "fire", Playing.getColor("Yellow")));
                                        bossTimerB = 0;
                                    }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] + 1 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1]) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] + 1 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] + 1) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] + 1 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] - 1) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] - 1 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1]) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] - 1 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] + 1) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] - 1 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] - 1) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] + 1) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] - 1) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] + 2 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1]) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] + 2 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] + 2) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] + 2 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] - 2) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] - 2 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1]) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] - 2 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] + 2) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] - 2 && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] - 2) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] + 2) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                    if (player.coords[0] + playing.currentCorner[0] == enemy.coords[0] && player.coords[1] + playing.currentCorner[1] == enemy.coords[1] - 2) { player.onFire = true; if (!enemy.hasHit) { enemy.hasHit = true; player.hit(6, new int[] { 0, 0 }); } }
                                }
                                break;
                            }
                    }
                }
                if (enemy.dying) { enemy.death(gameTime); if (enemy.toBeDeleted) { items.Add(new Item(enemy.coords, scene, enemy.drop)); } playing.addParticle(new Particle(enemy.coords, player.facing, 140, "stars", Playing.getColor("LightBlue"))); }
            }

            if (state.IsKeyDown(Keys.Tab) && selectSwitchUpdate > 128)
            {
                if (player.select < player.inventory.Count - 1) { player.select++; }
                else { player.select = 0; }
                selectSwitchUpdate = 0;
            }

            if (state.IsKeyDown(Keys.Space) && attackUpdate > 380)
            {
                attackUpdate = 0;
                switch (player.inventory[player.select])
                {
                    case "fist":
                        {
                            player.attacking = true;
                            Enemy hit = null;
                            hit = enemies.Find(a => a.coords[0] - playing.currentCorner[0] == player.coords[0] + player.facing[0] && a.coords[1] - playing.currentCorner[1] == player.coords[1] + player.facing[1]);
                            if (hit != null)
                            {
                                playing.addParticle(new Particle(hit.coords, player.facing, 250, string.Format("-{0}", Item.getDamage(player.inventory[player.select])), Playing.getColor("Red")));
                                hit.hit(Item.getDamage(player.inventory[player.select]), player.facing);
                                if (scene.collides(hit.coords[0] - playing.currentCorner[0], hit.coords[1] - playing.currentCorner[1])) { hit.coords[0] -= player.facing[0]; hit.coords[1] -= player.facing[1]; }
                            }
                            if (scene.collides(new int[] { player.coords[0] + player.facing[0], player.coords[1] + player.facing[1] })) { playing.addParticle(new Particle(new int[] { player.coords[0] + player.facing[0] + playing.currentCorner[0], player.coords[1] + player.facing[1] + playing.currentCorner[1] }, player.facing, 140, "\u2737", Playing.getColor("LightGray"))); }
                            break;
                        }
                    case "bow":
                        {
                            if (!player.charging) { player.charging = true; player.speed = 420; }
                            break;
                        }
                    case "sword": { goto case "fist"; }
                    case "long sword":
                        {
                            player.attacking = true;
                            List<Enemy> hit = new List<Enemy>();
                            hit = enemies.FindAll(
                                a => a.coords[0] - playing.currentCorner[0] == player.coords[0] + player.facing[0] && a.coords[1] - playing.currentCorner[1] == player.coords[1] + player.facing[1]
                                || a.coords[0] - playing.currentCorner[0] == player.coords[0] + player.facing[0] + player.facing[1] && a.coords[1] - playing.currentCorner[1] == player.coords[1] + player.facing[1] + player.facing[0]
                                || a.coords[0] - playing.currentCorner[0] == player.coords[0] + player.facing[0] - player.facing[1] && a.coords[1] - playing.currentCorner[1] == player.coords[1] + player.facing[1] - player.facing[0]
                                );
                            foreach (Enemy enemy in hit)
                            {
                                playing.addParticle(new Particle(enemy.coords, player.facing, 250, string.Format("-{0}", Item.getDamage(player.inventory[player.select])), Playing.getColor("Red")));
                                enemy.hit(Item.getDamage(player.inventory[player.select]), player.facing);
                                if (scene.collides(enemy.coords[0] - playing.currentCorner[0], enemy.coords[1] - playing.currentCorner[1])) { enemy.coords[0] -= player.facing[0]; enemy.coords[1] -= player.facing[1]; }
                            }
                            //hit boxes
                            if (scene.collides(new int[] { player.coords[0] + player.facing[0], player.coords[1] + player.facing[1] })) { playing.addParticle(new Particle(new int[] { player.coords[0] + player.facing[0] + playing.currentCorner[0], player.coords[1] + player.facing[1] + playing.currentCorner[1] }, player.facing, 140, "\u2737", Playing.getColor("LightGray"))); }
                            if (scene.collides(new int[] { player.coords[0] + player.facing[0] + player.facing[1], player.coords[1] + player.facing[1] + player.facing[0] })) { playing.addParticle(new Particle(new int[] { player.coords[0] + player.facing[0] + player.facing[0] + playing.currentCorner[0], player.coords[1] + player.facing[1] + player.facing[0] + playing.currentCorner[1] }, player.facing, 140, "\u2737", Playing.getColor("LightGray"))); }
                            if (scene.collides(new int[] { player.coords[0] + player.facing[0] - player.facing[1], player.coords[1] + player.facing[1] - player.facing[1] })) { playing.addParticle(new Particle(new int[] { player.coords[0] + player.facing[0] - player.facing[0] + playing.currentCorner[0], player.coords[1] + player.facing[1] - player.facing[0] + playing.currentCorner[1] }, player.facing, 140, "\u2737", Playing.getColor("LightGray"))); }
                            break;
                        }
                    case "food": { if (player.health > 6) { player.health = 10; } else { player.health += 3; } player.inventory.RemoveAt(player.select); player.select--; break; }
                    case "key":
                        {
                            if (scene.getTile(new int[] { player.coords[0] + player.facing[0] + playing.currentCorner[0], player.coords[1] + player.facing[1] + playing.currentCorner[1] }).getNum() == 2)
                            {
                                List<int[]> tiles = new List<int[]>();
                                int[] local = new int[] { player.coords[0] + player.facing[0] + playing.currentCorner[0], player.coords[1] + player.facing[1] + playing.currentCorner[1] };
                                tiles.Add(local);

                                if (scene.getTile(new int[] { local[0] + 1, local[1] }).getNum() == 2) { tiles.Add(new int[] { local[0] + 1, local[1] }); }
                                if (scene.getTile(new int[] { local[0], local[1] + 1 }).getNum() == 2) { tiles.Add(new int[] { local[0], local[1] + 1 }); }
                                if (scene.getTile(new int[] { local[0] - 1, local[1] }).getNum() == 2) { tiles.Add(new int[] { local[0] - 1, local[1] }); }
                                if (scene.getTile(new int[] { local[0], local[1] - 1 }).getNum() == 2) { tiles.Add(new int[] { local[0], local[1] - 1 }); }

                                foreach (int[] tile in tiles)
                                {
                                    scene.setCollision(tile[0], tile[1], 0);
                                }
                                player.inventory.RemoveAt(player.select);
                                player.select--;
                            }
                            break;
                        }
                    case "copper tome":
                        {
                            player.attacking = true;
                            playing.addProjectile(new Projectile(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] }, player.facing, new int[] { 0, 0 }, "copper", scene));
                            break;
                        }
                    case "malachite tome":
                        {
                            player.attacking = true;
                            playing.addProjectile(new Projectile(new int[] { player.coords[0] + playing.currentCorner[0], player.coords[1] + playing.currentCorner[1] }, player.facing, new int[] { 0, 0 }, "malachite", scene));
                            break;
                        }
                }
                scene.breakObject(new int[] { player.coords[0] + player.facing[0], player.coords[1] + player.facing[1]});
            }
            else if (player.attacking) { player.attack(gameTime); }
            if ((state.IsKeyUp(Keys.Space) && player.charging) || (player.charging && !player.inventory[player.select].Equals("bow")))
            {
                player.charging = false; player.speed = 120;
                if (player.inventory[player.select].Equals("bow"))
                {
                    //if (player.chargeTimer > 820) { playing.addProjectile(new Projectile(new int[] { player.coords[0] + playing.currentCorner[0] + player.facing[0], player.coords[1] + playing.currentCorner[1] + player.facing[1] }, player.facing, new int[] { player.facing[0]*-3, player.facing[1]*-3 }, "arrow", scene)); }
                    //else if (player.chargeTimer > 520) { playing.addProjectile(new Projectile(new int[] { player.coords[0] + playing.currentCorner[0] + player.facing[0], player.coords[1] + playing.currentCorner[1] + player.facing[1] }, player.facing, new int[] { 0,0 }, "arrow", scene)); }
                    //else if (player.chargeTimer > 280) { playing.addProjectile(new Projectile(new int[] { player.coords[0] + playing.currentCorner[0] + player.facing[0], player.coords[1] + playing.currentCorner[1] + player.facing[1] }, player.facing, new int[] { player.facing[0] * -1, player.facing[1] * -1 }, "arrow", scene)); }
                    //else if (player.chargeTimer > 80) { playing.addProjectile(new Projectile(new int[] { player.coords[0] + playing.currentCorner[0] + player.facing[0], player.coords[1] + playing.currentCorner[1] + player.facing[1] }, player.facing, new int[] { player.facing[0] * -2, player.facing[1] * -2 }, "arrow", scene)); }
                    //else { playing.addProjectile(new Projectile(new int[] { player.coords[0] + playing.currentCorner[0] + player.facing[0], player.coords[1] + playing.currentCorner[1] + player.facing[1] }, player.facing, new int[] { player.facing[0] * -3, player.facing[1] * -3 }, "arrow", scene)); }
                }
                player.chargeTimer = 0;
            }

        }

        public void draw()
        {

        }

        public void entering()
        {

        }

        public void leaving()
        {
            enemies.Clear();
            particles.Clear();
            projectiles.Clear();
            staticObjects.Clear();
            boxes.Clear();
            scene.events.Clear();
            textBox.clear();
        }

        public string getTag()
        {
            return tag;
        }
    }
    class Paused : GameState
    {
        private string tag = "Paused";
        Playing playing;
        KeyboardState state;
        private int escapeUpdate = 0;
        public ingameMenu menu;

        public Paused(Playing playing, ingameMenu menu)
        {
            this.playing = playing;
            this.menu = menu;
            this.tag = "Paused";
        }

        public void update(GameTime gameTime)
        {
            escapeUpdate += gameTime.ElapsedGameTime.Milliseconds;
            state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape) && escapeUpdate >= 128) { escapeUpdate = 0; playing.changeState("Unpaused"); }
        }

        public void draw()
        {

        }

        public void entering()
        {

        }

        public void leaving()
        {
            this.menu.clear();
        }

        public string getTag()
        {
            return tag;
        }
    }
    class ingameMenu
    {
        RogueLike ingame;
        KeyboardState keyboard;
        KeyboardState oldkeyboardState;
        SpriteFont output;
        Playing playing;

        string entry = "";
        string outputText = "";

        public ingameMenu(RogueLike ingame, SpriteFont output, Playing playing) { this.playing = playing; this.ingame = ingame; this.output = output; }


        public string draw()
        {
            return "> " + entry + "\n" + outputText;
        }
        public void clear() { entry = ""; outputText = ""; }

        public void update()
        {
            keyboard = Keyboard.GetState();
            char key;
            TryConvertKeyboardInput(keyboard, oldkeyboardState, out key);
            oldkeyboardState = keyboard;
            //Debug.Print(Convert.ToString(key));
            if (!key.Equals('\0') && entry.Length <= 56)
            {
                entry += Convert.ToString(key);
            }
            string[] outputSplit = outputText.Split('\n');
            if (outputSplit.Length > 570 / 32)
            {
                outputText = "\n" + string.Join("\n", outputSplit.Skip(outputSplit.Length - 570 / 32));
                //outputText = outputSplit.Skip(outputSplit.Length - 25).; 
            }
        }

        public void parseCommand(string command)
        {
            string[] key = command.ToUpper().Split(' ');
            switch (key[0])
            {
                case "HELP": { outputText += "\n<" + command + ">" + "\nCOMMANDS:\nNew           -> Start a new game\nSaved         -> List of saved games\nLoad [name]   -> Load a saved game\nDelete [name] -> Delete a saved game\nClear         -> Clear the console\nAbout         -> About the game\nQuit          -> Exit the game"; break; }
                case "CLEAR": { outputText = ""; break; }
                case "NEW": { outputText += "\n<" + command + ">" + "\nNEW"; break; }
                case "LOAD": { outputText += "\n<" + command + ">" + "\nOutput text if is broken"; break; }
                case "SAVE": { outputText += "\n<" + command + ">" + "\nList saved games here"; break; }
                case "ABOUT": { outputText += "\n<" + command + ">" + "\nThis is a game by Hank!  Thank you for playing!"; break; }
                case "QUIT": { ingame.Exit(); break; }
                case "EXIT": { ingame.Exit(); break; }
                case "MENU": { ingame.changeGameState("initialize"); break; }
                case "COLLIDE": { playing.collisionMarkers = key[1] == "TRUE"; break; }
                default: { outputText += "\n<" + command + ">" + "\nI do not recognize your command: '" + command.Split(' ')[0] + "' \n-- Please type 'help' for a list of accepted commands."; break; }
            }
        }

        public bool TryConvertKeyboardInput(KeyboardState keyboard, KeyboardState oldKeyboard, out char key)
        {
            Keys[] keys = keyboard.GetPressedKeys();

            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            for (int i = 0; i < keys.Length; i++)
            {
                if (!oldKeyboard.IsKeyDown(keys[i]))
                {
                    switch (keys[i])
                    {
                        //Alphabet keys
                        case Keys.A: if (shift) { key = 'A'; } else { key = 'a'; } return true;
                        case Keys.B: if (shift) { key = 'B'; } else { key = 'b'; } return true;
                        case Keys.C: if (shift) { key = 'C'; } else { key = 'c'; } return true;
                        case Keys.D: if (shift) { key = 'D'; } else { key = 'd'; } return true;
                        case Keys.E: if (shift) { key = 'E'; } else { key = 'e'; } return true;
                        case Keys.F: if (shift) { key = 'F'; } else { key = 'f'; } return true;
                        case Keys.G: if (shift) { key = 'G'; } else { key = 'g'; } return true;
                        case Keys.H: if (shift) { key = 'H'; } else { key = 'h'; } return true;
                        case Keys.I: if (shift) { key = 'I'; } else { key = 'i'; } return true;
                        case Keys.J: if (shift) { key = 'J'; } else { key = 'j'; } return true;
                        case Keys.K: if (shift) { key = 'K'; } else { key = 'k'; } return true;
                        case Keys.L: if (shift) { key = 'L'; } else { key = 'l'; } return true;
                        case Keys.M: if (shift) { key = 'M'; } else { key = 'm'; } return true;
                        case Keys.N: if (shift) { key = 'N'; } else { key = 'n'; } return true;
                        case Keys.O: if (shift) { key = 'O'; } else { key = 'o'; } return true;
                        case Keys.P: if (shift) { key = 'P'; } else { key = 'p'; } return true;
                        case Keys.Q: if (shift) { key = 'Q'; } else { key = 'q'; } return true;
                        case Keys.R: if (shift) { key = 'R'; } else { key = 'r'; } return true;
                        case Keys.S: if (shift) { key = 'S'; } else { key = 's'; } return true;
                        case Keys.T: if (shift) { key = 'T'; } else { key = 't'; } return true;
                        case Keys.U: if (shift) { key = 'U'; } else { key = 'u'; } return true;
                        case Keys.V: if (shift) { key = 'V'; } else { key = 'v'; } return true;
                        case Keys.W: if (shift) { key = 'W'; } else { key = 'w'; } return true;
                        case Keys.X: if (shift) { key = 'X'; } else { key = 'x'; } return true;
                        case Keys.Y: if (shift) { key = 'Y'; } else { key = 'y'; } return true;
                        case Keys.Z: if (shift) { key = 'Z'; } else { key = 'z'; } return true;

                        //Decimal keys
                        case Keys.D0: if (shift) { key = ')'; } else { key = '0'; } return true;
                        case Keys.D1: if (shift) { key = '!'; } else { key = '1'; } return true;
                        case Keys.D2: if (shift) { key = '@'; } else { key = '2'; } return true;
                        case Keys.D3: if (shift) { key = '#'; } else { key = '3'; } return true;
                        case Keys.D4: if (shift) { key = '$'; } else { key = '4'; } return true;
                        case Keys.D5: if (shift) { key = '%'; } else { key = '5'; } return true;
                        case Keys.D6: if (shift) { key = '^'; } else { key = '6'; } return true;
                        case Keys.D7: if (shift) { key = '&'; } else { key = '7'; } return true;
                        case Keys.D8: if (shift) { key = '*'; } else { key = '8'; } return true;
                        case Keys.D9: if (shift) { key = '('; } else { key = '9'; } return true;

                        //Decimal numpad keys
                        case Keys.NumPad0: key = '0'; return true;
                        case Keys.NumPad1: key = '1'; return true;
                        case Keys.NumPad2: key = '2'; return true;
                        case Keys.NumPad3: key = '3'; return true;
                        case Keys.NumPad4: key = '4'; return true;
                        case Keys.NumPad5: key = '5'; return true;
                        case Keys.NumPad6: key = '6'; return true;
                        case Keys.NumPad7: key = '7'; return true;
                        case Keys.NumPad8: key = '8'; return true;
                        case Keys.NumPad9: key = '9'; return true;

                        //Special keys
                        case Keys.OemTilde: if (shift) { key = '~'; } else { key = '`'; } return true;
                        case Keys.OemSemicolon: if (shift) { key = ':'; } else { key = ';'; } return true;
                        case Keys.OemQuotes: if (shift) { key = '"'; } else { key = '\''; } return true;
                        case Keys.OemQuestion: if (shift) { key = '?'; } else { key = '/'; } return true;
                        case Keys.OemPlus: if (shift) { key = '+'; } else { key = '='; } return true;
                        case Keys.OemPipe: if (shift) { key = '|'; } else { key = '\\'; } return true;
                        case Keys.OemPeriod: if (shift) { key = '>'; } else { key = '.'; } return true;
                        case Keys.OemOpenBrackets: if (shift) { key = '{'; } else { key = '['; } return true;
                        case Keys.OemCloseBrackets: if (shift) { key = '}'; } else { key = ']'; } return true;
                        case Keys.OemMinus: if (shift) { key = '_'; } else { key = '-'; } return true;
                        case Keys.OemComma: if (shift) { key = '<'; } else { key = ','; } return true;
                        case Keys.Space: key = ' '; return true;
                        case Keys.Back: key = '\0'; if (entry.Length > 0) { entry = entry.Substring(0, entry.Length - 1); }; return true;
                        //change enter command to put result up and clear entry
                        case Keys.Enter: key = '\0'; this.parseCommand(entry); entry = ""; return true;
                    }
                }
            }

            key = (char)0;
            return false;
        }
    }
}
