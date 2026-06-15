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