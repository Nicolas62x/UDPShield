using System;
using System.Runtime.InteropServices;

namespace UDPShield;
static class DataLogger
{
    public static object locker = new object();

    public static UInt64 InBound = 0;
    public static UInt64 OutBound = 0;
    public static UInt64 Blocked = 0;
    public static UInt64 InPackets = 0;
    public static UInt64 OutPackets = 0;
    public static UInt64 BlockedPackets = 0;

    public static MovingAvg InBound_AVG = new MovingAvg();
    public static MovingAvg OutBound_AVG = new MovingAvg();
    public static MovingAvg Blocked_AVG = new MovingAvg();
    public static MovingAvg InPackets_AVG = new MovingAvg();
    public static MovingAvg OutPackets_AVG = new MovingAvg();
    public static MovingAvg BlockedPackets_AVG = new MovingAvg();

    static DateTimeOffset LastUpdate = DateTimeOffset.Now;
    static uint tick = 0;

    public static void Tick()
    {
        tick++;

        lock (locker)
        {
            double dt = (DateTimeOffset.Now - LastUpdate).TotalSeconds;

            InBound_AVG.Push(InBound - Blocked, dt);
            OutBound_AVG.Push(OutBound, dt);
            Blocked_AVG.Push(Blocked, dt);
            InPackets_AVG.Push(InPackets - BlockedPackets, dt);
            OutPackets_AVG.Push(OutPackets, dt);
            BlockedPackets_AVG.Push(BlockedPackets, dt);

            LastUpdate = DateTimeOffset.Now;
        }

        string Info = $"Pipes: {Pipe.Connections.Count} In: {GetDataValue(InBound_AVG.GetAvg())}o/s {GetDataValue(InPackets_AVG.GetAvg())}p/s Out: {GetDataValue(OutBound_AVG.GetAvg())}o/s {GetDataValue(OutPackets_AVG.GetAvg())}p/s Blocked: {GetDataValue(Blocked_AVG.GetAvg())}o/s {GetDataValue(BlockedPackets_AVG.GetAvg())}p/s";


        if (tick % 40 == 0)
            Console.WriteLine($"[{DateTimeOffset.Now}] {Info}");

        TitleWrite(Info);
    }

    static void TitleWrite(string text)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Console.Title = text;
        else
            Console.Write($"\u001B]0;{text}\u0007");
    }

    static string[] Unit = ["", "Ki", "Mi", "Gi", "Ti", "Pi", "Ei", "Zi", "Yi"];
    public static string GetDataValue(double val)
    {
        int idx = 0;

        while (val >= 1000 && (idx + 1) < Unit.Length)
        {
            val /= 1024.0;
            idx++;
        }

        return $"{val:0.0}{Unit[idx]}";
    }

    public static string GetDataValue(UInt64 val)
    {
        int idx = 0;

        while (val >= 1000 && (idx + 1) < Unit.Length)
        {
            val /= 1024;
            idx++;
        }

        return $"{val}{Unit[idx]}";
    }
}
