using CardTableFool.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardTableFool.Tables
{
    /// <summary>
    /// Воспроизводит игру из записи матча
    /// </summary>
    internal class MPlaybackTable : MBaseTable
    {
        public GameRecord Record { get; private set; }
        int curRound = -1;
        int curAction = -1;
        bool IsDefenceMode;
        public MPlaybackTable(GameRecord record)
        {
            Record = record;
        }

		public override List<SCard> GetBeginDeck()
		{
			return Record.InitialDeck;
		}
		public override string GetPlayerName(bool first)
        {
            return first ? Record.Player1Name : Record.Player2Name;
        }
        public override List<SCard> GetPlayerHand(bool first)
        {
            var action = Record.rounds[curRound];
            /*
            if (curAction == Record.rounds[curRound].Snapshots.Count+1)
            {
                action = Record.rounds[curRound+1];
                return first ? action.PlayerHand1 : action.PlayerHand2;
            }*/
            return first ? action.PlayerHand1 : action.PlayerHand2;
        }
        public override void Initialize()
        {
            curRound = 0;
        }
        public override void PlayPreRound()
        {
            curAction = -1;
        }

        public override EndRound PlayDefence()
        {
            curAction++;

            IsDefenceMode = true;
            if (curAction < Record.rounds[curRound].PostAttackSnapshots.Count - 1)
                return EndRound.Continue;
            return Record.rounds[curRound].Result;
        }
        public override EndRound PlayAttack(EndRound DefenceResult)
        {
            IsDefenceMode = false;
            if (curAction < Record.rounds[curRound].PostAttackSnapshots.Count - 1)
                return EndRound.Continue;
            return Record.rounds[curRound].Result;
        }
        public override EndGame PlayPostRound(EndRound RoundResult)
        {
            curRound++;
            if (curRound < Record.rounds.Count-1)
                return EndGame.Continue;
            return Record.result;
        }
        public override bool GetPlayerTurn()
        {
            return Record.rounds[curRound].IsFirstAttacking;
        }
        public override List<SCardPair> GetTable()
        {
            if (curAction != -1)
                return (IsDefenceMode ? 
                    Record.rounds[curRound].PostDefenceSnapshots : 
                    Record.rounds[curRound].PostAttackSnapshots)[curAction];

            return SCardPair.CardsToCardPairs(Record.rounds[curRound].InitialAttack);
        }
        public override SCard GetCurrentTrump()
        {
            return Record.Trump;
        }
    }
}
