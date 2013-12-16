using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Eneter.Messaging.EndPoints.StringMessages;
using Eneter.Messaging.EndPoints.TypedMessages;
using Eneter.Messaging.MessagingSystems.MessagingSystemBase;
using Eneter.Messaging.MessagingSystems.TcpMessagingSystem;
using System.Threading.Tasks;

namespace SilverlightClient
{
	public partial class MainPage : UserControl
	{
		private IDuplexStringMessageSender Command_MessageSender;
		private IDuplexOutputChannel Worker4504_OutputChannel;

		//private int repeat = 72900; private int bytesLength = 1440;
		private int _repeat; private int _bytesLength;
		private int _packetCounter = 0;

		// TODO:optimize this
		// global stopwatch
		Stopwatch _stopwatch = new Stopwatch();

		public MainPage()
		{
			InitializeComponent();
		}

#region Common Utilities
		static byte[] GetBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		static string GetString(byte[] bytes)
		{
			char[] chars = new char[bytes.Length / sizeof(char)];
			System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		private void Log(string s)
		{
			// check if we not in main thread
			if (!Dispatcher.CheckAccess())
			{
				// call same method in main thread
				Dispatcher.BeginInvoke(() =>
				{
					Log(s);
				});
				return;
			}
			// in main thread now
			LogListBox.Items.Add(s);
		}

		private void EnableAllButton()
		{
			// check if we not in main thread
			if (!Dispatcher.CheckAccess())
			{
				// call same method in main thread
				Dispatcher.BeginInvoke(() =>
				{
					EnableAllButton();
				});
				return;
			}

			// in main thread now
			Download1MBButton.IsEnabled = true;
			Download5MBButton.IsEnabled = true;
			Download10MBButton.IsEnabled = true;
			Download50MBButton.IsEnabled = true;
			Download100MBButton.IsEnabled = true;

			Upload1MBButton.IsEnabled = true;
			Upload5MBButton.IsEnabled = true;
			Upload10MBButton.IsEnabled = true;
			Upload50MBButton.IsEnabled = true;
			Upload100MBButton.IsEnabled = true;
		}

		private void DisableAllButton()
		{
			// check if we not in main thread
			if (!Dispatcher.CheckAccess())
			{
				// call same method in main thread
				Dispatcher.BeginInvoke(() =>
				{
					EnableAllButton();
				});
				return;
			}

			// in main thread now
			Download1MBButton.IsEnabled = false;
			Download5MBButton.IsEnabled = false;
			Download10MBButton.IsEnabled = false;
			Download50MBButton.IsEnabled = false;
			Download100MBButton.IsEnabled = false;

			Upload1MBButton.IsEnabled = false;
			Upload5MBButton.IsEnabled = false;
			Upload10MBButton.IsEnabled = false;
			Upload50MBButton.IsEnabled = false;
			Upload100MBButton.IsEnabled = false;

		}

		private void ClearLogs_Click(object sender, RoutedEventArgs e)
		{
			clearLogs();
		}

		void clearLogs()
		{
			// check if we not in main thread
			if (!Dispatcher.CheckAccess())
			{
				// call same method in main thread
				Dispatcher.BeginInvoke(() =>
				{
					clearLogs();
				});
				return;
			}
			// in main thread now
			LogListBox.Items.Clear();
		}


#endregion

		private void Download1MB(object sender, RoutedEventArgs e)
		{
			// fragmented
			_repeat = 1;
			_bytesLength = 1048576;

			// non fragmented
			//repeat = 730;
			//bytesLength = 1440;

			DisableAllButton();

			if (Command_Initialize())
			{
				if (Worker4504_Initialize())
				{
					Command_MessageSender.SendMessage("1|send|" + Worker4504_OutputChannel.ResponseReceiverId + "|" + _bytesLength + "|" + _repeat); //344ms 1 repeat
				}
			}
		}

		private void Download5MB(object sender, RoutedEventArgs e)
		{
			// fragmented
			_repeat = 1;
			_bytesLength = 1048576*5;

			// non fragmented
			//repeat = 730*5;
			//bytesLength = 1440;

			DisableAllButton();

			if (Command_Initialize())
			{
				if (Worker4504_Initialize())
				{
					Command_MessageSender.SendMessage("1|send|" + Worker4504_OutputChannel.ResponseReceiverId + "|" + _bytesLength + "|" + _repeat); //344ms 1 repeat
				}
			}
		}

		private void Download10MB(object sender, RoutedEventArgs e)
		{
			// fragmented
			_repeat = 1;
			_bytesLength = 1048576*10;

			// non fragmented
			//repeat = 730*10;
			//bytesLength = 1440;

			DisableAllButton();

			if (Command_Initialize())
			{
				if (Worker4504_Initialize())
				{
					Command_MessageSender.SendMessage("1|send|" + Worker4504_OutputChannel.ResponseReceiverId + "|" + _bytesLength + "|" + _repeat); //344ms 1 repeat
				}
			}
		}

		private void Download50MB(object sender, RoutedEventArgs e)
		{
			// fragmented
			_repeat = 1;
			_bytesLength = 1048576*50;

			// non fragmented
			//repeat = 730*50;
			//bytesLength = 1440;

			DisableAllButton();

			if (Command_Initialize())
			{
				if (Worker4504_Initialize())
				{
					Command_MessageSender.SendMessage("1|send|" + Worker4504_OutputChannel.ResponseReceiverId + "|" + _bytesLength + "|" + _repeat); //344ms 1 repeat
				}
			}
		}

		private void Download100MB(object sender, RoutedEventArgs e)
		{
			// fragmented
			_repeat = 1;
			_bytesLength = 1048576*100;

			// non fragmented
			//repeat = 730*100;
			//bytesLength = 1440;

			DisableAllButton();

			if (Command_Initialize())
			{
				if (Worker4504_Initialize())
				{
					Command_MessageSender.SendMessage("1|send|" + Worker4504_OutputChannel.ResponseReceiverId + "|" + _bytesLength + "|" + _repeat); //344ms 1 repeat
				}
			}
		}

		private bool Command_Initialize()
		{
			// Create duplex message sender.
			// It can send messages and also receive messages.
			IDuplexStringMessagesFactory aStringMessagesFactory = new DuplexStringMessagesFactory();
			Command_MessageSender = aStringMessagesFactory.CreateDuplexStringMessageSender();
			Command_MessageSender.ResponseReceived += Command_ResponseReceived;

			// Create TCP based messaging.
			IMessagingSystemFactory aMessaging = new TcpMessagingSystemFactory();
			IDuplexOutputChannel aDuplexOutputChannel = aMessaging.CreateDuplexOutputChannel("tcp://" + serverIP.Text + ":4502/");

			// Attach the duplex output channel to the message sender
			// and be able to send messages and receive messages.
			try
			{
				Command_MessageSender.AttachDuplexOutputChannel(aDuplexOutputChannel);
			}
			catch (Exception e)
			{
				Log(e.Message);
			}

			if (Command_MessageSender.IsDuplexOutputChannelAttached)
			{
				Log("Initialized connection.");
				return true;
			}
			else
			{
				Log("Unable to initialize connection");
				return false;
			}
		}

		private void Command_ResponseReceived(object sender, StringResponseReceivedEventArgs e)
		{
			Log("Received CMD : " + e.ResponseMessage);

			//downloading
			if (e.ResponseMessage.Length == 1)
			{
				// this is a command from server
				if (e.ResponseMessage.Equals("a"))
				{
					// start
					_stopwatch.Reset();
					_stopwatch.Start();
					_packetCounter = 0;
				}
			}
			//uploading
			//else if (e.ResponseMessage.Length == 0)
			//{
			//	stopwatch.Stop();
			//	Log("Received completed.");
			//	Log("Total packets : " + packetCounter); // should equals to repeat
			//	Log("Packets length : " + bytesLength + " bytes");
			//	Log("Duration : " + stopwatch.ElapsedMilliseconds + " ms");
			//	Log("Size transfered : " + sizeTransfered + " bytes");
			//	Log("Bandwidth : " + MBps + " MB/s");
			//	Log("Bandwidth : " + Mbps + " Mb/s");
			//	Log("");
			//}
		}

		private bool Worker4504_Initialize()
		{
			// Create TCP messaging
			IMessagingSystemFactory aMessaging = new TcpMessagingSystemFactory();
			Worker4504_OutputChannel = aMessaging.CreateDuplexOutputChannel("tcp://" + serverIP.Text + ":4504/");

			// Subscribe to response messages.
			Worker4504_OutputChannel.ConnectionClosed += Worker4504_ConnectionClosed;
			Worker4504_OutputChannel.ConnectionOpened += Worker4504_ConnectionOpened;
			Worker4504_OutputChannel.ResponseMessageReceived += Worker4504_ResponseMessageReceived;

			// Open connection and be able to send messages and receive response messages.
			Worker4504_OutputChannel.OpenConnection();

			if (Worker4504_OutputChannel.IsConnected)
			{
				return true;		        
			}
			else
			{
				return false;
			}
		}

		void Worker4504_ResponseMessageReceived(object sender, DuplexChannelMessageEventArgs e)
		{
			int length = (e.Message as byte[]).Length;
			Log("Worker received length : " + length);

			// download
			if (length != 0)
			{
				_packetCounter++;

				if (_packetCounter >= (_repeat - 1))
				{
					_stopwatch.Stop();
					Disconnect();

					int sizeTransfered = _packetCounter * _bytesLength;
					double MBps = ((double)sizeTransfered / 1048576) / ((double)_stopwatch.ElapsedMilliseconds / 1000);
					double Mbps = (((double)sizeTransfered * 8) / 1000000) / ((double)_stopwatch.ElapsedMilliseconds / 1000);

					Log("Received completed.");
					Log("Total packets : " + _packetCounter); // should equals to repeat
					Log("Packets length : " + _bytesLength + " bytes");
					Log("Duration : " + _stopwatch.ElapsedMilliseconds + " ms");
					Log("Size transfered : " + sizeTransfered + " bytes");
					Log("Bandwidth : " + MBps + " MB/s");
					Log("Bandwidth : " + Mbps + " Mb/s");
					Log("");

					_packetCounter = 0;
				}

				// notify client to that we received the last byte
				//Worker4504_InputChannel.SendResponseMessage(e.ResponseReceiverId, "z");
			}
			// upload
			else if (length == 0)
			{
				_stopwatch.Stop();
				Disconnect();

				int sizeTransfered = _repeat * _bytesLength;
				double MBps = ((double)sizeTransfered / 1048576) / ((double)_stopwatch.ElapsedMilliseconds / 1000);
				double Mbps = (((double)sizeTransfered * 8) / 1000000) / ((double)_stopwatch.ElapsedMilliseconds / 1000);

				Log("Send completed.");
				Log("Total packets : " + _repeat); // should equals to repeat
				Log("Packets length : " + _bytesLength + " bytes");
				Log("Duration : " + _stopwatch.ElapsedMilliseconds + " ms");
				Log("Size transfered : " + sizeTransfered + " bytes");
				Log("Bandwidth : " + MBps + " MB/s");
				Log("Bandwidth : " + Mbps + " Mb/s");
				Log("");
			}

		}

		void Worker4504_ConnectionOpened(object sender, DuplexChannelEventArgs e)
		{
			Log("Worker 4504 connected to " + e.ResponseReceiverId);
		}

		void Worker4504_ConnectionClosed(object sender, DuplexChannelEventArgs e)
		{
			Log("Worker 4504 disconnected to " + e.ResponseReceiverId);

			EnableAllButton();
		}

		public void Disconnect()
		{
			// close all connection
			Command_MessageSender.DetachDuplexOutputChannel();
			Worker4504_OutputChannel.CloseConnection();
		}

		void Upload(object sender, RoutedEventArgs e, int repeat, int bytesLength)
		{
			DisableAllButton();

			if (Command_Initialize())
			{
				if (Worker4504_Initialize())
				{
					// create data to send
					var data = new byte[bytesLength];
					var random = new Random();
					random.NextBytes(data);

					int i = 0;
					Log("Start sending " + bytesLength + " bytes of data " + repeat + " times to " + Worker4504_OutputChannel.ResponseReceiverId);
					_stopwatch = new Stopwatch();
					_stopwatch.Start();
					// notify server
					Command_MessageSender.SendMessage("1|receive|" + Worker4504_OutputChannel.ResponseReceiverId + "|" + bytesLength + "|" + repeat);
					do
					{
						Worker4504_OutputChannel.SendMessage(data);

						i++;
					} while (i < repeat);
					// notify server that sending complete
					var b = new byte[0];
					Worker4504_OutputChannel.SendMessage(b);

					//s = "z";
					//Command_MessageReceiver.SendResponseMessage(e.ResponseReceiverId, s);
					//stopwatch.Stop();
					
					Log("Finished sending " + bytesLength + " bytes of data " + repeat + " times to " + Worker4504_OutputChannel.ResponseReceiverId + " in " + _stopwatch.ElapsedMilliseconds + " milliseconds");
				}
			}
		}

		private void Upload1MB(object sender, RoutedEventArgs e)
		{
			int multiplier = 1;

			// fragmented
			_repeat = 1;
			_bytesLength = 1048576*multiplier;

			// non fragmented
			//repeat = 730*multiplier;
			//bytesLength = 1440;

			Upload(sender, e, _repeat, _bytesLength);
			//var task1 = Task.Factory.StartNew(() => Upload(sender, e, _repeat, _bytesLength));
		}

		private void Upload5MB(object sender, RoutedEventArgs e)
		{
			int multiplier = 5;

			// fragmented
			_repeat = 1;
			_bytesLength = 1048576 * multiplier;

			// non fragmented
			//repeat = 730*multiplier;
			//bytesLength = 1440;

			Upload(sender, e, _repeat, _bytesLength);
		}

		private void Upload10MB(object sender, RoutedEventArgs e)
		{
			int multiplier = 10;

			// fragmented
			_repeat = 1;
			_bytesLength = 1048576 * multiplier;

			// non fragmented
			//repeat = 730*multiplier;
			//bytesLength = 1440;

			Upload(sender, e, _repeat, _bytesLength);
		}

		private void Upload50MB(object sender, RoutedEventArgs e)
		{
			int multiplier = 50;

			// fragmented
			_repeat = 1;
			_bytesLength = 1048576 * multiplier;

			// non fragmented
			//repeat = 730*multiplier;
			//bytesLength = 1440;

			Upload(sender, e, _repeat, _bytesLength);
		}

		private void Upload100MB(object sender, RoutedEventArgs e)
		{
			int multiplier = 100;

			// fragmented
			_repeat = 1;
			_bytesLength = 1048576 * multiplier;

			// non fragmented
			//repeat = 730*multiplier;
			//bytesLength = 1440;

			Upload(sender, e, _repeat, _bytesLength);
		}
	}
}
