using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmulatorUI
{

	public partial class CommandBreakdownElementControl : UserControl
	{
		public string Bits
		{
			get => BitsTBlock.Text;
			set => BitsTBlock.Text = value;
		}

		public string Description
		{
			get => DescTBlock.Text;
			set => DescTBlock.Text = value;
		}

		public CommandBreakdownElementControl()
		{
			InitializeComponent();
		}

		public CommandBreakdownElementControl(string bits, string desc)
		{
			InitializeComponent();
			Bits = bits;
			Description = desc;
		}
	}
}
