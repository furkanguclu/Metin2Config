using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace config
{
    public partial class config : Form
    {
        public config()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigurationFileInit();

            InitComponentList();

            InitComponentValue();
        }

        private void InitComponentValue()
        {
            //Display the configuration :

            if (STATICS.allowResolution.Contains(STATICS.currentConfigDict["WIDTH"] + "x" + STATICS.currentConfigDict["HEIGHT"]))
                ResolutionList.Text = STATICS.currentConfigDict["WIDTH"] + "x" + STATICS.currentConfigDict["HEIGHT"];
            else
                ResolutionList.Text = "800x600";

            FrequencyList.Text = STATICS.currentConfigDict["FREQUENCY"];

            GammaList.Text = STATICS.currentConfigDict["GAMMA"];

            if (STATICS.currentConfigDict["SOFTWARE_CURSOR"] == "1")
                CursorButton.Checked = true;

            switch (STATICS.currentConfigDict["VISIBILITY"])
            {
                case "1":
                    FogList.Text = "Kapalı";
                    break;

                case "2":
                    FogList.Text = "ORTA";
                    break;

                case "3":
                    FogList.Text = "Uzak";
                    break;
            }

            switch (STATICS.currentConfigDict["SOFTWARE_TILING"])
            {
                case "0":
                    TNLList.Text = "Auto";
                    break;

                case "1":
                    TNLList.Text = "CPU";
                    break;

                case "2":
                    TNLList.Text = "GPU";
                    break;
            }

            switch (STATICS.currentConfigDict["SHADOW_LEVEL"])
            {
                case "0":
                    ShadowList.Text = "Hiçbiri";
                    break;

                case "1":
                    ShadowList.Text = "Zayıf";
                    break;

                case "3":
                    ShadowList.Text = "Orta";
                    break;

                case "5":
                    ShadowList.Text = "Yüksek";
                    break;
            }

            if (STATICS.currentConfigDict["WINDOWED"] == "0")
                FullScreenModeButton.Checked = true;
            else
                WindowModeButton.Checked = true;

            if (STATICS.currentConfigDict["USE_DEFAULT_IME"] == "0")
                InternalIMEButton.Checked = true;
            else
                ExternalIMEButton.Checked = true;

            BGMBar.Maximum = 5000; // Adjust maximum for BGMBar
            SFXBar.Maximum = 5000; // Adjust maximum for SFXBar

            BGMBar.Value = Convert.ToInt32(Math.Round(Convert.ToDouble(STATICS.currentConfigDict["MUSIC_VOLUME"].Replace(".", ",")) * 1000));
            SFXBar.Value = Convert.ToInt32(Math.Round(Convert.ToDouble(STATICS.currentConfigDict["VOICE_VOLUME"].Replace(".", ",")) * 1000));
        }

        private void InitComponentList()
        {
            //Resolution
            foreach (string item in STATICS.allowResolution)
            {
                string[] result;
                result = Regex.Split(item, "x");

                if (int.Parse(result[0]) <= Screen.PrimaryScreen.Bounds.Width && int.Parse(result[1]) <= Screen.PrimaryScreen.Bounds.Height)
                    ResolutionList.Items.Add(item);
            }

            // Array
            foreach (KeyValuePair<string, List<string>> item in STATICS.configDict)
            {
                switch (item.Key)
                {
                    case "FREQUENCY":
                        foreach (string val in item.Value) { FrequencyList.Items.Add(val); }
                        break;

                    case "VISIBILITY":
                        FogList.Items.Add("Kapalı");//1
                        FogList.Items.Add("Orta");//2
                        FogList.Items.Add("Uzak");//3
                        break;

                    case "SOFTWARE_TILING":
                        TNLList.Items.Add("Auto");//0
                        TNLList.Items.Add("CPU");//1
                        TNLList.Items.Add("GPU");//2
                        break;

                    case "SHADOW_LEVEL":
                        ShadowList.Items.Add("Hiçbiri");//0
                        ShadowList.Items.Add("Zayıf");//1
                        ShadowList.Items.Add("Orta");//3
                        ShadowList.Items.Add("Yüksek");//5
                        break;

                    case "GAMMA":
                        foreach (string val in item.Value) { GammaList.Items.Add(val); }
                        break;
                }
            }
        }

        // Load configuration, or init her.
        private void ConfigurationFileInit()
        {
            // If file doesn't exist
            if (!File.Exists(STATICS.path))
            {
                //Write default configuration
                ConfigurationWriter();
                return;
            }

            //If configuration exist, checking if he isn't corrupted
            if (!ConfigurationFileChecker())
            {
                File.Delete(STATICS.path);
                ConfigurationWriter();
                return;
            }

            // If all is right, we load the configuration
            ConfigurationFileLoader();
        }

        private void ConfigurationFileLoader()
        {
            using (StreamReader file = new StreamReader(STATICS.path))
            {
                string line;
                string[] result;
                while ((line = file.ReadLine()) != null)
                {
                    result = Regex.Split(line, "[\\s\\t]+");
                    if (result.Length > 1)
                    {
                        STATICS.currentConfigDict[result[0]] = result[1];
                    }
                }
            }
        }

        //Check if file isn't corrupted
        private bool ConfigurationFileChecker()
        {
            using (StreamReader file = new StreamReader(STATICS.path))
            {
                string line;
                string[] result;
                while ((line = file.ReadLine()) != null)
                {
                    result = Regex.Split(line, "[\\s\\t]+");
                    if (result.Length > 1)
                    {
                        if ((!STATICS.configDict.ContainsKey(result[0]) || !STATICS.configDict[result[0]].Contains(result[1])) && (result[0] != "") && result[0] != "MUSIC_VOLUME" && result[0] != "VOICE_VOLUME")
                            return false;

                        if (result[0] == "WIDTH")
                        {
                            // Need to check if width is accepted by allowResolution
                            if (!STATICS.allowResolution.Contains(result[1] + "x" + STATICS.currentConfigDict["HEIGHT"]))
                                return false;
                        }

                        if (result[0] == "HEIGHT")
                        {
                            // Need to check if height is accepted by allowResolution
                            if (!STATICS.allowResolution.Contains(STATICS.currentConfigDict["WIDTH"] + "x" + result[1]))
                                return false;
                        }

                        if (result[0] == "VOICE_VOLUME")
                        {
                            double voiceVolume = Convert.ToDouble(result[1].Replace(".", ","));
                            double minVoiceVolume = Convert.ToDouble(STATICS.configDict["VOICE_VOLUME"][0].Replace(".", ","));
                            double maxVoiceVolume = Convert.ToDouble(STATICS.configDict["VOICE_VOLUME"][1].Replace(".", ","));

                            if (voiceVolume < minVoiceVolume || voiceVolume > maxVoiceVolume)
                            {
                                return false;
                            }
                        }

                        if (result[0] == "MUSIC_VOLUME")
                        {
                            double musicVolume = Convert.ToDouble(result[1].Replace(".", ","));
                            double minMusicVolume = Convert.ToDouble(STATICS.configDict["MUSIC_VOLUME"][0].Replace(".", ","));
                            double maxMusicVolume = Convert.ToDouble(STATICS.configDict["MUSIC_VOLUME"][1].Replace(".", ","));

                            if (musicVolume < minMusicVolume || musicVolume > maxMusicVolume)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        // Write configDict in Configuration file
        private void ConfigurationWriter()
        {
            try
            {
                using (StreamWriter file = new StreamWriter(STATICS.path))
                {
                    foreach (KeyValuePair<string, string> item in STATICS.currentConfigDict)
                    {
                        if (item.Key == "MUSIC_VOLUME" || item.Key == "VOICE_VOLUME")
                        {
                            file.WriteLine("{0}\t{1}", item.Key, item.Value.Replace(",", "."));
                        }
                        else
                        {
                            file.WriteLine("{0}\t{1}", item.Key, item.Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Yapılandırma dosyası oluşturulamıyor, programı yönetici olarak başlatmayı deneyin : {0}", e.ToString()));
                Application.Exit();
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            currentConfigUpdater();

            ConfigurationWriter();

            MessageBox.Show("Config Kaydedildi");

            Application.Exit();
        }

        private void currentConfigUpdater()
        {
            // Resolution
            STATICS.currentConfigDict["WIDTH"] = Regex.Split(ResolutionList.Text, "x")[0];
            STATICS.currentConfigDict["HEIGHT"] = Regex.Split(ResolutionList.Text, "x")[1];

            // Frequency
            STATICS.currentConfigDict["FREQUENCY"] = FrequencyList.Text;

            // Gamma
            STATICS.currentConfigDict["GAMMA"] = GammaList.Text;

            // BGM
            STATICS.currentConfigDict["MUSIC_VOLUME"] = (Convert.ToDouble(BGMBar.Value) / 1000).ToString("0.000").Replace(",", ".");

            // SFX
            STATICS.currentConfigDict["VOICE_VOLUME"] = (Convert.ToDouble(SFXBar.Value) / 1000).ToString("0.000").Replace(",", ".");

            // MOUSE
            STATICS.currentConfigDict["SOFTWARE_CURSOR"] = CursorButton.Checked ? "1" : "0";

            // Window
            STATICS.currentConfigDict["WINDOWED"] = WindowModeButton.Checked ? "1" : "0";

            // IME
            STATICS.currentConfigDict["USE_DEFAULT_IME"] = ExternalIMEButton.Checked ? "1" : "0";

            // Fog
            switch (FogList.SelectedIndex)
            {
                case 0:
                    STATICS.currentConfigDict["VISIBILITY"] = "1";
                    break;

                case 1:
                    STATICS.currentConfigDict["VISIBILITY"] = "2";
                    break;

                case 2:
                    STATICS.currentConfigDict["VISIBILITY"] = "3";
                    break;
            }

            // Shadow
            switch (ShadowList.SelectedIndex)
            {
                case 0:
                    STATICS.currentConfigDict["SHADOW_LEVEL"] = "0";
                    break;

                case 1:
                    STATICS.currentConfigDict["SHADOW_LEVEL"] = "1";
                    break;

                case 2:
                    STATICS.currentConfigDict["SHADOW_LEVEL"] = "3";
                    break;

                case 3:
                    STATICS.currentConfigDict["SHADOW_LEVEL"] = "5";
                    break;
            }

            // Tiling
            switch (TNLList.SelectedIndex)
            {
                case 0:
                    STATICS.currentConfigDict["SOFTWARE_TILING"] = "0";
                    break;

                case 1:
                    STATICS.currentConfigDict["SOFTWARE_TILING"] = "1";
                    break;

                case 2:
                    STATICS.currentConfigDict["SOFTWARE_TILING"] = "2";
                    break;
            }
        }

        private void FrequencyLabel_Click(object sender, EventArgs e)
        {
        }

        private void GammaLabel_Click(object sender, EventArgs e)
        {
        }

        private void IMEBox_Enter(object sender, EventArgs e)
        {
        }

        private void FogLabel_Click(object sender, EventArgs e)
        {
        }

        private void SFXBar_Scroll(object sender, EventArgs e)
        {
        }

        private void CursorButton_CheckedChanged(object sender, EventArgs e)
        {

        }
    }

    /****************************************/
    /*               STATICS               */
    /**************************************/

    internal class STATICS
    {
        // Path of your configuration file
        public static string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "metin2.cfg");

        // Default configuration values
        public static Dictionary<string, string> currentConfigDict = new Dictionary<string, string>()
        {
            {"WIDTH",           "800"},
            {"HEIGHT",          "600"},
            {"BPP",             "32"},
            {"FREQUENCY",       "60"},
            {"SOFTWARE_CURSOR", "0"},
            {"OBJECT_CULLING",  "1"},
            {"VISIBILITY",      "3"},
            {"SOFTWARE_TILING", "0"},
            {"SHADOW_LEVEL",    "3"},
            {"MUSIC_VOLUME",    "0"},
            {"VOICE_VOLUME",    "0"},
            {"GAMMA",           "3"},
            {"WINDOWED",        "1"},
            {"USE_DEFAULT_IME", "0"},
        };

        // Configuration ranges
        public static Dictionary<string, List<string>> configDict = new Dictionary<string, List<string>>()
        {
            {"WIDTH", new List<string>(){"800", "1024", "1152", "1176", "1280", "1360", "1366", "1440", "1600", "1680", "1768", "1920"}},
            {"HEIGHT", new List<string>(){"600", "720", "768", "800", "864", "900", "960", "992", "1024", "1027", "1050", "1080", "1200"}},
            {"BPP", new List<string>(){"16", "32"}},
            {"FREQUENCY", new List<string>(){"56", "60", "72", "75"}},
            {"SOFTWARE_CURSOR", new List<string>(){"0", "1"}},
            {"VISIBILITY", new List<string>(){"1", "2", "3"}},
            {"SOFTWARE_TILING", new List<string>(){"0", "1", "2"}},
            {"SHADOW_LEVEL", new List<string>(){"0", "1", "3", "5"}},
            {"MUSIC_VOLUME", new List<string>(){ "0.000", "5.000"}}, // Min & Max
            {"VOICE_VOLUME", new List<string>(){ "0.000", "5.000"}}, // Min & Max
            {"GAMMA", new List<string>(){"0", "1", "2", "3", "4", "5"}},
            {"WINDOWED", new List<string>(){"0", "1"}},
            {"USE_DEFAULT_IME", new List<string>(){"0", "1"}}
        };

        // Allowed resolutions
        public static string[] allowResolution =
        {
            "800x600", "1024x720", "1152x864", "1176x664", "1280x720", "1280x768",
            "1280x800", "1280x960", "1280x1024", "1360x768", "1366x768", "1600x900",
            "1600x1024", "1600x1200", "1680x1050", "1768x992", "1920x1080", "1440x900"
        };
    }
}