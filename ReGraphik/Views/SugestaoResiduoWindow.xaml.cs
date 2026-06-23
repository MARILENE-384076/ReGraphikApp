using System;
using System.Windows;
using ReGraphik.Models;
using ReGraphik.ViewModels;

namespace ReGraphik.Views
{
    /// <summary>
    /// Define a interação lógica para a janela SugestaoResiduoWindow.xaml.
    /// Incorpora tratamentos de erro explícitos para assegurar o ciclo de fechamento e carregamento.
    /// </summary>
    public partial class SugestaoResiduoWindow : Window
    {
        /// <summary>
        /// Referência interna para a ViewModel de controle do contexto de dados.
        /// </summary>
        private SugestaoResiduoViewModel _viewModel;

        /// <summary>
        /// CONSTRUTOR PADRÃO: Inicializa os componentes visuais essenciais e associa uma ViewModel padrão vazia 
        /// para prevenir exceções de quebra de Binding caso o DataContext fique nulo.
        /// </summary>
        public SugestaoResiduoWindow()
        {
            try
            {
                InitializeComponent();

                // Instanciação preventiva de segurança para evitar DataContext nulo em chamadas sem parâmetros
                _viewModel = new SugestaoResiduoViewModel(null);
                this.DataContext = _viewModel;

                // Assina o evento para delegar o fechamento seguro da interface através da thread de UI
                _viewModel.SugestaoAplicadaComSucesso += () =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        try { this.Close(); } catch { }
                    });
                };
            }
            catch (Exception exInit)
            {
                System.Diagnostics.Debug.WriteLine($"Erro crítico no InitializeComponent de SugestaoResiduoWindow: {exInit.Message}");
            }
        }

        /// <summary>
        /// CONSTRUTOR DE SELEÇÃO: Acionado externamente injetando o resíduo alvo focado na listagem do estoque.
        /// </summary>
        /// <param name="residuo">A entidade <see cref="Residuo"/> que guiará o filtro inicial de sugestões.</param>
        public SugestaoResiduoWindow(Residuo residuo)
        {
            try
            {
                InitializeComponent();

                // Atribui a ViewModel injetando explicitamente a entidade de dados recebida por parâmetro
                _viewModel = new SugestaoResiduoViewModel(residuo);
                this.DataContext = _viewModel;

                // Registra a escuta do evento de sucesso vinculando a rotina de fechamento de janela
                _viewModel.SugestaoAplicadaComSucesso += () =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        try { this.Close(); } catch { }
                    });
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro crítico ao instanciar SugestaoResiduoWindow com parâmetro: {ex.Message}");

                // Fallback de segurança para impedir travamentos irreversíveis na interface do usuário
                try { this.Close(); } catch { }
            }
        }

        /// <summary>
        /// Trata o evento de clique do botão físico de fechamento, encerrando o ciclo de vida do componente.
        /// </summary>
        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception exClose)
            {
                System.Diagnostics.Debug.WriteLine($"Falha isolada ao fechar janela de sugestões: {exClose.Message}");
            }
        }
    }
}