﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.Timers
{
	/// <summary>
	/// Drop in Silverlight compatible replacement for the System.Timers.Timer
	/// Doesn't do synchronisation
	/// 
	public class Timer
	{
		private readonly uint _interval;
		private System.Threading.Timer _internalTimer;
		private const uint MaxTime = (uint)0xFFFFFFFD;

		public event EventHandler<EventArgs> Elapsed;

		public Timer(uint interval)
		{
			_interval = interval;
		}

		public bool AutoReset { get; set; }

		public void Start()
		{
			Stop();
			_internalTimer = CreateTimer();
		}

		public void Stop()
		{
			if (_internalTimer != null)
			{

				_internalTimer.Change(MaxTime, MaxTime);
				_internalTimer.Dispose();
				_internalTimer = null;
			}
		}

		private Threading.Timer CreateTimer()
		{
			var timer = new System.Threading.Timer(InternalTick);
			timer.Change(_interval, AutoReset ? _interval : MaxTime);
			return timer;
		}

		private void InternalTick(object state)
		{
			var handler = Elapsed;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}
	}
}

namespace System.Diagnostics
{
	/// <summary>
	/// Stopwatch is used to measure the general performance of Silverlight functionality. Silverlight
	/// does not currently provide a high resolution timer as is available in many operating systems,
	/// so the resolution of this timer is limited to milliseconds. This class is best used to measure
	/// the relative performance of functions over many iterations.
	/// 
	public sealed class Stopwatch
	{
		private long _startTick;
		private long _elapsed;
		private bool _isRunning;

		/// <summary>
		/// Creates a new instance of the class and starts the watch immediately.
		/// 
		/// <returns>An instance of Stopwatch, running.
		public static Stopwatch StartNew()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			return sw;
		}

		/// <summary>
		/// Creates an instance of the Stopwatch class.
		/// 
		public Stopwatch() { }

		/// <summary>
		/// Completely resets and deactivates the timer.
		/// 
		public void Reset()
		{
			_elapsed = 0;
			_isRunning = false;
			_startTick = 0;
		}

		/// <summary>
		/// Begins the timer.
		/// 
		public void Start()
		{
			if (!_isRunning)
			{
				_startTick = GetCurrentTicks();
				_isRunning = true;
			}
		}

		/// <summary>
		/// Stops the current timer.
		/// 
		public void Stop()
		{
			if (_isRunning)
			{
				_elapsed += GetCurrentTicks() - _startTick;
				_isRunning = false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the instance is currently recording.
		/// 
		public bool IsRunning
		{
			get { return _isRunning; }
		}

		/// <summary>
		/// Gets the Elapsed time as a Timespan.
		/// 
		public TimeSpan Elapsed
		{
			get { return TimeSpan.FromMilliseconds(ElapsedMilliseconds); }
		}

		/// <summary>
		/// Gets the Elapsed time as the total number of milliseconds.
		/// 
		public long ElapsedMilliseconds
		{
			get { return GetCurrentElapsedTicks() / TimeSpan.TicksPerMillisecond; }
		}

		/// <summary>
		/// Gets the Elapsed time as the total number of ticks (which is faked
		/// as Silverlight doesn't have a way to get at the actual "Ticks")
		/// 
		public long ElapsedTicks
		{
			get { return GetCurrentElapsedTicks(); }
		}

		private long GetCurrentElapsedTicks()
		{
			return (long)(this._elapsed + (IsRunning ? (GetCurrentTicks() - _startTick) : 0));
		}

		private long GetCurrentTicks()
		{
			// TickCount: Gets the number of milliseconds elapsed since the system started.
			return Environment.TickCount * TimeSpan.TicksPerMillisecond;
		}
	}
}