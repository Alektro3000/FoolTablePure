using CardTableFool.Main;
using CardTableFool.Players;
using SharpDX.Direct2D1;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;



namespace CardTableFool.Tables
{
	/// <summary>
	/// Реализация стола проводящая матч и следящая за корректностью игры
	/// </summary>
	internal class MTable : MBaseTable
	{
		/// <summary>
		/// Исключения происходящие во время карточной игры
		/// </summary>
		public class GameException : Exception
		{
			/// <summary>
			/// Ошибку допустил первый игрок?
			/// </summary>
			public bool IsFirstPlayerError;
			/// <summary>
			/// Состояние стола после ошибки
			/// </summary>
			public List<SCardPair> Table;
			public GameException(bool isFirstError, List<SCardPair> table, string message) : base(message)
			{
				IsFirstPlayerError = isFirstError;
				Table = table.ToList();
			}
		}

		// Количество карт на руке при раздаче
		public const int TotalCards = 6;

		public static bool StrictCheck { get => Program.GetBoolSetting(Program.GlobalSetting.AttackCountCheck); }
		public static bool AttackTotalCheck { get => Program.GetBoolSetting(Program.GlobalSetting.AttackTotalCheck); }

		// Колода карт в прикупе
		protected List<SCard> deck = new List<SCard>();
		// Первоначальня колода
		protected List<SCard> BeginDeck = new List<SCard>();

		public ICardPlayer Player1;        // игрок 1
		public ICardPlayer Player2;        // игрок 2
		protected List<SCard> PlHand1 = new List<SCard>();
		protected List<SCard> PlHand2 = new List<SCard>();

		//Человеские имена для участников раунда 

		/// <summary>
		/// Атакующий в текущем ходе
		/// </summary>
		protected ICardPlayer attacker;
		/// <summary>
		/// Защищающийся в текущем ходе
		/// </summary>
		protected ICardPlayer defender;
		/// <summary>
		/// Рука атакующего на момент начала хода
		/// </summary>
		protected List<SCard> attackerHand;
		/// <summary>
		/// Рука защищающегося на момент начала хода
		/// </summary>
		protected List<SCard> defenderHand;

		protected SCard Trump;             // козырь
		protected List<SCardPair> Table = [];   // карты на столе
		public bool IsFirstPlayerAttacking = true;
		public MTable(ICardPlayer NewPlayer1, ICardPlayer NewPlayer2,
			bool isFirstPlayerAttacking = false, List<SCard> InitDeck = null)
		{
			IsFirstPlayerAttacking = isFirstPlayerAttacking;
			Player1 = NewPlayer1;
			Player2 = NewPlayer2;
			if (InitDeck != null)
				deck = InitDeck.ToList();
		}
		public override List<SCard> GetBeginDeck()
		{
			return BeginDeck;
		}
		public override string GetPlayerName(bool first)
		{
			return (first ? Player1 : Player2).GetName();
		}
		public override List<SCard> GetPlayerHand(bool first)
		{
			return first ? PlHand1 : PlHand2;
		}
		protected ICardPlayer GetPlayer(bool first)
		{
			return first ? Player1 : Player2;
		}
		public override SCard GetCurrentTrump()
		{
			return Trump;
		}
		public override bool GetPlayerTurn()
		{
			return IsFirstPlayerAttacking;
		}
		/// <summary>
		/// Получает описание карты на столе
		/// </summary>
		/// <returns></returns>
		public override List<SCardPair> GetTable()
		{
			return Table.ToList();
		}

		/// <summary>
		/// Простейшая реализация игры, полностью проводит игру без дополнительных вмешательств
		/// </summary>
		public EndGame PlayGameUnstoppable()
		{
			try
			{
				Initialize();
				while (true)
				{
					PlayPreRound();

					EndRound RoundResult;
					while ((RoundResult = PlayAttack(PlayDefence())) == EndRound.Continue) ;

					var GameResult = PlayPostRound(RoundResult);
					if (GameResult != EndGame.Continue)
						return GameResult;
				}

			}
			catch (GameException error)
			{
				OnError(error);
				return EndGame.Error;
			}
			catch (AggregateException error) when (error.InnerException is GameException)
			{
				OnError(error.InnerException as GameException);
				return EndGame.Error;
			}
		}

