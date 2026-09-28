# v0.1.0 validation

Validated on 2026-09-28:

- Release build with .NET SDK 10.0.401: no warnings or errors.
- 18 deterministic core tests: passed.
- 8 Windows Forms integration assertions: passed, with synthetic data and isolated preferences.
- Actual OpenSubtitles response and ZIP replayed through the production C# parser: passed (2 PT-BR results, valid UTF-8 SRT).
- Separate live protocol probe: anonymous search/download succeeded; language catalog returned 113 languages.
- Installer compiled with Inno Setup 7.1.0 x64; self-contained runtime and translated resources included.
- User confirmed installation, the Windows 11 classic context-menu command and opening the search popup.

The local sandbox cannot use Schannel credentials and does not expose all Windows profile folders to setup. Its full executable network smoke check and silent installation could not complete there. TLS certificate validation remains enabled.

GitHub Actions contains deterministic tests and isolated installer lifecycle checks (install, reinstall, menu removal/re-enable, uninstall, preferences retained). A manually dispatched `live_service` option runs the C# live search/download check; routine pushes do not contact the subtitle service. See the repository Actions history for the result of each exact commit.

Manual checks at 150%/200% display scaling and a clean Windows 11 machine without any preinstalled .NET runtime remain additional compatibility checks. The shipped installer includes its own runtime.
