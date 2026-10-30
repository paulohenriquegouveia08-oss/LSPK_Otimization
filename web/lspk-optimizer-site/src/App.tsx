import React, { useState } from 'react';
import { 
  Zap, 
  ShieldCheck, 
  Trash2, 
  Rocket, 
  Cpu, 
  HardDrive, 
  CheckCircle2, 
  Download, 
  ChevronDown, 
  ChevronUp, 
  RefreshCw, 
  Lock, 
  Layers, 
  Monitor, 
  AlertTriangle 
} from 'lucide-react';

/**
 * Componente principal da Landing Page do LSPK Optimizer.
 * Desenvolvido em React 19 com TypeScript e Tailwind CSS.
 * Implementa todas as 9 seções obrigatórias especificadas para o MVP:
 * 1. Apresentação (Hero com Mockup)
 * 2. Benefícios (Diferenciais Técnicos)
 * 3. Como Funciona (Pipeline de 1 Clique)
 * 4. Módulo 1 (Limpeza de Arquivos)
 * 5. Módulo 2 (Gerenciamento de Inicialização)
 * 6. Segurança e Blindagem
 * 7. Compatibilidade e Requisitos
 * 8. Perguntas Frequentes (FAQ Interativo)
 * 9. Rodapé Institucional
 */
