using System.Windows;
using ReGraphik.Models;
using ReGraphik.ViewModels;

namespace ReGraphik.Views
{
    public partial class SugestaoResiduoWindow : Window
    {
        private SugestaoResiduoViewModel _viewModel;

        /// <summary>
        /// Construtor padrão interno 
        /// </summary>
        public SugestaoResiduoWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// CONSTRUTOR REAL: Esse é o que o seu botão "Sugestões" da tabela deve chamar!
        /// </summary>
        /// <param name="residuo"></param>
        public SugestaoResiduoWindow(Residuo residuo) : this()
        {
            if (residuo != null)
            {
                /// Instancia a ViewModel passando o resíduo clicado
                _viewModel = new SugestaoResiduoViewModel(residuo);

                /// Define o DataContext para o XAML enxergar as listas e propriedades!
                this.DataContext = _viewModel;

                /// Registra uma ação para fechar a janela quando a API retornar sucesso
                _viewModel.SugestaoAplicadaComSucesso += () => this.Close();
            }
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }
    }
}