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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetPemStreaming
{
	public partial class MainPage : UserControl
	{
		public MainPage()
		{
			InitializeComponent();
		}

		// After the Frame navigates, ensure the HyperlinkButton representing the current page is selected
		private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
		{
			foreach (UIElement child in LinksStackPanel.Children)
			{
				HyperlinkButton hb = child as HyperlinkButton;
				if (hb != null && hb.NavigateUri != null)
				{
					if (hb.NavigateUri.ToString().Equals(e.Uri.ToString()))
					{
						VisualStateManager.GoToState(hb, "ActiveLink", true);
					}
					else
					{
						VisualStateManager.GoToState(hb, "InactiveLink", true);
					}
				}
			}
		}

		// If an error occurs during navigation, show an error window
		private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			e.Handled = true;
			ChildWindow errorWin = new ErrorWindow(e.Uri);
			errorWin.Show();
		}


		private void InitParameter()
		{
			if (App.Current.Resources.Count != 0)
			{
				myMediaInfo media;
				/* 256 512 1 2 4
				 * The Adventures of Tintin
				 * /Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(240p_H.264-AAC)500kbps.mp4
				 * /Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(360p_H.264-AAC)740kbps.mp4
				 * /Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(480p_H.264-AAC)1800kbps.mp4
				 * /Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(720p_H.264-AAC)2500kbps.mp4
				 * /Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(1080p_H.264-AAC)4500kbps.mp4
				 * 
				 * sample links
				 * http://202.188.95.245/Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(240p_H.264-AAC)500kbps.mp4
				 * http://202.188.95.245/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(0240p_H.264-AAC_0336kbps).mp4
				 */

				//to enable this, ensure to add corresponding res in result page
				//media = new myMediaInfo();
				//media.Resolution = myMediaInfo.Resolutions.r240p;
				////media.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(0240p_H.264-AAC_0336kbps).mp4?query=");
				//media.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(240p_H.264-AAC)500kbps.mp4?query=");
				//media.LowerThreshold = Convert.ToInt32(Application.Current.Resources["Lower240"]);
				//media.UpperThreshold = Convert.ToInt32(Application.Current.Resources["Upper240"]);
				//media.DropThreshold = Convert.ToInt32(Application.Current.Resources["Drop240"]);
				////media.DurationThreshold = Convert.ToDouble(Application.Current.Resources["Duration240"]);
				//media.RecommendedSubscription = 0.3;
				//App.myMediaList.Add(media);

				media = new myMediaInfo();
				media.Resolution = myMediaInfo.Resolutions.r360p;
				//media.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(0360p_H.264-AAC_0574kbps).mp4?query=");
				media.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(360p_H.264-AAC)740kbps.mp4?query=");
				media.LowerThreshold = Convert.ToInt32(Application.Current.Resources["Lower360"]);
				media.UpperThreshold = Convert.ToInt32(Application.Current.Resources["Upper360"]);
				media.DropThreshold = Convert.ToInt32(Application.Current.Resources["Drop360"]);
				//media.DurationThreshold = Convert.ToDouble(Application.Current.Resources["Duration360"]);
				media.RecommendedSubscription = 0.5;
				App.myMediaList.Add(media);

				media = new myMediaInfo();
				media.Resolution = myMediaInfo.Resolutions.r480p;
				//media.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(0480p_H.264-AAC_1189kbps).mp4?query=");
				//media.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(480p_H.264-AAC)1800kbps.mp4?query=");
				media.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(480p_H.264-AAC)1800kbpsmod16.mp4?query=");
				media.LowerThreshold = Convert.ToInt32(Application.Current.Resources["Lower480"]);
				media.UpperThreshold = Convert.ToInt32(Application.Current.Resources["Upper480"]);
				media.DropThreshold = Convert.ToInt32(Application.Current.Resources["Drop480"]);
				//media.DurationThreshold = Convert.ToDouble(Application.Current.Resources["Duration480"]);
				media.RecommendedSubscription = 1.0;
				App.myMediaList.Add(media);

				media = new myMediaInfo();
				media.Resolution = myMediaInfo.Resolutions.r720p;
				//media.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(0720p_H.264-AAC_2110kbps).mp4?query=");
				media.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(720p_H.264-AAC)2500kbps.mp4?query=");
				media.LowerThreshold = Convert.ToInt32(Application.Current.Resources["Lower720"]);
				media.UpperThreshold = Convert.ToInt32(Application.Current.Resources["Upper720"]);
				media.DropThreshold = Convert.ToInt32(Application.Current.Resources["Drop720"]);
				//media.DurationThreshold = Convert.ToDouble(Application.Current.Resources["Duration720"]);
				media.RecommendedSubscription = 2.0;
				App.myMediaList.Add(media);

				media = new myMediaInfo();
				media.Resolution = myMediaInfo.Resolutions.r1080p;
				//media.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/Toy story 3 trailers/Toy_Story_3_ Trailer(1080p_H.264-AAC_3597kbps).mp4?query=");
				media.Source = new System.Uri("http://" + App.MediaServerIp + "/Media/TheAdventuresofTintin/TheAdventuresofTintin-TVSpot-3(1080p_H.264-AAC)4500kbps.mp4?query=");
				media.LowerThreshold = Convert.ToInt32(Application.Current.Resources["Lower1080"]);
				media.UpperThreshold = Convert.ToInt32(Application.Current.Resources["Upper1080"]);
				media.DropThreshold = Convert.ToInt32(Application.Current.Resources["Drop1080"]);
				//media.DurationThreshold = Convert.ToDouble(Application.Current.Resources["Duration1080"]);
				media.RecommendedSubscription = 4.0;
				App.myMediaList.Add(media);

				App.ro.Subscription = Convert.ToDouble(Application.Current.Resources["Subscription"]);
				string RecommendedResolution;
				{
					StringBuilder resolution = new StringBuilder();
					resolution.Append("r");
					resolution.Append(Application.Current.Resources["Recommended"] as String);
					RecommendedResolution = resolution.ToString();
				}
				App.ro.RecommendedResolution = (myMediaInfo.Resolutions)Enum.Parse(typeof(myMediaInfo.Resolutions), RecommendedResolution, false);
				App.ro.SelectedMedia = (myMediaInfo.Resolutions)Enum.Parse(typeof(myMediaInfo.Resolutions), RecommendedResolution, false);
				try
				{
					App.ro.sessionId = Application.Current.Resources["SessionId"] as String;
				}
				catch (System.Exception ex)
				{
					App.ro.sessionId = "NoSessionId";
				}
				if (App.ro.sessionId == null)
				{
					App.ro.sessionId = "NullSessionId";
				}
			}
		}

	}
}