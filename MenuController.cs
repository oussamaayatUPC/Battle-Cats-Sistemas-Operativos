using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public Button close;
    public Button background_change;
    public Button forfeit;
    public ScreenController screenController;
    public Spawner spawn_control;
    
    

    // Start is called before the first frame update
    /// <summary>
    /// Gestión de los botones del menú pausa.
    /// </summary>
    void Start()
    {
        Application.runInBackground = true;
        spawn_control = GameObject.Find("Main Camera").GetComponent<Spawner>();
        forfeit.onClick.AddListener(delegate {
           
            spawn_control.set_pause_instance(false);
            spawn_control.game_over = true;
            Destroy(gameObject);
        } );
        close.onClick.AddListener(() => { spawn_control.set_pause_instance(false); Destroy(gameObject); });
        screenController = GameObject.Find("Background gameplay").GetComponent<ScreenController>();
        background_change.onClick.AddListener(() => screenController.BackGroundChange());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
