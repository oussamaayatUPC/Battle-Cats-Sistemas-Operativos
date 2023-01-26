using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Transitioner : MonoBehaviour
{
    // Start is called before the first frame update
    /// <summary>
    /// Gestiona la transición entre escenas.
    /// </summary>
    void Start()
    {
        Application.runInBackground = true;
        DontDestroyOnLoad(this.gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }}