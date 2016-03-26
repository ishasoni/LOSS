using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akavache;
using System.Reactive.Linq;
using PCLCrypto;
using System.Text;


namespace LOSSPortable
{

    public class ChatPage : ContentPage
    {

        private String Key;
        private Button send;
        List<string> Messages = new List<string>();
        private Grid gridLayout;
        private StackLayout outerStack;
        private ScrollView innerScroll;
        private int MessageCount;
        private Entry editor = new Entry();

        String name;
        //        List<Message> msgs = new List<Message>(); //history of messaging
        Conversation conv = new Conversation();
        

        public ChatPage(String inputname, List<Message> msgs, string key)  //use the key to store
        {   
            

            if (Helpers.Settings.ContrastSetting == true)
            {
                BackgroundColor = Colors.contrastBg;
            }
            else
            {
                BackgroundColor = Colors.background;

            }

            this.Key = key;
            conv.msgs = msgs;
            name = inputname;

            Title = "Chat";
            Icon = "Accounts.png";

            //entry:
            
            editor.Placeholder = "Enter Message: ";

            //double height = innerScroll.HeightRequest;

            editor.Focused += delegate {

                System.Diagnostics.Debug.WriteLine("messages count = " + MessageCount);
                innerScroll.HeightRequest = innerScroll.HeightRequest / 3;
                 
                ScrollEvent();
                this.Content = outerStack;
            };
            
            editor.Unfocused += delegate { //hide the keyboard

                innerScroll.HeightRequest = innerScroll.HeightRequest * 3;
                //ScrollEvent();
                this.Content = outerStack;
            };
            

            //var label = new Label { Text = "Message " + this.getName(), FontSize = 30, BackgroundColor = Color.Blue, TextColor = Color.White, XAlign = TextAlignment.Center };
            send = new Button { Text = "Send" };

            //upon sending a message
            send.Clicked += delegate {

                String mes = editor.Text;
                editor.Text = "";


                try
                {
                    Messages.Add(mes);
                }
                catch (NullReferenceException ex)
                {
                    DisplayAlert("Alert", "Processor Usage" + ex.Message + mes, "OK");
                }


                Message message = new Message();
                //constructor:

                message.icon = "drawable/prof.png";
                message.sender = "Sender ";
                message.reciever = "Reciever";
                message.text = mes;
                message.time = " " + currentTime();
                message.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff");
                message.id = CalculateSha1Hash(message.date);
                // "Sender: ", mes, "drawable/prof.png", " " + currentTime() );
                //msgs.Add(message);

                System.Diagnostics.Debug.WriteLine("message id: " + message.id);
                System.Diagnostics.Debug.WriteLine("message: " + message.text);
                DisplayResponse(message);
                conv.msgs.Add(message);
                MessageCount++;


                this.Content = outerStack;
            };

            Device.BeginInvokeOnMainThread(() =>   //automatically updates
            {

                gridLayout = new Grid
                {
                    RowSpacing = 3,
                    ColumnSpacing = 0,
                    Padding = new Thickness(0, 0, 0, 0),
                    VerticalOptions = LayoutOptions.Start,
                    //BackgroundColor = Color.FromHex("CCCCFF"),
                    RowDefinitions = { new RowDefinition { Height = GridLength.Auto } },

                    Children =
                        {
                            //label
                        }

                };

                innerScroll = new ScrollView
                {
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Start,
                    Padding = new Thickness(5, 5, 5, 10),
                    //BackgroundColor = Color.FromHex("CCCCFF"),
                    Content = gridLayout

                };

                gridLayout.ChildAdded += delegate // Automatically scrolls to bottom of the chat 
                {
                     ScrollEvent();
                };

                outerStack = new StackLayout
                {
                    //BackgroundColor = Color.FromHex("CCCCFF"),
                    VerticalOptions = LayoutOptions.Start,
                    Spacing = 0,
                    Children =
                            {
                                innerScroll,
                                editor,
                                send
                            }

                };

                innerScroll.HeightRequest = 440;

                Content = outerStack;
                Title = "" + this.getName();
                
            });

            refreshView();
        }

        //individual message tapped in chat:
        async void OnLabelClicked(Label label, Message msg, int Type)
        {
            var action = await DisplayActionSheet(null, null, null, "Hide Text", "Report", "Delete Message");
            switch (action)
            {
                case "Report":
                    //await DisplayAlert("Alert", "Reporting not implemented yet.", "OK");
                    reportMessage(msg);
                    break;
                case "Hide Text":
                    label.Text = "\n" + "***" + "\t";
                    break;
                case "Delete Message":
                    deleteMessage(msg.id);
                    break;

            }
            this.Content = outerStack;

        }


        private void DisplayResponse(Message message)
        {
            String msg = message.text;
            int numRows;
            if (msg == null || msg == "")
            {
                return;
            }
            else
            {
                numRows = msg.Length / 35;
            }

            Grid innerGrid = new Grid
            {   //CCCCFF
                VerticalOptions = LayoutOptions.Start,

                Padding = 3,
                RowSpacing = 3,
                RowDefinitions = {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(numRows+1, GridUnitType.Auto) }
                },
                ColumnDefinitions = {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            }
            };

