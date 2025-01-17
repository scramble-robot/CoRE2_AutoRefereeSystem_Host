using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Timers;
using System.Windows;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Windows.Threading;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Configuration;

namespace CoRE2_AutoRefereeSystem_Host
{
    public class Master
    {
        // シングルトン
        private static readonly Lazy<Master> _instance = new Lazy<Master>(() => new Master());
        public static Master Instance => _instance.Value;

        // 出場チーム
        public string[] TeamName = {   "",
                                "[VXGA]VERTEX-Gamma",
                                "[VXZE]VERTEX-Zeta",
                                "[FRCI]FRENTE-Cielo",
                                "[FRRO]FRENTE-Rosa",
                                "[YKHK]YOOKATORE-Hakata",
                                "[KTTM]KT-tokitama",
                                "[JKK]jkk女坂",
                                "[KMOK]KmoKHS-CoRE",
                                "[RKGR]洛北ギアーズ"};

        public Dictionary<string, int> TeamNodeNo = new Dictionary<string, int> {
            {"", 9999},
            {"[VXGA]VERTEX-Gamma", 9999},
            {"[VXZE]VERTEX-Zeta", 9999},
            {"[FRCI]FRENTE-Cielo", 9999},
            {"[FRRO]FRENTE-Rosa", 9999},
            {"[YKHK]YOOKATORE-Hakata", 9999},
            {"[KTTM]KT-tokitama", 9999},
            {"[JKK]jkk女坂", 9999},
            {"[KMOK]KmoKHS-CoRE", 9999},
            {"[VXGA]VERTEX-Gamma", 9999},
            {"[VXZE]VERTEX-Zeta", 9999},
            {"[RKGR]洛北ギアーズ", 9999},
        };

        public Dictionary<string, int> HostCH = new Dictionary<string, int> {
            {"Red1", 1},
            {"Red2", 3},
            {"Red3", 5},
            {"Blue1", 2},
            {"Blue2", 4},
            {"Blue3", 6}
        };

        /***** 試合のルール *******************************************************************************************************/
        public int SettingTimeMin { private set; get; } = 3;
        public int AllianceMtgTimeMin { private set; get; } = 3;
        public int PreSettingTimeMin { private set; get; } = 2;

        public int GameTimeMin { private set; get; } = 5;
        public int MaxHP { private set; get; } = 40;
        public int PreGameTimeMin { private set; get; } = 2;

        public int PreRedMaxHP { private set; get; } = 100;  // 予選の赤（攻撃サイド）のMaxHP

        public int PreBlueMaxHP { private set; get; } = 20;  // 予選の青（迎撃サイド）のMaxHP

        public int HitDamage { private set; get; } = 10;

        public int AttackBuffTime { private set; get; } = 45;

        public int PenaltyDamage { private set; get; } = 10;
        public int RespawnTime { private set; get; } = 45;
        public int RespawnHP { private set; get; } = 30;
        public int InvincibleTime { private set; get; } = 5;


        /***** 各種フラグ *******************************************************************************************************/
        public bool AutoConnect { set; get; } = false;
        public bool GameEndFlag { set; get; } = false;
        public bool DuringGame { private set; get; } = false;

        public bool Added3min { private set; get; } = false;


        /***** enum定義 *******************************************************************************************************/
        #region
        public enum RobotConnectionEnum
        {
            DISABLED,
            ENABLED,
            CONNECTED,
            // DISCONNECTED,
        }

        public enum BaseConnectionEnum
        {
            CONNECTED,
            DISCONNECTED
        }

        public enum TeamColorEnum
        {
            NONE,
            RED,
            GREEN,
            BLUE,
            CYAN,
            MAGENTA,
            YELLOW,
            WHITE,
        };

        public enum GameFormatEnum
        {
            NONE,
            PRELIMINALY,
            SEMIFINALS,
            FINALS
        };

        public enum GameStatusEnum
        {
            NONE,
            SETTING,
            PREGAME,
            GAME,
            POSTGAME,
        };

        public enum SettingStatusEnum
        {
            NONE,
            RUNNING,
            TECH_TIMEOUT,
            RESUME,
            SKIP,
        };

        public enum WinnerEnum
        {
            NONE,
            RED,
            BLUE,
            DRAW,
        };

