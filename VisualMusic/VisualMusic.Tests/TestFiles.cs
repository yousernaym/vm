using System;
using System.IO;
using System.Runtime.InteropServices;

namespace VisualMusic.Tests
{
    static class TestFiles
    {
        /// <summary>
        /// Resolves a fixture under the test assembly's copied <c>test-files/&lt;owner&gt;/</c>
        /// tree (see None items in the test csproj). <paramref name="owner"/> is the
        /// submodule namespace (<c>midiLib</c>, <c>Media</c>, <c>MidMix</c>, <c>libRemuxer</c>).
        /// </summary>
        public static string PathTo(string owner, string relative)
        {
            string candidate = Path.Combine(AppContext.BaseDirectory, "test-files", owner, relative);
            if (!File.Exists(candidate))
            {
                throw new FileNotFoundException(
                    "Could not locate test fixture '" + owner + "/" + relative + "' under " +
                    Path.Combine(AppContext.BaseDirectory, "test-files") + ".");
            }
            return candidate;
        }

        /// <summary>
        /// Loads a native DLL from beside the test assembly, or throws with build guidance
        /// if it is missing (Integration tests need a prior VisualMusic.sln Any CPU build).
        /// </summary>
        public static void EnsureNativeLoaded(string dllName)
        {
            string dll = Path.Combine(AppContext.BaseDirectory, dllName);
            if (!File.Exists(dll))
                throw new FileNotFoundException(
                    dllName + " not found beside the test assembly. Build VisualMusic.sln (Any CPU) so VisualMusic.Tests copies native runtime DLLs.");
            NativeLibrary.Load(dll);
        }

        /// <summary>
        /// Ensures <c>remuxer/remuxer.exe</c> + <c>libRemuxer.dll</c> sit beside the test assembly
        /// (same layout <see cref="Program.Dir"/> / ImportSong expects).
        /// </summary>
        public static void EnsureRemuxerAvailable()
        {
            string exe = Path.Combine(AppContext.BaseDirectory, "remuxer", "remuxer.exe");
            string dll = Path.Combine(AppContext.BaseDirectory, "remuxer", "libRemuxer.dll");
            if (!File.Exists(exe) || !File.Exists(dll))
                throw new FileNotFoundException(
                    "remuxer/remuxer.exe (+ libRemuxer.dll) not found beside the test assembly. " +
                    "Build VisualMusic.sln (Any CPU) so VisualMusic.Tests copies the Remuxer package.");
        }

        /// <summary>
        /// Unique path under the system temp dir; deletes the file or directory on Dispose.
        /// </summary>
        public sealed class TempPath : IDisposable
        {
            public string Path { get; }
            readonly bool _isDirectory;

            TempPath(string path, bool isDirectory)
            {
                Path = path;
                _isDirectory = isDirectory;
            }

            /// <summary>Unique file path (not created). <paramref name="extension"/> may include a leading dot.</summary>
            public static TempPath File(string prefix, string extension)
            {
                if (extension.Length > 0 && extension[0] != '.')
                    extension = "." + extension;
                return new TempPath(
                    System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                        prefix + Guid.NewGuid().ToString("N") + extension),
                    isDirectory: false);
            }

            /// <summary>Unique directory; created empty.</summary>
            public static TempPath Directory(string prefix)
            {
                string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                    prefix + Guid.NewGuid().ToString("N"));
                System.IO.Directory.CreateDirectory(path);
                return new TempPath(path, isDirectory: true);
            }

            /// <summary>
            /// Unique directory whose name includes non-ASCII characters. Used by Media/MidMix
            /// Integration tests so a regression to ANSI P/Invoke marshalling fails loudly —
            /// ASCII-only temp paths would still pass under the system code page.
            /// </summary>
            public static TempPath NonAsciiDirectory(string asciiPrefix = "vm_utf8_")
            {
                // café is Latin-1; 日本語 is outside Windows-1252. Together they corrupt under ANSI.
                string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                    asciiPrefix + "café_日本語_" + Guid.NewGuid().ToString("N"));
                System.IO.Directory.CreateDirectory(path);
                return new TempPath(path, isDirectory: true);
            }

            public void Dispose()
            {
                try
                {
                    if (_isDirectory)
                    {
                        if (System.IO.Directory.Exists(Path))
                            System.IO.Directory.Delete(Path, recursive: true);
                    }
                    else if (System.IO.File.Exists(Path))
                        System.IO.File.Delete(Path);
                }
                catch { /* best-effort cleanup */ }
            }
        }
    }
}
