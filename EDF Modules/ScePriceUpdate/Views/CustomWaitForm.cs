using DevExpress.XtraWaitForm;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Databox.Libs.Common
{
	public  partial class CustomWaitForm : WaitForm
	{
		public CustomWaitForm()
		{
			InitializeComponent();
		}

		public override void SetCaption(string caption)
		{
			base.SetCaption(caption);
			this.progressPanel1.Caption = caption;
		}

		public override void SetDescription(string description)
		{
			base.SetDescription(description);
			this.progressPanel1.Description = description;
		}
	}
}
