using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using TechSystem.Data;


namespace TechSystem.Forms
{
    public class OpenTicketForm : UserControl
    {
        private readonly string connectionString;
        private readonly int usuarioId;

        private Label lblTitulo, lblDescricao, lblSugestao;
        private TextBox txtTitulo, txtDescricao, txtSugestaoIA;
        private Button btnGerarSugestao, btnAbrirChamado;

        private string idiomaAtual;
        private bool isDark;

        public OpenTicketForm(int userId)
        {
            usuarioId = userId;
            connectionString = ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString;

            LoadSettings();
            BuildUI();
            ApplyTheme();
            ApplyLanguage();


            AppEvents.OnSettingsChanged += HandleSettingsChanged;
            this.Disposed += (s, e) => AppEvents.OnSettingsChanged -= HandleSettingsChanged;
        }

        private void LoadSettings()
        {
            idiomaAtual = (Properties.Settings.Default["Language"] as string ?? "pt").ToLower();
            isDark = (Properties.Settings.Default["AppTheme"] as string ?? "light").ToLower() == "dark";
        }

        private void HandleSettingsChanged()
        {
            LoadSettings();
            ApplyLanguage();
            ApplyTheme();
        }

        private void ApplyLanguage()
        {
            if (idiomaAtual == "en")
            {
                lblTitulo.Text = "Ticket Title:";
                lblDescricao.Text = "Detailed Description:";
                lblSugestao.Text = "AI Detailed Analysis:";
                btnGerarSugestao.Text = "🔍 Generate AI Analysis";
                btnAbrirChamado.Text = "📨 Open Ticket";
            }
            else if (idiomaAtual == "es")
            {
                lblTitulo.Text = "Título del Ticket:";
                lblDescricao.Text = "Descripción Detallada:";
                lblSugestao.Text = "Análisis Detallado de la IA:";
                btnGerarSugestao.Text = "🔍 Generar Análisis de IA";
                btnAbrirChamado.Text = "📨 Abrir Ticket";
            }
            else
            {
                lblTitulo.Text = "Título do Chamado:";
                lblDescricao.Text = "Descrição Detalhada:";
                lblSugestao.Text = "Análise Detalhada da IA:";
                btnGerarSugestao.Text = "🔍 Gerar Análise da IA";
                btnAbrirChamado.Text = "📨 Abrir Chamado";
            }
        }

        private void ApplyTheme()
        {
            BackColor = isDark ? Color.FromArgb(25, 25, 25) : Color.WhiteSmoke;
            Color fore = isDark ? Color.WhiteSmoke : Color.FromArgb(40, 40, 40);
            Color inputBg = isDark ? Color.FromArgb(40, 40, 40) : Color.White;

            foreach (Control c in Controls)
                ApplyThemeRecursive(c, fore, inputBg);
        }

        private void ApplyThemeRecursive(Control c, Color fore, Color inputBg)
        {
            if (c is Label lbl)
                lbl.ForeColor = fore;
            else if (c is TextBox tb)
            {
                tb.BackColor = inputBg;
                tb.ForeColor = fore;
                tb.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (c is Button btn)
            {
                btn.BackColor = Color.FromArgb(0, 120, 215);
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderSize = 0;
            }

            foreach (Control child in c.Controls)
                ApplyThemeRecursive(child, fore, inputBg);
        }

        private void BuildUI()
        {
            Dock = DockStyle.Fill;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                AutoSize = true,
                Padding = new Padding(50, 80, 50, 40),
            };

            lblTitulo = new Label
            {
                Text = "Título do Chamado:",
                AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Margin = new Padding(3, 0, 3, 5)
            };

            txtTitulo = new TextBox
            {
                Width = 550,
                Height = 35,
                Font = new Font("Segoe UI", 10f),
                Margin = new Padding(0, 0, 0, 10)
            };

            lblDescricao = new Label
            {
                Text = "Descrição detalhada:",
                AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Margin = new Padding(3, 10, 3, 5)
            };

            txtDescricao = new TextBox
            {
                Width = 550,
                Height = 150,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10f),
                Margin = new Padding(0, 0, 0, 10)
            };

            btnGerarSugestao = new Button
            {
                Text = "🔍 Gerar Análise da IA",
                Width = 240,
                Height = 45,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 10, 0, 10)
            };
            btnGerarSugestao.Click += BtnGerarSugestao_Click;

            lblSugestao = new Label
            {
                Text = "Análise detalhada da IA:",
                AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Margin = new Padding(3, 10, 3, 5)
            };

