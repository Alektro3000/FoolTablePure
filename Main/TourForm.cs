using CardTableFool.Tables;
using CardTableFool.Players;
using CardTableFool.Forms;

namespace CardTableFool.Main
{
	//Основной класс ответственный за соединение всего вместе
	public partial class TourForm : MainForm
	{

		List<BatchRecord> PreRecords = [];
		List<BatchRecord> TourRecords = [];

		ResultForm TourResultsSubForm;

		MCustomPlayer[] Candidates;
		ICardPlayer[] TourTable;
		BatchRecord[] TourTableResults;
		Task[] AsyncMatches;

		int CurrentTour;

		int CurrentSubTable;
		int SubTableDepth { get => Program.GetIntSetting(Program.GlobalSetting.DepthOfFinal); }
		public TourForm() : base()
		{
			PlayerInput.CanSelectHandSecond = true;
			Candidates = [];
			TourTable = [];


			DXControl.DoubleClick += (x, e) => PlayerInput.OnClick();
			DXControl.Click += (x, e) => PlayerInput.OnClick();
			DXControl.OnMouseMoveWorldSpace += (x, e) => PlayerInput.UpdatePointer((e as MouseMoveWorldSpace).Location);
			Program.SettingsChanged += OnUpdateSettings;
			FormClosed += (x, e) => { Program.SettingsChanged -= OnUpdateSettings; };
			SwitchToTournament();
		}
		protected override void ResetWorld(MBaseTable Table, 
			bool PlayerExist = false, List<VisualCard> BeginPositions = null)
		{
			base.ResetWorld(Table, PlayerExist, BeginPositions);
			World.ExtraOffset = true;
		}

		private void OnUpdateSettings(object sender, EventArgs e)
		{
			WorldTick(0.01f);
			DXControl.SetUpTourTable(ConvertPlayersToNames(TourTable));
		}
		protected override void OnRecallGame(object sender, GameEventArgs e)
		{
			ResetWorld(new MPlaybackTable(e.Record), false);
			SwitchToGame();
		}
		protected override void OnRepeatGame(object sender, GameEventArgs e)
		{
			var First = e.Batch.Player1;
			First.Reset();
			var Second = e.Batch.Player2;
			Second.Reset();
			ResetWorld(new MLoggingTable(e.Record, First, Second), false);
			SwitchToGame();
		}

		private void HideGameTour()
		{
			FirstPlayerName.Hide();
			SecondPlayerName.Hide();
			AutoplayButton.Hide();
			Undo.Hide();
		}


		/// <summary>
		/// Текущая открытая вкладка
		/// Tour = 0
		/// Results = 1
		/// Game = 2
		/// TourResults = 3
		/// </summary>
		int IsShowingGame = -1;
		private void SwitchToResults()
		{
			if (IsShowingGame == 1)
			{
				ResultsSubForm.SwitchToBatches();
				return;
			}
			GameGroup.Hide();
			TourResultsSubForm.Hide();

			HideGameTour();

			ResultsSubForm.Show();
			ResultsSubForm.SwitchToBatches();
			IsShowingGame = 1;
		}
		private void SwitchToTourResults()
		{
			if (IsShowingGame == 3)
			{
				TourResultsSubForm.SwitchToBatches();
				return;
			}
			TourResultsSubForm.Show();

			GameGroup.Hide();
			ResultsSubForm.Hide();

			HideGameTour();

			TourResultsSubForm.SwitchToBatches();
			IsShowingGame = 3;
		}
		private void SwitchToTournament()
		{
			if (TourTable != null)
				DXControl.ShowTable = true;

			World.ForceEndGame();
			if (IsShowingGame == 0)
				return;

			ResultsSubForm.Hide();

			HideGameTour();

			TourResultsSubForm.Hide();
			GameGroup.Show();
			IsShowingGame = 0;
		}
		private void SwitchToGame()
		{
			DXControl.ShowTable = false;
			if (IsShowingGame == 2)
				return;

			AutoplayButton.Show();
			FirstPlayerName.Show();
			SecondPlayerName.Show();
			Undo.Show();

			ResultsSubForm.Hide();
			TourResultsSubForm.Hide();

			GameGroup.Show();
			IsShowingGame = 2;
		}

