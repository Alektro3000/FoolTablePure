using System.Reflection;
using CardTableFool.Tables;
using CardTableFool.Main;

namespace CardTableFool.Players
{
    //Игрок который ответственнен за создание бота из путя к dll сборке

    internal class MCustomPlayer : CustomPlayerBase, ICardPlayer
    {
        private MCustomPlayer(MCustomPlayer player) : base(player)
        {

        }

        public MCustomPlayer(string PathToCode, bool FromDll = false) : base (PathToCode, FromDll) 
        {

        }
		public override string ToString() =>  GetName();
		
		// Возвращает имя игрока
		public string GetName()
        {
            //Собираем метод используя класс игрока, и вызываем его
            //результат конвертируем в string
            string ans = (string)PlayerClass.GetMethod("GetName").Invoke(Player, []);
            if (ans == null)
                return "Null";
            if (ans == "")
                return " ";
            return Program.GetBoolSetting(Program.GlobalSetting.ShowSurnames) ? DirectoryName.Split()[0] : ans;
        }

        public int GetCount()
        {
            //Собираем метод используя класс игрока, и вызываем его
            //результат конвертируем в int
            return (int)PlayerClass.GetMethod("GetCount").Invoke(Player, []);
        }

        public void AddToHand(SCard card)
        {
            PlayerClass.GetMethod("AddToHand").Invoke(Player, [ConvertCard(card)]);
        }

        public List<SCard> LayCards()
        {
            object ret = PlayerClass.GetMethod("LayCards").Invoke(Player, []);
            List<SCard> returnVal = [];

            var retType = PlayerClass.GetMethod("LayCards").ReturnType;
            int n = (int)retType.GetMethod("get_Count").Invoke(ret, []);

            for (int i = 0; i < n; i++)
                returnVal.Add(ConvertCard(retType.GetMethod("get_Item").Invoke(ret, [i])));

            return returnVal;
        }

        public bool Defend(List<SCardPair> table)
        {

            var arr = ConvertPairList(table);
            object ret = PlayerClass.GetMethod("Defend").Invoke(Player, [arr]);

            for (int i = 0; i < table.Count; i++)
                table[i] = (ConvertPair(CardPairListClass.GetMethod("get_Item").Invoke(arr, [i])));
            return (bool)ret;
        }

        public bool AddCards(List<SCardPair> table, bool IsOpponentDefenced)
        {
            var arr = ConvertPairList(table);
            var method = PlayerClass.GetMethod("AddCards");
            object ret;

            //Если у фукнции только один параметр то не передаём булевый параметр
			if (method.GetParameters().Length == 1)
			  ret = method.Invoke(Player, [arr]);
			else
			  ret = method.Invoke(Player, [arr, IsOpponentDefenced]);


			for (int i = 0; i < table.Count; i++)
                table[i] = (ConvertPair(CardPairListClass.GetMethod("get_Item").Invoke(arr, [i])));
            int n = (int)CardPairListClass.GetMethod("get_Count").Invoke(arr, []);

            for (int i = table.Count; i < n; i++)
                table.Add(ConvertPair(CardPairListClass.GetMethod("get_Item").Invoke(arr, [i])));

            return (bool)ret;
		}
		public void OnEndRound(List<SCardPair> table, bool IsDefenceSuccesful)
		{
            //Если фукнции нет, то её вызов приведёт к ошибке
            if (PlayerClass.GetMethod("OnEndRound") == null)
                return;

			var arr = ConvertPairList(table);
			PlayerClass.GetMethod("OnEndRound").Invoke(Player, [arr, IsDefenceSuccesful]);
		}

        FieldInfo TrumpField;
        public void SetTrump(SCard NewTrump)
        {
            Trump = NewTrump;

            //Если найден стол, то пытаемся записать по старому API
            if (TableClass != null)
            {
                try
                {
                    //Получаем закрытое поле хранящее козырь
                    TrumpField = TableClass.GetFields(BindingFlags.NonPublic | BindingFlags.Static).First(x => x.Name == "trump");

                    //Записываем переменную
                    TrumpField.SetValue(null, ConvertCard(NewTrump));
                }
                catch
                {

                }
			}
			if (PlayerClass.GetMethod("SetTrump") == null)
				return;
			PlayerClass.GetMethod("SetTrump").Invoke(Player, [ConvertCard(NewTrump)]);
			
		}

        public void Reset()
        {
            //Создаём игрока по новой
            Player = assembly.CreateInstance(PlayerClass.FullName);
            foreach (var item in assembly.GetTypes())
            {
                foreach (var item1 in item.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    
                }
            }
        }
        public ICardPlayer Copy()
        {
            return new MCustomPlayer(this);
        }
    }
}
