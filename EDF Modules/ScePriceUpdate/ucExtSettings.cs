#region using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Databox.Libs.Common;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraSplashScreen;
using WheelsScraper;

#endregion

namespace Databox.Libs.ScePriceUpdate
{
    public partial class ucExtSettings : XtraUserControl
    {
        #region Delegates

        public delegate List<SceProduct> GetSceProductsDlg();

        public delegate List<string> GetStrListDlg();

        #endregion

        public ucExtSettings()
        {
            InitializeComponent();
            layoutControl1.Dock = DockStyle.Fill;
        }

        public GetSceProductsDlg LoadSceProducts { get; set; }
        public GetStrListDlg LoadBrands { get; set; }
        public GetStrListDlg LoadCategories { get; set; }
        public Func<string, List<SceProduct>> ReadProductsFromFile { get; set; }
        public Func<List<string>> LoadFTPFilesNames { get; set; }

        public ExtSettings ExtSett
        {
            get { return (ExtSettings) Sett.SpecialSettings; }
        }

        public ScraperSettings Sett { get; set; }

        public void RefreshBindings()
        {
            bsSett.DataSource = ExtSett;

            sceProductBindingSource.DataSource = new List<SceProduct>();
            priceMarkupBindingSource.DataSource = new List<PriceMarkup>();

            sceProductBindingSource.DataSource = ExtSett.ProductsFromSce;
            priceMarkupBindingSource.DataSource = ExtSett.PriceMarkups;
            sceProductBindingSource.ResetBindings(false);
            priceMarkupBindingSource.ResetBindings(false);
        }

        public void DoLoadProductsFromSce()
        {
            var products = LoadSceProducts();

            ModuleSettings.Default.ProductsFromSce.Clear();
            ModuleSettings.Default.ProductsFromSce.AddRange(products);
            RefreshBindings();
        }

        public void DoReadProductsFromFile(string fileName)
        {
            var products = ReadProductsFromFile(fileName);
            products = products.GroupBy(p => p.PartNumber).Select(g => g.First()).ToList();

            ModuleSettings.Default.ProductsFromSce.Clear();
            ModuleSettings.Default.ProductsFromSce.AddRange(products);
            RefreshBindings();
        }

        public void DoAddProductsFromFile(string fileName)
        {
            var products = ReadProductsFromFile(fileName);
            products = products.GroupBy(p => p.PartNumber).Select(g => g.First()).ToList();

            ModuleSettings.Default.ProductsFromSce.AddRange(products);
            ModuleSettings.Default.ProductsFromSce = ModuleSettings.Default.ProductsFromSce.GroupBy(p => p.PartNumber).Select(g => g.First()).ToList();
            RefreshBindings();
        }

        public void DoLoadBrandList()
        {
            var brands = LoadBrands();
            brands.Sort();
            BoxBrands.Properties.Items.Clear();
            foreach (var brand in brands)
            {
                var checkItem = new CheckedListBoxItem
                {
                    Value = brand,
                    Description = brand
                };
                BoxBrands.Properties.Items.Add(checkItem);
            }
        }

        public void DoLoadCategoriesList()
        {
            var categories = LoadCategories();
            categories.Sort();
            BoxCategories.Properties.Items.Clear();
            foreach (var category in categories)
            {
                var checkItem = new CheckedListBoxItem
                {
                    Value = category,
                    Description = category
                };
                BoxCategories.Properties.Items.Add(checkItem);
            }
        }

        private void DoSaveSettings()
        {
            ModuleSettings.Default.SaveConfig();
        }

