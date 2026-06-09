using System.Windows.Input;
using ReGraphik.Services;

namespace ReGraphik.ViewModels
{
    public class UsuarioViewModel : BaseViewModel
    {
        public string? ImgFoto
        {
            get => UsuarioSessaoService.Instancia.FotoCaminho;
            set
            {
                UsuarioSessaoService.Instancia.FotoCaminho = value;
                OnPropertyChanged();
            }
        }

        private string _nome = string.Empty;
        public string Nome
        {
            get => _nome;
            set { _nome = value; OnPropertyChanged(); }
        }

        private string _cpf = string.Empty;
        public string Cpf
        {
            get => _cpf;
            set { _cpf = value; OnPropertyChanged(); }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
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

        public UsuarioViewModel()
        {
            SelecionarFotoCommand = new RelayCommand(SelecionarFoto);

            // Carrega foto salva do disco ao iniciar
            var fotoSalva = ConfiguracaoLocalService.CarregarFoto();
            if (fotoSalva != null)
                UsuarioSessaoService.Instancia.FotoCaminho = fotoSalva;

            UsuarioSessaoService.Instancia.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UsuarioSessaoService.FotoCaminho))
                    OnPropertyChanged(nameof(ImgFoto));
            };
        }

        private void SelecionarFoto()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagens (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Selecione uma foto de perfil"
            };

            if (dialog.ShowDialog() == true)
            {
                ImgFoto = dialog.FileName;
                // Salva no disco para persistir entre sessões
                ConfiguracaoLocalService.SalvarFoto(dialog.FileName);
            }
        }
    }
}