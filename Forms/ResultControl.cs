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

namespace CardTableFool.Forms
{
    internal class ResultControl : TableControl
    {
        BatchRecord gameRecords;

        public event EventHandler<GameEventArgs> GameRecordRecall;
        public event EventHandler<GameEventArgs> GameRecordRepeat;
        public ResultControl() : base()
        {
            gameRecords = new BatchRecord();

            sizes = [0, 65, 350, 150, 150, 200, 150, 150];
            percents = [0, 0, 0f, 0, 0, 1f, 0, 0];
            BlockOffsets = new float[8];

            Paint += (object x, PaintEventArgs e) => { Render(); };

            MouseDown += OnClick;
            MouseMove += OnMove;
            
        }
        System.Drawing.Point MousePos;
        private void OnMove(object sender, MouseEventArgs e)
        {
            MousePos = e.Location;
            Render();
        }

        public void OnClick(object x, MouseEventArgs e)
        {
            int id = (int)((e.Location.Y + PixelOffset) / BlockHeight);

            if (id >= gameRecords.Records.Count)
                return;
            if (e.Location.Y <= BlockHeight)
                return;


            if (e.Location.X <= BlockOffsets[5])
                return;

            if (e.Location.X <= BlockOffsets[6])
                GameRecordRecall.Invoke(this, new GameEventArgs(gameRecords.Records[id],gameRecords));
            else
                GameRecordRepeat.Invoke(this, new GameEventArgs(gameRecords.Records[id], gameRecords));

        }
        public void DrawTooltip()
        {
            int id = (int)((MousePos.Y + PixelOffset) / BlockHeight);
            if (id >= gameRecords.Records.Count)
                return;
            if (MousePos.Y <= BlockHeight)
                return;

            if (MousePos.X <= BlockOffsets[5])
                return;

            string Text = "Переигрывает всю игру с такими же начальными условиями";
            Vector2 size =  new Vector2(330,60);
            
            if (MousePos.X <= BlockOffsets[6])
            {
                Text = "Проигрывает всю игру";
                size = new Vector2(230,40);
            }
            
            Vector2 MousePosAdj = new Vector2(Math.Min(MousePos.X + 10, Width - size.X), Math.Min(MousePos.Y + 10, Height-size.Y));
            d2dRenderTarget.FillRectangle(new RawRectangleF
                (MousePosAdj.X, MousePosAdj.Y+size.Y, MousePosAdj.X+size.X, MousePosAdj.Y),BlackColorBrush);
            d2dRenderTarget.FillRectangle(new RawRectangleF
                (MousePosAdj.X+2, MousePosAdj.Y + size.Y-3, MousePosAdj.X + size.X-3, MousePosAdj.Y+2), WhiteColorBrush);

            TextLayout la = new TextLayout(FactoryDWrite, Text,CenterTextFormat, size.X-5, size.Y-5);
            la.SetFontSize(la.GetFontSize(0) * 0.7f,new TextRange(0,1000));
            d2dRenderTarget.DrawTextLayout(MousePosAdj + new Vector2(2),la, BlackColorBrush);
            Utilities.Dispose(ref la);

        }
		protected override int GetRowsCount()
        {
            return gameRecords.Records.Count;
        }
        public void SetInfos(BatchRecord Info)
        {
            gameRecords = Info;
        }
        public void Render()
        {
            BeginDrawTable();

            CenterTextFormat.TextAlignment = TextAlignment.Center;

            int mx = Math.Min((int)((PixelOffset + BlockHeight + Size.Height) / BlockHeight), gameRecords.Records.Count);

            for (int i = Math.Max(0, (int)(PixelOffset / BlockHeight)); i < mx; i++)
                DrawBlock(i, PixelOffset);

            DrawHeader("№", "Имена", "Раунды", "Очки", "Код ошибки");

            EndDrawTable();
            DrawTooltip();
            EndDraw();
        }
        void DrawBlock(int id, float PixelOffset)
        {
            var Record = gameRecords.Records[id];
            float low = id * BlockHeight - PixelOffset;
            float high = BlockHeight + id * BlockHeight - PixelOffset;

            DrawBaseBlock(high);

            //Рисуем фон кнопки
            DrawBlockBackground(low, 5, new (0.8f, 0.8f, 0.8f, 1f), 3);
            
            DrawBlockBackground(low, 6, new (0.6f, 0.6f, 0.6f, 1f), 3);

            //Рисуем надписи
            string[] values = [
                id+"","",
                Record.rounds.Count - 1 + "",
                Record.rounds.Last().PlayerHand1.Count + Record.rounds.Last().PlayerHand2.Count + "",
                Record.ErrorCode + "",
                "Показать","Сыграть"];

            for (int i = 0; i < values.Length; i++)
                d2dRenderTarget.DrawText(values[i], CenterTextFormat,
                    new RawRectangleF(BlockOffsets[i], low, BlockOffsets[i + 1], high), BlackColorBrush);

            //Отрисовка цветного текста игроков
            var FirstColor = Record.result == EndGame.First ? WinColor : LoseColor;
            var SecondColor = Record.result == EndGame.Second ? WinColor : LoseColor;

            DrawColoredText(new RawVector2(BlockOffsets[1] + OffsetForText, low),
                [Record.Player1Name, " vs ", Record.Player2Name],
                [FirstColor, Color4.Black, SecondColor]);
        }
    }
}
