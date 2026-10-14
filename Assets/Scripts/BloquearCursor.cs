using UnityEngine;
using UnityEngine.InputSystem;

// Prende e esconde o rato quando se clica dentro do ecrã do jogo.
// Escape solta o rato. No fim do jogo (Ganhaste / Perdeste) o rato fica solto.
public class BloquearCursor : MonoBehaviour
{
    // fica true quando aparece o ecrã de fim; a partir daí o clique já não prende o rato
    public static bool jogoTerminado = false;

    void Start()
    {
        jogoTerminado = false;   // volta a false quando a cena recomeça
    }

    void Update()
    {
        if (jogoTerminado) return;

        // clique esquerdo dentro do ecrã do jogo: prende e esconde o rato
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Prender();

        // Escape: solta o rato
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Soltar();
    }

    void Prender()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    static void Soltar()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // chamar quando aparece "Ganhaste" ou "Perdeste"
    public static void FimDeJogo()
    {
        jogoTerminado = true;
        Soltar();
    }
}