#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdio.h>
#include <pthread.h>
#include <mysql.h>
#include <my_global.h>
#define port 50055
#define id_max_length 10
#define email_max_length 50
#define username_max_length 20
#define password_max_length 20
#define max_users 100
#define email_min_length 4
#define FechaYHora_length 200
#define username_min_length 4
#define password_min_length 4
#define sql_query_max_length 2048
#define read_buffer_length 512
#define write_buffer_length 512
#define database_name "TG2_BC_BBDD"
#define database_username "root"
#define database_password "mysql"
#define database_host "shiva2.upc.es"
typedef struct
{
	int socket; // User's socket
	
	char username[username_max_length]; //Username
	
} User;

typedef struct
{
	User usuario[max_users];
	int num;
}UserList;

UserList llista;
pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
int socket_num;
int sockets[2000];
pthread_t threads [100];

//Devuleve -1 si hay espacio en la lista para a￱adir un nuevo uesuario y devuelve 0 si el usuario se ha a￱adido a la lista.
int agregar_usuari (UserList *l, char name[20], int socket)
{
	pthread_mutex_lock(&mutex);
	if (l->num == 100)
	{
		pthread_mutex_unlock(&mutex);
		return -1;
	}
	else
	{
		strcpy((l->usuario[l->num].username),name);
		
		l->usuario[l->num].socket = socket;
		l->num++;
		pthread_mutex_unlock(&mutex);
		return 0;
		
	}			
}

//Devuleve -1 si ha habido un error en la consulta y devuelve la pposicion del socket si la consulta se ha realizado correctamente.
int Dame_Socket_position (UserList *l, char name[20])
{
	int i = 0;
	int encontrado = 0;
	pthread_mutex_lock(&mutex);
	while ((i<l->num) && !encontrado)
	{
		if(strcmp((l->usuario[i].username),name)==0)
			encontrado = 1;
		if(!encontrado)
			i++;
		
	}
	if(encontrado) {
		pthread_mutex_unlock(&mutex);
		return i;
	}
	else {
		pthread_mutex_unlock(&mutex);
		return -1;
	}
	
}

//No devuelve nada.
void send_user_list(char resposta[write_buffer_length]) 
{ //La llista ja est￯﾿ﾃ￯ﾾﾠ a dalt com a par￯﾿ﾃ￯ﾾﾠmetre.
	
	
	pthread_mutex_lock(&mutex);
	strcpy(resposta,"");
	sprintf(resposta, "%d/", llista.num); //N￯﾿ﾃ￯ﾾﾺmero d'elements a la llista.
	
	for (int i = 0; i < llista.num; i++) 
		sprintf(resposta, "%s%s/", resposta, llista.usuario[i].username);
	resposta[strlen(resposta)-1] = '\0';
	
	printf("\nEl envio de conectados efectuado ha sido: %s\n",resposta);
	pthread_mutex_unlock(&mutex);
	
}

//No devuelve nada.
int Dame_Todos_Sockets_Conectados (UserList *l,int sock [2000])
{
	int i = 0;
	pthread_mutex_lock(&mutex);
	while (i<l->num)
	{
		sock[i] = l->usuario[i].socket;
		i++;
		
	}
	pthread_mutex_unlock(&mutex);
	return i;
	
}

//No devuelve nada.
void see_conectados() {
	printf("La lista de conectados es: \n");
	char resultados[write_buffer_length];
	char lista[write_buffer_length];
	pthread_mutex_lock(&mutex);
	strcpy(resultados,"");
	for (int k = 0; k< llista.num;k++) {
		sprintf(resultados,"%s/%s",resultados,llista.usuario[k].username);
	}
	resultados[strlen(resultados)-1] = '\0';
	sprintf(lista,"1/%s",resultados);
	pthread_mutex_unlock(&mutex);
	printf("%s\n",lista);
}
//Devuleve 1 sino ha encontrado el nombre a eliminar y devuelve 0 si la consulta se ha realizado correctamente.
int elimina_usuario (UserList *l, int socket)
{
	char nombre [username_max_length]; 
	int pos = 0;
	int encontrado = 0;
	int i = 0;
	pthread_mutex_lock(&mutex);
	while (encontrado == 0)
	{
		if(l->usuario[i].socket == socket){
			pos = i;
			encontrado = 1;
			printf("Lo he encontrado\n");
			strcpy(nombre,l->usuario[i].username);
		}
		i++;
		printf("Buscando\n");
	}
	
	if(encontrado == 1)
	{
		printf("Es %s \n", nombre);
		printf("%s,%d \n",nombre, pos);
		
		for(int i = pos;i< l->num;i++)
		{
			strcpy(l->usuario[i].username,l->usuario[i+1].username);
			l->usuario[i].socket = l->usuario[i+1].socket;
		}
		l->num = l->num - 1;
		pthread_mutex_unlock(&mutex);
		return 0;
	}
	
	else
	{
		printf("No se ha encontrado el usuario a eliminar");
		return 1;
	}	
}


