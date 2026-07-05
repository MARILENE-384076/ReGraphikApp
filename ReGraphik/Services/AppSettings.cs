using System.IO;
using System.Text.Json;

namespace ReGraphik.Services
{
    /// <summary>
    /// Gerencia as configurações da aplicação WPF carregadas a partir do arquivo <c>appsettings.json</c>
    /// localizado ao lado do executável. Substitui strings hard-coded espalhadas nos ViewModels.
    /// </summary>
    public static class AppSettings
    {
        private static readonly string _caminhoArquivo =
            Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        private static ConfigModel? _config;

        /// <summary>
        /// Configurações carregadas do JSON. Inicializadas na primeira leitura (lazy).
        /// </summary>
        private static ConfigModel Config => _config ??= Carregar();

        /// <summary>
        /// URL base da API REST do ReGraphik (ex: https://webregraphik.runasp.net/api/).
        /// </summary>
        public static string UrlApi => Config.UrlApi;

        /// <summary>
        /// URL do Firebase Realtime Database.
        /// </summary>
        public static string FirebaseDatabaseUrl => Config.FirebaseDatabaseUrl;

        /// <summary>
        /// Listas de opções dos ComboBoxes carregadas do JSON,
        /// evitando valores hard-coded no XAML.
        /// </summary>
        public static DropdownsConfig Dropdowns => Config.Dropdowns;

        /// <summary>
        /// Lê e desserializa o arquivo <c>appsettings.json</c>.
        /// Em caso de falha, retorna um objeto com valores padrão para não travar a aplicação.
        /// </summary>
        private static ConfigModel Carregar()
        {
            try
            {
                if (!File.Exists(_caminhoArquivo))
                    return ConfigModel.Padrao();

                var json = File.ReadAllText(_caminhoArquivo);
                return JsonSerializer.Deserialize<ConfigModel>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? ConfigModel.Padrao();
            }
            catch
            {
                return ConfigModel.Padrao();
            }
        }

        /// <summary>
        /// Raiz do arquivo appsettings.json do cliente WPF.
        /// </summary>
        private class ConfigModel
        {
            public string UrlApi { get; set; } = "https://webregraphik.runasp.net/api/";
            public string FirebaseDatabaseUrl { get; set; } = "https://regraphikfirebase-default-rtdb.firebaseio.com/";
            public DropdownsConfig Dropdowns { get; set; } = new();

            /// <summary>
            /// Valores padrão usados quando o arquivo não é encontrado ou está corrompido.
            /// </summary>
            public static ConfigModel Padrao() => new ConfigModel
            {
                UrlApi = "https://webregraphik.runasp.net/api/",
                FirebaseDatabaseUrl = "https://regraphikfirebase-default-rtdb.firebaseio.com/",
                Dropdowns = DropdownsConfig.Padrao()
            };
        }
    }

    /// <summary>
    /// Contém as listas de opções exibidas nos ComboBoxes da aplicação.
    /// Carregadas do <c>appsettings.json</c> para evitar hard-coding no XAML.
    /// </summary>
    public class DropdownsConfig
    {
        /// <summary>
        /// Tipos de material aceitos no cadastro de resíduos.
        /// </summary>
        public List<string> TiposMaterial { get; set; } = new();

        /// <summary>
        /// Especificações técnicas de acabamento/superfície do material.
        /// </summary>
        public List<string> Especificacoes { get; set; } = new();

        /// <summary>
        /// Origens possíveis do resíduo no processo produtivo.
        /// </summary>
        public List<string> Origens { get; set; } = new();

        /// <summary>
        /// Condições físicas em que o material pode se encontrar.
        /// </summary>
        public List<string> Condicoes { get; set; } = new();

        /// <summary>
        /// Status possíveis de um resíduo no estoque reverso.
        /// </summary>
        public List<string> Status { get; set; } = new();

        /// <summary>
        /// Unidades de medida disponíveis para a quantidade do resíduo.
        /// </summary>
        public List<string> UnidadesMedida { get; set; } = new();

        /// <summary>
        /// Unidades de medida disponíveis para o comprimento e largura (Dimensões).
        /// </summary>
        public List<string> UnidadesDimensao { get; set; } = new();

        /// <summary>
        /// Perfis de usuário gerenciáveis pelo administrador.
        /// </summary>
        public List<string> Perfis { get; set; } = new();

        /// <summary>
        /// Retorna listas com os valores padrão do domínio gráfico.
        /// </summary>
        public static DropdownsConfig Padrao() => new DropdownsConfig
        {
            TiposMaterial = new List<string>
            {
                "Papel Offset", "Papel Couché", "Papel Kraft", "Papelão",
                "Papel Reciclado", "Papel Supremo", "Adesivo Vinil", "Lona",
                "PVC", "Acrílico", "Placa PS", "ACM", "MDF", "Tecido", "Plástico", "Outro"
            },
            Especificacoes = new List<string>
            {
                "Padrão / Comum", "Brilhoso / Brilho", "Fosco", "Acetinado / Semi-brilho",
                "Soft Touch (Aveludado)", "Metalizado / Perolado", "Texturizado",
                "Transparente", "Translúcido / Leitoso", "Opaco / Blockout",
                "Gramatura Baixa", "Gramatura Média", "Gramatura Alta", "Rígido (Espesso)", "Outro"
            },
            Origens = new List<string>
            {
                "Produção", "Acabamento", "Pré-impressão", "Expedição", "Outro"
            },
            Condicoes = new List<string>
            {
                "Bom", "Regular", "Danificado", "Para descarte"
            },
            Status = new List<string>
            {
                "Disponível",     
                "Reservado",       
                "Em Análise",      
                "Coletado",        
                "Indisponível"     
            },
            UnidadesMedida = new List<string>
            {
                "kg", "g", "ton", "unid", "m²", "m"
            },
            UnidadesDimensao = new List<string> 
            {
                "cm", "m", "mm"
            },
            Perfis = new List<string>
            {
                "Administrador", "Usuário"
            }
        };
    }
}
