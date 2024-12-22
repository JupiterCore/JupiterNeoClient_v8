namespace Installer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            panel1 = new Panel();
            btnFullRemove = new Button();
            button1 = new Button();
            btnRemove = new Button();
            btnSalir = new Button();
            progressBar = new ProgressBar();
            cancelarBtn = new Button();
            label2 = new Label();
            installBtn = new Button();
            labelInfo = new Label();
            pictureBox1 = new PictureBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ControlLightLight;
            panel1.Controls.Add(btnFullRemove);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(btnRemove);
            panel1.Controls.Add(btnSalir);
            panel1.Controls.Add(progressBar);
            panel1.Controls.Add(cancelarBtn);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(installBtn);
            panel1.Controls.Add(labelInfo);
            panel1.Controls.Add(pictureBox1);
            panel1.Location = new Point(-3, -1);
            panel1.Margin = new Padding(3, 2, 3, 2);
            panel1.Name = "panel1";
            panel1.Size = new Size(438, 509);
            panel1.TabIndex = 0;
            // 
            // btnFullRemove
            // 
            btnFullRemove.BackColor = Color.Gray;
            btnFullRemove.ForeColor = Color.Snow;
            btnFullRemove.Location = new Point(44, 296);
            btnFullRemove.Margin = new Padding(3, 2, 3, 2);
            btnFullRemove.Name = "btnFullRemove";
            btnFullRemove.Size = new Size(348, 34);
            btnFullRemove.TabIndex = 12;
            btnFullRemove.Text = "Remover Totalmente";
            btnFullRemove.UseVisualStyleBackColor = false;
            btnFullRemove.Click += btnFullRemove_Click;
            // 
            // button1
            // 
            button1.Location = new Point(44, 440);
            button1.Margin = new Padding(3, 2, 3, 2);
            button1.Name = "button1";
            button1.Size = new Size(348, 28);
            button1.TabIndex = 11;
            button1.Text = "Licencia";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // btnRemove
            // 
            btnRemove.Location = new Point(44, 407);
            btnRemove.Margin = new Padding(3, 2, 3, 2);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(348, 28);
            btnRemove.TabIndex = 10;
            btnRemove.Text = "Desinstalar";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // btnSalir
            // 
            btnSalir.Location = new Point(226, 474);
            btnSalir.Margin = new Padding(3, 2, 3, 2);
            btnSalir.Name = "btnSalir";
            btnSalir.Size = new Size(166, 28);
            btnSalir.TabIndex = 9;
            btnSalir.Text = "Salir";
            btnSalir.UseVisualStyleBackColor = true;
            btnSalir.Click += btnSalir_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(44, 338);
            progressBar.Margin = new Padding(3, 2, 3, 2);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(348, 22);
            progressBar.Step = 5;
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 8;
            progressBar.Visible = false;
            progressBar.Click += progressBar_Click;
            // 
            // cancelarBtn
            // 
            cancelarBtn.Location = new Point(44, 473);
            cancelarBtn.Margin = new Padding(3, 2, 3, 2);
            cancelarBtn.Name = "cancelarBtn";
            cancelarBtn.Size = new Size(177, 28);
            cancelarBtn.TabIndex = 7;
            cancelarBtn.Text = "Cancelar";
            cancelarBtn.UseVisualStyleBackColor = true;
            cancelarBtn.Click += button2_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F);
            label2.Location = new Point(190, 203);
            label2.Name = "label2";
            label2.Size = new Size(0, 21);
            label2.TabIndex = 6;
            // 
            // installBtn
            // 
            installBtn.Location = new Point(44, 374);
            installBtn.Margin = new Padding(3, 2, 3, 2);
            installBtn.Name = "installBtn";
            installBtn.Size = new Size(348, 28);
            installBtn.TabIndex = 5;
            installBtn.Text = "Instalar";
            installBtn.UseVisualStyleBackColor = true;
            installBtn.Click += button1_Click;
            // 
            // labelInfo
            // 
            labelInfo.Font = new Font("Arial", 12F, FontStyle.Bold);
            labelInfo.Location = new Point(3, 226);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(435, 103);
            labelInfo.TabIndex = 4;
            labelInfo.Text = "¡Bienvenido al instalador de Jupiter NEO!";
            labelInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            pictureBox1.ErrorImage = (Image)resources.GetObject("pictureBox1.ErrorImage");
            pictureBox1.InitialImage = Properties.Resources.Captura_de_pantalla_2024_12_21_195509;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Margin = new Padding(3, 2, 3, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(438, 225);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(435, 511);
            Controls.Add(panel1);
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            Text = "visu";
            KeyDown += Form1_KeyDown;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private PictureBox pictureBox1;
        private Label labelInfo;
        private Button cancelarBtn;
        private Label label2;
        private Button installBtn;
        private ProgressBar progressBar;
        private Button btnSalir;
        private Button btnRemove;
        private Button button1;
        private Button btnFullRemove;
    }



}