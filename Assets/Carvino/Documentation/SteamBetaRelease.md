# Steam beta release plan

Steam, rather than the game executable, should deliver all player updates. When the
Steamworks application exists, upload the Windows build using SteamPipe and assign it
to a private `alpha` branch. Testers opt into that branch through Steam Properties.

Suggested branches:

- `alpha` — private, password-protected development builds.
- `beta` — selected external testers.
- `default` — public release builds only.

Every build must show the matching `Application.version` in the title screen and save
file. Do not build a separate self-updater; Steam validates and patches depot files.
