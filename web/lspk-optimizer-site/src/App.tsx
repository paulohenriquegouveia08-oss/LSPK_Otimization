import React, { useState, useEffect } from "react";
import {
  Zap,
  ShieldCheck,
  Trash2,
  Cpu,
  HardDrive,
  CheckCircle2,
  XCircle,
  Lock,
  RefreshCw,
  Play,
  ArrowRight,
  ChevronDown,
  Download,
  Layers,
  Activity,
  Sparkles,
  Clock,
  Terminal,
  ExternalLink,
  Sliders,
  Check,
  AlertCircle,
  X
} from "lucide-react";

/**
 * Interface representando uma categoria de limpeza para o simulador interativo.
 */
interface CleanCategory {
  id: string;
  name: string;
  path: string;
  sizeMb: number;
  selected: boolean;
  safe: boolean;
}

/**
 * Interface representando um programa de inicialização para o simulador interativo.
 */
interface StartupItem {
  id: string;
  name: string;
  publisher: string;
  impact: "Alto" | "Médio" | "Baixo";
  enabled: boolean;
  isSystemProtected: boolean;
  isOrphan: boolean;
}

/**
 * Componente principal da Landing Page do LSPK Optimizer.
 * Desenvolvido em React 19 com TypeScript, Tailwind CSS e micro-interações nativas.
 * Inclui simulador interativo do aplicativo no Hero, alternador de módulos em tempo real,
 * matriz de segurança e animações fluidas.
 */