		protected override void ActionButton_Click(object sender, EventArgs e)
		{
			if (IsShowingGame == 2)
			{
				World.MoveToNextState();
			}
			else if (TourTable != null)
				NextMatch();
		}
		protected override void Undo_Click(object sender, EventArgs e)
		{
			if (IsShowingGame == 2)
			{
				base.Undo_Click(sender, e);
			}
			else if (TourTable != null)
				PrevMatch();
		}
		private void StopAsyncOperations()
		{
			if (AsyncMatches != null && CurrentTour >= 0 && CurrentTour < AsyncMatches.Length && AsyncMatches[CurrentTour] != null)
				AsyncMatches[CurrentTour].Wait();
		}
		private int MoveOneMatch(int TourToMove)
		{
			int k = System.Numerics.BitOperations.Log2((uint)TourToMove);
			int kPrev = System.Numerics.BitOperations.Log2((uint)(TourToMove - 1 - CurrentSubTable * (1 << (k - SubTableDepth))));

			if (k <= SubTableDepth)
			{
				if (CurrentSubTable <= 0)
				{
					CurrentSubTable = -1;
					return TourToMove - 1;
				}
				CurrentSubTable--;
				int mxDepth = System.Numerics.BitOperations.Log2((uint)TourTable.Length - 1) - 1;
				return (1 << mxDepth) + ((CurrentSubTable + 1) << mxDepth - SubTableDepth) - 1;
			}

			if (k == kPrev)
				return TourToMove - 1;

			int First = 1 << (k - 1);

			return First - 1 + (1 << k - SubTableDepth - 1) + (CurrentSubTable << (k - SubTableDepth - 1));
		}
		private void MoveToNextMatch()
		{
			int NextTour = MoveOneMatch(CurrentTour);
			while (TourTable[NextTour] != null)
				NextTour = MoveOneMatch(NextTour);

			CurrentTour = NextTour;
		}
		private string GetBlockName()
		{
			return CurrentSubTable == -1 || (SubTableDepth == 0) ? null : "Блок " + (char)('А' + (1 << SubTableDepth) - CurrentSubTable - 1);
		}
		private void PrevMatch()
		{

		}
		private bool NextMatch()
		{
			if (CurrentTour == 0)
				return false;
			if (AsyncMatches[CurrentTour] != null)
				AsyncMatches[CurrentTour].Wait();

			var Record = TourTableResults[CurrentTour];
			TourRecords.Add(Record);


			int WinnerPosition = CurrentTour * 2;

			if (Record.FirstWin < 0)
				WinnerPosition++;

			DXControl.SetUpBlockName(GetBlockName());

			var StringTable = ConvertPlayersToNames(TourTable);
			DXControl.SetUpTourTable(StringTable);
			DXControl.SetUpTourTableAnim(GlobalToLocal(WinnerPosition), GlobalToLocal(CurrentTour), TourTable[WinnerPosition].GetName());


			TourTable[CurrentTour] = TourTable[WinnerPosition];

			const int SubRound = 3;
			VisualTourControl.BarInfo Val1 = new(SubRound, StringTable[GlobalToLocal(CurrentTour * 2)]);
			VisualTourControl.BarInfo Val2 = new(SubRound, StringTable[GlobalToLocal(CurrentTour * 2 + 1)]);
			int cnt = Program.GetIntSetting(Program.GlobalSetting.BarAnimFetch);
			var Slice = new List<GameRecord>();
			
			for (int i = 0; i < SubRound-1; i++)
			{
				Slice.AddRange(Record.Records.Slice(i*cnt, cnt));
				Val1.Scores[i] = Slice.Sum(x => x.Score1);
				Val2.Scores[i] = Slice.Sum(x => x.Score2);
				Val1.Errors[i] = Slice.Sum(x => (x.ErrorCode != null && x.IsFirstCausedError ? 1 : 0));
				Val2.Errors[i] = Slice.Sum(x => (x.ErrorCode != null && !x.IsFirstCausedError ? 1 : 0));
			}

			Val1.Scores[SubRound-1] = Record.TotalScore1;
			Val2.Scores[SubRound - 1] = Record.TotalScore2;
			Val1.Errors[SubRound - 1] = Record.TotalErrors1;
			Val2.Errors[SubRound - 1] = Record.TotalErrors2;

			DXControl.SetUpBarAnim(Val1, Val2);

			MoveToNextMatch();

			if (CurrentTour != 0 && AsyncMatches[CurrentTour] == null)
				AsyncMatches[CurrentTour] = CalculateMatch(CurrentTour);

			return true;
		}

		private Task CalculateMatch(int pos)
		{
			return Task.Run(() =>
			{
				var Record = MLoggingTable.ProcessBatchTesting(TourTable[pos * 2], TourTable[pos * 2 + 1],
				Program.GetIntSetting(Program.GlobalSetting.TourBatchCount));

				TourTableResults[pos] = Record;
			});
		}

		private void InitTourTable(ICardPlayer[] Players)
		{
			var Shuffled = Players.OrderBy(x => Random.Shared.NextSingle()).ToArray();

			int length = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)Shuffled.Length);

			ICardPlayer[] arr = new ICardPlayer[length * 2];
			TourTableResults = new BatchRecord[length];
			AsyncMatches = new Task[length];
			int[] colors = new int[length * 2];

			for (int i = 0; i < Shuffled.Length; i++)
			{
				int quater = i / 2;
				int k = quater + length;
				switch (i % 4)
				{
					case 1:
						k = 2 * length - 2 - quater;
						break;
					case 2:
						k = length + length / 2 + quater;
						break;
					case 3:
						k = length + length / 2 - quater;
						break;

				}
				arr[k] = Shuffled[i];
				colors[k] = 1;
			}

