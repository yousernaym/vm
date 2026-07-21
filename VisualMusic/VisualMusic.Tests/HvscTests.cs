using System.IO;
using Xunit;

namespace VisualMusic.Tests
{
    public class HvscTests
    {
        [Fact]
        public void GetSongLengths_reads_matching_md5_line()
        {
            string sid = TestFiles.PathTo("libRemuxer", "minimal.sid");
            string hash = Hvsc.ComputeMd5Hex(sid);

            using var db = TestFiles.TempPath.File("vm_songlengths_", ".md5");
            File.WriteAllText(db.Path, $"; comment\n{hash}=3:21 1:05\notherhash=0:01\n");
            string[] lengths = Hvsc.GetSongLengths(sid, db.Path);
            Assert.NotNull(lengths);
            Assert.Equal(new[] { "3:21", "1:05" }, lengths);
        }

        [Fact]
        public void GetSongLengths_missing_entry_returns_null()
        {
            string sid = TestFiles.PathTo("libRemuxer", "minimal.sid");
            using var db = TestFiles.TempPath.File("vm_songlengths_", ".md5");
            File.WriteAllText(db.Path, "00000000000000000000000000000000=1:00\n");
            Assert.Null(Hvsc.GetSongLengths(sid, db.Path));
        }
    }
}
