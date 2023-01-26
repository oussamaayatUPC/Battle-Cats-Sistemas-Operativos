using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
public class FSM : MonoBehaviour
{
    public float game_over;
    private int prefab_index;
    public string type; // Type of mob: cat or enemy
    public string target; // Target to attack: cat or enemy
    private Animator anim; // Animator of the mob
    private string state; // State in the FSM of the mob
    public float health_points; // Health points of the mob
    public float speed; // Speed of the mob
    public float attack_damage; // Attack damage of the mob
    public List<GameObject> currentCollisions = new List<GameObject>(); // Current collisions of the mob
    public List<GameObject> currentEnemies = new List<GameObject>(); // Current enemies (type target) colliding with the mob
    public GameObject focus; // Focus of the mob (mob to be attacked by this mob)
    public bool area_damage; // Whether if this mob or not has area damage
    public Spawner spawn_control; // Our spawn controller
    public Socket servidor;
    public IPEndPoint endpoint;
    public int port;
    public float reference_height;
    public Spawner.Area currentArea;
    public Spawner.Area previousArea;
    public float[] area_positions;
    public float first_contact_position;
    public int index_pos;
    public bool stop_checking;
    IPAddress ip; 
    public void set_prefab_index(int index) { prefab_index = index; }
    //public void set_reference_height(float ref_height) { reference_height = ref_height; }

    /// <summary>
    /// Detecta colisión con los gatos y enemigos que haya en partida y procesa su estado en la FSM y empieza o deja de atacar según conveniente.
    /// </summary>
    /// <param name="collision"></param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        
        currentCollisions.Add(collision.gameObject);
        if (collision.gameObject.tag == target || (collision.gameObject.tag == "Enemy base" && type == "Cat") || (collision.gameObject.tag == "Cat base" && type == "Enemy")) {
            currentEnemies.Add(collision.gameObject);
            state = "Attack";
            anim.SetBool("walk_state", false);
            anim.SetBool("attack_state", true);
           
            if (focus == null)
            {
                int focus_index = 0;
                Debug.Log(focus_index);
                focus = currentEnemies[focus_index];
            }
            StartCoroutine(AttackRoutine());

        }

        //if (collision.gameObject.tag == "Suelo")
        //{
        //    //state = "Walk";
        //    //if (!(spawn_control.prefabs_reference_level_already_set[prefab_index]))
        //    //{
        //    //    set_reference_height(GetComponent<Transform>().position.y);
        //    //    spawn_control.prefab_reference_level_height[prefab_index] = GetComponent<Transform>().position.y;
        //    //    spawn_control.prefabs_reference_level_already_set[prefab_index] = true;
        //    //    if (prefab_index < 7)
        //    //    {

