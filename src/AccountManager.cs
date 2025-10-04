using System.Text.Json;

namespace OdinVault;

public sealed class AccountManager(OdinVaultOptions options)
{
    private AccountUser? user;

    public string? GetUserPath()
    {
        return user?.Path;
    }
    
    public bool IsSignedIn()
    {
        return user is not null;
    }
    
    public Task<Result> SignInSanitizedAsync(string path)
    {
        if (path.Contains('/') || path.Contains('\\'))
            return Task.FromResult(Result.Fail("File can not contain '/' or '\\'"));
        
        path = Path.Combine(options.VaultPath ?? "", path + ".ov");
        return SignInAsync(path);
    }
    
    public async Task<Result> SignInAsync(string path)
    {
        if (!File.Exists(path))
            return Result.Fail("File doesn't exist.");
        
        try
        {
            var contents = await File.ReadAllTextAsync(path);
            var data = JsonSerializer.Deserialize<List<AccountRecord>>(contents, JsonSerializerOptions.Web);
            if (data == null)
                return Result.Fail("Invalid file contents, try another file.");
        }
        catch
        {
            return Result.Fail("Invalid file contents, try another file.");
        }
        
        user = new AccountUser(path);
        return Result.Success();
    }

    public void SignOut()
    {
        user = null;
    }
    
    public Task<Result> CreateFileSanitizedAsync(string path)
    {
        if (path.Contains('/') || path.Contains('\\'))
            return Task.FromResult(Result.Fail("File can not contain '/' or '\\'"));
        
        if (!string.IsNullOrWhiteSpace(options.VaultPath))
            Directory.CreateDirectory(options.VaultPath);
        
        path = Path.Combine(options.VaultPath ?? "", path + ".ov");
        return CreateFileAsync(path);
    }

    public async Task<Result> CreateFileAsync(string path)
    {
        if (File.Exists(path))
            return Result.Fail("File already exists.");

        var contents = "[]";
        await File.WriteAllTextAsync(path, contents);
        return Result.Success();
    }

    public async Task<Result<List<AccountRecord>>> GetAccountsAsync()
    {
        if (user is null)
            return Result<List<AccountRecord>>.Fail("You are not signed in.");

        if (!File.Exists(user.Path))
            return Result<List<AccountRecord>>.Fail("The file doesn't exist.");
        
        try
        {
            var contents = await File.ReadAllTextAsync(user.Path);
            return JsonSerializer.Deserialize<List<AccountRecord>>(contents, JsonSerializerOptions.Web)!;
        }
        catch
        {
            return Result<List<AccountRecord>>.Fail("Failed to read the file.");
        }
    }

    public async Task<Result> SaveAccountsAsync(IEnumerable<AccountRecord> accounts)
    {
        if (user is null)
            return Result.Fail("You are not signed in.");

        var contents = JsonSerializer.Serialize(accounts, JsonSerializerOptions.Web);
        await File.WriteAllTextAsync(user.Path, contents);
        return Result.Success();
    }
}

public sealed record AccountUser(string Path);