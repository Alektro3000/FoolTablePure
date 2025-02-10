using CardTableFool.Tables;

namespace CardTableFool.Players
{
    public interface ICardPlayer
    {
        // Возвращает имя игрока
        public string GetName();

        // количество карт на руке
        public int GetCount();
        // Добавляет новую карту в руку
        public void AddToHand(SCard card);
        // Сделать ход (первый)
        public List<SCard> LayCards();
        // Отбиться.
        // На вход подается набор карт на столе, часть из них могут быть уже покрыты
        public bool Defend(List<SCardPair> table);
        // Подбросить карты
        // На вход подаются карты на столе
        public bool AddCards(List<SCardPair> table, bool IsOpponentDefenced);
        //Вызывается после основной битвы когда известно отбился ли защищавшийся
        //На вход подается набор карт на столе, а также была ли успешной защита
        public void OnEndRound(List<SCardPair> table, bool IsDefenceSuccesful);
		public void SetTrump(SCard NewTrump);
        //Возвращает в исходное состояние
        public void Reset();
        //Получает копию, которая установлена в исходное состояние(не используется)
        public ICardPlayer Copy();
    }
}
