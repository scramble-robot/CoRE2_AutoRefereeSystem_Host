using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
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


namespace CoRE2_AutoRefereeSystem_Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            GameStartButton.IsEnabled = false;
            Settings settings = SettingsManager.Instance.LoadSettings();

            if (settings.GameFormat == Master.GameFormatEnum.PRELIMINALY)
                PreliminaryRadioButton.IsChecked = true;
            else if (settings.GameFormat == Master.GameFormatEnum.SEMIFINALS)
                SemifinalsRadioButton.IsChecked = true;
            else
                FinalsRadioButton.IsChecked = true;
            
            Red1.Robot1.TeamNameComboBox.SelectedItem = settings.Red1TeamName;
            Red2.Robot1.TeamNameComboBox.SelectedItem = settings.Red2TeamName;
            Red3.Robot1.TeamNameComboBox.SelectedItem = settings.Red3TeamName;
            Blue1.Robot1.TeamNameComboBox.SelectedItem = settings.Blue1TeamName;
            Blue2.Robot1.TeamNameComboBox.SelectedItem = settings.Blue2TeamName;
            Blue3.Robot1.TeamNameComboBox.SelectedItem = settings.Blue3TeamName;

            Red1.ComPortSelectionComboBox.SelectedItem = settings.Red1ComPort;
            Red2.ComPortSelectionComboBox.SelectedItem = settings.Red2ComPort;
            Red3.ComPortSelectionComboBox.SelectedItem = settings.Red3ComPort;
            Blue1.ComPortSelectionComboBox.SelectedItem = settings.Blue1ComPort;
            Blue2.ComPortSelectionComboBox.SelectedItem= settings.Blue2ComPort;
            Blue3.ComPortSelectionComboBox.SelectedItem= settings.Blue3ComPort;

            Master.Instance.SettingsJson = settings;

            NnChSettings nnChSettings = NnChSettingsManager.Instance.LoadSettings();
            NnChSettingsManager.Instance.SaveSettings(nnChSettings);
            Master.Instance.TeamNodeNo = nnChSettings.TeamNodeNo;
            Master.Instance.HostCH = nnChSettings.HostCH;
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            bool allHostShutdown = (
                !Red1.ShutdownButton.IsEnabled
                && !Red2.ShutdownButton.IsEnabled
                && !Red3.ShutdownButton.IsEnabled
                && !Blue1.ShutdownButton.IsEnabled
                && !Blue2.ShutdownButton.IsEnabled
                && !Blue3.ShutdownButton.IsEnabled
            );

            if (!allHostShutdown) {
                MessageBox.Show("You must excecute 'shutdown' command on all booted HostPCBs.",
                    "Failed to terminate CoRE-2 2024 Host program", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }
        }

        private void GameStartButton_Click(object sender, RoutedEventArgs e) {
            Master.Instance.GameStart();
        }

        private void GameResetButton_Click(object sender, RoutedEventArgs e) {
            Master.Instance.GameReset();
        }


        private void PreliminaryRadioButton_Checked(object sender, RoutedEventArgs e) {
            Master.Instance.GameFormat = Master.GameFormatEnum.PRELIMINALY;
            Master.Instance.SettingsChanged = true;

            AllControlPanelDisabled();

            Red1.IsEnabled = true;
            Red1.Robot1.IsEnabled = true;
            Red1.CommEnabledToggleButton1.IsEnabled = true;
            Red1.CommEnabledToggleButton1.IsChecked = true;

            Blue1.IsEnabled = true;
            Blue1.Robot1.IsEnabled = true;
            Blue1.CommEnabledToggleButton1.IsEnabled = true;
            Blue1.CommEnabledToggleButton1.IsChecked = true;

            Blue2.IsEnabled = true;
            Blue2.Robot1.IsEnabled = true;
            Blue2.CommEnabledToggleButton1.IsEnabled = true;
            Blue2.CommEnabledToggleButton1.IsChecked = true;

            Blue3.IsEnabled = true;
            Blue3.Robot1.IsEnabled = true;
            Blue3.CommEnabledToggleButton1.IsEnabled = true;
            Blue3.CommEnabledToggleButton1.IsChecked = true;

            RedTeamEMStackPanel.IsEnabled = false;
            BlueTeamEMStackPanel.IsEnabled = false;

            PreliminaryRadioButton.IsEnabled = true;
            GameStartButton.IsEnabled = true;
            GameResetButton.IsEnabled = false;

            Master.Instance.GameStatus = Master.GameStatusEnum.PREGAME;
            int time = Master.Instance.PreGameTimeMin;
            string timeText = $"{time:D2}:00";
            GameCountDown.Text = timeText;
            Master.Instance.GameTime = timeText;
            Master.Instance.SettingTime = timeText;
        }

        private void SemifinalsRadioButton_Checked(object sender, RoutedEventArgs e) {
            Master.Instance.GameFormat = Master.GameFormatEnum.SEMIFINALS;
            Master.Instance.SettingsChanged = true;

            AllControlPanelDisabled();

            Red1.IsEnabled = true;
            Red1.Robot1.IsEnabled = true;
            Red1.CommEnabledToggleButton1.IsEnabled = true;
            Red1.CommEnabledToggleButton1.IsChecked = true;

            Red2.IsEnabled = true;
            Red2.Robot1.IsEnabled = true;
            Red2.CommEnabledToggleButton1.IsEnabled = true;
            Red2.CommEnabledToggleButton1.IsChecked = true;

            Blue1.IsEnabled = true;
            Blue1.Robot1.IsEnabled = true;
            Blue1.CommEnabledToggleButton1.IsEnabled = true;
            Blue1.CommEnabledToggleButton1.IsChecked = true;

            Blue2.IsEnabled = true;
            Blue2.Robot1.IsEnabled = true;
            Blue2.CommEnabledToggleButton1.IsEnabled = true;
            Blue2.CommEnabledToggleButton1.IsChecked = true;

            RedTeamEMStackPanel.IsEnabled = true;
            BlueTeamEMStackPanel.IsEnabled = true;

            PreliminaryRadioButton.IsEnabled = true;
            GameStartButton.IsEnabled = true;
            GameResetButton.IsEnabled = false;

            Master.Instance.GameStatus = Master.GameStatusEnum.PREGAME;
            int time = Master.Instance.GameTimeMin;
            string timeText = $"{time:D2}:00";
            GameCountDown.Text = timeText;
            Master.Instance.GameTime = timeText;
            Master.Instance.SettingTime = timeText;
        }

        private void FinalsRadioButton_Checked(object sender, RoutedEventArgs e) {
            Master.Instance.GameFormat = Master.GameFormatEnum.FINALS;
            Master.Instance.SettingsChanged = true;

            AllControlPanelDisabled();

            Red1.IsEnabled = true;
            Red1.Robot1.IsEnabled = true;
            Red1.CommEnabledToggleButton1.IsEnabled = true;
            Red1.CommEnabledToggleButton1.IsChecked = true;

            Red2.IsEnabled = true;
            Red2.Robot1.IsEnabled = true;
            Red2.CommEnabledToggleButton1.IsEnabled = true;
            Red2.CommEnabledToggleButton1.IsChecked = true;

            Red3.IsEnabled = true;
            Red3.Robot1.IsEnabled = true;
            Red3.CommEnabledToggleButton1.IsEnabled = true;
            Red3.CommEnabledToggleButton1.IsChecked = true;

            Blue1.IsEnabled = true;
            Blue1.Robot1.IsEnabled = true;
            Blue1.CommEnabledToggleButton1.IsEnabled = true;
            Blue1.CommEnabledToggleButton1.IsChecked = true;

            Blue2.IsEnabled = true;
            Blue2.Robot1.IsEnabled = true;
            Blue2.CommEnabledToggleButton1.IsEnabled = true;
            Blue2.CommEnabledToggleButton1.IsChecked = true;

            Blue3.IsEnabled = true;
            Blue3.Robot1.IsEnabled = true;
            Blue3.CommEnabledToggleButton1.IsEnabled = true;
            Blue3.CommEnabledToggleButton1.IsChecked = true;

            RedTeamEMStackPanel.IsEnabled = true;
            BlueTeamEMStackPanel.IsEnabled = true;

            PreliminaryRadioButton.IsEnabled = true;
            GameStartButton.IsEnabled = true;
            GameResetButton.IsEnabled = false;

            Master.Instance.GameStatus = Master.GameStatusEnum.PREGAME;
            int time = Master.Instance.GameTimeMin;
            string timeText = $"{time:D2}:00";
            GameCountDown.Text = timeText;
            Master.Instance.GameTime = timeText;
            Master.Instance.SettingTime = timeText;
        }

        private void AllControlPanelDisabled() {
            // Red1
            Red1.IsEnabled = false;
            Red1.Robot1.IsEnabled = false;
            Red1.CommEnabledToggleButton1.IsEnabled = false;
            Red1.CommEnabledToggleButton1.IsChecked = false;

            // Red2
            Red2.IsEnabled = false;
            Red2.Robot1.IsEnabled = false;
            Red2.CommEnabledToggleButton1.IsEnabled = false;
            Red2.CommEnabledToggleButton1.IsChecked = false;

            // Red3
            Red3.IsEnabled = false;
            Red3.Robot1.IsEnabled = false;
            Red3.CommEnabledToggleButton1.IsEnabled = false;
            Red3.CommEnabledToggleButton1.IsChecked = false;

            // Blue1
            Blue1.IsEnabled = false;
            Blue1.Robot1.IsEnabled = false;
            Blue1.CommEnabledToggleButton1.IsEnabled = false;
            Blue1.CommEnabledToggleButton1.IsChecked = false;

            // Blue2
            Blue2.IsEnabled = false;
            Blue2.Robot1.IsEnabled = false;
            Blue2.CommEnabledToggleButton1.IsEnabled = false;
            Blue2.CommEnabledToggleButton1.IsChecked = false;

            // Blue3
            Blue3.IsEnabled = false;
            Blue3.Robot1.IsEnabled = false;
            Blue3.CommEnabledToggleButton1.IsEnabled = false;
            Blue3.CommEnabledToggleButton1.IsChecked = false;
        }


        private void RedEMSpot1Button_Click(object sender, RoutedEventArgs e) {
            Master.Instance.IsRedAttackBuff1Active = true;
            Master.Instance._redAttackBuff1StartTime = DateTime.Now;
            RedAttackbuff1TimeTextBlock.IsEnabled = true;
            RedEMSpot1Button.IsEnabled = false;
        }

        private void RedEMSpot3Button_Click(object sender, RoutedEventArgs e) {
            Master.Instance.IsRedAttackBuff3Active = true;
            Master.Instance._redAttackBuff3StartTime = DateTime.Now;
            RedAttackbuff3TimeTextBlock.IsEnabled = true;
            RedEMSpot3Button.IsEnabled = false;
        }

        private void BlueEMSpot1Button_Click(object sender, RoutedEventArgs e) {
            Master.Instance.IsBlueAttackBuff1Active = true;
            Master.Instance._blueAttackBuff1StartTime = DateTime.Now;
            BlueAttackbuff1TimeTextBlock.IsEnabled = true;
            BlueEMSpot1Button.IsEnabled = false;
        }


        private void BlueEMSpot3Button_Click(object sender, RoutedEventArgs e) {
            Master.Instance.IsBlueAttackBuff3Active = true;
            Master.Instance._blueAttackBuff3StartTime = DateTime.Now;
            BlueAttackbuff3TimeTextBlock.IsEnabled = true;
            BlueEMSpot3Button.IsEnabled = false;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (!Master.Instance.DuringGame) return;

            // ダメージ
            /*if (e.Key == Key.D1)
                Red12.Robot1.PunishButton_Click(sender, new RoutedEventArgs());

            else if (e.Key == Key.D2)
                Red12.Robot2.PunishButton_Click(sender, new RoutedEventArgs());

            else if (e.Key == Key.D3)
                Red34.Robot1.PunishButton_Click(sender, new RoutedEventArgs());

            else if (e.Key == Key.D4)
                Red34.Robot2.PunishButton_Click(sender, new RoutedEventArgs());

            else if (e.Key == Key.D5)
                Red5.Robot1.PunishButton_Click(sender, new RoutedEventArgs());

            if (e.Key == Key.D6)
                Blue12.Robot1.PunishButton_Click(sender, new RoutedEventArgs());

            else if (e.Key == Key.D7)
                Blue12.Robot2.PunishButton_Click(sender, new RoutedEventArgs());

            else if (e.Key == Key.D8)
                Blue34.Robot1.PunishButton_Click(sender, new RoutedEventArgs());

            else if (e.Key == Key.D9)
                Blue34.Robot2.PunishButton_Click(sender, new RoutedEventArgs());

            else if (e.Key == Key.D0)
                Blue5.Robot1.PunishButton_Click(sender, new RoutedEventArgs());

            // 陣地
            if (e.Key == Key.A)
                BaseL.RedLevelButton_Click(sender, new RoutedEventArgs());
            else if (e.Key == Key.S)
                BaseL.NeurtralButton_Click(sender, new RoutedEventArgs());
            else if (e.Key == Key.D)
                BaseL.BlueLevelButton_Click(sender, new RoutedEventArgs());

            if (e.Key == Key.F)
                BaseC.RedLevelButton_Click(sender, new RoutedEventArgs());
            else if (e.Key == Key.G)
                BaseC.NeurtralButton_Click(sender, new RoutedEventArgs());
            else if (e.Key == Key.H)
                BaseC.BlueLevelButton_Click(sender, new RoutedEventArgs());

            if (e.Key == Key.J)
                BaseR.RedLevelButton_Click(sender, new RoutedEventArgs());
            else if (e.Key == Key.K)
                BaseR.NeurtralButton_Click(sender, new RoutedEventArgs());
            else if (e.Key == Key.L)
                BaseR.BlueLevelButton_Click(sender, new RoutedEventArgs());
            */
        }
    }
}
