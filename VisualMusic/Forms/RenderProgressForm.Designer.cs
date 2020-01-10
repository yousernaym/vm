namespace VisualMusic
{
	partial class RenderProgressForm
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
			this.SuspendLayout();
			// 
			// RenderProgressForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(393, 97);
			this.Name = "RenderProgressForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RenderProgressForm_FormClosing);
			this.VisibleChanged += new System.EventHandler(this.RenderProgressForm_VisibleChanged);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
