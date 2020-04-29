using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSTvMovie : MonoBehaviour 
{
    [System.Serializable]
    public class DSTvMovieValues
    {
        public bool IsActive;
        [Header("max emission bright")]
        public float MaxBright = 3.0f;
        [Header("frames speed")]
        public float Speed;
        [Header("shared material")]
        public Material SharedMaterial;
        [Header("frames")]
        public Texture[] Frames;
        [HideInInspector]
        public int i;
    }
    public DSTvMovieValues[] _DSTvMovieValues;

    void Awake()
    {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks * 1000);
    }

	// Use this for initialization
	void Start () 
    {
        for (int k0 = 0; k0 < _DSTvMovieValues.Length; k0++)
        {
            if (_DSTvMovieValues[k0].IsActive)
            {
                //bright
                float random = Random.Range(1.5f, _DSTvMovieValues[k0].MaxBright);
                //color variations
                float random2 = Random.Range(0.2f, 0.8f);
                float random3 = Random.Range(0.2f, 0.8f);
                float random4 = Random.Range(0.2f, 0.8f);
                _DSTvMovieValues[k0].SharedMaterial.SetColor("_EmissionColor", new Color(random + random2, random + random3, random + random4));
            }
            else
            {
                _DSTvMovieValues[k0].SharedMaterial.SetColor("_EmissionColor", Color.black);
            }
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        for (int k0 = 0; k0 < _DSTvMovieValues.Length; k0++)
        {
            if (_DSTvMovieValues[k0].IsActive)
            {
                _DSTvMovieValues[k0].i = (int)(Time.time * _DSTvMovieValues[k0].Speed);
                _DSTvMovieValues[k0].i = _DSTvMovieValues[k0].i % _DSTvMovieValues[k0].Frames.Length;
                _DSTvMovieValues[k0].SharedMaterial.SetTexture("_EmissionMap", _DSTvMovieValues[k0].Frames[_DSTvMovieValues[k0].i]);
            }
        }
	}

    void OnDestroy()
    {
        for (int k0 = 0; k0 < _DSTvMovieValues.Length; k0++)
        {
            if (_DSTvMovieValues[k0].IsActive)
            {
                //_DSTvMovieValues[k0].SharedMaterial.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}
