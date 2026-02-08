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

### 1. Local demo (no server needed)

**Windows:**
```
dist\win-x64\Labyrinth\Labyrinth.exe
```

**Linux:**
```bash
chmod +x dist/linux-x64/Labyrinth/Labyrinth
dist/linux-x64/Labyrinth/Labyrinth
```

### 2. With the included local server

**Windows (2 terminals):**
```
# Terminal 1 - Start the server
dist\win-x64\LabyrinthServer\LabyrinthServer.exe

# Terminal 2 - Run the client
dist\win-x64\Labyrinth\Labyrinth.exe http://localhost:5000 <appKey> --multi 3 settings.json
```

**Linux (2 terminals):**
```bash
# Terminal 1 - Start the server
chmod +x dist/linux-x64/LabyrinthServer/LabyrinthServer
dist/linux-x64/LabyrinthServer/LabyrinthServer

# Terminal 2 - Run the client
chmod +x dist/linux-x64/Labyrinth/Labyrinth
dist/linux-x64/Labyrinth/Labyrinth http://localhost:5000 <appKey> --multi 3 settings.json
```

### 3. With the remote server (no local server needed)

**Windows:**
```
dist\win-x64\Labyrinth\Labyrinth.exe https://labyrinth.syllab.com <appKey> --multi 3 settings.json
```

**Linux:**
```bash
dist/linux-x64/Labyrinth/Labyrinth https://labyrinth.syllab.com <appKey> --multi 3 settings.json
```

`<appKey>` is a GUID identifying your session (e.g. `550e8400-e29b-41d4-a716-446655440000`).

### Options

| Flag | Description |
|------|-------------|
| `--random` | Use random explorer instead of smart pathfinding |
| `--multi <n>` | Use n crawlers (1-3) with coordination |
| `--visual` | Slower rendering for visualization |
| `settings.json` | Pass a settings file (e.g. `{ "random-seed": 2 }`) |

## Build from source

Requires .NET 10 SDK (preview).

```bash
dotnet build Labyrinth.sln
dotnet test
dotnet run --project Labyrinth -- http://localhost:5000 <appKey> --multi 3 settings.json
```
