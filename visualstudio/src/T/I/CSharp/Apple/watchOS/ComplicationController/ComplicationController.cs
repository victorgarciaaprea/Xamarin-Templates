using System;

using Foundation;
using ClockKit;

namespace $rootnamespace$
{
	[Register ("$safeitemrootname$")]
	public class $safeitemrootname$ : CLKComplicationDataSource
	{
		public $safeitemrootname$ ()
		{
		}

		public override void GetCurrentTimelineEntry (CLKComplication complication, Action<CLKComplicationTimelineEntry> handler)
		{
			// Call the handler with the current timeline entry
		}

		public override void GetPlaceholderTemplate (CLKComplication complication, Action<CLKComplicationTemplate> handler)
		{
			// This method will be called once per supported complication, and the results will be cached
		}

		public override void GetSupportedTimeTravelDirections (CLKComplication complication, Action<CLKComplicationTimeTravelDirections> Handler)
		{
			// Retrieves the time travel directions supported by your complication
		}
	}
}