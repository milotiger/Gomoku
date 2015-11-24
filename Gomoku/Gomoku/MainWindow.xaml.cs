using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;

namespace Gomoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Button[,] CaroButt;
        Socket GomokuSocket;
        bool YourTurn = false;
        public MainWindow()
        {
            InitializeComponent();
            GomokuSocket = IO.Socket("ws://gomoku-lajosveres.rhcloud.com:8000");
        }


        #region Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreatePlayGround();
            ChatTb.Focus();
            MyName = NameTb.Text;
            SocketConnect();
        }
        
        private void CreatePlayGround()
        {
            int ButtonSize = (int)Math.Min(PlayBound.ActualWidth, PlayBound.ActualHeight)/12;
            CaroButt = new Button[12, 12];
            for (int i = 0; i < 12; i++)
            {
                WrapPanel Wp = new WrapPanel();
                PlayPanel.Children.Add(Wp);
                for (int j = 0; j < 12; j++)
                {
                    Button Bt = new Button();
                    Bt.Tag = new BoxInformation{ Row = i, Col = j, Checked = false };
                    if ((i + j) % 2 == 0)
                        Bt.Background = Brushes.Gray;
                    else
                        Bt.Background = Brushes.LightGray;
                    Bt.Width = ButtonSize;
                    Bt.Height = ButtonSize;
                    Bt.Click += Check_Click;
                  
                    Wp.Children.Add(Bt);
                    CaroButt[i, j] = Bt;
                }
            }
        }
        #endregion

        #region Socket
        bool Connected = false;
        private void SocketConnect()
        {
            GomokuSocket.On(Socket.EVENT_CONNECT, () =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    PushMessage("Connected", "Server");
                }));
               
            });
            GomokuSocket.On(Socket.EVENT_MESSAGE, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    
                    PushMessage(((JObject)data)["message"].ToString(), ((JObject)data)["from"].ToString());
                }));
                
            });
            GomokuSocket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {

                    PushMessage(((JObject)data)["message"].ToString(), "Server");
                }));
            });
            GomokuSocket.On("ChatMessage", (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    string From = "";
                    try
                    {
                        From = ((JObject)data)["from"].ToString(); //Check if the message had the attribute "from"
                    }
                    catch
                    {
                        From = "Server"; //If not, It was from "Server" :D
                    }
                    PushMessage(((JObject)data)["message"].ToString(), From);
                }));

                if (((JObject)data)["message"].ToString() == "Welcome!")
                {
                    GomokuSocket.Emit("MyNameIs", MyName);
                    Connected = true;
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        NameChangeBt.IsEnabled = true;
                    }));
                }

                if (((JObject)data)["message"].ToString().EndsWith("first player!") == true)
                {
                    YourTurn = true;
                }

            });
            GomokuSocket.On(Socket.EVENT_ERROR, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    PushMessage(((JObject)data)["message"].ToString(), "Server");
                }));
            });

            GomokuSocket.On("NextStepIs", (data) =>
            {
                int Row = (int)((JObject)data)["row"];
                int Col = (int)((JObject)data)["col"];
                this.Dispatcher.Invoke((Action)(() =>
                {
                    Check(false, CaroButt[Row, Col]);
                }));
            });

            GomokuSocket.On("EndGame", (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    MessageBox.Show(((JObject)data)["message"].ToString(),"We have a winner!");
                    NewGame();
                }));
            });
           
        }
        #endregion

        #region Resizing
        double ButtonSize;
        private void ResizeButton()
        {
            ButtonSize = Math.Min(PlayBound.ActualWidth, this.ActualHeight - 57) / 12;
            

            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    CaroButt[i, j].Width = ButtonSize;
                    CaroButt[i, j].Height = ButtonSize;
                }
            }
        }
   
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChatRow.Height = this.Height - 150;
        }

        private void PlayGround_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.IsLoaded == true)
                ResizeButton();
        }
        #endregion

        #region Chating
        private void CreateMessage(string Content, string Name)
        {
            Line SeperateLine = new Line()
            {
                X1 = 0,
                Y1 = 0,
                X2 = 200,
                Y2 = 0,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 4, 4 },
                Stroke = Brushes.Blue,
                Width = 200,
            };

            Label NameLb = new Label();
            NameLb.Content = Name;
            NameLb.FontWeight = FontWeights.SemiBold;
            DockPanel.SetDock(NameLb, Dock.Left);

            Label TimeLb = new Label();
            TimeLb.FontSize = 10;
            TimeLb.Content = DateTime.Now.ToLongTimeString();
            DockPanel.SetDock(TimeLb, Dock.Right);

            DockPanel Dp = new DockPanel();
            Dp.LastChildFill = false;
            Dp.Children.Add(NameLb);
            Dp.Children.Add(TimeLb);

            TextBox ContentTb = new TextBox();
            ContentTb.Text = Content;
            ContentTb.TextWrapping = TextWrapping.Wrap;
            ContentTb.BorderBrush = Brushes.Transparent;


            StackPanel Sp = new StackPanel();
            Sp.Children.Add(SeperateLine);
            Sp.Children.Add(Dp);
            Sp.Children.Add(ContentTb);
            Sp.Margin = new Thickness(10,10,0,10);    
            DockPanel.SetDock(Sp, Dock.Top);

            ChatPanel.Children.Add(Sp);
            ChatRow.ScrollToEnd();
        }
        
        private void PushMessage(string Content, string FromName)
        {
            Content = Content.Replace("<br />", "\r\n"); //convert from HTML to Textbox's text
            CreateMessage(Content, FromName);
            ChatTb.Text = "";
            ChatTb.Focus();
        }
        private void PushMessage_Click(object sender, RoutedEventArgs e)
        {
            //PushMessage(ChatTb.Text, YourName);
            GomokuSocket.Emit("ChatMessage", ChatTb.Text);
        }

        string MyName;
        bool isConnectedToPeople = false;
        private void NameChanged_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnectedToPeople && Connected)
            {
                GomokuSocket.Emit("ConnectToOtherPlayer");
                ((Button)sender).Content = "Change!";
                isConnectedToPeople = true;
            }
            else
            {
                ChatTb.Focus();
            }
            MyName = NameTb.Text;
            GomokuSocket.Emit("MyNameIs", MyName);
        }

        
        private void GetKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ChatTb.Focus();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                GomokuSocket.Emit("ChatMessage", ChatTb.Text);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ChatTb.Text != "" && ChatTb.Text != "Type and press Enter")
                return;
            ChatTb.Text = "Type and press Enter";
            ChatTb.Foreground = Brushes.Gray;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ChatTb.Text == "Type and press Enter")
            {
                ChatTb.Text = "";
                ChatTb.Foreground = Brushes.Black;
            }
              
              
        }

        #endregion

        #region Checking
        private void Check_Click(object sender, RoutedEventArgs e)
        {
            Button Bt = (Button)sender;
            BoxInformation Infor = (BoxInformation)Bt.Tag;
            int i = Infor.Row;
            int j = Infor.Col;
            
            if (YourTurn == true)
            {
                Check(true, Bt);
            }
            else
            {
                PushMessage("That's not your turn", "Server");
            }

            MessageBox.Show("Row = " + i.ToString() + "\r\nCol = " + j.ToString() + "\r\nChecked = " + Infor.Checked, "Position");
        }

        private void Check(bool isMe, Button BeCheckedButt)
        {
            BoxInformation Infor = (BoxInformation)BeCheckedButt.Tag;

            if (Infor.Checked == true)
            {
                return;
            }

            if (isMe == true)
            {
                BeCheckedButt.Background = Brushes.Black;
                GomokuSocket.Emit("MyStepIs", JObject.FromObject(new { row = Infor.Row, col = Infor.Col }));
            }
            else
            {
                BeCheckedButt.Background = Brushes.White;
            }

            Infor.Checked = true;
            YourTurn = !YourTurn;
        }
        #endregion

        #region TestThread
        private void chatThread()
        {
           
        }


        #endregion

        #region Reset
        private void NewGame()
        {
            NameChangeBt.Content = "New Game!";
            YourTurn = false;
            isConnectedToPeople = false;

            foreach (Button Bt in CaroButt)
            {
                BoxInformation Infor = (BoxInformation)Bt.Tag;
                Infor.Checked = false;
                if ((Infor.Row + Infor.Col) % 2 == 0)
                    Bt.Background = Brushes.Gray;
                else
                    Bt.Background = Brushes.LightGray;
            }
        }
        #endregion
    }
}

