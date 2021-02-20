namespace VisualMusic
{
	partial class TrackPropsTypeForm
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
			this.styleCb = new System.Windows.Forms.CheckBox();
			this.materialCb = new System.Windows.Forms.CheckBox();
			this.lightCb = new System.Windows.Forms.CheckBox();
			this.spatialCb = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.Location = new System.Drawing.Point(12, 68);
			this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Location = new System.Drawing.Point(93, 68);
			// 
			// styleCb
			// 
			this.styleCb.AutoSize = true;
			this.styleCb.Location = new System.Drawing.Point(12, 12);
			this.styleCb.Name = "styleCb";
			this.styleCb.Size = new System.Drawing.Size(49, 17);
			this.styleCb.TabIndex = 3;
			this.styleCb.Text = "Style";
			this.styleCb.UseVisualStyleBackColor = true;
			// 
			// materialCb
			// 
			this.materialCb.AutoSize = true;
			this.materialCb.Location = new System.Drawing.Point(67, 12);
			this.materialCb.Name = "materialCb";
			this.materialCb.Size = new System.Drawing.Size(63, 17);
			this.materialCb.TabIndex = 3;
			this.materialCb.Text = "Material";
			this.materialCb.UseVisualStyleBackColor = true;
			// 
			// lightCb
			// 
			this.lightCb.AutoSize = true;
			this.lightCb.Location = new System.Drawing.Point(12, 35);
			this.lightCb.Name = "lightCb";
			this.lightCb.Size = new System.Drawing.Size(49, 17);
			this.lightCb.TabIndex = 3;
			this.lightCb.Text = "Light";
			this.lightCb.UseVisualStyleBackColor = true;
			// 
			// spatialCb
			// 
			this.spatialCb.AutoSize = true;
			this.spatialCb.Location = new System.Drawing.Point(67, 35);
			this.spatialCb.Name = "spatialCb";
			this.spatialCb.Size = new System.Drawing.Size(58, 17);
			this.spatialCb.TabIndex = 3;
			this.spatialCb.Text = "Spatial";
			this.spatialCb.UseVisualStyleBackColor = true;
			// 
			// TrackPropsTypeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(180, 103);
			this.Controls.Add(this.lightCb);
			this.Controls.Add(this.spatialCb);
			this.Controls.Add(this.materialCb);
			this.Controls.Add(this.styleCb);
			this.Name = "TrackPropsTypeForm";
			this.VisibleChanged += new System.EventHandler(this.TrackPropsTypeForm_VisibleChanged);
			this.Controls.SetChildIndex(this.okBtn, 0);
			this.Controls.SetChildIndex(this.cancelBtn, 0);
			this.Controls.SetChildIndex(this.styleCb, 0);
			this.Controls.SetChildIndex(this.materialCb, 0);
			this.Controls.SetChildIndex(this.spatialCb, 0);
			this.Controls.SetChildIndex(this.lightCb, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox styleCb;
		private System.Windows.Forms.CheckBox materialCb;
		private System.Windows.Forms.CheckBox lightCb;
		private System.Windows.Forms.CheckBox spatialCb;
	}
}