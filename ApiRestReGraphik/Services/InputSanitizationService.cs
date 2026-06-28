using System.Text.RegularExpressions;

namespace ApiRestReGraphik.Services
{
    /// <summary>
    /// Serviço utilitário para sanitização e validação de entradas recebidas pela API.
    /// Como o ReGraphik usa Firebase Realtime Database (NoSQL), não há risco de SQL Injection
    /// clássico via query strings SQL. Porém, ataques de injeção ainda podem ocorrer via:
    /// <list type="bullet">
    ///   <item>Path traversal nos nós do Firebase (ex: <c>../../admin</c>)</item>
    ///   <item>Scripts em campos de texto exibidos na UI (XSS)</item>
    ///   <item>Payloads JSON malformados que corrompem a estrutura do banco</item>
    /// </list>
    /// Este serviço centraliza as validações, aplicadas pelos Controllers antes de
    /// persistir qualquer dado.
    /// </summary>
    public static class InputSanitizationService
    {
        // ─── Padrões proibidos ────────────────────────────────────────────────────

        /// <summary>
        /// Caracteres inválidos para chaves/nós do Firebase Realtime Database.
        /// O Firebase proíbe: . $ # [ ] / e caracteres de controle ASCII.
        /// </summary>
        private static readonly Regex _firebaseKeyInvalida =
            new(@"[.$#\[\]/\x00-\x1F]", RegexOptions.Compiled);

        /// <summary>
        /// Detecta tentativas de path traversal (ex: <c>../</c>, <c>..\</c>).
        /// </summary>
        private static readonly Regex _pathTraversal =
            new(@"\.\.[/\\]", RegexOptions.Compiled);

        /// <summary>
        /// Detecta tags HTML/script que podem indicar tentativa de XSS.
        /// </summary>
        private static readonly Regex _htmlTag =
            new(@"<[^>]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // ─── Métodos públicos ─────────────────────────────────────────────────────

        /// <summary>
        /// Valida se um ID (usado como chave de nó no Firebase) é seguro para uso.
        /// Rejeita strings nulas, vazias, com caracteres proibidos pelo Firebase ou
        /// com tentativas de path traversal.
        /// </summary>
        /// <param name="id">ID a ser validado.</param>
        /// <returns><c>true</c> se o ID for seguro; <c>false</c> caso contrário.</returns>
        public static bool IdEhSeguro(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))  return false;
            if (id.Length > 128)                return false;
            if (_firebaseKeyInvalida.IsMatch(id)) return false;
            if (_pathTraversal.IsMatch(id))      return false;
            return true;
        }

        /// <summary>
        /// Sanitiza um campo de texto livre removendo tags HTML e truncando
        /// para o comprimento máximo especificado.
        /// </summary>
        /// <param name="valor">Valor de entrada.</param>
        /// <param name="tamanhoMaximo">Tamanho máximo permitido (padrão: 500).</param>
        /// <returns>Texto sanitizado ou string vazia se o valor for nulo.</returns>
        public static string SanitizarTexto(string? valor, int tamanhoMaximo = 500)
        {
            if (string.IsNullOrWhiteSpace(valor)) return string.Empty;

            // Remove tags HTML para mitigar XSS em campos que possam ser renderizados
            var sanitizado = _htmlTag.Replace(valor.Trim(), string.Empty);

            return sanitizado.Length > tamanhoMaximo
                ? sanitizado[..tamanhoMaximo]
                : sanitizado;
        }

        /// <summary>
        /// Valida e sanitiza todos os campos de texto de um <see cref="Models.Residuo"/>,
        /// retornando uma lista de mensagens de erro encontradas.
        /// </summary>
        /// <param name="residuo">Objeto a ser validado.</param>
        /// <returns>Lista vazia se válido; lista com mensagens de erro caso contrário.</returns>
        public static List<string> ValidarResiduo(Models.Residuo residuo)
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(residuo.TipoResiduo))
                erros.Add("O campo 'tipo_residuo' é obrigatório.");

            if (string.IsNullOrWhiteSpace(residuo.Origem))
                erros.Add("O campo 'origem' é obrigatório.");

            if (residuo.Quantidade <= 0)
                erros.Add("O campo 'quantidade' deve ser maior que zero.");

            if (_htmlTag.IsMatch(residuo.TipoResiduo   ?? "")) erros.Add("'tipo_residuo' contém conteúdo inválido.");
            if (_htmlTag.IsMatch(residuo.Origem         ?? "")) erros.Add("'origem' contém conteúdo inválido.");
            if (_htmlTag.IsMatch(residuo.Especificacao  ?? "")) erros.Add("'especificacao' contém conteúdo inválido.");
            if (_htmlTag.IsMatch(residuo.Observacao     ?? "")) erros.Add("'observacao' contém conteúdo inválido.");

            return erros;
        }

        /// <summary>
        /// Valida os campos de um <see cref="Models.Usuario"/> antes da persistência.
        /// </summary>
        /// <param name="usuario">Objeto a ser validado.</param>
        /// <returns>Lista vazia se válido; lista com mensagens de erro caso contrário.</returns>
        public static List<string> ValidarUsuario(Models.Usuario usuario)
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(usuario.Email))
                erros.Add("O campo 'email' é obrigatório.");
            else if (!usuario.Email.Contains('@') || !usuario.Email.Contains('.'))
                erros.Add("O campo 'email' não é um endereço válido.");

            if (string.IsNullOrWhiteSpace(usuario.Login))
                erros.Add("O campo 'login' é obrigatório.");
            else if (usuario.Login.Length < 3 || usuario.Login.Length > 50)
                erros.Add("O campo 'login' deve ter entre 3 e 50 caracteres.");

            if (string.IsNullOrWhiteSpace(usuario.Senha))
                erros.Add("O campo 'senha' é obrigatório.");
            else if (usuario.Senha.Length < 6)
                erros.Add("A senha deve ter pelo menos 6 caracteres.");

            if (_htmlTag.IsMatch(usuario.Nome  ?? "")) erros.Add("'nome' contém conteúdo inválido.");
            if (_htmlTag.IsMatch(usuario.Login ?? "")) erros.Add("'login' contém conteúdo inválido.");

            return erros;
        }
    }
}