        //    //        spawn_control.prefab_reference_level_height[prefab_index + 7] = GetComponent<Transform>().position.y;
        //    //        spawn_control.prefabs_reference_level_already_set[prefab_index + 7] = true;
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    set_reference_height(spawn_control.prefab_reference_level_height[prefab_index]);
        //    //}
        //    //gameObject.transform.position = new Vector3(gameObject.transform.position.x,-3.5f,0f);
        //}

        
    }

    /// <summary>
    /// Función para que el mob gestione qué debe hacer mientras esté tocando a otro mob.
    /// </summary>
    /// <param name="collision"></param>
    void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.gameObject.tag == target || (collision.gameObject.tag == "Enemy base" && type == "Cat") || (collision.gameObject.tag == "Cat base" && type == "Enemy"))
        {
            if (focus == null)
            {
                int focus_index = 0;
                Debug.Log(focus_index);
                focus = currentEnemies[focus_index];
            }

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
    /// Función que gestion qué ocurre cuando existe un trigger exit (salida del trigger box), que en nuestro caso solo ocurre cuando el enemigo muere.
    /// </summary>
    /// <param name="collision"></param>
    void OnTriggerExit2D(Collider2D collision)
    {
        currentCollisions.Remove(collision.gameObject);
        if (collision.gameObject.tag == target || (collision.gameObject.tag == "Enemy base" && type == "Cat") || (collision.gameObject.tag == "Cat base" && type == "Enemy"))
        {
            currentEnemies.Remove(collision.gameObject);
        }
        
        if (collision.gameObject == focus)
        {
            focus = null;
            if (currentEnemies.Count > 0)
            {
                int focus_index = 0;
                Debug.Log(focus_index);
                focus = currentEnemies[focus_index];
            }
            else
            {
                StopCoroutine(AttackRoutine());
                state = "Walk";
                anim.SetBool("attack_state", false);
                anim.SetBool("walk_state", true);
            }
        } 
    }
    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
        gameObject.transform.SetParent(GameObject.Find("Main Camera").transform);
        gameObject.transform.SetParent(GameObject.Find("Main Camera").transform);
        index_pos = 0;
        spawn_control = GameObject.Find("Main Camera").GetComponent<Spawner>();
        
        currentArea = spawn_control.list_of_areas.GetAreaFromPosition(gameObject.transform.position.x);
        currentArea.AddMob(this.gameObject);
        
        area_positions = new float[spawn_control.list_of_areas.GetAreaCount()-1];
        int i = 0;
        foreach (Spawner.Area area in spawn_control.list_of_areas.GetAreaList()) {
            if (area!= spawn_control.list_of_areas.GetAreaList()[0]) {
                area_positions[i] = area.GetXi();
                i++;
            }
                
        }
        if (type == "Cat") first_contact_position = area_positions[area_positions.Length-1];
        else first_contact_position = area_positions[0];
        GetComponent<Animator>().enabled = false;
        state = "Falling";
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Chequea posición de contacto definida (fronteras de áreas que básicamente avisa al cerebro de la torre de dónde está cada mob). Esto servirá para disparar la bola de energía donde haya más enemigos.
    /// </summary>
    /// <returns></returns>
    public bool CheckIfIAmInAreaFrontier()
    {
        if ((gameObject.transform.position.x >= first_contact_position && type == "Enemy") || (gameObject.transform.position.x <= first_contact_position && type == "Cat")) return true;
        return false;
    }
    // Update is called once per frame

    /// <summary>
    /// Función que usamos para controlar los estados de la FSM del personaje además de gestionar si ha acabado la partida y su posición.
    /// </summary>
    void Update()
    {
        if (!spawn_control.game_over)
        {
            if (!stop_checking)
            {
                if (gameObject.transform.position.y <= reference_height)
                {
                    state = "Walk";
                    stop_checking = true;
                }

            }

            

            if (CheckIfIAmInAreaFrontier())
            {
                index_pos++;
                currentArea.RemoveMob(this.gameObject);
                currentArea = spawn_control.list_of_areas.GetAreaFromPosition(gameObject.transform.position.x);
                currentArea.AddMob(this.gameObject);
                if (type == "Cat") first_contact_position = area_positions[area_positions.Length - 1 - index_pos];
                else first_contact_position = area_positions[index_pos];
            }

            if (index_pos == area_positions.Length - 1) index_pos = 0;

            if (state == "Walk" || state == "Attack")
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, reference_height, 0);
            if (state == "Walk")
            {
                GetComponent<Animator>().enabled = true;
                gameObject.transform.position -= Vector3.right * speed;
            }

            else if (state == "Falling")
            {
                gameObject.transform.position -= Vector3.up * 0.05f;
            }


            if (health_points <= 0)
            {

                state = "Dead";
                StartCoroutine(DestroyRoutine());

            }
        }

        if (spawn_control.game_over)
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// Atacar.
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackRoutine()
    {   while (state == "Attack")
        {
            anim.Play("Base Layer.Attack");
            if (area_damage) {
                foreach (GameObject go in currentEnemies)
                {
                    try
                    {
                        go.GetComponent<FSM>().GetDamage(attack_damage);
                    }
                    catch
                    {
                        go.GetComponent<Tower>().GetDamage(attack_damage);
                    }
                    
                } 
            }

            else
            {
                try {
                    focus.GetComponent<FSM>().GetDamage(attack_damage);
                }
                catch {
                    focus.GetComponent<Tower>().GetDamage(attack_damage);
                }
                
            }
            
            yield return new WaitForSecondsRealtime(1);
            
        }
    }


    /// <summary>
    /// Morir.
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyRoutine()
    {
        currentArea.RemoveMob(this.gameObject);
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        Destroy(GetComponent<Rigidbody2D>());
        spawn_control.currentMobs.Remove(gameObject);
        Destroy(gameObject);
        yield return null;
    }

    /// <summary>
    /// Establecer altura.
    /// </summary>
    /// <param name="height"></param>
    public void set_height(float height)
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, height, 0);
    }
}