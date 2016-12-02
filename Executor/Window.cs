using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Executor
{
    public delegate void FCN();

    public partial class Window : Form
    {
        public VirtualScreen vs;
        public Player p;
        public Map m;
        public bool initialized;

        float MULTI_SIZE_X = 800 / 8;
        float MULTI_SIZE_Y = 600 / 8;
        int UPDATE_CMDS_INTERVAL = 12;

        string[] lines_map;
        string[] cLines;
        int cLine = 0;
        int cFrame = 0;


        bool wait;
        Dictionary<string, int> LabelList;


        string m_Data = @"E:\VisualStudios\VirtualExecution\Executor\bin\Debug\data\";
        Image cPlayerLeftImage;
        Image cPlayerRightImage;
        Image cBombImage;
        Image cWallStoneImage;
        Image cFloorStoneImage;



        public Window()
        {
            InitializeComponent();
            Log.CreateConsole();

            m = new Map(this);
            p = new Player(this);
            vs = new VirtualScreen(Handle, Width - 15, Height - 15, 25);
            LabelList = new Dictionary<string, int>();


            cPlayerLeftImage        = Image.FromFile(m_Data + "player_left.png");
            cPlayerRightImage        = Image.FromFile(m_Data + "player_right.png");
            //cBombImage          = Image.FromFile(m_Data + "player.png");
            //cWallStoneImage     = Image.FromFile(m_Data + "player.png");
            //cFloorStoneImage    = Image.FromFile(m_Data + "player.png");
            cLines              = File.ReadAllLines(m_Data + "map01_cmd.txt");
            lines_map           = File.ReadAllLines(m_Data + "map01.txt");



            VirtualScreenEventHandler.UpdateFrame += VirtualScreenEventHandler_UpdateFrame;
            vs.RunRendererThread();


            foreach (string line in lines_map)
            {
                // commented line
                if (line.Length <= 0 || line[0] == 0 || line[0] == '/' || line[0] == ' ')
                    continue;

                Log.Info("code", line);

                string[] cmds = line.Split(' ');

                if (cmds[0] == "setPosition")
                {
                    p.SetPosition(int.Parse(cmds[1]), int.Parse(cmds[2]));
                    continue;
                }


                // Spawn a Entity
                if (cmds.Length < 3)
                    throw new Exception("fuck you need 3 params for that shit");
                
                m.AddEntity(int.Parse(cmds[0]), int.Parse(cmds[1]), int.Parse(cmds[2]));
            }

            for(int i = 0; i < cLines.Length; i++)
            {
                string[] line = cLines[i].Split(' ');
                if (line.Length >= 2)
                    if (line[0] == "LABEL")
                        LabelList.Add(line[1], i);
            }

            initialized = true;
        }
        public void ProceedCMDS()
        {
            if (cLines.Length <= cLine)
                return;

            string[] line = cLines[cLine].Split(' ');
            Log.Info("code", cLines[cLine]);

            if (wait)
                wait = false;

            

            if (line[0] == StringCommands.IF)
            {
                bool nextIsBomb = false;
                int index = -1; 
                if (line[1] == StringCommands.CHECKBOMB)
                {
                    index = 0; // set index to 1 so it knowa its a bomb for 1  -1 is none
                    //if (p.CheckBomb())
                    //    nextIsBomb = true;
                }

                if(index != -1)
                {
                    if (index == 0)
                    {
                        if (nextIsBomb)
                        {
                            if (line[2] == "goto")
                            {
                                try
                                {
                                    cLine = LabelList[line[3]] - 1;
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            if(line.Length == 6)
                            {
                                if (line[4] == "else")
                                {
                                    if (line[5] == "goto")
                                    {
                                        try
                                        {
                                            cLine = LabelList[line[6]] - 1;
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (line[0] == StringCommands.WAIT)
            {
                wait = true;
                cFrame -= UPDATE_CMDS_INTERVAL;
            }
            if (line[0] == StringCommands.TURN_LEFT)
                p.turn(-1);

            if (line[0] == StringCommands.TURN_RIGHT)
                p.turn(1);

            if (line[0] == StringCommands.MOVE_FORWARD)
                p.move_forward();
            if (line[0] == "return")
                cLine = cLines.Length;

            cLine++;

            if (p.Health <= 0)
                Log.Error("DEAD", "");
        }
        private void VirtualScreenEventHandler_UpdateFrame(Object sender, VirtualScreen e)
        {
            if (!initialized)
                return;

            // Process the Player Commands every 'UPDATE_CMDS_INTERVAL' Frame.
            if (cFrame % UPDATE_CMDS_INTERVAL == 1)
                ProceedCMDS();

            // Update the Command Frame.
            cFrame++;


            // draw the map
            // map items are 8 * 8 so fiels are 8 bytes..
            foreach (Entity et in m.GetAllEntity())
            {
                if (et.item == 1)
                    e.UI_DrawFilledRectangle(new Vector2(MULTI_SIZE_X * et.x, MULTI_SIZE_Y * et.y), new Vector2(MULTI_SIZE_X, MULTI_SIZE_Y), Brushes.White);
                
                // fuck my life its a bomb !!!!
                if (et.item == 2)
                    e.UI_DrawFilledRectangle(new Vector2(MULTI_SIZE_X * et.x, MULTI_SIZE_Y * et.y), new Vector2(MULTI_SIZE_X, MULTI_SIZE_Y), Brushes.Red);
            }


            // Draw Player
            //e.UI_DrawFilledRectangle(new Vector2(MULTI_SIZE_X * p.x, MULTI_SIZE_Y * p.y), new Vector2(MULTI_SIZE_X, MULTI_SIZE_Y), Brushes.Green);
            if (p.r == 0 || p.r == 1)
                e.UI_DrawImage(new GfxImage(cPlayerRightImage), new Vector2(MULTI_SIZE_X * p.x, MULTI_SIZE_Y * p.y), new Vector2(MULTI_SIZE_X, MULTI_SIZE_Y));

            if (p.r == 2 || p.r == 3)
                e.UI_DrawImage(new GfxImage(cPlayerLeftImage), new Vector2(MULTI_SIZE_X * p.x, MULTI_SIZE_Y * p.y), new Vector2(MULTI_SIZE_X, MULTI_SIZE_Y));





            // debug
            string debugInfo =  hString.va("cFrame = %i (%i)\n", cFrame, cFrame % UPDATE_CMDS_INTERVAL);
            debugInfo += hString.va("p.r = %i\n", p.r);
            debugInfo += hString.va("p.x = %i\n", p.x);
            debugInfo += hString.va("p.y = %i\n", p.y);
            debugInfo += hString.va("p.isEntityForwad = %b\n", p.isEntityForwad);
            debugInfo += hString.va("p.Health = %i\n", p.Health);
            
            e.UI_DrawText(debugInfo, TextAlignment_t.TOP_RIGHT, new Font("Arial", 12), Brushes.Cyan);


            // Inverted again -.-
            Vector2 p_m_pos = new Vector2(MULTI_SIZE_X * p.x + MULTI_SIZE_X / 2, MULTI_SIZE_Y * p.y + MULTI_SIZE_Y / 2);

            Vector2 p_up_pos = new Vector2(p_m_pos.x, p_m_pos.y + MULTI_SIZE_Y);
            Vector2 p_right_pos = new Vector2(p_m_pos.x + MULTI_SIZE_X, p_m_pos.y);
            Vector2 p_down_pos = new Vector2(p_m_pos.x, p_m_pos.y - MULTI_SIZE_Y);
            Vector2 p_left_pos = new Vector2(p_m_pos.x - MULTI_SIZE_X, p_m_pos.y);

            if (p.r == 0)
                e.UI_DrawLine(p_m_pos, p_down_pos, Pens.Red);

            if (p.r == 1)
                e.UI_DrawLine(p_m_pos, p_right_pos, Pens.Red);

            if (p.r == 2)
                e.UI_DrawLine(p_m_pos, p_up_pos, Pens.Red);

            if (p.r == 3)
                e.UI_DrawLine(p_m_pos, p_left_pos, Pens.Red);
        }
        private void Window_FormClosing(Object sender, FormClosingEventArgs e)
        {
            vs.StopRendererThread();
        }
    }
    public class StringCommands
    {
        public static string WAIT = "Wait";
        public static string TURN_LEFT = "TurnLeft";
        public static string TURN_RIGHT = "TurnRight";
        public static string MOVE_FORWARD = "MoveForward";

        public static string CHECKBOMB = "CheckBomb()";
        public static string IF = "if";
    }
    public class Entity
    {
        public int x, y;
        public int item;
        public bool canMoveOver;
        public FCN OnTriggerEnter;
        public FCN Execute;
    }
    public class Player
    {
        Window code;

        public int x, y;

        // 0 = up
        // 1 = right
        // 2 = down
        // 3 = left
        public int r;

        public bool isEntityForwad;
        public float Health;
        public float MaxHealth = 100;





        public Player(Window w)
        {
            code = w;
            Health = MaxHealth;
        }

        // direction ( -1 = left, 1 = right)
        public void turn(int direction)
        {
            if (Health <= 0)
                return;

            // left
            if (direction == -1)
            {
                if (r == 0)
                    r = 3;

                else if (r == 1)
                    r = 0;

                else if (r == 2)
                    r = 1;

                else if (r == 3)
                    r = 2;
            }

            // right
            if (direction == 1)
            {
                if (r == 0)
                    r = 1;

                else if (r == 1)
                    r = 2;

                else if (r == 2)
                    r = 3;

                else if (r == 3)
                    r = 0;
            }
            UPDATE_ENTITY_CHECK();
        }
        public void move_forward()
        {
            if (Health <= 0)
                return;

            int n_x = x;
            int n_y = y;

            // -- for up say thanks to windows forms -.-
            if (r == 0)
                n_y--;

            if (r == 1)
                n_x++;

            if (r == 2)
                n_y++;

            if (r == 3)
                n_x--;


            // Check if a Entity Blocks the way.
            // if there is a entity 
            Entity e = code.m.GetEntityAtPosition(n_x, n_y);
            if (e == null || e.canMoveOver)
            {
                x = n_x;
                y = n_y;

                if (e != null && e.OnTriggerEnter != null)
                    e.OnTriggerEnter();
            }
            else
                Log.Error("move_forward()", "Could not move forward Entity is Blocking");

            UPDATE_ENTITY_CHECK();
        }
        Entity getEntityForward()
        {
            int n_x = x;
            int n_y = y;

            // -- for up say thanks to windows forms -.-
            if (r == 0)
                n_y--;

            if (r == 1)
                n_x++;

            if (r == 2)
                n_y++;

            if (r == 3)
                n_x--;

            return code.m.GetEntityAtPosition(n_x, n_y);
        }
        public void UPDATE_ENTITY_CHECK()
        {
            if (getEntityForward() == null/* || e.canMoveOver*/)
                isEntityForwad = false;
            else
                isEntityForwad = true;
        }
        public void SetPosition(int x, int y)
        {
            this.x = x;
            this.y = y;

            UPDATE_ENTITY_CHECK();
        }


        // Health management
        public void Suicide()
        {
            Health = 0;
        }
        public void SetHealth(float Health)
        {
            this.Health = Health;
        }
        public void Revive()
        {
            Health = MaxHealth;
        }
        
        // others
        public bool CheckBomb()
        {
            if(getEntityForward().item == 2)
                return true;
            return false;
        }
    }
    public class Map
    {
        Window code;
        List<Entity> mapEntity;

        public Map(Window main)
        {
            code = main;
            mapEntity = new List<Entity>();
        }
        public Entity AddEntity(int x, int y, int item)
        {
            Entity t;
            t = new Entity();
            t.x = x;
            t.y = y;
            t.item = item;

            if (item == 2)
            {
                t.OnTriggerEnter = code.p.Suicide;
                t.canMoveOver = true;
            }

            mapEntity.Add(t);

            return t;
        }
        public Entity[] GetAllEntity()
        {
            return mapEntity.ToArray();
        }
        public Entity GetEntityAtPosition(int x, int y)
        {
            foreach(Entity e in mapEntity)
                if (e.x == x && e.y == y)
                    return e;
            return null;
        }
    }
}