# WPF Phase 6 — Main Window Layout Shell

## Context

Phases 1–5 of the WinForms→WPF migration have landed: WPF entry point, MonoGame host, web browsers, menu bar, project commands, import dialogs. But the current `MainWindow.xaml` is missing **everything that used to live around the MonoGame area in WinForms Form1**:

- The **right panel** (`trackPropsPanel`) — track list + per-track property tabs
- The **left panel** (`songPropsPanel`) — project-wide settings (background, viewport, camera, pitch, audio offset)
- The **bottom song scrollbar** (`songScrollBar`)
- The **toggle panel** (`propsTogglePanel`) — two checkboxes to show/hide the left/right panels independently

The original Phase 4 plan said it would deliver this layout, but the commits only wired up the menu bar. The original Phase 5 added import dialogs and edit commands but no custom controls or property-tab UI. Per discussion, this phase ("Phase 6" by commit order) delivers the **layout shell only** — empty placeholders for the property tabs and the left panel. Each of the 5 per-track tabs (Style / Material / Light / Spatial / Audio) and the left songPropsPanel will follow as their own phases.

After this phase the app should: load a project, show the track list with correct colours, toggle the two panels with the top checkboxes, drag the splitter, scrub the timeline with the scrollbar. No property editing yet — clicking the tabs shows empty pages.

---

## Layout

Outer `DockPanel` stays. Add side panels + scrollbar as dock children. Center `Grid` (MonoGameHost + 3 browsers) stays as `LastChildFill`.

`MainWindow.xaml` skeleton:

```xml
<DockPanel>
  <!-- Top row: menu + toggle buttons, sharing one dock -->
  <DockPanel DockPanel.Dock="Top">
    <StackPanel DockPanel.Dock="Right" Orientation="Horizontal" Margin="0,0,8,0"
                IsEnabled="{Binding HasProject}">
      <ToggleButton Content="Pr_oject properties" IsChecked="{Binding ShowSongProps, Mode=TwoWay}" Margin="2,1"/>
      <ToggleButton Content="_Track properties"   IsChecked="{Binding ShowTrackProps, Mode=TwoWay}" Margin="2,1"/>
    </StackPanel>
    <Menu> ...existing menu items... </Menu>
  </DockPanel>

  <!-- Bottom: timeline scrollbar (Song screen only) -->
  <ScrollBar DockPanel.Dock="Bottom" Orientation="Horizontal"
             Minimum="0" Maximum="1"
             SmallChange="0.0625" LargeChange="1.0"
             Value="{Binding ScrollPosition, Mode=TwoWay}"
             IsEnabled="{Binding HasProject}"
             Visibility="{Binding CurrentScreen,
                           Converter={StaticResource ScreenVis},
                           ConverterParameter=Song}"/>

  <!-- Left: project-wide props placeholder -->
  <ctrl:SongPropsPanel DockPanel.Dock="Left" Width="216"
                       DataContext="{Binding SongProps}"
                       Visibility="{Binding ShowSongProps,
                                     Converter={StaticResource BoolVis}}"/>

  <!-- Right: track list (with splitter) + property tabs -->
  <Grid DockPanel.Dock="Right" Width="395"
        Visibility="{Binding ShowTrackProps,
                      Converter={StaticResource BoolVis}}">
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="186" MinWidth="120"/>
      <ColumnDefinition Width="4"/>
      <ColumnDefinition Width="*" MinWidth="180"/>
    </Grid.ColumnDefinitions>
    <ctrl:TrackListView   Grid.Column="0" DataContext="{Binding TrackList}"/>
    <GridSplitter         Grid.Column="1" HorizontalAlignment="Stretch" Width="4"/>
    <ctrl:TrackPropsView  Grid.Column="2" DataContext="{Binding SelectedTrackProps}"/>
  </Grid>

  <!-- Center (LastChildFill): existing Grid with MonoGameHost + browsers -->
  <Grid> ...existing... </Grid>
</DockPanel>
```

Use the built-in `BooleanToVisibilityConverter` (add as `BoolVis` in `MainWindow.Resources` next to the existing `ScreenVis`).

---

## ViewModels

Four new files under [VisualMusic/ViewModels](VisualMusic/ViewModels/) plus modifications to [MainViewModel.cs](VisualMusic/ViewModels/MainViewModel.cs).

