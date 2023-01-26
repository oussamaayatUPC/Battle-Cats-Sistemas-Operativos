using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using static Spawner;
public class Spawner : MonoBehaviour
{
    public GameObject pause_menu;
    public LoginScript login_controller;
    public GameObject suelo;
    public Tower cat_tower;
    public Tower enemy_tower;
    public Button[] buttons;
    public GameObject[] prefabs;
    public Text[] pricetags;
    public int[] prices;
    public int index;
    private int index_destroy;
    public Socket servidor;
    public IPEndPoint endpoint;
    public int port;
    IPAddress ip;
    public ScreenController screen;
    public List<GameObject> currentMobs; // Mobs active in the current scene 
    public List<Vector3> currentpositions;
    public bool game_over;
    public bool leader;
    public Vector3 area_with_most_mobs;
    public int area_with_most_mobs_count;
    public List<int> area_of_each_mob;
    private bool pause_instance;
    //public bool[] prefabs_reference_level_already_set;
    //public float[] prefab_reference_level_height;

    public void set_pause_instance(bool pause) { pause_instance = pause; }
    public ListOfAreas list_of_areas;

    /// <summary>
    /// Lista de áreas imaginarias del juego (para saber dónde están los mobs de la partida). Interesante para saber dónde debe ir la bola de energía de la torre.
    /// </summary>
    public class ListOfAreas
    {
        private List<Area> areas;
        public ListOfAreas(List<Area> areas)
        {
            this.areas = areas;
        }
        public Area GetAreaWithMostMobsOfType(string type)
        {
            List<int> listCount = new List<int>() { };
            

            foreach (Area area in areas)
            {
                if (type == "Cat")
                {
                    listCount.Add(area.GetCatMobCount());
                }
                else if (type == "Enemy")
                {
                    listCount.Add(area.GetEnemyMobCount());
                }
                else {
                    listCount.Add(area.GetMobCount());
                }
              
            }
            return areas[listCount.IndexOf(listCount.Max())];
        }
        public int GetAreaCount() { return areas.Count; }
        public void AddArea(Area area) { areas.Add(area); }
        public List<Area> GetAreaList() { return areas; }
        public Area GetArea(float xi, float xf)
        {
            foreach(Area area in areas)
            {
                if (area.GetXi() == xi && area.GetXf() == xf) return area;
            }
            return null;
        }

        public Area GetAreaFromPosition(float x)
        {
            foreach (Area area in areas)
            {
                if (area.GetXi() <= x && area.GetXf() >= x) return area;
            }
            return null;
        }
        public int GetAreaIndex(Area area)
        {
            for (int i = 0; i < areas.Count; i++)
            {
                if (areas[i] == area)
                {
                    return i;
                }
            }
            return -1;
        }
        public void ShowAreasInfo()
        {
            foreach(Area area in areas)
            {
                area.ShowAreaInfo();
            }
        }

        public List<GameObject> GetAllMobsInArea(Area area)
        {
            return area.Get_list_of_mobs();
        }
    }

    /// <summary>
    /// Clase que define un área imaginaria, a partir de sus posiciones horizontales inciales y finales, además de guardar información sobre los mobs hallados en esa área (se coordina con la FSM de cada mob para tener información actualizada cada fotograma de ejecución del juego).
    /// </summary>
    public class Area
    {
        private float xi;
        private float xf;
        private int mob_count;
        private int enemy_count;
        private int cat_count;
        private float center;
        private List<GameObject> list_of_mobs;
        public Area(float xi, float xf)
        {
            this.xi = xi;
            this.xf = xf;
            center = (xi + xf)/2;
            list_of_mobs = new List<GameObject>() { };
        }

        public void set_list_of_mobs(List<GameObject> list_of_mobs) { this.list_of_mobs = list_of_mobs; }
        public List<GameObject> Get_list_of_mobs() { return list_of_mobs; }
        public void AddMob(GameObject mob) {
            if (!(CheckIfMobAlreadyThere(mob)))
            {
                list_of_mobs.Add(mob);
                mob_count++;
                if (mob.GetComponent<FSM>().type == "Cat") cat_count++;
                else enemy_count++;
            }
        }

