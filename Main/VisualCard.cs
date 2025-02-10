using CardTableFool.Tables;
using SharpDX;


namespace CardTableFool.Main
{
	using Matrix4 = SharpDX.Matrix;
	//Класс предоставляющий функции для заполнения вертексных буферов видеокарты картами
	public struct VisualCardBase
	{
		public static float[] GetVertices(float id)
		{
			var ans = MakeVertexes();
			for (int i = 0; i < GetVertexCount() / 4; i++)
			{
				ans[i * 5 + 4] = id + 1;
				ans[(GetVertexCount() / 2 + i) * 5 + 4] = id + 1;
				ans[(GetVertexCount() / 4 + i) * 5 + 4] = -id - 1;
				ans[(GetVertexCount() / 2 + GetVertexCount() / 4 + i) * 5 + 4] = -id - 1;
			}
			return ans;
		}
		public static uint[] GetIndices()
		{
			return MakeIndices();
		}
		public static uint GetVertexClearCount()
		{
			return 12;
		}
		public static uint GetVertexCount()
		{
			return (GetVertexClearCount()+1) * 2 * 2;
		}
		public static int GetIndicesCount()
		{
			return (int)GetVertexClearCount() * 3 * 2 * 2;
		}
		public static Vector2 GetBoundingBox()
		{
			return new Vector2(0.5f, 0.72f);
		}
		public static float GetWidth()
		{
			return 0.02f;
		}
		public static uint[] MakeIndices()
		{
			uint[] ints = new uint[GetIndicesCount()];
			uint VertexPerSide = GetVertexClearCount();
			//Front
			for (uint i = 0; i < VertexPerSide; i++)
			{
				ints[i * 3] = 0;
				ints[i * 3 + 1] = i+1;
				ints[i * 3 + 2] = (i+1)%VertexPerSide+ 1;
			}
			//Back
			uint offs = VertexPerSide * 3;
			for (uint i = 0; i < VertexPerSide; i++)
			{
				ints[offs + i * 3] = VertexPerSide+1;
				ints[offs + i * 3 + 2] = i + 2 + VertexPerSide;
				ints[offs + i * 3 + 1] = (i + 1) % VertexPerSide + VertexPerSide + 2;
			}
			offs *= 2;
			uint VertOf = (VertexPerSide+1)*2;
			//Side
			for (uint i = 0; i < VertexPerSide; i++)
			{
				uint ind = i + 1;
				ints[offs + i * 6] = VertOf + ind + VertexPerSide + 1;
				ints[offs + i * 6 + 1] = VertOf + (ind +1)% VertexPerSide + 1;
				ints[offs + i * 6 + 2] = VertOf + ind;

				ints[offs + i * 6 + 3] = VertOf + ind + VertexPerSide + 1;
				ints[offs + i * 6 + 4] = VertOf + (ind + 1) % VertexPerSide + 2;
				ints[offs + i * 6 + 5] = VertOf + (ind + 1) % VertexPerSide + 1;
			}
			return ints;
		}
		public static float[] MakeVertexes()
		{
			float[] Ans = new float[GetVertexCount()*5];
			int offs = Pos.Length * 5;
			int PosOffs = Pos.Length;
			Ans[2] = 0.5f;
			Ans[3] = 0.5f;
			for (int i = 0; i < Pos.Length; i++)
			{
				MakeVertex(Pos[i]).CopyTo(Ans, 5+i*5);
				MakeVertex(Pos[PosOffs - i-1] * new Vector2(-1,1)).CopyTo(Ans, 5 + offs + i * 5);
				MakeVertex(-Pos[i]).CopyTo(Ans, 5 + offs *2+ i * 5);
				MakeVertex(Pos[PosOffs - i - 1] * new Vector2(1, -1)).CopyTo(Ans, 5 + offs *3+ i * 5);
			}
			Array.Copy(Ans,0,Ans, Ans.Length/4, Ans.Length / 4);
			Array.Copy(Ans, 0, Ans, Ans.Length / 2, Ans.Length / 2);
			for (int i = 0; i < GetVertexCount()/2; i++)
			{
				Ans[(GetVertexCount() / 2 + i) * 5 + 2] = 0;
				Ans[(GetVertexCount() / 2 + i) * 5 + 3] = 0;
			}

			return Ans;
		}
		public static float[] MakeVertex(Vector2 VertexPos)
		{
			return [VertexPos.X, VertexPos.Y, 
					VertexPos.X/GetBoundingBox().X/2+0.5f,VertexPos.Y/GetBoundingBox().Y/2+0.5f
					,0f];
		}
		static Vector2[] Pos =
		{
			new(0.5f,0.68f),
			new(0.486f,0.706f),
			new(0.46f,0.72f),
		};

	}

