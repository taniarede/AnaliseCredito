// Tipos iguais aos DTOs da API (.NET). Os enums chegam como texto.

export type EstadoPedido = 'Aprovado' | 'AnaliseManual' | 'Recusado' | 'PedidoInvalido';
export type SituacaoProfissional = 'Efetivo' | 'ContratoPrazo' | 'Desempregado';

/** Campos numéricos vazios são enviados como null (a Regra 1 do backend assinala-os). */
export interface PedidoRequest {
  nif: string;
  idade: number | null;
  rendimentoMensalLiquido: number | null;
  prestacoesAtuais: number | null;
  valorPretendido: number | null;
  prazoMeses: number | null;
  situacaoProfissional: SituacaoProfissional | string;
  incidentesCredito: boolean;
}

export interface Motivo {
  codigoRegra: string;
  decisao: EstadoPedido;
  decisaoDescricao: string;
  descricao: string;
  /** Só nos erros da Regra 1: o campo do formulário com problema (ex.: "nif", "idade"). */
  campo: CampoPedido | null;
}

/** Nomes dos campos do pedido (iguais aos da API). */
export type CampoPedido = keyof PedidoRequest;

export interface Indicadores {
  prestacaoEstimada: number;
  taxaEsforco: number;
  idadeFinalContrato: number;
  montanteMaximoRecomendado: number;
}

export interface Historico {
  estadoAnterior: EstadoPedido | null;
  estadoNovo: EstadoPedido;
  estadoNovoDescricao: string;
  dataAlteracao: string;
  utilizador: string;
  observacao: string | null;
}

export interface PedidoResponse {
  id: number;
  numeroPedido: string;
  dataPedido: string;
  decisaoAutomatica: EstadoPedido;
  estadoAtual: EstadoPedido;
  estadoAtualDescricao: string;
  motivos: Motivo[];
  indicadores: Indicadores | null;
  dados: PedidoRequest;
  historico: Historico[];
}

/** Resposta de POST /api/simulacoes: sem número de pedido, estado atual nem histórico. */
export interface SimulacaoResponse {
  id: number;
  dataSimulacao: string;
  decisao: EstadoPedido;
  decisaoDescricao: string;
  motivos: Motivo[];
  indicadores: Indicadores | null;
  dados: PedidoRequest;
}

/** O que o cartão de resultado pode mostrar: um pedido ou uma simulação. */
export type Resultado = PedidoResponse | SimulacaoResponse;

/** Uma simulação não tem número de pedido. */
export const ehSimulacao = (r: Resultado): r is SimulacaoResponse => !('numeroPedido' in r);

export interface DecisaoAnalistaRequest {
  novoEstado: 'Aprovado' | 'Recusado';
  utilizador: string;
  observacao: string;
}
