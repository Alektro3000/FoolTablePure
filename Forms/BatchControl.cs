using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardTableFool.Tables;
using CardTableFool.Players;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using Microsoft.VisualBasic.Logging;
using System.DirectoryServices.ActiveDirectory;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;

namespace CardTableFool.Forms
{
    internal class BatchControl : TableControl
    {
        List<BatchRecord> BatchRecords;
        List<bool> SelectionFlags;

        public event EventHandler<BatchEventArgs> RecallBatch;
        public event EventHandler<BatchEventArgs> SaveBatch_Click;
        public event EventHandler<EventArgs> LoadBatch_Click;
        bool ShowLoad;
        public BatchControl(bool showLoad) : base()
        {
            ShowLoad = showLoad;
            sizes = [0, 65, 350, 180, 230, 200, 150, 165];
            percents = [0, 0, 1, 0, 0, 0, 0, 0];
            BlockOffsets = new float[8];
            BatchRecords = [];

            Paint += (object x, PaintEventArgs e) => { Render(); };

            MouseDown += OnClick;
        }
        public void OnClick(object x, MouseEventArgs e)
        {
            int id = (int)((e.Location.Y + PixelOffset) / BlockHeight);
            if (e.Location.X >= BlockOffsets[6])
            {
                if (e.Location.Y <= BlockHeight)
                {
                    if (ShowLoad && LoadBatch_Click != null)
                        LoadBatch_Click.Invoke(this, new EventArgs());
                }
                else
                    if (SaveBatch_Click != null)
                        SaveBatch_Click.Invoke(this, new BatchEventArgs(BatchRecords[id]));
                return;
            }    
            if (id >= BatchRecords.Count)
                return;
            if (e.Location.Y <= BlockHeight)
                return;


            if (e.Location.X <= BlockOffsets[5])
            {
                if (SelectionFlags != null)
                {
                    SelectionFlags[id] = !SelectionFlags[id];
                    Render();
                }
                return;
            }

            RecallBatch.Invoke(this, new BatchEventArgs(BatchRecords[id]));

        }
        protected override int GetRowsCount()
        {
            return BatchRecords.Count;
        }
        public void SetInfos(List<BatchRecord> Infos, int Candidates)
        {
            BatchRecords = Infos;
            if (Candidates < 0)
            {
                SelectionFlags = null;
                return;
            }
            SelectionFlags = new List<bool>(new bool[Infos.Count]);
            for (int i = 0; i < Math.Min(Candidates, Infos.Count); i++)
                SelectionFlags[i] = true;
            Render();
        }
        public void AddRecord(BatchRecord info)
        {
            BatchRecords.Add(info);
            if (SelectionFlags != null)
                SelectionFlags.Add(false);
            
            Render();
        }
        public IEnumerable<ICardPlayer> GetSelectedPlayers()
        {
            if (SelectionFlags == null)
                return [];

            return Enumerable.Zip(BatchRecords, SelectionFlags).Where(x => x.Second).Select(x => x.First.Player1);
        }
        public void Render()
        {
            BeginDrawTable();

            //Last drown block
            int mx = Math.Min((int)((PixelOffset + BlockHeight + Size.Height) / BlockHeight), BatchRecords.Count);

            for (int i = Math.Max(0, (int)(PixelOffset / BlockHeight)); i < mx; i++)
                DrawBlock(i, PixelOffset);

            DrawHeader(["№", "Имена", "Победы", "Очки", "Ошибки","", ShowLoad? "Загрузить":""]);

            EndDrawTable();
            EndDraw();

        }



        void DrawBlock(int id, float PixelOffset)
        {

            var Record = BatchRecords[id];
            float low = id * BlockHeight - PixelOffset;
            float high = BlockHeight + id * BlockHeight - PixelOffset;

            DrawBaseBlock(high);

            //Если есть выбор участников то подчёркиваем выбор
            if (SelectionFlags != null)
            {
                AnyColorBrush.Color = SelectionFlags[id] ? new Color4(0.1f, 1f, 0.1f, 1f) : new Color4(1f, .1f, 0.1f, 1f);
                d2dRenderTarget.DrawLine(new RawVector2(0, high - 1), 
                    new RawVector2(BlockOffsets[1], high - 1), AnyColorBrush, 3);
            }

            //Рисуем фон ошибок (если они есть)
            if (Record.ErrorCount > 0)
                DrawBlockBackground(low, 4, new (0.9f, 0.2f, 0.2f, 1), 0);

            //Рисуем фон кнопки
            DrawBlockBackground(low, 5, new (0.8f, 0.8f, 0.8f, 1f), 3);

            DrawBlockBackground(low, 6, new (0.6f, 0.6f, 0.6f, 1f), 3);

            //Рисуем надписи
            string[] values = [id.ToString(),"",
                Record.TotalWins1 + " vs " + Record.TotalWins2,
                Record.TotalScore1 + " vs " + Record.TotalScore2,
                Record.ErrorCount.ToString(),"Больше","Сохранить"];

            
            for (int i = 0; i < values.Length; i++)
                d2dRenderTarget.DrawText(values[i], CenterTextFormat,
                    new RawRectangleF(BlockOffsets[i], low, BlockOffsets[i + 1], high), BlackColorBrush);

            //Рисуем цветные надписи
            var FirstColor = Record.FirstWin > 0 ? WinColor : LoseColor;
            var SecondColor = Record.FirstWin < 0 ? WinColor : LoseColor;

            DrawColoredText(new RawVector2(BlockOffsets[1] + OffsetForText, low),
                [Record.Records[0].Player1Name, " vs ", Record.Records[0].Player2Name],
                [FirstColor, Color4.Black, SecondColor]);

        }
    }
}