using Microsoft.Win32;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ReGraphik.ViewModels
{
    public class UsuarioViewModel : BaseViewModel
    {
        /// <summary>
        /// A UsuarioViewModel é responsável por gerenciar as informações do usuário logado, permitindo que ele 
        /// visualize e edite seus dados pessoais, como nome, email, CPF, login e senha. 
        /// </summary>
        private readonly IAutorizarService _autorizarService;
        private string _senha;
        private bool _ocupado;

        /// <summary>
        /// Armazena as informações do usuário logado, permitindo que elas sejam acessadas e modificadas na view de conta,
        /// </summary>
        public Usuario UsuarioLogado { get; }

        /// <summary>
        /// Propriedade para vincular a senha digitada pelo usuário, permitindo que ela seja editada e salva corretamente,
        /// </summary>
        public string Senha
        {
            get => _senha;
            set { _senha = value; OnPropertyChanged(); }
        }

        public bool Ocupado
        {
            get => _ocupado;
            set { _ocupado = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comandos para alterar a foto de perfil e salvar as alterações feitas pelo usuário,
        /// </summary>
        public ICommand AlterarFotoCommand { get; }
        public ICommand SalvarAlteracoesCommand { get; }

        /// <summary>
        /// O construtor da UsuarioViewModel recebe um objeto Usuario representando o usuário logado,
        /// </summary>
        /// <param name="usuario"></param>
        public UsuarioViewModel(Usuario usuario)
        {
            UsuarioLogado = usuario;
            _autorizarService = new AutorizarService();

            AlterarFotoCommand = new RelayCommand(async (p) => await AlterarFoto());

            /// O comando de salvar alterações só pode ser executado quando a view não estiver ocupada, evitando múltiplas requisições simultâneas,
            SalvarAlteracoesCommand = new RelayCommand(async (p) => await SalvarAlteracoes(), (p) => !Ocupado);
        }

        /// <summary>
        /// O método AlterarFoto é responsável por permitir que o usuário selecione uma nova foto de perfil, faça o upload para 
        /// o Firebase Storage e atualize a URL da foto no Realtime Database, garantindo que a nova imagem seja exibida corretamente na aplicação. 
        /// </summary>
        /// <returns></returns>
        private async Task AlterarFoto()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imagens (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                string caminhoLocalImagem = openFileDialog.FileName;

                try
                {
                    Ocupado = true;

                    // 1. Chame o novo serviço do Imgur (Totalmente Grátis)
                    var imgurService = new ImgurApiService();

                    // O Imgur só precisa do caminho do arquivo, ele gera o link na hora!
                    string urlPublicaImgur = await imgurService.EnviarFotoPerfilAsync(caminhoLocalImagem);

                    // 2. Atualiza o objeto do usuário na memória do WPF
                    string fotoAntiga = UsuarioLogado.FotoPerfil;
                    UsuarioLogado.FotoPerfil = urlPublicaImgur;

                    // 3. Envia para o seu backend da Runasp salvar no banco de dados!
                    string senhaAntiga = UsuarioLogado.Senha;
                    UsuarioLogado.Senha = null;

                    bool sucessoApi = await _autorizarService.AtualizarAsync(UsuarioLogado.Id, UsuarioLogado);

                    UsuarioLogado.Senha = senhaAntiga; // Restaura localmente

                    if (sucessoApi)
                    {
                        // 4. Carrega a foto na Dashboard na hora
                        BitmapImage novaFoto = new BitmapImage();
                        novaFoto.BeginInit();
                        novaFoto.UriSource = new Uri(urlPublicaImgur, UriKind.Absolute);
                        novaFoto.CacheOption = BitmapCacheOption.OnLoad;
                        novaFoto.EndInit();
                        novaFoto.Freeze();

                        FotoUserService.Foto = novaFoto;

                        MessageBox.Show("Foto de perfil atualizada com sucesso de forma gratuita!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        UsuarioLogado.FotoPerfil = fotoAntiga; // Desfaz
                        MessageBox.Show("Não foi possível atualizar a foto na sua API.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao processar a foto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Ocupado = false;
                }
            }
        }

        /// <summary>
        /// O método SalvarAlteracoes é responsável por validar os dados editados pelo usuário, enviar as alterações para a 
        /// API e lidar com o feedback de sucesso ou erro, garantindo que as informações do usuário sejam atualizadas corretamente.
        /// </summary>
        /// <returns></returns>
        private async Task SalvarAlteracoes()
        {
            if (string.IsNullOrWhiteSpace(UsuarioLogado.Nome) || string.IsNullOrWhiteSpace(UsuarioLogado.Login))
            {
                MessageBox.Show("Nome e Login são obrigatórios.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string senhaOriginal = UsuarioLogado.Senha; 
            if (!string.IsNullOrWhiteSpace(Senha))
            {
                UsuarioLogado.Senha = Senha;
            }
            else
            {
                UsuarioLogado.Senha = null; 
            }

            try
            {
                Ocupado = true;

                bool sucesso = await _autorizarService.AtualizarAsync(UsuarioLogado.Id, UsuarioLogado);

                if (sucesso)
                {
                    MessageBox.Show("Dados atualizados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    Senha = string.Empty;
                }
                else
                {
                    UsuarioLogado.Senha = senhaOriginal; 
                    MessageBox.Show("Erro ao atualizar os dados.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                UsuarioLogado.Senha = senhaOriginal;
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Ocupado = false;
            }
        }

        /// <summary>
        /// O método AtualizarFotoNoRealtimeDatabaseAsync é responsável por enviar uma requisição PUT para a API REST do Firebase,
        /// </summary>
        /// <param name="usuarioId"></param>
        /// <param name="novaUrlFoto"></param>
        /// <returns></returns>
        private async Task<bool> AtualizarFotoNoRealtimeDatabaseAsync(string usuarioId, string novaUrlFoto)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string urlFirebase = $"https://regraphikfirebase-default-rtdb.firebaseio.com/usuarios/{usuarioId}.json";
                    var dadosAtualizados = new { foto_perfil = novaUrlFoto };
                    string json = JsonSerializer.Serialize(dadosAtualizados);
                    var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(new HttpMethod("PUT"), urlFirebase) { Content = conteudo };
                    var response = await client.SendAsync(request);
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
