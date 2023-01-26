using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Blast : MonoBehaviour
{
    public float obj_height;
    public float damage;
    public Spawner spawn_control;
    public string type;
    public Tower tower;
    float x;
    float y;
    public float velocity_x;
    public float velocity_y;
    public bool move;
    public List<GameObject> all_mobs;
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
        anim = GetComponent<Animator>();
        anim.enabled = false;
        spawn_control = GameObject.Find("Main Camera").GetComponent<Spawner>();
        x = GetComponent<Transform>().position.x;
        y = GetComponent<Transform>().position.y;
        move = false;
        tower = GameObject.Find(type + " base").GetComponent<Tower>();
        StartCoroutine(ShootBlast());
    }    

    /// <summary>
    /// Cálculo de velocidad x de la bola de energía.
    /// </summary>
    /// <param name="velocity_y"></param>
    public void calc_x_vel(float velocity_y)
    {
        // x = vt t = x/v
        float time_to_fall = (GetComponent<Transform>().position.y - spawn_control.suelo.GetComponent<Transform>().position.y) / velocity_y;
        velocity_x = (GetComponent<Transform>().position.x - tower.center) / time_to_fall;

    }

    /// <summary>
    /// Animación que crea la bola y la hace grande.
    /// </summary>
    /// <returns></returns>
    IEnumerator ShootBlast()
    {
        calc_x_vel(velocity_y);
        tower.blast.color = new Color(1f, 1f, 1f, 0f);
        tower.blast.GetComponent<SpriteRenderer>().enabled = true;
        for (float i = 0; i <= 3; i += Time.deltaTime)
        {
            tower.blast.color = new Color(1f, 1f, 1f, i);
            transform.localScale = new Vector3(i, i, i);
            yield return null;
        }
        tower.blast.GetComponent<SpriteRenderer>().enabled = false;
        move = true;

    }

    /// <summary>
    /// Gestiona colisión con cualquier target de la torre. Es decir, daña solo a los mobs enemigos del juhador.
    /// </summary>
    /// <param name="collision"></param>
    void OnTriggerEnter2D(Collider2D collision)
    { 
        if (collision.gameObject.tag == tower.target)
        {
            collision.gameObject.GetComponent<FSM>().GetDamage(damage);
        }
    }
    // Update is called once per frame
    void Update()

    {   if(gameObject.transform.position.y <= spawn_control.suelo.transform.position.y)
        {
            move = false;
            all_mobs = spawn_control.list_of_areas.GetAreaFromPosition(gameObject.transform.position.x).GetListOfMobs();
            StartCoroutine(DestroyRoutine());
        }
        if (move)
        {
            GetComponent<Transform>().position = new Vector3(x, y, 0);
            x += velocity_x;
            y += velocity_y;
        }
    }

    /// <summary>
    /// Destruye la bola de energía.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DestroyRoutine()
    {
        anim.enabled = true;
        transform.localScale = new Vector3(5, 5, 5);
        anim.Play("Base Layer.Boom");
        GetComponent<SpriteRenderer>().sortingOrder = 6;
        yield return new WaitForSecondsRealtime(1);
        Destroy(this.gameObject);
    }}