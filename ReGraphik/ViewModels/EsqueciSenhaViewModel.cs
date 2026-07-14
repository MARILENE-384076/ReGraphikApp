using CommunityToolkit.Mvvm.Input;
using ReGraphik.Services;
using ReGraphik.Views;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel para a janela de "Esqueci minha senha", responsável por gerenciar a lógica de recuperação de senha.
    /// </summary>
    public class EsqueciSenhaViewModel : BaseViewModel
    {
        /// <summary>
        /// Serviço de autorização que lida com a comunicação com a API para recuperação de senha.
        /// </summary>
        private readonly AutorizarService _autorizarService;

        /// <summary>
        /// O e-mail corporativo do usuário que deseja recuperar a senha.
        /// </summary>
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private bool _processando;
        public bool Processando
        {
            get => _processando;
            set
            {
                _processando = value;
                OnPropertyChanged();

                /// Atualiza o estado do comando RecuperarSenhaCommand
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Comando que inicia o processo de recuperação de senha quando executado.
        /// </summary>
        public ICommand RecuperarSenhaCommand { get; }

        /// <summary>
        /// Inicializa uma nova instância do ViewModel de recuperação de senha, configurando o serviço de autorização e o comando de recuperação.
        /// </summary>
        public EsqueciSenhaViewModel()
        {
            /// Instancia o seu serviço real de API
            _autorizarService = new AutorizarService();

            RecuperarSenhaCommand = new RelayCommand(async _ => await RecuperarSenhaAsync(), _ => !Processando);
        }

        /// <summary>
        /// Método assíncrono que gerencia o processo de recuperação de senha, incluindo validação do e-mail, chamada à API e exibição de mensagens de sucesso ou erro.
        /// </summary>
        /// <returns></returns>
        private async Task RecuperarSenhaAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Informe o e-mail corporativo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Email.Contains('@') || !Email.Contains('.'))
            {
                MessageBox.Show("Informe um e-mail válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Processando = true;

                /// Chama a sua API real para fazer o envio do e-mail de recuperação
                string respostaApi = await _autorizarService.RecuperarSenhaAsync(Email.Trim());

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir(
                        "Solicitação Enviada",
                        respostaApi, /// Exibe a mensagem de sucesso retornada pela sua API
                        MensagemWindow.TipoMensagem.Sucesso
                    );
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    /// Exibe o erro retornado pela API (ex: e-mail não encontrado)
                    MensagemWindow.Exibir(
                        "Erro na Recuperação",
                        ex.Message,
                        MensagemWindow.TipoMensagem.Erro
                    );
                });
            }
            finally
            {
                Processando = false;
            }
        }
    }
}