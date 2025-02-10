using CardTableFool.Tables;

namespace CardTableFool.Players
{
    public class MHumanPlayer : ICardPlayer
    {
        public MHumanPlayer(string newName)
        {
            Name = newName;
        }

        private List<SCard> hand = new List<SCard>();       // карты на руке
        private List<SCard> PrevHand = new List<SCard>();       // карты на руке
        public List<SCard> SelectedCards = new List<SCard>();   // карты выбранные для атаки
        private string Name;
        // Возвращает имя игрока
        public string GetName()
        {
            return Name + " (Вы)";
        }

        public int GetCount()
        {
            return hand.Count;
        }
        public void AddToHand(SCard card)
        {
            hand.Add(card);
        }

        public List<SCard> LayCards()
        {
            foreach (SCard card in SelectedCards)
                hand.Remove(card);
            var temp = SelectedCards.ToList();
            return temp;
        }
        public bool Defend(List<SCardPair> table)
        {
            //Индекс пары на столе
            int j = 0;
            //Идём по всем картам
            for(int i = 0; i < SelectedCards.Count; i++)
            {
                var cardToDefend = SelectedCards[i];

                //Если карта уже бита или мы не можем её побить то переходим к следующему
                while (table[j].Beaten || !SCard.CanBeat(table[j].Down, cardToDefend, Trump.Suit))
                    j++;

                //Записываем карту в массив
                var temp = table[j];
                temp.SetUp(cardToDefend, Trump.Suit);
                table[j] = temp;

                //Очищаем руку
                hand.Remove(cardToDefend);
            }
            //Проверяем что мы действительно отбились
            return hand.Count == 0 || table.Count(x => !x.Beaten) == 0;
        }

        public bool AddCards(List<SCardPair> table, bool IsDefenced)
        {
            if (SelectedCards.Count == 0)
                return false;

            foreach (SCard card in SelectedCards)
                hand.Remove(card);

            table.AddRange(SCardPair.CardsToCardPairs(SelectedCards));
            return true;
        }
        public void UpdateHand()
        {
            PrevHand = hand.ToList();
        }
        public void Prepare()
        {
            hand = PrevHand.ToList();
        }
        SCard Trump;
        public void SetTrump(SCard NewTrump)
        {
            Trump = NewTrump;
        }
        public void Reset()
        {
            hand = new List<SCard>();       // карты на руке
            PrevHand = new List<SCard>();       // карты на руке
            SelectedCards = new List<SCard>();   // карты выбранные для атаки
        }
        public ICardPlayer Copy()
        {
            throw new NotImplementedException();
		}
		public void OnEndRound(List<SCardPair> table, bool IsDefenceSuccesful)
		{

		}
	}
}
