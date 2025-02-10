using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using CardTableFool.Tables;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.Win32;
using StbImageSharp;

namespace CardTableFool.Forms
{
    public class VisualTourControl : VisualControl
    {
        string BlockName;

        public bool ShowTable = false;

        float DeltaMovement = 1;
        int PosBegin;
        int PosEnd;
        string MovingName;

        string[] TourTable;

        public struct BarInfo
        {
            public int[] Scores;
			public int[] Errors;
			public string Name;
            public BarInfo(int n, string name)
            {
				Scores = new int[n];
				Errors = new int[n];
                Name = name;
			}
        }
            

        bool ShowBar;
        float Bar = 0.5f;
		float SubBar = 0.5f;
		float Time;
        int Current;
        BarInfo Player1;
        BarInfo Player2;

        int SubTableDepth;

        public VisualTourControl() : base()
        {
            TextScale = 0.95f;

            SetUpTourTable([""]);
        }
        public void SetUpTourTable(string[] Input)
        {
            TourTable = Input;

            for (int i = TourTable.Length / 2 - 1; i >= 0; i--)
            {
                if (TourTable[i] == null && (TourTable[i * 2 + 1] != null || TourTable[i * 2] != null))
                    TourTable[i] = "";
            }
            ShowBar = false;
        }

        public void SetUpBlockName(string Name)
        {
            BlockName = Name;
        }
        public void SetUpTourTableAnim(int WinnerPosition, int CurrentTour, string Name)
        {
            DeltaMovement = -1f;
            PosBegin = WinnerPosition >> SubTableDepth;
            PosEnd = CurrentTour >> SubTableDepth;
            MovingName = Name;
        }

        public void SetUpBarAnim(BarInfo First, BarInfo Second)
        {
            Player1 = First;
            Player2 = Second;

            ShowBar = true;
            SubBar = 0.5f;
            Time = 0;
            Current = 0;
        }
        protected override void DrawAbovePlay(float DeltaTime)
        {
            if (ShowTable)
            {
                d2dRenderTarget.BeginDraw();
                if (ShowTable)
                    RenderTable(DeltaTime);

                if (ShowBar)
                    RenderBar(DeltaTime);

                if(BlockName!=null)
                {
                    TextLayout Layout = new TextLayout(FactoryDWrite, BlockName, CenterTextFormat, ClientSize.Width, ClientSize.Height * 0.04f);
                    float wid = Layout.Metrics.Width/2 + ClientSize.Height/50f;
                    float Center = ClientSize.Width / 2f;
                    d2dRenderTarget.FillRectangle(new RawRectangleF(Center-wid,0,Center+wid, ClientSize.Height * 0.04f), WhiteColorBrush);
                    d2dRenderTarget.DrawTextLayout(Vector2.Zero, Layout, BlackColorBrush);
                    Layout.Dispose();
                }

                d2dRenderTarget.EndDraw();
            }
        }
        void RenderTable(float DeltaTime)
        {
            int Depth = System.Numerics.BitOperations.Log2((uint)TourTable.Length) - 1;
            for (int k = Depth; k > 0; k--)
                for (int i = 0; i < 1 << k; i++)
                    if (TourTable[(1 << k) + i] != null)
                        DrawTourArrow(GetTourPos(k, i, Depth), GetTourPos(k - 1, i / 2, Depth));

            for (int k = Depth; k >= 0; k--)
                for (int i = 0; i < 1 << k; i++)
                    if (TourTable[(1 << k) + i] != null)
                        DrawTourLabel(GetTourPos(k, i, Depth), TourTable[(1 << k) + i]);

            if (DeltaMovement >= 1)
                return;

            DeltaMovement += DeltaTime * 0.6f;
            if (DeltaMovement < 0)
                return;

            if (DeltaMovement > 1)
            {
                DeltaMovement = 1;
                TourTable[PosEnd] = MovingName;
                int k2 = System.Numerics.BitOperations.Log2((uint)PosEnd);
                DrawTourLabel(GetTourPos(k2, PosEnd % (1 << k2), Depth), MovingName);
                return;
            }
            float SmoothedTime = 2 * DeltaMovement * DeltaMovement;
            if (DeltaMovement > 0.5)
                SmoothedTime = -2 * (DeltaMovement - 1) * (DeltaMovement - 1) + 1;

            int kBegin = System.Numerics.BitOperations.Log2((uint)PosBegin);
            int kEnd = System.Numerics.BitOperations.Log2((uint)PosEnd);

            var Begin = GetTourPos(kBegin, PosBegin % (1 << kBegin), Depth);
            var End = GetTourPos(kEnd, PosEnd % (1 << kEnd), Depth);

            var Intermideate = new Vector2(End.X, Begin.Y);

            float distTo = (Begin - Intermideate).Length();
            float distFrom = (End - Intermideate).Length();
            float Dist = SmoothedTime * (distTo + distFrom);

            if (Dist < distTo)
            {
                float T = Dist / distTo;
                DrawTourLabel(Begin * (1 - T) + Intermideate * T, MovingName);
            }
            else
            {
                float T = (Dist - distTo) / distFrom;
                DrawTourLabel(Intermideate * (1 - T) + End * T, MovingName);
            }


        }

