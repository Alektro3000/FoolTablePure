using CardTableFool;
using CardTableFool.Main;
using CardTableFool.Players;
using CardTableFool.Tables;


namespace CardTableFool
{
    public partial class ResultForm : Form
    {
        public event EventHandler<GameEventArgs> ShowGameRecord;
        public event EventHandler<GameEventArgs> RepeatGameFromRecord;

        public bool ShowLoadButton;
        public ResultForm(bool ShowLoad)
        {
            ShowLoadButton = ShowLoad;
            InitializeComponent();

            GameInfos.GameRecordRecall += OnShowGame;
            GameInfos.GameRecordRepeat += OnRepeatGame;

            GameInfos.MouseWheel += (object k, MouseEventArgs e) =>
            {
                UpdateGameScroller();
                int offset = (1 + (Math.Abs(e.Delta) - 1) / GameScrollBar.Maximum);
                int NewVal = GameScrollBar.Value - Math.Sign(e.Delta) * GameScrollBar.SmallChange * offset;
                GameScrollBar.Value = Math.Clamp(NewVal, 0, GameScrollBar.Maximum - GameScrollBar.LargeChange + 1);

                GameInfos.SetNewOffset(GameScrollBar.Value);
            };

            BatchRecords.RecallBatch += OnShowBatch;
            BatchRecords.SaveBatch_Click += OnSaveBatch;
            BatchRecords.LoadBatch_Click += OnLoadBatch;

            BatchRecords.MouseWheel += (object k, MouseEventArgs e) =>
            {
                UpdateBatchScroller();
                int offset = (1 + (Math.Abs(e.Delta) - 1) / BatchScrollBar.Maximum);
                int NewVal = BatchScrollBar.Value - Math.Sign(e.Delta) * BatchScrollBar.SmallChange * offset;
                BatchScrollBar.Value = Math.Clamp(NewVal, 0, BatchScrollBar.Maximum - BatchScrollBar.LargeChange+1);

                BatchRecords.SetNewOffset(BatchScrollBar.Value);
            };
            UpdateGameScroller();
        }
        void UpdateGameScroller()
        {

            GameScrollBar.Maximum = (int)(1000L * GameInfos.GetNeededSize() / GameInfos.Size.Height);
            GameScrollBar.LargeChange = 1000;
            GameScrollBar.SmallChange = 60;
            if (GameInfos.GetNeededSize() < GameInfos.Size.Height)
            {
                GameScrollBar.Hide();
                return;
            }
            GameScrollBar.Show();
        }
        void UpdateBatchScroller()
        {
            BatchScrollBar.Maximum = (int)(1000L * BatchRecords.GetNeededSize() / BatchRecords.Size.Height);
            BatchScrollBar.LargeChange = 1000;
            BatchScrollBar.SmallChange = 60;
            if (BatchRecords.GetNeededSize() < BatchRecords.Size.Height)
            {
                BatchScrollBar.Hide();
                return;
            }
            BatchScrollBar.Show();
        }
        public void SwitchToBatches()
        {
            UpdateBatchScroller();
            TabControl.SelectTab(0);
        }

        public void UpdRecord(List<BatchRecord> records, bool WithSelect = false)
        {
            TabControl.SelectTab(0);
            BatchRecords.SetInfos(records, WithSelect ? Program.GetIntSetting(Program.GlobalSetting.CandidatesCount) : -1);
            UpdateBatchScroller();
        }
        void OnSaveBatch(object? a, BatchEventArgs e)
        {
            if(SaveBatch.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(SaveBatch.FileName,e.Record.ToJson());
        }
        void OnLoadBatch(object? a, EventArgs e)
        {

            if (OpenFile.ShowDialog() != DialogResult.OK) return;
            BatchRecords.AddRecord(BatchRecord.FromJson(File.ReadAllText(OpenFile.FileName)));
        }
        void OnShowBatch(object? a, BatchEventArgs e)
        {
            TabControl.SelectTab(1);
            GameInfos.SetInfos(e.Record);
            UpdateGameScroller();
        }
        void OnShowGame(object? a, GameEventArgs e)
        {
            ShowGameRecord.Invoke(this, e);
        }
        void OnRepeatGame(object? a, GameEventArgs e)
        {
            if(RepeatGameFromRecord != null)
                RepeatGameFromRecord.Invoke(this, e);
        }
        public List<ICardPlayer> GetSelectedPlayers()
        {
            return BatchRecords.GetSelectedPlayers().ToList();
        }

        private void Game_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateGameScroller();
            GameInfos.SetNewOffset(GameScrollBar.Value);
        }

        private void Batch_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateBatchScroller();
            BatchRecords.SetNewOffset(BatchScrollBar.Value);
        }
    }
    public class GameEventArgs : EventArgs
    {
        public BatchRecord Batch;
        public GameRecord Record;
        public GameEventArgs(GameRecord NewRecord, BatchRecord batch)
        {
            Record = NewRecord;
            Batch = batch;
        }
    }
    public class BatchEventArgs : EventArgs
    {
        public BatchRecord Record;
        public BatchEventArgs(BatchRecord NewRecord)
        {
            Record = NewRecord;
        }
    }
}
