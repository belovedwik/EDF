namespace Databox.Libs.Sanmar
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
            this.bsSett = new System.Windows.Forms.BindingSource(this.components);
            this.FTPWorkingDir = new System.Windows.Forms.TextBox();
            this.FTPPort = new System.Windows.Forms.TextBox();
            this.lblFTPDir = new System.Windows.Forms.Label();
            this.lblFTPPort = new System.Windows.Forms.Label();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.lblDtFrom = new System.Windows.Forms.Label();
            this.lblDtTo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.bsSett)).BeginInit();
            this.SuspendLayout();
            // 
            // bsSett
            // 
            this.bsSett.DataSource = typeof(Databox.Libs.Sanmar.ExtSettings);
            // 
            // FTPWorkingDir
            // 
            this.FTPWorkingDir.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bsSett, "FTPWorkingDirectory", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.FTPWorkingDir.DataBindings.Add(new System.Windows.Forms.Binding("Tag", this.bsSett, "FTPWorkingDirectory", true));
            this.FTPWorkingDir.Location = new System.Drawing.Point(91, 56);
            this.FTPWorkingDir.Name = "FTPWorkingDir";
            this.FTPWorkingDir.Size = new System.Drawing.Size(140, 21);
            this.FTPWorkingDir.TabIndex = 0;
            this.FTPWorkingDir.Text = "000090463Status";
            // 
            // FTPPort
            // 
            this.FTPPort.DataBindings.Add(new System.Windows.Forms.Binding("Tag", this.bsSett, "FTPPort", true));
            this.FTPPort.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bsSett, "FTPPort", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.FTPPort.Location = new System.Drawing.Point(91, 83);
            this.FTPPort.Name = "FTPPort";
            this.FTPPort.Size = new System.Drawing.Size(140, 21);
            this.FTPPort.TabIndex = 1;
            this.FTPPort.Text = "2200";
            // 
            // lblFTPDir
            // 
            this.lblFTPDir.AutoSize = true;
            this.lblFTPDir.Location = new System.Drawing.Point(13, 59);
            this.lblFTPDir.Name = "lblFTPDir";
            this.lblFTPDir.Size = new System.Drawing.Size(72, 13);
            this.lblFTPDir.TabIndex = 2;
            this.lblFTPDir.Text = "FTP Directory";
            // 
            // lblFTPPort
            // 
            this.lblFTPPort.AutoSize = true;
            this.lblFTPPort.Location = new System.Drawing.Point(37, 86);
            this.lblFTPPort.Name = "lblFTPPort";
            this.lblFTPPort.Size = new System.Drawing.Size(48, 13);
            this.lblFTPPort.TabIndex = 3;
            this.lblFTPPort.Text = "FTP Port";
            // 
            // dtpFrom
            // 
            this.dtpFrom.Location = new System.Drawing.Point(91, 124);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(149, 21);
            this.dtpFrom.TabIndex = 4;
            this.dtpFrom.ValueChanged += new System.EventHandler(this.dtpFrom_ValueChanged);
            // 
            // dtpTo
            // 
            this.dtpTo.Location = new System.Drawing.Point(91, 152);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(149, 21);
            this.dtpTo.TabIndex = 5;
            this.dtpTo.ValueChanged += new System.EventHandler(this.dtpTo_ValueChanged);
            // 
            // lblDtFrom
            // 
            this.lblDtFrom.AutoSize = true;
            this.lblDtFrom.Location = new System.Drawing.Point(28, 130);
            this.lblDtFrom.Name = "lblDtFrom";
            this.lblDtFrom.Size = new System.Drawing.Size(57, 13);
            this.lblDtFrom.TabIndex = 6;
            this.lblDtFrom.Text = "Date From";
            // 
            // lblDtTo
            // 
            this.lblDtTo.AutoSize = true;
            this.lblDtTo.Location = new System.Drawing.Point(28, 158);
            this.lblDtTo.Name = "lblDtTo";
            this.lblDtTo.Size = new System.Drawing.Size(45, 13);
            this.lblDtTo.TabIndex = 7;
            this.lblDtTo.Text = "Date To";
            // 
            // ucExtSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblDtTo);
            this.Controls.Add(this.lblDtFrom);
            this.Controls.Add(this.dtpTo);
            this.Controls.Add(this.dtpFrom);
            this.Controls.Add(this.lblFTPPort);
            this.Controls.Add(this.lblFTPDir);
            this.Controls.Add(this.FTPPort);
            this.Controls.Add(this.FTPWorkingDir);
            this.Name = "ucExtSettings";
            this.Size = new System.Drawing.Size(257, 244);
            ((System.ComponentModel.ISupportInitialize)(this.bsSett)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.BindingSource bsSett;
        private System.Windows.Forms.TextBox FTPWorkingDir;
        private System.Windows.Forms.TextBox FTPPort;
        private System.Windows.Forms.Label lblFTPDir;
        private System.Windows.Forms.Label lblFTPPort;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.Label lblDtFrom;
        private System.Windows.Forms.Label lblDtTo;

    }
}
