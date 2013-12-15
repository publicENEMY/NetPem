using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Eneter.Messaging.EndPoints.StringMessages;
using Eneter.Messaging.EndPoints.TypedMessages;
using Eneter.Messaging.MessagingSystems.MessagingSystemBase;
using Eneter.Messaging.MessagingSystems.TcpMessagingSystem;
using NLog;

namespace WpfServer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ObservableCollection<string> _messages = new ObservableCollection<string>();
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		private const string TargetName = "memoryex";

		public MainWindow()
		{
			InitializeComponent();

			var target =
				LogManager.Configuration.AllTargets
				.Where(x => x.Name == TargetName)
				.Single() as MemoryTargetEx;

			if (target != null)
				target.Messages.Subscribe(msg => Dispatcher.InvokeAsync(() =>
				{
					_messages.Add(msg);
				}));

			Logger.Info("Initialized");
		}

		public ObservableCollection<string> IncomingMessages
		{
			get { return _messages; }
			private set { _messages = value; }
		}

		private TcpPolicyServer myPolicyServer = new TcpPolicyServer();
		private IDuplexStringMessageReceiver CommandReceiver;
		private IDuplexInputChannel Worker4504InputChannel;

		public void StartServer()
		{
			// Start the policy server to be able to communicate with silverlight.
			myPolicyServer.StartPolicyServer();

			// Create duplex message receiver.
			// It can receive messages and also send back response messages.
			IDuplexStringMessagesFactory aStringMessagesFactory = new DuplexStringMessagesFactory();
			CommandReceiver = aStringMessagesFactory.CreateDuplexStringMessageReceiver();
			CommandReceiver.ResponseReceiverConnected += ClientConnected;
			CommandReceiver.ResponseReceiverDisconnected += ClientDisconnected;
			CommandReceiver.RequestReceived += MessageReceived;

			// Create TCP based messaging.
			IMessagingSystemFactory aMessaging = new TcpMessagingSystemFactory();
			IDuplexInputChannel aDuplexInputChannel = aMessaging.CreateDuplexInputChannel("tcp://127.0.0.1:4502");

			// Attach the duplex input channel to the message receiver and start listening.
			// Note: Duplex input channel can receive messages but also send messages back.
			CommandReceiver.AttachDuplexInputChannel(aDuplexInputChannel);
			Logger.Info("Server started");

			StartWorker4504Server();
		}

		public void StartWorker4504Server()
		{
			// Create TCP based messaging.
			IMessagingSystemFactory aMessaging = new TcpMessagingSystemFactory();
			Worker4504InputChannel = aMessaging.CreateDuplexInputChannel("tcp://127.0.0.1:4504");
			Worker4504InputChannel.MessageReceived += Worker4504InputChannel_MessageReceived;
			Worker4504InputChannel.ResponseReceiverConnected += ClientConnected;
			Worker4504InputChannel.ResponseReceiverDisconnected += ClientDisconnected;
			//Worker4504InputChannel.ResponseReceiverConnected += Worker4504InputChannel_ResponseReceiverConnected;
			//Worker4504InputChannel.ResponseReceiverDisconnected += Worker4504InputChannel_ResponseReceiverDisconnected;
			Worker4504InputChannel.StartListening();

			Logger.Info("Worker server 4504 started");
		}

		void Worker4504InputChannel_MessageReceived(object sender, DuplexChannelMessageEventArgs e)
		{
			// echo back
			//Logger.Info(BitConverter.ToString(e.Message as byte[]));
			//string s = BitConverter.ToString(e.Message as byte[]);
			//Logger.Info("Received : " + s);
			Logger.Info("Received data length : " + (e.Message as byte[]).Length);
			//Logger.Info(e.Message.ToString());
			Worker4504InputChannel.SendResponseMessage(e.ResponseReceiverId, e.Message);
		}

		public void StopServer()
		{
			// Close listenig.
			// Note: If the listening is not closed, then listening threads are not ended
			//       and the application would not be closed properly.
			CommandReceiver.DetachDuplexInputChannel();
			Worker4504InputChannel.StopListening();

			myPolicyServer.StopPolicyServer();
		}

		// The method is called when a message from the client is received.
		private async void MessageReceived(object sender, StringRequestReceivedEventArgs e)
		{
			Logger.Info("Received : " + e.RequestMessage + " from " + e.ResponseReceiverId);
			try
			{
				CommandReceiver.SendResponseMessage(e.ResponseReceiverId, e.RequestMessage);
			}
			catch (Exception)
			{
			}

			// Analyze message
				// split strings
			string[] received = e.RequestMessage.Split('|');
			//Logger.Info("Split");
			foreach (string s in received)
			{
				//Logger.Info(s);
			}
			// version|command|target    |param1|param2
			// 1      |send   |responseid|123456|10
			// 1      |receive|responseid|123456|10
			// 1      |notify |responseid|open  |0
			// 1      |notify |responseid|close |0

			// receiving responsereceiverid from client
			// identify command
			// TODO:Try catch
			double version = double.Parse(received[0]);
			string command = received[1];
			string target = received[2];
			string param1 = received[3];
			string param2 = received[4];

			Logger.Info("Split " + version + " " + command + " " + target + " " + param1 + " " + param2);
			//Logger.Info("Version : " + version);
			//Logger.Info("Command : " + command);
			//Logger.Info("Target : " + target);
			//Logger.Info("Param1 : " + param1);
			//Logger.Info("Param2 : " + param2);

			if (version == 1.0)
			{
				if (command.Equals("send"))
				{
					// convert param to appropriate type
					// TODO:tryparse
					int length = int.Parse(param1);
					int repeat = int.Parse(param2);

					// Send a message.
					byte[] data = new byte[length]; // initialize 1MB data
					//byte[] data = new byte[10]; // initialize 1MB data
					Random random = new Random();
					random.NextBytes(data);
					
					int i = 0;
					string s = "a";
					Logger.Info("Start sending " + length + " bytes of data " + repeat + " times to " + target);
					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();
					CommandReceiver.SendResponseMessage(e.ResponseReceiverId, s);
					do
					{
						Worker4504InputChannel.SendResponseMessage(target, data);

						i++;
					} while (i<repeat);
					s = "z";
					CommandReceiver.SendResponseMessage(e.ResponseReceiverId, s);
					stopwatch.Stop();
					Logger.Info("Finished sending " + length + " bytes of data " + repeat + " times to " + target + " in " + stopwatch.ElapsedMilliseconds + " milliseconds");
				}
				else if (command.Equals("receive"))
				{
					
				}
				else if (command.Equals("notify"))
				{

				}
				else
				{
					Logger.Error("Unknown command : " + command);                    
				}
			}
			else
			{
				Logger.Error("Unknown command : " + e.RequestMessage);
			}

			// Calculate mtu
			// Warm up stopwatch
			// Send message
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			StopServer();
		}

		// The method is called when a client is connected.
		// The Silverlight client is connected when the client attaches the output duplex channel.
		private void ClientConnected(object sender, ResponseReceiverEventArgs e)
		{
			// Add the connected client to the listbox.
			Dispatcher.InvokeAsync(() =>
			{
				ConnectedClientsListBox.Items.Add(e.ResponseReceiverId);
			});
			Logger.Info("Connected " + e.ResponseReceiverId);
			//Logger.Info("ResponseReceiverId : " + e.ResponseReceiverId);
			//Logger.Info("SenderAddress : " + e.SenderAddress);
		}

		// The method is called when a client is disconnected.
		// The Silverlight client is disconnected if the web page is closed.
		private void ClientDisconnected(object sender, ResponseReceiverEventArgs e)
		{
			// Remove the disconnected client from the listbox.
			Dispatcher.InvokeAsync(() =>
			{
				ConnectedClientsListBox.Items.Remove(e.ResponseReceiverId);
			});
			Logger.Info("Disconnected " + e.ResponseReceiverId);
			//Logger.Info("ResponseReceiverId : " + e.ResponseReceiverId);
			//Logger.Info("SenderAddress : " + e.SenderAddress);
		}

		private void BroadcastMessage(string s)
		{
			// Send the message to all connected clients.
			foreach (string aClientId in ConnectedClientsListBox.Items)
			{
				CommandReceiver.SendResponseMessage(aClientId, s);
			}
		}

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


		private void Server_Click(object sender, RoutedEventArgs e)
		{
			if (CommandReceiver == null)
			{
				StartServer();
				var s = sender as Button;
				s.Content = "Stop Server";
			}
			else if (CommandReceiver.IsDuplexInputChannelAttached)
			{
				StopServer();
				var s = sender as Button;
				s.Content = "Start Server";
			}
			else
			{
				StartServer();
				var s = sender as Button;
				s.Content = "Stop Server";
			}
		}
	}
}
