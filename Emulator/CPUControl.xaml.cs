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

	public partial class CPUControl : UserControl
	{

		public CPUViewModel ViewModel { get; set; }
		List<TextBlock> StackBlocks { get; set; }
		List<CommandInProgramControl> CommandsList { get; set; }

		public CPUControl()
		{
			InitializeComponent();
		}

		public void AddNewRegister( string name, int value, int indent )
		{
			RegistersSP.Children.Add( new RegisterControl( name, value.ToString(), indent * 10 ) );
		}

		public void UnhighlightRegisters()
		{
			foreach (var child in RegistersSP.Children)
			{
				if (child is RegisterControl reg)
				{
					reg.Unhighlight();
				}
			}
		}

		public void UpdateRegisterValue( string name, int value )
		{
			foreach (var child in RegistersSP.Children)
			{
				if (child is RegisterControl reg && reg.RegName.ToLower() == name.ToLower())
				{
					string valueStr = value.ToString();
					if (valueStr != reg.Value)
					{
						reg.Value = valueStr;
						reg.Highlight();
					}
				}
			}
		}

		public void InitializeStackElements( int count )
		{
			StackBlocks = new List<TextBlock>( count );
			for (int i = 0; i < count; i++)
			{
				var tb = new TextBlock { Text = "0" };
				StackSP.Children.Add( tb );
				StackBlocks.Add( tb );
			}
		}

		public void UpdateStackValues( int[] stack )
		{
			for (int i = 0; i < stack.Length; i++)
			{
				StackBlocks[i].Text = stack[i].ToString();
			}
		}

		public void SetProgram( CommandAsmUI[] commands )
		{
			CommandsList = new List<CommandInProgramControl>( commands.Length );
			for (int i = 0; i < commands.Length && i < 1000; i++)
			{
				var control = new CommandInProgramControl( commands[i].address, commands[i].asm );
				ProgramSP.Children.Add( control );
				CommandsList.Add( control );
			}
		}

		public void WriteOutputLine( string output )
		{
			OutputTBlock.Text += output + "\n";
		}

		public void HighlightCommandAtAddress( string address )
		{
			for (int i = 0; i < CommandsList.Count; i++)
			{
				if (CommandsList[i].Address == address)
				{
					CommandsList[i].Background = Brushes.Green;
				}
				else
				{
					CommandsList[i].Background = Brushes.Transparent;
				}
			}
		}

		public void SetCommandFields( (string bits, string desc)[] fields )
		{
			CommandBreakdownSP.Children.Clear();
			for (int i = 0; i < fields.Length; i++)
			{
				CommandBreakdownSP.Children.Add( new CommandBreakdownElementControl( fields[i].bits, fields[i].desc ) );
			}
		}

		private void NextCommandButton_Click( object sender, RoutedEventArgs e )
		{
			ViewModel?.DoNextCommand();
		}

		private void RunProgramButton_Click( object sender, RoutedEventArgs e )
		{
			ViewModel?.RunProgram();
		}
	}
}
