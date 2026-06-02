using System;
using System.Windows;
using System.Windows.Controls;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;

namespace ReGraphik.Views.Pages
{
    /// <summary>
    /// Interação lógica para ContaPage.xaml
    /// </summary>
    public partial class ContaPage : Page
    {
        private Usuario _usuarioAtual;
        private readonly IAutorizarService _autorizarService;

        public ContaPage(Usuario usuario)
        {
            InitializeComponent();

            _usuarioAtual = usuario;
            _autorizarService = new AutorizarService();

            CarregarDadosNaTela();
        }

        private void CarregarDadosNaTela()
        {
            TxtNome.Text = _usuarioAtual.Nome;
            TxtCpf.Text = _usuarioAtual.CPF; 
            TxtEmail.Text = _usuarioAtual.Email;
            TxtLogin.Text = _usuarioAtual.Login;
        }

        private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNome.Text) || string.IsNullOrWhiteSpace(TxtLogin.Text))
            {
                MessageBox.Show("Nome e Login são obrigatórios.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _usuarioAtual.Nome = TxtNome.Text;
            _usuarioAtual.CPF = TxtCpf.Text; 
            _usuarioAtual.Email = TxtEmail.Text;
            _usuarioAtual.Login = TxtLogin.Text;

            if (!string.IsNullOrWhiteSpace(TxtSenha.Password))
            {
                _usuarioAtual.Senha = TxtSenha.Password;
            }

            try
            {
                BtnSalvar.IsEnabled = false;
                BtnSalvar.Content = "Salvando...";

                bool sucesso = await _autorizarService.AtualizarAsync(_usuarioAtual.Id, _usuarioAtual);

                if (sucesso)
                {
                    MessageBox.Show("Dados atualizados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    TxtSenha.Clear();
                }
                else
                {
                    MessageBox.Show("Erro ao atualizar os dados. Verifique sua conexão.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnSalvar.IsEnabled = true;
                BtnSalvar.Content = "Salvar Alterações";
            }
        }
    }
}