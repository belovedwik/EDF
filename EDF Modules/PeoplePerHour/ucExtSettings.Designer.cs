namespace Databox.Libs.PeoplePerHour
{
    partial class ucExtSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblTags = new System.Windows.Forms.Label();
            this.txtTags = new System.Windows.Forms.TextBox();
            this.bsSett = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.bsSett)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTags
            // 
            this.lblTags.AutoSize = true;
            this.lblTags.Location = new System.Drawing.Point(32, 47);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(32, 13);
            this.lblTags.TabIndex = 0;
            this.lblTags.Text = "TAGs";
            // 
            // txtTags
            // 
            this.txtTags.DataBindings.Add(new System.Windows.Forms.Binding("Tag", this.bsSett, "Tags", true));
            this.txtTags.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bsSett, "Tags", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtTags.Location = new System.Drawing.Point(35, 63);
            this.txtTags.Name = "txtTags";
            this.txtTags.Size = new System.Drawing.Size(149, 21);
            this.txtTags.TabIndex = 1;
            // 
            // bsSett
            // 
            this.bsSett.DataSource = typeof(Databox.Libs.PeoplePerHour.ExtSettings);
            // 
            // ucExtSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtTags);
            this.Controls.Add(this.lblTags);
            this.Name = "ucExtSettings";
            this.Size = new System.Drawing.Size(257, 244);
            ((System.ComponentModel.ISupportInitialize)(this.bsSett)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.BindingSource bsSett;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.TextBox txtTags;

    }
}
