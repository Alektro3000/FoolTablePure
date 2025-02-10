

namespace CardFool
{
    public class MPlayer1
    {
        private string Name = "First";
        private List<SCard> hand = new List<SCard>();       // карты на руке

        // Возвращает имя игрока
        public string GetName()
        {
            return Name;
        }
        //Возвращает количество карт на руке
        public int GetCount()
        {
            return 0;
        }
        //Добавление карты в руку, во время добора из колоды, или взятия карт
        public void AddToHand(SCard card)
        {

        }

        //Начальная атака
        public List<SCard> LayCards()
        {
            return [];
        }

        //Защита от карт
        //На вход подается набор карт на столе, часть из них могут быть уже покрыты
        public bool Defend(List<SCardPair> table)
        {
            return false;
        }
        //Добавление карт
        //На вход подается набор карт на столе, а также отбился ли оппонент
        public bool AddCards(List<SCardPair> table, bool OpponentDefenced)
        {
            return false;
        }
        //Вызывается после основной битвы когда известно отбился ли защищавшийся
        //На вход подается набор карт на столе, а также была ли успешной защита
        public void OnEndRound(List<SCardPair> table, bool IsDefenceSuccesful)
        {

        }
        //Установка козыря, на вход подаётся козырь
        public void SetTrump(SCard NewTrump)
        {

        }
    }
}
