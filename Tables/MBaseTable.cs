
namespace CardTableFool.Tables
{
    /// <summary>
    /// Базовый стол предоставляющий интерфейс для использования столов
    /// </summary>
    public abstract class MBaseTable
    {
        /// <summary>
        /// Получить имя игрока
        /// </summary>
        /// <param name="first">True: получить имя первого, False: получить имя второго</param>
        public virtual string GetPlayerName(bool first)
        {
            return "Null";
        }
        /// <summary>
        /// Получить карты в руке игрока, карты в фазе атак/защиты даются на состояние RoundBegin
        /// </summary>
        /// <param name="first">True: получить руку первого, False: получить руку второго</param>
        public virtual List<SCard> GetPlayerHand(bool first)
        {
            return null;
		}
		public virtual List<SCard> GetBeginDeck()
		{
            return [];
		}
		public virtual SCard GetCurrentTrump()
        {
            return new SCard();
        }
        /// <summary>
        /// Инициализация игры
        /// </summary>
        public virtual void Initialize()
        {

        }
        /// <summary>
        /// Процесс инициации хода - первого хода атакующего
        /// </summary>
        public virtual void PlayPreRound()
        {

		}


		/// <summary>
		/// Обработка защиты обороняющегося
		/// </summary>
		/// <returns>Возвращает Take если игрок принял карты, иначе Continue </returns>
		public virtual EndRound PlayDefence()
        {
            return EndRound.Take;
        }

        /// <summary>
        /// Обработка нападения атакующего
        /// </summary>
        /// <returns>Если обоняющийся принял карты: Take
        /// Если нападающий добавил карты: Continue
        /// Иначе: Defence</returns>
        public virtual EndRound PlayAttack(EndRound DefenceResult)
        {
            return EndRound.Defend;
        }
        /// <summary>
        /// Обработка добора карт и перехода хода, а также условия завершения игры
        /// </summary>
        public virtual EndGame PlayPostRound(EndRound RoundResult)
        {
            return EndGame.Draw;
        }

        public virtual bool GetPlayerTurn()
        {
            return false;
        }
        public virtual List<SCardPair> GetTable()
        {
            return [];
        }
    }
}