        public enum ARSSequenceEnum
        {
            NONE,
            HARDWARE_RESET,
            SOFTWARE_RESET,
            OPENED,
            CLOSING,
            SETTING_CH,
            PING,
            BOOTING,
            UPDATING,
            SHUTING_DOWN,
        };

        public enum HPBarColorEnum
        {
            NONE,
            RED,
            GREEN,
            BLUE,
            YELLOW,
            WHITE,
        };

        public enum DamagePanelColorEnum
        {
            NONE,
            RED,
            GREEN,
            BLUE,
            CYAN,
            MAGENTA,
            YELLOW,
            WHITE,
            FULL_WHITE,
        };

        public enum OccupiedEnum
        {
            NO,
            RED,
            BLUE,
        };

        #endregion

        /***** 各種設定 *******************************************************************************************************/
        public GameFormatEnum GameFormat { set; get; } = GameFormatEnum.NONE;
        public GameStatusEnum GameStatus { set; get; } = GameStatusEnum.PREGAME;
        public SettingStatusEnum SettingStatus { set; get; } = SettingStatusEnum.NONE;

        public bool IsUpdatingStatus { set; get; } = false;

        public Settings SettingsJson { set; get; }

        public bool SettingsChanged { set; get; } = false;


        // ARS通信のタイムアウト設定の乱数
        public int TimeoutMin { set; get; } = 3000;
        public int TimeoutMax { set; get; } = 4000;
        public Random ARSTimeoutRandom { set; get; } = new Random();

        /***** 各種試合状況 *******************************************************************************************************/
        public string GameTime { set; get; } = "00:00";
        public string SettingTime { set; get; } = "00:00";
        public TimeSpan CurrentTime { private set; get; } = TimeSpan.Zero;
        public int TotalRedDefeated { set; get; } = 0;
        public int TotalRedDamageTaken { set; get; } = 0;
        public int TotalBlueDefeated { set; get; } = 0;
        public int TotalBlueDamageTaken { set; get; } = 0;


        public int TotalRedEMs {
            private set {; }
            get {
                int num = 0;
                Application.Current.Dispatcher.Invoke(() => {
                    var window = GetMainWindow();
                    if (!window.RedEMSpot1Button.IsEnabled) num++;
                    if (!window.RedEMSpot3Button.IsEnabled) num++;
                });
                return num;
            }
        }

        public int TotalBlueEMs {
            private set {; }
            get {
                int num = 0;
                Application.Current.Dispatcher.Invoke(() => {
                    var window = GetMainWindow();
                    if (!window.BlueEMSpot1Button.IsEnabled) num++;
                    if (!window.BlueEMSpot3Button.IsEnabled) num++;
                });
                return num;
            }
        }

        public int RedAttackBuff { set; get; } = 1;
        public bool IsRedAttackBuff1Active { set; get; } = false;
        public bool IsRedAttackBuff3Active { set; get; } = false;

        public int BlueAttackBuff { set; get; } = 1;
        public bool IsBlueAttackBuff1Active { set; get; } = false;
        public bool IsBlueAttackBuff3Active { set; get; } = false;


        // -5 ~ -1: Red
        // 1 ~ 5: Blue
        public int BaseLOccupationLevel { set; get; } = 0;
        public int BaseCOccupationLevel { set; get; } = 0;
        public int BaseROccupationLevel { set; get; } = 0;

        public WinnerEnum Winner { set; get; } = WinnerEnum.NONE;

        public RobotStatusManager.RobotStatus[] RedRobot { set; get; }
        public RobotStatusManager.RobotStatus[] BlueRobot { set; get; }

        // 操縦画面用プログラムに送信するためのクラス
        public CoreClass Msgs = new CoreClass();

        private const int _port = 12345;
        private UdpClient _udpClient = new UdpClient();

        public static MainWindow GetMainWindow() {
            return Application.Current.MainWindow as MainWindow;
        }

        // サーバー全体の更新
        // 100msの間隔で通信を試みる
        // ただし実際には500ms程度はかかる
        private static System.Timers.Timer _updateTimer;
        public event Action UpdateEvent;

        // 強化素材による攻撃バフの時間の測定用
        public DateTime _redAttackBuff1StartTime;
        public DateTime _redAttackBuff3StartTime;
        public DateTime _blueAttackBuff1StartTime;
        public DateTime _blueAttackBuff3StartTime;

