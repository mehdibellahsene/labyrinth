# Labyrinth

Labyrinth explorer with smart pathfinding, multi-crawler coordination, and server API.

## Pre-built binaries

Self-contained binaries (no .NET SDK required):

| Platform | Client | Server |
|----------|--------|--------|
| Windows | `dist/win-x64/Labyrinth/Labyrinth.exe` | `dist/win-x64/LabyrinthServer/LabyrinthServer.exe` |
| Linux | `dist/linux-x64/Labyrinth/Labyrinth` | `dist/linux-x64/LabyrinthServer/LabyrinthServer` |

On Linux, make the binary executable first: `chmod +x Labyrinth`

## Usage

### Local demo (no server needed)

```bash
# Windows
dist/win-x64/Labyrinth/Labyrinth.exe

# Linux
./dist/linux-x64/Labyrinth/Labyrinth
```

### With server

```bash
# 1. Start the server
./LabyrinthServer

# 2. Run the client
./Labyrinth http://localhost:5000 <appKey>
```

### Options

| Flag | Description |
|------|-------------|
| `--random` | Use random explorer instead of smart pathfinding |
| `--multi <n>` | Use n crawlers (1-3) with coordination |
| `--visual` | Slower rendering for visualization |
| `settings.json` | Pass a settings file (e.g. `{ "random-seed": 2 }`) |

### Examples

```bash
./Labyrinth http://localhost:5000 <appKey> --multi 3 settings.json
./Labyrinth http://localhost:5000 <appKey> --random
```

## Build from source

Requires .NET 10 SDK (preview).

```bash
dotnet build Labyrinth.sln
dotnet test
dotnet run --project Labyrinth -- <args>
```
