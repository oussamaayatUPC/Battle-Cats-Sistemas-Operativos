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

        public void set_socket(Socket s)
        {
            servidor = s;
        }

        public void set_nombre_usuario(string usuario)
        {
            nombre_usuario = usuario;
            texto_bienvenida.Text = "Bienvenido/a " + usuario;
        }

        public void cargar()
        {
            //endpoint = new IPEndPoint(ip, port);
            //servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                //servidor.Connect(endpoint);
                string mensaje = "6/ usuarios";
                byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                servidor.Send(msg);

                //byte[] respuesta = new byte[80];
                //servidor.Receive(respuesta);
                //mensaje = Encoding.ASCII.GetString(respuesta).Split('\0')[0];


            }
            catch (SocketException)
            {
                MessageBox.Show("Se ha producido un error al intentar conectar con el servidor.");
            }
        }

        private void Formulario_de_bienvenida_Load(object sender, EventArgs e)
        {

            


        }

        private void AtenderServidor()
        {
            Boolean conectado = true;
            while (conectado)

            {

                byte[] respuesta = new byte[80];
                servidor.Receive(respuesta);
                string[] trozos = Encoding.ASCII.GetString(respuesta).Split('\0');

                trozos = trozos[0].Split('/');
                MessageBox.Show(trozos[0]);
                int codigo = Convert.ToInt32(trozos[0]);
                
                
                switch (codigo)
                {

                    case 0:
                        MessageBox.Show("Desconexion realizada con exito");
                        Invoke(new Action(() => {
                            this.BackColor = Color.Gray;
                            servidor.Shutdown(SocketShutdown.Both);
                            servidor.Close(); 
                        }
                        ));
                        conectado = false;

                        break;
                    //consultar partides del dia
                    case 2:
                       
                        MessageBox.Show("El número total de partidas jugadas en el " + dia_textbox.Text + " es igual a " + trozos[1]);
                        break;


                    case 4:
                        
                        MessageBox.Show("El tiempo medio de partidas de " + selected_user + " es igual a " + trozos[1]);
                        break;


                    case 5:
                        
                        MessageBox.Show("El número total de victorias de " + selected_user + " es igual a " + trozos[1]);
                        break;

                    case 6:
                        usuarios_grid.Invoke(new Action(() =>
                        {
                            usuarios_grid.RowCount = trozos.Length;
                            usuarios_grid.ColumnCount = 1;
                            for (int i = 1; i < trozos.Length; i++)
                            {
                                usuarios_grid.Rows[i].Cells[0].Value = trozos[i];
                            }
                        }));
                        break;

                    case 7:

                        

                        
                        Conectados_Grid_View.Invoke(new Action(() =>
                        {
                            int i = 2;
                            int j = 0;

                            Conectados_Grid_View.RowCount = Convert.ToInt32(trozos[1]);
                            Conectados_Grid_View.ColumnCount = 1;
                            
                            for (j = 0; j < Convert.ToInt32(trozos[1]); j++)
                            {
                               
                                Conectados_Grid_View.Rows[j].Cells[0].Value = trozos[i];
                                i++;
                            }
                        }));
                        
                        
                                             
                       
                        
                        break;
                }
               
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
                    //try
                    {
                        //servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        //servidor.Connect(endpoint);
                        String daytoadd;
                        daytoadd = Convert.ToString(Convert.ToInt32(partes[2]) + 1);
                        if (Convert.ToInt32(daytoadd) < 10)
                            daytoadd = "0" + daytoadd;
                        String diadespues = partes[0] + "-" + partes[1] + "-" + daytoadd;
                        string mensaje = "2/" + dia_textbox.Text + "/" + diadespues;
                        byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                        servidor.Send(msg);

                        
                       
                       

                    }
                    //catch (SocketException)
                    {
                        //MessageBox.Show("Se ha producido un error al intentar conectar con el servidor."); ;
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
            //try
            {
                //servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //servidor.Connect(endpoint);
                string mensaje = "5/" + selected_user;
                byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                servidor.Send(msg);
                
                
               
                

            }
            //catch (SocketException)
            {
                //MessageBox.Show("Se ha producido un error al intentar conectar con el servidor."); ;
            }
        }


            private void tiempo_medio_button_Click(object sender, EventArgs e)
            {
                selected_user = (string)usuarios_grid.CurrentCell.Value;
                //try
                {
                    //servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //servidor.Connect(endpoint);
                    //string mensaje = "4/" + selected_user;
                    //byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                    //servidor.Send(msg);
                    //System.Threading.ThreadStart ts = new System.Threading.ThreadStart(delegate { AtenderServidor(); });
                    //new System.Threading.Thread(ts).Start();




            }
                //catch (SocketException)
                {
                    //MessageBox.Show("Se ha producido un error al intentar conectar con el servidor."); ;
                }
            }

        

        private void Conectar_Button_Click(object sender, EventArgs e)
        {
            
            //endpoint = new IPEndPoint(ip, port); //igual que al servidor


            //Creamos el socket 
            //servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                //servidor.Connect(endpoint);//Intentamos conectar el socket
                this.BackColor = Color.Red;
                string conexio = "7/" + nombre_usuario;
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(conexio);
                servidor.Send(msg);
            }
            catch (SocketException)
            {

                //Si hay excepcion imprimimos error y salimos del programa con return 
                MessageBox.Show("No he podido conectar con el servidor");
                return;
            }

            System.Threading.ThreadStart ts = new System.Threading.ThreadStart(delegate { AtenderServidor(); });
            new System.Threading.Thread(ts).Start();
            //AtenderServidor();
        }

        private void Desconectar_Button_Click(object sender, EventArgs e)
        {
            string desconnexio = "0/";
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(desconnexio);
            servidor.Send(msg);

            // Se terminó el servicio. 
            // Nos desconectamos
            
        }

        private void Conectados_Grid_View_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
   }
