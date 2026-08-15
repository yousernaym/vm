using System;
using System.Collections.Generic;
using System.IO;
using VisualMusic.Keyframes;

namespace VisualMusic
{
    public enum MissingFileKind
    {
        Note,
        MasterAudio,
        TrackAudio,
        Texture,
        Heightmap,
        Background
    }

    public enum MissingFileAction
    {
        Browse,
        SearchFolder,
        Ignore,
        Abort
    }

    public sealed class MissingFileRef
    {
        public MissingFileKind Kind { get; init; }
        public string Path { get; init; }
        public FileType NoteFileType { get; init; }

        public string RoleLabel => Kind switch
        {
            MissingFileKind.Note => "Note file",
            MissingFileKind.MasterAudio => "Audio file",
            MissingFileKind.TrackAudio => "Track audio file",
            MissingFileKind.Texture => "Texture",
            MissingFileKind.Heightmap => "Heightmap",
            MissingFileKind.Background => "Background image",
            _ => "File"
        };

        public string OpenFilter => Kind switch
        {
            MissingFileKind.Note => ImportFileFormats.NoteOpenFilter(NoteFileType),
            MissingFileKind.MasterAudio or MissingFileKind.TrackAudio => AudioOpenFilter,
            _ => ImageOpenFilter
        };

        public const string AudioOpenFilter = "Audio files|*.wav;*.flac;*.mp3;*.ogg|All files|*.*";
        public const string ImageOpenFilter =
            "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.dds|All files|*.*";
    }

    public sealed class MissingFileResolveResult
    {
        public bool Aborted { get; init; }
        public bool ReplacedAny { get; init; }

        public static MissingFileResolveResult Continue(bool replacedAny)
            => new() { Aborted = false, ReplacedAny = replacedAny };

        public static MissingFileResolveResult Abort()
            => new() { Aborted = true };
    }

    public interface IMissingFilePrompt
    {
        MissingFileAction Prompt(MissingFileRef file);
        /// <returns>Selected path, or null if the picker was cancelled.</returns>
        string PickReplacementFile(MissingFileRef file);
        /// <returns>Selected directory, or null if the picker was cancelled.</returns>
        string PickSearchFolder(MissingFileRef file);
        void NotifyNotFound(string fileName, string directory);
    }

    /// <summary>
    /// Enumerates missing project file references and relocates or ignores them before Open continues.
    /// </summary>
    public static class ProjectMissingFiles
    {
        static readonly StringComparer PathComparer = StringComparer.OrdinalIgnoreCase;

        public static MissingFileResolveResult Resolve(Project project, IMissingFilePrompt prompt)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (prompt == null) throw new ArgumentNullException(nameof(prompt));

            var ignored = new HashSet<string>(PathComparer);
            bool replacedAny = false;

            while (true)
            {
                var missing = EnumerateMissing(project, ignored);
                if (missing.Count == 0)
                    return MissingFileResolveResult.Continue(replacedAny);

                var file = missing[0];
                switch (prompt.Prompt(file))
                {
                    case MissingFileAction.Abort:
                        return MissingFileResolveResult.Abort();

                    case MissingFileAction.Ignore:
                        Ignore(project, file, ignored);
                        break;

                    case MissingFileAction.Browse:
                        string picked = prompt.PickReplacementFile(file);
                        if (!string.IsNullOrWhiteSpace(picked))
                        {
                            ApplyReplacement(project, file.Path, picked);
                            replacedAny = true;
                        }
                        break;

                    case MissingFileAction.SearchFolder:
                        string dir = prompt.PickSearchFolder(file);
                        if (string.IsNullOrWhiteSpace(dir))
                            break;
                        string name = FileNameForSearch(file.Path);
                        string found = FindFileByName(dir, name, file.Path);
                        if (!string.IsNullOrEmpty(found))
                        {
                            ApplyReplacement(project, file.Path, found);
                            replacedAny = true;
                        }
                        else
                            prompt.NotifyNotFound(name, dir);
                        break;
                }
            }
        }

