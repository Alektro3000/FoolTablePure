
using CardTableFool;
using CardTableFool.Forms;
using StbImageSharp;
using System.Diagnostics;
using System.Windows.Forms;

namespace CardTableFool.Main
{
    partial class GameForm
    {
        protected override void InitializeForm() {
			base.InitializeForm();
            InitializeComponent();            
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			SetUpPlayers = new ToolStripMenuItem();
			FirstPlayerMenu = new ToolStripMenuItem();
			SetFirstPlayer = new ToolStripMenuItem();
			SetDllFirstPlayer = new ToolStripMenuItem();
			ResetFirstPlayer = new ToolStripMenuItem();
			RecompileFirst = new ToolStripMenuItem();
			SecondPlayerMenu = new ToolStripMenuItem();
			SetSecondPlayer = new ToolStripMenuItem();
			SetDllSecondPlayer = new ToolStripMenuItem();
			ResetSecondPlayer = new ToolStripMenuItem();
			RecompileSecond = new ToolStripMenuItem();
			SingleGame = new ToolStripMenuItem();
			humanToolStripMenuItem = new ToolStripMenuItem();
			oneMatchToolStripMenuItem = new ToolStripMenuItem();
			batchMatchToolStripMenuItem = new ToolStripMenuItem();
			ShowResult = new ToolStripMenuItem();
			GameGroup.SuspendLayout();
			SuspendLayout();

			Menu.Items.AddRange(new ToolStripItem[] {SetUpPlayers, SingleGame, ShowResult});
			// 
			// FirstPlayerName
			// 
			FirstPlayerName.Location = new Point(814, 620);
			// 
			// SetUpPlayers
			// 
			SetUpPlayers.DropDownItems.AddRange(new ToolStripItem[] { FirstPlayerMenu, SecondPlayerMenu });
			SetUpPlayers.Name = "SetUpPlayers";
			SetUpPlayers.Size = new Size(170, 25);
			SetUpPlayers.Text = "Установить Игроков";
			SetUpPlayers.DropDownOpened += SetUpPlayers_DropDownOpened;
			SetUpPlayers.Click += SetUpPlayers_Click_1;
			// 
			// FirstPlayerMenu
			// 
			FirstPlayerMenu.DropDownItems.AddRange(new ToolStripItem[] { SetFirstPlayer, SetDllFirstPlayer, ResetFirstPlayer, RecompileFirst });
			FirstPlayerMenu.Name = "FirstPlayerMenu";
			FirstPlayerMenu.Size = new Size(160, 30);
			FirstPlayerMenu.Text = "Первого";
			// 
			// SetFirstPlayer
			// 
			SetFirstPlayer.Name = "SetFirstPlayer";
			SetFirstPlayer.Size = new Size(248, 30);
			SetFirstPlayer.Text = "Установить";
			SetFirstPlayer.ToolTipText = "Устанавливает игрока из папки, если в папке есть dll достаёт его, иначе компилирует из исходного кода";
			SetFirstPlayer.Click += SetFirstPlayer_Click;
			// 
			// SetDllFirstPlayer
			// 
			SetDllFirstPlayer.Name = "SetDllFirstPlayer";
			SetDllFirstPlayer.Size = new Size(248, 30);
			SetDllFirstPlayer.Text = "Установить из dll";
			SetDllFirstPlayer.ToolTipText = "Устанавливает игрока из dll сборки";
			SetDllFirstPlayer.Click += SetDllFirstPlayer_Click;
			// 
			// ResetFirstPlayer
			// 
			ResetFirstPlayer.Name = "ResetFirstPlayer";
			ResetFirstPlayer.Size = new Size(248, 30);
			ResetFirstPlayer.Text = "Сбросить";
			ResetFirstPlayer.ToolTipText = "Сбрасывает игрока до базовой версии";
			ResetFirstPlayer.Click += ResetFirstPlayer_Click;
			// 
			// RecompileFirst
			// 
			RecompileFirst.Name = "RecompileFirst";
			RecompileFirst.Size = new Size(248, 30);
			RecompileFirst.Text = "Перекомпилировать";
			RecompileFirst.ToolTipText = "Компилирует игрока из папки с исходным кодом\r\n";
			RecompileFirst.Click += RecompileFirst_Click;
			// 
			// SecondPlayerMenu
			// 
			SecondPlayerMenu.DropDownItems.AddRange(new ToolStripItem[] { SetSecondPlayer, SetDllSecondPlayer, ResetSecondPlayer, RecompileSecond });
			SecondPlayerMenu.Name = "SecondPlayerMenu";
			SecondPlayerMenu.Size = new Size(160, 30);
			SecondPlayerMenu.Text = "Второго";
			// 
			// SetSecondPlayer
			// 
			SetSecondPlayer.Name = "SetSecondPlayer";
			SetSecondPlayer.Size = new Size(248, 30);
			SetSecondPlayer.Text = "Установить";
			SetSecondPlayer.ToolTipText = "Устанавливает игрока из папки, если в папке есть dll достаёт его, иначе компилирует из исходного кода";
			SetSecondPlayer.Click += SetSecondPlayer_Click;
			// 
			// SetDllSecondPlayer
			// 
			SetDllSecondPlayer.Name = "SetDllSecondPlayer";
			SetDllSecondPlayer.Size = new Size(248, 30);
			SetDllSecondPlayer.Text = "Установить из dll";
			SetDllSecondPlayer.ToolTipText = "Устанавливает игрока из dll сборки";
			SetDllSecondPlayer.Click += SetDllSecondPlayer_Click;
			// 
			// ResetSecondPlayer
			// 
			ResetSecondPlayer.Name = "ResetSecondPlayer";
			ResetSecondPlayer.Size = new Size(248, 30);
			ResetSecondPlayer.Text = "Сбросить";
			ResetSecondPlayer.ToolTipText = "Сбрасывает игрока до базовой версии";
			ResetSecondPlayer.Click += ResetSecondPlayer_Click;
			// 
			// RecompileSecond
			// 
			RecompileSecond.Name = "RecompileSecond";
			RecompileSecond.Size = new Size(248, 30);
			RecompileSecond.Text = "Перекомпилировать";
			RecompileSecond.ToolTipText = "Компилирует игрока из папки с исходным кодом";
			RecompileSecond.Click += RecompileSecond_Click;
			// 
			// SingleGame
			// 
			SingleGame.DropDownItems.AddRange(new ToolStripItem[] { humanToolStripMenuItem, oneMatchToolStripMenuItem, batchMatchToolStripMenuItem });
			SingleGame.Name = "SingleGame";
			SingleGame.Size = new Size(104, 25);
			SingleGame.Text = "Новая игра";
			SingleGame.Click += SingleGame_Click;
			// 
			// humanToolStripMenuItem
			// 
			humanToolStripMenuItem.Name = "humanToolStripMenuItem";
			humanToolStripMenuItem.Size = new Size(265, 30);
			humanToolStripMenuItem.Text = "Матч против Человека";
			humanToolStripMenuItem.ToolTipText = "Проводит матч между первым игроком и человеком";
			humanToolStripMenuItem.Click += HumanGame_Click;
			// 
			// oneMatchToolStripMenuItem
			// 
			oneMatchToolStripMenuItem.Name = "oneMatchToolStripMenuItem";
			oneMatchToolStripMenuItem.Size = new Size(265, 30);
			oneMatchToolStripMenuItem.Text = "Одиночный матч";
			oneMatchToolStripMenuItem.ToolTipText = "Проводит матч между первым и вторым игроком";
			oneMatchToolStripMenuItem.Click += NewSingleGame_Click;
			// 
			// batchMatchToolStripMenuItem
			// 
			batchMatchToolStripMenuItem.Name = "batchMatchToolStripMenuItem";
			batchMatchToolStripMenuItem.Size = new Size(265, 30);
			batchMatchToolStripMenuItem.Text = "Пачка матчей";
			batchMatchToolStripMenuItem.ToolTipText = "Проводит пачку матчей между двумя игроками";
			batchMatchToolStripMenuItem.Click += NewBatchGame_Click;
			// 
			// ShowResult
			// 
			ShowResult.Name = "ShowResult";
			ShowResult.Size = new Size(164, 25);
			ShowResult.Text = "Показать результат";
			ShowResult.ToolTipText = "Показывает результаты пачек битв между игроками";
			ShowResult.Click += ShowResult_Click;
			// 
			// GameForm
			// 
			AutoScaleDimensions = new SizeF(9F, 21F);
			ClientSize = new Size(1062, 672);
			Name = "GameForm";
			GameGroup.ResumeLayout(false);
			GameGroup.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}



		#endregion

		private ToolStripMenuItem SingleGame;
		private ToolStripMenuItem humanToolStripMenuItem;
        private ToolStripMenuItem oneMatchToolStripMenuItem;
        private ToolStripMenuItem batchMatchToolStripMenuItem;
        private ToolStripMenuItem SetUpPlayers;
        private ToolStripMenuItem ShowResult;
        private ToolStripMenuItem FirstPlayerMenu;
        private ToolStripMenuItem SetFirstPlayer;
        private ToolStripMenuItem ResetFirstPlayer;
        private ToolStripMenuItem SetDllFirstPlayer;
        private ToolStripMenuItem SecondPlayerMenu;
        private ToolStripMenuItem SetSecondPlayer;
        private ToolStripMenuItem ResetSecondPlayer;
        private ToolStripMenuItem SetDllSecondPlayer;
        private ToolStripMenuItem RecompileFirst;
        private ToolStripMenuItem RecompileSecond;
    }
}