	//Структура для удобной записи положения и вращений объектов
	public struct Transform
	{
		public Vector3 Position;
		public Quaternion Rotation = new Quaternion(0, 0, 1, 0);
		public float Scale = 1;
		public Transform()
		{
			Position = new Vector3();
			Rotation = Quaternion.Identity;
			Scale = 1;
		}
		public Transform(Vector3 position)
		{
			Position = position;
		}
		public Transform(Quaternion rotation)
		{
			Rotation = rotation;
		}
		public Transform(Vector3 position, Quaternion rotation, float scale = 1)
		{
			Position = position;
			Rotation = rotation;
			Scale = scale;
		}
		public static Transform Lerp(Transform a, Transform b, float blend)
		{
			Transform ans = new Transform();
			ans.Position = a.Position * (1 - blend) + b.Position * blend;
			ans.Rotation = Slerp(a.Rotation, b.Rotation, blend);
			ans.Scale = a.Scale * (1 - blend) + b.Scale * blend;
			return ans;
		}
		//Упрощенный Slerp из OpenTK
		public static Quaternion Slerp(Quaternion q1, Quaternion q2, float blend)
		{
			//Векторы из q1 и q2
			Vector3 vector1 = new(q1.X, q1.Y, q1.Z);
			Vector3 vector2 = new(q2.X, q2.Y, q2.Z);


			var cosHalfAngle = q1.W * q2.W + Vector3.Dot(vector1, vector2);

			if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
			{
				//Угол равен 0, значит возвращаем любой
				return q1;
			}

			//Если косинус меньше нуля меняем кватернион на противоположный, чтобы выбрать более короткий путь
			if (cosHalfAngle < 0.0f)
			{
				vector2 = -vector2;
				q2.W = -q2.W;
				cosHalfAngle = -cosHalfAngle;
			}

			float blendA;
			float blendB;

			if (cosHalfAngle < 0.99f)
			{
				//Делаем корректный Slerp для больших углов
				var halfAngle = MathF.Acos(cosHalfAngle);
				var sinHalfAngle = MathF.Sin(halfAngle);
				var oneOverSinHalfAngle = 1.0f / sinHalfAngle;
				blendA = MathF.Sin(halfAngle * (1.0f - blend)) * oneOverSinHalfAngle;
				blendB = MathF.Sin(halfAngle * blend) * oneOverSinHalfAngle;
			}
			else
			{
				//Для малых углов хватит обычного lerp
				blendA = 1.0f - blend;
				blendB = blend;
			}
			var result = new Quaternion(blendA * vector1 + blendB * vector2, blendA * q1.W + blendB * q2.W);
			result.Normalize();
			return result;
		}

		public static implicit operator Matrix4(Transform tran)
		{
			return Matrix4.Scaling(tran.Scale) * Matrix4.RotationQuaternion(tran.Rotation) * Matrix4.Translation(tran.Position);
		}

	}
	//Класс отвечающий за анимации отдельной карты
	public class VisualCard
	{
		public enum SpecialType
		{
			normal, // обычная карта
			trump, // карта козыря, которая остаётся, когда закончилась колода
			player // карта игрока показывающая как будет атаковать игрок
		}
		//Текущее положение карты или положение куда летит карт
		public enum CurrentLocation
		{
			Deck,
			Dump,
			HandFirst,
			HandSecond,
			TableUp,
			TableDown,
			Trump,
		}
		public SpecialType CardType;

		public SCard card;


		private CurrentLocation _Loc;
		public CurrentLocation Location
		{
			get => _Loc;
		}

		public VisualCard(SCard nw, Transform newTransform)
		{
			card = nw;
			CurrentTransform = newTransform;
		}
		public VisualCard(VisualCard nw)
		{
			CardType = nw.CardType;
			card = nw.card;
			CurrentTransform = nw.CurrentTransform;
			SelectionRate = nw.SelectionRate;
			SelectionRateSmoothed = nw.SelectionRateSmoothed;
			IsCurrentlyHover = nw.IsCurrentlyHover;
			IsCurrentlySelected = nw.IsCurrentlySelected;
			BeginTranform = nw.BeginTranform;
			EndTransform = nw.EndTransform;
			AnimationNormalizedTime = nw.AnimationNormalizedTime;
			AnimationTotalTime = nw.AnimationTotalTime;
			Moving = nw.Moving;
			_Loc = nw._Loc;
		}
		public Transform CurrentTransform;
		float SelectionRate;
		float SelectionRateSmoothed;


		float ErrorRate;

		public bool IsCurrentlyHover;
		public bool IsCurrentlySelected;

