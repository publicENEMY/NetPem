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
using System.Text;

namespace StreamingExperience.Views
{
	public partial class Results : Page
	{
		public Results()
		{
			InitializeComponent();

#if DEBUG
			debugStack.Visibility = Visibility.Visible;
			benchmarkTableContent.Visibility = Visibility.Visible;
#else
			debugStack.Visibility = Visibility.Collapsed;
			benchmarkTableContent.Visibility = Visibility.Collapsed;
#endif

		}

		// TODO: Enum those results
		// Executes when the user navigates to this page.
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			int CurrentLower = 0;
			int CurrentUpper = 0;
			int CurrentDrop = 0;
			int CurrentDuration = 0;

			//shouldnt be any error here unless there is no parameter passed. try loading from streamingexperience.html
			//if (App.myMediaList.Count != 0)
			{
				int i = 0;

				string debug;
				debug = App.myMediaList[i].Resolution.ToString();
				debug = App.ro.SelectedMedia.ToString();

				while (App.myMediaList[i].Resolution != App.ro.SelectedMedia)
				{
					i++;
				}
				CurrentLower = App.myMediaList[i].LowerThreshold;
				CurrentUpper = App.myMediaList[i].UpperThreshold;
				CurrentDrop = App.myMediaList[i].DropThreshold;
				//CurrentDuration = App.myMediaList[i].DurationThreshold;
			}

			rebuffFrequency.Text = App.ro.rebufferingCounter.ToString();
			rebuffPar.Text = CurrentLower.ToString();
			if (App.ro.rebufferingCounter >= CurrentLower)
			{
				rebuffConclusion.Text = "Exceed threshold.";
			}
			else
			{
				rebuffConclusion.Text = "Below threshold.";
			}

			droppedFramesFrequency.Text = App.ro.droppedFramesCounter.ToString();
			dropFramesPar.Text = CurrentDrop.ToString();
			if (App.ro.droppedFramesCounter >= CurrentDrop)
			{
				droppedFramesConclusion.Text = "Exceed threshold.";
			}
			else
			{
				droppedFramesConclusion.Text = "Below threshold.";
			}

			//if (App.ro.duration > CurrentDuration)
			//{
			//	durationConclusion.Text = "Exceed threshold.";
			//}
			//else
			//{
			//	durationConclusion.Text = "Below threshold.";
			//}
			
			//debug
			tbRebufferingCount.Text = App.ro.rebufferingCounter.ToString();
			tbDroppedFrameCount.Text = App.ro.droppedFramesCounter.ToString();
			tbDesyncCount.Text = App.ro.desyncCounter.ToString();

			//hide suggestion and solution
			resultSolution.Visibility = Visibility.Collapsed;
			solutionRebuffering.Visibility = Visibility.Collapsed;
			solutionDroppedFrames.Visibility = Visibility.Collapsed;

			//show only appropriate suggestion and solution
			if (App.ro.resultRebuffering == 0)
			{
				tbResultRebuffering.Text = "GOOD";
			}
			else if (App.ro.resultRebuffering == 1)
			{
				tbResultRebuffering.Text = "POOR";
				resultSolution.Visibility = Visibility.Visible;
				solutionRebuffering.Visibility = Visibility.Visible;

				//old results
				//tbResultRebuffering.Text = "FAIR";
				//resultSolution.Visibility = Visibility.Visible;
				//solutionRebuffering.Visibility = Visibility.Visible;
			}
			else if (App.ro.resultRebuffering == 2)
			{
				tbResultRebuffering.Text = "POOR";
				resultSolution.Visibility = Visibility.Visible;
				solutionRebuffering.Visibility = Visibility.Visible;
			}
			else
			{
				tbResultRebuffering.Text = "UNKNOWN";
			}

