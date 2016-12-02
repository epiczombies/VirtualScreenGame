using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;


public delegate void VirtualScreenUpdateFrameEventHandler(object sender, VirtualScreen e);
public delegate void VirtualScreenEndFrameEventHandler(object sender, VirtualScreen e);
public delegate void VirtualScreenBeginUpdateFrameEventHandler(object sender, VirtualScreen e);
public class VirtualScreenEventHandler
{
    public static event VirtualScreenUpdateFrameEventHandler UpdateFrame;
    public static event VirtualScreenEndFrameEventHandler EndFrame;
    public static event VirtualScreenBeginUpdateFrameEventHandler BeginUpdateFrame;
    public static event VirtualScreenBeginUpdateFrameEventHandler StopScreen;

    // Before Clearing the Virtual Screen
    public static void onBeginUpdateFrame(object sender, VirtualScreen e)
    {
        if (BeginUpdateFrame != null)
            BeginUpdateFrame(sender, e);
    }

    // After Clearing the Virtual Screen
    public static void onUpdateFrame(object sender, VirtualScreen e)
    {
        if (UpdateFrame != null)
            UpdateFrame(sender, e);
    }

    // After Rendering the Virtual Screen
    public static void onEndFrame(object sender, VirtualScreen e)
    {
        if (EndFrame != null)
            EndFrame(sender, e);
    }

    // Stop the Virtual Screen
    public static void onStopScreen(object sender, VirtualScreen e)
    {
        if (StopScreen != null)
            StopScreen(sender, e);
    }
}
public class GfxImage
{
    Image m_Image;
    Graphics m_Graphics;

    public GfxImage()
    {
        m_Image = new Bitmap(0, 0); // isnt it 1,1?
        m_Graphics = Graphics.FromImage(m_Image);
    }
    public GfxImage(Vector2 Size)
    {
        m_Image = new Bitmap((int)Size.x, (int)Size.y);
        m_Graphics = Graphics.FromImage(m_Image);
    }
    public GfxImage(Image image)
    {
        m_Image = image;
        m_Graphics = Graphics.FromImage(m_Image);
    }
    public GfxImage(string path)
    {
        m_Image = Image.FromFile(path);
        m_Graphics = Graphics.FromImage(m_Image);
    }


    public Image GetImage()
    {
        m_Graphics.DrawImage(m_Image, 0, 0);
        m_Graphics.Flush();

        return m_Image;
    }
    public Graphics GetGraphics()
    {
        return Graphics.FromImage(m_Image);
    }
}
public struct VirtualScreenInfo
{
    // Screen to Draw
    internal Bitmap Screen;

    // Device to draw the Image
    public IntPtr Device;

    // Screen Width
    public int Width;

    // Screen Height
    public int Height;

    // try FPS Rate
    public int RefreshRate;

    // while isDrawing we Update
    public bool isDrawing;
}
public enum TextAlignment_t
{
    TOP_LEFT,
    TOP_RIGHT,
    TOP_CENTER,

    CENTER_LEFT,
    CENTER_RIGHT,
    CENTER_CENTER,

    BOTTOM_LEFT,
    BOTTOM_RIGHT,
    BOTTOM_CENTER
};
public class VirtualScreen
{
    public VirtualScreenInfo VSInfo;
    internal Thread UpdateThread;

    public VirtualScreen(IntPtr hwnd, int Width, int Height, int RefreshRate)
    {
        // Initialize the VirtualScreenInfo.
        VSInfo = new VirtualScreenInfo();
        VSInfo.Screen = new Bitmap(Width, Height);
        VSInfo.Device = API.GetDC(hwnd);
        VSInfo.Width = Width;
        VSInfo.Height = Height;
        VSInfo.RefreshRate = RefreshRate;

        // Clear the Screen.
        Clear();
    }

    public void RunRendererThread()
    {
        // Enable Drawing.
        VSInfo.isDrawing = true;

        // Clear the Screen.
        Clear();

        // Start the Update Thread.
        UpdateThread = new Thread(new ThreadStart(Update));
        UpdateThread.Start();
    }
    public void Resize(int Width, int Height)
    {
        // Stop the Renderer Thread.
        StopRendererThread();

        // Clear the Screen.
        Clear();

        // Edit the VirtualScreenInfo.
        VSInfo.Screen = new Bitmap(Width, Height);
        VSInfo.Width = Width;
        VSInfo.Height = Height;

        // Run the Thread Again.
        RunRendererThread();
    }
    public void StopRendererThread()
    {
        // Stop the While Loop.
        VSInfo.isDrawing = false;

        // For Security we Stop the Update Thread.
        UpdateThread.Abort();

        // Call the Delegate to Stop the screen.
        VirtualScreenEventHandler.onStopScreen(this, this);
    }


