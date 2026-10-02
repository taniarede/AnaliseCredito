import type { PedidoResponse } from '../types';
import { identificarCenario } from '../cenarios';
import { classeEstado, descricaoEstado, formatarData, formatarEuros, formatarNumero } from '../formatacao';

interface Props {
  pedidos: PedidoResponse[];
  onSelecionar: (pedido: PedidoResponse) => void;
  onVerDetalhes: (pedido: PedidoResponse) => void;
  onAtualizar: () => void;
}

/**
 * Pedidos registados.
 * - ANÁLISE MANUAL: botão Decidir -> abre o painel de detalhes com a decisão do analista.
 * - Restantes: as regras que dispararam (ou "Detalhes") -> abre o painel de detalhes.
 * Em ecrãs pequenos a tabela passa a cartões (ver styles.css).
 */
export function ListaPedidos({ pedidos, onSelecionar, onVerDetalhes, onAtualizar }: Props) {
  return (
    <section className="cartao">
      <div className="lista-cabecalho">
        <h2>Pedidos registados</h2>
        <button type="button" className="botao-secundario" onClick={onAtualizar}>
          Atualizar
        </button>
      </div>

      {pedidos.length === 0 ? (
        <p className="sem-motivos">Ainda não há pedidos.</p>
      ) : (
        <table className="tabela-pedidos">
          <thead>
            <tr>
              <th>N.º</th>
              <th>Data</th>
              <th>NIF</th>
              <th>Valor</th>
              <th>Taxa</th>
              <th>Automática</th>
              <th>Estado atual</th>
              <th>Ação / Regras</th>
            </tr>
          </thead>
          <tbody>
            {pedidos.map((p) => {
              const cenario = identificarCenario(p.dados);
              return (
                <tr key={p.id}>
                  <td data-rotulo="N.º" className="celula-numero">
                    <button type="button" className="ligacao" onClick={() => onSelecionar(p)}>
                      {p.numeroPedido}
                    </button>
                    {p.ehSimulacao && <small className="simulacao">simulação</small>}
                    {cenario && <span className="etiqueta etiqueta-cenario">Cenário {cenario}</span>}
                  </td>
                  <td data-rotulo="Data">{formatarData(p.dataPedido)}</td>
                  <td data-rotulo="NIF">{p.dados.nif || '—'}</td>
                  <td data-rotulo="Valor">
                    {p.dados.valorPretendido !== null ? formatarEuros(p.dados.valorPretendido) : '—'}
                  </td>
                  <td data-rotulo="Taxa">{p.indicadores ? `${formatarNumero(p.indicadores.taxaEsforco)}%` : '—'}</td>
                  <td data-rotulo="Automática">
                    <span className={`etiqueta ${classeEstado[p.decisaoAutomatica]}`}>
                      {descricaoEstado[p.decisaoAutomatica]}
                    </span>
                  </td>
                  <td data-rotulo="Estado atual">
                    <span className={`etiqueta ${classeEstado[p.estadoAtual]}`}>{p.estadoAtualDescricao}</span>
                  </td>
                  <td data-rotulo="Ação / Regras" className="celula-acao">
                    {p.estadoAtual === 'AnaliseManual' && !p.ehSimulacao ? (
                      <button type="button" className="botao-secundario botao-decidir" onClick={() => onVerDetalhes(p)}>
                        Decidir
                      </button>
                    ) : (
                      <button
                        type="button"
                        className="botao-regras"
                        onClick={() => onVerDetalhes(p)}
                        aria-label={`Ver os detalhes do pedido ${p.numeroPedido}`}
                      >
                        <span className="rotulo-regras">
                          {p.motivos.length > 0 ? 'Ver regras' : 'Ver detalhes'}
                        </span>
                        {p.motivos.length === 0 && <span className="texto-detalhes">Detalhes</span>}
                        {codigosUnicos(p).map(({ codigo, classe }) => (
                          <span key={codigo} className={`etiqueta ${classe}`}>
                            {codigo === 'ANALISTA' ? 'Analista' : codigo}
                          </span>
                        ))}
                        <span className="seta" aria-hidden="true">›</span>
                      </button>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}

    </section>
  );
}

/** Uma etiqueta por regra (um pedido inválido pode ter vários erros da R1), com a cor da decisão mais grave. */
function codigosUnicos(p: PedidoResponse) {
  const ordem = ['Aprovado', 'AnaliseManual', 'Recusado', 'PedidoInvalido'];
  const mapa = new Map<string, string>();
  for (const m of p.motivos) {
    const atual = mapa.get(m.codigoRegra);
    if (!atual || ordem.indexOf(m.decisao) > ordem.indexOf(atual)) mapa.set(m.codigoRegra, m.decisao);
  }
  return [...mapa].map(([codigo, decisao]) => ({
    codigo,
    classe: classeEstado[decisao as PedidoResponse['estadoAtual']],
  }));
}