        // 試合時間のタイマー関係
        private static System.Timers.Timer _countDownTimer;
        private static DateTime _startTime;
        private static TimeSpan _remainingTime;
        private static bool _isPaused = false;

        // Settingsのタイマー
        public static System.Timers.Timer _settingsTimer;

        // UDPのタイマー
        public readonly DispatcherTimer _udpTimer;

        private Master() {
            _updateTimer = new System.Timers.Timer();
            _updateTimer.Interval = 100;
            _updateTimer.Elapsed += UpdateAttackBuff;
            _updateTimer.Elapsed += OnEventArrived;
            _updateTimer.Elapsed += AggregateDamage;
            _updateTimer.Elapsed += CheckGameEnd;
            _updateTimer.Start();

            _countDownTimer = new System.Timers.Timer();
            _countDownTimer.Interval = 50;
            _countDownTimer.Elapsed += OnCountDownTimedEvent;

            _settingsTimer = new System.Timers.Timer();
            _settingsTimer.Interval = 1000;
            _settingsTimer.Elapsed += SaveSettings;
            _settingsTimer.Start();

            _udpTimer = new DispatcherTimer();
            _udpTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _udpTimer.Tick += new EventHandler(SendMsgsToOperatorScreen);
            _udpTimer.Start();
        }

        private void OnEventArrived(object sender, EventArgs e) {
            UpdateEvent?.Invoke();
        }

        private void AggregateDamage(object sender, EventArgs e) {
            Application.Current.Dispatcher.Invoke((() => {
                var window = GetMainWindow();

                Instance.TotalRedDamageTaken = (
                    window.Red1.Robot1.Status.DamageTaken
                    + window.Red2.Robot1.Status.DamageTaken
                    + window.Red3.Robot1.Status.DamageTaken
                );

                Instance.TotalBlueDamageTaken = (
                    window.Blue1.Robot1.Status.DamageTaken
                    + window.Blue2.Robot1.Status.DamageTaken
                    + window.Blue3.Robot1.Status.DamageTaken
                );

                Instance.TotalRedDefeated = (
                    window.Red1.Robot1.Status.DefeatedNum
                    + window.Red2.Robot1.Status.DefeatedNum
                    + window.Red3.Robot1.Status.DefeatedNum
                );

                Instance.TotalBlueDefeated = (
                    window.Blue1.Robot1.Status.DefeatedNum
                    + window.Blue2.Robot1.Status.DefeatedNum
                    + window.Blue3.Robot1.Status.DefeatedNum
                );


                window.RedDamageTakenTextBlock.Text = $"{Instance.TotalRedDamageTaken:0000}";
                window.BlueDamageTakenTextBlock.Text = $"{Instance.TotalBlueDamageTaken:0000}";
                window.RedDefeatedTextBlock.Text = $"{Instance.TotalRedDefeated:0000}";
                window.BlueDefeatedTextBlock.Text = $"{Instance.TotalBlueDefeated:0000}";

                window.RedAttackBuffTextBlock.Text = $"x{Instance.RedAttackBuff:0}";
                window.BlueAttackBuffTextBlock.Text = $"x{Instance.BlueAttackBuff:0}";
            }));
        }

