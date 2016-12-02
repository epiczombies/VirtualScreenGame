//  __  __           _        _             __  __ _      _     _ _____ _____  ___ ___  
// |  \/  |         | |      | |           |  \/  (_)    | |   (_) ____| ____|/ _ \__ \ 
// | \  / | __ _  __| | ___  | |__  _   _  | \  / |_  ___| |__  _| |__ | |__ | | | | ) |
// | |\/| |/ _` |/ _` |/ _ \ | '_ \| | | | | |\/| | |/ __| '_ \| |___ \|___ \| | | |/ / 
// | |  | | (_| | (_| |  __/ | |_) | |_| | | |  | | | (__| | | | |___) |___) | |_| / /_ 
// |_|  |_|\__,_|\__,_|\___| |_.__/ \__, | |_|  |_|_|\___|_| |_|_|____/|____/ \___/____|
//   ____                      _____ __/ |                                              
//  / __ \                    / ____|___/                                               
// | |  | |_ __   ___ _ __   | (___   ___  _   _ _ __ ___ ___                           
// | |  | | '_ \ / _ \ '_ \   \___ \ / _ \| | | | '__/ __/ _ \                          
// | |__| | |_) |  __/ | | |  ____) | (_) | |_| | | | (_|  __/                          
//  \____/| .__/ \___|_| |_| |_____/ \___/ \__,_|_|  \___\___|                          
//        | |                                                                           
//        |_|                                                                           

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Globalization;
using UnknownType = System.Object;

//namespace MemoryClass
//{
public enum LogLevel
{
    LogLevel_None = 0,
    LogLevel_Warning = 1,
    LogLevel_Error = 2,
    LogLevel_Debug = 4,
    LogLevel_Info = 8,
    LogLevel_Trace = 16,
    LogLevel_All = 31,
};
public enum LogColor
{
    LogColor_Black = 0,
    LogColor_Blue = 1,
    LogColor_Green = 2,
    LogColor_Cyan = 3,
    LogColor_Red = 4,
    LogColor_Magenta = 5,
    LogColor_Brown = 6,
    LogColor_LightGrey = 7,
    LogColor_DarkGrey = 8,
    LogColor_LightBlue = 9,
    LogColor_LightGreen = 10,
    LogColor_LightCyan = 11,
    LogColor_LightRed = 12,
    LogColor_LightMagenta = 13,
    LogColor_Yellow = 14,
    LogColor_White = 15,
    LogColor_Blink = 128,
};
public class Log
{
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(UInt32 nStdHandle);
    [DllImport("kernel32.dll")]
    private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
    [DllImport("kernel32")]
    static extern bool AllocConsole();

    private const UInt32 StdOutputHandle = 0xFFFFFFF5;


    static void Print(LogLevel type, LogColor color, char identifier, string svc, string msg)
    {
        SetConsoleTextColor(LogColor.LogColor_DarkGrey);

        DateTime a1 = DateTime.Now;
        Console.Write(a1.Hour + ":" + a1.Minute + ":" + a1.Second);
        SetConsoleTextColor(color);
        Console.Write(hString.va(" %c ", identifier));
        SetConsoleTextColor(LogColor.LogColor_White);
        Console.Write(hString.va("%s : ", svc));
        SetConsoleTextColor(color);
        Console.Write(hString.va("%s\r\n", msg));
    }
    static void SetConsoleTextColor(LogColor color)
    {
        Console.ForegroundColor = (ConsoleColor)color;
    }


    public static void CreateConsole()
    {
        AllocConsole();

        // stdout's handle seems to always be equal to 7
        IntPtr defaultStdout = new IntPtr(7);
        IntPtr currentStdout = GetStdHandle(StdOutputHandle);

        if (currentStdout != defaultStdout)
            // reset stdout
            SetStdHandle(StdOutputHandle, defaultStdout);

        // reopen stdout
        TextWriter writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(writer);
    }
    public static void Debug(string svc, string msg, params object[] args)
    {
        Print(LogLevel.LogLevel_Debug, LogColor.LogColor_LightCyan, 'D', svc, hString.va(msg, args));
    }
    public static void Error(string svc, string msg, params object[] args)
    {
        Print(LogLevel.LogLevel_Error, LogColor.LogColor_Red, 'E', svc, hString.va(msg, args));
    }
    public static void Info(string svc, string msg, params object[] args)
    {
        Print(LogLevel.LogLevel_Info, LogColor.LogColor_LightGrey, 'I', svc, hString.va(msg, args));
    }
    public static void Trace(string svc, string msg, params object[] args)
    {
        Print(LogLevel.LogLevel_Trace, LogColor.LogColor_DarkGrey, 'T', svc, hString.va(msg, args));
    }
    public static void Warning(string svc, string msg, params object[] args)
    {
        Print(LogLevel.LogLevel_Warning, LogColor.LogColor_Yellow, 'W', svc, hString.va(msg, args));
    }
}
public class hString
{
    // %s = string
    // %c = char
    // %f = float
    // %i = int
    // %b = bool
    public static string va(string Text, params object[] args)
    {
        int textLength = Text.Length;

        // arguments passed.
        int argC = args.Length;
        int argP = 0;

        for (int i = 0; i < textLength; i++)
        {
            if (argP >= argC)
                break;

            // find the next %(char)
            if (Text[i] == '%')
            {
                // the next char after the current ( i ) is out of the array so fuck all up and return the string.
                if (textLength < i)
                    return Text;

                // Not sure about this! does the P goes 1 over C?
                if (argP >= argC)
                    break;

                if (Text[i + 1] == 's')
                {
                    if (args[argP].GetType() == typeof(string))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString());
                    }
                    else if (args[argP].GetType() == typeof(char))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString()[0].ToString());
                    }
                    else if (args[argP].GetType() == typeof(int))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString());
                    }
                    else if (args[argP].GetType() == typeof(long))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString());
                    }
                }
                else if (Text[i + 1] == 'c')
                {
                    if (args[argP].GetType() == typeof(string))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString()[0].ToString());
                    }
                    else if (args[argP].GetType() == typeof(char))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString());
                    }
                    else if (args[argP].GetType() == typeof(int))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), Convert.ToChar(args[argP]).ToString());
                    }
                    else if (args[argP].GetType() == typeof(DWORD))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), Convert.ToChar(args[argP]).ToString());
                    }
                    else if (args[argP].GetType() == typeof(DWORD64))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), Convert.ToChar(args[argP]).ToString());
                    }
                }
                else if (Text[i + 1] == 'f')
                {
                    if (args[argP].GetType() == typeof(float))
                    {
                        //Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString());
                    }
                    else if (args[argP].GetType() == typeof(int))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString());
                    }
                    else if (args[argP].GetType() == typeof(DWORD))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), Convert.ToSingle(args[argP]).ToString());
                    }
                    else if (args[argP].GetType() == typeof(DWORD64))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), Convert.ToSingle(args[argP]).ToString());
                    }
                }
                else if (Text[i + 1] == 'i')
                {
                    if (args[argP].GetType() == typeof(float))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), Convert.ToInt32(args[argP]).ToString());
                    }
                    else if (args[argP].GetType() == typeof(int))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString());
                    }
                    else if (args[argP].GetType() == typeof(DWORD))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString());
                    }
                    else if (args[argP].GetType() == typeof(DWORD64))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString());
                    }
                }
                else if (Text[i + 1] == 'b')
                {
                    if (args[argP].GetType() == typeof(bool))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), args[argP].ToString());
                    }
                    else if (args[argP].GetType() == typeof(int))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), Convert.ToBoolean(args[argP]).ToString());
                    }
                    else if (args[argP].GetType() == typeof(float))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), Convert.ToBoolean(args[argP]).ToString());
                    }
                    else if (args[argP].GetType() == typeof(DWORD))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), Convert.ToBoolean(args[argP]).ToString());
                    }
                    else if (args[argP].GetType() == typeof(DWORD64))
                    {
                        Text = ReplaceFirst(Text, ("%" + Text[i + 1]), Convert.ToBoolean(args[argP]).ToString());
                    }
                }
                argP++;
            }

            // Update 'textLength'
            textLength = Text.Length;
        }
        return Text;
    }
    static string ReplaceFirst(string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
            return text;
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
}
public class MemoryBuffer
{
    byte[] _Buffer;
    DWORD _Position = 0;

    public byte[] Buffer
    {
        get { return _Buffer; }
        set { _Buffer = value; }
    }
    public DWORD Position
    {
        get { return _Position; }
        set { _Position = value; }
    }

    public MemoryBuffer()
    {
        _Position = 0;
        _Buffer = new byte[] { };
    }
    public MemoryBuffer(byte[] buffer)
    {
        //_Position = buffer.Length;
        _Buffer = buffer;
    }
    public MemoryBuffer(byte buffer)
    {
        //_Position = 1;

        _Buffer = new byte[1];
        _Buffer[0] = buffer;
    }
    public MemoryBuffer(string path)
    {
        _Buffer = File.ReadAllBytes(path);
        //_Position = _Buffer.Length;
    }
    public MemoryBuffer(DWORD size)
    {
        _Buffer = new byte[size];
        //_Position = size;
    }
    public MemoryBuffer(MemoryBuffer buffer)
    {
        _Buffer = buffer._Buffer;
        _Position = _Buffer.Length;
    }



    public DWORD GetBufferSize()
    {
        return _Buffer.Length;
    }
    public DWORD AllocMem(int Size)
    {
        if (Size < 1)
            throw new ArgumentException("Argument \"Size\" is Smaller than 1");

        Array.Resize(ref _Buffer, GetBufferSize() + Size);
        return GetBufferSize() - Size;
    }




    public byte[] ReadBytes(int Length)
    {
        if (_Position + Length >= GetBufferSize() || _Position >= GetBufferSize())
            throw new ArgumentException("Cursor out of Array.");

        byte[] pBuffer = new byte[Length];
        Array.Copy(_Buffer, _Position, pBuffer, 0, Length);
        _Position += Length;
        return pBuffer;
    }
    public byte ReadByte()
    {
        return ReadBytes(1)[0];
    }
    public string ReadString()
    {
        StringBuilder sb = new StringBuilder();

        byte i;
        while ((i = ReadByte()) != 0)
        {
            sb.Append(Convert.ToChar(i));
        }
        return sb.ToString();
    }
    public string ReadString(int Length)
    {
        return new ASCIIEncoding().GetString(ReadBytes(Length));
    }
    public float ReadFloat()
    {
        return BitConverter.ToSingle(ReadBytes(4), 0);
    }
    public bool ReadBool()
    {
        return ReadByte() == 1 ? true : false;
    }
    public Vector4 ReadVector4()
    {
        return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
    }
    public Vector3 ReadVector3()
    {
        return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
    }
    public Vector2 ReadVector2()
    {
        return new Vector2(ReadFloat(), ReadFloat());
    }
    public short ReadInt16()
    {
        return BitConverter.ToInt16(ReadBytes(2), 0);
    }
    public ushort ReadUInt16()
    {
        return BitConverter.ToUInt16(ReadBytes(2), 0);
    }
    public int ReadInt32()
    {
        return BitConverter.ToInt32(ReadBytes(4), 0);
    }
    public uint ReadUInt32()
    {
        return BitConverter.ToUInt32(ReadBytes(4), 0);
    }
    public long ReadInt64()
    {
        return BitConverter.ToInt64(ReadBytes(8), 0);
    }
    public ulong ReadUInt64()
    {
        return BitConverter.ToUInt64(ReadBytes(8), 0);
    }





    /// <summary>
    /// Add buffer to the mem
    /// </summary>
    /// <param name="buffer"></param>
    public void WriteBytes(byte[] buffer)
    {
        DWORD allocMemPos = AllocMem(buffer.Length);
        Array.Copy(buffer, 0, _Buffer, allocMemPos, buffer.Length);
        _Position += buffer.Length;
    }
    public void WriteByte(byte value)
    {
        WriteBytes(new byte[] { value });
    }
    public void WriteString(string Text, bool ZeroTerminating)
    {
        byte[] b = Encoding.UTF8.GetBytes(Text);
        if (ZeroTerminating)
        {
            Array.Resize(ref b, b.Length + 1);
            b[b.Length - 1] = 0x00;
        }
        WriteBytes(b);
    }
    public void WriteFloat(float value)
    {
        WriteBytes(BitConverter.GetBytes(value));
    }
    public void WriteBool(bool value)
    {
        WriteByte((value ? (byte)1 : (byte)0));
    }
    public void WriteVector4(Vector4 value)
    {
        WriteFloat(value.x);
        WriteFloat(value.y);
        WriteFloat(value.z);
        WriteFloat(value.w);
    }
    public void WriteVector3(Vector3 value)
    {
        WriteFloat(value.x);
        WriteFloat(value.y);
        WriteFloat(value.z);
    }
    public void WriteVector2(Vector2 value)
    {
        WriteFloat(value.x);
        WriteFloat(value.y);
    }
    public void WriteInt16(short value)
    {
        WriteBytes(BitConverter.GetBytes(value));
    }
    public void WriteUInt16(short value)
    {
        WriteBytes(BitConverter.GetBytes(value));
    }
    public void WriteInt32(int value)
    {
        WriteBytes(BitConverter.GetBytes(value));
    }
    public void WriteUInt32(uint value)
    {
        WriteBytes(BitConverter.GetBytes(value));
    }
    public void WriteInt64(long value)
    {
        WriteBytes(BitConverter.GetBytes(value));
    }
    public void WriteUInt64(ulong value)
    {
        WriteBytes(BitConverter.GetBytes(value));
    }





