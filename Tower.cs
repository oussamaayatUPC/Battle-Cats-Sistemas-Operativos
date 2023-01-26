using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
public class Tower : MonoBehaviour
{
    public Spawner spawn_control;
    public float game_over;
    public float health_points;
    public SpriteRenderer blast;
    public float center;
    public string target;    
    public bool move;
    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
        move = false;
    }
    // Update is called once per frame

    /// <summary>
    /// Gestiona el estado de la torre (avisa de si debe destruirse).
    /// </summary>
    void Update()
    {
        spawn_control = GameObject.Find("Main Camera").GetComponent<Spawner>();
        center = spawn_control.list_of_areas.GetAreaWithMostMobsOfType(target).GetCenter();
        if (spawn_control.game_over)
        {
            Destroy(gameObject);
        }
        if (health_points <= 0)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, -50, 0);
            StartCoroutine(DestroyRoutine());
        }
        
    }
     
    /// <summary>
    /// Recibir daño.
    /// </summary>
    /// <param name="damage"></param>
    public void GetDamage(float damage)
    {
        this.health_points -= damage;
    }

    /// <summary>
    /// Destruir la torre (diferente a la función de Unity por temas de conveniencia y gestión de la información disponible del estado de la partida). Creada para solventar problemas y evitar errores.
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyRoutine()
    {
        yield return new WaitForSecondsRealtime(1);
        Destroy(gameObject);
    }
}