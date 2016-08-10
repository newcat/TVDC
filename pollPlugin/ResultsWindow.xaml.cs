using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using tvdc.EventArguments;
using tvdc.Plugin;

namespace pollPlugin
{
    /// <summary>
    /// Interaktionslogik für ResultsWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {

        private readonly Color[,] colors = new Color[7,2]
        {
            { Color.FromRgb(255, 0, 0), Color.FromRgb(255, 60, 60)},
            { Color.FromRgb(0, 255, 0), Color.FromRgb(60, 255, 60)},
            { Color.FromRgb(0, 0, 255), Color.FromRgb(60, 60, 255)},
            { Color.FromRgb(255, 255, 0), Color.FromRgb(255, 255, 60)},
            { Color.FromRgb(0, 255, 255), Color.FromRgb(60, 255, 255)},
            { Color.FromRgb(255, 0, 255), Color.FromRgb(255, 60, 255)},
            { Color.FromRgb(255, 136, 0), Color.FromRgb(255, 165, 61)} 
        };

        private Dictionary<string, OptionData> pollOptions = new Dictionary<string, OptionData>();
        private Dictionary<string, List<string>> voters = new Dictionary<string, List<string>>();
        private int totalVotes = 0;
        private bool multiVote;

        private IPluginHost host;

        private DispatcherTimer tmr = new DispatcherTimer();
        private DateTime startTime;

        public ResultsWindow(IEnumerable<string> options, IPluginHost host, bool multiVote)
        {
            InitializeComponent();

            cbNotify.IsChecked = Properties.Settings.Default.notify;

            this.host = host;
            this.host.IRC_PrivmsgReceived += messageReceived;

            this.multiVote = multiVote;
            tbVoteType.Text = multiVote ? "Multi Vote" : "Single Vote";

            startTime = DateTime.Now;
            tmr.Interval = TimeSpan.FromSeconds(1);
            tmr.Tick += Tmr_Tick;
            tmr.Start();

            int i = 0;
            foreach (string option in options)
            {
                ChartBar cb = new ChartBar(colors[i, 0], colors[i, 1]);
                Canvas.SetTop(cb, 10);

                TextBlock tb = new TextBlock();
                tb.Foreground = Brushes.White;
                tb.Text = option;
                Canvas.SetBottom(tb, 10);

                MainCanvas.Children.Add(cb);
                MainCanvas.Children.Add(tb);

                pollOptions.Add(option, new OptionData { cb = cb, tb = tb, voteCount = 0 });
                i++;
            }

            Show();
            positionBarsAndText();
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            tbTimeRunning.Text = (DateTime.Now - startTime).ToString("hh':'mm':'ss");
            tbTotalVotes.Text = totalVotes.ToString();

            foreach (OptionData od in pollOptions.Values)
            {
                if (totalVotes == 0)
                {
                    od.cb.setValue(0);
                } else
                {
                    od.cb.setValue((od.voteCount / totalVotes * 1.0) * 200);
                }
            }
        }

        private void positionBarsAndText()
        {

            int optionCount = pollOptions.Count;
            double canvasWidth = MainCanvas.ActualWidth;
            double spacing = (canvasWidth - 40 - optionCount * 30) / (optionCount + 1);

            int i = 1;
            foreach (string key in pollOptions.Keys)
            {
                double x = spacing * i + (i - 1) * 30 + 40;
                Canvas.SetLeft(pollOptions[key].cb, x);
                Canvas.SetLeft(pollOptions[key].tb,
                    (x + 15) - (pollOptions[key].tb.ActualWidth / 2));
                i++;
            }

        }

        public void messageReceived(object sender, PrivmsgReceivedEventArgs e)
        {
            if (pollOptions.ContainsKey(e.message.Trim().ToLower()))
            {
                string option = e.message.Trim().ToLower();
                if (voters.ContainsKey(e.username))
                {
                    if (multiVote)
                    {
                        if (voters[e.username].Contains(option) && Properties.Settings.Default.notify)
                        {
                            host.sendMesssage(string.Format("/w {0} You already voted for {1}.",
                            e.username, option));
                            return;
                        } else
                        {
                            voters[e.username].Add(option);
                            totalVotes += 1;
                            if (Properties.Settings.Default.notify)
                                host.sendMesssage(string.Format("/w {0} Thanks for voting! You voted for {1}.",
                                e.username, string.Join(", ", voters[e.username].ToArray())));
                        }
                    } else
                    {
                        pollOptions[voters[e.username][0]].voteCount -= 1;
                        voters[e.username][0] = option;
                        if (Properties.Settings.Default.notify)
                            host.sendMesssage(string.Format("/w {0} You changed your vote to {1}.",
                            e.username, option));
                    }
                } else
                {
                    totalVotes += 1;
                    List<string> l = new List<string>();
                    l.Add(option);
                    voters.Add(e.username, l);
                    if (Properties.Settings.Default.notify)
                        host.sendMesssage(string.Format("/w {0} Thanks for voting! You voted for {1}.",
                            e.username, option));
                }
                pollOptions[option].voteCount += 1;
            }
        }

        private class OptionData
        {
            public ChartBar cb;
            public TextBlock tb;
            public int voteCount;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            positionBarsAndText();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            tmr.Stop();
            host.IRC_PrivmsgReceived -= messageReceived;
        }

        private void cbNotify_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.notify = true;
            Properties.Settings.Default.Save();
        }

        private void cbNotify_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.notify = false;
            Properties.Settings.Default.Save();
        }
    }
}
