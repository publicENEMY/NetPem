using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Text;
using System.IO;

namespace StreamingExperience.Views
{
	public partial class Streaming : Page
	{
		DispatcherTimer generalUpdateTimer = new DispatcherTimer();
		DispatcherTimer framePerformanceUpdate = new DispatcherTimer();
		DispatcherTimer playbakProgressTimer = new DispatcherTimer();
		//RebufferingOverrider ro = new RebufferingOverrider();

		//Data structure for media
		//public static List<myMediaInfo> App.myMediaList = new List<myMediaInfo>();
		private bool useOverriding = false;

		public Streaming()
		{
			InitializeComponent();

#if DEBUG
			debugStack.Visibility = Visibility.Visible;
#else
			debugStack.Visibility = Visibility.Collapsed;
#endif

			framePerformanceUpdate.Interval = TimeSpan.FromMilliseconds(1000);
			framePerformanceUpdate.Tick += (o, s) => DroppedFramesCounter();
			framePerformanceUpdate.Start();

			generalUpdateTimer.Interval = TimeSpan.FromMilliseconds(500);
			generalUpdateTimer.Tick += (o, s) => GeneralUpdate();//buffering overriding method 

			playbakProgressTimer.Interval = TimeSpan.FromMilliseconds(30);
			playbakProgressTimer.Tick += (o, s) => PlaybackProgressUpdate();//updates position

			// sets buffering time to 1 day
			if (useOverriding)
			{
				SilverlightPlayer.BufferingTime = new System.TimeSpan(1, 0, 0, 0);
			}
			else
			{
				//SilverlightPlayer.BufferingTime = new System.TimeSpan(0, 0, 0, 5);// 5 seconds
				SilverlightPlayer.BufferingTime = new System.TimeSpan(0, 0, 0, 1);// 1 seconds
				//SilverlightPlayer.BufferingTime = new System.TimeSpan(0, 0, 0, 0, 1);// 1 milliseconds
			}


		}

		private void PlaybackProgressUpdate()
		{
			pbPlaybackProgress.Value = SilverlightPlayer.Position.TotalSeconds;
		}

		private void DroppedFramesCounter()
		{
			// get frames per second and dropped frames per second here
			tbRenderedFPS.Text = SilverlightPlayer.RenderedFramesPerSecond.ToString();
			tbDroppedFrames.Text = SilverlightPlayer.DroppedFramesPerSecond.ToString();

			if (useOverriding)
			{
				if (SilverlightPlayer.CurrentState == MediaElementState.Playing)
				{
					droppedFramesCounter.Text = App.ro.droppedFramesCounter.ToString();
					droppedFramesPerSecond.Text = SilverlightPlayer.DroppedFramesPerSecond.ToString();
					App.ro.droppedFramesCounter = App.ro.droppedFramesCounter + SilverlightPlayer.DroppedFramesPerSecond;

					//droppedFramesCounter.Text = App.ro.droppedFramesCounter.ToString();
					//DroppedFramesPerSecond.Text = SilverlightPlayer.DroppedFramesPerSecond.ToString();
					//App.ro.droppedFramesCounter = App.ro.droppedFramesCounter + SilverlightPlayer.DroppedFramesPerSecond;
					//tbDroppedFramesCounter.Text = App.ro.droppedFramesCounter.ToString();
				}
			}
			else
			{
				if (SilverlightPlayer.CurrentState == MediaElementState.Playing)
				{
					droppedFramesCounter.Text = App.ro.droppedFramesCounter.ToString();
					droppedFramesPerSecond.Text = SilverlightPlayer.DroppedFramesPerSecond.ToString();
					App.ro.droppedFramesCounter = App.ro.droppedFramesCounter + SilverlightPlayer.DroppedFramesPerSecond;
				}
			}
		}

