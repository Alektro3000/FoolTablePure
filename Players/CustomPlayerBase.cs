using Basic.Reference.Assemblies;
using CardTableFool.Main;
using CardTableFool.Tables;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace CardTableFool.Players
{
	internal class CustomPlayerBase
	{
		protected string DirectoryName;

		protected object Player;

		protected Type PlayerClass;
		protected Type SuitClass;
		protected Type CardClass;
		protected Type CardPairClass;
		protected Type CardPairListClass;

		protected Type TableClass;
		protected Assembly assembly;


		protected SCard Trump;
		protected CustomPlayerBase(CustomPlayerBase player)
		{
			assembly = player.assembly;

			PlayerClass = player.PlayerClass;
			SuitClass = player.SuitClass;
			CardClass = player.CardClass;
			CardPairClass = player.CardPairClass;
			CardPairListClass = player.CardPairListClass;
		}

		public CustomPlayerBase(string PathToCode, bool FromDll = false)
		{
			if (FromDll && (File.Exists(PathToCode) || (Directory.Exists(PathToCode) && Directory.EnumerateFiles(PathToCode, "*.dll").Count() != 0)))
			{
				if (File.Exists(PathToCode))
				{
					var a = File.ReadAllBytes(PathToCode);
					assembly = Assembly.Load(a);

				}
				else
				{
					var a = File.ReadAllBytes(Directory.EnumerateFiles(PathToCode, "*.dll").First());
					assembly = Assembly.Load(a);
				}
			}
			else
				//Загружаем скопилированную библиотеку
				assembly = Compile(PathToCode);
			string NameSpace;
			try
			{
				NameSpace = assembly.DefinedTypes.Where(x => x.Name == "MPlayer1" || x.Name == "MPlayer2").Select(x => x.Namespace).First();
			}
			catch
			{
				throw new Exception("Player class not found");
			}
			DirectoryName = Path.GetFileName(PathToCode);
			//Так как мы подгрузили библиотеку в рантайме, мы не можем использовать обычные функции
			//Для того чтобы использовать функции классов нам нужны ссылки на эти классы
			PlayerClass = assembly.GetType(NameSpace + ".MPlayer1");

			//Пробуем ещё раз(может быть поможет)
			if (PlayerClass == null)
				PlayerClass = assembly.GetType(NameSpace + ".MPlayer2");
			if (PlayerClass == null)
				throw new Exception("Player class not found");

			SuitClass = assembly.GetType(NameSpace + ".Suits") 
				?? throw new Exception("Suits enum not found");

			CardClass = assembly.GetType(NameSpace + ".SCard") 
				?? throw new Exception("SCard struct not found");

			CardPairClass = assembly.GetType(NameSpace + ".SCardPair") 
				?? throw new Exception("SCardPair struct not found");

			TableClass = assembly.GetType(NameSpace + ".MTable");
			//Сокращение для List<SCardPair>
			CardPairListClass = typeof(List<>).MakeGenericType(CardPairClass);


			//Создаём игрока
			Player = assembly.CreateInstance(PlayerClass.FullName);
		}

		private static Assembly Compile(string DirectoryPath)
		{
			string DirectoryLocation = DirectoryPath;

			List<string> FilesToLoad = Directory.GetFiles(DirectoryLocation)
				.ToList().FindAll(x => Path.GetExtension(x) == ".cs");
			FilesToLoad.AddRange(Directory.GetFiles(Path.GetDirectoryName(DirectoryLocation))
				.ToList().FindAll(x => Path.GetExtension(x) == ".cs"));
			if (FilesToLoad.Count == 0)
				throw new Exception("No Files to compile was selected");
			List<SyntaxTree> syntaxTrees = FilesToLoad.ConvertAll(x => CSharpSyntaxTree.ParseText(File.ReadAllText(x)));


			string assemblyName = Path.GetRandomFileName();

			//Собираем промежуточный код из cинтаксического леса
			CSharpCompilation compilation = CSharpCompilation.Create(
				assemblyName,
				syntaxTrees: syntaxTrees,
				references: References(),
				options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

			using (var ms = new MemoryStream())
			{
				EmitResult result = compilation.Emit(ms);

				if (!result.Success)
				{
					//Обрабатываем ошибки компиляции
					IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
						diagnostic.IsWarningAsError ||
						diagnostic.Severity == DiagnosticSeverity.Error);

					string outError = "Failed to Compile File: \n";

					foreach (Diagnostic diagnostic in failures)
					{
						outError += $"{diagnostic.Id}: {diagnostic.GetMessage()}\n";
						Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
					}

					throw new FileLoadException(outError);
				}
				else
				{
					ms.Seek(0, SeekOrigin.Begin);
					if (Program.GetBoolSetting(Program.GlobalSetting.SaveCompiled))
					{
						var file = File.Create(Path.Combine(DirectoryPath, "Compiled.dll"));
						ms.WriteTo(file);
						file.Close();
					}
					return (Assembly.Load(ms.ToArray()));
				}
			}

		}
		static IEnumerable<PortableExecutableReference> references;
		static IEnumerable<PortableExecutableReference> References()
		{
			if (references == null)
				references = ReferenceAssemblies.Net80;
			return references;
		}

		//Конвертирует стандартную масть в масть из сборки
		protected object ConvertSuit(Suits Suit)
		{
			//Мы можем использовать методы Enum
			var ans = Enum.ToObject(SuitClass, Suit);
			return ans;
		}
		//Конвертирует масть из сборки в стандартную масть
		protected Suits ConvertSuit(object Suit)
		{
			//Мы можем практически напрямую приводить к Suits
			var ans = (Suits)(int)Suit;
			return ans;
		}
		//Конвертирует стандартную карту в карту из сборки
		protected object ConvertCard(SCard Card)
		{
			var ans = Activator.CreateInstance(CardClass, [ConvertSuit(Card.Suit), Card.Rank]);
			return ans;
		}
		//Конвертирует карту из сборки в стандартную карту
		protected SCard ConvertCard(object Card)
		{
			Suits suit = ConvertSuit((int)CardClass.GetMethod("get_Suit").Invoke(Card, []));
			var ans = new SCard(suit, (int)CardClass.GetMethod("get_Rank").Invoke(Card, []));
			return ans;
		}
		private object ConvertPair(SCardPair Pair)
		{
			object? alienPair = Activator.CreateInstance(CardPairClass, [ConvertCard(Pair.Down)]);
			if (Pair.Beaten)
				CardPairClass.GetMethod("SetUp").Invoke(alienPair, [ConvertCard(Pair.Up), ConvertSuit(Trump.Suit)]);

			return alienPair;
		}

		//Немного боли
		protected SCardPair ConvertPair(object Pair)
		{
			SCardPair pair = new SCardPair(
				ConvertCard(CardPairClass.GetMethod("get_Down").Invoke(Pair, [])));
			if ((bool)CardPairClass.GetMethod("get_Beaten").Invoke(Pair, []))
			{
				SCard SCar = ConvertCard(CardPairClass.GetMethod("get_Up").Invoke(Pair, []));
				pair.SetUp(SCar, Trump.Suit);

				//Исключение для более удобной отладки
				if (!pair.Beaten)
					throw new MTable.GameException(false, [],
						$"{this} think, that {SCar} can beat {pair.Down}, trump: {Trump}");
			}
			return pair;
		}

		//
		protected object ConvertPairList(List<SCardPair> PairList)
		{
			object? Ac = Activator.CreateInstance(CardPairListClass);
			for (int i = 0; i < PairList.Count; i++)
				CardPairListClass.GetMethod("Add").Invoke(Ac, [ConvertPair(PairList[i])]);
			return Ac;
		}
	}
}
