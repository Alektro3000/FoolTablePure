using CardTableFool.Players;
using CardTableFool.Tables;
using System.Text.Json;

namespace CardTableFool.Main
{

    //Основной класс ответственный за соединение всего вместе
    public partial class MainForm : Form
    {
		protected PlaySubsystem World;
		protected System.Windows.Forms.Timer _timer;
		protected PlayerInputSubsystem PlayerInput;
        protected ResultForm ResultsSubForm;
		protected bool Autoplay = true;
		public MainForm()
        {
            PlayerInput = new PlayerInputSubsystem();
			World = new PlaySubsystem();

			InitializeForm();

            FormClosed += (x, e) => { _timer.Stop(); };
        }
        //Пересоздаём мир с новым столом
        protected virtual void ResetWorld(MBaseTable Table, bool PlayerExist = false, List<VisualCard> BeginPositions = null)
        {
            if (World != null)
            {
                World.OnMovedToNewStage -= PlayerInput.NextTurn;
                World.OnPreMovedToNewStage -= PlayerInput.PreNextTurn;
            }

            //Создаём новый игровой контекст
            World = new PlaySubsystem(Table, BeginPositions);
            World.StopOnAny = !Autoplay;
            if (PlayerExist)
            {
                World.StopOnFirstPlayer = true;
                World.HideSecondPlayerCards = Program.GetBoolSetting(Program.GlobalSetting.HideCards);
                PlayerInput.CanSelectDeck = !Program.GetBoolSetting(Program.GlobalSetting.HideDeckCards);
                PlayerInput.CanSelectDump = !Program.GetBoolSetting(Program.GlobalSetting.HideDumpCards);
                PlayerInput.CanSelectHandSecond = false;
                Undo.Hide();
            }
            else
            {
                PlayerInput.Player = null;
                World.HideSecondPlayerCards = false;
                PlayerInput.CanSelectDeck = true;
                PlayerInput.CanSelectDump = true;
                PlayerInput.CanSelectHandSecond = true;
				Undo.Show();
			}
            PlayerInput.SetUpWorld(World);
            DXControl.SetUpCards(World.Cards);
            FirstPlayerName.Text = Table.GetPlayerName(true);
            SecondPlayerName.Text = Table.GetPlayerName(false);
        }

		protected void WorldTick(float DeltaTick)
        {
            DeltaTick *= Program.GetIntSetting(Program.GlobalSetting.AnimationScale) / 100f;

            //Обновляем ввод игрока
            PlayerInput.UpdateSelectedCard(World.Cards);

            //Обновляем карты
            World.UpdateTick(DeltaTick);

            //Отрисовываем кадр
            DXControl.Render(DeltaTick);

            //Обновляем подсказку для игрока
            ActionButton.Text = PlayerInput.TEXT;
        }
        
        DateTime LastTime;
		protected void TimerTick(object sender, EventArgs e)
        {

            var CurTime = DateTime.Now;

            WorldTick((float)(CurTime - LastTime).TotalSeconds);
            LastTime = CurTime;
        }


		protected void GameForm_Load(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            //Иницилизируем таймер
            _timer = new System.Windows.Forms.Timer();

            _timer.Tick += TimerTick;
            _timer.Interval = 1;
            _timer.Start();
            LastTime = DateTime.Now;
        }

        protected virtual void OnRepeatGame(object sender, GameEventArgs e)
        {
			throw new NotImplementedException();
		}
		protected virtual void OnRecallGame(object sender, GameEventArgs e)
		{
			throw new NotImplementedException();
		}

		protected void Autoplay_Click(object sender, EventArgs e) =>
            UpdateAutoplay(!Autoplay);

		protected void UpdateAutoplay(bool NewVal)
        {
			Autoplay = NewVal;
			World.StopOnAny = !Autoplay;
			AutoplayButton.BackColor = Autoplay ? Color.White : Color.IndianRed;
		}

		protected virtual void ActionButton_Click(object sender, EventArgs e)
        {
            PlayerInput.MoveToNextStage();
        }

		protected void SetPlayer(ref ICardPlayer player, bool ForcedRecompile = false)
        {
            //Вызываем диалоговое окно для выбора первой сборки
            if (openDirectory.ShowDialog() == DialogResult.Cancel)
                //Если отказывается возвращаемся
                return;
            // получаем выбранный файл
            string filename = openDirectory.SelectedPath;
            if (filename != null && Directory.Exists(filename))
                player = new MCustomPlayer(filename, !ForcedRecompile);
        }
		protected void SetPlayerDll(ref ICardPlayer player)
        {
            //Вызываем диалоговое окно для выбора первой сборки
            if (SetUpDllDialog.ShowDialog() == DialogResult.Cancel)
                //Если отказывается возвращаемся
                return;

            // получаем выбранный файл
            string filename = SetUpDllDialog.FileName;
            if (filename != null && File.Exists(filename))
                player = new MCustomPlayer(filename, true);
        }

		protected virtual void Undo_Click(object sender, EventArgs e)
		{
			if (PlayerInput.Player != null)
				return;
			int tr = World.Transitions - 1;
			UpdateAutoplay(false);

			MBaseTable table;
            if (World.Table is MPlaybackTable)
            {
                table = new MPlaybackTable((World.Table as MPlaybackTable).Record);
            }
            else
            {
                var b = World.Table as MLoggingTable;
                var pl1 = b.Player1;
                var pl2 = b.Player2;
                pl1.Reset();
                pl2.Reset();
                table = new MLoggingTable(b.Record, pl1, pl2);
            }
            ResetWorld(table, false, World.Cards.FindAll(x => x.CardType == VisualCard.SpecialType.normal));
            for (int i = 0; i < tr; i++)
                World.MoveToNextState();
        }

		protected void Settings_Click(object sender, EventArgs e) =>
            new SettingsForm().ShowDialog();
        
    }

}
