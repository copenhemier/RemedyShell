# RemedyShell

A command-line shell for Windows with built-in file encryption. RemedyShell is focused on three things: protection, encryption, and trust. It is open source, works entirely offline, and uses standard, widely-used cryptographic algorithms.

## What it does

- Runs as a normal command-line shell with built-in commands and external program support
- Encrypts and decrypts files with a password using AES-256-GCM
- Keeps your original file by default when encrypting (no accidental data loss while you learn the tool)
- Detects wrong passwords and tampered files cleanly, without producing garbage output
- Operates fully offline. No network, no telemetry, no accounts, no cloud

## Built-in commands

- `cd [dir]` — change directory (no argument goes to home)
- `pwd` — print the current directory
- `history` — show previously typed commands
- `clear` / `cls` — clear the screen
- `encrypt <file>` — encrypt a file with a password
- `decrypt <file>` — decrypt a `.rmdy` file
- `help` — show built-in commands
- `exit` / `quit` — leave the shell

Anything not matching a built-in is run as an external program. Up and down arrows recall previous commands.

## Requirements

- Windows
- .NET 8 SDK (to build from source)

## Building and running

1. Clone or download this repository
2. Open `RemedyShellv1.csproj` in Visual Studio 2022
3. Build with Ctrl+Shift+B
4. Run with Ctrl+F5

A standalone executable is also available on the Releases page if you do not want to build from source.

## Security

RemedyShell uses AES-256-GCM for file encryption and PBKDF2 (SHA-256, 300,000 iterations) for deriving keys from passwords. These are standard, widely-used algorithms provided by the .NET cryptography library. RemedyShell does not invent its own encryption.

- All cryptographic operations happen locally on your computer
- No file contents, passwords, or other data are ever sent over a network
- There is no master key, no backdoor, and no recovery mechanism. If a password is lost, the encrypted file cannot be recovered

### What it protects against

- Keeping personal files private from someone who gains access to your computer
- Protecting files on a lost or stolen drive

### What it does not protect against

RemedyShell is an open-source hobby project and has not been independently security-audited. It should not be relied upon as the sole protection for critical data, nor against sophisticated or state-level adversaries. Always keep backups of important files.

## License

MIT License — see the LICENSE file.

## Roadmap

Planned additions:

- `hash <file>` and `verify <file> <hash>` for integrity checking
- `genpass` for generating strong random passwords
- A `security` command that prints this information from inside the shell
- Tab completion for file paths
- Built-in `ls` with colors
- Aliases, persistent history, and a config file
- File integrity monitoring (`watch`)

## Source

The full source code is available in this repository. You are encouraged to read it rather than take any security claim on trust.
