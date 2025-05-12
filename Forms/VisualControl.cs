using System.Runtime.InteropServices;
using CardTableFool.Main;
using CardTableFool.Tables;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using StbImageSharp;

namespace CardTableFool.Forms
{
	using Buffer = SharpDX.Direct3D11.Buffer;

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CardData
	{
		public Matrix Position;
		public Vector4 Color;
		public Vector4 Id;
	}
	public class VisualControl : DXControl
	{
		const int MaxCardCount = 60;

		Buffer IndexBuffer;
		VertexBufferBinding VertexBuffer;

		InputLayout CardLayout;
		Texture2D CardTextures;
		VertexShader CardVertexShader;
		PixelShader CardPixelShader;
		ShaderResourceView CardTextureResources;

		VertexShader TableVertexShader;
		PixelShader TablePixelShader;
		Texture2D TableTexture;
		ShaderResourceView TableTextureResources;

		DepthStencilView depthStencilView;

		List<VisualCard> Cards;

		DepthStencilStateDescription dsDesc;
		RenderTargetView ShadowView;

		VertexShader ShadowVertexShader;
		PixelShader ShadowPixelShader;

		public VisualControl() : base()
		{
			Cards = [];

			LoadShader();
			LoadVertex();
			LoadCardTextures();
			LoadTableTexture();

			Paint += (a, e) => Render(0.01f);

		}
		void LoadShader()
		{

			// Compile Vertex and Pixel shaders
			var vertexShaderByteCode = ShaderBytecode.CompileFromFile(LocalToGlobal("Shaders\\Shader.fx"), "VS", "vs_4_0");
			CardVertexShader = new VertexShader(device, vertexShaderByteCode);

			var pixelShaderByteCode = ShaderBytecode.CompileFromFile(LocalToGlobal("Shaders\\Shader.fx"), "PS", "ps_4_0");
			CardPixelShader = new PixelShader(device, pixelShaderByteCode);

			var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
			// Layout from VertexShader input signature
			CardLayout = new InputLayout(device, signature, new[]
					{
						new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
						new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0),
						new InputElement("ID", 0, Format.R32_Float, 16, 0)
					});

			vertexShaderByteCode.Dispose();
			pixelShaderByteCode.Dispose();

			// Compile Vertex and Pixel shaders
			var TableVertexShaderByteCode = ShaderBytecode.CompileFromFile(LocalToGlobal("Shaders\\Shader.fx"), "VSTable", "vs_4_0");
			TableVertexShader = new VertexShader(device, TableVertexShaderByteCode);

			var TablePixelShaderByteCode = ShaderBytecode.CompileFromFile(LocalToGlobal("Shaders\\Shader.fx"), "PSTable", "ps_4_0");
			TablePixelShader = new PixelShader(device, TablePixelShaderByteCode);

			TableVertexShaderByteCode.Dispose();
			TablePixelShaderByteCode.Dispose();


			// Compile Vertex and Pixel shaders
			var ShadowVertexShaderByteCode = ShaderBytecode.CompileFromFile(LocalToGlobal("Shaders\\Shader.fx"), "VSShadow", "vs_4_0");
			ShadowVertexShader = new VertexShader(device, ShadowVertexShaderByteCode);

			var ShadowPixelShaderByteCode = ShaderBytecode.CompileFromFile(LocalToGlobal("Shaders\\Shader.fx"), "PSShadow", "ps_4_0");
			ShadowPixelShader = new PixelShader(device, ShadowPixelShaderByteCode);

