using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Frost")]
public class FrostEffect : MonoBehaviour
{
    public float frostAmount = 1f; //0-1 (0=minimum Frost, 1=maximum frost)
    public float EdgeSharpness = 1; //>=1
    public float minFrost = 0; //0-1
    public float maxFrost = 1; //0-1
    public float seethroughness = 0.2f; //blends between 2 ways of applying the frost effect: 0=normal blend mode, 1="overlay" blend mode
    public float distortion = 0.1f; //how much the original image is distorted through the frost (value depends on normal map)
    public Texture2D frost; //RGBA
    public Texture2D frostNormals; //normalmap
    public Shader shader; //ImageBlendEffect.shader
	
	private Material material;

	private void Awake() { 
        material = new Material(shader);
        material.SetTexture("_BlendTex", frost);
        material.SetTexture("_BumpMap", frostNormals);
	}

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {     
        if (!Application.isPlaying) {         
            material.SetTexture("_BlendTex", frost);
            material.SetTexture("_BumpMap", frostNormals);
            EdgeSharpness = Mathf.Max(1, EdgeSharpness);
        }
        material.SetFloat("_BlendAmount", Mathf.Clamp01(Mathf.Clamp01(frostAmount) * (maxFrost - minFrost) + minFrost));
        material.SetFloat("_EdgeSharpness", EdgeSharpness);
        material.SetFloat("_SeeThroughness", seethroughness);
        material.SetFloat("_Distortion", distortion);

		Graphics.Blit(source, destination, material);
	}
}