        /// <summary>Distinct missing paths, in prompt order (note, master audio, track audio, visuals).</summary>
        public static List<MissingFileRef> EnumerateMissing(Project project, ISet<string> ignored = null)
        {
            var result = new List<MissingFileRef>();
            var seen = new HashSet<string>(PathComparer);
            bool skipTrackAudio = WillRegenerateTrackAudio(project);

            foreach (var slot in EnumerateSlots(project))
            {
                if (string.IsNullOrWhiteSpace(slot.Path))
                    continue;
                if (ignored != null && ignored.Contains(slot.Path))
                    continue;
                if (slot.Kind == MissingFileKind.TrackAudio && skipTrackAudio)
                    continue;
                if (!IsMissing(slot.Path))
                    continue;
                if (!seen.Add(slot.Path))
                    continue;

                result.Add(new MissingFileRef
                {
                    Kind = slot.Kind,
                    Path = slot.Path,
                    NoteFileType = slot.NoteFileType
                });
            }
            return result;
        }

        public static string FileNameForSearch(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "";
            if (path.IsUrl())
            {
                try { return Path.GetFileName(new Uri(path).LocalPath); }
                catch { /* fall through */ }
            }
            try { return Path.GetFileName(path); }
            catch { return ""; }
        }

        /// <summary>
        /// Recursive case-insensitive filename search. Several hits: longest matching path-suffix
        /// with <paramref name="originalPath"/>, then shortest full path.
        /// </summary>
        public static string FindFileByName(string directory, string fileName, string originalPath = null)
        {
            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(fileName))
                return null;

