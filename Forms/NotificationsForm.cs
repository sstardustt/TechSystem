using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TechSystem.Data;
using TechSystem.Models;

namespace TechSystem.Forms
{
    public class NotificationsControl : UserControl
    {
        private readonly int _userId;
        private readonly FlowLayoutPanel _flowPanel;
        private readonly Label _header;
        private readonly Timer _refreshTimer;
        private string idiomaAtual;
        private bool isDark;

        public NotificationsControl(int userId)
        {
            _userId = userId;
            Dock = DockStyle.Fill;

            LoadSettings();

            _header = new Label
            {
                Text = GetHeaderText(),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 15, 0, 0),
                ForeColor = isDark ? Color.WhiteSmoke : Color.FromArgb(40, 40, 40)
            };

            _flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(15),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            Controls.Add(_flowPanel);
            Controls.Add(_header);

            ApplyTheme();

            _refreshTimer = new Timer { Interval = 30000 }; // 30s
            _refreshTimer.Tick += (s, e) => CarregarNotificacoes();

            Load += (s, e) =>
            {
                CarregarNotificacoes();
                _refreshTimer.Start();
            };

            AppEvents.OnSettingsChanged += HandleSettingsChanged;
            Disposed += (s, e) =>
            {
                _refreshTimer?.Stop();
                AppEvents.OnSettingsChanged -= HandleSettingsChanged;
            };
        }

        private void LoadSettings()
        {
            idiomaAtual = (Properties.Settings.Default["Language"] as string ?? "pt").ToLower();
            isDark = (Properties.Settings.Default["AppTheme"] as string ?? "light").ToLower() == "dark";
        }

        private void HandleSettingsChanged()
        {
            if (IsHandleCreated && !IsDisposed)
            {
                BeginInvoke((Action)(() =>
                {
                    LoadSettings();
                    _header.Text = GetHeaderText();
                    ApplyTheme();
                    CarregarNotificacoes();
                }));
            }
        }

        private string GetHeaderText()
        {
            if (idiomaAtual == "en") return "🔔 My Notifications";
            if (idiomaAtual == "es") return "🔔 Mis Notificaciones";
            return "🔔 Minhas Notificações";
        }

        private string GetEmptyMessage()
        {
            if (idiomaAtual == "en") return "No notifications yet 👀";
            if (idiomaAtual == "es") return "No hay notificaciones por aquí 👀";
            return "Nenhuma notificação por aqui 👀";
        }

        private void ApplyTheme()
        {
            BackColor = isDark ? Color.FromArgb(25, 25, 25) : Color.FromArgb(245, 245, 245);
            _header.ForeColor = isDark ? Color.WhiteSmoke : Color.FromArgb(40, 40, 40);
        }

