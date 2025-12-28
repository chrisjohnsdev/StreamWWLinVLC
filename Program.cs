using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        string vlcPath = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
        string streamUrl = "https://prod-44-201-29-151.amperwave.net/audacy-wwlamaac-llhlsc.m3u8";

        Console.WriteLine("Launching WWL stream in VLC...");
        Console.WriteLine("Stream URL: " + streamUrl);

        if (!System.IO.File.Exists(vlcPath))
        {
            Console.WriteLine("VLC not found at: " + vlcPath);
            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
            return;
        }

        try
        {
            Process.Start(vlcPath, streamUrl);
            Console.WriteLine("VLC launched. Press ENTER to exit.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error launching VLC: " + ex.Message);
        }

        Console.ReadLine();
    }
}