			ShadowVertexShaderByteCode.Dispose();
			ShadowPixelShaderByteCode.Dispose();

		}
		void LoadVertex()
		{

			const int n = MaxCardCount;

			//Вертексы            
			float[] TempVer = new float[20 + n * 5 * VisualCardBase.GetVertexCount()];
			for (int i = 0; i < n; i++)
				VisualCardBase.GetVertices(i).CopyTo(TempVer, i * 5 * VisualCardBase.GetVertexCount());

			float TableHeight = 9f * (float)Math.Tan(Fov / 4);
			float TableWidth = TableHeight * 1.77f;

			float[] TempTable = {
				-TableWidth, TableHeight,1,0,0,
				-TableWidth,-TableHeight,1,1,0,
				 TableWidth,-TableHeight,0,1,0,
				 TableWidth, TableHeight,0,0,0,};
			TempTable.CopyTo(TempVer, 5 * VisualCardBase.GetVertexCount() * n);

			var vertices = Buffer.Create(device, BindFlags.VertexBuffer, TempVer);
			VertexBuffer = new VertexBufferBinding(vertices, Utilities.SizeOf<float>() * 5, 0);

			//Индексы
			uint[] TempInd = new uint[6 + n * VisualCardBase.GetIndicesCount()];
			for (int i = 0; i < n; i++)
			{
				var offset = i * VisualCardBase.GetIndicesCount();
				VisualCardBase.GetIndices().CopyTo(TempInd, offset);
				for (int j = 0; j < VisualCardBase.GetIndicesCount(); j++)
					TempInd[j + offset] += (uint)(i * VisualCardBase.GetVertexCount());
			}
			uint[] TableTempId = { 0, 1, 2, 0, 2, 3 };
			for (int i = 0; i < 6; i++)
				TableTempId[i] += (uint)VisualCardBase.GetVertexCount() * n;
			TableTempId.CopyTo(TempInd, VisualCardBase.GetIndicesCount() * n);

			IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, TempInd);


			// Создаём константные буфферы
			constantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
			constantCardBuffer = new Buffer(device, Utilities.SizeOf<CardData>() * MaxCardCount, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

			//Создаём описание буфера глубины
			dsDesc = new DepthStencilStateDescription();
			dsDesc.DepthComparison = Comparison.LessEqual;
			dsDesc.DepthWriteMask = DepthWriteMask.All;
			dsDesc.IsDepthEnabled = true;

			//Подготавливаем все общие части для шейдеров
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			context.InputAssembler.SetVertexBuffers(0, VertexBuffer);
			context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
			context.VertexShader.SetConstantBuffer(0, constantBuffer);
			context.VertexShader.SetConstantBuffer(1, constantCardBuffer);
			context.Rasterizer.SetViewport(new Viewport(0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f));
			context.OutputMerger.SetDepthStencilState(new DepthStencilState(device, dsDesc));
			context.OutputMerger.SetTargets(depthStencilView, targetview);
			context.InputAssembler.InputLayout = CardLayout;

			vertices.Dispose();

			/*

			var desc = MakeTexDescription(1024,1024);
			desc.Format = Format.R32G32B32A32_Float;
			desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;

			using (var ShadowTex = new Texture2D(device, desc))
			{
				// Setup the description of the render target view.
				RenderTargetViewDescription renderTargetViewDesc = new RenderTargetViewDescription()
				{
					Format = desc.Format,
					Dimension = RenderTargetViewDimension.Texture2D,
				};
				renderTargetViewDesc.Texture2D.MipSlice = 0;

				ShadowView = new RenderTargetView(device, ShadowTex, renderTargetViewDesc);
			}
			*/
		}
		void LoadCardTextures()
		{
			string Globalpath = LocalToGlobal("CardsImg");

			//Все игровые карты
			const int CardsCount = 36;
			//Все скины игровых карт(все карты + рубашка)
			const int TotalCount = 37;

			// загружаем карты, сначала рубашку, чтобы узнать размеры изображений
			var OpenedBack = File.OpenRead(Path.Combine(Globalpath, "рубашка.png"));
			ImageResult image = ImageResult.FromStream(OpenedBack, ColorComponents.RedGreenBlueAlpha);
			OpenedBack.Close();

			//Выделяем буфер (Ширина * Высота * Количество канало * Общее количество карт)
			byte[] AllocatedMemory = new byte[image.Width * image.Height * 4 * TotalCount];

			//Копируем рубашку в самый конец массива
			Array.Copy(image.Data, 0, AllocatedMemory, image.Width * image.Height * 4 * CardsCount, image.Width * image.Height * 4);

			//Загружаем все карты массива
			for (int i = 0; i < CardsCount; i++)
			{
				//Загружаем карту
				var OpenedCard = File.OpenRead(Path.Combine(Globalpath, (SCard)i + ".png"));
				ImageResult cardImage = ImageResult.FromStream(OpenedCard, ColorComponents.RedGreenBlueAlpha);
				OpenedCard.Close();

				//Убеждаемся, что карты одного размера, ибо иначе их нельзя использовать в массиве
				if (cardImage.Width != image.Width || cardImage.Height != image.Height)
					throw new Exception("Not all Card Images have same resolution");

				//Копируем в массив
				Array.Copy(cardImage.Data, 0, AllocatedMemory, image.Width * image.Height * 4 * i, image.Width * image.Height * 4);
			}

			//И заносим их в текстуру видеокарты
			DataStream s = DataStream.Create(AllocatedMemory, true, true);
			DataRectangle[] rects = new DataRectangle[TotalCount];
			for (int i = 0; i < TotalCount; i++)
				rects[i] = new DataRectangle(s.DataPointer + i * image.Width * 4 * image.Height, image.Width * 4);
			CardTextures = new Texture2D(device, MakeTexDescription(image.Width, image.Height, TotalCount), rects);
			s.Close();
			CardTextureResources = new ShaderResourceView(device, CardTextures);
			context.PixelShader.SetShaderResource(0, CardTextureResources);
		}
		private void LoadTableTexture()
		{
			var OpenedTable = File.OpenRead(LocalToGlobal("CardsImg/UnshadedTable.png"));
			ImageResult TableImage = ImageResult.FromStream(OpenedTable, ColorComponents.RedGreenBlueAlpha);
			OpenedTable.Close();

			byte[] TableAllocatedMemory = new byte[TableImage.Width * TableImage.Height * 4];
			Array.Copy(TableImage.Data, TableAllocatedMemory, TableImage.Data.Length);

			DataStream s = DataStream.Create(TableAllocatedMemory, true, true);
			TableTexture = new Texture2D(device, MakeTexDescription(TableImage.Width, TableImage.Height), new DataRectangle(s.DataPointer, TableImage.Width * 4));
			s.Close();

			TableTextureResources = new ShaderResourceView(device, TableTexture);
			context.PixelShader.SetShaderResource(1, TableTextureResources);
		}
		public void SetUpCards(List<VisualCard> visualCards)
		{
			Cards = visualCards;
		}
		public void Render(float DeltaTime)
		{
			if (constantBuffer == null || Disposing || IsDisposed)
				return;

			var CardsToRender = Cards.OrderByDescending(x => x == null ? 10f : x.GetColor().W).ToList(); //
			UpdateUniformBuffer(CardsToRender);

			context.ClearRenderTargetView(targetview, new Color4(0, 0, 0, 1));
			context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1f, 0);

