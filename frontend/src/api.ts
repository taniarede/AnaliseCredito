import type { DecisaoAnalistaRequest, PedidoRequest, PedidoResponse, SimulacaoResponse } from './types';

// Chamadas à API. O Vite reencaminha /api para http://localhost:5080 (ver vite.config.ts).

async function pedidoHttp<T>(url: string, init?: RequestInit): Promise<T> {
  let resposta: Response;
  try {
    resposta = await fetch(url, {
      headers: { 'Content-Type': 'application/json' },
      ...init,
    });
  } catch {
    throw new Error('Não foi possível contactar a API. Confirme que está a correr em http://localhost:5080.');
  }

  if (!resposta.ok) {
    throw new Error(await lerErro(resposta));
  }
  return (await resposta.json()) as T;
}

/** Converte a resposta de erro (ProblemDetails do ASP.NET) num texto legível. */
async function lerErro(resposta: Response): Promise<string> {
  try {
    const corpo = await resposta.json();
    if (corpo?.errors) {
      return Object.values(corpo.errors as Record<string, string[]>).flat().join(' ');
    }
    return corpo?.detail ?? corpo?.title ?? `Erro ${resposta.status}`;
  } catch {
    if (resposta.status >= 500) {
      return `Erro ${resposta.status} na API. Veja o terminal da API (a base de dados está a correr?).`;
    }
    return `Erro ${resposta.status}`;
  }
}

export const api = {
  simular: (pedido: PedidoRequest) =>
    pedidoHttp<SimulacaoResponse>('/api/simulacoes', { method: 'POST', body: JSON.stringify(pedido) }),

  submeter: (pedido: PedidoRequest) =>
    pedidoHttp<PedidoResponse>('/api/pedidos', { method: 'POST', body: JSON.stringify(pedido) }),

  listar: (top = 50) => pedidoHttp<PedidoResponse[]>(`/api/pedidos?top=${top}`),

  decidir: (id: number, decisao: DecisaoAnalistaRequest) =>
    pedidoHttp<PedidoResponse>(`/api/analise-manual/${id}/decisao`, {
      method: 'POST',
      body: JSON.stringify(decisao),
    }),
};