        void RenderBar(float DeltaTime)
        {
            Time += DeltaTime*1.3f;
            float BarTarget = (float)Player2.Scores[Current] / (Player2.Scores[Current] + Player1.Scores[Current]);

            float TotalErrors = Player2.Errors[Current] + Player1.Errors[Current];
			float SubBarTarget = (float)Player2.Errors[Current] / TotalErrors;
			
            float TempBar = Bar + (Math.Sign(BarTarget - Bar) * 0.01f + BarTarget - Bar) * DeltaTime * 3;
            Bar = Math.Clamp(TempBar, Math.Min(Bar, BarTarget), Math.Max(Bar, BarTarget));

			if (Time > 1f)
            {
                Time = 0;
                if (Current != Player1.Scores.Length - 1)
                    Current++;
            }

            var h = ClientSize.Height;
            var w = ClientSize.Width;
            var Down = h * 0.95f;
            var Up = h * 0.92f;
            var b = w * (Bar * (0.85f - 0.15f) + 0.15f);

            d2dRenderTarget.DrawRectangle(new RawRectangleF(w * 0.15f, Up, w * 0.85f, Down), BlackColorBrush, 5f);
            d2dRenderTarget.DrawLine(new Vector2(w * 0.5f, Down), new Vector2(w * 0.5f, Up), WhiteColorBrush, 5f);

            d2dRenderTarget.FillRectangle(new RawRectangleF(w * 0.15f + 2f, Up + 2f, w * 0.85f - 2f, Down - 2f), WhiteColorBrush);

            AnyColorBrush.Color = new Color4(0.9f, 0.5f, 0.1f, 1);
            d2dRenderTarget.FillRectangle(new RawRectangleF(w * 0.15f + 2f, Up + 2f, Math.Max(b - 5f, w * 0.15f + 2f), Down - 2f), AnyColorBrush);

            AnyColorBrush.Color = new Color4(0.1f, 0.6f, 0.9f, 1);
            d2dRenderTarget.FillRectangle(new RawRectangleF(Math.Min(b + 5f, w * 0.85f - 2f), Up + 2f, w * 0.85f - 2f, Down - 2f), AnyColorBrush);

            if (TotalErrors > 0)
			{
				float TempSubBar = SubBar + (Math.Sign(SubBarTarget - SubBar) * 0.01f + SubBarTarget - SubBar) * DeltaTime * 3;
				SubBar = Math.Clamp(TempSubBar, Math.Min(SubBar, SubBarTarget), Math.Max(SubBar, SubBarTarget));

				float SubDown = h * 0.93f;
				b = w * (SubBar * (0.85f - 0.15f) + 0.15f);

                AnyColorBrush.Color = new Color4(0.9f, 0.1f, 0.1f, 1);
				d2dRenderTarget.FillRectangle(new RawRectangleF(w * 0.15f + 2f, Up + 2f, w * 0.85f - 2f, SubDown), AnyColorBrush);
				
				AnyColorBrush.Color =  new Color4(0.9f, 0.5f, 0.1f, 1);
                d2dRenderTarget.FillRectangle(new RawRectangleF(w * 0.15f + 2f, Up + 2f, Math.Max(b - 10f, w * 0.15f + 2f), SubDown), AnyColorBrush);

                AnyColorBrush.Color = new Color4(0.1f, 0.6f, 0.9f, 1);
                d2dRenderTarget.FillRectangle(new RawRectangleF(Math.Min(b + 10f, w * 0.85f - 2f), Up + 2f, w * 0.85f - 2f, SubDown), AnyColorBrush);

				d2dRenderTarget.DrawLine(new Vector2(w * 0.15f, SubDown), new Vector2(w * 0.85f, SubDown), BlackColorBrush, 3f);
			}

			TextLayout First = new TextLayout(FactoryDWrite, Player1.Name, TourTextFormat, 1000, h * 0.1f);

            var offset1 = First.Metrics.Width;
            d2dRenderTarget.DrawTextLayout(new RawVector2(w * 0.16f, h * 0.92f), First, WhiteColorBrush);

            First.Dispose();

            TextLayout Second = new TextLayout(FactoryDWrite, Player2.Name, TourTextFormat, 1000, h * 0.1f);

            var offset2 = Second.Metrics.Width;
            d2dRenderTarget.DrawTextLayout(new RawVector2(w * 0.84f - offset2, h * 0.92f), Second, WhiteColorBrush);

            Second.Dispose();


        }
        Vector2 GetTourPos(int Level, int Player, int TotalLevels)
        {
            if (Level == 0)
                return new Vector2(0.5f, 0.475f);
            int cnt = 1 << Level - 1;
            float centerX = Level * 0.1f + 0.03f;

            if (TotalLevels >= 5)
                centerX = Level >= 5 ? -0.37f + 0.16f * Level : Level * 0.07f;

            float off = 0.5f + (Player >= cnt ? -centerX : centerX);
            float topoff = 0.95f - 0.95f * (Player % cnt + 0.5f) / cnt;

            if (Level == 5)
                topoff += 0.03125f * 0.95f;

            if (Level == 1)
                topoff += 1.5f * (Player < cnt ? 0.0625f : -0.0625f);

            return new Vector2(off, topoff);
        }
        void DrawTourLabel(Vector2 Pos, string Text)
        {
            float off = ClientSize.Width * Pos.X;
            float topoff = ClientSize.Height * Pos.Y;
            float halfHeight = ClientSize.Height * 0.023f;
            float halfWidth = ClientSize.Width * 0.067f;
            float OffsetText = ClientSize.Width * 0.003f;

            d2dRenderTarget.FillRoundedRectangle(new SharpDX.Direct2D1.RoundedRectangle()
            {
                RadiusX = 5,
                RadiusY = 5,
                Rect =
                new RawRectangleF(off - halfWidth - 1, topoff + halfHeight + 1, off + halfWidth + 1,
                topoff - halfHeight - 1)
            }, BlackColorBrush);

            d2dRenderTarget.FillRoundedRectangle(new SharpDX.Direct2D1.RoundedRectangle()
            {
                RadiusX = 5,
                RadiusY = 5,
                Rect =
                new RawRectangleF(off - halfWidth, topoff + halfHeight,
                off + halfWidth, topoff - halfHeight)
            }, WhiteColorBrush);

            int MaxSymbols = (int)(AspectRatio * 7) - 1;

            d2dRenderTarget.DrawText(Text.Length > MaxSymbols ? Text.Substring(0, MaxSymbols) + ".." : Text, TourTextFormat,
                new RawRectangleF(off - halfWidth + OffsetText, topoff + halfHeight,
                off + halfWidth * 2, topoff - halfHeight), BlackColorBrush);

        }
        void DrawTourArrow(Vector2 Begin, Vector2 End)
        {
            Vector2 Size = new Vector2(ClientSize.Width, ClientSize.Height);
            Begin *= Size;
            End *= Size;
            float offs = Math.Sign(Begin.X - End.X) * 2.5f;


            d2dRenderTarget.DrawLine(Begin + new Vector2(offs, 0), new RawVector2(End.X - offs, Begin.Y), BlackColorBrush, 5);

            float Direction = Math.Sign(Begin.Y - End.Y);
            if ((Begin.Y - End.Y) * Direction < 0.001f)
                Direction = 0;

            End.Y += Direction * Size.Y * 0.023f * 1f;

            d2dRenderTarget.DrawLine(new RawVector2(End.X, Begin.Y), End, BlackColorBrush, 5);

            End.Y += Direction * Size.Y * 0.023f * 0.1f;

            float ArrowOffset = Direction * 10;

            d2dRenderTarget.DrawLine(End + new RawVector2(-4, ArrowOffset), End, BlackColorBrush, 5);
            d2dRenderTarget.DrawLine(End + new RawVector2(4, ArrowOffset), End, BlackColorBrush, 5);

        }
    }
}
