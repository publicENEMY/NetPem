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
		public MainPage()
		{
			InitializeComponent();
		}

#region Old
		// The method is called when a message from the desktop application is received.
		private void ResponseReceived(object sender, TypedResponseReceivedEventArgs<byte[]> e)
		{
			textBox2.Text = GetString(e.ResponseMessage);
		}

		// The method is called when the button to send message is clicked.
		private void SendMessage_Click(object sender, RoutedEventArgs e)
		{
			// Create message sender sending request messages of type Person and receiving responses of type string.
			IDuplexTypedMessagesFactory aTypedMessagesFactory = new DuplexTypedMessagesFactory();
			myMessageSender = aTypedMessagesFactory.CreateDuplexTypedMessageSender<byte[], byte[]>();
			myMessageSender.ResponseReceived += ResponseReceived;

			// Create messaging based on TCP.
			IMessagingSystemFactory aMessagingSystemFactory = new TcpMessagingSystemFactory();
			IDuplexOutputChannel aDuplexOutputChannel = aMessagingSystemFactory.CreateDuplexOutputChannel("tcp://127.0.0.1:4502/");

			// Attach output channel and be able to send messages and receive response messages.
			myMessageSender.AttachDuplexOutputChannel(aDuplexOutputChannel);

			myMessageSender.SendRequestMessage(GetBytes(textBox1.Text));
		}
		
		private IDuplexTypedMessageSender<byte[], byte[]> myMessageSender;
#endregion

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

#endregion
		private void Download1MB(object sender, RoutedEventArgs e)
		{
			if (InitializeCommandConnection())
			{
				// 1|send|responseid|123456|10
				if (InitializeWorkerConnection())
				{
					//CommandMessageSender.SendMessage("1|send|" + Worker4504OutputChannel.ResponseReceiverId + "|1048576|100");
					//CommandMessageSender.SendMessage("1|send|" + Worker4504OutputChannel.ResponseReceiverId + "|104857600|1"); //
					//CommandMessageSender.SendMessage("1|send|" + Worker4504OutputChannel.ResponseReceiverId + "|1440|72900"); //2184ms
					CommandMessageSender.SendMessage("1|send|" + Worker4504OutputChannel.ResponseReceiverId + "|104976000|1"); //344ms
				}
			}

		}

		//private IDuplexTypedMessageSender<byte[], byte[]>[] WorkerMessageSender = new IDuplexTypedMessageSender<byte[], byte[]>[256];
		//private IDuplexTypedMessageSender<byte[], byte[]> WorkerMessageSender;
		private IDuplexStringMessageSender CommandMessageSender;

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

		// TODO:optimize this
		// global stopwatch
		Stopwatch stopwatch = new Stopwatch();
		private int packetCounter = 0;

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
					stopwatch.Start();
					packetCounter = 0;
				}
				else if (e.ResponseMessage.Equals("z"))
				{
					// stop
					//stopwatch.Stop();

					// disconnect
					//Disconnect();

					// display report
					//Log("Received completed. Total " + packetCounter + " packets in " + stopwatch.ElapsedMilliseconds + " milliseconds.");
				}
			}

			// Analyze command received(received list of server to connect to) ip:port
			// if download
			// connect to worker 
			// ready for download
			// if upload
			// use this for upload
			// Connect to supplied list of server
			
			/*
			var ip = new IPEndPoint[256];
			ip[0].Address = IPAddress.Loopback;
			ip[0].Port = 4504;
			byte[] data = new byte[1048576]; // initialize 1MB data
			Random random = new Random();
			random.NextBytes(data);

			if (InitializeWorkerConnection(ip[0]))
			{
				WorkerMessageSender[0].SendRequestMessage(data);
			}
			*/

			// test disconnect
			//if(CommandMessageSender.IsDuplexOutputChannelAttached)
			//	CommandMessageSender.DetachDuplexOutputChannel();
		}

		private IDuplexOutputChannel Worker4504OutputChannel;
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
			//Log("ChannelId : " + Worker4504OutputChannel.ChannelId);
			//Log("ResponseReceiverId : " + Worker4504OutputChannel.ResponseReceiverId);


			// Send a message.
			//byte[] data = new byte[1048576]; // initialize 1MB data
			////byte[] data = new byte[10]; // initialize 1MB data
			//Random random = new Random();
			//random.NextBytes(data);
			//Worker4504OutputChannel.SendMessage(data);
			//Log("Sent data length : " + data.Length);

			// Close connection.
			//Worker4504OutputChannel.CloseConnection();

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
			Log("Received WRK no " + packetCounter + " length " + (e.Message as byte[]).Length + " from " + e.ResponseReceiverId);

			packetCounter++;

			// temp
			stopwatch.Stop();
			Log("Received completed. Total " + packetCounter + " packets in " + stopwatch.ElapsedMilliseconds + " milliseconds.");
			
			//Log("ChannelId : " + e.ChannelId);
			//Log("ResponseReceiverId : " + e.ResponseReceiverId);

			//string s = BitConverter.ToString(e.Message as byte[]);
			//Log("Received : " + s);
			//Log("Received : " + e.Message.ToString());
			//Log("Received : " + BitConverter.ToString(e.Message as byte[]));
			//Worker4504OutputChannel.SendMessage(e.Message);
		}

		void Worker4504OutputChannel_ConnectionOpened(object sender, DuplexChannelEventArgs e)
		{
			Log("Worker 4504 connected to " + e.ResponseReceiverId);
			//Log("ChannelId : " + e.ChannelId);
			//Log("ResponseReceiverId : " + e.ResponseReceiverId);
			//Log("SenderAddress : " + e.SenderAddress);

			//send to server connection id
			// 1      |send   |responseid|123456|10
			// 1      |receive|responseid|123456|10
			// 1      |notify |responseid|open  |0
			// 1      |notify |responseid|close |0
			CommandMessageSender.SendMessage("1|notify|" + e.ResponseReceiverId + "|open|0");	
		}

		void Worker4504OutputChannel_ConnectionClosed(object sender, DuplexChannelEventArgs e)
		{
			Log("Worker 4504 disconnected to " + e.ResponseReceiverId);
			//Log("ChannelId : " + e.ChannelId);
			//Log("ResponseReceiverId : " + e.ResponseReceiverId);
			//Log("SenderAddress : " + e.SenderAddress);

			//notify server closed connection id
			try
			{
				// probably unable to execute due to client already dc
				CommandMessageSender.SendMessage("1|notify|" + e.ResponseReceiverId + "|close|0");
			}
			catch (Exception)
			{
			}
		}
	}
}
