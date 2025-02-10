
using CardTableFool.Tables;

namespace CardTableFool.Main
{

    internal class Program
    {
        public enum GlobalSetting
        {
            TourMode,
            CandidatesCount,
            BatchCount,
            SwitchCount,

            HideCards,
            HideDeckCards,
            HideDumpCards,
            SaveCompiled,
            CompressedJson,
            AnimationScale,

            AttackCountCheck,
            AttackTotalCheck,
            TourBatchCount,
            DepthOfFinal,
            ShowSurnames,
			BarAnimFetch
		}


        static public int[] Settings = {0,4,100,10,1,1,1,1,1,100, 1, 1, 200, 0,0,6};

        static string SettingsName = "Settings.txt";
        static public event EventHandler SettingsChanged;
        public static void UpdateSettings()
        {
            if(SettingsChanged!=null)
            SettingsChanged.Invoke(null, null);
        }
        public static void SaveSettings()
        {
            string[] strings = new string[Settings.Length];
            for (int i = 0; i < Settings.Length; i++) 
                strings[i] = (GlobalSetting)i + " = " + Settings[i].ToString();
            
            File.WriteAllLines(SettingsName, strings);
        }
        public static bool GetBoolSetting(GlobalSetting NameOfSetting)
        {
            return Settings[(int)NameOfSetting] != 0;
        }
        public static void SetBoolSetting(GlobalSetting NameOfSetting, bool NewValue)
        {
            Settings[(int)NameOfSetting] = NewValue ? 1 : 0;
        }
        public static int GetIntSetting(GlobalSetting NameOfSetting)
        {
            return Settings[(int)NameOfSetting];
        }
        public static void SetIntSetting(GlobalSetting NameOfSetting, int NewValue)
        {
            Settings[(int)NameOfSetting] = NewValue;
        }
        public static void LoadSettings()
        {
            try
            {
                var S = File.ReadAllLines(SettingsName);

                foreach (string l in S)
                {
                    var Parts = l.Replace(" ", null).Split("=");
                    if (Parts.Count() == 2)
                    {
                        int k;
                        GlobalSetting set;
                        if (int.TryParse(Parts[1], out k) && Enum.TryParse(Parts[0],out set))
                            Settings[(int)set] = k;
                    }
                }
            }
            catch
            {
                SaveSettings();
            }
        }
        public static bool ShouldRestart;
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            LoadSettings();
            ShouldRestart = true;
            while (ShouldRestart)
            {
                ShouldRestart = false;
                ReloadForm();
            }
        }
        public static bool IsTourCurrently;
        public static Form MainForm;
        public static void ReloadForm()
        {
            if (IsTourCurrently = GetBoolSetting(GlobalSetting.TourMode))
                Application.Run(MainForm = new TourForm());
            else
                Application.Run(MainForm = new GameForm());

        }
    }
}