			//Переключаемся в режим отрисовки непрозрачных объектов
			dsDesc.DepthWriteMask = DepthWriteMask.All;
			context.OutputMerger.SetDepthStencilState(new DepthStencilState(device, dsDesc));


			//Рисуем стол
			context.VertexShader.Set(TableVertexShader);
			context.PixelShader.Set(TablePixelShader);
			context.DrawIndexed(6, VisualCardBase.GetIndicesCount() * MaxCardCount, 0);

			//Рисуем непрозрачные карты
			context.VertexShader.Set(CardVertexShader);
			context.PixelShader.Set(CardPixelShader);
			context.DrawIndexed(VisualCardBase.GetIndicesCount() * Cards.Where(x => x.GetColor().W == 1).Count(), 0, 0);

			//Рисуем прозрачные карты
			int TransparentCount = Cards.Where(x => x.GetColor().W != 1).Count();
			if (TransparentCount != 0)
			{
				//Переключаемся в режим отрисовки прощрачных объектов
				dsDesc.DepthWriteMask = DepthWriteMask.Zero;
				context.OutputMerger.SetDepthStencilState(new DepthStencilState(device, dsDesc));

				context.VertexShader.Set(CardVertexShader);
				context.PixelShader.Set(CardPixelShader);

				context.DrawIndexed(VisualCardBase.GetIndicesCount() * TransparentCount,
					VisualCardBase.GetIndicesCount() * Cards.FindIndex(x => x.GetColor().W != 1), 0);
			}

			DrawAbovePlay(DeltaTime);