    /// <summary>
    /// Write buffer to Position / OverStrikeMode
    /// </summary>
    /// <param name="Position"></param>
    /// <param name="Buffer"></param>
    /// <param name="OverStrikeMode"></param>
    /// <param name="CountPosition"></param>
    public void WriteBytes(DWORD Position, byte[] Buffer, bool OverStrikeMode)
    {
        DWORD BufferLength = GetBufferSize();


        if (Buffer.Length > GetBufferSize())
            AllocMem(Buffer.Length);


        if (OverStrikeMode)
            Array.Copy(Buffer, 0, _Buffer, Position, Buffer.Length);
        else
        {
            AllocMem(Buffer.Length);
            Array.Copy(_Buffer, Position, _Buffer, Position + Buffer.Length, BufferLength - Position);

            Array.Copy(Buffer, 0, _Buffer, Position, Buffer.Length);
        }
    }
    public void WriteByte(DWORD Position, byte value, bool OverStrikeMode)
    {
        WriteBytes(Position, new byte[] { value }, OverStrikeMode);
    }
    public void WriteString(DWORD Position, string Text, bool ZeroTerminating, bool OverStrikeMode)
    {
        byte[] b = Encoding.UTF8.GetBytes(Text);
        if (ZeroTerminating)
        {
            Array.Resize(ref b, b.Length + 1);
            b[b.Length - 1] = 0x00;
        }
        WriteBytes(Position, b, OverStrikeMode);
    }
    public void WriteFloat(DWORD Position, float value, bool OverStrikeMode)
    {
        WriteBytes(Position, BitConverter.GetBytes(value), OverStrikeMode);
    }
    public void WriteBool(DWORD Position, bool value, bool OverStrikeMode)
    {
        WriteByte(Position, (value ? (byte)1 : (byte)0), OverStrikeMode);
    }
    public void WriteVector4(DWORD Position, Vector4 value, bool OverStrikeMode)
    {
        WriteFloat(Position, value.x, OverStrikeMode);
        WriteFloat(Position, value.y, OverStrikeMode);
        WriteFloat(Position, value.z, OverStrikeMode);
        WriteFloat(Position, value.w, OverStrikeMode);
    }
    public void WriteVector3(DWORD Position, Vector3 value, bool OverStrikeMode)
    {
        WriteFloat(Position, value.x, OverStrikeMode);
        WriteFloat(Position, value.y, OverStrikeMode);
        WriteFloat(Position, value.z, OverStrikeMode);
    }
    public void WriteVector2(DWORD Position, Vector2 value, bool OverStrikeMode)
    {
        WriteFloat(Position, value.x, OverStrikeMode);
        WriteFloat(Position, value.y, OverStrikeMode);
    }
    public void WriteInt16(DWORD Position, short value, bool OverStrikeMode)
    {
        WriteBytes(Position, BitConverter.GetBytes(value), OverStrikeMode);
    }
    public void WriteUInt16(DWORD Position, short value, bool OverStrikeMode)
    {
        WriteBytes(Position, BitConverter.GetBytes(value), OverStrikeMode);
    }
    public void WriteInt32(DWORD Position, int value, bool OverStrikeMode)
    {
        WriteBytes(Position, BitConverter.GetBytes(value), OverStrikeMode);
    }
    public void WriteUInt32(DWORD Position, uint value, bool OverStrikeMode)
    {
        WriteBytes(Position, BitConverter.GetBytes(value), OverStrikeMode);
    }
    public void WriteInt64(DWORD Position, long value, bool OverStrikeMode)
    {
        WriteBytes(Position, BitConverter.GetBytes(value), OverStrikeMode);
    }
    public void WriteUInt64(DWORD Position, ulong value, bool OverStrikeMode)
    {
        WriteBytes(Position, BitConverter.GetBytes(value), OverStrikeMode);
    }


    public byte[] ReadBytes(DWORD Position, int Length)
    {
        if (Position + Length >= GetBufferSize() || Position >= GetBufferSize())
            throw new ArgumentException("Cursor out of Array.");

        byte[] pBuffer = new byte[Length];
        Array.Copy(_Buffer, Position, pBuffer, 0, Length);
        return pBuffer;
    }
    public byte ReadByte(DWORD Position)
    {
        return ReadBytes(Position, 1)[0];
    }
    public string ReadString(DWORD Position)
    {
        StringBuilder sb = new StringBuilder();
        int s = 0;
        byte i;
        while ((i = ReadByte(Position + s)) != 0)
        {
            s++;
            sb.Append(Convert.ToChar(i));
        }
        return sb.ToString();
    }
    public string ReadString(DWORD Position, int Length)
    {
        return new ASCIIEncoding().GetString(ReadBytes(Position, Length));
    }
    public float ReadFloat(DWORD Position)
    {
        return BitConverter.ToSingle(ReadBytes(Position, 4), 0);
    }
    public bool ReadBool(DWORD Position)
    {
        return ReadByte(Position) == 1 ? true : false;
    }
    public Vector4 ReadVector4(DWORD Position)
    {
        return new Vector4(ReadFloat(Position), ReadFloat(Position), ReadFloat(Position), ReadFloat(Position));
    }
    public Vector3 ReadVector3(DWORD Position)
    {
        return new Vector3(ReadFloat(Position), ReadFloat(Position), ReadFloat(Position));
    }
    public Vector2 ReadVector2(DWORD Position)
    {
        return new Vector2(ReadFloat(Position), ReadFloat(Position));
    }
    public short ReadInt16(DWORD Position)
    {
        return BitConverter.ToInt16(ReadBytes(Position, 2), 0);
    }
    public ushort ReadUInt16(DWORD Position)
    {
        return BitConverter.ToUInt16(ReadBytes(Position, 2), 0);
    }
    public int ReadInt32(DWORD Position)
    {
        return BitConverter.ToInt32(ReadBytes(Position, 4), 0);
    }
    public uint ReadUInt32(DWORD Position)
    {
        return BitConverter.ToUInt32(ReadBytes(Position, 4), 0);
    }
    public long ReadInt64(DWORD Position)
    {
        return BitConverter.ToInt64(ReadBytes(Position, 8), 0);
    }
    public ulong ReadUInt64(DWORD Position)
    {
        return BitConverter.ToUInt64(ReadBytes(Position, 8), 0);
    }










    public void RemoveLast()
    {
        RemoveAt(GetBufferSize() - 1);
    }
    public void RemoveAt(DWORD Position, int Length = 1)
    {
        if (GetBufferSize() <= 0 || Position < 0)
            return;

        if (Position == 0)
        {
            _Buffer[0] = 0x00;
            _Position = 0;
            return;
        }

        Array.Copy(_Buffer, Position, _Buffer, Position - Length + 1, GetBufferSize() - Position);

        _Buffer[GetBufferSize() - 1] = 0x00;

        Array.Resize(ref _Buffer, GetBufferSize() - Length);

        _Position -= Length;
    }
}
public struct Mathf
{
    public const float PI = 3.141593f;
    public const float Infinity = float.PositiveInfinity;
    public const float NegativeInfinity = float.NegativeInfinity;
    public const float Deg2Rad = 0.01745329f;
    public const float Rad2Deg = 57.29578f;
    public const float Epsilon = float.Epsilon;

