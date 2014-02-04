using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace StreamingClientNavigation.Views
{
	public partial class Streaming : Page
	{
		public Streaming()
		{
			InitializeComponent();
#if DEBUG
			debugPanel.Visibility = Visibility.Visible;
			_debugMode = true;

#else
			debugPanel.Visibility = Visibility.Collapsed;
			_debugMode = false;
#endif

			// manual override
			//debugPanel.Visibility = Visibility.Visible;
			//_debugMode = false;

			InitializeMediaPlayer();
		}

		// Executes when the user navigates to this page.
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
		}

		#region Log

		private void Log(object message)
		{
			if (_debugMode)
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					LogListBox.Items.Add(new ListBoxItem { Content = DateTime.Now.ToString("hh:mm:ss.fff") + "|" + message });
					//ScrollListboxToEnd(LogListBox);
				}));
			}
		}

		private void ClearLog()
		{
			Dispatcher.BeginInvoke(new Action(() => LogListBox.Items.Clear()));
		}

		void ScrollListboxToEnd(ListBox lb)
		{
			//Force listbox to scroll to end
			var svAutomation = (ListBoxAutomationPeer)FrameworkElementAutomationPeer.CreatePeerForElement(lb);

			var scrollInterface = (IScrollProvider)svAutomation.GetPattern(PatternInterface.Scroll);
			var scrollVertical = ScrollAmount.LargeIncrement;
			var scrollHorizontal = ScrollAmount.NoAmount;
			//If the vertical scroller is not available, the operation cannot be performed, which will raise an exception. 
			if (scrollInterface.VerticallyScrollable)
				scrollInterface.Scroll(scrollHorizontal, scrollVertical);
		}

		#endregion

		#region Javascript stuff
		// Call a global JavaScript method defined on the HTML page.
		// Pass result to javascript here
		// Results
		// sample_1080p_buffer - int rebuffering count
		// sample_1080p_frame - int max frame lost per second
		// sample_1080p_bandwidth - megabits
		// sample_1080p_time - duration in seconds

		// sample_720p_buffer
		// sample_720p_frame
		// sample_720p_bandwidth
		// sample_720p_time 

		// sample_480p_buffer
		// sample_480p_frame
		// sample_480p_bandwidth
		// sample_480p_time 

		// sample_360p_buffer
		// sample_360p_frame
		// sample_360p_bandwidth
		// sample_360p_time 

		// upload download are similar
		// sample_size1_rate - megabits?

		// sample_size2_rate

		// sample_size3_rate

		private void SendFakeStreamingResultsToJs(object sender, RoutedEventArgs e)
		{
			var r = new Random();
			//rebuffering count
			int buffer = r.Next(0, 10);
			//frame drops
			int frame = r.Next(0, 10);
			//estimate bandwidth
			double bandwidth = r.Next(0, 10) + r.NextDouble();

			double duration = r.Next(0, 100) + r.NextDouble();

			Log("Send fake streaming results to js");
			HtmlPage.Window.Invoke("ProcessStreamingResults", buffer, frame, bandwidth, duration);
		}

		private void SendStreamingResultsToJs()
		{
			Log("Send streaming results to js");
			HtmlPage.Window.Invoke("ProcessStreamingResults", _rebuffering, _droppedFrames, _bandwidth, _duration);
		}

		#endregion

		private bool _debugMode;

		// total rebuffering
		private int _rebuffering = 0;

		// highest frame drops per second
		private int _droppedFrames = 0;

		// average bandwidth
		private double _bandwidth = 0;

		// playback duration
		private double _duration = 0;

		DispatcherTimer _stateUpdate = new DispatcherTimer();

		private void InitializeMediaPlayer()
		{
			Log("Initialize media player");
			// register timer events
			_stateUpdate.Interval = TimeSpan.FromMilliseconds(1000);
			_stateUpdate.Tick += PlaybackUpdates;
			//_stateUpdate.Start();

			// register events
			MediaPlayer.BufferingProgressChanged += MediaPlayer_BufferingProgressChanged;
			MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
			MediaPlayer.DownloadProgressChanged += MediaPlayer_DownloadProgressChanged;
			MediaPlayer.Loaded += MediaPlayer_Loaded;
			MediaPlayer.LogReady += MediaPlayer_LogReady;
			MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
			MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
			MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
			MediaPlayer.RateChanged += MediaPlayer_RateChanged;

			//PlayerWindow.Visibility = Visibility.Collapsed;
		}

		void PlaybackUpdates(object sender, EventArgs e)
		{
			// updates stats

			// rebuffering

			// frame drops
			if (_droppedFrames < MediaPlayer.DroppedFramesPerSecond)
			{
				_droppedFrames = (int)MediaPlayer.DroppedFramesPerSecond;
				if (_debugMode)
				{
					MaxDroppedFpsTextBox.Text = _droppedFrames.ToString();
				}
			}

			// bandwidth
			// updates in downloadprogresschanged

			// duration
			_duration = ((double)_playbackDurationStopwatch.ElapsedMilliseconds / 1000);

			if (_debugMode)
			{
				//Log("PlaybackUpdates");
				// updates ui
				DurationTextBox.Text = _duration.ToString();
				DroppedFpsTextBox.Text = MediaPlayer.DroppedFramesPerSecond.ToString();
				RenderedFpsTextBox.Text = MediaPlayer.RenderedFramesPerSecond.ToString();
				MediaFpsTextBox.Text = (MediaPlayer.DroppedFramesPerSecond + MediaPlayer.RenderedFramesPerSecond).ToString();
			}
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Log("Window size changed");
			UpdatePlayerWindowSize();
		}

		void UpdatePlayerWindowSize()
		{
			MediaPlayer.MaxHeight = ActualHeight - (ProgressBarGrid.ActualHeight + PlayerPanel.ActualHeight + StatusBar.ActualHeight + 25);
			MediaPlayer.MaxWidth = ActualWidth;
			PlayerWindow.MaxWidth = ActualWidth;
			PlayerWindow.MaxHeight = ActualHeight;

			//MediaPlayer.Height = MediaPlayer.MaxHeight;
			//MediaPlayer.Width = MediaPlayer.MaxWidth;
			//PlayerWindow.Width = PlayerWindow.MaxWidth;
			//PlayerWindow.Height = PlayerWindow.MaxHeight;

			//Log("MediaPlayer.MaxHeight = " + MediaPlayer.MaxHeight);
			//Log("MediaPlayer.MaxWidth = " + MediaPlayer.MaxWidth);
			//Log("PlayerWindow.MaxHeight = " + PlayerWindow.MaxHeight);
			//Log("PlayerWindow.MaxWidth = " + PlayerWindow.MaxWidth);

			//Log("MediaPlayer.ActualHeight = " + MediaPlayer.ActualHeight);
			//Log("MediaPlayer.ActualWidth = " + MediaPlayer.ActualWidth);
			//Log("PlayerWindow.ActualHeight = " + PlayerWindow.ActualHeight);
			//Log("PlayerWindow.ActualWidth = " + PlayerWindow.ActualWidth);

			Log("Updated window size");
		}

		void MediaPlayer_RateChanged(object sender, RateChangedRoutedEventArgs e)
		{
			if (_debugMode)
			{
				PlaybackRateTextBox.Text = MediaPlayer.PlaybackRate.ToString();
			}
		}

		void ResetVariables()
		{
			// total rebuffering
			_rebuffering = 0;

			// highest frame drops per second
			_droppedFrames = 0;

			// average bandwidth
			_bandwidth = 0;

			// playback duration
			_duration = 0;

			// file size
			_mediaFileSize = 0;

			StopAllStopwatch();

			if (_debugMode)
			{
				Log("Resetted variables");
				// reset debug ui
				CurrentStateListBox.Items.Clear();
				BandwidthListBox.Items.Clear();
				LogListBox.Items.Clear();
			}
		}

		Stopwatch _playbackDurationStopwatch = new Stopwatch();

		// used to calculate bandwidth
		Stopwatch _downloadProgressStopwatch = new Stopwatch();

		// file size
		private long _mediaFileSize = 0;

		private void StopAllStopwatch()
		{
			_playbackDurationStopwatch.Stop();
			_downloadProgressStopwatch.Stop();
			if (_debugMode)
			{
				Log("Stopped all stopwatch");
			}
		}

		void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
		{
			Log("Media opened");
			StatusBar.Text = "Media opened";
			//_playbackDurationStopwatch.Reset();
			//_playbackDurationStopwatch.Start();

			//_downloadProgressStopwatch.Reset();
			//_downloadProgressStopwatch.Start();

			//resize player window
			UpdatePlayerWindowSize();

			// reset counter
			//_rebuffering = 0;
			//_droppedFrames = 0;
			//_bandwidth = 0.0;

			if (_debugMode)
			{
				// update ui
				PlaybackRateTextBox.Text = MediaPlayer.PlaybackRate.ToString();
			}
		}

		void MediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
		{
			Log("Media failed" + e.ErrorException);
			StatusBar.Text = "Media playback failed : " + e.ErrorException;
		}

		void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
		{
			_duration = ((double)_playbackDurationStopwatch.ElapsedMilliseconds / 1000);
			_playbackDurationStopwatch.Stop();
			_stateUpdate.Stop();

			if (_debugMode)
			{
				DurationTextBox.Text = _duration.ToString();
			}

			Log("Media playback ended");
			StatusBar.Text = "Media playback ended";
			ProceedToNextPage();
		}

		void MediaPlayer_LogReady(object sender, LogReadyRoutedEventArgs e)
		{
		}

		void MediaPlayer_Loaded(object sender, RoutedEventArgs e)
		{
			Log("Media player loaded");
			StatusBar.Text = "Media player loaded";
			UpdatePlayerWindowSize();
		}

		private double excessDownloaded = 0;
		private double lastBandwidth;
		void MediaPlayer_DownloadProgressChanged(object sender, RoutedEventArgs e)
		{
			if (!_stateUpdate.IsEnabled)
			{
				Log("Start playback update timer");
				_stateUpdate.Start();
			}
			if (!_playbackDurationStopwatch.IsRunning)
			{
				Log("Start duration stopwatch");
				_playbackDurationStopwatch.Reset();
				_playbackDurationStopwatch.Start();
			}
			if (!_downloadProgressStopwatch.IsRunning)
			{
				if (MediaPlayer.DownloadProgress < 1)
				{
					Log("Start download progress stopwatch");
					_downloadProgressStopwatch.Reset();
					_downloadProgressStopwatch.Start();
					//StartAllStopwatch();
					// ignore downloaded bits before stopwatch is started
					excessDownloaded = MediaPlayer.DownloadProgress;
				}
			}
			else
			{
				//double elapsedDownloadDuration = (double)_downloadProgressStopwatch.ElapsedMilliseconds / 1000.0;
				//double downloadProgress = MediaPlayer.DownloadProgress - excessDownloaded;
				//long fileSize = _mediaFileSize * 8;

				if (MediaPlayer.DownloadProgress >= 1)
				{
					Log("Download completed and download progress stopwatch stopped");
					_downloadProgressStopwatch.Stop();
				}
				//_bandwidth = ((_mediaFileSize * MediaPlayer.DownloadProgress * 8) / (_downloadProgressStopwatch.ElapsedMilliseconds / 1000.0)) / 1000000;
				_bandwidth = ((_mediaFileSize * (MediaPlayer.DownloadProgress - excessDownloaded) * 8) / (_downloadProgressStopwatch.ElapsedMilliseconds / 1000.0)) / 1000000;
				//_bandwidth = ((fileSize * downloadProgress) / elapsedDownloadDuration) / 1000000;

				if (_debugMode)
				{
					AverageBandwidthTextBox.Text = _bandwidth.ToString();
					if (Double.IsNaN(_bandwidth))
					{
						Log("Bandwidth = NaN");
					}
					else if (_bandwidth == 0.0)
					{
						Log("Bandwidth = 0");
					}
					DownloadProgressTextBox.Text = MediaPlayer.DownloadProgress.ToString();

					BandwidthListBox.Items.Add(new ListBoxItem { Content = DateTime.Now.ToString("hh:mm:ss.fff") + "|" + _bandwidth });
					//ScrollListboxToEnd(BandwidthListBox);
				}

				if (Double.IsNaN(_bandwidth))
				{
					_bandwidth = lastBandwidth;
				}
				else if (_bandwidth == 0.0)
				{
					_bandwidth = lastBandwidth;
				}
				else
				{
					lastBandwidth = _bandwidth;
				}
			}
		}

		void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
		{
			if (MediaPlayer.CurrentState == MediaElementState.Buffering)
			{
				_rebuffering++;
				if (_debugMode)
				{
					RebufferingCountTextBox.Text = _rebuffering.ToString();
				}
			}
			if (_debugMode)
			{
				var lbi = new ListBoxItem();
				lbi.Content = DateTime.Now.ToString("hh:mm:ss.fff") + "|" + MediaPlayer.CurrentState;
				CurrentStateListBox.Items.Add(lbi);
				//ScrollListboxToEnd(CurrentStateListBox);		
				Log("State changed to " + MediaPlayer.CurrentState);
			}
			StatusBar.Text = MediaPlayer.CurrentState.ToString();
		}

		void MediaPlayer_BufferingProgressChanged(object sender, RoutedEventArgs e)
		{
			if (_debugMode)
			{
				BufferingProgressTextBox.Text = MediaPlayer.BufferingProgress.ToString();
			}
			StatusBar.Text = "Buffering " + (MediaPlayer.BufferingProgress * 100) + "%";
		}

		void ProceedToNextPage()
		{
			SendStreamingResultsToJs();
		}

		private static string ServerPath = "http://219.93.8.3/netpem/streaming/media/";
		private static string InvalidateMedia = "?query=";
		private string Media360p = ServerPath + "The Adventures of Tintin- Biggest Adventure- Now Playing(360p_H.264-AAC).mp4" + InvalidateMedia;
		private string Media480p = ServerPath + "The Adventures of Tintin- Biggest Adventure- Now Playing(480p_H.264-AAC).mp4" + InvalidateMedia;
		private string Media720p = ServerPath + "The Adventures of Tintin- Biggest Adventure- Now Playing(720p_H.264-AAC).mp4" + InvalidateMedia;
		private string Media1080p = ServerPath + "The Adventures of Tintin- Biggest Adventure- Now Playing(1080p_H.264-AAC).mp4" + InvalidateMedia;

		private void GetFileSize(string url)
		{
			Log("Requesting file size");
			StatusBar.Text = "Requesting file size";
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(url);
				request.BeginGetResponse(FinishWebRequest, request);
			}
			catch (WebException e)
			{
				Log("Error requesting file size : " + e.Status + "Response : " + e.Response);
				StatusBar.Text = "Error requesting file size : " + e.Status + "Response : " + e.Response;
			}
			catch (Exception e)
			{
				Log("Error requesting file size : " + e.Message);
				StatusBar.Text = "Error requesting file size : " + e.Message;
			}
		}

		private void FinishWebRequest(IAsyncResult result)
		{
			var response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
			_mediaFileSize = response.ContentLength;
			Log("Filesize aquired");

			Dispatcher.BeginInvoke(new Action(() =>
			{
				var r = new Random();

				if (_debugMode)
				{
					FileSizeTextBox.Text = _mediaFileSize.ToString();
				}
				//StopAllStopwatch();
				StatusBar.Text = "Filesize aquired";
				Log("Assigning source to media player");
				StatusBar.Text = "Initiating media player";
				MediaPlayer.Source = new Uri(response.ResponseUri.AbsoluteUri + r.Next());
			}));
		}

		private void StartStreaming1080p_Click(object sender, RoutedEventArgs e)
		{
			Log("1080p media selected");
			StatusBar.Text = "Streaming 1080p media";
			StartStack.Children.Clear();
			PlayerWindow.Visibility = Visibility.Visible;
			ResetVariables();
			GetFileSize(Media1080p);
			StatusGrid.Visibility = Visibility.Visible;
		}

		private void StartStreaming720p_Click(object sender, RoutedEventArgs e)
		{
			Log("720p media selected");
			StatusBar.Text = "Streaming 720p media";
			StartStack.Children.Clear();
			PlayerWindow.Visibility = Visibility.Visible;
			ResetVariables();
			GetFileSize(Media720p);
			StatusGrid.Visibility = Visibility.Visible;
		}

		private void StartStreaming480p_Click(object sender, RoutedEventArgs e)
		{
			Log("480p media selected");
			StatusBar.Text = "Streaming 480p media";
			StartStack.Children.Clear();
			PlayerWindow.Visibility = Visibility.Visible;
			ResetVariables();
			GetFileSize(Media480p);
			StatusGrid.Visibility = Visibility.Visible;
		}

		private void StartStreaming360p_Click(object sender, RoutedEventArgs e)
		{
			Log("360p media selected");
			StatusBar.Text = "Streaming 360p media";
			StartStack.Children.Clear();
			PlayerWindow.Visibility = Visibility.Visible;
			ResetVariables();
			GetFileSize(Media360p);
			StatusGrid.Visibility = Visibility.Visible;
		}

		private void Pause(object sender, RoutedEventArgs e)
		{
			Log("Manual Pause");
			MediaPlayer.Pause();
		}

		private void Stop(object sender, RoutedEventArgs e)
		{
			Log("Manual Stop");
			MediaPlayer.Stop();

		}

		private void Play(object sender, RoutedEventArgs e)
		{
			Log("Manual Play");
			MediaPlayer.Play();

		}
	}
}
