using ReGraphik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReGraphik.Services.Interface
{
    /// <summary>
    /// Interface que define os métodos para o serviço de resíduos, incluindo a obtenção de todos os resíduos.
    /// </summary>
    internal interface IResiduoService
    {
        Task<List<Residuo>> ObterTodosResiduosAsync();
    }
}