        private void UpdateAttackBuff(object sender, EventArgs e) {
            if (Instance.GameFormat == GameFormatEnum.PRELIMINALY) return;

            Application.Current.Dispatcher.Invoke(() => {
                var window = GetMainWindow();
                Instance.RedAttackBuff = 1;
                if (Instance.IsRedAttackBuff1Active) {
                    var timePassed = DateTime.Now - Instance._redAttackBuff1StartTime;
                    var remainingTime = TimeSpan.FromSeconds(Instance.AttackBuffTime) - timePassed;
                    if (remainingTime.TotalSeconds <= 0) {
                        Instance.IsRedAttackBuff1Active = false;
                        window.RedAttackbuff1TimeTextBlock.Text = "30 sec..";
                        window.RedAttackbuff1TimeTextBlock.IsEnabled = false;
                    } else {
                        Instance.RedAttackBuff *= 2;
                        window.RedAttackbuff1TimeTextBlock.Text = $"{remainingTime.TotalSeconds:00} sec..";
                    }
                }

                if (Instance.IsRedAttackBuff3Active) {
                    var timePassed = DateTime.Now - Instance._redAttackBuff3StartTime;
                    var remainingTime = TimeSpan.FromSeconds(Instance.AttackBuffTime) - timePassed;
                    if (remainingTime.TotalSeconds <= 0) {
                        Instance.IsRedAttackBuff3Active = false;
                        window.RedAttackbuff3TimeTextBlock.Text = "30 sec..";
                        window.RedAttackbuff3TimeTextBlock.IsEnabled = false;
                    } else {
                        Instance.RedAttackBuff *= 2;
                        window.RedAttackbuff3TimeTextBlock.Text = $"{remainingTime.TotalSeconds:00} sec..";
                    }
                }

                Instance.BlueAttackBuff = 1;
                if (Instance.IsBlueAttackBuff1Active) {
                    var timePassed = DateTime.Now - Instance._blueAttackBuff1StartTime;
                    var remainingTime = TimeSpan.FromSeconds(Instance.AttackBuffTime) - timePassed;
                    if (remainingTime.TotalSeconds <= 0) {
                        Instance.IsBlueAttackBuff1Active = false;
                        window.BlueAttackbuff1TimeTextBlock.Text = "30 sec..";
                        window.BlueAttackbuff1TimeTextBlock.IsEnabled = false;
                    } else {
                        Instance.BlueAttackBuff *= 2;
                        window.BlueAttackbuff1TimeTextBlock.Text = $"{remainingTime.TotalSeconds:00} sec..";
                    }
                }

                if (Instance.IsBlueAttackBuff3Active) {
                    var timePassed = DateTime.Now - Instance._blueAttackBuff3StartTime;
                    var remainingTime = TimeSpan.FromSeconds(Instance.AttackBuffTime) - timePassed;
                    if (remainingTime.TotalSeconds <= 0) {
                        Instance.IsBlueAttackBuff3Active = false;
                        window.BlueAttackbuff3TimeTextBlock.Text = "30 sec..";
                        window.BlueAttackbuff3TimeTextBlock.IsEnabled = false;
                    } else {
                        Instance.BlueAttackBuff *= 2;
                        window.BlueAttackbuff3TimeTextBlock.Text = $"{remainingTime.TotalSeconds:00} sec..";
                    }
                }
                window.RedAttackBuffTextBlock.Text = $"x{Instance.RedAttackBuff:0}";
                window.BlueAttackBuffTextBlock.Text = $"x{Instance.BlueAttackBuff:0}";
            });
        }

