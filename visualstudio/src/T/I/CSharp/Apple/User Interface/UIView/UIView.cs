using System;
using System.Drawing;

using CoreGraphics;
using Foundation;
using UIKit;

namespace $rootnamespace$
{
	[Register ("$safeitemrootname$")]
	public class $safeitemrootname$ : UIView
	{
		public $safeitemrootname$ ()
		{
			Initialize ();
		}
 
		public $safeitemrootname$ (RectangleF bounds) : base (bounds)
		{
			Initialize ();
		}
 
		void Initialize ()
		{
			BackgroundColor = UIColor.Red;
		}
	}
}