			if (App.ro.resultDroppedFrames == 0)
			{
				tbResultDroppedFrames.Text = "GOOD";
			}
			else if (App.ro.resultDroppedFrames == 1)
			{
				tbResultDroppedFrames.Text = "POOR";
				resultSolution.Visibility = Visibility.Visible;
				solutionDroppedFrames.Visibility = Visibility.Visible;

				//old results
				//tbResultDroppedFrames.Text = "FAIR";
				//resultSolution.Visibility = Visibility.Visible;
				//solutionDroppedFrames.Visibility = Visibility.Visible;
			}
			else if (App.ro.resultDroppedFrames == 2)
			{
				tbResultDroppedFrames.Text = "POOR";
				resultSolution.Visibility = Visibility.Visible;
				solutionDroppedFrames.Visibility = Visibility.Visible;
			}
			else
			{
				tbResultDroppedFrames.Text = "UNKNOWN";
			}

			//TODO desync
			if (App.ro.resultDesync == 0)
			{
				tbResultDesync.Text = "GOOD";
			}
			else if (App.ro.resultDesync == 1)
			{
				tbResultDesync.Text = "FAIR";
			}
			else if (App.ro.resultDesync == 2)
			{
				tbResultDesync.Text = "POOR";
			}
			else
			{
				tbResultDesync.Text = "UNKNOWN";
			}

			////result, best to worst 
			//if (App.ro.resultDesync == 0 || App.ro.resultDroppedFrames == 0 || App.ro.resultRebuffering == 0)
			//{
			//    App.ro.result = 0;
			//}
			//if (App.ro.resultDesync == 1 || App.ro.resultDroppedFrames == 1 || App.ro.resultRebuffering == 1)
			//{
			//    App.ro.result = 1;
			//}
			//if (App.ro.resultDesync == 2 || App.ro.resultDroppedFrames == 2 || App.ro.resultRebuffering == 2)
			//{
			//    App.ro.result = 2;
			//}

			//Presents the results
			if (App.ro.result == 0)
			{
				tbResult.Text = "GOOD";
			}
			else if (App.ro.result == 1)
			{
				tbResult.Text = "POOR";

				//old result
				//tbResult.Text = "FAIR";
			}
			else if (App.ro.result == 2)
			{
				tbResult.Text = "POOR";
			}
			else
			{
				tbResult.Text = "UNKNOWN";
			}

			if (tbResult.Text == "GOOD")
			{
				GoodSummary.Visibility = Visibility.Visible;
				PoorSummary.Visibility = Visibility.Collapsed;
			}
			else if (tbResult.Text == "POOR")
			{
				GoodSummary.Visibility = Visibility.Collapsed;
				PoorSummary.Visibility = Visibility.Visible;				
			}

			//display resolution information
			{
				StringBuilder sb = new StringBuilder();
				string s;

				sb.Append(App.ro.SelectedMedia.ToString());
				sb.Remove(0, 1);
				s = sb.ToString();
				LastTestResolution.Text = "Tested resolution: " + s;

				sb = new StringBuilder();
				sb.Append(App.ro.RecommendedResolution.ToString());
				sb.Remove(0, 1);
				s = sb.ToString();
				RecommendedResolution.Text = "Recommended resolution: " + s;

			}

			//display benchmark table
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("Benchmark table for ");
				sb.Append(App.ro.Subscription);
				sb.Append("Mbps subscription.");
				benchmarkTableTitle.Text = sb.ToString();
			}
			rebuffering360p.Text = App.myMediaList[0].LowerThreshold.ToString();
			rebuffering480p.Text = App.myMediaList[1].LowerThreshold.ToString();
			rebuffering720p.Text = App.myMediaList[2].LowerThreshold.ToString();
			rebuffering1080p.Text = App.myMediaList[3].LowerThreshold.ToString();

			dropped360p.Text = App.myMediaList[0].DropThreshold.ToString();
			dropped480p.Text = App.myMediaList[1].DropThreshold.ToString();
			dropped720p.Text = App.myMediaList[2].DropThreshold.ToString();
			dropped1080p.Text = App.myMediaList[3].DropThreshold.ToString();

			//TODO duration baseline in benchmark table
		}

		private void restartMeasurement_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if (resolutionSelection.SelectedIndex > 0)
			{
				App.ro.SelectedMedia = (myMediaInfo.Resolutions)resolutionSelection.SelectedIndex;
				NavigationService.Navigate(new Uri(@"\Streaming", UriKind.Relative));
			}
		}
	}
}