    public static bool isInRect(Vector2 pt, Vector4 rec)
    {
        if (pt.x >= rec.x && pt.x <= rec.w && pt.y <= rec.z && pt.y >= rec.y)
            return true;
        else
            return false;
    }
    public static float Sin(float f)
    {
        return (float)Math.Sin(f);
    }
    public static float Cos(float f)
    {
        return (float)Math.Cos(f);
    }
    public static float Tan(float f)
    {
        return (float)Math.Tan(f);
    }
    public static float Asin(float f)
    {
        return (float)Math.Asin((double)f);
    }
    public static float Acos(float f)
    {
        return (float)Math.Acos((double)f);
    }
    public static float Atan(float f)
    {
        return (float)Math.Atan((double)f);
    }
    public static float Atan2(float y, float x)
    {
        return (float)Math.Atan2((double)y, (double)x);
    }
    public static float Sqrt(float f)
    {
        return (float)Math.Sqrt((double)f);
    }
    public static float Abs(float f)
    {
        return Math.Abs(f);
    }
    public static int Abs(int value)
    {
        return Math.Abs(value);
    }
    public static float Min(float a, float b)
    {
        return ((a >= b) ? b : a);
    }
    public static float Min(params float[] values)
    {
        int length = values.Length;
        if (length == 0)
        {
            return 0.0f;
        }

        float num2 = values[0];
        for (int i = 1; i < length; i++)
        {
            if (values[i] < num2)
            {
                num2 = values[i];
            }
        }
        return num2;
    }
    public static int Min(int a, int b)
    {
        return ((a >= b) ? b : a);
    }
    public static int Min(params int[] values)
    {
        int length = values.Length;
        if (length == 0)
        {
            return 0;
        }
        int num2 = values[0];
        for (int i = 1; i < length; i++)
        {
            if (values[i] < num2)
            {
                num2 = values[i];
            }
        }
        return num2;
    }
    public static float Max(float a, float b)
    {
        return ((a <= b) ? b : a);
    }
    public static float Max(params float[] values)
    {
        int length = values.Length;
        if (length == 0)
        {
            return 0f;
        }
        float num2 = values[0];
        for (int i = 1; i < length; i++)
        {
            if (values[i] > num2)
            {
                num2 = values[i];
            }
        }
        return num2;
    }
    public static int Max(int a, int b)
    {
        return ((a <= b) ? b : a);
    }
    public static int Max(params int[] values)
    {
        int length = values.Length;
        if (length == 0)
        {
            return 0;
        }
        int num2 = values[0];
        for (int i = 1; i < length; i++)
        {
            if (values[i] > num2)
            {
                num2 = values[i];
            }
        }
        return num2;
    }
    public static float Pow(float f, float p)
    {
        return (float)Math.Pow(f, p);
    }
    public static float Exp(float power)
    {
        return (float)Math.Exp(power);
    }
    public static float Log(float f, float p)
    {
        return (float)Math.Log(f, p);
    }
    public static float Log(float f)
    {
        return (float)Math.Log(f);
    }
    public static float Log10(float f)
    {
        return (float)Math.Log10(f);
    }
    public static float Ceil(float f)
    {
        return (float)Math.Ceiling(f);
    }
    public static float Floor(float f)
    {
        return (float)Math.Floor(f);
    }
    public static float Round(float f)
    {
        return (float)Math.Round(f);
    }
    public static int CeilToInt(float f)
    {
        return (int)Math.Ceiling(f);
    }
    public static int FloorToInt(float f)
    {
        return (int)Math.Floor(f);
    }
    public static int RoundToInt(float f)
    {
        return (int)Math.Round(f);
    }
    public static float Sign(float f)
    {
        return ((f < 0f) ? -1f : 1f);
    }
    public static float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
            value = min;
            return value;
        }
        if (value > max)
        {
            value = max;
        }
        return value;
    }
    public static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            value = min;
            return value;
        }
        if (value > max)
        {
            value = max;
        }
        return value;
    }
    public static float Clamp01(float value)
    {
        if (value < 0f)
        {
            return 0f;
        }
        if (value > 1f)
        {
            return 1f;
        }
        return value;
    }
    public static float Lerp(float from, float to, float t)
    {
        return (from + ((to - from) * Clamp01(t)));
    }
    public static float LerpAngle(float a, float b, float t)
    {
        float num = Repeat(b - a, 360f);
        if (num > 180f)
        {
            num -= 360f;
        }
        return (a + (num * Clamp01(t)));
    }
    public static float MoveTowards(float current, float target, float maxDelta)
    {
        if (Abs((float)(target - current)) <= maxDelta)
        {
            return target;
        }
        return (current + (Sign(target - current) * maxDelta));
    }
    public static float MoveTowardsAngle(float current, float target, float maxDelta)
    {
        target = current + DeltaAngle(current, target);
        return MoveTowards(current, target, maxDelta);
    }
    public static float SmoothStep(float from, float to, float t)
    {
        t = Clamp01(t);
        t = (((-2f * t) * t) * t) + ((3f * t) * t);
        return ((to * t) + (from * (1f - t)));
    }
    public static float Gamma(float value, float absmax, float gamma)
    {
        bool flag = false;
        if (value < 0f)
        {
            flag = true;
        }
        float num = Abs(value);
        if (num > absmax)
        {
            return (!flag ? num : -num);
        }
        float num2 = Pow(num / absmax, gamma) * absmax;
        return (!flag ? num2 : -num2);
    }
    public static bool Approximately(float a, float b)
    {
        return (Abs((float)(b - a)) < Max((float)(1E-06f * Max(Abs(a), Abs(b))), (float)1.121039E-44f));
    }
    public static float Repeat(float t, float length)
    {
        return (t - (Floor(t / length) * length));
    }
    public static float PingPong(float t, float length)
    {
        t = Repeat(t, length * 2f);
        return (length - Abs((float)(t - length)));
    }
    public static float InverseLerp(float from, float to, float value)
    {
        if (from < to)
        {
            if (value < from)
            {
                return 0f;
            }
            if (value > to)
            {
                return 1f;
            }
            value -= from;
            value /= to - from;
            return value;
        }
        if (from <= to)
        {
            return 0f;
        }
        if (value < to)
        {
            return 1f;
        }
        if (value > from)
        {
            return 0f;
        }
        return (1f - ((value - to) / (from - to)));
    }
    public static float DeltaAngle(float current, float target)
    {
        float num = Repeat(target - current, 360f);
        if (num > 180f)
        {
            num -= 360f;
        }
        return num;
    }
    internal static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 result)
    {
        float num = p2.x - p1.x;
        float num2 = p2.y - p1.y;
        float num3 = p4.x - p3.x;
        float num4 = p4.y - p3.y;
        float num5 = (num * num4) - (num2 * num3);
        if (num5 == 0f)
        {
            return false;
        }
        float num6 = p3.x - p1.x;
        float num7 = p3.y - p1.y;
        float num8 = ((num6 * num4) - (num7 * num3)) / num5;
        result = new Vector2(p1.x + (num8 * num), p1.y + (num8 * num2));
        return true;
    }
    internal static bool LineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 result)
    {
        float num = p2.x - p1.x;
        float num2 = p2.y - p1.y;
        float num3 = p4.x - p3.x;
        float num4 = p4.y - p3.y;
        float num5 = (num * num4) - (num2 * num3);
        if (num5 == 0f)
        {
            return false;
        }
        float num6 = p3.x - p1.x;
        float num7 = p3.y - p1.y;
        float num8 = ((num6 * num4) - (num7 * num3)) / num5;
        if ((num8 < 0f) || (num8 > 1f))
        {
            return false;
        }
        float num9 = ((num6 * num2) - (num7 * num)) / num5;
        if ((num9 < 0f) || (num9 > 1f))
        {
            return false;
        }
        result = new Vector2(p1.x + (num8 * num), p1.y + (num8 * num2));
        return true;
    }
}
public struct Vector2
{
    public const float kEpsilon = 1E-05f;
    public float x;
    public float y;
    public float this[int index]
    {
        get
        {
            if (index == 0)
            {
                return this.x;
            }
            if (index != 1)
            {
                throw new IndexOutOfRangeException("Invalid Vector2 index!");
            }
            return this.y;
        }
        set
        {
            if (index != 0)
            {
                if (index != 1)
                {
                    throw new IndexOutOfRangeException("Invalid Vector2 index!");
                }
                this.y = value;
            }
            else
            {
                this.x = value;
            }
        }
    }
    public Vector2 normalized
    {
        get
        {
            Vector2 result = new Vector2(this.x, this.y);
            result.Normalize();
            return result;
        }
    }
    public float magnitude
    {
        get
        {
            return Mathf.Sqrt(this.x * this.x + this.y * this.y);
        }
    }
    public float sqrMagnitude
    {
        get
        {
            return this.x * this.x + this.y * this.y;
        }
    }
    public static Vector2 zero
    {
        get
        {
            return new Vector2(0f, 0f);
        }
    }
    public static Vector2 one
    {
        get
        {
            return new Vector2(1f, 1f);
        }
    }
    public static Vector2 up
    {
        get
        {
            return new Vector2(0f, 1f);
        }
    }
    public static Vector2 right
    {
        get
        {
            return new Vector2(1f, 0f);
        }
    }
    public Vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
    public void Set(float new_x, float new_y)
    {
        this.x = new_x;
        this.y = new_y;
    }
    public static Vector2 Lerp(Vector2 from, Vector2 to, float t)
    {
        t = Mathf.Clamp01(t);
        return new Vector2(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t);
    }
    public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
    {
        Vector2 a = target - current;
        float magnitude = a.magnitude;
        if (magnitude <= maxDistanceDelta || magnitude == 0f)
        {
            return target;
        }
        return current + a / magnitude * maxDistanceDelta;
    }
    public static Vector2 Scale(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x, a.y * b.y);
    }
    public void Scale(Vector2 scale)
    {
        this.x *= scale.x;
        this.y *= scale.y;
    }
    public void Normalize()
    {
        float magnitude = this.magnitude;
        if (magnitude > 1E-05f)
        {
            this /= magnitude;
        }
        else
        {
            this = Vector2.zero;
        }
    }
    public override string ToString()
    {
        // FUCK MY LIFE I NEED %s TO OverStrikeMode THAT NIGGA WUUUUUUUUUT!!
        return hString.va("(%s, %s)", x.ToString().Replace(',', '.'), y.ToString().Replace(',', '.'));
        //return string.Format("({0:F1}, {1:F1})", new object[]
        //{
        //        this.x,
        //        this.y
        //});
    }
    public string ToString(string format)
    {
        return string.Format("({0}, {1})", new object[]
        {
            this.x.ToString(format),
            this.y.ToString(format)
        });
    }
    public override int GetHashCode()
    {
        return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
    }
    public override bool Equals(object other)
    {
        if (!(other is Vector2))
        {
            return false;
        }
        Vector2 vector = (Vector2)other;
        return this.x.Equals(vector.x) && this.y.Equals(vector.y);
    }
    public static float Dot(Vector2 lhs, Vector2 rhs)
    {
        return lhs.x * rhs.x + lhs.y * rhs.y;
    }
    public static float Angle(Vector2 from, Vector2 to)
    {
        return Mathf.Acos(Mathf.Clamp(Vector2.Dot(from.normalized, to.normalized), -1f, 1f)) * 57.29578f;
    }
    public static float Distance(Vector2 a, Vector2 b)
    {
        return (a - b).magnitude;
    }
    public static Vector2 ClampMagnitude(Vector2 vector, float maxLength)
    {
        if (vector.sqrMagnitude > maxLength * maxLength)
        {
            return vector.normalized * maxLength;
        }
        return vector;
    }
    public static float SqrMagnitude(Vector2 a)
    {
        return a.x * a.x + a.y * a.y;
    }
    public float SqrMagnitude()
    {
        return this.x * this.x + this.y * this.y;
    }
    public static Vector2 Min(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
    }
    public static Vector2 Max(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));
    }
    //[ExcludeFromDocs]
    //public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, float maxSpeed)
    //{
    //    float deltaTime = Time.deltaTime;
    //    return Vector2.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    //}
    //[ExcludeFromDocs]
    //public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime)
    //{
    //    float deltaTime = Time.deltaTime;
    //    float maxSpeed = float.PositiveInfinity;
    //    return Vector2.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    //}
    //public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
    //{
    //    smoothTime = Mathf.Max(0.0001f, smoothTime);
    //    float num = 2f / smoothTime;
    //    float num2 = num * deltaTime;
    //    float d = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
    //    Vector2 vector = current - target;
    //    Vector2 vector2 = target;
    //    float maxLength = maxSpeed * smoothTime;
    //    vector = Vector2.ClampMagnitude(vector, maxLength);
    //    target = current - vector;
    //    Vector2 vector3 = (currentVelocity + num * vector) * deltaTime;
    //    currentVelocity = (currentVelocity - num * vector3) * d;
    //    Vector2 vector4 = target + (vector + vector3) * d;
    //    if (Vector2.Dot(vector2 - current, vector4 - vector2) > 0f)
    //    {
    //        vector4 = vector2;
    //        currentVelocity = (vector4 - vector2) / deltaTime;
    //    }
    //    return vector4;
    //}
    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x + b.x, a.y + b.y);
    }
    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x - b.x, a.y - b.y);
    }
    public static Vector2 operator -(Vector2 a)
    {
        return new Vector2(-a.x, -a.y);
    }
    public static Vector2 operator *(Vector2 a, float d)
    {
        return new Vector2(a.x * d, a.y * d);
    }
    public static Vector2 operator *(float d, Vector2 a)
    {
        return new Vector2(a.x * d, a.y * d);
    }
    public static Vector2 operator /(Vector2 a, float d)
    {
        return new Vector2(a.x / d, a.y / d);
    }
    public static bool operator ==(Vector2 lhs, Vector2 rhs)
    {
        return Vector2.SqrMagnitude(lhs - rhs) < 9.99999944E-11f;
    }
    public static bool operator !=(Vector2 lhs, Vector2 rhs)
    {
        return Vector2.SqrMagnitude(lhs - rhs) >= 9.99999944E-11f;
    }
    public static implicit operator Vector2(Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }
    public static implicit operator Vector3(Vector2 v)
    {
        return new Vector3(v.x, v.y, 0f);
    }
}
public struct Vector3
{
    public const float kEpsilon = 1E-05f;
    public float x;
    public float y;
    public float z;
    public float this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                return this.x;
                case 1:
                return this.y;
                case 2:
                return this.z;
                default:
                throw new IndexOutOfRangeException("Invalid Vector3 index!");
            }
        }
        set
        {
            switch (index)
            {
                case 0:
                this.x = value;
                break;
                case 1:
                this.y = value;
                break;
                case 2:
                this.z = value;
                break;
                default:
                throw new IndexOutOfRangeException("Invalid Vector3 index!");
            }
        }
    }
    public Vector3 normalized
    {
        get
        {
            return Vector3.Normalize(this);
        }
    }
    public float magnitude
    {
        get
        {
            return Mathf.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
        }
    }
    public float sqrMagnitude
    {
        get
        {
            return this.x * this.x + this.y * this.y + this.z * this.z;
        }
    }
    public static Vector3 zero
    {
        get
        {
            return new Vector3(0f, 0f, 0f);
        }
    }
    public static Vector3 one
    {
        get
        {
            return new Vector3(1f, 1f, 1f);
        }
    }
    public static Vector3 forward
    {
        get
        {
            return new Vector3(0f, 0f, 1f);
        }
    }
    public static Vector3 back
    {
        get
        {
            return new Vector3(0f, 0f, -1f);
        }
    }
    public static Vector3 up
    {
        get
        {
            return new Vector3(0f, 1f, 0f);
        }
    }
    public static Vector3 down
    {
        get
        {
            return new Vector3(0f, -1f, 0f);
        }
    }
    public static Vector3 left
    {
        get
        {
            return new Vector3(-1f, 0f, 0f);
        }
    }
    public static Vector3 right
    {
        get
        {
            return new Vector3(1f, 0f, 0f);
        }
    }
    [Obsolete("Use Vector3.forward instead.")]
    public static Vector3 fwd
    {
        get
        {
            return new Vector3(0f, 0f, 1f);
        }
    }
    public Vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public Vector3(float x, float y)
    {
        this.x = x;
        this.y = y;
        this.z = 0f;
    }
    public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
    {
        t = Mathf.Clamp01(t);
        return new Vector3(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t, from.z + (to.z - from.z) * t);
    }
    public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        Vector3 a = target - current;
        float magnitude = a.magnitude;
        if (magnitude <= maxDistanceDelta || magnitude == 0f)
        {
            return target;
        }
        return current + a / magnitude * maxDistanceDelta;
    }
    //public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
    //{
    //    smoothTime = Mathf.Max(0.0001f, smoothTime);
    //    float num = 2f / smoothTime;
    //    float num2 = num * deltaTime;
    //    float d = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
    //    Vector3 vector = current - target;
    //    Vector3 vector2 = target;
    //    float maxLength = maxSpeed * smoothTime;
    //    vector = Vector3.ClampMagnitude(vector, maxLength);
    //    target = current - vector;
    //    Vector3 vector3 = (currentVelocity + num * vector) * deltaTime;
    //    currentVelocity = (currentVelocity - num * vector3) * d;
    //    Vector3 vector4 = target + (vector + vector3) * d;
    //    if (Vector3.Dot(vector2 - current, vector4 - vector2) > 0f)
    //    {
    //        vector4 = vector2;
    //        currentVelocity = (vector4 - vector2) / deltaTime;
    //    }
    //    return vector4;
    //}
    public void Set(float new_x, float new_y, float new_z)
    {
        this.x = new_x;
        this.y = new_y;
        this.z = new_z;
    }
    public static Vector3 Scale(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
    public void Scale(Vector3 scale)
    {
        this.x *= scale.x;
        this.y *= scale.y;
        this.z *= scale.z;
    }
    public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
    }
    public override int GetHashCode()
    {
        return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
    }
    public override bool Equals(object other)
    {
        if (!(other is Vector3))
        {
            return false;
        }
        Vector3 vector = (Vector3)other;
        return this.x.Equals(vector.x) && this.y.Equals(vector.y) && this.z.Equals(vector.z);
    }
    public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
    {
        return -2f * Vector3.Dot(inNormal, inDirection) * inNormal + inDirection;
    }
    public static Vector3 Normalize(Vector3 value)
    {
        float num = Vector3.Magnitude(value);
        if (num > 1E-05f)
        {
            return value / num;
        }
        return Vector3.zero;
    }
    public void Normalize()
    {
        float num = Vector3.Magnitude(this);
        if (num > 1E-05f)
        {
            this /= num;
        }
        else
        {
            this = Vector3.zero;
        }
    }
    public override string ToString()
    {
        return string.Format("({0:F1}, {1:F1}, {2:F1})", new object[]
        {
            this.x,
            this.y,
            this.z
        });
    }
    public string ToString(string format)
    {
        return string.Format("({0}, {1}, {2})", new object[]
        {
            this.x.ToString(format),
            this.y.ToString(format),
            this.z.ToString(format)
        });
    }
    public static float Dot(Vector3 lhs, Vector3 rhs)
    {
        return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
    }
    public static Vector3 Project(Vector3 vector, Vector3 onNormal)
    {
        float num = Vector3.Dot(onNormal, onNormal);
        if (num < Mathf.Epsilon)
        {
            return Vector3.zero;
        }
        return onNormal * Vector3.Dot(vector, onNormal) / num;
    }
    public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
    {
        return vector - Vector3.Project(vector, planeNormal);
    }
    [Obsolete("Use Vector3.ProjectOnPlane instead.")]
    public static Vector3 Exclude(Vector3 excludeThis, Vector3 fromThat)
    {
        return fromThat - Vector3.Project(fromThat, excludeThis);
    }
    public static float Angle(Vector3 from, Vector3 to)
    {
        return Mathf.Acos(Mathf.Clamp(Vector3.Dot(from.normalized, to.normalized), -1f, 1f)) * 57.29578f;
    }
    public static float Distance(Vector3 a, Vector3 b)
    {
        Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }
    public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
    {
        if (vector.sqrMagnitude > maxLength * maxLength)
        {
            return vector.normalized * maxLength;
        }
        return vector;
    }
    public static float Magnitude(Vector3 a)
    {
        return Mathf.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
    }
    public static float SqrMagnitude(Vector3 a)
    {
        return a.x * a.x + a.y * a.y + a.z * a.z;
    }
    public static Vector3 Min(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Mathf.Min(lhs.z, rhs.z));
    }
    public static Vector3 Max(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Mathf.Max(lhs.z, rhs.z));
    }
    [Obsolete("Use Vector3.Angle instead. AngleBetween uses radians instead of degrees and was deprecated for this reason")]
    public static float AngleBetween(Vector3 from, Vector3 to)
    {
        return Mathf.Acos(Mathf.Clamp(Vector3.Dot(from.normalized, to.normalized), -1f, 1f));
    }
    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }
    public static Vector3 operator -(Vector3 a)
    {
        return new Vector3(-a.x, -a.y, -a.z);
    }
    public static Vector3 operator *(Vector3 a, float d)
    {
        return new Vector3(a.x * d, a.y * d, a.z * d);
    }
    public static Vector3 operator *(float d, Vector3 a)
    {
        return new Vector3(a.x * d, a.y * d, a.z * d);
    }
    public static Vector3 operator /(Vector3 a, float d)
    {
        return new Vector3(a.x / d, a.y / d, a.z / d);
    }
    public static bool operator ==(Vector3 lhs, Vector3 rhs)
    {
        return Vector3.SqrMagnitude(lhs - rhs) < 9.99999944E-11f;
    }
    public static bool operator !=(Vector3 lhs, Vector3 rhs)
    {
        return Vector3.SqrMagnitude(lhs - rhs) >= 9.99999944E-11f;
    }
}
public struct Vector4
{
    public const float kEpsilon = 1E-05f;
    public float x;
    public float y;
    public float z;
    public float w;
    public float this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                return this.x;
                case 1:
                return this.y;
                case 2:
                return this.z;
                case 3:
                return this.w;
                default:
                throw new IndexOutOfRangeException("Invalid Vector4 index!");
            }
        }
        set
        {
            switch (index)
            {
                case 0:
                this.x = value;
                break;
                case 1:
                this.y = value;
                break;
                case 2:
                this.z = value;
                break;
                case 3:
                this.w = value;
                break;
                default:
                throw new IndexOutOfRangeException("Invalid Vector4 index!");
            }
        }
    }
    public Vector4 normalized
    {
        get
        {
            return Vector4.Normalize(this);
        }
    }
    public float magnitude
    {
        get
        {
            return Mathf.Sqrt(Vector4.Dot(this, this));
        }
    }
    public float sqrMagnitude
    {
        get
        {
            return Vector4.Dot(this, this);
        }
    }
    public static Vector4 zero
    {
        get
        {
            return new Vector4(0f, 0f, 0f, 0f);
        }
    }
    public static Vector4 one
    {
        get
        {
            return new Vector4(1f, 1f, 1f, 1f);
        }
    }
    public Vector4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
    public Vector4(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = 0f;
    }
    public Vector4(float x, float y)
    {
        this.x = x;
        this.y = y;
        this.z = 0f;
        this.w = 0f;
    }
    public void Set(float new_x, float new_y, float new_z, float new_w)
    {
        this.x = new_x;
        this.y = new_y;
        this.z = new_z;
        this.w = new_w;
    }
    public static Vector4 Lerp(Vector4 from, Vector4 to, float t)
    {
        t = Mathf.Clamp01(t);
        return new Vector4(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t, from.z + (to.z - from.z) * t, from.w + (to.w - from.w) * t);
    }
    public static Vector4 MoveTowards(Vector4 current, Vector4 target, float maxDistanceDelta)
    {
        Vector4 a = target - current;
        float magnitude = a.magnitude;
        if (magnitude <= maxDistanceDelta || magnitude == 0f)
        {
            return target;
        }
        return current + a / magnitude * maxDistanceDelta;
    }
    public static Vector4 Scale(Vector4 a, Vector4 b)
    {
        return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
    }
    public void Scale(Vector4 scale)
    {
        this.x *= scale.x;
        this.y *= scale.y;
        this.z *= scale.z;
        this.w *= scale.w;
    }
    public override int GetHashCode()
    {
        return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
    }
    public override bool Equals(object other)
    {
        if (!(other is Vector4))
        {
            return false;
        }
        Vector4 vector = (Vector4)other;
        return this.x.Equals(vector.x) && this.y.Equals(vector.y) && this.z.Equals(vector.z) && this.w.Equals(vector.w);
    }
    public static Vector4 Normalize(Vector4 a)
    {
        float num = Vector4.Magnitude(a);
        if (num > 1E-05f)
        {
            return a / num;
        }
        return Vector4.zero;
    }
    public void Normalize()
    {
        float num = Vector4.Magnitude(this);
        if (num > 1E-05f)
        {
            this /= num;
        }
        else
        {
            this = Vector4.zero;
        }
    }
    public override string ToString()
    {
        return string.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", new object[]
        {
            this.x,
            this.y,
            this.z,
            this.w
        });
    }
    public string ToString(string format)
    {
        return string.Format("({0}, {1}, {2}, {3})", new object[]
        {
            this.x.ToString(format),
            this.y.ToString(format),
            this.z.ToString(format),
            this.w.ToString(format)
        });
    }
    public static float Dot(Vector4 a, Vector4 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
    }
    public static Vector4 Project(Vector4 a, Vector4 b)
    {
        return b * Vector4.Dot(a, b) / Vector4.Dot(b, b);
    }
    public static float Distance(Vector4 a, Vector4 b)
    {
        return Vector4.Magnitude(a - b);
    }
    public static float Magnitude(Vector4 a)
    {
        return Mathf.Sqrt(Vector4.Dot(a, a));
    }
    public static float SqrMagnitude(Vector4 a)
    {
        return Vector4.Dot(a, a);
    }
    public float SqrMagnitude()
    {
        return Vector4.Dot(this, this);
    }
    public static Vector4 Min(Vector4 lhs, Vector4 rhs)
    {
        return new Vector4(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Mathf.Min(lhs.z, rhs.z), Mathf.Min(lhs.w, rhs.w));
    }
    public static Vector4 Max(Vector4 lhs, Vector4 rhs)
    {
        return new Vector4(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Mathf.Max(lhs.z, rhs.z), Mathf.Max(lhs.w, rhs.w));
    }
    public static Vector4 operator +(Vector4 a, Vector4 b)
    {
        return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
    }
    public static Vector4 operator -(Vector4 a, Vector4 b)
    {
        return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
    }
    public static Vector4 operator -(Vector4 a)
    {
        return new Vector4(-a.x, -a.y, -a.z, -a.w);
    }
    public static Vector4 operator *(Vector4 a, float d)
    {
        return new Vector4(a.x * d, a.y * d, a.z * d, a.w * d);
    }
    public static Vector4 operator *(float d, Vector4 a)
    {
        return new Vector4(a.x * d, a.y * d, a.z * d, a.w * d);
    }
    public static Vector4 operator /(Vector4 a, float d)
    {
        return new Vector4(a.x / d, a.y / d, a.z / d, a.w / d);
    }
    public static bool operator ==(Vector4 lhs, Vector4 rhs)
    {
        return Vector4.SqrMagnitude(lhs - rhs) < 9.99999944E-11f;
    }
    public static bool operator !=(Vector4 lhs, Vector4 rhs)
    {
        return Vector4.SqrMagnitude(lhs - rhs) >= 9.99999944E-11f;
    }
    public static implicit operator Vector4(Vector3 v)
    {
        return new Vector4(v.x, v.y, v.z, 0f);
    }
    public static implicit operator Vector3(Vector4 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
    public static implicit operator Vector4(Vector2 v)
    {
        return new Vector4(v.x, v.y, 0f, 0f);
    }
    public static implicit operator Vector2(Vector4 v)
    {
        return new Vector2(v.x, v.y);
    }
}
public class DWORD64
{
    public static readonly DWORD64 Zero = 0;
    public long Address = 0;

