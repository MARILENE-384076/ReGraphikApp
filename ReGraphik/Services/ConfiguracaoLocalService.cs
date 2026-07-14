using System;
using System.IO;

namespace ReGraphik.Services
{
    /// <summary>
    /// Serviço para salvar e carregar configurações locais, como o caminho da foto do usuário.
    /// </summary>
    public static class ConfiguracaoLocalService
    {
        private static readonly string _pasta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReGraphik");

        private static readonly string _arquivo = Path.Combine(_pasta, "config.txt");

        public static void SalvarFoto(string caminho)
        {
            Directory.CreateDirectory(_pasta);
            File.WriteAllText(_arquivo, caminho);
        }

        public static string? CarregarFoto()
        {
            if (!File.Exists(_arquivo)) return null;
            var caminho = File.ReadAllText(_arquivo).Trim();
            return File.Exists(caminho) ? caminho : null;
        }

        public static void LimparFoto()
        {
            if (File.Exists(_arquivo))
                File.Delete(_arquivo);
        }
    }
}