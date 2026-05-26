# WinForms → WPF Migration Plan: VisualMusic

## Context
VisualMusic is a WinForms app on .NET 8 that combines 3D music visualization (MonoGame), web browsing (CefSharp), and complex property editing. The goal is a full UI rewrite to WPF with a modern dark theme (MahApps.Metro) and MVVM architecture. The app logic is already largely decoupled from WinForms — the pain is concentrated in the UI layer.

---

## What Survives Unchanged
All domain model classes (`Project.cs`, `TrackProps.cs`, `NoteStyle*.cs`, `Camera.cs`, `KeyFrame.cs`, `UndoItems.cs`, `Media.cs`, etc.) — pure C#, no UI dependencies. The custom MonoGame fork in `Dependencies/MonoGame` stays as-is.

## What Gets Rewritten
- `Forms/Form1.cs` + `Form1.designer.cs` (2,698 + 2,720 lines) → `MainWindow.xaml` + `MainViewModel.cs`
- `Controls/SongPanel.cs` → split into `SongRenderer` (MonoGame logic) + `MonoGameHost` (WPF HwndHost)
- All 10 controls in `Controls/` → WPF UserControls
- All 19 dialogs in `Forms/` → WPF Windows
- `Program.cs` → `App.xaml.cs`
- `Settings.cs` settings coupling → `SettingsData` POCO

---

## Key Technical Decisions

### MonoGame Integration: HwndHost (not D3DImage/WpfInterop)
`GraphicsDeviceService.AddRef(IntPtr hwnd, ...)` already takes a raw HWND. Create `MonoGameHost : HwndHost`, override `BuildWindowCore` to create a Win32 child window via P/Invoke, pass that HWND to `GraphicsDeviceService.AddRef`. This keeps the custom MonoGame fork, `GraphicsDeviceService`, and rendering architecture intact. The WpfInterop/D3DImage approach would require porting the device management — not worth the risk with a custom fork.

**Airspace note:** WPF controls cannot overlap an HwndHost region. The existing layout (MonoGame panel beside the properties side-panel, not above) naturally avoids this. Any overlays (region-select rectangle, etc.) stay inside MonoGame SpriteBatch — they already do.

### MVVM Infrastructure: CommunityToolkit.Mvvm
Eliminates the `updatingControls` guard-flag pattern. Key packages: `CommunityToolkit.Mvvm`, `MahApps.Metro`, `MahApps.Metro.IconPacks.Material`, `CefSharp.Wpf.NETCore 121.3.70`.

### ViewModels to Create
- `MainViewModel` — project, undo stack, screen navigation, window title
- `ProjectViewModel` — wraps `Project`, exposes song position, playback state
- `TrackListViewModel` / `TrackItemViewModel` — track list and per-track color swatches
- `TrackPropsViewModel` — merged selection properties (replaces `mergedTrackProps`)
- `SongPropsViewModel` — wraps `ProjProps` fields

---

## Phase 1 — Foundation (Week 1–2)
**Goal:** WPF project compiles, blank dark window opens.

1. Change project file: `<UseWPF>true</UseWPF>`, remove `<UseWindowsForms>`. Keep `net8.0-windows`.
2. Add NuGet packages (see above). Remove `CefSharp.WinForms.NETCore`.
3. `App.xaml` with MahApps resource dictionaries (`BaseDark.Dark` theme).
4. `MainWindow.xaml : MetroWindow` with `DataContext = new MainViewModel()`.
5. Empty ViewModel shells in `ViewModels/`.

**Risk:** Custom MonoGame fork targets `net6.0-windows` with `UseWindowsForms=true`. Verify it compiles when consumed from a `UseWPF` project. If needed, add `<UseWindowsForms>true</UseWindowsForms>` alongside `<UseWPF>true</UseWPF>` or update the fork to `net8.0-windows`.

---

## Phase 2 — MonoGame Host (Week 2–3)
**Goal:** 3D visualization renders inside WPF. Mouse/keyboard input works.

