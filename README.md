# ReGraphik — Plataforma de Gestão de Estoque Reverso

<div align="center">

![C#](https://img.shields.io/badge/C%23-.NET%208-512BD4?style=for-the-badge&logo=dotnet)
![WPF](https://img.shields.io/badge/Desktop-WPF-0078D4?style=for-the-badge&logo=windows)
![ASP.NET Core](https://img.shields.io/badge/API-ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet)
![Firebase](https://img.shields.io/badge/Database-Firebase-FFCA28?style=for-the-badge&logo=firebase&logoColor=black)
![Google Maps](https://img.shields.io/badge/Maps-Google%20Places%20API-4285F4?style=for-the-badge&logo=googlemaps&logoColor=white)
![Swagger](https://img.shields.io/badge/Docs-Swagger%20%2F%20OpenAPI-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)
![QuestPDF](https://img.shields.io/badge/PDF-QuestPDF-orange?style=for-the-badge)
![OxyPlot](https://img.shields.io/badge/Charts-OxyPlot-blueviolet?style=for-the-badge)

**Sistema desktop completo para gestão de resíduos no setor gráfico, com API REST, banco de dados em nuvem, mapa interativo, chat em tempo real e módulo ESG.**

[📖 Documentação Online](https://brunomaia.mintlify.app/introduction) · [🚀 Quickstart](https://brunomaia.mintlify.app/quickstart) · [🌐 API em Produção](https://webregraphik.runasp.net)

</div>

---

## Sumário

- [Sobre o Projeto](#sobre-o-projeto)
- [O Problema do Setor Gráfico](#o-problema-do-setor-gráfico)
- [Nossa Solução — Os Três Pilares](#nossa-solução--os-três-pilares)
- [Demonstração — Telas e Funcionalidades](#demonstração--telas-e-funcionalidades)
- [Arquitetura do Sistema](#arquitetura-do-sistema)
- [Padrão MVVM em Detalhe](#padrão-mvvm-em-detalhe)
- [Stack Tecnológica](#stack-tecnológica)
- [Pacotes e Dependências](#pacotes-e-dependências)
- [Estrutura do Repositório](#estrutura-do-repositório)
- [API REST — Referência Completa de Endpoints](#api-rest--referência-completa-de-endpoints)
- [Fluxo de Autenticação e Cadastro](#fluxo-de-autenticação-e-cadastro)
- [Modelos de Dados](#modelos-de-dados)
- [Integrações Externas](#integrações-externas)
- [Conceitos Técnicos Implementados](#conceitos-técnicos-implementados)
- [Status dos Workflows de Status do Resíduo](#status-dos-workflows-de-status-do-resíduo)
- [Como Executar o Projeto](#como-executar-o-projeto)
- [Documentação Complementar](#documentação-complementar)
- [Integrantes](#integrantes)

---

## Sobre o Projeto

O **ReGraphik** é uma plataforma de **gestão de estoque reverso** desenvolvida como Trabalho de Conclusão de Curso (TCC) no **SENAI**, com foco no setor de impressão gráfica. O sistema resolve um problema real e recorrente do setor: o descarte inadequado de materiais como papel, cartão e vinil, que são jogados fora sem nenhum critério, gerando custo, impacto ambiental e desperdício de matéria-prima que ainda tem valor.

O projeto é composto por três componentes principais que funcionam de forma integrada:

| Componente | Tecnologia | Função |
|---|---|---|
| **Cliente Desktop** | WPF + .NET 8 | Interface principal do operador |
| **API REST** | ASP.NET Core | Backend que gerencia dados e regras de negócio |
| **Banco de Dados** | Firebase Realtime Database | Persistência em nuvem com acesso em tempo real |

A documentação técnica completa da API está publicada em **[brunomaia.mintlify.app](https://brunomaia.mintlify.app/introduction)** e a API está disponível em produção em **[webregraphik.runasp.net](https://webregraphik.runasp.net)**.

> ⚠️ A API está hospedada no plano gratuito do runasp.net. Na primeira requisição após período de inatividade, pode haver um tempo de aquecimento de alguns segundos.

---

## O Problema do Setor Gráfico

Empresas de impressão gráfica geram diariamente volumes expressivos de resíduos como:

- **Papel A4 e cartão** com impressão parcial ou unilateral
- **Recortes de vinil** de projetos de plotagem e adesivos
- **Papéis especiais** e materiais compostos descartados após produção

Sem um sistema de controle, esses materiais são descartados de forma irregular, gerando:

- 💸 **Custos desnecessários** de coleta e descarte irregular
- 🌱 **Alto impacto ambiental** por destinação incorreta
- 📦 **Perda de matéria-prima** que poderia ser reaproveitada em novos produtos

---

## Nossa Solução — Os Três Pilares

### 1. Gestão de Estoque Reverso
Controle centralizado de todos os resíduos gerados na gráfica. Cada material é registrado com tipo, quantidade, condição, dimensões, origem, projeto e status (`Disponível`, `Reservado`, `Descartado`). O operador tem visibilidade completa do que está disponível para reaproveitamento.

### 2. Economia Circular
Algoritmo de sugestões que cruza o tipo de resíduo cadastrado com ações de reaproveitamento validadas — como transformar recortes de vinil em sacolas personalizadas, ou papel com impressão unilateral em blocos de rascunho para parceiros.

### 3. Localização de Pontos de Coleta
Integração com a **Google Maps Places API** para encontrar, em tempo real, pontos de coleta e reciclagem certificados por cidade. Os resultados são salvos no Firebase para consulta rápida e exibidos em mapa interativo dentro do aplicativo desktop.

---

## Demonstração — Telas e Funcionalidades

### Módulos disponíveis no cliente desktop

| Módulo | Descrição | Status |
|---|---|---|
| **Login / Cadastro** | Autenticação com validação de token por e-mail em dois passos | ✅ Funcional |
| **Dashboard** | Painel com resumo de indicadores e atividade recente | ✅ Funcional |
| **Cadastrar Resíduos** | Formulário completo de registro de resíduos com upload de foto | ✅ Funcional |
| **Estoque Reverso** | Listagem filtrada por tipo, status e período com `ICollectionView` | ✅ Funcional |
| **Mapa / Pontos de Coleta** | Busca por cidade, pins no mapa Leaflet.js via WebView2 | ✅ Funcional |
| **Sugestão de Resíduos** | Associação de resíduos às sugestões de reaproveitamento | ✅ Funcional |
| **Chat em Tempo Real** | Troca de mensagens entre usuários via Firebase | ✅ Funcional |
| **Relatórios** | Geração de relatórios filtráveis com exportação em PDF (QuestPDF) | ✅ Funcional |
| **ESG** | Painel de indicadores ambientais com exportação de documento | ✅ Funcional |
| **Conta / Perfil** | Edição de dados e foto de perfil (integração com Imgur API) | ✅ Funcional |

### Janelas auxiliares

| Janela | Descrição |
|---|---|
| `LoginWindow` | Autenticação com login e senha + link para recuperação |
| `RecuperarSenhaWindow` | Fluxo de recuperação de acesso |
| `ChatPainelWindow` | Lista de conversas com badge de mensagens não lidas |
| `MensagemWindow` | Janela de troca de mensagens entre dois usuários |
| `SugestaoResiduoWindow` | Aplicação interativa de sugestão a um resíduo específico |
| `SairMensagemWindow` | Confirmação de logout |
| `MensagemWindow` | Modal de mensagens e alertas do sistema |

---

## Arquitetura do Sistema

```
┌─────────────────────────────────────────────────────┐
│              Cliente Desktop (WPF)                  │
│  Views (XAML) ↔ ViewModels (C#) ↔ Services (C#)    │
└───────────────────┬─────────────────────────────────┘
                    │  HTTP/REST (JSON)
                    ▼
┌─────────────────────────────────────────────────────┐
│            API REST (ASP.NET Core .NET 8)           │
│   Controllers → Services → Firebase Realtime DB     │
└───────┬──────────────────────────────┬──────────────┘
        │                              │
        ▼                              ▼
┌───────────────┐            ┌──────────────────────┐
│ Firebase      │            │  Google Maps         │
│ Realtime DB   │            │  Places API          │
│ (Persistência)│            │  (Pontos de Coleta)  │
└───────────────┘            └──────────────────────┘
```

O cliente WPF também se comunica **diretamente** com o Firebase para o módulo de **Chat em Tempo Real**, sem passar pela API REST — garantindo baixa latência nas mensagens.

---

## Padrão MVVM em Detalhe

O projeto segue rigorosamente o padrão **MVVM (Model-View-ViewModel)** em toda a camada de apresentação. Nenhuma lógica de negócio vive no code-behind das Views.

```
┌─────────────────────────────────────────────────────────┐
│  VIEW (XAML)                                            │
│  Controles visuais, Data Binding declarativo            │
│  Ex: ResiduosControl.xaml, EstoqueReversoControl.xaml   │
└─────────────────────┬───────────────────────────────────┘
                      │ ICommand (RelayCommand)
                      ▼
┌─────────────────────────────────────────────────────────┐
│  VIEWMODEL (C#)                                         │
│  Lógica de apresentação, estado da tela, comandos       │
│  Ex: ResiduoViewModel, EstoqueReversoViewModel          │
│  Herda de: BaseViewModel → INotifyPropertyChanged       │
└─────────────────────┬───────────────────────────────────┘
                      │ async/await
                      ▼
┌─────────────────────────────────────────────────────────┐
│  SERVICES + MODELS (C#)                                 │
│  Chamadas HTTP, Firebase, regras de negócio             │
│  Ex: AutorizarService, ChatService, ResiduoService      │
└─────────────────────────────────────────────────────────┘
```

## Dashboard — Indicadores e Gráficos em Tempo Real
 
O Dashboard é a tela inicial após o login. Ele carrega dados direto da API e renderiza dois gráficos via **OxyPlot**:
 
- **Gráfico de pizza** — distribuição dos resíduos por status, com fatias coloridas por estado (`#0d2a56` para "Aguardando CADRI", `#1649a2` para "Aguardando Triagem", etc.)
- **Gráfico de barras horizontais** — peso total (kg) por tipo de resíduo, ordenado de forma crescente
Além dos gráficos, o Dashboard exibe quatro cards de indicadores calculados em tempo real:
 
| Indicador | Como é calculado |
|---|---|
| **Total de Resíduos** | Contagem total dos registros da API |
| **Reaproveitados** | Contagem de resíduos com `Status == "Reaproveitado"` |
| **Em Estoque** | Contagem de resíduos com `Status == "Em Estoque"` |
| **Valor Estimado** | `Soma(Quantidade × R$ 5,50)` formatado como moeda |
 
A tabela de **Últimos 5 Resíduos** exibe os mais recentes ordenados por `DataCadastro` decrescente, com IDs renumerados de 1 a 5 para exibição no card.
 
A foto de perfil do usuário logado é carregada de `UsuarioSessaoService.Instancia.FotoCaminho` e reage a mudanças via evento `PropertyChanged` — sem necessidade de reiniciar a tela.
 
```csharp
// DashboardViewModel.cs — reatividade da foto de perfil
UsuarioSessaoService.Instancia.PropertyChanged += (s, e) =>
{
    if (e.PropertyName == nameof(UsuarioSessaoService.FotoCaminho))
        OnPropertyChanged(nameof(FotoPerfil));
};
```
 
---

### Como funciona na prática

1. **Usuário interage com a View** — clica num botão em `ResiduosControl.xaml`, que está vinculado a um `RelayCommand` via `Command="{Binding SalvarCommand}"`.
2. **RelayCommand chama a ViewModel** — `ResiduoViewModel` processa a ação sem nenhum código na View.
3. **ViewModel chama o Service** — `AutorizarService` (ou outro service) faz a chamada HTTP para a API ou acessa o Firebase.
4. **ViewModel atualiza suas propriedades** — a `BaseViewModel` notifica a View via `INotifyPropertyChanged`.
5. **View se atualiza automaticamente** — o binding do WPF reflete os dados na tela sem nenhuma linha de code-behind.

---

## Stack Tecnológica

| Camada | Tecnologia | Versão |
|---|---|---|
| Linguagem | C# | .NET 8 |
| Frontend Desktop | WPF — Windows Presentation Foundation | .NET 8 |
| Padrão de Projeto | MVVM + BaseViewModel | — |
| Backend | ASP.NET Core Web API | .NET 8 |
| Banco de Dados | Firebase Realtime Database | — |
| Autenticação Firebase | Google Service Account (JSON) | — |
| Mapa Interativo | Google Maps Places API + Leaflet.js | — |
| Renderização de Mapa | Microsoft WebView2 | 1.0.2903.40 |
| Chat em Tempo Real | Firebase Realtime Database (SDK direto) | — |
| Geração de PDF (Relatórios) | QuestPDF | 2026.5.0 |
| Geração de PDF (ESG) | WPF FlowDocument + PrintDialog | — |
| Gráficos | OxyPlot.Wpf | 2.2.0 |
| Ícones UI | MahApps.Metro.IconPacks.Material | 6.2.1 |
| Modais flutuantes | MahApps.Metro.SimpleChildWindow | 2.2.1 |
| Upload de Imagem | Imgur API | 5.0.0 |
| Validação de Cadastro | Token numérico de 6 dígitos via e-mail | — |
| Documentação da API | Swagger / OpenAPI (com comentários XML) | — |
| IDE utilizada | JetBrains Rider | — |

---

## Pacotes e Dependências

### Cliente WPF (`ReGraphik.csproj`)

```xml
<PackageReference Include="FirebaseDatabase.Net"            Version="4.2.0" />
<PackageReference Include="Imgur.API"                       Version="5.0.0" />
<PackageReference Include="MahApps.Metro.IconPacks.Material" Version="6.2.1" />
<PackageReference Include="MahApps.Metro.SimpleChildWindow" Version="2.2.1" />
<PackageReference Include="Microsoft.Web.WebView2"          Version="1.0.2903.40" />
<PackageReference Include="OxyPlot.Wpf"                    Version="2.2.0" />
<PackageReference Include="QuestPDF"                        Version="2026.5.0" />
```

### API REST (`ApiRestReGraphik.csproj`)

- ASP.NET Core Web API (.NET 8)
- Swagger / OpenAPI com comentários XML
- `HttpClient` para integração com Google Maps e Firebase
- CORS configurado para permitir todas as origens (ajustável para produção)

---

## Estrutura do Repositório

```
ReGraphikApp/
│
├── ApiRestReGraphik/                  # Projeto da API REST (ASP.NET Core)
│   ├── Controllers/
│   │   ├── UsuarioController.cs       # Gerencia usuários e fluxo de cadastro
│   │   ├── ResiduoController.cs       # CRUD de resíduos
│   │   ├── PontosColetaController.cs  # Coleta + integração Google Maps
│   │   ├── SugestaoController.cs      # CRUD de sugestões de reaproveitamento
│   │   └── SugestaoResiduosController.cs # Aplicação de sugestões a resíduos
│   ├── Services/
│   │   ├── UsuarioService.cs
│   │   ├── ResiduoService.cs
│   │   ├── PontosColetaService.cs
│   │   ├── SugestaoService.cs
│   │   └── SugestaoResiduosService.cs
│   ├── Models/
│   │   ├── Usuario.cs
│   │   ├── Residuo.cs
│   │   ├── PontosColeta.cs
│   │   ├── Sugestao.cs
│   │   ├── SugestaoResiduo.cs
│   │   ├── LoginRequest.cs
│   │   └── RequisicaoToken.cs
│   ├── Models/DTOs/
│   │   ├── UsuarioDto.cs              # Usado em finalizar-cadastro
│   │   ├── ResiduoDto.cs              # Classe vazia (placeholder)
│   │   ├── PontosColetaDto.cs         # Classe vazia (placeholder)
│   │   ├── SugestaoDto.cs             # Classe vazia (placeholder)
│   │   ├── SugestaoResiduoDto.cs      # Classe vazia (placeholder)
│   │   └── SolicitarAcessoDto.cs      # Usado em POST /api/Usuario
│   ├── Data/
│   │   └── DbReGraphik.cs             # Configuração da conexão Firebase
│   ├── Program.cs                     # Configuração de DI, Swagger, CORS
│   └── appsettings.json
│
├── ReGraphik/                         # Projeto WPF (cliente desktop)
│   ├── Views/                         # Janelas e controles da UI
│   │   ├── LoginWindow.xaml
│   │   ├── MainWindow.xaml
│   │   ├── ChatPainelWindow.xaml
│   │   ├── MensagemWindow.xaml
│   │   ├── RecuperarSenhaWindow.xaml
│   │   ├── SairMensagemWindow.xaml
│   │   └── SugestaoResiduoWindow.xaml
│   ├── Views/Controls/                # UserControls das seções do sistema
│   │   ├── DashboardControl.xaml
│   │   ├── ResiduosControl.xaml
│   │   ├── EstoqueReversoControl.xaml
│   │   ├── MapaControl.xaml
│   │   ├── SugestaoResiduoControl.xaml
│   │   ├── RelatoriosControl.xaml
│   │   ├── ContaControl.xaml
│   │   └── EsgControl.xaml
│   ├── ViewModels/
│   │   ├── BaseViewModel.cs           # INotifyPropertyChanged base
│   │   ├── MainViewModel.cs           # Controla navegação lateral
│   │   ├── LoginViewModel.cs
│   │   ├── CadastroViewModel.cs
│   │   ├── DashboardViewModel.cs
│   │   ├── ResiduoViewModel.cs
│   │   ├── EstoqueReversoViewModel.cs # Filtro com ICollectionView
│   │   ├── MapaViewModel.cs
│   │   ├── SugestaoResiduoViewModel.cs
│   │   ├── ChatViewModel.cs
│   │   ├── RelatorioViewModel.cs      # Geração de PDF com QuestPDF
│   │   ├── ContaViewModel.cs
│   │   ├── EsgViewModel.cs            # Exportação ESG com FlowDocument
│   │   └── UsuarioViewModel.cs
│   ├── Models/
│   │   ├── Usuario.cs
│   │   ├── Residuo.cs
│   │   ├── PontosColeta.cs
│   │   ├── Sugestao.cs
│   │   ├── SugestaoResiduo.cs
│   │   ├── Mensagem.cs
│   │   ├── Conversa.cs
│   │   └── RespostaToken.cs
│   ├── Services/
│   │   ├── Interfaces/
│   │   │   ├── IAutorizarService.cs
│   │   │   ├── IChatService.cs
│   │   │   └── IResiduoService.cs
│   │   ├── AutorizarService.cs        # Login, cadastro, token
│   │   ├── ChatService.cs             # Mensagens no Firebase
│   │   ├── FirebaseConfig.cs          # Inicialização do Firebase SDK
│   │   ├── GooglePlacesService.cs     # Busca pontos via Google Maps
│   │   ├── ResiduoService.cs
│   │   ├── ConfiguracaoLocalService.cs
│   │   ├── UsuarioSessaoService.cs    # Singleton de sessão do usuário
│   │   └── ValidacaoCpfService.cs     # Algoritmo de validação de CPF
│   ├── Commands/
│   │   └── RelayCommand.cs            # ICommand parametrizado e não-param.
│   ├── Converters/
│   │   ├── BadgeNotificacaoConverter.cs
│   │   ├── Base64ToImageConverter.cs
│   │   ├── BoolToVisibilityConverter.cs
│   │   ├── ChatConverter.cs
│   │   ├── NaoLidasVisibilidadeConverter.cs
│   │   ├── NullToVisibilityConverter.cs
│   │   ├── StatusToColorConverter.cs
│   │   └── StringToVisibilityConverter.cs
│   ├── Styles/
│   │   ├── Botoes.xaml
│   │   ├── Cores.xaml
│   │   ├── Inputs.xaml
│   │   └── Textos.xaml
│   └── App.xaml
│
├── Modelagem/                         # Documentação de banco de dados
│   ├── MiniMundo Demanda.pdf
│   ├── Modelo Conceitual.pdf
│   ├── Modelo Lógico.pdf
│   └── Modelo Físico.pdf
│
├── Banco de Dados/
│   └── Documentação Criação Modelagem.pdf
│
├── ReGraphik_MVVM_APIRest.pptx        # Apresentação técnica da arquitetura
├── ReGraphik_IntegraSenai_Documentacao_TCC_01.pdf
└── ReGraphik.slnx                     # Solution file
```

---

## API REST — Referência Completa de Endpoints

A API está disponível em **`https://webregraphik.runasp.net`**. A documentação interativa com Swagger é a página inicial da aplicação.

> Todos os endpoints retornam JSON. Erros seguem o padrão HTTP com corpo `{ "mensagem": "..." }`.

---

### 👤 Usuário — `api/Usuario`

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/Usuario` | Lista todos os usuários cadastrados |
| `GET` | `/api/Usuario/{id}` | Obtém usuário por ID |
| `POST` | `/api/Usuario` | Solicita acesso ao sistema (pré-cadastro — inicia fila de aprovação) |
| `POST` | `/api/Usuario/autorizar-token` | Administrador autoriza o token para o e-mail solicitado |
| `POST` | `/api/Usuario/validar-token` | Usuário valida o token recebido por e-mail |
| `POST` | `/api/Usuario/finalizar-cadastro?token=` | Finaliza o cadastro com dados completos (nome, CPF, login, senha) |
| `POST` | `/api/Usuario/login` | Autentica o usuário com login e senha, retornando o objeto completo |
| `PUT` | `/api/Usuario/{id}` | Atualiza dados de um usuário existente |
| `DELETE` | `/api/Usuario/{id}` | Remove um usuário |

---

### 🗑️ Resíduo — `api/Residuo`

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/Residuo` | Lista todos os resíduos cadastrados no estoque reverso |
| `GET` | `/api/Residuo/{id}` | Obtém resíduo por ID |
| `POST` | `/api/Residuo` | Registra novo resíduo no estoque reverso |
| `PUT` | `/api/Residuo/{id}` | Atualiza dados de um resíduo (ex: altera status) |
| `DELETE` | `/api/Residuo/{id}` | Remove resíduo do estoque |

> No `POST`, o `IdUsuario` do resíduo é resolvido a partir de `User.FindFirst(ClaimTypes.NameIdentifier)`. Como a API não tem nenhum esquema de autenticação configurado em `Program.cs` (não há `UseAuthentication()`), essa claim nunca é preenchida em produção — o controller cai sempre no fallback de um GUID fixo de teste (`0d95265b-2757-424e-8ea9-445e8fd2a422`), a menos que o `IdUsuario` já venha definido no corpo da requisição.

---

### 📍 Pontos de Coleta — `api/PontosColeta`

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/PontosColeta` | Lista todos os pontos de coleta salvos no Firebase |
| `GET` | `/api/PontosColeta/{id}` | Obtém ponto de coleta por ID |
| `POST` | `/api/PontosColeta/sincronizar?cidade=...` | Busca pontos via Google Maps Places API e salva no Firebase os que ainda não existem |
| `POST` | `/api/PontosColeta` | Cadastra ponto de coleta manualmente |
| `PUT` | `/api/PontosColeta/{id}` | Atualiza dados de um ponto de coleta |
| `DELETE` | `/api/PontosColeta/{id}` | Remove ponto de coleta |

> O endpoint `/sincronizar` consulta a Google Maps Places API com a query fixa `"ponto de coleta reciclagem {cidade}"`, compara as coordenadas (lat/lng) retornadas com as de **todos** os pontos já salvos no Firebase via `HashSet` e grava apenas os que ainda não existem — ignorando duplicatas exatas mesmo que sejam de cidades diferentes. Os campos `Estado`, `CEP` e `ResiduosAceitos` não vêm do Google; são preenchidos com valores fixos (`"BR"`, `"—"` e `"Reciclável"`, respectivamente). A resposta não traz a lista de pontos, apenas os contadores:
> ```json
> { "Mensagem": "Sincronização de 'São Paulo' concluída com sucesso!", "PontosSalvos": 12, "PontosIgnoradosPorDuplicidade": 3 }
> ```

---

### 💡 Sugestão — `api/Sugestao`

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/Sugestao` | Lista todas as sugestões de reaproveitamento disponíveis |
| `GET` | `/api/Sugestao/{id}` | Obtém sugestão por ID |
| `POST` | `/api/Sugestao` | Cadastra nova sugestão de reaproveitamento |
| `PUT` | `/api/Sugestao/{id}` | Atualiza sugestão existente |
| `DELETE` | `/api/Sugestao/{id}` | Remove sugestão |

---

### 🔁 Sugestão de Resíduos — `api/SugestaoResiduos`

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/SugestaoResiduos` | Lista todas as aplicações de sugestões a resíduos |
| `GET` | `/api/SugestaoResiduos/{id}` | Obtém registro por ID |
| `POST` | `/api/SugestaoResiduos` | Registra a aplicação de uma sugestão a um resíduo específico |
| `PUT` | `/api/SugestaoResiduos/{id}` | Atualiza o registro de aplicação |
| `DELETE` | `/api/SugestaoResiduos/{id}` | Remove o registro |

---

## Fluxo de Autenticação e Cadastro

O sistema utiliza um fluxo de **dois passos com token de e-mail** para garantir que apenas colaboradores autorizados criem contas. O domínio de e-mail exigido é `@regraphik.com.br`.

```
1. Usuário preenche e-mail corporativo (WPF)
         ↓
2. POST /api/Usuario  →  Solicitação entra na fila de aprovação
         ↓
3. Administrador aprova via POST /api/Usuario/autorizar-token
         ↓
4. API gera token de 6 dígitos e envia por e-mail
         ↓
5. Usuário digita o token no WPF
         ↓
6. POST /api/Usuario/validar-token  →  Token confirmado
         ↓
7. POST /api/Usuario/finalizar-cadastro?token=...  →  Conta criada
         ↓
8. POST /api/Usuario/login  →  Sessão iniciada
```

O serviço `AutorizarService.cs` no cliente WPF encapsula todas essas etapas, e `UsuarioSessaoService.cs` mantém os dados do usuário logado em memória durante toda a sessão.

No `POST /api/Usuario`, a API valida o domínio do e-mail (rejeitando qualquer remetente fora de `@regraphik.com.br`) e verifica se já existe uma solicitação ou cadastro com o mesmo e-mail antes de aceitar a solicitação.

Os tokens gerados em `autorizar-token` ficam num `Dictionary<string, string>` estático em memória no próprio `UsuarioController` — não são persistidos no Firebase. Isso significa que um restart da API invalida todos os tokens pendentes. O token é removido do dicionário ("queimado") tanto em `validar-token` quanto em `finalizar-cadastro`, então cada token só pode ser usado uma vez. O login (`POST /api/Usuario/login`) ainda verifica o campo `Ativo` do usuário e retorna 401 com a mensagem "Conta não validada" caso a conta não tenha sido finalizada. A autenticação em `UsuarioService.Autenticar()` compara `Login` e `Senha` diretamente contra os valores armazenados no Firebase, sem hashing.

---

## Modelos de Dados

### Usuario

| Campo | Tipo | Descrição |
|---|---|---|
| `id` | `string` | ID único gerado pelo Firebase |
| `name` | `string` | Nome completo |
| `cpf` | `string` | CPF no formato `XXX.XXX.XXX-XX` |
| `email` | `string` | E-mail corporativo `@regraphik.com.br` |
| `login` | `string` | Nome de usuário para autenticação |
| `senha` | `string` | Senha (mínimo 8 caracteres) |
| `perfil` | `string` | `"User"` ou `"Admin"` |
| `foto_perfil` | `string?` | URL da imagem de perfil (Imgur) |
| `data_cadastro` | `datetime` | Data/hora de criação da conta |
| `ativo` | `bool` | Indica se a conta está ativa |
| `token_validacao` | `string?` | Campo reservado para o token de validação (atualmente os tokens ativos são controlados em memória pelo `UsuarioController`, não persistidos aqui) |

### Residuo

| Campo | Tipo | Descrição |
|---|---|---|
| `id` | `string` | ID único gerado pelo Firebase |
| `id_usuario` | `string` | ID do usuário que cadastrou o resíduo |
| `tipo_residuo` | `string` | Tipo do material (ex: `"Papel A4"`, `"Vinil"`, `"Cartão"`) |
| `origem` | `string` | Setor ou processo que gerou o resíduo |
| `especificacao` | `string` | Detalhes adicionais do material |
| `projeto` | `string` | Projeto de produção que gerou o resíduo |
| `quantidade` | `double` | Quantidade (unidades ou peso) |
| `data_cadastro` | `datetime` | Data de registro no estoque |
| `condicao` | `string` | Estado físico do material |
| `dimensoes_cm` | `double?` | Comprimento em cm (opcional) |
| `dimensoes_lm` | `double?` | Largura em cm (opcional) |
| `observacao` | `string` | Observações livres |
| `anexo` | `string` | URL da foto do material (Base64 → Imgur) |
| `status` | `string` | `"Disponível"`, `"Reservado"` ou `"Descartado"` |

#### Propriedade Calculada — `IdCard`

O modelo `Residuo` expõe a propriedade calculada `IdCard` para exibir o ID de forma amigável nos cards da interface — sem lógica de formatação no XAML:

```csharp
public string IdCard
{
    get
    {
        if (string.IsNullOrEmpty(Id)) return "#00000000";
        return Id.Length > 8 ? $"#{Id.Substring(0, 8)}" : $"#{Id}";
    }
}
// Ex: ID "10dcd90e-f234-..." → exibe "#10dcd90e"
```

### PontosColeta

| Campo | Tipo | Descrição |
|---|---|---|
| `id` | `string` | ID gerado pelo Firebase |
| `nome_ponto` | `string` | Nome do ponto de coleta |
| `cidade` | `string` | Cidade onde está localizado |
| `estado` | `string` | Estado (UF) |
| `cep` | `string` | CEP do endereço |
| `residuos_aceitos` | `string` | Tipos de resíduos aceitos no local |
| `latitude` | `double` | Latitude (Google Maps) |
| `longitude` | `double` | Longitude (Google Maps) |

### Sugestao

| Campo | Tipo | Descrição |
|---|---|---|
| `id` | `string` | ID único |
| `tipo_residuo_aceito` | `string` | Tipo de resíduo ao qual a sugestão se aplica |
| `descricao_sugestao` | `string` | Descrição da ação de reaproveitamento |

### SugestaoResiduo

| Campo | Tipo | Descrição |
|---|---|---|
| `id` | `string` | ID único |
| `id_cadastro_residuo` | `string` | GUID do resíduo no estoque (campo obrigatório) |
| `id_sugestao` | `string` | GUID da sugestão aplicada (campo obrigatório) |
| `data_aplicacao` | `datetime?` | Data em que a sugestão foi aplicada — se omitida no `POST`, a API preenche automaticamente com `DateTime.UtcNow` |

### Mensagem (Chat — Firebase direto)

| Campo | Tipo | Descrição |
|---|---|---|
| `id` | `string` | ID único da mensagem |
| `remetente_id` | `string` | ID do usuário que enviou |
| `destinatario_id` | `string` | ID do usuário que recebeu |
| `texto` | `string` | Conteúdo da mensagem |
| `data_hora` | `datetime` | Data e hora do envio |
| `lida` | `bool` | Indica se a mensagem foi lida |

---

## Integrações Externas

### Firebase Realtime Database

Toda a persistência dos dados é feita no Firebase. A autenticação com o banco é realizada via **Google Service Account** (arquivo `.json` de credenciais), com os seguintes escopos OAuth:

- `https://www.googleapis.com/auth/userinfo.email`
- `https://www.googleapis.com/auth/firebase.database`

**Nós do banco de dados:**

| Nó Firebase | Entidade |
|---|---|
| `usuarios` | Usuários do sistema |
| `residuos` | Resíduos cadastrados no estoque reverso |
| `pontos_coleta` | Pontos de coleta e reciclagem |
| `sugestoes` | Sugestões de reaproveitamento |
| `sugestoes_residuos` | Registros de sugestões aplicadas a resíduos |
| `mensagens` | Mensagens de chat entre usuários |

### Google Maps Places API

Utilizada em dois pontos do sistema:

**Na API REST** (`PontosColetaController` → `PontosColetaService`):
- Consulta pontos de coleta e reciclagem por cidade
- Valida se a cidade já tem pontos cadastrados no Firebase antes de consultar
- Salva os resultados automaticamente com coordenadas de lat/lng

**No cliente WPF** (`GooglePlacesService`):
- Busca pontos de coleta próximos por cidade e tipo de material
- Gera HTML com Leaflet.js e abre via WebView2 para exibir o mapa interativo

#### Google Places — Detalhe de Implementação (Cliente WPF)

O `GooglePlacesService` faz dois níveis de chamada à API do Google Maps:

1. **Text Search** — busca por termo livre (`"{material} em {cidade}"`) e retorna lista de locais com `place_id`
2. **Place Details** — para cada `place_id` retornado, faz uma segunda chamada para obter telefone, site e endereço completo

```csharp
// GooglePlacesService.cs — query de busca
string termoBusca = $"{material} em {cidade}";
string searchUrl = $"https://maps.googleapis.com/maps/api/place/textsearch/json" +
                   $"?query={Uri.EscapeDataString(termoBusca)}&key={_apiKey}";
```

Por segurança, falhas HTTP são capturadas e os detalhes (incluindo a API Key) são ocultados nos logs de diagnóstico:

```csharp
catch (HttpRequestException)
{
    Debug.WriteLine("[SEGURANÇA] Falha na comunicação. Detalhes ocultados para proteger as credenciais.");
    return listaDePostos; // Retorna lista vazia em vez de propagar a exceção
}
```

O mapa é renderizado usando **Leaflet.js** carregado via CDN dentro de um arquivo HTML temporário gerado em `Path.GetTempPath()`, aberto pelo componente `WebView2` (Microsoft Edge embutido). Isso permite renderização completa de mapas interativos dentro de uma janela WPF sem depender de WebBrowser legado.

### Imgur API

Utilizada no módulo **Conta / Perfil** para fazer upload da foto de perfil do usuário. A imagem é convertida para Base64 antes do envio e a URL retornada pela Imgur é salva no Firebase junto ao perfil do usuário.

---

## Conceitos Técnicos Implementados

### RelayCommand — Padrão de Binding MVVM

O `RelayCommand` implementa `ICommand` e garante que nenhum evento de UI acesse a ViewModel diretamente. Ele existe em duas versões: sem parâmetro e com parâmetro (`object`).

```csharp
// Commands/RelayCommand.cs
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object parameter)    => _execute(parameter);

    public event EventHandler CanExecuteChanged
    {
        add    => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
```

```xml
<!-- Binding na View (XAML) — zero code-behind -->
<Button Content="Salvar"
        Command="{Binding SalvarCommand}"
        CommandParameter="{Binding ResiduoSelecionado}" />
```
### RelayCommand — Implementação Completa (4 variantes)
 
O `RelayCommand` implementado no projeto suporta **quatro variantes** de execução para cobrir todos os padrões usados nas ViewModels — síncronos, assíncronos, com e sem parâmetro:
 
```csharp
// Commands/RelayCommand.cs — construtores disponíveis
 
// 1. Ação assíncrona sem parâmetro — usado em carregamentos de tela
new RelayCommand(async () => await CarregarDadosAsync())
 
// 2. Ação assíncrona com parâmetro — usado com CommandParameter no XAML
new RelayCommand(async (param) => await ProcessarAsync(param))
 
// 3. Ação síncrona sem parâmetro — usado para navegação e limpeza de filtros
new RelayCommand(() => LimparFiltros())
 
// 4. Ação síncrona com parâmetro — usado para abrir janelas com item selecionado
new RelayCommand((param) => AbrirSugestoes(param as Residuo))
```
 
O `CanExecuteChanged` é vinculado ao `CommandManager.RequerySuggested` do WPF, que reavalia automaticamente o estado (`CanExecute`) de todos os comandos sempre que há interação na UI — mantendo botões habilitados/desabilitados de forma reativa sem código extra.
 
Existe também uma versão genérica `RelayCommand<T>` para uso fortemente tipado:
 
```csharp
// Uso tipado — sem cast manual no Execute
public ICommand SelecionarResiduoCommand { get; } =
    new RelayCommand<Residuo>(residuo => AbrirDetalhes(residuo));
```
 
---
 
### Converters — Camada de Adaptação Visual
 
O projeto usa **8 value converters** registrados como recursos globais no XAML para transformar dados do modelo em valores visuais sem lógica no code-behind:
 
| Converter | Entrada → Saída |
|---|---|
| `StatusToColorConverter` | `string` (status) → `SolidColorBrush` (cor do badge) |
| `StatusToColorConverter` (param `"Foreground"`) | `string` (status) → `Brushes.White` ou cor escura para contraste |
| `Base64ToImageConverter` | `string` (Base64 ou data URL) → `BitmapImage` |
| `BoolToVisibilityConverter` | `bool` → `Visibility.Visible` / `Collapsed` |
| `NullToVisibilityConverter` | `null` / valor → `Visibility` |
| `StringToVisibilityConverter` | `string` vazia/nula → `Visibility.Collapsed` |
| `BadgeNotificacaoConverter` | contagem de notificações → texto do badge |
| `NaoLidasVisibilidadeConverter` | contagem de não lidas → `Visibility` do indicador |
| `ChatConverter` | dados de mensagem → alinhamento/cor da bolha do chat |
 
O `Base64ToImageConverter` trata tanto strings Base64 puras quanto data URLs com prefixo (`data:image/jpeg;base64,...`), extraindo apenas o payload antes de decodificar:
 
```csharp
if (base64String.Contains(","))
    base64String = base64String.Substring(base64String.IndexOf(",") + 1);
```
 
---
 
### Tratamento de Erros na API — Padrão por Camada
 
Todos os controllers da API seguem um padrão consistente de tratamento de exceções em três camadas:
 
```csharp
// Padrão aplicado em todos os controllers
try
{
    var result = await _service.Listar();
    return Ok(result);
}
catch (ArgumentException ex)
{
    _logger.LogWarning(ex, "Requisição inválida");
    return BadRequest("Requisição inválida.");           // 400
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "Falha de comunicação Firebase");
    return StatusCode(404, "Recurso não encontrado.");  // 404
}
catch (Exception ex)
{
    _logger.LogError(ex, "Erro interno");
    return StatusCode(500, "Erro interno ao processar a solicitação."); // 500
}
```
 
Os services tratam ainda `FirebaseException` (falha de conexão/autenticação com o banco) e `JsonException` (dados corrompidos ou estrutura incompatível no Firebase), com mensagens de log específicas para cada cenário.

---

### Enriquecimento de Dados Relacionados — Join Client-Side

Como o Firebase Realtime Database não suporta `JOIN` nativo, os services que retornam entidades com referências a outros nós buscam os dados relacionados em paralelo e os associam em memória com um dicionário.

`ResiduoService.Listar()` busca os nós `residuos` e `usuarios` simultaneamente com `Task.WhenAll`, monta um dicionário de usuários por ID e preenche a propriedade de navegação `Residuo.Usuario` de cada item:

```csharp
// ResiduoService.cs — busca em paralelo + associação por dicionário
var tarefaResiduos = _firebaseClient.Child(NodeName).OnceAsync<Residuo>();
var tarefaUsuarios = _firebaseClient.Child(UsersNodeName).OnceAsync<Usuario>();
await Task.WhenAll(tarefaResiduos, tarefaUsuarios);

var dicionarioUsuarios = tarefaUsuarios.Result
    .GroupBy(u => u.Key)
    .ToDictionary(g => g.Key, g => g.First().Object);

foreach (var residuo in listaResiduos)
{
    if (!string.IsNullOrEmpty(residuo.IdUsuario) &&
        dicionarioUsuarios.TryGetValue(residuo.IdUsuario, out var usuario))
        residuo.Usuario = usuario;
}
```

`SugestaoResiduosService.Listar()` segue o mesmo padrão, mas com três nós simultâneos (`sugestoes_residuos`, `sugestoes`, `residuos`), populando `SugestaoResiduo.Sugestao` e `SugestaoResiduo.CadastroResiduo`. Essas propriedades de navegação são marcadas com `[JsonIgnore]` e `[ValidateNever]` nos models, então não aparecem na serialização — servem apenas para uso interno da API, não para o payload retornado ao cliente.

---
---

### BaseViewModel — INotifyPropertyChanged

Toda ViewModel herda de `BaseViewModel`, que implementa `INotifyPropertyChanged` e expõe o método `OnPropertyChanged()`. Isso garante que qualquer alteração numa propriedade seja automaticamente refletida na View.

```csharp
// ViewModels/BaseViewModel.cs
public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```
### Conexão Firebase — API vs. Cliente WPF
 
O projeto usa **duas formas diferentes** de conectar ao Firebase, cada uma adequada ao seu contexto:
 
**Na API REST** (`DbReGraphik.cs`) — autenticação via Service Account com `FirebaseAdmin` SDK:
```csharp
// Inicializa o FirebaseApp com credenciais do servidor (uma única vez)
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(caminhoCompletoChave)
    });
}
DbFirebase = new FirebaseClient(configuration["Firebase:RealtimeDatabaseUrl"]);
```
 
**No cliente WPF** (`FirebaseConfig.cs`) — acesso direto com URL pública (database rules controlam acesso):
```csharp
// Singleton — reutiliza a mesma instância em todos os services do cliente
public static FirebaseClient Client =>
    _client ??= new FirebaseClient("https://regraphikfirebase-default-rtdb.firebaseio.com/");
```
 
A separação existe porque a API precisa de autenticação privilegiada (Service Account) para operações administrativas, enquanto o cliente WPF acessa o Firebase diretamente apenas para o chat, onde a latência importa mais que a camada de autorização centralizada.
 
---
---

### ICollectionView — Filtro no Estoque Reverso

A tela de Estoque Reverso usa `ICollectionView` para aplicar filtros em tempo real sobre a coleção de resíduos sem recarregar dados da API.

```csharp
// EstoqueReversoViewModel.cs (trecho)
private ICollectionView _residuosView;

public EstoqueReversoViewModel()
{
    _residuosView = CollectionViewSource.GetDefaultView(_residuos);
    _residuosView.Filter = AplicarFiltro;
}

private bool AplicarFiltro(object item)
{
    if (item is not Residuo r) return false;
    bool passaTipo   = FiltroTipo   == "Todos" || r.TipoResiduo == FiltroTipo;
    bool passaStatus = FiltroStatus == "Todos" || r.Status      == FiltroStatus;
    return passaTipo && passaStatus;
}
```

---

### Estoque Reverso — Observable em Tempo Real (Firebase Streaming)
 
O carregamento do Estoque Reverso não usa uma lista estática — ele assina o nó `residuos` do Firebase com **`.AsObservable<Residuo>()`**, o que significa que qualquer inserção, edição ou exclusão feita por outro usuário aparece automaticamente na tela sem recarregar.
 
```csharp
// EstoqueReversoViewModel.cs
firebase
    .Child("residuos")
    .AsObservable<Residuo>()
    .Subscribe(subsecao =>
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (subsecao.EventType == FirebaseEventType.InsertOrUpdate)
            {
                // Remove versão antiga para evitar duplicatas no grid
                var existente = _todosResiduos.FirstOrDefault(r => r.Id == subsecao.Object.Id);
                if (existente != null) _todosResiduos.Remove(existente);
 
                _todosResiduos.Add(subsecao.Object);
            }
            else if (subsecao.EventType == FirebaseEventType.Delete)
            {
                var existente = _todosResiduos.FirstOrDefault(r => r.Id == subsecao.Key);
                if (existente != null) _todosResiduos.Remove(existente);
            }
 
            ResiduosFiltrados.Refresh(); // Reaplica os filtros ativos
        });
    });
```
 
Os ComboBoxes de filtro (Tipo, Origem, Status) são populados **dinamicamente** a partir dos próprios dados que chegam do Firebase — não são listas fixas no código. Cada novo resíduo que chega inclui seu valor nos filtros automaticamente se ele ainda não estiver presente.
 
Os filtros disponíveis na tela de Estoque Reverso são:
 
| Filtro | Lógica aplicada |
|---|---|
| Tipo | `Contains` case-insensitive sobre `TipoResiduo` |
| Origem | `Contains` case-insensitive sobre `Origem` |
| Status | `Contains` case-insensitive sobre `Status` |
| Período | Últimos 7 / 30 / 90 dias por `DataCadastro` |
 
Todos os filtros são combinados com lógica **AND** — o item precisa passar em todos para aparecer na lista.
 
---

### ChatService — Firebase direto do cliente

O chat comunica diretamente com o Firebase Realtime Database para garantir baixa latência. O ID de conversa é gerado de forma determinística — independente de quem iniciou a conversa.

```csharp
// Services/ChatService.cs (trecho)
private static string ConversaId(string id1, string id2)
{
    var ids = new[] { id1, id2 };
    Array.Sort(ids, StringComparer.Ordinal);
    return $"{ids[0]}_{ids[1]}";
}

public async Task EnviarMensagemAsync(Mensagem mensagem)
{
    var convId = ConversaId(mensagem.RemetenteId, mensagem.DestinatarioId);
    await _db.Child("mensagens").Child(convId).Child(mensagem.Id).PutAsync(mensagem);
}
```

---

---
 
### ValidacaoCpfService — Algoritmo dos Dígitos Verificadores
 
O cadastro de usuários valida o CPF via algoritmo oficial dos dois dígitos verificadores, implementado como serviço estático (`static class`) para ser chamado sem instância.
 
```csharp
// Services/ValidacaoCpfService.cs
public static bool Validar(string? cpf)
{
    var digits = Regex.Replace(cpf, @"\D", "");
 
    if (digits.Length != 11) return false;
    if (new string(digits[0], 11) == digits) return false; // Bloqueia "111.111.111-11" etc.
 
    // Primeiro dígito verificador
    int soma = 0;
    for (int i = 0; i < 9; i++)
        soma += int.Parse(digits[i].ToString()) * (10 - i);
    int resto = soma % 11;
    int d1 = resto < 2 ? 0 : 11 - resto;
    if (d1 != int.Parse(digits[9].ToString())) return false;
 
    // Segundo dígito verificador
    soma = 0;
    for (int i = 0; i < 10; i++)
        soma += int.Parse(digits[i].ToString()) * (11 - i);
    resto = soma % 11;
    int d2 = resto < 2 ? 0 : 11 - resto;
    return d2 == int.Parse(digits[10].ToString());
}
```
 
O service também expõe `Formatar(string? cpf)` que retorna o CPF no padrão `000.000.000-00` caso seja válido, usado na exibição no perfil do usuário.
 
---
 
### UsuarioSessaoService — Singleton de Sessão
 
O `UsuarioSessaoService` é um **Singleton** que mantém o estado do usuário logado durante toda a sessão do aplicativo. Ele também implementa `INotifyPropertyChanged`, permitindo que qualquer ViewModel reaja a mudanças (como troca de foto de perfil) sem precisar ser reconstruído.
 
```csharp
// Services/UsuarioSessaoService.cs
public class UsuarioSessaoService : INotifyPropertyChanged
{
    private static UsuarioSessaoService? _instancia;
    public static UsuarioSessaoService Instancia => _instancia ??= new UsuarioSessaoService();
 
    private string? _fotoCaminho;
    public string? FotoCaminho
    {
        get => _fotoCaminho;
        set { _fotoCaminho = value; OnPropertyChanged(); }
    }
 
    private UsuarioSessaoService() { } // Construtor privado — garante instância única
}
```
 
---
 
### ConfiguracaoLocalService — Persistência de Preferências
 
O `ConfiguracaoLocalService` persiste preferências do usuário (como o caminho da foto de perfil) em disco, no diretório `AppData\Roaming\ReGraphik\config.txt`. Isso garante que a foto seja restaurada automaticamente no próximo login, sem precisar fazer novo upload.
 
```csharp
// Services/ConfiguracaoLocalService.cs
private static readonly string _pasta =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReGraphik");
 
public static void SalvarFoto(string caminho) =>
    File.WriteAllText(Path.Combine(_pasta, "config.txt"), caminho);
 
public static string? CarregarFoto()
{
    var caminho = File.ReadAllText(Path.Combine(_pasta, "config.txt")).Trim();
    return File.Exists(caminho) ? caminho : null; // Só retorna se o arquivo ainda existir
}
```
 
---

### Mapa Interativo — Do clique ao pin

O fluxo completo de busca e exibição de pontos de coleta passa por quatro etapas:

```
1. Usuário digita cidade e clica em "Buscar" (WPF)
         ↓
2. MapaViewModel → POST /api/PontosColeta/sincronizar?cidade=SaoPaulo
         ↓
3. API consulta Google Maps Places, salva no Firebase os pontos novos e retorna os contadores de sincronização
         ↓
4. MapaViewModel busca a lista atualizada via GET /api/PontosColeta e gera HTML com Leaflet.js, salvo em arquivo temporário
         ↓
5. WebView2 renderiza o mapa com os pins na tela do WPF
```

---

### Geração de Relatórios em PDF (QuestPDF)

A tela de Relatórios usa o **QuestPDF** para gerar PDFs com os dados filtrados do estoque. Os filtros disponíveis são: tipo de resíduo, status, origem e intervalo de datas.

```csharp
// RelatorioViewModel.cs (trecho)
private void ExportarPdf()
{
    var saveDialog = new SaveFileDialog { Filter = "PDF|*.pdf" };
    if (saveDialog.ShowDialog() != true) return;

    QuestPDF.Settings.License = LicenseType.Community;

    Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Content().Table(table =>
            {
                // Cabeçalhos e linhas com os resíduos filtrados
            });
        });
    }).GeneratePdf(saveDialog.FileName);
}
```

---

### Módulo ESG

O módulo ESG apresenta os indicadores ambientais da empresa com base nos dados já registrados no sistema — resíduos cadastrados, sugestões aplicadas e pontos de coleta utilizados — e permite exportar um documento com os pilares Ambiental, Social e Governança usando `FlowDocument` e `PrintDialog` do WPF.

---

### Conta / Perfil — Funcionalidades Detalhadas

A tela de Conta gerencia o perfil completo do usuário logado. As propriedades calculadas evitam código na View:

| Propriedade | Comportamento |
|---|---|
| `SemFoto` | `true` quando `ImgFoto == null` — exibe inicial do nome no lugar |
| `InicialNome` | Primeiro caractere do nome em maiúsculo (`Nome[..1].ToUpper()`) |
| `LoginExibicao` | Login formatado com `@` (ex: `@lucas.aquino`) |
| `EmailResumido` | E-mail mascarado (ex: `l*****@regraphik.com.br`) para exibição no card |

O upload de foto segue o fluxo: `OpenFileDialog` → leitura do arquivo → envio para a **Imgur API** → URL retornada salva no Firebase junto ao perfil → caminho local salvo em `ConfiguracaoLocalService` para acesso offline → `UsuarioSessaoService.FotoCaminho` atualizado para propagar para todas as Views abertas.

---

## Status dos Workflows de Status do Resíduo

Os resíduos percorrem um ciclo de vida definido pelos seguintes status, cada um com cor associada na interface:

```
Cadastrado
    ↓
Aguardando Triagem  (#1649a2 — Azul Médio)
    ↓
Disponível          (#64748B — Cinza)
    ├──→ Disponível para Coleta  (#3274ba — Azul Claro)
    │        ↓
    │    Aguardando CADRI        (#0d2a56 — Azul Escuro)
    │
    └──→ Liberado para Venda    (#2f80ec — Azul Vivo)
```

O `StatusToColorConverter` mapeia cada status para sua cor de badge e calcula automaticamente a cor do texto (`Foreground`) para garantir contraste — branco sobre fundos escuros, escuro sobre fundos claros.

---

## Como Executar o Projeto

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) ou superior
- Windows 10/11 (WPF exige Windows)
- Conta no Firebase com Realtime Database configurado
- Chave de API do Google Maps com **Places API** habilitada
- Visual Studio 2022 ou JetBrains Rider

---

### 1. Clonar o repositório

```bash
git clone https://github.com/BrunoMaiaSenai/ReGraphikApp.git
cd ReGraphikApp
```

---

### 2. Configurar a API REST

**2.1 — Adicionar credenciais do Firebase**

Crie um Service Account no Console do Firebase e baixe o arquivo `.json`. Salve-o em:

```
ApiRestReGraphik/ReGraphikFirebaseKey.json
```

**2.2 — Configurar `appsettings.json`**

```json
{
  "Firebase": {
    "RealtimeDatabaseUrl": "https://seu-projeto-default-rtdb.firebaseio.com/",
    "CredentialFilePath": "ReGraphikFirebaseKey.json"
  },
  "GoogleMaps": {
    "ApiKey": "SUA_CHAVE_DO_GOOGLE_MAPS_AQUI"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**2.3 — Executar a API**

```bash
cd ApiRestReGraphik
dotnet run
```

Acesse o Swagger em: `http://localhost:PORT/`

---

### 3. Configurar e executar o cliente WPF

**3.1 — Configurar Firebase no cliente**

O arquivo `FirebaseConfig.cs` usa as mesmas credenciais do Firebase para o chat e para operações diretas no banco. Certifique-se de que o arquivo de credenciais está acessível ao projeto WPF ou configure a URL do banco no código.

**3.2 — Abrir a solução**

1. Abra o arquivo `ReGraphik.slnx` no Visual Studio ou Rider
2. Defina `ReGraphik` como projeto de inicialização
3. Verifique se a API está em execução
4. Pressione `F5` (ou `Run`) para iniciar o cliente

---

### Observações de ambiente

- O cliente WPF requer **Windows 10 ou superior** — WPF não executa em macOS ou Linux.
- O componente **WebView2** (usado no mapa) exige que o Microsoft Edge WebView2 Runtime esteja instalado. Em Windows 11 ele já vem pré-instalado. Em Windows 10, o instalador está disponível em [developer.microsoft.com/microsoft-edge/webview2](https://developer.microsoft.com/microsoft-edge/webview2).
- O arquivo `ReGraphikFirebaseKey.json` (Service Account) **não deve ser versionado**. Adicione ao `.gitignore`:
```
ApiRestReGraphik/ReGraphikFirebaseKey.json
```

- O `appsettings.json` com a chave do Google Maps também não deve ser commitado com a chave real. Use variáveis de ambiente ou `appsettings.Development.json` (que já está no `.gitignore` do projeto).

---

### Ambiente de produção

A API está publicada em: **`https://webregraphik.runasp.net`**

O cliente WPF aponta para esse endereço por padrão. Não é necessário rodar a API localmente para usar o cliente em modo normal.

---

## Documentação Complementar

| Documento | Descrição |
|---|---|
| [Documentação Online (Mintlify)](https://brunomaia.mintlify.app/introduction) | Documentação completa da plataforma e da API em inglês |
| [Quickstart da API](https://brunomaia.mintlify.app/quickstart) | Guia de primeiros passos com a API REST |
| [Autenticação](https://brunomaia.mintlify.app/authentication) | Fluxo completo de cadastro e login |
| [Erros da API](https://brunomaia.mintlify.app/api/errors) | Referência de códigos HTTP e como resolver |
| [MiniMundo e Demanda](./Modelagem/MiniMundo%20Demanda.pdf) | Contexto do negócio e descrição do problema |
| [Modelo Conceitual](./Modelagem/Modelo%20Conceitual.pdf) | Diagrama entidade-relacionamento conceitual |
| [Modelo Lógico](./Modelagem/Modelo%20L%C3%B3gico.pdf) | Estrutura lógica do banco de dados |
| [Modelo Físico](./Modelagem/Modelo%20F%C3%ADsico.pdf) | Script e estrutura física do banco |
| [Documentação do Banco](./Banco%20de%20Dados/Documenta%C3%A7%C3%A3o%20Cria%C3%A7%C3%A3o%20Modelagem.pdf) | Documentação de criação e modelagem |
| [Apresentação Técnica (PPTX)](./ReGraphik_MVVM_APIRest.pptx) | Slides explicando MVVM e a arquitetura da API |
| [TCC — Documentação IntegraSENAI](./ReGraphik_IntegraSenai_Documentacao_TCC_01.pdf) | Documento oficial do TCC |

---

## Integrantes

Projeto desenvolvido por alunos do curso técnico do **SENAI**:

| Nome | GitHub |
|---|---|
| Lucas Aquino Guedes | — |
| Bruno Maia Santos | [@BrunoMaiaSenai](https://github.com/BrunoMaiaSenai) |
| Otavio Henrique Barbosa Soares | — |
| Luna Beatriz Alves | — |
| Kaio Alves Gonzaga Silva | — |

---

<div align="center">

Desenvolvido com foco em **sustentabilidade**, **economia circular** e **boas práticas de engenharia de software** para o setor gráfico.

**SENAI — Trabalho de Conclusão de Curso**

</div>