//Devuleve 0 si ha habido un error en la consulta o si la contrase￱a o el usuario son incorrectos y devuelve 1 si la consulta se ha realizado correctamente y el usuario y la contrase￱a esxisten.
int dime_si_usuario_y_contra_son_correctas(char nombre_usuario[username_max_length],char password[password_max_length], MYSQL *conn) {
	MYSQL_RES *result;
	MYSQL_ROW row;
	char str_query[sql_query_max_length];
	sprintf(str_query, "SELECT Contrasenya FROM jugador WHERE Username='%s' AND Contrasenya = '%s'", nombre_usuario, password);
	if (mysql_query(conn, str_query) != 0) {
		
		printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
		return 0;
	}
	result = mysql_store_result(conn);
	row = mysql_fetch_row(result);
	
	if (row == NULL)
		return 0; // Contrase￯﾿ﾃ￯ﾾﾱa o usuario no son correctas.
	else
		return 1; // Contrase￯﾿ﾃ￯ﾾﾱa y usuario son correctas.
}

//Devuleve 0 si ha habido un error en la consulta y devuelve 1 si la consulta se ha realizado correctamente.
int dime_si_usuario_existe(char nombre_usuario[username_max_length], MYSQL *conn) {
	MYSQL_RES *result;
	MYSQL_ROW row;
	char str_query[sql_query_max_length];
	sprintf(str_query, "SELECT Username FROM jugador WHERE Username='%s'", nombre_usuario);
	if (mysql_query(conn, str_query) != 0) {
		
		printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
		return 0;
	}
	result = mysql_store_result(conn);
	row = mysql_fetch_row(result);
	
	if (row == NULL)
		return 0; // Usuario no existe.
	else
		return 1; // Usuario existe.
}

//Devuleve 0 si ha habido un error en la consulta o si el correo no existe y devuelve 1 si la consulta se ha realizado correctamente.
int dime_si_correo_existe(char correo[email_max_length], MYSQL *conn) {
	MYSQL_RES *result;
	MYSQL_ROW row;
	char str_query[sql_query_max_length];
	sprintf(str_query, "SELECT Correo FROM jugador WHERE Correo='%s'", correo);
	if (mysql_query(conn, str_query) != 0) {
		
		printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
		return 0;
	}
	result = mysql_store_result(conn);
	row = mysql_fetch_row(result);
	
	if (row == NULL)
		return 0; // Correo no existe.
	else
		return 1; // Correo existe.
	
}

//Devuleve 0 si ha habido un error en la consulta o no hay jugadores y devuelve el ID del ￺ltimo usuario si la consulta se ha realizado correctamente.
int obtener_id_ultimo_usuario(MYSQL *conn) {
	MYSQL_RES *result;
	MYSQL_ROW row;
	char str_query[sql_query_max_length];
	sprintf(str_query, "SELECT MAX(ID) FROM jugador");
	if (mysql_query(conn, str_query) != 0) {
		
		printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
		return 0;
	}
	result = mysql_store_result(conn);
	row = mysql_fetch_row(result);
	
	if (row == NULL)
		return 0; // No hay jugadores.
	else
		return atoi(row[0]); // M￯﾿ﾃ￯ﾾﾡximo.
}

//Devuleve 0 si ha habido un error en la consulta o si el correo o el nombre ya existen, y devuelve 1 si la consulta se ha realizado correctamente.
int anadir_usario_a_la_base_de_datos(char nombre_usuario[username_max_length],char contrasena[password_max_length], char email[email_max_length], char fecha[10], MYSQL *conn) {
	MYSQL_RES *result;
	MYSQL_ROW row;
	char str_query[sql_query_max_length];
	char id[id_max_length];
	sprintf(id, "%d",obtener_id_ultimo_usuario(conn)+1);
	if ((dime_si_usuario_existe(nombre_usuario,conn)  == 0) && (dime_si_correo_existe(email, conn) == 0) ) {
		sprintf(str_query, "INSERT INTO jugador VALUES ('%s','%s','%s','%s','%s')",id, nombre_usuario,email,contrasena,fecha);
		
		if (mysql_query(conn, str_query) != 0) {
			printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
			return 0;
		}
		else {
			return 1;
		}
	}
	else {
		printf("El correo electronico o el nombre de usuario ya existen.\n");
		return 0;
	}
	
}
//Retorna 0 si ha habido un error en la peticion y devuelve 1 si ha habido exito borrando al usuario.
int elimina_usuario_de_la_base_de_datos (char nombre[username_max_length], MYSQL *conn)
{
	MYSQL_RES *result;
	MYSQL_ROW row;
	char str_query[sql_query_max_length];
	if (dime_si_usuario_existe(nombre,conn) == 1)
	{
		sprintf(str_query,"DELETE FROM jugador WHERE username = '%s'", nombre);
		
		if (mysql_query(conn, str_query) != 0) {
			printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
			return 0;
		}
		return 1;
	}
	else 
	{
		printf("No ha sido posible borrar el usuario");
		return 0;
		
		
	}
}