1. Create `Controls/MonoGameHost.cs : HwndHost`. Override `BuildWindowCore` — P/Invoke `CreateWindowEx` for child Win32 window, call `GraphicsDeviceService.AddRef(hwnd, width, height)`.
2. Extract `SongPanel`'s rendering into `SongRenderer` — `Initialize(GraphicsDevice)`, `Update(double dt)`, `Draw()`. No WinForms references.
3. Replace `System.Windows.Forms.Timer` with `DispatcherTimer`. Replace `Invalidate()` + `OnPaint` with direct `Update()` / `Draw()` calls in the timer tick.
4. Mouse/keyboard events: wire on `MonoGameHost` or its parent `Grid`, forward normalized coords to `SongRenderer` (same `NormMouseX`/`NormMouseY` math).
5. Replace `((Form1)Parent)` calls in `selectRegion()` with `ITrackSelectionService` injected into `SongRenderer`. `MainViewModel` implements it.
6. Replace `RenderProgressForm` references in `renderVideo()` with `IRenderProgressCallback`.

**Critical file:** `VisualMusic/Controls/SongPanel.cs` + `VisualMusic/tparty/Xna forms/GraphicsDeviceService.cs`

---

## Phase 3 — CefSharp Browser Panels (Week 3)
**Goal:** MOD, SID, MIDI browser panels work in WPF.

1. Create `Controls/SongWebBrowserWpf.cs : UserControl` with `CefSharp.Wpf.ChromiumWebBrowser` (same API surface as WinForms version).
2. Replace `InvokeOnUiThreadIfRequired` (WinForms `Control.InvokeRequired`) with `Dispatcher.InvokeAsync`.
3. Replace `mainForm.getImportFormFromFileType()` in `DownloadHandler.OnBeforeDownload` with `IImportService` implemented by `MainViewModel`.
4. Three browser instances defined in XAML, shown via `ContentControl` bound to `MainViewModel.CurrentView`.
5. `App.xaml.cs OnStartup()`: move `Cef.Initialize()` here before window creation.

---

## Phase 4 — Main Window Layout & Navigation (Week 4)
**Goal:** Full window structure, menu, track list, screen switching.

1. `MainWindow.xaml` layout: `MetroWindow` → top menu, left panel (track list + properties `TabControl`), right `MonoGameHost`, bottom `ScrollBar`.
2. Screen switching: replace `List<Control> screens` + `ChangeToScreen()` with a `ContentControl` + `DataTemplate` selectors keyed on `MainViewModel.CurrentView` enum.
3. Menu items: all bind to `[RelayCommand]` methods on `MainViewModel`. Undo/redo headers bind to `MainViewModel.UndoDescription`/`RedoDescription`.
4. Track list: WPF `ListView` with `GridView` — color swatches are `Rectangle.Fill` bound to `TrackItemViewModel.Color` (replaces GDI `trackListGfxObj`/`Pen` drawing). Drag-to-reorder via WPF `DragDrop`.
5. Right-side `TabControl` (Style, Material, Light, Spatial, Audio) — each tab a separate `UserControl` bound to `TrackPropsViewModel`.
6. `updatingControls` guard pattern: eliminated — WPF two-way bindings don't cause re-entrancy.

---

## Phase 5 — Custom Controls & Property Tabs (Week 5–6)
**Goal:** All 5 property tabs functional.

**TbSlider** (used ~20+ times across tabs): WPF `UserControl` with `Slider` + `TextBox`, expose `Value`/`Min`/`Max`/`Decimals`/`ExpBase` as `DependencyProperty`. Exponential mode logic unchanged.

**TwoDHueSat** (hue-sat color picker canvas): WPF `UserControl`, gradient via WPF `LinearGradientBrush` + `DrawingBrush` (replaces `System.Drawing` pixel loop). Crosshair as positioned `Line` elements on a `Canvas`.

