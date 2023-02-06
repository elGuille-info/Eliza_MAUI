using System.Diagnostics;
using System.Text;

using Eliza_gcnl;

namespace Eliza_MAUI;

public partial class ElizaMainPage : ContentPage
{
	public ElizaMainPage()
	{
		InitializeComponent();

        App.CopiarPalabrasEliza();

        PrimeraVez = true;
    }

    private Frases frase = null;
    private string text;
    private bool PrimeraVez;
    private string CopiaSalida;
    //private string CopiaAnalisis;

    //private const string CrLf = "\r\n";
    /// <summary>
    /// El retorno de carro según sea WinUI/Windows u otro sistema.
    /// </summary>
    public static string CrLf => DeviceInfo.Platform == DevicePlatform.WinUI ? "\r" : "\r\n";

    private string sNombre = "";
    private cEliza Eliza = null;
    private bool m_Terminado;
    private bool SesionGuardada = true;
    private string sEntradaAnterior = "";

    // El contenido de la sesión actual
    private readonly List<string> ColList1 = new();
    // Los nombres y el sexo
    private readonly Dictionary<string, cEliza.eSexo> ColList2 = new();

    private void ContentPage_Appearing(object sender, EventArgs e)
    {
        if (PrimeraVez)
        {
            PrimeraVez = false;
            //if (DeviceInfo.Platform != DevicePlatform.WinUI)
            {
                Inicializar();
            }
            BtnMostrar0.IsEnabled = false;
            // Esto no va, en Android da excepción
            // En Windows cxomo si no estuviera...
            //BtnNuevaSesion_Clicked(null, null);
            //BtnNuevaSesion.Focus();
        }
        // si frase no está asignada, deshabilitar los botones de mostrar
        bool habilitar = frase != null;
        HabilitarBotones(habilitar);
        //BtnMostrar0.IsEnabled = false;
    }

    private async void AnalizarTexo(string tmp)
    {
        if (string.IsNullOrEmpty(tmp)) return;

        text = tmp;

        HabilitarBotones(false);
        await Task.Run(() =>
        {
            MostrarAviso("Analizando el texto...", esError: false);
            frase = Frases.Add(text);
            QuitarAviso();
        });
        HabilitarBotones(true);
        CopiaSalida = txtSalida.Text;
        BtnMostrar0.IsEnabled = false;
    }

    private void HabilitarBotones(bool habilitar)
    {
        BtnMostrar1.IsEnabled = habilitar;
        BtnMostrar2.IsEnabled = habilitar;
        BtnMostrar3.IsEnabled = habilitar;
        BtnMostrar4.IsEnabled = habilitar;
        BtnMostrar5.IsEnabled = habilitar;
        BtnMostrar6.IsEnabled = habilitar;
    }

    private void BtnMostrar0_Clicked(object sender, EventArgs e)
    {
        // Este botón para mostrar el texto normal, ocultar el análisis
        if (!string.IsNullOrEmpty(CopiaSalida))
        {
            txtSalida.Text = CopiaSalida;
            BtnMostrar0.IsEnabled = false;
        }
    }