            var matches = new List<string>();
            CollectFilesNamed(directory, fileName, matches);
            if (matches.Count == 0)
                return null;
            if (matches.Count == 1)
                return matches[0];
            return PreferBestMatch(matches, originalPath);
        }

        static void Ignore(Project project, MissingFileRef file, HashSet<string> ignored)
        {
            ignored.Add(file.Path);
            var io = project.ImportOptions;
            if (io == null) return;
            if (file.Kind == MissingFileKind.Note)
                io.SkipNoteLoad = true;
            else if (file.Kind == MissingFileKind.MasterAudio)
                io.SkipAudioLoad = true;
        }

        static void ApplyReplacement(Project project, string oldPath, string newPath)
        {
            foreach (var slot in EnumerateSlots(project))
            {
                if (PathsEqual(slot.Path, oldPath))
                    slot.Set(newPath);
            }
        }

        static bool WillRegenerateTrackAudio(Project project)
        {
            var io = project.ImportOptions;
            if (io == null || !io.TrackAudio || io.SkipNoteLoad || io.SavedMidi)
                return false;
            if (io.NoteFileType is FileType.Midi or FileType.Audio)
                return false;
            string note = !string.IsNullOrWhiteSpace(io.NotePath) ? io.NotePath : io.RawNotePath;
            return !string.IsNullOrWhiteSpace(note) && !note.IsUrl() && File.Exists(note);
        }

        static bool IsMissing(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            if (path.IsUrl())
                return true;
            try { return !File.Exists(path); }
            catch { return true; }
        }

        static bool PathsEqual(string a, string b)
            => PathComparer.Equals(a ?? "", b ?? "");

        sealed class FileSlot
        {
            public MissingFileKind Kind;
            public string Path;
            public FileType NoteFileType;
            public Action<string> Set;
        }

        static IEnumerable<FileSlot> EnumerateSlots(Project project)
        {
            var io = project.ImportOptions;
            var noteType = io?.NoteFileType ?? FileType.Midi;

            if (io != null && io.NoteFileType != FileType.Audio && !io.SkipNoteLoad)
            {
                if (io.SavedMidi)
                {
                    yield return Slot(MissingFileKind.Note, io.MidiOutputPath, noteType,
                        p => io.MidiOutputPath = p);
                }
                else
                {
                    try { io.SetNotePath(); }
                    catch (FileImportException) { /* treat as missing */ }

                    string notePath = !string.IsNullOrWhiteSpace(io.NotePath) ? io.NotePath : io.RawNotePath;
                    yield return Slot(MissingFileKind.Note, notePath, noteType, io.ReplaceNotePath);
                }
            }

            if (io != null && io.HasSuppliedAudio && !io.SkipAudioLoad)
            {
                yield return Slot(MissingFileKind.MasterAudio, io.AudioPath, noteType,
                    p => io.AudioPath = p);
            }

            var views = project.TrackViews;
            if (views != null)
            {
                for (int i = 0; i < views.Count; i++)
                {
                    var tv = views[i];
                    var ap = tv.TrackProps.AudioProps;
                    if (ap.VoiceAudioFiles is { Count: > 0 } voices)
                    {
                        for (int v = 0; v < voices.Count; v++)
                        {
                            int idx = v;
                            var list = voices;
                            yield return Slot(MissingFileKind.TrackAudio, list[idx].Path, noteType, p =>
                            {
                                list[idx] = (list[idx].Channel, p);
                            });
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(ap.Filename))
                    {
                        yield return Slot(MissingFileKind.TrackAudio, ap.Filename, noteType,
                            p => ap.Filename = p);
                    }

                    var tex = tv.TrackProps.MaterialProps.TexProps;
                    if (tex != null && !string.IsNullOrWhiteSpace(tex.Path))
                    {
                        yield return Slot(MissingFileKind.Texture, tex.Path, noteType,
                            p => tex.Path = p);
                    }
                    var hmap = tv.TrackProps.MaterialProps.HmapProps;
                    if (hmap != null && !string.IsNullOrWhiteSpace(hmap.Path))
                    {
                        yield return Slot(MissingFileKind.Heightmap, hmap.Path, noteType,
                            p => hmap.Path = p);
                    }
                }
            }

            string bkg = project.Props?.BackgroundImagePath;
            if (!string.IsNullOrWhiteSpace(bkg))
            {
                yield return Slot(MissingFileKind.Background, bkg, noteType,
                    p => project.Props.BackgroundImagePath = p);
            }

            var kfs = project.PropertyKeyframes;
            if (kfs?.Tracks == null)
                yield break;

            foreach (var (id, track) in kfs.Tracks)
            {
                MissingFileKind kind;
                if (id == "proj/BackgroundImagePath")
                    kind = MissingFileKind.Background;
                else if (id.EndsWith("/TexturePath", StringComparison.Ordinal))
                    kind = MissingFileKind.Texture;
                else
                    continue;

                foreach (var kf in track.Frames)
                {
                    if (kf.Value is not StringKfValue sv || string.IsNullOrWhiteSpace(sv.S))
                        continue;
                    var captured = sv;
                    yield return Slot(kind, captured.S, noteType, p => captured.S = p);
                }
            }
        }

        static FileSlot Slot(MissingFileKind kind, string path, FileType noteType, Action<string> set)
            => new() { Kind = kind, Path = path, NoteFileType = noteType, Set = set };

        static void CollectFilesNamed(string dir, string fileName, List<string> matches)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(dir, fileName, SearchOption.TopDirectoryOnly))
                    matches.Add(file);
                foreach (var sub in Directory.EnumerateDirectories(dir))
                    CollectFilesNamed(sub, fileName, matches);
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
            catch (IOException) { }
        }

        static string PreferBestMatch(List<string> matches, string originalPath)
        {
            string[] origParts = SplitPath(originalPath);
            int bestScore = -1;
            int bestLen = int.MaxValue;
            string best = matches[0];
            foreach (var m in matches)
            {
                int score = CommonSuffixLength(origParts, SplitPath(m));
                if (score > bestScore || (score == bestScore && m.Length < bestLen))
                {
                    bestScore = score;
                    bestLen = m.Length;
                    best = m;
                }
            }
            return best;
        }

        static string[] SplitPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Array.Empty<string>();
            return path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);
        }

        static int CommonSuffixLength(string[] a, string[] b)
        {
            int n = 0;
            int i = a.Length - 1;
            int j = b.Length - 1;
            while (i >= 0 && j >= 0 && PathComparer.Equals(a[i], b[j]))
            {
                n++;
                i--;
                j--;
            }
            return n;
        }
    }
}
