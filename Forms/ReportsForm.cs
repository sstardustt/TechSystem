using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using TechSystem.Data;

namespace TechSystem.Forms
{
    public class ReportsControl : UserControl
    {
        private readonly int usuarioId;
        private readonly int roleId;
        private DataGridView gridChamados;
        private ComboBox filtroStatus, filtroPrioridade;
        private Button btnAtualizar;
        private Label lblTitulo;

        private string Lang => (Properties.Settings.Default["Language"] as string ?? "pt").ToLower();
        private string Theme => (Properties.Settings.Default["AppTheme"] as string ?? "light").ToLower();

        public ReportsControl(int userId)
        {
            usuarioId = userId;
            roleId = Database.GetUserRole(userId);
            InitializeComponent();
            ApplyLanguage();
            ApplyTheme();
            LoadChamados();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.WhiteSmoke;

            lblTitulo = new Label
            {
                Text = "📊 Relatórios de Chamados",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            this.Controls.Add(lblTitulo);

            Panel panelFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 10, 20, 10)
            };
            this.Controls.Add(panelFiltros);

            filtroStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 180
            };
            filtroStatus.Items.AddRange(new[] { "Todos", "Em Aberto", "Em Andamento", "Concluído", "Cancelado" });
            filtroStatus.SelectedIndex = 0;
            filtroStatus.Location = new Point(0, 10);
            panelFiltros.Controls.Add(filtroStatus);