        public bool CheckIfMobAlreadyThere(GameObject mob)
        {
            foreach(GameObject mob_i in list_of_mobs)
            {
                if (mob_i == mob) return true;
            }
            return false;
        }
        public void RemoveMob(GameObject mob)
        {
            list_of_mobs.Remove(mob);
            mob_count--;
            if (mob.GetComponent<FSM>().type == "Cat") cat_count--;
            else enemy_count--;
        }
        public float GetXi() { return xi; }
        public float GetXf() { return xf; }
        public void SetMobCount(int count) { mob_count = count; }
        public void SetCatMobCount(int count) { cat_count = count; }
        public void SetEnemyMobCount(int count) { enemy_count = count; }
        public int GetMobCount() { return mob_count; }
        public int GetCatMobCount() { return cat_count; }
        public int GetEnemyMobCount() { return enemy_count; }
        public List<GameObject> GetListOfMobs() { return list_of_mobs; }
        public float GetCenter() { return center; }
        public void ShowAreaInfo() { Debug.Log("Area: xf " + xf + "xi " + xi); }
    }

    /// <summary>
    /// Establecer áreas del juego.
    /// </summary>
    public void InitializeAreas()
    {
        list_of_areas = new ListOfAreas(new List<Area>() { }){ };
        int k = 0;
        for (float i = -15f; i < 15f; i += 2.5f)
        {
            list_of_areas.AddArea(new Area(i,i+2.5f));
            k++;
        }

        
    }

    /// <summary>
    /// Gestión del botón "Esc" entre otras cosas para crear una instancia del menú de pausa (para que el jugador pueda rendirse o cambiar el fondo de pantalla o mutear la música).
    /// </summary>
    void OnGUI()
    {
        if (Event.current.Equals(Event.KeyboardEvent(KeyCode.Escape.ToString())) && pause_instance == false)
        {
            Instantiate(pause_menu);
            pause_instance = true;
        }
    }
    

        /// <summary>
        /// Inicialización de estructuras, datos e información necesaria para el juego.
        /// </summary>
        // Start is called before the first frame update
        void Start()
        {
        Application.runInBackground = true;
        pause_instance = false;
        login_controller = GameObject.Find("Canvas UI Login").GetComponent<LoginScript>();
        login_controller.spawn_control = this;

        //cat_tower = Instantiate(cat_tower);
        //enemy_tower = Instantiate(enemy_tower);
        //cat_tower.transform.SetParent(GameObject.Find("Main Camera").transform);
        //enemy_tower.transform.SetParent(GameObject.Find("Main Camera").transform);
        InitializeAreas();
        //prefabs_reference_level_already_set = new bool[prefabs.Length];
        //prefab_reference_level_height = new float[prefabs.Length];
        
        
        if (buttons.Length != prefabs.Length)
            Debug.Log("Plese, check that ammount of buttons is the same as for prefabs");
        else
        {
            foreach (Button btn in buttons)
            {
                int value = index;
                Button buttoncomponent = btn.GetComponent<Button>();
                buttoncomponent.onClick.AddListener(() => InstantiatePrefab(value, true));
                index++;
            }
            index = 0;
        }

        foreach (Text text in pricetags)
        {
            int value = index;
            Text text_component = text.GetComponent<Text>();
            text_component.text = prices[value].ToString() + "$";
            index++;
        }
        
        index = 0;
        
    }

    /// <summary>
    /// Gestión de posiciones e información varia que luego será procesada por la mente principal y por varios entes del juego.
    /// </summary>
    // Update is called once per frame
    void Update()
    {   if (!(game_over)) {
            if (enemy_tower.health_points <= 0 || cat_tower.health_points <= 0)
            {
               
                game_over = true;
                login_controller.set_stop_playing(true);
                
            }
            currentpositions = GetAllMobsPosition();
            area_with_most_mobs = new Vector3(list_of_areas.GetAreaWithMostMobsOfType("").GetXi(), list_of_areas.GetAreaWithMostMobsOfType("").GetXf(), 0);
            area_with_most_mobs_count = list_of_areas.GetAreaWithMostMobsOfType("").GetMobCount();
            int[] check_if_possible = CheckIfPossible();

            foreach (Text text in pricetags)
            {
                int value = index;
                Text text_component = text.GetComponent<Text>();

                if (check_if_possible[value] == 1) text_component.color = Color.green;
                else text_component.color = Color.red;
                index++;
            }
            index = 0;
        }

        if (game_over)
        {
            login_controller.set_stop_playing(true);
        }
        

    }

    /// <summary>
    /// Comprobar si es posible spawnear un mob.
    /// </summary>
    /// <returns></returns>
    int[] CheckIfPossible()
    {
        int[] check_if_possible = new int[prices.Length];
        foreach (int price in prices) { 
            int value = index;
            if (screen.money >= price)
            {
                check_if_possible[value] = 1;
            }
            else
            {
                check_if_possible[value] = 0;
            }
            index++;
        }
        index = 0;
        return check_if_possible;
    } 

