# Release verification

1. Update `Directory.Build.props`, `CHANGELOG.md`, and any versioned user instructions. Keep the installer AppId unchanged.
2. Run `./build.ps1` and the opt-in real-service smoke check from a normal Windows 11 desktop.
3. In a clean Windows 11 x64 account without VLC or .NET, install in Portuguese and English. Confirm the classic context menu on a supported video and correct handling of a path with spaces, accents, ampersands and apostrophes. Confirm unsupported file types are not modified.
4. Select a default language, restart, and verify it persists. Change only the search language and check the default remains unchanged. Switch interface languages; check 100%, 150% and 200% display scaling.
5. Search by hash, exercise title fallback, download SRT, preserve an existing subtitle and test an unwritable destination, network failure and closing mid-request.
6. Reinstall/update without losing preferences. Disable the menu task during an update and verify associations are removed. Uninstall and verify program shortcuts, uninstall entry and all 21 owned context-menu keys are removed; user subtitle files and preferences must remain.
7. Create tag `v0.1.0` only when validation is complete. GitHub Actions creates an installer/checksum artifact and a **draft prerelease**. Review the notes and publish the draft once manual verification is recorded.

Initial installers are unsigned. Do not claim code signing or guaranteed legacy-service access. There is no automatic updater in v0.1.0; install the new release over the old version.