    private void BtnMostrar1_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(CopiaSalida)) CopiaSalida = txtSalida.Text;
        txtSalida.Text = frase.Analizar(conTokens: true, soloEntities: false);
        // Mostrar el texto desde el principio
        txtSalida.CursorPosition = 0;
        BtnMostrar0.IsEnabled = true;
    }

    private void BtnMostrar2_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(CopiaSalida)) CopiaSalida = txtSalida.Text;
        txtSalida.Text = frase.Analizar(conTokens: false, soloEntities: false);
        txtSalida.CursorPosition = 0;
        BtnMostrar0.IsEnabled = true;
    }

    private void BtnMostrar3_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(CopiaSalida)) CopiaSalida = txtSalida.Text;
        txtSalida.Text = frase.MostrarTokens();
        txtSalida.CursorPosition = 0;
        BtnMostrar0.IsEnabled = true;
    }

    private void BtnMostrar4_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(CopiaSalida)) CopiaSalida = txtSalida.Text;
        txtSalida.Text = frase.Analizar(conTokens: false, soloEntities: true);
        txtSalida.CursorPosition = 0;
        BtnMostrar0.IsEnabled = true;
    }

    private void BtnMostrar5_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(CopiaSalida)) CopiaSalida = txtSalida.Text;
        txtSalida.Text = Frases.MostrarResumen(true);
        txtSalida.CursorPosition = 0;
        BtnMostrar0.IsEnabled = true;
    }

    private void BtnMostrar6_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(CopiaSalida)) CopiaSalida = txtSalida.Text;
        txtSalida.Text = Frases.MostrarResumen(false);
        txtSalida.CursorPosition = 0;
        BtnMostrar0.IsEnabled = true;
    }

    private void QuitarAviso()
    {
        LabelAviso.Dispatcher.Dispatch(() => { LabelAviso.IsVisible = false; });
        GrbAviso.Dispatcher.Dispatch(() => { GrbAviso.BackgroundColor = Colors.Transparent; });
    }

    private void MostrarAviso(string aviso, bool esError)
    {
        GrbAviso.Dispatcher.Dispatch(() =>
        {
            GrbAviso.BackgroundColor = esError ? Colors.Firebrick : Colors.SteelBlue;
        });

        LabelAviso.Dispatcher.Dispatch(() =>
        {
            LabelAviso.Text = aviso;
            LabelAviso.IsVisible = true;
        });
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (LabelStatus.Text == LabelStatus.ClassId)
        {
            InfoTamañoVentana();
        }
        else
        {
            LabelStatus.Text = LabelStatus.ClassId;
        }
    }

    private void StackLayoutStatus_SizeChanged(object sender, EventArgs e)
    {
        // solo mostrarlo en Windows
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            InfoTamañoVentana();
        }
    }

    private void InfoTamañoVentana()
    {
        LabelStatus.Text = $"{LabelStatus.ClassId}{CrLf}Tamaño ventana Width: {(int)Width}, Height: {(int)Height}";
    }

    private async void BtnNuevaSesion_Clicked(object sender, EventArgs e)
    {
        if (Eliza == null) Inicializar();

        cEliza.eSexo tSexo = cEliza.eSexo.Masculino;
        string sSexo;
        string sMsgTmp;
        Random m_rnd = new();
        bool tmpSexo;

        // Si no se ha guardado la sesión anterior, preguntar si se quiere guardar.
        if (!SesionGuardada)
        {
            if (await DisplayAlert("Guardar la sesión actual", sNombre + " ¿Quieres guardar el contenido de la sesión actual?", "Sí", "No"))
            {
                GuardarSesion();
            }
        }
        SesionGuardada = true;

        txtSalida.Text = "";
        txtEntrada.Text = "> ";

        sMsgTmp = "";
        // Preguntar el nombre y el sexo
        do
        {
            // Si se pulsa en cancelar, se devuelve null
            sNombre = await DisplayPromptAsync("Saber quién eres",
                                               "Por favor dime tu nombre, o la forma en que quieres que te llame, " +
                                               "(deja la respuesta en blanco para terminar)",
                                               "Aceptar", "Cancelar", initialValue: sNombre);
            if (!string.IsNullOrEmpty(sNombre))
            {
                sNombre = sNombre.Trim();
            }
            if (string.IsNullOrEmpty(sNombre))
            {
                m_Terminado = true;
                sMsgTmp = "Adios, hasta la próxima sesión.";
                break;
            }
            m_Terminado = false;

            // Comprobar si está en la lista
            tmpSexo = false;
            foreach (var unNombre in ColList2)
            {
                if (unNombre.Key == sNombre)
                {
                    tSexo = unNombre.Value;
                    tmpSexo = true;
                    break;
                }
            }
            // Salir si se ha encontrado y está asignado el sexo
            if (tmpSexo && tSexo != cEliza.eSexo.Ninguno)
            {
                break;
            }
            // tener una serie de nombres para no pecar de "tonto"
            //sSexo = "masculino";
            //tSexo = cEliza.eSexo.Masculino;
            tSexo = SexoNombre();
            sSexo = tSexo == cEliza.eSexo.Femenino ? "femenino" : "masculino";
            if (tSexo == cEliza.eSexo.Ninguno)
            {
                if (sNombre.EndsWith("a"))
                {
                    sSexo = "femenino";
                    tSexo = cEliza.eSexo.Femenino;
                }
                else
                {
                    sSexo = "masculino";
                    tSexo = cEliza.eSexo.Masculino;
                }
                if (await DisplayAlert("Confirmar",
                                       sNombre + " por favor confirmame que tu sexo es: " + sSexo,
                                       "Sí", "No") == false)
                {
                    if (tSexo == cEliza.eSexo.Femenino)
                    {
                        tSexo = cEliza.eSexo.Masculino;
                        sSexo = "masculino";
                    }
                    else
                    {
                        tSexo = cEliza.eSexo.Femenino;
                        sSexo = "femenino";
                    }
                }
            }
            if (await DisplayAlert("Confirmar",
                                   "Por favor confirma que estos datos son correctos:" + CrLf +
                                   "Nombre: " + sNombre + CrLf +
                                   "Sexo: " + sSexo,
                                   "Sí", "No"))
            {
                break;
            }
        }
        while (true);
        // Si no se escribe el nombre, terminar el programa
        if (string.IsNullOrEmpty(sNombre))
        {
            if (string.IsNullOrEmpty(sMsgTmp))
            {
                sMsgTmp = "Me parece que no confías lo suficiente en mí, así que no hay sesión que valga, ¡ea!";
            }
            await DisplayAlert("Acabamos el programa", sMsgTmp, "Aceptar");
            App.Current.CloseWindow(this.Window);

            return;
        }

        // Comprobar si está en la lista
        tmpSexo = false;
        foreach (var unNombre in ColList2)
        {
            if (unNombre.Key == sNombre)
            {
                tmpSexo = true;
                break;
            }
        }
        if (tmpSexo == false)
        {
            // añadirlo
            ColList2.Add(sNombre, tSexo);
            // guardar los nombres
            //GuardarNombres();
        }
        // actualizarlo por si no se había definido el sexo previamente
        else
        {
            if (ColList2.ContainsKey(sNombre))
            {
                ColList2[sNombre] = tSexo;
            }
            // Este caso no debería darse, pero ya que he puesto la comprobación...
            else
            {
                ColList2.Add(sNombre, tSexo);
            }
        }
        // guardar siempre los nombres
        GuardarNombres();

        ColList1.Clear();
        ColList1.Add($"Sesión iniciada el: {DateTime.Now}");
        ColList1.Add("-----------------------------------------------");

        sMsgTmp = "Hola " + sNombre + ", soy Eliza para .NET MAUI";
        ImprimirDOS(sMsgTmp);
        sMsgTmp = "Por favor, intenta evitar los monosílabos y tutéame, yo así lo haré.";
        ImprimirDOS(sMsgTmp);

        //Cursor = System.Windows.Forms.Cursors.WaitCursor;

        // Inicializar los valores, mientras el usuario escribe
        if (Eliza.Iniciado)
        {
            // Si es la primera vez... dar un poco de tiempo.
            var unMensaje = m_rnd.Next(10);
            switch (unMensaje)
            {
                case > 6:
                    {
                        sMsgTmp = "Espera un momento, mientras ordeno mi base de datos...";
                        break;
                    }

                case > 3:
                    {
                        sMsgTmp = "Espera un momento, mientras busco un bolígrafo...";
                        break;
                    }

                default:
                    {
                        sMsgTmp = "Espera un momento, mientras lleno mis chips de conocimiento...";
                        break;
                    }
            }
            ImprimirDOS(sMsgTmp);
        }
        Eliza.Sexo = tSexo;
        Eliza.Nombre = sNombre;
        var sw = Stopwatch.StartNew();
        Eliza.Inicializar();
        sw.Stop();
        if (!Eliza.Iniciado)
        {
            ImprimirDOS($"Tiempo en inicializar (y asignar las palabras): {sw.Elapsed:mm\\:ss\\.fff}");
        }
        else
        {
            ImprimirDOS($"Tiempo en inicializar: {sw.Elapsed:mm\\:ss\\.fff}");
        }
        //Cursor = System.Windows.Forms.Cursors.Default;

        txtEntrada.Text = "> ";
        txtEntrada.CursorPosition = 2;

        ImprimirDOS("Vamos a ello..., ¿en qué puedo ayudarte?");
        if (txtEntrada.IsVisible)
        {
            txtEntrada.Focus();
        }
    }

    private void txtEntrada_Completed(object sender, EventArgs e)
    {
        txtEntrada.Text = txtEntrada.Text.Replace(CrLf, "");
        ProcesarEntrada();
    }

    // Funciones adicionales para ElizaMainPage

    #region Funciones adicionales para ElizaMainPage

    private void Inicializar()
    {
        // Inicializar las variables
        //txtSalida.Text = "Por favor, pulsa en 'Nueva sesión' para iniciar una nueva sesión.";
        txtEntrada.Text = "";

        SesionGuardada = true;

        ColList2.Clear();

        if (DateTime.Now.Year > 2023)
            LabelStatus.Text = "Eliza para .NET MAUI ©Guillermo Som (Guille), 1998-2002, 2023-" + DateTime.Now.Year.ToString();
        else
            LabelStatus.Text = "Eliza para .NET MAUI ©Guillermo Som (Guille), 1998-2002, 2023";
        // Copia del texto original
        LabelStatus.ClassId = LabelStatus.Text;

        Eliza = new cEliza(App.ElizaDatos)
        {
            Sexo = cEliza.eSexo.Ninguno
        };

        // leer la lista de nombres que han usado el programa
        LeerNombres();

        // Empezar una nueva sesión
        //await Task.Run(() => { BtnNuevaSesion_Clicked(null, null); });
        //BtnNuevaSesion_Clicked(null, null);
        //BtnNuevaSesion.Focus();
    }

    private void ProcesarEntrada()
    {
        // Toma lo que ha escrito el usuario y lo envía a la clase
        // para procesar lo escrito y obtener la respuesta
        string sTmp;

        if (string.IsNullOrEmpty(txtEntrada.Text))
            sTmp = "";
        else
            sTmp = txtEntrada.Text.Trim();

        // Sólo si se ha escrito algo
        if (string.IsNullOrEmpty(sTmp) == false)
        {
            txtEntrada.Text = "> ";
            txtEntrada.CursorPosition = 2;

            bool noRepitas = false;
            if (sEntradaAnterior == sTmp) { noRepitas = true; }

            // Mostrar el aviso que no te repitas si no es no, sí, etc.

            // Las palabras que se pueden repetir están en Eliza.SiNo
            // Simplificando:
            // si la respuesta tiene menos de 5 caracteres no considerarlo repetición
            if (sTmp.Length < 5) { noRepitas = false; }

            if (noRepitas)
            {
                ImprimirDOS("Por favor, no te repitas.");
            }
            else
            {
                // guardar la última entrada
                sEntradaAnterior = sTmp;
                // mostrar en la lista lo que se ha escrito
                ImprimirDOS(sTmp);
                if (sTmp.StartsWith("> "))
                {
                    //sTmp = sTmp.Substring(2).TrimStart();
                    sTmp = sTmp[2..].TrimStart();
                }
                // si se escribe ?, -?, --?, -h o --help			(24/ene/23 17.58)
                // mostrar la ayuda de comandos en principio ponerlo en modo consulta.
                var losHelp = new string[] { "?", "-?", "--?", "-h", "--h", "--help", "-help" };
                if (losHelp.Contains(sTmp))
                {
                    ImprimirDOS("No hay ayuda definida, puedes usar *consulta* para buscar en las palabras clave.");
                    return;
                }
                // Si se escribe *consulta*, usar lo que viene después
                // para buscar en las palabras claves de Eliza.
                // De esa comprobación se encarga cEliza.ProcesarEntrada
                // Una vez que se entra en el modo de consulta, lo que
                // se escriba se buscará en las claves y sub-claves,
                // para salir del modo consulta, hay que escribir de
                // nuevo *consulta*

                // Analizar el texto
                AnalizarTexo(sTmp);

                // Procesar la entrada del usuario y
                // mostrar la respuesta de Eliza
                sTmp = Eliza.ProcesarEntrada(sTmp);
                ImprimirDOS(sTmp);
                //Application.DoEvents();

                if (sTmp.StartsWith("adios", StringComparison.OrdinalIgnoreCase))
                {
                    sEntradaAnterior = "";
                    SesionGuardada = false;
                    m_Terminado = false;
                    BtnNuevaSesion_Clicked(null, null);
                }
                // Por ahora para que C# no de un warning...
                if (m_Terminado)
                {
                    sEntradaAnterior = "";
                    SesionGuardada = false;
                    m_Terminado = false;
                    BtnNuevaSesion_Clicked(null, null);
                }
            }
        }
        else
        {
            ImprimirDOS("¿Te lo estás pensando?");
        }

        SesionGuardada = false;
    }
    private async void GuardarSesion()
    {
        await Task.Run(() =>
        {
            string sDir = Path.Combine(App.ElizaDatos, "sesiones");
            var sFic = Path.Combine(sDir, $"{sNombre}_{DateTime.Now:ddMMMyyyy_HHmm}.txt");

            MostrarAviso("Guardando la sesión actual...", esError: false);

            ColList1.Add("-----------------------------------------------");
            ColList1.Add($"Sesión guardada el: {DateTime.Now:dddd, dd/MMM/yyyy HH:mm}");

            // Crear el directorio si no existe
            if (Directory.Exists(sDir) == false) { Directory.CreateDirectory(sDir); }

            using StreamWriter sw = new(sFic, false, Encoding.UTF8);
            foreach (var s in ColList1) { sw.WriteLine(s); }

            QuitarAviso();
        });
    }

    private void LeerNombres()
    {
        string sFic = Path.Combine(App.ElizaDatos, "ListaDeNombres.txt");
        if (File.Exists(sFic))
        {
            ColList2.Clear();
            using StreamReader sr = new(sFic, Encoding.UTF8, true);
            while (!sr.EndOfStream)
            {
                cEliza.eSexo elSexo = cEliza.eSexo.Ninguno;
                var tmpNombre = sr.ReadLine();
                if (string.IsNullOrEmpty(sNombre))
                {
                    sNombre = tmpNombre;
                }
                if (sr.EndOfStream == false)
                {
                    string tmpSexo = sr.ReadLine().Trim().ToLower();
                    if (tmpSexo == "masculino") elSexo = cEliza.eSexo.Masculino;
                    else if (tmpSexo == "femenino") elSexo = cEliza.eSexo.Femenino;
                }
                ColList2.Add(tmpNombre, elSexo);
            }
        }
    }

    private void GuardarNombres()
    {
        string sFic = Path.Combine(App.ElizaDatos, "ListaDeNombres.txt");
        using StreamWriter sw = new(sFic, false, Encoding.UTF8);
        foreach (var unNombre in ColList2)
        {
            sw.WriteLine(unNombre.Key);
            sw.WriteLine(unNombre.Value.ToString());
        }
    }
    private void ImprimirDOS(string sText, bool NuevaLinea = true)
    {
        // Imprimir el texto de entrada en el TextBox de salida
        // Si se NuevaLinea tiene un valor True (valor por defecto)
        // lo siguiente que se imprima se hará en una nueva línea

        txtSalida.Dispatcher.Dispatch(() =>
        {
            string s = txtSalida.Text + sText;
            if (NuevaLinea) { s += CrLf; }
            txtSalida.Text = s;
            ColList1.Add(sText);
            // Posicionar el cursor al final de la caja de texto
            txtSalida.CursorPosition = s.Length;
            txtSalida.SelectionLength = 0;
        });
    }
    private cEliza.eSexo SexoNombre()
    {
        // devolverá el sexo según el nombre introducido
        cEliza.eSexo tSexo;

        var Mujeres = new[] { " adela", " alicia", " amalia", " amanda", " " + "ana", " anita", " asunción", " aurora", " belinda", " " + "berioska", " carmen", " carmeli", " caty", " celia", " " + "delia", " diana", " dolores", " elena", " elisa", " " + "eva", " felisa", " gabriela", " gemma", " guillermina", " " + "inma", " isa", " josef", " julia", " juana", " juanita", " " + "laura", " luisa", " maite", " manoli", " manuela", " mari", " " + "maría", " marta", " merce", " mónica", " nadia", " " + "paqui", " pepa", " rita", " rosa", " sara", " silvia", " " + "sonia", " susan", " svetlana", " tere", " vane", " " + "vero", " verónica", " vivian" };
        var Hombres = new[] { " adán", " alvaro", " andrés", " bartolo", " " + "borja", " cándido", " carlos", " dámaso", " damián", " " + "daniel", " darío", " félix", " gabriel", " guillermo", " " + "harvey", " jaime", " javier", " joaquín", " joe", " jorge", " " + "jose", " josé", " juan", " luis", " manuel", " miguel", " " + "pepe", " ramón", " santiago", " tomás" };
        // masculino si acaba con 'o', femenino si acaba con 'a'
        var Ambos = new[] { " albert", " antoni", " armand", " bernard", " " + "carmel", " dionisi", " ernest", " fernand", " " + "francisc", " gerard", " ignaci", " manol", " maurici", " " + "pac", " rosend", " venanci" };

        sNombre = " " + sNombre.Trim();
        tSexo = cEliza.eSexo.Ninguno;
        for (int i = 0; i < Mujeres.Length; i++)
        {
            if (sNombre.IndexOf(Mujeres[i]) > -1)
            {
                tSexo = cEliza.eSexo.Femenino;
                break;
            }
        }
        if (tSexo == cEliza.eSexo.Ninguno)
        {
            for (int i = 0; i < Hombres.Length; i++)
            {
                if (sNombre.IndexOf(Hombres[i]) > -1)
                {
                    tSexo = cEliza.eSexo.Masculino;
                    break;
                }
            }
        }
        if (tSexo == cEliza.eSexo.Ninguno)
        {
            for (int i = 0; i < Ambos.Length; i++)
            {
                if (sNombre.IndexOf(Ambos[i] + "a") > -1)
                {
                    tSexo = cEliza.eSexo.Femenino;
                    break;
                }
                else if (sNombre.IndexOf(Ambos[i] + "o") > -1)
                {
                    tSexo = cEliza.eSexo.Masculino;
                    break;
                }
            }
        }
        sNombre = sNombre.Trim();
        return tSexo;
    }

    #endregion

}