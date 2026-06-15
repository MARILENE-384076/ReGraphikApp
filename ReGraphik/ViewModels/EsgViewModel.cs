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
            
            // Guia de Certificação
            doc.Blocks.Add(Paragrafo("🏅  Como obter o Certificado ReGraphik ESG",
                14, FontWeights.Bold, "#B45309"));
            doc.Blocks.Add(Paragrafo(
                "O certificado é emitido para empresas parceiras que atingem critérios mínimos de " +
                "engajamento com a plataforma e comprovam destinação sustentável dos seus resíduos.",
                12, FontWeights.Normal, "#1E293B"));

            var lista = new List
            {
                MarkerStyle = TextMarkerStyle.Decimal,
                Padding     = new Thickness(20, 0, 0, 0)
            };
            lista.ListItems.Add(ItemLista("Cadastre sua empresa e realize o login na plataforma ReGraphik."));
            lista.ListItems.Add(ItemLista("Registre todos os resíduos gerados mensalmente em \"Cadastrar Resíduos\", informando tipo, quantidade e condição."));
            lista.ListItems.Add(ItemLista("No Estoque Reverso, aplique ao menos uma sugestão de reaproveitamento a cada lote de resíduos."));
            lista.ListItems.Add(ItemLista("Localize um ponto de coleta homologado no Mapa e registre a destinação do resíduo."));
            lista.ListItems.Add(ItemLista("Após 3 meses consecutivos de uso ativo, acesse Relatórios e gere o comprovante de movimentação."));
            lista.ListItems.Add(ItemLista("Envie o relatório para certificacao@regraphik.com.br com CNPJ e razão social da empresa."));
            lista.ListItems.Add(ItemLista("Nossa equipe avalia o dossiê em até 10 dias úteis e emite o Certificado ReGraphik ESG digitalmente."));
            doc.Blocks.Add(lista);

            doc.Blocks.Add(Separador());
            doc.Blocks.Add(Paragrafo(
                "O Certificado ReGraphik ESG pode ser utilizado em materiais de comunicação, licitações " +
                "públicas, relatórios de sustentabilidade (GRI, CDP) e processos de due diligence ambiental.",
                11, FontWeights.Normal, "#475569"));
            doc.Blocks.Add(Separador());
            doc.Blocks.Add(Paragrafo(
                "ReGraphik — Gestão de Estoque Reverso para o Setor Gráfico · Desenvolvido por alunos do SENAI",
                10, FontWeights.Normal, "#94A3B8"));

            return doc;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static Paragraph Paragrafo(string texto, double tamanho, FontWeight peso, string hex)
        {
            return new Paragraph(new Run(texto))
            {
                FontSize   = tamanho,
                FontWeight = peso,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)),
                Margin     = new Thickness(0, 4, 0, 8)
            };
        }

        private static BlockUIContainer Separador()
        {
            return new BlockUIContainer(new System.Windows.Shapes.Rectangle
            {
                Height = 1,
                Fill   = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                Margin = new Thickness(0, 8, 0, 12)
            });
        }

        private static ListItem ItemLista(string texto)
        {
            return new ListItem(new Paragraph(new Run(texto))
            {
                FontSize   = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                Margin     = new Thickness(0, 2, 0, 4)
            });
        }
    }
}