### `TrackListViewModel`
- `ObservableCollection<TrackItemViewModel> Items`
- `[ObservableProperty] TrackItemViewModel selectedItem` + an `IList SelectedItems` exposed via two-way binding (WPF `ListView.SelectedItems` is read-only — use code-behind to push selection into the VM)
- `Rebuild(Project p)` — clears and repopulates, mirroring `CreateTrackList()` at [Form1.cs:629](VisualMusic/Forms/Form1.cs:629). Track index 0 = "Global"
- `Reorder(int from, int to)` — mutates both `Items` (via `ObservableCollection.Move`) and `Project.TrackViews` (the WinForms code already mutates `Project.TrackViews` directly — see [Form1.cs:996](VisualMusic/Forms/Form1.cs:996))
- `event Action SelectionChanged` — MainViewModel subscribes to refresh `SelectedTrackProps.MergedProps`

### `TrackItemViewModel`
- `Name`, `NormalColor` (`System.Windows.Media.Color`), `HilitedColor`, `IsSelected` (all `[ObservableProperty]`)
- Back-reference to `TrackView` so a future "refresh colors" call can re-read `TrackProps.MaterialProps.GetSysColor(...)` (logic exists at [Form1.cs:948](VisualMusic/Forms/Form1.cs:948))
- Helper to convert `System.Drawing.Color` → `System.Windows.Media.Color` (3-line static, no `IValueConverter` needed)

### `TrackPropsViewModel`
- `[ObservableProperty] TrackProps mergedProps` — set externally by MainViewModel after selection change
- `[ObservableProperty] int selectedTabIndex` — kept for future CTRL-drag copy phase
- `bool IsTrackSpecific => /* derived from selection */` — placeholder for future Light tab
- Empty otherwise; tab UserControls in future phases will set `DataContext="{Binding MergedProps}"`

### `SongPropsViewModel`
- Empty placeholder. Properties added when the left-panel phase ships.

### `MainViewModel` additions
- `[ObservableProperty] bool showSongProps` / `bool showTrackProps` (both default false)
- `public TrackListViewModel TrackList { get; }` / `TrackPropsViewModel SelectedTrackProps { get; }` / `SongPropsViewModel SongProps { get; }` — instantiated in constructor
- `public double ScrollPosition { get => Project?.NormSongPos ?? 0; set { if (Project != null) Project.NormSongPos = value; } }` — bound by the scrollbar
- `public void NotifyScrollPositionChanged() => OnPropertyChanged(nameof(ScrollPosition));` — invoked by the renderer's `OnSongPosChanged` delegate during playback
- `partial void OnProjectChanged(Project value)` — reset both toggles to false, `TrackList.Rebuild(value)`, clear `SelectedTrackProps.MergedProps`, `NotifyScrollPositionChanged()`

---

## Scrollbar wiring

The WPF [SongRenderer.NotifySongPosChanged()](VisualMusic/Controls/SongRenderer.cs:470) is currently a no-op. Restore the one-liner from WinForms [SongPanel.cs:639](VisualMusic/Controls/SongPanel.cs:639):

```csharp
public void NotifySongPosChanged() => OnSongPosChanged?.Invoke();
```

In [MainWindow.xaml.cs](VisualMusic/MainWindow.xaml.cs) `OnLoaded`, wire the delegate to the VM:

```csharp
monoGameHost.Renderer.OnSongPosChanged = () =>
    Dispatcher.InvokeAsync(() => vm.NotifyScrollPositionChanged());
```

Range stays 0–1 to match `NormSongPos` exactly — avoids the latent `(int)Project.SongLengthT` overflow at [Form1.cs:411](VisualMusic/Forms/Form1.cs:411) for long songs. `SmallChange=0.0625` and `LargeChange=1.0` match the existing `SongRenderer.SmallScrollStep` / `LargeScrollStep` constants used by NudgeBack / JumpBack commands.

---

## TrackList drag-to-reorder

Plain reorder only — **CTRL-drag copy deferred** to whichever per-tab phase first surfaces TrackProps content. Code-behind on [TrackListView.xaml.cs](VisualMusic/Controls/TrackListView.xaml.cs) (new):

- `PreviewMouseLeftButtonDown` → record start point + hit-tested item
- `MouseMove` → if past `SystemParameters.MinimumHorizontalDragDistance`, `DragDrop.DoDragDrop`
- `Drop` → resolve target row, call `TrackListViewModel.Reorder(from, to)`

Port the index-math from [Form1.cs:978–1046](VisualMusic/Forms/Form1.cs:978). ~70 lines total, no third-party dependency.

---

## Merge helper

Add an overload to [Project.cs:567](VisualMusic/Project.cs:567):

```csharp
public TrackProps mergeTrackProps(IEnumerable<int> indices) { /* same body, IEnumerable input */ }
```

Keep the original `ListView.SelectedIndexCollection` overload — Form1.cs still depends on it and Form1 cannot be deleted until later phases.

`TrackListViewModel.SelectionChanged` raises → MainViewModel reads `TrackList.SelectedItems` indices → calls `Project.mergeTrackProps(indices)` → assigns to `SelectedTrackProps.MergedProps`.

