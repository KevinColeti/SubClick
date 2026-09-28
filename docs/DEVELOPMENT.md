# Development

## Build

Install the .NET SDK version specified in `global.json` and Inno Setup 7.1.0. Then run from PowerShell:

```powershell
./build.ps1
# Or specify a portable compiler:
./build.ps1 -Iscc 'C:\Tools\Inno\ISCC.exe'
```

The script restores, builds with warnings as errors, runs deterministic tests, publishes a self-contained Windows x64 application and builds the installer plus SHA-256 in `artifacts/installer`. `-SkipInstaller` builds/tests/publishes without the compiler. `-PackageSource` can point to a trusted local NuGet cache for offline environments. No runtime service credentials are needed.

```powershell
dotnet run --project tests/SubClick.Tests -c Release
# Explicit opt-in: contacts the real service and downloads one Sintel subtitle.
dotnet run --project tests/SubClick.Tests -c Release -- --live
```

Tests use a dependency-free executable runner and fail with a nonzero exit code. They exercise the real core classes with fake HTTP responses/provider implementations, not a live service by default. Test directories are uniquely created below the system temp directory and removed by the runner.

## Architecture

- **Core:** video hash/title parsing; search orchestration; provider interface; XML-RPC adapter; catalog/cache; versioned atomic preferences; ZIP decoding and atomic no-overwrite SRT saving.
- **App:** Windows Forms presentation, translated `.resx` resources, first-run selection and asynchronous search/download. Service errors become stable codes translated in the UI.
- **Installer:** per-user Inno Setup, stable application ID, 21 video associations, properly quoted executable/video paths. It never changes the default video player.
- **Tests:** deterministic protocol/encoding/storage coverage; optional real service smoke check.

Provider APIs accept a `CancellationToken`. `SearchRequest.LanguageId` is the exact OpenSubtitles identifier, not a locale string; `pob` and `por` remain distinct. `GetSubLanguages` may omit `status`, unlike the login/search methods. API response and uncompressed subtitle limits are 8 MiB. All download redirect hosts are validated. Unknown non-UTF encodings are refused rather than guessed.

Preference schema v1 stores `SubtitleLanguageId` and `InterfaceLanguage` (`auto`, `pt-BR`, `en`). An invalid file is preserved with an `.invalid-*` suffix when new preferences are saved. Cross-process updates use a named mutex and replace the JSON file atomically. Search language changes do not write settings.

New translations must keep the English resource keys. Avoid machine-dependent paths in project files. Do not commit runtime outputs, secrets, certificates, user settings or downloaded subtitle content. Keep the upstream VLSub attribution intact.

## Known environment constraints

Some restricted development sandboxes block Windows Schannel credentials and user registry writes. A failed TLS handshake there is not proof that the external service is unavailable. Do not turn off TLS validation. Verify the application and installer on a normal Windows desktop before declaring a release verified.
