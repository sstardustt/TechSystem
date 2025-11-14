using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using TechSystem.Data;

namespace TechSystem.Forms
{
    public class DashboardForm : Form
    {
        private readonly int usuarioId;
        private readonly string nomeUsuario;
        private readonly int roleId;

        private Panel painelLateral;
        private Panel painelConteudo;
        private Label lblHeader;
        private FlowLayoutPanel flowCards;
        private Chart chartStatus;
        private Button btnNotifs;

        private Timer refreshTimer;

        private string Lang { get { return (Properties.Settings.Default["Language"] as string ?? "pt").ToLower(); } }
        private string Theme { get { return (Properties.Settings.Default["AppTheme"] as string ?? "light").ToLower(); } }

        public DashboardForm(int userId, string nome, int role)
        {
            usuarioId = userId;
            nomeUsuario = nome;
            roleId = role;

            InitializeComponent();
            ApplyLanguage();
            ApplyTheme();
            LoadHomeView();

            // Eventos globais
            AppEvents.OnSettingsChanged += HandleAppSettingsChanged;
            AppEvents.OnNotificationChanged += UpdateNotificationBadge;

            this.Disposed += (s, e) => {
                AppEvents.OnSettingsChanged -= HandleAppSettingsChanged;
                AppEvents.OnNotificationChanged -= UpdateNotificationBadge;
            };

            // Timer para atualizar dashboard
            refreshTimer = new Timer { Interval = 10000 };
            refreshTimer.Tick += delegate
            {
                try
                {
                    if (flowCards != null && chartStatus != null && !flowCards.IsDisposed && !chartStatus.IsDisposed)
                    {
                        LoadSummaryAndChart();
                        UpdateNotificationBadge();
                    }
                }
                catch { }
            };
            refreshTimer.Start();
        }

        private void HandleAppSettingsChanged()
        {
            ApplyLanguage();
            ApplyTheme();
            LoadHomeView();
        }

        private void InitializeComponent()
        {
            this.Text = "TechSystem - Dashboard";
            this.WindowState = FormWindowState.Maximized;
            this.Font = new Font("Segoe UI", 10);

            // PAINEL DE CONTEÚDO
            painelConteudo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke,
                AutoScroll = false
            };
            this.Controls.Add(painelConteudo);

            lblHeader = new Label
            {
                Text = "Painel de Controle",
                Dock = DockStyle.Top,
                Height = 70,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            painelConteudo.Controls.Add(lblHeader);

            // PAINEL LATERAL
            painelLateral = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(70, 130, 180)
            };
            this.Controls.Add(painelLateral);

            // FlowLayoutPanel para botões
            FlowLayoutPanel panelBotoes = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(10, 20, 10, 10)
            };
            painelLateral.Controls.Add(panelBotoes);

            // Botões do menu lateral
            Button btnDashboard = MakeSideButton("🏠  Início", BtnHome_Click);
            Button btnAbrir = MakeSideButton("🎫  Abrir Chamado", BtnOpen_Click);
            Button btnReports = MakeSideButton("📊  Relatórios", BtnReports_Click);
            btnNotifs = MakeSideButton("🔔  Notificações", BtnNotifs_Click);
            Button btnSettings = MakeSideButton("⚙️  Configurações", BtnSettings_Click);
            Button btnLogout = MakeSideButton("🚪  Sair", BtnLogout_Click);

            panelBotoes.Controls.Add(btnDashboard);
            panelBotoes.Controls.Add(btnAbrir);
            panelBotoes.Controls.Add(btnReports);
            panelBotoes.Controls.Add(btnNotifs);
            panelBotoes.Controls.Add(btnSettings);
            panelBotoes.Controls.Add(btnLogout);

            foreach (Button b in panelBotoes.Controls.OfType<Button>())
            {
                b.Width = 220;
                b.Margin = new Padding(0, 10, 0, 10);
            }

