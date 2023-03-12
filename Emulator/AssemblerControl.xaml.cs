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
using Microsoft.Win32;
using System.IO;

namespace EmulatorUI
{

	public partial class AssemblerControl : UserControl
	{
		string? SelectedFile { get; set; }
		MainWindow Window { get; set; }

		public ICommand CompileCommand
		{
			get { return (ICommand)GetValue( CompileCommandProperty ); }
			set { SetValue( CompileCommandProperty, value ); }
		}

		public static readonly DependencyProperty CompileCommandProperty =
			DependencyProperty.Register( nameof(CompileCommand), typeof( ICommand ), typeof( AssemblerControl ), new PropertyMetadata( null ) );

		public AssemblerControl( MainWindow window )
		{
			DataContext = window;
			Window = window;
			InitializeComponent();
			SetBinding( CompileCommandProperty, new Binding( nameof( CompileCommand ) ) );
		}

		public void WriteCompilationError( string text )
		{
			Dispatcher.InvokeAsync( () => ErrorTBlock.Text = text );
		}

		public void StartCPU( CPUViewModel vm )
		{
			Dispatcher.InvokeAsync( () => Window?.StartCPU( vm ) );
		}

		private void OpenFileButton_Click( object sender, RoutedEventArgs e )
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Multiselect = false;
			openFileDialog.InitialDirectory = Environment.CurrentDirectory;
			openFileDialog.Filter = "Text files (*.txt)|*.txt";
			if (openFileDialog.ShowDialog() == true)
			{
				ProgramTB.Text = File.ReadAllText( openFileDialog.FileName );
				SelectedFile = openFileDialog.FileName;
			}
		}

		private void SaveFileButton_Click( object sender, RoutedEventArgs e )
		{
			Save();
		}

		bool Save()
		{
			if (SelectedFile != null)
			{
				File.WriteAllText( SelectedFile, ProgramTB.Text );
			}
			else
			{
				SaveFileDialog dialog = new SaveFileDialog();
				dialog.Filter = "Text files (*.txt)|*.txt";
				dialog.InitialDirectory = Environment.CurrentDirectory;
				if (dialog.ShowDialog() == true)
				{
					File.WriteAllText( dialog.FileName, ProgramTB.Text );
					SelectedFile = dialog.FileName;
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		private void CompileButton_Click( object sender, RoutedEventArgs e )
		{
			if (CompileCommand != null)
			{
				var parameter = new CompileCommandParams( Window, this, ProgramTB.Text );
				CompileCommand.Execute( parameter );
			}
		}

		private void NewFileButton_Click( object sender, RoutedEventArgs e )
		{
			SelectedFile = null;
			ProgramTB.Clear();
		}
	}
}
