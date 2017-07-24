using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace mg_visual_music
{
	class Project
	{
		public static string NoteFilePath =  Path.GetTempPath() + @"\notefile";
		public static string AudioFilePath = Path.GetTempPath() + @"\audiofile";
		ZipArchive zipArchive;
		MemoryStream stream;
		public Project()
		{

		}
		public void create(string noteFile, string audioFile)
		{
			if (stream != null)
				stream.Dispose();
			stream = new MemoryStream();

			using (zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, true))
			{
				zipArchive.CreateEntryFromFile(noteFile, "notefile" + Path.GetExtension(noteFile));
				using (FileStream file = File.Open(noteFile, FileMode.Open))
				{


				}
			}
		}

	}
}
