using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using ReGraphik.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ReGraphik.Views.UserControls
{
    public partial class ContaView : UserControl
    {
        private Usuario _usuarioAtual;
        private readonly IAutorizarService _autorizarService;
        private readonly UsuarioViewModel _viewModel;

        public ContaView(Usuario usuario)
        {
            InitializeComponent();

            _usuarioAtual = usuario;
            _autorizarService = new AutorizarService();

            // Instancia o ViewModel e define como DataContext
            // (isso faz o botão Mudar Foto e a foto funcionarem)
            _viewModel = new UsuarioViewModel();
            DataContext = _viewModel;

            CarregarDadosNaTela();
        }

        private void CarregarDadosNaTela()
        {
            // Preenche tanto a tela quanto o ViewModel
            TxtNome.Text = _usuarioAtual.Nome;
            TxtCpf.Text = _usuarioAtual.CPF;
            TxtEmail.Text = _usuarioAtual.Email;
            TxtLogin.Text = _usuarioAtual.Login;

            _viewModel.Nome = _usuarioAtual.Nome ?? string.Empty;
            _viewModel.Cpf = _usuarioAtual.CPF ?? string.Empty;
            _viewModel.Email = _usuarioAtual.Email ?? string.Empty;
            _viewModel.Login = _usuarioAtual.Login ?? string.Empty;
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
                _usuarioAtual.Senha = TxtSenha.Password;
            else
                _usuarioAtual.Senha = null;

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
                    MessageBox.Show("Erro ao atualizar os dados.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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