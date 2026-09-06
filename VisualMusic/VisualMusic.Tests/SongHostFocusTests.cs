using VisualMusic.ViewModels;
using Xunit;

namespace VisualMusic.Tests
{
    public class SongHostFocusTests
    {
        [Fact]
        public void ShowSong_requests_host_focus()
        {
            var vm = new MainViewModel { CurrentScreen = AppScreen.ModBrowser };
            int n = 0;
            vm.FocusSongHost = () => n++;

            vm.ShowSongCommand.Execute(null);

            Assert.Equal(AppScreen.Song, vm.CurrentScreen);
            Assert.Equal(1, n);
        }

        [Fact]
        public void RequestSongHostFocusIfOnSong_skips_browser_screens()
        {
            var vm = new MainViewModel { CurrentScreen = AppScreen.SidBrowser };
            int n = 0;
            vm.FocusSongHost = () => n++;

            vm.RequestSongHostFocusIfOnSong();

            Assert.Equal(0, n);
        }

        [Fact]
        public void RequestSongHostFocusIfOnSong_invokes_on_song_screen()
        {
            var vm = new MainViewModel { CurrentScreen = AppScreen.Song };
            int n = 0;
            vm.FocusSongHost = () => n++;

            vm.RequestSongHostFocusIfOnSong();

            Assert.Equal(1, n);
        }
    }
}
