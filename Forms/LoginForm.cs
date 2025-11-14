using BCrypt.Net;
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using TechSystem.Data;

namespace TechSystem.Forms
{
    public class LoginForm : Form
    {
        private PictureBox pbLogo;
        private Label lblEmail;
        private Label lblSenha;
        private TextBox txtEmail;
        private TextBox txtSenha;
        private Button btnLogin;
        private Button btnRegistrar;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "TechSystem - Login";
            Size = new Size(450, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.WhiteSmoke;

            pbLogo = new PictureBox
            {
                Image = Properties.Resources.TechSystem2,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(125, 30),
                Size = new Size(200, 120)
            };

            lblEmail = new Label
            {
                Text = "Email:",
                ForeColor = Color.Black,
                Location = new Point(60, 180),
                AutoSize = true
            };

            txtEmail = new TextBox
            {
                Location = new Point(60, 210),
                Width = 320
            };

            lblSenha = new Label
            {
                Text = "Senha:",
                ForeColor = Color.Black,
                Location = new Point(60, 250),
                AutoSize = true
            };

            txtSenha = new TextBox
            {
                Location = new Point(60, 280),
                Width = 320,
                UseSystemPasswordChar = true
            };

            btnLogin = new Button
            {
                Text = "Entrar",
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(60, 330),
                Width = 320,
                Height = 40
            };
            btnLogin.Click += BtnLogin_Click;

            btnRegistrar = new Button
            {
                Text = "Registrar novo usuário",
                ForeColor = Color.FromArgb(70, 130, 180),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Underline),
                Location = new Point((ClientSize.Width - 150) / 2, 390), // centralizado
                Width = 150,
                BackColor = Color.Transparent
            };
            btnRegistrar.Click += BtnRegistrar_Click;

            Controls.Add(pbLogo);
            Controls.Add(lblEmail);
            Controls.Add(txtEmail);
            Controls.Add(lblSenha);
            Controls.Add(txtSenha);
            Controls.Add(btnLogin);
            Controls.Add(btnRegistrar);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string senha = txtSenha.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                MessageBox.Show("Preencha todos os campos.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(Database.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, Nome, Senha, RoleId FROM Users WHERE Email = @Email", conn);
                cmd.Parameters.AddWithValue("@Email", email);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string hash = reader["Senha"].ToString();
                        if (BCrypt.Net.BCrypt.Verify(senha, hash))
                        {
                            int userId = Convert.ToInt32(reader["Id"]);
                            string nome = reader["Nome"].ToString();
                            int roleId = Convert.ToInt32(reader["RoleId"]);

                            var dashboard = new DashboardForm(userId, nome, roleId);
                            dashboard.Show();
                            Hide();
                        }
                        else
                        {
                            MessageBox.Show("Senha incorreta.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Usuário não encontrado.");
                    }
                }
            }
        }

        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            var registerForm = new RegisterForm();
            registerForm.FormClosed += (s, args) => Show();
            registerForm.Show();
            Hide();
        }
    }
}
