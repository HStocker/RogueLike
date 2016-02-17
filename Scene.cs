using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Xml;

namespace RogueLikeGame
{
    class Scene
    {
        int[] dimensions;
        Constructor constructor;
        Playing playing;
        public List<Event> events = new List<Event>();
        TextBox textBox;
        int[,] triggerArray;
        public string currentArea;
        int level = 1;

        public Scene(Playing playing, TextBox textBox)
        {

            this.textBox = textBox;
            this.playing = playing;
            this.constructor = new Constructor(string.Format("Level{0}.csv", level));
            dimensions = constructor.getDimensions();


            string[] ioArray = File.ReadAllText(string.Format("Content//Level{0}Event.csv", level)).Split('\n');
            triggerArray = new int[(ioArray[0].Length + 1) / 2, ioArray.GetLength(0)];
            for (int i = 0; i < ioArray.Length; i++)
            {
                string[] lineSplit = ioArray[i].Split(',');
                for (int j = 0; j < lineSplit.Length; j++)
                {
                    triggerArray[j, i] = Convert.ToInt16(lineSplit[j]);
                }
            }
            processXML("jail", level);

        }

        public void processXML(string area, int levelNum)
        {
            this.currentArea = area;
            XmlDocument xmlReader = new XmlDocument();
            xmlReader.Load("Content/eventText.xml");
            XmlNode levels = xmlReader.ChildNodes[1].ChildNodes[levelNum - 1];
            string[] colors = new string[levels.Attributes.Count - 1];
            for (int i = 1; i < levels.Attributes.Count; i++) { colors[i - 1] = levels.Attributes[i].Value; }
            playing.setColors(colors);
            XmlNode level = levels.ChildNodes[0];
            foreach (XmlNode node in levels) { if (node.Attributes[0].Value == area) { level = node; break; } }
            foreach (XmlNode node in level)
            {
                if (node.Name.Equals("initial"))
                {
                    foreach (XmlNode initial in node.ChildNodes)
                    {
                        switch (initial.Name)
                        {
                            case "text": { textBox.addLine(initial.InnerText, Convert.ToInt16(initial.Attributes[0].Value), initial.Attributes[1].Value, initial.Attributes[2].Value); break; }
                            case "enemy": { bool aggro = initial.Attributes[6].Value == "true"; playing.addEnemy(new Enemy(new int[] { Convert.ToInt16(initial.Attributes[0].Value), Convert.ToInt16(initial.Attributes[1].Value) }, this, Convert.ToInt16(initial.Attributes[2].Value), initial.Attributes[3].Value, Convert.ToInt16(initial.Attributes[4].Value), initial.Attributes[5].Value, playing, aggro, initial.Attributes[7].Value, Convert.ToInt16(initial.Attributes[8].Value))); break; }
                            case "item": { playing.addItem(new Item(new int[] { Convert.ToInt16(initial.Attributes[0].Value), Convert.ToInt16(initial.Attributes[1].Value) }, this, initial.Attributes[2].Value)); break; }
                            case "box": { playing.addBox(new Box(new int[] { Convert.ToInt16(initial.Attributes[0].Value), Convert.ToInt16(initial.Attributes[1].Value) }, this, initial.Attributes[2].Value, initial.Attributes[3].Value, Convert.ToInt16(initial.Attributes[4].Value))); break; }
                            case "static": { playing.addStatic(new StaticObject(new int[] { Convert.ToInt16(initial.Attributes[0].Value), Convert.ToInt16(initial.Attributes[1].Value) }, initial.Attributes[2].Value, initial.Attributes[3].Value, initial.Attributes[4].Value, initial.Attributes[5].Value, Convert.ToInt16(initial.Attributes[6].Value), initial.Attributes[7].Value, playing, new int[] { Convert.ToInt32(initial.Attributes[8].Value), Convert.ToInt32(initial.Attributes[9].Value) }, initial.Attributes[10].Value == "true", initial.Attributes[11].Value == "true", Convert.ToInt16(initial.Attributes[12].Value))); break; }
                        }
                    }
                }
                if (node.Name == "timed")
                {
                    foreach (XmlNode timed in node.ChildNodes)
                    {
                        textBox.addWaitLine(timed.InnerText, Convert.ToInt32(timed.Attributes[0].Value), timed.Attributes[1].Value, timed.Attributes[2].Value);
                    }
                }
                if (node.Name == "event")
                {
                    Event newEvent = new Event(this, playing, textBox, Convert.ToInt16(node.Attributes[0].Value), node.Attributes[1].Value, node.Attributes[2].Value);
                    events.Add(newEvent);
                    foreach (XmlNode eventNode in node.ChildNodes)
                    {
                        newEvent.addLine(new TextLine(eventNode.InnerText, Convert.ToInt32(eventNode.Attributes[0].Value), eventNode.Attributes[1].Value, eventNode.Attributes[2].Value));
                    }
                }
                //Debug.Print(node.Attributes);
            }
        }

