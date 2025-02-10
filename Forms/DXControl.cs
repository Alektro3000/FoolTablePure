using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using StbImageSharp;

namespace CardTableFool.Forms
{
    using Device = SharpDX.Direct3D11.Device;
    using Buffer = SharpDX.Direct3D11.Buffer;
    using D2D = SharpDX.Direct2D1;

    public class DXControl : Control
    {
        protected Device device;
        protected SwapChain swapChain;

        protected Texture2D target;
        protected RenderTargetView targetview;

        protected Buffer constantBuffer;
        protected Buffer constantCardBuffer;

        protected DeviceContext context;
        protected BlendState Blend;


        protected D2D.Factory d2dFactory = new D2D.Factory();
        protected Surface surface;
        protected D2D.RenderTarget d2dRenderTarget;
        protected D2D.SolidColorBrush WhiteColorBrush;
        protected D2D.SolidColorBrush BlackColorBrush;
        protected D2D.SolidColorBrush AnyColorBrush;

        public TextFormat TourTextFormat { get; private set; }
        public TextFormat CenterTextFormat { get; private set; }
        
        protected SharpDX.DirectWrite.Factory FactoryDWrite;

        protected float Fov = float.Pi / 2;
        protected float AspectRatio;

        protected float TextScale = 1;
        public DXControl()
        {
            Resize += OnResizeEvent;
            InitSwapChain();
        }
        public void OnResizeEvent(object a, EventArgs e)
        {
            OnResize();
        }
        public void InitSwapChain()
        {

            FactoryDWrite = new SharpDX.DirectWrite.Factory();


            SwapChainDescription swapChainDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                Flags = SwapChainFlags.None,
                IsWindowed = true,                               //it's windowed
                ModeDescription = new ModeDescription(
                    ClientSize.Width,                       //windows veiwable width
                    ClientSize.Height,                      //windows veiwable height
                    new Rational(60, 1),                         //refresh rate
                    Format.R8G8B8A8_UNorm),                      //pixel format, you should resreach this for your specific implementation

                OutputHandle = Handle,                      //the magic 

                SampleDescription = new SampleDescription(4, 0), //the first number is how many samples to take, anything above one is multisampling.
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport,
                swapChainDescription,
                out device, out swapChain
                );


            context = device.ImmediateContext;
            OnResize();

            BlendStateDescription blendStateDescription = new BlendStateDescription
            {
                AlphaToCoverageEnable = false,
            };

            blendStateDescription.RenderTarget[0].IsBlendEnabled = true;
            blendStateDescription.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            blendStateDescription.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            blendStateDescription.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendStateDescription.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
            blendStateDescription.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            blendStateDescription.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendStateDescription.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            context.OutputMerger.BlendState = Blend = new BlendState(device, blendStateDescription);
        }
        public static Texture2DDescription MakeTexDescription(int Width, int Height, int Count = 1)
        {
            Texture2DDescription desc;
            desc.Width = Width;
            desc.Height = Height;
            desc.ArraySize = Count;
            desc.BindFlags = BindFlags.ShaderResource;
            desc.Usage = ResourceUsage.Immutable;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            desc.Format = Format.R8G8B8A8_UNorm;
            desc.MipLevels = 1;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            return desc;
        }
        public virtual void OnResize()
        {
            AspectRatio = (float)ClientSize.Width / ClientSize.Height;

            //Удаляем старые "текстуры" для отрисовки
            Utilities.Dispose(ref target);
            Utilities.Dispose(ref targetview);
            Utilities.Dispose(ref surface);
            Utilities.Dispose(ref d2dRenderTarget);
            Utilities.Dispose(ref WhiteColorBrush);
            Utilities.Dispose(ref BlackColorBrush);
            Utilities.Dispose(ref AnyColorBrush);

            //Переписываем SwapChain
            swapChain.ResizeBuffers(2, ClientSize.Width, ClientSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

            //Создаём новые "текстуры" для отрисовки
            target = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(swapChain, 0);
            targetview = new RenderTargetView(device, target);

            //Устанавливаем их
            context.Rasterizer.SetViewport(new Viewport(0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f));
            //context.OutputMerger.SetTargets(targetview);

            //Создаём холст для 2D графики
            surface = target.QueryInterface<Surface>();
            d2dRenderTarget = new D2D.RenderTarget(d2dFactory, surface,
                new D2D.RenderTargetProperties(new
                D2D.PixelFormat(Format.Unknown, D2D.AlphaMode.Premultiplied)));

            //Создаём кисти
            WhiteColorBrush = new D2D.SolidColorBrush(d2dRenderTarget, SharpDX.Color.White);
            BlackColorBrush = new D2D.SolidColorBrush(d2dRenderTarget, SharpDX.Color.Black);
            AnyColorBrush = new D2D.SolidColorBrush(d2dRenderTarget, SharpDX.Color.White);

            //Создаём Формат текста
            int TextHeight = (int)(TextScale * ClientSize.Height * 0.04);
            if (TextHeight != 0)
            {
                if (TourTextFormat != null)
                {
                    TourTextFormat.Dispose();
                    CenterTextFormat.Dispose();
                }
                TourTextFormat = new TextFormat(FactoryDWrite, "Times New Roman", TextHeight) { TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Center };

                CenterTextFormat = new TextFormat(FactoryDWrite, "Times New Roman", TextHeight) { TextAlignment = TextAlignment.Center, ParagraphAlignment = ParagraphAlignment.Center };

            }
        }

        protected override void Dispose(bool Close)
        {
            Utilities.Dispose(ref d2dFactory);
            Utilities.Dispose(ref surface);
            Utilities.Dispose(ref d2dRenderTarget);

            Utilities.Dispose(ref WhiteColorBrush);
            Utilities.Dispose(ref BlackColorBrush);
            Utilities.Dispose(ref AnyColorBrush);

            if (TourTextFormat != null)
            {
                TourTextFormat.Dispose();
                CenterTextFormat.Dispose();
            }

            Utilities.Dispose(ref target);
            Utilities.Dispose(ref targetview);

            Utilities.Dispose(ref device);
            Utilities.Dispose(ref swapChain);


            Utilities.Dispose(ref constantBuffer);
            Utilities.Dispose(ref constantCardBuffer);

            Utilities.Dispose(ref Blend);
            Utilities.Dispose(ref context);
            Utilities.Dispose(ref FactoryDWrite);

            base.Dispose(Close);
        }
    }
}
