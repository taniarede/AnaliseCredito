import { ehSimulacao, type Resultado } from '../types';
import { identificarCenario } from '../cenarios';
import { classeEstado, formatarEuros, formatarIdadeFinal, formatarNumero } from '../formatacao';

/**
 * Apresenta o resultado no formato pedido pelo enunciado: decisão final, motivos e indicadores.
 * Num pedido mostra também o número (e o cenário) e o selo "Decisão final".
 * Numa simulação mostra a etiqueta "Simulação" no lugar do número e não mostra o selo.
 */
export function ResultadoDecisao({ resultado }: { resultado: Resultado }) {
  const { indicadores } = resultado;
  const simulacao = ehSimulacao(resultado);
  const estado = simulacao ? resultado.decisao : resultado.estadoAtual;
  const descricao = simulacao ? resultado.decisaoDescricao : resultado.estadoAtualDescricao;
  const cenario = simulacao ? null : identificarCenario(resultado.dados);

  return (
    <section className={`cartao resultado ${classeEstado[estado]}`}>
      <div className="resultado-cabecalho">
        <div>
          {simulacao ? (
            <small>Simulação</small>
          ) : (
            <small>
              Pedido n.º {resultado.numeroPedido}
              {cenario && ` · Cenário ${cenario}`}
            </small>
          )}
          <h2>{descricao}</h2>
        </div>
        {!simulacao && <span className="selo">Decisão final</span>}
      </div>

      <h3>Motivos da decisão</h3>
      {resultado.motivos.length === 0 ? (
        <p className="sem-motivos">Todas as regras foram cumpridas.</p>
      ) : (
        <ul className="motivos">
          {resultado.motivos.map((m, i) => (
            <li key={i}>
              <span className={`etiqueta ${classeEstado[m.decisao]}`}>{m.codigoRegra}</span>
              {m.descricao}
            </li>
          ))}
        </ul>
      )}

      <h3>Indicadores calculados</h3>
      {indicadores ? (
        <dl className="indicadores">
          <div>
            <dt>Taxa de esforço</dt>
            <dd>{formatarNumero(indicadores.taxaEsforco)}%</dd>
          </div>
          <div>
            <dt>Prestação estimada</dt>
            <dd>{formatarEuros(indicadores.prestacaoEstimada)}</dd>
          </div>
          <div>
            <dt>Idade no fim do contrato</dt>
            <dd>{formatarIdadeFinal(indicadores.idadeFinalContrato)}</dd>
          </div>
          <div>
            <dt>Montante máximo (20× rendimento)</dt>
            <dd>{formatarEuros(indicadores.montanteMaximoRecomendado)}</dd>
          </div>
        </dl>
      ) : (
        <p className="sem-motivos">
          Não calculados: {simulacao ? 'a simulação' : 'o pedido'} não passou a validação inicial.
        </p>
      )}
    </section>
  );
}
