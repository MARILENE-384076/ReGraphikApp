using ReGraphik.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel da tela de Proposta de Valor ESG.
    /// Exibe os pilares Ambiental, Social e Governança, o guia de certificação
    /// e permite exportar o relatório em PDF via impressão.
    /// </summary>
    public class EsgViewModel : BaseViewModel
    {
        /// <summary>
        /// Usuário logado, usado para personalizar o relatório exportado.
        /// </summary>
        private readonly Usuario _usuario;

        /// <summary>
        /// Exporta o conteúdo ESG como documento PDF imprimível.
        /// </summary>
        public ICommand ExportarPdfCommand { get; }

        /// <summary>
        /// Inicializa o ViewModel com o usuário logado.
        /// </summary>
        public EsgViewModel(Usuario usuario)
        {
            _usuario = usuario;
            ExportarPdfCommand = new RelayCommand(() => ExportarPdf());
        }
        /// <summary>
        /// Gera um FlowDocument com o conteúdo ESG e abre o diálogo de impressão,
        /// permitindo salvar como PDF ou imprimir fisicamente.
        /// </summary>
        private void ExportarPdf()
        {
            try
            {
                var doc = CriarDocumento();
                var dlg = new PrintDialog();
                if (dlg.ShowDialog() != true) return;

                IDocumentPaginatorSource idoc = doc;
                dlg.PrintDocument(idoc.DocumentPaginator, "ReGraphik — Relatório ESG");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao exportar: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Monta o FlowDocument com todos os pilares ESG e o guia de certificação.
        /// </summary>
        private FlowDocument CriarDocumento()
        {
            var doc = new FlowDocument
            {
                FontFamily  = new FontFamily("Segoe UI"),
                FontSize    = 12,
                PagePadding = new Thickness(60, 48, 60, 48),
                ColumnWidth = double.MaxValue
            };
            // Cabeçalho
            doc.Blocks.Add(Paragrafo("REGRAPHIK — Relatório de Proposta de Valor ESG",
                16, FontWeights.Bold, "#1649a2"));
            doc.Blocks.Add(Paragrafo(
                $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}   |   Empresa: {_usuario?.Nome ?? "—"}",
                10, FontWeights.Normal, "#64748B"));
            doc.Blocks.Add(Separador());
            // E — Ambiental
            doc.Blocks.Add(Paragrafo("🌿  E — Ambiental (Environmental)",
                14, FontWeights.Bold, "#166534"));
            doc.Blocks.Add(Paragrafo(
                "O ReGraphik atua diretamente na redução do impacto ambiental gerado pelo setor gráfico. " +
                "Ao cadastrar resíduos como papel, cartão e vinil, as empresas parceiras transformam " +
                "descarte em matéria-prima circular, reduzindo a quantidade de material enviado a aterros " +
                "e diminuindo emissões de CO₂ associadas ao descarte inadequado.",
                12, FontWeights.Normal, "#1E293B"));
            doc.Blocks.Add(Paragrafo(
                "Indicadores monitorados: kg de resíduos cadastrados · pontos de coleta ativados · " +
                "estimativa de CO₂ evitado por tonelada reciclada.",
                11, FontWeights.Normal, "#475569"));
            doc.Blocks.Add(Separador());

            // S — Social
            doc.Blocks.Add(Paragrafo("🤝  S — Social",
                14, FontWeights.Bold, "#1D4ED8"));
            doc.Blocks.Add(Paragrafo(
                "A plataforma conecta empresas gráficas a cooperativas e coletores, gerando renda " +
                "para trabalhadores da cadeia de reciclagem. O sistema de sugestões de reaproveitamento " +
                "permite que resíduos virem produtos personalizados — como camisetas e brindes — " +
                "criando valor para toda a cadeia produtiva.",
                12, FontWeights.Normal, "#1E293B"));
            doc.Blocks.Add(Paragrafo(
                "Benefícios: geração de renda para catadores · fomento à economia circular local · " +
                "redução de custos operacionais de descarte para as empresas parceiras.",
                11, FontWeights.Normal, "#475569"));
            doc.Blocks.Add(Separador());

            // G — Governança
            doc.Blocks.Add(Paragrafo("📋  G — Governança",
                14, FontWeights.Bold, "#6B21A8"));
            doc.Blocks.Add(Paragrafo(
                "O ReGraphik oferece rastreabilidade completa dos resíduos gerados: origem, tipo, " +
                "quantidade, condição e destino final ficam registrados e auditáveis. Isso permite " +
                "que as empresas demonstrem conformidade com a Política Nacional de Resíduos Sólidos " +
                "(Lei 12.305/2010) e fortaleçam sua governança ambiental corporativa.",
                12, FontWeights.Normal, "#1E293B"));
            doc.Blocks.Add(Paragrafo(
                "Diferenciais: histórico auditável · relatórios exportáveis · dados para laudos e " +
                "prestação de contas a órgãos reguladores.",
                11, FontWeights.Normal, "#475569"));
            doc.Blocks.Add(Separador());