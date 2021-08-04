using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Dispatcher = System.Windows.Threading.Dispatcher;
using Process = System.Diagnostics.Process;
using System.Windows;
using System.Windows.Controls;

namespace TimeOff
{
    enum State { On, Off };
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        State timerState = State.Off;
        uint hours, minutes, seconds;
        Task timer;
        bool cancel = false;
        Dispatcher dispatcher;
        
        public MainWindow()
        {
            InitializeComponent();
            dispatcher = Dispatcher;
        }

        void Validate()
        {
            if (!uint.TryParse(HoursBox.Text, out hours)) throw new Exception("Hours are invalid.");
            if (!uint.TryParse(MinutesBox.Text, out minutes)) throw new Exception("Minutes are invalid.");
            if (!uint.TryParse(SecondsBox.Text, out seconds)) throw new Exception("Seconds are invalid.");
            if (hours > 23) throw new Exception("Hours value may not be bigger than 23.");
            if (minutes > 59) throw new Exception("Minutes value may not be bigger than 59.");
            if (seconds > 59) throw new Exception("Seconds value may not be bigger than 59.");
            if (hours == 0 && minutes == 0 && seconds == 0) throw new Exception("Timer has no time.");
        }

        void switchOnTimer()
        {
            dispatcher.Invoke(() => HoursBox.IsReadOnly = MinutesBox.IsReadOnly = SecondsBox.IsReadOnly = true);

            while (true)
            {
                if (cancel)
                {
                    cancel = false;
                    dispatcher.Invoke(() => HoursBox.IsReadOnly = MinutesBox.IsReadOnly = SecondsBox.IsReadOnly = false);
                    return;
                }
                Thread.Sleep(1000);
                if (seconds <= 0)
                {
                    if (minutes <= 0)
                    {
                        if (hours <= 0) break;
                        else
                        {
                            hours--;
                            minutes = 59;
                        }
                    }
                    else
                    {
                        minutes--;
                    }
                    seconds = 59;
                }
                else seconds--;
                Console.WriteLine($"{hours} : {minutes} : {seconds}");
                dispatcher.Invoke(() => {
                    HoursBox.Text = hours.ToString();
                    MinutesBox.Text = minutes.ToString();
                    SecondsBox.Text = seconds.ToString();
                });
            }

            dispatcher.Invoke(() => {
                HoursBox.Text = hours.ToString();
                MinutesBox.Text = minutes.ToString();
                SecondsBox.Text = seconds.ToString();
            });

            dispatcher.Invoke(() => stopTimer());
            Process.Start("shutdown", "/s /f /t 0");
        }
        void stopTimer()
        {
            timerState = State.Off;
            cancel = true;
            TimerButton.Header = "Start Timer";
        }

        private void TimerButton_Click(object sender, RoutedEventArgs e)
        {
            try { Validate(); }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Time Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (timerState == State.Off)
            {
                timerState = State.On;
                timer = Task.Run(switchOnTimer);
                TimerButton.Header = "Stop Timer";
            }
            else stopTimer();
        }
    }
}
