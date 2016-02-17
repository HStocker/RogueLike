using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace RogueLikeGame
{
    class Player : Drawable
    {
        Scene scene;
        Playing playing;
        public int[] facing = { 0, 1 };

        public bool onFire = false;
        public bool attacking = false;
        public bool charging = false;
        public bool damaged = false;
        public int damageTimer = 0;
        public int chargeTimer = 0;

        int attackTimer = 0;
        public int health = 4;
        public int select = 0;
        public int hitcount = 0;
        public int speed = 120;
        public Color color;
        public int sight = 12;
        public List<string> inventory = new List<string>();
        KeyboardState state;
        public Dictionary<Keys, int[]> facingDict = new Dictionary<Keys,int[]>();
        public Player(int[] coords, Scene scene, Playing playing)
        {
            this.playing = playing;
            this.color = Playing.getColor("White");
            inventory.Add("fist");
            this.coords = coords;
            this.scene = scene;
        }
        public void update(GameTime gameTime)
        {
            if (damaged) { damageTimer += gameTime.ElapsedGameTime.Milliseconds; }
            if (damageTimer > 180) { this.color = Playing.getColor("White"); damageTimer = 0; damaged = false; }
            if (charging)
            {
                chargeTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (chargeTimer > 820) { this.color = Playing.getColor("Gray"); }
                else if (chargeTimer > 520) { this.color = Playing.getColor("LightBlue"); if (chargeTimer % 10 < 3) { playing.addParticle(new Particle(new int[] { coords[0] + playing.currentCorner[0], coords[1] + playing.currentCorner[1] }, new int[] { 0, -1 }, 120, "charge", Playing.getColor("LightBlue"))); } }
                else if (chargeTimer > 280) { this.color = Playing.getColor("Blue"); playing.addParticle(new Particle(new int[] { coords[0] + playing.currentCorner[0], coords[1] + playing.currentCorner[1] }, new int[] { 0, -1 }, 100, "charge", Playing.getColor("Blue"))); }
                else if (chargeTimer > 80) { this.color = Playing.getColor("LightGray"); playing.addParticle(new Particle(new int[] { coords[0] + playing.currentCorner[0], coords[1] + playing.currentCorner[1] }, new int[] { 0, -1 }, 80, "charge", Playing.getColor("Purple"))); }
            }
            else { chargeTimer = 0; this.color = Playing.getColor("White"); }
        }
        public bool getFacing(KeyboardState state) 
        {
            this.state = state;
            if (state.IsKeyDown(Keys.Left)) { facing = new int[] { -1, 0 }; return true; }
            if (state.IsKeyDown(Keys.Right)) { facing = new int[] { 1, 0 }; return true; }
            if (state.IsKeyDown(Keys.Up)) { facing = new int[] { 0, -1 }; return true; }
            if (state.IsKeyDown(Keys.Down)) { facing = new int[] { 0, 1 }; return true; }
            return false;
        }

        public bool moveLeft(KeyboardState state) { this.state = state; if (!this.getFacing(state)) { facing = new int[] { -1, 0 }; } if (scene.collides(new int[] { this.coords[0] - 1, this.coords[1] })) { return false; } return true; }
        public bool moveRight(KeyboardState state) { this.state = state; if (!this.getFacing(state)) { facing = new int[] { 1, 0 }; } if (scene.collides(new int[] { this.coords[0] + 1, this.coords[1] })) { return false; } return true; }
        public bool moveUp(KeyboardState state) { this.state = state; if (!this.getFacing(state)) { facing = new int[] { 0, -1 }; } if (scene.collides(new int[] { this.coords[0], this.coords[1] - 1 })) { return false; } return true; }
        public bool moveDown(KeyboardState state) { this.state = state; if (!this.getFacing(state)) { facing = new int[] { 0, 1 }; } if (scene.collides(new int[] { this.coords[0], this.coords[1] + 1})) { return false; } return true; }

        public void attack(GameTime gameTime)
        {
            attackTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (attackTimer > 80) { attacking = false; attackTimer = 0; }
        }
        public void process(Item item)
        {
            switch (item.getTag())
            {
                case "life": { this.health++; break; }
                case "sword": { if (!inventory.Contains("sword")) { inventory.Add("sword"); } select = inventory.IndexOf("sword"); break; }
                case "long sword": { if (!inventory.Contains("long sword")) { inventory.Add("long sword"); } select = inventory.IndexOf("long sword"); break; }
                case "bow": { if (!inventory.Contains("bow")) { inventory.Add("bow"); } select = inventory.IndexOf("bow"); break; }
                case "copper tome": { if (!inventory.Contains("copper tome")) { inventory.Add("copper tome"); } select = inventory.IndexOf("copper tome"); break; }
                case "malachite tome": { if (!inventory.Contains("malachite tome")) { inventory.Add("malachite tome"); } select = inventory.IndexOf("malachite tome"); break; }
                default: { inventory.Add(item.getTag()); break; }
            }
        }
        public void hit(int damage, int[] direction)
        {
            playing.hit.Play();
            hitcount++;
            damaged = true;
            this.color = Playing.getColor("Red");
            this.health -= damage;
            playing.addParticle(new Particle(new int[] {coords[0] + playing.currentCorner[0],coords[1]+playing.currentCorner[1]},new int[] {0,0},240,string.Format("-{0}",damage),Playing.getColor("Red")));
            if (!scene.collides(new int[] { coords[0] + direction[0], coords[1] + direction[1] }))
            {
                playing.currentCorner[0] += direction[0];
                playing.currentCorner[1] += direction[1];
            }
        }
    }

    class Box : Drawable
    {
        Scene scene;
        public string uniVal;
        public string drop;
        public int eventId;

        public Box(int[] coords, Scene scene, string uniVal, string drop, int eventId)
        {
            this.scene = scene;
            this.coords = coords;
            this.uniVal = char.ConvertFromUtf32(Convert.ToInt32(uniVal));
            this.drop = drop;
            this.eventId = eventId;
        }
    }
    class StaticObject : Drawable
    {
        public int[] pixMod;
        Playing playing;
        public string icon;
        public string type;
        public bool collision;
        public bool destructible;
        public bool solid;
        public string particles;
        public int frequency;
        public int particleTimer = 0;
        public int particleLife;
        public Color color;
        //9604 , 9607 , 9749 (table?
        public StaticObject(int[] coords, string icon, string type, string collision, string particles, int frequency, string color, Playing playing, int[] pixMod, bool destructible, bool solid, int particleLife) 
        {
            this.particleLife = particleLife;
            this.solid = solid;
            this.destructible = destructible;
            this.pixMod = pixMod;
            this.coords = coords;
            this.icon = char.ConvertFromUtf32(Convert.ToInt32(icon));
            this.playing = playing;
            this.color = Playing.getColor(color);
            //this.icon = "\u25B2";
            this.type = type;
            this.collision = collision == "true";
            this.particles = particles;
            this.frequency = frequency;
        }
        public void destroy() { if (destructible) { this.toBeDeleted = true; playing.addParticle(new Particle(coords, new int[] { 0, 0 }, 120, "stars", Playing.getColor("LightBlue"))); } }
        public void update(GameTime gameTime) 
        {
            particleTimer += gameTime.ElapsedGameTime.Milliseconds;
        }
    }
    class Projectile : Drawable
    {
        //\u2736
        Scene scene;
        int decayTimer = 0;
        public int[] direction;
        public int[] velocity;
        int[] decay = new int[] { 0, 0 };
        public int[] pixMod = new int[] { 0, 0 };
        public Color color;
        public string icon;
        public string type;

        public Projectile(int[] coords, int[] direction, int[] decay, string type, Scene scene)
        {
            this.coords = coords;
            this.direction = direction;
            this.type = type;
            switch (type)
            {
                case "arrow": { this.velocity = new int[] { 3 * direction[0], 3 * direction[1] }; this.icon = "\u2727"; this.color = Playing.getColor("Red"); break; }
                case "copper": { this.velocity = new int[] { 3 * direction[0], 3 * direction[1] }; this.decay = new int[] { direction[0] * (-1), direction[1] * (-1) }; this.icon = "\u2600"; this.color = Playing.getColor("Yellow"); break; }
                case "malachite": { this.velocity = new int[] { 3 * direction[0], 3 * direction[1] }; this.icon = "\u2600"; this.color = Playing.getColor("Green"); break; }
                case "fireball": { this.velocity = new int[] { 3 * direction[0], 3 * direction[1] }; this.icon = "\u2600"; this.color = Playing.getColor("Red"); break; }
            }
            this.coords = coords;
            this.scene = scene;
        }
        public void update(GameTime gameTime)
        {
            this.time += gameTime.ElapsedGameTime.Milliseconds;
            this.decayTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (decayTimer > 150) { this.velocity = new int[] { velocity[0] + decay[0], velocity[1] + decay[1] }; decayTimer = 0; }
            this.pixMod = new int[] { (int)(4 * velocity[0]) + pixMod[0], (int)(4 * velocity[1]) + pixMod[1] };

        }
        public static int getDamage(string type)
        {
            switch (type)
            {
                case "arrow": { return 4; }
                case "copper": { return 18; }
                case "malachite": { return 18; }
                default: { return 0; }
            }
        }
    }
    class Particle : Drawable
    {
        int[] direction;
        double[] velocity;
        int[] decay = new int[] { 0, 0 };
        public int[] pixMod = new int[] { 0, 0 };
        public Color color;
        string[] icon;
        int lifespan;

        Random rand = new Random(Guid.NewGuid().GetHashCode());

        public Particle(int[] start, int[] direction, int lifespan, string type, Color color)
        {
            this.lifespan = lifespan;
            this.level = 1;
            this.color = color;
            this.direction = direction;
            switch (type)
            {
                case "stars":
                    {
                        this.icon = new string[] { "\u273A", "\u2739", "\u2738", "\u2737", "\u2736" };
                        this.setTag(icon[0]);
                        this.direction = new int[] { rand.Next(-1, 2), rand.Next(-1, 2) };
                        break;
                    }
                case "blood":
                    {
                        this.icon = new string[] { "\u273F", "\u273F", "\u273F", "\u273E", "\u273E" };
                        this.setTag(icon[0]);
                        break;
                    }
                case "magic":
                    {
                        this.icon = new string[] { "\u25CB", "\u25E6", "\u25CC", "\u25E6", "\u25E6" };
                        this.setTag(icon[0]);
                        this.direction = new int[] { direction[0] + rand.Next(-1, 2), direction[1] + rand.Next(-1, 2) };
                        break;
                    }
                case "fire":
                    {
                        this.icon = new string[] { "\u25BC", "\u2666", "\u25BE", "\u25BD", "\u25CF", "\u25BF" };
                        this.setTag(icon[0]);
                        this.direction = new int[] { 0, -1 };
                        Color[] colors = new Color[] { Playing.getColor("Red"), Playing.getColor("Yellow"), Playing.getColor("White"), Playing.getColor("Salmon") };
                        this.color = colors[rand.Next(colors.Length)];
                        this.pixMod[0] = rand.Next(-10, 11);
                        break;
                        //FIRE: 25BD  25CF 25BF
                        //      25BC  2666 25BE
                    }
                case "barrier":
                    {
                        this.icon = new string[] { "\u25CB", "\u25E6", "\u25CC", "\u25E6", "\u25E6" };
                        this.setTag(icon[0]);
                        this.direction = new int[] { 3, 0 };
                        Color[] colors = new Color[] { Playing.getColor("LightGreen"), Playing.getColor("Purple"), Playing.getColor("LightBlue"), Playing.getColor("Yellow") };
                        this.color = colors[rand.Next(colors.Length)];
                        this.pixMod[1] = rand.Next(-10, 11);
                        break;
                    }
                case "light":
                    { //\u25AA  \u25BE
                        this.icon = new string[] { "*", "+", ":", "-", "`" };
                        this.setTag(icon[0]);
                        this.direction = new int[] { rand.Next(-1,2), 1 };
                        Color[] colors = new Color[] { Playing.getColor("White"), Playing.getColor("LightBlue"), Playing.getColor("White"), Playing.getColor("Blue") };
                        this.color = colors[rand.Next(colors.Length)];
                        this.pixMod[0] = rand.Next(-10, 11);
                        break;
                    }
                case "boss1prefire":
                    {
                        this.icon = new string[] { "\u2591", "\u2588", "\u2588", "\u2588", "\u2588" };
                        this.setTag(icon[0]);
                        this.direction = new int[] { 0, 0 };
                        Color[] colors = new Color[] { Playing.getColor("Red"), Playing.getColor("Yellow"), Playing.getColor("Yellow"), Playing.getColor("Sienna") };
                        this.color = colors[rand.Next(colors.Length)];
                        this.pixMod = new int[] {rand.Next(-1, 2),rand.Next(-1,2)};
                        break;
                    }
                case "boss1fire":
                    {
                        this.icon = new string[] { "\u2593", "\u2588", "\u2588", "\u2588", "\u2593" };
                        this.setTag(icon[0]);
                        this.direction = new int[] { 0, 0 };
                        Color[] colors = new Color[] { Playing.getColor("Red"), Playing.getColor("Red"), Playing.getColor("Red"), Playing.getColor("Red") };
                        this.color = colors[rand.Next(colors.Length)];
                        this.pixMod = new int[] { 0, 0 };
                        break;
                    }
                case "charge":
                    {
                        this.icon = new string[] { "\u25B5", "\u25B5", "\u25B5", "\u25B5", "\u25B5" };
                        this.setTag(icon[0]);
                        this.pixMod = new int[] { rand.Next(-15,15), 0 };
                        break;
                    }
                default:
                    {
                        this.icon = new string[] { type, type, type, type, type };
                        this.setTag(icon[0]);
                        this.direction = new int[] { 0,-1 };
                        break;
                    }
            }
            this.coords = start;
            velocity = new double[] { rand.NextDouble() + .5, rand.NextDouble() + .5 };
        }
        public void update(GameTime gameTime)
        {
            this.time += gameTime.ElapsedGameTime.Milliseconds;
            if (this.time > lifespan) { this.toBeDeleted = true; }
            else
            {
                if (this.time > 60) { this.setTag(icon[1]); }
                if (this.time > 130) { this.setTag(icon[2]); }
                if (this.time > 190) { this.setTag(icon[3]); }
                if (this.time > 230) { this.setTag(icon[4]); }
                this.pixMod = new int[] { (int)(4 * direction[0] * velocity[0]) + pixMod[0], (int)(4 * direction[1] * velocity[1]) + pixMod[1] };
            }
        }
        public int[] getLocation() { return coords; }
    }
    class Item : Drawable
    {
        Scene scene;
        string uniValue;
        public Color color;

        public Item(int[] coords, Scene scene, string tag)
        {
            this.coords = coords;
            this.setTag(tag);
            this.uniValue = Item.getUniVal(tag);
            this.color = Item.getColor(tag);
        }
        public string getUniVal() { return uniValue; }

        public static Color getColor(string id)
        {
            switch (id)
            {
                case "fist": { return Playing.getColor("Sienna"); }
                case "food": { return Playing.getColor("Brown"); }
                case "sword": { return Playing.getColor("LightBlue"); }
                case "long sword": { return Playing.getColor("Blue"); }
                case "bow": { return Playing.getColor("Brown"); }
                case "life": { return Playing.getColor("Red"); }
                case "key": { return Playing.getColor("Yellow"); }
                case "copper tome": { return Playing.getColor("Sienna"); }
                case "malachite tome": { return Playing.getColor("Green"); }
                case "galvanized tome": { return Playing.getColor("Red"); }
                default: { return Playing.getColor("Purple"); }
            }
        }
        public static string getUniVal(string id)
        {
            switch (id)
            {
                case "fist": { return "\u2666"; }
                case "food": { return "\u2668"; }
                case "sword": { return "\u2628"; }
                case "long sword": { return "\u2627"; }
                case "bow": { return "\u2608"; }
                case "life": { return "\u2665"; }
                case "key": { return "\u2669"; }
                case "copper tome": { return "\u25C8"; }
                case "malachite tome": { return "\u25CD"; }
                case "galvanized tome": { return "\u2638"; }
                default: { return "\u2639"; }
            }
        }
        public static int getDamage(string id)
        {
            switch (id)
            {
                case "fist": { return 5; }
                case "sword": { return 10; }
                case "long sword": { return 8; }
                default: { return 0; }
            }
        }
        public static string getAttackSymbol(string id)
        {
            switch (id)
            {
                case "sword": { return "\u266E"; }
                case "long sword": { return "\u266F"; }
                case "bow": { return "\u2625"; }
                case "fist": { return "\u2666"; }
                case "copper tome": { return "\u25CC"; }
                case "malachite tome": { return "\u25CD"; }
                case "galvanized tome": { return "\u2638"; }
                default: { return "?"; }
            }
        }
    }
    class Enemy : Drawable
    {
        Scene scene;
        int dialogTimer = 0;
        int damageTimer = 0;
        int attackTimer = 0;
        int moveTimer = 0;
        int swingTimer = 0;

        public Color color;

        public bool speaking = false;
        public bool damaged = false;
        public bool dying;
        public bool attacking;
        public bool swinging;
        public bool aggressive;
        public bool moving = false;
        public bool hasHit = false;

        public int speed;
        public int health;
        public int eventId;
        public int[] direction = new int[] { 0, 1 };
        public int[] destination;
        public Stack<int[]> path;

        public string uniVal;
        public Queue<TextLine> dialog = new Queue<TextLine>();
        Playing playing;
        public string drop;

        Random rand = new Random(Guid.NewGuid().GetHashCode());

        public Enemy(int[] coords, Scene scene, int health, string drop, int eventId, string tag, Playing playing, bool aggressive, string uniVal, int speed)
        {
            //Debug.Print(uniVal);

            this.speed = speed;
            this.setTag(tag);
            this.coords = coords;
            this.scene = scene;
            this.playing = playing;
            this.color = Playing.getColor("White");
            this.health = health;
            this.uniVal = char.ConvertFromUtf32(Convert.ToInt32(uniVal));
            this.drop = drop;
            this.eventId = eventId;
            this.aggressive = aggressive;
            dying = false;
        }
        public void speak(TextLine dialog) { this.dialog.Enqueue(dialog); this.speaking = true; }
        public void speak(Queue<TextLine> dialog) { this.dialog = dialog; this.speaking = true; }
        public void update(GameTime gameTime)
        {
            //if (!moving) { moving = true; path = playing.AstarSearch(coords, new int[] { rand.Next(2,29),rand.Next(2,20) }); }
            if (moving && !damaged && !attacking && !swinging)
            {
                if (path.Count == 0) { moving = false; }
                else
                {
                    moveTimer += gameTime.ElapsedGameTime.Milliseconds;
                    if (moveTimer > speed && !scene.collides(new int[] { path.Peek()[0] - playing.currentCorner[0], path.Peek()[1] - playing.currentCorner[1] })) { moveTimer = 0; coords = path.Pop(); }
                }
            }
            if (this.speaking) { dialogTimer += gameTime.ElapsedGameTime.Milliseconds; }

            if (dialog.Count > 0 && dialogTimer > dialog.Peek().time) { dialog.Dequeue(); dialogTimer = 0; }
            if (dialog.Count == 0) { speaking = false; }
            if (this.damaged) { damageTimer += gameTime.ElapsedGameTime.Milliseconds; }
            if (damageTimer >= 180) { color = Playing.getColor("White"); damageTimer = 0; this.damaged = false; }

            //switch for different attack types
            switch (getTag())
            {
                case "sword":
                    {
                        if (swinging) { swingTimer += gameTime.ElapsedGameTime.Milliseconds; if (swingTimer > 350) { attacking = true; swinging = false; swingTimer = 0; } }
                        if (attacking) { attackTimer += gameTime.ElapsedGameTime.Milliseconds; if (attackTimer > 850) { attacking = false; attackTimer = 0; hasHit = false; } }
                        break;
                    }
                case "bow":
                    {
                        if (swinging && !attacking) { swingTimer += gameTime.ElapsedGameTime.Milliseconds; if (swingTimer > 350) { attacking = true; hasHit = false; } }
                        if (attacking) { attackTimer += gameTime.ElapsedGameTime.Milliseconds; if (attackTimer > 850) { swinging = false; swingTimer = 0; attacking = false; attackTimer = 0; } }
                        break;
                    }
                case "galvanized tome":
                    {
                        if (swinging && !attacking) { swingTimer += gameTime.ElapsedGameTime.Milliseconds; hasHit = false; if (swingTimer > 650) { attacking = true; } }
                        if (attacking) { attackTimer += gameTime.ElapsedGameTime.Milliseconds; if (attackTimer > 650) { swinging = false; swingTimer = 0; attacking = false; attackTimer = 0; } }
                        break;
                    }
            }
        }
        public TextLine getDialog()
        {
            return dialog.Peek();
        }

        public void attack(GameTime gameTime, int[] playerLocation)
        {
            if (!attacking)
            {   
                swinging = true;
                if (coords[1] - playerLocation[1] == 0) { direction = new int[] {(playerLocation[0] - coords[0]) / (Math.Abs(playerLocation[0] - coords[0])),0 }; }
                if (coords[0] - playerLocation[0] == 0) { direction = new int[] { 0, (playerLocation[1] - coords[1]) / (Math.Abs(playerLocation[1] - coords[1])) }; }
            }

        }
        public int[] getDirection(int[] location)
        {
            if (coords[1] - location[1] == 0) { return new int[] { (location[0] - coords[0]) / (Math.Abs(location[0] - coords[0])), 0 }; }
            if (coords[0] - location[0] == 0) { return new int[] { 0, (location[1] - coords[1]) / (Math.Abs(location[1] - coords[1])) }; }
            return new int[] { 0, 0 };
            
        }
        public void death(GameTime gameTime)
        {
            this.time += gameTime.ElapsedGameTime.Milliseconds;
            if (time > 200) { this.toBeDeleted = true; if (this.eventId > 0 && scene.events.Contains(scene.events.Find(a => a.id == this.eventId))) { scene.events.Find(a => a.id == this.eventId).trigger(); } }
            else
            {
                if (time > 40) { this.color = Playing.getColor("Red"); }
                if (time > 80) { this.color = Playing.getColor("White"); }
                if (time > 120) { this.color = Playing.getColor("Red"); }
                if (time > 160) { this.color = Playing.getColor("White"); }
            }
        }
        public void hit(int damage, int[] facing)
        {
            playing.hit.Play();
            this.damaged = true;
            this.color = Playing.getColor("Red");
            this.health -= damage;
            if (health <= 0) { this.dying = true; }
            //else if(!scene.collides(new int[] {coords[0] - playing.currentCorner[0] + facing[0], coords[1] - playing.currentCorner[1] + facing[1]}))
            //{
                //this.coords[0] += facing[0];
                //this.coords[1] += facing[1];
            //}
        }
        public void setDestination(int[] destination)
        {
            this.destination = destination;
            this.path = playing.AstarSearch(coords, destination);
            moving = true;
        }
    }

    class Drawable
    {
        private string tag;
        public double time = 0;
        public int[] coords;
        public int level;
        public double rotation;
        //use level for animated sprites

        public bool toBeDeleted = false;
        public bool escaped = false;

        public void setTag(string tag) { this.tag = tag; }
        public string getTag() { return this.tag; }
        public Rectangle spriteRectangle { get { return spriteRectangle; } set { spriteRectangle = value; } }
        public Rectangle locationRectangle { get { return locationRectangle; } set { locationRectangle = value; } }

        public bool collides(Drawable other)
        {
            return this.locationRectangle.Intersects(other.locationRectangle);
        }
        public bool collides(Rectangle other)
        {
            return this.locationRectangle.Intersects(other);
        }
    }

    public class PrioQueue
    {
        int total_size;
        SortedDictionary<int, Queue> storage;

        public PrioQueue()
        {
            this.storage = new SortedDictionary<int, Queue>();
            this.total_size = 0;
        }

        public int Size() { return total_size; }

        public bool IsEmpty()
        {
            return (total_size == 0);
        }

        public object Dequeue()
        {
            if (IsEmpty())
            {
                throw new Exception("Please check that priorityQueue is not empty before dequeing");
            }
            else
                foreach (Queue q in storage.Values)
                {
                    // we use a sorted dictionary
                    if (q.Count > 0)
                    {
                        total_size--;
                        return q.Dequeue();
                    }
                }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return null; // not supposed to reach here.
        }

        // same as above, except for peek.

        public object Peek()
        {
            if (IsEmpty())
                throw new Exception("Please check that priorityQueue is not empty before peeking");
            else
                foreach (Queue q in storage.Values)
                {
                    if (q.Count > 0)
                        return q.Peek();
                }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return null; // not supposed to reach here.
        }

        public object Dequeue(int prio)
        {
            total_size--;
            return storage[prio].Dequeue();
        }

        public void Enqueue(object item, int prio)
        {
            if (!storage.ContainsKey(prio))
            {
                storage.Add(prio, new Queue());
            }
            storage[prio].Enqueue(item);
            total_size++;

        }
    }
}