        private void ExecuteActionWithWaitForm<T>(Action a, string msg = null)
        {
            SplashScreenManager.ShowForm(null, typeof(T), true, true, false, 1000);
            try
            {
                a();
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally
            {
                SplashScreenManager.CloseForm(false);
                if (!string.IsNullOrEmpty(msg))
                {
                    MessageBox.Show(msg);
                }
            }
        }

        private void ExecuteActionWithWaitForm<T>(Action<string> a, string param, string msg = null)
        {
            SplashScreenManager.ShowForm(null, typeof(T), true, true, false, 1000);
            try
            {
                a(param);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally
            {
                SplashScreenManager.CloseForm(false);
                if (!string.IsNullOrEmpty(msg))
                {
                    MessageBox.Show(msg);
                }
            }
        }

        private void LoadProductsClick(object sender, EventArgs e)
        {
            ExecuteActionWithWaitForm<CustomWaitForm>(DoLoadProductsFromSce);
        }

        private void CheckEditLoadBeforeUpdateCheckedChanged(object sender, EventArgs e)
        {
            ExtSett.LoadBeforeUpdate = ((CheckEdit) sender).Checked;
        }

        private void UcExtSettingsLoad(object sender, EventArgs e)
        {
            RefreshBindings();
        }

        private void SimpleButtonClearClick(object sender, EventArgs e)
        {
            ModuleSettings.Default.ProductsFromSce.Clear();
            RefreshBindings();
        }

        private void CheckEditRemoveFilesUfterUpdateCheckedChanged(object sender, EventArgs e)
        {
            ExtSett.RemoveFilesAfterUpdate = ((CheckEdit) sender).Checked;
        }

        private void checkEditUseBrands_CheckedChanged(object sender, EventArgs e)
        {
            var useBrands = ((CheckEdit) sender).Checked;
            if (useBrands)
            {
                ExecuteActionWithWaitForm<CustomWaitForm>(DoLoadBrandList);
                BoxBrands.Enabled = true;
            }
            else
            {
                BoxBrands.Properties.Items.Clear();
                ExtSett.SelectedBrands.Clear();
                BoxBrands.Enabled = false;
            }
            ExtSett.UseBrands = useBrands;
        }

        private void checkEditUseCategories_CheckedChanged(object sender, EventArgs e)
        {
            var useCategories = ((CheckEdit) sender).Checked;
            if (useCategories)
            {
                ExecuteActionWithWaitForm<CustomWaitForm>(DoLoadCategoriesList);
                BoxCategories.Enabled = true;
            }
            else
            {
                BoxCategories.Properties.Items.Clear();
                ExtSett.SelectedCategories.Clear();
                BoxCategories.Enabled = false;
            }
            ExtSett.UseCategories = useCategories;
        }

        private void comboBoxEditCategories_EditValueChanged(object sender, EventArgs e)
        {
            ExtSett.SelectedCategories =
                ((CheckedComboBoxEdit) sender).Properties.Items.GetCheckedValues().Cast<string>().ToList();
        }

        private void comboBoxEditBrands_EditValueChanged(object sender, EventArgs e)
        {
            ExtSett.SelectedBrands =
                ((CheckedComboBoxEdit) sender).Properties.Items.GetCheckedValues().Cast<string>().ToList();
        }

        private void checkEditUpdateMSRP_CheckedChanged(object sender, EventArgs e)
        {
            ExtSett.UpdateMSRP = ((CheckEdit)sender).Checked;
        }

        private void checkEditUpdateWebPrice_CheckedChanged(object sender, EventArgs e)
        {
            ExtSett.UpdateWebPrice = ((CheckEdit)sender).Checked;
        }

        private void ButtonSaveConfig_Click(object sender, EventArgs e)
        {
            ExecuteActionWithWaitForm<CustomWaitForm>(DoSaveSettings, "Settings saved");
        }

        private void ButtonReadFromFile_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileName = openFileDialog.FileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    ExecuteActionWithWaitForm<CustomWaitForm>(DoReadProductsFromFile, fileName);
                }
            }
        }

        private void ButtonAddFromFile_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileName = openFileDialog.FileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    ExecuteActionWithWaitForm<CustomWaitForm>(DoAddProductsFromFile, fileName);
                }
            }
        }

        private void checkEditLoadPricesFromFTP_CheckedChanged(object sender, EventArgs e)
        {
            ExtSett.LoadPriceFromFTP = ((CheckEdit) sender).Checked;
            textEditPriceFiles.Enabled = ExtSett.LoadPriceFromFTP;
            textEditFTPWorkDir.Enabled = ExtSett.LoadPriceFromFTP;
            textEditFTPPort.Enabled = ExtSett.LoadPriceFromFTP;
        }
    }
}