//Devuleve 0 si ha habido un error en la consulta y devuelve 1 si la consulta se ha realizado correctamente.
int Dame_jugadores_con_los_que_he_jugado (char usuarios_jugado [write_buffer_length],char jugador [username_max_length], MYSQL *conn)
{
	
	MYSQL_RES *result;
	MYSQL_ROW row;
	char *p;
	int count = 0;
	char resultado [write_buffer_length];
	char str_query[sql_query_max_length];
	char comparar[write_buffer_length];
	char ids_Partida [write_buffer_length];
	strcpy(resultado,"");
	strcpy(str_query,"");
	strcpy(comparar,"");
	strcpy(ids_Partida,"");
	sprintf(comparar,"SELECT  RelacionIDsPartidas.IDPartida FROM  jugador,RelacionIDsPartidas WHERE jugador.Username = '%s' AND jugador.ID =   RelacionIDsPartidas.IDJugador",jugador);
	printf("La consulta 1 es: %s \n",comparar);
	if (mysql_query(conn, comparar) != 0) 
	{
		
		printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
		return 0;
	}
	result = mysql_store_result(conn);
	row = mysql_fetch_row(result);	
	printf("Segunda operacion en marcha.\n");
	if (row == NULL)
	{
		printf("No se han encontrado partidas realizadas de este jugador.\n");
		return 0;
	}
	else
	{
		while (row !=NULL ) 
		{
			
			sprintf(ids_Partida,"%s%s/",ids_Partida,row[0]);
			printf("%s\n",row[0]);
			printf("%s\n",ids_Partida);
			count++;
			row = mysql_fetch_row(result);
		}
		sprintf(ids_Partida,"%d/%s",count, ids_Partida);
		ids_Partida[strlen(resultado)-1] = '\0';
	}
	p = strtok(ids_Partida,"/");
	int counter = atoi(p);
	for(int i = 0; i<counter;i++)
	{
		p = strtok(NULL,"/");
		sprintf(str_query,"SELECT jugador.Username FROM jugador,RelacionIDsPartidas WHERE RelacionIDsPartidas.IDPartida = '%d' AND RelacionIDsPartidas.IDJugador = jugador.ID" ,atoi(p));
		printf("La consulta de jugadores es: %s \n",str_query);
		if (mysql_query(conn, str_query) != 0) 
		{
			
			printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
			return 0;
		}
		result = mysql_store_result(conn);
		row = mysql_fetch_row(result);
		
		
		while (row !=NULL ) 
		{
				
			sprintf(resultado,"%s%s/",resultado,row[0]);
			printf("%s\n",row[0]);
			printf("%s\n",resultado);
			row = mysql_fetch_row(result);
		}
			
			 
		
	}
	resultado[strlen(resultado)-1] = '\0';
	printf("El resultado post-consulta es: %s\n",resultado);
	sprintf(usuarios_jugado,"%d/%s",count,resultado);
	printf("El resultado despues de ejecutar es: %s\n",usuarios_jugado);
	return 1;
}

//Devuleve 0 si ha habido un error en la consulta y devuelve 1 si la consulta se ha realizado correctamente.Tambien devuelve 2 arays con los resultados y a que equipo pertenecia el jugador.
int Dame_resultados_jugadores_con_los_que_he_jugado (char resultados [write_buffer_length],char equipos [write_buffer_length],char jugador [username_max_length],MYSQL *conn)
{
	
	MYSQL_RES *result;
	MYSQL_ROW row;
	char *p;
	int count = 0;
	char resultado[write_buffer_length];
	char EquiposCopiar [write_buffer_length];
	char str_query[sql_query_max_length];
	char comparar[write_buffer_length];
	char ids_Partida [write_buffer_length];
	sprintf(comparar,"SELECT  RelacionIDsPartidas.IDPartida,RelacionIDsPartidas.IDEquipo FROM  jugador,RelacionIDsPartidas WHERE jugador.Username = '%s' AND jugador.ID = RelacionIDsPartidas.IDJugador)",jugador);
	if (mysql_query(conn, str_query) != 0) 
	{
		
		printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
		return 0;
	}
	result = mysql_store_result(conn);
	row = mysql_fetch_row(result);	
	
	if (row == NULL)
	{
		printf("No se han encontrado partidas realizadas de este jugador.\n");
		return 0;
	}
	else
	{
		strcpy(ids_Partida,"");
		while (row !=NULL ) 
		{
			
			sprintf(ids_Partida,"%s%s/",ids_Partida,row[0]);
			printf("%s\n",row[0]);
			printf("%s\n",ids_Partida);count++;
			sprintf(EquiposCopiar,"%s%s/",EquiposCopiar,row[1]);
			count++;
			row = mysql_fetch_row(result);
			
			
		}
		sprintf(ids_Partida,"%d/%s",count, ids_Partida);
		ids_Partida[strlen(ids_Partida)-1] = '\0';
		sprintf(equipos,"%d/%s",count,EquiposCopiar);
		equipos[strlen(equipos)-1] = '\0';
			
	}
	p = strtok(ids_Partida,"/");
	int counter = atoi(p);
	for(int i = 0; i<counter;i++)
	{
		p = strtok(NULL,"/");
		sprintf(str_query,"SELECT partidas.Resultado FROM jugador,RelacionIDsPartidas WHERE RelacionIDsPartidas.IDPartida = partidas.IDPartida AND RelacionIDsPartidas.IDPartida = '%d'",atoi(p));
		if (mysql_query(conn, str_query) != 0) 
		{
			
			printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
			return 0;
		}
		result = mysql_store_result(conn);
		row = mysql_fetch_row(result);
		
		
		while (row !=NULL ) //en teoria nom￯﾿ﾩs ha de fer una iteraci￯﾿ﾳ per cada p, per tant el while es podria ometre.
		{
				
			sprintf(resultado,"%s%s/",resultado,row[0]);
			printf("%s\n",row[0]);
			printf("%s\n",resultado);
			row = mysql_fetch_row(result);
		}
			
			 
		}	
	resultado[strlen(resultado)-1] = '\0';
	printf("%s\n",resultado);
	sprintf(resultados,"%d/%s",count,resultado);
	printf("El resultado despues de ejecutar es: %s\n",resultados);
	return 1;
}
//Devuelve 0 si ha habido un error al guardar y 1 si la partida se ha guardado correctamente.
int Guardar_Partidas_Base_De_Datos (char FechaYHora[FechaYHora_length],int IDPartida,int IDEquipo1,int IDEquipo2,int resultado,int duracion,MYSQL *conn)
{
	MYSQL_RES *result;
	MYSQL_ROW row;
	char str_query[sql_query_max_length];
	strcpy(str_query,"");
	sprintf(str_query, "INSERT INTO partidas VALUES (%d,%d,%d,'%s',%d,%d)",IDPartida,IDEquipo1,IDEquipo2,FechaYHora,duracion,resultado);
	if (mysql_query(conn, str_query) != 0) 
	{
		printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
		return 0;
	}
	else
	{
		printf("Partida guardada con exito\n");
		return 1;
		
	}
}

