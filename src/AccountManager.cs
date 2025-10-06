using System.Text.Json;

namespace OdinVault;

public sealed class AccountManager(OdinVaultOptions options)
{
    private FileUser? user;

    public string? GetFilePath()
    {
        return user?.Path;
    }
    
    public bool IsSignedIn()
    {
        return user is not null;
    }
    
    public Task<Result> SignInSanitizedAsync(string path, string password)
    {
        if (path.Contains('/') || path.Contains('\\'))
            return Task.FromResult(Result.Fail("File can not contain '/' or '\\'"));
        
        path = Path.Combine(options.VaultPath ?? "", path + ".ov");
        return SignInAsync(path, password);
    }
    
    public async Task<Result> SignInAsync(string path, string password)
    {
        path = Path.GetFullPath(path);
        if (!File.Exists(path))
            return Result.Fail("File doesn't exist.");
        
        var passwordHash = Encryption.HashPassword(password);
        try
        {
            var contents = await ReadFileAsync(path, passwordHash);
            var data = JsonSerializer.Deserialize<List<AccountRecord>>(contents, JsonSerializerOptions.Web);
            if (data is null)
                return Result.Fail("Invalid file contents, try another file.");
        }
        catch
        {
            return Result.Fail("Invalid password or file contents, try another password.");
        }
        
        user = new FileUser(path, passwordHash);
        return Result.Success();
    }

    public void SignOut()
    {
        user = null;
    }
    
    public Task<Result> CreateFileSanitizedAsync(string path, string password)
    {
        if (path.Contains('/') || path.Contains('\\'))
            return Task.FromResult(Result.Fail("File can not contain '/' or '\\'"));
        
        if (!string.IsNullOrWhiteSpace(options.VaultPath))
            Directory.CreateDirectory(options.VaultPath);
        
        path = Path.Combine(options.VaultPath ?? "", path + ".ov");
        return CreateFileAsync(path, password);
    }

    public async Task<Result> CreateFileAsync(string path, string password)
    {
        if (File.Exists(path))
            return Result.Fail("File already exists.");
        
        try
        {
            var passwordHash = Encryption.HashPassword(password);
            await WriteFileAsync(path, "[]", passwordHash);
            return Result.Success();
        }
        catch
        {
            return Result.Fail("Something went wrong, try another name.");
        }
    }

    public Task<Result> UpdateFileSanitizedAsync(string? newPath, string? currentPassword, string? newPassword)
    {
        if (!string.IsNullOrWhiteSpace(newPath))
        {
            if (newPath.Contains('/') || newPath.Contains('\\'))
                return Task.FromResult(Result.Fail("File can not contain '/' or '\\'"));
            
            if (!string.IsNullOrWhiteSpace(options.VaultPath))
                Directory.CreateDirectory(options.VaultPath);
            
            newPath = Path.Combine(options.VaultPath ?? "", newPath + ".ov");
        }
        
        return UpdateFileAsync(newPath, currentPassword, newPassword);
    }
    
    public async Task<Result> UpdateFileAsync(string? newPath, string? currentPassword, string? newPassword)
    {
        if (user is null)
            return Result.Fail("You are not signed in.");

        if (!string.IsNullOrEmpty(newPassword) && string.IsNullOrEmpty(currentPassword))
            return Result.Fail("You need to provide the current and new password to change password.");

        if (newPath is null && currentPassword is null && newPassword is null)
            return Result.Success();
        
        string path = user.Path;
        string password = user.Password;
        
        // Change password.
        if (!string.IsNullOrEmpty(newPassword) && !string.IsNullOrEmpty(currentPassword))
        {
            var currentPasswordHash = Encryption.HashPassword(currentPassword);
            if (user.Password != currentPasswordHash)
                return Result.Fail("Current password is not correct.");
            
            try
            {
                _ = await ReadFileAsync(path, currentPasswordHash);
            }
            catch
            {
                return Result.Fail("Current password is not correct.");
            }
            
            password = Encryption.HashPassword(newPassword);
        }
        
        // Change path.
        bool changePath = false;
        var newFullPath = !string.IsNullOrEmpty(newPath) ? Path.GetFullPath(newPath) : null;
        if (newFullPath is not null && user.Path != newFullPath)
        {
            if (File.Exists(newFullPath))
                return Result.Fail("File already exists, try another name.");
            
            path = newFullPath;
            changePath = true;
        }
        
        // Update.
        string? contents;
        try
        {
            contents = await ReadFileAsync(user.Path, user.Password);
        }
        catch
        {
            return Result.Fail("Failed to read the file.");
        }
        
        // Save new file/password.
        await WriteFileAsync(path, contents, password);

        // Remove old.
        if (changePath && File.Exists(user.Path))
            File.Delete(user.Path);
        
        user = new FileUser(path, password);
        
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
            var contents = await ReadFileAsync(user.Path, user.Password);
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
        await WriteFileAsync(user.Path, contents, user.Password);
        return Result.Success();
    }

    private static async Task<string> ReadFileAsync(string path, string password)
    {
        var contents = await File.ReadAllBytesAsync(path);
        
        var extractedIV = Encryption.ExtractIV(contents);
        var (decryptKey, _) = Encryption.GenerateKeyFromPassword(password, extractedIV);
        var decrypted = Encryption.Decrypt(contents, decryptKey);
        
        return decrypted;
    }
    
    private static async Task WriteFileAsync(string path, string contents, string password)
    {
        var (encryptKey, iv) = Encryption.GenerateKeyFromPassword(password);
        var encrypted = Encryption.EncryptToBytes(contents, encryptKey, iv);
        
        await File.WriteAllBytesAsync(path, encrypted);
    }
}

public sealed record FileUser(string Path, string Password);