		private void GeneralUpdate()
		{
			//TODO : Dont get from ui. change to static or const
			// Get values from UI
			App.ro.upperThreshold = Convert.ToDouble(tbUpperThreshold.Text);
			App.ro.lowerThreshold = Convert.ToDouble(tbLowerThreshold.Text);

			// Set values to UI
			//tbRenderedFPS.Text = SilverlightPlayer.RenderedFramesPerSecond.ToString();
			//tbDroppedFrames.Text = SilverlightPlayer.DroppedFramesPerSecond.ToString();
			tbDownloadProgressOffset.Text = SilverlightPlayer.DownloadProgressOffset.ToString();
			//tbMediaTimeline.Text = string.Format("{0:00}:{1:00}:{2:00} / {3:00}:{4:00}:{5:00}",
			//    SilverlightPlayer.Position.Hours, 
			//    SilverlightPlayer.Position.Minutes, 
			//    SilverlightPlayer.Position.Seconds,
			//    SilverlightPlayer.NaturalDuration.TimeSpan.Hours, 
			//    SilverlightPlayer.NaturalDuration.TimeSpan.Minutes, 
			//    SilverlightPlayer.NaturalDuration.TimeSpan.Seconds);

			tbMediaTimeline.Text = string.Format("{0:0}:{1:00} / {2:0}:{3:00}",
				SilverlightPlayer.Position.Minutes,
				SilverlightPlayer.Position.Seconds,
				SilverlightPlayer.NaturalDuration.TimeSpan.Minutes,
				SilverlightPlayer.NaturalDuration.TimeSpan.Seconds);

			//position in seconds
			App.ro.overridePosition = SilverlightPlayer.Position.TotalSeconds;
			//download progress in seconds
			App.ro.overrideDownloadProgress = SilverlightPlayer.DownloadProgress *
				SilverlightPlayer.NaturalDuration.TimeSpan.TotalSeconds;
			//difference between downloaded and current playback position in seconds
			App.ro.overrideDelta = App.ro.overrideDownloadProgress - App.ro.overridePosition;

			// Set values to UI
			tbOverridePosition.Text = App.ro.overridePosition.ToString();
			tbOverrideDownloadProgress.Text = App.ro.overrideDownloadProgress.ToString();
			tbOverrideDelta.Text = App.ro.overrideDelta.ToString();

			if (useOverriding)
			{
				if (SilverlightPlayer.CurrentState == MediaElementState.Playing)
				{
					//framePerformanceUpdate.Start();

					//if (!App.ro.bufferingInProgress)//testing disable this
					{
						if (SilverlightPlayer.DownloadProgress != 1) //only pause and rebuffer if download progress is not complete
						{
							//if (App.ro.overrideDownloadProgress < (App.ro.overridePosition + App.ro.lowerThreshold))
							if (App.ro.overrideDelta < 1.0)
							{
								SilverlightPlayer.Pause();
								App.ro.bufferingInProgress = true;
								App.ro.rebufferingCounter++;
								tbRebufferingCounter.Text = App.ro.rebufferingCounter.ToString();
							}
						}
					}
					//App.ro.droppedFramesCounter = App.ro.droppedFramesCounter + SilverlightPlayer.DroppedFramesPerSecond;
					//tbDroppedFramesCounter.Text = App.ro.droppedFramesCounter.ToString();
				}
				if (SilverlightPlayer.CurrentState == MediaElementState.Paused || SilverlightPlayer.CurrentState == MediaElementState.Buffering)
				{
					//framePerformanceUpdate.Stop();

					if (App.ro.bufferingInProgress)
					{
						if (SilverlightPlayer.DownloadProgress == 1) // if completed download, theres no need to pause
						{
							SilverlightPlayer.Play();
							App.ro.bufferingInProgress = false;
						}
						//else if (App.ro.overrideDownloadProgress > (App.ro.overridePosition + App.ro.upperThreshold))
						else if (App.ro.overrideDelta > 1.0)
						{
							SilverlightPlayer.Play();
							App.ro.bufferingInProgress = false;
						}
					}
				}
			}

			UpdateAnalysis();

			//dirty workaround. sometimes total media time is not updated during mediaopened event. have to pool manually
			pbPlaybackProgress.Maximum = SilverlightPlayer.NaturalDuration.TimeSpan.TotalSeconds;
		}

