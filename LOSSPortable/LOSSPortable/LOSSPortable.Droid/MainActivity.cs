﻿using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Acr.UserDialogs;
using Plugin.TextToSpeech;
using LOSSPortable;

namespace LOSSPortable.Droid
{
    [Activity(
        Label = "Loss App", 
        Icon = "@drawable/icon", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        Theme = "@style/DefaultTheme"
        )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            App.PManager = new ParseManager(Parser.Default);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            UserDialogs.Init(this);
            CrossTextToSpeech.Current.Init();

            LoadApplication(new App());
        }
    }
}

