using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Legends_Email_Spammer
{
	/// <summary>
	/// Interaction logic for NumberInput.xaml
	/// </summary>
	public partial class NumberInput : UserControl
	{
		public static readonly DependencyProperty SuffixProperty = DependencyProperty.Register("Suffix", typeof(string), typeof(NumberInput));
		public static readonly DependencyProperty DefaultNumberProperty = DependencyProperty.Register("DefaultNumber", typeof(string), typeof(NumberInput));


		public string Suffix
		{
			get { return (string)GetValue(SuffixProperty); }
			set { SetValue(SuffixProperty, value); }
		}

		public string DefaultNumber
		{
			get { return (string)GetValue(DefaultNumberProperty); }
			set { SetValue(DefaultNumberProperty, value); }
		}

		public NumberInput()
		{
			InitializeComponent();
			this.DataContext = this;
		}

		private void NumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");

		}

		private void NumberBox_Pasting(object sender, DataObjectPastingEventArgs e)
		{

			if (e.DataObject.GetDataPresent(typeof(string)))
			{
				string paste = (string)e.DataObject.GetData(typeof(string));

				Trace.WriteLine("Parsing paste: '" + paste + "'");

				Trace.WriteLineIf(Regex.IsMatch(paste, "^[0-9]+$"), "Paste is matching :)");
				Trace.WriteLineIf(!Regex.IsMatch(paste, "^[0-9]+$"), "Paste is not matching :(");

				if (!Regex.IsMatch(paste, "^[0-9]+$"))
					e.CancelCommand();
			}
			else
			{
				Trace.WriteLine("Paste was not text :(");
				e.CancelCommand();
			}
		}
	}
}
