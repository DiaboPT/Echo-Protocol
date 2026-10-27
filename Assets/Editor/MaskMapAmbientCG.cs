using System.IO;
using UnityEditor;
using UnityEngine;

// Prepara um conjunto de texturas do ambientCG para o HDRP/Lit.
// Uso: seleciona no Project qualquer textura do conjunto (ex.: PaintedMetal004_2K-JPG_Color)
//      > Tools > ANJOCA > Preparar textura ambientCG
// Faz: acerta a importação de todas (Normal map, sRGB, Max Size, Aniso) e cria a _MaskMap.png
// Mask Map: R = Metallic, G = Ambient Occlusion, B = Detail Mask (1), A = Smoothness (1 - Roughness)
public static class MaskMapAmbientCG
{
    const int tamanhoMaximo = 2048;
    const int aniso = 8;

    [MenuItem("Tools/ANJOCA/Preparar textura ambientCG")]
    static void Preparar()
    {
        if (Selection.activeObject is not Texture2D escolhida)
        {
            EditorUtility.DisplayDialog("ambientCG", "Seleciona uma textura do conjunto (por exemplo a _Color).", "OK");
            return;
        }

        // "PaintedMetal004_2K-JPG_Color" -> base "PaintedMetal004_2K-JPG"
        string caminho = AssetDatabase.GetAssetPath(escolhida);
        string pasta = Path.GetDirectoryName(caminho).Replace("\\", "/");
        string nome = Path.GetFileNameWithoutExtension(caminho);
        int ultimo = nome.LastIndexOf('_');
        if (ultimo < 0)
        {
            EditorUtility.DisplayDialog("ambientCG", "O nome não parece do ambientCG (falta o _Color, _Roughness...).", "OK");
            return;
        }
        string baseNome = nome.Substring(0, ultimo);

        string color = Procurar(pasta, baseNome, "_Color");
        string normal = Procurar(pasta, baseNome, "_NormalGL");
        string rough = Procurar(pasta, baseNome, "_Roughness");
        string metal = Procurar(pasta, baseNome, "_Metalness");
        string ao = Procurar(pasta, baseNome, "_AmbientOcclusion");
        string disp = Procurar(pasta, baseNome, "_Displacement");

        Importar(color, TextureImporterType.Default, true);
        Importar(normal, TextureImporterType.NormalMap, false);
        Importar(rough, TextureImporterType.Default, false);
        Importar(metal, TextureImporterType.Default, false);
        Importar(ao, TextureImporterType.Default, false);
        Importar(disp, TextureImporterType.Default, false);

        if (rough == null)
        {
            EditorUtility.DisplayDialog("ambientCG", "Não encontrei a _Roughness na mesma pasta, por isso não criei a Mask Map.", "OK");
            return;
        }

        Texture2D texRough = AssetDatabase.LoadAssetAtPath<Texture2D>(rough);
        Color[] r = Ler(texRough, texRough.width, texRough.height);
        int w = texRough.width, h = texRough.height;
        Color[] m = metal != null ? Ler(AssetDatabase.LoadAssetAtPath<Texture2D>(metal), w, h) : null;
        Color[] a = ao != null ? Ler(AssetDatabase.LoadAssetAtPath<Texture2D>(ao), w, h) : null;

        var px = new Color[w * h];
        for (int i = 0; i < px.Length; i++)
        {
            float metalico = m != null ? m[i].r : 0f;
            float oclusao = a != null ? a[i].r : 1f;
            px[i] = new Color(metalico, oclusao, 1f, 1f - r[i].r);
        }

        string saida = pasta + "/" + baseNome + "_MaskMap.png";
        Gravar(px, w, h, saida);
        Importar(saida, TextureImporterType.Default, false);

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(saida);
        Debug.Log($"ambientCG pronto: {baseNome}. Mask Map com Metalness: {(m != null ? "sim" : "não")}, AO: {(a != null ? "sim" : "não")}.");
    }

    static string Procurar(string pasta, string baseNome, string sufixo)
    {
        foreach (string ext in new[] { ".jpg", ".png", ".jpeg", ".tif", ".exr" })
        {
            string c = pasta + "/" + baseNome + sufixo + ext;
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
        imp.alphaSource = caminho.EndsWith("_MaskMap.png") ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
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