            // Painel topo (logo + mensagem)
            Panel panelTopo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 180
            };
            painelLateral.Controls.Add(panelTopo);

            PictureBox pbLogo = new PictureBox
            {
                Image = Properties.Resources.TechSystem2,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(200, 120),
                Location = new Point((panelTopo.Width - 200) / 2, 10),
                Anchor = AnchorStyles.Top
            };
            panelTopo.Controls.Add(pbLogo);

            Label lblWelcome = new Label
            {
                Text = "💼  " + nomeUsuario,
                Dock = DockStyle.Bottom,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            panelTopo.Controls.Add(lblWelcome);
        }

        private Button MakeSideButton(string text, EventHandler click)
        {
            Button b = new Button
            {
                Text = text,
                Height = 55,
                Width = 220,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 90, 160),
                Font = new Font("Segoe UI", 11),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += click;

            // Hover
            Color normal = b.BackColor;
            Color hover = ControlPaint.Light(normal);
            b.MouseEnter += (s, e) => b.BackColor = hover;
            b.MouseLeave += (s, e) => b.BackColor = normal;

            return b;
        }

        private void ApplyTheme()
        {
            bool dark = Theme == "dark";

            this.BackColor = dark ? Color.FromArgb(25, 25, 25) : Color.WhiteSmoke;
            painelConteudo.BackColor = dark ? Color.FromArgb(30, 30, 30) : Color.WhiteSmoke;
            painelLateral.BackColor = dark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(245, 247, 250);
            lblHeader.ForeColor = dark ? Color.White : Color.FromArgb(30, 30, 30);

            foreach (Control c in painelLateral.Controls.OfType<FlowLayoutPanel>().SelectMany(f => f.Controls.OfType<Button>()))
            {
                Color normal = dark ? Color.FromArgb(60, 60, 60) : Color.FromArgb(60, 130, 180);
                Color hover = dark ? Color.FromArgb(80, 80, 80) : ControlPaint.Light(normal);

                c.BackColor = normal;
                c.ForeColor = Color.White;

                Button b = c as Button;
                if (b != null)
                {
                    b.MouseEnter += (s, e) => b.BackColor = hover;
                    b.MouseLeave += (s, e) => b.BackColor = normal;
                }
            }

            if (flowCards != null)
            {
                foreach (Panel p in flowCards.Controls.OfType<Panel>())
                {
                    foreach (Label lbl in p.Controls.OfType<Label>())
                        lbl.ForeColor = Color.White;
                }
            }

            if (chartStatus != null)
            {
                chartStatus.BackColor = dark ? Color.FromArgb(30, 30, 30) : Color.Transparent;
                chartStatus.ChartAreas[0].BackColor = dark ? Color.FromArgb(30, 30, 30) : Color.Transparent;
                chartStatus.Legends[0].ForeColor = dark ? Color.White : Color.Black;
            }
        }

        private void ApplyLanguage()
        {
            bool en = Lang == "en";
            bool es = Lang == "es";

            lblHeader.Text = en ? "Dashboard" : es ? "Panel de Control" : "Painel de Controle";

            foreach (Button btn in painelLateral.Controls.OfType<FlowLayoutPanel>().SelectMany(f => f.Controls.OfType<Button>()))
            {
                if (btn.Text.Contains("🏠")) btn.Text = en ? "🏠  Home" : es ? "🏠  Inicio" : "🏠  Início";
                else if (btn.Text.Contains("🎫")) btn.Text = en ? "🎫  Open Ticket" : es ? "🎫  Abrir Ticket" : "🎫  Abrir Chamado";
                else if (btn.Text.Contains("📊")) btn.Text = en ? "📊  Reports" : es ? "📊  Reportes" : "📊  Relatórios";
                else if (btn.Text.Contains("🔔")) btn.Text = en ? "🔔  Notifications" : es ? "🔔  Notificaciones" : "🔔  Notificações";
                else if (btn.Text.Contains("⚙️")) btn.Text = en ? "⚙️  Settings" : es ? "⚙️  Configuración" : "⚙️  Configurações";
                else if (btn.Text.Contains("🚪")) btn.Text = en ? "🚪  Logout" : es ? "🚪  Salir" : "🚪  Sair";
            }
        }

        private void LoadHomeView()
        {
            painelConteudo.Controls.Clear();
            painelConteudo.Controls.Add(lblHeader);

            flowCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 160,
                Padding = new Padding(20),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true
            };
            painelConteudo.Controls.Add(flowCards);

            chartStatus = new Chart
            {
                Dock = DockStyle.Top,
                Height = 400,
                Palette = ChartColorPalette.SeaGreen,
                BackColor = Color.Transparent
            };
            chartStatus.ChartAreas.Add(new ChartArea("ca"));
            chartStatus.Legends.Add(new Legend("legenda"));
            chartStatus.Legends["legenda"].Docking = Docking.Bottom;
            chartStatus.Legends["legenda"].Font = new Font("Segoe UI", 10);
            painelConteudo.Controls.Add(chartStatus);

            LoadSummaryAndChart();
            UpdateNotificationBadge(); // atualização imediata
        }

        private void LoadSummaryAndChart()
        {
            bool en = Lang == "en";
            bool es = Lang == "es";

            int abertos = GetCountByStatus("Em Aberto");
            int andamento = GetCountByStatus("Em Andamento");
            int concluidos = GetCountByStatus("Concluído");
            int total = abertos + andamento + concluidos;

            flowCards.Controls.Clear();

                flowCards.Controls.Add(MakeCard("📋 " + (en ? "Total" : es ? "Total" : "Total de Chamados"), total, Color.MediumPurple));
                flowCards.Controls.Add(MakeCard("🟠 " + (en ? "Open" : es ? "Abiertos" : "Abertos"), abertos, Color.SeaGreen));
                flowCards.Controls.Add(MakeCard("🔵 " + (en ? "In Progress" : es ? "En Progreso" : "Em Andamento"), andamento, Color.FromArgb(105, 203, 172)));
                flowCards.Controls.Add(MakeCard("🟢 " + (en ? "Closed" : es ? "Cerrados" : "Concluídos"), concluidos, Color.SteelBlue));

            chartStatus.Series.Clear();
            Series serie = new Series("Status")
            {
                ChartType = SeriesChartType.Doughnut,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                IsValueShownAsLabel = true,
                LabelForeColor = Color.White
            };

            serie.Points.AddXY(en ? "Open" : es ? "Abiertos" : "Abertos", abertos);
            serie.Points.AddXY(en ? "In Progress" : es ? "En Progreso" : "Em Andamento", andamento);
            serie.Points.AddXY(en ? "Closed" : es ? "Cerrados" : "Concluídos", concluidos);

            chartStatus.Series.Add(serie);
        }

        private Panel MakeCard(string title, int value, Color color)
        {
            Panel p = new Panel
            {
                Width = 230,
                Height = 120,
                BackColor = color,
                Margin = new Padding(15),
                Padding = new Padding(10),
                BorderStyle = BorderStyle.None
            };

            Label lblT = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Height = 35
            };

            Label lblV = new Label
            {
                Text = value.ToString(),
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            p.Controls.Add(lblV);
            p.Controls.Add(lblT);
            return p;
        }

        private int GetCountByStatus(string status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Database.ConnectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    if (roleId != 1 && roleId != 2)
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM Chamados WHERE Status = @s AND UsuarioId = @u";
                        cmd.Parameters.AddWithValue("@u", usuarioId);
                    }
                    else
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM Chamados WHERE Status = @s";
                    }

                    cmd.Parameters.AddWithValue("@s", status);
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 0; }
        }

        private void UpdateNotificationBadge()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Database.ConnectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    if (roleId == 1 || roleId == 2)
                        cmd.CommandText = "SELECT COUNT(*) FROM Notifications WHERE Lida = 0";
                    else
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM Notifications WHERE UserId = @u AND Lida = 0";
                        cmd.Parameters.AddWithValue("@u", usuarioId);
                    }

                    conn.Open();
                    int qtd = Convert.ToInt32(cmd.ExecuteScalar());

                    string baseText = Lang == "en" ? "🔔 Notifications" : Lang == "es" ? "🔔 Notificaciones" : "🔔 Notificações";
                    btnNotifs.Text = qtd > 0 ? baseText + " (" + qtd + ")" : baseText;
                }
            }
            catch { }
        }

        // ---------- Botões ----------
        private void BtnHome_Click(object sender, EventArgs e)
        {
            painelConteudo.Controls.Clear();
            painelConteudo.Controls.Add(lblHeader);
            LoadHomeView();
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            painelConteudo.Controls.Clear();

            Panel container = new Panel { Dock = DockStyle.Fill };
            painelConteudo.Controls.Add(container);

            OpenTicketForm open = new OpenTicketForm(usuarioId) { Dock = DockStyle.Fill };
            container.Controls.Add(open);

            painelConteudo.Controls.Add(lblHeader);
            lblHeader.BringToFront();
            open.BringToFront();
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            painelConteudo.Controls.Clear();
            painelConteudo.Controls.Add(lblHeader);
            ReportsControl reports = new ReportsControl(usuarioId) { Dock = DockStyle.Fill };
            painelConteudo.Controls.Add(reports);
            reports.BringToFront();
        }

        private void BtnNotifs_Click(object sender, EventArgs e)
        {
            painelConteudo.Controls.Clear();
            painelConteudo.Controls.Add(lblHeader);
            NotificationsControl nots = new NotificationsControl(usuarioId) { Dock = DockStyle.Fill };
            painelConteudo.Controls.Add(nots);
            nots.BringToFront();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            painelConteudo.Controls.Clear();
            painelConteudo.Controls.Add(lblHeader);
            SettingsControl settings = new SettingsControl(usuarioId) { Dock = DockStyle.Fill };
            painelConteudo.Controls.Add(settings);
            settings.BringToFront();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            LoginForm lf = new LoginForm();
            lf.Show();
            this.Close();
        }
    }
}
