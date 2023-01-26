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

public class WriteChat : MonoBehaviour
{
    private bool update_chat;
    public List<string> messages;
    public List<string> author;
    public int last_chat;
    public string username;  
    public void set_update_chat(bool update_chat) { this.update_chat = update_chat; }
    public void set_username(string username) { this.username = username;}
    public void AddAuthor(string author) { this.author.Add(author);}
    public void AddMessage(string message) { this.messages.Add(message);}
    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
        last_chat = 0;
    }
    // Update is called once per frame

    /// <summary>
    /// Gestión del estado del chat, con la información disponible.
    /// </summary>
    void Update()
    {
        if (update_chat) {
            GameObject author_message = new GameObject(author[last_chat]);
            GameObject message = new GameObject(messages[last_chat]);
            author_message.transform.SetParent(this.transform);
            message.transform.SetParent(this.transform);
            Text message_text = message.AddComponent<Text>();
            Text author_message_text = author_message.AddComponent<Text>();
            author_message_text.text = author[last_chat];
            author_message_text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            author_message_text.fontSize = 30;
            
            message_text.text = messages[last_chat];
            message_text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            message_text.fontSize = 25;
            
            message.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            message.GetComponent<RectTransform>().localPosition = new Vector3(GetComponent<RectTransform>().localPosition.x, GetComponent<RectTransform>().localPosition.y, 26);
            author_message.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            author_message.GetComponent<RectTransform>().localPosition = new Vector3(GetComponent<RectTransform>().localPosition.x, GetComponent<RectTransform>().localPosition.y, 26);

            if (author[last_chat] == username)
            {
                author_message_text.alignment = TextAnchor.MiddleRight;
                message_text.alignment = TextAnchor.MiddleRight;
                message.GetComponent<RectTransform>().sizeDelta = new Vector2(650, 50);
                author_message.GetComponent<RectTransform>().sizeDelta = new Vector2(650, 50);
            }


            else
            {
                author_message_text.alignment = TextAnchor.MiddleLeft;
                message_text.alignment = TextAnchor.MiddleLeft;
                message.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 50);
                author_message.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 50);
                message_text.text = "   " + messages[last_chat];
                author_message_text.text = "   " + author[last_chat];
            }
            update_chat = false;
            last_chat++;
            
        }
    }
}
