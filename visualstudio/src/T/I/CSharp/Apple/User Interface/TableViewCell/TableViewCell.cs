using System;

using Foundation;
using UIKit;

namespace $rootnamespace$
{
	public partial class $safeitemrootname$ : UITableViewCell
	{
		public static readonly NSString Key = new NSString("$safeitemrootname$");
		public static readonly UINib Nib;

	static $safeitemrootname$()
		{
			Nib = UINib.FromName("$safeitemrootname$", NSBundle.MainBundle);
		}

	protected $safeitemrootname$(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}
	}
}