    public DWORD64(DWORD64 binary)
    {
        Address = binary;
    }
    public DWORD64(Int64 binary)
    {
        Address = binary;
    }
    public DWORD64(Int32 binary)
    {
        Address = Convert.ToInt64(binary);
    }
    public DWORD64(IntPtr binary)
    {
        Address = binary.ToInt64();
    }

    // Create a DWORD64 with 8 bytes from a byte array, offset if needed
    public DWORD64(byte[] binary, int offset)
    {
#if _DEBUG
        if (binary.Length < 8)
            throw new Exception("The Binary Array is smaller than 8 bytes.");

        // at offset we start reading 8 bytes for the DWORD64.
        // if offset + 8 is out of array thow a exeption.
        if (offset + 8 > binary.Length) // do we need equalent too?
            throw new Exception("The Binary Array at the given offset is out of Array.");
#else
        if (binary.Length < 8)
            Address = 0;

        if (offset + 8 > binary.Length) // do we need equalent too?
            Address = 0;
#endif
        Address = BitConverter.ToInt64(binary, offset);
    }
    public DWORD64(byte[] binary)
    {
#if _DEBUG
        if (binary.Length < 8)
            throw new Exception("The Binary Array is smaller than 8 bytes.");

        // at offset we start reading 8 bytes for the DWORD64.
        // if offset + 8 is out of array thow a exeption.
        if (8 > binary.Length) // do we need equalent too?
            throw new Exception("The Binary Array at the given offset is out of Array.");
#else
        if (binary.Length < 8)
            Address = 0;

        if (8 > binary.Length) // do we need equalent too?
            Address = 0;
#endif
        Address = BitConverter.ToInt64(binary, 0);
    }



    // long to DWORD64
    public static implicit operator DWORD64(long binary)
    {
        return new DWORD64(binary);
    }

    // DWORD64 to long
    public static implicit operator long(DWORD64 binary)
    {
        return binary.Address;
    }

    // DWORD64 to string
    public static implicit operator string(DWORD64 binary)
    {
        if (binary == null)
            return "<null>";

        if (binary == 0)
            return "0";

        if (binary < 0)
            return "0x" + ((long)binary).ToString("X");

        if (binary <= 10)
            return binary.ToString();

        return "0x" + binary.ToString();
    }

    // DWORD64 to IntPtr
    public static implicit operator IntPtr(DWORD64 binary)
    {
        return new IntPtr(binary.Address);
    }

    // IntPtr to DWORD64
    public static implicit operator DWORD64(IntPtr binary)
    {
        return new DWORD64(binary.ToInt64());
    }

    // DWORD64 to byte[]
    public static implicit operator byte[] (DWORD64 binary)
    {
        return BitConverter.GetBytes(binary);
    }


    public static DWORD64 operator +(DWORD64 a1, DWORD64 a2)
    {
        if (a1 == null)
            if (a2 == null)
                return 0;
            else
                return a2;
        else
            if (a2 == null)
            return a1;
        else
            return new DWORD64(a1.Address + a2.Address);
    }
    public static DWORD64 operator -(DWORD64 a1, DWORD64 a2)
    {
        if (a1 == null)
            if (a2 == null)
                return 0;
            else
                return a2;
        else
            if (a2 == null)
            return a1;
        else
            return new DWORD64(a1.Address - a2.Address);
    }
    public static DWORD64 operator /(DWORD64 a1, DWORD64 a2)
    {
        if (a1 == null)
            if (a2 == null)
                return 0;
            else
                return a2;
        else
            if (a2 == null)
            return a1;
        else
            return new DWORD64(a1.Address / a2.Address);
    }
    public static DWORD64 operator *(DWORD64 a1, DWORD64 a2)
    {
        if (a1 == null)
            if (a2 == null)
                return 0;
            else
                return a2;
        else
            if (a2 == null)
            return a1;
        else
            return new DWORD64(a1.Address * a2.Address);
    }

