using JpCommon;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Installer
{
    public partial class RegisterLicense : Form
    {

        private bool shouldRequestLicense { get; set; }
        private JpApi api { get; set; }
        private bool wasLicenseRegistered { get; set; }
        public RegisterLicense()
        {
            InitializeComponent();
            this.shouldRequestLicense = true;
            this.btnSalir.Visible = false;
            this.label.Text = "";
            this.wasLicenseRegistered = false;
            api = new JpApi();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            string input = textBoxLicense.Text.Replace("-", ""); // Remove existing dashes from input

            if (input.Length > 25)
            {
                input = input.Substring(0, 25); // Truncate the input to 25 characters
            }
            StringBuilder formattedInput = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (i > 0 && i % 5 == 0)
                {
                    formattedInput.Append("-");
                }
                formattedInput.Append(input[i]);
            }
            textBoxLicense.Text = formattedInput.ToString();
            textBoxLicense.SelectionStart = textBoxLicense.Text.Length;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }


        private void button1_Click(object sender, EventArgs e)
        {

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += registerLicense!;
            worker.RunWorkerAsync();
        }


        private async void registerLicense(object sender, DoWorkEventArgs args)
        {
            this.btnRegistrarLicencia.Invoke((MethodInvoker)(() =>
            {
                this.btnRegistrarLicencia.Enabled = false;
            }));
            this.textBoxLicense.Invoke((MethodInvoker)(() =>
            {
                this.textBoxLicense.Enabled = false;
            }));
            try
            {

                MetadataModel metadataModel = new MetadataModel();

                if (metadataModel.hasLicense())
                {
                    metadataModel.removeLicense();
                }

                this.label.Invoke((MethodInvoker)(() =>
                {
                    this.label.Text = "Verificando licencia";
                }));


                using (HttpClient client = new HttpClient())
                {

                    var content = new
                    {
                        license = this.textBoxLicense.Text,
                        specs = SystemInfo.GetSystemInfo(),
                    };

                    var response = await api.post("/license", content);
                    response.EnsureSuccessStatusCode();

                    this.shouldRequestLicense = true;

                    this.label.Invoke((MethodInvoker)(() =>
                    {
                        this.label.Text = "Licencia Valida. Guardando...";
                    }));

                    try
                    {
                        metadataModel.insertLicense(this.textBoxLicense.Text.Replace("-", ""));
                    }
                    catch (Exception ex)
                    {
                        this.label.Invoke((MethodInvoker)(() =>
                        {
                            this.label.Text = "FGL: " + ex.Message;
                        }));
                    }


                    this.label.Invoke((MethodInvoker)(() =>
                    {
                        this.label.Text = "Se ha registrado la licencia de forma exitosa.";
                    }));

                    this.wasLicenseRegistered = true;

                    var jpServiceManager = new JpServiceManager();

                    jpServiceManager.StartService(JpConstants.UpdaterServiceName);
                    jpServiceManager.StartService(JpConstants.ClientServiceName);


                    this.cancelarBtn.Invoke((MethodInvoker)(() =>
                    {
                        this.cancelarBtn.Visible = false;
                    }));
                    this.btnRegistrarLicencia.Invoke((MethodInvoker)(() =>
                    {
                        this.btnRegistrarLicencia.Visible = true;
                    }));
                }

            }
            catch (Exception ex)
            {
                this.label.Invoke((MethodInvoker)(() =>
                {
                    this.label.Text = "Error. No se ha podido verificar la licencia. " + ex.Message;
                }));
                this.shouldRequestLicense = false;
            }
            finally
            {
                this.btnRegistrarLicencia.Invoke((MethodInvoker)(() =>
                {
                    this.btnRegistrarLicencia.Enabled = true;
                }));
                this.textBoxLicense.Invoke((MethodInvoker)(() =>
                {
                    this.textBoxLicense.Enabled = true;
                }));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void RegisterLicense_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                System.Windows.Forms.Application.Exit();
            }
        }

        private void RegisterLicense_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label_Click(object sender, EventArgs e)
        {

        }

        private void label_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // Verify if there is currently a license.

            var metadataModel = new MetadataModel(false);


            string licence = metadataModel.getLicense();
            if (licence!= null && licence.Length > 0)
            {
                this.textBoxLicense.Text = licence;
            }
            else
            {
                MessageBox.Show("Este equipo no cuenta con licencia.");
            }

        }
    }
}
