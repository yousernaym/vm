using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visual_Music
{
	public partial class TrackPropsTypeForm : BaseDialog
	{
		int styleFlag => styleCb.Checked ? (int)TrackPropsType.TPT_Style : 0;
		int materialFlag => materialCb.Checked ? (int)TrackPropsType.TPT_Material : 0;
		int lightFlag => lightCb.Checked ? (int)TrackPropsType.TPT_Light : 0;
		int spatialFlag => spatialCb.Checked ? (int)TrackPropsType.TPT_Spatial : 0;

		public int TypeFlags
		{
			get
			{
				return styleFlag | materialFlag | lightFlag | spatialFlag;
			}
			set
			{
				styleCb.Checked = styleCb.Enabled = (value & (int)TrackPropsType.TPT_Style) > 0;
				materialCb.Checked = materialCb.Enabled = (value & (int)TrackPropsType.TPT_Material) > 0;
				lightCb.Checked = lightCb.Enabled = (value & (int)TrackPropsType.TPT_Light) > 0;
				spatialCb.Checked = spatialCb.Enabled = (value & (int)TrackPropsType.TPT_Spatial) > 0;
			}
		}
		public TrackPropsTypeForm(int typeFlags) : base()
		{
			InitializeComponent();
			TypeFlags = typeFlags;
		}

		private void TrackPropsTypeForm_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
				Close();
		}
	}
}
