//Michel Herraiz Gurillo
//Javier Gómez Zúñiga
using System;
using System.IO;
using System.Linq.Expressions;

namespace Flappy_Bird
{
    internal class Program
    {
        static Random rnd = new Random(); // generador de aleatorios para colocar los obstáculos verticales
        const bool DEBUG = true; // para sacar información de depuración en el Renderizado
        const int ANCHO = 10, ALTO = 5, // tamaño del área de juego
        SEP_OBS = 7, // separación horizontal entre obstáculos
        HUECO = 4, // hueco de los obstáculos (en vertical)
        COL_BIRD = ANCHO / 3, // columna fija del pájaro
        IMPULSO = 3, // unidades de ascenso por aleteo
        DELTA = 300; // retardo entre frames (ms)
        static void Main()
        { // programa principal
            Console.CursorVisible = false; //Oculta el cursor

            int[] suelo, techo;
            int fil, ascenso, frame, puntos;
            bool colision, abort = false;

            Console.WriteLine("¿Quieres cargar una partida guardada?");
            string res = Console.ReadLine();
            if (res == "Sí" || res == "Si" || res == "sí" || res == "si") CargaJuego("PartidaGuardada.txt", out suelo, out techo, out fil, out ascenso, out frame, out puntos, out colision);
            else Inicializa(out suelo, out techo, out fil, out ascenso, out frame, out puntos, out colision);
            
            Render(suelo, techo, fil, frame, puntos, colision);
            while (!colision && !abort)
            {
                char input = LeeInput();
                if (input == 'q')
                {
                    abort = true;
                }
                else if (input == 'p')
                {
                    Console.WriteLine("Pausado, Intro para continuar");
                    Console.ReadLine();
                }
                else
                {
                    Avanza(suelo, techo, frame);
                    Mueve(input, ref fil, ref ascenso);
                    colision = Colision(suelo, techo, fil);
                    Puntua(suelo, techo, ref puntos);
                    frame++;

                    Render(suelo, techo, fil, frame, puntos, colision);
                    System.Threading.Thread.Sleep(DELTA);
                }
            }
            if (abort)
            {
                Console.WriteLine("¿Quieres guardar partida?");
                res = Console.ReadLine();
                if (res == "Sí" || res == "Si" || res == "sí" || res == "si")
                {
                    GuardaJuego("PartidaGuardada.txt", suelo, techo, fil, ascenso, frame, puntos);
                    Console.WriteLine("Partida guardada con éxito");
                }
            }
        }

