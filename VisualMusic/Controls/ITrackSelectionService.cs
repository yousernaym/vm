namespace VisualMusic
{
    public interface ITrackSelectionService
    {
        int TrackListCount { get; }
        void SetTrackSelected(int index, bool selected);
    }
}
