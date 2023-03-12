using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmulatorUI
{
	internal struct CompileCommandParams
	{
		public MainWindow window;
		public AssemblerControl control;
		public string programText;

		public CompileCommandParams( MainWindow window, AssemblerControl control, string text)
		{
			this.window = window;
			this.control = control;
			this.programText = text;
		}
	}
}
