namespace Databox.Libs.BackLinks
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
            this.dgSearchCat = new System.Windows.Forms.DataGridView();
            this.categoryNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.categotySearchListBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.bsSett = new System.Windows.Forms.BindingSource(this.components);
            this.lblSearchCat = new System.Windows.Forms.Label();
            this.lblCatResult = new System.Windows.Forms.Label();
            this.lbFoundCategory = new System.Windows.Forms.ListBox();
            this.categotyFoundListBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgSearchCat)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.categotySearchListBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsSett)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.categotyFoundListBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dgSearchCat
            // 
            this.dgSearchCat.AutoGenerateColumns = false;
            this.dgSearchCat.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgSearchCat.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.categoryNameDataGridViewTextBoxColumn});
            this.dgSearchCat.DataSource = this.categotySearchListBindingSource;
            this.dgSearchCat.Location = new System.Drawing.Point(16, 33);
            this.dgSearchCat.Name = "dgSearchCat";
            this.dgSearchCat.Size = new System.Drawing.Size(205, 353);
            this.dgSearchCat.TabIndex = 0;
            // 
            // categoryNameDataGridViewTextBoxColumn
            // 
            this.categoryNameDataGridViewTextBoxColumn.DataPropertyName = "CategoryName";
            this.categoryNameDataGridViewTextBoxColumn.HeaderText = "CategoryName";
            this.categoryNameDataGridViewTextBoxColumn.Name = "categoryNameDataGridViewTextBoxColumn";
            // 
            // categotySearchListBindingSource
            // 
            this.categotySearchListBindingSource.DataMember = "CategotySearchList";
            this.categotySearchListBindingSource.DataSource = this.bsSett;
            // 
            // bsSett
            // 
            this.bsSett.DataSource = typeof(Databox.Libs.BackLinks.ExtSettings);
            // 
            // lblSearchCat
            // 
            this.lblSearchCat.AutoSize = true;
            this.lblSearchCat.Location = new System.Drawing.Point(76, 16);
            this.lblSearchCat.Name = "lblSearchCat";
            this.lblSearchCat.Size = new System.Drawing.Size(138, 13);
            this.lblSearchCat.TabIndex = 1;
            this.lblSearchCat.Text = "Category names for search";
            // 
            // lblCatResult
            // 
            this.lblCatResult.AutoSize = true;
            this.lblCatResult.Location = new System.Drawing.Point(440, 16);
            this.lblCatResult.Name = "lblCatResult";
            this.lblCatResult.Size = new System.Drawing.Size(120, 13);
            this.lblCatResult.TabIndex = 2;
            this.lblCatResult.Text = "Found Category Names";
            // 
            // lbFoundCategory
            // 
            this.lbFoundCategory.DataSource = this.categotyFoundListBindingSource;
            this.lbFoundCategory.FormattingEnabled = true;
            this.lbFoundCategory.Location = new System.Drawing.Point(408, 32);
            this.lbFoundCategory.Name = "lbFoundCategory";
            this.lbFoundCategory.Size = new System.Drawing.Size(185, 355);
            this.lbFoundCategory.TabIndex = 3;
            // 
            // categotyFoundListBindingSource
            // 
            this.categotyFoundListBindingSource.AllowNew = true;
            this.categotyFoundListBindingSource.DataMember = "CategotyFoundList";
            this.categotyFoundListBindingSource.DataSource = this.bsSett;
            // 
            // ucExtSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbFoundCategory);
            this.Controls.Add(this.lblCatResult);
            this.Controls.Add(this.lblSearchCat);
            this.Controls.Add(this.dgSearchCat);
            this.Name = "ucExtSettings";
            this.Size = new System.Drawing.Size(700, 424);
            ((System.ComponentModel.ISupportInitialize)(this.dgSearchCat)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.categotySearchListBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsSett)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.categotyFoundListBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.BindingSource bsSett;
        private System.Windows.Forms.DataGridView dgSearchCat;
        private System.Windows.Forms.Label lblSearchCat;
        private System.Windows.Forms.Label lblCatResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn categoryNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource categotySearchListBindingSource;
        private System.Windows.Forms.ListBox lbFoundCategory;
        private System.Windows.Forms.BindingSource categotyFoundListBindingSource;

    }
}
