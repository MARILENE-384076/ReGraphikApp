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
            _viewModel = new UsuarioViewModel();
            DataContext = _viewModel;

            CarregarDadosNaTela();
            AplicarMascaras();
        }

        private void CarregarDadosNaTela()
        {
            TxtNome.Text = _usuarioAtual.Nome;
            TxtCpf.Text = MascararCpf(_usuarioAtual.CPF);
            TxtEmail.Text = MascararEmail(_usuarioAtual.Email);
            TxtLogin.Text = _usuarioAtual.Login;

            _viewModel.Nome = _usuarioAtual.Nome ?? string.Empty;
            _viewModel.Cpf = _usuarioAtual.CPF ?? string.Empty;
            _viewModel.Email = _usuarioAtual.Email ?? string.Empty;
            _viewModel.Login = _usuarioAtual.Login ?? string.Empty;
        }

        private void AplicarMascaras()
        {
            // Ao focar: mostra valor real
            TxtCpf.GotFocus += (s, e) => TxtCpf.Text = _usuarioAtual.CPF ?? string.Empty;
            TxtEmail.GotFocus += (s, e) => TxtEmail.Text = _usuarioAtual.Email ?? string.Empty;

            // Ao perder foco: volta a mascarar
            TxtCpf.LostFocus += (s, e) =>
            {
                _usuarioAtual.CPF = TxtCpf.Text;
                TxtCpf.Text = MascararCpf(TxtCpf.Text);
            };
            TxtEmail.LostFocus += (s, e) =>
            {
                _usuarioAtual.Email = TxtEmail.Text;
                TxtEmail.Text = MascararEmail(TxtEmail.Text);
            };
        }

        // CPF: 123.456.789-10 → 123.***.***-**
        private static string MascararCpf(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return string.Empty;
            var digits = System.Text.RegularExpressions.Regex.Replace(cpf, @"\D", "");
            if (digits.Length >= 3)
                return digits[..3] + ".***.***-**";
            return cpf;
        }

        // Email: teste@gmail.com → te***@gmail.com
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
                MessageBox.Show("Nome e Login são obrigatórios.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _usuarioAtual.Nome = TxtNome.Text;
            _usuarioAtual.Login = TxtLogin.Text;
            // CPF e Email já foram atualizados no LostFocus

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
                    // Reaplica máscaras após salvar
                    TxtCpf.Text = MascararCpf(_usuarioAtual.CPF);
                    TxtEmail.Text = MascararEmail(_usuarioAtual.Email);
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