    public void Update()
    {
        // Screen get at last cleared but we need to invert it so also the dlg. have to be inverted what get drawn first..
        while (VSInfo.isDrawing)
        {
            // Run the Delegate for Begin Update Frame
            VirtualScreenEventHandler.onBeginUpdateFrame(this, this);

            // Clear the Screen
            Clear();

            // Run the Delegate for Update Frame
            VirtualScreenEventHandler.onUpdateFrame(this, this);

            // Update the Screen.
            Render();

            // Run the Delegate for End Frame
            VirtualScreenEventHandler.onEndFrame(this, this);
        }

        // Release the DC on the End.
        API.ReleaseDC(IntPtr.Zero, VSInfo.Device);
    }
    public void Render()
    {
        // Check for Zero
        if (VSInfo.Device == IntPtr.Zero)
            return;

        Graphics g;
        try
        {
            g = Graphics.FromHdc(VSInfo.Device);
        }
        catch
        {
            return;
        }
        lock (g)
        {
            lock (VSInfo.Screen)
            {
                Graphics sg = Graphics.FromImage(VSInfo.Screen);

                //Color a1 = API.GetPixel(g, 0, 0);
                //g.DrawLine(Pens.Aqua, 0, 0, 12, 12);

                //if (API.GetPixel(g, 0, 0) != API.GetPixel(sg, 0, 0))
                //{
                g.DrawImage(VSInfo.Screen, 0, 0);
                //}
            }
        }
        Thread.Sleep(1000 / VSInfo.RefreshRate);
    }
    public void Clear()
    {
        Graphics g = Graphics.FromImage(VSInfo.Screen);
        g.FillRectangle(Brushes.Black, new Rectangle(0, 0, VSInfo.Width, VSInfo.Height));
        g.Flush();
        //for (int x = 0; x < VSInfo.Width; x++)
        //    for (int y = 0; y < VSInfo.Height; y++)
        //        if(VSInfo.isDrawing)
        //            VSInfo.Screen.SetPixel(x, y, Color.Black);            
    }






    public void UI_DrawText(string Text, Vector2 Position, Font Style, Brush Brush)
    {
        Graphics g = Graphics.FromImage(VSInfo.Screen);
        g.DrawString(Text, Style, Brush, Position.x, Position.y);
        g.Flush();


        Vector2 Size = GetTextInfo(Text, Style);
        UI_DrawOutLine(Position, Size);
    }
    public void UI_DrawText(string Text, TextAlignment_t TextAlignment, Font Style, Brush Brush)
    {
        UI_DrawText(Text, TextAlignment, new Vector2(0, 0), Style, Brush);
    }
    public void UI_DrawText(string Text, TextAlignment_t TextAlignment, Vector2 Position, Font Style, Brush Brush)
    {
        // Get the Size of the String if it where drawn
        Vector2 strWidth = GetTextInfo(Text, Style);

        switch (TextAlignment)
        {
            case TextAlignment_t.TOP_LEFT:
            UI_DrawText(Text, new Vector2(Position.x, Position.y + strWidth.y), Style, Brush);
            break;
            case TextAlignment_t.TOP_RIGHT:
            UI_DrawText(Text, new Vector2(Position.x + VSInfo.Width - strWidth.x, Position.y + strWidth.y), Style, Brush);
            break;
            case TextAlignment_t.TOP_CENTER:
            UI_DrawText(Text, new Vector2(Position.x + VSInfo.Width / 2 - strWidth.x / 2, Position.y + strWidth.y), Style, Brush);
            break;



            case TextAlignment_t.CENTER_LEFT:
            UI_DrawText(Text, new Vector2(Position.x, Position.y + VSInfo.Height / 2 - strWidth.y / 2), Style, Brush);
            break;
            case TextAlignment_t.CENTER_RIGHT:
            UI_DrawText(Text, new Vector2(Position.x + VSInfo.Width - strWidth.x, Position.y + VSInfo.Height / 2 - strWidth.y / 2), Style, Brush);
            break;
            case TextAlignment_t.CENTER_CENTER:
            UI_DrawText(Text, new Vector2(Position.x + VSInfo.Width / 2 - strWidth.x / 2, Position.y + VSInfo.Height / 2 - strWidth.y / 2), Style, Brush);
            break;


            case TextAlignment_t.BOTTOM_LEFT:
            UI_DrawText(Text, new Vector2(Position.x, Position.y + VSInfo.Height - strWidth.y), Style, Brush);
            break;
            case TextAlignment_t.BOTTOM_RIGHT:
            UI_DrawText(Text, new Vector2(Position.x + VSInfo.Width - strWidth.x, Position.y + VSInfo.Height - strWidth.y), Style, Brush);
            break;
            case TextAlignment_t.BOTTOM_CENTER:
            UI_DrawText(Text, new Vector2(Position.x + VSInfo.Width / 2 - strWidth.x / 2, Position.y + VSInfo.Height - strWidth.y), Style, Brush);
            break;

        }
    }

