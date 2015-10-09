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
using System.Windows.Shapes;

namespace Legends_Email_Spammer
{
	/// <summary>
	/// Interaction logic for Output.xaml
	/// </summary>
	public partial class Output : Window
	{
		int LineCount = 0;
		int LineHeight = 70;

		public Output()
		{
			InitializeComponent();
			WriteLine("Initializing debug...");
			OutputBox.Height = OutputBox.LineHeight * 70; // The number of lines is 70 because Mads said so LOL.
			LineCount = 70;
        }

		public void WriteLine(string line)
		{
			OutputBox.Text += line + "\n";
			OutputBox.Height += OutputBox.LineHeight;
			LineCount++;
			if (LineCount >= LineHeight)
			{
				OutputBox.Height += OutputBox.LineHeight;
            }
        }

		public void Write(string line)
		{
			OutputBox.Text += line;
			OutputBox.Height += OutputBox.LineHeight;
		}
	}
}
