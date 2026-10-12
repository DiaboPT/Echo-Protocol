using System.Collections;
using UnityEngine;

public class PlayerQueda : MonoBehaviour
{
    // a câmara do player (a arma é filha dela, por isso cai junto)
    public Transform cameraPlayer;
    // scripts a desligar ao morrer: movimento, olhar com o rato, Weapon
    public MonoBehaviour[] scriptsADesligar;
    // quanto tempo demora a cair (segundos)
    public float duracaoQueda = 0.6f;
    // quanto a câmara desce (metros)
    public float descida = 1.4f;

    private bool morto = false;

    public void Morrer()
    {
        if (morto) return; // já caiu: não cai duas vezes
        morto = true;

        // desliga os controlos
        foreach (MonoBehaviour s in scriptsADesligar)
            if (s != null) s.enabled = false;

        // desliga o Character Controller, se houver
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        StartCoroutine(Cair());
    }

    // tomba a câmara para trás e desce-a até ao chão
    private IEnumerator Cair()
    {
        Quaternion rotInicio = cameraPlayer.localRotation;
        Quaternion rotFim = Quaternion.Euler(-60f, 0f, 25f); // a olhar para o céu, inclinado de lado
        Vector3 posInicio = cameraPlayer.localPosition;
        Vector3 posFim = posInicio + Vector3.down * descida;

        float tempo = 0f;
        while (tempo < duracaoQueda)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracaoQueda;
            t = t * t; // começa devagar e acelera, como uma queda
            cameraPlayer.localRotation = Quaternion.Slerp(rotInicio, rotFim, t);
            cameraPlayer.localPosition = Vector3.Lerp(posInicio, posFim, t);
            yield return null; // espera pelo frame seguinte
        }
    }
}