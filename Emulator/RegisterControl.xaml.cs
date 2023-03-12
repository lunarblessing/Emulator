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

	public partial class RegisterControl : UserControl
	{

		public string RegName
		{
			get => NameTBlock.Text;
			set => NameTBlock.Text = value;
		}

		public string Value
		{
			get => ValueTBlock.Text;
			set => ValueTBlock.Text = value;
		}

		Brush DefaultBrush { get; set; }
		Brush RedBrush { get; set; }

		public RegisterControl()
		{
			InitializeComponent();
			DefaultBrush = NameTBlock.Foreground;
			RedBrush = Brushes.Red;
		}

		public RegisterControl( string name, string value, int namePadding ) : this()
		{
			NameTBlock.Padding = new Thickness( namePadding, 0, namePadding, 0 );
			RegName = name;
			Value = value;
		}

		public void Highlight()
		{
			NameTBlock.Foreground = RedBrush;
			ValueTBlock.Foreground = RedBrush;
		}

		public void Unhighlight()
		{
			NameTBlock.Foreground = DefaultBrush;
			ValueTBlock.Foreground = DefaultBrush;
		}
	}
}
