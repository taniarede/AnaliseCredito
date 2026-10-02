import type { PedidoRequest } from './types';

/**
 * Cenários do enunciado (Tarefa 2).
 * Usados para identificar os cenários na lista de pedidos e no cartão de resultado.
 */
export const CENARIOS: Record<string, PedidoRequest> = {
  A: { nif: '123456789', idade: 35, rendimentoMensalLiquido: 2500, prestacoesAtuais: 200, valorPretendido: 10000, prazoMeses: 60, situacaoProfissional: 'Efetivo', incidentesCredito: false },
  B: { nif: '123456789', idade: 42, rendimentoMensalLiquido: 2000, prestacoesAtuais: 500, valorPretendido: 20000, prazoMeses: 48, situacaoProfissional: 'ContratoPrazo', incidentesCredito: false },
  C: { nif: '123456789', idade: 40, rendimentoMensalLiquido: 3000, prestacoesAtuais: 300, valorPretendido: 15000, prazoMeses: 72, situacaoProfissional: 'Efetivo', incidentesCredito: true },
  D: { nif: '123456789', idade: 30, rendimentoMensalLiquido: 1200, prestacoesAtuais: 300, valorPretendido: 25000, prazoMeses: 36, situacaoProfissional: 'Efetivo', incidentesCredito: false },
};

/** Devolve "A", "B", "C" ou "D" se os dados do pedido forem exatamente os de um cenário do enunciado. */
export function identificarCenario(dados: PedidoRequest): string | null {
  const campos: (keyof PedidoRequest)[] = [
    'nif', 'idade', 'rendimentoMensalLiquido', 'prestacoesAtuais',
    'valorPretendido', 'prazoMeses', 'situacaoProfissional', 'incidentesCredito',
  ];
  const encontrado = Object.entries(CENARIOS).find(
    ([, cenario]) => campos.every((c) => cenario[c] === dados[c]),
  );
  return encontrado ? encontrado[0] : null;
}
