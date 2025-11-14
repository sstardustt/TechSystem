using System;
using System.Drawing;
using System.Windows.Forms;
using TechSystem.Data;

namespace TechSystem.Forms
{
    public class RegisterForm : Form
    {
        private Label lblTitulo;
        private TextBox txtNome, txtEmail, txtSenha, txtConfirmar;
        private Button btnCadastrar, btnVoltar;
        private PictureBox logoBox;

        public RegisterForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // Janela
            this.Text = "TechSystem - Registro";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(480, 650);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);

            // Logo
            logoBox = new PictureBox
            {
                Image = Properties.Resources.TechSystem2,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 160,
                Height = 160,
                Top = 40,
                Left = (this.ClientSize.Width - 160) / 2,
                Anchor = AnchorStyles.Top
            };
            this.Controls.Add(logoBox);

            // Título
            lblTitulo = new Label
            {
                Text = "Crie sua conta",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 70,
                Margin = new Padding(0, 10, 0, 20)
            };
            this.Controls.Add(lblTitulo);
            lblTitulo.BringToFront();

            // Campos
            txtNome = MakeTextBox("Nome completo", 230);
            txtEmail = MakeTextBox("E-mail", 280);
            txtSenha = MakeTextBox("Senha", 330, true);
            txtConfirmar = MakeTextBox("Senha", 380, true);

            // Botão Cadastrar
            btnCadastrar = new Button
            {
                Text = "Cadastrar",
                Width = 320,
                Height = 45,
                Top = 440,
                Left = (this.ClientSize.Width - 320) / 2,
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnCadastrar.FlatAppearance.BorderSize = 0;
            btnCadastrar.Click += BtnCadastrar_Click;
            this.Controls.Add(btnCadastrar);

            // Botão Voltar
            btnVoltar = new Button
            {
                Text = "Voltar ao Login",
                Width = 320,
                Height = 40,
                Top = 500,
                Left = (this.ClientSize.Width - 320) / 2,
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnVoltar.FlatAppearance.BorderSize = 0;
            btnVoltar.Click += (s, e) =>
            {
                new LoginForm().Show();
                this.Close();
            };
            this.Controls.Add(btnVoltar);
        }

        private TextBox MakeTextBox(string placeholder, int top, bool isPassword = false)
        {
            TextBox txt = new TextBox
            {
                Width = 320,
                Height = 35,
                Left = (this.ClientSize.Width - 320) / 2,
                Top = top,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                ForeColor = Color.Gray,
                Text = placeholder,
                UseSystemPasswordChar = isPassword
            };

            txt.GotFocus += (s, e) =>
            {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";
                    txt.ForeColor = Color.Black;
                }
            };

            txt.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.Text = placeholder;
                    txt.ForeColor = Color.Gray;
                }
            };

            this.Controls.Add(txt);
            return txt;
        }

        private void BtnCadastrar_Click(object sender, EventArgs e)
        {
            string nome = txtNome.Text.Trim();
            string email = txtEmail.Text.Trim();
            string senha = txtSenha.Text;
            string confirmar = txtConfirmar.Text;

            // Placeholder   vazio
            if (nome == "Nome completo") nome = "";
            if (email == "E-mail") email = "";
            if (senha == "Senha") senha = "";
            if (confirmar == "Senha") confirmar = "";

            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(confirmar))
            {
                MessageBox.Show("Preencha todos os campos.");
                return;
            }

            if (senha != confirmar)
            {
                MessageBox.Show("As senhas não conferem.");
                return;
            }
         
            string senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);

            if (Database.CreateUser(nome, email, senhaHash))
            {
                MessageBox.Show("✅ Cadastro realizado com sucesso!");
                new LoginForm().Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("⚠️ Erro: este e-mail já está cadastrado.");
            }
        }
    }
}