		/// <summary>
		/// Функция вызываемая при ошибке
		/// </summary>
		protected virtual void OnError(GameException error)
		{

		}
		/// <summary>
		/// Инициализация Колоды 
		/// </summary>
		protected virtual void InitDeck()
		{
			List<SCard> temp = SCard.GetDeck();
			temp.RemoveAll(deck.Contains);

			// формирование прикупа - перемешиваем карты
			for (int c = 0, end = temp.Count; c < end; c++)
			{
				int num = Random.Shared.Next(temp.Count);
				deck.Add(temp[num]);
				temp.RemoveAt(num);
			}
			BeginDeck = deck.ToList();

		}
		public override void Initialize()
		{
			InitDeck();
			// формирование козыря
			Trump = deck[0];

			Player1.SetTrump(Trump);
			Player2.SetTrump(Trump);

			// раздача карт первому и второму игроку
			for (int c = 0; c < TotalCards; c++)
			{
				Player1.AddToHand(deck.Last());
				PlHand1.Add(deck.Last());
				deck.RemoveAt(deck.Count - 1);

				Player2.AddToHand(deck.Last());
				PlHand2.Add(deck.Last());
				deck.RemoveAt(deck.Count - 1);
			}
		}
		public override void PlayPreRound()
		{
			//Обозначаем роли на текущий ход
			attacker = GetPlayer(IsFirstPlayerAttacking);
			defender = GetPlayer(!IsFirstPlayerAttacking);
			attackerHand = GetPlayerHand(IsFirstPlayerAttacking);
			defenderHand = GetPlayerHand(!IsFirstPlayerAttacking);

			if (attacker.GetCount() == 0)
				throw new GameException(IsFirstPlayerAttacking, [], $"Attacker({attacker.GetName()}) has no card to attack");

			// игрок делает ход
			List<SCard> cards = [];
			try
			{
				cards = attacker.LayCards();
			}
			catch (Exception e)
			{
				throw GatherErrorsFromPlayer(e, "LayCards", IsFirstPlayerAttacking);
			}
			// Выкладываем все карты на стол
			var table = SCardPair.CardsToCardPairs(cards);

			//Проверяем что атакующий действительно атакует
			if (cards.Count == 0)
				throw new GameException(IsFirstPlayerAttacking, table, $"Attacker({attacker.GetName()} didn't select any cards for initial attack");

			//Проверяем что у защищающегося достаточно карт чтобы отбиться
			if (StrictCheck && cards.Count > defenderHand.Count)
				throw new GameException(IsFirstPlayerAttacking, table,
					$"Attacker({attacker.GetName()}) added more cards than Defender has");

			//Проверяем наличие карт у атакующего
			cards.All(card => attackerHand.Contains(card) ? true :
				throw new GameException(IsFirstPlayerAttacking, table,
					$"Attacker({attacker.GetName()}) doesnot have card({card}) for initial attack"));

			//Проверяем докинутые карты на корректность, считаем первую корректной
			CheckAddedCard(table, 1);
			Table = table;

		}
		public override EndRound PlayDefence()
		{
			var tablePrev = Table.ToList();
			var table = Table.ToList();
			bool defendLocal = false;
			try
			{
				// второй игрок отбивается
				defendLocal = defender.Defend(table);
			}
			catch (Exception e)
			{
				throw GatherErrorsFromPlayer(e, "Defence", !IsFirstPlayerAttacking);
			}

			//После обороны количество карт не должно измениться
			if (tablePrev.Count != table.Count)
				throw new GameException(!IsFirstPlayerAttacking, table, $"Defender({defender.GetName()}) added card to Table");

			//Достаём все использованные карты и сортируем их
			var UpCards = table.FindAll(x => x.Beaten).ConvertAll(x => x.Up);
			UpCards.Sort();

			//Проверяем, что он не отбился одной картой от двух пар
			for (int i = 1; i < UpCards.Count; i++)
				if (UpCards[i] == UpCards[i - 1])
					throw new GameException(!IsFirstPlayerAttacking, table, $"Defender({defender.GetName()}) " +
						$"used card({UpCards[i]}) twice");

			//Защищавшийся действительно имел эти карты
			for (int i = 0; i < UpCards.Count; i++)
				if (!defenderHand.Contains(UpCards[i]))
					throw new GameException(!IsFirstPlayerAttacking, table, $"Defender({defender.GetName()}) " +
						$"doesn't have card({UpCards[i]}) to defend");

			//Защищавшийся не изменил карты уже лежавшие на столе 
			foreach (var prevPair in tablePrev)
				if (prevPair.Beaten && !table.Contains(prevPair))
					throw new GameException(!IsFirstPlayerAttacking, table, $"Defender({defender.GetName()}) " +
						$"card pair({prevPair}) to another");

			//Проверяем что он действительно отбился, если он заявляет об этом
			int BeatenCount = table.FindAll(x => x.Beaten).Count;
			bool DefendFact = (BeatenCount == defenderHand.Count) || (BeatenCount == table.Count);

			if (!DefendFact && defendLocal)
				throw new GameException(!IsFirstPlayerAttacking, table, $"Defender({defender.GetName()}) claimed his defence, but in fact...");

			//Если все проверки прошли успешно обновляем состояние стола
			Table = table;

			return defendLocal ? EndRound.Continue : EndRound.Take;
		}
		public override EndRound PlayAttack(EndRound DefenceResult)
		{
			var tablePrev = Table.ToList();
			var table = Table.ToList();
			bool added = false;

			try
			{
				// игрок подкидывает
				added = attacker.AddCards(table, DefenceResult != EndRound.Take);
			}
			catch (Exception e)
			{
				throw GatherErrorsFromPlayer(e, "AddCards", IsFirstPlayerAttacking);
			}

			//Проверка что атакующий не докинул слишком много карт
			if (AttackTotalCheck && table.Count > TotalCards)
				throw new GameException(IsFirstPlayerAttacking, table, $"Attacker({attacker.GetName()}) added too many cards({table.Count})");

			//Проверка что атакующий не докинул больше карт чем есть у обороняющегося
			if (StrictCheck && table.Count > defenderHand.Count)
				throw new GameException(IsFirstPlayerAttacking, table, $"Attacker({attacker.GetName()}) added more cards than Defender has");

			//Проверка заявления атакующего
			if (tablePrev.Count == table.Count == added)
				throw new GameException(IsFirstPlayerAttacking, table, $"Attacker({attacker.GetName()}) claimed that he " +
				(added ? "added" : "didnt add") + $" cards but in fact he not");

			//Проверка что атакующий не изменил уже лежавшие карты
			tablePrev.All(prevPair => table.Contains(prevPair) ? true :
					throw new GameException(IsFirstPlayerAttacking, table, $"Attacker({attacker.GetName()}) " +
						$"switched cardpair({prevPair})"));

			//Атакующий действительно имел эти карты
			table.All(x => attackerHand.Contains(x.Down) ? true :
				throw new GameException(IsFirstPlayerAttacking, table, $"Attacker({attacker.GetName()}) " +
						$"doesn't have card({x.Down}) to attack"));

			CheckAddedCard(table, tablePrev.Count);

			//Если все проверки прошли успешно обновляем состояние стола
			Table = table;


			if (DefenceResult == EndRound.Continue)
			{
				// если защитился и подкинули, то продолжаем
				if (added)
					return EndRound.Continue;
				// если защитился, но не подкинули, то успешная защита
				// Но сначала очистить руки
				foreach (var pair in table)
				{
					if (pair.Beaten)
					{
						//Сброс в стопку сброса
						defenderHand.Remove(pair.Up);
						attackerHand.Remove(pair.Down);
					}
					else
					{
						//Если карту не отбили она возвращается к обладателю
						//attackerHand.Add(pair.Down); она итак у него в руке
						attacker.AddToHand(pair.Down);
					}
				}
				return EndRound.Defend;
			}

			// если не отбился, то принимает
			foreach (var pair in table)
			{
				// но сначала обновляем карты атакующей стороны
				attackerHand.Remove(pair.Down);

				defender.AddToHand(pair.Down);
				if (pair.Beaten)
					defender.AddToHand(pair.Up);

				//Добавляем карты атакующего в руку защищающегося
				defenderHand.Add(pair.Down);
				//Мы не должны добавлять в руку обороняющегося карты которыми он отбивался
			}

			return EndRound.Take;
		}

