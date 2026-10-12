using UnityEngine;

// Fica no objeto Cartao_Acesso, que tem um Box Collider com Is Trigger ligado.
public class CartaoAcesso : MonoBehaviour
{
    public float velocidadeRotacao = 90f;  // graus por segundo, só para chamar a atenção
    public AudioClip somApanhar;           // opcional: som ao apanhar

    void Update()
    {
        // o cartão roda sobre si próprio para o jogador o ver
        transform.Rotate(0f, velocidadeRotacao * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        // só o Player tem InventarioPlayer; inimigos e balas são ignorados
        InventarioPlayer inventario = other.GetComponentInParent<InventarioPlayer>();
        if (inventario == null) return;

        inventario.temCartao = true;
        Debug.Log("Cartão de acesso apanhado");

        if (somApanhar != null)
            AudioSource.PlayClipAtPoint(somApanhar, transform.position);

        // o cartão desaparece da mesa
        Destroy(gameObject);
    }
}