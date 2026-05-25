# Unity TA Performance Optimization

## Project Overview

This is a **Unity 2022.3.62f3 LTS** project focused on Technical Art (TA) performance optimization and material/showcase scene construction. It uses the **Universal Render Pipeline (URP)** and contains a pre-built sci-fi office environment for profiling, shader development, and rendering optimization experiments.

The project is currently in a simplified state with minimal active C# code. Historically, it contained editor automation scripts that procedurally generated showcase scenes (a material showroom and a closed sci-fi office room), but these scripts and their generated outputs have been removed from the current working tree.

## Technology Stack

| Component | Version / Details |
|---|---|
| Unity Editor | 2022.3.62f3 (LTS) |
| Render Pipeline | Universal Render Pipeline (URP) 14.0.12 |
| Scripting Runtime | .NET Framework 4.7.1 |
| C# Language Version | 9.0 |
| Color Space | Linear |
| Primary Platform | Windows Standalone (Win64) |

### Key Unity Packages

- `com.unity.render-pipelines.universal` @ 14.0.12
- `com.unity.textmeshpro` @ 3.0.7
- `com.unity.timeline` @ 1.7.7
- `com.unity.visualscripting` @ 1.9.4
- `com.unity.collab-proxy` @ 2.12.4
- `com.unity.feature.development` @ 1.0.1 (bundles Test Framework, Profile Analyzer, Code Coverage)
- `com.unity.burst` @ 1.8.21
- `com.unity.mathematics` @ 1.2.6

### Third-Party Assets

- **DOTween** (`Assets/ThirdParty/Demigiant/DOTween/`) — A tweening library by Demigiant. Includes runtime DLL, editor DLL, and optional C# modules (Audio, Physics, Physics2D, Sprite, UI, Utils). An Assembly Definition (`DOTweenModules.asmdef`) is present in the Modules folder.

## Project Structure

```
Assets/
├── Art/                    # Custom art assets
│   ├── Materials/          # Custom materials (emissive screens, light pipes)
│   ├── Models/             # (empty or minimal)
│   ├── Textures/           # Custom textures (noise, screen data, light streaks)
│   └── VFX/                # (empty or minimal)
├── Prefabs/                # (empty in current state)
├── Record/                 # Screenshots and progress captures
├── Render/                 # URP Pipeline Asset and Forward Renderer
├── Resources/              # Contains DOTweenSettings.asset
├── Scenes/
│   └── Showcase_Scene.unity      # Only active scene in Build Settings
│   └── Showcase_Scene/           # Lighting data (lightmaps, reflection probes)
├── ScifiOfficeLite/        # Third-party sci-fi office environment asset pack
│   ├── Meshes/             # FBX models and their materials
│   ├── Prefabs/            # Reusable environment prefabs (floors, lights, furniture)
│   ├── Scene/              # Demo scene data from the asset pack
│   └── Scripts/            # Demo scripts (first-person controller, door trigger)
├── Scripts/
│   ├── Editor/             # (empty — no custom editor scripts currently)
│   └── Runtime/
│       └── MoveAnim.cs     # Empty MonoBehaviour stub
├── Shaders/
│   └── ShaderGraphs/       # Shader Graph assets
│       ├── SG_Hologram.shadergraph
│       ├── SG_LightFlow.shadergraph
│       └── SG_ScreenFlow.shadergraph
├── ThirdParty/
│   └── Demigiant/          # DOTween library
└── UI/                     # (empty or minimal)
```

## Build and Runtime Architecture

- **Build Target:** Windows Standalone (x64). The `Assembly-CSharp.csproj` confirms `UNITY_STANDALONE_WIN` and `PLATFORM_STANDALONE_WIN` preprocessor defines.
- **Active Scene:** Only `Assets/Scenes/Showcase_Scene.unity` is registered in `EditorBuildSettings.asset`.
- **URP Configuration:** `Assets/Render/Universal Render Pipeline Asset.asset` is assigned as the custom render pipeline in `GraphicsSettings`.
  - Main Light: shadow-casting, 2048 shadowmap resolution
  - Additional Lights: Per-Vertex/Per-Pixel mixed, limit 8 per object
  - MSAA: Off (1x)
  - HDR: Enabled
  - Render Scale: 1.0
  - No custom Renderer Features are currently attached.

