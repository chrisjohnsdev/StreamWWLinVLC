using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private const string VlcPath = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
    private const string AudacyContentUrl = "https://api.audacy.com/experience/v1/content?contentId=101-991&objectType=FULL";
    private const string FallbackStreamUrl = "https://live.amperwave.net/manifest/audacy-wwlamaac-llhlsc.m3u8";

    static async Task Main()
    {
        Console.WriteLine("Resolving WWL stream URL...");
        string streamUrl = await GetStreamUrlAsync();

        Console.WriteLine("Launching WWL stream in VLC...");
        Console.WriteLine("Stream URL: " + streamUrl);

        if (!File.Exists(VlcPath))
        {
            Console.WriteLine("VLC not found at: " + VlcPath);
            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
            return;
        }

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = VlcPath,
                UseShellExecute = false
            };
            startInfo.ArgumentList.Add(streamUrl);

            Process.Start(startInfo);

            Console.WriteLine("VLC launched. Press ENTER to exit.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error launching VLC: " + ex.Message);
        }

        Console.ReadLine();
    }

    private static async Task<string> GetStreamUrlAsync()
    {
        try
        {
            using HttpClient client = new();
            AddAudacyHeaders(client);

            using HttpResponseMessage response = await client.GetAsync(AudacyContentUrl);
            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync();
            using JsonDocument document = await JsonDocument.ParseAsync(stream);

            if (TryGetStreamUrl(document.RootElement, out string streamUrl))
            {
                return CleanStreamUrl(streamUrl);
            }

            Console.WriteLine("Audacy response did not include a WWL stream URL; using fallback.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not resolve WWL stream dynamically: " + ex.Message);
            Console.WriteLine("Using fallback stream URL.");
        }

        return FallbackStreamUrl;
    }

    private static void AddAudacyHeaders(HttpClient client)
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Aud-Correlation-Id", Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.TryAddWithoutValidation("Aud-Platform", "WEB");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Aud-Platform-Variant", "web");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Aud-Client-Session-ID", Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.TryAddWithoutValidation("Aud-User-Token", "app:f8k:" + Guid.NewGuid().ToString("N"));
        client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://www.audacy.com");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.audacy.com/");
    }

    private static bool TryGetStreamUrl(JsonElement root, out string streamUrl)
    {
        streamUrl = string.Empty;

        if (!root.TryGetProperty("content", out JsonElement content) ||
            content.ValueKind != JsonValueKind.Array ||
            content.GetArrayLength() == 0)
        {
            return false;
        }

        JsonElement station = content[0];
        if (!station.TryGetProperty("streamUrl", out JsonElement streamUrls))
        {
            return false;
        }

        streamUrl = GetStringProperty(streamUrls, "m3u8");
        if (string.IsNullOrWhiteSpace(streamUrl))
        {
            streamUrl = GetStringProperty(streamUrls, "aac");
        }

        if (string.IsNullOrWhiteSpace(streamUrl))
        {
            streamUrl = GetStringProperty(streamUrls, "mp3");
        }

        return !string.IsNullOrWhiteSpace(streamUrl);
    }

    private static string GetStringProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static string CleanStreamUrl(string streamUrl)
    {
        if (Uri.TryCreate(streamUrl, UriKind.Absolute, out Uri uri) &&
            uri.Host.Equals("live.amperwave.net", StringComparison.OrdinalIgnoreCase) &&
            uri.AbsolutePath.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
        {
            return uri.GetLeftPart(UriPartial.Path);
        }

        return streamUrl;
    }
}