			for (int i = colors.Length / 2 - 1; i >= 0; i--)
				if (colors[i * 2 + 1] != 0 || colors[i * 2] != 0)
					colors[i] = 1;

			//Выравнивание таблицы
			for (int i = 2; i < arr.Length; i++)
			{
				int par = i + (i % 2 == 0 ? 1 : -1);
				if (colors[i] == 0 && (colors[par] != 0))
				{
					arr[i / 2] = arr[par];
					colors[i / 2] = colors[par];
					arr[par] = null;
					colors[par] = 0;
				}
			}

			TourTable = arr;
			CurrentSubTable = (1 << SubTableDepth) - 1;
			DXControl.SetUpTourTable(ConvertPlayersToNames(TourTable));
			DXControl.SetUpBlockName(GetBlockName());

			for (int i = TourTable.Length / 2 - 1; i > 0; i--)
				if (TourTable[i] == null)
				{
					AsyncMatches[i] = CalculateMatch(i);
					CurrentTour = i;
					return;
				}
			CurrentTour = 0;
			return;
		}

		private string PlayerToName(ICardPlayer player) =>
			player == null ? null : player.GetName();
		private int GlobalToLocal(int Global)
		{
			if (CurrentSubTable == -1)
				return Global;

			int Depth = System.Numerics.BitOperations.Log2((uint)(Global)) - SubTableDepth;
			return Global - (1 << Depth + SubTableDepth) - CurrentSubTable * (1 << Depth) + (1 << Depth);
		}
		private string[] ConvertPlayersToNames(ICardPlayer[] players)
		{
			if (CurrentSubTable == -1)
			{
				string[] ans = new string[2 << SubTableDepth];
				for (int i = 0; i < ans.Length; i++)
					ans[i] = PlayerToName(players[i]);
				return ans;
			}

			string[] arr = new string[(players.Length) >> SubTableDepth];
			int Depth = System.Numerics.BitOperations.Log2((uint)(players.Length)) - 1;

			for (int j = Depth - SubTableDepth; j >= 0; j--)
				for (int i = 0; i < 1 << j; i++)
					arr[i + (1 << j)] = PlayerToName(players[i + (1 << j + SubTableDepth) + CurrentSubTable * (1 << j)]);


			return arr;
		}

		private void SetUpPlayers_Click(object sender, EventArgs e) => LoadPlayers(true);
		private void Recompile_Click(object sender, EventArgs e) => LoadPlayers(false);

		void LoadPlayers(bool Dll)
		{
			//Вызываем диалоговое окно для выбора первой сборки
			if (OpenTour.ShowDialog() == DialogResult.Cancel)
				//Если отказывается возвращаемся
				return;
			// получаем выбранный файл
			string filename = OpenTour.SelectedPath;
			if (filename == null)
				return;

			//Собираем все папки в один массив, чтобы лишний раз не вызывать перечисление всех папок
			var Folders = Directory.EnumerateDirectories(filename).ToArray();

			//Подготовливаем выходной массив
			Candidates = new MCustomPlayer[Folders.Length];

			//В отдельном потоке для каждого игрока загружаем код
			Parallel.For(0, Folders.Length, i => Candidates[i] = new MCustomPlayer(Folders[i], Dll));

			//Создаём турнирную таблицу
			InitTourTable(Candidates);

			SwitchToTournament();
		}

		private void RecallMatches_Click(object sender, EventArgs e)
		{
			TourResultsSubForm.UpdRecord(TourRecords);
			SwitchToTourResults();
		}


		private void BeginTour_Click(object sender, EventArgs e)
		{
			var Players = ResultsSubForm.GetSelectedPlayers();
			InitTourTable(Players.ToArray());
			TourRecords = [];


			SwitchToTournament();
		}

		private void PreTourBegin_Click(object sender, EventArgs e)
		{
			BatchRecord[] rec = new BatchRecord[Candidates.Length];

			StopAsyncOperations();

			Parallel.For(0, Candidates.Length, i =>
				rec[i] = MLoggingTable.ProcessBatchTesting(Candidates[i],
				new MSimplePlayer("Тестовый"),
				Program.GetIntSetting(Program.GlobalSetting.BatchCount)));

			PreRecords = Enumerable.OrderBy(rec, x => x.ErrorCount != 0 ? x.Records.Count * 100 + x.TotalScore1 : x.TotalScore1).ToList();
			ResultsSubForm.UpdRecord(PreRecords, true);
			SwitchToResults();
		}

		private void вернутьсяToolStripMenuItem_Click(object sender, EventArgs e) => SwitchToTournament();

		private void результатыToolStripMenuItem_Click(object sender, EventArgs e) => SwitchToResults();

	}

}
