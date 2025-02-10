using CardTableFool.Tables;
using SharpDX;

namespace CardTableFool.Main
{
    //Класс отвечающий за воспроизведение игры стола и его анимации
    public class PlaySubsystem
    {
        //Карты, порядок влияет на приоритет отрисовки (карта с большим id будет закрывать карту с меньшим)
        public List<VisualCard> Cards;
        //Стол
        public MBaseTable Table;
        public enum PlayState
        {
            //Формирование рук оппонентов
            Begin,
            //Начальной атаки
            PreRound,
            //Защита
            ActionDefend,
            //Атака
            ActionAttack,
            //Добор Карт
            PostRound,
            //Конец игры
            EndGame
        }
        PlayState _state;
        public PlayState State
        {
            get => _state;
        }


        int dumpStackCount = 1;
        int cardCountPrev;

        public int Transitions = 0;

        public EndRound RoundResult;

        public bool StopOnFirstPlayer = false;
        public bool StopOnSecondPlayer = false;
        public bool StopOnAny = false;

        public bool HideFirstPlayerCards = false;
        public bool HideSecondPlayerCards = false;


		public bool ExtraOffset = false;

		bool CurTurn;
        public bool IsFirstAttacking
        {
            get { return CurTurn; }
        }
        public VisualCard FindCard(SCard sCard)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                if (Cards[i].card == sCard && Cards[i].CardType == VisualCard.SpecialType.normal)
                    return Cards[i];
            }
            throw new Exception(sCard + " card wasn't found, id:" + (int)sCard);
        }

        public PlaySubsystem()
        {
            Cards = [];
            _state = PlayState.EndGame;

        }
        public PlaySubsystem(MBaseTable table, List<VisualCard> cards = null)
        {
            Table = table;

			//Иницилизируем стол, чтобы получить информацию о козыре
			table.Initialize();
            var deck = table.GetBeginDeck();
			//Иницилизируем карты 
			if (cards == null)
            {
                Cards = [];
                for (int i = 0; i < 36; i++)
                {
                    int id = (int)deck[i];
					var card = new VisualCard(deck[i], new(new Vector3(-id / 4 * 1.2f + 5f, id % 4 * 1.8f - 3, 0.5f)));

                    Cards.Add(card);
                    card.MoveToDeck(i, id * 0.02f);
                }
            }
            else
            {
                Cards = cards;
                for (int i = 0; i < Cards.Count; i++)
                {
                    Cards[i].MoveToDeck(i, (int)Cards[i].card * 0.02f);
                }
            }


            var CardTrump = FindCard(table.GetCurrentTrump());

            //Перемещаем козырь на его место
            Cards.Remove(CardTrump);
            Cards.Insert(0, CardTrump);
            CardTrump.MoveToTrump(false,(int)CardTrump.card * 0.02f);


            //Формируем специальную карту обозначающую козырь
            VisualCard Card = new VisualCard(CardTrump);
            Card.CardType = VisualCard.SpecialType.trump;
            Cards.Insert(0, Card);
            Card.MoveToTrump(true,cards != null ? -1000 : (int)CardTrump.card * 0.02f);

            _state = PlayState.Begin;


        }


        void MoveCardToDump(SCard card)
        {
            var cardVisual = FindCard(card);
            cardVisual.MoveToDump();
        }
        public bool IsAutoPlay()
        {
            return State == PlayState.PostRound ||//Добор не зависит ни от одного игрока
                (State == PlayState.ActionDefend != CurTurn
                ? !StopOnFirstPlayer : !StopOnSecondPlayer);
        }
        public void UpdateTick(float DeltaTick)
        {
            //Если все карты остановились и есть автопереход
            if (Cards.All(x=>!x.Moving) && !StopOnAny && IsAutoPlay())
            {
                if (OnPreMovedToNewStage != null)
                    OnPreMovedToNewStage.Invoke(this, new EventArgs());
                MoveToNextState();
                if (OnMovedToNewStage != null)
                    OnMovedToNewStage.Invoke(this, new EventArgs());
            }

            //Обновляем все карты
            foreach (var card in Cards)
                card.UpdateTick(DeltaTick);
        }
        void SetVisualHands()
        {
            bool T = Table.GetPlayerTurn();

            var AttackerCards = Table.GetPlayerHand(T);
            AttackerCards.Sort();
            for (int i = 0; i < AttackerCards.Count; i++)
                FindCard(AttackerCards[i]).MoveToHand(i, AttackerCards.Count, T, 
                    T ? HideFirstPlayerCards : HideSecondPlayerCards, ExtraOffset);

            var DefenderCards = Table.GetPlayerHand(!T);
            DefenderCards.Sort();
            for (int i = 0; i < DefenderCards.Count; i++)
                FindCard(DefenderCards[i]).MoveToHand(i, DefenderCards.Count, !T, 
                    !T ? HideFirstPlayerCards : HideSecondPlayerCards, ExtraOffset);

        }
        public EventHandler OnMovedToNewStage;
        public EventHandler OnPreMovedToNewStage;
        
        public void MoveToNextState()
        {
            Transitions++;
            if (_state == PlayState.Begin)
            {
                SetVisualHands();

                _state = PlayState.PreRound;
                CurTurn = Table.GetPlayerTurn();
            }
            else if (_state == PlayState.PreRound)
            {
                Table.PlayPreRound();
                var curTable = Table.GetTable();

                for (int i = 0; i < curTable.Count; i++)
                    FindCard(curTable[i].Down).MoveToTable(i, curTable.Count, true);

                _state = PlayState.ActionDefend;
            }
            else if (_state == PlayState.ActionDefend)
            {
                //Понижаем уровень всех карт что уже лежат на столе
                var prevTable = Table.GetTable();

                RoundResult = Table.PlayDefence();

                cardCountPrev = prevTable.Count;

                //Добавляем карты защитившегося
                var curTable = Table.GetTable();
                for (int i = 0; i < curTable.Count; i++)
                {
                    FindCard(curTable[i].Down).MoveToTable(i, cardCountPrev, true);
                    if (curTable[i].Beaten)
                    {
                        FindCard(curTable[i].Up).MoveToTable(i, cardCountPrev, false);
                    }
                }

                //Переходим к атаке
                _state = PlayState.ActionAttack;

			}
            else if (_state == PlayState.ActionAttack)
            {
                var prevTableCount = Table.GetTable().Count;

                RoundResult = Table.PlayAttack(RoundResult);

                var curTable = Table.GetTable();

                if (true || prevTableCount != curTable.Count)
                {

                    //Смещаем все карты по столу
                    for (int i = 0; i < curTable.Count; i++)
                    {
                        FindCard(curTable[i].Down).MoveToTable(i, curTable.Count, true);
                        if (curTable[i].Beaten)
                            FindCard(curTable[i].Up).MoveToTable(i, curTable.Count, false);
                    }
                }
                //Если продолжаем то переходим обратно к защите
                if (RoundResult == EndRound.Continue)
                    _state = PlayState.ActionDefend;
                else
                    _state = PlayState.PostRound;
                if (RoundResult == EndRound.Error)
                    _state = PlayState.EndGame;
            }
            else if (_state == PlayState.PostRound)
            {

                var curTable = Table.GetTable();

                //Добираем карты 
                EndGame Result = Table.PlayPostRound(RoundResult);

                //Все карты смещаем в стопку сброса
                if (RoundResult == EndRound.Defend)
                    for (int i = 0; i < curTable.Count; i++)
                    {
                        MoveCardToDump(curTable[i].Down);
                        if (curTable[i].Beaten)
                            MoveCardToDump(curTable[i].Up);
                    }

                SetVisualHands();

                CurTurn = Table.GetPlayerTurn();

                //Проверяем условия выхода из игры
                if (Result == EndGame.Continue)
                    _state = PlayState.PreRound;
                else
                    _state = PlayState.EndGame;
            }
            else
                Transitions--;

        }
        //Используется длы быстрого завершения игры 
        public void ForceEndGame()
        {
            _state = PlayState.EndGame;
            foreach (var Card in Cards)
                Card.TeleportTo(new (new Vector3(-1000, 1000, 0)));
        }
    }
}