		public override EndGame PlayPostRound(EndRound RoundResult)
		{
			try
			{
				//Вызываем ивент конца раунда у первого игрока
				Player1.OnEndRound(Table.ToList(), RoundResult == EndRound.Defend);
			}
			catch (Exception e)
			{
				throw GatherErrorsFromPlayer(e, "OnEndRound", true);
			}
			try
			{
				//Вызываем ивент конца раунда у второго игрока
				Player2.OnEndRound(Table.ToList(), RoundResult == EndRound.Defend);
			}
			catch (Exception e)
			{
				throw GatherErrorsFromPlayer(e, "OnEndRound", false);
			}

			// Добавляем игрокам карты из колоды
			AddCards();

			// Если игрок защитился даём ему ход
			if (RoundResult == EndRound.Defend) IsFirstPlayerAttacking = !IsFirstPlayerAttacking;

			//Собираем информацию о игроках
			int Player1Count = Player1.GetCount();
			int Player2Count = Player2.GetCount();

			//Проверяем корректность количества карт первого игрока
			if (Player1Count != PlHand1.Count)
				throw new GameException(true, [], $"Player 1({Player1.GetName()} Claimed wrong amount of cards" +
					$"{Player1Count}, fact: {PlHand1.Count}");

			//Проверяем корректность количества карт второго игрока
			if (Player2Count != PlHand2.Count)
				throw new GameException(false, [], $"Player 2({Player2.GetName()} Claimed wrong amount of cards" +
					$"{Player2Count}, fact: {PlHand2.Count}");

			// Если конец игры, то выходим
			if (Player1Count == 0 && Player2Count == 0) return EndGame.Draw;
			if (Player1Count == 0) return EndGame.First;
			if (Player2Count == 0) return EndGame.Second;
			//А если нет то остаёмся
			return EndGame.Continue;
		}

