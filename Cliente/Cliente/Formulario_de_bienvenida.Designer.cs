namespace Cliente
{
    partial class Formulario_de_bienvenida
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.texto_bienvenida = new System.Windows.Forms.Label();
            this.consultar_partidas_dia = new System.Windows.Forms.Button();
            this.victorias_button = new System.Windows.Forms.Button();
            this.tiempo_medio_button = new System.Windows.Forms.Button();
            this.dia_textbox = new System.Windows.Forms.TextBox();
            this.etiqueta_dia = new System.Windows.Forms.Label();
            this.usuarios_grid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.usuarios_grid)).BeginInit();
            this.SuspendLayout();
            // 
            // texto_bienvenida
            // 
            this.texto_bienvenida.AutoSize = true;
            this.texto_bienvenida.Location = new System.Drawing.Point(314, 13);
            this.texto_bienvenida.Name = "texto_bienvenida";
            this.texto_bienvenida.Size = new System.Drawing.Size(71, 13);
            this.texto_bienvenida.TabIndex = 0;
            this.texto_bienvenida.Text = "Bienvenido/a";
            this.texto_bienvenida.Click += new System.EventHandler(this.texto_bienvenida_Click);
            // 
            // consultar_partidas_dia
            // 
            this.consultar_partidas_dia.Location = new System.Drawing.Point(40, 75);
            this.consultar_partidas_dia.Name = "consultar_partidas_dia";
            this.consultar_partidas_dia.Size = new System.Drawing.Size(169, 61);
            this.consultar_partidas_dia.TabIndex = 1;
            this.consultar_partidas_dia.Text = "Consultar número de partidas jugadas en X día.";
            this.consultar_partidas_dia.UseVisualStyleBackColor = true;
            this.consultar_partidas_dia.Click += new System.EventHandler(this.consultar_partidas_dia_Click);
            // 
            // victorias_button
            // 
            this.victorias_button.Location = new System.Drawing.Point(40, 154);
            this.victorias_button.Name = "victorias_button";
            this.victorias_button.Size = new System.Drawing.Size(169, 52);
            this.victorias_button.TabIndex = 2;
            this.victorias_button.Text = "Número de victorias de un jugador";
            this.victorias_button.UseVisualStyleBackColor = true;
            this.victorias_button.Click += new System.EventHandler(this.victorias_button_Click);
            // 
            // tiempo_medio_button
            // 
            this.tiempo_medio_button.Location = new System.Drawing.Point(40, 232);
            this.tiempo_medio_button.Name = "tiempo_medio_button";
            this.tiempo_medio_button.Size = new System.Drawing.Size(169, 61);
            this.tiempo_medio_button.TabIndex = 3;
            this.tiempo_medio_button.Text = "Tiempo medio de partidas de un jugador";
            this.tiempo_medio_button.UseVisualStyleBackColor = true;
            this.tiempo_medio_button.Click += new System.EventHandler(this.tiempo_medio_button_Click);
            // 
            // dia_textbox
            // 
            this.dia_textbox.Location = new System.Drawing.Point(377, 96);
            this.dia_textbox.Name = "dia_textbox";
            this.dia_textbox.Size = new System.Drawing.Size(254, 20);
            this.dia_textbox.TabIndex = 4;
            // 
            // etiqueta_dia
            // 
            this.etiqueta_dia.AutoSize = true;
            this.etiqueta_dia.Location = new System.Drawing.Point(377, 77);
            this.etiqueta_dia.Name = "etiqueta_dia";
            this.etiqueta_dia.Size = new System.Drawing.Size(211, 13);
            this.etiqueta_dia.TabIndex = 5;
            this.etiqueta_dia.Text = "Escribe un día en el formato \"aaaa-mm-dd\"";
            // 
            // usuarios_grid
            // 
            this.usuarios_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.usuarios_grid.Location = new System.Drawing.Point(378, 123);
            this.usuarios_grid.Name = "usuarios_grid";
            this.usuarios_grid.Size = new System.Drawing.Size(254, 170);
            this.usuarios_grid.TabIndex = 8;
            // 
            // Formulario_de_bienvenida
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.usuarios_grid);
            this.Controls.Add(this.etiqueta_dia);
            this.Controls.Add(this.dia_textbox);
            this.Controls.Add(this.tiempo_medio_button);
            this.Controls.Add(this.victorias_button);
            this.Controls.Add(this.consultar_partidas_dia);
            this.Controls.Add(this.texto_bienvenida);
            this.Name = "Formulario_de_bienvenida";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Formulario_de_bienvenida_Load);
            ((System.ComponentModel.ISupportInitialize)(this.usuarios_grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label texto_bienvenida;
        private System.Windows.Forms.Button consultar_partidas_dia;
        private System.Windows.Forms.Button victorias_button;
        private System.Windows.Forms.Button tiempo_medio_button;
        private System.Windows.Forms.TextBox dia_textbox;
        private System.Windows.Forms.Label etiqueta_dia;
        private System.Windows.Forms.DataGridView usuarios_grid;
    }
}