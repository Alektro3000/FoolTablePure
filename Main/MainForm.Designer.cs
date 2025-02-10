
using CardTableFool;
using CardTableFool.Forms;
using StbImageSharp;
using System.Diagnostics;
using System.Windows.Forms;

namespace CardTableFool.Main
{
	partial class MainForm
	{
		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
				DXControl.Dispose();
				ResultsSubForm.Dispose();
				_timer.Dispose();
			}
			base.Dispose(disposing);
		}
		protected virtual void InitializeForm()
		{
			InitializeComponent();

			DXControl = new VisualTourControl();
			DXControl.Location = new Point(0, 0);
			DXControl.Margin = new Padding(0);
			DXControl.Name = "dxControl";
			DXControl.Size = new Size(ClientSize.Width, ClientSize.Height);
			DXControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			DXControl.TabIndex = 0;
			DXControl.Text = "dxControl";
			DXControl.DoubleClick += (object x, EventArgs e) => PlayerInput.OnClick();
			DXControl.Click += (object x, EventArgs e) => PlayerInput.OnClick();
			DXControl.OnMouseMoveWorldSpace += (object x, EventArgs e) => PlayerInput.UpdatePointer((e as MouseMoveWorldSpace).Location);

			GameGroup.Controls.Add(DXControl);

			ResultsSubForm = new ResultForm(true);
			ResultsSubForm.ShowGameRecord += OnRecallGame;
			ResultsSubForm.RepeatGameFromRecord += OnRepeatGame;
			ResultsSubForm.FormBorderStyle = FormBorderStyle.None;
			ResultsSubForm.TopLevel = false;
			ResultsSubForm.ShowInTaskbar = false;
			ResultsSubForm.Location = new Point(0, 32);
			ResultsSubForm.Size = new Size(ClientSize.Width, ClientSize.Height - 32);
			ResultsSubForm.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

			Controls.Add(ResultsSubForm);

		}


		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			Menu = new MenuStrip();
			Settings = new ToolStripMenuItem();
			ActionButton = new Button();
			openDirectory = new FolderBrowserDialog();
			SecondPlayerName = new TextBox();
			FirstPlayerName = new TextBox();
			GameGroup = new GroupBox();
			Undo = new Button();
			AutoplayButton = new Button();
			SetUpDllDialog = new OpenFileDialog();
			toolTip1 = new ToolTip(components);
			Menu.SuspendLayout();
			GameGroup.SuspendLayout();
			SuspendLayout();
			// 
			// Menu
			// 
			Menu.Dock = DockStyle.None;
			Menu.ImageScalingSize = new Size(21, 21);
			Menu.Items.AddRange(new ToolStripItem[] { Settings });
			Menu.Location = new Point(0, 0);
			Menu.Name = "Menu";
			Menu.Size = new Size(109, 29);
			Menu.TabIndex = 2;
			Menu.Text = "menuStrip1";
			// 
			// Settings
			// 
			Settings.Name = "Settings";
			Settings.Size = new Size(101, 25);
			Settings.Text = "Настройки";
			Settings.Click += Settings_Click;
			// 
			// ActionButton
			// 
			ActionButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			ActionButton.Font = new Font("Segoe UI", 15F);
			ActionButton.Location = new Point(0, 625);
			ActionButton.Name = "ActionButton";
			ActionButton.Size = new Size(402, 47);
			ActionButton.TabIndex = 3;
			ActionButton.Text = "Выберите карты";
			ActionButton.UseVisualStyleBackColor = true;
			ActionButton.Click += ActionButton_Click;
			// 
			// openDirectory
			// 
			openDirectory.AddToRecent = false;
			openDirectory.Description = "Выберите папку исходных кодов первого игрока";
			openDirectory.UseDescriptionForTitle = true;
			// 
			// SecondPlayerName
			// 
			SecondPlayerName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			SecondPlayerName.Font = new Font("Segoe UI", 19F);
			SecondPlayerName.Location = new Point(819, 0);
			SecondPlayerName.Name = "SecondPlayerName";
			SecondPlayerName.ReadOnly = true;
			SecondPlayerName.Size = new Size(243, 52);
			SecondPlayerName.TabIndex = 4;
			// 
			// FirstPlayerName
			// 
			FirstPlayerName.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			FirstPlayerName.Font = new Font("Segoe UI", 19F);
			FirstPlayerName.Location = new Point(819, 620);
			FirstPlayerName.Name = "FirstPlayerName";
			FirstPlayerName.ReadOnly = true;
			FirstPlayerName.Size = new Size(245, 52);
			FirstPlayerName.TabIndex = 5;
			// 
			// GameGroup
			// 
			GameGroup.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			GameGroup.Controls.Add(Undo);
			GameGroup.Controls.Add(AutoplayButton);
			GameGroup.Controls.Add(FirstPlayerName);
			GameGroup.Controls.Add(SecondPlayerName);
			GameGroup.Controls.Add(ActionButton);
			GameGroup.Location = new Point(0, 0);
			GameGroup.Margin = new Padding(0);
			GameGroup.Name = "GameGroup";
			GameGroup.Padding = new Padding(0);
			GameGroup.Size = new Size(1062, 672);
			GameGroup.TabIndex = 6;
			GameGroup.TabStop = false;
			GameGroup.Text = "groupBox1";
			// 
			// Undo
			// 
			Undo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			Undo.Font = new Font("Segoe UI", 15F);
			Undo.Location = new Point(408, 625);
			Undo.Name = "Undo";
			Undo.Size = new Size(115, 47);
			Undo.TabIndex = 7;
			Undo.Text = "Назад";
			Undo.UseVisualStyleBackColor = true;
			Undo.Click += Undo_Click;
			Undo.DoubleClick += Undo_Click;
			// 
			// AutoplayButton
			// 
			AutoplayButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			AutoplayButton.Font = new Font("Segoe UI", 15F);
			AutoplayButton.Location = new Point(0, 579);
			AutoplayButton.Name = "AutoplayButton";
			AutoplayButton.Size = new Size(112, 47);
			AutoplayButton.TabIndex = 6;
			AutoplayButton.Text = "Авто";
			AutoplayButton.UseVisualStyleBackColor = true;
			AutoplayButton.Click += Autoplay_Click;
			AutoplayButton.DoubleClick += Autoplay_Click;
			// 
			// SetUpDllDialog
			// 
			SetUpDllDialog.AddToRecent = false;
			SetUpDllDialog.DefaultExt = "dll";
			SetUpDllDialog.Filter = "|.dll";
			SetUpDllDialog.Tag = "Выберите файл dll для игрока";
			// 
			// toolTip1
			// 
			toolTip1.Tag = "";
			toolTip1.ToolTipIcon = ToolTipIcon.Info;
			toolTip1.ToolTipTitle = "Шаг назад";
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(9F, 21F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1062, 672);
			Controls.Add(Menu);
			Controls.Add(GameGroup);
			MainMenuStrip = Menu;
			Name = "MainForm";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "Durak, sam durak";
			WindowState = FormWindowState.Maximized;
			Load += GameForm_Load;
			Menu.ResumeLayout(false);
			Menu.PerformLayout();
			GameGroup.ResumeLayout(false);
			GameGroup.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}



		#endregion
		
		protected VisualTourControl DXControl;
		protected MenuStrip Menu;
		protected Button ActionButton;
		protected FolderBrowserDialog openDirectory;
		protected TextBox SecondPlayerName;
		protected TextBox FirstPlayerName;
		protected GroupBox GameGroup;
		protected Button AutoplayButton;
		protected OpenFileDialog SetUpDllDialog;
		protected ToolTip toolTip1;
		protected Button Undo;
		protected ToolStripMenuItem Settings;
		private System.ComponentModel.IContainer components;
	}
}