        // 試合時間中に勝敗が決定しているか確認
        private void CheckGameEnd(object sender, EventArgs e) {
            //private void CheckGameEnd() {
            if (!Instance.DuringGame) return;

            // 予選の勝敗条件
            // 攻撃サイドのロボットが撃破される
            // 迎撃サイドのすべてのロボットが撃破される
            // 上記2条件に当てはまらず、2分間が経過する
            Application.Current.Dispatcher.Invoke(() => {
                var window = GetMainWindow();

                if (Instance.GameFormat == GameFormatEnum.PRELIMINALY) {
                    if (window.Red1.Robot1.Status.DefeatedFlag) { // 攻撃サイドが撃破
                        Instance.GameEndFlag = true;
                        Instance.Winner = WinnerEnum.BLUE;
                        window.TimerLabel.Text = "BLUE WINS!!!";
                        Instance.DuringGame = false;
                        Instance.GameStatus = GameStatusEnum.POSTGAME;
                    } else if (window.Blue1.Robot1.Status.DefeatedFlag &&
                               window.Blue2.Robot1.Status.DefeatedFlag &&
                               window.Blue3.Robot1.Status.DefeatedFlag) { // 迎撃サイドがすべて撃破
                        Instance.GameEndFlag = true;
                        Instance.Winner = WinnerEnum.RED;
                        window.TimerLabel.Text = "RED WINS!!!";
                        Instance.DuringGame = false;
                        Instance.GameStatus = GameStatusEnum.POSTGAME;
                    }
                }

                // 準決勝・決勝の勝敗条件
                // 1. 相手操縦ロボットすべてを同時刻に撃破状態とした同盟の勝利
                // 2. 相手操縦ロボットを撃破した回数が多い同盟の勝利
                // 3. 5分経過時にスポットをより多く獲得した同盟の勝利
                // 4. 相手同盟への与ダメージが多い同盟の勝利
                // 5. 以上の条件で決定できない場合、引き分けとなり当該ラウンドは再試合となる

                else if (Instance.GameFormat == GameFormatEnum.SEMIFINALS) {
                      // 条件1
                      if (window.Red1.Robot1.Status.DefeatedFlag &&
                          window.Red2.Robot1.Status.DefeatedFlag) { // 赤同盟がすべて撃破
                        Instance.GameEndFlag = true;
                        Instance.Winner = WinnerEnum.BLUE;
                        window.TimerLabel.Text = "BLUE WINS!!!";
                        Instance.DuringGame = false;
                        Instance.GameStatus = GameStatusEnum.POSTGAME;
                    } else if (window.Blue1.Robot1.Status.DefeatedFlag &&
                                 window.Blue2.Robot1.Status.DefeatedFlag) { // 青同盟がすべて撃破
                        Instance.GameEndFlag = true;
                        Instance.Winner = WinnerEnum.RED;
                        window.TimerLabel.Text = "RED WINS!!!";
                        Instance.DuringGame = false;
                        Instance.GameStatus = GameStatusEnum.POSTGAME;
                    }
                } 
                
                else if (Instance.GameFormat == GameFormatEnum.FINALS) {
                    // 条件1
                    if (window.Red1.Robot1.Status.DefeatedFlag &&
                        window.Red2.Robot1.Status.DefeatedFlag &&
                        window.Red3.Robot1.Status.DefeatedFlag) { // 赤同盟がすべて撃破
                        Instance.GameEndFlag = true;
                        Instance.Winner = WinnerEnum.BLUE;
                        window.TimerLabel.Text = "BLUE WINS!!!";
                        Instance.DuringGame = false;
                        Instance.GameStatus = GameStatusEnum.POSTGAME;
                    } 
                    else if (window.Blue1.Robot1.Status.DefeatedFlag &&
                             window.Blue2.Robot1.Status.DefeatedFlag &&
                             window.Blue3.Robot1.Status.DefeatedFlag) { // 青同盟がすべて撃破
                        Instance.GameEndFlag = true;
                        Instance.Winner = WinnerEnum.RED;
                        window.TimerLabel.Text = "RED WINS!!!";
                        Instance.DuringGame = false;
                        Instance.GameStatus = GameStatusEnum.POSTGAME;
                    }
                }
            });
        }

        public event Action GameStartEvent;
        /// <summary>
        /// GAME STARTボタンが押されると更新タイマーが開始
        /// また，ゲームスタートイベントが発生
        /// </summary>
        public void GameStart() {
            GetMainWindow().TimerLabel.Text = "GAME TIME";
            Instance.GameStatus = GameStatusEnum.GAME;
            Instance.DuringGame = true;
            //_updateTimer.Start();
            int gameTime = Instance.GameTimeMin;
            if (Instance.GameFormat == GameFormatEnum.PRELIMINALY) gameTime = Instance.PreGameTimeMin;
            StartTimer(gameTime * 60 + 5 + 1);
            GameStartEvent?.Invoke();
            AllocateButton();
        }


        public event Action GameResetEvent;
        /// <summary>
        /// GAME RESETボタンが押されると更新タイマーが停止
        /// また，ゲームリセットイベントが発生
        /// </summary>
        public void GameReset() {
            GetMainWindow().TimerLabel.Text = "GAME READY?";
            Instance.GameStatus = GameStatusEnum.PREGAME;
            Instance.DuringGame = false;
            //_updateTimer.Stop();

            int gameTime = Instance.GameTimeMin;
            if (Instance.GameFormat == GameFormatEnum.PRELIMINALY)
                gameTime = Instance.PreGameTimeMin;
            ResetTimer(gameTime * 60);
            GameResetEvent?.Invoke();
            AllocateButton();
        }


        public event Action ClearDataEvent;
        public void ClearData() {
            Instance.DuringGame = false;
            Instance.GameEndFlag = false;
            Added3min = false;
            ClearDataEvent?.Invoke();
        }


        private static void StartTimer(double timeSec) {
            _remainingTime = TimeSpan.FromSeconds(timeSec);
            _startTime = DateTime.Now;
            _countDownTimer.Start();
        }

        private static void PauseTimer() {
            if (_isPaused) return;

            _countDownTimer.Stop();
            _remainingTime -= DateTime.Now - _startTime;
            _isPaused = true;
        }

