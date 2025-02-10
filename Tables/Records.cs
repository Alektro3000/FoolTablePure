using CardTableFool.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using CardTableFool.Players;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace CardTableFool.Tables
{
    public class CardConverter : JsonConverter<SCard>
    {
        public override SCard Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => SCard.Parse(reader.GetString()!);

        public override void Write(
            Utf8JsonWriter writer,
            SCard card,
            JsonSerializerOptions options) => writer.WriteStringValue(card.ToString());
    }
    public class CardPairConverter : JsonConverter<SCardPair>
    {
        public override SCardPair Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => SCardPair.Parse(reader.GetString()!);

        public override void Write(
            Utf8JsonWriter writer,
            SCardPair card,
            JsonSerializerOptions options) => writer.WriteStringValue(card.ToString());
    }

    //Запись раунда
    public struct RoundRecord
    {
        public bool IsFirstAttacking { get; set; }
        /// <summary>
        /// Рука игрока в начале хода
        /// </summary>
        public List<SCard> PlayerHand1 { get; set; }
        /// <summary>
        /// Рука игрока в начале хода
        /// </summary>
        public List<SCard> PlayerHand2 { get; set; }
        public List<SCard> InitialAttack { get; set; }

        /// <summary>
        /// Снимки стола после каждой защиты
        /// </summary>
        public List<List<SCardPair>> PostDefenceSnapshots { get; set; }
        /// <summary>
        /// Снимки стола после каждой атаки
        /// </summary>
        public List<List<SCardPair>> PostAttackSnapshots { get; set; }

        public EndRound Result { get; set; }

        public RoundRecord()
        {
            PostDefenceSnapshots = [];
            PostAttackSnapshots = [];
        }
    }
    /// <summary>
    /// Запись игры
    /// </summary>
    public struct GameRecord
    {
        /// <summary>
        /// Текст ошибки, если ошибки не было: null
        /// </summary>
        public string ErrorCode { get; set; }
        public bool IsFirstCausedError { get; set; }

        public GameRecord()
        {
            rounds = new List<RoundRecord>();
            InitialDeck = new List<SCard>();
        }
        public SCard Trump { get; set; }
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }

        public bool IsFirstStart { get; set; }
        public List<SCard> InitialDeck { get; set; }
        /// <summary>
        /// Все записанные раунды + один завершающий
        /// </summary>
        public List<RoundRecord> rounds { get; set; }
        public int Score1 { get => ErrorCode == null ? rounds[rounds.Count-1].PlayerHand1.Count : 0; }
        public int Score2 { get => ErrorCode == null ? rounds[rounds.Count - 1].PlayerHand2.Count : 0; }
        public EndGame result { get; set; }
    }
    public struct BatchRecord
    {
        public ICardPlayer Player1;
        public ICardPlayer Player2;

        public int TotalScore1 { get; set; }
        public int TotalScore2 { get; set; }

        public int TotalWins1 { get; set; }
        public int TotalWins2 { get; set; }

        public int FirstWin { get; set; }
        public int TotalErrors1 { get; set; }
        public int TotalErrors2 { get; set; }
        public int ErrorCount { get; set; }
        public List<GameRecord> Records { get; set; }
        public BatchRecord()
        {
            Records = [];
        }
        public BatchRecord(List<GameRecord> records, ICardPlayer player = null, ICardPlayer player2 = null)
        {
            Records = records;
            Player1 = player;
            Player2 = player2;


            TotalScore1 = Records.Sum(x => x.Score1);
            TotalScore2 = Records.Sum(x => x.Score2);

            TotalWins1 = Records.FindAll(x => x.result == EndGame.First).Count;
            TotalWins2 = Records.FindAll(x => x.result == EndGame.Second).Count;

            TotalErrors1 = Records.Sum(x=>(x.ErrorCode != null && x.IsFirstCausedError ? 1 : 0));
            TotalErrors2 = Records.Sum(x => (x.ErrorCode != null && !x.IsFirstCausedError ? 1 : 0));

            ErrorCount = Records.FindAll(x => x.ErrorCode != null).Count;

            FirstWin = TotalErrors2 == TotalErrors1 ? -Comparer<int>.Default.Compare(TotalScore1,TotalScore2) 
                : -Comparer<int>.Default.Compare(TotalErrors1, TotalErrors2);

        }
        public override string ToString()
        {
            return Records[0].Player1Name + " vs " + Records[0].Player2Name +
                " with total score " + TotalScore1 + " vs " + TotalScore2 +
                " Wins: " + TotalWins1 + " vs " + TotalWins2 +
                " with total errors: " + ErrorCount;
        }
        static JsonSerializerOptions OptionsCompr = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true,
            Converters =
                {
                    new CardConverter(),
                    new CardPairConverter(),
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
        };
        static JsonSerializerOptions OptionsUnCompr = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true,
            Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
        };
        public string ToJson()
        {
            string an = JsonSerializer.Serialize(this, Program.GetBoolSetting(Program.GlobalSetting.CompressedJson) 
                ? OptionsCompr: OptionsUnCompr);
            return an;
        }
        public static BatchRecord FromJson(string Input)
        {
            var an = JsonSerializer.Deserialize<BatchRecord>(Input, 
                Input.Contains("\"Beaten\"") ? OptionsUnCompr : OptionsCompr);
            return an;
        }
    }
}
