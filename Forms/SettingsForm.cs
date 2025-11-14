using System;
using System.Drawing;
using System.Windows.Forms;

namespace TechSystem.Forms
{
    public class SettingsControl : UserControl
    {
        private readonly int usuarioId;
        private bool isDarkMode;
        private string idiomaAtual;

        private Label lblTema;
        private Button btnTema;
        private Label lblIdioma;
        private ComboBox cmbIdioma;
        private Button btnSalvar;
        private Button btnRedefinirSenha;

        public SettingsControl(int userId)
        {
            usuarioId = userId;


            AppEvents.OnSettingsChanged += OnAppSettingsChanged;
            this.Disposed += (s, e) => AppEvents.OnSettingsChanged -= OnAppSettingsChanged;

            LoadSettings();
            ConfigureLayout();
            ApplyTheme();
            ApplyLanguage();
        }

        private void OnAppSettingsChanged()
        {
            if (IsHandleCreated && !IsDisposed)
            {
                BeginInvoke((Action)(() =>
                {
                    LoadSettings();
                    ApplyTheme();
                    ApplyLanguage();
                }));
            }
        }

        private void LoadSettings()
        {
            try
            {
                isDarkMode = (Properties.Settings.Default.AppTheme == "dark");
            }
            catch
            {
                isDarkMode = false;
            }

            try
            {
                idiomaAtual = Properties.Settings.Default.Language ?? "pt";
            }
            catch
            {
                idiomaAtual = "pt";
            }
        }

        private void ConfigureLayout()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.WhiteSmoke;

            lblTema = new Label
            {
                Text = "Tema:",
                Left = 40,
                Top = 40,
                AutoSize = true
            };

            btnTema = new Button
            {
                Text = isDarkMode ? "Modo Claro" : "Modo Escuro",
                Left = 140,
                Top = 34,
                Width = 200,
                Height = 36
            };
            btnTema.Click += BtnTema_Click;

            lblIdioma = new Label
            {
                Text = "Idioma:",
                Left = 40,
                Top = 100,
                AutoSize = true
            };

            cmbIdioma = new ComboBox
            {
                Left = 140,
                Top = 96,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbIdioma.Items.AddRange(new string[] { "Português", "English", "Español" });

            if (idiomaAtual == "en")
                cmbIdioma.SelectedItem = "English";
            else if (idiomaAtual == "es")
                cmbIdioma.SelectedItem = "Español";
            else
                cmbIdioma.SelectedItem = "Português";

            btnRedefinirSenha = new Button
            {
                Text = "Redefinir Senha",
                Left = 140,
                Top = 160,
                Width = 200,
                Height = 38
            };
            btnRedefinirSenha.Click += BtnRedefinirSenha_Click;

            btnSalvar = new Button
            {
                Text = "Salvar",
                Left = 140,
                Top = 230,
                Width = 200,
                Height = 36
            };
            btnSalvar.Click += BtnSalvar_Click;

            Controls.Add(lblTema);
            Controls.Add(btnTema);
            Controls.Add(lblIdioma);
            Controls.Add(cmbIdioma);
            Controls.Add(btnRedefinirSenha);
            Controls.Add(btnSalvar);
            BringToFront();
            btnTema.BringToFront();

        }

        private void BtnTema_Click(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            btnTema.Text = isDarkMode ?
                (idiomaAtual == "en" ? "Light Mode" :
                 idiomaAtual == "es" ? "Modo Claro" : "Modo Claro")
                : (idiomaAtual == "en" ? "Dark Mode" :
                 idiomaAtual == "es" ? "Modo Oscuro" : "Modo Escuro");
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            bool dark = isDarkMode;
            BackColor = dark ? Color.FromArgb(30, 30, 30) : Color.WhiteSmoke;
            ForeColor = dark ? Color.WhiteSmoke : Color.Black;

            foreach (Control c in Controls)
            {
                if (c is Button b)
                {
                    b.BackColor = dark ? Color.FromArgb(70, 70, 70) : Color.FromArgb(50, 120, 220);
                    b.ForeColor = Color.White;
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 0;
                }
                else if (c is Label)
                {
                    c.ForeColor = ForeColor;
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = dark ? Color.FromArgb(60, 60, 60) : Color.White;
                    cb.ForeColor = ForeColor;
                }
            }
        }

        private void ApplyLanguage()
        {
            if (idiomaAtual == "en")
            {
                lblTema.Text = "Theme:";
                lblIdioma.Text = "Language:";
                btnTema.Text = isDarkMode ? "Light Mode" : "Dark Mode";
                btnRedefinirSenha.Text = "Reset Password";
                btnSalvar.Text = "Save";
            }
            else if (idiomaAtual == "es")
            {
                lblTema.Text = "Tema:";
                lblIdioma.Text = "Idioma:";
                btnTema.Text = isDarkMode ? "Modo Claro" : "Modo Oscuro";
                btnRedefinirSenha.Text = "Restablecer Contraseña";
                btnSalvar.Text = "Guardar";
            }
            else
            {
                lblTema.Text = "Tema:";
                lblIdioma.Text = "Idioma:";
                btnTema.Text = isDarkMode ? "Modo Claro" : "Modo Escuro";
                btnRedefinirSenha.Text = "Redefinir Senha";
                btnSalvar.Text = "Salvar";
            }
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            string selected = cmbIdioma.SelectedItem != null ? cmbIdioma.SelectedItem.ToString() : "Português";
            if (selected == "English") idiomaAtual = "en";
            else if (selected == "Español") idiomaAtual = "es";
            else idiomaAtual = "pt";

            Properties.Settings.Default.AppTheme = isDarkMode ? "dark" : "light";
            Properties.Settings.Default.Language = idiomaAtual;
            Properties.Settings.Default.Save();

            ApplyLanguage();


            AppEvents.RaiseSettingsChanged();

            MessageBox.Show(
                idiomaAtual == "en" ? "Settings saved." :
                idiomaAtual == "es" ? "Configuración guardada." :
                "Configurações salvas."
            );
        }

        private void BtnRedefinirSenha_Click(object sender, EventArgs e)
        {
            ResetSenhaForm reset = new ResetSenhaForm(usuarioId);
            reset.ShowDialog();
        }
    }
}
