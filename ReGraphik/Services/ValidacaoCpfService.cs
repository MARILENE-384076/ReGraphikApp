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
            if (string.IsNullOrWhiteSpace(cpf)) return false;

            var digits = System.Text.RegularExpressions.Regex.Replace(cpf, @"\D", "");

            if (digits.Length != 11) return false;

            if (new string(digits[0], 11) == digits) return false;

            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(digits[i].ToString()) * (10 - i);

            int resto = soma % 11;
            int primeiroDigito = resto < 2 ? 0 : 11 - resto;

            if (primeiroDigito != int.Parse(digits[9].ToString())) return false;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(digits[i].ToString()) * (11 - i);

            resto = soma % 11;
            int segundoDigito = resto < 2 ? 0 : 11 - resto;

            return segundoDigito == int.Parse(digits[10].ToString());
        }

        /// <summary>
        /// Formata o CPF no padrão 000.000.000-00 caso seja válido.
        /// </summary>
        public static string Formatar(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return string.Empty;
            var d = System.Text.RegularExpressions.Regex.Replace(cpf, @"\D", "");
            return d.Length == 11
                ? $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..11]}"
                : cpf;
        }
    }
}