		private void UpdateAnalysis()
		{
			//cbSubscription.Text = App.ro.subscription.ToString();
			tbRebufferingFrequency.Text = App.ro.rebufferingCounter.ToString();
			//tbAboveAverageFrequency.Text = App.ro.aboveAverageFrequency.ToString();
			//tbGOODFrequency.Text = App.ro.GOODFrequency.ToString();
			//tbFAIRFrequency.Text = App.ro.FAIRFrequency.ToString();
			tbResultRebuffering.Text = App.ro.resultRebuffering.ToString();
			tbFrameDropsAccum.Text = App.ro.droppedFramesCounter.ToString();
			//tbFrameDropsGOOD.Text = App.ro.frameDropsGOOD.ToString();
			//tbFrameDropsPOOR.Text = App.ro.frameDropsPOOR.ToString();
			//tbResult.Text = App.ro.result.ToString();
			tbBandwidth.Text = App.ro.bandwidth.ToString();
		}

		private void Button_PlayPause(object sender, System.Windows.RoutedEventArgs e)
		{
			if ((SilverlightPlayer.CurrentState == MediaElementState.Stopped) || (SilverlightPlayer.CurrentState == MediaElementState.Paused) || (SilverlightPlayer.CurrentState == MediaElementState.Buffering))
			{
				SilverlightPlayer.Play();
				App.ro.bufferingInProgress = false;
			}
			else if (SilverlightPlayer.CurrentState == MediaElementState.Playing)
			{
				SilverlightPlayer.Pause();
				App.ro.bufferingInProgress = true;
			}
		}

		int defaultRebufferingCounter = 0;
		bool? defaultBufferingInProgress = null;
		private void SilverlightPlayer_BufferingProgressChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			if (useOverriding)
			{
				if (SilverlightPlayer.CurrentState == MediaElementState.Buffering)
				{
					if (defaultBufferingInProgress != true)
					{
						defaultRebufferingCounter++;
						tbDefaultRebufferingCounter.Text = defaultRebufferingCounter.ToString();
						defaultBufferingInProgress = true;
					}
				}
				else if (SilverlightPlayer.CurrentState != MediaElementState.Buffering)
				{
					defaultBufferingInProgress = false;
				}
			}
			else
			{
				if (SilverlightPlayer.CurrentState == MediaElementState.Buffering)
				{
						defaultRebufferingCounter++;
						tbDefaultRebufferingCounter.Text = defaultRebufferingCounter.ToString();
				}
			}

			tbBufferingProgress.Text = SilverlightPlayer.BufferingProgress.ToString();
		}