		Transform BeginTranform;
		Transform InterTransform;
		Transform EndTransform;
		float AnimationNormalizedTime;
		float AnimationTotalTime;
		public bool Moving;
		public void UpdateTick(float DeltaTick)
		{
			UpdateSelection(DeltaTick);

			if (!Moving)
				return;

			AnimationNormalizedTime += DeltaTick / AnimationTotalTime;
			if (AnimationNormalizedTime >= 1)
			{
				AnimationNormalizedTime = 1;
				CurrentTransform = EndTransform;
				Moving = false;
				return;
			}
			if (AnimationNormalizedTime < 0)
				return;

			float SmoothedTime = 2 * AnimationNormalizedTime * AnimationNormalizedTime;
			if (AnimationNormalizedTime > 0.5)
				SmoothedTime = -2 * (AnimationNormalizedTime - 1) * (AnimationNormalizedTime - 1) + 1;

			CurrentTransform = Transform.Lerp(Transform.Lerp(BeginTranform, InterTransform, SmoothedTime),
					Transform.Lerp(InterTransform, EndTransform, SmoothedTime), SmoothedTime);
		}

		public void UpdateSelection(float DeltaTick)
		{
			if (IsCurrentlySelected)
				SelectionRate = 2f;
			else if (IsCurrentlyHover)
				SelectionRate = 1f;
			else
				SelectionRate = Math.Max(0, SelectionRate - DeltaTick * 3f);


			float Delta = DeltaTick * 5;
			float DeltaRising = DeltaTick * 5;

			if (SelectionRateSmoothed > SelectionRate)
				SelectionRateSmoothed = Math.Max(SelectionRate, SelectionRateSmoothed - Delta);
			else
				SelectionRateSmoothed = Math.Min(SelectionRate, SelectionRateSmoothed + DeltaRising);

			ErrorRate -= DeltaTick * 3;
			if (ErrorRate < 0)
				ErrorRate = 0;
		}

		public void MoveToHand(int id, int count, bool first, bool hidden = false, bool ExtraOffset = false)
		{
			var offsetforCard = (!first ? 0 : float.Pi) -
				float.Pi / 8 * ((id + 0.5f) * 2 / count - 1);

			var Rotation = new Quaternion(0, 0, (float)Math.Sin(offsetforCard / 2), (float)Math.Cos(offsetforCard / 2));


			Vector3 vecToRot = new(0, -6, 0);
			Vector3 Rot = new Vector3(Rotation.X, Rotation.Y, Rotation.Z);
			Vector3 loc = vecToRot + Vector3.Cross(2 * Rot, Rotation.W * vecToRot + Vector3.Cross(Rot, vecToRot));

			const float angl1 = 0.03489949f;
			const float angl2 = 0.99939082f;


			Transform tran = new(loc +
				new Vector3(0, first ? (ExtraOffset ? -9.5f : -10) : 10, 0.04f),
				Rotation * new Quaternion(0, hidden ? angl2 : angl1,
				0, hidden ? angl1 : angl2));

			bool IsFromHand = _Loc == CurrentLocation.HandFirst || _Loc == CurrentLocation.HandSecond;
			Transform Inter = tran;
			if (!IsFromHand)

			{
				Inter = new(new Vector3((id * 7f / count - 3.5f), 0, 1f), tran.Rotation);
				if (_Loc == CurrentLocation.Deck)
					Inter.Rotation = new Quaternion(0, -0.707f, 0, 0.707f);
			}
			MoveTo(Inter, tran, id * 0.05f);

			_Loc = first ? CurrentLocation.HandFirst : CurrentLocation.HandSecond;
		}
		public void MoveToTable(int id, int count, bool down)
		{
			const int cutoff = 4;

			var offset = new Vector3(down ? 0.2f : 0f, down ? 0.2f : 0f, down ? 0.05f : 0.08f);

			var location = new Vector3(-(id % cutoff - Math.Min(count, cutoff) * 0.5f + 0.5f) * 1.4f,
				(id / cutoff - (count - 1) / cutoff * 0.5f) * 2.1f, 0);

			var rot = EndTransform.Rotation;
			Vector3 Loc = Vector3.Zero;
			bool Hand = true;
			if (_Loc == CurrentLocation.HandFirst)
			{
				rot = new Quaternion(0, 0, 1, 0);
				Loc = new Vector3(0, -3, 0.1f);
			}
			else if (_Loc == CurrentLocation.HandSecond)
			{
				rot = new Quaternion(0, 0, 0, 1);
				Loc = new Vector3(0, 3, 0.1f);
			}
			else
				Hand = false;



			_Loc = down ? CurrentLocation.TableDown : CurrentLocation.TableUp;

			MoveTo(Hand ? new(Loc, rot) : new(location + offset, rot), new(location + offset, rot));
		}
		public event EventHandler OnMoveToDump;
		public void MoveToDump()
		{
			if (Quaternion.Dot(CurrentTransform.Rotation, new Quaternion(0,0,0,1)) > 0)
				MoveTo(new(DeckPosition.Position + new Vector3(0, 1f, 0.5f), new Quaternion(0, 1, 0, 0)), DumpPosition);
			else
				MoveTo(new(DeckPosition.Position + new Vector3(0, 1f, 0.5f), new Quaternion(1, 0, 0, 0)), DumpPositionRev);

			_Loc = CurrentLocation.Dump;
			if (OnMoveToDump != null)
				OnMoveToDump.Invoke(this, new EventArgs());
		}
		public void MoveToPreview(int id, float Delay)
		{
			Transform Target = new(new(id / 4 * 0.9f - 3.6f, id % 4 * 1.2f - 1.8f, 0.2f), new Quaternion(0, 0, 1, 0), 0.75f);
			MoveTo(new(new(3, 0, 0.5f), Target.Rotation), Target, Delay);
		}
		public void MoveToDeck(int pos, float delay = 0)
		{
			_Loc = CurrentLocation.Deck;
			var p = DeckPosition;
			p.Position.Z += pos*VisualCardBase.GetWidth();
			MoveTo(new(DeckPosition.Rotation), p, delay);
		}
		public void MoveToTrump(bool Pseudo = false, float delay = 0)
		{
			_Loc = CurrentLocation.Trump;
			if (Pseudo)
				CurrentTransform.Position -= new Vector3(0, 0, 0.02f);
			MoveTo(new(TrumpPosition.Position + new Vector3(0, 0, Pseudo ? -0.02f : 0f), TrumpPosition.Rotation), Pseudo ? PseudoTrumpPosition : TrumpPosition, delay);
		}