        private static void ResumeTimer() {
            if (!_isPaused) return;
            _startTime = DateTime.Now;
            _countDownTimer.Start();
            _isPaused = false;
        }

        private static void ResetTimer(int timeSec = 0) {
            _countDownTimer.Stop();

            string timeText = $"{timeSec / 60:D2}:{timeSec % 60:D2}";
            GetMainWindow().GameCountDown.Text = timeText;
            Instance.GameTime = timeText;
            Instance.SettingTime = timeText;
        }

        private static void OnCountDownTimedEvent(object source, ElapsedEventArgs e) {
            Instance.CurrentTime = DateTime.Now - _startTime;
            var currentRemainingTime = _remainingTime - Instance.CurrentTime;

            // 試合終了。勝敗判定
            if (Instance.GameEndFlag || currentRemainingTime.TotalSeconds <= 0) {
                _countDownTimer.Stop();
                if (Instance.GameEndFlag) return;

                Instance.GameStatus += 1;
                if (Instance.GameStatus == GameStatusEnum.POSTGAME
                    && Instance.GameFormat != GameFormatEnum.PRELIMINALY) {

                    // 決勝トーナメントにおけるラウンドの勝敗条件2以降
                    // 条件2 相手操縦ロボットを撃破した回数が多い同盟の勝利
                    if (Instance.TotalRedDefeated != Instance.TotalBlueDefeated) {
                        if (Instance.TotalRedDefeated > Instance.TotalBlueDefeated) {
                            Instance.Winner = WinnerEnum.BLUE;
                            Application.Current.Dispatcher.Invoke(() => {
                                GetMainWindow().TimerLabel.Text = "BLUE WINS!!!";
                            });
                            Instance.DuringGame = false;
                        } else {
                            Instance.Winner = WinnerEnum.RED;
                            Application.Current.Dispatcher.Invoke(() => {
                                GetMainWindow().TimerLabel.Text = "RED WINS!!!";
                            });
                            Instance.DuringGame = false;
                        }
                    }

                    // 条件3 5分経過時にスポットをより多く獲得した同盟の勝利
                    else if (Instance.TotalRedEMs != Instance.TotalBlueEMs) {
                        if (Instance.TotalRedEMs > Instance.TotalBlueEMs) {
                            Instance.Winner = WinnerEnum.RED;
                            Application.Current.Dispatcher.Invoke(() => {
                                GetMainWindow().TimerLabel.Text = "RED WINS!!!";
                            });
                            Instance.DuringGame = false;
                        } else {
                            Instance.Winner = WinnerEnum.BLUE;
                            Application.Current.Dispatcher.Invoke(() => {
                                GetMainWindow().TimerLabel.Text = "BLUE WINS!!!";
                            });
                            Instance.DuringGame = false;
                        }
                    }

                    // 条件4 相手同盟への与ダメージが多い同盟の勝利
                    else if (Instance.TotalRedDamageTaken != Instance.TotalBlueDamageTaken) {
                        if (Instance.TotalRedDamageTaken > Instance.TotalBlueDamageTaken) {
                            Instance.Winner = WinnerEnum.BLUE;
                            Application.Current.Dispatcher.Invoke(() => {
                                GetMainWindow().TimerLabel.Text = "BLUE WINS!!!";
                            });
                            Instance.DuringGame = false;
                        } else {
                            Instance.Winner = WinnerEnum.RED;
                            Application.Current.Dispatcher.Invoke(() => {
                                GetMainWindow().TimerLabel.Text = "RED WINS!!!";
                            });
                            Instance.DuringGame = false;
                        }
                    }

                    // 勝敗条件を満たさない
                    else {
                        Instance.Winner = WinnerEnum.DRAW;
                        Application.Current.Dispatcher.Invoke(() => {
                            GetMainWindow().TimerLabel.Text = "DRAW!!!";
                        });
                        Instance.DuringGame = false;
                    }
                }

                AllocateButton();
                Instance.DuringGame = false;
                return;
            }

            string timeText = $"{currentRemainingTime.Minutes:00}:{currentRemainingTime.Seconds:00}";
            Application.Current.Dispatcher.Invoke(() => {
                GetMainWindow().GameCountDown.Text = timeText;
            });
            Instance.GameTime = timeText;
            Instance.SettingTime = timeText;
        }

