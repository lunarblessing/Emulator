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

	public partial class CommandInProgramControl : UserControl
	{

		public string Address
		{
			get => AddressTBlock.Text;
			set => AddressTBlock.Text = value;
		}

		public string CommandAsm
		{
			get => CommandAsmTBlock.Text;
			set => CommandAsmTBlock.Text = value;
		}

		public CommandInProgramControl()
		{
			InitializeComponent();
		}

		public CommandInProgramControl(string address, string asm)
		{
			InitializeComponent();
			Address = address;
			CommandAsm = asm;
		}
	}
}
