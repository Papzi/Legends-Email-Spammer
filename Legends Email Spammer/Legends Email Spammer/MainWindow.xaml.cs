using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Width = 525;

			var bc = new BrushConverter();

			Application.Current.Properties.Add("started", false);
			Application.Current.Properties.Add("bc", new BrushConverter());

			Application.Current.Properties.Add("TextBoxBorder", (Brush)bc.ConvertFrom("#FF000000"));
			Application.Current.Properties.Add("TextBoxBorderSuccess", (Brush)bc.ConvertFrom("#FF00FF00"));
			Application.Current.Properties.Add("TextBoxBorderFailed", (Brush)bc.ConvertFrom("#FFFF0000"));
			Application.Current.Properties.Add("ButtonStarted", (Brush)bc.ConvertFrom("#FF337F33"));
			Application.Current.Properties.Add("ButtonStopped", (Brush)bc.ConvertFrom("#FF7F3333"));

			Application.Current.Properties.Add("SpammerThread", new Thread(new ThreadStart(spammerThread)));
		}

		private void validateEmail_Click(object sender, RoutedEventArgs e)
		{
			Trace.WriteLine(delayBox.DefaultNumber);
		}

		private void advancedSwitch_Click(object sender, RoutedEventArgs e)
		{
			if (Width < 1000)
			{
				int finalLength = (int)1000;
				double speed = 13.71337;
				advancedSwitch.Content = "Advanced <<";
				while (Width < finalLength)
				{
					Width = Width + speed;
					speed -= 0.2;
					Trace.WriteLine(Width);
					Thread.Sleep(10);
				}
			}
			else
			{
				int finalLength = (int)525;
				double speed = 13.71337;
				advancedSwitch.Content = "Advanced >>";
				while (Width > finalLength)
				{
					Width = Width - speed;
					speed -= 0.2;
					Trace.WriteLine(Width);
					Thread.Sleep(10);
				}
			}
		}

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			if (((bool)Application.Current.Properties["started"]) == false)
			{

				startSpam();
			}
			else
			{

				stopSpam();
			}

		}

		private bool checkEmail(string email)
		{
			if (email == "")
				return false;
			if (!Regex.IsMatch(email, "^\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*$"))
				return false;
			return true;
		}

		private bool checkSMTP(string server)
		{
			if (server == "")
				return false;

			using (var client = new TcpClient())
			{
				var port = 465;
				try
				{
					client.Connect(server, port);
				}
				catch (System.Net.Sockets.SocketException e)
				{
					return false;
				}
				// As GMail requires SSL we should use SslStream
				// If your SMTP server doesn't support SSL you can
				// work directly with the underlying stream
				using (var stream = client.GetStream())
				using (var sslStream = new SslStream(stream))
				{
					sslStream.AuthenticateAsClient(server);
					using (var writer = new StreamWriter(sslStream))
					using (var reader = new StreamReader(sslStream))
					{
						writer.WriteLine("EHLO " + server);
						writer.Flush();
						if (reader.ReadLine().StartsWith("220 smtp.gmail.com ESMTP"))
							return true;
						// GMail responds with: 220 mx.google.com ESMTP
					}
				}
			}
			return true;
		}

		private bool validateFields()
		{
			var bc = new BrushConverter();

			if (!checkEmail(emailAddress.Text))
			{
				emailAddress.BorderBrush = (Brush)Application.Current.Properties["TextBoxBorderFailed"];
				emailAddress.Focus();
				return false;
			}
			else
				emailAddress.BorderBrush = (Brush)Application.Current.Properties["TextBoxBorderSuccess"];


			if (!checkSMTP(EmailSMTPServer.Text))
			{
				EmailSMTPServer.BorderBrush = (Brush)Application.Current.Properties["TextBoxBorderFailed"];
				EmailSMTPServer.Focus();
				return false;
			}
			else
				EmailSMTPServer.BorderBrush = (Brush)Application.Current.Properties["TextBoxBorderSuccess"];


			if (EmailPassword.Password == "")
			{
				EmailPassword.BorderBrush = (Brush)Application.Current.Properties["TextBoxBorderFailed"];
				EmailPassword.Focus();
				return false;
			}
			else
				EmailPassword.BorderBrush = (Brush)Application.Current.Properties["TextBoxBorderSuccess"];


			if (!checkEmail(toBox.Text))
			{
				toBox.BorderBrush = (Brush)Application.Current.Properties["TextBoxBorderFailed"];
				toBox.Focus();
				return false;
			}
			else
				toBox.BorderBrush = (Brush)Application.Current.Properties["TextBoxBorderSuccess"];


			if (subjectBox.Text == "")
			{
				subjectBox.BorderBrush = (Brush)Application.Current.Properties["TextBoxBorderFailed"];
				subjectBox.Focus();
				return false;
			}
			else
				subjectBox.BorderBrush = (Brush)Application.Current.Properties["TextBoxBorderSuccess"];

			return true;
		}

		private void spammerThread()
		{
			while (true)
			{
				Trace.WriteLine("running!");
				while ((bool)Application.Current.Properties["started"])
				{
					sendMail(emailAddress.Text, EmailPassword.Password, emailAddress.Text, subjectBox.Text, bodyBox.Text);
					Thread.Sleep(int.Parse(delayBox.DefaultNumber));
				}
				Thread.Sleep(1000);
			}
		}

		private void startSpam()
		{
			if (!validateFields())
			{
				errorBox.Text = "The information was not valid";
				return;
			}
			StartButton.Background = (Brush)Application.Current.Properties["ButtonStopped"];
			StartButton.Content = "Stop";

			Application.Current.Properties["started"] = true;
			emailAddress.IsEnabled = false;
			EmailPassword.IsEnabled = false;
			EmailSMTPServer.IsEnabled = false;
		}

		private void stopSpam()
		{

			StartButton.Background = (Brush)Application.Current.Properties["ButtonStarted"];
			StartButton.Content = "Start";

			Application.Current.Properties["started"] = false;

			emailAddress.IsEnabled = true;
			EmailPassword.IsEnabled = true;
			EmailSMTPServer.IsEnabled = true;
		}

		private void sendMail(string email, string password, string to, string subject, string body)
		{
			try
			{
				MailMessage mail = new MailMessage();
				SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

				mail.From = new MailAddress(email);
				mail.To.Add(to);
				mail.Subject = subject;
				mail.Body = body;

				SmtpServer.Port = 587;
				SmtpServer.Credentials = new System.Net.NetworkCredential(email, password);
				SmtpServer.EnableSsl = true;

				SmtpServer.Send(mail);
				MessageBox.Show("mail Send");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

		}

		private void emailAddress_TextChanged(object sender, TextChangedEventArgs e)
		{
			emailAddress.BorderBrush = (Brush)Application.Current.Properties["TextBoxBorder"];
		}

		private void SendButton_Click(object sender, RoutedEventArgs e)
		{
			if (!validateFields())
			{
				errorBox.Text = "The information was not valid";
				return;
			}
			sendMail(emailAddress.Text, EmailPassword.Password, emailAddress.Text, subjectBox.Text, bodyBox.Text);
		}

		private void toBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void aboutButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			Trace.WriteLine("per");
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
