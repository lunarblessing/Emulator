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

using Emulator.Assembler;
using Emulator.Assembler.Compiler;

namespace EmulatorUI
{
	public partial class MainWindow : Window
	{

		public CompileCommand CompileCommand { get; set; }

		public MainWindow()
		{
			InitializeComponent();
			CompileCommand = new CompileCommand();
			var control = new AssemblerControl( this );
			Content = control;
			//AssemblerViewModel vm = new AssemblerViewModel();
			//vm.SetControl( control );
		}

		public void StartCPU( CPUViewModel vm )
		{
			CPUControl control = new CPUControl();
			Content = control;
			vm.SetControl( control );
		}
	}
}
