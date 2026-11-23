# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

ClassicAssist is a plugin for [ClassicUO](https://github.com/andreakarasho/ClassicUO) (Ultima Online client) providing a UOSteam-like interface with IronPython 3.4 macro scripting, hotkeys, autoloot, and various automation agents. UI is Avalonia (cross-platform) on .NET 11.

## Build & Test

```bash
dotnet restore
dotnet build                                    # Debug build
dotnet build -c Release                         # Release build
dotnet test --no-build --verbosity normal       # Run all tests (MSTest)
dotnet test --no-build --filter "FullyQualifiedName~ClassName.MethodName"  # Single test
```

Build output goes to `.\Output\`. Configurations: Debug, Develop, Release. Platform: x64. All projects target `net11.0`.

## Architecture

**Plugin loading:** ClassicUO's net472 bootstrapper loads `ClassicAssist.Proxy.dll` (net472, `ClassicAssist.Proxy/`), which starts the .NET runtime in-process via hostfxr and calls `Assistant.EngineCore.Install` in `ClassicAssist.dll` (net11.0), which forwards to `Engine.Install( PluginHeader* )` (ClassicAssist/Engine.cs). The proxy mirrors `PluginHeader` inline (no cuoapi reference) and must keep the namespace `Assistant`, class `Engine`, single `public static void Install(PluginHeader*)`. The proxy is platform-agnostic (Windows/Linux/macOS, matching the bootstrapper): it discovers hostfxr per OS (`hostfxr.dll` / `libhostfxr.so` / `libhostfxr.dylib` under the per-OS dotnet root or `DOTNET_ROOT`) and marshals hostfxr's `char_t` as UTF-16 on Windows and UTF-8 elsewhere. The managed entry point is resolved via an explicit delegate type (`Assistant.ComponentEntryPoint`) — do NOT use `[UnmanagedCallersOnly]` with a null delegate type: CoreCLR then builds a managed delegate to the method, which is illegal and fail-fasts the process (or escapes a JIT failure as SEHException). `ClassicAssist.csproj` sets `CopyLocalLockFileAssemblies=true` because hostfxr resolves component dependencies from the deps.json against files in the output folder — without it the deployment is incomplete and the entry point dies with FileNotFoundException/SEH. `Engine.Install` initializes the engine, then boots Avalonia (`AppBuilder.SetupWithoutStarting()`) on a dedicated background UI thread and shows `SplashWindow` + `MainWindow`. There is no separate UI process or IPC layer. All ClassicUO callbacks are wrapped in exception-safe shims (`Safe*` methods in Engine.cs) so an unhandled exception cannot cross the native thunk and fail-fast the client.

### Key Projects

| Project | Role |
|---------|------|
| **ClassicAssist** | Core library — macros, hotkeys, networking, Avalonia UI, game logic |
| **ClassicAssist.Shared** | Localization (Strings.resx), settings, utilities, UI compat shims |
| **ClassicAssist.Controls** | Reusable Avalonia controls |
| **ClassicAssist.Launcher** | Launches ClassicUO with the plugin configured |
| **ClassicAssist.Updater** | Self-update UI (Avalonia) |
| **ClassicAssist.Tests** | MSTest unit tests |
| **ClassicAssist.UITest** | Avalonia headless UI test harness |
| **Tools/ExportCommands, Tools/SyncPOEditor** | Command-line utilities |

### Core Singletons

- `Engine` (namespace `Assistant`, ClassicAssist/Engine.cs) — Central static partial class: player state, packet send/receive, options, client version. Most game state flows through here. UI work goes through `Avalonia.Threading.Dispatcher.UIThread`.
- `Options` / `Options.CurrentOptions` — Global settings, profile-aware.
- Managers: `MacroManager`, `HotkeyManager`, `AutolootManager`, `ActionPacketQueue` — all use `GetInstance()` singleton pattern.

### Macro System

Macros use IronPython 3.4. Command functions are static methods in `Data/Macros/Commands/*Commands.cs`, decorated with `[CommandsDisplay]` for UI categorization and wiki generation. Each file is a category (e.g., `TargetCommands.cs`, `MovementCommands.cs`). Translations for command descriptions/examples are loaded from `MacroCommandHelp.resx` using the convention `{METHOD_NAME_UPPER}_COMMAND_DESCRIPTION`, `_COMMAND_INSERTTEXT`, `_COMMAND_EXAMPLE`.

### Packet Handling

- `UO/Network/IncomingPacketHandlers.cs` — Server-to-client packet processing
- `UO/Network/OutgoingPacketHandlers.cs` — Client-to-server packet processing
- `UO/Network/IncomingPacketFilters.cs` — Filtered/modified incoming packets (name overrides, message filters)
- `UO/Network/OutgoingPacketFilters.cs` — Filtered outgoing packets
- `UO/Network/Packets/` — Packet construction classes
- `UO/Network/PacketFilter/` — Packet wait/filter infrastructure

Variable-length packets store length big-endian at bytes [1],[2]: high byte = `(byte)(length >> 8)`, low byte = `(byte)length`.

### UI Pattern (MVVM, Avalonia)

- ViewModels in `UI/ViewModels/`, Views in `UI/Views/`. XAML files are Avalonia XAML (included as `AvaloniaXaml` items).
- **Code-behind must declare its base class explicitly** (`public partial class FooWindow : Window`) — unlike WPF, Avalonia's generated partial does not supply it.
- `BaseViewModel` (UI/ViewModels/BaseViewModel.cs) uses `Avalonia.Threading.Dispatcher.UIThread`. ViewModels **must be created on the UI thread** — if created from a background thread (e.g., a macro), wrap in `Dispatcher.UIThread.Invoke()`.
- `Dispatcher.UIThread.Invoke()` is used throughout ViewModels for ObservableCollection mutations from background threads.
- `BaseViewModel.Instances` tracks live instances via weak references (thread-safe, latest instance per type). `Options.Save`/`Options.Load` iterate it to serialize every `ISettingProvider`.
- Settings persistence uses `ISettingProvider` with `Serialize`/`Deserialize` methods using `Newtonsoft.Json.Linq.JObject`.
- Dialogs: `MessageBox.Avalonia`; async `ShowDialog` tasks are obtained on the UI thread via `Dispatcher.UIThread.Invoke(() => window.ShowDialog(owner))` and awaited from the caller thread.

### Key Packages

Avalonia 11.3.x (Desktop, Themes.Fluent, Diagnostics in Debug/Develop, Xaml.Behaviors, Controls.DataGrid, Controls.ColorPicker), Avalonia.AvaloniaEdit 11.4.x (macro editor), MessageBox.Avalonia, IronPython 3.4, Newtonsoft.Json, Sentry, Exceptionless (Launcher/Updater), MSTest 3.11 + Microsoft.NET.Test.Sdk 18, Avalonia.Headless (tests).

### Tests

All tests run in a single process — there is no per-test `AppDomain` isolation (removed in modern .NET). Static state (`Engine.Journal`, managers, event subscriptions) is shared across tests: reset it in `[TestInitialize]`/`[TestCleanup]` and always unsubscribe `Engine.InternalPacketSentEvent`/`InternalPacketReceivedEvent` handlers a test adds, or background-thread assertion failures can crash the test host.

### Game Objects

- `UO/Objects/` — `Entity`, `Item`, `Mobile`, `ItemCollection`, `PlayerMobile`
- `UO/Data/` — Static game data (TileData, Art, Cliloc, Statics)
- `UO/Gumps/` — In-game gump rendering
- Serial ranges: `< 0x40000000` = mobile, `>= 0x40000000` = item (`UOMath.IsMobile`)

## Code Style

Defined in `.editorconfig`:
- 4-space indentation for C#/XAML
- **Spaces inside parentheses**: `if ( condition )`, `Method( arg )`, `(int) value`
- Explicit types preferred over `var`
- Braces required for all control flow
- Max line length: 180
- Constants: `ALL_UPPER_CASE`
- Private fields: `_camelCase`
- Local functions: `OnPascalCase` prefix

## Localization

All user-facing strings go through `ClassicAssist.Shared/Resources/Strings.resx`. Translations managed via POEditor.
