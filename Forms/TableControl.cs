using CardTableFool.Tables;
using Microsoft.VisualBasic.Logging;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardTableFool.Forms
{
    internal class TableControl : DXControl
    {
        protected float Offset = 0;
        protected float PixelOffset;
        protected int UpBlockOffset = 1;
        protected float BlockHeight = 40;

        protected float OffsetForText;
        protected float[] BlockOffsets;

        protected float[] sizes;
        protected float[] percents;

        protected float ShadowOffset = 1;
        protected float ShadowWidth = 2;
        protected Color4 ShadowColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
        protected float ShadowOffsetV = 1;

        public TableControl() : base()
        {
            TextScale = 0.8f;
            Padding = new Padding(0);

        }
		public override void OnResize()
        {
            base.OnResize();

            if (Height != 0)
            {
                BlockHeight = Height / 20;

                PixelOffset = Offset / 1000 * Size.Height - UpBlockOffset * BlockHeight;

                float Scale = Height / 1000f;
                float MinWidth = sizes.Sum() * Scale;
                float size = (Size.Width - MinWidth) / Scale;


                if (Size.Width < MinWidth)
                {
                    Scale *= Size.Width / MinWidth;
                    size = 0;
                }

                BlockOffsets[0] = (sizes[0] + percents[0] * size) * Scale;
                for (int i = 1; i < sizes.Length; i++)
                    BlockOffsets[i] = BlockOffsets[i - 1] + (sizes[i] + percents[i] * size) * Scale;

                OffsetForText = 10 * Scale;

            }

        }
		public void SetNewOffset(float offset)
        {
            Offset = offset;
            PixelOffset = Offset / 1000 * Size.Height - UpBlockOffset*BlockHeight;
            InvokePaint(this, null);
        }
		protected void DrawHeader(params string[] Headers)
        {
            d2dRenderTarget.FillRectangle(
                new RawRectangleF(0, 0, BlockOffsets.Last(), BlockHeight), WhiteColorBrush);

            AnyColorBrush.Color = ShadowColor;
            foreach (var i in BlockOffsets)
            {
                d2dRenderTarget.DrawLine(new RawVector2(i + ShadowOffset, 0), new RawVector2(i + ShadowOffset, BlockHeight), AnyColorBrush, ShadowWidth);
                d2dRenderTarget.DrawLine(new RawVector2(i, 0), new RawVector2(i, BlockHeight), BlackColorBrush);
            }
            for (int i = 0; i < Headers.Length; i++)
                d2dRenderTarget.DrawText(Headers[i], CenterTextFormat,
                    new RawRectangleF(BlockOffsets[i], 0, BlockOffsets[i + 1], BlockHeight), BlackColorBrush);

            d2dRenderTarget.DrawLine(new RawVector2(0, ShadowOffsetV), new RawVector2(BlockOffsets.Last(), ShadowOffsetV), AnyColorBrush, ShadowWidth);
            d2dRenderTarget.DrawLine(new RawVector2(0, BlockHeight + ShadowOffsetV), new RawVector2(BlockOffsets.Last(), BlockHeight + ShadowOffsetV), AnyColorBrush, ShadowWidth);
            
            d2dRenderTarget.DrawLine(new RawVector2(0, 0), new RawVector2(BlockOffsets.Last(), 0), BlackColorBrush);
            d2dRenderTarget.DrawLine(new RawVector2(0, BlockHeight), new RawVector2(BlockOffsets.Last(), BlockHeight), BlackColorBrush);

        }
        protected void DrawBaseBlock(float high)
        {
            AnyColorBrush.Color = ShadowColor;
            d2dRenderTarget.DrawLine(new RawVector2(0, high + ShadowOffsetV), new RawVector2(ClientSize.Width, high + ShadowOffsetV), AnyColorBrush, ShadowWidth);
            d2dRenderTarget.DrawLine(new RawVector2(0, high), new RawVector2(ClientSize.Width, high), BlackColorBrush);
        }
        protected void DrawBlockBackground(float low, int Id, Color4 Color, int offset = 0)
        {
            AnyColorBrush.Color = Color;
            d2dRenderTarget.FillRectangle(
                new RawRectangleF(BlockOffsets[Id] + offset + ShadowOffset, low + offset + ShadowOffsetV, 
                BlockOffsets[Id+1] - offset, low+BlockHeight - offset), AnyColorBrush);
        }
		protected virtual int GetRowsCount()
        {
            return 0;
        }
		public int GetNeededSize()
        {
            return (int)((UpBlockOffset + GetRowsCount()) * BlockHeight);
        }
		protected void BeginDrawTable()
        {
            context.ClearRenderTargetView(targetview, new Color4(1f, 1f, 1f, 1));

            d2dRenderTarget.BeginDraw();

            AnyColorBrush.Color = ShadowColor;
            foreach (var i in BlockOffsets)
                d2dRenderTarget.DrawLine(new RawVector2(i + ShadowOffset, 0), new RawVector2(i + ShadowOffset, ClientSize.Height), AnyColorBrush, ShadowWidth);

        }
		protected void EndDrawTable()
        {
            foreach (var i in BlockOffsets)
                d2dRenderTarget.DrawLine(new RawVector2(i, 0), new RawVector2(i, ClientSize.Height), BlackColorBrush);
        }
		protected void EndDraw()
        {
            d2dRenderTarget.EndDraw();

            swapChain.Present(0, SharpDX.DXGI.PresentFlags.None);
        }
        protected static readonly RawColor4 LoseColor = new RawColor4(0.7f, 0.1f, 0.1f, 1);
        protected static readonly RawColor4 WinColor = new RawColor4(0.1f, 0.7f, 0.1f, 1);
		protected void DrawColoredText(Vector2 BeginPos, string[] Texts, Color4[] Colors)
        {
            for (int i = 0; i < Texts.Length; i++)
            {
                var First = new TextLayout(FactoryDWrite, Texts[i], TourTextFormat, 10000, BlockHeight);
                AnyColorBrush.Color = Colors[i];
                d2dRenderTarget.DrawTextLayout(BeginPos, First, AnyColorBrush);
                BeginPos.X += First.Metrics.WidthIncludingTrailingWhitespace;
                Utilities.Dispose(ref First);
            }
        }
    }
}
