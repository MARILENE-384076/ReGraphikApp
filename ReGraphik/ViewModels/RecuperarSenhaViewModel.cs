using ReGraphik.Services.Interface;
using System.Windows;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    

    public class RecuperarSenhaViewModel : BaseViewModel
    {
        private readonly IAutorizarService _autorizarService;

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public ICommand RecuperarSenhaCommand { get; }

        public RecuperarSenhaViewModel(IAutorizarService autorizarService)
        {
            _autorizarService = autorizarService ?? throw new ArgumentNullException(nameof(autorizarService));

            RecuperarSenhaCommand = new RelayCommand(async _ => await RecuperarSenhaAsync(), _ => !string.IsNullOrWhiteSpace(Email));
        }

        private async Task RecuperarSenhaAsync()
        {
            string email = Email.Trim();

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                MessageBox.Show("Por favor, insira um endereço de e-mail válido.", "Campo Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {

                await _autorizarService.RecuperarSenhaAsync(email);

                MessageBox.Show($"Se o e-mail {email} estiver cadastrado, você receberá as instruções de recuperação em instantes.",
                                "E-mail Enviado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao tentar enviar o e-mail: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