    public override string ToString()
    {
        return Address.ToString();
    }
    public string ToString(string fmt)
    {
        if (fmt == "X" || fmt == "x")
            return "0x" + Address.ToString("X12");
        return Address.ToString(fmt);
    }
}
public class DWORD
{
    public static readonly DWORD Zero = 0;
    public int Address = 0;

    public DWORD(DWORD binary)
    {
        Address = binary;
    }
    public DWORD(Int32 binary)
    {
        Address = Convert.ToInt32(binary);
    }
    public DWORD(IntPtr binary)
    {
        Address = binary.ToInt32();
    }

    // Create a DWORD64 with 8 bytes from a byte array, offset if needed
    public DWORD(byte[] binary, int offset)
    {
#if _DEBUG
        if (binary.Length < 4)
            throw new Exception("The Binary Array is smaller than 4 bytes.");

        // at offset we start reading 4 bytes for the DWORD.
        // if offset + 4 is out of array thow a exeption.
        if (offset + 4 > binary.Length) // do we need equalent too?
            throw new Exception("The Binary Array at the given offset is out of Array.");
#else
        if (binary.Length < 4)
            Address = 0;

        if (offset + 4 > binary.Length) // do we need equalent too?
            Address = 0;
#endif
        Address = BitConverter.ToInt32(binary, offset);
    }
    public DWORD(byte[] binary)
    {
#if _DEBUG
        if (binary.Length < 4)
            throw new Exception("The Binary Array is smaller than 4 bytes.");

        // at offset we start reading 4 bytes for the DWORD.
        // if offset + 4 is out of array thow a exeption.
        if (4 > binary.Length) // do we need equalent too?
            throw new Exception("The Binary Array at the given offset is out of Array.");
#else
        if (binary.Length < 4)
            Address = 0;

        if (4 > binary.Length) // do we need equalent too?
            Address = 0;
#endif
        Address = BitConverter.ToInt32(binary, 0);
    }



    // int to DWORD
    public static implicit operator DWORD(int binary)
    {
        return new DWORD(binary);
    }

    // DWORD to int
    public static implicit operator int(DWORD binary)
    {
        return binary.Address;
    }

    // DWORD to string
    public static implicit operator string(DWORD binary)
    {
        if (binary == null)
            return "<null>";

        if (binary == 0)
            return "0";

        if (binary < 0)
            return "0x" + ((int)binary).ToString("X");

        if (binary <= 10)
            return binary.ToString();

        return "0x" + binary.ToString();
    }

    // DWORD to IntPtr
    public static implicit operator IntPtr(DWORD binary)
    {
        return new IntPtr(binary.Address);
    }

    // IntPtr to DWORD
    public static implicit operator DWORD(IntPtr binary)
    {
        return new DWORD(binary.ToInt32());
    }

    // DWORD to byte[]
    public static implicit operator byte[] (DWORD binary)
    {
        return BitConverter.GetBytes(binary);
    }


    public static DWORD operator +(DWORD a1, DWORD a2)
    {
        if (a1 == null)
            if (a2 == null)
                return 0;
            else
                return a2;
        else
            if (a2 == null)
            return a1;
        else
            return new DWORD(a1.Address + a2.Address);
    }
    public static DWORD operator -(DWORD a1, DWORD a2)
    {
        if (a1 == null)
            if (a2 == null)
                return 0;
            else
                return a2;
        else
            if (a2 == null)
            return a1;
        else
            return new DWORD(a1.Address - a2.Address);
    }
    public static DWORD operator /(DWORD a1, DWORD a2)
    {
        if (a1 == null)
            if (a2 == null)
                return 0;
            else
                return a2;
        else
            if (a2 == null)
            return a1;
        else
            return new DWORD(a1.Address / a2.Address);
    }
    public static DWORD operator *(DWORD a1, DWORD a2)
    {
        if (a1 == null)
            if (a2 == null)
                return 0;
            else
                return a2;
        else
            if (a2 == null)
            return a1;
        else
            return new DWORD(a1.Address * a2.Address);
    }

    public override string ToString()
    {
        return Address.ToString();
    }
    public string ToString(string fmt)
    {
        if (fmt == "X" || fmt == "x")
            return "0x" + Address.ToString("X12");
        return Address.ToString(fmt);
    }
}
public class Utils
{
    public static string CanonicalDirectory(string path)
    {
        FileInfo info = new FileInfo(path + "." + Path.DirectorySeparatorChar);
        MakeDirectory(info.DirectoryName);
        return (info.DirectoryName + Path.DirectorySeparatorChar);
    }
    public static bool CopyFile(string sourceFileName, string destinationFileName)
    {
        return CopyFile(sourceFileName, destinationFileName, false);
    }
    public static bool CopyFile(string sourceFileName, string destinationFileName, bool smartCopy)
    {
        if (!File.Exists(sourceFileName))
        {
            if (smartCopy)
            {
                DeleteFile(destinationFileName, false);
            }
            return false;
        }
        FileInfo info = new FileInfo(sourceFileName);
        if (smartCopy)
        {
            FileInfo info2 = new FileInfo(destinationFileName);
            if (((info.Exists && info2.Exists) && ((info.CreationTime == info2.CreationTime) && (info.LastWriteTime == info2.LastWriteTime))) && (info.Length == info2.Length))
            {
                return true;
            }
        }
        //          WriteMessage("Copying  " + sourceFileName + "\n     to  " + destinationFileName + "\n");
        if (!DeleteFile(destinationFileName, false))
        {
            return false;
        }
        MakeDirectory(Path.GetDirectoryName(destinationFileName));
        try
        {
            File.Copy(sourceFileName, destinationFileName);
            if (smartCopy)
            {
                File.SetCreationTime(destinationFileName, info.CreationTime);
                File.SetLastWriteTime(destinationFileName, info.LastWriteTime);
            }
        }
        catch (Exception exception)
        {
            //              WriteError("ERROR: " + exception.Message + "\n");
            return false;
        }
        return true;
    }
    public static bool CopyFileSmart(string sourceFileName, string destinationFileName)
    {
        return CopyFile(sourceFileName, destinationFileName, true);
    }
    public static bool DeleteFile(string fileName)
    {
        return DeleteFile(fileName, true);
    }
    public static bool DeleteFile(string fileName, bool verbose)
    {
        if (File.Exists(fileName))
        {
            if (verbose)
            {
                //                  WriteMessage("Deleting " + fileName + "\n");
            }
            try
            {
                File.SetAttributes(fileName, FileAttributes.Normal);
                File.Delete(fileName);
            }
            catch (Exception exception)
            {
                if (verbose)
                {
                    //                      WriteError("ERROR: " + exception.Message + "\n");
                }
                return false;
            }
        }
        return true;
    }
    public static string[] GetDirs(string directory)
    {
        string[] stringArray = new string[0];
        foreach (DirectoryInfo info in new DirectoryInfo(directory).GetDirectories())
            StringArrayAdd(ref stringArray, info.Name);
        return stringArray;
    }
    public static string[] GetFiles(string directory, string searchFilter)
    {
        string[] stringArray = new string[0];
        foreach (FileInfo info in new DirectoryInfo(directory).GetFiles(searchFilter))
        {
            StringArrayAdd(ref stringArray, Path.GetFileName(info.Name));
        }
        return stringArray;
    }
    public static string[] GetFilesRecursively(string directory)
    {
        return GetFilesRecursively(directory, "*");
    }
    public static string[] GetFilesRecursively(string directory, string filesToIncludeFilter)
    {
        string[] files = new string[0];
        GetFilesRecursively(directory, filesToIncludeFilter, ref files);
        return files;
    }
    public static void GetFilesRecursively(string directory, string filesToIncludeFilter, ref string[] files)
    {
        foreach (DirectoryInfo info in new DirectoryInfo(directory).GetDirectories())
        {
            GetFilesRecursively(Path.Combine(directory, info.Name), filesToIncludeFilter, ref files);
        }
        foreach (FileInfo info2 in new DirectoryInfo(directory).GetFiles(filesToIncludeFilter))
        {
            StringArrayAdd(ref files, Path.Combine(directory, info2.Name.ToLower()));
        }
    }
    public static string[] GetFilesWithoutExtension(string directory, string searchFilter)
    {
        string[] stringArray = new string[0];
        foreach (FileInfo info in new DirectoryInfo(directory).GetFiles(searchFilter))
        {
            StringArrayAdd(ref stringArray, Path.GetFileNameWithoutExtension(info.Name));
        }
        return stringArray;
    }
    private static string GetLocalApplicationDirectory()
    {
        return GetRootDirectory();
    }
    public static string GetRootDirectory()
    {
        return CanonicalDirectory(Path.Combine(GetStartupDirectory(), ".."));
    }
    //public static string GetStartupDirectory()
    //{
    //    string path = use_exedir_as_startupdir ? Path.GetDirectoryName(/*Application.ExecutablePath*/a1) : Path.GetFullPath(".");
    //    Directory.SetCurrentDirectory(path);
    //    return CanonicalDirectory(path);
    //}
    public static string GetStartupDirectory()
    {
        string path = Path.GetFullPath(".");
        Directory.SetCurrentDirectory(path);
        return CanonicalDirectory(path);
    }
    public static string[] HashTableToStringArray(Hashtable hashTable)
    {
        int num = 0;
        string[] array = new string[hashTable.Count];
        foreach (DictionaryEntry entry in hashTable)
        {
            array[num++] = entry.Key + ((entry.Value != null) ? ("," + entry.Value) : "");
        }
        Array.Sort<string>(array);
        return array;
    }
    public static string[] LoadTextFile(string textFile)
    {
        return LoadTextFile(textFile, null);
    }
    public static string[] LoadTextFile(string textFile, string skipCommentLinesStartingWith)
    {
        string[] stringArray = new string[0];
        try
        {
            using (StreamReader reader = new StreamReader(textFile))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    str.Trim();
                    if ((str != "") && ((skipCommentLinesStartingWith == null) || !str.StartsWith(skipCommentLinesStartingWith)))
                    {
                        StringArrayAdd(ref stringArray, str);
                    }
                }
            }
        }
        catch
        {
        }
        return stringArray;
    }
    public static void MakeDirectory(string directoryName)
    {
        while (!Directory.Exists(directoryName))
        {
            string str = Path.GetDirectoryName(directoryName);
            if (str != directoryName)
            {
                MakeDirectory(str);
            }
            Directory.CreateDirectory(directoryName);
        }
    }
    public static bool MoveFile(string sourceFileName, string destinationFileName)
    {
        if (!File.Exists(sourceFileName))
        {
            return false;
        }
        if (!DeleteFile(destinationFileName, false))
        {
            return false;
        }
        MakeDirectory(Path.GetDirectoryName(destinationFileName));
        try
        {
            File.Move(sourceFileName, destinationFileName);
        }
        catch (Exception exception)
        {

        }
        return true;
    }
    public static void SaveTextFile(string textFile, string[] text)
    {
        using (StreamWriter writer = new StreamWriter(textFile))
        {
            foreach (string str in text)
            {
                writer.WriteLine(str);
            }
        }
    }
    public static void StringArrayAdd(ref string[] stringArray, string stringItem)
    {
        Array.Resize<string>(ref stringArray, stringArray.Length + 1);
        stringArray[stringArray.Length - 1] = stringItem;
    }
    public static Hashtable StringArrayToHashTable(string[] stringArray)
    {
        Hashtable hashtable = new Hashtable(stringArray.Length);
        foreach (string str in stringArray)
        {
            string[] strArray = str.Split(new char[] { ',' });
            if (strArray.Length > 0)
            {
                hashtable.Add(strArray[0], (strArray.Length > 1) ? strArray[1] : null);
            }
        }
        return hashtable;
    }
    public static string StringArrayToString(string[] stringArray)
    {
        StringBuilder builder = new StringBuilder();
        foreach (string str in stringArray)
        {
            builder.Append(str).AppendLine();
        }
        return builder.ToString();
    }
    public static T[] MergeArrays<T>(T[] first, T[] second)
    {
        T[] result = new T[first.Length + second.Length];
        Array.Copy(first, result, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);
        return result;
    }
    public static byte[] GetBytes(string str)
    {
        byte[] bytes = new byte[str.Length * sizeof(char)];
        Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }
    public static string GetString(byte[] bytes)
    {
        char[] chars = new char[bytes.Length / sizeof(char)];
        Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
        return new string(chars);
    }

    // Struct to byte[].
    public static byte[] SerializeMessage<T>(T msg) where T : struct
    {
        int objsize = Marshal.SizeOf(typeof(T));
        byte[] ret = new byte[objsize];
        IntPtr buff = Marshal.AllocHGlobal(objsize);
        Marshal.StructureToPtr(msg, buff, true);
        Marshal.Copy(buff, ret, 0, objsize);
        Marshal.FreeHGlobal(buff);
        return ret;
    }

    // byte[] to Struct.
    public static T DeserializeMsg<T>(byte[] data) where T : struct
    {
        int objsize = Marshal.SizeOf(typeof(T));
        IntPtr buff = Marshal.AllocHGlobal(objsize);
        Marshal.Copy(data, 0, buff, objsize);
        T retStruct = (T)Marshal.PtrToStructure(buff, typeof(T));
        Marshal.FreeHGlobal(buff);
        return retStruct;
    }

    string[] FilteredSplit(string strIn, char[] separator)
    {
        List<string> listOfValues = new List<string>();
        string[] valuesUnfiltered = strIn.Split(separator);
        foreach (string str in valuesUnfiltered)
            if (str != "")
                listOfValues.Add(str);
        return listOfValues.ToArray();
    }

    // To ReWrite!
    public static bool ToBool(object Value)
    {
        if (Value.GetType() == typeof(string))
            if ((string)Value == "True" || (string)Value == "true")
                return true;
            else
                return false;

        if (Value.GetType() == typeof(int))
            if ((int)Value == 1)
                return true;
            else
                return false;

        return false;
    }
}




