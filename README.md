# LSPK OPTIMIZER — DOCUMENTAÇÃO OFICIAL DO PROJETO

> **Desenvolvido por:** LSPK Technology  
> **Versão:** 1.0.0 (MVP)  
> **Alvo:** Windows 10 (22H2+) / Windows 11 (x64)  
> **Stack Principal:** C# / .NET 10 / WPF / XAML / React 19 + TypeScript (Website)

---

## 1. Visão Geral da Solução

O **LSPK Optimizer** é uma suíte de software profissional para otimização do Windows com foco em:
- **Operação em 1 Clique:** O botão central executa diagnóstico, limpeza de arquivos desnecessários e calibração de inicialização sequencialmente.
- **Segurança Absoluta (Path Whitelist):** Bloqueio inegociável contra exclusão de pastas pessoais (Documentos, Downloads, Área de Trabalho, Fotos).
- **Reversibilidade de Inicialização:** Entradas nunca são apagadas permanentemente; backups automáticos no registro e sistema de arquivos permitem desfazer qualquer alteração.
- **Zero Métricas Falsas:** Aferição física real de bytes deletados e espaço em disco via APIs do sistema operacional.
- **Zero Telemetria:** Operação 100% offline no dispositivo do usuário, sem logins nem banco de dados remoto no MVP.

---

## 2. Estrutura do Repositório

```
lspk-optimizer/
├── src/
│   ├── LSPK.Optimizer.Core/        # Biblioteca central: regras de negócio, diagnósticos, cleaners e segurança
│   ├── LSPK.Optimizer.App/         # Aplicativo Desktop WPF principal (Dark Mode, Dashboard, Health Score)
│   ├── LSPK.Optimizer.Installer/   # Instalador standalone de clique único (LSPKOptimizerSetup.exe)
│   ├── LSPK.Optimizer.Updater/     # Utilitário desacoplado de atualização atômica e rollback
│   └── LSPK.Optimizer.Tests/       # Suíte de 21 testes unitários (PathValidator, Whitelist, SemVer, Scoring)
├── web/
│   └── lspk-optimizer-site/        # Landing Page comercial oficial (React + TypeScript + Tailwind CSS)
├── server/
│   └── optimizer/                  # Estrutura estática de deploy e manifesto remoto de updates (latest.json)
└── LSPK.Optimizer.sln              # Arquivo de solução mestre do .NET
```

---

## 3. Instruções de Compilação e Publicação

### 3.1. Requisitos para Compilação no Windows
- .NET 10 SDK (ou .NET 9 SDK com suporte a Windows Desktop)
- Visual Studio 2022 / 2026 (ou `dotnet build` via CLI no Windows com workload `wcf` / `desktop`)
- Node.js 18+ (para o site comercial)

### 3.2. Compilação da Solução .NET
```powershell
# Restaurar dependências
dotnet restore LSPK.Optimizer.sln

# Executar testes unitários do Core
dotnet test src/LSPK.Optimizer.Tests/LSPK.Optimizer.Tests.csproj

# Publicar o Aplicativo Principal (Self-Contained x64)
dotnet publish src/LSPK.Optimizer.App/LSPK.Optimizer.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist/app

# Publicar o Instalador Standalone (LSPKOptimizerSetup.exe)
dotnet publish src/LSPK.Optimizer.Installer/LSPK.Optimizer.Installer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist/installer

# Publicar o Atualizador Desacoplado
dotnet publish src/LSPK.Optimizer.Updater/LSPK.Optimizer.Updater.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist/updater
```

### 3.3. Compilação da Landing Page (Site)
```bash
cd web/lspk-optimizer-site
npm install
npm run build
# Os arquivos estáticos otimizados serão gerados na pasta dist/
```

---

## 4. Arquitetura de Módulos

### 4.1. Módulo 1 — Limpeza Inteligente
- **Categorias:** Temporários do usuário (`%TEMP%`), Temporários do Windows (`%WINDIR%\Temp`), Cache de miniaturas (`thumbcache_*.db`), Cache de shaders de GPU (DirectX, NVIDIA, AMD), Relatórios de erro e crash dumps antigos (WER), Cache seguro do Windows Update (`SoftwareDistribution\Download`), Caches de navegadores (Chrome, Edge, Brave) e Lixeira com confirmação.
- **Tratamento de Bloqueios:** Arquivos travados por processos ativos ou com restrições de permissão são ignorados pacificamente sem gerar erros para o usuário.

### 4.2. Módulo 2 — Gerenciamento de Inicialização
- **Fontes Auditadas:** `HKCU Run`, `HKLM Run` (32 e 64-bit), pastas `Startup` do usuário e comum.
- **Backup e Restauração:** Entradas desativadas são mantidas com segurança em `HKCU\Software\LSPK\Optimizer\StartupBackup` e pasta local de backup de atalhos.
- **Blindagem do Sistema:** Processos críticos (`explorer.exe`, `msmpeng.exe`, drivers NVIDIA/AMD/Realtek) são protegidos e não podem ser desativados.

### 4.3. Sistema de Atualização Automática
- Consulta estática ao manifesto `latest.json`.
- Download em segundo plano sem congelar a UI nem interromper otimizações em andamento.
- Validação criptográfica rigorosa via **SHA-256**.
- Executável desacoplado `LSPK.Optimizer.Updater.exe` garante substituição segura com rollback automático em caso de falha.