int numero_de_partidas_jugadas_en_X_intervalo_de_tiempo(char dia1[10], char dia2[10], MYSQL *conn) {
	MYSQL_RES *result;
	MYSQL_ROW row;
	char str_query[sql_query_max_length];
	sprintf(str_query, "SELECT COUNT(IDPartida) FROM partidas WHERE FechayHoraFinal>='%s 00:00:00' AND FechayHoraFinal < '%s 00:00:00'", dia1, dia2);
	if (mysql_query(conn, str_query) != 0) {
		
		printf("Error al ejecutar la consulta: %u %s\n", mysql_errno(conn), mysql_error(conn));
		return 0;
	}
	result = mysql_store_result(conn);
	row = mysql_fetch_row(result);
	
	return atoi(row[0]);
}

int cuenta_cantidad_de_usuarios(MYSQL *conn) {
	MYSQL_RES *result;
	MYSQL_ROW row;
	char consulta[sql_query_max_length];
	strcpy(consulta, "SELECT COUNT(Username) FROM jugador");
	
	if (mysql_query (conn, consulta) != 0) {
		printf ("Error al consultar datos de la base %u %s\n", mysql_errno(conn), mysql_error(conn));
		return -1;
	}
	
	result = mysql_store_result(conn);
	row = mysql_fetch_row(result);
	return atoi(row[0]);
}

void dame_todos_los_usuarios(char usuarios[write_buffer_length]  , MYSQL *conn) {
	MYSQL_RES *result;
	MYSQL_ROW row;
	char consulta[sql_query_max_length];
	char resultado[write_buffer_length];
	int count = 0;
	strcpy(consulta, "SELECT Username FROM jugador");
	if (mysql_query (conn, consulta) != 0) {
		printf ("Error al consultar datos de la base %u %s\n", mysql_errno(conn), mysql_error(conn));
		exit(1);
	}
	
	result = mysql_store_result(conn);
	row = mysql_fetch_row(result);
	strcpy(resultado,"");
	while (row !=NULL ) {
		
		sprintf(resultado,"%s%s/",resultado,row[0]);
		printf("%s\n",row[0]);
		printf("%s\n",resultado);
		count++;
		row = mysql_fetch_row(result);
	}
	
	resultado[strlen(resultado)-1] = '\0';
	printf("%s\n",resultado);
	sprintf(usuarios,"%d/%s",count,resultado);
	printf("El resultado despues de ejecutar es: %s\n",usuarios);
}

float dame_tiempo_medio_partidas_jugador(char usuario[username_max_length], MYSQL *conn) {
	MYSQL_RES *result;
	MYSQL_ROW row;
	char consulta[sql_query_max_length];
	
	strcpy(consulta, "SELECT AVG(partidas.Duracion) FROM (jugador, partidas, RelacionIDsPartidas) WHERE jugador.USERNAME = '");
	strcat(consulta, usuario);
	strcat(consulta, "' AND jugador.ID = RelacionIDsPartidas.IDJugador AND (RelacionIDsPartidas.IDEquipo = partidas.IDEquipo1 OR RelacionIDsPartidas.IDEquipo = partidas.IDEquipo2) ");
	
	if (mysql_query (conn, consulta) != 0) {
		printf ("Error al consultar datos de la base %u %s\n", mysql_errno(conn), mysql_error(conn));
		return 0.0;
		exit(1);
	}
	result = mysql_store_result(conn);
	row = mysql_fetch_row(result);
	if (row[0] == NULL) {
		return 0.0;
	}
	return atof(row[0]);
	
	
}

