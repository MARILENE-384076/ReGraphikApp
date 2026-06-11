using ReGraphik.Models;
using ReGraphik.Views;
using ReGraphik.Views.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel para o controle da tela de Estoque Reverso.
    /// </summary>
    public class EstoqueReversoViewModel : BaseViewModel
    {
        /// <summary>
        /// Cliente para chamadas na API.
        /// </summary>
        private readonly HttpClient _http = new();

        /// <summary>
        /// Lista vinculada ao DataGrid na interface.
        /// </summary>
        public ObservableCollection<Residuo> ListaResiduos { get; set; } = new();

        /// <summary>
        /// Comando para abrir a tela de sugestões.
        /// </summary>
        public ICommand SugestaoCommand { get; }

        /// <summary>
        /// Comando para exportar os dados internamente (ex: Cadastro de Resíduos).
        /// </summary>
        public ICommand ExportarCommand { get; }

        /// <summary>
        /// Inicializa os comandos e carrega os dados ao abrir a tela.
        /// </summary>
        public EstoqueReversoViewModel()
        {
            SugestaoCommand = new RelayCommand(
                (param) => AbrirSugestoes(param),
                (param) => param is Residuo
            );

            ExportarCommand = new RelayCommand(() => ExportarDadosInternos());

            _ = CarregarEstoqueDoBancoAsync();
        }

        /// <summary>
        /// Lógica para exportar os dados do estoque de volta para o Cadastro de resíduos.
        /// </summary>
        private void ExportarDadosInternos()
        {
            // TODO: Implementar a lógica de exportação interna.
            MessageBox.Show("Comando de exportação acionado internamente!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Abre a tela de sugestões passando o resíduo selecionado.
        /// </summary>
        /// <param name="param">O objeto Residuo passado pelo botão da Grid.</param>
        private void AbrirSugestoes(object param)
        {
            if (param is not Residuo residuo) return;

            if (Application.Current.MainWindow is MainWindow mainWin)
            {
                mainWin.Content = new SugestaoResiduoControl(residuo);
            }
        }

        /// <summary>
        /// Busca a lista de resíduos no banco via API e atualiza a interface.
        /// </summary>
        private async Task CarregarEstoqueDoBancoAsync()
        {
            try
            {
                string urlApi = "https://webregraphik.runasp.net/api/Residuo";
                var resposta = await _http.GetAsync(urlApi);

                if (resposta.IsSuccessStatusCode)
                {
                    var json = await resposta.Content.ReadAsStringAsync();
                    var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var lista = JsonSerializer.Deserialize<List<Residuo>>(json, opcoes);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ListaResiduos.Clear();
                        if (lista != null)
                        {
                            foreach (var item in lista)
                                ListaResiduos.Add(item);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar estoque: {ex.Message}");
            }
        }
    }
}