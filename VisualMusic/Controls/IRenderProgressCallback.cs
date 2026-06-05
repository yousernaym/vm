using System.Threading;

namespace VisualMusic
{
    public interface IRenderProgressCallback
    {
        bool Cancel { get; }
        object CancelLock { get; }
        // Signalled the moment the user cancels. Lets a job react immediately (e.g. abort a
        // network request) instead of only noticing Cancel the next time it polls — which a
        // stalled download waiting on an unresponsive server never gets to do.
        CancellationToken CancelToken { get; }
        void ShowMessage(string message);
        void UpdateProgress(float normProgress);
    }
}
