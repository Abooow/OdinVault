# Odin Vault
A self-hosted password/account manager built with ASP.NET Core and Blazor Server.

> [!NOTE]
> This is a personal project, built for learning purposes and not intended for production use.

___

## What it does
Odin Vault lets you store and manage account credentials locally. You can save details like site URLs, usernames, passwords, and notes - all encrypted and stored in a single file on your machine.

___

## How it works
### Vaults
Each vault is a `.ov file` (Odin Vault format) stored locally. When you create a vault named `MyVault`, it's saved as `MyVault.ov`. The filename acts as the sign-in identifier, and access is protected by a password.
Once signed in, you can create account entries with the following fields:
* Site URL / App name
* Username / Email
* Password
* Notes / Description

All entries are serialized as JSON and encrypted before being written to the vault file.

### Encryption
Encryption is handled using `System.Security.Cryptography` with AES.
* The password is hashed with **SHA-256** and then derived into an AES key using **PBKDF2** (`Rfc2898DeriveBytes`) with SHA-256 and 10 000 iterations.
* A random **16-byte IV** is generated via `RandomNumberGenerator` for each write operation.
* The IV is prepended to the encrypted file so it can be extracted during decryption.
* The encrypted content is stored as raw bytes in the `.ov` file.

This means the vault file is unreadable without the correct password, and each save produces a different ciphertext due to the randomized IV.

___

## Stack
* ASP.NET Core - backend
* Blazor Server - interactive UI