        private void CarregarNotificacoes()
        {
            _flowPanel.Controls.Clear();
            int roleId = GetRoleId();
            List<Notification> notificacoes = Database.GetNotifications(_userId, roleId);

            if (notificacoes == null || notificacoes.Count == 0)
            {
                _flowPanel.Controls.Add(new Label
                {
                    Text = GetEmptyMessage(),
                    Font = new Font("Segoe UI", 12, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Margin = new Padding(10)
                });
                return;
            }

            foreach (var n in notificacoes)
                _flowPanel.Controls.Add(CriarCardNotificacao(n));

            ApplyTheme();
        }

        private Panel CriarCardNotificacao(Notification n)
        {
            Color corFundo = n.Lida
                ? (isDark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(235, 235, 235))
                : (isDark ? Color.FromArgb(70, 70, 70) : Color.White);

            var card = new Panel
            {
                BackColor = corFundo,
                Width = 500,
                Height = 150,
                Margin = new Padding(5, 8, 5, 8),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTitulo = new Label
            {
                Text = TraduzirTitulo(n.Titulo),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true,
                ForeColor = isDark ? Color.White : Color.Black
            };

            var lblMensagem = new Label
            {
                Text = TraduzirMensagem(n.Mensagem),
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 35),
                Size = new Size(card.Width - 30, 60),
                AutoSize = false,
                ForeColor = isDark ? Color.Gainsboro : Color.FromArgb(40, 40, 40)
            };

            var lblData = new Label
            {
                Text = n.DataCriacao.ToString("dd/MM/yyyy HH:mm"),
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(card.Width - 150, 10),
                AutoSize = true
            };

            var btnLida = new Button
            {
                Text = n.Lida ? GetReadText() : GetMarkAsReadText(),
                Font = new Font("Segoe UI", 9),
                BackColor = n.Lida ? Color.LightGray : Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(10, 100),
                Size = new Size(card.Width - 20, 35)
            };
            btnLida.FlatAppearance.BorderSize = 0;

            btnLida.Click += (s, e) =>
            {
                if (!n.Lida)
                {
                    Database.MarkNotificationAsRead(n.Id);
                    CarregarNotificacoes();
                }
            };

            card.Controls.Add(lblTitulo);
            card.Controls.Add(lblMensagem);
            card.Controls.Add(lblData);
            card.Controls.Add(btnLida);

            return card;
        }

        private string GetMarkAsReadText()
        {
            if (idiomaAtual == "en") return "Mark as read";
            if (idiomaAtual == "es") return "Marcar como leída";
            return "Marcar como lida";
        }

        private string GetReadText()
        {
            if (idiomaAtual == "en") return "Read";
            if (idiomaAtual == "es") return "Leída";
            return "Lida";
        }

        private int GetRoleId()
        {
            try { return Database.GetUserRole(_userId); }
            catch { return 3; }
        }

        private string TraduzirTitulo(string tituloOriginal)
        {
            if (string.IsNullOrEmpty(tituloOriginal))
                return tituloOriginal;

            string titulo = tituloOriginal.ToLower();

            if (idiomaAtual == "en")
            {
                if (titulo.Contains("chamado"))
                    titulo = titulo.Replace("chamado", "Ticket");
                if (titulo.Contains("atualizado"))
                    titulo = titulo.Replace("atualizado", "updated");
            }
            else if (idiomaAtual == "es")
            {
                if (titulo.Contains("chamado"))
                    titulo = titulo.Replace("chamado", "Ticket");
                if (titulo.Contains("atualizado"))
                    titulo = titulo.Replace("atualizado", "actualizado");
            }
            else
            {
                if (titulo.Contains("ticket"))
                    titulo = titulo.Replace("ticket", "Chamado");
                if (titulo.Contains("updated"))
                    titulo = titulo.Replace("updated", "atualizado");
                if (titulo.Contains("actualizado"))
                    titulo = titulo.Replace("actualizado", "atualizado");
            }

            return char.ToUpper(titulo[0]) + titulo.Substring(1);
        }

        private string TraduzirMensagem(string mensagemOriginal)
        {
            if (string.IsNullOrEmpty(mensagemOriginal))
                return mensagemOriginal;

            string msg = mensagemOriginal.Trim();

            // Normaliza tudo pra minúsculo antes de traduzir, mas mantém a primeira letra maiúscula no retorno
            string lower = msg.ToLower();

            if (idiomaAtual == "en")
            {
                lower = lower
                    .Replace("o status do seu chamado", "Your ticket")
                    .Replace("foi atualizado para", "was updated to")
                    .Replace("mudou para", "changed to");
            }
            else if (idiomaAtual == "es")
            {
                lower = lower
                    .Replace("o status do seu chamado", "El estado de tu ticket")
                    .Replace("foi atualizado para", "cambió a")
                    .Replace("mudou para", "cambió a");
            }
            else
            {
                // volta pro português se estiver vindo traduzido
                lower = lower
                    .Replace("your ticket", "O status do seu chamado")
                    .Replace("was updated to", "foi atualizado para")
                    .Replace("changed to", "foi atualizado para")
                    .Replace("el estado de tu ticket", "O status do seu chamado")
                    .Replace("cambió a", "foi atualizado para");
            }

            // Garante a primeira letra maiúscula
            return char.ToUpper(lower[0]) + lower.Substring(1);
        }

    }
}
