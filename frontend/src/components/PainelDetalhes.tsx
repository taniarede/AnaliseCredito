import { useEffect, useRef, useState } from 'react';
import type { DecisaoAnalistaRequest, PedidoResponse } from '../types';
import { identificarCenario } from '../cenarios';
import {
  classeEstado,
  formatarData,
  formatarEuros,
  formatarIdadeFinal,
  formatarNumero,
  nomeRegra,
} from '../formatacao';

interface Props {
  pedido: PedidoResponse | null;
  onFechar: () => void;
  /** Decisão do analista (só para pedidos em ANÁLISE MANUAL). */
  onDecidir: (id: number, decisao: DecisaoAnalistaRequest) => Promise<void>;
}

const SITUACAO: Record<string, string> = {
  Efetivo: 'Efetivo',
  ContratoPrazo: 'Contrato a prazo',
  Desempregado: 'Desempregado',
};

/**
 * Janela deslizante do lado direito com os detalhes do pedido e as condições que não cumpriu.
 * Nos pedidos em ANÁLISE MANUAL inclui, no fundo, a decisão do analista.
 * Fecha com o botão ×, com a tecla Esc ou clicando fora. Em telemóvel ocupa o ecrã todo.
 */
export function PainelDetalhes({ pedido, onFechar, onDecidir }: Props) {
  const botaoFechar = useRef<HTMLButtonElement>(null);
  const aberto = pedido !== null;
  const [utilizador, setUtilizador] = useState('');
  const [observacao, setObservacao] = useState('');
  const [aDecidir, setADecidir] = useState(false);
  /** Decisão escolhida, à espera de confirmação (null = ainda não escolheu). */
  const [aConfirmar, setAConfirmar] = useState<DecisaoAnalistaRequest['novoEstado'] | null>(null);

  // Ao abrir outro pedido, a observação e a confirmação recomeçam do zero (o nome do analista mantém-se)
  useEffect(() => {
    setObservacao('');
    setAConfirmar(null);
  }, [pedido?.id]);

  const dadosCompletos = utilizador.trim() !== '' && observacao.trim() !== '';

  const podeDecidir = pedido !== null && pedido.estadoAtual === 'AnaliseManual';

  const confirmar = async () => {
    if (!pedido || !aConfirmar) return;
    setADecidir(true);
    try {
      await onDecidir(pedido.id, { novoEstado: aConfirmar, utilizador: utilizador.trim(), observacao: observacao.trim() });
    } finally {
      setADecidir(false);
      setAConfirmar(null);
    }
  };

  useEffect(() => {
    if (!aberto) return;
    const teclado = (e: KeyboardEvent) => e.key === 'Escape' && onFechar();
    document.addEventListener('keydown', teclado);
    document.body.style.overflow = 'hidden'; // a página de fundo não faz scroll
    botaoFechar.current?.focus();
    return () => {
      document.removeEventListener('keydown', teclado);
      document.body.style.overflow = '';
    };
  }, [aberto, onFechar]);

  const cenario = pedido ? identificarCenario(pedido.dados) : null;

  return (
    <div className={`gaveta-fundo ${aberto ? 'aberta' : ''}`} onClick={onFechar} aria-hidden={!aberto}>
      <aside
        className="gaveta"
        role="dialog"
        aria-modal="true"
        aria-labelledby="gaveta-titulo"
        onClick={(e) => e.stopPropagation()}
      >
        {pedido && (
          <>
            <div className="gaveta-cabecalho">
              <div>
                <small>
                  Pedido n.º {pedido.numeroPedido}
                  {cenario && ` · Cenário ${cenario}`}
                </small>
                <h2 id="gaveta-titulo" className={classeEstado[pedido.estadoAtual]}>
                  {pedido.estadoAtualDescricao}
                </h2>
              </div>
              <button ref={botaoFechar} type="button" className="botao-fechar" onClick={onFechar} aria-label="Fechar">
                ×
              </button>
            </div>

            <div className="gaveta-corpo">
              <h3>{pedido.estadoAtual === 'PedidoInvalido' ? 'Erros de preenchimento' : 'Condições não cumpridas'}</h3>
              {pedido.motivos.length === 0 && <p className="sem-motivos">Todas as regras foram cumpridas.</p>}
              <ul className="condicoes">
                {pedido.motivos.map((m, i) => (
                  <li key={i} className={classeEstado[m.decisao]}>
                    <div className="condicao-topo">
                      <span className={`etiqueta ${classeEstado[m.decisao]}`}>
                        {m.codigoRegra === 'ANALISTA' ? 'Analista' : m.codigoRegra}
                      </span>
                      <strong>{nomeRegra[m.codigoRegra] ?? m.codigoRegra}</strong>
                      <span className="condicao-decisao">{m.decisaoDescricao}</span>
                    </div>
                    <p>{m.descricao}</p>
                  </li>
                ))}
              </ul>

              {pedido.indicadores && (
                <>
                  <h3>Indicadores</h3>
                  <dl className="lista-dados">
                    <dt>Taxa de esforço</dt>
                    <dd>{formatarNumero(pedido.indicadores.taxaEsforco)}%</dd>
                    <dt>Prestação estimada</dt>
                    <dd>{formatarEuros(pedido.indicadores.prestacaoEstimada)}</dd>
                    <dt>Idade no fim do contrato</dt>
                    <dd>{formatarIdadeFinal(pedido.indicadores.idadeFinalContrato)}</dd>
                    <dt>Montante máximo (20×)</dt>
                    <dd>{formatarEuros(pedido.indicadores.montanteMaximoRecomendado)}</dd>
                  </dl>
                </>
              )}

              <h3>Dados do pedido</h3>
              <dl className="lista-dados">
                <dt>NIF</dt>
                <dd>{pedido.dados.nif || '—'}</dd>
                <dt>Idade</dt>
                <dd>{pedido.dados.idade ?? '—'}</dd>
                <dt>Rendimento mensal</dt>
                <dd>{pedido.dados.rendimentoMensalLiquido !== null ? formatarEuros(pedido.dados.rendimentoMensalLiquido) : '—'}</dd>
                <dt>Prestações atuais</dt>
                <dd>{pedido.dados.prestacoesAtuais !== null ? formatarEuros(pedido.dados.prestacoesAtuais) : '—'}</dd>
                <dt>Valor pretendido</dt>
                <dd>{pedido.dados.valorPretendido !== null ? formatarEuros(pedido.dados.valorPretendido) : '—'}</dd>
                <dt>Prazo</dt>
                <dd>{pedido.dados.prazoMeses !== null ? `${pedido.dados.prazoMeses} meses` : '—'}</dd>
                <dt>Situação profissional</dt>
                <dd>{SITUACAO[pedido.dados.situacaoProfissional] ?? pedido.dados.situacaoProfissional}</dd>
                <dt>Incidentes de crédito</dt>
                <dd>{pedido.dados.incidentesCredito ? 'Sim' : 'Não'}</dd>
              </dl>

              <h3>Histórico</h3>
              <ol className="historico">
                {pedido.historico.map((h, i) => (
                  <li key={i}>
                    <span className={`etiqueta ${classeEstado[h.estadoNovo]}`}>{h.estadoNovoDescricao}</span>
                    <small>
                      {formatarData(h.dataAlteracao)} · {h.utilizador}
                    </small>
                    {h.observacao && <p>{h.observacao}</p>}
                  </li>
                ))}
              </ol>
            </div>

            {podeDecidir && (
              <div className="gaveta-decisao">
                <h3>Decisão do analista</h3>

                {aConfirmar === null ? (
                  <>
                    <label className="campo">
                      <span>Nome do analista (obrigatório)</span>
                      <input value={utilizador} onChange={(e) => setUtilizador(e.target.value)} autoComplete="name" />
                    </label>
                    <label className="campo">
                      <span>Observação (obrigatória)</span>
                      <textarea rows={2} value={observacao} onChange={(e) => setObservacao(e.target.value)} />
                    </label>
                    {!dadosCompletos && <small className="ajuda">Preencha o nome e a observação para poder decidir.</small>}
                    <div className="acoes">
                      <button type="button" className="botao-recusar" disabled={!dadosCompletos}
                        onClick={() => setAConfirmar('Recusado')}>
                        Recusar
                      </button>
                      <button type="button" className="botao-principal" disabled={!dadosCompletos}
                        onClick={() => setAConfirmar('Aprovado')}>
                        Aprovar
                      </button>
                    </div>
                  </>
                ) : (
                  <div className={`confirmacao ${aConfirmar === 'Aprovado' ? 'confirmar-aprovar' : 'confirmar-recusar'}`} role="alertdialog" aria-live="assertive">
                    <p className="confirmacao-pergunta">
                      Confirma a {aConfirmar === 'Aprovado' ? 'aprovação' : 'recusa'} do pedido {pedido.numeroPedido}?
                    </p>
                    <p className="confirmacao-nota">Esta decisão fica registada no histórico e não pode ser alterada.</p>
                    <dl className="lista-dados">
                      <dt>Analista</dt>
                      <dd>{utilizador.trim()}</dd>
                      <dt>Observação</dt>
                      <dd>{observacao.trim()}</dd>
                    </dl>
                    <div className="acoes">
                      <button type="button" className="botao-secundario" disabled={aDecidir} onClick={() => setAConfirmar(null)}>
                        Voltar
                      </button>
                      <button
                        type="button"
                        className={aConfirmar === 'Aprovado' ? 'botao-aprovar' : 'botao-recusar'}
                        disabled={aDecidir}
                        onClick={confirmar}
                      >
                        {aDecidir ? 'A registar…' : aConfirmar === 'Aprovado' ? 'Confirmar aprovação' : 'Confirmar recusa'}
                      </button>
                    </div>
                  </div>
                )}
              </div>
            )}
          </>
        )}
      </aside>
    </div>
  );
}
