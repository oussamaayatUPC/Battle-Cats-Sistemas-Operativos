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
using static LoginScript;

public class LoginScript : MonoBehaviour {

    // Hay que reinicializar todas las variables al salir de la partida
    public Scene currentScene;
    public User me;
    public GameObject back_prefab;
    public GameObject back_instance;
    public int myTeam;
    public string own_username;
    public string own_password;
    public GameObject invite_list;
    public GameObject lobby_list;
    public WriteChat chat_controller;
    public Spawner spawn_control;
    public Canvas canvas_welcome;
    public Button delete_user;
    public Button play;
    public Button login;
    public Button disconnect;
    public Button wins;
    public Button average;
    public Button day_button;
    public Button register_button;
    public Button register_instancer;
    public Button login_instancer;
    public Button chat_button;
    public Button invite_button;
    public Button played_with_button;
    public InputField username;
    public InputField password;
    public InputField day;
    public InputField birthday;
    public InputField email;
    public InputField message;
    public Socket servidor;
    public IPEndPoint endpoint;
    public int port;
    public int last_selected_invite;
    public int last_selected_lobby;
    public string ippaddr;
    private GameObject instance;
    public GameObject login_instance;
    public GameObject register_instance;
    IPAddress ip;
    private WriteTable table;
    public GameObject user_info;
    public GameObject register_menu;
    public GameObject login_menu;
    public GameObject invite_box;
    public string[] trozos;
    private bool conectado;
    private bool change_chat;
    private bool show_invite;
    private bool show_wins;
    private bool show_average;
    private bool show_users;
    private bool show_day;
    private bool close_server;
    private bool show_connected;
    private bool show_users_i_have_played_with;
    private bool confirmation_invite;
    private bool instantiated;
    private bool instantiate_prefab;
    private bool establish_global_structure;
    private bool update_lobby_structure;
    private bool stop_playing;
    private bool start_playing;
    public bool leader;
    private int own_socket;
    private bool first_time;
    private string last_pressed_button;
    public List<string> connected;
    public List<string> users;
    public List<string> AllPlayedWith;
    public List<string> selected_users;
    public List<string> lobby_users;
    public List<bool> accepted_lobby_users;
    public WriteTable users_table;
    public WriteTable users_played_with_table;
    public UserList user_list = new UserList(new List<User>());

    /// <summary>
    /// Clase lista de usuario para almacenar información de los usuarios de la partida (nombre, socket, equipo)
    /// </summary>
    public class UserList
    {
        private List<User> user_list;
        private int user_count;
        private int result_against;
        public UserList(List<User> user_list)
        {
            this.user_list = user_list;
            user_count = 0;
        }

        public int GetUserCount() { return user_count; }
        public void AddUser(User user)
        {
            if (!(UserExists(user))) {
                user_list.Add(user);
                user_count++;
            }
                
        }

        public User GetUser(User user)
        {
            for (int i = 0; i < user_list.Count; i++)
            {
                if (user.CompareUser(user_list[i]))
                    return user_list[i];
            }
            return null;
        }

        
        public void RemoveUser(User user) { 
            if (user!=null)
            {
                user_list.Remove(user);
                user_count--;
            }
        }

        public void ClearList()
        {
            user_list.Clear();
            user_count = 0;
        }

        public bool UserExists(User user)
        {
            for (int i = 0; i<user_list.Count;i++)
            {
                if (user.CompareUser(user_list[i]))
                    return true;
            }
            return false;
        }

        public User GetLastSocket()
        {
            return user_list.Last();
        }
        public void ShowAllUserInfo()
        {
            foreach (User user in user_list)
            {
                user.ShowUserInfo();
            }
        }

        public User GetTeamMate(User this_user)
        {
            foreach (User user in user_list)
            {
                if (user.get_equipo() == this_user.get_equipo() && user!=this_user) return user;
            }
            return null;
        }
        public User GetRival(User this_user)
        {

            foreach (User user in user_list)
            {
                if (user.get_equipo() != this_user.get_equipo()) return user;
            }
            return null;
        }
        public List<User> GetRivals(User this_user)
        {
            List<User> Rivals = new List<User>();
            foreach (User user in user_list)
            {
                if (user.get_equipo() != this_user.get_equipo()) Rivals.Add(user);
            }
            return Rivals;
        }

        public List<User> GetAllUsersExceptMe(User this_user)
        {
            List<User> AllUsersExceptMe = new List<User>();
            foreach (User user in user_list)
            {
                if (user != this_user) AllUsersExceptMe.Add(user);
            }
            return AllUsersExceptMe;
        }

        public int[] GenerateRandomTeam()
        {
            int[] randomchain = new int[2];
            randomchain[0] = UnityEngine.Random.Range(0, 20000);
            randomchain[1] = UnityEngine.Random.Range(0, 20000);
            while (randomchain[0] == randomchain[1])
            {
                randomchain[1] = UnityEngine.Random.Range(0, 20000);
            }
            return randomchain;
        }
        public User GetUserByName(string name)
        {
            foreach (User user in user_list)
            {
                if (user.get_username() == name) {
                    Debug.Log("Found");
                    user.ShowUserInfo();
                    return user; 
                }
            }

            return null;
        }

       
        
    }

    /// <summary>
    /// Clase usuario que almacena usuario, socket y equipo
    /// </summary>
    public class User
    {
        private string username;
        private int associated_socket;
        private int equipo;

        public void set_equipo(int equipo) { this.equipo = equipo; }
        public void set_username(string username) { this.username = username; }
        public void set_associated_socket(int socket) { this.associated_socket = socket; }
        public string get_username() { return this.username; }
        public int get_associated_socket() { return this.associated_socket; }
        public int get_equipo() { return this.equipo; }
        

