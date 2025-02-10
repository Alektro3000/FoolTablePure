using CardTableFool.Players;
using CardTableFool.Main;
using CardTableFool.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.Design.AxImporter;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace CardTableFool.Tables
{
    /// <summary>
    /// Модификация MTable записывающая результат матча в лог
    /// </summary>
    internal class MLoggingTable : MTable
    {
        GameRecord _record;
        public GameRecord Record
        {
            get => _record;
        }

        RoundRecord RoundRecord;

        public MLoggingTable(ICardPlayer NewPlayer1, ICardPlayer NewPlayer2, bool IsFirstAttacking = true)
            : base(NewPlayer1, NewPlayer2, IsFirstAttacking)
        {
        }
        public MLoggingTable(GameRecord Record,ICardPlayer NewPlayer1, ICardPlayer NewPlayer2)
            : base(NewPlayer1, NewPlayer2, Record.IsFirstStart, Record.InitialDeck)
        {
        }
		protected override void InitDeck()
        {
            base.InitDeck();
            _record = new GameRecord();
            _record.InitialDeck = deck.ToList();
            _record.Player1Name = GetPlayerName(true);
            _record.Player2Name = GetPlayerName(false);
            _record.IsFirstStart = IsFirstPlayerAttacking;
        }
        bool PreRound = false;
        public override void PlayPreRound()
        {
            _record.Trump = GetCurrentTrump();
            RoundRecord = new RoundRecord();
            RoundRecord.PlayerHand1 = GetPlayerHand(true).ToList();
            RoundRecord.PlayerHand2 = GetPlayerHand(false).ToList();
            RoundRecord.IsFirstAttacking = GetPlayerTurn();
            PreRound = true;
                base.PlayPreRound();
            PreRound = false;
                var a = GetTable();
                RoundRecord.InitialAttack = new List<SCard>(a.Count);
                for (int i = 0; i < a.Count; i++)
                    RoundRecord.InitialAttack.Add(a[i].Down);

        }
        protected override void OnError(GameException error)
        {
            if (PreRound)
            {
                RoundRecord.InitialAttack = error.Table.ConvertAll(x => x.Down);
                RoundRecord.PostAttackSnapshots = [error.Table];
                RoundRecord.PostDefenceSnapshots = [error.Table];
            }
            else
            {
                RoundRecord.PostAttackSnapshots.Add(error.Table);
                RoundRecord.PostDefenceSnapshots.Add(error.Table);
            }
            RoundRecord.Result = EndRound.Error;
            _record.rounds.Add(RoundRecord);
            _record.rounds.Add(RoundRecord);

            _record.result = EndGame.Error;
            if (error != null)
            {
                _record.ErrorCode = error.Message;
                _record.IsFirstCausedError = error.IsFirstPlayerError;
            }
            else
            {
                _record.ErrorCode = "Error with bot code";
            }

        }
        public override EndRound PlayDefence()
        {
            var a = base.PlayDefence();
            RoundRecord.PostDefenceSnapshots.Add(Table.ToList());
            return a;  
        }
        public override EndRound PlayAttack(EndRound DefenceResult)
        {
            var a = base.PlayAttack(DefenceResult);
            RoundRecord.PostAttackSnapshots.Add(Table.ToList());
            return a;
        }
        public override EndGame PlayPostRound(EndRound RoundResult)
        {
            RoundRecord.Result = RoundResult;
            var a = base.PlayPostRound(RoundResult);
            _record.rounds.Add(RoundRecord);
            if (a != EndGame.Continue)
            {
                //Добавляем информацию на начало следующего хода
                //Равна информации на конец текущего
                RoundRecord = new RoundRecord();
                RoundRecord.PlayerHand1 = GetPlayerHand(true).ToList();
                RoundRecord.PlayerHand2 = GetPlayerHand(false).ToList();
                RoundRecord.IsFirstAttacking = GetPlayerTurn();

                _record.rounds.Add(RoundRecord);

                _record.result = a;
            }

            return a;
        }
        /// <summary>
        /// Проводит пачку матчей и выводит результат в виде BatchRecord
        /// </summary>
        /// <param name="Count">Количество матчей</param>
        /// <returns></returns>
        public static BatchRecord ProcessBatchTesting(ICardPlayer player1, ICardPlayer player2, int Count)
        {
            int CountToSwitch = Program.GetIntSetting(Program.GlobalSetting.SwitchCount);

            List<GameRecord> ans = new(new GameRecord[Count]);

            for (int i = 0; i < Count; i++)
            {
                player1.Reset();
                player2.Reset();

                MLoggingTable table = new MLoggingTable(player1, player2, (i % CountToSwitch * 2) < CountToSwitch);

                table.PlayGameUnstoppable();
                ans[i] = table.Record;
            }

            return new BatchRecord(ans, player1, player2);
        }
    }
}
