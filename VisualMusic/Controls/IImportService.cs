namespace VisualMusic
{
    public interface IImportService
    {
        /// <summary>
        /// Imports a browser download after Chromium has written it to <paramref name="downloadedFilePath"/>.
        /// Chromium must perform the transfer so browser cookies, referrer, and request state are preserved.
        /// </summary>
        /// <param name="preferredFileType">
        /// Browser context (Mod / SID / MIDI). Used when the extension is valid for that type
        /// so ambiguous names like .mus follow the browser that started the download.
        /// </param>
        void ImportBrowserDownload(string url, string downloadedFilePath, string suggestedFileName,
            FileType preferredFileType);
    }
}
