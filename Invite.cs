using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
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
using UnityEngine.SceneManagement;
public class Invite : MonoBehaviour
{
    public LoginScript loginController;
    public string origin_user;
    public string own_socket;
    public string own_equipo;
    public Text chat_text;
    public Button exit;
    public Button decline;
    public Button accept;

    public void set_own_equipo(string equipo) { this.own_equipo = equipo; }
    public void set_own_socket(string socket) { this.own_socket = socket; }
    public void set_origin_user(string origin_user) { this.origin_user = origin_user; }
    public string get_origin_user() { return this.origin_user; }
    public string get_own_equipo() { return this.own_equipo; }
    public string get_own_socket() { return this.own_socket; }
    // Start is called before the first frame update



    /// <summary>
    /// Mensaje de invitación y gestión de los botones de aceptar y rechazar.
    /// </summary>
    void Start()
    {
        Application.runInBackground = true;
        loginController = GameObject.Find("Canvas UI Login").GetComponent<LoginScript>();
        exit.onClick.AddListener(()=>Destroy(gameObject));
        accept.onClick.AddListener(() => {
            loginController.myTeam = Convert.ToInt32(own_equipo);
            loginController.set_own_socket(Convert.ToInt32(own_socket));
            if (loginController.me == null)
            {
                loginController.me = new LoginScript.User(loginController.own_username, Convert.ToInt32(own_socket));
                loginController.me.set_equipo(Convert.ToInt32(own_equipo));
                loginController.user_list.AddUser(loginController.me);
            }
            if (!loginController.lobby_users.Contains(loginController.own_username))
            {
                loginController.lobby_users.Add(origin_user);
                loginController.last_selected_lobby++;
                loginController.accepted_lobby_users.Add(true);
                loginController.lobby_users.Add(loginController.own_username);
                loginController.last_selected_lobby++;
                loginController.accepted_lobby_users.Add(true);

            }
            loginController.set_leader(false);
            loginController.SendResponseToInvite("0", get_own_socket(), loginController.own_username, get_own_equipo()); Destroy(gameObject); });
            
        decline.onClick.AddListener(()=> {loginController.SendResponseToInvite("1", get_own_socket(), loginController.own_username, get_own_equipo()); Destroy(gameObject); });
        StartCoroutine(ChatInviteBoxFade());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// Animación para que aparezca el Invite Box de manera progresiva.
    /// </summary>
    /// <returns></returns>
    
    private IEnumerator ChatInviteBoxFade()
    {
        for (float i = 0; i <= 3; i += Time.deltaTime)
        {
            GetComponent<Image>().color = new UnityEngine.Color(0, 0, 0, i / 4);
            yield return null;
        }
        chat_text.text = origin_user + " has invited you to play.";
    }
}