		// Добавляем карты из колоды первому и второму игроку
		private void AddCards()
		{
			//Проверка на коррректность количества карт
			if (attacker.GetCount() != attackerHand.Count)
				throw new GameException(IsFirstPlayerAttacking, [], $"Attacker({attacker.GetName()}), claimed incorrect amount of cards" +
					$"Attacker: {attacker.GetCount()}, in fact {attackerHand.Count}");

			// добавляем карты атаковавшему игроку
			while (attacker.GetCount() < TotalCards && deck.Count > 0)
			{
				attacker.AddToHand(deck.Last());
				attackerHand.Add(deck.Last());
				deck.RemoveAt(deck.Count - 1);

				if (attacker.GetCount() != attackerHand.Count)
					throw new GameException(IsFirstPlayerAttacking, [], $"Attacker({attacker.GetName()}), claimed incorrect amount of cards" +
						$"Attacker: {attacker.GetCount()}, in fact {attackerHand.Count}");
			}

			//Проверка на коррректность количества карт
			if (defender.GetCount() != defenderHand.Count)
				throw new GameException(!IsFirstPlayerAttacking, [], $"Defender({defender.GetName()}), claimed incorrect amount of cards" +
					$"Defender: {defender.GetCount()}, in fact {defenderHand.Count}");


			// добавляем защищавшемуся игроку
			while (defender.GetCount() < TotalCards && deck.Count > 0)
			{
				defender.AddToHand(deck.Last());
				defenderHand.Add(deck.Last());
				deck.RemoveAt(deck.Count - 1);

				if (defender.GetCount() != defenderHand.Count)
					throw new GameException(!IsFirstPlayerAttacking, [], $"Defender({defender.GetName()}), claimed incorrect amount of cards" +
						$"Defender: {defender.GetCount()}, in fact {defenderHand.Count}");
			}
		}
		//Проверяет подброшенные карты
		private void CheckAddedCard(List<SCardPair> table, int beginning)
		{
			var tested = table.Slice(0, beginning);
			var ToBeTested = table.Slice(beginning, table.Count - beginning);

			foreach (var card in ToBeTested)
			{
				if (tested.Exists(x => x.Up == card.Down || x.Down == card.Down))
					throw new GameException(IsFirstPlayerAttacking, table, $"Attacker({attacker.GetName()}) added copy of card({card.Down})");

				if (!tested.Any(x => SCardPair.CanBeAddedToPair(card.Down, x)))
					throw new GameException(IsFirstPlayerAttacking, table, $"Attacker({attacker.GetName()}) added card({card.Down}) which " +
						$"rank is not equal to any card on table");
			}
		}

		Exception GatherErrorsFromPlayer(Exception ex, string State, bool IsFirstGuilty)
		{
			if (ex is GameException)
			{
				(ex as GameException).IsFirstPlayerError = IsFirstGuilty;
				return ex;
			}
			throw new AggregateException(new GameException(IsFirstGuilty, Table,
				$"{Player1.GetName()} has error in {State}: " + ex.Message), ex);
		}

	}
}