            txtSugestaoIA = new TextBox
            {
                Width = 550,
                Height = 270,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(235, 240, 255),
                Margin = new Padding(0, 0, 0, 10)
            };

            btnAbrirChamado = new Button
            {
                Text = "📨 Abrir Chamado",
                Width = 240,
                Height = 45,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 20, 0, 0)
            };
            btnAbrirChamado.Click += BtnAbrirChamado_Click;

            layout.Controls.Add(lblTitulo);
            layout.Controls.Add(txtTitulo);
            layout.Controls.Add(lblDescricao);
            layout.Controls.Add(txtDescricao);
            layout.Controls.Add(btnGerarSugestao);
            layout.Controls.Add(lblSugestao);
            layout.Controls.Add(txtSugestaoIA);
            layout.Controls.Add(btnAbrirChamado);

            Controls.Add(layout);
        }

        private async void BtnGerarSugestao_Click(object sender, EventArgs e)
        {
            string titulo = txtTitulo.Text.Trim();
            string descricao = txtDescricao.Text.Trim();

            if (string.IsNullOrEmpty(titulo) || string.IsNullOrEmpty(descricao))
            {
                MessageBox.Show(GetText("Preencha título e descrição antes de gerar a sugestão da IA.",
                                        "Fill in the title and description before generating the AI suggestion.",
                                        "Complete el título y la descripción antes de generar la sugerencia de IA."));
                return;
            }

            try
            {
                string sugestaoIA = await AIHelper.CategorizarChamado(titulo, descricao);
                txtSugestaoIA.Text =
                    "📘 " + GetText("Análise da IA:", "AI Analysis:", "Análisis de IA:") + "\r\n\r\n" +
                    sugestaoIA.Replace("Categoria:", "Categoria:\r\n")
                              .Replace("Sugestão de solução:", "\r\n\r\nSugestão de solução:\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(GetText("Erro ao gerar sugestão da IA:\r\n", "Error generating AI suggestion:\r\n", "Error al generar sugerencia de IA:\r\n") + ex.Message);
            }
        }

        private async void BtnAbrirChamado_Click(object sender, EventArgs e)
        {
            string titulo = txtTitulo.Text.Trim();
            string descricao = txtDescricao.Text.Trim();
            string sugestaoIA = txtSugestaoIA.Text.Trim();

            if (string.IsNullOrEmpty(titulo) || string.IsNullOrEmpty(descricao))
            {
                MessageBox.Show(GetText("Preencha título e descrição para abrir o chamado.",
                                        "Fill in the title and description to open the ticket.",
                                        "Complete el título y la descripción para abrir el ticket."));
                return;
            }

            string prioridade = "Média";
            string lower = descricao.ToLower();
            if (lower.Contains("erro crítico") || lower.Contains("falha total") || lower.Contains("parado"))
                prioridade = "Alta";
            else if (lower.Contains("lento") || lower.Contains("demora") || lower.Contains("instável"))
                prioridade = "Média";
            else
                prioridade = "Baixa";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(@"
            INSERT INTO Chamados 
            (Titulo, Descricao, Prioridade, Status, Categoria, SugestaoIA, UsuarioID)
            VALUES (@Titulo, @Descricao, @Prioridade, 'Em Aberto', 'IA', @SugestaoIA, @UsuarioID)", conn))
                {
                    cmd.Parameters.Add("@Titulo", SqlDbType.NVarChar).Value = titulo;
                    cmd.Parameters.Add("@Descricao", SqlDbType.NVarChar).Value = descricao;
                    cmd.Parameters.Add("@Prioridade", SqlDbType.NVarChar).Value = prioridade;
                    cmd.Parameters.Add("@SugestaoIA", SqlDbType.NVarChar).Value = sugestaoIA;
                    cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }


                // Dispara o evento para atualizar o badge de notificações do Dashboard
                AppEvents.NotifyDashboardToUpdateNotifications();

                MessageBox.Show(GetText(
                    $"Chamado aberto com sucesso!\nPrioridade sugerida pela IA: {prioridade}",
                    $"Ticket successfully opened!\nPriority suggested by AI: {prioridade}",
                    $"¡Ticket abierto con éxito!\nPrioridad sugerida por la IA: {prioridade}"));

                txtTitulo.Clear();
                txtDescricao.Clear();
                txtSugestaoIA.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(GetText("Erro ao abrir chamado:\r\n", "Error opening ticket:\r\n", "Error al abrir el ticket:\r\n") + ex.Message);
            }
        }


        private string GetText(string pt, string en, string es)
        {
            if (idiomaAtual == "en") return en;
            if (idiomaAtual == "es") return es;
            return pt;
        }
    }
}