		public event EventHandler OnBeginMovement;
		public void MoveTo(Transform Through, Transform newLocation, float delay = 0)
		{
			BeginTranform = CurrentTransform;
			EndTransform = newLocation;
			double Dist = (BeginTranform.Position - EndTransform.Position).Length();

			double Dist1 = (BeginTranform.Position - Through.Position).Length();
			double Dist2 = (Through.Position - EndTransform.Position).Length();


			AnimationTotalTime = 0.01f + 0.3f * (float)Math.Sqrt(Dist1+Dist2);
			if (Dist <= 0.2f)
				InterTransform = EndTransform;
			else
				InterTransform = Through;
			AnimationNormalizedTime = -delay / AnimationTotalTime;
			Moving = true;
			if (OnBeginMovement != null)
				OnBeginMovement(this, new EventArgs());
		}
		public void TeleportTo(Transform newLocation)
		{
			BeginTranform = newLocation;
			EndTransform = newLocation;
			CurrentTransform = newLocation;
			AnimationNormalizedTime = 0.99f;
			AnimationTotalTime = 0.001f;
			Moving = true;
		}
		public void PlayErrorAnimation()
		{
			ErrorRate = 1f;
		}
		public Matrix4 GetMatrix()
		{
			var m = Matrix4.Translation(0, -Math.Abs(SelectionRateSmoothed) / 3, 0) * CurrentTransform;

			if (ErrorRate == 0)
				return m;
			float NormalizedTime = ErrorRate;
			bool fl = NormalizedTime > 0.5f;
			if (fl)
				NormalizedTime -= 1f;
			float Smoothed = (fl ? 2 : -2) * NormalizedTime * NormalizedTime + NormalizedTime;

			Matrix4 ErrorMatrix = Matrix4.RotationZ(Smoothed);
			return ErrorMatrix * m;
		}
		public Matrix4 GetMatrixCollision()
		{
			return CurrentTransform;
		}

		public static readonly Transform DeckPosition =
			new Transform(new (-5.5f, 0, 0.07f), new Quaternion(1, 0, 0, 0));
		public static readonly Transform TrumpPosition =
			new Transform(new (-5f, 0, 0.03f), new Quaternion(0, 0, 0.707f, 0.707f));
		public static readonly Transform PseudoTrumpPosition =
			new Transform(new (-5f, 0, 0.01f), new Quaternion(0, 0, 0.707f, 0.707f));
		public static readonly Transform DumpPosition =
			new Transform(new (-5.2f, -2.7f, 0.01f), new Quaternion(0.707f, -0.707f, 0, 0));
		public static readonly Transform DumpPositionRev =
			new Transform(new (-5.2f, -2.7f, 0.01f), new Quaternion(0.707f, +0.707f, 0, 0));

		public override string ToString()
		{
			return "Vis" + card;
		}
		static readonly Vector4[] Colors = [new Vector4(1, 1, 1, 1), new Vector4(0.5f, 0.5f, 0.5f, 1), new Vector4(0.8f, 0.8f, 0.8f, 0.5f)];
		public Vector4 GetColor()
		{
			return Colors[(int)CardType];
		}
		public float GetId()
		{
			return (int)card;
		}

		public float GetIdBack()
		{
			return 36;
		}


	}
}