        //MÉTODOS
        static void Inicializa(out int[] suelo, out int[] techo, out int fil, out int ascenso, out int frame, out int puntos, out bool colision)
        { //Inicializa los valores de todos los parámetros al inicio del juego
            suelo = new int[ANCHO];
            techo = new int[ANCHO];
            for (int i = 0; i < ANCHO; i++)
            {
                techo[i] = ALTO - 1;
                suelo[i] = 0;
            }

            fil = ALTO / 2;
            ascenso = -1;
            frame = 0;
            puntos = 0;
            colision = false;
        }
        static void Render(int[] suelo, int[] techo, int fil, int frame, int puntos, bool colision)
        {//Renderizado del escenario con obstáculos, el pájaro, los puntos y el DEBUG si está habilitado
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();

            Console.BackgroundColor = ConsoleColor.DarkCyan;
            for (int i = 0; i < ANCHO; i++)
            {//Conversión de coordenadas reales al sistema de representación (Espejo)
                int coordsuelo = (ALTO - 1) - suelo[i]; 
                for (int j = coordsuelo; j <= (ALTO - 1); j++)
                {
                    Console.SetCursorPosition(2 * i, j);
                    Console.Write("  ");
                }
                int coordtecho = (ALTO - 1) - techo[i];
                for (int k = coordtecho; k >= 0; k--)
                {
                    Console.SetCursorPosition(2 * i, k);
                    Console.Write("  ");
                }
            }
            //Renderizado del pájaro dependiendo de si ha colisionado o no
            Console.BackgroundColor = ConsoleColor.Green;
            if(fil>ALTO-1) fil = ALTO-1; //Evita que el pájaro se salga del área representable
            Console.SetCursorPosition(COL_BIRD*2, (ALTO - 1) - fil);
            if(!colision)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("->");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("**");
            }
            //DEBUG
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, ALTO);
            Console.WriteLine("Puntos: " + puntos);
            if (DEBUG)
            {
                for(int i = 0;i < ANCHO; i++)
                {
                    Console.Write($"{techo[i],3:#0}");
                }
                Console.WriteLine("(techo)");
                for (int i = 0; i < ANCHO; i++)
                {
                    Console.Write($"{suelo[i],3:#0}");
                }
                Console.WriteLine("(suelo)");
                Console.WriteLine($"Pos. Bird: {fil, 5}   Frame: {frame, 5}");
            }
        }
        static void Avanza(int[] suelo, int[] techo, int frame)
        {//Nueva posición del techo y suelo para generar obstáculos cada cierto número de frames (SEP_OBS) de forma procedural
            int s, t; 
            if (frame % SEP_OBS == 0)
            {
                s = rnd.Next(0, (ALTO - 1) - HUECO);
                t = s + HUECO + 1;
            }
            else
            {
                s = 0;
                t = ALTO - 1;
            }
            for (int i = 0; i < ANCHO - 1; i++)
            {
                suelo[i] = suelo[i + 1];
                techo[i] = techo[i + 1];
            }
            techo[ANCHO - 1] = t;
            suelo[ANCHO - 1] = s;
        }
        static char LeeInput()
        {//Recoge el input
            char ch = ' ';
            if (Console.KeyAvailable)
            {
                string s = Console.ReadKey(true).Key.ToString();
                if (s == "X" || s == "Spacebar") ch = 'i'; // impulso                   
                else if (s == "P") ch = 'p'; // pausa					
                else if (s == "Q" || s == "Escape") ch = 'q'; // salir
                while (Console.KeyAvailable) Console.ReadKey();
            }
            return ch;
        }
        static void Mueve(char ch, ref int fil, ref int ascenso)
        {//Mueve el pájaro hacia abajo cada frame o hacia arriba si se recibe 'impulso' en el input
            if (ch == 'i')
            {
                ascenso = IMPULSO;
            }
            else
            {
                ascenso = -1;
            }
            fil = fil + ascenso;
        }
        static bool Colision(int[] suelo, int[] techo, int fil)
        {//Comprueba si la posición del pájaro coincide con el techo o el suelo y sus obstáculos
            bool colision = false;
            if (fil >= techo[COL_BIRD] || fil <= suelo[COL_BIRD]) colision = true;
            return colision;
        }
        static void Puntua(int[] suelo, int[] techo, ref int puntos)
        {//Añade un punto cada vez que se supera un obstáculo
            if (suelo[COL_BIRD] > 0 || techo[COL_BIRD] < ALTO - 1) puntos++;
        }
        static void GuardaJuego(string file, int[] suelo, int[] techo, int fil, int ascenso, int frame, int puntos)
        {//Genera un archivo con los datos de la partida que se ha guardado
            StreamWriter salida;
            salida = new StreamWriter(file);
                salida.WriteLine(fil);
                salida.WriteLine(ascenso);
                salida.WriteLine(frame);
                salida.WriteLine(puntos);

                int primero = 0;
                while (suelo[primero] == 0 && techo[primero] == ALTO - 1) primero++;
                salida.WriteLine(primero);

                for (int i = primero; i < ANCHO; i=i+SEP_OBS)
                {
                    salida.Write(suelo[i] + " " + techo[i] + " ");
                }
            salida.Close();
        }
        static void CargaJuego(string file, out int[] suelo, out int[] techo, out int fil, out int ascenso, out int frame, out int puntos, out bool colision)
        {
            StreamReader entrada;
            entrada = new StreamReader(file);
            fil = int.Parse(entrada.ReadLine());
            ascenso = int.Parse(entrada.ReadLine());
            frame = int.Parse(entrada.ReadLine());
            puntos = int.Parse(entrada.ReadLine());

            suelo = new int[ANCHO];
            techo = new int[ANCHO];
            for (int i = 0; i < ANCHO; i++)
            {
                techo[i] = ALTO - 1;
                suelo[i] = 0;
            }
            int primero = int.Parse(entrada.ReadLine());
            string s = new string(entrada.ReadLine());
            string[] v = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int pos = 0;
            for (int i = primero; i < ANCHO; i = i + SEP_OBS)
            {
                suelo[i] = int.Parse(v[pos]);
                pos++;
                techo[i] = int.Parse(v[pos]);
                pos++;
            }
            entrada.Close ();
            colision = false;
        }
    }
}