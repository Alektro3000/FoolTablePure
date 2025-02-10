using CardTableFool.Tables;

namespace CardTableFool.Players
{
    internal class MSimplePlayer : ICardPlayer
    {
        public MSimplePlayer()
        {
            Name = "21";
            Reset();
        }
        public MSimplePlayer(string newName)
        {
            Name = newName;
            Reset();
        }

        private List<SCard> hand = new List<SCard>();       // карты на руке

        private int OpponentCardCount;
        private List<SCardPair> SavedTable;
        private HashSet<SCard> CardsInTable;
        private HashSet<SCard> KnownOpponentCards;
        private HashSet<SCard> DestroyedCards;
        private bool IsDefending;

        private string Name;
        // Возвращает имя игрока
        public string GetName()
        {
            return Name;
        }

        public int GetCount()
        {
            return hand.Count;
        }
        public void AddToHand(SCard card)
        {
            CardsInTable.Remove(card);
            KnownOpponentCards.Remove(card);
            hand.Add(card);
        }

        public List<SCard> LayCards()
        {
            if (IsDefending)
            {
                //Attack after Defence (you defended)
                DestroyedCards.UnionWith(SCardPair.CardPairsToCards(SavedTable.FindAll(x => x.Beaten)));
                CardsInTable.RemoveWhere(x => SavedTable.Exists(y => y.Down == x));
                //Убираем все карты от которых мы отбились
                KnownOpponentCards.RemoveWhere(x => SavedTable.Exists(y => y.Down == x && y.Beaten));
                //Добавляем все карты которые вернулись к нему в руку
                KnownOpponentCards.UnionWith(SavedTable.FindAll(y => !y.Beaten).ConvertAll(x => x.Down));
            }
            else
            {
                //Attack after Attack (he took cards)
                CardsInTable.RemoveWhere(x => SavedTable.Exists(y => y.Up == x));
                KnownOpponentCards.UnionWith(SCardPair.CardPairsToCards(SavedTable));
            }
            IsDefending = false;

            OpponentCardCount = Math.Min(6, KnownOpponentCards.Count + CardsInTable.Count);
            return RealLayCards();
        }

        public bool Defend(List<SCardPair> table)
        {
            if (IsDefending)
            {
                //Defence after Defence (you took cards)
                //Handled by AddCard
            }
            else
            {
                //Defence after Attack (opponent defenced)
                DestroyedCards.UnionWith(SCardPair.CardPairsToCards(SavedTable.FindAll(x => x.Beaten)));
                KnownOpponentCards.RemoveWhere(x => SavedTable.Exists(y => y.Up == x));
                CardsInTable.RemoveWhere(x => SavedTable.Exists(y => y.Up == x));

            }

            IsDefending = true;
            var temp = RealDefend(table);
            SavedTable = table.ToList();
            return temp;
        }

        public bool AddCards(List<SCardPair> table, bool IsOpponentDefenced)
        {
            OpponentCardCount = Math.Min(6, KnownOpponentCards.Count + CardsInTable.Count);
            var temp = RealAddCards(table);
            SavedTable = table.ToList();
            return temp;
        }
        SCard Trump;
        public void SetTrump(SCard NewTrump)
        {
            Trump = NewTrump;
        }
        public void Reset()
        {
            hand = new List<SCard>();
            CardsInTable = SCard.GetDeck().ToHashSet();
            KnownOpponentCards = [];
            SavedTable = [];
            DestroyedCards = [];
        }
        private List<SCard> RealLayCards()
        {
            var a = hand[0];
            var di = hand.FindAll(x => x.Rank == a.Rank);
            if (di.Count > OpponentCardCount)
                di.RemoveRange(0, di.Count - OpponentCardCount);

            hand.RemoveAll(di.Contains);
            return di;
        }
        private bool RealAddCards(List<SCardPair> table)
        {
            if (table.Count == OpponentCardCount)
                return false;

            bool fl = false;
            int n = table.Count;
            //Идём в обратном порядке, потому что мы будем удалять элементы
            for (int i = hand.Count - 1; i >= 0; i--)
                if (table.Any(x => SCardPair.CanBeAddedToPair(hand[i], x)))
                {
                    table.Add(new SCardPair(hand[i]));
                    hand.RemoveAt(i);
                    if (table.Count == OpponentCardCount)
                        return true;
                    fl = true;
                    break;
                }

            return fl;
        }
        private bool RealDefend(List<SCardPair> table)
        {
            if (table.Count == 0)
                return true;

            for (int i = 0; i < table.Count; ++i)
                if (!table[i].Beaten)
                {
                    bool fl = true;
                    SCardPair a = new(table[i].Down);
                    for (int j = hand.Count - 1; j >= 0; j--)
                    {
                        a.SetUp(hand[j], Trump.Suit);
                        if (a.Beaten)
                        {
                            hand.RemoveAt(j);
                            table[i] = a;
                            fl = false;
                            break;
                        }

                    }
                    if (hand.Count == 0)
                        break;
                    if (fl)
                        return false;
                }
            return true;
        }
        public ICardPlayer Copy()
        {
            return new MSimplePlayer(Name);
        }

		public void OnEndRound(List<SCardPair> table, bool IsDefenceSuccesful)
		{
			
		}
	}
}