### Quality Settings
The project includes the default Unity quality tiers (Very Low, Low, Medium, High, Very High, Ultra). The current active quality is **Ultra** (index 5).

## Code Organization

### Custom Scripts
Custom runtime code is extremely minimal:

| File | Namespace | Purpose |
|---|---|---|
| `Assets/Scripts/Runtime/MoveAnim.cs` | global | Empty `MonoBehaviour` stub. |

### Asset-Pack Scripts
These scripts ship with the `ScifiOfficeLite` environment and are **not** project-original:

| File | Namespace | Purpose |
|---|---|---|
| `Assets/ScifiOfficeLite/Scripts/DemoFirstPersonController.cs` | `ScifiOffice` | Basic first-person rigidbody controller with keyboard, keyboard+mouse, and mobile input modes. Supports crouching. |
| `Assets/ScifiOfficeLite/Scripts/DemoDoor.cs` | `ScifiOffice` | Simple trigger-based Animator controller for doors. |

### DOTween Modules
Third-party extension methods for tweening various Unity components. These are standard DOTween module files and should not be edited unless upgrading the library.

## Development Conventions

- **No Assembly Definition files** are used for custom code. The project relies on the implicit `Assembly-CSharp` (runtime) and `Assembly-CSharp-Editor` assemblies.
- **File naming:** Uses PascalCase for C# scripts and Unity assets. Some record/screenshot files use Chinese characters in their names.
- **Namespaces:** Custom code does not currently use namespaces. The asset-pack scripts use `namespace ScifiOffice`.
- **Shader workflow:** Custom shaders are authored with **Shader Graph** (`.shadergraph` files) rather than hand-written HLSL.
- **Scene workflow:** The project appears to be scene-centric. Most content lives in the single showcase scene or in prefabs from the asset pack.

## Build Process

There is **no active CI/CD configuration or build script** in the repository today. However, build logs in `Logs/` indicate that the project previously supported batch-mode editor builds via two editor utility classes (now deleted):

- `PerformanceShowroomSceneBuilder.Build()` — procedurally built a material showroom scene.
- `ScifiOfficeClosedRoomBuilder.Build()` — procedurally built a closed sci-fi office room scene.

If these workflows need to be restored, the build logs show they were invoked like this:

```bash
Unity.exe -batchmode -quit \
  -projectPath <repo_root> \
  -executeMethod PerformanceShowroomSceneBuilder.Build \
  -logFile <log_path>
```

Standard Unity builds can be produced through the Editor GUI (`File > Build Settings`) or via the Unity CLI with `BuildPipeline.BuildPlayer`.

## Testing

- The `com.unity.test-framework` package is present (via `com.unity.feature.development`), but **no custom tests or Test Runner assemblies exist** in the project.
- There is no active test suite to run.

## Security & Sensitivity Considerations

- **No `.env` files or hardcoded secrets** were found in the repository.
- The `.gitignore` is the standard Unity template and correctly excludes `Library/`, `Temp/`, `Logs/`, `UserSettings/`, and build artifacts.
- **Licensing logs** in `Logs/` contain local machine identifiers and Unity license serial numbers. These files are gitignored in principle, but old logs may persist in the working directory. Do not commit `Logs/` to version control.
- `Library/` and `Temp/` should never be committed.

## Notes for AI Agents

- This is an **art-heavy, code-light** project. Most value is in scenes, materials, textures, and URP settings rather than C# logic.
- Before adding new C# systems, consider whether the existing URP/Shader Graph tooling can achieve the goal without runtime scripts.
- The empty `MoveAnim.cs` in `Assets/Scripts/Runtime/` is safe to delete or repurpose.
- If modifying URP settings, always verify changes in the **Frame Debugger** and **Profiler** because performance optimization is the stated goal of the project.