            if (message.id == "456")
            { innerGrid.BackgroundColor = Color.FromHex("f2f2f2"); }
            else
            { innerGrid.BackgroundColor = Color.FromHex("d9d9d9"); }





            var profilePicture = new Image { };
            profilePicture.Source = message.icon;
            profilePicture.VerticalOptions = LayoutOptions.StartAndExpand;
            //profilePicture.BackgroundColor = Color.FromHex("CCCCFF");


            Label name = new Label { Text = message.sender, TextColor = Color.Black, FontAttributes = FontAttributes.Bold, FontSize = 20, FontFamily = "Arial" }; //, XAlign = TextAlignment.Start
            name.VerticalOptions = LayoutOptions.StartAndExpand;

            var datetime = DateTime.Now;
            Label time = new Label { Text = message.time, TextColor = Color.Black, FontSize = 20, FontFamily = "Arial" };



            Label response = new Label { Text = message.text, TextColor = Color.Black, FontSize = 18, FontFamily = "Arial" }; //, XAlign = TextAlignment.Start             
            var tgr = new TapGestureRecognizer();


            tgr.Tapped += (s, e) => OnLabelClicked(response, message, 1);
            response.GestureRecognizers.Add(tgr);
            response.VerticalOptions = LayoutOptions.Start;
            int labelLength = 2 + numRows;


            innerGrid.RowSpacing = 1;


            gridLayout.RowSpacing = 1;

            innerGrid.Children.Add(profilePicture);
            innerGrid.Children.AddHorizontal(name);
            innerGrid.Children.AddHorizontal(time);
            innerGrid.Children.Add(response, 0, 5, 1, labelLength);

            gridLayout.Children.AddVertical(innerGrid);
            gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            //add buttons to grid, push that grid to the stack
        }

        public String getName()
        {
            return name;
        }

        public void setChat(List<Message> msgs)
        {
            conv.msgs = msgs;
            conv.name = this.name;
            refreshView();
        }
        public List<Message> getChat()
        {
            return conv.msgs;
        }

        public String currentTime()
        {
            var datetime = DateTime.Now;
            String minutes = "" + datetime.Minute;
            if (minutes.Length == 1)
            { minutes = "0" + minutes; }

            return "" + datetime.Hour + ":" + minutes;

        }

        public void deleteMessage(String id)
        {
            Message found = conv.msgs.Find(x => x.id == id);
            conv.msgs.Remove(found);
            MessageCount--;
            

            refreshView();
        }

        public void refreshView()
        {
            gridLayout.Children.Clear();

            foreach (Message msg in conv.msgs) //INITIALIZE HISTORY OF MESSAGES
            {
                DisplayResponse(msg);
            }

            this.Content = outerStack;
        }

        public void reportMessage(Message msg)
        {

            Navigation.PushAsync(new ReportMessage(msg));
        }
        private static string CalculateSha1Hash(string input)  //hashing
        {
            // step 1, calculate MD5 hash from input
            var hasher = WinRTCrypto.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Sha1);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = hasher.HashData(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public void ScrollEvent()
        {
            innerScroll.ScrollToAsync(0, innerScroll.HeightRequest * (MessageCount + 10), false);
            System.Diagnostics.Debug.WriteLine("message count= " + MessageCount);
             
        }



        //----------------------------------------------------------
        


        protected override void OnDisappearing() //leaving the page ->cache history
        {

            editor.Keyboard = null;
            editor.Unfocus();
            
            System.Diagnostics.Debug.WriteLine("unfocused.");
            

            Store(conv);
            System.Diagnostics.Debug.WriteLine("storing");

        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("trying to get cache.");
            Conversation con = await Get();
            this.MessageCount = con.msgs.Count;
            System.Diagnostics.Debug.WriteLine("returned messages: " + con.msgs.Count);
            //await DisplayAlert("", con.msgs[0].getMessage(), "ok"); CAUSE OF BUGS
            this.setChat(con.msgs);
            this.Content = outerStack;
            ScrollEvent();
        }

        public async Task<Conversation> Get()
        { 
            try
            {
                System.Diagnostics.Debug.WriteLine("fetching cached. object key = " + Key);
                return await BlobCache.LocalMachine.GetOrCreateObject<Conversation>(Key, NewConv);

            }
            catch (KeyNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("error");
                return new Conversation();

            }


        }

        public async Task Store<Conversation>(Conversation value)
        {

            try
            {
                await BlobCache.LocalMachine.InsertObject(Key, conv);
                System.Diagnostics.Debug.WriteLine("Finished storing");
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine("Error with storing in cache.");
            }
        }

        public Conversation NewConv()
        {
            System.Diagnostics.Debug.WriteLine("creating new conv");
            return new Conversation();
        }

        

    }


}