int devuelvaPartidasGanadas(char usuario[username_max_length], MYSQL *conn)
{
	
	MYSQL_RES *resultado;
	MYSQL_ROW row;
	int victorias = 0;
	char consulta[sql_query_max_length];
	strcpy(consulta,"SELECT partidas.Resultado,RelacionIDsPartidas.IDEquipo,partidas.IDEquipo1,partidas.IDEquipo2 FROM (RelacionIDsPartidas,jugador,partidas) WHERE jugador.USERNAME = '");
	strcat(consulta,usuario);
	strcat(consulta, "' AND jugador.ID = RelacionIDsPartidas.IDJugador AND RelacionIDsPartidas.IDPartida = partidas.IDPartida");
	if (mysql_query (conn, consulta)!=0) 
	{
		printf ("Error al consultar datos de la base %u %s\n", mysql_errno(conn), mysql_error(conn));
		return 0;
		exit(1);
	}
	
	resultado = mysql_store_result(conn); 
	row = mysql_fetch_row(resultado);
	if (row == NULL) {
		
		return 0;
	}
	else {
		
		while(row != NULL)
		{
			
			if ((atoi(row[1]) == atoi(row[2]) && atoi(row[0]) == 0) || (atoi(row[1]) == atoi(row[3]) && atoi(row[0]) == 1) )   //we supose that if the tinyint is 0 team1 wins, if 1 otherwise
				victorias = victorias + 1;
			row = mysql_fetch_row (resultado);
		}
		
		
		
		// cerrar la conexion con el servidor MYSQL 
		
		return victorias; 
	}// We return the value obtained 
	
}	



