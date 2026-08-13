# Histórico de versões

## Versão 5

- Login com JWT assinado e expiração
- Senhas protegidas com `PasswordHasher`
- Perfis de Solicitante, Técnico e Administrador
- Autorização das rotas por perfil
- Isolamento dos chamados do Solicitante
- Atribuição de chamados a técnicos
- Comentários e histórico de auditoria
- Tela de login e interface adaptada às permissões
- Repositório de usuários para JSON e SQL Server
- Migration inicial do SQL Server
- Quatro testes automatizados com xUnit
- README e guia de entrevista atualizados

## Versão 4

- Persistência local entre reinicializações
- Repository Pattern com `ITicketRepository`
- Implementação local em JSON com controle de concorrência
- Implementação alternativa para Entity Framework Core e SQL Server
- Busca por texto
- Filtros combinados por status e prioridade
- Endpoint de indicadores
- Interface atualizada com busca e filtros
- README e guia de entrevista revisados

## Versão 3

- README profissional para publicação no GitHub
- Documentação de arquitetura, endpoints e decisões técnicas

## Versão 2

- Interface web responsiva
- Cadastro e acompanhamento de chamados pelo navegador

## Versão 1

- API REST em ASP.NET Core
- CRUD de chamados
- Validação de entrada
- Entity Framework Core preparado para SQL Server
