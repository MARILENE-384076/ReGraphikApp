using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Database;  // ← ADICIONAR esta linha

namespace ReGraphik.Services
{
    /// <summary>
    /// Fornece acesso singleton ao FirebaseClient configurado
    /// para o Realtime Database do ReGraphik.
    /// </summary>
    public static class FirebaseConfigService
    {
        /// URL do Realtime Database do projeto Firebase
        private const string DatabaseUrl =
            "https://regraphikfirebase-default-rtdb.firebaseio.com/";

        private static FirebaseClient? _client;

        /// <summary>
        /// Instancia unica do FirebaseClient reutilizada por toda a aplicacao.
        /// </summary>
        public static FirebaseClient Client =>
            _client ??= new FirebaseClient(DatabaseUrl);
    }
}

