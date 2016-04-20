﻿using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace LOSSPortable
{
    // This page displays all the video playlist(s) to the user.
    public class VideoResources : ContentPage
    {
        // Holds all info for each playlist.
        public RangeObservableCollection<OnlinePlaylistModel> playlist { get; set; }

        // Gets info on each playlist and determines how it is displayed.
        public VideoResources()
        {

            //sets the background color based on settings
            if (Helpers.Settings.ContrastSetting == true)
            {
                BackgroundColor = Colors.contrastBg;
            }
            else
            {
                BackgroundColor = Colors.background;
            }


            // Holds data to be displayed on this content page.
            playlist = AmazonUtils.getOnlinePlaylist;
            // View type for this content page.
            ListView lstView = new ListView();
            // Set size (height) of each element displayed on this page.
            lstView.RowHeight = 100;
            lstView.BackgroundColor = BackgroundColor;
            // Set the title of this page.
            this.Title = "Playlist(s)";
            // Set the source of data for page's list view.
            lstView.ItemsSource = playlist;
            // Set layout for each element in this list view.
            lstView.ItemTemplate = new DataTemplate(typeof(VideoCell));
            // Set behavior of element when selected by user.
            lstView.ItemSelected += Onselected;
            // Assign the list view created above to this content page.
            Content = lstView;

            

            // Accomodate iPhone status bar.
            this.Padding = new Thickness(10, Device.OnPlatform(20, 0, 0), 10, 5);
        }// End VideoResources() constructor.

        // Determines what happens when an element from the list is chosen by the user.
        void Onselected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }
            // This deselects the item after it is selected.
            ((ListView)sender).SelectedItem = null;
            // When given playlist is selected user is brought to the page listed below.
            var item = e.SelectedItem as OnlinePlaylistModel;
            Navigation.PushAsync(new PlaylistPage(item.ID));
        }// End of Onselected() method.
    }// End of VideoResources class.
}// End of namespace LOSS.