			swapChain.Present(0, PresentFlags.None);
		}
		virtual protected void DrawAbovePlay(float DeltaTime)
		{

		}
		void UpdateUniformBuffer(List<VisualCard> CardsToRender)
		{
			var view = GetViewMatrix();
			context.UpdateSubresource(ref view, constantBuffer);

			CardData[] cardsinfo = new CardData[MaxCardCount];

			for (int i = 0; i < CardsToRender.Count; i++)
			{
				cardsinfo[i].Position = CardsToRender[i].GetMatrix();

				cardsinfo[i].Color = CardsToRender[i].GetColor();
				cardsinfo[i].Id.X = CardsToRender[i].GetId();
				cardsinfo[i].Id.Y = CardsToRender[i].GetIdBack();
			}
			context.UpdateSubresource(cardsinfo, constantCardBuffer);
        }
        float interpolation = 0;
        protected Matrix GetViewMatrix()
		{
			interpolation = Program.GetBoolSetting(Program.GlobalSetting.View3d) ? 1 : 0;
            Vector3 cameraPos;
            cameraPos = Vector3.Hermite(new Vector3(0.0f, 0, 10.0f), new Vector3(0.0f, 0, 1.0f), new Vector3(0.0f, -9.0f, 6.0f), new Vector3(0.0f, 0, 1.0f), interpolation);

			Matrix view = Matrix.LookAtLH(cameraPos, new Vector3(0.0f, 0, 1.0f), Vector3.UnitY);

			view *= Matrix.PerspectiveFovLH(
				Fov / 2, AspectRatio, 2f, 32.0f);
			return view;
		}
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (DesignMode)
				return;
			MouseEventArgs mouse = e;
			//interpolation = (float)mouse.X / ClientSize.Width;
            Vector2 point = -2 * new Vector2(AspectRatio * ((float)mouse.X / ClientSize.Width - 0.5f),
				(float)mouse.Y / ClientSize.Height - 0.5f);

			point *= 10 * (float)Math.Tan(Fov / 4);

			if (OnMouseMoveWorldSpace != null)
				OnMouseMoveWorldSpace.Invoke(this, new MouseMoveWorldSpace(point));
		}

		protected override void Dispose(bool Close)
		{
			Utilities.Dispose(ref depthStencilView);

			if (context != null)
			{
				context.ClearState();
				context.Flush();
			}
			//Utilities.Dispose(ref VertexBuffer);
			Utilities.Dispose(ref IndexBuffer);

			Utilities.Dispose(ref CardTextures);
			Utilities.Dispose(ref CardLayout);
			Utilities.Dispose(ref CardVertexShader);
			Utilities.Dispose(ref CardTextureResources);
			Utilities.Dispose(ref CardPixelShader);

			Utilities.Dispose(ref TableVertexShader);
			Utilities.Dispose(ref TablePixelShader);
			Utilities.Dispose(ref TableTextureResources);
			Utilities.Dispose(ref TableTexture);

			base.Dispose(Close);
		}

		public EventHandler OnMouseMoveWorldSpace;
		static public string LocalToGlobal(string directory)
		{
			//Ищем директорию с файлом
			string Globalpath = Directory.GetCurrentDirectory();
			while (Globalpath != null && !(
				Directory.Exists(Path.Combine(Globalpath, directory))
				|| File.Exists(Path.Combine(Globalpath, directory))))
			{
				Globalpath = Path.TrimEndingDirectorySeparator(Path.GetDirectoryName(Globalpath));
			}
			//Если не нашли поднимаем ошибку
			if (Globalpath == null)
				throw new Exception($"Failed to find directory({directory}), from ({Globalpath})");
			return Path.Combine(Globalpath, directory);
		}

		public override void OnResize()
		{

			if (ClientSize.Width == 0)
				return;

			Utilities.Dispose(ref depthStencilView);

			base.OnResize();

			var desc = new Texture2DDescription
			{
				Format = Format.D16_UNorm,
				ArraySize = 1,
				MipLevels = 1,
				Width = ClientSize.Width,
				Height = ClientSize.Height,
				SampleDescription = new SampleDescription(4, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.DepthStencil,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			};
			using (var zBufferTexture = new Texture2D(device, desc))
				depthStencilView = new DepthStencilView(device, zBufferTexture);

			context.OutputMerger.SetTargets(depthStencilView, targetview);


		}
	}
	public class MouseMoveWorldSpace : EventArgs
	{
		public Vector2 Location;
		public MouseMoveWorldSpace(Vector2 NewLocation)
		{
			Location = NewLocation;
		}
	}
}