        private static void AllocateButton() {
            Application.Current.Dispatcher.Invoke((Delegate)(() => {
                var window = GetMainWindow();
                if (Instance.GameStatus == GameStatusEnum.NONE
                    || Instance.GameStatus == GameStatusEnum.POSTGAME) {
                    window.PreliminaryRadioButton.IsEnabled = true;
                    window.GameStartButton.IsEnabled = true;
                    window.GameResetButton.IsEnabled = false;
                } else if (Instance.GameStatus == GameStatusEnum.SETTING) {
                    if (Instance.SettingStatus == SettingStatusEnum.RUNNING) {
                        window.PreliminaryRadioButton.IsEnabled = false;
                        window.GameStartButton.IsEnabled = false;
                        window.GameResetButton.IsEnabled = false;
                    } else if (Instance.SettingStatus == SettingStatusEnum.TECH_TIMEOUT) {
                        window.PreliminaryRadioButton.IsEnabled = false;
                        window.GameStartButton.IsEnabled = false;
                        window.GameResetButton.IsEnabled = false;
                    }
                } else if (Instance.GameStatus == GameStatusEnum.PREGAME) {
                    window.PreliminaryRadioButton.IsEnabled = false;
                    window.GameStartButton.IsEnabled = true;
                    window.GameResetButton.IsEnabled = false;
                } else if (Instance.GameStatus == GameStatusEnum.GAME) {
                    window.PreliminaryRadioButton.IsEnabled = false;
                    window.GameStartButton.IsEnabled = false;
                    window.GameResetButton.IsEnabled = true;
                }
            }));
        }


        private static void SaveSettings(object sender, EventArgs e) {
            if (!Instance.SettingsChanged) return;

            Settings settings = new Settings();
            settings.GameFormat = Instance.GameFormat;

            // HACK
            if (Instance.SettingsJson is null) {
                settings.NumRedWins = 0;
                settings.NumBlueWins = 0;
            } else {
                settings.NumRedWins = Instance.SettingsJson.NumRedWins;
                settings.NumBlueWins = Instance.SettingsJson.NumBlueWins;
            }

            Application.Current.Dispatcher.Invoke((Delegate)(() => {
                var window = GetMainWindow();
                settings.Red1TeamName = window.Red1.Robot1.Status.TeamName;
                settings.Red2TeamName = window.Red2.Robot1.Status.TeamName;
                settings.Red3TeamName = window.Red3.Robot1.Status.TeamName;
                settings.Blue1TeamName = window.Blue1.Robot1.Status.TeamName;
                settings.Blue2TeamName = window.Blue2.Robot1.Status.TeamName;
                settings.Blue3TeamName = window.Blue3.Robot1.Status.TeamName;
                settings.Red1ComPort = window.Red1.ComPortSelectionComboBox.SelectedItem;
                settings.Red2ComPort = window.Red2.ComPortSelectionComboBox.SelectedItem;
                settings.Red3ComPort = window.Red3.ComPortSelectionComboBox.SelectedItem;
                settings.Blue1ComPort = window.Blue1.ComPortSelectionComboBox.SelectedItem;
                settings.Blue2ComPort = window.Blue2.ComPortSelectionComboBox.SelectedItem;
                settings.Blue3ComPort = window.Blue3.ComPortSelectionComboBox.SelectedItem;
            }));

            try {
                SettingsManager.Instance.SaveSettings(settings);
            } catch (BusyException ex) {
                ;
            } catch (Exception ex) {
                ;
            }

            Instance.SettingsChanged = false;
        }

