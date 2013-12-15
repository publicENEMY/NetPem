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

namespace SilverlightClient
{
	public partial class MainPage : UserControl
	{
		private IDuplexStringMessageSender CommandMessageSender;
		private IDuplexOutputChannel Worker4504OutputChannel;

		//private int repeat = 72900; private int bytesLength = 1440;
		private int repeat = 1; private int bytesLength = 104976000;

		// TODO:optimize this
		// global stopwatch
		Stopwatch stopwatch = new Stopwatch();
		private int packetCounter = 0;

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
			repeat = 1;
			bytesLength = 1048576;

			// non fragmented
			//repeat = 730;
			//bytesLength = 1440;

			var b = sender as Button;
			b.IsEnabled = false;

			if (InitializeCommandConnection())
			{
				if (InitializeWorkerConnection())
				{
					CommandMessageSender.SendMessage("1|send|" + Worker4504OutputChannel.ResponseReceiverId + "|" + bytesLength + "|" + repeat); //344ms 1 repeat
				}
			}
		}

		private void Download5MB(object sender, RoutedEventArgs e)
		{
			// fragmented
			repeat = 1;
			bytesLength = 1048576*5;

			// non fragmented
			//repeat = 730*5;
			//bytesLength = 1440;

			var b = sender as Button;
			b.IsEnabled = false;

			if (InitializeCommandConnection())
			{
				if (InitializeWorkerConnection())
				{
					CommandMessageSender.SendMessage("1|send|" + Worker4504OutputChannel.ResponseReceiverId + "|" + bytesLength + "|" + repeat); //344ms 1 repeat
				}
			}
		}

		private void Download10MB(object sender, RoutedEventArgs e)
		{
			// fragmented
			repeat = 1;
			bytesLength = 1048576*10;

			// non fragmented
			//repeat = 730*10;
			//bytesLength = 1440;

			var b = sender as Button;
			b.IsEnabled = false;

			if (InitializeCommandConnection())
			{
				if (InitializeWorkerConnection())
				{
					CommandMessageSender.SendMessage("1|send|" + Worker4504OutputChannel.ResponseReceiverId + "|" + bytesLength + "|" + repeat); //344ms 1 repeat
				}
			}
		}

		private void Download50MB(object sender, RoutedEventArgs e)
		{
			// fragmented
			repeat = 1;
			bytesLength = 1048576*50;

			// non fragmented
			//repeat = 730*50;
			//bytesLength = 1440;

			var b = sender as Button;
			b.IsEnabled = false;

			if (InitializeCommandConnection())
			{
				if (InitializeWorkerConnection())
				{
					CommandMessageSender.SendMessage("1|send|" + Worker4504OutputChannel.ResponseReceiverId + "|" + bytesLength + "|" + repeat); //344ms 1 repeat
				}
			}
		}

		private void Download100MB(object sender, RoutedEventArgs e)
		{
			// fragmented
			repeat = 1;
			bytesLength = 1048576*100;

			// non fragmented
			//repeat = 730*100;
			//bytesLength = 1440;

			var b = sender as Button;
			b.IsEnabled = false;

			if (InitializeCommandConnection())
			{
				if (InitializeWorkerConnection())
				{
					CommandMessageSender.SendMessage("1|send|" + Worker4504OutputChannel.ResponseReceiverId + "|" + bytesLength + "|" + repeat); //344ms 1 repeat
				}
			}
		}

		private bool InitializeCommandConnection()
		{
			// Create duplex message sender.
			// It can send messages and also receive messages.
			IDuplexStringMessagesFactory aStringMessagesFactory = new DuplexStringMessagesFactory();
			CommandMessageSender = aStringMessagesFactory.CreateDuplexStringMessageSender();
			CommandMessageSender.ResponseReceived += CommandResponseReceived;

			// Create TCP based messaging.
			IMessagingSystemFactory aMessaging = new TcpMessagingSystemFactory();
			IDuplexOutputChannel aDuplexOutputChannel = aMessaging.CreateDuplexOutputChannel("tcp://127.0.0.1:4502/");

			// Attach the duplex output channel to the message sender
			// and be able to send messages and receive messages.
			try
			{
				CommandMessageSender.AttachDuplexOutputChannel(aDuplexOutputChannel);
			}
			catch (Exception e)
			{
				Log(e.Message);
			}

			if (CommandMessageSender.IsDuplexOutputChannelAttached)
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

		public void Disconnect()
		{
			// close all connection
			CommandMessageSender.DetachDuplexOutputChannel();
			Worker4504OutputChannel.CloseConnection();
		}

		private void CommandResponseReceived(object sender, StringResponseReceivedEventArgs e)
		{
			Log("Received CMD : " + e.ResponseMessage);

			if (e.ResponseMessage.Length == 1)
			{
				// this is a command from server
				if (e.ResponseMessage.Equals("a"))
				{
					// start
					stopwatch.Reset();
					stopwatch.Start();
					packetCounter = 0;
				}
			}
		}

		private bool InitializeWorkerConnection()
		{
			// Create TCP messaging
			IMessagingSystemFactory aMessaging = new TcpMessagingSystemFactory();
			Worker4504OutputChannel = aMessaging.CreateDuplexOutputChannel("tcp://127.0.0.1:4504/");

			// Subscribe to response messages.
			Worker4504OutputChannel.ConnectionClosed += Worker4504OutputChannel_ConnectionClosed;
			Worker4504OutputChannel.ConnectionOpened += Worker4504OutputChannel_ConnectionOpened;
			Worker4504OutputChannel.ResponseMessageReceived += Worker4504OutputChannel_ResponseMessageReceived;

			// Open connection and be able to send messages and receive response messages.
			Worker4504OutputChannel.OpenConnection();

			if (Worker4504OutputChannel.IsConnected)
			{
				return true;		        
			}
			else
			{
				return false;
			}
		}

		void Worker4504OutputChannel_ResponseMessageReceived(object sender, DuplexChannelMessageEventArgs e)
		{
			packetCounter++;

			if (packetCounter >= (repeat - 1)) 
			{
				stopwatch.Stop();
				Disconnect();

				int sizeTransfered = packetCounter*bytesLength;
				double MBps = ((double)sizeTransfered / 1048576) / ((double)stopwatch.ElapsedMilliseconds / 1000);
				double Mbps = (((double)sizeTransfered * 8) / 1048576) / ((double)stopwatch.ElapsedMilliseconds / 1000);

				Log("Received completed.");
				Log("Total packets : " + packetCounter); // should equals to repeat
				Log("Packets length : " + bytesLength + " bytes");
				Log("Duration : " + stopwatch.ElapsedMilliseconds + " ms");
				Log("Size transfered : " + sizeTransfered + " bytes");
				Log("Bandwidth : " + MBps + " MB/s");
				Log("Bandwidth : " + Mbps + " Mb/s");
				Log("");

				packetCounter = 0;
			}
		}

		void Worker4504OutputChannel_ConnectionOpened(object sender, DuplexChannelEventArgs e)
		{
			Log("Worker 4504 connected to " + e.ResponseReceiverId);

			// irrelevant
			CommandMessageSender.SendMessage("1|notify|" + e.ResponseReceiverId + "|open|0");	
		}

		void Worker4504OutputChannel_ConnectionClosed(object sender, DuplexChannelEventArgs e)
		{
			Log("Worker 4504 disconnected to " + e.ResponseReceiverId);

			EnableAllButton();
		}
	}
}
