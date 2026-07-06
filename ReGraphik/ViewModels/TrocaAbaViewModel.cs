using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReGraphik.Services.Interface;
using System.Windows.Input;


namespace ReGraphik.ViewModels
{
    /// <summary>
    /// Comando de troca das abas de login e cadastro
    /// </summary>
    internal class TrocaAbaViewModel : BaseViewModel
    {
        private int _abaSelecionadaIndex;

        /// Instâncias das ViewModels filhas
        public LoginViewModel LoginVM { get; }
        public CadastroViewModel CadastroVM { get; }

        public int AbaSelecionadaIndex
        {
            get => _abaSelecionadaIndex;
            set { _abaSelecionadaIndex = value; OnPropertyChanged(); }
        }

        /// O comando que o botão do XAML vai chamar
        public ICommand AlternarAbasCommand { get; }

        public TrocaAbaViewModel(IAutorizarService? autorizarService = null)
        {
            LoginVM = new LoginViewModel(autorizarService);
            CadastroVM = new CadastroViewModel();

            /// Inicializa o comando de alternar no formato padrão do seu projeto
            AlternarAbasCommand = new RelayCommand(_ => AlternarAbas());
        }

        private void AlternarAbas()
        {
            /// Se estiver no Login (0), vai para Cadastro (1), e vice-versa
            AbaSelecionadaIndex = AbaSelecionadaIndex == 0 ? 1 : 0;
        }
    }
}
