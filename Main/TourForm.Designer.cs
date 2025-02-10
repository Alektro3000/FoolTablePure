
using CardTableFool.Forms;

namespace CardTableFool.Main
{
    partial class TourForm
    {

        protected override void InitializeForm() {
			base.InitializeForm();
            InitializeComponent();


            TourResultsSubForm = new ResultForm(false);
            TourResultsSubForm.ShowGameRecord += OnRecallGame;
            TourResultsSubForm.RepeatGameFromRecord += OnRepeatGame;
            TourResultsSubForm.FormBorderStyle = FormBorderStyle.None;
            TourResultsSubForm.TopLevel = false;
            TourResultsSubForm.ShowInTaskbar = false;
            TourResultsSubForm.Location = new Point(0, 32);
            TourResultsSubForm.Size = new Size(ClientSize.Width, ClientSize.Height - 32);
            TourResultsSubForm.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            Controls.Add(TourResultsSubForm);
			int Center = GameGroup.ClientSize.Width/2;
			
			AutoplayButton.Anchor = AnchorStyles.Bottom;
			AutoplayButton.Location = new Point(Center - 10 - ActionButton.Width / 2 - AutoplayButton.Width, 623);

			Undo.Anchor = AnchorStyles.Bottom;
			Undo.Location = new Point(Center + 10 + ActionButton.Width / 2, 623);

			ActionButton.Anchor = AnchorStyles.Bottom;
			ActionButton.Location = new Point(Center-ActionButton.Width/2, 623);

		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			SetUpTourPlayers = new ToolStripMenuItem();
			SetUpPlayers = new ToolStripMenuItem();
			Recompile = new ToolStripMenuItem();
			PreTour = new ToolStripMenuItem();
			PreTourBegin = new ToolStripMenuItem();
			RecallPreTour = new ToolStripMenuItem();
			TheTour = new ToolStripMenuItem();
			BeginTour = new ToolStripMenuItem();
			BackToTour = new ToolStripMenuItem();
			RecallMatches = new ToolStripMenuItem();
			OpenTour = new FolderBrowserDialog();

			Menu.Items.AddRange(new ToolStripItem[] {SetUpTourPlayers, PreTour, TheTour, RecallMatches });

			// 
			// SetUpTourPlayers
			// 
			SetUpTourPlayers.DropDownItems.AddRange(new ToolStripItem[] { SetUpPlayers, Recompile });
			SetUpTourPlayers.Name = "SetUpTourPlayers";
			SetUpTourPlayers.Size = new Size(170, 25);
			SetUpTourPlayers.Text = "Установить Игроков";
			// 
			// SetUpPlayers
			// 
			SetUpPlayers.Name = "SetUpPlayers";
			SetUpPlayers.Size = new Size(248, 30);
			SetUpPlayers.Text = "Установить из папки";
			SetUpPlayers.Click += SetUpPlayers_Click;
			// 
			// Recompile
			// 
			Recompile.Name = "Recompile";
			Recompile.Size = new Size(248, 30);
			Recompile.Text = "Перекомпилировать";
			Recompile.Click += Recompile_Click;
			// 
			// PreTour
			// 
			PreTour.DropDownItems.AddRange(new ToolStripItem[] { PreTourBegin, RecallPreTour });
			PreTour.Name = "PreTour";
			PreTour.Size = new Size(152, 25);
			PreTour.Text = "Отборочный этап";
			// 
			// PreTourBegin
			// 
			PreTourBegin.Name = "PreTourBegin";
			PreTourBegin.Size = new Size(181, 30);
			PreTourBegin.Text = "Провести";
			PreTourBegin.Click += PreTourBegin_Click;
			// 
			// RecallPreTour
			// 
			RecallPreTour.Name = "RecallPreTour";
			RecallPreTour.Size = new Size(181, 30);
			RecallPreTour.Text = "Результаты";
			RecallPreTour.Click += результатыToolStripMenuItem_Click;
			// 
			// TheTour
			// 
			TheTour.DropDownItems.AddRange(new ToolStripItem[] { BeginTour, BackToTour });
			TheTour.Name = "TheTour";
			TheTour.Size = new Size(76, 25);
			TheTour.Text = "Турнир";
			// 
			// BeginTour
			// 
			BeginTour.Name = "BeginTour";
			BeginTour.Size = new Size(173, 30);
			BeginTour.Text = "Начать";
			BeginTour.Click += BeginTour_Click;
			// 
			// BackToTour
			// 
			BackToTour.Name = "BackToTour";
			BackToTour.Size = new Size(173, 30);
			BackToTour.Text = "Вернуться";
			BackToTour.Click += вернутьсяToolStripMenuItem_Click;
			// 
			// RecallMatches
			// 
			RecallMatches.Name = "RecallMatches";
			RecallMatches.Size = new Size(131, 25);
			RecallMatches.Text = "Записи матчей";
			RecallMatches.Click += RecallMatches_Click;
			// 
			// OpenTour
			// 
			OpenTour.AddToRecent = false;
			OpenTour.Description = "Выберите папку с папками исходных кодов игроков";
			OpenTour.UseDescriptionForTitle = true;
		}



		#endregion

        private FolderBrowserDialog OpenTour;
        private ToolStripMenuItem SetUpTourPlayers;
        private ToolStripMenuItem PreTour;
        private ToolStripMenuItem TheTour;
        private ToolStripMenuItem SetUpPlayers;
        private ToolStripMenuItem Recompile;
        private ToolStripMenuItem RecallMatches;
        private ToolStripMenuItem BeginTour;
        private ToolStripMenuItem PreTourBegin;
        private ToolStripMenuItem RecallPreTour;
        private ToolStripMenuItem BackToTour;
	}
}
