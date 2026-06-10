using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using ReGraphik.ViewModels;
using System;
using System.Text.RegularExpressions;
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
            _viewModel = new UsuarioViewModel();
            DataContext = _viewModel;

            CarregarDadosNaTela();
        }

        private void CarregarDadosNaTela()
        {
            TxtNome.Text = _usuarioAtual.Nome ?? string.Empty;
            TxtLogin.Text = _usuarioAtual.Login ?? string.Empty;

            TxtCpf.Text = MascararCpf(_usuarioAtual.CPF);
            TxtEmail.Text = MascararEmail(_usuarioAtual.Email);
        }

       
        private void TxtCpf_GotFocus(object sender, RoutedEventArgs e)
            => TxtCpf.Text = _usuarioAtual.CPF ?? string.Empty;

        private void TxtCpf_LostFocus(object sender, RoutedEventArgs e)
        {
            _usuarioAtual.CPF = TxtCpf.Text;
            TxtCpf.Text = MascararCpf(TxtCpf.Text);
        }

       
        private void TxtEmail_GotFocus(object sender, RoutedEventArgs e)
            => TxtEmail.Text = _usuarioAtual.Email ?? string.Empty;

        private void TxtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            _usuarioAtual.Email = TxtEmail.Text;
            TxtEmail.Text = MascararEmail(TxtEmail.Text);
        }

       
        private static string MascararCpf(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return string.Empty;
            var d = Regex.Replace(cpf, @"\D", "");
            return d.Length >= 3 ? d[..3] + ".***.***-**" : cpf;
        }

       
        private static string MascararEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;
            var at = email.IndexOf('@');
            if (at <= 2) return email;
            return email[..2] + new string('*', at - 2) + email[at..];
        }

        
        private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNome.Text) || string.IsNullOrWhiteSpace(TxtLogin.Text))
            {
                MessageBox.Show("Nome e Login são obrigatórios.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _usuarioAtual.Nome = TxtNome.Text;
            _usuarioAtual.Login = TxtLogin.Text;
            // CPF e Email já atualizados no LostFocus

            _usuarioAtual.Senha = !string.IsNullOrWhiteSpace(TxtSenha.Password)
                ? TxtSenha.Password
                : null;

            try
            {
                BtnSalvar.IsEnabled = false;
                BtnSalvar.Content = "Salvando...";

                bool sucesso = await _autorizarService.AtualizarAsync(_usuarioAtual.Id, _usuarioAtual);

                if (sucesso)
                {
                    MessageBox.Show("Dados atualizados com sucesso!", "Sucesso",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    TxtSenha.Clear();
                    // Reaplica máscaras após salvar
                    TxtCpf.Text = MascararCpf(_usuarioAtual.CPF);
                    TxtEmail.Text = MascararEmail(_usuarioAtual.Email);
                }
                else
                {
                    MessageBox.Show("Erro ao atualizar os dados.", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnSalvar.IsEnabled = true;
                BtnSalvar.Content = "Salvar Alterações";
            }
        }
    }
}