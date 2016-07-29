using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EDF_dev
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			MessageBox.Show("This is a dummy app for debugging EDF modules. Dont start it directly.");
			
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form2());
		}
	}
}
