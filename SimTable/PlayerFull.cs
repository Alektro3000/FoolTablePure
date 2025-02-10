

namespace CardFool
{
    public class CardPlayer
    {
        private string Name = "Norm";
        private List<SCard> hand = new List<SCard>();       // карты на руке
        private int DumpCards = 0;

        public CardPlayer(string newName)
        {
            Name = newName;
        }

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
            hand.Add(card);
        }

        //Начальная атака
        public List<SCard> LayCards()
        {
            var a = hand[0];
            hand.RemoveAt(0);
            return [a];
        }

        //Защита от карт
        public bool Defend(List<SCardPair> table)
        {
            for (int i = 0; i < table.Count(); ++i)
                if (!table[i].Beaten)
                {
                    var Candidates = hand.Where(card => SCard.CanBeat(table[i].Down, card, Trump.Suit));
                    if (Candidates.Count() == 0)
                        return false;

                    //Сортировка карт от меньших к большим
                    Candidates = Candidates.OrderBy(x => x.Rank + (x.Suit == Trump.Suit ? 10 : 0));

                    var a = table[i];
                    a.SetUp(Candidates.First(), Trump.Suit);
                    table[i] = a;

                    hand.Remove(Candidates.First());
                }
            return true;
        }
        //Добавление карт
        public bool AddCards(List<SCardPair> table, bool OpponentDefenced)
        {
            var OpponentCardCount = Math.Min(6,36 - DumpCards - table.Count() - hand.Count()) - table.Count();

            var Candidates = hand.FindAll(card => table.Any(pair=>SCardPair.CanBeAddedToPair(card, pair)));

            if (Candidates.Count - OpponentCardCount > 0)
                Candidates.RemoveRange(OpponentCardCount, Candidates.Count - OpponentCardCount);
            
            hand.RemoveAll(Candidates.Contains);
            table.AddRange(SCardPair.CardsToCardPairs(Candidates));

            return Candidates.Count > 0;
        }
        //Вызывается после всех циклов атаки/обороны, но до добора карт из колоды
        public void OnEndRound(List<SCardPair> table, bool IsDefenceSuccesful)
        {
            if (IsDefenceSuccesful)
                DumpCards += table.Count * 2;
        }
        SCard Trump;
        public void SetTrump(SCard NewTrump)
        {
            Trump = NewTrump;
        }
    }
}
