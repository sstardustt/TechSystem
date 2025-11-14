using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BCrypt.Net;

namespace TechSystem.Forms
{
    public class ResetSenhaForm : Form
    {
        private readonly int userId;
        private readonly string connectionString;

        private TextBox txtAtual;
        private TextBox txtNova;
        private TextBox txtConfirmar;
        private Button btnSalvar;
        private Label lblAtual, lblNova, lblConfirmar;

        private string Lang { get { return (Properties.Settings.Default["Language"] as string ?? "pt").ToLower(); } }
        private string Theme { get { return (Properties.Settings.Default["AppTheme"] as string ?? "light").ToLower(); } }

        public ResetSenhaForm(int usuarioId)
        {
            userId = usuarioId;
            connectionString = ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString;
            InitializeComponent();
            ApplyLanguage();
            ApplyTheme();

            AppEvents.OnSettingsChanged += HandleAppSettingsChanged;
            this.Disposed += (s, e) => AppEvents.OnSettingsChanged -= HandleAppSettingsChanged;
        }

        private void HandleAppSettingsChanged()
        {
            ApplyLanguage();
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            Text = "Redefinir Senha";
            Width = 500;
            Height = 450;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            PictureBox pbLogo = new PictureBox
            {
                Image = Properties.Resources.TechSystem2,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(150, 100),
                Location = new Point((Width - 150) / 2, 20)
            };
            Controls.Add(pbLogo);

            int topOffset = pbLogo.Bottom + 20;

            lblAtual = new Label { Left = 80, Top = topOffset, Width = 320, Font = new Font("Segoe UI", 10) };
            Controls.Add(lblAtual);

            txtAtual = new TextBox { Left = 80, Top = lblAtual.Bottom + 5, Width = 320, Height = 30, UseSystemPasswordChar = true, Font = new Font("Segoe UI", 10) };
            Controls.Add(txtAtual);

            lblNova = new Label { Left = 80, Top = txtAtual.Bottom + 20, Width = 320, Font = new Font("Segoe UI", 10) };
            Controls.Add(lblNova);

            txtNova = new TextBox { Left = 80, Top = lblNova.Bottom + 5, Width = 320, Height = 30, UseSystemPasswordChar = true, Font = new Font("Segoe UI", 10) };
            Controls.Add(txtNova);

            lblConfirmar = new Label { Left = 80, Top = txtNova.Bottom + 20, Width = 320, Font = new Font("Segoe UI", 10) };
            Controls.Add(lblConfirmar);

            txtConfirmar = new TextBox { Left = 80, Top = lblConfirmar.Bottom + 5, Width = 320, Height = 30, UseSystemPasswordChar = true, Font = new Font("Segoe UI", 10) };
            Controls.Add(txtConfirmar);

            btnSalvar = new Button
            {
                Width = 200,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point((Width - 200) / 2, txtConfirmar.Bottom + 30),
                Cursor = Cursors.Hand
            };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += BtnSalvar_Click;
            Controls.Add(btnSalvar);
        }

        private void ApplyLanguage()
        {
            bool en = Lang == "en";
            bool es = Lang == "es";

            Text = en ? "Reset Password" : es ? "Restablecer Contraseña" : "Redefinir Senha";
            lblAtual.Text = en ? "Current Password" : es ? "Contraseña Actual" : "Senha Atual";
            lblNova.Text = en ? "Enter New Password" : es ? "Ingrese Nueva Contraseña" : "Digite Nova Senha";
            lblConfirmar.Text = en ? "Confirm New Password" : es ? "Confirmar Nueva Contraseña" : "Confirme Nova Senha";
            btnSalvar.Text = en ? "Save Changes" : es ? "Guardar Cambios" : "Salvar Alterações";
        }

        private void ApplyTheme()
        {
            bool dark = Theme == "dark";
            BackColor = dark ? Color.FromArgb(25, 25, 25) : Color.White;

            foreach (Control c in Controls)
            {
                switch (c)
                {
                    case Label lbl: lbl.ForeColor = dark ? Color.White : Color.Black; break;
                    case TextBox tb:
                        tb.BackColor = dark ? Color.FromArgb(50, 50, 50) : Color.White;
                        tb.ForeColor = dark ? Color.White : Color.Black;
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case Button btn:
                        btn.BackColor = dark ? Color.FromArgb(70, 70, 70) : Color.FromArgb(50, 120, 220);
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.BorderColor = btn.BackColor;
                        break;
                }
            }
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            string atual = txtAtual.Text.Trim();
            string nova = txtNova.Text.Trim();
            string confirmar = txtConfirmar.Text.Trim();

            bool en = Lang == "en";
            bool es = Lang == "es";

            string msgFill = en ? "Fill in all fields!" : es ? "¡Complete todos los campos!" : "Preencha todos os campos!";
            string msgMatch = en ? "New passwords do not match!" : es ? "¡Las nuevas contraseñas no coinciden!" : "As novas senhas não coincidem!";
            string msgUser = en ? "User not found." : es ? "Usuario no encontrado." : "Usuário não encontrado.";
            string msgWrong = en ? "Current password is incorrect." : es ? "Contraseña actual incorrecta." : "Senha atual incorreta.";
            string msgSuccess = en ? "Password reset successfully!" : es ? "¡Contraseña restablecida correctamente!" : "Senha redefinida com sucesso!";

            if (string.IsNullOrWhiteSpace(atual) || string.IsNullOrWhiteSpace(nova) || string.IsNullOrWhiteSpace(confirmar))
            {
                MessageBox.Show(msgFill, en ? "Warning" : es ? "Aviso" : "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (nova != confirmar)
            {
                MessageBox.Show(msgMatch, en ? "Error" : es ? "Error" : "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmdCheck = new SqlCommand("SELECT Senha FROM Users WHERE Id = @id", conn);
                cmdCheck.Parameters.AddWithValue("@id", userId);
                var senhaHashAtual = cmdCheck.ExecuteScalar()?.ToString();

                if (senhaHashAtual == null)
                {
                    MessageBox.Show(msgUser, en ? "Error" : es ? "Error" : "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!BCrypt.Net.BCrypt.Verify(atual, senhaHashAtual))
                {
                    MessageBox.Show(msgWrong, en ? "Error" : es ? "Error" : "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(nova);

                SqlCommand cmdUpdate = new SqlCommand("UPDATE Users SET Senha = @nova WHERE Id = @id", conn);
                cmdUpdate.Parameters.AddWithValue("@nova", novaSenhaHash);
                cmdUpdate.Parameters.AddWithValue("@id", userId);
                cmdUpdate.ExecuteNonQuery();

                MessageBox.Show(msgSuccess, en ? "Success" : es ? "Éxito" : "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
        }
    }
}
