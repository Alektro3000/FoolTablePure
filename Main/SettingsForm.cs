
namespace CardTableFool.Main
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            Tooltip.SetToolTip(TourMode, "Будет ли форма запущена в режиме турнира(Требуется перезагрузка)");
            
            Tooltip.SetToolTip(StrictCheck, "Будет ли стол проверять, чтобы игрок не подкинул больше карт чем есть у оппонента");
            Tooltip.SetToolTip(AttackTotal, "Будет ли стол проверять, чтобы игрок не подкинул больше 6 карт");
            Tooltip.SetToolTip(HideDeck, "В режиме игры с человеком спрятать стопку добора (требуется начать новую игру)");
            Tooltip.SetToolTip(HideDump, "В режиме игры с человеком спрятать стопку сброса (требуется начать новую игру)");
            Tooltip.SetToolTip(HideCards, "В режиме игры с человеком спрятать карты в руке оппонента (требуется начать новую игру)");
            
            Tooltip.SetToolTip(SaveCompiled, "Будет ли компиляция оставлять Compiled.dll для быстрой последующей загрузки");
            Tooltip.SetToolTip(Json, "Будет ли сохранение в json сокращать SCard и SCardPair");

            Tooltip.SetToolTip(BatchCount, "Количество игр в пачке");
            Tooltip.SetToolTip(Switch, "Количество игр перед тем как меняется очередность игроков");
            Tooltip.SetToolTip(Candidate, "Количество игроков, которые будут автоматически выбраны после отбора");
            Tooltip.SetToolTip(Animation, "Множитель скорости всех анимаций");


            TourMode.Checked = Program.GetBoolSetting(Program.GlobalSetting.TourMode);

            StrictCheck.Checked = Program.GetBoolSetting(Program.GlobalSetting.AttackCountCheck);
            AttackTotal.Checked = Program.GetBoolSetting(Program.GlobalSetting.AttackTotalCheck);
            HideCards.Checked = Program.GetBoolSetting(Program.GlobalSetting.HideCards);
            HideDeck.Checked = Program.GetBoolSetting(Program.GlobalSetting.HideDeckCards);
            HideDump.Checked = Program.GetBoolSetting(Program.GlobalSetting.HideDumpCards);

            SaveCompiled.Checked = Program.GetBoolSetting(Program.GlobalSetting.SaveCompiled);
            Json.Checked = Program.GetBoolSetting(Program.GlobalSetting.CompressedJson);
            Surnames.Checked = Program.GetBoolSetting(Program.GlobalSetting.ShowSurnames);

            BatchCount.Text = Program.GetIntSetting(Program.GlobalSetting.BatchCount).ToString();
            TourCount.Text = Program.GetIntSetting(Program.GlobalSetting.TourBatchCount).ToString();
            Switch.Text = Program.GetIntSetting(Program.GlobalSetting.SwitchCount).ToString();
            Candidate.Text = Program.GetIntSetting(Program.GlobalSetting.CandidatesCount).ToString();
            Animation.Text = Program.GetIntSetting(Program.GlobalSetting.AnimationScale).ToString();
            TableDepth.Text = Program.GetIntSetting(Program.GlobalSetting.DepthOfFinal).ToString();
        }

        private void Load_Click(object sender, EventArgs e)
        {
            Program.SetBoolSetting(Program.GlobalSetting.TourMode,TourMode.Checked);
            Program.SetBoolSetting(Program.GlobalSetting.AttackCountCheck, StrictCheck.Checked);
            Program.SetBoolSetting(Program.GlobalSetting.AttackTotalCheck, AttackTotal.Checked);
            Program.SetBoolSetting(Program.GlobalSetting.HideCards, HideCards.Checked);
            Program.SetBoolSetting(Program.GlobalSetting.HideDeckCards, HideDeck.Checked);
            Program.SetBoolSetting(Program.GlobalSetting.HideDumpCards, HideDump.Checked);
            Program.SetBoolSetting(Program.GlobalSetting.SaveCompiled, SaveCompiled.Checked);
            Program.SetBoolSetting(Program.GlobalSetting.CompressedJson, Json.Checked);
            Program.SetBoolSetting(Program.GlobalSetting.ShowSurnames, Surnames.Checked);

            int k;
            if(int.TryParse(BatchCount.Text, out k))
                Program.SetIntSetting(Program.GlobalSetting.BatchCount,k);
            if (int.TryParse(TourCount.Text, out k))
                Program.SetIntSetting(Program.GlobalSetting.TourBatchCount, k);
            if (int.TryParse(Switch.Text, out k))
                Program.SetIntSetting(Program.GlobalSetting.SwitchCount, k);
            if (int.TryParse(Candidate.Text, out k))
                Program.SetIntSetting(Program.GlobalSetting.CandidatesCount, k);
            if (int.TryParse(Animation.Text, out k))
                Program.SetIntSetting(Program.GlobalSetting.AnimationScale, k);
            if (int.TryParse(TableDepth.Text, out k))
                Program.SetIntSetting(Program.GlobalSetting.DepthOfFinal, k);


            Program.UpdateSettings();
            Program.SaveSettings();


            
            if (Program.IsTourCurrently != Program.GetBoolSetting(Program.GlobalSetting.TourMode))
            {
                Program.ShouldRestart = true;
                this.Close();
                Program.MainForm.Close();
                return;
            }
            
            this.Close();
        }
    }
}
