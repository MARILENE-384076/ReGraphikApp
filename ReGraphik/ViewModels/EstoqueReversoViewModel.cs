using ReGraphik.Models;
using ReGraphik.Views;
using ReGraphik.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    public class EstoqueReversoViewModel : BaseViewModel
    {
        /// <summary>
        /// Instância única do HttpClient para toda a classe
        /// </summary>
        private readonly HttpClient _http = new();

        /// <summary>
        /// Lista de resíduos exibida no DataGrid
        /// </summary>
        public ObservableCollection<Residuo> ListaResiduos { get; set; } = new();

        /// <summary>
        /// Comando vinculado ao botão "💡 Sugestões" de cada linha do DataGrid.
        /// Recebe o Residuo da linha como CommandParameter (object) e abre a janela modal.
        /// </summary>
        public ICommand SugestaoCommand { get; }

        /// <summary>
        /// Comando vinculado ao botão "Exportar".
        /// </summary>
        public ICommand ExportarCommand { get; }

        public EstoqueReversoViewModel()
        {
            /// RelayCommand com parâmetro (Action<object>) para receber o Residuo da linha
            SugestaoCommand = new RelayCommand(
                (param) => AbrirSugestoes(param),
                (param) => param is Residuo
            );

            /// Inicializa o comando de Exportar apontando para o método correspondente
            ExportarCommand = new RelayCommand(() => ExportarParaCsv());

            _ = CarregarEstoqueDoBancoAsync();
        }

        /// <summary>
        /// Pega a lista atual de resíduos e gera um arquivo CSV (Excel) para o usuário salvar no computador.
        /// </summary>
        private void ExportarParaCsv()
        {
            if (ListaResiduos == null || ListaResiduos.Count == 0)
            {
                MessageBox.Show("Não há dados no estoque para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                /// Abre a janela padrão do Windows para o usuário escolher onde salvar o arquivo
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"EstoqueReverso_{DateTime.Now:dd-MM-yyyy}",
                    DefaultExt = ".csv",
                    Filter = "Planilha Excel (CSV) (*.csv)|*.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    var sb = new StringBuilder();

                    /// Usa Reflection para pegar o nome de todas as propriedades da classe Residuo automaticamente (ex: Nome, Quantidade, Categoria) para montar o cabeçalho
                    var propriedades = typeof(Residuo).GetProperties();
                    var cabecalho = string.Join(";", propriedades.Select(p => p.Name));
                    sb.AppendLine(cabecalho);

                    /// Percorre todos os itens da lista e escreve os valores separados por ponto e vírgula
                    foreach (var item in ListaResiduos)
                    {
                        var linha = string.Join(";", propriedades.Select(p =>
                        {
                            var valor = p.GetValue(item)?.ToString() ?? "";
                            /// Remove quebras de linha e troca ponto-e-vírgula por vírgula para não quebrar as colunas do Excel
                            return valor.Replace("\r", "").Replace("\n", " ").Replace(";", ",");
                        }));
                        sb.AppendLine(linha);
                    }

                    /// Salva o arquivo no caminho escolhido pelo usuário com a codificação UTF8 (para acentos funcionarem)
                    File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Planilha exportada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao exportar o arquivo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Abre a janela de sugestões para o resíduo selecionado 
        /// </summary>
        /// <param name="param"></param>
        private void AbrirSugestoes(object param)
        {
            if (param is not Residuo residuo)
            {
                MessageBox.Show("Selecione um resíduo válido.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            /// Verifica se a janela principal é do tipo MainWindow para evitar erros de cast
            if (Application.Current.MainWindow is MainWindow mainWin)
            {
                /// Substitui o conteúdo da janela principal pela view de sugestões, passando o resíduo selecionado
                mainWin.Content = new SugestaoResiduoView(residuo);
            }
        }

        /// <summary>
        /// Carrega resíduos da API
        /// </summary>
        /// <returns></returns>
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