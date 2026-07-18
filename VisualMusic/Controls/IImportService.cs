namespace VisualMusic
{
    public interface IImportService
    {
        /// <param name="preferredFileType">
        /// Browser context (Mod / SID / MIDI). Used when the extension is valid for that type
        /// so ambiguous names like .mus follow the browser that started the download.
        /// </param>
        void ImportFromUrl(string url, string suggestedFileName, FileType preferredFileType);
    }
}
