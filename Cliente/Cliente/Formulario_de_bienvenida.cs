using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Cliente
{
    public partial class Formulario_de_bienvenida : Form
    {
        Socket servidor;
        string nombre_usuario;
        IPAddress ip;
        int port;
        IPEndPoint endpoint;
        string selected_user;
        public Formulario_de_bienvenida()
        {
            InitializeComponent();
        }



        public void set_ip(IPAddress ipadd)
        {
            ip = ipadd;
        }
        public void set_port(int port) {
            this.port = port;
        }

        public void set_nombre_usuario(string usuario)
        {
            nombre_usuario = usuario;
            texto_bienvenida.Text = "Bienvenido/a " + usuario;
        }
        private void Formulario_de_bienvenida_Load(object sender, EventArgs e)
        {

            endpoint = new IPEndPoint(ip, port);
            servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                servidor.Connect(endpoint);
                string mensaje = "6/ usuarios";
                byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                servidor.Send(msg);

                byte[] respuesta = new byte[80];
                servidor.Receive(respuesta);
                mensaje = Encoding.ASCII.GetString(respuesta).Split('\0')[0];

                string[] usuarios = mensaje.Split('-');
                usuarios_grid.RowCount = usuarios.Length;
                usuarios_grid.ColumnCount = 1;
                for (int i = 0; i < usuarios.Length; i++)
                {
                    if (usuarios[i] != "-")
                        usuarios_grid.Rows[i].Cells[0].Value = usuarios[i];
                }
                servidor.Close();
            }
            catch (SocketException)
            {
                MessageBox.Show("Se ha producido un error al intentar conectar con el servidor."); ;
            }


        }

        private void consultar_partidas_dia_Click(object sender, EventArgs e)
        {

            if (dia_textbox.Text == "")
                MessageBox.Show("Por favor, introduzca un día válido");
            else
            {

                string[] partes = dia_textbox.Text.Split('-');
                if ((Convert.ToInt32(partes[1]) > 12) || (Convert.ToInt32(partes[1]) < 1))
                    MessageBox.Show("Por favor, introduzca un mes válido");
                else if ((Convert.ToInt32(partes[2]) > 31) || (Convert.ToInt32(partes[2]) < 1))
                    MessageBox.Show("Por favor, introduzca un día válido");
                else
                {
                    try {
                        servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        servidor.Connect(endpoint);
                        String daytoadd;
                        daytoadd = Convert.ToString(Convert.ToInt32(partes[2]) + 1);
                        if (Convert.ToInt32(daytoadd) < 10)
                            daytoadd = "0" + daytoadd;
                        String diadespues = partes[0] + "-" + partes[1] + "-" + daytoadd;
                        string mensaje = "2/" + dia_textbox.Text + "/" + diadespues;
                        byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                        servidor.Send(msg);

                        byte[] respuesta = new byte[80];
                        servidor.Receive(respuesta);
                        mensaje = Encoding.ASCII.GetString(respuesta).Split('\0')[0];
                        MessageBox.Show("El número total de partidas jugadas en el " + dia_textbox.Text + " es igual a " + mensaje);
                        servidor.Close();

                    }
                    catch (SocketException)
                    {
                        MessageBox.Show("Se ha producido un error al intentar conectar con el servidor."); ;
                    }
                }

            }
        }

        private void texto_bienvenida_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void victorias_button_Click(object sender, EventArgs e)
        {
            selected_user = (string)usuarios_grid.CurrentCell.Value;
            selected_user = (string)usuarios_grid.CurrentCell.Value;
            try
            {
                servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                servidor.Connect(endpoint);
                string mensaje = "5/" + selected_user;
                byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                servidor.Send(msg);
                byte[] respuesta = new byte[80];
                servidor.Receive(respuesta);
                mensaje = Encoding.ASCII.GetString(respuesta).Split('\0')[0];
                MessageBox.Show("El número total de victorias de " + selected_user + " es igual a " + mensaje);
                servidor.Close();

            }
            catch (SocketException)
            {
                MessageBox.Show("Se ha producido un error al intentar conectar con el servidor."); ;
            }
        }


            private void tiempo_medio_button_Click(object sender, EventArgs e)
            {
                selected_user = (string)usuarios_grid.CurrentCell.Value;
                try
                {
                    servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    servidor.Connect(endpoint);
                    string mensaje = "4/" + selected_user;
                    byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                    servidor.Send(msg);
                    byte[] respuesta = new byte[80];
                    servidor.Receive(respuesta);
                    mensaje = Encoding.ASCII.GetString(respuesta).Split('\0')[0];
                    MessageBox.Show("El tiempo medio de partidas de " + selected_user + " es igual a " + mensaje);
                    servidor.Close();

                }
                catch (SocketException)
                {
                    MessageBox.Show("Se ha producido un error al intentar conectar con el servidor."); ;
                }
            }
        }
    }