            filtroPrioridade = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150
            };
            filtroPrioridade.Items.AddRange(new[] { "Todas", "Baixa", "Média", "Alta", "Crítica" });
            filtroPrioridade.SelectedIndex = 0;
            filtroPrioridade.Location = new Point(filtroStatus.Right + 20, 10);
            panelFiltros.Controls.Add(filtroPrioridade);

            btnAtualizar = new Button
            {
                Text = "🔄 Atualizar",
                AutoSize = true,
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(10, 5, 10, 5),
                Margin = new Padding(10, 0, 0, 0)
            };
            btnAtualizar.FlatAppearance.BorderSize = 0;
            btnAtualizar.Click += (s, e) => LoadChamados();
            btnAtualizar.Location = new Point(filtroPrioridade.Right + 20, 7);
            panelFiltros.Controls.Add(btnAtualizar);

            Panel panelGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 120, 20, 20)
            };
            this.Controls.Add(panelGrid);

            gridChamados = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            panelGrid.Controls.Add(gridChamados);

            gridChamados.CellValueChanged += GridChamados_CellValueChanged;
            gridChamados.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (gridChamados.IsCurrentCellDirty)
                    gridChamados.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
        }

        private void ApplyLanguage()
        {
            bool en = Lang == "en";
            bool es = Lang == "es";

            lblTitulo.Text = en ? "📊 Ticket Reports" : es ? "📊 Reportes de Tickets" : "📊 Relatórios de Chamados";
            btnAtualizar.Text = en ? "🔄 Update" : es ? "🔄 Actualizar" : "🔄 Atualizar";
        }

        private void ApplyTheme()
        {
            bool dark = Theme == "dark";
            this.BackColor = dark ? Color.FromArgb(25, 25, 25) : Color.WhiteSmoke;
            lblTitulo.ForeColor = dark ? Color.White : Color.Black;
        }

        private void LoadChamados()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Database.ConnectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    string query = @"SELECT c.Id, u.Nome AS Usuario, c.Titulo, c.Descricao, 
                                     c.Status, c.Prioridade, c.DataAbertura 
                                     FROM Chamados c 
                                     INNER JOIN Users u ON c.UsuarioId = u.Id ";

                    if (roleId == 1 || roleId == 2)
                    {
                        query += "WHERE 1=1 ";
                    }
                    else
                    {
                        query += "WHERE c.UsuarioId = @usuarioId ";
                        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                    }

                    if (filtroStatus.Text != "Todos")
                    {
                        query += "AND c.Status = @status ";
                        cmd.Parameters.AddWithValue("@status", filtroStatus.Text);
                    }

                    if (filtroPrioridade.Text != "Todas")
                    {
                        query += "AND c.Prioridade = @prioridade ";
                        cmd.Parameters.AddWithValue("@prioridade", filtroPrioridade.Text);
                    }

                    query += "ORDER BY c.DataAbertura DESC";
                    cmd.CommandText = query;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gridChamados.DataSource = dt;
                    if (dt.Columns.Count > 0)
                        FormatGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar chamados: " + ex.Message);
            }
        }

        private void FormatGrid()
        {
            if (gridChamados.Columns.Count == 0) return;

            foreach (DataGridViewColumn col in gridChamados.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                col.ReadOnly = true;
            }

            bool en = Lang == "en";
            bool es = Lang == "es";

            gridChamados.Columns["Id"].HeaderText = "ID";
            gridChamados.Columns["Usuario"].HeaderText = en ? "User" : es ? "Usuario" : "Usuário";
            gridChamados.Columns["Titulo"].HeaderText = en ? "Title" : es ? "Título" : "Título";
            gridChamados.Columns["Descricao"].HeaderText = en ? "Description" : es ? "Descripción" : "Descrição";
            gridChamados.Columns["Status"].HeaderText = en ? "Status" : es ? "Estado" : "Status";
            gridChamados.Columns["Prioridade"].HeaderText = en ? "Priority" : es ? "Prioridad" : "Prioridade";
            gridChamados.Columns["DataAbertura"].HeaderText = en ? "Open Date" : es ? "Fecha de Apertura" : "Data de Abertura";

            if (roleId == 1 || roleId == 2)
            {
                int colIndex = gridChamados.Columns["Status"].Index;
                if (!(gridChamados.Columns[colIndex] is DataGridViewComboBoxColumn))
                {
                    DataGridViewComboBoxColumn comboStatus = new DataGridViewComboBoxColumn
                    {
                        Name = "Status",
                        DataPropertyName = "Status",
                        HeaderText = gridChamados.Columns[colIndex].HeaderText,
                        DataSource = new[] { "Em Aberto", "Em Andamento", "Concluído", "Cancelado" },
                        FlatStyle = FlatStyle.Flat
                    };

                    gridChamados.Columns.RemoveAt(colIndex);
                    gridChamados.Columns.Insert(colIndex, comboStatus);
                }
            }
        }

        private void GridChamados_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!gridChamados.Columns.Contains("Status")) return;
            if (gridChamados.Columns[e.ColumnIndex].Name != "Status") return;

            if (roleId != 1 && roleId != 2)
            {
                MessageBox.Show(Lang == "en" ? "Only admins or technicians can change ticket status."
                                : Lang == "es" ? "Solo administradores o técnicos pueden cambiar el estado del ticket."
                                : "Apenas administradores ou técnicos podem alterar o status de chamados.");
                return;
            }

            if (gridChamados.Rows[e.RowIndex].Cells["Id"].Value == null) return;

            int id = Convert.ToInt32(gridChamados.Rows[e.RowIndex].Cells["Id"].Value);
            string novoStatus = gridChamados.Rows[e.RowIndex].Cells["Status"].Value?.ToString();
            if (string.IsNullOrEmpty(novoStatus)) return;

            try
            {
                int usuarioChamadoId = 0;

                using (SqlConnection conn = new SqlConnection(Database.ConnectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT UsuarioId FROM Chamados WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    usuarioChamadoId = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd.CommandText = "UPDATE Chamados SET Status = @s WHERE Id = @id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@s", novoStatus);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();

                    if (usuarioChamadoId != usuarioId)
                    {
                        cmd.CommandText = @"INSERT INTO Notifications (UserId, Titulo, Mensagem, Lida, DataCriacao) 
                        VALUES (@UserId, @Titulo, @Mensagem, 0, GETDATE())";
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@UserId", usuarioChamadoId);
                        cmd.Parameters.AddWithValue("@Titulo", $"Chamado #{id} atualizado");

                        string msgNotif = Lang == "en"
                            ? $"Your ticket #{id} status changed to '{novoStatus}'."
                            : Lang == "es"
                                ? $"El estado de tu ticket #{id} cambió a '{novoStatus}'."
                                : $"O status do seu chamado #{id} mudou para '{novoStatus}'";

                        cmd.Parameters.AddWithValue("@Mensagem", msgNotif);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(Lang == "en" ? $"Ticket #{id} updated to '{novoStatus}'."
                                  : Lang == "es" ? $"Ticket #{id} actualizado a '{novoStatus}'."
                                  : $"Chamado #{id} atualizado para '{novoStatus}'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar status: " + ex.Message);
            }
        }
    }
}
