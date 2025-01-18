using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Security.RightsManagement;
using System.Threading;

namespace CoRE2_AutoRefereeSystem_Host
{
    public class Settings {
        public Master.GameFormatEnum GameFormat { get; set; } = Master.GameFormatEnum.NONE;

        public int NumRedWins { get; set; } = 0;
        public int NumBlueWins { get; set; } = 0;

        public string Red1TeamName { get; set; } = "";
        public string Red2TeamName { get; set; } = "";
        public string Red3TeamName { get; set; } = "";
        public string Blue1TeamName { get; set; } = "";
        public string Blue2TeamName { get; set; } = "";
        public string Blue3TeamName { get; set; } = "";

        public object Red1ComPort { get; set; } = "";
        public object Red2ComPort { get; set; } = "";
        public object Red3ComPort { get; set; } = "";
        public object Blue1ComPort { get; set; } = "";
        public object Blue2ComPort { get; set; } = "";
        public object Blue3ComPort { get; set; } = "";
    }

    public class SettingsManager {
        // シングルトン
        private static readonly Lazy<SettingsManager> _instance = new Lazy<SettingsManager>(() => new SettingsManager());
        public static SettingsManager Instance => _instance.Value;


        private readonly string settingsFilePath = "Settings.json";
        public bool FileExsits { get; set; } = false;

        private int isBusy = 0;
        public Settings LoadSettings() {
            if (Interlocked.CompareExchange(ref isBusy, 1, 0) != 0) 
                throw new BusyException();

            try {
                if (File.Exists(settingsFilePath)) {
                    FileExsits = true;
                    string json = File.ReadAllText(settingsFilePath);
                    return JsonConvert.DeserializeObject<Settings>(json);
                }
                return new Settings();
            } catch (Exception ex) {
                return new Settings();
            } finally {
                Interlocked.Exchange(ref isBusy, 0);
            }
        }

        public void SaveSettings(Settings settings) {
            if (Interlocked.CompareExchange(ref isBusy, 1, 0) != 0)
                throw new BusyException();

            try {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsFilePath, json);
            } catch (Exception ex) {
                ;
            } finally {
                Interlocked.Exchange(ref isBusy, 0);
            }
        }
    }

    public class NnChSettings {
        public Dictionary<string, int> TeamNodeNo = new Dictionary<string, int> {
             {"", 9999},
            {"[VXGA]VERTEX-Gamma", 2103},
            {"[VXZE]VERTEX-Zeta", 2503},
            {"[FRCI]FRENTE-Cielo", 603},
            {"[FRRO]FRENTE-Rosa", 3503},
            {"[YKHK]YOOKATORE-Hakata", 1303},
            {"[KTTM]KT-tokitama", 1303},
            {"[JKK]jkk女坂", 1503},
            {"[KMOK]KmoKHS-CoRE", 1403},
            {"[RKGR]洛北ギアーズ", 1203},
        };

        public Dictionary<string, int> HostCH = new Dictionary<string, int> {
            {"Red1", 1},
            {"Red2", 3},
            {"Red3", 5},
            {"Blue1", 2},
            {"Blue2", 4},
            {"Blue3", 6}
        };
    }

    public class NnChSettingsManager
    {
        // シングルトン
        private static readonly Lazy<NnChSettingsManager> _instance = new Lazy<NnChSettingsManager>(() => new NnChSettingsManager());
        public static NnChSettingsManager Instance => _instance.Value;


        private readonly string settingsFilePath = "NnChSettings.json";
        public bool FileExsits { get; set; } = false;

        private int isBusy = 0;
        public NnChSettings LoadSettings() {
            if (Interlocked.CompareExchange(ref isBusy, 1, 0) != 0)
                throw new BusyException();

            try {
                if (File.Exists(settingsFilePath)) {
                    FileExsits = true;
                    string json = File.ReadAllText(settingsFilePath);
                    return JsonConvert.DeserializeObject<NnChSettings>(json);
                } else {
                    var settings = new NnChSettings();
                    return settings;
                }
            } catch (Exception ex) {
                return new NnChSettings();
            } finally {
                Interlocked.Exchange(ref isBusy, 0);
            }
        }

        public void SaveSettings(NnChSettings settings) {
            if (Interlocked.CompareExchange(ref isBusy, 1, 0) != 0)
                throw new BusyException();

            try {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsFilePath, json);
            } catch (Exception ex) {
                ;
            } finally {
                Interlocked.Exchange(ref isBusy, 0);
            }
        }
    }

    [Serializable]
    public class BusyException : Exception 
    {
        public BusyException() { }
        public BusyException(string message) : base(message) { }
        public BusyException(string message, Exception inner) : base(message, inner) { } 
        protected BusyException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context
        ) : base(info, context) { }
    }
}
