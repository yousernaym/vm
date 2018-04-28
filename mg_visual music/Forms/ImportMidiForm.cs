using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

namespace Visual_Music
{
	public partial class ImportMidiForm : SourceFileForm
	{
		static public readonly string[] Formats = Properties.Resources.MidiFormats.ToLower().Split(null);
		public ImportMidiForm()
		{
			InitializeComponent();
		}
		public ImportMidiForm(Form1 _parent) : base(_parent)
		{
			InitializeComponent();
		}

		private void Ok_Click(object sender, EventArgs e)
		{
			importFiles();
		}

		public override void importFiles()
		{
			importFiles(new MidiImportOptions());
		}

		private void ImportMidiForm_Load(object sender, EventArgs e)
		{
			createFormatFilter("Midi files", Formats);
		}
	}

	[Serializable()]
	class MidiImportOptions : ImportOptions
	{
		public MidiImportOptions() : base(Midi.FileType.Midi)
		{
			MixdownType = Midi.MixdownType.None;
		}

		public MidiImportOptions(SerializationInfo info, StreamingContext context) : base(info, context)
		{

		}
	}
}