        public Tile[,] getArray() { return constructor.getTileArray(); }
        public int[] getDimensions() { return dimensions; }

        public bool collides(int[] coordinates)
        {
            if (playing.enemies.Find(a => a.coords[0] == coordinates[0] + playing.currentCorner[0] && a.coords[1] == coordinates[1] + playing.currentCorner[1]) != null) { return true; }
            if (playing.staticObjects.Find(a => a.coords[0] == coordinates[0] + playing.currentCorner[0] && a.coords[1] == coordinates[1] + playing.currentCorner[1]) != null)
            {
                if (playing.staticObjects.Find(a => a.coords[0] == coordinates[0] + playing.currentCorner[0] && a.coords[1] == coordinates[1] + playing.currentCorner[1]).collision) { return true; }
            }
            if (playing.boxes.Find(a => a.coords[0] == coordinates[0] + playing.currentCorner[0] && a.coords[1] == coordinates[1] + playing.currentCorner[1]) != null) { return true; }
            if (coordinates[0] == playing.player.coords[0] && coordinates[1] == playing.player.coords[1]) { return true; }

            if (constructor.getTile(new int[] { coordinates[0] + playing.currentCorner[0], coordinates[1] + playing.currentCorner[1] }).getNum() == 0
                || constructor.getTile(new int[] { coordinates[0] + playing.currentCorner[0], coordinates[1] + playing.currentCorner[1] }).getNum() == 5) { return false; }
            return true;
        }
        public bool collides(int x, int y) { if (constructor.getTile(new int[] { x + playing.currentCorner[0], y + playing.currentCorner[1] }).getNum() != 0) { return true; } return false; }
        public void breakObject(int[] coordinates)
        {
            List<StaticObject> objects = playing.staticObjects.FindAll(a => a.coords[0] == coordinates[0] + playing.currentCorner[0] && a.coords[1] == coordinates[1] + playing.currentCorner[1]);
            foreach (StaticObject @object in objects) { if (@object.destructible) { @object.destroy(); } }
            Box box = playing.boxes.Find(a => a.coords[0] == coordinates[0] + playing.currentCorner[0] && a.coords[1] == coordinates[1] + playing.currentCorner[1]);
            if (box != null)
            {
                //add destroyBox function;
                //public void destroyBox(Box box){}
                box.toBeDeleted = true;
                playing.items.Add(new Item(box.coords, this, box.drop));
                if (box.eventId > 0) { this.events.Find(a => a.id == box.eventId).trigger(); }
                playing.addParticle(new Particle(box.coords, new int[] { 2, 1 }, 160, "stars", Playing.getColor("White")));
                playing.addParticle(new Particle(box.coords, new int[] { -1, -1 }, 160, "stars", Playing.getColor("White")));
                playing.addParticle(new Particle(box.coords, new int[] { 1, 0 }, 160, "stars", Playing.getColor("White")));
                playing.addParticle(new Particle(box.coords, new int[] { 0, 0 }, 160, "stars", Playing.getColor("White")));
            }
        }
        public bool collidesPlayer(int[] coordinates)
        { return coordinates[0] == playing.player.coords[0] && coordinates[1] == playing.player.coords[1]; }

        public bool collidesEnemy(int[] coordinates)
        { return playing.enemies.Find(a => a.coords[0] == coordinates[0] && a.coords[1] == coordinates[1]) != null; }

        //ONLY FOR A* PATHFINDING
        public bool collidesAbsolute(int[] coordinates)
        {
            if (constructor.getTile(new int[] { coordinates[0], coordinates[1] }).getNum() != 0) { return true; }
            //if (playing.enemies.Find(a => a.coords[0] == coordinates[0]&& a.coords[1] == coordinates[1]) != null) { return true; }
            if (playing.staticObjects.Find(a => a.coords[0] == coordinates[0] && a.coords[1] == coordinates[1]) != null) { return true; }
            if (playing.boxes.Find(a => a.coords[0] == coordinates[0] && a.coords[1] == coordinates[1]) != null) { return true; }
            //if (coordinates[0] == playing.player.coords[0] && coordinates[1] == playing.player.coords[1]) { return true; }
            return false;
        }
        public bool collidesAbsolute(int x, int y) { if (constructor.getTile(new int[] { x, y }).getNum() != 0) { return true; } return false; }

