using System;

namespace TechSystem.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public DateTime DataCriacao { get; set; }
        public bool Lida { get; set; }
    }
}