**HueSatButton**: WPF `Button` whose template shows current color as background; opens `HueSatForm` (now a `MetroWindow` containing `TwoDHueSat`).

**NoteStyleControl / LineStyleControl / BarStyleControl**: Replace `ParentForm as Form1` casts with `DataContext` as `TrackPropsViewModel`. Eliminate all `Form1.setNumericUdValue()` / `Form1.toCheckState()` helper calls — handled by bindings.

**Material tab texture thumbnail**: `RenderTarget2D.GetData<Color[]>()` → `WriteableBitmap` → WPF `Image` (same logic, different sink).

**Critical file:** `VisualMusic/Controls/TbSlider.cs` — unblock all other property tabs.

---

## Phase 6 — Dialog Forms (Week 7)
**Goal:** All import, export, and utility dialogs work.

- **Import forms** (`ImportMidiForm`, `ImportModForm`, `ImportSidForm`): Extract business logic to `ImportService`. Dialogs become thin WPF `Window` views calling the service.
- **Progress dialogs** (`ProgressForm`, `RenderProgressForm`): ViewModel property updated via `Dispatcher.InvokeAsync`; WPF `ProgressBar` bound to it.
- **`WaitForTaskForm` / `WaitForFileSearchForm`**: Replace with MahApps `ProgressRing` — ~5 lines of XAML.
- **`VideoExportForm`**: `VideoExportOptions` class is unchanged; form becomes a `MetroWindow` bound to a `VideoExportOptionsViewModel`.
- **`SubSongForm` / `TrackPropsTypeForm`**: Simple `ListBox`/`ComboBox` dialogs — trivial rewrites.
- **`TpartyIntegrationForm`**: Contains a web browser; becomes a `MetroWindow` with `SongWebBrowserWpf`.

**Pre-requisite:** Before Phase 6, fix the 6 places in `Settings.cs` that access `Form1.VidExpForm.Options` statically. Replace with `AppState.VideoExportOptions` singleton or pass through `MainViewModel`.

---

## Phase 7 — Settings, Undo/Redo, Polish (Week 8)
**Goal:** Feature-complete application.

1. **Settings**: Extract `SettingsData` POCO from `Settings.cs` (all the same fields). `MainViewModel` reads/writes `SettingsData` on startup/shutdown. XML format stays backward-compatible.
2. **Undo/redo**: `UndoItems.cs` moves into `MainViewModel`. Replace recursive `AddEventHandlers(Control.ControlCollection)` with explicit `AddUndoItem()` calls at ViewModel command sites.
3. **ScrollBar**: WPF `ScrollBar` in XAML, `Value` bound to `ProjectViewModel.SongScrollPosition`.
4. **KeyFrame/Lyrics DataGrids**: WPF `DataGrid` bound to `ObservableCollection<KeyFrameViewModel>` and `ObservableCollection<LyricsSegment>`.
5. **Drag-and-drop file opening**: `AllowDrop="True"` on `MetroWindow`, `Drop` event → `MainViewModel.OpenFilesCommand`.
6. **Startup args**: Move from `Form1_Load` to `App.xaml.cs OnStartup()`.
7. **Window title**: `MetroWindow.Title` bound to `MainViewModel.WindowTitle`.

---

## Verification
- Phase 2: MonoGame renders, camera rotates with mouse, spacebar starts/stops playback.
- Phase 3: All three web browser tabs load, downloading a file triggers the import dialog.
- Phase 4: Screen switching works (song view ↔ browsers), undo/redo menu items update labels, track list shows correct colors.
- Phase 5: Changing a property (e.g., TbSlider for line width) updates the 3D visualization immediately.
- Phase 6: Import a MIDI file end-to-end; export video with progress reporting.
- Phase 7: Close and reopen app — settings are restored. Undo across multiple operations works correctly.

---

## Work Branch
All WPF work on `feature/wpf-migration`. Keep `master` building with WinForms throughout.
