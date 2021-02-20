
namespace VisualMusic
{
	partial class WaitForFileSearchForm
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
			this.dirTb = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// messageLabel
			// 
			this.messageLabel.Location = new System.Drawing.Point(12, 9);
			this.messageLabel.Size = new System.Drawing.Size(66, 13);
			this.messageLabel.Text = "Searching in";
			// 
			// cancelBtn
			// 
			this.cancelBtn.Location = new System.Drawing.Point(234, 55);
			// 
			// dirTb
			// 
			this.dirTb.Location = new System.Drawing.Point(12, 25);
			this.dirTb.Name = "dirTb";
			this.dirTb.ReadOnly = true;
			this.dirTb.Size = new System.Drawing.Size(297, 20);
			this.dirTb.TabIndex = 2;
			// 
			// WaitForFileSearchForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(321, 90);
			this.Controls.Add(this.dirTb);
			this.Name = "WaitForFileSearchForm";
			this.Controls.SetChildIndex(this.cancelBtn, 0);
			this.Controls.SetChildIndex(this.messageLabel, 0);
			this.Controls.SetChildIndex(this.dirTb, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox dirTb;
	}
}