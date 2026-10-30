# Diretório de Distribuição do LSPK Optimizer

Este diretório contém os binários oficiais prontos para deploy estático via CDN (Vercel, Netlify, Cloudflare Pages ou AWS S3).

- `LSPKOptimizerSetup.exe`: Instalador standalone oficial (gerado a partir de `src/LSPK.Optimizer.Installer`).
- `/updates/latest.json`: Manifesto consumido automaticamente pelo aplicativo para detecção de novas versões.
- `/updates/releases/`: Pacotes ZIP versionados para atualização incremental.
