using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
		private Output ConsoleBox = new Output();
		private bool started = false;
		private BrushConverter bc = new BrushConverter();
		private EmailDetails emailDetails = new EmailDetails();
		public MainWindow()
		{
			InitializeComponent();

			Width = 525;

			// defining some default colors:
			Application.Current.Properties.Add("TextBoxBorder", (Brush)bc.ConvertFrom("#FF000000"));
			Application.Current.Properties.Add("TextBoxBorderSuccess", (Brush)bc.ConvertFrom("#FF00FF00"));
			Application.Current.Properties.Add("TextBoxBorderFailed", (Brush)bc.ConvertFrom("#FFFF0000"));
			Application.Current.Properties.Add("ButtonStarted", (Brush)bc.ConvertFrom("#FF337F33"));
			Application.Current.Properties.Add("ButtonStopped", (Brush)bc.ConvertFrom("#FF7F3333"));

			Application.Current.Properties.Add("SpammerThread", new Thread(spammerThread));
        }

		public struct EmailDetails
		{
			public string email;
			public string password;
			public string SMTP;
			public string receiver;
			public string subject;
			public string body;
			public string time; // This is the sleep time between each email.
			public EmailDetails(string Email, string Password, string sMTP, string Receiver, string Subject, string Body, string Time)
			{
                email = Email;
				password = Password;
				SMTP = sMTP;
				receiver = Receiver;
				subject = Subject;
				body = Body;
				time = Time;
            }
        }


		private void validateEmail_Click(object sender, RoutedEventArgs e)
		{
			Trace.WriteLine(delayBox.DefaultNumber);
		}

		private void advancedSwitch_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();

			//Shitty prototype code dont look please.

			/*if (Width < 1000)
			{
				int finalLength = (int) 1000; // Im just quitly casting an int to an int 
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
			}*/
		}

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			if (!started)
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
			ConsoleBox.WriteLine("Testing smtp server");
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
					ConsoleBox.WriteLine(string.Format("An error was thrown while connecting to '{0}:{1}'", server, port));
                    return false;
				}
				// As GMail requires SSL we should use SslStream
				// If your SMTP server doesn't support SSL you can
				// work directly with the underlying stream
				ConsoleBox.WriteLine(string.Format("Sending request to '{0}:{1}'...", server, port));
				using (var stream = client.GetStream())
				using (var sslStream = new SslStream(stream))
				{
					sslStream.AuthenticateAsClient(server);
					using (var writer = new StreamWriter(sslStream))
					using (var reader = new StreamReader(sslStream))
					{
						writer.WriteLine("EHLO " + server);
						writer.Flush();
						string respond = reader.ReadLine();
                        ConsoleBox.WriteLine(string.Format("'{0}:{1}' responded with:\n{2}", server, port, respond));
						if (respond.StartsWith("220"))
							return true;
						// GMail responds with: 220 mx.google.com ESMTP
					}
				}
			}
			return true;
		}

		private bool validateFields()
		{

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
            while (started)
			{

				sendMail(emailDetails.email, emailDetails.password, emailDetails.SMTP, emailDetails.receiver, emailDetails.subject, emailDetails.body, false);
				Thread.Sleep(int.Parse(emailDetails.time));
			}
		}

		private void startSpam()
		{
			if (!validateFields())
			{
				errorBox.Text = "The information was not valid";
				return;
			}
			ConsoleBox.WriteLine("Starting email thread.");
			StartButton.Background = (Brush)Application.Current.Properties["ButtonStopped"];
			StartButton.Content = "Stop";

			emailDetails.email = emailAddress.Text;
			emailDetails.password = EmailPassword.Password;
			emailDetails.SMTP = EmailSMTPServer.Text;
			emailDetails.receiver = toBox.Text;
			emailDetails.subject = subjectBox.Text;
			emailDetails.body = bodyBox.Text;
			emailDetails.time = delayBox.DefaultNumber;

			((Thread)Application.Current.Properties["SpammerThread"]).Start();
            started = true;
			emailAddress.IsEnabled = false;
			EmailPassword.IsEnabled = false;
			EmailSMTPServer.IsEnabled = false;
			toBox.IsEnabled = false;
			subjectBox.IsEnabled = false;
			delayBox.IsEnabled = false;
			sendButton.IsEnabled = false;
			validateEmail.IsEnabled = false;
		}

		public EmailDetails get_email_details()
		{
			EmailDetails tmp;
			tmp.email = emailAddress.Text;
			tmp.password = EmailPassword.Password;
			tmp.SMTP = EmailSMTPServer.Text;
			tmp.receiver = toBox.Text;
			tmp.subject = subjectBox.Text;
			tmp.body = bodyBox.Text;
			tmp.time = delayBox.DefaultNumber;
			return tmp;
        }

		private void stopSpam()
		{
			ConsoleBox.WriteLine("Stopping email thread.");

			StartButton.Background = (Brush)Application.Current.Properties["ButtonStarted"];
			StartButton.Content = "Start";

			started = false;
			emailAddress.IsEnabled = true;
			EmailPassword.IsEnabled = true;
			EmailSMTPServer.IsEnabled = true;
			toBox.IsEnabled = true;
			subjectBox.IsEnabled = true;
			delayBox.IsEnabled = true;
			sendButton.IsEnabled = true;
			validateEmail.IsEnabled = true;

			Application.Current.Properties["SpammerThread"] = new Thread(spammerThread);

		}

		private void sendMail(string email, string password, string smtp, string to, string subject, string body, bool debug=true)
		{
			if (debug)
				ConsoleBox.WriteLine("Sending email...");
			try
			{
				MailMessage mail = new MailMessage();
				SmtpClient SmtpServer = new SmtpClient(smtp);

				mail.From = new MailAddress(email);
				mail.To.Add(to);
				mail.Subject = subject;
				mail.Body = body;

				SmtpServer.Port = 587;
				SmtpServer.Credentials = new System.Net.NetworkCredential(email, password);
				SmtpServer.EnableSsl = true;

				SmtpServer.Send(mail);
				if (debug)
					ConsoleBox.WriteLine("Send mail");
            }
			catch (Exception ex)
			{
				if (debug)
				{
					ConsoleBox.WriteLine("Failed to send email...");
					ConsoleBox.WriteLine(ex.ToString());
				}
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
			sendMail(emailAddress.Text, EmailPassword.Password, EmailSMTPServer.Text, emailAddress.Text, subjectBox.Text, bodyBox.Text);
		}

		private void aboutButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Made by Speedyjens & NoHax @ Hacktify Digital Group");
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

		private void ConsoleToggle_Click(object sender, RoutedEventArgs e)
		{
			ConsoleBox.Show();
		}
	}
}
