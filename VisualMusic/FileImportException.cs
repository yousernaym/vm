using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualMusic
{
	public enum ImportError { Missing, Corrupt }
	public enum ImportFileType { Note, Audio }
	public class FileImportException : Exception
	{
		public ImportError Error { get; private set; }
		public ImportFileType FileType { get; private set; }
		public string FileName { get; private set; }
		new public string Message { get; private set; }
		
		public FileImportException(string message, ImportError error, ImportFileType fileType, string fileName)
		{
			Message = message;
			Error = error;
			FileType = fileType;
			FileName = fileName;
		}
	}
}
