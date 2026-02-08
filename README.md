# Labyrinth

Labyrinth explorer with smart pathfinding, multi-crawler coordination, and server API.

## Prerequisites

- .NET 10 SDK (preview)

## Pre-built binaries

Available in `dist/`:

```
dist/Labyrinth/Labyrinth.exe       # Client
dist/LabyrinthServer/LabyrinthServer.exe  # Server
```

## Usage

### Local demo (no server needed)

```
dist/Labyrinth/Labyrinth.exe
```

### With server

```
# 1. Start the server
dist/LabyrinthServer/LabyrinthServer.exe

# 2. Run the client
dist/Labyrinth/Labyrinth.exe http://localhost:5000 <appKey>
```

### Options

| Flag | Description |
|------|-------------|
| `--random` | Use random explorer instead of smart pathfinding |
| `--multi <n>` | Use n crawlers (1-3) with coordination |
| `--visual` | Slower rendering for visualization |
| `settings.json` | Pass a settings file (e.g. `{ "random-seed": 2 }`) |

### Examples

```
Labyrinth.exe http://localhost:5000 <appKey> --multi 3 settings.json
Labyrinth.exe http://localhost:5000 <appKey> --random
```

## Build from source

```
dotnet build Labyrinth.sln
dotnet test
dotnet run --project Labyrinth -- <args>
```
