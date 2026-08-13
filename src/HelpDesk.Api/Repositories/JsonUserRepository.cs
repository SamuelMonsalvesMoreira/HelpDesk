using System.Text.Json;
using System.Text.Json.Serialization;
using HelpDesk.Api.Models;

namespace HelpDesk.Api.Repositories;

public sealed class JsonUserRepository : IUserRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonUserRepository(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredPath = configuration["Storage:JsonUsersFilePath"] ?? "App_Data/users.json";
        _filePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            return await ReadAllAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var users = await ReadAllAsync(cancellationToken);
            return users.FirstOrDefault(user => user.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var users = await ReadAllAsync(cancellationToken);
            return users.FirstOrDefault(user =>
                user.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var users = await ReadAllAsync(cancellationToken);

            if (users.Any(current => current.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Já existe um usuário com este e-mail.");
            }

            user.Id = users.Count == 0 ? 1 : users.Max(current => current.Id) + 1;
            users.Add(user);
            await WriteAllAsync(users, cancellationToken);
            return user;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<User>> ReadAllAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<List<User>>(stream, _jsonOptions, cancellationToken)
            ?? [];
    }

    private async Task WriteAllAsync(List<User> users, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_filePath)
            ?? throw new InvalidOperationException("Não foi possível determinar a pasta de usuários.");

        Directory.CreateDirectory(directory);
        var temporaryPath = $"{_filePath}.tmp";

        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, users, _jsonOptions, cancellationToken);
        }

        File.Move(temporaryPath, _filePath, true);
    }
}