enum ConnectionState_t
{
    CONNECTED,
    FAILED,
    NOPROCESS,
};
public class MemoryClass
{
    #region DllImport
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateEvent(IntPtr lpEventAttributes, int bManualReset, int bInitialState, string lpName);

    [DllImport("kernel32")]
    private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, [Out] uint lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, [Out] int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern int VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

    [DllImport("kernel32.dll")]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, [Out] int lpNumberOfBytesWritten);

    [DllImport("user32.dll")]
    public static extern int GetKeyState(VirtualKeyStates vKey);
    #endregion
    #region Procedure Call
    /// <summary>
    /// Win32 ASM Call
    /// </summary>
    /// <param name="CallAdress"></param>
    /// <param name="Parameters"></param>
    /// <returns></returns>
#if _WIN32
public DWORD Call(DWORD CallAdress, params object[] Parameters)
{
    DWORD ReturnAddr = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)4, 0x3000, 0x40);
    DWORD AllocAddr = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)0x200, 0x3000, 0x40);

    int line = 0;
    WriteBytes(AllocAddr + line, WrapperToCallx86, ref line);


    if (Parameters.Length != 0)
    {
        for (int i = Parameters.Length; i-- > 0;)
        {
            if (Parameters[i].GetType() == typeof(string))
            {
                DWORD _String = Malloc(((string)Parameters[i]).Length + 1);
                WriteString(_String, (string)Parameters[i], true);

                WriteByte(AllocAddr + line, 0x68, ref line);
                WriteInt32(AllocAddr + line, _String, ref line);
            }
            if (Parameters[i].GetType() == typeof(float))
            {
                DWORD _Float = Malloc(4);
                WriteFloat(_Float, (float)Parameters[i]);

                WriteByte(AllocAddr + line, 0x68, ref line);
                WriteInt32(AllocAddr + line, _Float, ref line);
            }
            if (Parameters[i].GetType() == typeof(int))
            {
                if((int)Parameters[i] < 0x80)
                {
                    WriteByte(AllocAddr + line, 0x6A, ref line);
                    WriteByte(AllocAddr + line, BitConverter.GetBytes((int)Parameters[i])[0], ref line);
                }
                else
                {
                    WriteByte(AllocAddr + line, 0x68, ref line);
                    WriteInt32(AllocAddr + line, (int)Parameters[i], ref line);
                }
            }
            if (Parameters[i].GetType() == typeof(bool))
            {
                WriteByte(AllocAddr + line, 0x6A, ref line);
                WriteBool(AllocAddr + line + 1, (bool)Parameters[i], ref line);
            }
        }
    }

    WriteByte(AllocAddr + line, 0xB8, ref line);//0xB8 -> mov eax
    WriteInt32(AllocAddr + line, CallAdress, ref line);//mov eax, -> call address
    WriteBytes(AllocAddr + line, new byte[] { 0xFF, 0xD0 }, ref line);//call eax


    // if wrapper to call end get changed we need to change the ret address (- 8...)
    WriteBytes(AllocAddr + line, WrapperToCallEndx86, ref line);
    WriteInt32(AllocAddr + line - 8, ReturnAddr);


    CreateRemoteThread(AllocAddr);
    System.Threading.Thread.Sleep(100);
    return ReadInt32(ReturnAddr);
}
private byte[] WrapperToCallx86 = new byte[]
{
    0x55, // push ebp
	0x8B, 0xEC, // mov ebp, esp
	0x83, 0xEC, 0x08, // sub esp, 8
};
private byte[] WrapperToCallEndx86 = new byte[]
{
    0x83, 0xC4, 0x08, // add esp, 8
    0xA3, 0xFC, 0xFF, 0x04, 0x30,//mov [...],eax
    0x8B, 0xE5, // mov esp, ebp
    0x5D, // pop ebp
    0xC3 // retn
};
#endif
#if _WIN64
    // maybe add this as parameter adding  would be a better idear?
    //.text:00000001 80 15 DA 1B                 mov     dword ptr [rsp+28h], 1
    //.text:00000001 80 15 DA 23                 mov     dword ptr [rsp+20h], 1
    public DWORD64 Call(DWORD64 CallAdress, params object[] Parameters)
    {
        DWORD ReturnAddr = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)4, 0x3000, 0x40);
        DWORD AllocAddr = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)0x200, 0x3000, 0x40);
        Log.Debug("", AllocAddr.ToString("X"));

        int line = 0;
        WriteBytes(AllocAddr + line, WrapperToCallx64, ref line);


        if (Parameters.Length != 0)
        {
            for (int i = Parameters.Length; i-- > 0;)
            {
                if (Parameters[i].GetType() == typeof(string))
                {
                    DWORD _String = Malloc(((string)Parameters[i]).Length + 1);
                    WriteString(_String, (string)Parameters[i], true);

                    WriteByte(AllocAddr + line, 0x68, ref line);
                    WriteInt32(AllocAddr + line, _String, ref line);
                }
            }
        }

        WriteByte(AllocAddr + line, 0x48, ref line);
        WriteByte(AllocAddr + line, 0xB8, ref line);
        WriteInt64(AllocAddr + line, CallAdress.Address, ref line);
        WriteBytes(AllocAddr + line, new byte[] { 0xFF, 0xD0 }, ref line);//call eax


        // if wrapper to call end get changed we need to change the ret address (- 8...)
        WriteBytes(AllocAddr + line, WrapperToCallEndx64, ref line);
        WriteInt32(AllocAddr + line - 9, ReturnAddr);


        CreateRemoteThread(AllocAddr);
        System.Threading.Thread.Sleep(100);
        return ReadInt32(ReturnAddr);
    }
    private byte[] WrapperToCallx64 = new byte[]
    {
    0x55,                     									    // push rbp
    0x48, 0x8B, 0xEC,               								// mov rbp, rsp
    0x48, 0x83, 0xEC, 0x08,                                         // sub rsp, 08
    };
    private byte[] WrapperToCallEndx64 = new byte[]
    {
    0x48, 0x83, 0xC4, 0x08,            							    // add rsp, 08
    0x48, 0x89, 0x04, 0x25, 0x00, 0x00, 0x00, 0x00,   			    // mov [...], rax
    0x48, 0x8B, 0xE5,               							    // mov rsp, rbp
    0x5D,                     									    // pop rbp
    0xC3                                                            // ret
    };
#endif
    #endregion Procedure Call

    private IntPtr ProcessHandle = IntPtr.Zero;

    private ConnectionState_t m_ConnectionState;
    private string m_ProcessName;
    private Process m_Process;


    public List<Process> FindProcesses(params string[] pcs)
    {
        List<Process> a1 = new List<Process>();
        for (int i = 0; i < pcs.Length; i++)
            a1.AddRange(Process.GetProcessesByName(pcs[i]));
        return a1;
    }

    /// <summary>
    /// Are we Connected to a Game?
    /// </summary>
    public bool isConnected { get { return m_ConnectionState == ConnectionState_t.CONNECTED; } }
    public Process GetProcess { get { return m_Process; } }
    public string GetProcessName { get { return m_ProcessName; } }


    private ConnectionState_t TryConnect(params string[] Processes)
    {
        List<Process> pcs = FindProcesses(Processes);
        foreach (Process p in pcs)
        {
            if (OpenProcess(p))
            {
                m_Process = p;
                m_ProcessName = p.ProcessName;
                m_ConnectionState = ConnectionState_t.CONNECTED;
                break;
            }
            else
                m_ConnectionState = ConnectionState_t.FAILED;
        }
        if (pcs.Count <= 0)
            m_ConnectionState = ConnectionState_t.NOPROCESS;
        return m_ConnectionState;
    }
    public bool TryConnect(Action Connected, Action Failed, Action NoProcess, params string[] Processes)
    {
        ConnectionState_t CS = TryConnect(Processes);
        if (CS == ConnectionState_t.CONNECTED)
            Connected?.Invoke();
        else if (CS == ConnectionState_t.FAILED)
            Failed?.Invoke();
        else if (CS == ConnectionState_t.NOPROCESS)
            NoProcess?.Invoke();
        return (m_ConnectionState == ConnectionState_t.CONNECTED) ? true : false;
    }





    #region Reading
    // x86
    public byte[] ReadBytes(DWORD Address, int Length = 4)
    {
        byte[] Readed = new byte[Length];
        ReadProcessMemory(ProcessHandle, Address, Readed, Length, 0);
        return Readed;
    }
    public byte ReadByte(DWORD Address)
    {
        return ReadBytes(Address, 1)[0];
    }
    public string ReadString(DWORD Address)
    {
        StringBuilder sb = new StringBuilder();
        while (ReadByte(Address) != 0)
        {
            sb.Append(Convert.ToChar(ReadByte(Address)));
            Address++;
        }
        return sb.ToString();
    }
    public string ReadString(DWORD Address, int Length)
    {
        byte[] lpBuffer = ReadBytes(Address, Length);
        return new ASCIIEncoding().GetString(lpBuffer);
    }
    public float ReadFloat(DWORD Address)
    {
        return BitConverter.ToSingle(ReadBytes(Address, 4), 0);
    }
    public bool ReadBool(DWORD Address)
    {
        return ReadByte(Address) == 1 ? true : false;
    }
    public Vector4 ReadVector4(DWORD Address)
    {
        return new Vector4(ReadFloat(Address), ReadFloat(Address + 4), ReadFloat(Address + 8), ReadFloat(Address + 12));
    }
    public Vector3 ReadVector3(DWORD Address)
    {
        return new Vector3(ReadFloat(Address), ReadFloat(Address + 4), ReadFloat(Address + 8));
    }
    public Vector2 ReadVector2(DWORD Address)
    {
        return new Vector2(ReadFloat(Address), ReadFloat(Address + 4));
    }
    public short ReadInt16(DWORD Address)
    {
        return BitConverter.ToInt16(ReadBytes(Address, 2), 0);
    }
    public int ReadInt32(DWORD Address)
    {
        return BitConverter.ToInt32(ReadBytes(Address, 4), 0);
    }
    public long ReadInt64(DWORD Address)
    {
        return BitConverter.ToInt64(ReadBytes(Address, 8), 0);
    }
    public ushort ReadUInt16(DWORD Address)
    {
        return BitConverter.ToUInt16(ReadBytes(Address, 2), 0);
    }
    public uint ReadUInt32(DWORD Address)
    {
        return BitConverter.ToUInt32(ReadBytes(Address, 4), 0);
    }
    public ulong ReadUInt64(DWORD Address)
    {
        return BitConverter.ToUInt64(ReadBytes(Address, 8), 0);
    }


    public byte[] ReadBytes(DWORD Address, ref int Count, int Length = 4)
    {
        byte[] Readed = new byte[Length];
        ReadProcessMemory(ProcessHandle, Address, Readed, Length, 0);
        Count += Readed.Length;
        return Readed;
    }
    public byte ReadByte(DWORD Address, ref int Count)
    {
        return ReadBytes(Address, ref Count, 1)[0];
    }
    public string ReadString(DWORD Address, ref int Count)
    {
        StringBuilder sb = new StringBuilder();
        while (ReadByte(Address, ref Count) != 0)
        {
            sb.Append(Convert.ToChar(ReadByte(Address)));
            Address++;
        }
        return sb.ToString();
    }
    public string ReadString(DWORD Address, int Length, ref int Count)
    {
        byte[] lpBuffer = ReadBytes(Address, ref Count, Length);
        return new ASCIIEncoding().GetString(lpBuffer);
    }
    public float ReadFloat(DWORD Address, ref int Count)
    {
        return BitConverter.ToSingle(ReadBytes(Address, ref Count, 4), 0);
    }
    public bool ReadBool(DWORD Address, ref int Count)
    {
        return ReadByte(Address, ref Count) == 1 ? true : false;
    }
    public Vector4 ReadVector4(DWORD Address, ref int Count)
    {
        return new Vector4(ReadFloat(Address, ref Count), ReadFloat(Address + 4, ref Count), ReadFloat(Address + 8, ref Count), ReadFloat(Address + 12, ref Count));
    }
    public Vector3 ReadVector3(DWORD Address, ref int Count)
    {
        return new Vector3(ReadFloat(Address, ref Count), ReadFloat(Address + 4, ref Count), ReadFloat(Address + 8, ref Count));
    }
    public Vector2 ReadVector2(DWORD Address, ref int Count)
    {
        return new Vector2(ReadFloat(Address, ref Count), ReadFloat(Address + 4, ref Count));
    }
    public short ReadInt16(DWORD Address, ref int Count)
    {
        return BitConverter.ToInt16(ReadBytes(Address, ref Count, 2), 0);
    }
    public int ReadInt32(DWORD Address, ref int Count)
    {
        return BitConverter.ToInt32(ReadBytes(Address, ref Count, 4), 0);
    }
    public long ReadInt64(DWORD Address, ref int Count)
    {
        return BitConverter.ToInt64(ReadBytes(Address, ref Count, 8), 0);
    }
    public ushort ReadUInt16(DWORD Address, ref int Count)
    {
        return BitConverter.ToUInt16(ReadBytes(Address, ref Count, 2), 0);
    }
    public uint ReadUInt32(DWORD Address, ref int Count)
    {
        return BitConverter.ToUInt32(ReadBytes(Address, ref Count, 4), 0);
    }
    public ulong ReadUInt64(DWORD Address, ref int Count)
    {
        return BitConverter.ToUInt64(ReadBytes(Address, ref Count, 8), 0);
    }




    //x64
