using UnityEngine;

// Muda o ícone do objetivo conforme o jogo avança: cartão, depois terminal, depois abrigo.
// Pôr no mesmo Quad que tem o IconeMinimapa (Tipo = Objetivo).
[RequireComponent(typeof(IconeMinimapa))]
public class ObjetivoMinimapa : MonoBehaviour
{
    [Header("Ligações")]
    public InventarioPlayer inventario;        // o do Player
    public TerminalAutodestruicao terminal;

    [Header("Sítios do objetivo")]
    public Transform cartao;                   // cartão de acesso na sala técnica
    public Transform abrigo;                   // Cobertura Experimental

    IconeMinimapa icone;

    void Awake()
    {
        icone = GetComponent<IconeMinimapa>();
    }

    // Update corre antes do LateUpdate do ícone, por isso o ícone já usa o alvo novo neste frame
    void Update()
    {
        if (inventario == null) return;   // o Player foi destruído: o objetivo fica como estava

        if (!inventario.temCartao)
            icone.alvo = cartao;
        else if (terminal != null && !terminal.Ativado)
            icone.alvo = terminal.transform;
        else
            icone.alvo = abrigo;
    }
}
