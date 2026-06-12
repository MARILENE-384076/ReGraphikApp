using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    public class ContaViewModel : BaseViewModel
    {
        private Usuario _usuarioAtual;
        private readonly IAutorizarService _autorizarService;
        private readonly UsuarioViewModel _viewModel;
        private string _emailReal = string.Empty;

        private string? _nome;
        public string Nome
        {
            get => _nome;
            set { _nome = value; OnPropertyChanged(); }
        }

        private string? _cpf;
        public string CPF
        {
            get => _cpf;
            set { _cpf = value; OnPropertyChanged(); }
        }

        private string? _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string? _login;
        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        private PasswordBox? _senha;
        public PasswordBox Senha
        {
            get => _senha;
            set { _senha = value; OnPropertyChanged(); }
        }

        private bool _ocupado;
        public bool Ocupado
        {
            get => _ocupado;
            set { _ocupado = value; OnPropertyChanged(); }
        }

        public ICommand SalvarCommand;
        public ICommand EmailGotFocusCommand { get; }
        public ICommand EmailLostFocusCommand { get; }
        public ICommand SelecionarFotoCommand { get; }

        public ContaViewModel(Usuario usuario)
        {
            _usuarioAtual = usuario;
            _autorizarService = new AutorizarService();
            _viewModel = new UsuarioViewModel();

            CarregarDadosNaTela();
            SalvarCommand = new RelayCommand(async (param) => await SalvarPerfilAsync(param));

            EmailGotFocusCommand = new RelayCommand(EmailGotFocus);
            EmailLostFocusCommand = new RelayCommand(EmailLostFocus);
            SelecionarFotoCommand = new RelayCommand((_) => MudarFoto());
        }

        private void CarregarDadosNaTela()
        {
            Nome = _usuarioAtual.Nome ?? string.Empty;
            Login = _usuarioAtual.Login ?? string.Empty;

            // CPF: mascarado e bloqueado
            CPF = MascararCpf(_usuarioAtual.CPF);

            // Email: mascarado mas editável
            _emailReal = _usuarioAtual.Email ?? string.Empty;
            Email = MascararEmail(_emailReal);
        }

        // ── Email: mostra real ao focar, mascara ao sair ─────────
        public void EmailGotFocus()
        {
            Email = _emailReal;
        }
           

        public void EmailLostFocus()
        {
            _emailReal = Email;
            _usuarioAtual.Email = _emailReal;
            Email = MascararEmail(_emailReal);
        }
        private void MudarFoto()
        {
            // Lógica para abrir OpenFileDialog e definir ImgFoto
            MessageBox.Show("Abrir seletor de arquivo...");
        }

        // ── Máscaras ─────────────────────────────────────────────
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

        // ── Salvar ───────────────────────────────────────────────
        private async Task SalvarPerfilAsync(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Login))
            {
                MessageBox.Show("Nome e Login são obrigatórios.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string novaSenha = string.Empty;
            if (parameter is PasswordBox passwordBox)
            {
                novaSenha = passwordBox.Password;
            }

            _usuarioAtual.Nome = Nome;
            _usuarioAtual.Login = Login;
            _usuarioAtual.Email = _emailReal;
            _usuarioAtual.Nome = Nome;
            _usuarioAtual.Login = Login;
            _usuarioAtual.Email = _emailReal;

            try
            {
                Ocupado = true;

                bool sucesso = await _autorizarService.AtualizarAsync(_usuarioAtual.Id, _usuarioAtual);

                if (sucesso)
                {
                    MessageBox.Show("Dados atualizados com sucesso!", "Sucesso",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Email = MascararEmail(_emailReal);
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
                Ocupado = false;
            }
        }
    }
}
