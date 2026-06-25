namespace ReGraphik.Services
{
    /// <summary>
    /// Serviço responsável por validar o CPF do usuário utilizando o algoritmo oficial dos dígitos verificadores.
    /// </summary>
    public static class ValidacaoCpfService
    {
        /// <summary>
        /// Valida se o CPF informado é válido de acordo com o algoritmo dos dígitos verificadores.
        /// Remove caracteres não numéricos antes de validar.
        /// </summary>
        public static bool Validar(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            /// Remove pontos, traços e espaços, deixando apenas os números
            string cpfLimpo = System.Text.RegularExpressions.Regex.Replace(cpf, @"[^\d]", "");

            /// O CPF deve conter exatamente 11 dígitos numéricos
            if (cpfLimpo.Length != 11)
                return false;

            // Ignora CPFs com todos os dígitos iguais (ex: 111.111.111-11)
            if (new string(cpfLimpo[0], 11) == cpfLimpo)
                return false;

            /// Cálculo do primeiro dígito verificador
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf = cpfLimpo.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf = tempCpf + digito;

            /// Cálculo do segundo dígito verificador
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            /// Verifica se os dois últimos dígitos calculados batem com os do CPF digitado
            return cpfLimpo.EndsWith(digito);
        }

        /// <summary>
        /// Formata o CPF no padrão 000.000.000-00 caso seja válido.
        /// </summary>
        public static string Formatar(string? cpf)
        {
            string apenasNumeros = System.Text.RegularExpressions.Regex.Replace(cpf, @"[^\d]", "");
            if (apenasNumeros.Length != 11) return cpf;

            return Convert.ToUInt64(apenasNumeros).ToString(@"000\.000\.000\-00");
        }
    }
}