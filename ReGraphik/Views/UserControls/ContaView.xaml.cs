using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReGraphik.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ContaView.xaml
    /// </summary>
    public partial class ContaView : UserControl
    {
        private Usuario _usuarioAtual;
        private readonly IAutorizarService _autorizarService;

        public ContaView(Usuario usuario)
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

            /// Atualiza os dados comuns
            _usuarioAtual.Nome = TxtNome.Text;
            _usuarioAtual.CPF = TxtCpf.Text;
            _usuarioAtual.Email = TxtEmail.Text;
            _usuarioAtual.Login = TxtLogin.Text;

            /// Se o usuário digitou algo na senha, enviamos a nova senha
            if (!string.IsNullOrWhiteSpace(TxtSenha.Password))
            {
                _usuarioAtual.Senha = TxtSenha.Password;
            }
            else
            {
                /// Deixamos como null para a API não duplicar no Firebase
                _usuarioAtual.Senha = null;
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
