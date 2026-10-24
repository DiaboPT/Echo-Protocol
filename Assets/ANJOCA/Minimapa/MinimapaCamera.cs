using UnityEngine;

// Câmara vista de cima que segue o jogador e roda com ele (a seta do jogador aponta sempre para cima).
// Pôr numa Camera com Projection = Orthographic e Target Texture = RT_Minimapa. Só uma na cena.
[RequireComponent(typeof(Camera))]
public class MinimapaCamera : MonoBehaviour
{
    [Header("Jogador")]
    [Tooltip("Vazio: procura o GameObject chamado \"Player\".")]
    public Transform jogador;

    [Header("Posição e zoom")]
    public float altura = 40f;          // metros acima do jogador
    public float zoom = 25f;            // metros do centro à borda do minimapa (Orthographic Size)
    public bool rodarComJogador = true; // desligado: norte sempre para cima

    [Header("Indicador do norte (opcional)")]
    [Tooltip("RectTransform com o pivô no centro do minimapa e o texto \"N\" em cima.")]
    public RectTransform norte;

    // Os ícones leem estes valores para saber onde fica a borda do minimapa
    public static MinimapaCamera Instancia { get; private set; }
    public Camera Camara { get; private set; }

    void Awake()
    {
        Instancia = this;
        Camara = GetComponent<Camera>();
        Camara.orthographic = true;
    }

    void Start()
    {
        if (jogador == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null) jogador = p.transform;
        }
    }

    // LateUpdate: corre depois de o jogador se mexer neste frame, por isso o minimapa não treme
    void LateUpdate()
    {
        // o Player é destruído 1 s depois de morrer: o minimapa fica parado onde estava
        if (jogador == null) return;

        Camara.orthographicSize = zoom;
        transform.position = jogador.position + Vector3.up * altura;

        // X = 90 olha para baixo; Y = para onde o jogador está virado (o corpo roda para os lados)
        float rumo = rodarComJogador ? jogador.eulerAngles.y : 0f;
        transform.rotation = Quaternion.Euler(90f, rumo, 0f);

        // o "N" roda à volta do minimapa para continuar a apontar para o norte do mapa
        if (norte != null) norte.localEulerAngles = new Vector3(0f, 0f, rumo);
    }
}
