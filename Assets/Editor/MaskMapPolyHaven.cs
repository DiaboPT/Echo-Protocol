using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Prepara um conjunto de texturas do Poly Haven para o HDRP/Lit.
// Uso: seleciona no Project qualquer textura do conjunto (ex.: corrugated_iron_03_diff_4k)
//      > Tools > ANJOCA > Preparar textura Poly Haven
// Faz: acerta a importação de todas (Normal map, sRGB, Max Size, Aniso) e cria a _MaskMap.png
// Aceita os mapas soltos (rough, metal, ao) ou o mapa "arm" (R = AO, G = Rough, B = Metal).
// Mask Map: R = Metallic, G = Ambient Occlusion, B = Detail Mask (1), A = Smoothness (1 - Roughness)
public static class MaskMapPolyHaven
{
    const int tamanhoMaximo = 2048;
    const int aniso = 8;

    // "corrugated_iron_03_diff_4k" -> base "corrugated_iron_03", mapa "diff", resolução "4k"
    static readonly Regex padrao = new Regex(@"^(.+)_(diff|col|nor_gl|nor_dx|rough|metal|ao|disp|arm)_(\d+k)$", RegexOptions.IgnoreCase);

    [MenuItem("Tools/ANJOCA/Preparar textura Poly Haven")]
    static void Preparar()
    {
        if (Selection.activeObject is not Texture2D escolhida)
        {
            EditorUtility.DisplayDialog("Poly Haven", "Seleciona uma textura do conjunto (por exemplo a _diff_).", "OK");
            return;
        }

        string caminho = AssetDatabase.GetAssetPath(escolhida);
        string pasta = Path.GetDirectoryName(caminho).Replace("\\", "/");
        Match mt = padrao.Match(Path.GetFileNameWithoutExtension(caminho));
        if (!mt.Success)
        {
            EditorUtility.DisplayDialog("Poly Haven", "O nome não parece do Poly Haven (algo como nome_diff_2k).", "OK");
            return;
        }
        string baseNome = mt.Groups[1].Value;
        string res = mt.Groups[3].Value;

        string color = Procurar(pasta, baseNome, "diff", res) ?? Procurar(pasta, baseNome, "col", res);
        string normal = Procurar(pasta, baseNome, "nor_gl", res);
        string rough = Procurar(pasta, baseNome, "rough", res);
        string metal = Procurar(pasta, baseNome, "metal", res);
        string ao = Procurar(pasta, baseNome, "ao", res);
        string arm = Procurar(pasta, baseNome, "arm", res);
        string disp = Procurar(pasta, baseNome, "disp", res);

        Importar(color, TextureImporterType.Default, true);
        Importar(normal, TextureImporterType.NormalMap, false);
        Importar(rough, TextureImporterType.Default, false);
        Importar(metal, TextureImporterType.Default, false);
        Importar(ao, TextureImporterType.Default, false);
        Importar(arm, TextureImporterType.Default, false);
        Importar(disp, TextureImporterType.Default, false);

        if (rough == null && arm == null)
        {
            EditorUtility.DisplayDialog("Poly Haven", "Não encontrei a _rough_ nem a _arm_ na mesma pasta, por isso não criei a Mask Map.", "OK");
            return;
        }

        Texture2D referencia = AssetDatabase.LoadAssetAtPath<Texture2D>(rough ?? arm);
        int w = referencia.width, h = referencia.height;
        Color[] r = rough != null ? Ler(referencia, w, h) : null;
        Color[] m = metal != null ? Ler(AssetDatabase.LoadAssetAtPath<Texture2D>(metal), w, h) : null;
        Color[] a = ao != null ? Ler(AssetDatabase.LoadAssetAtPath<Texture2D>(ao), w, h) : null;
        Color[] p = arm != null ? Ler(AssetDatabase.LoadAssetAtPath<Texture2D>(arm), w, h) : null;

        var px = new Color[w * h];
        for (int i = 0; i < px.Length; i++)
        {
            // Os mapas soltos têm prioridade; se faltarem, usa o canal do "arm"
            float rugosidade = r != null ? r[i].r : p[i].g;
            float metalico = m != null ? m[i].r : (p != null ? p[i].b : 0f);
            float oclusao = a != null ? a[i].r : (p != null ? p[i].r : 1f);
            px[i] = new Color(metalico, oclusao, 1f, 1f - rugosidade);
        }

        string saida = pasta + "/" + baseNome + "_MaskMap_" + res + ".png";
        Gravar(px, w, h, saida);
        Importar(saida, TextureImporterType.Default, false);

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(saida);
        Debug.Log($"Poly Haven pronto: {baseNome}. Usou {(arm != null && (rough == null || metal == null || ao == null) ? "o mapa arm" : "os mapas soltos")}.");
    }

    static string Procurar(string pasta, string baseNome, string mapa, string res)
    {
        foreach (string ext in new[] { ".jpg", ".png", ".jpeg", ".tif", ".exr" })
        {
            string c = pasta + "/" + baseNome + "_" + mapa + "_" + res + ext;
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(c) != null) return c;
        }
        return null;
    }

    static void Importar(string caminho, TextureImporterType tipo, bool srgb)
    {
        if (caminho == null) return;
        var imp = (TextureImporter)AssetImporter.GetAtPath(caminho);
        imp.textureType = tipo;
        if (tipo == TextureImporterType.Default) imp.sRGBTexture = srgb;
        imp.alphaSource = caminho.Contains("_MaskMap_") ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
        imp.maxTextureSize = tamanhoMaximo;
        imp.anisoLevel = aniso;
        imp.SaveAndReimport();
    }

    // Lê os píxeis mesmo que a textura não esteja marcada como Read/Write
    static Color[] Ler(Texture2D tex, int w, int h)
    {
        var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        Graphics.Blit(tex, rt);
        var anterior = RenderTexture.active;
        RenderTexture.active = rt;
        var copia = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
        copia.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        copia.Apply();
        RenderTexture.active = anterior;
        RenderTexture.ReleaseTemporary(rt);
        Color[] px = copia.GetPixels();
        Object.DestroyImmediate(copia);
        return px;
    }

    static void Gravar(Color[] px, int w, int h, string caminho)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(caminho, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(caminho);
    }
}
