namespace Installer
{
    partial class RegisterLicense
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegisterLicense));
            panel1 = new Panel();
            button1 = new Button();
            pictureBox1 = new PictureBox();
            btnSalir = new Button();
            label = new Label();
            cancelarBtn = new Button();
            btnRegistrarLicencia = new Button();
            label1 = new Label();
            textBoxLicense = new TextBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ControlLightLight;
            panel1.Controls.Add(button1);
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(btnSalir);
            panel1.Controls.Add(label);
            panel1.Controls.Add(cancelarBtn);
            panel1.Controls.Add(btnRegistrarLicencia);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(textBoxLicense);
            panel1.Location = new Point(-6, -2);
            panel1.Margin = new Padding(4);
            panel1.Name = "panel1";
            panel1.Size = new Size(599, 611);
            panel1.TabIndex = 0;
            panel1.Paint += panel1_Paint;
            // 
            // button1
            // 
            button1.Location = new Point(20, 482);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(559, 36);
            button1.TabIndex = 9;
            button1.Text = "Verificar Licencia";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(161, 0);
            pictureBox1.Margin = new Padding(4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(292, 152);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 8;
            pictureBox1.TabStop = false;
            // 
            // btnSalir
            // 
            btnSalir.Location = new Point(24, 541);
            btnSalir.Margin = new Padding(4);
            btnSalir.Name = "btnSalir";
            btnSalir.Size = new Size(559, 45);
            btnSalir.TabIndex = 7;
            btnSalir.Text = "Salir";
            btnSalir.UseVisualStyleBackColor = true;
            btnSalir.Click += button3_Click;
            // 
            // label
            // 
            label.Location = new Point(20, 321);
            label.Margin = new Padding(4, 0, 4, 0);
            label.Name = "label";
            label.Size = new Size(559, 59);
            label.TabIndex = 6;
            label.Text = "Info";
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Click += label_Click_1;
            // 
            // cancelarBtn
            // 
            cancelarBtn.Location = new Point(19, 419);
            cancelarBtn.Margin = new Padding(4);
            cancelarBtn.Name = "cancelarBtn";
            cancelarBtn.Size = new Size(258, 36);
            cancelarBtn.TabIndex = 5;
            cancelarBtn.Text = "Cancelar";
            cancelarBtn.UseVisualStyleBackColor = true;
            cancelarBtn.Click += button2_Click;
            // 
            // btnRegistrarLicencia
            // 
            btnRegistrarLicencia.Location = new Point(305, 419);
            btnRegistrarLicencia.Margin = new Padding(4);
            btnRegistrarLicencia.Name = "btnRegistrarLicencia";
            btnRegistrarLicencia.Size = new Size(274, 36);
            btnRegistrarLicencia.TabIndex = 4;
            btnRegistrarLicencia.Text = "Registrar Licencia";
            btnRegistrarLicencia.UseVisualStyleBackColor = true;
            btnRegistrarLicencia.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial", 10.2F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(189, 156);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(234, 24);
            label1.TabIndex = 3;
            label1.Text = "REGISTRAR LICENCIA";
            label1.Click += label1_Click;
            // 
            // textBoxLicense
            // 
            textBoxLicense.BackColor = SystemColors.Window;
            textBoxLicense.Location = new Point(21, 240);
            textBoxLicense.Margin = new Padding(4);
            textBoxLicense.Multiline = true;
            textBoxLicense.Name = "textBoxLicense";
            textBoxLicense.Size = new Size(558, 56);
            textBoxLicense.TabIndex = 2;
            textBoxLicense.TextChanged += textBox1_TextChanged_1;
            // 
            // RegisterLicense
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(590, 608);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            Name = "RegisterLicense";
            Text = "Registrar Licencia";
            FormClosing += RegisterLicense_FormClosing;
            Load += RegisterLicense_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button cancelarBtn;
        private Button btnRegistrarLicencia;
        private Label label1;
        private TextBox textBoxLicense;
        private Label label;
        private Button btnSalir;
        public PictureBox pictureBox1;
        private Button button1;
    }
}