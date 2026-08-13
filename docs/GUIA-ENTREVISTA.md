# Como defender o HelpDesk em uma entrevista

## Apresentação em 30 segundos

> O HelpDesk é um sistema full stack em ASP.NET Core para gerenciar chamados de suporte. Ele possui autenticação JWT, autorização para Solicitante, Técnico e Administrador, atribuição de atendimento, comentários, histórico, busca e indicadores. Separei o acesso aos dados com Repository Pattern: localmente há persistência em JSON e, por configuração, a aplicação usa Entity Framework Core com SQL Server e migration. Também criei testes automatizados com xUnit.

## Problema resolvido

O sistema centraliza solicitações que poderiam ficar perdidas em mensagens ou e-mails. Cada chamado possui prioridade, status, responsável, comentários e uma trilha das alterações.

## Fluxo principal

1. O usuário faz login e recebe um JWT.
2. O Solicitante cria um chamado e visualiza somente os próprios registros.
3. Um Técnico assume o chamado, comenta e atualiza o status.
4. O Solicitante acompanha as mudanças.
5. O Administrador possui a permissão adicional de exclusão.

## Como funciona a segurança

- A senha é transformada em hash pelo `PasswordHasher`; texto puro não é armazenado.
- O login devolve um JWT assinado e com validade limitada.
- O token contém identificador, nome, e-mail e perfil.
- O handler valida assinatura, emissor, público e expiração em cada requisição.
- Os atributos `Authorize` protegem rotas e perfis no back-end.
- Esconder um botão na interface melhora a experiência, mas a segurança real fica na API.

## Por que Repository Pattern?

O controller depende de `ITicketRepository` e `IUserRepository`, não de JSON ou SQL Server. A injeção de dependência seleciona a implementação durante a inicialização. Isso reduz acoplamento e facilita trocar infraestrutura e testar regras.

## JSON é banco SQL?

Não. JSON é a persistência local usada para executar a demonstração sem instalações. O caminho empresarial usa `EfTicketRepository`, Entity Framework Core, SQL Server e migration. Dizer isso com clareza mostra maturidade técnica.

## Como a persistência local evita problemas?

- `SemaphoreSlim` impede duas gravações simultâneas.
- Um arquivo temporário é escrito antes da substituição do arquivo principal.
- Comentários e eventos recebem identificadores próprios.
- A pasta com dados locais é ignorada pelo Git.

## Auditoria

O histórico registra quem realizou a ação, quando ocorreu e quais informações mudaram. A versão atual registra criação, atribuição, comentário e atualização de chamado.

## Códigos HTTP importantes

- `200 OK`: consulta ou login realizado.
- `201 Created`: chamado ou comentário criado.
- `204 No Content`: atualização, atribuição ou exclusão concluída.
- `400 Bad Request`: dados inválidos.
- `401 Unauthorized`: token ausente, inválido ou expirado.
- `403 Forbidden`: usuário autenticado sem permissão.
- `404 Not Found`: recurso não encontrado.

## Testes

Os testes com xUnit verificam persistência após uma nova instância, filtros combinados, identificadores de comentários/histórico e bloqueio de e-mails duplicados. Além disso, o fluxo integrado dos três perfis foi validado via HTTP.

## Decisões conscientes

- O JSON simplifica a demonstração, mas SQL Server deve ser usado em produção.
- `localStorage` simplifica o front-end; um sistema de maior risco pode usar cookie `HttpOnly` com proteção CSRF.
- Contas demo só são criadas no ambiente de desenvolvimento.
- Chaves JWT e strings de conexão reais devem vir de variáveis de ambiente.
- A exclusão é restrita ao Administrador.

## Perguntas que podem aparecer

### Por que JWT?

Porque a API consegue validar a identidade sem manter uma sessão em memória. Isso facilita consumidores diferentes e escalabilidade horizontal.

### Por que não confiar apenas na tela?

Porque qualquer pessoa pode chamar a API diretamente. Toda autorização importante é validada novamente no servidor.

### Por que métodos assíncronos?

Para não ocupar uma thread enquanto a aplicação aguarda arquivo ou banco de dados.

### Por que DTOs?

Para controlar exatamente quais dados o cliente pode fornecer. Na criação, por exemplo, nome e e-mail vêm do JWT, não do corpo da requisição.

### O que você faria depois?

Validaria a migration em SQL Server real, criaria refresh tokens, recuperação de senha, paginação, testes de integração automatizados, pipeline de CI e publicação online.
