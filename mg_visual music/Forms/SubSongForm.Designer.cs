namespace Visual_Music.Forms
{
	partial class SubSongForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.subSongsLB = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.Location = new System.Drawing.Point(116, 48);
			this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Location = new System.Drawing.Point(197, 48);
			// 
			// subSongsLB
			// 
			this.subSongsLB.Dock = System.Windows.Forms.DockStyle.Top;
			this.subSongsLB.FormattingEnabled = true;
			this.subSongsLB.Location = new System.Drawing.Point(0, 0);
			this.subSongsLB.Name = "subSongsLB";
			this.subSongsLB.Size = new System.Drawing.Size(284, 30);
			this.subSongsLB.TabIndex = 3;
			this.subSongsLB.Resize += new System.EventHandler(this.subSongsLB_Resize);
			// 
			// SubSongForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(284, 83);
			this.Controls.Add(this.subSongsLB);
			this.Name = "SubSongForm";
			this.Text = "Select sub song";
			this.Controls.SetChildIndex(this.okBtn, 0);
			this.Controls.SetChildIndex(this.cancelBtn, 0);
			this.Controls.SetChildIndex(this.subSongsLB, 0);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox subSongsLB;
	}
}
