using OpenAI.Chat;
using System.Configuration;
using System.Threading.Tasks;

namespace TechSystem.Data
{
    public static class AIHelper
    {
        private static readonly ChatClient chatClient;

        static AIHelper()
        {
            string apiKey = ConfigurationManager.AppSettings["OpenAIKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new System.Exception("OpenAIKey não encontrada no App.config");

            chatClient = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);
        }

        public static async Task<string> CategorizarChamado(string titulo, string descricao)
        {
            string prompt =
                $"O usuário abriu um chamado com título: '{titulo}' e descrição: '{descricao}'. " +
                "Sugira uma categoria (Rede, Hardware, Software, Acesso) e uma breve sugestão de solução.";

            
            ChatCompletion completion = await chatClient.CompleteChatAsync(prompt);

            if (completion?.Content != null && completion.Content.Count > 0)
            {
                return completion.Content[0].Text?.Trim() ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
