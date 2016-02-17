using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace RogueLikeGame
{
    

    class UI : Drawable
    {
        int maxHealth = 10;
        Player player;
        // empty = 2661
        //full = 2665
        public UI(Player player) 
        {
            this.player = player;
        }
        public string getHealth() { return String.Concat(Enumerable.Repeat("\u2665", Math.Abs(player.health))); }
        public string getMissingHealth() { return String.Concat(Enumerable.Repeat("\u2665", maxHealth - player.health)); }
        public int getMaxHealth() { return maxHealth; }

    }
    class TextBox : Drawable
    {
        int textTimer = 1500;
        int waitTimer = 0;
        public Queue<TextLine> waitOutput = new Queue<TextLine>();
        public Queue<TextLine> output = new Queue<TextLine>();
        public Queue<TextLine> text = new Queue<TextLine>();
        Scene scene;
        public bool writing = false;

        public TextBox(Scene scene) 
        {
            this.scene = scene;
        }
        public void addLine(string line, int timer, string color, string type) { output.Enqueue(new TextLine(line,timer,color, type)); }
        public void addLines(Queue<TextLine> lines) { foreach (TextLine line in lines.ToArray()) { output.Enqueue(line); }; }
        public void setLines(Queue<TextLine> lines) { foreach (TextLine line in output.ToArray()) { text.Enqueue(line); } output = lines; }

        public void addWaitLine(string line, int timer, string color, string type) { waitOutput.Enqueue(new TextLine(line, timer, color, type)); }
        public void addLine(TextLine line) { output.Enqueue(line); }
        public void addWaitLine(TextLine line) { waitOutput.Enqueue(line); }
        public void clear() { text.Clear(); output.Clear(); waitOutput.Clear(); }
        public void update(GameTime gameTime)
        {
            textTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (output.Count > 0 && textTimer > output.Peek().time) { writing = true; text.Enqueue(output.Dequeue()); textTimer = 0; }
            else 
            {
                if (output.Count == 0) { writing = false; textTimer = 2000; }
                waitTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (waitOutput.Count > 0 && waitTimer >= waitOutput.Peek().time) { text.Enqueue(waitOutput.Dequeue()); waitTimer = 0; }
            }
        }
        public Queue<TextLine> getText()
        {
            if (text.Count > 12) { text.Dequeue(); }
            return text;
        }
    }
    class TextLine
    {
        public string text;
        public int time;
        public string color;
        public string type;

        public TextLine(string text, int time, string color, string type)
        {
            this.text = text;
            this.time = time;
            this.color = color;
            this.type = type;
        }
    }
}
