import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';

/**
 * Ponto de entrada da aplicação Web React da Landing Page do LSPK Optimizer.
 * Monta a árvore de componentes com StrictMode ativo no elemento #root do DOM.
 */
ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