        public User(string username, int associated_socket)
        {
            this.username = username;
            this.associated_socket = associated_socket;
        }
        public User(string username, int associated_socket, int equipo)
        {
            this.username = username;
            this.associated_socket = associated_socket;
            this.equipo = equipo;
        }

        public void ShowUserInfo()
        {
            Debug.Log("Username: " + username + " Associated socket: " + associated_socket.ToString() + " Associated team: " + equipo);
        }

        public bool CompareUser(User other)
        {
            return username == other.get_username() && associated_socket == other.get_associated_socket();
        }
        
    }   

    public void set_leader(bool lead) { this.leader = lead; }
    public void set_instantiated(bool instantiated) { this.instantiated = instantiated; }
    public void set_stop_playing(bool stop_playing) { this.stop_playing = stop_playing;}
    public bool get_instantiated() { return this.instantiated; }
    public void set_own_socket(int sock) { this.own_socket = sock; }
    public int get_own_socket() { return this.own_socket; }
    public string GetWins() { return trozos[1]; }
    public string GetTiempoMedio() { return trozos[1]; }
    public string GetMatches() { return trozos[1]; }
    public void UpdateAllUsers() { users = GetAllUsers(); }
    public void UpdateAllPlayedWith() { AllPlayedWith = GetAllUsers(); }
    public void UpdateAllConnected() { connected = GetAllConnected(); }
    public string GetLastPressedButton() { return last_pressed_button; }
    
    //Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
        start_playing = false;
        back_instance = Instantiate(back_prefab);
        leader = false;
        foreach (Transform child in invite_list.transform)
        {
            child.gameObject.AddComponent<Button>();
            child.gameObject.GetComponent<Button>().onClick.AddListener(() => { if (selected_users.Contains(child.gameObject.GetComponent<Text>().text)) { selected_users.Remove(child.gameObject.GetComponent<Text>().text); last_selected_invite--; child.gameObject.GetComponent<Text>().text = ""; modify_selected_invite_list(); } });
        }

