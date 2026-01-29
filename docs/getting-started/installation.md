---
layout: default
title: Installation
parent: Getting Started
nav_order: 1
---

# Installation

This guide covers installing and building the OPAL compiler.

---

## Prerequisites

Before installing OPAL, ensure you have:

| Requirement | Version | Check Command |
|:------------|:--------|:--------------|
| .NET SDK | 8.0+ | `dotnet --version` |
| Git | Any recent | `git --version` |

### Installing .NET SDK

**macOS (Homebrew):**
```bash
brew install dotnet-sdk
```

**Windows:**
Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)

**Linux (Ubuntu/Debian):**
```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

---

## Clone and Build

```bash
# Clone the repository
git clone https://github.com/juanmicrosoft/opal.git
cd opal

# Build the entire solution
dotnet build

# Verify the build
dotnet run --project src/Opal.Compiler -- --help
```

Expected output:
```
OPAL Compiler - Optimized Programming for Agent Logic

Usage:
  opalc --input <file.opal> --output <file.cs>

Options:
  --input, -i    Input OPAL source file
  --output, -o   Output C# file
  --help, -h     Show this help message
```

---

## Project Structure

```
opal/
├── src/
│   └── Opal.Compiler/      # The OPAL compiler
├── samples/
│   └── HelloWorld/         # Sample OPAL program
├── tests/
│   ├── E2E/                # End-to-end tests
│   └── Opal.Evaluation/    # Evaluation framework
└── docs/                   # This documentation
```

---

## Compiling OPAL Files

Basic usage:

```bash
dotnet run --project src/Opal.Compiler -- \
  --input path/to/your/program.opal \
  --output path/to/output/program.g.cs
```

The `.g.cs` extension is a convention indicating "generated C#".

---

## Running Generated Code

After compilation, you need a C# project to run the generated code:

### Option 1: Use the HelloWorld Sample

```bash
# Compile your OPAL file
dotnet run --project src/Opal.Compiler -- \
  --input your-program.opal \
  --output samples/HelloWorld/your-program.g.cs

# Run it (requires modifying HelloWorld.csproj or including the file)
dotnet run --project samples/HelloWorld
```

### Option 2: Create a New Project

```bash
# Create a new console project
dotnet new console -o MyOpalProgram
cd MyOpalProgram

# Compile OPAL to the project directory
dotnet run --project ../src/Opal.Compiler -- \
  --input ../my-code.opal \
  --output my-code.g.cs

# Run the program
dotnet run
```

---

## Running Tests

### E2E Tests

```bash
# macOS/Linux
./tests/E2E/run-tests.sh

# Windows
.\tests\E2E\run-tests.ps1
```

### Evaluation Framework

```bash
# Run the evaluation
dotnet run --project tests/Opal.Evaluation -- --output report.json

# Generate markdown report
dotnet run --project tests/Opal.Evaluation -- --output report.md --format markdown
```

---

## Troubleshooting

### Build Errors

**"SDK not found":**
```bash
# Verify .NET is installed
dotnet --info

# If missing, install .NET 8.0 SDK
```

**"Project not found":**
```bash
# Ensure you're in the opal directory
pwd  # Should show .../opal

# List available projects
ls src/
```

### Runtime Errors

**"Main method not found":**
- Ensure your OPAL code has a public `Main` function:
  ```
  §F[f001:Main:pub]
    §O[void]
    // ...
  §/F[f001]
  ```

---

## Next Steps

- [Hello World](/opal/getting-started/hello-world/) - Understand OPAL syntax
- [Syntax Reference](/opal/syntax-reference/) - Complete language reference
