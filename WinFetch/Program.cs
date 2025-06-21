using System;
using System.IO;
using System.Management;
using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;


class Program
{

    static void Main()
    {
        string[] logo = new string[]
        {
"                              .oodMMMM     ",
"                     .oodMMMMMMMMMMMMM     ",
"         ..oodMMM  MMMMMMMMMMMMMMMMMMM     ",
"   oodMMMMMMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"   MMMMMMMMMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"   MMMMMMMMMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"   MMMMMMMMMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"   MMMMMMMMMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"   MMMMMMMMMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"                                           ",
"   MMMMMMMMMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"   MMMMMMMMMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"   MMMMMMMMMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"   MMMMMMMMMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"   MMMMMMMMMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"   `^^^^^^MMMMMMM  MMMMMMMMMMMMMMMMMMM     ",
"         ````^^^^  ^^MMMMMMMMMMMMMMMMM     ",
"                        ````^^^^^^MMMM     ",
"                                           "
        };
        Console.WriteLine("-------------------------------------------------------------------------------------------------------");
        List<string> info = new List<string>();
        info.Add($"Username:        {Environment.UserName}");
        info.Add($"Hostname:        {Environment.MachineName}");
        info.Add($"OS:              {Environment.OSVersion}");
        info.Add($"CLR Version:     {Environment.Version}");
        info.Add($"CPU:             {GetCPU()}");
        info.Add($"GPU:             {GetGPU()}");
        info.Add($"VRAM:            {GetGPUMemory()}");


        int trueWidth, trueHeight;
        float scaling;
        GetRealResolutionAndScaling(out trueWidth, out trueHeight, out scaling);
        info.Add($"Resolution:      {trueWidth}x{trueHeight}");
        info.Add($"Scaling:         {Math.Round(scaling)}%");
        info.Add($"Refresh Rate:    {GetRefreshRate()} Hz");


        ComputerInfo ram = new ComputerInfo();
        info.Add($"RAM:             {BytesToGB(ram.TotalPhysicalMemory - ram.AvailablePhysicalMemory)} GB / {BytesToGB(ram.TotalPhysicalMemory)} GB");

        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady)
            {
                // Remove trailing backslash from drive name (e.g., "C:\\" -> "C:")
                string driveLabel = drive.Name.TrimEnd('\\');
                info.Add($"Disk {driveLabel}          {BytesToGB((ulong)(drive.TotalSize - drive.TotalFreeSpace))} GB / {BytesToGB((ulong)drive.TotalSize)} GB");
            }
        }


        info.Add($"Time Zone:       {TimeZoneInfo.Local.StandardName}");
        info.Add($"Uptime:          {GetUptime()}");


        int maxLines = Math.Max(logo.Length, info.Count);
        for (int i = 0; i < maxLines; i++)
        {
            // Left: ASCII Logo
            string logoPart = i < logo.Length ? logo[i].PadRight(10) : "     ";

            // Right: Info line
            string infoPart = i < info.Count ? info[i] : "";

            // Print Logo (in Cyan)
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(logoPart);


            // Spacer
            Console.ResetColor();

            // Print Label (Yellow) and Value (White)
            if (!string.IsNullOrWhiteSpace(infoPart))
            {
                int separatorIndex = infoPart.IndexOf(':');
                if (separatorIndex > 0)
                {
                    // Label (e.g. "Username:")
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(infoPart.Substring(0, separatorIndex + 1));

                    // Value (e.g. " Nikola")
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(infoPart.Substring(separatorIndex + 1));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(infoPart);
                }
            }
            else
            {
                Console.WriteLine();
            }
        }

        Console.ResetColor();
        // ... after printing logo + info lines

        PrintColorPalette();
        Console.WriteLine("-------------------------------------------------------------------------------------------------------");

    }


    static string GetUptime()
    {
        TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
        return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
    }

    static void PrintColorPalette()
    {
        Console.WriteLine();
        ConsoleColor[] colors = new ConsoleColor[]
        {
        ConsoleColor.Black,
                ConsoleColor.DarkGray,
        ConsoleColor.DarkRed,
                ConsoleColor.Red,
        ConsoleColor.DarkGreen,
                ConsoleColor.Green,
        ConsoleColor.DarkYellow,
                ConsoleColor.Yellow,
        ConsoleColor.DarkBlue,
                ConsoleColor.Blue,
        ConsoleColor.DarkMagenta,
                ConsoleColor.Magenta,
        ConsoleColor.DarkCyan,
                ConsoleColor.Cyan,
        ConsoleColor.Gray,
                ConsoleColor.White
        };

        for (int i = 0; i < colors.Length; i++)
        {
            Console.ForegroundColor = colors[i];
            Console.Write("██████");  // Colored block

            // After 8 blocks, print new line
            if ((i + 1) % 16 == 0)
            {
                Console.ResetColor();
                Console.WriteLine();
            }
        }
        Console.ResetColor();
    }



    static string GetCPU()
    {
        using (var searcher = new ManagementObjectSearcher("select Name from Win32_Processor"))
        {
            foreach (ManagementObject item in searcher.Get())
            {
                return item["Name"]?.ToString() ?? "Unknown";
            }
        }
        return "Unknown";
    }

    static string GetGPU()
    {
        using (var searcher = new ManagementObjectSearcher("select Name from Win32_VideoController"))
        {
            foreach (ManagementObject item in searcher.Get())
            {
                return item["Name"]?.ToString() ?? "Unknown";
            }
        }
        return "Unknown";
    }



    static string GetGPUMemory()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT AdapterRAM FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["AdapterRAM"] != null)
                    {
                        ulong bytes = Convert.ToUInt64(obj["AdapterRAM"]);
                        return $"{Math.Round(bytes / 1024.0 / 1024.0 / 1024.0, 1)} GB";
                    }
                }
            }
        }
        catch
        {
            // Ignore errors and return unknown
        }
        return "Unknown";
    }



    static string BytesToGB(ulong bytes)
    {
        return Math.Round(bytes / 1024.0 / 1024.0 / 1024.0, 2).ToString();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern IntPtr GetDC(IntPtr hwnd);

    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    const int HORZRES = 8;
    const int VERTRES = 10;
    const int DESKTOPHORZRES = 118;
    const int DESKTOPVERTRES = 117;

    static void GetRealResolutionAndScaling(out int width, out int height, out float scaling)

    {
        IntPtr hdc = GetDC(IntPtr.Zero);
        int screenWidth = GetDeviceCaps(hdc, HORZRES);
        int screenHeight = GetDeviceCaps(hdc, VERTRES);
        int fullWidth = GetDeviceCaps(hdc, DESKTOPHORZRES);
        int fullHeight = GetDeviceCaps(hdc, DESKTOPVERTRES);

        width = fullWidth;
        height = fullHeight;
        scaling = (float)fullWidth / screenWidth * 100f;
    }

    static int GetRefreshRate()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT CurrentRefreshRate FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToInt32(obj["CurrentRefreshRate"]);
                }
            }
        }
        catch { }
        return -1;
    }


}
