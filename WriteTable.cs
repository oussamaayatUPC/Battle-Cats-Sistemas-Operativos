using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WriteTable : MonoBehaviour
{
    public bool all_users;
    public LoginScript loginScript;
    public List<string> users_connected;
    public List<string> users_to_show;
    List<GameObject> gameObjects = new List<GameObject>();
    List<GameObject> gameObjects_images = new List<GameObject>();
    List<Text> texts = new List<Text>();
    List<Image> images = new List<Image>();
    private RectTransform rect;
    private int changelog;
    private bool change;
    public string selected_user;
    //Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
        users_connected = new List<string>() { };
        rect = GameObject.Find("Table").GetComponent<RectTransform>();
        changelog = 0;
    }
    //Update is called once per frame

    /// <summary>
    /// Actualización dinámica por fotograma de la lista de conectados. El changelog sirve básicamente para que la función no se invoque nada más recibir la lista de conectads, ya que además tenemos la lista de usuarios.
    /// </summary>
    void Update()
    {
        if (change && changelog != 0)
        {
            PutElements(users_to_show);
            set_change(false);
        }
    }
    public string Get_Selected_user() { return selected_user; }
    public void increase_change_log() { changelog++; }
    
    public void set_users_connected(List<string> users_connected) { this.users_connected = users_connected; }
    public void set_change(bool change) { this.change = change; }
    public void reinitialize_selected_user() { 
        selected_user = "";
        set_change(true);
    }

    /// <summary>
    /// Seleccionar usuario, además de mirar si hay MessageBox y ejecutar consultas desde el servidor usando la mente principal como intermediario.
    /// </summary>
    /// <param name="selected_user"></param>
    public void set_selected_user(string selected_user) {
        if (GameObject.Find("Canvas UI Login").GetComponent<LoginScript>().get_instantiated())
        {
            switch (GameObject.Find("Canvas UI Login").GetComponent<LoginScript>().GetLastPressedButton())
            {
                case "victory":
                    GameObject.Find("Canvas UI Login").GetComponent<LoginScript>().RequestWins(selected_user);
                    break;
                case "average":
                    GameObject.Find("Canvas UI Login").GetComponent<LoginScript>().RequestAverage(selected_user);
                    break;
            }
        }
        set_change(true);
        this.selected_user = selected_user; 
    }


    /// <summary>
    /// Comprobar si usuario está conectad (solo localmente), de la parte del servidor ya se encarga la mente principal cuando lo vea conveniente.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool CheckIfConnected(string s)
    {
        foreach (string t in users_connected)
        {
            if (t == s)
                return true;
        }

        return false;
    }
    
    /// <summary>
    /// Actualización de los usuarios a mostrar (función que nos da versatilidad a la hora de mostrar información de diversa índole sin hacer una reestructuración considerable del servidor o cliente).
    /// </summary>
    /// <param name="users"></param>
    public void users_to_show_update(List<string> users)
    {
        users_to_show.Clear();
        foreach(string conn in users_connected)
        {
            users_to_show.Add(conn);
        }
        foreach(string user in users)
        {
            if (!(users_to_show.Contains(user))) users_to_show.Add(user);
        }
    }
    /// <summary>
    /// Función que pone a todos los usuarios, conectados primero y en verde y no conectados después y en rojo en un panel.
    /// </summary>
    /// <param name="users"></param>
    public void PutElements(List<string> users)
    {
        GameObject go_text;
        GameObject go_image;
        Text text;
        Image image;
        images.Clear();
        texts.Clear();
        gameObjects_images.Clear();
        gameObjects.Clear();
        for (int i = 0; i < transform.childCount; ++i)
            Destroy(transform.GetChild(i).gameObject);
        foreach (string s in users)
        {
            go_image = new GameObject(s + "_bg");
            go_image.transform.SetParent(this.transform);
            image = go_image.AddComponent<Image>();
            go_image.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 25f);
            go_image.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            go_image.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 50);
            image.color = new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b, 0f);
            go_text = new GameObject(s);
            go_text.transform.SetParent(go_image.transform);
            text = go_text.AddComponent<Text>();
            go_text.AddComponent<Button>();
            go_text.GetComponent<Button>().onClick.AddListener(()=> {if (all_users) {set_selected_user(s); if ((loginScript.selected_users.Count < 3) && (loginScript.own_username != s) && !(loginScript.selected_users.Contains(s)) &&  (users_connected.Contains(s))) { loginScript.selected_users.Add(s); loginScript.last_selected_invite++; loginScript.modify_selected_invite_list(); } } });
            go_text.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 25f);
            go_text.GetComponent<RectTransform>().sizeDelta = new Vector2(350,50);
            go_text.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            text.text = s;
            text.color = Color.red;
            if (s == selected_user)
            {
                image.color = new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b, 0.5f);   
            }
            if (all_users) { if (CheckIfConnected(s)) { text.color = Color.green; } }
            else { text.color = Color.white; }
            text.fontSize = 36;
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.alignment = TextAnchor.MiddleCenter;
            gameObjects.Add(go_text);
            gameObjects_images.Add(go_image);
            texts.Add(text);
            images.Add(image);
        }
    }
    /// <summary>
    /// Función que devuelve un objeto del tipo Text, característico de Unity, de un objeto del tipo List de objetos del tipo Text.
    /// </summary>
    /// <param name="text_list"></param>
    /// <param name="texts"></param>
    /// <returns></returns>
    public Text GetTextFromList(string text_list, List<Text> texts)
    {
        foreach (Text t in texts)
        {
            if (t.text == text_list)
            {
                return t;
            }
        }
        return null;
    }

    /// <summary>
    /// Función que modifica los conectados (función residual entre otras cosas).
    /// </summary>
    /// <param name="connected"></param>
    public void ModifyConnected(List<string> connected)
    {
        foreach (string t in connected)
        {

            GetTextFromList(t, texts).color = Color.green;
        }
    }
}