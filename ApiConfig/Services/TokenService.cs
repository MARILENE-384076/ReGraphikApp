namespace ApiConfig.Services
{
    public class TokenService
    {
        public string GerarToken()
        {
            Random random = new();

            return random
                .Next(100000, 999999)
                .ToString();
        }
    }
}