    public void UI_DrawImage(GfxImage Image, Vector2 Position, Vector2 Size)
    {
        Graphics g = Graphics.FromImage(VSInfo.Screen);
        g.DrawImage(Image.GetImage(), Position.x, Position.y, Size.x, Size.y);
        g.Flush();


        UI_DrawOutLine(Position, Size);
    }
    public void UI_DrawImage(GfxImage Image, Vector2 Position)
    {
        Image m_image = Image.GetImage();
        UI_DrawImage(Image, Position, new Vector2(m_image.Width, m_image.Height));
    }

    public void UI_DrawLine(Vector2 Position, Vector2 EndPosition, Pen Pen)
    {
        Graphics g = Graphics.FromImage(VSInfo.Screen);
        g.DrawLine(Pen, Position.x, Position.y, EndPosition.x, EndPosition.y);
        g.Flush();
    }
    public void UI_DrawRectangle(Vector2 Position, Vector2 EndPosition, Pen Pen)
    {
        //left to lower left
        UI_DrawLine(new Vector2(Position.x, Position.y), new Vector2(Position.x, Position.y + EndPosition.x), Pens.Red);

        //right to lower right
        UI_DrawLine(new Vector2(Position.x + EndPosition.y, Position.y + EndPosition.y), new Vector2(Position.x + EndPosition.x, Position.y), Pens.Red);

        //left to right top
        UI_DrawLine(new Vector2(Position.x, Position.y), new Vector2(Position.x + EndPosition.x, Position.y), Pens.Red);

        //left to right lower
        UI_DrawLine(new Vector2(Position.x, Position.y + EndPosition.y), new Vector2(Position.x + EndPosition.x, Position.y + EndPosition.y), Pens.Red);
    }
    public void UI_DrawFilledRectangle(Vector2 Position, Vector2 Size, Brush Brush)
    {
        GfxImage m_Image = new GfxImage(Size);
        m_Image.GetGraphics().FillRectangle(Brush, 0, 0, (int)Size.x, (int)Size.y);
        m_Image.GetGraphics().Flush();

        UI_DrawImage(m_Image, Position, Size);
    }


    public void UI_DrawOutLine(Vector2 Position, Vector2 Size)
    {
        return;
        //left to lower left
        UI_DrawLine(Position, new Vector2(Position.x, Position.y + Size.y), Pens.Red);

        //right to lower right
        UI_DrawLine(new Vector2(Position.x + Size.x, Position.y), new Vector2(Position.x + Size.x, Position.y + Size.y), Pens.Red);

        //left to right top
        UI_DrawLine(Position, new Vector2(Position.x + Size.x, Position.y), Pens.Red);

        //left to right lower
        UI_DrawLine(new Vector2(Position.x, Position.y + Size.y), new Vector2(Position.x + Size.x, Position.y + Size.y), Pens.Red);

        // Crossline lower left to upper right
        UI_DrawLine(new Vector2(Position.x, Position.y + Size.y), new Vector2(Position.x + Size.x, Position.y), Pens.Red);

        // Crossline upper left to lower right
        UI_DrawLine(Position, new Vector2(Position.x + Size.x, Position.y + Size.y), Pens.Red);
    }
    public Vector2 GetTextInfo(string Text, Font Style)
    {
        if (VSInfo.Device == IntPtr.Zero)
            new Vector2(0, 0);

        Graphics g;
        try
        {
            g = Graphics.FromHdc(VSInfo.Device);
        }
        catch
        {
            return Vector2.zero;
        }

        SizeF sf = g.MeasureString(Text, Style);
        return new Vector2(sf.Width, sf.Height);
    }
}
public class API
{
    [DllImport("User32.dll")]
    public static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("User32.dll")]
    public static extern void ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern int GetPixel(IntPtr hDC, int x, int y);

    public static Color GetPixel(Graphics g, int x, int y)
    {
        int color = 0;
        color = GetPixel(g.GetHdc(), x, y);
        return Color.FromArgb(color);
    }


    [DllImport("kernel32")]
    public static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool AttachConsole(uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Beep(uint dwFreq, uint dwDuration);//500, 100
}