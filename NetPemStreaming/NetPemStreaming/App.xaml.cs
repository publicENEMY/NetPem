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

namespace NetPemStreaming
{
	public partial class App : Application
	{
		public App()
		{
			this.Startup += this.Application_Startup;
			this.UnhandledException += this.Application_UnhandledException;

			InitializeComponent();
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			this.RootVisual = new MainPage();
		}

		private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			// If the app is running outside of the debugger then report the exception using
			// a ChildWindow control.
			if (!System.Diagnostics.Debugger.IsAttached)
			{
				// NOTE: This will allow the application to continue running after an exception has been thrown
				// but not handled. 
				// For production applications this error handling should be replaced with something that will 
				// report the error to the website and stop the application.
				e.Handled = true;
				ChildWindow errorWin = new ErrorWindow(e.ExceptionObject);
				errorWin.Show();
			}
		}

		//public static String ServerIp = "58.26.233.174";
		//public static String MediaServerIp = "58.26.233.174";
		public static String ServerIp = "202.188.95.247"; //production server
		public static String MediaServerIp = "202.188.95.245"; //production server
		//public static String ReportingIp = "58.26.233.174";

		public static List<myMediaInfo> myMediaList = new List<myMediaInfo>();
		public static RebufferingOverrider ro = new RebufferingOverrider();

		public class RebufferingOverrider
		{
			public double lowerThreshold = 1; // pause playback if less than. in seconds.
			public double upperThreshold = 5; // continue playback and if more than. in seconds.        
			public double overridePosition = 0;
			public double overrideDownloadProgress = 0;
			public double overrideDelta = 0;
			public bool bufferingInProgress = false;
			public int rebufferingCounter = 0;
			public double droppedFramesCounter = 0;

			public double Subscription = 0;
			public myMediaInfo.Resolutions RecommendedResolution = myMediaInfo.Resolutions.r240p;
			public myMediaInfo.Resolutions SelectedMedia = myMediaInfo.Resolutions.r240p;

			//public int Lower240 = 0;
			//public int Upper240 = 0;
			//public int Drop240 = 0;
			//public int Lower360 = 0;
			//public int Upper360 = 0;
			//public int Drop360 = 0;
			//public int Lower480 = 0;
			//public int Upper480 = 0;
			//public int Drop480 = 0;
			//public int Lower720 = 0;
			//public int Upper720 = 0;
			//public int Drop720 = 0;
			//public int Lower1080 = 0;
			//public int Upper1080 = 0;
			//public int Drop1080 = 0;

			//public int CurrentLower = 0;
			//public int CurrentUpper = 0;
			//public int CurrentDrop = 0;

			//public int aboveAverageFrequency = 0;
			//public int goodFrequency = 0;
			//public int fairFrequency = 0;
			//public int frameDropsGood = 0;
			//public int frameDropsBad = 0;
			public int desyncCounter = 0;
			public int result = -1;
			public int resultRebuffering = -1;
			public int resultDroppedFrames = -1;
			public int resultDesync = -1;
			public int bandwidth = 0;
			public string sessionId = null;
		}

		public class myMediaInfo
		{
			public enum Resolutions { r240p, r360p, r480p, r720p, r1080p };

			public Uri Source;
			public Resolutions Resolution;
			public int LowerThreshold;
			public int UpperThreshold;
			public int DropThreshold;
			public double DurationThreshold;
			public double RecommendedSubscription;

			public myMediaInfo()
			{

			}
		}

	}
}