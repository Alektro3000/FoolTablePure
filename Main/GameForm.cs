using CardTableFool.Players;
using CardTableFool.Tables;
using System.Text.Json;

namespace CardTableFool.Main
{

    //Основной класс ответственный за соединение всего вместе
    public partial class GameForm : MainForm
    {
        public List<BatchRecord> Records = [];

        ICardPlayer ICardPlayer1;
        ICardPlayer ICardPlayer2;

        public GameForm() : base()
        {
            ICardPlayer1 = new MSimplePlayer("Первый");
            ICardPlayer2 = new MSimplePlayer("Второй");

            MLoggingTable mTable = new MLoggingTable(ICardPlayer1, ICardPlayer2);

            ResetWorld(mTable);

            toolTip1.SetToolTip(this.Undo, "Не работает для недетерминированных ботов");

        }
        protected override void OnRecallGame(object sender, GameEventArgs e)
        {
            ResetWorld(new MPlaybackTable(e.Record));
            SwitchToGame();
        }
		protected override void OnRepeatGame(object sender, GameEventArgs e)
        {
            ICardPlayer1.Reset();
            ICardPlayer2.Reset();
            ResetWorld(new MLoggingTable(e.Record, ICardPlayer1, ICardPlayer2));
            SwitchToGame();
        }
        //Results = 1
        //Game = 2
        int IsShowingGame = 0;
        private void SwitchToResults()
        {
            if (IsShowingGame == 1)
            {
                ResultsSubForm.SwitchToBatches();
                ResultsSubForm.UpdRecord(Records);
                return;
            }
            GameGroup.Hide();
            ResultsSubForm.Show();
            ResultsSubForm.SwitchToBatches();
            ResultsSubForm.UpdRecord(Records);
            IsShowingGame = 1;
        }

        private void SwitchToGame()
        {
            if (IsShowingGame == 2)
                return;
            ResultsSubForm.Hide();
            GameGroup.Show();
            IsShowingGame = 2;
        }
        private void HumanGame_Click(object sender, EventArgs e)
        {
            SwitchToGame();
            ICardPlayer1.Reset();
            ResetWorld(new MLoggingTable(PlayerInput.Player = new MHumanPlayer("Ал"), ICardPlayer1), true);
            PlayerInput.Player.UpdateHand();
        }

        private void NewBatchGame_Click(object sender, EventArgs e)
        {
            Records.Add(MLoggingTable.ProcessBatchTesting(
                ICardPlayer1, ICardPlayer2, Program.GetIntSetting(Program.GlobalSetting.BatchCount)));
            World.ForceEndGame();
            SwitchToResults();
        }
        private void NewSingleGame_Click(object sender, EventArgs e)
        {
            SwitchToGame();
            ICardPlayer1.Reset();
            ICardPlayer2.Reset();
            ResetWorld(new MLoggingTable(ICardPlayer1, ICardPlayer2));
        }


        private void SetFirstPlayer_Click(object sender, EventArgs e) => SetPlayer(ref ICardPlayer1);
        private void SetSecondPlayer_Click(object sender, EventArgs e) => SetPlayer(ref ICardPlayer2);

        private void RecompileFirst_Click(object sender, EventArgs e) => SetPlayer(ref ICardPlayer1, true);
        private void RecompileSecond_Click(object sender, EventArgs e) => SetPlayer(ref ICardPlayer2, true);

        private void SetDllFirstPlayer_Click(object sender, EventArgs e) => SetPlayerDll(ref ICardPlayer1);
        private void SetDllSecondPlayer_Click(object sender, EventArgs e) => SetPlayerDll(ref ICardPlayer2);

        private void ResetFirstPlayer_Click(object sender, EventArgs e) => ICardPlayer1 = new MSimplePlayer("Первый");
        private void ResetSecondPlayer_Click(object sender, EventArgs e) => ICardPlayer2 = new MSimplePlayer("Второй");

        private void SetUpPlayers_DropDownOpened(object sender, EventArgs e)
        {
            FirstPlayerMenu.Text = "Первого(" + ICardPlayer1.GetName() + ")";
            SecondPlayerMenu.Text = "Второго(" + ICardPlayer2.GetName() + ")";
        }

        private void ShowResult_Click(object sender, EventArgs e) => SwitchToResults();
        private void SingleGame_Click(object sender, EventArgs e) => SwitchToGame();
        private void SetUpPlayers_Click_1(object sender, EventArgs e) => SwitchToGame();

    }

}
