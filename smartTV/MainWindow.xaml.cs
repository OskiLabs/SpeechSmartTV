using System;
using System.Globalization;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System.Windows;
using TTSModule;
using ASRModule;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Data.SqlClient;

namespace smartTV
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker speechWorker = new BackgroundWorker();
        static TextSpeach synth = new TextSpeach();
        static SpeechRecognitionModule sre = new SpeechRecognitionModule();
        SpeechSynthesizer ss = new SpeechSynthesizer();
        //static Televisor smartTV = new Televisor();
        static bool done = false;

        int Volume = 50;
        int ChannelNumber = 15;
        int LastChannel = 0;
        bool is_On = false;
        bool is_Program_On = false;
        bool mute = false;
        System.Windows.Threading.Dispatcher dispatcher;

        int ChanelListChosenField = 1;

        DispatcherTimer mute_Timer = new DispatcherTimer();
        DispatcherTimer channel_Number_Timer = new DispatcherTimer();
        DispatcherTimer sound_Control_Timer = new DispatcherTimer();
        DispatcherTimer channel_Info_Timer = new DispatcherTimer();
        DispatcherTimer channel_List_Timer = new DispatcherTimer();

        Rectangle[] ChanelListRectangles = new Rectangle[10];
        Label[] ChannelListLabels = new Label[10];
        Label[] ChannelListValuesLabels = new Label[10];

        TextBlock[] ProgramChannelNowTextBlocks = new TextBlock[6];
        TextBlock[] ProgramChannelLaterTextBlocks = new TextBlock[6];
        TextBlock[] ProgramChannelSecondTextBlocks = new TextBlock[6];
        TextBlock[] ProgramChannelThirdTextBlocks = new TextBlock[6];

        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

        public struct Channel
        {
            public string Name;
            public int ID;
            public string BoardAdress;

            public Channel(string par1, int par2, string par3)
            {
                Name = par1;
                ID = par2;
                BoardAdress = par3;
            }
        }

        Channel[] channels = new Channel[16];

        public MainWindow()
        {
            InitializeComponent();

            mute_Timer.Interval = TimeSpan.FromSeconds(2);
            mute_Timer.Tick += Mute_Tick;

            channel_Number_Timer.Interval = TimeSpan.FromSeconds(2);
            channel_Number_Timer.Tick += Channel_Number_Tick;

            sound_Control_Timer.Interval = TimeSpan.FromSeconds(2);
            sound_Control_Timer.Tick += Sound_Control_Tick;

            channel_Info_Timer.Interval = TimeSpan.FromSeconds(4);
            channel_Info_Timer.Tick += Channel_Info_Tick;

            channel_List_Timer.Interval = TimeSpan.FromSeconds(4);
            channel_List_Timer.Tick += Channel_List_Tick;

            Sound_ProgressBar.Value = Volume;
            Sound_Label.Content = Volume.ToString();
            dispatcher = Application.Current.MainWindow.Dispatcher;

            channels[0] = new Channel("TVP1", 3, "TVP1_Board.png");
            channels[1] = new Channel("TVP2", 5, "TVP2_Board.png");
            channels[2] = new Channel("TVP3", 57, "TVP3_Board.png");
            channels[3] = new Channel("TV4", 11, "TV4_Board.png");
            channels[4] = new Channel("Polsat", 9, "Polsat_Board.png");
            channels[5] = new Channel("TVN", 14, "TVN_Board.png");
            channels[6] = new Channel("TV Puls", 12, "TV_Puls_Board.png");
            channels[7] = new Channel("Comedy Central", 124, "CC_Board.png");
            channels[8] = new Channel("AXN", 68, "AXN_Board.png");
            channels[9] = new Channel("Stopklatka TV", 48, "Stopklatka_Board.png");
            channels[10] = new Channel("Kino Polska", 29, "Kino_Polska_Board.png");
            channels[11] = new Channel("WP TV", 158, "WP_TV_Board.png");
            channels[12] = new Channel("MTV", 94, "MTV_Board.png");
            channels[13] = new Channel("Nat Geo", 190, "Nat_Geo_Board.png");
            channels[14] = new Channel("Nickelodeon", 86, "Nick_Board.png");

            ChanelListRectangles[0] = Channel_1_Rectangle;
            ChanelListRectangles[1] = Channel_2_Rectangle;
            ChanelListRectangles[2] = Channel_3_Rectangle;
            ChanelListRectangles[3] = Channel_4_Rectangle;
            ChanelListRectangles[4] = Channel_5_Rectangle;
            ChanelListRectangles[5] = Channel_6_Rectangle;
            ChanelListRectangles[6] = Channel_7_Rectangle;
            ChanelListRectangles[7] = Channel_8_Rectangle;
            ChanelListRectangles[8] = Channel_9_Rectangle;

            ChannelListLabels[0] = Channel_List_1_Label;
            ChannelListLabels[1] = Channel_List_2_Label;
            ChannelListLabels[2] = Channel_List_3_Label;
            ChannelListLabels[3] = Channel_List_4_Label;
            ChannelListLabels[4] = Channel_List_5_Label;
            ChannelListLabels[5] = Channel_List_6_Label;
            ChannelListLabels[6] = Channel_List_7_Label;
            ChannelListLabels[7] = Channel_List_8_Label;
            ChannelListLabels[8] = Channel_List_9_Label;

            ChannelListValuesLabels[0] = Channel_List_1_Value_Label;
            ChannelListValuesLabels[1] = Channel_List_2_Value_Label;
            ChannelListValuesLabels[2] = Channel_List_3_Value_Label;
            ChannelListValuesLabels[3] = Channel_List_4_Value_Label;
            ChannelListValuesLabels[4] = Channel_List_5_Value_Label;
            ChannelListValuesLabels[5] = Channel_List_6_Value_Label;
            ChannelListValuesLabels[6] = Channel_List_7_Value_Label;
            ChannelListValuesLabels[7] = Channel_List_8_Value_Label;
            ChannelListValuesLabels[8] = Channel_List_9_Value_Label;

            ProgramChannelNowTextBlocks[0] = Program_Channel_Now_1_TextBlock;
            ProgramChannelNowTextBlocks[1] = Program_Channel_Now_2_TextBlock;
            ProgramChannelNowTextBlocks[2] = Program_Channel_Now_3_TextBlock;
            ProgramChannelNowTextBlocks[3] = Program_Channel_Now_4_TextBlock;
            ProgramChannelNowTextBlocks[4] = Program_Channel_Now_5_TextBlock;

            ProgramChannelLaterTextBlocks[0] = Program_Channel_Later_1_TextBlock;
            ProgramChannelLaterTextBlocks[1] = Program_Channel_Later_2_TextBlock;
            ProgramChannelLaterTextBlocks[2] = Program_Channel_Later_3_TextBlock;
            ProgramChannelLaterTextBlocks[3] = Program_Channel_Later_4_TextBlock;
            ProgramChannelLaterTextBlocks[4] = Program_Channel_Later_5_TextBlock;

            ProgramChannelSecondTextBlocks[0] = Program_Channel_Second_1_TextBlock;
            ProgramChannelSecondTextBlocks[1] = Program_Channel_Second_2_TextBlock;
            ProgramChannelSecondTextBlocks[2] = Program_Channel_Second_3_TextBlock;
            ProgramChannelSecondTextBlocks[3] = Program_Channel_Second_4_TextBlock;
            ProgramChannelSecondTextBlocks[4] = Program_Channel_Second_5_TextBlock;

            ProgramChannelThirdTextBlocks[0] = Program_Channel_Third_1_TextBlock;
            ProgramChannelThirdTextBlocks[1] = Program_Channel_Third_2_TextBlock;
            ProgramChannelThirdTextBlocks[2] = Program_Channel_Third_3_TextBlock;
            ProgramChannelThirdTextBlocks[3] = Program_Channel_Third_4_TextBlock;
            ProgramChannelThirdTextBlocks[4] = Program_Channel_Third_5_TextBlock;

            /*smartTV.CurrentVolume = 50;
            smartTV.LastVolume = 50;
            smartTV.IsRecording = false;
            smartTV.CurrentChannel = 1;
            smartTV.NumberOfChannels = 15;*/

            speechWorker.DoWork += speechWorker_DoWork;
            speechWorker.RunWorkerAsync();

            //sre.startSpeechRecognition(Sre_SpeechRecognized, ".\\Grammar\\commandList.xml");
            //sre.addAndLoadGrammar(".\\Grammar\\commandList.xml");
            // synth.SpeakIt("Podaj komendę");

            ss.SetOutputToDefaultAudioDevice();

            builder.DataSource = "DESKTOP-RI8BEUG\\MSSQLSERVER01";
            builder.UserID = "Admin";
            //builder.Password = "password";
            builder.InitialCatalog = "SmartTV";
            builder.IntegratedSecurity = true;


            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                string query = "SELECT LastChannel FROM Basic";
                Console.WriteLine(query);
                SqlCommand query_comm = new SqlCommand(query, connection);
                SqlDataReader reader = query_comm.ExecuteReader();
                reader.Read();
                LastChannel = (int)reader.GetValue(0) - 1;
            }
        }


        private void Power_Button_Click(object sender, RoutedEventArgs e)
        {
            is_On = !is_On;

            if (is_On == true)
            {
                Console.WriteLine("On!!");
                ChangeChannel(LastChannel);
            }
            else
            {
                mute_Timer.Stop();
                channel_Number_Timer.Stop();
                sound_Control_Timer.Stop();
                channel_Info_Timer.Stop();
                channel_List_Timer.Stop();

                Channel_Info.Visibility = Visibility.Hidden;
                Mute_Tag.Visibility = Visibility.Hidden;
                Sound_Control.Visibility = Visibility.Hidden;
                Channel_List.Visibility = Visibility.Hidden;
                Fav_List.Visibility = Visibility.Hidden;
                Channel_Program.Visibility = Visibility.Hidden;
                Channel_Number.Visibility = Visibility.Hidden;
                Command_List.Visibility = Visibility.Hidden;

                ChannelNumber = 15;
                Screen_Image.Source = null;
            }
        }

        private void One_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (Channel_Number.Visibility.Equals(Visibility.Hidden))
                {
                    Channel_Number.Visibility = Visibility.Visible;

                    Channel_Number_Label.Content = "1";
                    channel_Number_Timer.Start();
                }
                else
                {
                    if (Channel_Number_Label.Content.ToString().Length < 3)
                    {
                        channel_Number_Timer.Stop();
                        Channel_Number_Label.Content = Channel_Number_Label.Content + "1";
                        channel_Number_Timer.Start();
                    }
                }
            }
        }

        private void Two_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (Channel_Number.Visibility.Equals(Visibility.Hidden))
                {
                    Channel_Number.Visibility = Visibility.Visible;

                    Channel_Number_Label.Content = "2";
                    channel_Number_Timer.Start();
                }
                else
                {
                    if (Channel_Number_Label.Content.ToString().Length < 3)
                    {
                        channel_Number_Timer.Stop();
                        Channel_Number_Label.Content = Channel_Number_Label.Content + "2";
                        channel_Number_Timer.Start();
                    }
                }
            }
        }

        private void Three_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (Channel_Number.Visibility.Equals(Visibility.Hidden))
                {
                    Channel_Number.Visibility = Visibility.Visible;

                    Channel_Number_Label.Content = "3";
                    channel_Number_Timer.Start();
                }
                else
                {
                    if (Channel_Number_Label.Content.ToString().Length < 3)
                    {
                        channel_Number_Timer.Stop();
                        Channel_Number_Label.Content = Channel_Number_Label.Content + "3";
                        channel_Number_Timer.Start();
                    }
                }
            }
        }

        private void Four_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (Channel_Number.Visibility.Equals(Visibility.Hidden))
                {
                    Channel_Number.Visibility = Visibility.Visible;

                    Channel_Number_Label.Content = "4";
                    channel_Number_Timer.Start();
                }
                else
                {
                    if (Channel_Number_Label.Content.ToString().Length < 3)
                    {
                        channel_Number_Timer.Stop();
                        Channel_Number_Label.Content = Channel_Number_Label.Content + "4";
                        channel_Number_Timer.Start();
                    }
                }
            }
        }

        private void Five_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (Channel_Number.Visibility.Equals(Visibility.Hidden))
                {
                    Channel_Number.Visibility = Visibility.Visible;

                    Channel_Number_Label.Content = "5";
                    channel_Number_Timer.Start();
                }
                else
                {
                    if (Channel_Number_Label.Content.ToString().Length < 3)
                    {
                        channel_Number_Timer.Stop();
                        Channel_Number_Label.Content = Channel_Number_Label.Content + "5";
                        channel_Number_Timer.Start();
                    }
                }
            }
        }

        private void Six_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (Channel_Number.Visibility.Equals(Visibility.Hidden))
                {
                    Channel_Number.Visibility = Visibility.Visible;

                    Channel_Number_Label.Content = "6";
                    channel_Number_Timer.Start();
                }
                else
                {
                    if (Channel_Number_Label.Content.ToString().Length < 3)
                    {
                        channel_Number_Timer.Stop();
                        Channel_Number_Label.Content = Channel_Number_Label.Content + "6";
                        channel_Number_Timer.Start();
                    }
                }
            }
        }

        private void Seven_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (Channel_Number.Visibility.Equals(Visibility.Hidden))
                {
                    Channel_Number.Visibility = Visibility.Visible;

                    Channel_Number_Label.Content = "7";
                    channel_Number_Timer.Start();
                }
                else
                {
                    if (Channel_Number_Label.Content.ToString().Length < 3)
                    {
                        channel_Number_Timer.Stop();
                        Channel_Number_Label.Content = Channel_Number_Label.Content + "7";
                        channel_Number_Timer.Start();
                    }
                }
            }
        }

        private void Eight_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (Channel_Number.Visibility.Equals(Visibility.Hidden))
                {
                    Channel_Number.Visibility = Visibility.Visible;

                    Channel_Number_Label.Content = "8";
                    channel_Number_Timer.Start();
                }
                else
                {
                    if (Channel_Number_Label.Content.ToString().Length < 3)
                    {
                        channel_Number_Timer.Stop();
                        Channel_Number_Label.Content = Channel_Number_Label.Content + "8";
                        channel_Number_Timer.Start();
                    }
                }
            }
        }

        private void Nine_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (Channel_Number.Visibility.Equals(Visibility.Hidden))
                {
                    Channel_Number.Visibility = Visibility.Visible;

                    Channel_Number_Label.Content = "9";
                    channel_Number_Timer.Start();
                }
                else
                {
                    if (Channel_Number_Label.Content.ToString().Length < 3)
                    {
                        channel_Number_Timer.Stop();
                        Channel_Number_Label.Content = Channel_Number_Label.Content + "9";
                        channel_Number_Timer.Start();
                    }
                }
            }
        }

        private void Zero_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (Channel_Number.Visibility.Equals(Visibility.Hidden))
                {
                    Channel_Number.Visibility = Visibility.Visible;

                    Channel_Number_Label.Content = "0";
                    channel_Number_Timer.Start();
                }
                else
                {
                    if (Channel_Number_Label.Content.ToString().Length < 3)
                    {
                        channel_Number_Timer.Stop();
                        Channel_Number_Label.Content = Channel_Number_Label.Content + "0";
                        channel_Number_Timer.Start();
                    }
                }
            }
        }

        private void Vol_Up_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On && Volume < 100)
            {
                if (mute)
                {
                    mute = !mute;
                    Volume = 0;
                    Mute_Tag.Visibility = Visibility.Hidden;
                }
                sound_Control_Timer.Stop();
                Volume = Volume + 10;
                Sound_ProgressBar.Value = Volume;
                Sound_Label.Content = Volume.ToString();
                Sound_Control.Visibility = Visibility.Visible;
                sound_Control_Timer.Start();
            }
        }

        private void Vol_Down_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On && Volume > 0 && !mute)
            {
                sound_Control_Timer.Stop();
                Volume = Volume - 10;
                Sound_ProgressBar.Value = Volume;
                Sound_Label.Content = Volume.ToString();
                Sound_Control.Visibility = Visibility.Visible;
                sound_Control_Timer.Start();
            }
        }

        private void Ch_Up_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                Channel_Program.Visibility = Visibility.Hidden;

                if (ChannelNumber + 1 > 14)
                {
                    ChangeChannel(0);
                }
                else
                {
                    ChangeChannel(ChannelNumber + 1);
                }

                if (Channel_List.Visibility.Equals(Visibility.Visible))
                {
                    channel_List_Timer.Stop();
                    ShowChannelList();
                }
            }
        }

        private void Ch_Down_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                Channel_Program.Visibility = Visibility.Hidden;
                if (ChannelNumber - 1 < 0)
                {
                    ChangeChannel(14);
                }
                else
                {
                    ChangeChannel(ChannelNumber - 1);
                }

                if (Channel_List.Visibility.Equals(Visibility.Visible))
                {
                    channel_List_Timer.Stop();
                    ShowChannelList();
                }
            }
        }

        private void Mute_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (mute)
                {
                    mute = false;
                    if (Mute_Tag.Visibility.Equals(Visibility.Hidden))
                    {
                        Sound_Control.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        //mute_Timer.Stop();
                        Mute_Tag.Visibility = Visibility.Hidden;
                        Sound_Control.Visibility = Visibility.Visible;
                    }
                    sound_Control_Timer.Start();
                }
                else
                {
                    sound_Control_Timer.Stop();
                    Sound_Control.Visibility = Visibility.Hidden;
                    Mute_Tag.Visibility = Visibility.Visible;
                    // mute_Timer.Start();
                    mute = true;
                }

            }

        }

        private void Channel_List_Button_Click(object sender, RoutedEventArgs e)
        {
            channel_List_Timer.Stop();
            ShowChannelList();
        }

        private void Mute_Tick(object sender, EventArgs e)
        {
            mute_Timer.Stop();
            Mute_Tag.Visibility = Visibility.Hidden;
        }

        private void Channel_Number_Tick(object sender, EventArgs e)
        {
            channel_Number_Timer.Stop();
            Channel_Number.Visibility = Visibility.Hidden;
            Channel_Program.Visibility = Visibility.Hidden;

            int tempInt = Int32.Parse(Channel_Number_Label.Content.ToString());
            if (tempInt < 16) ChangeChannel(--tempInt);
        }
        private void Sound_Control_Tick(object sender, EventArgs e)
        {
            sound_Control_Timer.Stop();
            Sound_Control.Visibility = Visibility.Hidden;
        }
        private void Channel_Info_Tick(object sender, EventArgs e)
        {
            channel_Info_Timer.Stop();
            Channel_Info.Visibility = Visibility.Hidden;
        }
        private void Channel_List_Tick(object sender, EventArgs e)
        {
            channel_List_Timer.Stop();
            Channel_List.Visibility = Visibility.Hidden;
        }

        private void ShowChannelList()
        {
            BrushConverter bc = new BrushConverter();
            Brush brush = (Brush)bc.ConvertFrom("SkyBlue");
            ChanelListRectangles[ChanelListChosenField].Fill = brush;
            brush = (Brush)bc.ConvertFrom("Black");
            ChannelListLabels[ChanelListChosenField].Foreground = brush;
            ChannelListValuesLabels[ChanelListChosenField].Foreground = brush;

            if (ChannelNumber < 4)
            {
                for (int i = 0; i < 9; ++i)
                {
                    ChannelListLabels[i].Content = i + 1;
                    ChannelListValuesLabels[i].Content = channels[i].Name;
                }

                bc = new BrushConverter();
                brush = (Brush)bc.ConvertFrom("SteelBlue");
                ChanelListRectangles[ChannelNumber].Fill = brush;
                brush = (Brush)bc.ConvertFrom("SkyBlue");
                ChannelListLabels[ChannelNumber].Foreground = brush;
                ChannelListValuesLabels[ChannelNumber].Foreground = brush;
                ChanelListChosenField = ChannelNumber;
            }
            else if (ChannelNumber > 10)
            {

                for (int i = 0; i < 9; ++i)
                {
                    ChannelListLabels[i].Content = 7 + i;
                    ChannelListValuesLabels[i].Content = channels[6 + i].Name;
                }
                bc = new BrushConverter();
                brush = (Brush)bc.ConvertFrom("SteelBlue");
                ChanelListRectangles[ChannelNumber - 6].Fill = brush;
                brush = (Brush)bc.ConvertFrom("SkyBlue");
                ChannelListLabels[ChannelNumber - 6].Foreground = brush;
                ChannelListValuesLabels[ChannelNumber - 6].Foreground = brush;
                ChanelListChosenField = ChannelNumber - 6;
            }
            else
            {

                for (int i = 0; i < 9; ++i)
                {
                    ChannelListLabels[i].Content = ChannelNumber - 3 + i;
                    ChannelListValuesLabels[i].Content = channels[ChannelNumber - 4 + i].Name;
                }
                bc = new BrushConverter();
                brush = (Brush)bc.ConvertFrom("SteelBlue");
                ChanelListRectangles[4].Fill = brush;
                brush = (Brush)bc.ConvertFrom("SkyBlue");
                ChannelListLabels[4].Foreground = brush;
                ChannelListValuesLabels[4].Foreground = brush;
                ChanelListChosenField = 4;
            }

            channel_List_Timer.Start();
            Channel_List.Visibility = Visibility.Visible;
        }

        private void ChangeChannel(int ChNum)
        {
            if (!ChannelNumber.Equals(ChNum))
            {
                using (var webClient = new System.Net.WebClient())
                {
                    channel_Info_Timer.Stop();
                    Channel_Info.Visibility = Visibility.Hidden;
                    Screen_Image.Visibility = Visibility.Hidden;
                    Screen_Image.Source = null;
                    ChannelNumber = ChNum;
                    string temp = "/data/" + channels[ChNum].BoardAdress;
                    Console.WriteLine(temp);
                    Screen_Image.Source = new BitmapImage(new Uri(@temp, UriKind.Relative));
                    Screen_Image.Visibility = Visibility.Visible;

                    Channel_Label.Content = channels[ChNum].Name;
                    Date_Label.Content = DateTime.Now.ToString("d MMMM HH:mm");
                    Channel_Info_Number_Label.Content = (ChNum + 1).ToString();

                    try
                    {
                        Console.WriteLine("https://pilot.wp.pl/static/epg/0/channel/" + channels[ChNum].ID.ToString() + ".json");
                        var json = webClient.DownloadString("https://pilot.wp.pl/static/epg/0/channel/" + channels[ChNum].ID.ToString() + ".json");
                        Console.WriteLine(json.ToString());
                        JObject jsonObject = JObject.Parse(json);

                        int i = 0;

                        Int32 timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        string tempstamp = jsonObject["timeline"][i]["end_time"].ToString().Substring(0, 10);
                        Console.WriteLine("Timestamp:" + timestamp);
                        Console.WriteLine("Tempstamp:" + tempstamp);
                        while (Int32.Parse(tempstamp) < timestamp)
                        {
                            ++i;
                            tempstamp = jsonObject["timeline"][i]["end_time"].ToString().Substring(0, 10);
                            Console.WriteLine("Tempstamp:" + tempstamp);
                        }

                        byte[] bytes = Encoding.Default.GetBytes(jsonObject["timeline"][i]["title"].ToString());
                        //Console.WriteLine(jsonObject["timeline"][i]["title"].ToString());
                        string myString = Encoding.UTF8.GetString(bytes);
                        Now_Value_Label.Content = myString;
                        //Now_Value_Label.Content = jsonObject["timeline"][i]["title"].ToString();
                        //Console.WriteLine(jsonObject["timeline"][i + 1]["title"].ToString());
                        try
                        {
                            bytes = Encoding.Default.GetBytes(jsonObject["timeline"][i + 1]["title"].ToString());
                            myString = Encoding.UTF8.GetString(bytes);
                            Later_Value_Label.Content = myString;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            //Later_Value_Label.Content = "Brak";
                            json = webClient.DownloadString("https://pilot.wp.pl/static/epg/1/channel/" + channels[ChNum].ID.ToString() + ".json");
                            jsonObject = JObject.Parse(json);
                            bytes = Encoding.Default.GetBytes(jsonObject["timeline"][1]["title"].ToString());
                            myString = Encoding.UTF8.GetString(bytes);
                            Later_Value_Label.Content = myString;
                        }
                    }
                    catch (System.Net.WebException)
                    {
                        Now_Value_Label.Content = "Brak";
                        Later_Value_Label.Content = "Brak";
                    }

                    //Later_Value_Label.Content = jsonObject["timeline"][i + 1]["title"].ToString();
                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {
                        connection.Open();
                        string query = "UPDATE Basic SET LastChannel = " + (ChannelNumber + 1).ToString();
                         
                        Console.WriteLine(query);
                        SqlCommand query_comm = new SqlCommand(query, connection);
                        query_comm.ExecuteNonQuery();

                        connection.Close();
                        connection.Open();
                        query = "SELECT Channel FROM Favorites WHERE Channel = " + (ChannelNumber + 1).ToString();

                        Console.WriteLine(query);
                        query_comm = new SqlCommand(query, connection);
                        SqlDataReader reader = query_comm.ExecuteReader();
                        reader.Read();

                        try
                        {
                            Console.WriteLine(reader.GetValue(0));
                            Channel_Info_Fav_Image.Visibility = Visibility.Visible;
                        }
                        catch (InvalidOperationException)
                        {
                            Channel_Info_Fav_Image.Visibility = Visibility.Hidden;
                        }


                    }

                    LastChannel = ChannelNumber;
                    Channel_Info.Visibility = Visibility.Visible;
                    channel_Info_Timer.Start();
                }
            }
        }

        private void Program_List_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_Program_On)
            {
                Channel_Program.Visibility = Visibility.Hidden;
            }
            else
            {
                Program_Channel_1_Label.Content = channels[ChannelNumber % 15].Name;
                Program_Channel_2_Label.Content = channels[(ChannelNumber + 1) % 15].Name;
                Program_Channel_3_Label.Content = channels[(ChannelNumber + 2) % 15].Name;
                Program_Channel_4_Label.Content = channels[(ChannelNumber + 3) % 15].Name;
                Program_Channel_5_Label.Content = channels[(ChannelNumber + 4) % 15].Name;

                try
                {
                    for (int i = 0; i < 5; ++i)
                    {
                        using (var webClient = new System.Net.WebClient())
                        {
                            var json = webClient.DownloadString("https://pilot.wp.pl/static/epg/0/channel/" + channels[ChannelNumber + i].ID.ToString() + ".json");
                            JObject jsonObject = JObject.Parse(json);
                            int r = 0;

                            try
                            {
                                int n = 0;
                                Int32 timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                                string tempstamp = jsonObject["timeline"][n]["end_time"].ToString().Substring(0, 10);
                                while (Int32.Parse(tempstamp) < timestamp)
                                {
                                    ++n;
                                    tempstamp = jsonObject["timeline"][n]["end_time"].ToString().Substring(0, 10);
                                    Console.WriteLine("Tempstamp:" + tempstamp);
                                }

                                string startstamp = jsonObject["timeline"][n]["start_time"].ToString().Substring(0, 10);
                                string endstamp = jsonObject["timeline"][n]["end_time"].ToString().Substring(0, 10);
                                string progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n]["title"].ToString()));
                                string progDesc = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n]["description"].ToString()));
                                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                ProgramChannelNowTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName + "\n" + progDesc;
                                ++r;

                                startstamp = jsonObject["timeline"][n + 1]["start_time"].ToString().Substring(0, 10);
                                endstamp = jsonObject["timeline"][n + 1]["end_time"].ToString().Substring(0, 10);
                                progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n + 1]["title"].ToString()));
                                ProgramChannelLaterTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                                ++r;

                                startstamp = jsonObject["timeline"][n + 2]["start_time"].ToString().Substring(0, 10);
                                endstamp = jsonObject["timeline"][n + 2]["end_time"].ToString().Substring(0, 10);
                                progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n + 2]["title"].ToString()));
                                ProgramChannelSecondTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                                ++r;

                                startstamp = jsonObject["timeline"][n + 3]["start_time"].ToString().Substring(0, 10);
                                endstamp = jsonObject["timeline"][n + 3]["end_time"].ToString().Substring(0, 10);
                                progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n + 3]["title"].ToString()));
                                ProgramChannelThirdTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                json = webClient.DownloadString("https://pilot.wp.pl/static/epg/1/channel/" + channels[ChannelNumber + i].ID.ToString() + ".json");
                                jsonObject = JObject.Parse(json);
                                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                int n = 1;
                                switch (r)
                                {
                                    case 0:

                                        string startstamp = jsonObject["timeline"][n]["start_time"].ToString().Substring(0, 10);
                                        string endstamp = jsonObject["timeline"][n]["end_time"].ToString().Substring(0, 10);
                                        string progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n]["title"].ToString()));
                                        string progDesc = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n]["description"].ToString()));
                                        ProgramChannelNowTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName + "\n" + progDesc;
                                        ++r;

                                        startstamp = jsonObject["timeline"][n + 1]["start_time"].ToString().Substring(0, 10);
                                        endstamp = jsonObject["timeline"][n + 1]["end_time"].ToString().Substring(0, 10);
                                        progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n + 1]["title"].ToString()));
                                        ProgramChannelLaterTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                                        ++r;

                                        startstamp = jsonObject["timeline"][n + 2]["start_time"].ToString().Substring(0, 10);
                                        endstamp = jsonObject["timeline"][n + 2]["end_time"].ToString().Substring(0, 10);
                                        progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n + 2]["title"].ToString()));
                                        ProgramChannelSecondTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                                        ++r;

                                        startstamp = jsonObject["timeline"][n + 3]["start_time"].ToString().Substring(0, 10);
                                        endstamp = jsonObject["timeline"][n + 3]["end_time"].ToString().Substring(0, 10);
                                        progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n + 3]["title"].ToString()));
                                        ProgramChannelThirdTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                                        break;

                                    case 1:

                                        startstamp = jsonObject["timeline"][n]["start_time"].ToString().Substring(0, 10);
                                        endstamp = jsonObject["timeline"][n]["end_time"].ToString().Substring(0, 10);
                                        progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n]["title"].ToString()));
                                        ProgramChannelLaterTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                                        ++r;

                                        startstamp = jsonObject["timeline"][n + 1]["start_time"].ToString().Substring(0, 10);
                                        endstamp = jsonObject["timeline"][n + 1]["end_time"].ToString().Substring(0, 10);
                                        progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n + 1]["title"].ToString()));
                                        ProgramChannelSecondTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                                        ++r;

                                        startstamp = jsonObject["timeline"][n + 2]["start_time"].ToString().Substring(0, 10);
                                        endstamp = jsonObject["timeline"][n + 2]["end_time"].ToString().Substring(0, 10);
                                        progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n + 2]["title"].ToString()));
                                        ProgramChannelThirdTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                                        break;

                                    case 2:

                                        startstamp = jsonObject["timeline"][n]["start_time"].ToString().Substring(0, 10);
                                        endstamp = jsonObject["timeline"][n]["end_time"].ToString().Substring(0, 10);
                                        progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n]["title"].ToString()));
                                        ProgramChannelSecondTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                                        ++r;

                                        startstamp = jsonObject["timeline"][n + 1]["start_time"].ToString().Substring(0, 10);
                                        endstamp = jsonObject["timeline"][n + 1]["end_time"].ToString().Substring(0, 10);
                                        progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n + 1]["title"].ToString()));
                                        ProgramChannelThirdTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                                        break;

                                    default:

                                        startstamp = jsonObject["timeline"][n]["start_time"].ToString().Substring(0, 10);
                                        endstamp = jsonObject["timeline"][n]["end_time"].ToString().Substring(0, 10);
                                        progName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsonObject["timeline"][n]["title"].ToString()));
                                        ProgramChannelThirdTextBlocks[i].Text = dtDateTime.AddSeconds(Int32.Parse(startstamp)).ToLocalTime().ToString("HH:mm") + "-" + dtDateTime.AddSeconds(Int32.Parse(endstamp)).ToLocalTime().ToString("HH:mm") + "\n" + progName;
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (System.Net.WebException)
                {

                }
                Channel_Program.Visibility = Visibility.Visible;
            }

            is_Program_On = !is_Program_On;
        }

        private void Command_List_Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_On)
            {
                if (Command_List.Visibility.Equals(Visibility.Visible))
                {
                    Command_List.Visibility = Visibility.Hidden;
                }
                else
                {
                    Command_List.Visibility = Visibility.Visible;
                }
            }
        }

        private void showFavList()
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                string query = "SELECT * FROM  Favorites ASC";

                Console.WriteLine(query);
                SqlCommand query_comm = new SqlCommand(query, connection);
                query_comm.ExecuteNonQuery();

                connection.Close();
                connection.Open();
                query = "SELECT Channel FROM Favorites WHERE Channel = " + (ChannelNumber + 1).ToString();

                Console.WriteLine(query);
                query_comm = new SqlCommand(query, connection);
                SqlDataReader reader = query_comm.ExecuteReader();
                reader.Read();

                try
                {
                    Console.WriteLine(reader.GetValue(0));
                    Channel_Info_Fav_Image.Visibility = Visibility.Visible;
                }
                catch (InvalidOperationException)
                {
                    Channel_Info_Fav_Image.Visibility = Visibility.Hidden;
                }


            }
        }

        private void speechWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {

                sre.startSpeechRecognition(Sre_SpeechRecognized, ".\\Grammar\\commandList.xml");
                synth.SpeakIt("Podaj komendę");

                while (done == false) {; }
            }
        }


        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string txt = e.Result.Text;
            float confidence = e.Result.Confidence;

            if (confidence > 0.7)
            { 
                Console.WriteLine("------------------------------------------------------------------------------");
                Console.WriteLine("---------------Rozpoznano: " + txt + " pewność: " + e.Result.Confidence + "---------------");
                Console.WriteLine("------------------------------------------------------------------------------");
                synth.SpeakIt("Rozpoznano");

                if (sre.grammarInfo == ".\\Grammar\\commandList.xml")
                {
                    string command = e.Result.Semantics["command"].Value.ToString();
                    string[] Result = e.Result.Text.Split(' ');

                    if (command == "help")
                    {
                        // Ustawienie podpowiedzi
                        if (is_On)
                        {
                            if (Command_List.Visibility.Equals(Visibility.Hidden))
                            {
                                dispatcher.BeginInvoke((Action)(() =>
                                {
                                    Command_List.Visibility = Visibility.Visible;
                                }
                                ));
                                synth.SpeakIt("Komendy pozwalają: Zmienić poziom głośności, zmienić kanał, uruchomić nagrywanie, wyświetlić listę kanałów, wyświetlić program tiwi, wyłączyć telewizor");
                            }
                        }
                        //ss.Speak("Komendy pozwalają: Zmienić poziom głośności, zmienić program, włączyć aplikacje czy uruchomić nagrywanie");
                    }
                    if (command == "nohelp")
                    {
                        if(is_On)
                        {
                            if (Command_List.Visibility.Equals(Visibility.Visible))
                            {
                                // Ustawienie podpowiedzi
                                dispatcher.BeginInvoke((Action)(() =>
                                {
                                    Command_List.Visibility = Visibility.Hidden;
                                }
                                ));
                            }
                        }
                        //ss.Speak("Komendy pozwalają: Zmienić poziom głośności, zmienić program, włączyć aplikacje czy uruchomić nagrywanie");
                    }
                    if (command == "volumeUp")
                    {
                        if (is_On)
                        {
                            if (Result.Length >= 4) // podano pełna komendę - nie trzeba dopytywać
                            {
                                try
                                {
                                    int value = Convert.ToInt32(e.Result.Semantics["volumeValue"].Value);
                                    //smartTV.ChangeVolume(value);

                                    dispatcher.BeginInvoke((Action)(() =>
                                    {

                                        if (Volume == 100)
                                        {
                                            synth.SpeakIt("Głośność jest już na poziomie 100");
                                        }
                                        else
                                        {
                                            sound_Control_Timer.Stop();
                                            Volume = Volume + value;
                                            if (Volume > 100) Volume = 100;
                                            Sound_ProgressBar.Value = Volume;
                                            Sound_Label.Content = Volume.ToString();
                                            Sound_Control.Visibility = Visibility.Visible;
                                            synth.SpeakIt("Zwiększono głośność. Obecna głośność to " + Volume.ToString() + "punktów");
                                            sound_Control_Timer.Start();
                                        }
                                    }
                                    ));

                                    //Globals.grammarFlag = "commandList";
                                }
                                catch (Exception x)
                                {
                                    Console.WriteLine(x.Message);
                                }
                            }
                            else
                            {
                                sre.stopSpeechRecognition();
                                sre.startSpeechRecognition(Sre_SpeechRecognized, ".\\Grammar\\volumeValue.xml");
                                synth.SpeakIt("Podaj wartość o którą zwiększyć głośność");
                                sre.volumeChangeType = "up";
                            }
                        }
                    }
                    if (command == "volumeDown")
                    {
                        if (is_On)
                        {
                            if (Result.Length >= 4) //podano pełna komendę - nie trzeba dopytywać
                            {
                                try
                                {
                                    int value = Convert.ToInt32(e.Result.Semantics["volumeValue"].Value);
                                    //smartTV.ChangeVolume(-value);

                                    if(Volume == 0)
                                    {
                                        synth.SpeakIt("Głośność jest już na poziomie zero");
                                    }
                                    else
                                    {
                                        dispatcher.BeginInvoke((Action)(() =>
                                        {

                                            sound_Control_Timer.Stop();
                                            Volume = Volume - value;
                                            if (Volume < 0) Volume = 100;
                                            Sound_ProgressBar.Value = Volume;
                                            Sound_Label.Content = Volume.ToString();
                                            Sound_Control.Visibility = Visibility.Visible;
                                            synth.SpeakIt("Zmniejszono głośność. Obecna głośność to " + Volume.ToString() + "punktów");
                                            sound_Control_Timer.Start();
                                        }
                                        ));
                                    }
                                }
                                catch (Exception x)
                                {
                                    Console.WriteLine(x.Message);
                                }
                            }
                            else
                            {
                                sre.stopSpeechRecognition();
                                sre.volumeChangeType = "down";
                                sre.startSpeechRecognition(Sre_SpeechRecognized, ".\\Grammar\\volumeValue.xml");
                                synth.SpeakIt("Podaj wartość o którą zmniejszyć głośność");
                            }
                        }
                    }
                    if (command == "volumeOff")
                    {
                        //smartTV.MuteVolume();

                        if (is_On)
                        {
                            dispatcher.BeginInvoke((Action)(() =>
                            {
                                if (mute)
                                {
                                    synth.SpeakIt("Dźwięk jest już wyłączony");
                                }
                                else
                                {
                                    Mute_Button_Click(this, null);
                                    synth.SpeakIt("Wyłączono dźwięk");
                                }
                            }
                            ));
                        }
                    }
                    if (command == "volumeOn")
                    {
                        //smartTV.UnMuteVolume();
                        if (is_On)
                        {
                            dispatcher.BeginInvoke((Action)(() =>
                            {

                                if (!mute)
                                {
                                    synth.SpeakIt("Dźwięk jest już włączony");
                                }
                                else
                                {
                                    synth.SpeakIt("Włączono dźwięk");
                                    Mute_Button_Click(this, null);
                                }
                            }
                            ));
                        }
                    }
                    if (command == "volumeLevel")
                    {
                        if (is_On)
                        {
                            if (Volume == 0 && mute)
                            {
                                //smartTV.CurrentVolume = smartTV.LastVolume;
                                synth.SpeakIt("Obecnie dźwięk jest wyłączony");
                            }
                            else
                            {
                                dispatcher.BeginInvoke((Action)(() =>
                                {

                                    Sound_Control.Visibility = Visibility.Visible;
                                    sound_Control_Timer.Start();
                                }
                                ));
                                synth.SpeakIt("Poziom dźwięku wynosi " + Volume + " punktów");
                            }
                        }
                    }
                    if (command == "changeChannel")
                    {
                        if (is_On)
                        {
                            if (Result.Length >= 4) // podano pełna komendę - nie trzeba dopytywać
                            {
                                try
                                {
                                    int channelNumber = Convert.ToInt32(e.Result.Semantics["channelList"].Value);
                                    Console.WriteLine(channelNumber);
                                    dispatcher.BeginInvoke((Action)(() =>
                                    {
                                        channel_Number_Timer.Stop();
                                        Channel_Number.Visibility = Visibility.Hidden;
                                        Channel_Program.Visibility = Visibility.Hidden;

                                        if (channelNumber < 16 && channelNumber > 0)
                                        {
                                            synth.SpeakIt("Zmieniono kanał na: " + channelNumber + ".");
                                            ChangeChannel(--channelNumber);
                                        }
                                        else if(channelNumber == 0)
                                        {
                                            synth.SpeakIt("Zmieniono kanał na następny");
                                            Ch_Up_Button_Click(this,null);
                                        }
                                        else if (channelNumber == -1)
                                        {
                                            synth.SpeakIt("Zmieniono kanał na poprzedni");
                                            Ch_Down_Button_Click(this, null);
                                        }
                                        else
                                        {
                                            synth.SpeakIt("Nie ma takiego kanału.");
                                        }
                                    }
                                    ));
                                    //smartTV.ChangeChannel(channelNumber);
                                }
                                catch (Exception x)
                                {
                                    Console.WriteLine(x.Message);
                                }
                            }
                            else
                            {
                                sre.stopSpeechRecognition();
                                sre.startSpeechRecognition(Sre_SpeechRecognized, ".\\Grammar\\channelList.xml");
                                synth.SpeakIt("Na który kanał zmienić?");
                            }
                        }
      
                    }
                    if (command == "recordingOn")
                    {
                        //Obsługa włączania nagrywania
                       // smartTV.TurnOnRecording();
                       
                    }
                    if (command == "recordingOff")
                    {
                        //Obsługa włączania nagrywania
                        //smartTV.TurnOffRecording();
                    }
                    if (command == "addtofav")
                    {
                        if (is_On)
                        {
                            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                            {
                                connection.Open();
                                string query = "SELECT * FROM Favorites WHERE Channel = " + (ChannelNumber + 1).ToString();

                                Console.WriteLine(query);
                                SqlCommand query_comm = new SqlCommand(query, connection);
                                SqlDataReader reader = query_comm.ExecuteReader();
                                reader.Read();

                                // if (String.IsNullOrEmpty(reader.GetString(0)))
                                try
                                {
                                    Console.WriteLine(reader.GetValue(0));
                                    synth.SpeakIt("Kanał " + (ChannelNumber + 1).ToString() + " jest już w ulubionych");
                                }
                                catch (InvalidOperationException)
                                {
                                    connection.Close();
                                    connection.Open();
                                    query = "INSERT INTO Favorites (Channel) VALUES (" + (ChannelNumber + 1).ToString() + ")";
                                    query_comm = new SqlCommand(query, connection);
                                    query_comm.ExecuteNonQuery();
                                    connection.Close();
                                    synth.SpeakIt("Kanał: " + (ChannelNumber + 1).ToString() + " dodany do ulubionych");
                                    dispatcher.BeginInvoke((Action)(() =>
                                    {
                                        Channel_Info_Fav_Image.Visibility = Visibility.Visible;
                                    }
                                    ));
                                }
                            }
                            //Obsługa włączania nagrywania
                            // smartTV.TurnOnRecording();
                        }
                    }
                    if (command == "deletefromfav")
                    {
                        if (is_On)
                        {

                            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                            {
                                connection.Open();
                                string query = "SELECT * FROM Favorites WHERE Channel = " + (ChannelNumber + 1).ToString();

                                Console.WriteLine(query);
                                SqlCommand query_comm = new SqlCommand(query, connection);
                                SqlDataReader reader = query_comm.ExecuteReader();
                                reader.Read();

                                try
                                {
                                    Console.WriteLine(reader.GetValue(0));
                                    connection.Close();
                                    connection.Open();
                                    query = "DELETE FROM Favorites WHERE Channel = " + (ChannelNumber + 1).ToString();
                                    query_comm = new SqlCommand(query, connection);
                                    query_comm.ExecuteNonQuery();
                                    connection.Close();
                                    synth.SpeakIt("Kanał: " + (ChannelNumber + 1).ToString() + " usunięty z ulubionych");
                                    dispatcher.BeginInvoke((Action)(() =>
                                    {
                                            Channel_Info_Fav_Image.Visibility = Visibility.Hidden;
                                    }
                                    ));
                                }
                                catch (InvalidOperationException)
                                {
                                    synth.SpeakIt("Kanał: " + (ChannelNumber + 1).ToString() + " nie występuje w ulubionych");
                                }
                            }
                        }
                        //Obsługa włączania nagrywania
                        //smartTV.TurnOffRecording();
                    }
                    if (command == "showTelecast")
                    {
                        if (is_On)
                        {
                            //Obsługa włączenia programu TV dla aktualnego programu
                            //smartTV.ShowTelecast();
                            dispatcher.BeginInvoke((Action)(() =>
                            {
                                if (Channel_Program.Visibility == Visibility.Hidden)
                                {
                                    Program_List_Button_Click(this, null);
                                }
                            }
                            ));
                        }

                    }
                    if(command == "hideTelecast")
                    {
                        //Obsługa wyłączenia programu TV dla aktualnego programu
                        //smartTV.HideTelecast();
                        if (is_On)
                        {
                            dispatcher.BeginInvoke((Action)(() =>
                            {
                                if (Channel_Program.Visibility == Visibility.Visible)
                                {
                                    Program_List_Button_Click(this, null);
                                }
                            }
                            ));
                        }
                    }
                    if(command == "showChannelList")
                    {
                        //Obsługa pokazania listy kanałów
                        //smartTV.ShowChannelList();
                        if (is_On)
                        {
                            dispatcher.BeginInvoke((Action)(() =>
                            {
                                if (Channel_List.Visibility == Visibility.Hidden)
                                {
                                    Channel_List_Button_Click(this, null);
                                    channel_List_Timer.Stop();
                                }
                            }
                            ));
                        }
                    }
                    if (command == "hideChannelList")
                    {
                        //Obsługa schowania listy kanałów
                        //smartTV.HideChannelList();
                        if (is_On)
                        {
                            dispatcher.BeginInvoke((Action)(() =>
                            {
                                if (Channel_List.Visibility == Visibility.Visible)
                                {
                                    Channel_List_Button_Click(this, null);
                                }
                            }
                            ));
                        }
                    }
                    if (command == "currentChannel")
                    {
                        if (is_On)
                        {
                            //Obsługa pokazania opisu obecnego programu
                            //smartTV.ShowDescription();
                            dispatcher.BeginInvoke((Action)(() =>
                            {
                                Channel_Info.Visibility = Visibility.Visible;
                            }
                            ));
                        }
                    }
                    if (command == "showDescription")
                    {
                        //Obsługa pokazania opisu obecnego programu
                        //smartTV.ShowDescription();
                        if (is_On)
                        {
                            dispatcher.BeginInvoke((Action)(() =>
                            {
                                Channel_Info.Visibility = Visibility.Visible;
                                synth.SpeakIt("Aktualnie grany jest program" + Now_Value_Label.Content.ToString());
                            }
                            ));
                        }
                    }
                    if (command == "hideDescription")
                    {
                        if (is_On)
                        {
                            //Obsługa schowania opisu obecnego programu
                            //smartTV.HideDescription();
                            dispatcher.BeginInvoke((Action)(() =>
                            {
                                Channel_Info.Visibility = Visibility.Hidden;
                            }
                            ));
                        }
                    }
                    if(command == "turnOffTV")
                    {
                        if (is_On)
                        {
                            dispatcher.BeginInvoke((Action)(() =>
                            {
                                Power_Button_Click(this, null);
                            }
                            ));
                        }
                        else
                        {
                            dispatcher.BeginInvoke((Action)(() =>
                            {
                                System.Windows.Application.Current.Shutdown();
                            }
                            ));
                        }
                    }

                    if (command == "turnOnTV")
                    {
                        if(!is_On)
                        {
                            dispatcher.BeginInvoke((Action)(() =>
                            {
                                Power_Button_Click(this, null);
                            }
                       ));
                        }
                        else
                        {
                            synth.SpeakIt("Telewizor jest już włączony");
                        }
                        //Obsługa wyłączenia telewizora
                        //smartTV.TurnOff();
                    }
                }
                else if(sre.grammarInfo == ".\\Grammar\\volumeValue.xml")
                {
                    sre.stopSpeechRecognition();
                    int value = Convert.ToInt32(e.Result.Semantics["volumeValue"].Value);

                    if(sre.volumeChangeType == "up")
                    {
                        //smartTV.ChangeVolume(value);
                        dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (Volume == 100)
                            {
                                synth.SpeakIt("Głośność jest już na poziomie 100");
                            }
                            else
                            {
                                sound_Control_Timer.Stop();
                                Volume = Volume + value;
                                if (Volume > 100) Volume = 100;
                                Sound_ProgressBar.Value = Volume;
                                Sound_Label.Content = Volume.ToString();
                                Sound_Control.Visibility = Visibility.Visible;
                                synth.SpeakIt("Zwiększono głośność. Obecna głośność to " + Volume.ToString() + "punktów");
                                sound_Control_Timer.Start();
                                sre.startSpeechRecognition(Sre_SpeechRecognized, ".\\Grammar\\commandList.xml");
                            }
                        }
                        ));
                    }
                    else if(sre.volumeChangeType == "down")
                    {
                        //smartTV.ChangeVolume(-value);
                        dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (Volume == 0)
                            {
                                synth.SpeakIt("Głośność jest już na poziomie zero");
                            }
                            else
                            {
                                sound_Control_Timer.Stop();
                                Volume = Volume - value;
                                if (Volume < 0) Volume = 0;
                                Sound_ProgressBar.Value = Volume;
                                Sound_Label.Content = Volume.ToString();
                                Sound_Control.Visibility = Visibility.Visible;
                                synth.SpeakIt("Zmniejszono głośność. Obecna głośność to " + Volume.ToString() + "punktów");
                                sound_Control_Timer.Start();
                                sre.startSpeechRecognition(Sre_SpeechRecognized, ".\\Grammar\\commandList.xml");
                            }
                        }
                        ));
                    }
                    //Globals.grammarFlag = "commandList";
                    //Recognize(".\\Grammar\\commandList.xml", "");
                }
                else if(sre.grammarInfo == ".\\Grammar\\channelList.xml")
                {
                    sre.stopSpeechRecognition();
                    int channelNumber = Convert.ToInt32(e.Result.Semantics["channelNumber"].Value);
                    Console.WriteLine(channelNumber);
                    //smartTV.ChangeChannel(channelNumber);
                    dispatcher.BeginInvoke((Action)(() =>
                    {
                        channel_Number_Timer.Stop();
                        Channel_Number.Visibility = Visibility.Hidden;
                        Channel_Program.Visibility = Visibility.Hidden;

                        if (channelNumber < 16 && channelNumber > 0)
                        {
                            synth.SpeakIt("Zmieniono kanał na: " + channelNumber + ".");
                            ChangeChannel(--channelNumber);
                        }
                        else if (channelNumber == 0)
                        {
                            synth.SpeakIt("Zmieniono kanał na następny");
                            Ch_Up_Button_Click(this, null);
                        }
                        else if (channelNumber == -1)
                        {
                            synth.SpeakIt("Zmieniono kanał na poprzedni");
                            Ch_Down_Button_Click(this, null);
                        }
                        else
                        {
                            synth.SpeakIt("Nie ma takiego kanału.");
                        }

                        sre.startSpeechRecognition(Sre_SpeechRecognized, ".\\Grammar\\commandList.xml");
                    }
                    ));

                    //Globals.grammarFlag = "commandList";
                    //(".\\Grammar\\commandList.xml", "");
                }
            }
            else
            {
                ss.Speak("Nie rozpoznano komendy");
                // synth.SpeakIt("Nie rozpoznano komendy");
            }
        }

    }
    /*class Televisor
    {
        TextSpeach synth = new TextSpeach();

        public int CurrentChannel { get; set; } // obecny kanał

        public int CurrentVolume { get; set; } // obecna głośność

        public int LastVolume { get; set; } // zmienna pomocnicza do łatwego wracania do poprzedniej wartości głośności po wyłączeniu wyciszenia

        public bool IsRecording { get; set; }

        public string[] ChannelList { get; set; } // nie wiem czy to Ci potrzebne, możesz zmodyfikować, żeby odczytywać z bazy danych

        public int NumberOfChannels { get; set; }

        public void TurnOn() // wyłączenie telewizora
        {

            Console.WriteLine("Włączono TV");
        }

        public void TurnOff() // wyłączenie telewizora
        {

            Console.WriteLine("Wyłączono TV");
        }

        public void ChangeVolume(int value) // zmień głośność
        {
            if(value < 0)
            {
                if (CurrentVolume - value > 0)
                {
                    CurrentVolume -= value;
                }
                else
                {
                    CurrentVolume = 0;
                }
                synth.SpeakIt("Zmniejszono głośność. Obecna głośność to " + CurrentVolume.ToString() + "punktów");
            }
            else if (value > 0)
            {
                if (CurrentVolume + value < 100)
                {
                    CurrentVolume += value;
                }
                else
                {
                    CurrentVolume = 100;
                }
                synth.SpeakIt("Zwiększono głośność. Obecna głośność to " + CurrentVolume.ToString() + "punktów");
            }
            Console.WriteLine("Zmieniono głośność");
        }

        public void VolumeLevel()
        {
            if (CurrentVolume == 0)
            {
                //smartTV.CurrentVolume = smartTV.LastVolume;
                synth.SpeakIt("Obecnie dźwięk jest wyłączony");
            }
            else
            {
                synth.SpeakIt("Poziom dźwięku wynosi " + CurrentVolume + " punktów");
            }
        } // dowiedz się o poziomie głośności

        public void MuteVolume() // wyłącz dźwięk
        {
            if (CurrentVolume != 0)
            {
                LastVolume = CurrentVolume;
                CurrentVolume = 0;
                synth.SpeakIt("Wyłączono dźwięk");
            }
            else
            {
                synth.SpeakIt("Dźwięk jest już wyłączony");
            }

            Console.WriteLine("Wyciszono dźwięk");
        }

        public void UnMuteVolume() // włącz dźwięk
        {
            if (CurrentVolume == 0)
            {
                CurrentVolume = LastVolume;
                synth.SpeakIt("Włączono dźwięk");
            }
            else
            {
                synth.SpeakIt("Dźwięk jest już włączony");
            }    
            Console.WriteLine("Włączono dźwięk");
        }

        public void ChangeChannel(int channelNumber) // zmień kanał
        {
            if (channelNumber == 0) // ustaw następny kanał
            {
                if (CurrentChannel + 1 <= NumberOfChannels)
                {
                    CurrentChannel++;
                }
                else
                {
                    CurrentChannel = 1;
                }
            }
            else if (channelNumber == -1) // ustaw poprzedni kanał
            {
                if (CurrentChannel - 1 > 0)
                {
                    CurrentChannel--;
                }
                else
                {
                    CurrentChannel = NumberOfChannels;
                }
            }
            else // ustaw dany kanał
            {
                CurrentChannel = channelNumber;
            }

            synth.SpeakIt("Zmieniono kanał na: " + CurrentChannel + ".");
            Console.WriteLine("Zmieniono kanał na: " + CurrentChannel +".");
        }

        public void TurnOnRecording() // włączenie nagrywania
        {
            if (IsRecording)
            {
                synth.SpeakIt("Nagrywanie jest już włączone!");
            }
            else
            {
                synth.SpeakIt("Włączono nagrywanie!");
                IsRecording = true;
            }
            Console.WriteLine("Włączono nagrywanie");
        }

        public void TurnOffRecording() // wyłączenie nagrywania
        {
            if (!IsRecording)
            {
                synth.SpeakIt("Nie można wyłączyć nagrywania, ponieważ nie było wcześniej włączone");
            }
            else
            {
                synth.SpeakIt("Wyłączono nagrywanie!");
                IsRecording = false;
            }
            
            Console.WriteLine("Wyłączono nagrywanie");
        }

        public void ShowTelecast() // włącz program TV
        {

            Console.WriteLine("Pokazano program TV");
        }

        public void HideTelecast() // wyłącz program TV
        {

            Console.WriteLine("Schowano program TV");
        }

        public void ShowChannelList() // włącz listę kanałów
        {

            Console.WriteLine("Pokazano listę kanałów");
        }

        public void HideChannelList() // wyłącz listę kanałów
        {

            Console.WriteLine("Schowano listę kanałów");
        }

        public void ShowDescription() // pokaż opis obecnego programu
        {
            Console.WriteLine("Pokazano opis obecnego programu ");
        }

        public void HideDescription() // schowaj opis obecnego programu
        {
            Console.WriteLine("Schowano opis obecnego programu ");
        }

    }*/

}
