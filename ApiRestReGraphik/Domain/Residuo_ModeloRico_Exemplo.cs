// ============================================================================
// EXEMPLO DE CODIGO RICO — Agregado Residuo
// ReGraphik — do modelo anemico atual para um modelo rico (DDD tatico)
// ============================================================================
//
// Este arquivo mostra dois estilos lado a lado:
//   1) ANTES — como o Residuo esta implementado hoje na API (anemico)
//   2) DEPOIS — como ficaria seguindo o modelo de dominio (rico)
//
// O objetivo NAO e substituir o arquivo Models/Residuo.cs diretamente —
// e servir de referencia para o grupo decidir se/como aplicar esse padrao
// nos outros agregados (Usuario, Sugestao, Conversa, etc.)
// ============================================================================

using System;
using System.Collections.Generic;


// ============================================================================
// 1) ANTES — Modelo Anemico (como esta implementado hoje)
// ============================================================================
// Characteristics: so tem propriedades com get/set publicos. Qualquer camada
// do sistema (Controller, ViewModel, teste, ate um bug de digitacao) pode
// alterar o Status livremente, sem nenhuma garantia de que RN-11 sera respeitada.

namespace ReGraphik.Models.Antes
{
    public class Residuo
    {
        public string Id { get; set; }
        public string IdUsuario { get; set; }
        public string TipoResiduo { get; set; }
        public string Especificacao { get; set; }
        public string Origem { get; set; }
        public string Projeto { get; set; }
        public double Quantidade { get; set; }
        public DateTime DataCadastro { get; set; }
        public string Condicao { get; set; }
        public string Status { get; set; } // string livre: "EmEstoque", "Reaproveitado", "Descartado"...
    }

    // Em algum lugar do Controller, isso e perfeitamente possivel hoje:
    //
    //   residuo.Status = "Reaproveitado";
    //   ... depois, em outro lugar do codigo, sem querer:
    //   residuo.Status = "EmEstoque"; // RN-11 violada, ninguem percebe
    //
    // Nao ha nada no compilador nem em tempo de execucao que impeça isso.
}


// ============================================================================
// 2) DEPOIS — Modelo Rico (aplicando o modelo de dominio)
// ============================================================================

namespace ReGraphik.Domain.Residuos
{
    // ------------------------------------------------------------------------
    // Value Object: Status do Residuo como enum fechado, nao mais string livre
    // ------------------------------------------------------------------------
    public enum StatusResiduo
    {
        EmEstoque,
        Reaproveitado,
        Descartado
    }