#if _WIN64
    public byte[] ReadBytes(DWORD64 Address, int Length = 4)
    {
        IntPtr a = new IntPtr(Address.Address);
        byte[] Readed = new byte[Length];
        ReadProcessMemory(ProcessHandle, a, Readed, Length, 0);
        return Readed;
    }
    public byte ReadByte(DWORD64 Address)
    {
        return ReadBytes(Address, 1)[0];
    }
    public string ReadString(DWORD64 Address)
    {
        StringBuilder sb = new StringBuilder();
        while (ReadByte(Address) != 0)
        {
            sb.Append(Convert.ToChar(ReadByte(Address)));
            Address++;
        }
        return sb.ToString();
    }
    public string ReadString(DWORD64 Address, int Length)
    {
        byte[] lpBuffer = ReadBytes(Address, Length);
        return new ASCIIEncoding().GetString(lpBuffer);
    }
    public float ReadFloat(DWORD64 Address)
    {
        return BitConverter.ToSingle(ReadBytes(Address, 4), 0);
    }
    public bool ReadBool(DWORD64 Address)
    {
        return ReadByte(Address) == 1 ? true : false;
    }
    public Vector4 ReadVector4(DWORD64 Address)
    {
        return new Vector4(ReadFloat(Address), ReadFloat(Address + 4), ReadFloat(Address + 8), ReadFloat(Address + 12));
    }
    public Vector3 ReadVector3(DWORD64 Address)
    {
        return new Vector3(ReadFloat(Address), ReadFloat(Address + 4), ReadFloat(Address + 8));
    }
    public Vector2 ReadVector2(DWORD64 Address)
    {
        return new Vector2(ReadFloat(Address), ReadFloat(Address + 4));
    }
    public short ReadInt16(DWORD64 Address)
    {
        return BitConverter.ToInt16(ReadBytes(Address, 2), 0);
    }
    public int ReadInt32(DWORD64 Address)
    {
        return BitConverter.ToInt32(ReadBytes(Address, 4), 0);
    }
    public long ReadInt64(DWORD64 Address)
    {
        return BitConverter.ToInt64(ReadBytes(Address, 8), 0);
    }
    public ushort ReadUInt16(DWORD64 Address)
    {
        return BitConverter.ToUInt16(ReadBytes(Address, 2), 0);
    }
    public uint ReadUInt32(DWORD64 Address)
    {
        return BitConverter.ToUInt32(ReadBytes(Address, 4), 0);
    }
    public ulong ReadUInt64(DWORD64 Address)
    {
        return BitConverter.ToUInt64(ReadBytes(Address, 8), 0);
    }


    public byte[] ReadBytes(DWORD64 Address, ref int Count, int Length = 4)
    {
        byte[] Readed = new byte[Length];
        ReadProcessMemory(ProcessHandle, Address, Readed, Length, 0);
        Count += Readed.Length;
        return Readed;
    }
    public byte ReadByte(DWORD64 Address, ref int Count)
    {
        return ReadBytes(Address, ref Count, 1)[0];
    }
    public string ReadString(DWORD64 Address, ref int Count)
    {
        StringBuilder sb = new StringBuilder();
        while (ReadByte(Address, ref Count) != 0)
        {
            sb.Append(Convert.ToChar(ReadByte(Address)));
            Address++;
        }
        return sb.ToString();
    }
    public string ReadString(DWORD64 Address, int Length, ref int Count)
    {
        byte[] lpBuffer = ReadBytes(Address, ref Count, Length);
        return new ASCIIEncoding().GetString(lpBuffer);
    }
    public float ReadFloat(DWORD64 Address, ref int Count)
    {
        return BitConverter.ToSingle(ReadBytes(Address, ref Count, 4), 0);
    }
    public bool ReadBool(DWORD64 Address, ref int Count)
    {
        return ReadByte(Address, ref Count) == 1 ? true : false;
    }
    public Vector4 ReadVector4(DWORD64 Address, ref int Count)
    {
        return new Vector4(ReadFloat(Address, ref Count), ReadFloat(Address + 4, ref Count), ReadFloat(Address + 8, ref Count), ReadFloat(Address + 12, ref Count));
    }
    public Vector3 ReadVector3(DWORD64 Address, ref int Count)
    {
        return new Vector3(ReadFloat(Address, ref Count), ReadFloat(Address + 4, ref Count), ReadFloat(Address + 8, ref Count));
    }
    public Vector2 ReadVector2(DWORD64 Address, ref int Count)
    {
        return new Vector2(ReadFloat(Address, ref Count), ReadFloat(Address + 4, ref Count));
    }
    public short ReadInt16(DWORD64 Address, ref int Count)
    {
        return BitConverter.ToInt16(ReadBytes(Address, ref Count, 2), 0);
    }
    public int ReadInt32(DWORD64 Address, ref int Count)
    {
        return BitConverter.ToInt32(ReadBytes(Address, ref Count, 4), 0);
    }
    public long ReadInt64(DWORD64 Address, ref int Count)
    {
        return BitConverter.ToInt64(ReadBytes(Address, ref Count, 8), 0);
    }
    public ushort ReadUInt16(DWORD64 Address, ref int Count)
    {
        return BitConverter.ToUInt16(ReadBytes(Address, ref Count, 2), 0);
    }
    public uint ReadUInt32(DWORD64 Address, ref int Count)
    {
        return BitConverter.ToUInt32(ReadBytes(Address, ref Count, 4), 0);
    }
    public ulong ReadUInt64(DWORD64 Address, ref int Count)
    {
        return BitConverter.ToUInt64(ReadBytes(Address, ref Count, 8), 0);
    }
