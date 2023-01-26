using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InfoWriter : MonoBehaviour
{
    private string selected_user;
    private string show;
    private string selected_day;
    private string victories;
    private string matches;
    private string time;
    private Text text_writer;
    private Button close_button;
    public void set_selected_user(string user) { selected_user = user; }
    public void set_show(string show) { this.show = show; }
    public void set_selected_day(string day) { selected_day = day; }
    public void set_time(string time) { this.time = time; }
    public void set_victories(string victories) { this.victories = victories; }
    public void set_matches(string matches) { this.matches = matches; } 
    public string get_selected_user() { return selected_user; }
    public string get_show() { return show; }
    public string get_selected_day() { return selected_day; }
    public string get_victories() { return victories; }
    public string get_matches(string matches) { return matches; }
    public string get_time() { return time; }
    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
        transform.SetParent(GameObject.Find("Canvas welcome").GetComponent<Transform>());
        text_writer = GameObject.Find("User info text").GetComponent<Text>();
        close_button = GameObject.Find("Close").GetComponent<Button>();
        close_button.onClick.AddListener(delegate { Close(); });
    }
    // Update is called once per frame

    /// <summary>
    /// Detección del último botón pulsado para actualizar dinámicamente nuestro MessageBox.
    /// </summary>
    void Update()
    {
        switch (show)
        {
            case "day":
                text_writer.text = matches + " matches played on " + selected_day + ".";
                break;
            case "victory":
                text_writer.text = victories + " matches won by " + selected_user + ".";
                break;
            case "average":
                text_writer.text = selected_user + " has played " + time + " s on average.";
                break;
            case "alert":
                switch (GameObject.Find("Canvas UI Login").GetComponent<LoginScript>().GetLastPressedButton())
                {
                    case "victory":
                        text_writer.text = "Please, select a user to show its info.";
                        break;
                    case "average":
                        text_writer.text = "Please, select a user to show its info.";
                        break;
                    case "day":
                        text_writer.text = "Please, be sure that date is in correct format (yyyy-mm-dd).";
                        break;
                    case "invite":
                        text_writer.text = "Please, be sure that only 1 or 3 players are selected and that every selected player is connected.";
                        break;
                }
                break;
        }
    }

    /// <summary>
    /// Cerrar MessageBox.
    /// </summary>
    private void Close() {
        GameObject.Find("Canvas UI Login").GetComponent<LoginScript>().set_instantiated(false);
        GameObject.Find("Users table").GetComponent<WriteTable>().reinitialize_selected_user();
        Destroy(this.gameObject);
    }
}