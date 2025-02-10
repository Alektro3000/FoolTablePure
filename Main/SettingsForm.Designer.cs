namespace CardTableFool.Main
{
    partial class SettingsForm
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
            components = new System.ComponentModel.Container();
            Load = new Button();
            TourMode = new CheckBox();
            HideCards = new CheckBox();
            HideDump = new CheckBox();
            HideDeck = new CheckBox();
            SaveCompiled = new CheckBox();
            StrictCheck = new CheckBox();
            Json = new CheckBox();
            textBox1 = new TextBox();
            BatchCount = new TextBox();
            Tooltip = new ToolTip(components);
            Switch = new TextBox();
            textBox3 = new TextBox();
            Candidate = new TextBox();
            textBox5 = new TextBox();
            Animation = new TextBox();
            textBox4 = new TextBox();
            TableDepth = new TextBox();
            textBox6 = new TextBox();
            Surnames = new CheckBox();
            AttackTotal = new CheckBox();
            TourCount = new TextBox();
            textBox7 = new TextBox();
            SuspendLayout();
            // 
            // Load
            // 
            Load.Location = new Point(289, 254);
            Load.Name = "Load";
            Load.Size = new Size(190, 31);
            Load.TabIndex = 0;
            Load.Text = "Применить настройки";
            Load.UseVisualStyleBackColor = true;
            Load.Click += Load_Click;
            // 
            // TourMode
            // 
            TourMode.AutoSize = true;
            TourMode.Location = new Point(12, 12);
            TourMode.Name = "TourMode";
            TourMode.Size = new Size(144, 25);
            TourMode.TabIndex = 1;
            TourMode.Text = "Режим турнира";
            TourMode.UseVisualStyleBackColor = true;
            // 
            // HideCards
            // 
            HideCards.AutoSize = true;
            HideCards.Location = new Point(12, 105);
            HideCards.Name = "HideCards";
            HideCards.Size = new Size(213, 25);
            HideCards.TabIndex = 2;
            HideCards.Text = "Скрыть карты оппонента";
            HideCards.UseVisualStyleBackColor = true;
            // 
            // HideDump
            // 
            HideDump.AutoSize = true;
            HideDump.Location = new Point(12, 167);
            HideDump.Name = "HideDump";
            HideDump.Size = new Size(190, 25);
            HideDump.TabIndex = 3;
            HideDump.Text = "Скрыть стопку сброса";
            HideDump.UseVisualStyleBackColor = true;
            // 
            // HideDeck
            // 
            HideDeck.AutoSize = true;
            HideDeck.Location = new Point(12, 136);
            HideDeck.Name = "HideDeck";
            HideDeck.Size = new Size(194, 25);
            HideDeck.TabIndex = 4;
            HideDeck.Text = "Скрыть стопку добора";
            HideDeck.UseVisualStyleBackColor = true;
            // 
            // SaveCompiled
            // 
            SaveCompiled.AutoSize = true;
            SaveCompiled.Location = new Point(12, 198);
            SaveCompiled.Name = "SaveCompiled";
            SaveCompiled.Size = new Size(198, 25);
            SaveCompiled.TabIndex = 5;
            SaveCompiled.Text = "Сохранять Compiled.dll";
            SaveCompiled.UseVisualStyleBackColor = true;
            // 
            // StrictCheck
            // 
            StrictCheck.AutoSize = true;
            StrictCheck.Location = new Point(12, 43);
            StrictCheck.Name = "StrictCheck";
            StrictCheck.Size = new Size(263, 25);
            StrictCheck.TabIndex = 6;
            StrictCheck.Text = "Карты на столе <= Карты в руке";
            StrictCheck.UseVisualStyleBackColor = true;
            // 
            // Json
            // 
            Json.AutoSize = true;
            Json.Location = new Point(12, 229);
            Json.Name = "Json";
            Json.Size = new Size(124, 25);
            Json.TabIndex = 7;
            Json.Text = "Сжатый Json";
            Json.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(289, 12);
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(161, 29);
            textBox1.TabIndex = 8;
            textBox1.Text = "Игры в пачке:";
            // 
            // BatchCount
            // 
            BatchCount.Location = new Point(456, 12);
            BatchCount.Name = "BatchCount";
            BatchCount.Size = new Size(106, 29);
            BatchCount.TabIndex = 9;
            // 
            // Tooltip
            // 
            Tooltip.AutoPopDelay = 1000;
            Tooltip.InitialDelay = 500;
            Tooltip.ReshowDelay = 100;
            // 
            // Switch
            // 
            Switch.Location = new Point(456, 82);
            Switch.Name = "Switch";
            Switch.Size = new Size(106, 29);
            Switch.TabIndex = 11;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(289, 82);
            textBox3.Name = "textBox3";
            textBox3.ReadOnly = true;
            textBox3.Size = new Size(161, 29);
            textBox3.TabIndex = 10;
            textBox3.Text = "Смена очерёдности";
            // 
            // Candidate
            // 
            Candidate.Location = new Point(456, 117);
            Candidate.Name = "Candidate";
            Candidate.Size = new Size(106, 29);
            Candidate.TabIndex = 13;
            // 
            // textBox5
            // 
            textBox5.Location = new Point(289, 117);
            textBox5.Name = "textBox5";
            textBox5.ReadOnly = true;
            textBox5.Size = new Size(161, 29);
            textBox5.TabIndex = 12;
            textBox5.Text = "Кол-во Кандидатов";
            // 
            // Animation
            // 
            Animation.Location = new Point(456, 152);
            Animation.Name = "Animation";
            Animation.Size = new Size(106, 29);
            Animation.TabIndex = 15;
            // 
            // textBox4
            // 
            textBox4.Location = new Point(289, 152);
            textBox4.Name = "textBox4";
            textBox4.ReadOnly = true;
            textBox4.Size = new Size(161, 29);
            textBox4.TabIndex = 14;
            textBox4.Text = "Скорость Анимаций";
            // 
            // TableDepth
            // 
            TableDepth.Location = new Point(456, 187);
            TableDepth.Name = "TableDepth";
            TableDepth.Size = new Size(106, 29);
            TableDepth.TabIndex = 17;
            // 
            // textBox6
            // 
            textBox6.Location = new Point(289, 187);
            textBox6.Name = "textBox6";
            textBox6.ReadOnly = true;
            textBox6.Size = new Size(161, 29);
            textBox6.TabIndex = 16;
            textBox6.Text = "Глубина PlayOff";
            // 
            // Surnames
            // 
            Surnames.AutoSize = true;
            Surnames.Location = new Point(12, 260);
            Surnames.Name = "Surnames";
            Surnames.Size = new Size(187, 25);
            Surnames.TabIndex = 18;
            Surnames.Text = "Показывать Фамилии";
            Surnames.UseVisualStyleBackColor = true;
            // 
            // AttackTotal
            // 
            AttackTotal.AutoSize = true;
            AttackTotal.Location = new Point(12, 74);
            AttackTotal.Name = "AttackTotal";
            AttackTotal.Size = new Size(179, 25);
            AttackTotal.TabIndex = 19;
            AttackTotal.Text = "Карты на столе <= 6";
            AttackTotal.UseVisualStyleBackColor = true;
            // 
            // TourCount
            // 
            TourCount.Location = new Point(456, 47);
            TourCount.Name = "TourCount";
            TourCount.Size = new Size(106, 29);
            TourCount.TabIndex = 21;
            // 
            // textBox7
            // 
            textBox7.Location = new Point(289, 47);
            textBox7.Name = "textBox7";
            textBox7.ReadOnly = true;
            textBox7.Size = new Size(161, 29);
            textBox7.TabIndex = 20;
            textBox7.Text = "Игры в турнире:";
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(568, 295);
            Controls.Add(TourCount);
            Controls.Add(textBox7);
            Controls.Add(AttackTotal);
            Controls.Add(Surnames);
            Controls.Add(TableDepth);
            Controls.Add(textBox6);
            Controls.Add(Animation);
            Controls.Add(textBox4);
            Controls.Add(Candidate);
            Controls.Add(textBox5);
            Controls.Add(Switch);
            Controls.Add(textBox3);
            Controls.Add(BatchCount);
            Controls.Add(textBox1);
            Controls.Add(Json);
            Controls.Add(StrictCheck);
            Controls.Add(SaveCompiled);
            Controls.Add(HideDeck);
            Controls.Add(HideDump);
            Controls.Add(HideCards);
            Controls.Add(TourMode);
            Controls.Add(Load);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "SettingsForm";
            Text = "Настройки";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Load;
        private CheckBox TourMode;
        private CheckBox HideCards;
        private CheckBox HideDump;
        private CheckBox HideDeck;
        private CheckBox SaveCompiled;
        private CheckBox StrictCheck;
        private CheckBox Json;
        private TextBox textBox1;
        private TextBox BatchCount;
        private ToolTip Tooltip;
        private TextBox Switch;
        private TextBox textBox3;
        private TextBox Candidate;
        private TextBox textBox5;
        private TextBox Animation;
        private TextBox textBox4;
        private TextBox TableDepth;
        private TextBox textBox6;
        private CheckBox Surnames;
        private CheckBox AttackTotal;
        private TextBox TourCount;
        private TextBox textBox7;
    }
}