    /// <summary>
    /// Instanciar prefabricado de un mob (gestión sobre si es posible o no instanciarlo en base al dinero disponible, además de enviar información del servidor y avisar a la mente principal de lo qué ocurre en partida).
    /// </summary>
    /// <param name="n"></param>
    /// <param name="local"></param>
    public void InstantiatePrefab(int n, bool local)
    {

        if (screen.money >= prices[n] && local)
        {
            if (prefabs[n].GetComponent<FSM>() == null)
            {
                
                if (login_controller.user_list.GetUserCount() == 2)
                {
                    string mensaje = "11/" + login_controller.user_list.GetUserCount().ToString() + "/" + login_controller.user_list.GetRival(login_controller.me).get_associated_socket().ToString() + "/" + n.ToString();
                    byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                    login_controller.servidor.Send(msg);
                }
                else
                {
                    List <LoginScript.User> rivals= login_controller.user_list.GetRivals(login_controller.me);
                    string mensaje = "11/" + login_controller.user_list.GetUserCount().ToString() + "/" +  rivals.ElementAt(0).get_associated_socket().ToString() + "/" + rivals.ElementAt(1).get_associated_socket().ToString() + "/" + login_controller.user_list.GetTeamMate(login_controller.me).get_associated_socket() + "/" + n.ToString();
                    byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                    login_controller.servidor.Send(msg);
                }
                GameObject instance = Instantiate(prefabs[n]);
                instance.transform.SetParent(GameObject.Find("Main Camera").GetComponent<Transform>());

            }
            else
            {
                if (login_controller.user_list.GetUserCount() == 2)
                {
                    string mensaje = "11/" + login_controller.user_list.GetUserCount().ToString() + "/" + login_controller.user_list.GetRival(login_controller.me).get_associated_socket().ToString() + "/" + n.ToString();
                    byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                    login_controller.servidor.Send(msg);
                }
                else
                {
                    List<LoginScript.User> rivals = login_controller.user_list.GetRivals(login_controller.me);
                    string mensaje = "11/" + login_controller.user_list.GetUserCount().ToString() + "/" + rivals.ElementAt(0).get_associated_socket().ToString() + "/" + rivals.ElementAt(1).get_associated_socket().ToString() + "/" + login_controller.user_list.GetTeamMate(login_controller.me).get_associated_socket() + "/" + n.ToString();
                    byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                    login_controller.servidor.Send(msg);
                }

                float spawn_x = -15;
                if (prefabs[n].GetComponent<FSM>().type == "Cat")
                {
                    spawn_x = 15;
                }
                GameObject instance = Instantiate(prefabs[n], new Vector3(spawn_x, 0, 0), Quaternion.identity);
                currentMobs.Add(instance);
                if (n > 7) instance.GetComponent<FSM>().set_prefab_index(n - 7);
                else instance.GetComponent<FSM>().set_prefab_index(n);
                instance.transform.SetParent(GameObject.Find("Main Camera").GetComponent<Transform>());
            }
            if ((screen.money - prices[n]) > 0)
            {
                screen.money -= prices[n];
            }
            else
            {
                screen.money = 0;
            }


        }
        else
        {
            if (prefabs[n].GetComponent<FSM>() == null)
            {
                GameObject instance = Instantiate(prefabs[n]);
            } else {
                float spawn_x = -15;
                if (prefabs[n].GetComponent<FSM>().type == "Cat")
                {
                    spawn_x = 15;
                }
                GameObject instance = Instantiate(prefabs[n], new Vector3(spawn_x, 0, 0), Quaternion.identity);
                currentMobs.Add(instance);
                if (n > 7) instance.GetComponent<FSM>().set_prefab_index(n - 7);
                else instance.GetComponent<FSM>().set_prefab_index(n);
            }
        }
        
    }   

    /// <summary>
    /// Obtener la posición de todos los mobs en pantalla (función de debuggeo para comprobar si la actualización constante de posiciones e información sobre cada área se realiza de manera correcta).
    /// </summary>
    /// <returns></returns>
    public List<Vector3> GetAllMobsPosition() {
        List<Vector3> positions = new List<Vector3>();
        foreach(GameObject mob in currentMobs) { 
            positions.Add(mob.transform.position);
        }
        return positions;
    }
}