#endif
    #endregion
    #region Writing

    // x86
    public void WriteBytes(DWORD Address, byte[] buffer)
    {
        if (ProcessHandle == IntPtr.Zero)
            throw new ArgumentNullException("Process is null");

        WriteProcessMemory(ProcessHandle, (IntPtr)Address, buffer, (uint)buffer.Length, 0);
    }
    public void WriteByte(DWORD Address, byte bytes)
    {
        WriteBytes(Address, new byte[] { bytes });
    }
    public void WriteString(DWORD Address, string Text, bool ZeroTerminating = true)
    {
        byte[] b = Encoding.UTF8.GetBytes(Text);
        if (ZeroTerminating)
        {
            Array.Resize(ref b, b.Length + 1);
            b[b.Length - 1] = 0x00;
        }
        WriteBytes(Address, b);
    }
    public void WriteFloat(DWORD Address, float value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteBool(DWORD Address, bool value)
    {
        WriteByte(Address, (value ? (byte)1 : (byte)0));
    }
    public void WriteVector4(DWORD Address, Vector4 value)
    {
        WriteFloat(Address, value.x);
        WriteFloat(Address + 0x04, value.y);
        WriteFloat(Address + 0x08, value.z);
        WriteFloat(Address + 0x0C, value.w);
    }
    public void WriteVector3(DWORD Address, Vector3 value)
    {
        WriteFloat(Address, value.x);
        WriteFloat(Address + 0x04, value.y);
        WriteFloat(Address + 0x08, value.y);
    }
    public void WriteVector2(DWORD Address, Vector2 value)
    {
        WriteFloat(Address, value.x);
        WriteFloat(Address + 0x04, value.y);
    }
    public void WriteInt16(DWORD Address, short value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteInt32(DWORD Address, int value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteInt64(DWORD Address, long value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteUInt16(DWORD Address, short value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteUInt32(DWORD Address, uint value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteUInt64(DWORD Address, ulong value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }



    public void WriteBytes(DWORD Address, byte[] buffer, ref int Count)
    {
        WriteBytes(Address, buffer);
        Count += buffer.Length;
    }
    public void WriteByte(DWORD Address, byte bytes, ref int Count)
    {
        WriteByte(Address, bytes);
        Count += 1;
    }
    public void WriteString(DWORD Address, string Text, ref int Count, bool ZeroTerminating = true)
    {
        WriteString(Address, Text, ZeroTerminating);
        Count += Text.Length;

        if (ZeroTerminating)
            Count++;
    }
    public void WriteFloat(DWORD Address, float value, ref int Count)
    {
        WriteFloat(Address, value);
        Count += 4;
    }
    public void WriteBool(DWORD Address, bool value, ref int Count)
    {
        WriteBool(Address, value);
        Count += 1;
    }
    public void WriteVector4(DWORD Address, Vector4 value, ref int Count)
    {
        WriteVector4(Address, value);
        Count += 16;
    }
    public void WriteVector3(DWORD Address, Vector3 value, ref int Count)
    {
        WriteVector3(Address, value);
        Count += 12;
    }
    public void WriteVector2(DWORD Address, Vector2 value, ref int Count)
    {
        WriteVector2(Address, value);
        Count += 8;
    }
    public void WriteInt16(DWORD Address, short value, ref int Count)
    {
        WriteInt16(Address, value);
        Count += 2;
    }
    public void WriteInt32(DWORD Address, int value, ref int Count)
    {
        WriteInt32(Address, value);
        Count += 4;
    }
    public void WriteInt64(DWORD Address, long value, ref int Count)
    {
        WriteInt64(Address, value);
        Count += 8;
    }
    public void WriteUInt16(DWORD Address, short value, ref int Count)
    {
        WriteUInt16(Address, value);
        Count += 2;
    }
    public void WriteUInt32(DWORD Address, uint value, ref int Count)
    {
        WriteUInt32(Address, value);
        Count += 4;
    }
    public void WriteUInt64(DWORD Address, ulong value, ref int Count)
    {
        WriteUInt64(Address, value);
        Count += 8;
    }






    // 64 bit addresses
#if _WIN64
    public void WriteBytes(DWORD64 Address, byte[] buffer)
    {
        if (ProcessHandle == IntPtr.Zero)
            throw new ArgumentNullException("Process is null");

        WriteProcessMemory(ProcessHandle, (IntPtr)Address, buffer, (uint)buffer.Length, 0);
    }
    public void WriteByte(DWORD64 Address, byte bytes)
    {
        WriteBytes(Address, new byte[] { bytes });
    }
    public void WriteString(DWORD64 Address, string Text, bool ZeroTerminating = true)
    {
        byte[] b = Encoding.UTF8.GetBytes(Text);
        if (ZeroTerminating)
        {
            Array.Resize(ref b, b.Length + 1);
            b[b.Length - 1] = 0x00;
        }
        WriteBytes(Address, b);
    }
    public void WriteFloat(DWORD64 Address, float value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteBool(DWORD64 Address, bool value)
    {
        WriteByte(Address, (value ? (byte)1 : (byte)0));
    }
    public void WriteVector4(DWORD64 Address, Vector4 value)
    {
        WriteFloat(Address, value.x);
        WriteFloat(Address + 0x04, value.y);
        WriteFloat(Address + 0x08, value.z);
        WriteFloat(Address + 0x0C, value.w);
    }
    public void WriteVector3(DWORD64 Address, Vector3 value)
    {
        WriteFloat(Address, value.x);
        WriteFloat(Address + 0x04, value.y);
        WriteFloat(Address + 0x08, value.y);
    }
    public void WriteVector2(DWORD64 Address, Vector2 value)
    {
        WriteFloat(Address, value.x);
        WriteFloat(Address + 0x04, value.y);
    }
    public void WriteInt16(DWORD64 Address, short value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteInt32(DWORD64 Address, int value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteInt64(DWORD64 Address, long value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteUInt16(DWORD64 Address, short value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteUInt32(DWORD64 Address, uint value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }
    public void WriteUInt64(DWORD64 Address, ulong value)
    {
        WriteBytes(Address, BitConverter.GetBytes(value));
    }


    public void WriteBytes(DWORD64 Address, byte[] buffer, ref int Count)
    {
        WriteBytes(Address, buffer);
        Count += buffer.Length;
    }
    public void WriteByte(DWORD64 Address, byte bytes, ref int Count)
    {
        WriteByte(Address, bytes);
        Count += 1;
    }
    public void WriteString(DWORD64 Address, string Text, ref int Count, bool ZeroTerminating = true)
    {
        WriteString(Address, Text, ZeroTerminating);
        Count += Text.Length;

        if (ZeroTerminating)
            Count++;
    }
    public void WriteFloat(DWORD64 Address, float value, ref int Count)
    {
        WriteFloat(Address, value);
        Count += 4;
    }
    public void WriteBool(DWORD64 Address, bool value, ref int Count)
    {
        WriteBool(Address, value);
        Count += 1;
    }
    public void WriteVector4(DWORD64 Address, Vector4 value, ref int Count)
    {
        WriteVector4(Address, value);
        Count += 16;
    }
    public void WriteVector3(DWORD64 Address, Vector3 value, ref int Count)
    {
        WriteVector3(Address, value);
        Count += 12;
    }
    public void WriteVector2(DWORD64 Address, Vector2 value, ref int Count)
    {
        WriteVector2(Address, value);
        Count += 8;
    }
    public void WriteInt16(DWORD64 Address, short value, ref int Count)
    {
        WriteInt16(Address, value);
        Count += 2;
    }
    public void WriteInt32(DWORD64 Address, int value, ref int Count)
    {
        WriteInt32(Address, value);
        Count += 4;
    }
    public void WriteInt64(DWORD64 Address, long value, ref int Count)
    {
        WriteInt64(Address, value);
        Count += 8;
    }
    public void WriteUInt16(DWORD64 Address, short value, ref int Count)
    {
        WriteUInt16(Address, value);
        Count += 2;
    }
    public void WriteUInt32(DWORD64 Address, uint value, ref int Count)
    {
        WriteUInt32(Address, value);
        Count += 4;
    }
    public void WriteUInt64(DWORD64 Address, ulong value, ref int Count)
    {
        WriteUInt64(Address, value);
        Count += 8;
    }
#endif
    #endregion Writing
    #region Other
    public DWORD GetApiAddress(string lib, string func)
    {
        IntPtr libPtr = LoadLibrary(lib);
        if (libPtr == IntPtr.Zero)
            return new DWORD(0);
        return GetProcAddress(libPtr, func).ToInt32();
    }
    public DWORD FindPattern(DWORD startAddress, DWORD endAddress, string pattern, string mask)
    {
        var buffer = ReadBytes(startAddress, endAddress - startAddress);
        for (int i = 0; i < buffer.Length; i++)
        {
            for (int x = 0; x < pattern.Length; x++)
            {
                if (buffer[i + x] == pattern[x] || mask[x] == '?')
                {
                    if (x == pattern.Length - 1)
                        return startAddress + i;
                    continue;
                }
                break;
            }
        }
        return -1;
    }

    // fix base address
    public DWORD FindPattern(string pattern, string mask)
    {
        return FindPattern(m_Process.MainModule.BaseAddress, 0x900000, pattern, mask);
    }
    public DWORD[] FindPatterns(DWORD startAddress, DWORD endAddress, string pattern, string mask)
    {
        List<DWORD> addresses = new List<DWORD>();
        var buffer = ReadBytes(startAddress, endAddress - startAddress);
        for (int i = 0; i < buffer.Length; i++)
        {
            for (int x = 0; x < pattern.Length; x++)
            {
                if (buffer[i + x] == pattern[x] || mask[x] == '?')
                {
                    if (x == pattern.Length - 1)
                        addresses.Add(startAddress + i);
                    continue;
                }
                break;
            }
        }
        return addresses.ToArray();
    }
    public DWORD[] FindPatterns(string pattern, string mask)
    {
        return FindPatterns(m_Process.MainModule.BaseAddress.ToInt32(), 0x900000, pattern, mask);
    }


    public void QCALL(DWORD Address, DWORD Data, bool jump)
    {
        WriteByte(Address, jump ? (byte)0xE9 : (byte)0xE8);
        WriteInt32(Address + 1, Data - Address - 5);
    }
    public void QNOP(DWORD Address, int Length)
    {
        for (int i = 0; i < Length; i++)
            WriteByte(Address + i, 0x90);
    }


    /// <summary>
    /// Read Pointer From address + offset and add to the readen Address correct
    /// </summary>
    /// <param name="address"></param>
    /// <param name="offset"></param>
    /// <param name="correct"></param>
    /// <returns></returns>
    public DWORD FixPointer(DWORD address, int offset, int correct)
    {
        return ReadInt32(address + offset) + correct;
    }

    /// <summary>
    ///  Reads from Address + offset the address (5 bytes with opcode)
    /// </summary>
    /// <param name="address"></param>
    /// <param name="offset"></param>
    /// <param name="correct"></param>
    /// <returns></returns>
    public DWORD FixCalculated(DWORD address, int offset, int correct)
    {
        return ReadInt32(address + offset + 1) + address + 5 + correct;
    }
    public bool OpenProcess(Process toOpen)
    {
        if (toOpen != null)
            ProcessHandle = OpenProcess(0x001F0FFF, false, toOpen.Id);
        else
            return false;

        if (ProcessHandle != IntPtr.Zero)
            return true;

        return false;
    }
    public void Free(DWORD address, int length)
    {
        VirtualFreeEx(ProcessHandle, address, (uint)length, 0x8000);
    }
    public void CreateRemoteThread(DWORD address)
    {
        const int bytes = 0;
        CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, bytes);
    }
    public int Malloc(int length)
    {
        return (int)VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)length, 0x3000, 0x40);
    }//check

    #region Keys
    public enum VirtualKeyStates : int
    {
        VK_LBUTTON = 0x01,
        VK_RBUTTON = 0x02,
        VK_CANCEL = 0x03,
        VK_MBUTTON = 0x04,
        //
        VK_XBUTTON1 = 0x05,
        VK_XBUTTON2 = 0x06,
        //
        VK_BACK = 0x08,
        VK_TAB = 0x09,
        //
        VK_CLEAR = 0x0C,
        VK_RETURN = 0x0D,
        //
        VK_SHIFT = 0x10,
        VK_CONTROL = 0x11,
        VK_MENU = 0x12,
        VK_PAUSE = 0x13,
        VK_CAPITAL = 0x14,
        //
        VK_KANA = 0x15,
        VK_HANGEUL = 0x15,  /* old name - should be here for compatibility */
        VK_HANGUL = 0x15,
        VK_JUNJA = 0x17,
        VK_FINAL = 0x18,
        VK_HANJA = 0x19,
        VK_KANJI = 0x19,
        //
        VK_ESCAPE = 0x1B,
        //
        VK_CONVERT = 0x1C,
        VK_NONCONVERT = 0x1D,
        VK_ACCEPT = 0x1E,
        VK_MODECHANGE = 0x1F,
        //
        VK_SPACE = 0x20,
        VK_PRIOR = 0x21,
        VK_NEXT = 0x22,
        VK_END = 0x23,
        VK_HOME = 0x24,
        VK_LEFT = 0x25,
        VK_UP = 0x26,
        VK_RIGHT = 0x27,
        VK_DOWN = 0x28,
        VK_SELECT = 0x29,
        VK_PRINT = 0x2A,
        VK_EXECUTE = 0x2B,
        VK_SNAPSHOT = 0x2C,
        VK_INSERT = 0x2D,
        VK_DELETE = 0x2E,
        VK_HELP = 0x2F,
        //
        VK_LWIN = 0x5B,
        VK_RWIN = 0x5C,
        VK_APPS = 0x5D,
        //
        VK_SLEEP = 0x5F,
        //
        VK_NUMPAD0 = 0x60,
        VK_NUMPAD1 = 0x61,
        VK_NUMPAD2 = 0x62,
        VK_NUMPAD3 = 0x63,
        VK_NUMPAD4 = 0x64,
        VK_NUMPAD5 = 0x65,
        VK_NUMPAD6 = 0x66,
        VK_NUMPAD7 = 0x67,
        VK_NUMPAD8 = 0x68,
        VK_NUMPAD9 = 0x69,
        VK_MULTIPLY = 0x6A,
        VK_ADD = 0x6B,
        VK_SEPARATOR = 0x6C,
        VK_SUBTRACT = 0x6D,
        VK_DECIMAL = 0x6E,
        VK_DIVIDE = 0x6F,
        VK_F1 = 0x70,
        VK_F2 = 0x71,
        VK_F3 = 0x72,
        VK_F4 = 0x73,
        VK_F5 = 0x74,
        VK_F6 = 0x75,
        VK_F7 = 0x76,
        VK_F8 = 0x77,
        VK_F9 = 0x78,
        VK_F10 = 0x79,
        VK_F11 = 0x7A,
        VK_F12 = 0x7B,
        VK_F13 = 0x7C,
        VK_F14 = 0x7D,
        VK_F15 = 0x7E,
        VK_F16 = 0x7F,
        VK_F17 = 0x80,
        VK_F18 = 0x81,
        VK_F19 = 0x82,
        VK_F20 = 0x83,
        VK_F21 = 0x84,
        VK_F22 = 0x85,
        VK_F23 = 0x86,
        VK_F24 = 0x87,
        //
        VK_NUMLOCK = 0x90,
        VK_SCROLL = 0x91,
        //
        VK_OEM_NEC_EQUAL = 0x92,   // '=' key on numpad
                                   //
        VK_OEM_FJ_JISHO = 0x92,   // 'Dictionary' key
        VK_OEM_FJ_MASSHOU = 0x93,   // 'Unregister word' key
        VK_OEM_FJ_TOUROKU = 0x94,   // 'Register word' key
        VK_OEM_FJ_LOYA = 0x95,   // 'Left OYAYUBI' key
        VK_OEM_FJ_ROYA = 0x96,   // 'Right OYAYUBI' key
                                 //
        VK_LSHIFT = 0xA0,
        VK_RSHIFT = 0xA1,
        VK_LCONTROL = 0xA2,
        VK_RCONTROL = 0xA3,
        VK_LMENU = 0xA4,
        VK_RMENU = 0xA5,
        //
        VK_BROWSER_BACK = 0xA6,
        VK_BROWSER_FORWARD = 0xA7,
        VK_BROWSER_REFRESH = 0xA8,
        VK_BROWSER_STOP = 0xA9,
        VK_BROWSER_SEARCH = 0xAA,
        VK_BROWSER_FAVORITES = 0xAB,
        VK_BROWSER_HOME = 0xAC,
        //
        VK_VOLUME_MUTE = 0xAD,
        VK_VOLUME_DOWN = 0xAE,
        VK_VOLUME_UP = 0xAF,
        VK_MEDIA_NEXT_TRACK = 0xB0,
        VK_MEDIA_PREV_TRACK = 0xB1,
        VK_MEDIA_STOP = 0xB2,
        VK_MEDIA_PLAY_PAUSE = 0xB3,
        VK_LAUNCH_MAIL = 0xB4,
        VK_LAUNCH_MEDIA_SELECT = 0xB5,
        VK_LAUNCH_APP1 = 0xB6,
        VK_LAUNCH_APP2 = 0xB7,
        //
        VK_OEM_1 = 0xBA,   // ';:' for US
        VK_OEM_PLUS = 0xBB,   // '+' any country
        VK_OEM_COMMA = 0xBC,   // ',' any country
        VK_OEM_MINUS = 0xBD,   // '-' any country
        VK_OEM_PERIOD = 0xBE,   // '.' any country
        VK_OEM_2 = 0xBF,   // '/?' for US
        VK_OEM_3 = 0xC0,   // '`~' for US
                           //
        VK_OEM_4 = 0xDB,  //  '[{' for US
        VK_OEM_5 = 0xDC,  //  '\|' for US
        VK_OEM_6 = 0xDD,  //  ']}' for US
        VK_OEM_7 = 0xDE,  //  ''"' for US
        VK_OEM_8 = 0xDF,
        //
        VK_OEM_AX = 0xE1,  //  'AX' key on Japanese AX kbd
        VK_OEM_102 = 0xE2,  //  "<>" or "\|" on RT 102-key kbd.
        VK_ICO_HELP = 0xE3,  //  Help key on ICO
        VK_ICO_00 = 0xE4,  //  00 key on ICO
                           //
        VK_PROCESSKEY = 0xE5,
        //
        VK_ICO_CLEAR = 0xE6,
        //
        VK_PACKET = 0xE7,
        //
        VK_OEM_RESET = 0xE9,
        VK_OEM_JUMP = 0xEA,
        VK_OEM_PA1 = 0xEB,
        VK_OEM_PA2 = 0xEC,
        VK_OEM_PA3 = 0xED,
        VK_OEM_WSCTRL = 0xEE,
        VK_OEM_CUSEL = 0xEF,
        VK_OEM_ATTN = 0xF0,
        VK_OEM_FINISH = 0xF1,
        VK_OEM_COPY = 0xF2,
        VK_OEM_AUTO = 0xF3,
        VK_OEM_ENLW = 0xF4,
        VK_OEM_BACKTAB = 0xF5,
        //
        VK_ATTN = 0xF6,
        VK_CRSEL = 0xF7,
        VK_EXSEL = 0xF8,
        VK_EREOF = 0xF9,
        VK_PLAY = 0xFA,
        VK_ZOOM = 0xFB,
        VK_NONAME = 0xFC,
        VK_PA1 = 0xFD,
        VK_OEM_CLEAR = 0xFE
    }
    public bool Keystate(VirtualKeyStates key)
    {
        int state = GetKeyState(key);
        if (state == -127 || state == -128)
        {
            return true;
        }
        return false;
    }
    #endregion

    //make something like a  dictionary with address and old value then if we write it saves it..
    #endregion
}
//}