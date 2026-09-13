# PlayWWL

PlayWWL is a small Windows utility that opens the live WWL radio stream in VLC.

When it starts, the app asks Audacy for the current WWL stream URL, cleans the stream URL for VLC, and launches VLC with that stream. If Audacy's API is unavailable, it falls back to a known working Amperwave stream URL.

## Requirements

- Windows x64
- VLC media player installed at the default location:
  `C:\Program Files\VideoLAN\VLC\vlc.exe`

## Installing VLC

If VLC is not installed, download it from the official VideoLAN site:

https://www.videolan.org/vlc/

Run the installer with the default options. After installation, PlayWWL should be able to find VLC automatically.

## Using PlayWWL

Download `PlayWWL.exe` from the release, then run it. VLC should open and start playing WWL live.

## Building From Source

Install the .NET SDK, then run:

```powershell
dotnet publish -c Release
```

The published executable will be created under:

```text
bin\Release\net9.0\win-x64\publish\PlayWWL.exe
```