        private int isSending = 0;
        private async void SendMsgsToOperatorScreen(object sender, EventArgs e) {
            if (Interlocked.CompareExchange(ref isSending, 1, 0) != 0) return;

            // 指定のクラスにセット
            Msgs.GameTime = Instance.GameTime;
            Msgs.GameSystem = (int)Instance.GameFormat;
            Msgs.RedDeathCnt = Instance.TotalRedDefeated;
            Msgs.BlueDeathCnt = Instance.TotalBlueDefeated;
            Msgs.RedReceivedDamage = Instance.TotalRedDamageTaken;
            Msgs.BlueReceivedDamage = Instance.TotalBlueDamageTaken;

            // 強化素材
            Msgs.RedSpot[0] = Bool2Int(Instance.IsRedAttackBuff1Active);
            Msgs.RedSpot[1] = 0; // 今回は無し
            Msgs.RedSpot[2] = Bool2Int(Instance.IsRedAttackBuff3Active);

            Msgs.BlueSpot[0] = Bool2Int(Instance.IsBlueAttackBuff1Active);
            Msgs.BlueSpot[1] = 0; // 今回は無し
            Msgs.BlueSpot[2] = Bool2Int(Instance.IsBlueAttackBuff3Active);


            // 陣地。今回は無し
            Msgs.RedArea = 10 - (Master.Instance.BaseLOccupationLevel + 5);
            Msgs.CenterArea = 10 - (Master.Instance.BaseCOccupationLevel + 5);
            Msgs.BlueArea = 10 - (Master.Instance.BaseROccupationLevel + 5);

            // 試合結果
            Msgs.RedWin = (uint)Master.Instance.SettingsJson.NumRedWins;
            Msgs.BlueWin = (uint)Master.Instance.SettingsJson.NumBlueWins;
            Msgs.Winner = (uint)Instance.Winner;

            // 自動ロボット。今回は無し
            var window = GetMainWindow();
            RobotClass redAutoRobot = new RobotClass();
            RobotClass blueAutoRobot = new RobotClass();
            redAutoRobot.TeamColor = "Red6"; redAutoRobot.TeamID = 0;
            blueAutoRobot.TeamColor = "Blue6"; blueAutoRobot.TeamID = 0;


            RobotStatusManager.RobotStatus[] AllRobotStatus = {
                window.Red1.Robot1.Status,
                window.Red2.Robot1.Status,
                window.Red3.Robot1.Status,

                // dummy
                window.Red3.Robot1.Status,
                window.Red3.Robot1.Status,

                window.Blue1.Robot1.Status,
                window.Blue2.Robot1.Status,
                window.Blue3.Robot1.Status,

                // dummy
                window.Blue3.Robot1.Status,
                window.Blue3.Robot1.Status,
            };

            for (int i = 0; i < AllRobotStatus.Length; i++) {
                Msgs.Robot[i].TeamID = AllRobotStatus[i].TeamID;
                Msgs.Robot[i].TeamColor = AllRobotStatus[i].TeamColor;
                Msgs.Robot[i].HP = AllRobotStatus[i].HP;
                Msgs.Robot[i].MaxHP = AllRobotStatus[i].MaxHP;
                if (Instance.GameFormat == GameFormatEnum.PRELIMINALY && AllRobotStatus[i].DefeatedFlag)
                    Msgs.Robot[i].DeathFlag = 2;
                else if (Instance.GameFormat != GameFormatEnum.PRELIMINALY && AllRobotStatus[i].DefeatedFlag)
                    Msgs.Robot[i].DeathFlag = 1;
                else
                    Msgs.Robot[i].DeathFlag = 0;

                Msgs.Robot[i].RespawnTime = AllRobotStatus[i].RespawnTimeString;
            }

            Msgs.Robot[10] = redAutoRobot;
            Msgs.Robot[11] = blueAutoRobot;

            // UDPでデータを送信
            try {
                string json = JsonConvert.SerializeObject(Msgs);
                byte[] data = Encoding.UTF8.GetBytes(json);

                await _udpClient.SendAsync(data, data.Length, "192.168.100.100", _port);
            } catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
            }

            try {
                string json = JsonConvert.SerializeObject(Msgs);
                byte[] data = Encoding.UTF8.GetBytes(json);

                await _udpClient.SendAsync(data, data.Length, "192.168.100.101", _port);
            } catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
            }

            try {
                string json = JsonConvert.SerializeObject(Msgs);
                byte[] data = Encoding.UTF8.GetBytes(json);

                await _udpClient.SendAsync(data, data.Length, "192.168.100.102", _port);
            } catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
            }

            try {
                string json = JsonConvert.SerializeObject(Msgs);
                byte[] data = Encoding.UTF8.GetBytes(json);

                await _udpClient.SendAsync(data, data.Length, "192.168.100.103", _port);
            } catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
            }

            Interlocked.Exchange(ref isSending, 0);
        }

        private int Bool2Int(bool value) {
            return value ? 1 : 0;
        }
    }
}
