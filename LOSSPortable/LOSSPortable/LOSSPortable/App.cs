﻿using System;
using Xamarin.Forms;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace LOSSPortable
{
    public class App : Application
    {
        public static int ScreenWidth;
        public static int ScreenHeight;

		public Boolean HardwareAccelerated { get; set; }
        public Boolean ChatPageActive { get; set; }
        public Boolean ChatSelectionPageActive { get; set; }

        public App()
        {
            MainPage = new LOSSPortable.RootPage();
			HardwareAccelerated	= true;
            ChatPageActive = false;
        }

        protected override void OnStart()
        {
            Helpers.Settings.IsVolunteer = false;
            AmazonUtils.updateInspirationalQuoteList();
            AmazonUtils.updateOnlineRList();
            AmazonUtils.updateOnlineVList();
            AmazonUtils.updateOnlinePlaylist();
            MessagingCenter.Subscribe<ChatPage>(this, "Start", (sender) =>
            {
                ChatPageActive = true;
            });
            MessagingCenter.Subscribe<ChatPage>(this, "End", (sender) =>
            {
                ChatPageActive = false;
            });
            MessagingCenter.Subscribe<ChatSelection>(this, "Start", (sender) =>
            {
                ChatSelectionPageActive = true;
            });
            MessagingCenter.Subscribe<ChatSelection>(this, "End", (sender) =>
            {
                ChatSelectionPageActive = false;
            });


            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes

        }

        public Boolean chatDisplayed()
        {
            return ChatPageActive;
        }


        /*
            {
            "Seen": "False",
            "Text": "Hello",
            "Time": "2016-04-06 16:38:08.618757",
            "ToFrom": "U-79854921478565921900#R-6031175061964857259"
            }
         */
        public void parseMessageObject(string msg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                System.Diagnostics.Debug.WriteLine(msg);
                SNSMessagePCL tmp = JsonConvert.DeserializeObject<SNSMessagePCL>(msg);

                ChatMessage message = new ChatMessage { ToFrom = tmp.ToFrom, Time = tmp.Time, Text = tmp.Text, Sender = tmp.Sender };
                if (tmp.Subject == "Handshake")
                {
                    MessagingCenter.Send<App, ChatMessage>(this, "Handshake", message);
                }
                else if (tmp.Subject == "HandshakeEnd")
                {
                    MessagingCenter.Send<App, ChatMessage>(this, "HandshakeEnd", message);
                }
                else
                {   
                    MessagingCenter.Send<App, ChatMessage>(this, "Hi", message);
                }
            });
            
        }
    }
}

