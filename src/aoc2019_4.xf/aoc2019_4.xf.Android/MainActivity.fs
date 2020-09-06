// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace aoc2019_4.xf.Android

open System

open Android
open Android.App
open Android.Content
open Android.Content.PM
open Android.Runtime
open Android.Views
open Android.Widget
open Android.OS
open Xamarin.Forms.Platform.Android
open Xamarin.Forms.Maps
open Xamarin.Essentials

[<Activity(Label = "aoc2019_4.xf.Android",
           Icon = "@drawable/icon",
           Theme = "@style/MainTheme",
           MainLauncher = true,
           ConfigurationChanges =
             (ConfigChanges.ScreenSize
              ||| ConfigChanges.Orientation))>]
type MainActivity() =
  inherit FormsAppCompatActivity()

  let RequestLocationId = 0

  let LocationPermissions: string[] =
    [|
      Android.Manifest.Permission.AccessCoarseLocation
      Android.Manifest.Permission.AccessFineLocation
    |]

  override this.OnCreate(bundle: Bundle) =
    FormsAppCompatActivity.TabLayoutResource <- Resources.Layout.Tabbar
    FormsAppCompatActivity.ToolbarResource <- Resources.Layout.Toolbar
    base.OnCreate(bundle)

    Xamarin.Essentials.Platform.Init(this, bundle)

    Xamarin.Forms.Forms.Init(this, bundle)

    //Xamarin.Forms.Maps.Init (this, bundle)
    Xamarin.FormsMaps.Init(this, bundle)

    let appcore = new aoc2019_4.xf.App()
    this.LoadApplication(appcore)

  override this.OnRequestPermissionsResult(requestCode: int,
                                           permissions: string [],
                                           [<GeneratedEnum>] grantResults: Android.Content.PM.Permission []) =
    Xamarin.Essentials.Platform.OnRequestPermissionsResult
      (requestCode, permissions, grantResults)

    base.OnRequestPermissionsResult(requestCode, permissions, grantResults)

  override this.OnStart() =
    base.OnStart()

    if ((int)Build.VERSION.SdkInt >= 23) then
      let tFineLocation = Permissions.Maps.C
      if  (Manifest.Permission.AccessFineLocation) <> Permission.Granted then
        RequestPermissions(LocationPermissions, RequestLocationId)
      else
        Console.WriteLine "Location permissions already granted."
    else
      failwith ""
