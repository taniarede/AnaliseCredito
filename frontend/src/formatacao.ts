import type { EstadoPedido } from './types';

const data = new Intl.DateTimeFormat('pt-PT', { dateStyle: 'short', timeStyle: 'short' });

/** 24000 -> "24.000,00" (ponto nos milhares, vírgula nas décimas - igual ao backend). */
export const formatarNumero = (v: number) => {
  const [inteiro, decimal] = Math.abs(v).toFixed(2).split('.');
  const comPontos = inteiro.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  return `${v < 0 ? '-' : ''}${comPontos},${decimal}`;
};

/** 24000 -> "24.000,00 €" */
export const formatarEuros = (v: number) => `${formatarNumero(v)} €`;
export const formatarData = (iso: string) => data.format(new Date(iso));

/** Classe CSS por estado (cores do cartão de decisão). */
export const classeEstado: Record<EstadoPedido, string> = {
  Aprovado: 'estado-aprovado',
  AnaliseManual: 'estado-manual',
  Recusado: 'estado-recusado',
  PedidoInvalido: 'estado-invalido',
};

export const descricaoEstado: Record<EstadoPedido, string> = {
  Aprovado: 'APROVADO',
  AnaliseManual: 'ANÁLISE MANUAL',
  Recusado: 'RECUSADO',
  PedidoInvalido: 'PEDIDO INVÁLIDO',
};

/**
 * Idade no fim do contrato, só para apresentação (o backend calcula com decimais).
 * - Perto do limite dos 75 anos (entre 74 e 76): anos e meses, ex.: "74 anos e 11 meses", "75 anos e 1 mês".
 * - Nos restantes casos: só os anos inteiros, ex.: "46 anos".
 */
export const formatarIdadeFinal = (idade: number) => {
  let anos = Math.floor(idade);
  if (anos < 74 || anos > 75) return `${anos} anos`;

  let meses = Math.round((idade - anos) * 12);
  if (meses === 12) {
    anos += 1;
    meses = 0;
  }
  if (meses === 0) return `${anos} anos`;
  return `${anos} anos e ${meses} ${meses === 1 ? 'mês' : 'meses'}`;
};

/** Nome de cada regra, para os detalhes. */
export const nomeRegra: Record<string, string> = {
  R1: 'Validação inicial',
  R2: 'Idade no fim do contrato',
  R3: 'Incidentes de crédito',
  R4: 'Situação profissional',
  R5: 'Limite do montante',
  R6: 'Taxa de esforço',
  R7: 'Montantes elevados',
  ANALISTA: 'Decisão do analista',
};
