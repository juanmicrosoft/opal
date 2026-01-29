---
layout: default
title: Getting Started
nav_order: 3
has_children: true
permalink: /getting-started/
---

# Getting Started with OPAL

This guide will help you install OPAL, write your first program, and understand how to integrate OPAL with AI coding agents.

---

## Quick Overview

OPAL is a programming language that compiles to C# via source-to-source transformation. The workflow is:

```
your_code.opal → OPAL Compiler → your_code.g.cs → .NET Build → executable
```

---

## What You'll Learn

1. **[Installation](/opal/getting-started/installation/)** - Set up the OPAL compiler
2. **[Hello World](/opal/getting-started/hello-world/)** - Write and run your first OPAL program
3. **[Claude Integration](/opal/getting-started/claude-integration/)** - Use OPAL with AI coding agents

---

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Git (for cloning the repository)
- A terminal or command prompt

---

## Quick Start

```bash
# Clone and build
git clone https://github.com/juanmicrosoft/opal.git
cd opal && dotnet build

# Compile OPAL to C#
dotnet run --project src/Opal.Compiler -- \
  --input samples/HelloWorld/hello.opal \
  --output samples/HelloWorld/hello.g.cs

# Run the generated program
dotnet run --project samples/HelloWorld
```

Output:
```
Hello from OPAL!
```

---

## Sample Files

The repository includes sample programs:

| Sample | Description |
|:-------|:------------|
| `samples/HelloWorld/` | Basic "Hello World" example |
| `tests/E2E/scenarios/` | End-to-end test programs |

---

## Next Steps

- [Installation](/opal/getting-started/installation/) - Detailed setup instructions
- [Hello World](/opal/getting-started/hello-world/) - Understand the hello world program
- [Syntax Reference](/opal/syntax-reference/) - Complete language reference
