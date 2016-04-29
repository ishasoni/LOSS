using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akavache;
using System.Reactive.Linq;
using PCLCrypto;
using System.Text;

using Acr.UserDialogs;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System.Threading;
using Newtonsoft.Json;
using Amazon.Lambda.Model;
using Amazon.Util;
using Amazon.Lambda;
using System.IO;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace LOSSPortable
{

    public class ChatPage : ContentPage
    {
        private String talkingToNickname = "User";
        private String talkingToID;
        private String Key;
        private Button send;
        List<string> Messages = new List<string>();
        private Grid gridLayout;
        private StackLayout outerStack;
        private ScrollView innerScroll;
        private int MessageCount;
        private Entry editor = new Entry();
        CancellationTokenSource ct = new CancellationTokenSource();

        private static List<ChatMessage> msgs;

        private double longitude;
        private double latitude;


        String name;
        //        List<Message> msgs = new List<Message>(); //history of messaging
        Conversation conv = new Conversation();
        

        public ChatPage(String inputname, string key, string Name)  //use the key to store
        {   
            

            if (Name== "" || Name=="Enter your name: " || Name == null )
            {
                this.name = "Anonymous";
            }
            else
            {
                if (Name.Length > 10)
                { Name = Name.Substring(0, 10) + "..."; }

                this.name = Name;
            }

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
			send = new Button { Text = "Send", TextColor = Color.White, BackgroundColor = Color.Gray, HorizontalOptions = LayoutOptions.FillAndExpand};

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


                ChatMessage message = new ChatMessage();
                //constructor:
                message.ToFrom = "" + AmazonUtils.Credentials.GetIdentityId() + "#" + AmazonUtils.Credentials.GetIdentityId();
                message.Icon = "drawable/prof.png";
                message.Sender = name;
                message.Reciever = "Reciever";
                message.Text = mes;
                message.Time = DateTime.Now.ToString("HH:mm:ss:ffff");
                message.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff");
                message.Id = AmazonUtils.Credentials.GetIdentityId();
                // "Sender: ", mes, "drawable/prof.png", " " + currentTime() );
                //msgs.Add(message);

                System.Diagnostics.Debug.WriteLine("message info: " + message.ToFrom + message.Time + message.Text);
                DisplayResponse(message);
                conv.msgs.Add(message);
                MessageCount++;
                sendMessage(message);
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

            //var heig = r.ParentView.Height;
                //Device.Styles.
                Rectangle bounds = outerStack.Bounds;
                int innerSize = Convert.ToInt32(bounds.Height - (bounds.Height / 8));
                innerScroll.HeightRequest =  App.ScreenHeight - 200; //440  -- change of 200;
                System.Diagnostics.Debug.WriteLine("window size set to: " + innerSize);
                Content = outerStack;
                Title = "" + inputname;
                
            });

            refreshView();
        }

        //individual message tapped in chat:
        async void OnLabelClicked(Label label, ChatMessage msg, int Type)
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
                    deleteMessage(msg.Id);
                    break;

            }
            this.Content = outerStack;

        }


        private void DisplayResponse(ChatMessage message) //adds message to log
        {
            String msg = message.Text;
            int numRows;
            if (msg == null || msg == "")
            {
                return;
            }
            else
            {
                numRows = msg.Length / 25;
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

            if (message.Id == "456")
            { innerGrid.BackgroundColor = Color.FromHex("f2f2f2"); }
            else
            { innerGrid.BackgroundColor = Color.FromHex("d9d9d9"); }





            var profilePicture = new Image { };
            profilePicture.Source = message.Icon;
            profilePicture.VerticalOptions = LayoutOptions.StartAndExpand;
            //profilePicture.BackgroundColor = Color.FromHex("CCCCFF");


            Label name = new Label { Text = message.Sender, TextColor = Color.Black, FontAttributes = FontAttributes.Bold, FontSize = 20, FontFamily = "Arial" }; //, XAlign = TextAlignment.Start
            name.VerticalOptions = LayoutOptions.StartAndExpand;

            var datetime = DateTime.Now;
            Label time = new Label { Text = message.Time.Substring(0,5), TextColor = Color.Black, FontSize = 20, FontFamily = "Arial" };
            


            Label response = new Label { Text = message.Text, TextColor = Color.Black, FontSize = 18, FontFamily = "Arial" }; //, XAlign = TextAlignment.Start             
            var tgr = new TapGestureRecognizer();


            tgr.Tapped += (s, e) => OnLabelClicked(response, message, 1);
            response.GestureRecognizers.Add(tgr);
            response.VerticalOptions = LayoutOptions.Start;
            int labelLength = 2 + numRows;


            innerGrid.RowSpacing = 1;


            gridLayout.RowSpacing = 1;

            innerGrid.Children.Add(profilePicture);
            //innerGrid.Children.AddHorizontal(name);
            //innerGrid.Children.AddHorizontal(time);
            innerGrid.Children.Add(name,1,4,0,1);
            innerGrid.Children.Add(time,4,0);

            innerGrid.Children.Add(response, 0, 5, 1, labelLength);

            gridLayout.Children.AddVertical(innerGrid);
            gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            //add buttons to grid, push that grid to the stack
        }

        public String getName()
        {
            return name;
        }

        public void setChat(List<ChatMessage> msgs)
        {
            conv.msgs = msgs;
            conv.name = this.name;
            refreshView();
        }
        public List<ChatMessage> getChat()
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
            ChatMessage found = conv.msgs.Find(x => x.Id == id);
            conv.msgs.Remove(found);
            MessageCount--;
            

            refreshView();
        }

        public void refreshView()
        {
       //     gridLayout.Children.Clear();

            foreach (ChatMessage msg in conv.msgs) //INITIALIZE HISTORY OF MESSAGES
            {
                DisplayResponse(msg);
            }

            this.Content = outerStack;
        }

        public void reportMessage(ChatMessage msg)
        {

            Navigation.PushAsync(new ReportMessage(msg));
        }

        

        public void ScrollEvent()
        {
            innerScroll.ScrollToAsync(0, innerScroll.HeightRequest * (MessageCount + 10), true);
            System.Diagnostics.Debug.WriteLine("message count= " + MessageCount);
             
        }

        public async void sendMessage(ChatMessage message)
        {
            System.Diagnostics.Debug.WriteLine("trying to send to server: ");   
            //message sending to server:
            try{
                await PushMessage(message.ToFrom, message.Text);
                //await SaveAsync<ChatMessage>(message, ct.Token);
                //await messageServer(message.Text, message.ToFrom);
                //System.Diagnostics.Debug.WriteLine("message sent to the server.");
                }
            catch(Exception E)
            {
                System.Diagnostics.Debug.WriteLine("error storing in server "+ E);
            };
            this.Content = outerStack;
        }
        //-------------------------Get from Server-------------------------
        public void messagesFromServer(MessageLst[] MessageList) //upon recieving a chat History log
        {
            List<ChatMessage> msgs = new List<ChatMessage>();
            foreach (MessageLst i in MessageList){
                msgs.Add(singleMessageFromServer(i.ToFrom, i.Time, i.Text));
            }
            setChat(msgs); //sets the chat to show the message history.
        }

        public ChatMessage singleMessageFromServer(String ToFrom, String Time, String Message)    //upon recieving a single message.
        {
            ChatMessage newMsg = new ChatMessage()
            {
                Text = Message,
                Sender = "placeholder",
                Icon = "drawable/prof.png",
                Time = "",
                ToFrom = ToFrom
            };
            newMsg.Time = Time.Substring(5, 2)+":"+Time.Substring(8,2); //change to only include HH:mm
            return newMsg;
        }

        //-------------------------Server----------------------------------
        async private Task PushMessage(string toFrom, string text) //Function to create a Json object and send to server using a lambda function
        {
            try
            {
                MessageItem message = new MessageItem { Item = new ChatMessage { ToFrom = Helpers.Settings.ToFromArn, Text = text } };
                MessageJson messageJson = new MessageJson { operation = "create", tableName = "Message", payload = message };
                string args = JsonConvert.SerializeObject(messageJson);
                //System.Diagnostics.Debug.WriteLine(args);
                var ir = new InvokeRequest()
                {
                    FunctionName = "arn:aws:lambda:us-east-1:987221224788:function:Test_Backend",
                    PayloadStream = AWSSDKUtils.GenerateMemoryStreamFromString(args),
                    InvocationType = InvocationType.RequestResponse
                };
                //System.Diagnostics.Debug.WriteLine("Before invoke: " + ir.ToString());


                InvokeResponse resp = await AmazonUtils.LambdaClient.InvokeAsync(ir);
                resp.Payload.Position = 0;
                var sr = new StreamReader(resp.Payload);
                var myStr = sr.ReadToEnd();

                //                System.Diagnostics.Debug.WriteLine("Status code: " + resp.StatusCode);
                //                System.Diagnostics.Debug.WriteLine("Response content: " + myStr);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + e);
            }
        }

        async private Task LoadMessages() //Function to create a Json object and send to server using a lambda function
        {
            try
            {
                UserInfoItem message = new UserInfoItem { Item = new MessageLst { ToFrom = Helpers.Settings.ToFromArn, Time = DateTime.UtcNow.ToString()} }; //Helpers.Settings.EndpointArnSetting
                UserInfoJson messageJson = new UserInfoJson { operation = "read", tableName = "Message", payload = message };
                string args = JsonConvert.SerializeObject(messageJson);
                var ir = new InvokeRequest()
                {
                    FunctionName = "arn:aws:lambda:us-east-1:987221224788:function:Test_Backend",
                    PayloadStream = AWSSDKUtils.GenerateMemoryStreamFromString(args),
                    InvocationType = InvocationType.RequestResponse
                };


                InvokeResponse resp = await AmazonUtils.LambdaClient.InvokeAsync(ir);
                resp.Payload.Position = 0;
                var sr = new StreamReader(resp.Payload);
                var myStr = sr.ReadToEnd();
                try
                {
                    System.Diagnostics.Debug.WriteLine("newConversations : " + myStr);
                    var response = JsonConvert.DeserializeObject<ConversationResponse>(myStr);
                    if (response.Success == "true")
                    {
                        var messages = response.MessageList;
                        messagesFromServer(messages);
                    }
                }
                catch (Exception e)
                {

                }

                //                System.Diagnostics.Debug.WriteLine("Status code: " + resp.StatusCode);
                //                System.Diagnostics.Debug.WriteLine("Response content: " + myStr);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error:" + e);
            }

        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            MessagingCenter.Subscribe<App, ChatMessage>(this, "ConversationEnd", (sender, arg) => //adds message to log
            {
                MessagingCenter.Send<ChatPage>(this, "End");
                MessagingCenter.Unsubscribe<App>(this, "ConversationEnd");
                terminateChat();
            });

            MessagingCenter.Send<ChatPage>(this, "Start");
            MessagingCenter.Subscribe<App, ChatMessage>(this, "Hi", (sender, arg) => //adds message to log
            {
                try
                {
                    arg.Time = arg.Time.Substring(11, 2) + ":" + arg.Time.Substring(14, 2);
                }
                catch (Exception e)
                {
                    arg.Time = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();

                }
                arg.Icon = "drawable/prof.png";
                DisplayResponse(arg);
                conv.msgs.Add(arg);
                ScrollEvent();
            });
            await LoadMessages();
            Conversation con = await Get(this.Key);
            this.MessageCount = con.msgs.Count;
            
            this.Content = outerStack;
            ScrollEvent();

        }
        
        protected override void OnDisappearing() //leaving the page ->cache history
        {
            
            editor.Keyboard = null;
            editor.Unfocus();
            
            MessagingCenter.Send<ChatPage>(this, "End");
            Store(conv);
            System.Diagnostics.Debug.WriteLine("storing");
            base.OnDisappearing();
        }

        private async void terminateChat()
        {
            await DisplayAlert("Conversation ended", talkingToNickname + " has left the conversation", "OK");
            await Navigation.PopAsync();
        }

        public async Task eraseCache()
        {
                await BlobCache.LocalMachine.InvalidateAll();
        }

        public async Task<Conversation> Get(string key)
        {
            if (Helpers.Settings.MessageCacheKey != key)
            {
                await eraseCache();
                Helpers.Settings.MessageCacheKey = key;
                return new Conversation();
            }
            else
            {
                try
                {
                    return await BlobCache.LocalMachine.GetOrCreateObject<Conversation>(Key, NewConv);

                }
                catch (KeyNotFoundException e)
                {
                    System.Diagnostics.Debug.WriteLine("error:" + e);
                    return new Conversation();
                }
            }
        }

        public async Task Store<Conversation>(Conversation value)
        {

            try
            {
                await BlobCache.LocalMachine.InsertObject(Key, conv);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error : " + e);
            }
        }

        public Conversation NewConv()
        {
            return new Conversation();
        }
    }
}