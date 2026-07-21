using System;
using System.IO;
using System.Runtime.InteropServices;

namespace VisualMusic.Tests
{
    static class TestFiles
    {
        /// <summary>
        /// Resolves a fixture under the test assembly's copied <c>test-files/</c> tree
        /// (see Content items in the test csproj).
        /// </summary>
        public static string PathTo(string relative)
        {
            string candidate = Path.Combine(AppContext.BaseDirectory, "test-files", relative);
            if (!File.Exists(candidate))
            {
                throw new FileNotFoundException(
                    "Could not locate test fixture '" + relative + "' under " +
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
    }
}
