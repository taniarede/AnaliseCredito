import { useCallback, useEffect, useState } from 'react';
import { api } from './api';
import type { CampoPedido, DecisaoAnalistaRequest, PedidoRequest, PedidoResponse } from './types';
import { FormularioPedido, type ErrosFormulario } from './components/FormularioPedido';
import { identificarCenario } from './cenarios';
import { ResultadoDecisao } from './components/ResultadoDecisao';
import { ListaPedidos } from './components/ListaPedidos';
import { PainelDetalhes } from './components/PainelDetalhes';

export default function App() {
  const [resultado, setResultado] = useState<PedidoResponse | null>(null);
  const [pedidos, setPedidos] = useState<PedidoResponse[]>([]);
  const [erro, setErro] = useState<string | null>(null);
  const [aProcessar, setAProcessar] = useState(false);
  const [erros, setErros] = useState<ErrosFormulario | null>(null);
  const [detalhe, setDetalhe] = useState<PedidoResponse | null>(null);
  const fecharDetalhe = useCallback(() => setDetalhe(null), []);

  const carregarPedidos = useCallback(async () => {
    try {
      const lista = await api.listar();
      setPedidos(lista);
      // Ao abrir a página, o cartão de resultado mostra o Cenário A
      setResultado((atual) => atual ?? lista.filter((p) => identificarCenario(p.dados) === 'A').at(-1) ?? null);
    } catch (e) {
      setErro((e as Error).message);
    }
  }, []);

  useEffect(() => {
    void carregarPedidos();
  }, [carregarPedidos]);

  const enviar = async (pedido: PedidoRequest, simulacao: boolean) => {
    setAProcessar(true);
    setErro(null);
    try {
      const resposta = simulacao ? await api.simular(pedido) : await api.submeter(pedido);
      setResultado(resposta);
      if (resposta.estadoAtual === 'PedidoInvalido') {
        // Campos com erro (Regra 1): o formulário apaga-os e rodeia-os a vermelho
        const campos = resposta.motivos.map((m) => m.campo).filter((c): c is CampoPedido => c !== null);
        setErros({ id: resposta.id, campos });
      }
      await carregarPedidos();
    } catch (e) {
      setErro((e as Error).message);
    } finally {
      setAProcessar(false);
    }
  };

  const decidir = async (id: number, decisao: DecisaoAnalistaRequest) => {
    setErro(null);
    try {
      const atualizado = await api.decidir(id, decisao);
      setResultado(atualizado);
      setDetalhe(atualizado); // o painel continua aberto, já com o novo estado e o histórico
      await carregarPedidos();
    } catch (e) {
      setErro((e as Error).message);
    }
  };

  return (
    <div className="pagina">
      <header>
        <h1>Análise de Pedidos de Crédito Pessoal</h1>
      </header>

      {erro && (
        <div className="erro" role="alert">
          {erro}
          <button type="button" className="ligacao" onClick={() => setErro(null)}>
            fechar
          </button>
        </div>
      )}

      <main className="colunas">
        <FormularioPedido aProcessar={aProcessar} erros={erros} onEnviar={enviar} />
        {resultado ? (
          <ResultadoDecisao resultado={resultado} />
        ) : (
          <section className="cartao vazio">O resultado da avaliação aparece aqui.</section>
        )}
      </main>

      <ListaPedidos
        pedidos={pedidos}
        onSelecionar={setResultado}
        onVerDetalhes={setDetalhe}
        onAtualizar={carregarPedidos}
      />

      <PainelDetalhes pedido={detalhe} onFechar={fecharDetalhe} onDecidir={decidir} />
    </div>
  );
}
