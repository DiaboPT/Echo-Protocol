using UnityEngine;

// Fica no objeto-pai da porta, que tem um Box Collider com Is Trigger
// maior do que a porta (a zona onde o jogador se aproxima).
public class PortaComando : MonoBehaviour
{
    public Transform folha;                                 // a parte da porta que se mexe (filho)
    public Vector3 deslocamento = new Vector3(0f, 3f, 0f);  // quanto anda ao abrir (3 m para cima)
    public float velocidade = 2f;                           // metros por segundo
    public bool precisaCartao = true;                       // desliga para portas normais

    Vector3 posFechada;
    Vector3 posAberta;
    bool aberta = false;
    
    public bool EstaAberta => aberta;

    void Start()
    {
        // guarda onde a porta está fechada e calcula onde fica aberta
        posFechada = folha.localPosition;
        posAberta = posFechada + deslocamento;
    }

    void Update()
    {
        // a porta desliza aos poucos até à posição certa
        Vector3 alvo = aberta ? posAberta : posFechada;
        folha.localPosition = Vector3.MoveTowards(folha.localPosition, alvo, velocidade * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // só o Player abre a porta
        InventarioPlayer inventario = other.GetComponentInParent<InventarioPlayer>();
        if (inventario == null) return;

        if (!precisaCartao || inventario.temCartao)
        {
            aberta = true;
            Debug.Log("Porta aberta");
        }
        else
        {
            Debug.Log("Acesso negado: precisa do cartão da sala técnica");
        }
    }
}