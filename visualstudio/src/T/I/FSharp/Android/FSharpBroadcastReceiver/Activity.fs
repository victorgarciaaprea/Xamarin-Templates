namespace $rootnamespace$

open System
open System.Collections.Generic
open System.Linq
open System.Text

open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Views
open Android.Widget

[<BroadcastReceiver>]
type $safeitemrootname$ () =
  inherit BroadcastReceiver()

  override this.OnReceive (context, intent) =
    let m = Toast.MakeText (context, "Received intent!", ToastLength.Short)
	m.Show()


