using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSBlinking : MonoBehaviour 
{
    [System.Serializable]
    public class DSBlinkingValues
    {
        public bool IsActive;
        [Header("Max HDR bright")]
        [ColorUsageAttribute(true, true)]
        public Color MaxBright;
        [Header("blinking")]
        public float SwitchTime = 0.1f;
        public enum SwitchMode
        {
            simple = 0,
            multi = 1,
            broken = 2,
        }
        [Header("Simple blinking, Increasing Blinking, Broken lamp")]
        public SwitchMode SwitchModes = SwitchMode.simple;
        [HideInInspector]
        public int mode;
        [HideInInspector]
        public int count;
        [Header("Shared Material")]
        public Material SharedMaterial;

        public IEnumerator ColorSwitch(float random)
        {
            while (true)
            {
                yield return new WaitForSeconds(SwitchTime + random);

                //0 - 1 - 0 - 1 - 0...
                if (SwitchModes == SwitchMode.simple)
                {
                    mode = 1 - mode;
                    if (mode == 0)
                    {
                        SharedMaterial.SetColor("_EmissionColor", Color.black);
                    }
                    else
                    {
                        SharedMaterial.SetColor("_EmissionColor", MaxBright);
                    }
                }

                //0 - 0.5 - 1 - 0...
                else if (SwitchModes == SwitchMode.multi)
                {
                    mode++;
                    if (mode > 2) mode = 0;
                    if (mode == 0)
                    {
                        SharedMaterial.SetColor("_EmissionColor", Color.black);
                    }
                    else if (mode == 1)
                    {
                        SharedMaterial.SetColor("_EmissionColor", MaxBright * 0.5f);
                    }
                    else
                    {
                        SharedMaterial.SetColor("_EmissionColor", MaxBright);
                    }
                }

                //broken lamp
                else
                {
                    if (count < 10)
                    {
                        mode++;
                        if (mode > 2) mode = 0;
                        if (mode == 0)
                        {
                            SharedMaterial.SetColor("_EmissionColor", Color.black);
                        }
                        else if (mode == 1)
                        {
                            SharedMaterial.SetColor("_EmissionColor", MaxBright * 0.5f);
                        }
                        else
                        {
                            SharedMaterial.SetColor("_EmissionColor", MaxBright);
                        }
                    }
                    else if (count < 20)
                    {
                        SharedMaterial.SetColor("_EmissionColor", MaxBright * 0.5f);
                    }
                    else
                    {
                        SharedMaterial.SetColor("_EmissionColor", MaxBright);
                    }
                    count++;
                    if (count > 50) count = 0;
                }
                random = 0;
            }
        }
    }
    public DSBlinkingValues[] _DSBlinkingValues;


    void Awake()
    {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks * 1000);
    }

	// Use this for initialization
	void Start () 
    {
        for (int k0 = 0; k0 < _DSBlinkingValues.Length; k0++)
        {
            if (_DSBlinkingValues[k0].IsActive)
            {
                float random = Random.Range(0.07f, 0.95f);
                StartCoroutine(_DSBlinkingValues[k0].ColorSwitch(random));
            }
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}

    void OnDestroy()
    {
        for (int k0 = 0; k0 < _DSBlinkingValues.Length; k0++)
        {
            if (_DSBlinkingValues[k0].IsActive)
            {
                _DSBlinkingValues[k0].SharedMaterial.SetColor("_EmissionColor", _DSBlinkingValues[k0].MaxBright);
            }
        }
    }

}
