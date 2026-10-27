using UnityEngine;
using UnityEngine.InputSystem;

// Debug visual: mostra os Line Renderers das rotas de patrulha,
// as linhas dos tiros (jogador, soldados e drones) e as barras de vida por cima dos soldados (BarraVidaInimigo).
// Pôr num GameObject vazio chamado "ModoDebugVisual" (só um na cena).
// Só mostra coisas: não muda nada no jogo (quem muda valores é o ModoTeste, F1).
public class ModoDebugVisual : MonoBehaviour
{
    [Header("Interruptor principal")]
    [Tooltip("Ligado = rotas, linhas dos tiros e barras de vida visíveis.")]
    public bool modoDebugVisual = false;

    [Tooltip("Tecla para mostrar/esconder durante o Play.")]
    public Key teclaTrocar = Key.F3;

    [Tooltip("Deixar o F3 funcionar também num Build (por defeito só no Editor).")]
    public bool funcionarNoBuild = false;

    [Header("Rotas (arrastar os objetos com Line Renderer, ex.: Drone_Rota)")]
    [Tooltip("Só os Line Renderers destes objetos são ligados/desligados; o resto do objeto não muda.")]
    public Renderer[] rotas;

    [Header("Linhas dos tiros")]
    [Tooltip("Mostra a linha de cada tiro do jogador (Weapon), dos soldados (Enemy) e dos drones (EnemyDrone).")]
    public bool mostrarTiros = true;

    [Header("Barras de vida")]
    [Tooltip("Liga as barras de vida por cima dos soldados (script BarraVidaInimigo).")]
    public bool mostrarBarrasVida = true;

    // ---------- Valores que os outros scripts leem ----------
    public static bool Ativo { get; private set; }
    public static bool BarrasVida => Ativo && instancia != null && instancia.mostrarBarrasVida;
    public static bool Tiros => Ativo && instancia != null && instancia.mostrarTiros;

    static ModoDebugVisual instancia;

    bool modoAplicado;

    void Awake()
    {
        instancia = this;
#if !UNITY_EDITOR
        if (!funcionarNoBuild) modoDebugVisual = false;   // num Build arranca sempre desligado
#endif
        Ativo = modoDebugVisual;
    }

    // No Start e não no Awake, para esconder as rotas depois de os outros scripts as prepararem
    void Start()
    {
        Aplicar();
    }

    void OnDestroy()
    {
        if (instancia == this) { instancia = null; Ativo = false; }
    }

    void Update()
    {
        bool teclaPermitida = Application.isEditor || funcionarNoBuild;
        if (teclaPermitida && Keyboard.current != null && Keyboard.current[teclaTrocar].wasPressedThisFrame)
            modoDebugVisual = !modoDebugVisual;

        // Também apanha a caixa do Inspector clicada durante o Play
        if (modoDebugVisual != modoAplicado) Aplicar();
    }

    void Aplicar()
    {
        modoAplicado = modoDebugVisual;
        Ativo = modoDebugVisual;

        // Ao arrastar um objeto para a lista, o Unity pode escolher outro Renderer do objeto
        // (ex.: o Mesh Renderer) em vez do Line Renderer, por isso procura os Line Renderers do objeto
        foreach (Renderer r in rotas)
        {
            if (r == null) continue;
            foreach (LineRenderer linha in r.GetComponents<LineRenderer>())
                linha.enabled = modoDebugVisual;
        }

        Debug.Log(modoDebugVisual ? "DEBUG VISUAL ligado" : "Debug visual desligado");
    }
}