void* atenderClientes(void *socket)	
{
	int list = 0;
	int ret;
	int sock_conn;
	char peticion[read_buffer_length];
	char notificacion[write_buffer_length];
	char respuesta[write_buffer_length];
	char todos[write_buffer_length];
	char usuarios_db[write_buffer_length];
	strcpy(notificacion,"");
	strcpy(respuesta,"");
	strcpy(todos,"");
	strcpy(usuarios_db,"");
	
	int *s;
	s = (int *) socket;
	sock_conn = *s;
	
	MYSQL *conn;
	conn = mysql_init(NULL);
	if (conn == NULL) {
		printf("Error al crear la conexion: %u %s\n", mysql_errno(conn), mysql_error(conn));
		exit(1);
	}
	conn = mysql_real_connect(conn, database_host, database_username, database_password, database_name, 0, NULL, 0);
	
	if (conn == NULL) {
		
		printf("Error conectando: %u %s\n", mysql_errno(conn), mysql_error(conn));
		exit(1);
		
	}
	int stop = 0;
	while(stop == 0){
		
		ret = read(sock_conn,peticion,sizeof(peticion));
		
		printf("Recibido\n");
		peticion[ret]='\0';
		printf("Ret es %d\n", ret);
		if (ret == -1) { continue; }
		else if (ret == 0) {
			break;
		}
		printf("Peticion: %s\n",peticion);
		
			char* p = strtok(peticion, "/");
			int codigo = atoi(p);

			char nombre_usuario[username_max_length];
			char contrasena[password_max_length];
			char correo[email_max_length];
			char fecha[10];
			char dia1[10];
			char dia2[10];
			strcpy(nombre_usuario, "");
			strcpy(contrasena, "");
			strcpy(correo, "");



			if (codigo == 0)
			{

				stop = 1;
				int i = 0;
				int sock[2000];
				int res = elimina_usuario(&llista, sock_conn);

				if (res == 0)
				{
					printf("Ha sido posible eliminar el usuario.\n");
					write(sock_conn, "0/", 2);
					see_conectados();
					send_user_list(respuesta);
					sprintf(notificacion, "7/%s", respuesta);
					Dame_Todos_Sockets_Conectados(&llista, sock);
					printf("La cantidad de conectados es: %d\n", llista.num);
					printf("La respuesta a enviar: %s\n", respuesta);
					printf("La notificaci￯﾿ﾃ￯ﾾﾳn a enviar es: %s\n", notificacion);

					while (i < llista.num)
					{
						write(sock[i], notificacion, strlen(notificacion));
						i++;
					}


				}


			}

			else if (codigo == 1)
			{
				p = strtok(NULL, "/");
				strcpy(nombre_usuario, p);

				p = strtok(NULL, "/");
				strcpy(contrasena, p);
				printf("Codigo: %d, Nombre: %s Contra: %s \n", codigo, nombre_usuario, contrasena);



				int valor = dime_si_usuario_y_contra_son_correctas(nombre_usuario, contrasena, conn);


				if (valor == 1)
					strcpy(respuesta, "Login");
				else
					strcpy(respuesta, "Permiso denegado");
				write(sock_conn, respuesta, strlen(respuesta));


			}

			else if (codigo == 2)
			{
				p = strtok(NULL, "/");
				strcpy(dia1, p);

				p = strtok(NULL, "/");
				strcpy(dia2, p);
				printf("Codigo: %d, Dia 1: %s Dia2: %s \n", codigo, dia1, dia2);


				int value = numero_de_partidas_jugadas_en_X_intervalo_de_tiempo(dia1, dia2, conn);


				sprintf(respuesta, "2/%d", value);
				printf("%s\n", respuesta);
				write(sock_conn, respuesta, strlen(respuesta));

			}

			else if (codigo == 3)
			{
				strcpy(respuesta, "");
				p = strtok(NULL, "/");
				strcpy(nombre_usuario, p);

				p = strtok(NULL, "/");
				strcpy(contrasena, p);
				p = strtok(NULL, "/");
				strcpy(correo, p);
				p = strtok(NULL, "/");
				strcpy(fecha, p);
				printf("Codigo: %d, Usuario: %s Contra: %s Correo: %s Fecha: %s \n", codigo, nombre_usuario, contrasena, correo, fecha);


				int registro = anadir_usario_a_la_base_de_datos(nombre_usuario, contrasena, correo, fecha, conn);


				if (registro == 1)
					sprintf(respuesta, "Registro");
				else {
					if ((dime_si_usuario_existe(nombre_usuario, conn) == 1))
						sprintf(respuesta, "Usuario existente");
					else if (dime_si_correo_existe(correo, conn) == 1)
						sprintf(respuesta, "Correo existente");


				}
				printf("Respuesta enviada tras registro: %s\n", respuesta);
				write(sock_conn, respuesta, strlen(respuesta));

			}
			else if (codigo == 4) {
				
				char nombre_usuario_average[username_max_length];

				p = strtok(NULL, "/");
				strcpy(nombre_usuario_average, p);
				sprintf(respuesta, "4/%0.01f", dame_tiempo_medio_partidas_jugador(nombre_usuario_average, conn));
				printf("%s\n", respuesta);
				write(sock_conn, respuesta, strlen(respuesta));





			}
			else if (codigo == 5)
			{
				char nombre_usuario_ganadas[username_max_length];
				p = strtok(NULL, "/");
				strcpy(nombre_usuario_ganadas, p);
				sprintf(respuesta, "5/%d", devuelvaPartidasGanadas(nombre_usuario_ganadas, conn));
				write(sock_conn, respuesta, strlen(respuesta));



			}
			else if (codigo == 6)
			{

				dame_todos_los_usuarios(todos, conn);
				pthread_mutex_lock(&mutex);
				sprintf(respuesta, "6/%s", todos);
				printf("Los usuarios a enviar son %s\n", respuesta);
				write(sock_conn, respuesta, strlen(respuesta));
				pthread_mutex_unlock(&mutex);

			}


			else if (codigo == 7)
			{
				int i = 0;
				char new_user[username_max_length];

				int sock[2000];

				//printf("11");
				p = strtok(NULL, "/");
				//printf("11");
				strcpy(new_user, p);
				printf("Usuario: %s\n", new_user);
				printf("%d\n", i);
				int res = agregar_usuari(&llista, new_user, sock_conn);
				see_conectados();

				if (res == -1)
					printf("Error al agregar usuario\n");


				else
				{

					send_user_list(respuesta);
					sprintf(notificacion, "7/%s", respuesta);
					int n = Dame_Todos_Sockets_Conectados(&llista, sock);
					printf("La cantidad de conectados es: %d\n", n);
					printf("La respuesta a enviar: %s\n", respuesta);
					printf("La notificaci￯﾿ﾃ￯ﾾﾳn a enviar es: %s\n", notificacion);

					
					while (i < n)
					{
						write(sock[i], notificacion, strlen(notificacion));
						i++;
					}

				}
			}
			else if (codigo == 8) { //PROTOCOLO PARA INVITAR. ORIGEN ENV\CDA: 8/numeroInvitados/nombreOrigen/nombreDestino1/nombreDestino2/nombreDestino3. Se le env\EDa al destino notificacion con 8/socketOrigen/nombreOrigen/0 o 1 seg\FAn su equipo asignado
				char testConsola[300];
				int cantidad;
				char origin[30];
				char target[30];
				//char randomChar[3];
				p = strtok(NULL, "/");
				cantidad = atoi(p);
				p = strtok(NULL, "/");
				strcpy(origin, p);
				int originTeamCount = 0;
				int rivalTeamCount = 0;
				int teamTarget;
				int j = 0;

				while (j < cantidad) {
					p = strtok(NULL, "/");
					strcpy(target, p);
					printf("Estoy invitando a %s \n", target);
					if (cantidad > 1) {
						if (rivalTeamCount < 2) {
							if (originTeamCount == 0) {
								teamTarget = rand() % 2;
								if (teamTarget == 0) {
									originTeamCount = originTeamCount + 1;
								}
								else if (teamTarget == 1) {
									rivalTeamCount = rivalTeamCount + 1;
								}
							}
							else {
								teamTarget = 1;
								rivalTeamCount = rivalTeamCount + 1;
							}
						}
						else {
							teamTarget = 0;
							originTeamCount = originTeamCount + 1;
						}
					}
					else {
						teamTarget = 1;
						rivalTeamCount = rivalTeamCount + 1;
					}
					int targetSocketPosition = Dame_Socket_position(&llista, target);
					// !mutex
					int targetSocket = llista.usuario[targetSocketPosition].socket;
					sprintf(notificacion, "8/%d/%s/%d", sock_conn, origin, teamTarget);
					printf("%s \n", notificacion);
					write(targetSocket, notificacion, strlen(notificacion));
					j = j + 1;
				}


			}
			else if (codigo == 9) { //PROTOCOLO PARA ACEPTAR INVITACI\D3N. DESTINO ENV\CDA: 9/0/socketOrigen/nombreDestino/equipo Aceptada. 9/1/socketOrigen/nombreDestino Denegada. Si aceptada se le da socket a origen y se le indica equipo: 9/0/socketDestino/nombreDestino/0 o 1. Si denegada se le dice quien le ha denegado: 9/1/nombreDestino
				//SERVIDOR DA A LIDER: 9/0/socketInvitado/nombreInvitado/Equipo
				int status;
				int originSocket;
				char target[30];
				p = strtok(NULL, "/");
				status = atoi(p);
				p = strtok(NULL, "/");
				originSocket = atoi(p);
				p = strtok(NULL, "/");
				strcpy(target, p);
				if (status == 0) {
					printf("Invitacion aceptada \n");
					int equipo;
					p = strtok(NULL, "/");
					equipo = atoi(p);
					sprintf(notificacion, "9/0/%d/%s/%d", sock_conn, target, equipo);
				}
				else {
					sprintf(notificacion, "9/1/%s", target);
				}
				write(originSocket, notificacion, strlen(notificacion));
				printf("%s \n", notificacion);
			}
			else if (codigo == 10) { //PROTOCOLO PARA CHATEAR. 10/nombreOrigen/mensaje. Se env\EDa 10/nombreOrigen/mensaje
				char origin[20];
				char message[100];
				p = strtok(NULL, "/");
				strcpy(origin, p);;
				p = strtok(NULL, "/");
				strcpy(message, p);
				sprintf(notificacion, "10/%s/%s", origin, message);
				int sock[2000];
				int n = Dame_Todos_Sockets_Conectados(&llista, sock);
				int j = 0;
				while (j < n) {
					write(sock[j], notificacion, strlen(notificacion));
					j = j + 1;
				}
			}
			else if (codigo == 11) {
				p = strtok(NULL, "/");
				int playerCount = atoi(p);
				if (playerCount == 2) {
					p = strtok(NULL, "/");
					int socketRival = atoi(p);
					p = strtok(NULL, "/");
					int prefab_index = atoi(p) + 7;
					sprintf(notificacion, "11/%d", prefab_index);
					write(socketRival, notificacion, strlen(notificacion));
				}
				else {
					p = strtok(NULL, "/");
					int rival1 = atoi(p);
					p = strtok(NULL, "/");
					int rival2 = atoi(p);
					p = strtok(NULL, "/");
					int companerodeequipo = atoi(p);
					p = strtok(NULL, "/");
					int prefab_index = atoi(p);
					sprintf(notificacion, "11/%d", prefab_index + 7);
					write(rival1, notificacion, strlen(notificacion));
					write(rival2, notificacion, strlen(notificacion));
					strcpy(notificacion, "");
					sprintf(notificacion, "11/%d", prefab_index);
					write(companerodeequipo, notificacion, strlen(notificacion));
				}


			}

			else if (codigo == 12) {
				char usuario_a_eliminar[username_max_length];
				char contra_a_eliminar[password_max_length];
				p = strtok(NULL, "/");
				strcpy(usuario_a_eliminar, p);
				p = strtok(NULL, "/");
				strcpy(contra_a_eliminar, p);
				int eliminacion = elimina_usuario_de_la_base_de_datos(usuario_a_eliminar, conn);
				if (eliminacion == 1) {
					printf("La consulta de eliminaci￯﾿ﾃ￯ﾾﾳn se ha ejecutado correctamente.\n");
					stop = 1;
					int i = 0;
					int sock[2000];
					int res = elimina_usuario(&llista, sock_conn);

					if (res == 0)
					{
						printf("Ha sido posible eliminar el usuario.\n");
						write(sock_conn, "0/", 2);
						see_conectados();
						send_user_list(respuesta);
						sprintf(notificacion, "7/%s", respuesta);
						int n = Dame_Todos_Sockets_Conectados(&llista, sock);
						printf("La cantidad de conectados es: %d\n", n);
						printf("La respuesta a enviar: %s\n", respuesta);
						printf("La notificaci￯﾿ﾃ￯ﾾﾳn a enviar es: %s\n", notificacion);

						while (i < n)
						{
							write(sock[i], notificacion, strlen(notificacion));
							i++;
						}
					}
				}
			}

			else if (codigo == 13) {
				char usuarios_jugado_con[write_buffer_length];
				char usuario_jug[username_max_length];
				p = strtok(NULL, "/");
				strcpy(usuario_jug, p);
				printf("Hola\n");
				int resultado_cons = Dame_jugadores_con_los_que_he_jugado(usuarios_jugado_con, usuario_jug, conn);
				printf("Hola\n");
				sprintf(respuesta, "13/%s", usuarios_jugado_con);
				if (resultado_cons == 1)
				{
					printf("La consulta ha tenido exito\n");
					write(sock_conn, respuesta, strlen(respuesta));
				}
			}

			else if (codigo == 14)
			{
				char FechaYHora[FechaYHora_length];
				int IDPartida;
				int IDEquipo1;
				int IDEquipo2;
				int Duracion;
				int ResultadoFinal;
				p = strtok(NULL, "/");
				IDPartida = atoi(p);
				p = strtok(NULL, "/");
				IDEquipo1 = atoi(p);
				p = strtok(NULL, "/");
				IDEquipo2 = atoi(p);
				p = strtok(NULL, "/");
				strcpy(FechaYHora, p);
				p = strtok(NULL, "/");
				Duracion = atoi(p);
				p = strtok(NULL, "/");
				ResultadoFinal = atoi(p);
				int r = Guardar_Partidas_Base_De_Datos(FechaYHora, IDPartida, IDEquipo1, IDEquipo2, Duracion, ResultadoFinal, conn);
				if (r == 0)
					printf("No se ha podido guardar la partida de manera exitosa.\n");
				else
					printf("Partida guardada de manera exitosa");

			}

			else if (codigo == 20) {
				p = strtok(NULL, ";");
				char lobby[write_buffer_length];
				strcpy(lobby, p);
				p = strtok(NULL, ";");
				sprintf(notificacion, "20/%s", lobby);
				printf("Notificacion del codigo 20: %s", notificacion);
				write(atoi(p), notificacion, strlen(notificacion));
			}
			else if (codigo == 30) { //MENSAJE QUE ENV\CDA EL LEADER PARA CREAR LA PARTIDA Y QUE TODOS CONOZCAN SUS EQUIPOS
				//AL SERVIDOR LLEGA: 30/nJugadores/nombreLider/nombreCompa\F1eroLider/sockCompa\F1eroLider/nombreRival1/sockRival1/nombreRival2/sockRival2
				//Si la partida es 1v1: 30/1/nombreLider/sockRival
				//SERVIDOR PASA 30/nJugadores/Compa\F1ero/sockCompa\Fo/Rival1/sockRival1/Rival2/sockRival2 a cada jugador
				//Si la partida es 1v1: 30/1/lider/sockLider
				char lider[username_max_length];
				int cantidad;
				p = strtok(NULL, "/");
				cantidad = atoi(p);
				printf("La cantidad de usuarios es: %d.\n", cantidad);
				p = strtok(NULL, "/");
				strcpy(lider, p);
				if (cantidad == 1) {
					int sockRival;
					p = strtok(NULL, "/");
					sockRival = atoi(p);
					sprintf(notificacion, "30/1/%s/%d", lider, sock_conn);
					write(sockRival, notificacion, strlen(notificacion));
				}
				else {
					char liderMate[username_max_length];
					int sockLiderMate;
					p = strtok(NULL, "/");
					strcpy(liderMate, p);
					p = strtok(NULL, "/");
					sockLiderMate = atoi(p);
					char organizerMate[write_buffer_length];

					char firstEnemy[username_max_length];
					int sockFirstEnemy;
					p = strtok(NULL, "/");
					strcpy(firstEnemy, p);
					p = strtok(NULL, "/");
					sockFirstEnemy = atoi(p);
					char organizerFirstEnemy[write_buffer_length];

					char secondEnemy[username_max_length];
					int sockSecondEnemy;
					p = strtok(NULL, "/");
					strcpy(secondEnemy, p);
					p = strtok(NULL, "/");
					sockSecondEnemy = atoi(p);
					char organizerSecondEnemy[write_buffer_length];

					sprintf(organizerMate, "30/3/%s/%d/%s/%d/%s/%d", lider, sock_conn, firstEnemy, sockFirstEnemy, secondEnemy, sockSecondEnemy);
					sprintf(organizerFirstEnemy, "30/3/%s/%d/%s/%d/%s/%d", secondEnemy, sockSecondEnemy, lider, sock_conn, liderMate, sockLiderMate);
					sprintf(organizerSecondEnemy, "30/3/%s/%d/%s/%d/%s/%d", firstEnemy, sockFirstEnemy, lider, sock_conn, liderMate, sockLiderMate);
					printf("OrganizerMate: %s\n", organizerMate);
					printf("organizerFirstEnemy: %s\n", organizerFirstEnemy);
					printf("organizerSecondEnemy: %s\n", organizerSecondEnemy);
					write(sockLiderMate, organizerMate, strlen(organizerMate));
					write(sockFirstEnemy, organizerFirstEnemy, strlen(organizerFirstEnemy));
					write(sockSecondEnemy, organizerSecondEnemy, strlen(organizerSecondEnemy));
				}

			}
	
		
		
	}
	
	close(sock_conn);
	
}
int main(int argc, char *argv[]) {
	socket_num = 0;
	llista.num = 0;
	int sock_conn, sock_listen;
	struct sockaddr_in serv_adr;
	if ((sock_listen = socket(AF_INET, SOCK_STREAM, 0)) < 0)
		printf("Error creant socket\n");
	
	memset(&serv_adr, 0, sizeof(serv_adr));
	serv_adr.sin_family = AF_INET;
	serv_adr.sin_addr.s_addr = htonl(INADDR_ANY); 
	serv_adr.sin_port = htons(port);
	
	setsockopt(sock_listen, SOL_SOCKET, SO_REUSEADDR, &(int){1},sizeof(int));
	if (bind(sock_listen, (struct sockaddr *) &serv_adr, sizeof(serv_adr)) < 0)
		printf("Error al bind\n");
	if (listen(sock_listen, 10) < 0)
		printf("Error en el listen\n");
	
	
	
	
	for (;;)
	{
		printf ("Escuchando\n");
		
		sock_conn = accept(sock_listen, NULL, NULL);
		
		printf ("He recibido conexion\n");
		

		//sockets[socket_num] =sock_conn;
		//sock_conn ￯﾿ﾃ￯ﾾﾩs el socket del client en q￯﾿ﾃ￯ﾾﾼesti￯﾿ﾃ￯ﾾﾳ.
		
		// Creem el thread i diem els procesos que ha de fer.
		pthread_create (&threads[socket_num], NULL, atenderClientes,&sock_conn);
		//pthread_create (&threads[socket_num], NULL, atenderClientes,&sockets[socket_num]);
		socket_num ++;

		//CLIENTES MUERTOS: EN VEZ DE SOCKETS[] USAR LISTA
		//SOCK[] se puede usar localmente, solo localmente!!, para llenar vector con sockets necesarios
		//Asegurar que las funciones que modifican la lista funcionan, sino caput
		//mutex_lock en este bucle para llista.usuario.socket[]=sock_conn ??
		
	}
	
	for(;;)
	{
		pthread_join(threads[socket_num],NULL);
	}
	
	
	return 0;
	
}

