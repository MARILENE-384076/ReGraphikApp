using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReGraphik.Models;
using ReGraphik.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel da tela de Proposta de Valor ESG.
    /// Exibe os pilares Ambiental, Social e Governança, o guia de certificação
    /// e permite exportar o relatório institucional em PDF moderno via QuestPDF.
    /// </summary>
    public class EsgViewModel : BaseViewModel
    {
        private readonly Usuario _usuario;

        public ICommand ExportarPdfCommand { get; }
        public ICommand IrParaRelatoriosCommand { get; }

        public EsgViewModel(Usuario usuario, ICommand irParaRelatorios)
        {
            _usuario = usuario;
            ExportarPdfCommand = new RelayCommand(() => ExportarPdf());
            IrParaRelatoriosCommand = irParaRelatorios;
        }

        /// <summary>
        /// Gera o documento institucional de Proposta de Valor ESG formatado em PDF de alta qualidade.
        /// </summary>
        private void ExportarPdf()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Salvar Proposta de Valor ESG",
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"Proposta_Valor_ESG_ReGraphik_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                DefaultExt = ".pdf"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                var caminho = dialog.FileName;

                /// Definição da Paleta de Cores Oficial do Sistema
                const string azulEscuro = "#0D2A56";
                const string azulMedio = "#1649A2";
                const string verdeEsg = "#137333";
                const string roxoEsg = "#6B21A8";
                const string laranjaEsg = "#B45309";
                const string cinzaTexto = "#1E293B";
                const string cinzaSubtitulo = "#475569";
                const string cinzaBorda = "#E2G2F0";
                const string cinzaLeve = "#94A3B8";

                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4); /// Formato Retrato para leitura institucional
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11).FontColor(cinzaTexto));

                        /// Cabeçalho Institucional
                        page.Header().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("ReGraphik").FontSize(24).Bold().FontColor(azulEscuro);
                                    c.Item().Text("Plataforma de Gestão de Estoque Reverso e Indicadores ESG").FontSize(10).FontColor(azulMedio);
                                });
                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text("DIRETRIZES & PROPOSTA ESG").FontSize(12).Bold().FontColor(verdeEsg);
                                    c.Item().Text($"Emitido em: {DateTime.Now:dd/MM/yyyy}").FontSize(9).FontColor(cinzaLeve);
                                    c.Item().Text($"Empresa: {_usuario?.Nome ?? "Parceira Homologada"}").FontSize(9).FontColor(cinzaLeve);
                                });
                            });
                            col.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(verdeEsg);
                        });

                        /// Conteúdo do Manifesto ESG
                        page.Content().PaddingTop(20).Column(col =>
                        {
                            col.Item().Text("REGRAPHIK — Relatório de Proposta de Valor ESG").FontSize(16).Bold().FontColor(azulMedio);
                            col.Item().PaddingTop(15);

                            /// Pilar E - Ambiental
                            col.Item().Text("🌿  E — Ambiental (Environmental)").FontSize(13).Bold().FontColor(verdeEsg);
                            col.Item().PaddingTop(4).Text(
                                "O ReGraphik atua diretamente na redução do impacto ambiental gerado pelo setor gráfico. " +
                                "Ao cadastrar resíduos como papel, cartão e vinil, as empresas parceiras transformam " +
                                "descarte em matéria-prima circular, reduzindo a quantidade de material enviado a aterros " +
                                "e diminuindo emissões de CO₂ associadas ao descarte inadequado.").Justify();
                            col.Item().PaddingTop(4).Text("Indicadores monitorados: kg de resíduos cadastrados · pontos de coleta ativados · estimativa de CO₂ evitado por tonelada reciclada.").FontSize(10).Italic().FontColor(cinzaSubtitulo);

                            col.Item().PaddingVertical(10).LineHorizontal(0.5f).LineColor("#E2E8F0");

                            /// Pilar S - Social
                            col.Item().Text("🤝  S — Social").FontSize(13).Bold().FontColor(azulMedio);
                            col.Item().PaddingTop(4).Text(
                                "A plataforma conecta empresas gráficas a cooperativas e coletores, gerando renda " +
                                "para trabalhadores da cadeia de reciclagem. O sistema de sugestões de reaproveitamento " +
                                "permite que resíduos virem produtos personalizados — como camisetas e brindes — " +
                                "criando valor para toda a cadeia produtiva.").Justify();
                            col.Item().PaddingTop(4).Text("Benefícios: geração de renda para catadores · fomento à economia circular local · redução de custos operacionais de descarte para as empresas parceiras.").FontSize(10).Italic().FontColor(cinzaSubtitulo);

                            col.Item().PaddingVertical(10).LineHorizontal(0.5f).LineColor("#E2E8F0");

                            /// Pilar G - Governança
                            col.Item().Text("📋  G — Governança").FontSize(13).Bold().FontColor(roxoEsg);
                            col.Item().PaddingTop(4).Text(
                                "O ReGraphik oferece rastreabilidade completa dos resíduos gerados: origem, tipo, " +
                                "quantidade, condição e destino final ficam registrados e auditáveis. Isso permite " +
                                "que as empresas demonstrem conformidade com a Política Nacional de Resíduos Sólidos " +
                                "(Lei 12.305/2010) e fortaleçam sua governança ambiental corporativa.").Justify();
                            col.Item().PaddingTop(4).Text("Diferenciais: histórico auditável · relatórios exportáveis · dados para laudos e prestação de contas a órgãos reguladores.").FontSize(10).Italic().FontColor(cinzaSubtitulo);

                            col.Item().PaddingVertical(15).LineHorizontal(1f).LineColor(laranjaEsg);

                            /// Guia de Certificação
                            col.Item().Text("🏅  Como obter o Certificado ReGraphik ESG").FontSize(14).Bold().FontColor(laranjaEsg);
                            col.Item().PaddingTop(4).Text("O certificado é emitido para empresas parceiras que atingem critérios mínimos de engajamento com a plataforma e comprovam destinação sustentável dos seus resíduos.").Justify();
                            col.Item().PaddingTop(8);

                            /// Passos da Certificação em formato de lista limpa
                            string[] passos = {
                                "1. Cadastre sua empresa e realize o login na plataforma ReGraphik.",
                                "2. Registre todos os resíduos gerados mensalmente em \"Cadastrar Resíduos\", informando tipo, quantidade e condição.",
                                "3. No Estoque Reverso, aplique ao menos uma sugestão de reaproveitamento a cada lote de resíduos.",
                                "4. Localize um ponto de coleta homologado no Mapa e registre a destinação do resíduo.",
                                "5. Após 3 meses consecutivos de uso ativo, acesse Relatórios e gere o comprovante de movimentação.",
                                "6. Envie o relatório para certificacao@regraphik.com.br com CNPJ e razão social da empresa.",
                                "7. Nossa equipe avalia o dossiê em até 10 dias úteis e emite o Certificado ReGraphik ESG digitalmente."
                            };

                            foreach (var passo in passos)
                            {
                                col.Item().PaddingBottom(3).Text(passo).FontSize(10);
                            }

                            col.Item().PaddingVertical(12).LineHorizontal(0.5f).LineColor("#E2E8F0");

                            col.Item().Text("O Certificado ReGraphik ESG pode ser utilizado em materiais de comunicação, licitações públicas, relatórios de sustentabilidade (GRI, CDP) e processos de due diligence ambiental.").FontSize(10).Italic().FontColor(cinzaSubtitulo);
                        });

                        /// Rodapé Técnico
                        page.Footer().Column(fcol =>
                        {
                            fcol.Item().LineHorizontal(1).LineColor("#CBD5E1");
                            fcol.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("ReGraphik — Gestão de Estoque Reverso · Trabalho de Conclusão de Curso").FontSize(8).FontColor(cinzaLeve);
                                row.RelativeItem().AlignRight().Text(t =>
                                {
                                    t.Span("Página ").FontSize(8).FontColor(cinzaLeve);
                                    t.CurrentPageNumber().FontSize(8).FontColor(cinzaLeve);
                                    t.Span(" de ").FontSize(8).FontColor(cinzaLeve);
                                    t.TotalPages().FontSize(8).FontColor(cinzaLeve);
                                });
                            });
                        });
                    });
                }).GeneratePdf(caminho);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    bool? abrir = MensagemPdfWindow.Exibir(
                        "Exportação Concluída",
                        $"Relatório ESG em PDF gerado com sucesso!\n\nSalvo em: {caminho}\n\nDeseja abrir o arquivo agora?",
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
    }
}