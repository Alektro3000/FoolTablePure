using static CardTableFool.Main.PlaySubsystem;
using static System.Windows.Forms.AxHost;
using SharpDX;
using CardTableFool.Tables;
using CardTableFool.Players;

namespace CardTableFool.Main
{
    public class PlayerInputSubsystem
    {
        VisualCard _SelectedCard;
        public PlaySubsystem World;
        public MHumanPlayer Player;
        public VisualCard SelectedCard
        {
            get { return _SelectedCard; }
        }

        Vector2 Pointer;
        public State state;
        public bool CanSelectHandFirst = true;
        public bool CanSelectHandSecond = true;
        public bool CanSelectDeck = true;
        public bool CanSelectDump = true;

        bool IsDeckPreviewing;

        bool IsDumpPreviewing;

        public string TEXT = "Следующий ход";
        const string DefaultText = "Следующий ход";

        //Переменные для фазы обороны игрока
        List<SCardPair> TablePreview = [];
        List<(SCard, int)> SelectedCards = [];
        public void MoveToNextStage()
        {
            if (Player == null)
            {
                World.MoveToNextState();
                return;
            }
            if (World.IsAutoPlay())
            {
                PreNextTurn(null, null);
                World.MoveToNextState();
                NextTurn(null, null);
                return;
            }
            //Если мы использовали буфер для выбранных карт используем его
            if (SelectedCards.Count != 0)
            {
				//Отправляем игроку его карты
				Player.SelectedCards = SelectedCards.ConvertAll(x => x.Item1);

				Player.Prepare();
                World.MoveToNextState();
				Player.UpdateHand();

                //Убираем временные карты
                World.Cards.RemoveAll(x => x.CardType == VisualCard.SpecialType.player);

                //Убираем выбранные карты
                SelectedCards.Clear();
            }
            else
            {
                if (World.Cards.FindAll(x => x.Location == VisualCard.CurrentLocation.TableDown
                    || x.IsCurrentlySelected).Count == 0)
                    return;

				Player.SelectedCards = World.Cards.FindAll(x => x.IsCurrentlySelected).ConvertAll(x => x.card);

				Player.Prepare();
                World.MoveToNextState();
				Player.UpdateHand();
            }
			Player.SelectedCards.Clear();
            //Снимаем выделение
            World.Cards.FindAll(x => x.IsCurrentlySelected).ForEach(x => x.IsCurrentlySelected = false);

            TEXT = GetCurPhrase(
                World.State, World.IsFirstAttacking,
                World.Cards.Exists(x => x.IsCurrentlySelected), World.RoundResult == EndRound.Take);

        }
        void TrySelectCard(VisualCard SelectedCard)
        {
            if (SelectedCard == null)
                return;

            if (World.State != PlayState.ActionDefend)
            {
                var PlayerCards = World.Cards.FindAll(x => x.IsCurrentlySelected || x.Location == VisualCard.CurrentLocation.TableDown);

                var PossibleToAdd = World.Cards.Exists(x =>
                    x.card.Rank == SelectedCard.card.Rank && (
                    x.IsCurrentlySelected ||
                    x.Location == VisualCard.CurrentLocation.TableUp ||
                    x.Location == VisualCard.CurrentLocation.TableDown));

                var OpponentsCards = World.Cards.FindAll(x => x.Location == VisualCard.CurrentLocation.HandSecond ||
                    x.Location == VisualCard.CurrentLocation.TableUp);


                if (SelectedCard.IsCurrentlySelected)
                {
                    SelectedCard.IsCurrentlySelected = false;
                    return;
                }

                if ((PlayerCards.Count == 0 || PossibleToAdd)
                    && OpponentsCards.Count != PlayerCards.Count
                    && PlayerCards.Count != MTable.TotalCards)
                {
                    SelectedCard.IsCurrentlySelected = true;
                    return;
                }
                SelectedCard.PlayErrorAnimation();
                return;
            }
            if (!SelectedCard.IsCurrentlySelected)
            {
                var TrumpSuit = World.Table.GetCurrentTrump().Suit;

                int id = TablePreview.FindIndex(x => !x.Beaten
                && SCard.CanBeat(x.Down, SelectedCard.card, TrumpSuit));

                if (id == -1)
                {
                    SelectedCard.PlayErrorAnimation();
                    return;
                }

                SelectedCard.IsCurrentlySelected = true;
                //Вставляем карту в массив перед первой картой с id большим чем её
                int insertId = SelectedCards.FindIndex(x => x.Item2 >= id);
                if (insertId != -1)
                    SelectedCards.Insert(insertId, (SelectedCard.card, id));
                else
                    SelectedCards.Add((SelectedCard.card, id));


                var PseudoCard = new VisualCard(SelectedCard);
                PseudoCard.CardType = VisualCard.SpecialType.player;
                PseudoCard.IsCurrentlyHover = false;
                PseudoCard.IsCurrentlySelected = false;
                PseudoCard.MoveToTable(id, TablePreview.Count, false);

                //Устаналиваем карту наверх пары
                var pair = TablePreview[id];
                pair.SetUp(SelectedCard.card, TrumpSuit);
                TablePreview[id] = pair;

                World.Cards.Add(PseudoCard);
                return;
            }
            int cardId = SelectedCards.FindIndex((x) => x.Item1 == SelectedCard.card);

            //Удаляем временную карту
            World.Cards.RemoveAt(World.Cards.FindIndex(
                x => x.CardType == VisualCard.SpecialType.player && SelectedCard.card == x.card));

            //Сбрасываем верхнюю карту по id
            SCardPair temp = TablePreview[SelectedCards[cardId].Item2];
            temp.Down = temp.Down;
            TablePreview[SelectedCards[cardId].Item2] = temp;

            SelectedCards.RemoveAt(cardId);

            SelectedCard.IsCurrentlySelected = false;

        }
        public void SetUpWorld(PlaySubsystem newWorld)
        {
            World = newWorld;
            World.OnMovedToNewStage += NextTurn;
            World.OnPreMovedToNewStage += PreNextTurn;
        }
        public void OnMoveCardToDump(object card, EventArgs args)
        {
            var a = card as VisualCard;
            a.MoveToPreview((int)a.card, 0f);
        }
        //Обработка перехода к новой стадии
        public void NextTurn(object sender, EventArgs e)
        {
            //Нет игрока нет фраз
            if (Player == null)
            {
                TEXT = DefaultText;
                return;
            }
			Player.UpdateHand();
            //Обработка фраз - подсказок
            string str = GetCurPhrase(World.State, World.IsFirstAttacking,
                false, World.RoundResult == EndRound.Take);
            if (str != null)
                TEXT = str;
        }
        //Обработка перехода к новой стадии
        public void PreNextTurn(object sender, EventArgs e)
        {
            if (Player == null)
            {
                TEXT = DefaultText;
                return;
            }
			Player.Prepare();
        }
        public void OnClick()
        {

            if (Player == null)
            {
                TEXT = DefaultText;
                return;
            }
            if (World.IsAutoPlay())
            {
                TEXT = GetCurPhrase(
                    World.State, World.IsFirstAttacking,
                    World.Cards.Exists(x => x.IsCurrentlySelected), World.RoundResult == EndRound.Take);
                return;
            }

            bool IsAnyCardSelected = World.Cards.Exists(x => x.IsCurrentlySelected);

            if (!IsAnyCardSelected && World.State == PlayState.ActionDefend)
                TablePreview = World.Table.GetTable().ToList();

            TrySelectCard(SelectedCard);

            string str = GetCurPhrase(
                World.State, World.IsFirstAttacking,
                World.Cards.Exists(x => x.IsCurrentlySelected), World.RoundResult == EndRound.Take);
            if (str != null)
                TEXT = str;
        }
        public void UpdateSelectedCard(List<VisualCard> Cards)
        {
            if (_SelectedCard != null)
                _SelectedCard.IsCurrentlyHover = false;
            _SelectedCard = null;
            
            if (World == null)
                return;

            if (CanSelectDeck)
            {
                if (HasCardIntersection(VisualCard.DeckPosition, Pointer))
                {
                    if (!IsDeckPreviewing)
                    {
                        IsDeckPreviewing = true;
                        //Действие при наведении на стопку добора
                        var DeckPreview = Cards.FindAll(x => x.Location == VisualCard.CurrentLocation.Deck);
                        for (int i = 0; i < DeckPreview.Count; i++)
                        {
                            int id = (int)DeckPreview[i].card;
                            DeckPreview[i].MoveToPreview(id, i * 0.01f);
                        }
                    }
                }
                else
                {
                    if (IsDeckPreviewing)
                    {
                        var deck = World.Table.GetBeginDeck();

						Cards.FindAll(x => x.Location == VisualCard.CurrentLocation.Deck)
                            .ForEach(x => { x.MoveToDeck(deck.FindIndex(y=>y == x.card)); });
                    }
                    IsDeckPreviewing = false;
                }
            }

            if (CanSelectDump)
            {
                if (HasCardIntersection(VisualCard.DumpPosition, Pointer))
                {
                    if (!IsDumpPreviewing)
                    {
                        IsDumpPreviewing = true;
                        //Действие при наведении на стопку добора
                        var DumpPreview = Cards.FindAll(x => x.Location == VisualCard.CurrentLocation.Dump);
                        for (int i = 0; i < DumpPreview.Count; i++)
                        {
                            int id = (int)DumpPreview[i].card;
                            DumpPreview[i].MoveToPreview(id, i * 0.01f);
                        }
                        Cards.FindAll(x => x.Location != VisualCard.CurrentLocation.Dump).ForEach(
                            x => x.OnMoveToDump += OnMoveCardToDump);
                    }
                }
                else
                {
                    if (IsDumpPreviewing)
                    {
                        Cards.ForEach(x => x.OnMoveToDump -= OnMoveCardToDump);
                        Cards.FindAll(x => x.Location == VisualCard.CurrentLocation.Dump)
                            .ForEach(x => x.MoveToDump());
                    }
                    IsDumpPreviewing = false;
                }
            }

            if (CanSelectHandFirst)
            {
                var card = Cards.FindLast(x => HasIntersectionWithCard(Pointer, x));
                if (_SelectedCard == null && card != null && card.Location == VisualCard.CurrentLocation.HandFirst)
                {
                    _SelectedCard = card;
                    _SelectedCard.IsCurrentlyHover = true;
                }
            }

            if (CanSelectHandSecond)
            {
                var card = Cards.FindLast(x => HasIntersectionWithCard(Pointer, x));
                if (_SelectedCard == null && card != null && card.Location == VisualCard.CurrentLocation.HandSecond)
                {
                    _SelectedCard = card;
                    _SelectedCard.IsCurrentlyHover = true;
                }
            }
        }
        public void UpdatePointer(Vector2 Point)
        {
            Pointer = Point;
        }
        bool HasCardIntersection(Matrix BoxTransform, Vector2 Point)
        {
            Vector4 Pos = new Vector4(Point.X, Point.Y, 0, 1);

            var localmatrix = Matrix.Invert(BoxTransform);
            Vector4 localPosition;
            localPosition.X = Vector4.Dot(localmatrix.Column1, Pos);
            localPosition.Y = Vector4.Dot(localmatrix.Column2, Pos);

            return Math.Abs(localPosition.X) < VisualCardBase.GetBoundingBox().X
                && Math.Abs(localPosition.Y) < VisualCardBase.GetBoundingBox().Y;
        }
        public bool HasIntersectionWithCard(Vector2 point, VisualCard card)
        {
            if (card.Moving)
                return false;
            if (HasCardIntersection(card.GetMatrixCollision(), point))
                return true;
            return HasCardIntersection(card.GetMatrix(), point);
        }

