# GeneratorOffOnExit

Author: Dmnv · GUID: `com.dmnv.generatoroffonexit` · Target: SPT 4.1.x client (BepInEx plugin only, no server mod) · Fika-safe

Adds a **Turn off & exit (Enter)** button to the exit confirmation when the hideout generator is running, so it doesn't burn fuel while the game is closed (SPT simulates offline hideout time). Also shows the vanilla "You left the generator on!" warning when the hideout wasn't visited this session.

Install: `BepInEx\plugins\Dmnv-GeneratorOffOnExit\` (`GeneratorOffOnExit.Client.dll`). Nothing goes on the server.

Build: open `GeneratorOffOnExit.slnx` (VS 2026 / Rider) or `dotnet build`. Release zip: `dotnet build -c Release` → `dist\`.
