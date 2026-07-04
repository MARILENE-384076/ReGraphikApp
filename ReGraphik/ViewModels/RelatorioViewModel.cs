using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using ReGraphik.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    public class RelatorioViewModel : BaseViewModel
    {
        private readonly IResiduoService _residuoService = new ResiduoService();

        private string _filtroTipo = "Todos";
        public string FiltroTipo
        {
            get => _filtroTipo;
            set { _filtroTipo = value; OnPropertyChanged(); }
        }

        private string _filtroStatus = "Todos";
        public string FiltroStatus
        {
            get => _filtroStatus;
            set { _filtroStatus = value; OnPropertyChanged(); }
        }

        private string _filtroOrigem = "Todas";
        public string FiltroOrigem
        {
            get => _filtroOrigem;
            set { _filtroOrigem = value; OnPropertyChanged(); }
        }

        private DateTime? _dataInicio;
        public DateTime? DataInicio
        {
            get => _dataInicio;
            set { _dataInicio = value; OnPropertyChanged(); }
        }

        private DateTime? _dataFim;
        public DateTime? DataFim
        {
            get => _dataFim;
            set { _dataFim = value; OnPropertyChanged(); }
        }

        private int _totalResiduos;
        public int TotalResiduos
        {
            get => _totalResiduos;
            set { _totalResiduos = value; OnPropertyChanged(); }
        }

        private double _pesoTotal;
        public double PesoTotal
        {
            get => _pesoTotal;
            set { _pesoTotal = value; OnPropertyChanged(); }
        }

        private int _reaproveitados;
        public int Reaproveitados
        {
            get => _reaproveitados;
            set { _reaproveitados = value; OnPropertyChanged(); }
        }

        private string _valorEconomico = "R$ 0,00";
        public string ValorEconomico
        {
            get => _valorEconomico;
            set { _valorEconomico = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Residuo> _residuosFiltrados = new();
        public ObservableCollection<Residuo> ResiduosFiltrados
        {
            get => _residuosFiltrados;
            set { _residuosFiltrados = value; OnPropertyChanged(); }
        }

        private int _totalRegistros;
        public int TotalRegistros
        {
            get => _totalRegistros;
            set { _totalRegistros = value; OnPropertyChanged(); }
        }

        private string _mensagemStatus = "Clique em \"Gerar Relatório\" para carregar os dados.";
        public string MensagemStatus
        {
            get => _mensagemStatus;
            set { _mensagemStatus = value; OnPropertyChanged(); }
        }

        private bool _carregando;
        public bool Carregando
        {
            get => _carregando;
            set { _carregando = value; OnPropertyChanged(); }
        }

        private bool _podeExportar;
        public bool PodeExportar
        {
            get => _podeExportar;
            set { _podeExportar = value; OnPropertyChanged(); }
        }

        private List<Residuo> _todosResiduos = new();

        public ICommand GerarRelatorioCommand { get; }
        public ICommand LimparFiltrosCommand { get; }
        public ICommand ExportarPdfCommand { get; }

        public RelatorioViewModel()
        {
            GerarRelatorioCommand = new RelayCommand(async () => await GerarRelatorioAsync());
            LimparFiltrosCommand = new RelayCommand(LimparFiltros);
            ExportarPdfCommand = new RelayCommand(ExportarPdf);

            QuestPDF.Settings.License = LicenseType.Community;
        }

        private async Task GerarRelatorioAsync()
        {
            try
            {
                Carregando = true;
                PodeExportar = false;
                MensagemStatus = "Carregando dados...";

                _todosResiduos = await _residuoService.ObterTodosResiduosAsync();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao carregar relatório: {ex.Message}", "Erro");
                MensagemStatus = "Erro ao carregar dados.";
            }
            finally
            {
                Carregando = false;
            }
        }

        private void AplicarFiltros()
        {
            if (_todosResiduos == null) return;

            var filtrados = _todosResiduos.AsEnumerable();

            if (FiltroTipo != "Todos") filtrados = filtrados.Where(r => r.TipoResiduo == FiltroTipo);
            if (FiltroStatus != "Todos") filtrados = filtrados.Where(r => r.Status == FiltroStatus);
            if (FiltroOrigem != "Todas") filtrados = filtrados.Where(r => r.Origem == FiltroOrigem);
            if (DataInicio.HasValue) filtrados = filtrados.Where(r => r.DataCadastro >= DataInicio.Value);
            if (DataFim.HasValue) filtrados = filtrados.Where(r => r.DataCadastro <= DataFim.Value.AddDays(1));

            var lista = filtrados.OrderByDescending(r => r.DataCadastro).ToList();

            for (int i = 0; i < lista.Count; i++)
            {
                lista[i].Id = (i + 1).ToString();
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ResiduosFiltrados = new ObservableCollection<Residuo>(lista);
                TotalRegistros = lista.Count;
                TotalResiduos = lista.Count;
                PesoTotal = lista.Sum(r => r.Quantidade);
                Reaproveitados = lista.Count(r => r.Status == "Reaproveitado");
                ValorEconomico = (lista.Sum(r => r.Quantidade * 5.50)).ToString("C2");
                PodeExportar = lista.Count > 0;
                MensagemStatus = lista.Count > 0
                    ? $"{lista.Count} registro(s) encontrado(s)."
                    : "Nenhum registro encontrado para os filtros selecionados.";
            });
        }

        private void LimparFiltros()
        {
            FiltroTipo = "Todos";
            FiltroStatus = "Todos";
            FiltroOrigem = "Todas";
            DataInicio = null;
            DataFim = null;
            ResiduosFiltrados = new ObservableCollection<Residuo>();
            TotalResiduos = 0;
            PesoTotal = 0;
            Reaproveitados = 0;
            ValorEconomico = "R$ 0,00";
            TotalRegistros = 0;
            PodeExportar = false;
            MensagemStatus = "Filtros limpos. Clique em \"Gerar Relatório\" para carregar os dados.";
        }

        private void ExportarPdf()
        {
            if (ResiduosFiltrados == null || ResiduosFiltrados.Count == 0)
            {
                MensagemPdfWindow.Exibir("Aviso", "Nenhum dado para exportar. Gere o relatório primeiro.", MensagemPdfWindow.TipoMensagem.Erro);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Salvar Relatório PDF",
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"Relatorio_ReGraphik_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                DefaultExt = ".pdf"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                var lista = ResiduosFiltrados.ToList();
                var filtros = MontarDescricaoFiltros();
                var caminho = dialog.FileName;

                /// Cores usando notação hex string (compatível com todas as versões do QuestPDF)
                const string azulEscuro = "#0D2A56";
                const string azulMedio = "#1649A2";
                const string azulClaro = "#3274BA";
                const string azulCard4 = "#2F80EC";
                const string headerBg = "#EFF6FF";
                const string headerFg = "#1E40AF";
                const string cinzaLeve = "#F1F5F9";
                const string cinzaBorda = "#CBD5E1";
                const string cinzaTexto = "#1E293B";

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9).FontColor(cinzaTexto));

                        /// Cabeçalho do PDF 
                        page.Header().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("ReGraphik").FontSize(22).Bold().FontColor(azulEscuro);
                                    c.Item().Text("Sistema de Gestão de Resíduos Gráficos").FontSize(10).FontColor(azulMedio);
                                });
                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text("RELATÓRIO DE RESÍDUOS").FontSize(14).Bold().FontColor(azulMedio);
                                    c.Item().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                                });
                            });
                            col.Item().PaddingTop(4).LineHorizontal(2).LineColor(azulMedio);
                            col.Item().PaddingTop(4).Text($"Filtros: {filtros}").FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                        });

                        /// Conteúdo
                        page.Content().PaddingTop(12).Column(col =>
                        {
                            /// Cards de resumo
                            col.Item().Row(row =>
                            {
                                Card(row, azulEscuro, "Total de Resíduos", lista.Count.ToString());
                                Card(row, azulMedio, "Peso Total", $"{PesoTotal:N2} kg");
                                Card(row, azulClaro, "Reaproveitados", Reaproveitados.ToString());
                                Card(row, azulCard4, "Valor Econômico", ValorEconomico);
                            });

                            col.Item().PaddingTop(14);

                            /// Tabela
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(45);
                                    c.RelativeColumn(2.2f);
                                    c.RelativeColumn(1.5f);
                                    c.RelativeColumn(1.8f);
                                    c.RelativeColumn(1.2f);
                                    c.RelativeColumn(1.2f);
                                    c.RelativeColumn(1.1f);
                                    c.RelativeColumn(1.2f);
                                });

                                /// Estilo do Cabeçalho da Tabela
                                IContainer CellHeader(IContainer c) =>
                                    c.Background(headerBg)
                                     .BorderBottom(1).BorderColor("#93C5FD")
                                     .Padding(6);

                                /// CORREÇÃO AQUI: table.Header é chamado apenas UMA vez
                                string[] headers = { "ID", "Material", "Origem", "Projeto", "Qtd (kg)", "Condição", "Data", "Status" };
                                table.Header(header =>
                                {
                                    foreach (var h in headers)
                                    {
                                        header.Cell().Element(CellHeader)
                                              .Text(h).Bold().FontSize(8).FontColor(headerFg);
                                    }
                                });

                                /// Linhas da Tabela
                                bool par = false;
                                foreach (var r in lista)
                                {
                                    par = !par;
                                    var bg = par ? "#FFFFFF" : cinzaLeve;

                                    IContainer Cell(IContainer c) =>
                                        c.Background(bg).BorderBottom(1).BorderColor(cinzaBorda).Padding(5);

                                    table.Cell().Element(Cell).Text(r.Id ?? "-").FontSize(8).FontColor(Colors.Grey.Medium);
                                    table.Cell().Element(Cell).Text(r.TipoResiduo ?? "-").FontSize(8).Bold();
                                    table.Cell().Element(Cell).Text(r.Origem ?? "-").FontSize(8);
                                    table.Cell().Element(Cell).Text(r.Projeto ?? "-").FontSize(8);
                                    table.Cell().Element(Cell).Text(r.Quantidade.ToString("N2")).FontSize(8);
                                    table.Cell().Element(Cell).Text(r.Condicao ?? "-").FontSize(8);
                                    table.Cell().Element(Cell).Text(r.DataCadastro.ToString("dd/MM/yyyy")).FontSize(8);

                                    var (bgStatus, fgStatus) = r.Status switch
                                    {
                                        "Reaproveitado" => ("#DCFCE7", "#166534"),
                                        "Processado" => ("#DBEAFE", "#1E40AF"),
                                        _ => ("#FEF08A", "#854D0E")
                                    };

                                    table.Cell().Element(Cell).Element(e =>
                                        e.Background(bgStatus).Padding(3)
                                         .Text(r.Status ?? "-").FontSize(7).Bold().FontColor(fgStatus));
                                }
                            });
                        });

                        /// Rodapé 
                        page.Footer().AlignCenter().Text(t =>
                        {
                            t.Span($"ReGraphik  •  Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}  •  Página ").FontSize(8).FontColor(Colors.Grey.Medium);
                            t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                            t.Span(" de ").FontSize(8).FontColor(Colors.Grey.Medium);
                            t.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });
                }).GeneratePdf(caminho);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    bool? abrir = MensagemPdfWindow.Exibir(
                        "Exportação Concluída",
                        $"Relatório em PDF gerado com sucesso!\n\nSalvo em: {caminho}\n\nDeseja abrir o arquivo agora?",
                        MensagemPdfWindow.TipoMensagem.Confirmacao);

                    if (abrir == true)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(caminho) { UseShellExecute = true });
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemPdfWindow.Exibir(
                        "Erro na Exportação",
                        $"Erro ao gerar o PDF institucional: {ex.Message}",
                        MensagemPdfWindow.TipoMensagem.Erro);
                });
            }
        }

        private static void Card(RowDescriptor row, string bg, string titulo, string valor)
        {
            row.RelativeItem().Background(bg).Padding(12).Column(c =>
            {
                c.Item().Text(valor).FontSize(16).Bold().FontColor(Colors.White);
                c.Item().Text(titulo).FontSize(8).FontColor("#CBD5E1");
            });
            row.ConstantItem(6);
        }

        private string MontarDescricaoFiltros()
        {
            var partes = new List<string>();
            if (FiltroTipo != "Todos") partes.Add($"Material: {FiltroTipo}");
            if (FiltroOrigem != "Todas") partes.Add($"Origem: {FiltroOrigem}");
            if (FiltroStatus != "Todos") partes.Add($"Status: {FiltroStatus}");
            if (DataInicio.HasValue) partes.Add($"De: {DataInicio:dd/MM/yyyy}");
            if (DataFim.HasValue) partes.Add($"Até: {DataFim:dd/MM/yyyy}");
            return partes.Count > 0 ? string.Join("  |  ", partes) : "Nenhum filtro aplicado";
        }
    }
}