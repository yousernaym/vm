namespace VisualMusic
{
    public interface IRenderProgressCallback
    {
        bool Cancel { get; }
        object CancelLock { get; }
        void ShowMessage(string message);
        void UpdateProgress(float normProgress);
    }
}
