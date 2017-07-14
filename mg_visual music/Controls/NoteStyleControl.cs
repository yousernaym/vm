using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visual_Music
{
	abstract public partial class NoteStyleControl : UserControl
	{
		protected Form1 parentForm;
		protected SongPanel songPanel;
		protected bool UpdatingControls => parentForm.UpdatingControls;
		protected ListViewNF TrackList => parentForm.TrackList;

		public NoteStyleControl()
		{
			InitializeComponent();
			
		}
		public void init(Form1 parent, SongPanel spanel)
		{
			parentForm = parent;
			songPanel = spanel;
		}
	}
}