        public static bool CurrentStateTurn(PlayState state, bool Player)
        {
            return state >= PlayState.PreRound &&
                state <= PlayState.ActionAttack &&
                state == PlayState.ActionDefend != Player;
        }

        public static string GetCurPhrase(PlayState state, bool IsCurrentRoundTurn, bool IsSelectedAnyCard = false, bool IsNotDefended = false)
        {
            if (!CurrentStateTurn(state, IsCurrentRoundTurn))
                return "Ход соперника";

            Dictionary<PlayState, string> Phrases = new Dictionary<PlayState, string>()
            {
                { PlayState.PreRound, "Выберите карты для атаки"},
                { PlayState.ActionDefend, "Брать?"},
                { PlayState.ActionAttack, "Бита?"},
            };
            Dictionary<PlayState, string> PhrasesOnAnyCardSelected = new Dictionary<PlayState, string>()
            {
                { PlayState.PreRound, "Атаковать"},
                { PlayState.ActionAttack, "Подкинуть"},
                { PlayState.ActionDefend, "Отбиться"},
            };
            if (IsNotDefended && state == PlayState.ActionAttack && !IsSelectedAnyCard)
                return "Подбросить?";
            return (IsSelectedAnyCard ? PhrasesOnAnyCardSelected : Phrases)[state].ToString();
        }
    }
}
