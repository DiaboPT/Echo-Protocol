using UnityEngine;

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Fica no objeto-pai do terminal (Terminal_Zona), que tem um Box Collider com Is Trigger:
// a zona onde o jogador tem de estar para ativar. O cubo do terminal é filho.
public class TerminalAutodestruicao : MonoBehaviour
{
    [Header("Ativação")]
    public Key tecla = Key.E;                 // tecla para ativar
    public PortaComando[] portas;             // portas da sala de comando (basta uma aberta)

    [Header("Explosão")]
    public float tempoContagem = 60f;         // segundos até explodir
    public float raioExplosao = 40f;          // metros destruídos à volta do terminal
    public LayerMask camadasDestruir = ~0;    // o que pode ser destruído (tira o Ground)
    //public GameObject efeitoExplosao;         // opcional: prefab de partículas
    //public AudioClip somExplosao;             // opcional: som da explosão

    [Header("HUD (opcional)")]
    public TextMeshProUGUI textoTimer;        // texto no Canvas para o aviso e a contagem

    InventarioPlayer inventario;              // o do jogador, só enquanto está dentro da zona
    bool ativado = false;
    float tempoRestante;

    void Start()
    {
        if (textoTimer != null) textoTimer.text = "";
    }

    void Update()
    {
        if (!ativado)
            VerificarAtivacao();
        else
            Contagem();
    }

    void OnTriggerEnter(Collider other)
    {
        // só o Player tem InventarioPlayer; inimigos e balas são ignorados
        InventarioPlayer inv = other.GetComponentInParent<InventarioPlayer>();
        if (inv == null) return;

        inventario = inv;
        if (!ativado && textoTimer != null)
            textoTimer.text = "[" + tecla + "] Ativar autodestruição";
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<InventarioPlayer>() == null) return;

        inventario = null;
        if (!ativado && textoTimer != null)
            textoTimer.text = "";
    }

    void VerificarAtivacao()
    {
        // só funciona com o jogador dentro da zona
        if (inventario == null || Keyboard.current == null) return;
        if (!Keyboard.current[tecla].wasPressedThisFrame) return;

        if (!inventario.temCartao)
        {
            Debug.Log("Acesso negado: precisa do cartão de acesso");
            return;
        }
        if (!PortaAberta())
        {
            Debug.Log("Acesso negado: a porta da sala de comando está fechada");
            return;
        }

        ativado = true;
        tempoRestante = tempoContagem;
        Debug.Log("Autodestruição ativada: " + tempoContagem + " s para fugir");
    }

    // pelo menos uma das portas ligadas já abriu
    bool PortaAberta()
    {
        if (portas == null || portas.Length == 0) return true;   // sem portas ligadas, não verifica
        foreach (PortaComando p in portas)
            if (p != null && p.EstaAberta) return true;
        return false;
    }

    void Contagem()
    {
        tempoRestante -= Time.deltaTime;

        if (textoTimer != null)
            textoTimer.text = "Autodestruição: " + Mathf.CeilToInt(Mathf.Max(tempoRestante, 0f)) + " s";

        if (tempoRestante <= 0f)
            Explodir();
    }

    void Explodir()
    {
        enabled = false;   // desliga o Update: a explosão só acontece uma vez
        if (textoTimer != null) textoTimer.text = "";

        //if (efeitoExplosao != null)
        //    Instantiate(efeitoExplosao, transform.position, Quaternion.identity);
        //if (somExplosao != null)
         //   AudioSource.PlayClipAtPoint(somExplosao, transform.position);

         // o jogador está dentro do raio? (por distância, não depende das Camadas Destruir)
         PlayerStats statsJogador = FindFirstObjectByType<PlayerStats>();
         bool jogadorApanhado = statsJogador != null &&
                                Vector3.Distance(statsJogador.transform.position, transform.position) <= raioExplosao;

         Collider[] atingidos = Physics.OverlapSphere(transform.position, raioExplosao, camadasDestruir);
         foreach (Collider c in atingidos)
         {
             if (c.GetComponentInParent<InventarioPlayer>() != null) continue;  // o Player não é destruído
             Destroy(c.gameObject);
         }

         if (jogadorApanhado)
         {
             Debug.Log("Sinal perdido: o jogador estava dentro do raio da explosão");
             statsJogador.playerHitDamage(9999f);   // tira-lhe a vida toda
         }
         else
         {
             Debug.Log("Base destruída e o jogador escapou");
         }

        Destroy(gameObject);   // a zona e o terminal (filho)
    }

    // desenha na Scene o raio da explosão (vermelho)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, raioExplosao);
    }
}