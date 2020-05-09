using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSScrollTexture : MonoBehaviour 
{
    [Header("Texture Offsets")]
    public Vector2[] MoveRate;
    [Header("Shared Materials")]
    public Material[] SharedMaterials;


	// Use this for initialization
	void Start () 
    {

	}
	
	// Update is called once per frame
	void Update () 
    {
        for (int i = 0; i < SharedMaterials.Length; i++)
        {
            Vector2 UVOffs = SharedMaterials[i].GetTextureOffset("_MainTex");
            UVOffs += (MoveRate[i] * Time.deltaTime);
            UVOffs.x = UVOffs.x % 50;
            UVOffs.y = UVOffs.y % 50;
            SharedMaterials[i].SetTextureOffset("_MainTex", UVOffs);
        }
	}

    void OnDestroy()
    {
        for (int i = 0; i < SharedMaterials.Length; i++)
        {
            SharedMaterials[i].SetTextureOffset("_MainTex", Vector2.zero);
        }
    }
}