    // ------------------------------------------------------------------------
    // Excecao de dominio: torna explicito que a violacao e uma regra de
    // negocio, nao um erro tecnico generico (facilita tratamento no Controller)
    // ------------------------------------------------------------------------
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }

    // ------------------------------------------------------------------------
    // Domain Events — registram fatos que aconteceram no dominio.
    // Sao objetos imutaveis (record), sempre no passado.
    // ------------------------------------------------------------------------
    public abstract record DomainEvent(DateTime OcorridoEm);

    public record ResiduoCadastradoEvent(string ResiduoId, string UsuarioId) : DomainEvent(DateTime.UtcNow);
    public record ResiduoReaproveitadoEvent(string ResiduoId) : DomainEvent(DateTime.UtcNow);
    public record ResiduoDescartadoEvent(string ResiduoId) : DomainEvent(DateTime.UtcNow);
    public record SugestaoAplicadaEvent(string ResiduoId, string SugestaoId) : DomainEvent(DateTime.UtcNow);
    public record ResiduoExcluidoEvent(string ResiduoId, string ExecutadoPorUsuarioId) : DomainEvent(DateTime.UtcNow);

    // ------------------------------------------------------------------------
    // Classe base simples para agregados que precisam emitir Domain Events.
    // Os eventos ficam acumulados ate a camada de aplicacao decidir publica-los
    // (ex: apos salvar no Firebase com sucesso).
    // ------------------------------------------------------------------------
    public abstract class AggregateRoot
    {
        private readonly List<DomainEvent> _eventos = new();
        public IReadOnlyCollection<DomainEvent> EventosDeDominio => _eventos.AsReadOnly();

        protected void Registrar(DomainEvent evento) => _eventos.Add(evento);
        public void LimparEventos() => _eventos.Clear();
    }

    // ------------------------------------------------------------------------
    // Entidade interna: AplicacaoSugestao (associacao Residuo <-> Sugestao)
    // ------------------------------------------------------------------------
    public class AplicacaoSugestao
    {
        public string Id { get; }
        public string SugestaoId { get; }
        public DateTime DataAplicacao { get; }

        public AplicacaoSugestao(string sugestaoId)
        {
            Id = Guid.NewGuid().ToString();
            SugestaoId = sugestaoId;
            DataAplicacao = DateTime.UtcNow;
        }
    }

    // ------------------------------------------------------------------------
    // Sugestao — versao simplificada, so o necessario para o exemplo
    // ------------------------------------------------------------------------
    public class Sugestao
    {
        public string Id { get; }
        public string TipoResiduoAceito { get; }
        public string DescricaoSugestao { get; }

        public Sugestao(string id, string tipoResiduoAceito, string descricaoSugestao)
        {
            Id = id;
            TipoResiduoAceito = tipoResiduoAceito;
            DescricaoSugestao = descricaoSugestao;
        }

        // RN-16: correspondencia parcial e bidirecional de texto
        public bool EhCompativelCom(string tipoResiduo)
        {
            if (string.IsNullOrWhiteSpace(tipoResiduo) || string.IsNullOrWhiteSpace(TipoResiduoAceito))
                return false;

            return TipoResiduoAceito.Contains(tipoResiduo, StringComparison.OrdinalIgnoreCase)
                || tipoResiduo.Contains(TipoResiduoAceito, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ------------------------------------------------------------------------
    // RESIDUO — Aggregate Root rico
    // ------------------------------------------------------------------------
    public class Residuo : AggregateRoot
    {
        public string Id { get; }
        public string IdUsuario { get; }
        public string TipoResiduo { get; }
        public string Especificacao { get; }
        public string Origem { get; }
        public string Projeto { get; }
        public double Quantidade { get; }
        public DateTime DataCadastro { get; }
        public string Condicao { get; private set; }
        public StatusResiduo Status { get; private set; }

        private readonly List<AplicacaoSugestao> _sugestoesAplicadas = new();
        public IReadOnlyCollection<AplicacaoSugestao> SugestoesAplicadas => _sugestoesAplicadas.AsReadOnly();

        // Construtor privado: forca a criacao via factory method Cadastrar(),
        // garantindo que nenhum Residuo nasca em estado invalido.
        private Residuo(
            string id, string idUsuario, string tipoResiduo, string especificacao,
            string origem, string projeto, double quantidade, string condicao)
        {
            Id = id;
            IdUsuario = idUsuario;
            TipoResiduo = tipoResiduo;
            Especificacao = especificacao;
            Origem = origem;
            Projeto = projeto;
            Quantidade = quantidade;
            Condicao = condicao;
            DataCadastro = DateTime.UtcNow;
            Status = StatusResiduo.EmEstoque; // RN-11: todo residuo nasce EmEstoque
        }

        // ---- Factory method (RN-09, RN-10: campos obrigatorios) ----
        public static Residuo Cadastrar(
            string idUsuario, string tipoResiduo, string especificacao,
            string origem, string projeto, double quantidade, string condicao)
        {
            if (string.IsNullOrWhiteSpace(idUsuario))
                throw new DomainException("Residuo precisa estar vinculado a um usuario.");
            if (string.IsNullOrWhiteSpace(tipoResiduo))
                throw new DomainException("Tipo de residuo e obrigatorio (RN-09).");
            if (quantidade <= 0)
                throw new DomainException("Quantidade deve ser maior que zero.");

            var residuo = new Residuo(
                id: Guid.NewGuid().ToString(),
                idUsuario, tipoResiduo, especificacao, origem, projeto, quantidade, condicao);

            residuo.Registrar(new ResiduoCadastradoEvent(residuo.Id, idUsuario));
            return residuo;
        }

        // ---- RN-11: EmEstoque -> Reaproveitado (estado final) ----
        public void Reaproveitar(string usuarioExecutorId)
        {
            GarantirAutorizacao(usuarioExecutorId);

            if (Status != StatusResiduo.EmEstoque)
                throw new DomainException(
                    $"So e possivel reaproveitar um residuo que esta EmEstoque. Status atual: {Status}.");

            Status = StatusResiduo.Reaproveitado;
            Registrar(new ResiduoReaproveitadoEvent(Id));
        }

        // ---- RN-11: EmEstoque -> Descartado (estado final) ----
        public void Descartar(string usuarioExecutorId)
        {
            GarantirAutorizacao(usuarioExecutorId);

            if (Status != StatusResiduo.EmEstoque)
                throw new DomainException(
                    $"So e possivel descartar um residuo que esta EmEstoque. Status atual: {Status}.");

            Status = StatusResiduo.Descartado;
            Registrar(new ResiduoDescartadoEvent(Id));
        }

        // ---- RN-16/17: aplicar sugestao compativel ----
        public AplicacaoSugestao AplicarSugestao(Sugestao sugestao)
        {
            if (sugestao is null)
                throw new ArgumentNullException(nameof(sugestao));

            if (!sugestao.EhCompativelCom(TipoResiduo))
                throw new DomainException(
                    $"A sugestao '{sugestao.DescricaoSugestao}' nao e compativel com o tipo de residuo '{TipoResiduo}'.");

            var aplicacao = new AplicacaoSugestao(sugestao.Id);
            _sugestoesAplicadas.Add(aplicacao);
            Registrar(new SugestaoAplicadaEvent(Id, sugestao.Id));
            return aplicacao;
        }

        // ---- Regra adotada no Event Storming: so o dono ou um Admin decidem o destino do residuo ----
        private void GarantirAutorizacao(string usuarioExecutorId)
        {
            // Nota: em codigo real, "ehAdministrador" viria de uma consulta ao
            // Usuario (via repositorio/servico de aplicacao), nao e mostrado
            // aqui para manter o exemplo focado no comportamento do Residuo.
            bool ehDono = usuarioExecutorId == IdUsuario;
            bool ehAdministrador = false; // placeholder — resolvido na camada de aplicacao

            if (!ehDono && !ehAdministrador)
                throw new DomainException(
                    "Apenas o usuario que cadastrou o residuo ou um administrador podem alterar seu status.");
        }
    }
}


// ============================================================================
// 3) Exemplo de uso — mostrando a diferenca na pratica
// ============================================================================

namespace ReGraphik.Domain.Exemplos
{
    using ReGraphik.Domain.Residuos;

    public static class ExemploDeUso
    {
        public static void Executar()
        {
            // Cadastro (RN-09/RN-10 validadas automaticamente no factory method)
            var residuo = Residuo.Cadastrar(
                idUsuario: "user-123",
                tipoResiduo: "Papel Couche",
                especificacao: "Sobras de impressao offset",
                origem: "Setor de Acabamento",
                projeto: "Catalogo Cliente XPTO",
                quantidade: 15.5,
                condicao: "Bom");

            // Aplicando uma sugestao compativel (RN-16)
            var sugestao = new Sugestao("sug-1", "Papel", "Reciclagem para producao de blocos de anotacao");
            residuo.AplicarSugestao(sugestao); // OK, "Papel Couche" contem "Papel"

            // Tentando reaproveitar um residuo que nao e do usuario logado
            try
            {
                residuo.Reaproveitar(usuarioExecutorId: "outro-usuario-999");
            }
            catch (DomainException ex)
            {
                Console.WriteLine($"Bloqueado corretamente: {ex.Message}");
                // "Apenas o usuario que cadastrou o residuo ou um administrador podem alterar seu status."
            }

            // Reaproveitando corretamente (RN-11: EmEstoque -> Reaproveitado)
            residuo.Reaproveitar(usuarioExecutorId: "user-123");

            // Tentando descartar um residuo que ja foi reaproveitado (estado final!)
            try
            {
                residuo.Descartar(usuarioExecutorId: "user-123");
            }
            catch (DomainException ex)
            {
                Console.WriteLine($"Bloqueado corretamente: {ex.Message}");
                // "So e possivel descartar um residuo que esta EmEstoque. Status atual: Reaproveitado."
            }

            // Os eventos de dominio ficam disponiveis para a camada de aplicacao publicar
            // (ex: salvar no Firebase e so entao disparar e-mail, atualizar dashboard, etc.)
            foreach (var evento in residuo.EventosDeDominio)
            {
                Console.WriteLine($"[Evento] {evento.GetType().Name} em {evento.OcorridoEm:HH:mm:ss}");
            }
        }
    }
}