export default function App() {
  // Link oficial de download direto do executável no GitHub Releases
  const downloadUrl = "https://github.com/paulohenriquegouveia08-oss/LSPK_Otimization/releases/download/v1.0.0/LSPKOptimizerSetup.exe";

  // Controle de FAQ expandido
  const [activeFaq, setActiveFaq] = useState<number | null>(null);

  // Controle de modal de download e instruções para o usuário
  const [showDownloadModal, setShowDownloadModal] = useState(false);

  /**
   * Dispara o download de forma confiável e abre o modal com links e orientações.
   */
  const handleDownload = (e?: React.MouseEvent) => {
    if (e) e.preventDefault();
    setShowDownloadModal(true);

    try {
      const link = document.createElement("a");
      link.href = downloadUrl;
      link.download = "LSPKOptimizerSetup.exe";
      link.target = "_blank";
      link.rel = "noopener noreferrer";
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    } catch {
      window.open(downloadUrl, "_blank");
    }
  };

  // Aba ativa na seção de módulos interativos (limpeza ou inicialização)
  const [activeModuleTab, setActiveModuleTab] = useState<"cleaner" | "startup">("cleaner");

  // Estado do Simulador do Hero
  const [simOptimizing, setSimOptimizing] = useState(false);
  const [simStep, setSimStep] = useState(0);
  const [simProgress, setSimProgress] = useState(0);
  const [simScore, setSimScore] = useState(64);
  const [simCompleted, setSimCompleted] = useState(false);
  const [simFreedMb, setSimFreedMb] = useState(0);

  // Lista de categorias de limpeza no módulo interativo
  const [cleanCategories, setCleanCategories] = useState<CleanCategory[]>([
    { id: "temp", name: "Arquivos Temporários do Usuário", path: "%TEMP%", sizeMb: 1840, selected: true, safe: true },
    { id: "wintemp", name: "Arquivos Temporários do Windows", path: "C:\\Windows\\Temp", sizeMb: 920, selected: true, safe: true },
    { id: "thumbs", name: "Cache de Miniaturas (Explorer)", path: "%LocalAppData%\\Microsoft\\Windows\\Explorer", sizeMb: 480, selected: true, safe: true },
    { id: "shaders", name: "Cache de Shaders DirectX / GPU", path: "%LocalAppData%\\D3DSCache", sizeMb: 610, selected: true, safe: true },
    { id: "wer", name: "Relatórios de Erros e Crash Dumps", path: "%LocalAppData%\\CrashDumps", sizeMb: 350, selected: true, safe: true },
    { id: "bin", name: "Lixeira do Windows", path: "Recycle.Bin (API SHEmpty)", sizeMb: 1250, selected: true, safe: true }
  ]);

  // Lista de itens de inicialização no módulo interativo
  const [startupItems, setStartupItems] = useState<StartupItem[]>([
    { id: "discord", name: "Discord", publisher: "Discord Inc.", impact: "Alto", enabled: true, isSystemProtected: false, isOrphan: false },
    { id: "spotify", name: "Spotify", publisher: "Spotify AB", impact: "Alto", enabled: true, isSystemProtected: false, isOrphan: false },
    { id: "steam", name: "Steam Client Bootstrapper", publisher: "Valve Corporation", impact: "Médio", enabled: true, isSystemProtected: false, isOrphan: false },
    { id: "orphan1", name: "UpdateHelper_Uninstalled", publisher: "Desconhecido (Órfão)", impact: "Baixo", enabled: true, isSystemProtected: false, isOrphan: true },
    { id: "defender", name: "Windows Security Notification", publisher: "Microsoft Corporation", impact: "Baixo", enabled: true, isSystemProtected: true, isOrphan: false },
    { id: "audio", name: "Realtek HD Audio Universal Service", publisher: "Realtek Semiconductor", impact: "Baixo", enabled: true, isSystemProtected: true, isOrphan: false }
  ]);

  // Passos textuais da simulação no Hero
  const simSteps = [
    "Iniciando diagnóstico seguro do sistema...",
    "Escaneando arquivos temporários e caches bloqueados...",
    "Auditando entradas do registro em HKCU e HKLM...",
    "Identificando programas de inicialização órfãos...",
    "Removendo com segurança arquivos elegíveis...",
    "Aplicando ajustes não destrutivos de inicialização...",
    "Otimização concluída! Desempenho restaurado."
  ];

  /**
   * Executa a simulação interativa do botão "OTIMIZAR AGORA" no Hero Mockup.
   */
  const handleStartSimulation = () => {
    if (simOptimizing) return;
    setSimOptimizing(true);
    setSimCompleted(false);
    setSimStep(0);
    setSimProgress(5);
    setSimFreedMb(0);

    let currentStep = 0;
    const interval = setInterval(() => {
      currentStep++;
      if (currentStep < simSteps.length) {
        setSimStep(currentStep);
        setSimProgress(Math.min(100, Math.round((currentStep / (simSteps.length - 1)) * 100)));
        setSimFreedMb(prev => Math.min(4250, prev + 700));
        setSimScore(prev => Math.min(98, prev + 5));
      } else {
        clearInterval(interval);
        setSimOptimizing(false);
        setSimCompleted(true);
        setSimScore(98);
        setSimFreedMb(4250);
        setSimProgress(100);
      }
    }, 900);
  };

  /**
   * Reinicia o estado do simulador para permitir novo teste pelo visitante.
   */
  const handleResetSimulation = () => {
    setSimOptimizing(false);
    setSimCompleted(false);
    setSimStep(0);
    setSimProgress(0);
    setSimScore(64);
    setSimFreedMb(0);
  };

  /**
   * Alterna seleção de uma categoria de limpeza no módulo interativo.
   */
  const toggleCleanCategory = (id: string) => {
    setCleanCategories(prev =>
      prev.map(cat => (cat.id === id ? { ...cat, selected: !cat.selected } : cat))
    );
  };

  /**
   * Alterna status de ativação de um item de inicialização no módulo interativo.
   */
  const toggleStartupItem = (id: string) => {
    setStartupItems(prev =>
      prev.map(item => {
        if (item.id === id && !item.isSystemProtected) {
          return { ...item, enabled: !item.enabled };
        }
        return item;
      })
    );
  };

  // Cálculo dinâmico do total de MB selecionados para limpeza
  const totalSelectedCleanMb = cleanCategories
    .filter(c => c.selected)
    .reduce((acc, curr) => acc + curr.sizeMb, 0);

  // Cálculo dinâmico do impacto de inicialização
  const activeStartupCount = startupItems.filter(i => i.enabled).length;

  return (
    <div className="min-h-screen bg-[#070B14] text-[#F8FAFC] selection:bg-cyan-500 selection:text-black relative overflow-hidden">
      {/* Malha de fundo cibernética e luzes radiais */}
      <div className="fixed inset-0 cyber-grid pointer-events-none opacity-40 z-0" />
      <div className="fixed top-0 left-1/2 -translate-x-1/2 w-full max-w-7xl h-[600px] radial-glow-top pointer-events-none z-0" />
      <div className="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[800px] radial-glow-center pointer-events-none z-0" />

      {/* =========================================================================
          1. HEADER / BARRA DE NAVEGAÇÃO FLUTUANTE
          ========================================================================= */}
      <header className="sticky top-0 z-50 backdrop-blur-xl bg-[#070B14]/80 border-b border-slate-800/80 transition-all">
        <div className="max-w-7xl mx-auto px-6 h-20 flex items-center justify-between">
          {/* Logo e Identidade LSPK Technology */}
          <div className="flex items-center space-x-3.5">
            <div className="w-10 h-10 rounded-xl glow-gradient flex items-center justify-center text-black shadow-lg shadow-cyan-500/20 ring-1 ring-cyan-400/40">
              <Zap className="w-5 h-5 fill-black text-black stroke-[2.5]" />
            </div>
            <div>
              <div className="flex items-center space-x-2">
                <span className="text-xl font-extrabold tracking-tight text-white">LSPK</span>
                <span className="text-xl font-black text-transparent bg-clip-text glow-gradient">
                  OPTIMIZER
                </span>
                <span className="text-[10px] uppercase font-bold tracking-widest px-2 py-0.5 rounded-md bg-cyan-950/80 text-cyan-300 border border-cyan-500/30">
                  MVP 1.0
                </span>
              </div>
              <p className="text-[10px] text-slate-400 font-medium tracking-wide">
                Engenharia de Performance • LSPK Technology
              </p>
            </div>
          </div>

          {/* Links de Navegação */}
          <nav className="hidden lg:flex items-center space-x-8 text-sm font-medium text-slate-300">
            <a href="#simulador" className="hover:text-cyan-400 transition-colors">
              Simulador
            </a>
            <a href="#beneficios" className="hover:text-cyan-400 transition-colors">
              Diferenciais
            </a>
            <a href="#funcionamento" className="hover:text-cyan-400 transition-colors">
              Como Funciona
            </a>
            <a href="#modulos" className="hover:text-cyan-400 transition-colors">
              Módulos
            </a>
            <a href="#seguranca" className="hover:text-cyan-400 transition-colors">
              Segurança
            </a>
            <a href="#faq" className="hover:text-cyan-400 transition-colors">
              FAQ
            </a>
          </nav>

          {/* Botão de Ação Primária do Header */}
          <div className="flex items-center space-x-4">
            <a
              href="https://github.com/paulohenriquegouveia08-oss/LSPK_Otimization"
              target="_blank"
              rel="noreferrer"
              className="hidden sm:flex items-center space-x-2 text-xs font-semibold px-3 py-2 rounded-lg bg-slate-900 border border-slate-700/80 text-slate-300 hover:text-white hover:border-slate-500 transition"
            >
              <Terminal className="w-3.5 h-3.5 text-cyan-400" />
              <span>Código Aberto</span>
            </a>
            <a
              href={downloadUrl}
              onClick={handleDownload}
              target="_blank"
              rel="noopener noreferrer"
              className="px-5 py-2.5 rounded-xl glow-gradient font-bold text-black text-xs sm:text-sm hover:opacity-95 hover:scale-[1.02] active:scale-[0.98] transition-all flex items-center space-x-2 shadow-lg shadow-cyan-500/25 ring-1 ring-white/30"
            >
              <Download className="w-4 h-4 stroke-[2.5]" />
              <span>Baixar Grátis</span>
            </a>
          </div>
        </div>
      </header>

      {/* =========================================================================
          2. SEÇÃO HERO COM SIMULADOR INTERATIVO AO VIVO DO APLICATIVO
          ========================================================================= */}
      <section className="relative pt-16 pb-24 z-10">
        <div className="max-w-7xl mx-auto px-6 text-center">
          {/* Título Principal de Alto Impacto */}
          <h1 className="text-4xl sm:text-6xl lg:text-7xl font-extrabold tracking-tight text-white max-w-4xl mx-auto leading-[1.1]">
            Acelere o seu Windows em{" "}
            <span className="text-transparent bg-clip-text text-gradient-cyan">
              1 Único Clique
            </span>
          </h1>

          {/* Subtítulo Descritivo */}
          <p className="mt-6 text-base sm:text-lg text-slate-300 max-w-2xl mx-auto leading-relaxed">
            Otimizador profissional desenvolvido em C# e .NET 10. Limpeza profunda de resíduos,
            calibração reversível de programas de inicialização e telemetria de hardware 100% real.
          </p>

          {/* Botões de Ação do Hero */}
          <div className="mt-8 flex flex-col sm:flex-row items-center justify-center gap-4">
            <a
              href={downloadUrl}
              onClick={handleDownload}
              target="_blank"
              rel="noopener noreferrer"
              className="w-full sm:w-auto px-8 py-4 rounded-xl glow-gradient text-black font-extrabold text-sm sm:text-base hover:scale-105 active:scale-95 transition-all flex items-center justify-center space-x-3 shadow-xl shadow-cyan-500/30 ring-1 ring-white/40"
            >
              <Download className="w-5 h-5 stroke-[2.5]" />
              <span>Baixar LSPKOptimizerSetup.exe</span>
            </a>
            <a
              href="#simulador"
              className="w-full sm:w-auto px-7 py-4 rounded-xl bg-slate-900/90 hover:bg-slate-800 border border-slate-700 text-slate-200 font-bold text-sm sm:text-base transition-all flex items-center justify-center space-x-2"
            >
              <Play className="w-4 h-4 text-cyan-400" />
              <span>Testar Simulador Interativo</span>
            </a>
          </div>

          {/* Indicadores de Credibilidade Técnica */}
          <div className="mt-8 flex flex-wrap items-center justify-center gap-6 text-xs text-slate-400">
            <div className="flex items-center space-x-1.5">
              <ShieldCheck className="w-4 h-4 text-cyan-400" />
              <span>Blindagem PathValidator</span>
            </div>
            <div className="flex items-center space-x-1.5">
              <Lock className="w-4 h-4 text-cyan-400" />
              <span>Sem Login ou Telemetria</span>
            </div>
            <div className="flex items-center space-x-1.5">
              <CheckCircle2 className="w-4 h-4 text-cyan-400" />
              <span>Instalação sem Elevação UAC</span>
            </div>
          </div>

          {/* =========================================================================
              SIMULADOR INTERATIVO AO VIVO DA INTERFACE WPF
              ========================================================================= */}
          <div id="simulador" className="mt-16 max-w-5xl mx-auto text-left">
            <div className="text-center mb-4">
              <span className="text-xs uppercase font-bold tracking-widest text-cyan-400">
                Experimente Antes de Instalar
              </span>
              <p className="text-sm text-slate-400 mt-1">
                Clique no botão central <strong>"OTIMIZAR AGORA"</strong> abaixo para ver a rotina em execução
              </p>
            </div>

            {/* Janela Simulada com Moldura do Windows */}
            <div className="rounded-2xl border border-cyan-500/30 bg-[#0B0F19] shadow-2xl shadow-cyan-950/50 overflow-hidden ring-1 ring-white/10 relative">
              {/* Barra de Título do Windows */}
              <div className="h-10 bg-[#070B14] border-b border-slate-800 px-4 flex items-center justify-between">
                <div className="flex items-center space-x-2.5">
                  <div className="w-4 h-4 rounded glow-gradient flex items-center justify-center">
                    <Zap className="w-2.5 h-2.5 fill-black text-black stroke-[3]" />
                  </div>
                  <span className="text-xs font-semibold text-slate-300">
                    LSPK Optimizer — Painel de Controle de Performance (v1.0.0)
                  </span>
                </div>
                <div className="flex items-center space-x-2">
                  <span className="w-3 h-3 rounded-full bg-slate-700/80 inline-block" />
                  <span className="w-3 h-3 rounded-full bg-slate-700/80 inline-block" />
                  <span className="w-3 h-3 rounded-full bg-rose-500/70 inline-block" />
                </div>
              </div>

              {/* Corpo da Janela do Aplicativo */}
              <div className="p-6 sm:p-8 bg-gradient-to-b from-[#0F172A]/90 to-[#0B0F19]">
                {/* Cartões de Telemetria de Hardware */}
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-8">
                  {/* SO */}
                  <div className="p-4 rounded-xl bg-slate-900/80 border border-slate-800">
                    <div className="flex items-center space-x-2 text-cyan-400 text-xs font-bold uppercase mb-1">
                      <Terminal className="w-3.5 h-3.5" />
                      <span>Sistema Operacional</span>
                    </div>
                    <div className="text-sm font-bold text-white truncate">Windows 11 Pro</div>
                    <div className="text-[11px] text-slate-400">Versão 22H2 (x64)</div>
                  </div>

                  {/* CPU */}
                  <div className="p-4 rounded-xl bg-slate-900/80 border border-slate-800">
                    <div className="flex items-center space-x-2 text-cyan-400 text-xs font-bold uppercase mb-1">
                      <Cpu className="w-3.5 h-3.5" />
                      <span>Processador</span>
                    </div>
                    <div className="text-sm font-bold text-white truncate">Intel Core i9-14900K</div>
                    <div className="text-[11px] text-slate-400">32 Núcleos Lógicos</div>
                  </div>

                  {/* RAM */}
                  <div className="p-4 rounded-xl bg-slate-900/80 border border-slate-800">
                    <div className="flex items-center space-x-2 text-cyan-400 text-xs font-bold uppercase mb-1">
                      <Layers className="w-3.5 h-3.5" />
                      <span>Memória RAM</span>
                    </div>
                    <div className="text-sm font-bold text-white">6.4 / 32.0 GB</div>
                    <div className="text-[11px] text-slate-400">20% em utilização</div>
                  </div>

                  {/* Disco */}
                  <div className="p-4 rounded-xl bg-slate-900/80 border border-slate-800">
                    <div className="flex items-center space-x-2 text-cyan-400 text-xs font-bold uppercase mb-1">
                      <HardDrive className="w-3.5 h-3.5" />
                      <span>Disco do Sistema</span>
                    </div>
                    <div className="text-sm font-bold text-white">342 GB Livres</div>
                    <div className="text-[11px] text-slate-400">Capacidade: 953 GB (NVMe)</div>
                  </div>
                </div>

                {/* Bloco Central do Simulador: Health Score e Botão Otimizar */}
                <div className="p-8 rounded-2xl bg-[#131B2E]/90 border border-slate-700/80 flex flex-col items-center text-center relative overflow-hidden">
                  {/* Indicador Circular de Saúde */}
                  <div className="relative w-36 h-36 flex items-center justify-center mb-6">
                    {/* Círculo de fundo */}
                    <svg className="w-full h-full transform -rotate-90" viewBox="0 0 100 100">
                      <circle
                        cx="50"
                        cy="50"
                        r="42"
                        stroke="#1E293B"
                        strokeWidth="8"
                        fill="transparent"
                      />
                      <circle
                        cx="50"
                        cy="50"
                        r="42"
                        stroke={simScore > 80 ? "#10B981" : "#00F2FE"}
                        strokeWidth="8"
                        strokeDasharray={264}
                        strokeDashoffset={264 - (264 * simScore) / 100}
                        strokeLinecap="round"
                        fill="transparent"
                        className="transition-all duration-700 ease-out"
                      />
                    </svg>
                    <div className="absolute inset-0 flex flex-col items-center justify-center">
                      <span className="text-3xl font-extrabold text-white">{simScore}</span>
                      <span className="text-[10px] uppercase font-bold text-slate-400">de 100</span>
                    </div>
                  </div>

                  <h3 className="text-lg font-bold text-white mb-1">
                    {simCompleted
                      ? "Sistema Otimizado com Sucesso!"
                      : simOptimizing
                      ? "Otimização em Andamento..."
                      : "Pontuação de Saúde Atual: 64/100"}
                  </h3>

                  <p className="text-xs text-slate-400 max-w-md mb-6">
                    {simCompleted
                      ? `Foram liberados ${simFreedMb} MB de resíduos e calibrados 5 itens de inicialização com segurança total.`
                      : simOptimizing
                      ? simSteps[simStep]
                      : "Identificamos 4.2 GB de arquivos temporários e 5 inicializadores com impacto que podem ser desativados sem risco."}
                  </p>

                  {/* Barra de Progresso Real */}
                  {simOptimizing && (
                    <div className="w-full max-w-md mb-6">
                      <div className="flex justify-between text-xs font-semibold text-slate-300 mb-2">
                        <span>Etapa {simStep + 1} de 7</span>
                        <span className="text-cyan-400">{simProgress}%</span>
                      </div>
                      <div className="w-full h-2.5 rounded-full bg-slate-800 overflow-hidden">
                        <div
                          className="h-full glow-gradient transition-all duration-300 rounded-full"
                          style={{ width: `${simProgress}%` }}
                        />
                      </div>
                    </div>
                  )}

                  {/* Ações do Simulador */}
                  <div className="flex items-center space-x-3">
                    {!simCompleted ? (
                      <button
                        onClick={handleStartSimulation}
                        disabled={simOptimizing}
                        className={`px-8 py-3.5 rounded-xl font-extrabold text-sm flex items-center space-x-2 transition-all shadow-lg ${
                          simOptimizing
                            ? "bg-slate-700 text-slate-400 cursor-not-allowed"
                            : "glow-gradient text-black hover:scale-105 active:scale-95 shadow-cyan-500/30"
                        }`}
                      >
                        {simOptimizing ? (
                          <>
                            <RefreshCw className="w-4 h-4 animate-spin text-slate-400" />
                            <span>Otimizando PC...</span>
                          </>
                        ) : (
                          <>
                            <Zap className="w-4 h-4 fill-black text-black stroke-[3]" />
                            <span>OTIMIZAR AGORA (Simular)</span>
                          </>
                        )}
                      </button>
                    ) : (
                      <button
                        onClick={handleResetSimulation}
                        className="px-6 py-3 rounded-xl bg-slate-800 hover:bg-slate-700 border border-slate-600 text-slate-200 text-xs font-bold transition flex items-center space-x-2"
                      >
                        <RefreshCw className="w-3.5 h-3.5 text-cyan-400" />
                        <span>Testar Novamente</span>
                      </button>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          3. BARRA DE IMPACTO E NÚMEROS REAIS
          ========================================================================= */}
      <section className="py-12 border-y border-slate-800/80 bg-[#090D18]">
        <div className="max-w-7xl mx-auto px-6">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
            <div className="p-4">
              <div className="text-3xl sm:text-4xl font-extrabold text-white text-gradient-cyan">
                1 Clique
              </div>
              <div className="text-xs text-slate-400 font-medium mt-1">
                Fluxo automático e completo de ponta a ponta
              </div>
            </div>
            <div className="p-4">
              <div className="text-3xl sm:text-4xl font-extrabold text-white">4.2+ GB</div>
              <div className="text-xs text-slate-400 font-medium mt-1">
                Média de espaço recuperado no primeiro escaneamento
              </div>
            </div>
            <div className="p-4">
              <div className="text-3xl sm:text-4xl font-extrabold text-emerald-400">100%</div>
              <div className="text-xs text-slate-400 font-medium mt-1">
                Reversibilidade total com backup de inicialização
              </div>
            </div>
            <div className="p-4">
              <div className="text-3xl sm:text-4xl font-extrabold text-cyan-400">0 Mocks</div>
              <div className="text-xs text-slate-400 font-medium mt-1">
                Sem telemetria oculta, sem banco remoto e sem login
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          4. SEÇÃO DE BENEFÍCIOS E DIFERENCIAIS DE ENGENHARIA
          ========================================================================= */}
      <section id="beneficios" className="py-24 relative z-10">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-3xl mx-auto mb-16">
            <h2 className="text-xs font-bold text-cyan-400 tracking-wider uppercase">
              Diferenciais de Engenharia
            </h2>
            <p className="text-3xl sm:text-4xl font-extrabold text-white mt-2">
              Por que o LSPK Optimizer é superior aos programas genéricos?
            </p>
            <p className="mt-4 text-sm text-slate-400 leading-relaxed">
              Desenvolvido pela LSPK Technology seguindo as diretrizes nativas do Windows, eliminando
              o risco de quebrar o sistema operacional ou apagar arquivos pessoais.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {/* Card 1 */}
            <div className="glass-panel p-8 rounded-2xl transition-all hover:-translate-y-1">
              <div className="w-12 h-12 rounded-xl bg-cyan-950/80 border border-cyan-500/30 flex items-center justify-center text-cyan-400 mb-6">
                <Cpu className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white mb-2">Desempenho Nativo em C# .NET 10</h3>
              <p className="text-xs text-slate-300 leading-relaxed">
                Sem Electron ou interfaces web empacotadas pesadas. O executável standalone consome
                menos de 35 MB de RAM durante a execução e responde instantaneamente.
              </p>
            </div>

            {/* Card 2 */}
            <div className="glass-panel p-8 rounded-2xl transition-all hover:-translate-y-1">
              <div className="w-12 h-12 rounded-xl bg-cyan-950/80 border border-cyan-500/30 flex items-center justify-center text-cyan-400 mb-6">
                <ShieldCheck className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white mb-2">Blindagem PathValidator</h3>
              <p className="text-xs text-slate-300 leading-relaxed">
                Algoritmo estrito de verificação de caminhos. Bloqueia qualquer tentativa de varredura
                em Documentos, Downloads, Área de Trabalho ou diretórios vitais do Windows.
              </p>
            </div>

            {/* Card 3 */}
            <div className="glass-panel p-8 rounded-2xl transition-all hover:-translate-y-1">
              <div className="w-12 h-12 rounded-xl bg-cyan-950/80 border border-cyan-500/30 flex items-center justify-center text-cyan-400 mb-6">
                <RefreshCw className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white mb-2">Inicialização Reversível</h3>
              <p className="text-xs text-slate-300 leading-relaxed">
                Diferente de limpadores comuns que apagam registros, o LSPK Optimizer move as chaves
                para chaves seguras de backup (Run-Disabled), permitindo reativar com 1 clique.
              </p>
            </div>

            {/* Card 4 */}
            <div className="glass-panel p-8 rounded-2xl transition-all hover:-translate-y-1">
              <div className="w-12 h-12 rounded-xl bg-cyan-950/80 border border-cyan-500/30 flex items-center justify-center text-cyan-400 mb-6">
                <Lock className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white mb-2">Instalação sem Elevação UAC</h3>
              <p className="text-xs text-slate-300 leading-relaxed">
                Instala no escopo por usuário (%LocalAppData%\Programs\LSPK Optimizer). Você instala e
                utiliza imediatamente sem exigir privilégios de administrador invasivos.
              </p>
            </div>

            {/* Card 5 */}
            <div className="glass-panel p-8 rounded-2xl transition-all hover:-translate-y-1">
              <div className="w-12 h-12 rounded-xl bg-cyan-950/80 border border-cyan-500/30 flex items-center justify-center text-cyan-400 mb-6">
                <Activity className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white mb-2">Privacidade Offline Total</h3>
              <p className="text-xs text-slate-300 leading-relaxed">
                Sem banco de dados remoto, sem criação de contas e sem envio de relatórios pela
                internet. Toda a telemetria é processada localmente na sua máquina.
              </p>
            </div>

            {/* Card 6 */}
            <div className="glass-panel p-8 rounded-2xl transition-all hover:-translate-y-1">
              <div className="w-12 h-12 rounded-xl bg-cyan-950/80 border border-cyan-500/30 flex items-center justify-center text-cyan-400 mb-6">
                <Sliders className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white mb-2">Atualizador Atômico com Rollback</h3>
              <p className="text-xs text-slate-300 leading-relaxed">
                Arquitetura desacoplada (LSPK.Optimizer.Updater.exe) com checagem SHA-256 e rollback
                automático caso haja interrupção no download da atualização.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          5. COMO FUNCIONA (PIPELINE DE 9 ETAPAS EM 1 CLIQUE)
          ========================================================================= */}
      <section id="funcionamento" className="py-24 border-t border-slate-800/80 bg-[#090D18]">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-3xl mx-auto mb-16">
            <h2 className="text-xs font-bold text-cyan-400 tracking-wider uppercase">
              Funcionamento Transparente
            </h2>
            <p className="text-3xl sm:text-4xl font-extrabold text-white mt-2">
              O que o botão "OTIMIZAR AGORA" faz nos bastidores?
            </p>
            <p className="mt-4 text-sm text-slate-400 leading-relaxed">
              Cada etapa é executada de forma sequencial com acompanhamento em tempo real e
              tratamento de exceções por arquivo. Uma falha individual nunca paralisa a rotina.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="p-6 rounded-2xl bg-[#0F172A]/70 border border-slate-800">
              <div className="text-cyan-400 text-xs font-bold mb-2">FASE 1: DIAGNÓSTICO</div>
              <h4 className="text-base font-bold text-white mb-2">Varredura e Auditoria</h4>
              <p className="text-xs text-slate-300 leading-relaxed">
                Varre %TEMP%, C:\Windows\Temp, thumbnail caches, shader caches e analisa as chaves de
                registro de inicialização (HKCU e HKLM Run), identificando entradas órfãs.
              </p>
            </div>

            <div className="p-6 rounded-2xl bg-[#0F172A]/70 border border-slate-800">
              <div className="text-cyan-400 text-xs font-bold mb-2">FASE 2: BLINDAGEM E LIMPEZA</div>
              <h4 className="text-base font-bold text-white mb-2">Exclusão Validada e Segura</h4>
              <p className="text-xs text-slate-300 leading-relaxed">
                Submete cada arquivo ao PathValidator. Remove resíduos confirmados, preservando
                arquivos em uso e pastas protegidas. Libera a Lixeira via API nativa do Windows.
              </p>
            </div>

            <div className="p-6 rounded-2xl bg-[#0F172A]/70 border border-slate-800">
              <div className="text-cyan-400 text-xs font-bold mb-2">FASE 3: INICIALIZAÇÃO E RELATÓRIO</div>
              <h4 className="text-base font-bold text-white mb-2">Ajuste Reversível e Métricas</h4>
              <p className="text-xs text-slate-300 leading-relaxed">
                Aplica backup das chaves de inicialização selecionadas. Protege antivírus e drivers
                via SystemWhitelist e emite relatório com espaço e itens otimizados.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          6. SEÇÃO INTERATIVA DOS MÓDULOS (LIMPEZA vs INICIALIZAÇÃO)
          ========================================================================= */}
      <section id="modulos" className="py-24 relative z-10">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-3xl mx-auto mb-12">
            <h2 className="text-xs font-bold text-cyan-400 tracking-wider uppercase">
              Módulos Especializados
            </h2>
            <p className="text-3xl sm:text-4xl font-extrabold text-white mt-2">
              Controle granular sobre cada aspecto do seu PC
            </p>
            <p className="mt-4 text-sm text-slate-400 leading-relaxed">
              Além do botão de otimização automática, você pode acessar e calibrar cada módulo
              individualmente. Experimente nos controles interativos abaixo:
            </p>
          </div>

          {/* Seletor de Abas de Módulo */}
          <div className="flex justify-center mb-10">
            <div className="p-1 rounded-xl bg-slate-900 border border-slate-800 flex space-x-2">
              <button
                onClick={() => setActiveModuleTab("cleaner")}
                className={`px-6 py-2.5 rounded-lg text-xs sm:text-sm font-bold flex items-center space-x-2 transition-all ${
                  activeModuleTab === "cleaner"
                    ? "glow-gradient text-black shadow-lg shadow-cyan-500/20"
                    : "text-slate-400 hover:text-white"
                }`}
              >
                <Trash2 className="w-4 h-4" />
                <span>Módulo 1: Limpeza de Arquivos</span>
              </button>
              <button
                onClick={() => setActiveModuleTab("startup")}
                className={`px-6 py-2.5 rounded-lg text-xs sm:text-sm font-bold flex items-center space-x-2 transition-all ${
                  activeModuleTab === "startup"
                    ? "glow-gradient text-black shadow-lg shadow-cyan-500/20"
                    : "text-slate-400 hover:text-white"
                }`}
              >
                <Clock className="w-4 h-4" />
                <span>Módulo 2: Inicialização do Windows</span>
              </button>
            </div>
          </div>

          {/* Conteúdo Interativo: Módulo 1 (Limpeza) */}
          {activeModuleTab === "cleaner" && (
            <div className="max-w-4xl mx-auto glass-panel p-6 sm:p-8 rounded-2xl border border-slate-700/80">
              <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-6 mb-6 border-b border-slate-800 gap-4">
                <div>
                  <h3 className="text-lg font-bold text-white flex items-center space-x-2">
                    <Trash2 className="w-5 h-5 text-cyan-400" />
                    <span>Seletor de Categorias de Limpeza</span>
                  </h3>
                  <p className="text-xs text-slate-400 mt-1">
                    Marque ou desmarque as áreas para simular o cálculo dinâmico de liberação de disco
                  </p>
                </div>
                <div className="text-right bg-slate-900/90 px-4 py-2 rounded-xl border border-cyan-500/30">
                  <span className="text-[10px] uppercase font-bold text-slate-400 block">
                    Espaço Selecionado
                  </span>
                  <span className="text-xl font-extrabold text-cyan-400">
                    {(totalSelectedCleanMb / 1024).toFixed(2)} GB
                  </span>
                </div>
              </div>

              {/* Lista de Categorias Interativas */}
              <div className="space-y-3">
                {cleanCategories.map(cat => (
                  <div
                    key={cat.id}
                    onClick={() => toggleCleanCategory(cat.id)}
                    className={`p-4 rounded-xl border transition-all cursor-pointer flex items-center justify-between ${
                      cat.selected
                        ? "bg-slate-900/80 border-cyan-500/40 text-white"
                        : "bg-slate-950/40 border-slate-800 text-slate-400 opacity-60 hover:opacity-100"
                    }`}
                  >
                    <div className="flex items-center space-x-3.5">
                      <div
                        className={`w-5 h-5 rounded flex items-center justify-center border transition ${
                          cat.selected
                            ? "bg-cyan-500 border-cyan-400 text-black"
                            : "border-slate-600 bg-slate-900"
                        }`}
                      >
                        {cat.selected && <Check className="w-3.5 h-3.5 stroke-[3]" />}
                      </div>
                      <div>
                        <div className="text-sm font-semibold">{cat.name}</div>
                        <div className="text-[11px] text-slate-500 font-mono">{cat.path}</div>
                      </div>
                    </div>
                    <div className="text-right">
                      <span className="text-sm font-bold text-slate-200">
                        {cat.sizeMb > 1000
                          ? `${(cat.sizeMb / 1024).toFixed(1)} GB`
                          : `${cat.sizeMb} MB`}
                      </span>
                      <span className="block text-[10px] text-emerald-400 font-medium">
                        Blindado por PathValidator
                      </span>
                    </div>
                  </div>
                ))}
              </div>

              <div className="mt-6 p-4 rounded-xl bg-cyan-950/30 border border-cyan-500/20 text-xs text-cyan-300 flex items-start space-x-3">
                <ShieldCheck className="w-5 h-5 text-cyan-400 shrink-0 mt-0.5" />
                <span>
                  <strong>Garantia de Integridade:</strong> O LSPK Optimizer nunca remove arquivos
                  pessoais, downloads, documentos, fotos ou pastas de trabalho. Pastas raízes como
                  C:\ e C:\Windows são protegidas por código inviolável.
                </span>
              </div>
            </div>
          )}

          {/* Conteúdo Interativo: Módulo 2 (Inicialização) */}
          {activeModuleTab === "startup" && (
            <div className="max-w-4xl mx-auto glass-panel p-6 sm:p-8 rounded-2xl border border-slate-700/80">
              <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-6 mb-6 border-b border-slate-800 gap-4">
                <div>
                  <h3 className="text-lg font-bold text-white flex items-center space-x-2">
                    <Clock className="w-5 h-5 text-cyan-400" />
                    <span>Gerenciador Reversível de Inicialização</span>
                  </h3>
                  <p className="text-xs text-slate-400 mt-1">
                    Ative ou desative itens de inicialização sem apagar chaves do registro
                  </p>
                </div>
                <div className="flex items-center space-x-3">
                  <div className="text-right bg-slate-900/90 px-4 py-2 rounded-xl border border-slate-700">
                    <span className="text-[10px] uppercase font-bold text-slate-400 block">
                      Programas Ativos
                    </span>
                    <span className="text-xl font-extrabold text-white">
                      {activeStartupCount} de {startupItems.length}
                    </span>
                  </div>
                </div>
              </div>

              {/* Lista de Programas de Inicialização */}
              <div className="space-y-3">
                {startupItems.map(item => (
                  <div
                    key={item.id}
                    className={`p-4 rounded-xl border transition-all flex items-center justify-between ${
                      item.enabled
                        ? "bg-slate-900/80 border-slate-700 text-white"
                        : "bg-slate-950/60 border-slate-800 text-slate-400"
                    }`}
                  >
                    <div className="flex items-center space-x-3.5">
                      <div
                        className={`w-9 h-9 rounded-lg flex items-center justify-center ${
                          item.isSystemProtected
                            ? "bg-emerald-950/70 border border-emerald-500/30 text-emerald-400"
                            : item.isOrphan
                            ? "bg-rose-950/70 border border-rose-500/30 text-rose-400"
                            : "bg-slate-800 border border-slate-700 text-slate-300"
                        }`}
                      >
                        {item.isSystemProtected ? (
                          <Lock className="w-4 h-4" />
                        ) : item.isOrphan ? (
                          <AlertCircle className="w-4 h-4" />
                        ) : (
                          <Activity className="w-4 h-4" />
                        )}
                      </div>
                      <div>
                        <div className="flex items-center space-x-2">
                          <span className="text-sm font-semibold">{item.name}</span>
                          {item.isSystemProtected && (
                            <span className="text-[10px] font-bold px-2 py-0.5 rounded bg-emerald-950 border border-emerald-500/30 text-emerald-400">
                              Protegido por Whitelist
                            </span>
                          )}
                          {item.isOrphan && (
                            <span className="text-[10px] font-bold px-2 py-0.5 rounded bg-rose-950 border border-rose-500/30 text-rose-400">
                              Executável Não Encontrado
                            </span>
                          )}
                        </div>
                        <div className="text-[11px] text-slate-400">{item.publisher}</div>
                      </div>
                    </div>

                    <div className="flex items-center space-x-4">
                      <div className="text-right">
                        <span
                          className={`text-xs font-bold ${
                            item.impact === "Alto"
                              ? "text-rose-400"
                              : item.impact === "Médio"
                              ? "text-amber-400"
                              : "text-emerald-400"
                          }`}
                        >
                          Impacto {item.impact}
                        </span>
                      </div>

                      {/* Botão de Toggle */}
                      {item.isSystemProtected ? (
                        <div
                          title="Componentes essenciais e antivírus não podem ser desativados"
                          className="w-12 h-6 rounded-full bg-emerald-950/60 border border-emerald-500/40 flex items-center px-1 cursor-not-allowed"
                        >
                          <div className="w-4 h-4 rounded-full bg-emerald-400 ml-auto" />
                        </div>
                      ) : (
                        <button
                          onClick={() => toggleStartupItem(item.id)}
                          className={`w-12 h-6 rounded-full transition-colors flex items-center px-1 ${
                            item.enabled
                              ? "bg-cyan-500 justify-end"
                              : "bg-slate-800 border border-slate-700 justify-start"
                          }`}
                        >
                          <div
                            className={`w-4 h-4 rounded-full transition-all ${
                              item.enabled ? "bg-black" : "bg-slate-400"
                            }`}
                          />
                        </button>
                      )}
                    </div>
                  </div>
                ))}
              </div>

              <div className="mt-6 p-4 rounded-xl bg-slate-900/80 border border-slate-800 text-xs text-slate-300 flex items-start space-x-3">
                <CheckCircle2 className="w-5 h-5 text-emerald-400 shrink-0 mt-0.5" />
                <span>
                  <strong>Backup Reversível:</strong> Desativar um aplicativo move seu comando para
                  a subchave <code>Software\Microsoft\Windows\CurrentVersion\Run-Disabled</code>.
                  Você pode reativá-lo com segurança a qualquer momento sem perder o caminho original.
                </span>
              </div>
            </div>
          )}
        </div>
      </section>

      {/* =========================================================================
          7. SEÇÃO DE SEGURANÇA E MATRIZ COMPARATIVA
          ========================================================================= */}
      <section id="seguranca" className="py-24 border-t border-slate-800/80 bg-[#090D18]">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-3xl mx-auto mb-16">
            <h2 className="text-xs font-bold text-cyan-400 tracking-wider uppercase">
              Segurança e Transparência
            </h2>
            <p className="text-3xl sm:text-4xl font-extrabold text-white mt-2">
              LSPK Optimizer vs. Softwares Genéricos
            </p>
            <p className="mt-4 text-sm text-slate-400 leading-relaxed">
              Veja a diferença técnica entre a abordagem profissional da LSPK Technology e os
              programas tradicionais de manutenção que colocam seu Windows em risco.
            </p>
          </div>

          <div className="max-w-4xl mx-auto rounded-2xl border border-slate-800 bg-[#0B0F19] overflow-hidden shadow-xl">
            <div className="grid grid-cols-12 bg-slate-900/90 border-b border-slate-800 p-4 text-xs font-bold text-slate-300">
              <div className="col-span-6 sm:col-span-6">Critério de Segurança e Arquitetura</div>
              <div className="col-span-3 sm:col-span-3 text-center text-cyan-400">LSPK Optimizer</div>
              <div className="col-span-3 sm:col-span-3 text-center text-slate-500">Programas Genéricos</div>
            </div>

            <div className="divide-y divide-slate-800/60 text-xs">
              {/* Linha 1 */}
              <div className="grid grid-cols-12 p-4 items-center">
                <div className="col-span-6 font-semibold text-slate-200">
                  Proteção de Arquivos Pessoais (Fotos, Documentos, Downloads)
                </div>
                <div className="col-span-3 text-center text-emerald-400 font-bold flex items-center justify-center space-x-1">
                  <CheckCircle2 className="w-4 h-4" />
                  <span>Blindado por Código</span>
                </div>
                <div className="col-span-3 text-center text-rose-400 font-medium flex items-center justify-center space-x-1">
                  <XCircle className="w-4 h-4" />
                  <span>Risco de Perda</span>
                </div>
              </div>

              {/* Linha 2 */}
              <div className="grid grid-cols-12 p-4 items-center bg-slate-900/30">
                <div className="col-span-6 font-semibold text-slate-200">
                  Backup Automático antes de Modificar Registros
                </div>
                <div className="col-span-3 text-center text-emerald-400 font-bold flex items-center justify-center space-x-1">
                  <CheckCircle2 className="w-4 h-4" />
                  <span>Reversível (Run-Disabled)</span>
                </div>
                <div className="col-span-3 text-center text-rose-400 font-medium flex items-center justify-center space-x-1">
                  <XCircle className="w-4 h-4" />
                  <span>Apaga Permanentemente</span>
                </div>
              </div>

              {/* Linha 3 */}
              <div className="grid grid-cols-12 p-4 items-center">
                <div className="col-span-6 font-semibold text-slate-200">
                  Proteção Obrigatória de Antivírus e Drivers Vitais
                </div>
                <div className="col-span-3 text-center text-emerald-400 font-bold flex items-center justify-center space-x-1">
                  <CheckCircle2 className="w-4 h-4" />
                  <span>SystemWhitelist Ativa</span>
                </div>
                <div className="col-span-3 text-center text-rose-400 font-medium flex items-center justify-center space-x-1">
                  <XCircle className="w-4 h-4" />
                  <span>Desativa sem Critério</span>
                </div>
              </div>

              {/* Linha 4 */}
              <div className="grid grid-cols-12 p-4 items-center bg-slate-900/30">
                <div className="col-span-6 font-semibold text-slate-200">
                  Instalação Limpa sem Pedir Elevação de Administrador (UAC)
                </div>
                <div className="col-span-3 text-center text-emerald-400 font-bold flex items-center justify-center space-x-1">
                  <CheckCircle2 className="w-4 h-4" />
                  <span>Escopo por Usuário</span>
                </div>
                <div className="col-span-3 text-center text-rose-400 font-medium flex items-center justify-center space-x-1">
                  <XCircle className="w-4 h-4" />
                  <span>Exige UAC Obrigatório</span>
                </div>
              </div>

              {/* Linha 5 */}
              <div className="grid grid-cols-12 p-4 items-center">
                <div className="col-span-6 font-semibold text-slate-200">
                  Privacidade Local (Sem Banco Remoto, Telemetria ou Login)
                </div>
                <div className="col-span-3 text-center text-emerald-400 font-bold flex items-center justify-center space-x-1">
                  <CheckCircle2 className="w-4 h-4" />
                  <span>100% Offline e Local</span>
                </div>
                <div className="col-span-3 text-center text-rose-400 font-medium flex items-center justify-center space-x-1">
                  <XCircle className="w-4 h-4" />
                  <span>Coleta Dados na Nuvem</span>
                </div>
              </div>

              {/* Linha 6 */}
              <div className="grid grid-cols-12 p-4 items-center bg-slate-900/30">
                <div className="col-span-6 font-semibold text-slate-200">
                  Adware, Barras Ocultas ou Ofertas de Terceiros
                </div>
                <div className="col-span-3 text-center text-emerald-400 font-bold flex items-center justify-center space-x-1">
                  <CheckCircle2 className="w-4 h-4" />
                  <span>Zero Bloatware</span>
                </div>
                <div className="col-span-3 text-center text-rose-400 font-medium flex items-center justify-center space-x-1">
                  <XCircle className="w-4 h-4" />
                  <span>Frequentemente Embutido</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          8. SEÇÃO DE COMPATIBILIDADE E REQUISITOS DO SISTEMA
          ========================================================================= */}
      <section id="compatibilidade" className="py-24 relative z-10">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-3xl mx-auto mb-16">
            <h2 className="text-xs font-bold text-cyan-400 tracking-wider uppercase">
              Compatibilidade
            </h2>
            <p className="text-3xl sm:text-4xl font-extrabold text-white mt-2">
              Pronto para as versões modernas do Windows
            </p>
            <p className="mt-4 text-sm text-slate-400 leading-relaxed">
              Projetado e homologado para sistemas operacionais de 64 bits da Microsoft.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8 max-w-4xl mx-auto">
            {/* Windows 11 */}
            <div className="p-8 rounded-2xl glass-panel border border-slate-700/80">
              <div className="flex items-center space-x-3 mb-4">
                <div className="w-10 h-10 rounded-xl bg-cyan-950 border border-cyan-500/30 flex items-center justify-center text-cyan-400">
                  <Terminal className="w-5 h-5" />
                </div>
                <div>
                  <h3 className="text-lg font-bold text-white">Windows 11 (64-bit)</h3>
                  <span className="text-xs text-emerald-400 font-semibold">100% Compatível e Otimizado</span>
                </div>
              </div>
              <ul className="space-y-2 text-xs text-slate-300">
                <li className="flex items-center space-x-2">
                  <Check className="w-4 h-4 text-cyan-400" />
                  <span>Versões 21H2, 22H2, 23H2 e 24H2 suportadas</span>
                </li>
                <li className="flex items-center space-x-2">
                  <Check className="w-4 h-4 text-cyan-400" />
                  <span>Suporte completo ao novo Gerenciador de Tarefas</span>
                </li>
                <li className="flex items-center space-x-2">
                  <Check className="w-4 h-4 text-cyan-400" />
                  <span>Otimização para arquiteturas com núcleos híbridos (P/E cores)</span>
                </li>
              </ul>
            </div>

            {/* Windows 10 */}
            <div className="p-8 rounded-2xl glass-panel border border-slate-700/80">
              <div className="flex items-center space-x-3 mb-4">
                <div className="w-10 h-10 rounded-xl bg-cyan-950 border border-cyan-500/30 flex items-center justify-center text-cyan-400">
                  <Terminal className="w-5 h-5" />
                </div>
                <div>
                  <h3 className="text-lg font-bold text-white">Windows 10 (64-bit)</h3>
                  <span className="text-xs text-emerald-400 font-semibold">100% Compatível</span>
                </div>
              </div>
              <ul className="space-y-2 text-xs text-slate-300">
                <li className="flex items-center space-x-2">
                  <Check className="w-4 h-4 text-cyan-400" />
                  <span>Versão 22H2 (Build 19045) ou superior</span>
                </li>
                <li className="flex items-center space-x-2">
                  <Check className="w-4 h-4 text-cyan-400" />
                  <span>Limpeza profunda de registros de atualização e crash dumps</span>
                </li>
                <li className="flex items-center space-x-2">
                  <Check className="w-4 h-4 text-cyan-400" />
                  <span>Requisitos mínimos: 1.0 GHz CPU, 100 MB RAM livre</span>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </section>

      {/* =========================================================================
          9. PERGUNTAS FREQUENTES (FAQ COM ACORDEÃO ANIMADO)
          ========================================================================= */}
      <section id="faq" className="py-24 border-t border-slate-800/80 bg-[#090D18]">
        <div className="max-w-4xl mx-auto px-6">
          <div className="text-center mb-16">
            <h2 className="text-xs font-bold text-cyan-400 tracking-wider uppercase">
              Tire suas Dúvidas
            </h2>
            <p className="text-3xl sm:text-4xl font-extrabold text-white mt-2">
              Perguntas Frequentes
            </p>
          </div>

          <div className="space-y-4">
            {[
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
                a: "Diferente de ferramentas genéricas, o LSPK Optimizer nunca deleta chaves permanentemente. Ele realiza backup automático em chaves dedicadas (Run-Disabled), permitindo que você reative o programa a qualquer momento com um único clique."
              },
              {
                q: "Como funcionam as atualizações automáticas?",
                a: "Ao abrir o aplicativo, uma consulta silenciosa verifica se há nova versão estável disponível. Caso haja, o pacote é baixado em segundo plano e validado com criptografia SHA-256. Você é notificado por um banner discreto para aplicar quando for conveniente."
              },
              {
                q: "Existe alguma telemetria remota ou envio de dados?",
                a: "Zero. O MVP do LSPK Optimizer opera 100% offline no seu computador. Não há criação de contas, logins, rastreadores ou bancos de dados remotos coletando informações dos seus hábitos ou hardware."
              }
            ].map((faq, index) => (
              <div
                key={index}
                className="rounded-xl bg-slate-900/70 border border-slate-800 overflow-hidden transition"
              >
                <button
                  onClick={() => setActiveFaq(activeFaq === index ? null : index)}
                  className="w-full p-6 text-left flex items-center justify-between font-semibold text-white text-sm sm:text-base hover:text-cyan-400 transition"
                >
                  <span>{faq.q}</span>
                  <ChevronDown
                    className={`w-5 h-5 text-cyan-400 transition-transform duration-300 ${
                      activeFaq === index ? "rotate-180" : ""
                    }`}
                  />
                </button>
                {activeFaq === index && (
                  <div className="px-6 pb-6 text-xs sm:text-sm text-slate-300 border-t border-slate-800/80 pt-4 leading-relaxed">
                    {faq.a}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* =========================================================================
          10. CALL-TO-ACTION FINAL COM ALTO IMPACTO
          ========================================================================= */}
      <section className="py-24 border-t border-slate-800/80 bg-gradient-to-b from-[#090D18] to-[#070B14] relative">
        <div className="max-w-4xl mx-auto px-6 text-center">
          <div className="w-16 h-16 rounded-2xl glow-gradient flex items-center justify-center text-black mx-auto mb-6 shadow-xl shadow-cyan-500/25 ring-1 ring-white/30">
            <Zap className="w-8 h-8 fill-black text-black stroke-[3]" />
          </div>

          <h2 className="text-3xl sm:text-5xl font-extrabold text-white tracking-tight">
            Recupere o Desempenho Máximo do seu Windows
          </h2>

          <p className="mt-4 text-sm sm:text-base text-slate-300 max-w-xl mx-auto leading-relaxed">
            Instalação rápida, sem anúncios e sem cadastro. Baixe agora o executável oficial otimizado
            compilado para Windows 10 e Windows 11.
          </p>

          <div className="mt-8 flex flex-col sm:flex-row items-center justify-center gap-4">
            <a
              href={downloadUrl}
              onClick={handleDownload}
              target="_blank"
              rel="noopener noreferrer"
              className="w-full sm:w-auto px-10 py-4 rounded-xl glow-gradient text-black font-extrabold text-base hover:scale-105 active:scale-95 transition-all flex items-center justify-center space-x-3 shadow-xl shadow-cyan-500/30 ring-1 ring-white/40"
            >
              <Download className="w-5 h-5 stroke-[2.5]" />
              <span>Baixar LSPKOptimizerSetup.exe (v1.0.0)</span>
            </a>
            <a
              href="https://github.com/paulohenriquegouveia08-oss/LSPK_Otimization/releases/tag/v1.0.0"
              target="_blank"
              rel="noreferrer"
              className="w-full sm:w-auto px-8 py-4 rounded-xl bg-slate-900 hover:bg-slate-800 border border-slate-700 text-slate-300 font-bold text-sm transition flex items-center justify-center space-x-2"
            >
              <ExternalLink className="w-4 h-4 text-cyan-400" />
              <span>Ver no GitHub Releases</span>
            </a>
          </div>
        </div>
      </section>

      {/* =========================================================================
          11. RODAPÉ INSTITUCIONAL DA LSPK TECHNOLOGY
          ========================================================================= */}
      <footer className="py-12 border-t border-slate-800/80 bg-[#05070D] text-slate-400 text-xs">
        <div className="max-w-7xl mx-auto px-6 flex flex-col md:flex-row items-center justify-between gap-6">
          <div className="flex items-center space-x-3">
            <div className="w-7 h-7 rounded-lg glow-gradient flex items-center justify-center text-black">
              <Zap className="w-4 h-4 fill-black text-black stroke-[3]" />
            </div>
            <div>
              <span className="font-bold text-slate-200">LSPK OPTIMIZER</span> — Um produto oficial da{" "}
              <strong className="text-white">LSPK Technology</strong>
            </div>
          </div>

          <div className="flex flex-wrap items-center justify-center gap-6 text-slate-400">
            <span>Privacidade Local Garantida</span>
            <span>•</span>
            <span>Sem Banco Remoto</span>
            <span>•</span>
            <span>Zero Bloatware</span>
            <span>•</span>
            <span>SHA-256 Verificado</span>
          </div>

          <div className="text-slate-500">
            © {new Date().getFullYear()} LSPK Technology. Todos os direitos reservados.
          </div>
        </div>
      </footer>

      {/* =========================================================================
          12. MODAL INFORMATIVO DE DOWNLOAD
          ========================================================================= */}
      {showDownloadModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md">
          <div className="relative w-full max-w-lg p-6 sm:p-8 rounded-2xl bg-[#0B0F19] border border-cyan-500/40 shadow-2xl shadow-cyan-950/60 ring-1 ring-white/10">
            {/* Botão Fechar */}
            <button
              onClick={() => setShowDownloadModal(false)}
              className="absolute top-4 right-4 p-2 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition"
            >
              <X className="w-5 h-5" />
            </button>

            {/* Cabeçalho do Modal */}
            <div className="flex items-center space-x-3 mb-4">
              <div className="w-10 h-10 rounded-xl glow-gradient flex items-center justify-center text-black">
                <Download className="w-5 h-5 stroke-[2.5]" />
              </div>
              <div>
                <h3 className="text-lg font-bold text-white">Download Oficial</h3>
                <span className="text-xs text-cyan-400 font-semibold">LSPKOptimizerSetup.exe (Instalador Otimizado)</span>
              </div>
            </div>

            <p className="text-xs text-slate-300 leading-relaxed mb-6">
              O instalador oficial está sendo transferido pelo seu navegador. Você também pode optar pelo formato portátil sem instalação:
            </p>

            {/* Botões de Ação Direta */}
            <div className="space-y-3 mb-6">
              <a
                href={downloadUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="w-full py-3.5 px-4 rounded-xl glow-gradient text-black font-extrabold text-sm flex items-center justify-center space-x-2 shadow-lg shadow-cyan-500/25 hover:opacity-95 transition"
              >
                <Download className="w-4 h-4 stroke-[2.5]" />
                <span>Baixar Instalador Oficial (.exe)</span>
              </a>

              <a
                href="https://github.com/paulohenriquegouveia08-oss/LSPK_Otimization/releases/download/v1.0.0/LSPKOptimizer-Portable.zip"
                target="_blank"
                rel="noopener noreferrer"
                className="w-full py-3 px-4 rounded-xl bg-slate-900 hover:bg-slate-800 border border-slate-700 text-slate-200 font-bold text-xs flex items-center justify-center space-x-2 transition"
              >
                <Download className="w-3.5 h-3.5 text-cyan-400" />
                <span>Baixar Versão Portátil (.zip — Sem Instalação)</span>
              </a>

              <a
                href="https://github.com/paulohenriquegouveia08-oss/LSPK_Otimization/releases/tag/v1.0.0"
                target="_blank"
                rel="noopener noreferrer"
                className="w-full py-2.5 px-4 rounded-xl bg-slate-950/70 hover:bg-slate-800 border border-slate-800 text-slate-300 font-medium text-xs flex items-center justify-center space-x-2 transition"
              >
                <ExternalLink className="w-3.5 h-3.5 text-cyan-400" />
                <span>Ver Releases e Checksums no GitHub</span>
              </a>
            </div>

            {/* Aviso Técnico do Windows Defender / SmartScreen */}
            <div className="p-4 rounded-xl bg-slate-900/90 border border-slate-800 text-xs text-slate-300">
              <div className="flex items-center space-x-2 font-bold text-cyan-400 mb-1">
                <ShieldCheck className="w-4 h-4 text-cyan-400" />
                <span>Dica de Execução no Windows 10 e Windows 11:</span>
              </div>
              <p className="text-[11px] text-slate-400 leading-relaxed">
                Se o Windows SmartScreen exibir a tela azul <em>"O Windows protegeu o seu computador"</em> (padrão para novos lançamentos na web): clique em <strong>"Mais informações"</strong> e em seguida em <strong>"Executar assim mesmo"</strong>. O instalador moderno abrirá imediatamente sem solicitar permissões invasivas de administrador.
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