		private void SilverlightPlayer_CurrentStateChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			tbCurrentState.Text = SilverlightPlayer.CurrentState.ToString();
			if (useOverriding)
			{
				if (SilverlightPlayer.CurrentState == MediaElementState.Playing)
				{
					generalUpdateTimer.Start();
					tbTimerEnabled.Text = generalUpdateTimer.IsEnabled.ToString();

					iconPause.Visibility = Visibility.Collapsed;
					iconPlay.Visibility = Visibility.Visible;
					iconStop.Visibility = Visibility.Collapsed;
				}
				else if (SilverlightPlayer.CurrentState == MediaElementState.Stopped)
				{
					generalUpdateTimer.Stop();
					tbTimerEnabled.Text = generalUpdateTimer.IsEnabled.ToString();
					tbMediaTimeline.Text = "0:00 / 0:00";
					playbakProgressTimer.Stop();

					iconPause.Visibility = Visibility.Collapsed;
					iconPlay.Visibility = Visibility.Collapsed;
					iconStop.Visibility = Visibility.Visible;
				}
				else if (SilverlightPlayer.CurrentState == MediaElementState.Buffering)
				{
					SilverlightPlayer.Pause();
					App.ro.bufferingInProgress = true;
					

					iconPause.Visibility = Visibility.Visible;
					iconPlay.Visibility = Visibility.Collapsed;
					iconStop.Visibility = Visibility.Collapsed;
				}
				else if (SilverlightPlayer.CurrentState == MediaElementState.Paused)
				{
					SilverlightPlayer.Pause();
					App.ro.bufferingInProgress = true;

					iconPause.Visibility = Visibility.Visible;
					iconPlay.Visibility = Visibility.Collapsed;
					iconStop.Visibility = Visibility.Collapsed;
				}
			}
			else
			{
				if (SilverlightPlayer.CurrentState == MediaElementState.Buffering)
				{
					App.ro.bufferingInProgress = true;
					App.ro.rebufferingCounter++;
					tbRebufferingCounter.Text = App.ro.rebufferingCounter.ToString();

					iconPause.Visibility = Visibility.Visible;
					iconPlay.Visibility = Visibility.Collapsed;
					iconStop.Visibility = Visibility.Collapsed;
				}
				else if (SilverlightPlayer.CurrentState == MediaElementState.Stopped)
				{
					App.ro.bufferingInProgress = false;

					iconPause.Visibility = Visibility.Collapsed;
					iconPlay.Visibility = Visibility.Collapsed;
					iconStop.Visibility = Visibility.Visible;

				}
				else if (SilverlightPlayer.CurrentState == MediaElementState.Playing)
				{
					App.ro.bufferingInProgress = false;

					iconPause.Visibility = Visibility.Collapsed;
					iconPlay.Visibility = Visibility.Visible;
					iconStop.Visibility = Visibility.Collapsed;
				}
			}
		}

		//debug stuff
		private void cbSource_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			ComboBoxItem cbi = (sender as ComboBox).SelectedItem as ComboBoxItem;
			SilverlightPlayer.Source = new Uri(cbi.Content.ToString());

			tbMediaFailed.Text = "";
			generalUpdateTimer.Stop();
			tbTimerEnabled.Text = generalUpdateTimer.IsEnabled.ToString();
			//SilverlightPlayer.Visibility = Visibility.Collapsed;
			//TODO : Update lower and upper threshold according to video. Maybe do it in own class
			//App.ro.CurrentLower = App.ro.Lower1080;
			//App.ro.CurrentUpper = App.ro.Upper1080;
			//App.ro.CurrentDrop = App.ro.Drop1080;

		}

		private void SilverlightPlayer_DownloadProgressChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			//TODO: maybe use this instead of timer
			tbDownloadProgress.Text = SilverlightPlayer.DownloadProgress.ToString();
			pbBufferProgress.Value = SilverlightPlayer.DownloadProgress;
		}

		private void SilverlightPlayer_MediaFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
		{
			tbMediaFailed.Text = e.ErrorException.Message;
			generalUpdateTimer.Stop();
			tbTimerEnabled.Text = generalUpdateTimer.IsEnabled.ToString();
			MessageBox.Show("Your firewall might block this media. Check your connection settings and restart this test. Error message = " + e.ErrorException.Message, "Unable to play streaming media.", MessageBoxButton.OK);
		}

		private void SilverlightPlayer_MediaOpened(object sender, System.Windows.RoutedEventArgs e)
		{
			// Dirty workaround fixing Mediaelement bug where download progress is 1 (should be less than 1) at start.
			// tears of steel 480 is effected.
			if (SilverlightPlayer.DownloadProgress == 1)
			{
				App.ro.rebufferingCounter = App.ro.rebufferingCounter - 1;
			}

			// Reset values
			App.ro.rebufferingCounter = 0;
			App.ro.droppedFramesCounter = 0.0;
			defaultRebufferingCounter = 0;

			if (useOverriding)
			{
				SilverlightPlayer.Pause();				
			}

			App.ro.bufferingInProgress = true;

			//Get new values
			//sPlaybackProgress.Maximum = SilverlightPlayer.NaturalDuration.TimeSpan.TotalSeconds;
			pbPlaybackProgress.Maximum = SilverlightPlayer.NaturalDuration.TimeSpan.TotalSeconds;
			tbWidth.Text = SilverlightPlayer.NaturalVideoWidth.ToString();
			tbHeight.Text = SilverlightPlayer.NaturalVideoHeight.ToString();
			tbDuration.Text = SilverlightPlayer.NaturalDuration.ToString();
			tbRebufferingCounter.Text = App.ro.rebufferingCounter.ToString();
			tbDroppedFramesCounter.Text = App.ro.droppedFramesCounter.ToString();

			//TODO: Finish this. Play selected media instead of recommended resolution.
			{
				int i = 0;
				while (App.myMediaList[i].Resolution != App.ro.SelectedMedia)
				{
					i++;
				}
				StringBuilder resolution = new StringBuilder();
				resolution.Append(Enum.GetName(typeof(myMediaInfo.Resolutions), App.myMediaList[i].Resolution));
				resolution.Remove(0, 1);
				tbResolution.Text = resolution.ToString();
			}

			generalUpdateTimer.Start();
			tbTimerEnabled.Text = generalUpdateTimer.IsEnabled.ToString();

			//set player width and height
			//SilverlightPlayer.Width = SilverlightPlayer.NaturalVideoWidth;
			//SilverlightPlayer.Height = SilverlightPlayer.NaturalVideoHeight;

			//SilverlightPlayer.Width = gridPlayer.ActualWidth-52;
			//SilverlightPlayer.Height = gridPlayer.ActualHeight-52;

			//bindPlaybackProgress = new Binding("");
			//bindPlaybackProgress.Source = SilverlightPlayer.Position.TotalSeconds;
			//pbPlaybackProgress.SetBinding(ProgressBar.ValueProperty, bindPlaybackProgress);
			playbakProgressTimer.Start();
			//SilverlightPlayer.Visibility = Visibility.Visible;

			//update player window size
			UpdatePlayerSize();
		}

		private void SilverlightPlayer_MediaEnded(object sender, RoutedEventArgs e)
		{
			generalUpdateTimer.Stop();
			tbTimerEnabled.Text = generalUpdateTimer.IsEnabled.ToString();
			playbakProgressTimer.Stop();

			UpdateResults();
			SendResults();

			// Navigate to results page
			NavigateToResultsPage();
		}

		private void NavigateToResultsPage()
		{
			var pop = new PopupToResults();

			pop.Closed += (s, eargs) =>
			{
				if (pop.DialogResult ?? true)
				{
					NavigationService.Navigate(new Uri(@"\Results", UriKind.Relative));
				}
			};

			pop.Show();
		}

		private void SendResults()
		{
			string finalURL = Application.Current.Host.Source.AbsoluteUri;
			string[] temps = finalURL.Split('/');
			string postUrl;

			//TODO: Enum this
			string result = "";
			string resultRebuffering = "";
			string resultFrameDrops = "";
			string resultDesync = "";
			string resultResolution = "";

			if (App.ro.result == 0)
			{
				result = "GOOD";
			}
			else if (App.ro.result == 1)
			{
				result = "POOR";

				//old results
				//result = "FAIR";
			}
			else if (App.ro.result == 2)
			{
				result = "POOR";
			}
			else
			{
				result = "Unknown";
			}

			if (App.ro.resultRebuffering == 0)
			{
				resultRebuffering = "GOOD";
			}
			else if (App.ro.resultRebuffering == 1)
			{
				resultRebuffering = "POOR";

				//old results
				//resultRebuffering = "FAIR";
			}
			else if (App.ro.resultRebuffering == 2)
			{
				resultRebuffering = "POOR";
			}
			else
			{
				resultRebuffering = "Unknown";
			}

			if (App.ro.resultDroppedFrames == 0)
			{
				resultFrameDrops = "GOOD";
			}
			else if (App.ro.resultDroppedFrames == 1)
			{
				resultFrameDrops = "POOR";

				//old results
				//resultFrameDrops = "FAIR";
			}
			else if (App.ro.resultDroppedFrames == 2)
			{
				resultFrameDrops = "POOR";
			}
			else
			{
				resultFrameDrops = "Unknown";
			}

			if (App.ro.resultDesync == 0)
			{
				resultDesync = "GOOD";
			}
			else if (App.ro.resultDesync == 1)
			{
				resultDesync = "POOR";

				//old results
				//resultDesync = "FAIR";
			}
			else if (App.ro.resultDesync == 2)
			{
				resultDesync = "POOR";
			}
			else
			{
				resultDesync = "Unknown";
			}

			StringBuilder mediaResolution = new StringBuilder();
			mediaResolution.Append(App.ro.SelectedMedia.ToString());
			mediaResolution.Remove(0, 1);
			resultResolution = mediaResolution.ToString();
			string sessionId = App.ro.sessionId;

			string parameter = new StringBuilder("result=" + result + "&" +
				"resultRebuffering=" + resultRebuffering + "&" +
				"resultFrameDrops=" + resultFrameDrops + "&" +
				"resultDesync=" + resultDesync + "&" +
				"resultResolution=" + resultResolution + "&" +
				"sessionId=" + sessionId).ToString();
			postUrl = "http://" + App.ServerIp + "/iemt/result.php?" + parameter;
			tbMessages.Text = postUrl;

			try
			{
				var wc = new WebClient();
				wc.Headers["Content-type"] = "application/x-www-form-urlencoded";
				wc.UploadStringAsync(new Uri(postUrl), "POST");
			}
			catch (System.Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}

			//debug
			string postDebugUrl = "http://" + App.MediaServerIp + "/iemt/result.php?" + parameter;
			try
			{
				var wc = new WebClient();
				wc.Headers["Content-type"] = "application/x-www-form-urlencoded";
				wc.UploadStringAsync(new Uri(postDebugUrl), "POST");
			}
			catch (System.Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void UpdateResults()
		{
			int CurrentLower = 0;
			int CurrentUpper = 0;
			int CurrentDrop = 0;
			int CurrentDuration = 0;
			{
				int i = 0;
				//while (App.myMediaList[i].Resolution != App.ro.RecommendedResolution)
				while (App.myMediaList[i].Resolution != App.ro.SelectedMedia)
				{
					i++;
				}
				CurrentLower = App.myMediaList[i].LowerThreshold;
				CurrentUpper = App.myMediaList[i].UpperThreshold;
				CurrentDrop = App.myMediaList[i].DropThreshold;
				//CurrentDuration = App.myMediaList[i].DurationThreshold;
			}
			// Calculate result rebuffering
			if (App.ro.rebufferingCounter < CurrentLower)
			{
				App.ro.resultRebuffering = 0;//GOOD result
			}
			else if (App.ro.rebufferingCounter > CurrentUpper)
			{
				App.ro.resultRebuffering = 2;//POOR result
			}
			else
			{
				App.ro.resultRebuffering = 1;//FAIR result
			}


			//TODO
			// Calculate result drop frames
			if (App.ro.droppedFramesCounter < CurrentDrop)
			{
				App.ro.resultDroppedFrames = 0;//GOOD result
			}
			else if (App.ro.droppedFramesCounter > CurrentDrop)
			{
				App.ro.resultDroppedFrames = 2;//POOR result
			}
			else 
			{
				App.ro.resultDroppedFrames = 1;//FAIR result
			}

			//if (App.ro.duration <= CurrentDuration)
			//{
			//	App.ro.resultDuration = 0;//GOOD result
			//}
			//else if (App.ro.duration > CurrentDrop)
			//{
			//	App.ro.resultDuration = 2;//POOR result
			//}

			////TODO
			//// Calculate result desync
			//App.ro.resultDesync = 0;

			//// Calculate final results from rebuffering, frames, desync
			//if (App.ro.resultDroppedFrames == 0)
			//{
			//    App.ro.result = 0;
			//}
			//else if (App.ro.resultDesync == 0)
			//{
			//    App.ro.result = 0;
			//}
			//else
			//{
			//    if (App.ro.resultRebuffering == 0)
			//    {
			//        App.ro.result = 0;
			//    }
			//    else if (App.ro.resultRebuffering == 1)
			//    {
			//        App.ro.result = 1;
			//    }
			//    else if (App.ro.resultRebuffering == 2)
			//    {
			//        App.ro.result = 2;
			//    }
			//}

			//experiment - fix if rebuffering higher than threshold, results good(should be poor)
			//result, best to worst 
			//if (App.ro.resultDesync == 0 || App.ro.resultDroppedFrames == 0 || App.ro.resultRebuffering == 0)
			//{
			//	App.ro.result = 0;//good
			//}
			//if (App.ro.resultDesync == 1 || App.ro.resultDroppedFrames == 1 || App.ro.resultRebuffering == 1)
			//{
			//	App.ro.result = 1;//fair
			//}
			//if (App.ro.resultDesync == 2 || App.ro.resultDroppedFrames == 2 || App.ro.resultRebuffering == 2)
			//{
			//	App.ro.result = 2;//poor
			//}

			App.ro.result = 0;//initially results are good unless
			//if (App.ro.resultDesync != 0)
			//{
			//	App.ro.result = 2;
			//}
			if (App.ro.resultDroppedFrames != 0)
			{
				App.ro.result = 2;
			}
			if (App.ro.resultRebuffering != 0)
			{
				App.ro.result = 2;
			}

			//update results in debugstack ui
			tbResultRebuffering.Text = App.ro.resultRebuffering.ToString();
			tbResultDroppedFrames.Text = App.ro.resultDroppedFrames.ToString();
			tbResultDesync.Text = App.ro.resultDesync.ToString();
			tbResult.Text = App.ro.result.ToString();
		}

		private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			SilverlightPlayer.Stop();
			App.ro.bufferingInProgress = true;
		}

		//debug stuff
		private void cbSubscription_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			ComboBoxItem cbi = (ComboBoxItem)(sender as ComboBox).SelectedItem;

			App.ro.Subscription = Convert.ToDouble(cbi.Content.ToString());
			tbSelectedSubscription.Text = App.ro.Subscription.ToString();

			//toy story 3 trailers
			if (App.ro.Subscription == 0.3)
			{
				SilverlightPlayer.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(0240p_H.264-AAC_0336kbps).mp4?query=queryvalue");
			}
			else if (App.ro.Subscription == 0.5)
			{
				SilverlightPlayer.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(0360p_H.264-AAC_0574kbps).mp4?query=queryvalue");
			}
			else if (App.ro.Subscription == 1)
			{
				SilverlightPlayer.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(0480p_H.264-AAC_1189kbps).mp4?query=queryvalue");
			}
			else if (App.ro.Subscription == 2)
			{
				SilverlightPlayer.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(0720p_H.264-AAC_2110kbps).mp4?query=queryvalue");
			}
			else if (App.ro.Subscription == 4)
			{
				SilverlightPlayer.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(1080p_H.264-AAC_3597kbps).mp4?query=queryvalue");
			}
			else if (App.ro.Subscription == 5)
			{
				SilverlightPlayer.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(1080p_H.264-AAC_3597kbps).mp4?query=queryvalue");
			}
			else if (App.ro.Subscription == 10)
			{
				SilverlightPlayer.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(1080p_H.264-AAC_3597kbps).mp4?query=queryvalue");
			}
			else if (App.ro.Subscription == 20)
			{
				SilverlightPlayer.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(1080p_H.264-AAC_3597kbps).mp4?query=queryvalue");
			}

			tbMediaFailed.Text = "";
			generalUpdateTimer.Stop();
			tbTimerEnabled.Text = generalUpdateTimer.IsEnabled.ToString();
		}

		// ENTRY POINT
		// Executes when the user navigates to this page. 
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			//debug stuff
			if (App.Current.Resources.Count != 0)
			{
				for (int i = 0; i < App.Current.Resources.Count; i++)
				{
					lbHttpGet.Items.Add(App.Current.Resources.ElementAt(i).ToString());
				}
			}
			tbSessionId.Text = App.ro.sessionId;
			tbGetSubscription.Text = App.ro.Subscription.ToString();
			tbGetRecommended.Text = Enum.GetName(typeof(myMediaInfo.Resolutions), App.ro.RecommendedResolution);
			for (int i = 0; i < App.myMediaList.Count; i++)
			{
				if (App.myMediaList[i].Resolution == App.ro.RecommendedResolution)
				{
					tbGetLowerThreshold.Text = App.myMediaList[i].LowerThreshold.ToString();
					tbGetUpperThreshold.Text = App.myMediaList[i].UpperThreshold.ToString();
					tbGetDroppedFrames.Text = App.myMediaList[i].DropThreshold.ToString();
				}
			}

			Random random = new Random(unchecked((int)(DateTime.Now.Ticks)));
			int randomNumber = random.Next(0, 99999999);
			StringBuilder SourceStringBuilder = new StringBuilder();
			{
				int i = 0;
				while (App.myMediaList[i].Resolution != App.ro.SelectedMedia)
				{
					i++;
				}
				SourceStringBuilder.Append(App.myMediaList[i].Source);
				SourceStringBuilder.Append(randomNumber.ToString());
			}
			SilverlightPlayer.Source = new System.Uri(SourceStringBuilder.ToString());
		}

		private void Slider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (SilverlightPlayer.Volume == 0)
			{
				iconVolumeFull.Visibility = Visibility.Collapsed;
				iconVolumeMin.Visibility = Visibility.Visible;
			}
			else
			{
				iconVolumeFull.Visibility = Visibility.Visible;
				iconVolumeMin.Visibility = Visibility.Collapsed;
			}
		}

		private void btnSendResult_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			SendResults();
		}

		private void btnToResultsPopup_Click(object sender, RoutedEventArgs e)
		{
			NavigateToResultsPage();
		}

		private void UpdatePlayerSize()
		{
			//debug

			playerWidth.Text = SilverlightPlayer.ActualWidth.ToString();
			playerHeight.Text = SilverlightPlayer.ActualHeight.ToString();
			playerWindowWidth.Text = PlayerWindow.ActualWidth.ToString();
			playerWindowHeight.Text = PlayerWindow.ActualHeight.ToString();
			pageWidth.Text = page.ActualWidth.ToString();
			pageHeight.Text = page.ActualHeight.ToString();
			//pageScrollerWidth.Text = PageScroller.ActualWidth.ToString();
			//pageScrollerHeight.Text = PageScroller.ActualHeight.ToString();


			//LayoutRoot.Height = Application.Current.Host.Content.ActualHeight;
			//LayoutRoot.Width = Application.Current.Host.Content.ActualWidth;
			//SilverlightPlayer.Height = Application.Current.Host.Content.ActualHeight;
			//SilverlightPlayer.Width = Application.Current.Host.Content.ActualWidth;
			//PlayerWindow.Height = Application.Current.Host.Content.ActualHeight;
			//PlayerWindow.Width = Application.Current.Host.Content.ActualWidth;
			SilverlightPlayer.MaxHeight = page.ActualHeight - 62;
			SilverlightPlayer.MaxWidth = page.ActualWidth - 4;
			//PlayerWindow.MaxHeight = page.ActualHeight-62;
			PlayerWindow.MaxWidth = page.ActualWidth - 4;
		}

		private void page_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdatePlayerSize();
		}
	}

}