        public bool isSolid(int[] coordinates)
        {
            if (constructor.getTile(new int[] { coordinates[0] + playing.currentCorner[0], coordinates[1] + playing.currentCorner[1] }).getNum() == 1
                || constructor.getTile(new int[] {coordinates[0] + playing.currentCorner[0], coordinates[1] + playing.currentCorner[1]}).getNum() == 4) { return true; }
            if (playing.staticObjects.Find(a => a.coords[0] == coordinates[0] + playing.currentCorner[0] && a.coords[1] == coordinates[1] + playing.currentCorner[1]) != null)
            { return playing.staticObjects.Find(a => a.coords[0] == coordinates[0] + playing.currentCorner[0] && a.coords[1] == coordinates[1] + playing.currentCorner[1]).solid; }
            if (!constructor.includesTile(new int[] { coordinates[0] + playing.currentCorner[0], coordinates[1] + playing.currentCorner[1] })) { return true; }
            return false;
        }
        public bool isSolid(int x, int y) { if (constructor.getTile(new int[] { x + playing.currentCorner[0], y + playing.currentCorner[1] }).getNum() == 1) { return true; } return false; }
        public void setCollision(int x, int y, int coll) { constructor.getTile(new int[] { x, y }).setNum(coll); }

        public bool trigger(int[] coordinates) { return events.Exists(a => a.id == triggerArray[coordinates[0], coordinates[1]]); }
        public Event getEvent(int[] coordinates) { return events.Find(a => a.id == triggerArray[coordinates[0], coordinates[1]]); }

        public bool includesTile(int[] coordinates) { return constructor.includesTile(coordinates); }
        public Tile getTile(int[] coordinates) { return constructor.getTile(coordinates); }
        public Tile getTile(int x, int y) { return constructor.getTile(x, y); }
    }


    class Constructor
    {
        Tile[,] tileArray;
        string currentLevel;
        string[] tileTypes = { "floor", "wall", "hatch", "window", "wall","floor" };

        public Constructor(string currentLevel)
        {
            this.currentLevel = currentLevel;
            this.build();
        }

        public void build()
        {
            string[] ioArray = File.ReadAllText(string.Format("Content//{0}", this.currentLevel)).Split('\n');
            tileArray = new Tile[(ioArray[0].Length + 1) / 2, ioArray.GetLength(0)];
            for (int i = 0; i < ioArray.Length; i++)
            {
                string[] lineSplit = ioArray[i].Split(',');
                for (int j = 0; j < lineSplit.Length; j++)
                {
                    int tileNumber = Convert.ToInt16(lineSplit[j]);
                    tileArray[j, i] = new Tile(tileNumber, tileTypes[tileNumber], j, i);
                    tileArray[j, i].setTag(tileTypes[tileNumber]);
                }
            }
        }

        public int[] getDimensions()
        {
            int[] temp = new int[2];
            temp[0] = this.tileArray.GetLength(0);
            temp[1] = this.tileArray.GetLength(1);
            return temp;
        }

        public bool includesTile(int x, int y)
        {
            //Debug.Print("x:"+Convert.ToString(x)+"\ny:"+Convert.ToString(y)+"\ndim0:"+Convert.ToString(tileArray.GetLength(0))+"\ndim1:"+Convert.ToString(tileArray.GetLength(1)));
            if (x < 0 || x >= this.tileArray.GetLength(0)) { return false; }
            if (y < 0 || y >= this.tileArray.GetLength(1)) { return false; }
            return true;
        }
        public bool includesTile(int[] coords)
        {
            int x = coords[0];
            int y = coords[1];
            //Debug.Print("x:"+Convert.ToString(x)+"\ny:"+Convert.ToString(y)+"\ndim0:"+Convert.ToString(tileArray.GetLength(0))+"\ndim1:"+Convert.ToString(tileArray.GetLength(1)));
            if (x < 0 || x >= this.tileArray.GetLength(0)) { return false; }
            if (y < 0 || y >= this.tileArray.GetLength(1)) { return false; }
            return true;
        }
        public Tile[,] getTileArray() { return this.tileArray; }
        public Tile getTile(int x, int y) { return tileArray[x, y]; }
        public Tile getTile(int[] coordinates) { return tileArray[coordinates[0], coordinates[1]]; }

    }

    class Tile
    {
        string tileType;
        int tileNum;
        public bool solid;
        private int[] coords;
        string tag;

        public Tile(int tileNum, string tileType, int x, int y)
        {
            this.tileNum = tileNum;
            this.tileType = tileType;
            this.coords = new int[] { x, y };
        }
        public int[] getCoords() { return coords; }
        public int getNum() { return this.tileNum; }
        public void setNum(int num) { this.tileNum = num; }
        public string getType() { return this.tileType; }
        public void setTag(string tag) { this.tag = tag; }
        public string getTag() { return this.tag; }
        public int distance(Tile inTile) { return Math.Abs(this.coords[0] - inTile.coords[0]) + Math.Abs(this.coords[1] - inTile.coords[1]); }
    }
}
