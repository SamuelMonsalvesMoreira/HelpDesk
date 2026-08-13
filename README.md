# HelpDesk

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-5C2D91)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens)
![Entity Framework Core](https://img.shields.io/badge/Entity_Framework-Core-6C3483)
![SQL Server](https://img.shields.io/badge/SQL_Server-ready-CC2927?logo=microsoftsqlserver)
![Tests](https://img.shields.io/badge/tests-4_passing-16a34a)

Sistema full stack para gerenciamento de chamados de suporte técnico. O projeto demonstra uma aplicação profissional em C# e ASP.NET Core com autenticação JWT, autorização por perfil, API REST, persistência, histórico de auditoria, testes automatizados e suporte ao Microsoft SQL Server.

## Destaques da versão 5

- Login seguro com JWT assinado e expiração
- Senhas protegidas com `PasswordHasher` do ASP.NET Core
- Perfis de Solicitante, Técnico e Administrador
- Autorização aplicada tanto na interface quanto na API
- Atribuição de chamados a técnicos
- Comentários e histórico completo das alterações
- Persistência local entre reinicializações
- Entity Framework Core e migration inicial para SQL Server
- Busca e filtros combinados
- Indicadores por perfil de usuário
- Testes automatizados com xUnit
- Interface responsiva com tela de login

## Demonstração dos perfis

| Perfil | Permissões |
|---|---|
| Solicitante | Cria chamados, visualiza apenas os próprios e adiciona comentários |
| Técnico | Visualiza todos, recebe atribuições, comenta e altera prioridade/status |
| Administrador | Possui as permissões do técnico e pode excluir chamados |

### Contas locais de demonstração

As contas abaixo são criadas somente no ambiente `Development`:

| Perfil | E-mail | Senha |
|---|---|---|
| Administrador | `admin@helpdesk.local` | `Admin@123` |
| Técnico | `tecnico@helpdesk.local` | `Tecnico@123` |
| Solicitante | `usuario@helpdesk.local` | `Usuario@123` |

Essas credenciais são apenas para demonstração local. `SeedDemoUsers` vem desativado na configuração padrão de produção.

## Tecnologias

- C# e .NET 10
- ASP.NET Core Web API
- Autenticação e autorização JWT
- ASP.NET Core Identity Password Hasher
- Entity Framework Core
- Microsoft SQL Server
- xUnit
- HTML, CSS e JavaScript
- Persistência JSON para execução local sem instalações adicionais
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

Os controllers dependem de interfaces, não de uma tecnologia específica de armazenamento. A injeção de dependência seleciona os repositórios JSON ou SQL Server durante a inicialização.

## Estrutura

```text
HelpDesk/
|-- src/HelpDesk.Api/
|   |-- Authentication/  # JWT, claims e validação do token
|   |-- Controllers/     # Endpoints e regras de autorização
|   |-- Data/            # DbContext, seed e migrations
|   |-- Dtos/            # Contratos e validações
|   |-- Models/          # Entidades e enums
|   |-- Repositories/    # Persistência JSON e SQL Server
|   `-- wwwroot/         # Interface web responsiva
|-- tests/HelpDesk.Api.Tests/
|   `-- Testes automatizados dos repositórios
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

O navegador abre automaticamente. Se necessário, acesse:

```text
http://localhost:5269
```

No ambiente de desenvolvimento, usuários e chamados são gravados em `src/HelpDesk.Api/App_Data`. Os arquivos continuam salvos após reiniciar a aplicação e não são enviados ao GitHub.

## Executar os testes

```powershell
dotnet test
```

Os testes atuais verificam:

- Persistência após criar uma nova instância do repositório
- Combinação de busca, status e prioridade
- Identificadores de comentários e histórico
- Bloqueio de e-mails duplicados

## SQL Server

O desenvolvimento local usa JSON para funcionar sem instalação adicional. Para utilizar SQL Server:

1. Disponibilize uma instância do SQL Server.
2. Configure `ConnectionStrings:SqlServer` com variável de ambiente ou User Secrets.
3. Defina `StorageProvider` como `SqlServer`.
4. Defina uma chave JWT segura em `Jwt:Key`.
5. Inicie a aplicação.

Quando SQL Server está selecionado, a aplicação executa `Database.MigrateAsync()` e aplica a migration inicial.

Exemplo de variáveis no PowerShell:

```powershell
$env:StorageProvider = "SqlServer"
$env:ConnectionStrings__SqlServer = "Server=localhost;Database=HelpDeskDb;Trusted_Connection=True;TrustServerCertificate=True"
$env:Jwt__Key = "UMA_CHAVE_SECRETA_COM_PELO_MENOS_32_BYTES"
dotnet run --project src/HelpDesk.Api
```

Nunca publique senhas, chaves JWT ou strings de conexão reais no repositório.

## Principais endpoints

| Método | Endpoint | Acesso | Função |
|---|---|---|---|
| `POST` | `/api/auth/login` | Público | Autentica e retorna um JWT |
| `GET` | `/api/auth/me` | Autenticado | Retorna o usuário atual |
| `GET` | `/api/tickets` | Autenticado | Lista chamados permitidos ao perfil |
| `GET` | `/api/tickets/summary` | Autenticado | Retorna indicadores permitidos ao perfil |
| `POST` | `/api/tickets` | Autenticado | Cria um chamado |
| `PUT` | `/api/tickets/{id}` | Técnico/Admin | Atualiza dados, prioridade e status |
| `PATCH` | `/api/tickets/{id}/assign` | Técnico/Admin | Atribui um técnico |
| `POST` | `/api/tickets/{id}/comments` | Autenticado | Adiciona comentário |
| `DELETE` | `/api/tickets/{id}` | Administrador | Exclui um chamado |
| `GET` | `/api/users?role=Technician` | Técnico/Admin | Lista técnicos ativos |

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

## Exemplo de criação

O nome e o e-mail do solicitante são obtidos do token, e não aceitos livremente no JSON:

```json
{
  "title": "Impressora não funciona",
  "description": "A impressora do financeiro aparece como offline.",
  "priority": "High"
}
```

## Decisões técnicas para explicar em entrevistas

- **JWT:** identifica o usuário sem armazenar sessão no servidor.
- **Autorização por perfil:** impede ações indevidas no back-end, mesmo que alguém tente ignorar a interface.
- **Password hashing:** nenhuma senha é armazenada em texto puro.
- **Repository Pattern:** desacopla a API da infraestrutura de dados.
- **DTOs:** evitam que clientes controlem campos internos.
- **Histórico:** registra criação, atribuição, comentários e alterações.
- **Operações assíncronas:** evitam bloquear threads durante entrada e saída.
- **Controle de concorrência:** protege as gravações no arquivo local.
- **Migration:** permite versionar e reproduzir o esquema SQL Server.
- **Testes automatizados:** protegem persistência, filtros e regras do repositório.

## Limitações conscientes

- O armazenamento JSON é adequado para demonstração local, não para múltiplas instâncias em produção.
- A migration SQL Server está preparada, mas exige uma instância instalada para validação de ponta a ponta.
- O token fica no `localStorage` nesta demonstração; aplicações com maior risco podem preferir cookies `HttpOnly` e proteção CSRF.
- Ainda não existem recuperação de senha, refresh token e paginação.

## Roadmap

- [x] CRUD de chamados
- [x] Interface responsiva
- [x] Persistência local
- [x] Repository Pattern
- [x] Busca, filtros e indicadores
- [x] JWT e perfis de acesso
- [x] Atribuição, comentários e histórico
- [x] Testes automatizados
- [x] Migration inicial do SQL Server
- [ ] Validar em uma instância real do SQL Server
- [ ] Adicionar refresh token e recuperação de senha
- [ ] Criar paginação
- [ ] Adicionar pipeline de integração contínua
- [ ] Publicar uma demonstração online

## Documentação adicional

- [Histórico de versões](CHANGELOG.md)
- [Guia para entrevistas](docs/GUIA-ENTREVISTA.md)

## Objetivo

Demonstrar competências relevantes para vagas de desenvolvimento C#/.NET: modelagem, API REST, autenticação, autorização, Entity Framework Core, SQL Server, persistência, testes, segurança, arquitetura, Git e documentação técnica.