export default function App() {
  // Estado para controle de qual item do acordeão de FAQ está expandido no momento
  const [activeFaq, setActiveFaq] = useState<number | null>(null);

  /**
   * Alterna a expansão da pergunta do FAQ clicada.
   * Se já estiver aberta, fecha; caso contrário, abre e fecha as demais.
   */
  const toggleFaq = (index: number) => {
    setActiveFaq(activeFaq === index ? null : index);
  };

  // Caminho público do executável do instalador standalone (relativo para compatibilidade com GitHub Pages e domínios personalizados)
  const downloadUrl = "./download/LSPKOptimizerSetup.exe";

  // Base de conhecimento das perguntas frequentes
  const faqs = [
    {
      q: "O LSPK Optimizer apaga meus arquivos pessoais ou documentos?",
      a: "Nunca. O LSPK Optimizer possui uma política rigorosa de segurança em nível de código (Path Whitelist). Pastas como Documentos, Downloads, Área de Trabalho, Imagens e Músicas são estritamente blindadas contra qualquer tipo de varredura ou exclusão."
    },
    {
      q: "Preciso de privilégios de Administrador (UAC) para instalar?",
      a: "Não para a instalação padrão. O instalador personalizado foi projetado para instalar no escopo do usuário (%LocalAppData%\\Programs\\LSPK Optimizer), permitindo uso imediato sem alertas intrusivos de elevação."
    },
    {
      q: "O que acontece ao desativar um programa de inicialização?",
      a: "Diferente de ferramentas genéricas, o LSPK Optimizer nunca deleta chaves permanentemente. Ele realiza backup automático em chaves dedicadas e espelha o status nativo do Windows, permitindo que você reative o programa a qualquer momento com um clique."
    },
    {
      q: "Como funcionam as atualizações automáticas?",
      a: "Ao abrir o aplicativo, uma consulta silenciosa verifica se há nova versão estável disponível. Caso haja, o pacote é baixado em segundo plano e validado com criptografia SHA-256. Você é notificado por um banner discreto para aplicar quando for conveniente."
    },
    {
      q: "Existe alguma telemetria remota ou envio de dados?",
      a: "Zero. O MVP do LSPK Optimizer opera 100% offline no seu computador. Não há criação de contas, logins, rastreadores ou bancos de dados remotos coletando informações dos seus hábitos ou hardware."
    }
  ];

  return (
    <div className="min-h-screen bg-[#0B0F17] text-[#F8FAFC]">
      {/* =========================================================================
          CABEÇALHO / NAVBAR FIXA COM LOGO E BOTÃO DE DOWNLOAD DIRETO
          ========================================================================= */}
      <header className="sticky top-0 z-50 backdrop-blur-md bg-[#0B0F17]/85 border-b border-[#1E293B]">
        <div className="max-w-7xl mx-auto px-6 h-20 flex items-center justify-between">
          {/* Logotipo da Marca LSPK Technology */}
          <div className="flex items-center space-x-3">
            <div className="w-10 h-10 rounded-xl glow-gradient flex items-center justify-center text-black shadow-lg shadow-cyan-500/20">
              <Zap className="w-6 h-6 fill-black text-black" />
            </div>
            <div>
              <span className="text-xl font-bold tracking-tight text-white">LSPK</span>{" "}
              <span className="text-xl font-extrabold text-transparent bg-clip-text glow-gradient">OPTIMIZER</span>
              <span className="ml-2 text-xs font-semibold px-2 py-0.5 rounded-full bg-[#162238] text-cyan-400 border border-cyan-500/30">MVP 1.0</span>
            </div>
          </div>

          {/* Links de Navegação Suave (Desktop) */}
          <nav className="hidden md:flex items-center space-x-8 text-sm font-medium text-slate-300">
            <a href="#beneficios" className="hover:text-cyan-400 transition">Benefícios</a>
            <a href="#funcionamento" className="hover:text-cyan-400 transition">Como Funciona</a>
            <a href="#modulos" className="hover:text-cyan-400 transition">Módulos</a>
            <a href="#seguranca" className="hover:text-cyan-400 transition">Segurança</a>
            <a href="#compatibilidade" className="hover:text-cyan-400 transition">Compatibilidade</a>
            <a href="#faq" className="hover:text-cyan-400 transition">FAQ</a>
          </nav>

          {/* Botão de Download na Navbar */}
          <div>
            <a 
              href={downloadUrl}
              className="px-5 py-2.5 rounded-xl glow-gradient font-bold text-black text-sm hover:opacity-90 transition flex items-center space-x-2 shadow-lg shadow-cyan-500/25"
            >
              <Download className="w-4 h-4" />
              <span>Baixar Grátis</span>
            </a>
          </div>
        </div>
      </header>

      {/* =========================================================================
          1. SEÇÃO HERO: APRESENTAÇÃO DO PRODUTO E MOCKUP DA APLICAÇÃO
          ========================================================================= */}
      <section className="relative pt-20 pb-28 overflow-hidden">
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-cyan-900/20 via-transparent to-transparent pointer-events-none"></div>

        <div className="max-w-7xl mx-auto px-6 text-center relative z-10">
          {/* Badge Superior Institucional */}
          <div className="inline-flex items-center space-x-2 px-4 py-1.5 rounded-full bg-[#131B2E] border border-cyan-500/30 text-xs font-semibold text-cyan-400 mb-8">
            <ShieldCheck className="w-4 h-4 text-cyan-400" />
            <span>Engenharia Profissional pela LSPK Technology</span>
          </div>

          {/* Título Principal de Alto Impacto */}
          <h1 className="text-4xl md:text-6xl lg:text-7xl font-extrabold tracking-tight text-white max-w-4xl mx-auto leading-tight md:leading-none">
            A Otimização Definitiva do Windows em <span className="text-transparent bg-clip-text glow-gradient">1 Único Clique</span>
          </h1>

          <p className="mt-6 text-lg md:text-xl text-slate-300 max-w-2xl mx-auto leading-relaxed">
            Elimine gigabytes de caches residuais, remova gargalos na inicialização e recupere o tempo de resposta do seu sistema sem riscos de corrupção.
          </p>

          {/* Botões Centrais de Chamada para Ação */}
          <div className="mt-10 flex flex-col sm:flex-row items-center justify-center gap-4">
            <a
              href={downloadUrl}
              className="w-full sm:w-auto px-8 py-4 rounded-xl glow-gradient text-black font-extrabold text-base hover:scale-105 transition duration-200 flex items-center justify-center space-x-3 shadow-xl shadow-cyan-500/30"
            >
              <Download className="w-5 h-5" />
              <span>Baixar LSPKOptimizerSetup.exe</span>
            </a>

            <a
              href="#modulos"
              className="w-full sm:w-auto px-8 py-4 rounded-xl bg-[#131B2E] hover:bg-[#1B253D] border border-slate-700 text-white font-semibold text-base transition flex items-center justify-center space-x-2"
            >
              <Layers className="w-5 h-5 text-slate-400" />
              <span>Explorar Funcionalidades</span>
            </a>
          </div>

          {/* Selos de Confiança Rápidos */}
          <div className="mt-8 flex flex-wrap items-center justify-center gap-6 text-xs text-slate-400 font-medium">
            <span className="flex items-center gap-1.5"><CheckCircle2 className="w-4 h-4 text-emerald-400" /> Windows 10 & 11 (x64)</span>
            <span className="flex items-center gap-1.5"><CheckCircle2 className="w-4 h-4 text-emerald-400" /> Instalação Segura sem UAC</span>
            <span className="flex items-center gap-1.5"><CheckCircle2 className="w-4 h-4 text-emerald-400" /> Zero Métricas Simuladas</span>
            <span className="flex items-center gap-1.5"><CheckCircle2 className="w-4 h-4 text-emerald-400" /> Atualizações Automáticas</span>
          </div>

          {/* MOCKUP VISUAL INTERATIVO DO DASHBOARD DO APLICATIVO */}
          <div className="mt-16 relative max-w-5xl mx-auto">
            <div className="rounded-2xl border border-cyan-500/30 bg-[#131B2E] shadow-2xl shadow-cyan-950/50 p-4 md:p-6 text-left glow-cyan">
              {/* Barra de Título Simulada da Janela */}
              <div className="flex items-center justify-between pb-4 border-b border-[#1E293B] mb-6">
                <div className="flex items-center space-x-3">
                  <div className="w-6 h-6 rounded-md glow-gradient flex items-center justify-center text-black">
                    <Zap className="w-3.5 h-3.5 fill-black text-black" />
                  </div>
                  <span className="text-sm font-bold text-slate-200">LSPK OPTIMIZER</span>
                  <span className="text-xs px-2 py-0.5 rounded bg-cyan-950/60 text-cyan-400 border border-cyan-800">v1.0.0</span>
                </div>
                <div className="flex items-center space-x-2">
                  <div className="w-3 h-3 rounded-full bg-slate-700"></div>
                  <div className="w-3 h-3 rounded-full bg-slate-700"></div>
                  <div className="w-3 h-3 rounded-full bg-red-500/80"></div>
                </div>
              </div>

              {/* Grid Simulado de Telemetria de Hardware */}
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
                <div className="p-4 rounded-xl bg-[#0B0F17] border border-[#1E293B]">
                  <div className="text-[10px] font-bold text-cyan-400">SISTEMA</div>
                  <div className="text-sm font-bold text-white mt-1">Windows 11 Pro</div>
                  <div className="text-xs text-slate-400">Build 22631 (x64)</div>
                </div>
                <div className="p-4 rounded-xl bg-[#0B0F17] border border-[#1E293B]">
                  <div className="text-[10px] font-bold text-cyan-400">PROCESSADOR</div>
                  <div className="text-sm font-bold text-white mt-1">Intel Core / Ryzen</div>
                  <div className="text-xs text-slate-400">16 Núcleos Lógicos</div>
                </div>
                <div className="p-4 rounded-xl bg-[#0B0F17] border border-[#1E293B]">
                  <div className="text-[10px] font-bold text-cyan-400">MEMÓRIA RAM</div>
                  <div className="text-sm font-bold text-white mt-1">7.8 / 16.0 GB</div>
                  <div className="w-full bg-slate-800 h-1.5 rounded-full mt-2">
                    <div className="bg-cyan-400 h-1.5 rounded-full w-[49%]"></div>
                  </div>
                </div>
                <div className="p-4 rounded-xl bg-[#0B0F17] border border-[#1E293B]">
                  <div className="text-[10px] font-bold text-cyan-400">DISCO (C:)</div>
                  <div className="text-sm font-bold text-emerald-400 mt-1">142 GB livres</div>
                  <div className="w-full bg-slate-800 h-1.5 rounded-full mt-2">
                    <div className="bg-emerald-500 h-1.5 rounded-full w-[65%]"></div>
                  </div>
                </div>
              </div>

              {/* Banner Central com o Medidor de Saúde e Botão Principal */}
              <div className="p-6 rounded-xl bg-[#0F1626] border border-cyan-500/40 flex flex-col md:flex-row items-center justify-between gap-6">
                <div className="flex items-center space-x-6">
                  <div className="w-20 h-20 rounded-full border-4 border-cyan-400 flex flex-col items-center justify-center bg-[#0B0F17]">
                    <span className="text-2xl font-black text-cyan-400">92</span>
                    <span className="text-[9px] uppercase font-bold text-slate-400">Score</span>
                  </div>
                  <div>
                    <h4 className="text-lg font-bold text-white">Status do Sistema: Excelente</h4>
                    <p className="text-xs text-slate-400 mt-1">Diagnóstico pronto: 3.4 GB de caches e 2 órfãos detectados.</p>
                  </div>
                </div>
                <button className="px-6 py-3 rounded-xl glow-gradient text-black font-extrabold text-sm flex items-center space-x-2 shadow-lg shadow-cyan-500/20">
                  <Zap className="w-4 h-4 fill-black" />
                  <span>OTIMIZAR AGORA</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          2. SEÇÃO DE BENEFÍCIOS E DIFERENCIAIS
          ========================================================================= */}
      <section id="beneficios" className="py-24 border-t border-[#1E293B] bg-[#0E1424]">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-2xl mx-auto mb-16">
            <h2 className="text-xs font-bold text-cyan-400 tracking-wider uppercase">Diferenciais Técnicos</h2>
            <p className="text-3xl font-extrabold text-white mt-2">Por que o LSPK Optimizer é diferente?</p>
            <p className="text-slate-400 text-sm mt-3">Construído do zero para profissionais e usuários exigentes que recusam programas de manutenção suspeitos e cheios de propagandas.</p>
          </div>

          <div className="grid md:grid-cols-3 gap-8">
            <div className="p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B] hover:border-cyan-500/50 transition">
              <div className="w-12 h-12 rounded-xl bg-cyan-950 flex items-center justify-center text-cyan-400 mb-6">
                <ShieldCheck className="w-6 h-6" />
              </div>
              <h3 className="text-xl font-bold text-white mb-2">Zero Métricas Falsas</h3>
              <p className="text-slate-400 text-sm leading-relaxed">
                Nenhuma barra de progresso simulada por temporizador ou espaço fictício. Cada byte calculado e liberado é aferido diretamente com as APIs do sistema de arquivos.
              </p>
            </div>

            <div className="p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B] hover:border-cyan-500/50 transition">
              <div className="w-12 h-12 rounded-xl bg-emerald-950 flex items-center justify-center text-emerald-400 mb-6">
                <RefreshCw className="w-6 h-6" />
              </div>
              <h3 className="text-xl font-bold text-white mb-2">Reversibilidade Garantida</h3>
              <p className="text-slate-400 text-sm leading-relaxed">
                Todas as alterações de inicialização criam backups espelhados automáticos. Nada é deletado de forma destrutiva sem que você possa restaurar com 1 toque.
              </p>
            </div>

            <div className="p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B] hover:border-cyan-500/50 transition">
              <div className="w-12 h-12 rounded-xl bg-blue-950 flex items-center justify-center text-blue-400 mb-6">
                <Lock className="w-6 h-6" />
              </div>
              <h3 className="text-xl font-bold text-white mb-2">Segurança em Nível de Kernel</h3>
              <p className="text-slate-400 text-sm leading-relaxed">
                Lista de proteção inviolável para serviços do Windows Defender, drivers de áudio/GPU e processos críticos do sistema. O software impede desativações perigosas.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          3. SEÇÃO DE FUNCIONAMENTO (PIPELINE DE 1 CLIQUE)
          ========================================================================= */}
      <section id="funcionamento" className="py-24 border-t border-[#1E293B] bg-[#0B0F17]">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-2xl mx-auto mb-16">
            <h2 className="text-xs font-bold text-cyan-400 tracking-wider uppercase">Pipeline Inteligente</h2>
            <p className="text-3xl font-extrabold text-white mt-2">Como Funciona o "Otimizar Agora"</p>
            <p className="text-slate-400 text-sm mt-3">Um fluxo sequencial orquestrado em 9 etapas à prova de falhas.</p>
          </div>

          <div className="grid md:grid-cols-3 gap-8 relative">
            <div className="p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B]">
              <div className="text-4xl font-black text-cyan-400 mb-4">01</div>
              <h3 className="text-lg font-bold text-white mb-2">Diagnóstico Profundo</h3>
              <p className="text-slate-400 text-sm leading-relaxed">
                O aplicativo audita o hardware, varre caches do usuário e do Windows, identifica dumps de erro e lê todas as chaves de inicialização (HKCU, HKLM e Startup folders).
              </p>
            </div>

            <div className="p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B]">
              <div className="text-4xl font-black text-cyan-400 mb-4">02</div>
              <h3 className="text-lg font-bold text-white mb-2">Limpeza com Whitelist</h3>
              <p className="text-slate-400 text-sm leading-relaxed">
                Cada caminho de arquivo passa por validação estrita. Arquivos travados por processos em execução são ignorados pacificamente sem lançar erros críticos.
              </p>
            </div>

            <div className="p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B]">
              <div className="text-4xl font-black text-cyan-400 mb-4">03</div>
              <h3 className="text-lg font-bold text-white mb-2">Calibração e Relatório</h3>
              <p className="text-slate-400 text-sm leading-relaxed">
                Resíduos órfãos de inicialização são desativados de modo reversível, os resultados são validados e um relatório com o ganho exato em megabytes é exibido.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          4. SEÇÃO DO MÓDULO 1: LIMPEZA DE ARQUIVOS
          ========================================================================= */}
      <section id="modulos" className="py-24 border-t border-[#1E293B] bg-[#0E1424]">
        <div className="max-w-7xl mx-auto px-6">
          <div className="grid md:grid-cols-2 gap-12 items-center">
            <div>
              <div className="inline-flex items-center space-x-2 px-3 py-1 rounded-full bg-cyan-950/60 border border-cyan-500/30 text-xs font-semibold text-cyan-400 mb-4">
                <Trash2 className="w-3.5 h-3.5" />
                <span>Módulo 1</span>
              </div>
              <h2 className="text-3xl md:text-4xl font-extrabold text-white tracking-tight">
                Limpeza Seletiva e Profunda de Arquivos Residuais
              </h2>
              <p className="mt-4 text-slate-300 text-sm md:text-base leading-relaxed">
                Com o passar dos meses, o Windows acumula gigabytes de arquivos temporários, caches compilados de shaders e logs antigos de falhas que degradam a taxa de leitura e escrita do seu SSD.
              </p>

              <div className="mt-6 space-y-3">
                {[
                  "Temporários do Usuário (%TEMP%) e do Windows (%WINDIR%\\Temp)",
                  "Cache de Miniaturas (thumbcache_*.db)",
                  "Cache de Shaders GPU (DirectX, NVIDIA e AMD)",
                  "Relatórios Antigos de Erros e Crash Dumps (WER)",
                  "Cache Residual do Windows Update (SoftwareDistribution\\Download)",
                  "Caches de Navegadores Homologados (Chrome, Edge, Brave)",
                  "Lixeira do Windows com Confirmação Prévia"
                ].map((item, idx) => (
                  <div key={idx} className="flex items-start space-x-3 text-sm text-slate-300">
                    <CheckCircle2 className="w-4 h-4 text-cyan-400 shrink-0 mt-0.5" />
                    <span>{item}</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B]">
              <h4 className="text-base font-bold text-white mb-4">Regras de Blindagem do Limpador:</h4>
              <ul className="space-y-4 text-sm text-slate-400">
                <li className="flex items-start space-x-3">
                  <CheckCircle2 className="w-5 h-5 text-emerald-400 shrink-0 mt-0.5" />
                  <span><strong className="text-slate-200">Zero Exclusão Pessoal:</strong> Jamais toca em Documentos, Downloads, Fotos ou Músicas.</span>
                </li>
                <li className="flex items-start space-x-3">
                  <CheckCircle2 className="w-5 h-5 text-emerald-400 shrink-0 mt-0.5" />
                  <span><strong className="text-slate-200">Proteção de Arquivos Recentes:</strong> Arquivos com menos de 24 horas são preservados para não quebrar sessões ativas.</span>
                </li>
                <li className="flex items-start space-x-3">
                  <CheckCircle2 className="w-5 h-5 text-emerald-400 shrink-0 mt-0.5" />
                  <span><strong className="text-slate-200">Tolerância a Bloqueios:</strong> Se um arquivo estiver em uso, o sistema ignora pacificamente sem interrupções.</span>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          5. SEÇÃO DO MÓDULO 2: GERENCIAMENTO DE INICIALIZAÇÃO
          ========================================================================= */}
      <section className="py-24 border-t border-[#1E293B] bg-[#0B0F17]">
        <div className="max-w-7xl mx-auto px-6">
          <div className="grid md:grid-cols-2 gap-12 items-center">
            <div className="order-2 md:order-1 p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B]">
              <div className="flex items-center justify-between pb-4 border-b border-[#1E293B] mb-4">
                <span className="text-sm font-bold text-white">Fontes de Inicialização Auditadas</span>
                <span className="text-xs text-cyan-400 font-semibold">100% Reversível</span>
              </div>
              <div className="space-y-3 text-xs text-slate-300">
                <div className="p-3 rounded-lg bg-[#0B0F17] flex justify-between items-center">
                  <span>HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run</span>
                  <span className="text-cyan-400 font-mono">HKCU</span>
                </div>
                <div className="p-3 rounded-lg bg-[#0B0F17] flex justify-between items-center">
                  <span>HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Run</span>
                  <span className="text-cyan-400 font-mono">HKLM 32/64</span>
                </div>
                <div className="p-3 rounded-lg bg-[#0B0F17] flex justify-between items-center">
                  <span>%AppData%\\Microsoft\\Windows\\Start Menu\\Programs\\Startup</span>
                  <span className="text-cyan-400 font-mono">User Startup</span>
                </div>
                <div className="p-3 rounded-lg bg-[#0B0F17] flex justify-between items-center">
                  <span>%ProgramData%\\Microsoft\\Windows\\Start Menu\\Programs\\Startup</span>
                  <span className="text-cyan-400 font-mono">Common Startup</span>
                </div>
              </div>
            </div>

            <div className="order-1 md:order-2">
              <div className="inline-flex items-center space-x-2 px-3 py-1 rounded-full bg-cyan-950/60 border border-cyan-500/30 text-xs font-semibold text-cyan-400 mb-4">
                <Rocket className="w-3.5 h-3.5" />
                <span>Módulo 2</span>
              </div>
              <h2 className="text-3xl md:text-4xl font-extrabold text-white tracking-tight">
                Gerenciamento Inteligente de Inicialização
              </h2>
              <p className="mt-4 text-slate-300 text-sm md:text-base leading-relaxed">
                Descubra quais programas estão atrasando a inicialização do seu computador. O LSPK Optimizer identifica executáveis órfãos deixados por programas já desinstalados e permite desativá-los com segurança.
              </p>

              <div className="mt-6 space-y-3">
                <div className="flex items-center space-x-3 text-sm text-slate-300">
                  <CheckCircle2 className="w-4 h-4 text-cyan-400" />
                  <span>Extração real de fabricante e metadados de assinatura.</span>
                </div>
                <div className="flex items-center space-x-3 text-sm text-slate-300">
                  <CheckCircle2 className="w-4 h-4 text-cyan-400" />
                  <span>Destaque visual para entradas órfãs (arquivos que não existem mais).</span>
                </div>
                <div className="flex items-center space-x-3 text-sm text-slate-300">
                  <CheckCircle2 className="w-4 h-4 text-cyan-400" />
                  <span>Backup automático das entradas de registro na chave da LSPK.</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          6. SEÇÃO DE SEGURANÇA E FILOSOFIA DE PROTEÇÃO
          ========================================================================= */}
      <section id="seguranca" className="py-24 border-t border-[#1E293B] bg-[#0E1424]">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-2xl mx-auto mb-16">
            <h2 className="text-xs font-bold text-cyan-400 tracking-wider uppercase">Engenharia Responsável</h2>
            <p className="text-3xl font-extrabold text-white mt-2">Segurança em Primeiro Lugar</p>
            <p className="text-slate-400 text-sm mt-3">Nosso compromisso inviolável com a estabilidade do seu sistema operacional.</p>
          </div>

          <div className="grid md:grid-cols-3 gap-8">
            <div className="p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B]">
              <div className="w-12 h-12 rounded-xl bg-emerald-950 flex items-center justify-center text-emerald-400 mb-4">
                <ShieldCheck className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white mb-2">Proteção a Pastas Pessoais</h3>
              <p className="text-slate-400 text-sm leading-relaxed">
                O módulo PathValidator possui bloqueio a nível de código-fonte impedindo acesso a Documentos, Downloads e Desktop.
              </p>
            </div>

            <div className="p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B]">
              <div className="w-12 h-12 rounded-xl bg-cyan-950 flex items-center justify-center text-cyan-400 mb-4">
                <Lock className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white mb-2">Blindagem de Antivírus</h3>
              <p className="text-slate-400 text-sm leading-relaxed">
                Windows Defender, Kaspersky, Bitdefender e outros softwares de segurança são blindados contra qualquer desativação.
              </p>
            </div>

            <div className="p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B]">
              <div className="w-12 h-12 rounded-xl bg-blue-950 flex items-center justify-center text-blue-400 mb-4">
                <Layers className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white mb-2">Backups Dedicados</h3>
              <p className="text-slate-400 text-sm leading-relaxed">
                Entradas desativadas são armazenadas na chave do registro da LSPK, garantindo restauração instantânea.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          7. SEÇÃO DE COMPATIBILIDADE E REQUISITOS
          ========================================================================= */}
      <section id="compatibilidade" className="py-24 border-t border-[#1E293B] bg-[#0B0F17]">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-2xl mx-auto mb-16">
            <h2 className="text-xs font-bold text-cyan-400 tracking-wider uppercase">Requisitos</h2>
            <p className="text-3xl font-extrabold text-white mt-2">Compatibilidade Homologada</p>
            <p className="text-slate-400 text-sm mt-3">Projetado e testado exclusivamente para o ecossistema moderno do Windows.</p>
          </div>

          <div className="grid md:grid-cols-2 gap-8 max-w-4xl mx-auto">
            <div className="p-8 rounded-2xl bg-[#131B2E] border border-cyan-500/40">
              <div className="flex items-center space-x-4 mb-6">
                <Monitor className="w-8 h-8 text-cyan-400" />
                <div>
                  <h3 className="text-xl font-bold text-white">Sistemas Operacionais Suportados</h3>
                  <span className="text-xs text-slate-400">Totalmente compatível</span>
                </div>
              </div>
              <ul className="space-y-3 text-sm text-slate-300">
                <li className="flex items-center space-x-2">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400" />
                  <span>Windows 11 (todas as edições x64)</span>
                </li>
                <li className="flex items-center space-x-2">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400" />
                  <span>Windows 10 versão 22H2 ou superior (x64)</span>
                </li>
                <li className="flex items-center space-x-2">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400" />
                  <span>Arquitetura x64 nativa</span>
                </li>
              </ul>
            </div>

            <div className="p-8 rounded-2xl bg-[#131B2E] border border-[#1E293B]">
              <div className="flex items-center space-x-4 mb-6">
                <Cpu className="w-8 h-8 text-slate-400" />
                <div>
                  <h3 className="text-xl font-bold text-white">Requisitos de Hardware</h3>
                  <span className="text-xs text-slate-400">Leve e eficiente</span>
                </div>
              </div>
              <ul className="space-y-3 text-sm text-slate-300">
                <li className="flex items-center space-x-2">
                  <CheckCircle2 className="w-4 h-4 text-cyan-400" />
                  <span>Processador 1.0 GHz ou superior</span>
                </li>
                <li className="flex items-center space-x-2">
                  <CheckCircle2 className="w-4 h-4 text-cyan-400" />
                  <span>100 MB de memória RAM livre</span>
                </li>
                <li className="flex items-center space-x-2">
                  <CheckCircle2 className="w-4 h-4 text-cyan-400" />
                  <span>50 MB de espaço em disco para instalação</span>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          8. SEÇÃO DE PERGUNTAS FREQUENTES (FAQ)
          ========================================================================= */}
      <section id="faq" className="py-24 border-t border-[#1E293B] bg-[#0E1424]">
        <div className="max-w-4xl mx-auto px-6">
          <div className="text-center mb-16">
            <h2 className="text-xs font-bold text-cyan-400 tracking-wider uppercase">Dúvidas Frequentes</h2>
            <p className="text-3xl font-extrabold text-white mt-2">Perguntas e Respostas</p>
          </div>

          <div className="space-y-4">
            {faqs.map((faq, index) => (
              <div 
                key={index}
                className="rounded-xl bg-[#131B2E] border border-[#1E293B] overflow-hidden transition"
              >
                <button
                  onClick={() => toggleFaq(index)}
                  className="w-full p-6 text-left flex items-center justify-between font-semibold text-white text-base hover:text-cyan-400 transition"
                >
                  <span>{faq.q}</span>
                  {activeFaq === index ? <ChevronUp className="w-5 h-5 text-cyan-400" /> : <ChevronDown className="w-5 h-5 text-slate-400" />}
                </button>
                {activeFaq === index && (
                  <div className="px-6 pb-6 text-sm text-slate-300 border-t border-[#1E293B] pt-4 leading-relaxed">
                    {faq.a}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* =========================================================================
          CHAMADA PARA AÇÃO FINAL ANTES DO RODAPÉ
          ========================================================================= */}
      <section className="py-20 border-t border-[#1E293B] bg-gradient-to-b from-[#0E1424] to-[#0B0F17]">
        <div className="max-w-4xl mx-auto px-6 text-center">
          <h2 className="text-3xl md:text-4xl font-extrabold text-white">
            Pronto para recuperar a velocidade do seu Windows?
          </h2>
          <p className="mt-4 text-slate-400 text-base max-w-xl mx-auto">
            Baixe o instalador oficial agora mesmo. Instalação rápida, limpa e sem complicação.
          </p>
          <div className="mt-8 flex justify-center">
            <a
              href={downloadUrl}
              className="px-10 py-4 rounded-xl glow-gradient text-black font-extrabold text-base hover:scale-105 transition flex items-center space-x-3 shadow-xl shadow-cyan-500/30"
            >
              <Download className="w-5 h-5" />
              <span>Baixar LSPKOptimizerSetup.exe (MVP)</span>
            </a>
          </div>
        </div>
      </section>

      {/* =========================================================================
          9. RODAPÉ INSTITUCIONAL COM COPYRIGHT E TERMOS
          ========================================================================= */}
      <footer className="py-12 border-t border-[#1E293B] bg-[#070A10] text-slate-400 text-xs">
        <div className="max-w-7xl mx-auto px-6 flex flex-col md:flex-row items-center justify-between gap-6">
          <div className="flex items-center space-x-3">
            <div className="w-7 h-7 rounded-lg glow-gradient flex items-center justify-center text-black">
              <Zap className="w-4 h-4 fill-black text-black" />
            </div>
            <div>
              <span className="font-bold text-slate-200">LSPK OPTIMIZER</span> — Um produto da <strong className="text-white">LSPK Technology</strong>
            </div>
          </div>

          <div className="flex items-center space-x-6">
            <span>Privacidade Local Garantida</span>
            <span>•</span>
            <span>Sem Banco Remoto</span>
            <span>•</span>
            <span>SHA-256 Verificado</span>
          </div>

          <div className="text-slate-500">
            © {new Date().getFullYear()} LSPK Technology. Todos os direitos reservados.
          </div>
        </div>
      </footer>
    </div>
  );
}
