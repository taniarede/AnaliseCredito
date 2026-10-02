import { useEffect, useState, type FormEvent } from 'react';
import type { CampoPedido, PedidoRequest } from '../types';

/** Valores do formulário guardados como texto, para o utilizador poder apagar/escrever livremente. */
type CamposTexto = Record<Exclude<CampoPedido, 'incidentesCredito'>, string> & { incidentesCredito: boolean };

const CAMPOS_VAZIOS: CamposTexto = {
  nif: '',
  idade: '',
  rendimentoMensalLiquido: '',
  prestacoesAtuais: '',
  valorPretendido: '',
  prazoMeses: '',
  situacaoProfissional: 'Efetivo',
  incidentesCredito: false,
};

// O que se pode escrever em cada tipo de campo (impede letras nos campos numéricos).
const PADRAO_INTEIRO = /^-?\d*$/;
const PADRAO_DECIMAL = /^-?\d*([.,]\d{0,2})?$/;

/** Texto -> número. Campo vazio -> null (o backend assinala-o como obrigatório). */
const paraNumero = (texto: string): number | null => {
  const limpo = texto.trim().replace(',', '.');
  if (limpo === '' || limpo === '-') return null;
  return Number(limpo);
};

/** Pedido de limpeza dos campos inválidos, enviado pela App depois de um PEDIDO INVÁLIDO. */
export interface ErrosFormulario {
  /** Muda a cada resposta, para voltar a limpar mesmo que os campos sejam os mesmos. */
  id: number;
  campos: CampoPedido[];
}

interface Props {
  aProcessar: boolean;
  erros: ErrosFormulario | null;
  onEnviar: (pedido: PedidoRequest, simulacao: boolean) => void;
}

export function FormularioPedido({ aProcessar, erros, onEnviar }: Props) {
  const [campos, setCampos] = useState<CamposTexto>(CAMPOS_VAZIOS);
  const [invalidos, setInvalidos] = useState<CampoPedido[]>([]);

  // Pedido inválido: apaga só os campos com erro e rodeia-os a vermelho. Os restantes mantêm-se.
  useEffect(() => {
    if (!erros) return;
    setInvalidos(erros.campos);
    setCampos((atual) => {
      const novo = { ...atual };
      for (const campo of erros.campos) {
        if (campo !== 'incidentesCredito') novo[campo] = CAMPOS_VAZIOS[campo];
      }
      return novo;
    });
  }, [erros]);

  const alterar = (campo: CampoPedido, valor: string | boolean) => {
    setCampos((atual) => ({ ...atual, [campo]: valor }));
    // Assim que o utilizador corrige o campo, deixa de estar a vermelho
    setInvalidos((atual) => atual.filter((c) => c !== campo));
  };

  const alterarNumero = (campo: CampoPedido, valor: string, padrao: RegExp) => {
    if (padrao.test(valor)) alterar(campo, valor);
  };

  const construirPedido = (): PedidoRequest => ({
    nif: campos.nif.trim(),
    idade: paraNumero(campos.idade),
    rendimentoMensalLiquido: paraNumero(campos.rendimentoMensalLiquido),
    prestacoesAtuais: paraNumero(campos.prestacoesAtuais),
    valorPretendido: paraNumero(campos.valorPretendido),
    prazoMeses: paraNumero(campos.prazoMeses),
    situacaoProfissional: campos.situacaoProfissional,
    incidentesCredito: campos.incidentesCredito,
  });

  // O botão que submete o form decide se é simulação ou pedido
  const submeter = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const botao = (e.nativeEvent as SubmitEvent).submitter as HTMLButtonElement | null;
    onEnviar(construirPedido(), botao?.value !== 'submeter');
  };

  const invalido = (campo: CampoPedido) => invalidos.includes(campo);

  return (
    <form className="cartao" onSubmit={submeter} noValidate>
      <h2>Novo pedido</h2>

      <div className="grelha">
        <Campo rotulo="NIF" dica="9 dígitos" valor={campos.nif} invalido={invalido('nif')}
          onChange={(v) => alterar('nif', v)} />
        <Campo rotulo="Idade" valor={campos.idade} invalido={invalido('idade')} modo="numeric"
          onChange={(v) => alterarNumero('idade', v, PADRAO_INTEIRO)} />
        <Campo rotulo="Rendimento mensal líquido" dica="agregado" valor={campos.rendimentoMensalLiquido}
          invalido={invalido('rendimentoMensalLiquido')}
          onChange={(v) => alterarNumero('rendimentoMensalLiquido', v, PADRAO_DECIMAL)} />
        <Campo rotulo="Prestações atuais" dica="mês" valor={campos.prestacoesAtuais}
          invalido={invalido('prestacoesAtuais')}
          onChange={(v) => alterarNumero('prestacoesAtuais', v, PADRAO_DECIMAL)} />
        <Campo rotulo="Valor pretendido" dica="€" valor={campos.valorPretendido}
          invalido={invalido('valorPretendido')}
          onChange={(v) => alterarNumero('valorPretendido', v, PADRAO_DECIMAL)} />
        <Campo rotulo="Prazo" dica="meses" valor={campos.prazoMeses} invalido={invalido('prazoMeses')} modo="numeric"
          onChange={(v) => alterarNumero('prazoMeses', v, PADRAO_INTEIRO)} />

        <label className="campo">
          <span>Situação profissional</span>
          <select
            className={invalido('situacaoProfissional') ? 'invalido' : undefined}
            value={campos.situacaoProfissional}
            onChange={(e) => alterar('situacaoProfissional', e.target.value)}
          >
            <option value="Efetivo">Efetivo</option>
            <option value="ContratoPrazo">Contrato a prazo</option>
            <option value="Desempregado">Desempregado</option>
          </select>
        </label>

        <label className="campo campo-checkbox">
          <input
            type="checkbox"
            checked={campos.incidentesCredito}
            onChange={(e) => alterar('incidentesCredito', e.target.checked)}
          />
          <span>Tem incidentes de crédito registados</span>
        </label>
      </div>

      <div className="acoes">
        <button type="submit" value="simular" className="botao-secundario" disabled={aProcessar}>
          Simular
        </button>
        <button type="submit" value="submeter" className="botao-principal" disabled={aProcessar}>
          {aProcessar ? 'A avaliar…' : 'Submeter pedido'}
        </button>
      </div>
    </form>
  );
}

interface CampoProps {
  rotulo: string;
  valor: string;
  dica?: string;
  invalido: boolean;
  modo?: 'decimal' | 'numeric';
  onChange: (valor: string) => void;
}

function Campo({ rotulo, valor, dica, invalido, modo = 'decimal', onChange }: CampoProps) {
  return (
    <label className="campo">
      <span>
        {rotulo} {dica && <small>({dica})</small>}
      </span>
      <input
        type="text"
        inputMode={modo}
        className={invalido ? 'invalido' : undefined}
        aria-invalid={invalido}
        value={valor}
        onChange={(e) => onChange(e.target.value)}
      />
    </label>
  );
}
