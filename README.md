# HelpDesk

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-5C2D91)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens)
![Entity Framework Core](https://img.shields.io/badge/Entity_Framework-Core-6C3483)
![SQL Server](https://img.shields.io/badge/SQL_Server-ready-CC2927?logo=microsoftsqlserver)
![Tests](https://img.shields.io/badge/tests-4_passing-16a34a)

Sistema full stack para gerenciamento de chamados de suporte tÃ©cnico. O projeto demonstra uma aplicaÃ§Ã£o profissional em C# e ASP.NET Core com autenticaÃ§Ã£o JWT, autorizaÃ§Ã£o por perfil, API REST, persistÃªncia, histÃ³rico de auditoria, testes automatizados e suporte ao Microsoft SQL Server.

## Destaques da versÃ£o 5

- Login seguro com JWT assinado e expiraÃ§Ã£o
- Senhas protegidas com `PasswordHasher` do ASP.NET Core
- Perfis de Solicitante, TÃ©cnico e Administrador
- AutorizaÃ§Ã£o aplicada tanto na interface quanto na API
- AtribuiÃ§Ã£o de chamados a tÃ©cnicos
- ComentÃ¡rios e histÃ³rico completo das alteraÃ§Ãµes
- PersistÃªncia local entre reinicializaÃ§Ãµes
- Entity Framework Core e migration inicial para SQL Server
- Busca e filtros combinados
- Indicadores por perfil de usuÃ¡rio
- Testes automatizados com xUnit
- Interface responsiva com tela de login

## DemonstraÃ§Ã£o dos perfis

| Perfil | PermissÃµes |
|---|---|
| Solicitante | Cria chamados, visualiza apenas os prÃ³prios e adiciona comentÃ¡rios |
| TÃ©cnico | Visualiza todos, recebe atribuiÃ§Ãµes, comenta e altera prioridade/status |
| Administrador | Possui as permissÃµes do tÃ©cnico e pode excluir chamados |

### Contas locais de demonstraÃ§Ã£o

As contas abaixo sÃ£o criadas somente no ambiente `Development`:

| Perfil | E-mail | Senha |
|---|---|---|
| Administrador | `admin@helpdesk.local` | `Admin@123` |
| TÃ©cnico | `tecnico@helpdesk.local` | `Tecnico@123` |
| Solicitante | `usuario@helpdesk.local` | `Usuario@123` |

Essas credenciais sÃ£o apenas para demonstraÃ§Ã£o local. `SeedDemoUsers` vem desativado na configuraÃ§Ã£o padrÃ£o de produÃ§Ã£o.

## Tecnologias

- C# e .NET 10
- ASP.NET Core Web API
- AutenticaÃ§Ã£o e autorizaÃ§Ã£o JWT
- ASP.NET Core Identity Password Hasher
- Entity Framework Core
- Microsoft SQL Server
- xUnit
- HTML, CSS e JavaScript
- PersistÃªncia JSON para execuÃ§Ã£o local sem instalaÃ§Ãµes adicionais
- Git e GitHub

## Arquitetura

```text
Interface web
    |
    | HTTP + JSON + Bearer Token
    v
Authentication Handler + Authorization Policies
    |
    v
Controllers
    |
    +--> IUserRepository
    |
    +--> ITicketRepository
             |
             +--> JSON local
             |
             +--> Entity Framework Core --> SQL Server
```

Os controllers dependem de interfaces, nÃ£o de uma tecnologia especÃ­fica de armazenamento. A injeÃ§Ã£o de dependÃªncia seleciona os repositÃ³rios JSON ou SQL Server durante a inicializaÃ§Ã£o.

## Estrutura

```text
HelpDesk/
|-- src/HelpDesk.Api/
|   |-- Authentication/  # JWT, claims e validaÃ§Ã£o do token
|   |-- Controllers/     # Endpoints e regras de autorizaÃ§Ã£o
|   |-- Data/            # DbContext, seed e migrations
|   |-- Dtos/            # Contratos e validaÃ§Ãµes
|   |-- Models/          # Entidades e enums
|   |-- Repositories/    # PersistÃªncia JSON e SQL Server
|   `-- wwwroot/         # Interface web responsiva
|-- tests/HelpDesk.Api.Tests/
|   `-- Testes automatizados dos repositÃ³rios
|-- docs/
|   `-- Guia para entrevistas
`-- CHANGELOG.md
```

## Executar localmente

### Requisito

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Comandos

```powershell
git clone https://github.com/SamuelMonsalvesMoreira/HelpDesk.git
cd HelpDesk
dotnet restore
dotnet run --project src/HelpDesk.Api
```

O navegador abre automaticamente. Se necessÃ¡rio, acesse:

```text
http://localhost:5269
```

No ambiente de desenvolvimento, usuÃ¡rios e chamados sÃ£o gravados em `src/HelpDesk.Api/App_Data`. Os arquivos continuam salvos apÃ³s reiniciar a aplicaÃ§Ã£o e nÃ£o sÃ£o enviados ao GitHub.

## Executar os testes

```powershell
dotnet test
```

Os testes atuais verificam:

- PersistÃªncia apÃ³s criar uma nova instÃ¢ncia do repositÃ³rio
- CombinaÃ§Ã£o de busca, status e prioridade
- Identificadores de comentÃ¡rios e histÃ³rico
- Bloqueio de e-mails duplicados

## SQL Server

O desenvolvimento local usa JSON para funcionar sem instalaÃ§Ã£o adicional. Para utilizar SQL Server:

1. Disponibilize uma instÃ¢ncia do SQL Server.
2. Configure `ConnectionStrings:SqlServer` com variÃ¡vel de ambiente ou User Secrets.
3. Defina `StorageProvider` como `SqlServer`.
4. Defina uma chave JWT segura em `Jwt:Key`.
5. Inicie a aplicaÃ§Ã£o.

Quando SQL Server estÃ¡ selecionado, a aplicaÃ§Ã£o executa `Database.MigrateAsync()` e aplica a migration inicial.

Exemplo de variÃ¡veis no PowerShell:

```powershell
$env:StorageProvider = "SqlServer"
$env:ConnectionStrings__SqlServer = "Server=localhost;Database=HelpDeskDb;Trusted_Connection=True;TrustServerCertificate=True"
$env:Jwt__Key = "UMA_CHAVE_SECRETA_COM_PELO_MENOS_32_BYTES"
dotnet run --project src/HelpDesk.Api
```

Nunca publique senhas, chaves JWT ou strings de conexÃ£o reais no repositÃ³rio.

## Principais endpoints

| MÃ©todo | Endpoint | Acesso | FunÃ§Ã£o |
|---|---|---|---|
| `POST` | `/api/auth/login` | PÃºblico | Autentica e retorna um JWT |
| `GET` | `/api/auth/me` | Autenticado | Retorna o usuÃ¡rio atual |
| `GET` | `/api/tickets` | Autenticado | Lista chamados permitidos ao perfil |
| `GET` | `/api/tickets/summary` | Autenticado | Retorna indicadores permitidos ao perfil |
| `POST` | `/api/tickets` | Autenticado | Cria um chamado |
| `PUT` | `/api/tickets/{id}` | TÃ©cnico/Admin | Atualiza dados, prioridade e status |
| `PATCH` | `/api/tickets/{id}/assign` | TÃ©cnico/Admin | Atribui um tÃ©cnico |
| `POST` | `/api/tickets/{id}/comments` | Autenticado | Adiciona comentÃ¡rio |
| `DELETE` | `/api/tickets/{id}` | Administrador | Exclui um chamado |
| `GET` | `/api/users?role=Technician` | TÃ©cnico/Admin | Lista tÃ©cnicos ativos |

Busca, status e prioridade podem ser combinados:

```text
GET /api/tickets?search=impressora&status=Open&priority=High
```

## Exemplo de login

```json
{
  "email": "usuario@helpdesk.local",
  "password": "Usuario@123"
}
```

## Exemplo de criaÃ§Ã£o

O nome e o e-mail do solicitante sÃ£o obtidos do token, e nÃ£o aceitos livremente no JSON:

```json
{
  "title": "Impressora nÃ£o funciona",
  "description": "A impressora do financeiro aparece como offline.",
  "priority": "High"
}
```

## DecisÃµes tÃ©cnicas para explicar em entrevistas

- **JWT:** identifica o usuÃ¡rio sem armazenar sessÃ£o no servidor.
- **AutorizaÃ§Ã£o por perfil:** impede aÃ§Ãµes indevidas no back-end, mesmo que alguÃ©m tente ignorar a interface.
- **Password hashing:** nenhuma senha Ã© armazenada em texto puro.
- **Repository Pattern:** desacopla a API da infraestrutura de dados.
- **DTOs:** evitam que clientes controlem campos internos.
- **HistÃ³rico:** registra criaÃ§Ã£o, atribuiÃ§Ã£o, comentÃ¡rios e alteraÃ§Ãµes.
- **OperaÃ§Ãµes assÃ­ncronas:** evitam bloquear threads durante entrada e saÃ­da.
- **Controle de concorrÃªncia:** protege as gravaÃ§Ãµes no arquivo local.
- **Migration:** permite versionar e reproduzir o esquema SQL Server.
- **Testes automatizados:** protegem persistÃªncia, filtros e regras do repositÃ³rio.

## LimitaÃ§Ãµes conscientes

- O armazenamento JSON Ã© adequado para demonstraÃ§Ã£o local, nÃ£o para mÃºltiplas instÃ¢ncias em produÃ§Ã£o.
- A migration SQL Server estÃ¡ preparada, mas exige uma instÃ¢ncia instalada para validaÃ§Ã£o de ponta a ponta.
- O token fica no `localStorage` nesta demonstraÃ§Ã£o; aplicaÃ§Ãµes com maior risco podem preferir cookies `HttpOnly` e proteÃ§Ã£o CSRF.
- Ainda nÃ£o existem recuperaÃ§Ã£o de senha, refresh token e paginaÃ§Ã£o.

## Roadmap

- [x] CRUD de chamados
- [x] Interface responsiva
- [x] PersistÃªncia local
- [x] Repository Pattern
- [x] Busca, filtros e indicadores
- [x] JWT e perfis de acesso
- [x] AtribuiÃ§Ã£o, comentÃ¡rios e histÃ³rico
- [x] Testes automatizados
- [x] Migration inicial do SQL Server
- [ ] Validar em uma instÃ¢ncia real do SQL Server
- [ ] Adicionar refresh token e recuperaÃ§Ã£o de senha
- [ ] Criar paginaÃ§Ã£o
- [ ] Adicionar pipeline de integraÃ§Ã£o contÃ­nua
- [ ] Publicar uma demonstraÃ§Ã£o online

## DocumentaÃ§Ã£o adicional

- [HistÃ³rico de versÃµes](CHANGELOG.md)
- [Guia para entrevistas](docs/GUIA-ENTREVISTA.md)

## Objetivo

Demonstrar competÃªncias relevantes para vagas de desenvolvimento C#/.NET: modelagem, API REST, autenticaÃ§Ã£o, autorizaÃ§Ã£o, Entity Framework Core, SQL Server, persistÃªncia, testes, seguranÃ§a, arquitetura, Git e documentaÃ§Ã£o tÃ©cnica.