        foreach (Transform child in lobby_list.transform)
        {
            child.gameObject.AddComponent<Button>();
            child.gameObject.GetComponent<Button>().onClick.AddListener(() => { if (lobby_users.Contains(child.gameObject.GetComponent<Text>().text)) { user_list.RemoveUser(user_list.GetUserByName(child.gameObject.GetComponent<Text>().text)); user_list.ShowAllUserInfo(); accepted_lobby_users.RemoveAt(lobby_users.IndexOf(child.gameObject.GetComponent<Text>().text)); lobby_users.Remove(child.gameObject.GetComponent<Text>().text); last_selected_lobby--; child.gameObject.GetComponent<Text>().text = ""; modify_selected_lobby_list();  } });
        }
        DefineLoginMenu();
        last_selected_invite = -1;
        last_selected_lobby = -1;
        instantiated = false;
        first_time = true;
        table = users_table.GetComponent<WriteTable>();
        ip = IPAddress.Parse(ippaddr);
        endpoint = new IPEndPoint(ip, port);
        disconnect.onClick.AddListener(() => Disconnect());
        wins.onClick.AddListener(delegate { last_pressed_button = "victory"; RequestWins(table.Get_Selected_user());  });
        average.onClick.AddListener(delegate { last_pressed_button = "average"; RequestAverage(table.Get_Selected_user());  });
        day_button.onClick.AddListener(delegate { last_pressed_button = "day"; RequestDay(day.text); });
        chat_button.onClick.AddListener(delegate { SendMessage(); });
        play.onClick.AddListener(delegate {
            SendGlobalStructure();
            LoadPlayScene();
            //start_playing = true;
            StartCoroutine(SetActive(SceneManager.GetSceneByName("GameScene")));
        });
        delete_user.onClick.AddListener(delegate { RequestUserDeletion(); });
        played_with_button.onClick.AddListener(delegate { RequestUsersIHavePlayedWith(); });
        invite_button.onClick.AddListener(delegate { last_pressed_button = "invite"; Invite(); });
    }
    
    /// <summary>
    /// Cargar la escena de juego.
    /// </summary>
    void LoadPlayScene() {
        string m_Scene = "GameScene";
        Destroy(back_instance);
        GameObject.Find("Canvas welcome").GetComponent<Canvas>().enabled  = false;
        GameObject.Find("Camera").GetComponent<AudioListener>().enabled = false; 
        GameObject.Find("Camera").GetComponent<Camera>().enabled = false; 
        GameObject.Find("EventSystemLogin").GetComponent<EventSystem>().enabled = false;
        SceneManager.LoadScene(m_Scene, LoadSceneMode.Additive);
        

    }
    public IEnumerator SetActive(Scene scene)
    {
        int i = 0;
        while (i == 0)
        {
            i++;
            yield return null;
        }
        SceneManager.SetActiveScene(scene);
        Debug.Log("Active Scene is: " + SceneManager.GetActiveScene().name);
        yield break;
    }
    /// <summary>
    /// Enviar estructura global de la partida (info de jugadores) al resto de jugadores que no sean líderes de la partida.
    /// </summary>
    private void SendGlobalStructure() {

        if (user_list.GetUserCount() == 4)
        {
            List<User> rivals = user_list.GetRivals(me);
            string mensaje = "30/" + user_list.GetUserCount().ToString() + "/" + own_username + "/" +  user_list.GetTeamMate(me).get_username() + "/" + user_list.GetTeamMate(me).get_associated_socket().ToString() + "/" + rivals.ElementAt(0).get_username() + "/" + rivals.ElementAt(0).get_associated_socket().ToString() + "/" + rivals.ElementAt(1).get_username() + "/" + rivals.ElementAt(1).get_associated_socket().ToString();
            byte[] msg = Encoding.ASCII.GetBytes(mensaje);
            servidor.Send(msg);
        }
        else
        {
            User rival = user_list.GetRival(me);
            string mensaje = "30/1/"+ own_username + "/" + rival.get_associated_socket().ToString();
            byte[] msg = Encoding.ASCII.GetBytes(mensaje);
            servidor.Send(msg);
        }
        
    }

    /// <summary>
    /// Establece información recibida del líder como estructura global.
    /// </summary>
    private void EstablishGlobalStucture()
    {
        if (trozos[1] == "1") {
            user_list.AddUser(new User(trozos[2], Convert.ToInt32(trozos[3]), 0));
        }        
        else
        {

            if (myTeam == 1) {
                user_list.AddUser(new User(trozos[2], Convert.ToInt32(trozos[3]), 1));
                user_list.AddUser(new User(trozos[4], Convert.ToInt32(trozos[5]), 0));
                user_list.AddUser(new User(trozos[6], Convert.ToInt32(trozos[7]), 0));
            }
            else
            {
                user_list.AddUser(new User(trozos[2], Convert.ToInt32(trozos[3]), 0));
                user_list.AddUser(new User(trozos[4], Convert.ToInt32(trozos[5]), 1));
                user_list.AddUser(new User(trozos[6], Convert.ToInt32(trozos[7]), 1));
            }
            
        }
    }

    /// <summary>
    /// Generador del formulario de login (se destruye y crea según conveniencia del usuario)
    /// </summary>
    private void DefineLoginMenu()
    {
        if (register_instance != null) Destroy(register_instance);
        login_instance = Instantiate(login_menu);
        username = GameObject.Find("Username").GetComponent<InputField>();
        password = GameObject.Find("Password").GetComponent<InputField>();
        register_instancer = GameObject.Find("Register text instance").GetComponent<Button>();
        login_instance.name = "Login Menu";
        login_instance.transform.SetParent(this.transform);
        login_instance.GetComponent<RectTransform>().localPosition = new Vector3(register_menu.GetComponent<RectTransform>().localPosition.x, register_menu.GetComponent<RectTransform>().localPosition.y, 0);
        login_instance.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        login = GameObject.Find("Login Button").GetComponent<Button>();
        login.onClick.AddListener(() => Login(username.text, password.text));
        register_instancer.onClick.AddListener(() => InstantiateRegisterMenu());
        GameObject.Find("Scroll up button").GetComponent<Button>().onClick.AddListener(() => StartCoroutine(SlideUpRoutine()));
    }
    //Update is called once per frame

    /// <summary>
    /// Invocaciones de funciones desde el thread secundario de AtenderCliente
    /// </summary>
    void Update()
    {


        if (last_selected_invite > 2) last_selected_invite = -1;
        if (last_selected_lobby > 3) last_selected_lobby = -1;

        //if (start_playing)
        //{
        //    SceneManager.SetActiveScene(SceneManager.GetSceneByName("GameScene"));
        //    start_playing= false;
        //}
        if (stop_playing == true)
        {
            StopPlaying();
            stop_playing = false;
        }
        if (establish_global_structure)
        {
            EstablishGlobalStucture();
            user_list.ShowAllUserInfo();
            LoadPlayScene();
            establish_global_structure = false;
        }
        if (show_invite)
        {
            ShowInvite();
            show_invite = false;
        }
        if (show_users_i_have_played_with)
        {
            UpdateAllPlayedWith();
            SendUsersIHavePlayedWith();
            StartCoroutine(SlideDownRoutine());
            show_users_i_have_played_with = false;
        }
        if (change_chat)
        {
            UpdateChatBox();
            change_chat = false;
        }
        if (show_wins)
        {
            ShowWins();
            show_wins = false;
        }
        if (show_users)
        {
            UpdateAllUsers();
            SendUsers();
            show_users = false;
        }
        if (close_server)
        {
            CloseServer();
            close_server = false;
        }

        if (show_connected)
        {
            UpdateAllConnected();
            SendConnected();
            SolicitarUsuarios();
            show_connected = false;
        }
        if (confirmation_invite)
        {
            ConfirmationInvite();
            confirmation_invite = false;
        }
        if (show_average)
        {
            ShowAverage();
            show_average = false;
        }
        if (show_day)
        {
            ShowDay();
            show_day = false;
        }

        if (instantiate_prefab)
        {
            spawn_control = GameObject.Find("Main Camera").GetComponent<Spawner>();
            spawn_control.InstantiatePrefab(Convert.ToInt32(trozos[1]),false);
            instantiate_prefab = false;
        }

        if (update_lobby_structure)
        {
            UpdateLobbyState();
            update_lobby_structure = false;
        }
    

    }
    /// <summary>
    /// Dejar de jugar la partida.
    /// </summary>
    private void StopPlaying()
    {
        
        back_instance = Instantiate(back_prefab);
        GameObject.Find("Canvas welcome").GetComponent<Canvas>().enabled = true;
        GameObject.Find("Image background").GetComponent<Image>().sprite = Resources.Load<Sprite>("GUI/menu_bg");
        GameObject.Find("Camera").GetComponent<AudioListener>().enabled = true;
        GameObject.Find("Camera").GetComponent<Camera>().enabled = true;
        GameObject.Find("EventSystemLogin").GetComponent<EventSystem>().enabled = true;
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());


    }


    /// <summary>
    /// Iniciar sesión (control de errores incluído).
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    void Login(string username, string password)
    {
        if (username == "" || password == "")
        {
            GameObject.Find("Error textbox").GetComponent<Text>().enabled = true;
            GameObject.Find("Error textbox").GetComponent<Text>().text = "Please, be sure that all data requested is filed";
        }
        else
        {
            servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            servidor.Connect(endpoint);
            string mensaje = "1/" + username + "/" + password;
            byte[] msg = Encoding.ASCII.GetBytes(mensaje);
            servidor.Send(msg);

            byte[] respuesta = new byte[80];
            servidor.Receive(respuesta);
            mensaje = Encoding.ASCII.GetString(respuesta).Split('\0')[0];
            if (mensaje == "Login")
            {
                own_password = password;
                own_username = username;
                Destroy(login_instance);
                chat_controller.set_username(username);
                Debug.Log("El login se ha efectuado con éxito");
                GetComponent<Canvas>().enabled = false;
                GameObject.Find("Image background").GetComponent<Image>().sprite = Resources.Load<Sprite>("GUI/menu_bg");
                GameObject.Find("Canvas welcome").GetComponent<Canvas>().enabled = true;
                GameObject.Find("Error textbox").GetComponent<Text>().enabled = false;
                try
                {
                    string conexio = "7/" + username;
                    msg = System.Text.Encoding.ASCII.GetBytes(conexio);
                    servidor.Send(msg);

                    System.Threading.ThreadStart ts = new System.Threading.ThreadStart(delegate { AtenderServidor(); });
                    new System.Threading.Thread(ts).Start();


                }
                catch (SocketException)
                {
                    Debug.Log("No he podido conectar con el servidor");
                    return;
                }

            }
            else
            {
                GameObject.Find("Error textbox").GetComponent<Text>().enabled = true;
                GameObject.Find("Error textbox").GetComponent<Text>().text = "Please, be sure that the data introduced is correct";
                Debug.Log("Asegúrese, por favor, de que los datos están correctamente introducidos.");

            }
        }
    }
    /// <summary>
    /// Registrarse (control de errores incluído).
    /// </summary>
    /// <param name="username"></param>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="birthday"></param>
    void Register(string username, string email, string password, string birthday)
    {
        if ((username == "") || (password == "") || (email == "") || (birthday == "")) 
        {
            GameObject.Find("Error textbox").GetComponent<Text>().enabled = true;
            GameObject.Find("Error textbox").GetComponent<Text>().text = "Please, be sure that the data introduced is filed.";
        }
        else
        {
            if (cumple_con_los_requisitos_fecha(birthday))
            {
                if (cumple_con_los_requisitors_correo(email))
                {
                    try
                    {
                        servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        servidor.Connect(endpoint);
                        string mensaje = "3/" + username + "/" + password + "/" + email + "/" + birthday;
                        byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                        servidor.Send(msg);

                        byte[] respuesta = new byte[80];
                        servidor.Receive(respuesta);
                        mensaje = Encoding.ASCII.GetString(respuesta).Split('\0')[0];
                        
                        if (mensaje == "Registro")
                        {
                            own_password = password;
                            own_username = username;
                            chat_controller.set_username(username);
                            Debug.Log("El registro se ha efectuado con éxito");
                            GetComponent<Canvas>().enabled = false;
                            GameObject.Find("Image background").GetComponent<Image>().sprite = Resources.Load<Sprite>("GUI/menu_bg");
                            GameObject.Find("Canvas welcome").GetComponent<Canvas>().enabled = true;
                            GameObject.Find("Error textbox").GetComponent<Text>().enabled = false;

                            try
                            {
                                string conexio = "7/" + username;
                                msg = System.Text.Encoding.ASCII.GetBytes(conexio);
                                servidor.Send(msg);

                                System.Threading.ThreadStart ts = new System.Threading.ThreadStart(delegate { AtenderServidor(); });
                                new System.Threading.Thread(ts).Start();
                                Destroy(register_instance);

                            }
                            catch (SocketException)
                            {
                                Debug.Log("No he podido conectar con el servidor");
                                return;
                            }
                        }
                        else if (mensaje == "Correo existente")
                        {
                            GameObject.Find("Error textbox").GetComponent<Text>().enabled = true;
                            GameObject.Find("Error textbox").GetComponent<Text>().text = "Introduced email already exists.";

                        }
                        else if (mensaje == "Usuario existente")
                        {
                            GameObject.Find("Error textbox").GetComponent<Text>().enabled = true;
                            GameObject.Find("Error textbox").GetComponent<Text>().text = "Introduced username already exists.";
                        }


                    }
                    catch (SocketException)
                    {
                        Debug.Log("Se ha producido un error al intentar conectar con el servidor."); ;
                    }
                }
                else
                {
                    GameObject.Find("Error textbox").GetComponent<Text>().enabled = true;
                    GameObject.Find("Error textbox").GetComponent<Text>().text = "Please, write email in a valid format.";
                }
            }

            else
            {
                GameObject.Find("Error textbox").GetComponent<Text>().enabled = true;
                GameObject.Find("Error textbox").GetComponent<Text>().text = "In order to register, you must be +18 years old and also be sure that the date is in the correct format (yyyy-mm-dd).";
            }
        }

    }
    /// <summary>
    /// Desconectar.
    /// </summary>
    void Disconnect()
    {
        
        string desconnexio = "0/";
        byte[] msg = Encoding.ASCII.GetBytes(desconnexio);
        servidor.Send(msg);

    }
    /// <summary>
    /// Solicitar usuarios de la base de datos.
    /// </summary>
    private void SolicitarUsuarios()
    {
        string conexio = "6/" + username.text;
        byte[] msg = System.Text.Encoding.ASCII.GetBytes(conexio);
        servidor.Send(msg);

    }

    /// <summary>
    /// Enviar mensaje por el chat.
    /// </summary>
    private void SendMessage()
    {
        if (!(message.text == ""))
        {
            string conexio = "10/" + username.text + "/" + message.text;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(conexio);
            servidor.Send(msg);
            message.text = "";
        }
        
    }

    /// <summary>
    /// Nuevo mensaje en el chat. Ver la notificación en la caja del chat.
    /// </summary>
    private void UpdateChatBox()
    {
        chat_controller.AddAuthor(trozos[1]);
        chat_controller.AddMessage(trozos[2]);
        chat_controller.set_update_chat(true);
    }

    /// <summary>
    /// Invitar jugador (control de errores incluído).
    /// </summary>
    private void Invite()
    {
        myTeam = 0;
        leader = true;
        if (!lobby_users.Contains(own_username))
        {
            lobby_users.Add(own_username);
            last_selected_lobby++;
            accepted_lobby_users.Add(true);
        }
        if ((selected_users.Count == 1 || selected_users.Count == 3) && CheckIfAllInviteListIsConnected()) {
            
            string conexio = "8/" + selected_users.Count.ToString() + "/" + own_username;

            foreach (string user in selected_users)
            {
                conexio += "/" + user;
            }
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(conexio);
            servidor.Send(msg);
            foreach (string user in selected_users)
            {
                if (!lobby_users.Contains(user)) {
                    lobby_users.Add(user);
                    last_selected_lobby++;
                    accepted_lobby_users.Add(false);
                }
                
            }
            modify_selected_lobby_list();
            selected_users.Clear();
            last_selected_invite = -1;
            modify_selected_invite_list();
        }
        else
        {
            CreateAlert();
        }
        
    }
    /// <summary>
    /// Comprobar si la lista de jugadores a invitar sigue conectada (importante para evitar invitar a un jugador desconectado).
    /// </summary>
    /// <returns></returns>
    public bool CheckIfAllInviteListIsConnected()
    {
        foreach (string user in selected_users) { 
            if (!connected.Contains(user)) { return false; }
        }
        return true;
    }

    /// <summary>
    /// Enviar respuesta a invitación de jugador (0 si aceptas, 1 si no).
    /// </summary>
    /// <param name="response"></param>
    /// <param name="user_socket"></param>
    /// <param name="username"></param>
    /// <param name="equipo"></param>
    public void SendResponseToInvite(string response, string user_socket, string username,string equipo)
    {
         
        string conexio = "9/" + response + "/" + user_socket + "/" + username  + "/" + equipo;
        byte[] msg = Encoding.ASCII.GetBytes(conexio);
        servidor.Send(msg);
    }

    /// <summary>
    /// Solicitar victorias de x jugador.
    /// </summary>
    /// <param name="username"></param>
    public void RequestWins(string username)
    {
        if (username == "")
        {
            CreateAlert();
        }
            
        else {

            string mensaje = "5/" + username;
            byte[] msg = Encoding.ASCII.GetBytes(mensaje);
            servidor.Send(msg);
        }
    }

    /// <summary>
    /// Solucionar tiempo promedio de juego en segundos de cada jugador.
    /// </summary>
    /// <param name="username"></param>
    public void RequestAverage(string username)
    {
        if (username == "")
        {
            CreateAlert();
        }

        else
        {
            string mensaje = "4/" + username;
            byte[] msg = Encoding.ASCII.GetBytes(mensaje);
            servidor.Send(msg);
        }
    }

    /// <summary>
    /// Solicitar partidas jugadas en x día.
    /// </summary>
    /// <param name="day"></param>
    private void RequestDay(string day)
    {
        if (day == "") { CreateAlert(); }
        else
        {
            string[] partes = day.Split('-');
            if ((Convert.ToInt32(partes[1]) > 12) || (Convert.ToInt32(partes[1]) < 1))
                CreateAlert();
            else if ((Convert.ToInt32(partes[2]) > 31) || (Convert.ToInt32(partes[2]) < 1))
                CreateAlert();
            else
            {
                string daytoadd;
                daytoadd = Convert.ToString(Convert.ToInt32(partes[2]) + 1);
                if (Convert.ToInt32(daytoadd) < 10)
                    daytoadd = "0" + daytoadd;
                String diadespues = partes[0] + "-" + partes[1] + "-" + daytoadd;
                string mensaje = "2/" + day + "/" + diadespues;
                byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                servidor.Send(msg);            
            }
        }
    }
    /// <summary>
    /// Método invocado al salir de la aplicación.
    /// </summary>
    void OnApplicationQuit()
    {
        if (conectado)
        {
            Disconnect();
            CloseServer();
            conectado = false;
        }
        
    }

    /// <summary>
    /// Solicitar borrado de usuario.
    /// </summary>
    private void RequestUserDeletion()
    {
        string desconnexio = "12/" + own_username + "/" + own_password;
        byte[] msg = Encoding.ASCII.GetBytes(desconnexio);
        servidor.Send(msg);

    }

    /// <summary>
    /// Solicitar jugadores con los que he jugado.
    /// </summary>
    private void RequestUsersIHavePlayedWith()
    {
        string mensaje = "13/" + own_username;
        byte[] msg = Encoding.ASCII.GetBytes(mensaje);
        servidor.Send(msg);
    }

    /// <summary>
    /// Thread para atender el servidor (estructura parecida a la dada en clase, con algún modificación y adaptación a Unity).
    /// </summary>
    private void AtenderServidor()
    {

        conectado = true;
        while (conectado)
        {

            byte[] respuesta = new byte[512];
            servidor.Receive(respuesta);
            trozos = Encoding.ASCII.GetString(respuesta).Split('\0');
            Debug.Log(trozos[0]);
            trozos = trozos[0].Split('/');
            int codigo = Convert.ToInt32(trozos[0]);

            switch (codigo)
            {

                case 0:
                    Debug.Log("Desconexión realizada con exito");
                    conectado = false;
                    close_server = true;
                    break;
                case 2:
                    show_day = true;
                    break;
                case 4:
                    show_average = true;
                    break;
                case 5:
                    show_wins = true;
                    break;
                case 6:
                    show_users = true;
                    break;
                case 7:
                    show_connected = true;
                    break;
                case 8:
                    show_invite = true;
                    break;
                case 9:
                    confirmation_invite = true;
                    break;
                case 10:
                    change_chat = true;
                    break;
                case 11:
                    instantiate_prefab = true;
                    break;
                case 13:
                    show_users_i_have_played_with = true;
                    break;
                case 20:
                    update_lobby_structure = true;
                    break;
                case 30:
                    establish_global_structure = true;
                    break;

            }

        }
    }

    /// <summary>
    /// Mostrar victorias en MessageBox.
    /// </summary>
    private void ShowWins()
    {
        
        if (!(instantiated))
        {
            instance = Instantiate(user_info, transform.position, Quaternion.identity);
            instance.GetComponent<InfoWriter>().set_show("victory");
            instance.GetComponent<InfoWriter>().set_selected_user(table.Get_Selected_user());
            instance.GetComponent<InfoWriter>().set_victories(GetWins());
            instantiated = true;
        }
        else 
        {
            instance.GetComponent<InfoWriter>().set_show("victory");
            instance.GetComponent<InfoWriter>().set_selected_user(table.Get_Selected_user());
            instance.GetComponent<InfoWriter>().set_victories(GetWins());
        }
    }

    /// <summary>
    /// Mostrar que un usuario ha confirmado invitación (aparecerá verde en el lobby).
    /// </summary>

    private void ConfirmationInvite()
    {
        if (me == null) {
            me = new User(own_username, own_socket);
            me.set_equipo(0);
            user_list.AddUser(me);
        }
        
        if (trozos[1] == "0") {

            User user = new User(trozos[3], Convert.ToInt32(trozos[2]));
            
            user.set_equipo(Convert.ToInt32(trozos[4]));
            user_list.AddUser(user);
            accepted_lobby_users[FindIndexStr(lobby_users, trozos[3])] = true;
            
            
        }
        else if (trozos[1] == "1") {
            Debug.Log(trozos[2]);
            accepted_lobby_users.RemoveAt(FindIndexStr(lobby_users,trozos[2]));
            lobby_users.Remove(trozos[2]);
            last_selected_lobby--;
        }

        //foreach (User user in user_list.GetAllUsersExceptMe(me))
        //{
        //    Debug.Log("Usuario envio");
        //    SendLobbyState(user.get_associated_socket());
        //}
        modify_selected_lobby_list();
        user_list.ShowAllUserInfo();
    }
    /// <summary>
    /// Función que permite mostrar el promedio de tiempo de duración de las partidas de cada jugador (desde que inicia la partida hasta que una de las 2 torres es destruída).
    /// </summary>
    private void ShowAverage()
    {
        
        if (!(instantiated))
        {
            instance = Instantiate(user_info, transform.position, Quaternion.identity);
            instance.GetComponent<InfoWriter>().set_show("average");
            instance.GetComponent<InfoWriter>().set_selected_user(table.Get_Selected_user());
            instance.GetComponent<InfoWriter>().set_time(GetTiempoMedio());
            instantiated = true;
        }
        else
        {
            instance.GetComponent<InfoWriter>().set_show("average");
            instance.GetComponent<InfoWriter>().set_selected_user(table.Get_Selected_user());
            instance.GetComponent<InfoWriter>().set_time(GetTiempoMedio());
        }
    }
    /// <summary>
    /// Función que muestra las invitaciones de chat provenientes de un jugador concreto.
    /// </summary>
    private void ShowInvite()
    {
        GameObject invite_instance = Instantiate(invite_box);
        invite_instance.transform.SetParent(GameObject.Find("Canvas welcome").GetComponent<RectTransform>());
        invite_instance.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        invite_instance.GetComponent<RectTransform>().localPosition = new Vector3(-625f, -423f, 25f);
        invite_instance.GetComponent<Invite>().set_own_socket(trozos[1]);
        invite_instance.GetComponent<Invite>().set_origin_user(trozos[2]);
        invite_instance.GetComponent<Invite>().set_own_equipo(trozos[3]);
        user_list.AddUser(new User(trozos[2], Convert.ToInt32(trozos[1])));
        myTeam = Convert.ToInt32(trozos[3]);
        Debug.Log(user_list.GetUserCount());
        user_list.GetLastSocket().ShowUserInfo();
    }
    /// <summary>
    /// Función que permite mostrar las partidas que se jugaron un día concreto.
    /// </summary>
    private void ShowDay()
    {
        if (!(instantiated))
        {
            instance = Instantiate(user_info, transform.position, Quaternion.identity);
            instance.GetComponent<InfoWriter>().set_show("day");
            instance.GetComponent<InfoWriter>().set_selected_day(day.text);
            instance.GetComponent<InfoWriter>().set_matches(GetMatches());
            instantiated = true;
        }
        else
        {
            instance.GetComponent<InfoWriter>().set_show("day");
            instance.GetComponent<InfoWriter>().set_selected_day(day.text);
            instance.GetComponent<InfoWriter>().set_matches(GetMatches());
        }

    }    
    /// <summary>
    /// Función que habilita crear un nuevo Message Box dinámico (se actualiza sin necesidad de cerrarlo, con solo cambiar de usuario en el panel de selección) para mostrar datos referentes a las funciones establecidas a principio de curso por los miembros de este grupo. Se ha definido porque MessageBox es un concepto de Windows Forms.
    /// </summary>
    private void CreateAlert()
    {
        if (!(instantiated))
        {
            instance = Instantiate(user_info, transform.position, Quaternion.identity);
            instance.GetComponent<InfoWriter>().set_show("alert");
            instantiated = true;
        }

        else
        {
            instance.GetComponent<InfoWriter>().set_show("alert");
        }
    }

    /// <summary>
    /// Función que cierra el servidor y devuelve el jugador a la pantalla de inicio.
    /// </summary>
    private void CloseServer()
    {
        servidor.Shutdown(SocketShutdown.Both);
        servidor.Close();
        user_list.ClearList();
        own_password = "";
        own_username = "";
        lobby_users.Clear();
        accepted_lobby_users.Clear();
        selected_users.Clear();
        user_list.ClearList();
        last_selected_lobby = -1;
        last_selected_invite = -1;
        modify_selected_invite_list();
        modify_selected_lobby_list();
        DefineLoginMenu();
        GameObject.Find("Image background").GetComponent<Image>().sprite = Resources.Load<Sprite>("GUI/Battle_Cats_Main_Screen");
        table.set_selected_user("");
        GetComponent<Canvas>().enabled = true;
        canvas_welcome.enabled = false;
        
    }
    /// <summary>
    /// Función que devuelve un objecto de tipo lista de todos los usuarios de la base de datos existentes, a partir de la respuesta del servidor.
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllUsers()
    {
        List<string> users_list = new List<string> { };
        int num = Convert.ToInt32(Duplicate(trozos[1]));
        string trozo;
        int k = 2;
        for (int i = 0; i < num; i++)
        {
            trozo = Duplicate(trozos[k]);
            users_list.Add(trozo);
            k++;
        }
        return users_list;
    }
    /// <summary>
    /// Función que devuelve un objeto de tipo lista de todos los usuarios conectados actualmente, a partir de la respuesta del servidor.
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllConnected()
    {
        List<string> connected_list = new List<string> { };
        int num = Convert.ToInt32(Duplicate(trozos[1]));
        string trozo;
        int k = 2;
        for (int i = 0; i < num; i++)
        {
            trozo = Duplicate(trozos[k]);
            connected_list.Add(trozo);
            k++;
        }
        return connected_list;
    }
    /// <summary>
    /// Función que envía todos los usuarios de la base de datos a un "objeto" definido como WriteTable que gestiona cómo se muestran los usuarios en una Scrollview.
    /// </summary>
    public void SendUsers()
    {
        users_table.users_to_show_update(users);
        table.increase_change_log();
        table.set_change(true);
        first_time = true;

    }
    public void SendUsersIHavePlayedWith()
    {
        users_played_with_table.users_to_show_update(AllPlayedWith);
        users_played_with_table.increase_change_log();
        users_played_with_table.set_change(true);
        first_time = true;

    }
    /// <summary>
    /// Función que envía los usuarios conectados a un "objeto" definido como WriteTable que gestiona cómo se muestran los usuarios en una Scrollview.
    /// </summary>
    public void SendConnected()
    {

        users_table.set_users_connected(connected);
        users_table.users_to_show_update(users);
        if (!(first_time)) { table.increase_change_log(); }
        table.set_change(true);
    }
    /// <summary>
    /// Función para duplicar strings (desarrollada para evitar ciertos problemas que plantea Unity).
    /// </summary>
    public string Duplicate(string s)
    {
        string copy = "";
        foreach (char c in s)
        {
            copy += c;
        }
        return copy;
    }
    /// <summary>
    /// Función para crear una instancia del menú de registro (lo invoca en la pantalla) a partir de un prefabricado creado previamente por el grupo.
    /// </summary>
    public void InstantiateRegisterMenu()
    {
        
        Destroy(login_instance);
        register_instance = Instantiate(register_menu);
        username = GameObject.Find("Username register").GetComponent<InputField>();
        birthday = GameObject.Find("Birthdate").GetComponent<InputField>();
        email = GameObject.Find("Email").GetComponent<InputField>();
        password = GameObject.Find("Password register").GetComponent<InputField>();
        register_instance.name = "Register menu";
        register_instance.transform.SetParent(this.transform);
        register_instance.GetComponent<RectTransform>().localPosition = new Vector3(register_menu.GetComponent<RectTransform>().localPosition.x, register_menu.GetComponent<RectTransform>().localPosition.y, 0);
        register_instance.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
        register_button = GameObject.Find("Register Button").GetComponent<Button>();
        login_instancer = GameObject.Find("Login instancer").GetComponent<Button>();
        register_button.onClick.AddListener(() => Register(username.text,email.text,password.text,birthday.text));
        login_instancer.onClick.AddListener(() => DefineLoginMenu());

    }
    /// <summary>
    /// Función para determinar que la fecha está en el formato correcto (aaaa-mm-dd).
    /// </summary>
    /// <param name="fecha"></param>
    /// <returns>Devuelve true si lo cumple, false si no lo cumple.</returns>
    public bool cumple_con_los_requisitos_fecha(string fecha)
    {
        string[] partes = fecha.Split('-');
        if (partes.Length > 1)
        {
            if ((Convert.ToInt32(partes[1]) > 12) || (Convert.ToInt32(partes[1]) < 1))
                return false;
            else if ((Convert.ToInt32(partes[2]) > 31) || (Convert.ToInt32(partes[2]) < 1))
                return false;
            else
            {
                DateTime today = DateTime.Today;
                String[] arrayfecha = fecha.Split('-');
                DateTime fecha_d = new DateTime(Convert.ToInt32(arrayfecha[0]), Convert.ToInt32(arrayfecha[1]), Convert.ToInt32(arrayfecha[2]), 0, 0, 0);

                TimeSpan span = (fecha_d - today);

                if (Convert.ToInt32(span.Days) / 365 <= -18)
                    return true;
                else
                    return false;
            }
        }
        return false;
    }
    /// <summary>
    /// Función que obtiene un índice entero referente a la posición de un carácter en una cadena de texto.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="str"></param>
    /// <returns></returns>
    public int getCharPos(char c,string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == c) return i;
        }
        return - 1;
    }
    /// <summary>
    /// Función que determina si un correo cumple con los requisitos o no en el formulario de registro. Que siga un formato ejemplo@proveedor.dominio. 
    /// </summary>
    /// <param name="correo"></param>
    /// <returns>Devuelve true si lo cumple, false si no lo cumple.</returns>
    public bool cumple_con_los_requisitors_correo(string correo)
    {
        int count = 0;
        if (getCharPos('@', correo) > -1)
        {
            for (int i = getCharPos('@', correo) + 1; i < correo.Length; i++) 
            {
                if (correo[i] == '.' || correo[i] == '@')
                {
                    count++;
                } 
            }
        }
        if (count == 1) { return true; }
        return false;
    } 

    /// <summary>
    /// Función que modifica la lista del lobby.
    /// </summary>
    public void modify_selected_lobby_list()
    {
        int count = 0;
        foreach (Transform child in lobby_list.transform)
        {
            if (count <= last_selected_lobby)
            {
                child.gameObject.GetComponent<Text>().text = lobby_users[count];
                if (accepted_lobby_users[FindIndexStr(lobby_users,lobby_users[count])])
                    child.gameObject.GetComponent<Text>().color = UnityEngine.Color.green;
                else
                    child.gameObject.GetComponent<Text>().color = UnityEngine.Color.red;

            }
            else
            {
                child.gameObject.GetComponent<Text>().text = "";
            }
            count++;
        }
    }

    /// <summary>
    /// Función que modifica la lista de usuarios seleccionados para invitar.
    /// </summary>
    public void modify_selected_invite_list() {
        int count = 0;
        foreach (Transform child in invite_list.transform)
        { 
            if (count<=last_selected_invite) {
                child.gameObject.GetComponent<Text>().text = selected_users[count];
                
            }
            else
            {
                child.gameObject.GetComponent<Text>().text = "";
            }
            count++;
        }
       
    }
    /// <summary>
    /// Encuentra el índice de un string en una lista (función de debuggeo y prueba)
    /// </summary>
    /// <param name="list"></param>
    /// <param name="element"></param>
    /// <returns></returns>
    public int FindIndexStr(List<string> list,string element)
    {
        int index = 0;
        foreach(string s in list)
        {
            if (s == element) 
                return index;
            index++;
        }
        return -1;
    }
    /// <summary>
    /// Función para que baje la lista de jugadores con los que he jugado.
    /// </summary>
    /// <returns></returns>
    IEnumerator SlideDownRoutine()
    {
        float i = 0;
        while ( i < 1 && GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition.y >=266)
        {
            
            GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition = new Vector3(GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition.x, GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition.y - 800*Time.deltaTime, 0);
            i += Time.deltaTime;
            yield return null;
        }
        GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition = new Vector3(GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition.x, 266, 0);

    }
    /// <summary>
    /// Función para que suba la lista de jugadores con los que he jugado.
    /// </summary>
    /// <returns></returns>
    IEnumerator SlideUpRoutine() {
        float i = 0;
        while (i < 1 && GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition.y <= 962)
        {

            GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition = new Vector3(GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition.x, GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition.y + 800*Time.deltaTime, 0);
            i += Time.deltaTime;
            yield return null;
        }
        GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition = new Vector3(GameObject.Find("Users_played_with_table").GetComponent<RectTransform>().localPosition.x, 962, 0);
    }

    public void SendLobbyState(int n)
    {
        string mensaje = "";
        int count = 0;
        foreach (string s in lobby_users)
        {
            mensaje += "/" + s;
            if (accepted_lobby_users[FindIndexStr(lobby_users, lobby_users[count])])
                mensaje += "/1";
            else
                mensaje += "/0";
            count++;
        }
        
        mensaje = "20/" + count.ToString() + mensaje + ";" + n.ToString();
        byte[] msg = Encoding.ASCII.GetBytes(mensaje);
        servidor.Send(msg);
    }

    public void UpdateLobbyState()
    {
        int usuarios = Convert.ToInt32(trozos[1]);
        lobby_users.Clear();
        accepted_lobby_users.Clear();
        last_selected_lobby = -1;
        int k = 2;
        for(int i = 0; i<usuarios; i++)
        {
            lobby_users.Add(trozos[k]);
            if (trozos[k + 1] == "1")
                accepted_lobby_users.Add(true);
            else
                accepted_lobby_users.Add(false);
            last_selected_lobby++;
            k = k + 2;
        }

        modify_selected_lobby_list();
    }
    
}    