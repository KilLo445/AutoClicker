using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Net;
using WinForms = System.Windows.Forms;

namespace AutoClicker
{
    public partial class MainWindow : Window
    {
        public MainWindow() { InitializeComponent(); }

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        string clickerVer = "1.0.0";
        string github = "https://github.com/KilLo445/AutoClicker/";
        string latestRelease = "https://github.com/KilLo445/AutoClicker/releases/latest";
        string latestVersion = "https://raw.githubusercontent.com/KilLo445/AutoClicker/main/.github/version.txt";

        // KEY CODES
        // https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        const uint LEFTDOWN = 0x02;
        const uint LEFTUP = 0x04;
        int HOTKEY;

        bool enableClicker = false;
        int clickInterval; // Milli-Seconds between clicks
        int clickDelay; // Delay before starting Auto Clicker in milli-seconds

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                CheckForUpdates();
                VersionText.Text = $"v{clickerVer}";
                HOTKEY = 0x77;
                WinForms.KeysConverter kc = new WinForms.KeysConverter();
                HotKeyBox.Text = kc.ConvertToString(HOTKEY);
                clickInterval = 500;
                CPSBox.Text = clickInterval.ToString();
                clickDelay = 3000;
                DelayBox.Text = clickDelay.ToString();
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void CheckForUpdates()
        {
            try
            {
                Version localVersion = new Version(clickerVer);
                WebClient webClient = new WebClient();
                Version onlineVersion = new Version(webClient.DownloadString(latestVersion));
                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    MessageBoxResult updateAvailableMSG = MessageBox.Show("An update has been found!\n\nWould you like to download it?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (updateAvailableMSG == MessageBoxResult.Yes) { Process.Start(latestRelease); Application.Current.Shutdown(); }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unable to check for updates", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void CPSNumberValidation(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void CPSBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return) { clickInterval = Convert.ToInt32(CPSBox.Text); Keyboard.ClearFocus(); }
            }
            catch (Exception ex) { ErrorMSG(ex); return; }

        }

        private void HotKeyBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                HOTKEY = KeyInterop.VirtualKeyFromKey(e.Key);
                WinForms.KeysConverter kc = new WinForms.KeysConverter();
                HotKeyBox.Text = kc.ConvertToString(HOTKEY);
                Keyboard.ClearFocus();
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void DelayBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return) { clickDelay = Convert.ToInt32(DelayBox.Text); Keyboard.ClearFocus(); }
            }
            catch (Exception ex) { ErrorMSG(ex); return; }

        }

        private void StartBTN_Click(object sender, RoutedEventArgs e) { ClickDelay(); }

        async Task ClickDelay()
        {
            try
            {
                StartBTN.IsEnabled = false;
                ClickStatus.Text = "Waiting";
                ClickStatus.Foreground = Brushes.Orange;
                await Task.Delay(clickDelay);
                StartAutoClicker();
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        async Task StartAutoClicker()
        {
            try
            {
                StartBTN.IsEnabled = false;
                ClickStatus.Text = "Clicking";
                ClickStatus.Foreground = Brushes.Green;
                enableClicker = true;
                while (enableClicker == true)
                {
                    if (GetAsyncKeyState(HOTKEY) < 0)
                    {
                        enableClicker = false;
                        ClickStatus.Text = "Not Clicking";
                        ClickStatus.Foreground = Brushes.Red;
                        StartBTN.IsEnabled = true;
                    }
                    else
                    {
                        mouse_event(LEFTDOWN, 0, 0, 0, IntPtr.Zero);
                        mouse_event(LEFTUP, 0, 0, 0, IntPtr.Zero);
                        await Task.Delay(clickInterval);
                    }
                }
                return;
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
            
        }

        private void ErrorMSG(Exception exception) { MessageBox.Show($"{exception}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

        private void openGitHub_Click(object sender, RoutedEventArgs e){ try { Process.Start(github); } catch (Exception ex) { ErrorMSG(ex); return; } }

        struct Version
        {
            internal static Version zero = new Version(0, 0, 0);

            private short major;
            private short minor;
            private short subMinor;

            internal Version(short _major, short _minor, short _subMinor)
            {
                major = _major;
                minor = _minor;
                subMinor = _subMinor;
            }
            internal Version(string _version)
            {
                string[] versionStrings = _version.Split('.');
                if (versionStrings.Length != 3)
                {
                    major = 0;
                    minor = 0;
                    subMinor = 0;
                    return;
                }

                major = short.Parse(versionStrings[0]);
                minor = short.Parse(versionStrings[1]);
                subMinor = short.Parse(versionStrings[2]);
            }

            internal bool IsDifferentThan(Version _otherVersion)
            {
                if (major != _otherVersion.major)
                {
                    return true;
                }
                else
                {
                    if (minor != _otherVersion.minor)
                    {
                        return true;
                    }
                    else
                    {
                        if (subMinor != _otherVersion.subMinor)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return $"{major}.{minor}.{subMinor}";
            }
        }
    }
}
