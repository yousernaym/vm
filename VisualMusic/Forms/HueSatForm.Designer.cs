namespace VisualMusic
{
	partial class HueSatForm
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
			this.twoDHueSat1 = new VisualMusic.TwoDHueSat();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.Location = new System.Drawing.Point(112, 247);
			this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Location = new System.Drawing.Point(193, 247);
			// 
			// twoDHueSat1
			// 
			this.twoDHueSat1.BackColor = System.Drawing.SystemColors.Control;
			this.twoDHueSat1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.twoDHueSat1.Dock = System.Windows.Forms.DockStyle.Top;
			this.twoDHueSat1.Hue = 0F;
			this.twoDHueSat1.Location = new System.Drawing.Point(0, 0);
			this.twoDHueSat1.Name = "twoDHueSat1";
			this.twoDHueSat1.Saturation = 0F;
			this.twoDHueSat1.SelectionSize = 20;
			this.twoDHueSat1.SetSelectionColor = System.Drawing.Color.Black;
			this.twoDHueSat1.Size = new System.Drawing.Size(280, 239);
			this.twoDHueSat1.TabIndex = 3;
			this.twoDHueSat1.SelectionChanged += new System.EventHandler(this.twoDHueSat1_SelectionChanged);
			// 
			// HueSatForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(280, 282);
			this.Controls.Add(this.twoDHueSat1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "HueSatForm";
			this.Text = "Hue/Saturation picker";
			this.Controls.SetChildIndex(this.okBtn, 0);
			this.Controls.SetChildIndex(this.cancelBtn, 0);
			this.Controls.SetChildIndex(this.twoDHueSat1, 0);
			this.ResumeLayout(false);

		}

		#endregion

		private TwoDHueSat twoDHueSat1;
	}
}