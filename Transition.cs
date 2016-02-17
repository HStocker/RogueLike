using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace RogueLikeGame
{
    class Transition : GameState
    {
        Playing playing;
        Scene scene;
        Queue<transitionText> textQueue = new Queue<transitionText>();
        int textTimer = 0;
        List<transitionText> liveText = new List<transitionText>();
        SpriteBatch spriteBatch;
        SpriteFont symbols;
        Texture2D bottomFade;
        Texture2D topFade;

        public Transition(Playing playing, Scene scene, SpriteBatch spriteBatch, SpriteFont symbols,Texture2D topFade, Texture2D bottomFade)
        {
            this.bottomFade = bottomFade;
            this.topFade = topFade;
            this.symbols = symbols;
            this.spriteBatch = spriteBatch;
            this.playing = playing;
            this.scene = scene;
        }

        public string getTag()
        {
            return "Transition";
        }

        public void process(int level)
        {
            XmlDocument xmlReader = new XmlDocument();
            xmlReader.Load("Content/Transition.xml");
            XmlNode text = xmlReader.ChildNodes[1].ChildNodes[level - 1];
            foreach (XmlNode node in text.ChildNodes)
            {
                switch (node.Name)
                {
                    case "text": 
                        {
                            Debug.Print(node.InnerText);
                            textQueue.Enqueue(new transitionText(node.InnerText,new int[] {Convert.ToInt16(node.Attributes[0].Value),Convert.ToInt16(node.Attributes[1].Value)},Convert.ToInt16(node.Attributes[2].Value),Playing.getColor(node.Attributes[3].Value)));
                            break; 
                        }
                    case "tileArray": 
                        {
 
                            break; 
                        }
                }
            }
        }

        public void update(GameTime gameTime)
        {
            textTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (textQueue.Count > 0 && textTimer > textQueue.Peek().timer) { liveText.Add(textQueue.Dequeue()); textTimer = 0; }
            foreach (transitionText text in liveText) { text.coords[1] -= 1; }
        }

        public void draw()
        {
            foreach (transitionText line in liveText) { spriteBatch.DrawString(symbols, line.text, new Vector2(line.coords[0], line.coords[1]), line.color); }
            spriteBatch.Draw(topFade, new Rectangle(0, -60, 1380, 880), Color.White);
            spriteBatch.Draw(bottomFade, new Rectangle(0, 60, 1380, 880), Color.White);
        }

        public void entering()
        {
        }

        public void leaving()
        {
        }
    }
    class transitionText
    {
        public string text;
        public int[] coords;
        public int timer;
        public Color color;
        public transitionText(string text, int[] coords, int timer, Color color) 
        {
            this.text = text;
            this.coords = coords;
            this.timer = timer;
            this.color = color;
            
        }
    }
}
