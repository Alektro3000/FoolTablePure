using CardTableFool.Main;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CardTableFool.Tables
{
    /// <summary>
    /// Колода
    /// </summary>
    public enum Suits { Hearts, Diamonds, Clubs, Spades };   // черви, бубны, крести, пики

    /// <summary>
    /// Карта
    /// </summary>
    public struct SCard : IComparable
    {
        [JsonInclude]
        public Suits Suit { get; private set; }    // масть, от 0 до 3
        [JsonInclude]
        public int Rank { get; private set; }      // величина, от 6 до 14

        public SCard(Suits suit, int rank)
        {
            Suit = suit;
            Rank = rank;
        }
        const string suit = "чбкп";
        const string rank = "-1234567890ВДКТ";
        public override string ToString()
        {
            if (Rank == 0) return "--";
            if (Rank == 10)
                return suit[(int)Suit] + "10";
            return suit[(int)Suit].ToString() + rank[Rank];
        }
        public static SCard Parse(string Input)
        {
            Suits a = (Suits)suit.IndexOf(Input[0]);
            if (Input.Length == 3)
                return new SCard(a, 10);
            return new SCard(a, rank.IndexOf(Input[1]));
        }
        //Сравнение карт на равенство
        public static bool operator ==(SCard right, SCard left)
        {
            return right.Suit == left.Suit && right.Rank == left.Rank;
        }
        public static bool operator !=(SCard right, SCard left)
        {
            return !(right == left);
        }
        //В число от 0 до 53 (сначала 6 всех мастей, потом 7, потом ...)
        public static explicit operator int(SCard d)
        {
            return (int)d.Suit + (d.Rank - 6) * 4;
        }
        //Из числа от 0 до 53 (сначала 6 всех мастей, потом 7, потом ...)
        public static explicit operator SCard(int d)
        {
            return new SCard((Suits)(d % 4), d / 4 + 6);
        }
        public static bool CanBeat(SCard down, SCard up, Suits trump)
        {
            //Если карта козырь, она бьёт любую некозырную
            if (up.Suit == trump && down.Suit != trump)
                return true;
            //Иначе обычные правила:
            return down.Suit == up.Suit && down.Rank < up.Rank;
        }
        //Сравнение для сортировки
        public int CompareTo(object another)
        {
            var an = another as SCard?;
            if (an == this)
                return 0;
            if ((int)an < (int)this)
                return -1;
            return 1;
        }
        //Создание всей колоды
        public static List<SCard> GetDeck()
        {
            List<SCard> temp = [];
            for (Suits c = 0; c <= Suits.Spades; c++)
                for (int d = 6; d <= 14; d++)
                {
                    SCard card = new SCard(c, d);
                    temp.Add(card);
                }
            return temp;
        }

    }
    /// <summary>
    /// Пара карт на столе
    /// </summary>
    public struct SCardPair : IComparable
    {
        private SCard _down;    // карта снизу
        private SCard _up;      // карта сверху
        public bool Beaten { get; private set; }   // признак бита карта или нет

        //Получение или установка нижней карты
        public SCard Down
        {
            get { return _down; }
            set { _down = value; Beaten = false; }
        }
        //Верхняя карта
        public SCard Up
        {
            get { return _up; }
        }
        //Установка верхней
        public bool SetUp(SCard up, Suits trump)
        {
            if (SCard.CanBeat(_down, up, trump))
            {
                _up = up;
                Beaten = true;
                return true;
            }
            return false;
        }
        //Конструктор из нижней карты
        public SCardPair(SCard down)
        {
            _down = down;
            _up = new SCard(0, 0);
            Beaten = false;
        }
        //Конструктор из нижней карты
        private SCardPair(SCard down, SCard up) : this(down)
        {
            Beaten = up.Rank != 0;
            _up = up;
        }
        public override string ToString()
        {
            return _down.ToString() + " vs " + _up.ToString();
        }
        public static SCardPair Parse(string Input)
        {
            var a = Input.Split(" vs ");
            return new(SCard.Parse(a[0]), SCard.Parse(a[1]));
        }

        /// <summary>
        /// Преобразование всех карт в список пар с указанной нижней картой
        /// </summary>
        public static List<SCardPair> CardsToCardPairs(List<SCard> cards)
           => cards.ConvertAll(x => new SCardPair(x));
        /// <summary>
        /// Преобразование всех пар в список карт
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public static List<SCard> CardPairsToCards(List<SCardPair> cards)
        {
            List<SCard> ans = [];
            foreach (var a in cards)
            {
                ans.Add(a.Down);
                if (a.Beaten)
                    ans.Add(a.Up);
            }
            return ans;
        }

        public static bool operator ==(SCardPair right, SCardPair left)
        {
            return right.Down == left.Down //Обе имеют одинаковые нижние карты
                && right.Beaten == left.Beaten //Обе биты/или нет
                && (!right.Beaten || right.Up == left.Up); //Если биты то верхние карты равны
        }
        public static bool operator !=(SCardPair right, SCardPair left)
        {
            return !(right == left);
        }
        public static bool operator <(SCardPair right, SCardPair left)
        {
            return (int)right.Down < (int)left.Down //Нижняя карта меньше
                || !right.Beaten || left.Beaten   //Правая небита, в то время как левая бита
                || (int)right.Up < (int)left.Up;  //Верхняя карта меньше
        }
        public static bool operator >(SCardPair right, SCardPair left)
        {
            return left < right;
        }
        public int CompareTo(object another)
        {
            var an = another as SCardPair?;
            if (an == this)
                return 0;
            if (an < this)
                return -1;
            return 1;
        }
        //Проверка может быть карта доброшена к этой паре
        public static bool CanBeAddedToPair(SCard newCard, SCardPair pair)
        {
            //Только для отладки
            if (newCard.Rank == pair.Down.Rank)
                return true;
            return pair.Beaten && newCard.Rank == pair.Up.Rank;
        }

    }

    /// <summary>
    /// Результат игры
    /// </summary>
    public enum EndGame { Error, First, Second, Draw, Continue };
    /// <summary>
    /// Результат пары защиты/атаки
    /// </summary>
    public enum EndRound { Continue, Take, Defend, Error };

}
