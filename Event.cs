using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RogueLikeGame
{
    class Event
    {
        Scene scene;
        Playing playing;
        public int id;
        public string tag;
        public string command;
        TextBox textBox;
        Queue<TextLine> lines = new Queue<TextLine>();

        public Event(Scene scene, Playing playing, TextBox textBox, int id, string tag, string command) 
        {
            this.scene = scene;
            this.playing = playing;
            this.textBox = textBox;
            this.id = id;
            this.tag = tag;
            this.command = command;
        }
        public void addLine(TextLine newLine) { lines.Enqueue(newLine); }
        public void trigger()
        {
            switch (command.Split(' ')[0])
            {
                case "clear": { textBox.setLines(lines); scene.events.Remove(this); break; }
                case "speak":
                    {
                        Enemy enemy = playing.getEnemy(command.Split(' ')[1]);
                        if (!textBox.writing && enemy != null)
                        {
                            textBox.setLines(lines);
                            foreach (TextLine line in lines)
                            {
                                if (line.type == "dialog" ) { enemy.speak(line); }
                            }
                            scene.events.Remove(this);
                        }
                        break;
                    }
                case "move":
                    {
                        Enemy enemy = playing.getEnemy(command.Split(' ')[1]);
                        enemy.setDestination(new int[] { Convert.ToInt32(command.Split(' ')[2]), Convert.ToInt32(command.Split(' ')[3]) });
                        textBox.addLines(lines);
                        scene.events.Remove(this);
                        break;
                    }
                case "next": 
                    { 
                        scene.events.Clear();
                        textBox.addLines(lines);
                        scene.processXML(command.Split(' ')[1], Convert.ToInt16(command.Split(' ')[2]));
                        textBox.waitOutput.Clear();
                        break; 
                    }
                case "transition":
                    {
                        playing.changeState("Transition");
                        playing.transition.process(Convert.ToInt16(command.Split(' ')[1]));
                        break;
                    }
                default: { if (!textBox.writing) { textBox.setLines(lines); scene.events.Remove(this); } break; }
            }
        }
    }
}
