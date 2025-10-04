namespace OdinVault;

public sealed class AccountRecord
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string? LogoUrl { get; set; }
    public required string Domain { get; set; }
    public required string Username { get; set; }
    public string? Password { get; set; }
    public string? Notes { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}