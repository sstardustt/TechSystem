using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using TechSystem.Models;

namespace TechSystem.Data
{
    public static class Database
    {
        public static readonly string ConnectionString;

        static Database()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString;
        }

        public static bool CreateUser(string nome, string email, string senhaHash)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    INSERT INTO Users (Nome, Email, Senha, RoleId)
                    VALUES (@Nome, @Email, @Senha, @RoleId)", con);

                cmd.Parameters.AddWithValue("@Nome", nome);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Senha", senhaHash);
                cmd.Parameters.AddWithValue("@RoleId", 3);

                try
                {
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"[DB ERROR] {ex.Message}");
                    return false;
                }
            }
        }

        public static Usuario GetUserByEmail(string email)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT Id, Nome, Email, Senha FROM Users WHERE Email = @Email", con))
            {
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = email;
                con.Open();

                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        return new Usuario
                        {
                            Id = r.GetInt32(0),
                            Nome = r.GetString(1),
                            Email = r.GetString(2),
                            Senha = r.GetString(3)
                        };
                    }
                }
            }

            return null;
        }

        public static bool UpdatePassword(int userId, string novaSenhaHash)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("UPDATE Users SET Senha = @Senha WHERE Id = @Id", con))
            {
                cmd.Parameters.Add("@Senha", SqlDbType.NVarChar).Value = novaSenhaHash;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = userId;

                con.Open();
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
        }

        public static bool CreateNotification(int userId, string titulo, string mensagem)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO Notifications (UserId, Titulo, Mensagem, DataCriacao, Lida)
                VALUES (@UserId, @Titulo, @Mensagem, GETDATE(), 0)", con))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@Titulo", SqlDbType.NVarChar).Value = titulo;
                cmd.Parameters.Add("@Mensagem", SqlDbType.NVarChar).Value = mensagem;

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public static List<Notification> GetNotifications(int userId, int roleId)
        {
            var list = new List<Notification>();

            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;

                if (roleId == 1 || roleId == 2) // Admin ou suporte
                {
                    cmd.CommandText = @"
                        SELECT Id, Titulo, Mensagem, DataCriacao, Lida
                        FROM Notifications
                        ORDER BY DataCriacao DESC";
                }
                else // Usuário comum
                {
                    cmd.CommandText = @"
                        SELECT Id, Titulo, Mensagem, DataCriacao, Lida
                        FROM Notifications
                        WHERE UserId = @UserId
                        ORDER BY DataCriacao DESC";
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                }

                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Notification
                        {
                            Id = reader.GetInt32(0),
                            Titulo = reader.GetString(1),
                            Mensagem = reader.GetString(2),
                            DataCriacao = reader.GetDateTime(3),
                            Lida = reader.GetBoolean(4)
                        });
                    }
                }
            }

            return list;
        }

        public static void MarkNotificationAsRead(int notificationId)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("UPDATE Notifications SET Lida = 1 WHERE Id = @Id", con))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = notificationId;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetUserRole(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT RoleId FROM Users WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 3; // 3 = usuário comum padrão
                }
            }
            catch
            {
                return 3;
            }
        }
    }
}
