# SubClick

[Português](../README.md)

Find subtitles from the Windows right-click menu. SubClick searches by video hash, falls back to the title, and lets you choose a subtitle to save next to the video.

## Getting started

1. Run the setup executable and keep context-menu integration enabled.
2. Right-click a video → **Show more options** → **Find subtitles — SubClick**.
3. Choose a default subtitle language on first use. It is remembered across launches and upgrades.
4. Select a result and click **Download selected**, or double-click it.

The Start menu shortcut also opens a video picker. Change the language for a single search without changing your preference; **Use as default** explicitly saves the new default. The interface supports English and Brazilian Portuguese.

## Requirements and behavior

Windows 11 x64 and internet access. VLC and a separately installed .NET runtime are not required. One video at a time; single-part SRT subtitles. Foreign-parts-only subtitles are excluded; hearing-impaired subtitles are identified in the list.

All service languages are supported, with a bundled 93-language catalog and a refreshed local cache. Saved filenames include the provider language ID, such as `Video.eng.srt` or `Video.pob.srt`. Existing subtitles are never overwritten.

SubClick uses the anonymous legacy VLSub protocol. It does not ask for a personal API key or account, and does not bypass service limits. Availability depends on OpenSubtitles. Initial setup binaries are unsigned.

## Preferences and privacy

Configuration lives at `%LOCALAPPDATA%\SubClick\settings.json`. Upgrading and uninstalling retain preferences. Delete that file manually while the app is closed to reset them.

Videos are never uploaded. Searches send the selected language and video hash/size or title to OpenSubtitles. No telemetry, ads, or background service.

See [development](DEVELOPMENT.md), [release verification](RELEASING.md), and [third-party notices](../THIRD-PARTY-NOTICES.md). License: GPL-3.0-or-later. This is an independent project, not an official VLC or OpenSubtitles product.
