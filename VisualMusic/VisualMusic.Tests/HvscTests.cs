using System;
using System.IO;
using Xunit;

namespace VisualMusic.Tests
{
    public class HvscTests
    {
        [Fact]
        public void GetSongLengths_reads_matching_md5_line()
        {
            string sid = TestFiles.PathTo("minimal.sid");
            string hash = Hvsc.ComputeMd5Hex(sid);

            string db = Path.Combine(Path.GetTempPath(), "vm_songlengths_" + Guid.NewGuid().ToString("N") + ".md5");
            File.WriteAllText(db, $"; comment\n{hash}=3:21 1:05\notherhash=0:01\n");
            try
            {
                string[] lengths = Hvsc.GetSongLengths(sid, db);
                Assert.NotNull(lengths);
                Assert.Equal(new[] { "3:21", "1:05" }, lengths);
            }
            finally
            {
                File.Delete(db);
            }
        }

        [Fact]
        public void GetSongLengths_missing_entry_returns_null()
        {
            string sid = TestFiles.PathTo("minimal.sid");
            string db = Path.Combine(Path.GetTempPath(), "vm_songlengths_" + Guid.NewGuid().ToString("N") + ".md5");
            File.WriteAllText(db, "00000000000000000000000000000000=1:00\n");
            try
            {
                Assert.Null(Hvsc.GetSongLengths(sid, db));
            }
            finally
            {
                File.Delete(db);
            }
        }
    }
}
