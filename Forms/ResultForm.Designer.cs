namespace CardTableFool
{
    partial class ResultForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TabControl = new TabControl();
            BatchesTab = new TabPage();
            BatchRecords = new Forms.BatchControl(ShowLoadButton);
            BatchScrollBar = new VScrollBar();
            GamesTab = new TabPage();
            GameInfos = new Forms.ResultControl();
            GameScrollBar = new VScrollBar();
            SaveBatch = new SaveFileDialog();
            OpenFile = new OpenFileDialog();
            TabControl.SuspendLayout();
            BatchesTab.SuspendLayout();
            GamesTab.SuspendLayout();
            SuspendLayout();
            // 
            // TabControl
            // 
            TabControl.Controls.Add(BatchesTab);
            TabControl.Controls.Add(GamesTab);
            TabControl.Dock = DockStyle.Fill;
            TabControl.Location = new Point(0, 0);
            TabControl.Margin = new Padding(0);
            TabControl.Name = "TabControl";
            TabControl.Padding = new Point(0, 0);
            TabControl.SelectedIndex = 0;
            TabControl.Size = new Size(820, 450);
            TabControl.TabIndex = 0;
            // 
            // BatchesTab
            // 
            BatchesTab.Controls.Add(BatchRecords);
            BatchesTab.Controls.Add(BatchScrollBar);
            BatchesTab.Location = new Point(4, 30);
            BatchesTab.Name = "BatchesTab";
            BatchesTab.Size = new Size(812, 416);
            BatchesTab.TabIndex = 1;
            BatchesTab.Text = "Пачки";
            BatchesTab.UseVisualStyleBackColor = true;
            // 
            // BatchRecords
            // 
            BatchRecords.Dock = DockStyle.Fill;
            BatchRecords.Location = new Point(0, 0);
            BatchRecords.Margin = new Padding(0);
            BatchRecords.Name = "BatchRecords";
            BatchRecords.Size = new Size(792, 416);
            BatchRecords.TabIndex = 0;
            BatchRecords.Text = "Batch";
            // 
            // BatchScrollBar
            // 
            BatchScrollBar.Dock = DockStyle.Right;
            BatchScrollBar.Location = new Point(792, 0);
            BatchScrollBar.Name = "BatchScrollBar";
            BatchScrollBar.Size = new Size(20, 416);
            BatchScrollBar.TabIndex = 1;
            BatchScrollBar.Scroll += Batch_Scroll;
            // 
            // GamesTab
            // 
            GamesTab.Controls.Add(GameInfos);
            GamesTab.Controls.Add(GameScrollBar);
            GamesTab.Location = new Point(4, 30);
            GamesTab.Margin = new Padding(0);
            GamesTab.Name = "GamesTab";
            GamesTab.Size = new Size(812, 416);
            GamesTab.TabIndex = 2;
            GamesTab.Text = "Игры";
            GamesTab.UseVisualStyleBackColor = true;
            // 
            // GameInfos
            // 
            GameInfos.Dock = DockStyle.Fill;
            GameInfos.Location = new Point(0, 0);
            GameInfos.Margin = new Padding(0);
            GameInfos.Name = "GameInfos";
            GameInfos.Size = new Size(792, 416);
            GameInfos.TabIndex = 1;
            GameInfos.Text = "GameInfos";
            // 
            // GameScrollBar
            // 
            GameScrollBar.Dock = DockStyle.Right;
            GameScrollBar.Location = new Point(792, 0);
            GameScrollBar.Name = "GameScrollBar";
            GameScrollBar.ScaleScrollBarForDpiChange = false;
            GameScrollBar.Size = new Size(20, 416);
            GameScrollBar.TabIndex = 0;
            GameScrollBar.Scroll += Game_Scroll;
            // 
            // SaveBatch
            // 
            SaveBatch.DefaultExt = "json";
            SaveBatch.ExpandedMode = false;
            SaveBatch.FileName = "Batchinfo";
            SaveBatch.Tag = "Сохранить результаты пачки";
            SaveBatch.Title = "Сохранить результаты пачки";
            // 
            // OpenFile
            // 
            OpenFile.DefaultExt = "json";
            OpenFile.FileName = "BatchInfo.json";
            OpenFile.Tag = "Загрузить запись пачки игр";
            OpenFile.Title = "Загрузить запись пачки игр";
            // 
            // ResultForm
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(820, 450);
            Controls.Add(TabControl);
            Name = "ResultForm";
            Text = "ResultForm";
            TabControl.ResumeLayout(false);
            BatchesTab.ResumeLayout(false);
            GamesTab.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl TabControl;
        private TabPage GamesTab;
        private Panel panel1;
        private TabPage BatchesTab;
        private VScrollBar GameScrollBar;
        private Forms.ResultControl GameInfos;
        private VScrollBar BatchScrollBar;
        private Forms.BatchControl BatchRecords;
        private SaveFileDialog SaveBatch;
        private OpenFileDialog OpenFile;
    }
}