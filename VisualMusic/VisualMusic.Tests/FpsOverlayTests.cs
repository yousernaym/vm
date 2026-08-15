using VisualMusic.ViewModels;
using Xunit;

namespace VisualMusic.Tests
{
    public class FpsOverlayTests
    {
        [Fact]
        public void ToggleFpsOverlay_flips_the_view_model_flag()
        {
            var vm = new MainViewModel();
            Assert.False(vm.ShowFpsOverlay);

            vm.ToggleFpsOverlayCommand.Execute(null);
            Assert.True(vm.ShowFpsOverlay);

            vm.ToggleFpsOverlayCommand.Execute(null);
            Assert.False(vm.ShowFpsOverlay);
        }
    }
}
