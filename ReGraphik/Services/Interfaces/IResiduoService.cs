using ReGraphik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReGraphik.Services.Interface
{
    // Esta interface define os métodos que a classe ResiduoService deve implementar para
    // lidar com as operações relacionadas aos resíduos, como obter a lista de resíduos do banco de dados.
    internal interface IResiduoService
    {
        Task<List<Residuo>> ObterTodosResiduosAsync();
    }
}