---

## Files to create / modify

**New** (8 files):
- [VisualMusic/ViewModels/TrackListViewModel.cs](VisualMusic/ViewModels/TrackListViewModel.cs)
- [VisualMusic/ViewModels/TrackItemViewModel.cs](VisualMusic/ViewModels/TrackItemViewModel.cs)
- [VisualMusic/ViewModels/TrackPropsViewModel.cs](VisualMusic/ViewModels/TrackPropsViewModel.cs)
- [VisualMusic/ViewModels/SongPropsViewModel.cs](VisualMusic/ViewModels/SongPropsViewModel.cs)
- [VisualMusic/Controls/TrackListView.xaml](VisualMusic/Controls/TrackListView.xaml) + `.xaml.cs` — `ListView` with 3-column `GridView` (Name / Normal swatch / Hilited swatch) + drag-drop code-behind
- [VisualMusic/Controls/TrackPropsView.xaml](VisualMusic/Controls/TrackPropsView.xaml) + `.xaml.cs` — `TabControl` with 5 empty `TabItem`s (Style / Material / Light / Spatial / Audio)
- [VisualMusic/Controls/SongPropsPanel.xaml](VisualMusic/Controls/SongPropsPanel.xaml) + `.xaml.cs` — empty `UserControl` placeholder

**Modified** (4 files):
- [VisualMusic/MainWindow.xaml](VisualMusic/MainWindow.xaml) — layout reshape, add `BoolVis` resource
- [VisualMusic/MainWindow.xaml.cs](VisualMusic/MainWindow.xaml.cs) — wire `OnSongPosChanged` after renderer is ready
- [VisualMusic/ViewModels/MainViewModel.cs](VisualMusic/ViewModels/MainViewModel.cs) — add toggles, child VM properties, `ScrollPosition`, `OnProjectChanged` partial, `NotifyScrollPositionChanged`
- [VisualMusic/Controls/SongRenderer.cs](VisualMusic/Controls/SongRenderer.cs:470) — implement `NotifySongPosChanged` (one line)
- [VisualMusic/Project.cs](VisualMusic/Project.cs:567) — add `mergeTrackProps(IEnumerable<int>)` overload (~5 lines)

**Not deleted in this phase**: Form1.cs / .designer.cs / .resx, TbSlider.cs, NoteStyleControl.cs, LineStyleControl.cs, BarStyleControl.cs, HueSatButton.cs, TwoD.cs, ListViewNF.cs. Form1 still references all the WinForms property controls, so deletion is deferred until each tab's WPF replacement ships in its own phase.

---

## Verification

1. **Build cleanly** — no new warnings.
2. **Empty start** — launch app with no project. Both toggle buttons disabled (`IsEnabled="{Binding HasProject}"`). Scrollbar disabled. MonoGame area visible, no side panels.
3. **Load a project** — File → Open or import a MIDI. Toggles become enabled, stay unchecked initially. Side panels remain hidden.
4. **Toggle right panel** — check "Track properties": right panel appears with track list populated (track 0 "Global" + real tracks) and 5 empty tabs. Colour swatches render correctly (compare against the WinForms screenshot/state).
5. **Toggle left panel** — check "Project properties": empty left panel appears at 216px wide. Both panels visible simultaneously.
6. **Drag splitter** — drag the splitter inside the right panel; track-list column resizes, tabs column fills the remainder.
7. **Drag-reorder a track** — drag track 2 above track 1 in the list. Order updates in both the list and the 3D visualization (the renderer reads from `Project.TrackViews`).
8. **Multi-select** — Ctrl+click two tracks in the list; selection changes propagate (no visible UI change until tab content lands, but no exceptions in the debugger output).
9. **Scrub the scrollbar** — drag the thumb; the 3D view jumps to that song position. Press Ctrl+Space to play; the scrollbar thumb advances in step with playback (proves the `OnSongPosChanged` round-trip works).
10. **Screen switch** — F2/F3/F4 switches to a browser screen; scrollbar hides (visibility bound to `CurrentScreen=Song`). Side panels stay where they were (their visibility is independent of screen — matches WinForms).

---

## Out of scope (future phases)

- Property tab contents (Style / Material / Light / Spatial / Audio) — one phase each
- Left panel content (`SongPropsPanel`) — its own phase
- CTRL+drag copy-properties between tracks
- `KeyFrameDGV` / `lyricsGridView` DataGrids — was already scheduled for Phase 7 of the original plan
- WinForms file deletions — happen incrementally as each phase replaces its corresponding WinForms control
- Persisting toggle state across sessions (WinForms didn't either)
