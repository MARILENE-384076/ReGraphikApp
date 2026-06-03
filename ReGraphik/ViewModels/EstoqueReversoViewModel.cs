using ReGraphik.Models;
using ReGraphik.Views.Pages;
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
    public class EstoqueReversoViewModel : BaseViewModel
    {
        // Instância única do HttpClient para toda a classe
        private readonly HttpClient _http = new();

        // Lista de resíduos exibida no DataGrid
        public ObservableCollection<Residuo> ListaResiduos { get; set; } = new();

        /// <summary>
        /// Comando vinculado ao botão "💡 Sugestões" de cada linha do DataGrid.
        /// Recebe o Residuo da linha como CommandParameter (object) e abre a janela modal.
        /// </summary>
        public ICommand SugestaoCommand { get; }

        public EstoqueReversoViewModel()
        {
            // RelayCommand com parâmetro (Action<object>) para receber o Residuo da linha
            SugestaoCommand = new RelayCommand(
                (param) => AbrirSugestoes(param),
                (param) => param is Residuo
            );

            _ = CarregarEstoqueDoBancoAsync();
        }

        // ─── Abre a janela de sugestões para o resíduo selecionado ────────
        private void AbrirSugestoes(object param)
        {
            if (param is not Residuo residuo)
            {
                MessageBox.Show("Selecione um resíduo válido.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Cria e exibe a janela modal de sugestões
            var janela = new SugestaoResiduoWindow(residuo)
            {
                // Define a janela principal como Owner para centralizar corretamente
                Owner = Application.Current.MainWindow
            };

            janela.ShowDialog();
        }

        // ─── Carrega resíduos da API ──────────────────────────────────────
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
                    var listaResiduos = JsonSerializer.Deserialize<List<Residuo>>(json, opcoes);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ListaResiduos.Clear();
                        if (listaResiduos != null)
                        {
                            foreach (var residuo in listaResiduos)
                                ListaResiduos.Add(residuo);
                        }
                    });
                }
                else
                {
                    MessageBox.Show("Não foi possível buscar a lista de resíduos no servidor.",
                                    "Erro de Comunicação", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao conectar na API de Estoque: " + ex.Message);
            }
        }
    }
}
