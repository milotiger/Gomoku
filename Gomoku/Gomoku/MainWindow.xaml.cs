using System;
using System.Configuration;
using System.Threading;
using System.Web.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;

namespace Gomoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public enum PlayMode    
    {
        Online,
        Offline,
        Machine,
        MachineVsOnline
    }
    public partial class MainWindow : Window
    {
        Button[,] CaroButt; //Buttons Array
        Socket GomokuSocket; //Socket to Gomoku server

        bool YourTurn = false;
        Board PlayBoard = new Board(); 

        private bool isWaiting = false;
        private bool isWin = false;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            Board.Notify += BoardOnNotify;
            Board.WinNotify += BoardOnWinNotify;
            Board.CurrentMode = PlayMode.Machine;
            AI.AIPlaceNotify += AIPlaceNotify;

            ModePicking();
        }

        #region Loaded

        private void ModePicking()
        {
            ModePicker modePicker = new ModePicker() { WindowStartupLocation = WindowStartupLocation.CenterScreen };
            if (modePicker.ShowDialog() == true)
            {
                Board.CurrentMode = modePicker.Mode;
                MyName = modePicker.MyName;
                NameTb.Text = MyName;
                this.Title = "Gomoku" + " - " + Board.CurrentMode.ToString();
            }
            //GameInit();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GameInit();
        }

        private void GameInit()
        {
            PlayPanel.Children.Clear();
            CreatePlayGround();
            ChatTb.Focus();
            MyName = NameTb.Text;
            if (Board.CurrentMode == PlayMode.Online || Board.CurrentMode == PlayMode.MachineVsOnline)
            {
                GomokuSocket = IO.Socket(WebConfigurationManager.AppSettings.Get("ServerAddress"));
                InitConnect(); //Initialize Connect
                ChatConnect(); //Create Thread for chatting
                GamePlayConnect(); //Create Threads for playing
            }
        }
        private void CreatePlayGround()
        {
            int ButtonSize = (int)Math.Min(PlayBound.ActualWidth, PlayBound.ActualHeight) / 12;
            CaroButt = new Button[12, 12];
            for (int i = 0; i < 12; i++)
            {
                WrapPanel Wp = new WrapPanel();
                PlayPanel.Children.Add(Wp);
                for (int j = 0; j < 12; j++)
                {
                    Button Bt = new Button();
                    Bt.Tag = new BoxInformation { Row = i, Col = j, Checked = false };
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

        #region Event Delegate
        private void AIPlaceNotify(int Row, int Col)
        {
            PlaceToUI(Row, Col);
            if (Board.CurrentMode == PlayMode.MachineVsOnline)
                GomokuSocket.Emit("MyStepIs", JObject.FromObject(new { row = Row, col = Col }));
        }
        private void PlaceToUI(int row, int col)
        {
            isWaiting = false;
            Button Bt = CaroButt[row, col];
            this.Dispatcher.Invoke(() =>
            {
                if (Board.CurrentMode == PlayMode.MachineVsOnline)
                {
                    Bt.Background = Brushes.Black;
                    return;
                }

                Brush BackupBackGround = Bt.Background;

                if (Board.PlayingPlayer == State.Player1)
                    Bt.Background = Brushes.Black;
                if (Board.PlayingPlayer == State.Player2)
                    Bt.Background = Brushes.White;
                Mouse.OverrideCursor = null;
            });
        }
        private void BoardOnWinNotify(State player)
        {
            MessageBox.Show("Player " + player.ToString() + " win!","We have a Winner", MessageBoxButton.OK, MessageBoxImage.Information);
            ResetButton();
            
            isWaiting = false;
            isWin = true;
            this.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = null;
            });
            
            Board.PlayingPlayer = State.Player1;

            Board.ResetBoard();
        }
        private void BoardOnNotify(string errorString)
        {
            MessageBox.Show(errorString);
        }
        #endregion
        
        #region Socket
        bool Connected = false;
        private void InitConnect()
        {
            GomokuSocket.On(Socket.EVENT_CONNECT, () =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    PushMessage("Connected", "Server");
                });
            });
        }
        
        private void ChatConnect() //Receive Message from server
        {
            GomokuSocket.On("ChatMessage", (data) =>
            {
                this.Dispatcher.Invoke(() =>
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
                });

                if (((JObject)data)["message"].ToString() == "Welcome!")
                {
                    GomokuSocket.Emit("MyNameIs", MyName);
                    Connected = true;
                    this.Dispatcher.Invoke(() =>
                    {
                        NameChangeBt.IsEnabled = true;
                    });
                }

                if (((JObject)data)["message"].ToString().EndsWith("first player!") == true)
                {
                    YourTurn = true;
                    if (Board.CurrentMode == PlayMode.MachineVsOnline)
                        PlaceRandom();
                }

            });
        }

        private void PlaceRandom()
        {
            int Col = new Random().Next(0,11);
            int Row = new Random().Next(0, 11);

            AIPlaceNotify(Row, Col);
            Board.PlayingPlayer = 3 - Board.PlayingPlayer;
        }

        private void GamePlayConnect()
        {
            PlaceFromOnline();
            EndGameSignalFromOnline();
        }


        private void PlaceFromOnline()
        {

            GomokuSocket.On("NextStepIs", (data) =>
            {
                int Row = (int)((JObject)data)["row"];
                int Col = (int)((JObject)data)["col"];

                if ((int)((JObject)data)["player"] == 0) //1 is Me, 0 is Other;
                    return;

                this.Dispatcher.Invoke(() =>
                {
                    Check(false, CaroButt[Row, Col]);
                });

                PlayBoard.Place(Row, Col);

                if (Board.CurrentMode == PlayMode.MachineVsOnline)
                {
                    Thread AIThread = new Thread(PlayBoard.AIPlace); //Create a new thread to run AI algorithm
                    AIThread.Start();
                }
            });
        }

        private void EndGameSignalFromOnline()
        {
            GomokuSocket.On("EndGame", (data) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(((JObject)data)["message"].ToString(), "The game is now stopped!");
                    NewGame();
                });
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
            ChatRow.Height = this.Height - 180;
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
            if (Board.CurrentMode == PlayMode.Machine || Board.CurrentMode == PlayMode.Offline)
            {
                PushMessage(ChatTb.Text, MyName);
            }
            else
            {
                GomokuSocket.Emit("ChatMessage", ChatTb.Text);
            }
                
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
            {
                if (Board.CurrentMode == PlayMode.Machine || Board.CurrentMode == PlayMode.Offline)
                {
                    PushMessage(ChatTb.Text, MyName);
                }
                else
                {
                    GomokuSocket.Emit("ChatMessage", ChatTb.Text);
                }
            }
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
            int Row = Infor.Row;
            int Col = Infor.Col;

            if (isWaiting)
                return;

            if (Board.CurrentMode == PlayMode.Offline || Board.CurrentMode == PlayMode.Machine)
            {
                Brush BackupBackGround = Bt.Background;
                bool isValid = true;

                if (Board.PlayingPlayer == State.Player1)
                    Bt.Background = Brushes.Black;
                if (Board.PlayingPlayer == State.Player2)
                    Bt.Background = Brushes.White;

                this.Dispatcher.Invoke(() =>
                {
                    isValid = PlayBoard.Place(Row, Col);
                    if (!isValid)
                    { 
                        Bt.Background = BackupBackGround;
                        return;
                    }

                    if (Board.CurrentMode == PlayMode.Machine)
                    {
                        Thread AIThread = new Thread(PlayBoard.AIPlace); //Create a new thread to run AI algorithm
                        AIThread.Start();
                    }
                });

                if (isWin)
                {
                    isWin = false;
                    return;
                }

                if (Board.CurrentMode == PlayMode.Machine && isValid)
                {
                    isWaiting = true;
                    Mouse.OverrideCursor = Cursors.Wait;

                }

            }
            if (Board.CurrentMode == PlayMode.Online)
            {  
                if (YourTurn == true)
                {
                    Check(true, Bt);
                }
                else
                {
                    PushMessage("That's not your turn", "Server");
                }
            }
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
        
        #region Reset
        private void NewGame()
        {
            NameChangeBt.Content = "New Game!";
            YourTurn = false;
            isConnectedToPeople = false;

            ResetButton();
        }

        private void ResetButton()
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (Button Bt in CaroButt)
                {
                    BoxInformation Infor = (BoxInformation)Bt.Tag;
                    Infor.Checked = false;
                    if ((Infor.Row + Infor.Col) % 2 == 0)
                        Bt.Background = Brushes.Gray;
                    else
                        Bt.Background = Brushes.LightGray;
                }
                
            });
            
        }
        #endregion

        private void ChangeMode_Click(object sender, RoutedEventArgs e)
        {
            if (Board.CurrentMode == PlayMode.Online || Board.CurrentMode == PlayMode.MachineVsOnline)
                GomokuSocket.Disconnect();

            ModePicking();
            
            NameChangeBt.IsEnabled = false;

            GameInit();
            //ResetButton();
            NewGame();
            Board.ResetBoard();
            Board.PlayingPlayer = State.Player1;
        }
    }
}

