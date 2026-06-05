using Firebase.Database;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace ApiRestReGraphik.Data
{
    public class DbReGraphik
    {
        /// <summary>
        /// Propriedade que representa a instância do cliente do Firebase, permitindo a interação com o Firebase Realtime 
        /// Database para realizar operações de leitura e escrita de dados.
        /// </summary>
        public FirebaseClient DbFirebase { get; }

        /// <summary>
        /// Construtor da classe DbReGraphik, responsável por configurar a conexão com o Firebase Realtime Database utilizando as credenciais fornecidas
        /// </summary>
        /// <param name="configuration"></param>
        [Obsolete]
        public DbReGraphik(IConfiguration configuration)
        {
            // Obtém o caminho do arquivo de credenciais do Firebase a partir do arquivo de configuração
            var credentialJson =
                configuration["Firebase:CredentialFilePath"];

            // Obtém o diretório de execução da aplicação
            var pastaExecucao =
                AppContext.BaseDirectory;

            // Combina o diretório de execução com o nome do arquivo de credenciais para obter o caminho completo
            var caminhoCompletoChave =
                Path.Combine(
                    pastaExecucao,
                    credentialJson);

            // Verifica se a instância do FirebaseApp já foi criada para evitar múltiplas inicializações
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential =
                        GoogleCredential.FromFile(
                            caminhoCompletoChave)
                });
            }

            // Configura a URL do Firebase Realtime Database a partir do arquivo de configuração
            DbFirebase = new FirebaseClient(configuration["Firebase:RealtimeDatabaseUrl"]);

        }
    }
}
