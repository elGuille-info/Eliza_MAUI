// ------------------------------------------------------------------------------
// cEliza                                                      (22/ene/23 10.08)
// Versión para .net 7.0
// 
// ©Guillermo Som (Guille), 1998-2002, 2023
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliza_gcnl;

public class cEliza
{
    /// <summary>
    /// El directorio de datos, debe estar en LocalApplicationData\Eliza.
    /// </summary>
    public string AppDataPath { get; private set; }

    /// <summary>
    /// Crear la instancia de Eliza.
    /// </summary>
    /// <param name="appDataPath">Directorio con los datos, debe estar en LocalApplicationData\Eliza</param>
    public cEliza(string appDataPath)
    {
        m_Sexo = eSexo.Masculino;
        Iniciado = false;
        m_Releer = false;
        this.AppDataPath = appDataPath;
    }

    private readonly Random m_rnd = new Random();

    public enum eTiposDeClaves
    {
        eClaves // = 0
,
        eExtras // = 1
,
        eExtras2 // = 2
,
        eVerbos // = 3
,
        eRS // = 4
,
        eSimp // = 5
,
        eRec // = 6
,
        eBU // = 7
    }

    // Para la base de datos del usuario                                 (14/Jun/98)
    private readonly cRespuestas m_colBaseUser = new cRespuestas();
    public cRespuestas ColBaseUser
    {
        get
        {
            return m_colBaseUser;
        }
    }

    private string sUsarBaseDatos;

    // Para interactuar con el usuario, se usará un array cuando se le
    // haga una pregunta a la que deberá responder con dos tipos de
    // respuesta, normalmente una positiva y otra negativa
    const int cAfirmativa = 1;
    const int cNegativa = 0;
    // de cero a uno
    private readonly string[] sRespuestas = new string[2];
    // Esta variable servirá de 'flag' y contendrá la condición que
    // usará Eliza para evaluar una "respuesta" que contenga "{*iif("
    private string sUsarPregunta;

    // Private m_Iniciado As Boolean
    private bool m_Releer;

    // Para la revisión 00.06.00
    // Este array tendrá cada palabra de la frase original
    // Private FraseOrig As String()
    private readonly List<string> FraseOrig = new List<string>();
    // el número de la palabra actual
    private int PalabraOrig;
    // aunque el número de palabras se puede conseguir con UBound(array)
    // es conveniente mantener una variable, entre otras cosas para saber
    // cuál es la que se añade
    // Private PalabrasOrig As Integer

    public string Nombre;

    // Para indicar si es del sexo femenino o masculino
    // (por defecto es masculino)
    public enum eSexo
    {
        Masculino   // = 0
,
        Femenino    // = 1
,
        Ninguno     // = 2
    }
    private eSexo m_Sexo;

    // Evento para indicar que el usuario se ha despedido
    // Public Event Terminado()

    // Colección para recordar lo que ha dicho el usuario                (12/Jun/98)
    private readonly cRespuestas m_colRec = new cRespuestas();
    public cRespuestas ColRec
    {
        get
        {
            return m_colRec;
        }
    }
    // colección para los verbos
    private readonly cRespuestas m_Verbos = new cRespuestas();
    public cRespuestas ColVerbos
    {
        get
        {
            return m_Verbos;
        }
    }
    // Colección para las palabras de las reglas de simplificación
    // Estas serán las que se usarán en SustituirEnEntrada
    private readonly cRespuestas m_colRS = new cRespuestas();
    public cRespuestas ColRS
    {
        get
        {
            return m_colRS;
        }
    }
    // Estas serán las que se usarán en SimplificarEntrada
    private readonly cRespuestas m_colSimp = new cRespuestas();
    public cRespuestas ColSimp
    {
        get
        {
            return m_colSimp;
        }
    }

    private static readonly string sSeparadores = " .,;:¿?¡!()[]/-\t\"\r\n";

    // Colección de Reglas
    // Private ReadOnly m_colReglas As New Dictionary(Of String, cRegla)
    private readonly cRegla lasReglas = new cRegla("Eliza");

    /// <summary>
    /// Las colección con las reglas
    /// </summary>
    public Dictionary<string, cRegla> ColReglas
    {
        get
        {
            return lasReglas.Reglas; // m_colReglas
        }
    }

    private bool ModoConsulta; // para no usar static en ProcesarEntrada (27/ene/23 11.17)

    // Esto es lo que en realidad hay que revisar porque no lo analiza... (24/ene/23 13.37)
    public string ProcesarEntrada(string sEntrada)
    {
        // Se procesará la entrada del usuario y devolverá la cadena con la respuesta
        // A este procedimiento se llamará desde el formulario,
        // después de que el usuario escriba.

        string sEntradaSimp;
        string sClaves = "";
        string sCopiaEntrada;
        // Static ModoConsulta As Boolean

        int i;
        string sPalabra = "";

        // Convertirla en minúscula
        // (no es necesario ya que se usa Option Compare Text)
        // Eso funcionaba bien en VB, pero en .NET es un lío,    (24/ene/23 11.02)
        // Tengo que usar StringComparison.OrdinalIgnoreCase para hacer las comprobaciones.
        // sEntrada = LCase(sEntrada)

        // Si se escribe *consulta* usar lo que viene a continuación
        // para buscar en las claves y subclaves

        // Quitar los dobles espacios que haya
        sEntrada = QuitarEspaciosExtras(sEntrada);

        if (string.IsNullOrEmpty(sEntrada) || sEntrada == ">")
            return "Por favor escribe algo, gracias.";

        if (sEntrada.StartsWith("*consulta*", StringComparison.OrdinalIgnoreCase))
        {
            ModoConsulta = !ModoConsulta;
            if (ModoConsulta)
                sEntradaSimp = "Escribe la clave o sub-clave a comprobar, para terminar de consultar, escribe nuevamente *consulta*";
            else
                sEntradaSimp = "Salimos del modo consulta, ya puedes continuar normalmente.";
            return sEntradaSimp;
        }
        if (ModoConsulta)
        {
            // buscar la clave y mostrar si existe o no
            // si existe, mostrar la siguiente respuesta
            sClaves = BuscarEsaClave(sEntrada);
            if (string.IsNullOrEmpty(sClaves) == false)
                return "Respuesta: " + sClaves;
            return "No existe '" + sEntrada + "' en las claves y sub-claves";
        }

        // Quitar los signos de separación del principio
        // Do While sSeparadores.IndexOf(sEntrada.Substring(0, 1), StringComparison.OrdinalIgnoreCase) > -1
        while (sSeparadores.IndexOf(sEntrada.Substring(0, 1)) > -1)
        {
            sEntrada = sEntrada.Substring(1);
            // Por si se han escito solo separadores             (24/ene/23 17.42)
            if (string.IsNullOrEmpty(sEntrada))
                return "Por favor escribe algo (aparte de signos o espacios), gracias.";
        }
        // Quitar también los del final                                  (16/Sep/02)
        // Do While sSeparadores.IndexOf(RightN(sEntrada, 1), StringComparison.OrdinalIgnoreCase) > -1
        while (sSeparadores.IndexOf(RightN(sEntrada, 1)) > -1)
            // sEntrada = Left$(sEntrada, Len(sEntrada) - 1)
            sEntrada = sEntrada.Substring(0, sEntrada.Length - 1);

        // Si hay que usar una respuesta, procesar aquí para el caso de
        // que esa respuesta sea sí o no...
        if (string.IsNullOrEmpty(sUsarPregunta) == false)
        {
            // Se puede usar *afirmativo*, *true*
            if (sUsarPregunta.IndexOf("*afirmativo*", StringComparison.OrdinalIgnoreCase) > -1 || sUsarPregunta.IndexOf("*true*", StringComparison.OrdinalIgnoreCase) > -1)
            {
                // Comprobar si el contenido de sEntrada es una respuesta
                // afirmativa
                var res = EsRespuestaNegativaPositiva(sEntrada, esNegativo: false).Trim();
                // Si es una cadena vacía, no es positiva
                if (string.IsNullOrEmpty(res))
                {
                    res = EsRespuestaNegativaPositiva(sEntrada, esNegativo: true).Trim();
                    // Si es una cadena vacía, no es negativa
                    if (string.IsNullOrEmpty(res))
                        sUsarPregunta = sEntrada;
                    else
                        sUsarPregunta = sRespuestas[cNegativa];
                }
                else
                {
                    sUsarPregunta = sRespuestas[cAfirmativa];
                    // Puede que después de la parte afirmativa indique lo que es (27/ene/23 16.57)
                    i = sEntrada.IndexOf(res);
                    if (i > -1)
                    {
                        // sCopiaEntrada = sEntrada
                        sEntrada = sEntrada.Substring(i + res.Length).Trim();
                        sEntrada = QuitarSeparadores(sEntrada, delPrincipio: true);
                        i = sUsarPregunta.IndexOf("{*base:=", StringComparison.OrdinalIgnoreCase);
                        if (i > -1)
                            // Tomar lo que haya después de {*base:=...} sin la llave final
                            sUsarBaseDatos = sUsarPregunta.Substring(i + "{*base:=".Length).TrimEnd('}');
                    }
                }
            }
            else
                // procesar la comparación
                // para probar se asume que es negativa
                if (EsNegativo(sEntrada))
                sUsarPregunta = sRespuestas[cNegativa];
            else if (EsAfirmativo(sEntrada))
                sUsarPregunta = sRespuestas[cAfirmativa];
            else
                sUsarPregunta = sEntrada;
            // Comprobar si sUsarPregunta contiene *equal:=
            // de ser así, buscar la respuesta adecuada,
            // en otro caso esa es la respuesta
            i = sUsarPregunta.IndexOf("*equal:=", StringComparison.OrdinalIgnoreCase);
            while (i > -1) // i = 1
            {
                // sPalabra = LTrim$(Mid$(sUsarPregunta, i + 8))
                sPalabra = sUsarPregunta.Substring(i + 8).TrimStart();
                cRegla tRegla;

                if (ColReglas.ContainsKey(sPalabra))
                    tRegla = ColReglas[sPalabra];
                else
                    tRegla = null;
                if (tRegla == null)
                    sUsarPregunta = BuscarReglas(sPalabra, tRegla);
                else
                    // No existe como clave principal,
                    // hay que buscarlo en las sub-claves
                    foreach (var tRegla1 in ColReglas.Values)
                    {
                        sUsarPregunta = BuscarReglas(sPalabra, tRegla1);
                        if (string.IsNullOrEmpty(sUsarPregunta) == false)
                            break;
                    }
                i = sUsarPregunta.IndexOf("*equal:=", StringComparison.OrdinalIgnoreCase);
            }
            i = sUsarPregunta.IndexOf("{*iif", StringComparison.OrdinalIgnoreCase);
            // Si el usuario no ha contestado con lo esperado
            // If sUsarPregunta <> sEntrada Then
            if (string.IsNullOrEmpty(sUsarBaseDatos) && sUsarPregunta != sEntrada)
            {
                var res = ComprobarEspeciales(sUsarPregunta, sEntrada, sPalabra);
                if (i == -1)
                    sUsarPregunta = "";
                return res;
            }
        }

        if (string.IsNullOrEmpty(sUsarBaseDatos) == false)
        {
            // antes de almacenar los datos, se debería chequear
            // para que el usuario no nos de 'datos erróneos'
            // Por ejemplo si se le pregunta el signo del zodiaco
            // que no nos diga cualquier cosa...
            sCopiaEntrada = sUsarBaseDatos;
            sUsarBaseDatos = ValidarDatosParaBase(sEntrada);
            // Si el valor es diferente es que se ha modificado,
            // para indicarle algo al usuario
            if (sUsarBaseDatos != sEntrada)
                return sUsarBaseDatos;
            sUsarBaseDatos = "";
            // Aquí debería salir, para que no diga una chorrada
            // cuando el usuario contesta

            // If CInt(Rnd() * 5) > 2 Then
            if (m_rnd.Next(6) > 2)
                sCopiaEntrada = "Gracias por indicarme tu " + sCopiaEntrada;
            else
                sCopiaEntrada = "Gracias por decirme que tu " + sCopiaEntrada + " es " + sEntrada;
            return sCopiaEntrada + ".";
        }

        // quitar los "sí," y "no," del principio
        if (sEntrada.StartsWith("sí,", StringComparison.OrdinalIgnoreCase))
            sEntrada = sEntrada.Substring(4).TrimStart();
        if (sEntrada.StartsWith("no,", StringComparison.OrdinalIgnoreCase))
            sEntrada = sEntrada.Substring(4).TrimStart();

        // sCopiaEntrada = sEntrada

        // Convertir la entrada en un array de palabras       (07/Jun/98)
        // En este procedimiento es dónde se asignarán las palabras que
        // debe recordar de lo que ha dicho el usuario        (12/Jun/98)
        Entrada2Array(sEntrada);

        // Sustituir las palabras de simplificación,
        // ahora ya no se usan todas, ni los verbos, eso se hará sólo
        // cuando haya que usar parte de la entrada del usuario
        // en la respuesta, es decir donde está *RESTO*
        sEntradaSimp = SustituirEnEntrada(sEntrada);

        // Buscar las claves incluidas en la entrada y devolverlas
        // ordenadas según el Nivel
        // sClaves = BuscarClaves(sEntradaSimp)
        // ---Ahora devolverá el Nivel más alto usado
        // sClaves se pasará como parámetro
        BuscarClaves(sEntradaSimp, ref sClaves);
        return CrearRespuesta(sClaves, sEntradaSimp);
    }

    private static string QuitarSeparadores(string sEntrada, bool delPrincipio)
    {
        if (delPrincipio)
        {
            // Do While sEntrada.Substring(0, 1).IndexOfAny(sSeparadores.ToCharArray()) > -1
            // sEntrada = sEntrada.Substring(1)
            // Loop
            // Quitar los signos de separación del principio
            while (sSeparadores.IndexOf(sEntrada.Substring(0, 1)) > -1)
                sEntrada = sEntrada.Substring(1);
        }
        else
            // Quitar los signos de separación del final
            while (sSeparadores.IndexOf(RightN(sEntrada, 1)) > -1)
                sEntrada = sEntrada.Substring(0, sEntrada.Length - 1);
        return sEntrada;
    }

    public async void Inicializar()
    {
        // Pone a cero todos los valores de la última respuesta usada

        if (m_Releer)
        {
            m_Releer = false;
            // m_Iniciado = False
            Iniciado = false;
        }

        if (!Iniciado)
        {
            Iniciado = true;
            // Leer el fichero de palabras y respuestas
            await Task.Run(() => { LeerReglasEliza(); });
            //LeerReglasEliza();
        }
        else
            // Inicializar el valor del último item usado
            foreach (var tRegla in ColReglas.Values)
            {
                // poner a cero las respuestas normales
                tRegla.Respuestas.UltimoItem = 0;
                // poner a cero las respuestas de la sección Extras
                foreach (var tRespuestas in tRegla.Extras.Valores)
                    tRespuestas.UltimoItem = 0;
            }
        // Se considera siempre que es un usuario nuevo, leer la base de datos del usuario
        DatosUsuario();
    }

    private string SustituirEnEntrada(string sEntrada)
    {
        // Cambia las palabras de la entrada por las de la colección
        // de palabras simplificadas
        string sPalabra;
        string sPalabra1;
        string sPalabra1Ant;
        string sSeparador = "";
        string sSeparador1 = "";
        System.Text.StringBuilder nuevaEntrada = new System.Text.StringBuilder();

        do
        {
            // Buscar dos palabras seguidas
            // En sPalabra estará la primera palabra antes de un separador,
            // sEntrada tendrá el resto y sSeparador el separador.
            sPalabra = SiguientePalabra(ref sEntrada, ref sSeparador);
            sPalabra1 = SiguientePalabra(ref sEntrada, ref sSeparador1);
            sPalabra1Ant = sPalabra1;
            PalabraOrig += 1;
            if (string.IsNullOrEmpty(sPalabra) == false && string.IsNullOrEmpty(sPalabra1) == false)
            {
                // Si existen las dos palabras juntas
                if (m_colRS.ExisteItem(sPalabra + sSeparador + sPalabra1))
                {
                    sPalabra = m_colRS.Item(sPalabra + sSeparador + sPalabra1).Contenido;
                    nuevaEntrada.Append(sPalabra);
                    nuevaEntrada.Append(sSeparador1);
                }
                else
                {
                    // sino, tomar la primera y seguir el proceso
                    if (m_colRS.ExisteItem(sPalabra))
                        sPalabra = m_colRS.Item(sPalabra).Contenido;
                    nuevaEntrada.Append(sPalabra);
                    nuevaEntrada.Append(sSeparador);
                    // dejar la entrada como estaba
                    // invertir la palabra, en caso de que antes se
                    // haya encontrado en los verbos
                    PalabraOrig -= 1;
                    sPalabra1 = sPalabra1Ant;
                    // Creo que no hay que invertir las palabras (24/ene/23 15.57)
                    sEntrada = sPalabra1 + sSeparador1 + sEntrada;
                }
            }
            else
            {
                // Sólo debería cumplirse esta cláusula
                if (string.IsNullOrEmpty(sPalabra) == false)
                {
                    if (m_colRS.ExisteItem(sPalabra))
                        sPalabra = m_colRS.Item(sPalabra).Contenido;
                    nuevaEntrada.Append(sPalabra);
                    nuevaEntrada.Append(sSeparador);
                }

                if (string.IsNullOrEmpty(sPalabra1) == false)
                {
                    if (m_colRS.ExisteItem(sPalabra1))
                        sPalabra1 = m_colRS.Item(sPalabra1).Contenido;
                    nuevaEntrada.Append(sPalabra1);
                    nuevaEntrada.Append(sSeparador1);
                }
            }
        }
        while (string.IsNullOrEmpty(sEntrada) == false);
        return nuevaEntrada.ToString();
    }

    private string SimplificarEntrada(string sEntrada)
    {
        // Cambia las palabras de la entrada por las de la colección
        // de palabras simplificadas
        string sPalabra;
        string sPalabra1;
        string sPalabra1Ant;
        string sSeparador = "";
        string sSeparador1 = "";
        string nuevaEntrada;

        nuevaEntrada = "";
        do
        {
            // Buscar dos palabras seguidas
            sPalabra = SiguientePalabra(ref sEntrada, ref sSeparador);
            sPalabra1 = SiguientePalabra(ref sEntrada, ref sSeparador1);
            sPalabra1Ant = sPalabra1;

            // ==========================================================
            // Creo que NO se debería dar por hallada una palabra
            // cuando se ha "conjugado", ya que puede que esté entre
            // las palabras claves y de esta forma no la encontraría
            // ==========================================================

            PalabraOrig += 1;
            sPalabra = ComprobarVerbos(sPalabra);
            if (string.IsNullOrEmpty(sPalabra1) == false)
            {
                PalabraOrig += 1;
                sPalabra1 = ComprobarVerbos(sPalabra1);
            }

            // De esta forma no funciona:
            // If Len(sPalabra) And Len(sPalabra1) Then
            if (string.IsNullOrEmpty(sPalabra) == false && string.IsNullOrEmpty(sPalabra1) == false)
            {
                // Si existen las dos palabras juntas
                if (m_colSimp.ExisteItem(sPalabra + sSeparador + sPalabra1))
                {
                    sPalabra = m_colSimp.Item(sPalabra + sSeparador + sPalabra1).Contenido;
                    nuevaEntrada = nuevaEntrada + sPalabra + sSeparador1;
                }
                else
                {
                    // sino, tomar la primera y seguir el proceso
                    if (m_colSimp.ExisteItem(sPalabra))
                        sPalabra = m_colSimp.Item(sPalabra).Contenido;
                    nuevaEntrada = nuevaEntrada + sPalabra + sSeparador;
                    // dejar la entrada como estaba
                    // invertir la palabra, en caso de que antes se
                    // haya encontrado en los verbos
                    PalabraOrig -= 1;
                    sPalabra1 = sPalabra1Ant;
                    // Creo que no hay que invertir las palabras (24/ene/23 15.57)
                    sEntrada = sPalabra1 + sSeparador1 + sEntrada;
                }
            }
            else
            {
                // Sólo debería cumplirse esta cláusula
                if (string.IsNullOrEmpty(sPalabra) == false)
                {
                    if (m_colSimp.ExisteItem(sPalabra))
                        sPalabra = m_colSimp.Item(sPalabra).Contenido;
                    nuevaEntrada = nuevaEntrada + sPalabra + sSeparador;
                }

                if (string.IsNullOrEmpty(sPalabra1) == false)
                {
                    if (m_colSimp.ExisteItem(sPalabra1))
                        sPalabra1 = m_colSimp.Item(sPalabra1).Contenido;
                    nuevaEntrada = nuevaEntrada + sPalabra1 + sSeparador1;
                }
            }
        }
        while (string.IsNullOrEmpty(sEntrada) == false);
        return nuevaEntrada;
    }

    private int BuscarClaves(string sEntrada, ref string sClaves)
    {
        // Devuelve las claves halladas
        // y asigna la variable Claves, devolviendo el número de claves halladas
        string sPalabra;

        sClaves = "";
        
        // Me ha dado error de que se ha modificado la colección (03/feb/23 21.09)
        //  Da el error tanto con foreach como con el for normal.
        // El fallo lo da en Android, en la tablet, no en el móvil (en Windows no puedo comprobarlo)
        //for (int r = 0; r < ColReglas.Values.Count; r++)
        // { var tRegla = ColReglas.Values.ElementAt(r);
        foreach (var tRegla in ColReglas.Values)
        {
            //var tRegla = ColReglas.Values.ElementAt(r);

            int i = sEntrada.IndexOf(tRegla.Contenido, StringComparison.OrdinalIgnoreCase);
            if (i > -1)
            {
                // comprobar si es una palabra completa
                if (sSeparadores.IndexOf(sEntrada.Substring(i + tRegla.Contenido.Length, 1)) > -1)
                {
                    // Si el carácter anterior es un separador o el principio de la palabra
                    if (i > 0)
                    {
                        var i1 = i;
                        i = sSeparadores.IndexOf(sEntrada.Substring(i1 - 1, 1));
                    }
                    if (i > -1)
                    {
                        foreach (var tRespuestas in tRegla.Extras.Valores)
                        {
                            i = sEntrada.IndexOf(tRespuestas.Contenido, StringComparison.OrdinalIgnoreCase);
                            if (i > -1)
                            {
                                // Añadir al principio las "subclaves"
                                // aunque no es necesario, ya que se le da un nivel mayor
                                if (sSeparadores.IndexOf(sEntrada.Substring(i + tRespuestas.Contenido.Length, 1), StringComparison.OrdinalIgnoreCase) > -1)
                                {
                                    sPalabra = tRespuestas.Contenido;
                                    sClaves = "{" + tRegla.Nivel + 1 + "}" + sPalabra + "," + sClaves;
                                }
                            }
                        }
                        sPalabra = tRegla.Contenido;
                        sClaves = sClaves + "{" + tRegla.Nivel + "}" + sPalabra + ",";
                    }
                }
            }
        }
        // Ordenar las claves
        // ---Ahora devuelve la de Nivel mayor al principio
        return OrdenarClaves(ref sClaves);
    }

    private static int OrdenarClaves(ref string sClaves)
    {
        // Ordena las claves según el Nivel
        // El nivel estará indicado entre llaves y cada palabra separada por una coma.
        // 
        // Nota: Esta forma de indicar el nivel no es como se hace en el fichero de palabras,
        // ya que en el fichero se indica después de la palabra que está entre corchetes.
        // Es la forma que se hace al llamar a esta función.

        int numPalabras;
        int i;
        cRespuestas tContenidos;
        string sNivel;
        string sPalabra;
        string[] aPalabras;
        System.Text.StringBuilder sTmp = new System.Text.StringBuilder();
        int LaMayor = 0;

        tContenidos = new cRespuestas();

        // sTmp = sClaves
        sTmp.Append(sClaves);
        do
        {
            i = sClaves.IndexOf("}");
            if (i > -1)
            {
                sNivel = sClaves.Substring(0, i);
                sClaves = sClaves.Substring(i + 1);
                i = sClaves.IndexOf(",");
                if (i > -1)
                {
                    sPalabra = sClaves.Substring(0, i);
                    sClaves = sClaves.Substring(i + 1);
                    // asignar como ID el nivel y como contenido la palabra, 26/ene/23 13.54
                    // tContenidos.Item(sPalabra).Contenido = sNivel
                    tContenidos.Item(sNivel).Contenido = sPalabra;
                }
            }
        }
        while (string.IsNullOrEmpty(sClaves) == false);
        numPalabras = tContenidos.Count;
        if (numPalabras > 0)
        {
            aPalabras = new string[numPalabras];
            for (i = 0; i <= numPalabras - 1; i++)
                // se asignará el nivel, (antes se añadía la palabra)
                // aPalabras(i) = tContenidos.Item(i).Contenido
                aPalabras[i] = tContenidos.Item(i).ID;
            
            // Clasificar de mayor a menor                               (01/Jun/98)
            Clasificar tClasificar = new Clasificar(deMayorAMenor: true);
            Array.Sort(aPalabras, tClasificar);

            // LaMayor = Val(Mid$(tContenidos(aOrden(1)).Contenido, 2))
            // LaMayor = CInt(tContenidos.Item(0).Contenido.Substring(1))
            LaMayor = System.Convert.ToInt32(aPalabras[0].Substring(1));
            // sTmp = ""
            sTmp.Clear();
            for (i = 0; i <= numPalabras - 1; i++)
            {
                // sTmp = sTmp & tContenidos.Item(i).ID & ","
                // aquí hay que tener en cuenta el orden de aPalabras
                // ahora tContenido.ID es el nivel y tContenido.Contenido es la palabra
                // antes era: tContenido.ID la palabra, tContenido.Contenido el nivel
                var s1 = tContenidos.Item(aPalabras[i]);
                // sTmp.Append(tContenidos.Item(i).ID)
                sTmp.Append(s1.Contenido);
                sTmp.Append(',');
            }
        }
        sClaves = sTmp.ToString();
        // Se devuelve el Nivel mayor de las claves
        return LaMayor;
    }

    private string CrearRespuesta(string sClaves, string sEntrada)
    {
        // Devuelve la respuesta,
        // las claves estarán separadas por comas

        string sRespuesta = "";
        string sPalabra; // = ""

        // Habría que analizar todas las palabras                (26/ene/23 13.28)
        // y usar la que tenga mayor nivel.
        // de esa forma si se indica hola, 'algo más', puede que 'algo más' tenga mayor peso.

        // Ahora las palabras de más peso están al principio de las que hay. (26/ene/23 14.01)

        // Dim lasClaves As String()
        // tomar la primera palabra y buscar la respuesta adecuada
        // If String.IsNullOrEmpty(sClaves) Then
        // sPalabra = ""
        // Else
        // If sClaves.Contains(","c) Then
        // lasClaves = sClaves.Split(",", StringSplitOptions.RemoveEmptyEntries)
        // For n = 0 To lasClaves.Length - 1
        // sPalabra = lasClaves(n)

        // Next
        // Else
        // ' si no tiene coma, usar sClaves como la palabra    (26/ene/23 13.30)
        // sPalabra = sClaves
        // End If
        // End If

        var i = sClaves.IndexOf(",");
        if (i > -1)
            sPalabra = sClaves.Substring(0, i).Trim();
        else
            // si no tiene coma, usar sClaves como la palabra    (26/ene/23 13.30)
            sPalabra = sClaves;

        // Si no hay una palabra clave
        if (string.IsNullOrEmpty(sPalabra))
        {
            sPalabra = "respuestas-aleatorias";
            // en principio sólo "recordará" si no ha entendido

            // Crear una respuesta con algo que dijo antes...
            // si es que se tiene constancia de ello...
            sRespuesta = CrearRespuestaRecordando(sPalabra);
        }
        // Si no se ha encontrado una respuesta "aleatoria"
        if (string.IsNullOrEmpty(sRespuesta))
            // Aquí se puede usar la nueva función BuscarEsaClave        (11/Jun/98)
            sRespuesta = BuscarEsaClave(sPalabra);

        sRespuesta = ComprobarEspeciales(sRespuesta, sEntrada, sPalabra);

        // Ahora se comprueba en el propio formulario si se debe terminar. (26/ene/23)

        // Si la respuesta es la despedida, disparar un evento indicando que se termina
        // con adios no llega aquí, a ver si llega con quit que es lo que se indica en [*rs*]
        // sí, llega con quit                                    (24/ene/23 22.05)
        // 
        // If sRespuesta.StartsWith("adios", StringComparison.OrdinalIgnoreCase) Then
        // If sRespuesta.StartsWith("adios", StringComparison.OrdinalIgnoreCase) Then
        // If sRespuesta.StartsWith("quit", StringComparison.OrdinalIgnoreCase) Then
        // RaiseEvent Terminado()
        // End If

        return sRespuesta;
    }

    private static string QuitarCaracterEx(string sValor, string sCaracter, string sPoner = "")
    {
        // --------------------------------------------------------------------------
        // Cambiar/Quitar caracteres                                     (17/Sep/97)
        // Si se especifica sPoner, se cambiará por ese carácter
        // 
        // Esta versión permite cambiar los caracteres    (17/Sep/97)
        // y sustituirlos por el/los indicados
        // a diferencia de QuitarCaracter, no se buscan uno a uno,
        // sino todos juntos
        // --------------------------------------------------------------------------
        int i;
        string sCh = "";
        bool bPoner;
        int iLen;

        if (string.IsNullOrEmpty(sCaracter))
            return sValor;

        bPoner = false;
        // If Not IsMissing(sPoner) Then
        if (string.IsNullOrEmpty(sPoner) == false)
        {
            sCh = sPoner;
            bPoner = true;
        }

        // Esto no estaba...                                     (24/ene/23 21.46)
        iLen = sCaracter.Length;
        if (iLen == 0)
            return sValor;

        // Si el caracter a quitar/cambiar es Chr$(0), usar otro método
        // If AscW(sCaracter) = 0 Then
        // ' Quitar todos los chr$(0) del final
        // Do While RightN(sValor, 1) = ChrW(0)
        // sValor = sValor.Substring(0, sValor.Length - 1)
        // If sValor.Length = 0 Then Exit Do
        // Loop
        // iLen = 0 '1 usando Instr
        // Do
        // i = sValor.IndexOf(sCaracter, iLen)
        // If i > -1 Then
        // If bPoner Then
        // sValor = $"{sValor.Substring(0, i)}{sCh}{sValor.Substring(i + 1)}"
        // Else
        // sValor = $"{sValor.Substring(0, i)}{sValor.Substring(i + 1)}"
        // End If
        // iLen = i
        // Else
        // ' ya no hay más, salir del bucle
        // Exit Do
        // End If
        // Loop
        // Else
        i = 0; // 1
        // Do While i <= sValor.Length
        // Do While i < sValor.Length
        while (i + iLen < sValor.Length)
        {
            if (sValor.Substring(i, iLen) == sCaracter)
            {
                if (bPoner)
                {
                    sValor = $"{sValor.Substring(0, i)}{sCh}{sValor.Substring(i + iLen)}";
                    i -= 1;
                    // Si lo que hay que poner está incluido en
                    // lo que se busca, incrementar el puntero
                    // (11/Jun/98)
                    if (sCh.IndexOf(sCaracter) > -1)
                        i += 1;
                }
                else
                    sValor = $"{sValor.Substring(0, i)}{sValor.Substring(i + iLen)}";
            }
            i += 1;
        }
        // End If

        return sValor;
    }

    private string SiguientePalabra(ref string sFrase, ref string sSeparador, string queSeparador = "")
    {
        // Busca la siguiente palabra de la frase de entrada
        // En la frase se devolverá el resto sin la palabra hallada

        // Si se especifica un caracter (o varios) en queSeparador
        // se usarán esos para comprobar cual es la siguiente palabra,
        // sino, se usará el contenido de sSeparadores
        int i;
        System.Text.StringBuilder sPalabra = new System.Text.StringBuilder();
        string sLosSeparadores;

        // Nueva forma de comprobar una nueva palabra                    (10/Jun/98)
        if (string.IsNullOrEmpty(queSeparador) == false)
            sLosSeparadores = queSeparador;
        else
            sLosSeparadores = sSeparadores;

        // sPalabra = ""
        sFrase = sFrase.Trim() + " ";
        for (i = 0; i <= sFrase.Length - 1; i++)
        {
            if (sLosSeparadores.IndexOf(sFrase[i]) > -1)
            {
                //sSeparador = sFrase[i];
                sSeparador = new string(sFrase[i], 1);
                sFrase = sFrase.Substring(i + 1);
                break;
            }
            else
                sPalabra.Append(sFrase[i]);
        }
        return sPalabra.ToString();
    }

    /// <summary>
    /// Lee las reglas de los ficheros del directorio "palabras" desde LocalApplicationData y crea las reglas y sub-reglas.
    /// </summary>
    /// <remarks>En la app de .NET MAUI asegurarse de copiar los ficheros a ese directorio.</remarks>
    private void LeerReglasEliza()
    {
        // Leer el/los ficheros de palabras clave y respuestas,
        // así como las reglas de simplificación

        string sFic;
        string sDir;
        string[] otrosEliza;

        // El directorio de datos
        sDir = System.IO.Path.Combine(AppDataPath, "palabras");

        // El primer fichero en leer será Eliza_SPA.txt
        // El resto se leerán a continuación, permitiendo de esta forma
        // sustituir algunas reglas y palabras existentes
        sFic = System.IO.Path.Combine(sDir, "Eliza_SPA.txt");
        if (System.IO.File.Exists(sFic))
            LeerReglas(sFic);

        // Buscar los ficheros llamados ElizaSP_*.txt            (23/ene/23 09.39)
        otrosEliza = System.IO.Directory.GetFiles(sDir, "ElizaSP_*.txt");
        foreach (var sFic1 in otrosEliza)
            LeerReglas(sFic1);
        // Buscar los ficheros llamados ElizaVB_*.txt, y leerlos
        otrosEliza = System.IO.Directory.GetFiles(sDir, "ElizaVB_*.txt");
        foreach (var sFic1 in otrosEliza)
            LeerReglas(sFic1);

        // Convertir las claves que tienen el formato:
        // [clave {* xxx; yyy}] en distintas entradas.

        // Las diferentes entradas TIENEN QUE ESTAR separadas por ;

        cRegla tRegla;
        cRespuestas tRespuestas;
        string sPalabra;
        string sPalabra1;
        string sSeparador = "";
        int j;

        string sSubKey;
        int i;
        string sTmp;

        string sContenidoRegla;
        int posContenidoRegla;
        // Dim nContenidoRegla As Integer
        // Dim rContenidoRegla As cRegla = Nothing
        // Dim totalContenidoRegla As Integer = 0

        sSubKey = "";

        // 
        // No se puede hacer for each porque se añaden reglas
        // For Each tRegla In m_col.Values
        // De esta forma, aunque se añadan más reglas,
        // terminará cuando se llegue al valor original de m_col.Values.Count - 1
        // For n = 0 To m_col.Values.Count - 1
        // 

        // De esta forma se continúa aunque se añadan más reglas
        int n = -1;
        // Dim cuantasReglasInicial = m_col.Values.Count - 1
        var cuantasReglas = ColReglas.Values.Count - 1; // cuantasReglasInicial
        do
        {
            n += 1;
            // ajustar el límite por si se añaden nuevas reglas
            if (ColReglas.Values.Count - 1 > cuantasReglas)
                cuantasReglas = ColReglas.Values.Count - 1;
            if (n > cuantasReglas)
                break;
            tRegla = ColReglas.Values.ElementAt(n);

            // nContenidoRegla = 0

            // Comprobar si tiene {* ...}
            i = tRegla.Contenido.IndexOf("{*");
            if (i > -1)
            {
                // 
                // <Forma nueva 25/ago/23>
                // 
                // rContenidoRegla = New cRegla()
                sContenidoRegla = tRegla.Contenido;
                posContenidoRegla = i;
                while (posContenidoRegla > -1)
                {
                    // En el caso que se ponga alguna palabra después
                    // de la llave de cierre, se usará también
                    j = sContenidoRegla.IndexOf("}", posContenidoRegla);
                    sPalabra1 = "";
                    if (j > -1)
                    {
                        sPalabra1 = sContenidoRegla.Substring(j + 1).Trim();
                        if (sPalabra1.Length > 0)
                            sPalabra1 = " " + sPalabra1;
                    }

                    sSubKey = sContenidoRegla.Substring(0, posContenidoRegla);
                    sTmp = sContenidoRegla.Substring(i + 2);

                    // La siguiente palabra será la que esté separada por
                    // un punto y coma, de esta forma se permiten palabras
                    // de más de "una palabra"
                    sPalabra = SiguientePalabra(ref sTmp, ref sSeparador, ";");

                    // Esto es necesario, ya que en la colección la clave
                    // es el contenido anterior, es decir lo que hay en sKey

                    // Para que no hayan dos espacios seguidos en la clave
                    sPalabra = QuitarEspaciosExtras(sSubKey + sPalabra + sPalabra1);
                    {
                        var withBlock = lasReglas.Item(sPalabra);
                        withBlock.Nivel = tRegla.Nivel;
                        withBlock.Aleatorio = tRegla.Aleatorio;
                        withBlock.Respuestas.Add("*equal:=" + tRegla.Contenido);
                    }
                    // Para probar
                    // With rContenidoRegla.Item(sPalabra)
                    // .Nivel = tRegla.Nivel
                    // .Aleatorio = tRegla.Aleatorio
                    // .Respuestas.Add("*equal:=" & tRegla.Contenido)
                    // End With
                    // nContenidoRegla += 1
                    // totalContenidoRegla += 1

                    while (sTmp.Length > 0)
                    {
                        // Si no tiene este separador se devuelve lo mismo
                        sPalabra = SiguientePalabra(ref sTmp, ref sSeparador, ";");
                        if (sPalabra.Length > 0)
                        {
                            i = sPalabra.IndexOf("}");
                            if (i > -1)
                            {
                                sPalabra = sPalabra.Substring(0, i).Trim();
                                sTmp = "";
                            }

                            sPalabra = QuitarEspaciosExtras(sSubKey + sPalabra + sPalabra1);
                            {
                                var withBlock = lasReglas.Item(sPalabra);
                                withBlock.Nivel = tRegla.Nivel;
                                withBlock.Aleatorio = tRegla.Aleatorio;
                                withBlock.Respuestas.Add("*equal:=" + tRegla.Contenido);
                            }
                        }
                    }

                    i = sContenidoRegla.IndexOf("{*", posContenidoRegla + 2);
                    posContenidoRegla = i;
                }
            }

            // If nContenidoRegla > 0 Then
            // 'Debug.WriteLine("{0}, {1}", tRegla.Contenido, nContenidoRegla)
            // Debug.WriteLine("{0}, {1}, {2}", tRegla.Contenido, rContenidoRegla.LasReglas.Count, totalContenidoRegla)
            // End If

            // Buscar en las sub-claves *extras*

            // Si aquí se encuentra {* se deberá crear una clave
            // principal, ya que los *equal:= sólo funcionan con
            // las claves principales.
            // ---También busca en las sub-claves extras      (10/Jun/98)

            // For Each tRespuestas In tRegla.Extras.Valores.Values
            for (var m = 0; m <= tRegla.Extras.Valores.Count - 1; m++)
            {
                tRespuestas = tRegla.Extras.Valores.ElementAt(m);
                i = tRespuestas.Contenido.IndexOf("{*");
                if (i > -1)
                {
                    // En el caso que se ponga alguna palabra después
                    // de la llave de cierre, se usará también
                    j = tRespuestas.Contenido.IndexOf("}");
                    sPalabra1 = "";
                    if (j > -1)
                    {
                        sPalabra1 = tRespuestas.Contenido.Substring(j + 1).Trim();
                        if (sPalabra1.Length > 0)
                            sPalabra1 = " " + sPalabra1;
                    }

                    sSubKey = tRespuestas.Contenido.Substring(0, i);
                    sTmp = tRespuestas.Contenido.Substring(i + 2);
                    sPalabra = SiguientePalabra(ref sTmp, ref sSeparador, ";");
                    sPalabra = QuitarEspaciosExtras(sSubKey + sPalabra + sPalabra1);
                    lasReglas.Item(tRegla.Contenido).Extras.Item(sPalabra).Add("*equal:=" + tRespuestas.Contenido);
                    while (sTmp.Length > 0)
                    {
                        sPalabra = SiguientePalabra(ref sTmp, ref sSeparador, ";");
                        if (sPalabra.Length > 0)
                        {
                            i = sPalabra.IndexOf("}");
                            if (i > -1)
                            {
                                sPalabra = sPalabra.Substring(0, i).Trim();
                                sTmp = "";
                            }

                            sPalabra = QuitarEspaciosExtras(sSubKey + sPalabra + sPalabra1);
                            lasReglas.Item(tRegla.Contenido).Extras.Item(sPalabra).Add("*equal:=" + tRespuestas.Contenido);
                        }
                    }
                }
            }
        }
        while (true)// .Values.Count - 1// .Values.ElementAt(m)
; // Next
    }

    /// <summary>
    /// Lee las reglas del fichero indicado, sin analizarlas ni crear las sub-reglas.
    /// </summary>
    /// <param name="sFic">El path completo del fichero.</param>
    /// <remarks>El formato del fichero debe ser UTF8.</remarks>
    private void LeerReglas(string sFic)
    {
        // Leer el fichero de palabras indicado en el parámetro          (17/Sep/02)
        string sTmp = ";";
        string sTmpLower;
        string sKey = "";
        string sSubKey = "";
        int i;
        bool UsarRs;
        bool esPeek = false;

        // Comprobar si existe el fichero, si no es así, salir.
        // ¡No comprobarlo, ya que se altera lo que Dir$ devuelve!
        // (se supone que existe el fichero, ya que
        // este procedimiento es llamado desde LeerReglasEliza)
        // If Len(Dir$(sFic)) = 0 Then Exit Sub

        using (System.IO.StreamReader sr = new System.IO.StreamReader(sFic, System.Text.Encoding.UTF8, true))
        {
            while (!sr.EndOfStream)
            {
                // si es true es que no hay que leer el contenido
                if (esPeek == false)
                    sTmp = sr.ReadLine().Trim();
                esPeek = false;
                // Si no hay más entradas,
                // esto es por si se quiere crear un fichero de prueba
                // y se limitará hasta dónde se va a examinar...
                if (sTmp.StartsWith(";fin", StringComparison.OrdinalIgnoreCase))
                    break;
                // Si hay un comentario o está vacía, no procesar
                if (string.IsNullOrEmpty(sTmp) || sTmp.StartsWith(";"))
                    continue;
                sTmpLower = sTmp.ToLower();
                // Si no es una sección EXTRAS
                if (sTmpLower != "[*extras*]")
                {
                    // Si son reglas de simplificación
                    if (sTmpLower == "[*rs*]")
                    {
                        UsarRs = true;
                        // leer las reglas de simplificación,
                        // siempre deben ir por pares
                        while (!sr.EndOfStream)
                        {
                            // Las palabras estarán separadas por comas
                            sTmp = sr.ReadLine().Trim();
                            if (string.IsNullOrEmpty(sTmp) || sTmp.StartsWith(";"))
                                continue;
                            sTmpLower = sTmp.ToLower();
                            if (sTmpLower == "[/rs]")
                                break;
                            else if (sTmpLower == "[*simp*]")
                                UsarRs = false;
                            i = sTmp.IndexOf(",");
                            if (i > -1)
                            {
                                sKey = sTmp.Substring(0, i);
                                sTmp = sTmp.Substring(i + 1);
                                // Si tiene el signo @ al principio,
                                // se creará una doble entrada
                                // Por ejemplo: @soy,eres
                                // crearía soy,eres y eres,soy
                                if (sKey.StartsWith("@"))
                                {
                                    sKey = sKey.Substring(1);
                                    if (UsarRs)
                                        m_colRS.Item(sTmp).Contenido = sKey;
                                    else
                                        m_colSimp.Item(sTmp).Contenido = sKey;
                                }
                                // añadirla a la colección
                                if (UsarRs)
                                    m_colRS.Item(sKey).Contenido = sTmp;
                                else
                                    m_colSimp.Item(sKey).Contenido = sTmp;
                            }
                        }
                    }
                    else if (sTmpLower == "[*verbos*]")
                    {
                        // leer los verbos y sus terminaciones
                        while (!sr.EndOfStream)
                        {
                            // Se pondrán los verbos y se añadirá a la
                            // colección sin la terminación, en el formato
                            // m_Verbos.Item("am")="ar"
                            // m_Verbos.Item("com")="er"
                            sTmp = sr.ReadLine().Trim();
                            sTmpLower = sTmp.ToLower();
                            if (string.IsNullOrEmpty(sTmp) || sTmp.StartsWith(";"))
                                continue;
                            // If sTmp.Length > 0 AndAlso sTmp.Substring(0, 1) <> ";" Then
                            if (sTmpLower == "[/verbos]")
                                break;
                            i = sTmp.Length;
                            sKey = sTmp.Substring(0, i - 2);
                            sTmp = RightN(sTmp, 2);
                            if (sKey.StartsWith("%"))
                            {
                                // quitarle el %
                                sKey = sKey.Substring(1);
                                // cambiar la última vocal
                                // por una acentuada
                                // sólo se busca la 'i'
                                if (RightN(sKey, 1) == "i")
                                {
                                    sSubKey = sKey.Substring(0, sKey.Length - 1) + "í";
                                    m_Verbos.Item(sSubKey).Contenido = sTmp;
                                }
                                sSubKey = "";
                            }
                            // añadirlo a la colección
                            m_Verbos.Item(sKey).Contenido = sTmp;
                        }
                    }
                    else
                                            // debe ser una clave normal
                                            if (sTmp.StartsWith("["))
                    {
                        // Es una Palabra Clave
                        // Quitarle los corchetes
                        sKey = sTmp.Substring(1, sTmp.Length - 2);
                        sSubKey = "";
                        // Leer el nivel
                        sTmp = sr.ReadLine().Trim();
                        // Si tiene @ quitárselo antes de convertir en número
                        // ( 9/Jun/98)
                        // Si tiene @ es para tomarlo aleatoriamente
                        if (sTmp.EndsWith("@"))
                        {
                            sTmp = sTmp.Replace("@", "");
                            lasReglas.Item(sKey).Aleatorio = true;
                        }
                        else
                            lasReglas.Item(sKey).Aleatorio = false;

                        var n = 0;
                        _ = int.TryParse(sTmp, out n);
                        lasReglas.Item(sKey).Nivel = n;
                    }
                    else
                        // Sino es una clave, es una respuesta
                        // de la última clave encontrada.
                        // Añadirla a la clave actual
                        lasReglas.Item(sKey).Respuestas.Add(sTmp);
                }
                else
                    // Comprobar si hay palabras EXTRAS (subClaves)
                    while (!sr.EndOfStream)
                    {
                        // Guardar la posición actual del fichero
                        // pSeek = sr.Peek ' Seek(nFic)
                        // esPeek = True
                        sTmp = sr.ReadLine().Trim();
                        sTmpLower = sTmp.ToLower();
                        // si es el final de la sección de EXTRAS:
                        // salir del bucle
                        if (string.IsNullOrEmpty(sTmp) || sTmp.StartsWith(";"))
                            continue;
                        if (sTmpLower == "[/extras]")
                            break;
                        // Si es una clave, empezará por [
                        if (sTmp.StartsWith("["))
                        {
                            sSubKey = sTmp.Substring(1, sTmp.Length - 2);
                            lasReglas.Item(sKey).Extras.Item(sSubKey).Contenido = sSubKey;
                        }
                        else
                            // si no es una clave

                            // Si no hay subClave especificada:
                            if (string.IsNullOrEmpty(sSubKey))
                        {
                            // posicionar el puntero del fichero
                            // y salir del bucle
                            // Seek nFic, pSeek
                            esPeek = true;
                            break;
                        }
                        else
                            // debe ser una respuesta para esta subClave
                            lasReglas.Item(sKey).Extras.Item(sSubKey).Add(sTmp);
                    }
            }
        }
    }

    public eSexo Sexo
    {
        get
        {
            return m_Sexo;
        }
        set
        {
            if (value == eSexo.Ninguno)
                m_Sexo = eSexo.Masculino;
            else
                m_Sexo = value;
        }
    }

    private string ComprobarVerbos(string sPalabra)
    {
        // Comprueba si cumple las nomas indicadas y busca en la lista de verbos,
        // si lo encuentra, lo convierte convenientemente.
        // Devolverá la nueva palabra o la original

        //cContenido tContenido;
        bool hallado;
        int i;
        string sInfinitivo;
        string sPalabraAnt;

        if (string.IsNullOrEmpty(sPalabra))
            return "";

        // Antes de convertir el verbo, comprobar si la palabra anterior
        // es 'el' o 'un' en ese caso, no se conjugará
        // el juego X -> el juegas X, sino el juego -> el juego
        hallado = false;
        // If PalabraOrig > 1 AndAlso PalabraOrig < PalabrasOrig Then
        if (PalabraOrig > 0 && PalabraOrig < FraseOrig.Count)
        {
            // If " el un al del ".IndexOf(FraseOrig(PalabraOrig - 1)) > -1 Then
            if (" el un al del ".IndexOf(FraseOrig[PalabraOrig]) > -1)
                hallado = true;
        }

        if (!hallado)
        {
            // Jugar con las terminaciones:
            // arme->arte, erme->erte, irme->irte
            // Se puede simplificar con rme->rte
            i = sPalabra.Length - 3;
            // Guardar la palabra por si resulta que no es un verbo
            sPalabraAnt = sPalabra;
            hallado = false;
            var sCase = RightN(sPalabra, 3);
            switch (sCase)// RightN(sPalabra, 3)
            {
                case "rme":
                    {
                        sPalabra = sPalabra.Substring(0, i) + "rte";
                        hallado = true;
                        break;
                    }

                case "rte":
                    {
                        sPalabra = sPalabra.Substring(0, i) + "rme";
                        hallado = true;
                        break;
                    }
            }
            // Comprobar las terminaciones telo/melo tela/mela
            sCase = RightN(sPalabra, 4);
            switch (sCase) // RightN(sPalabra, 4)
            {
                case "melo":
                    {
                        sPalabra = sPalabra.Substring(0, i - 1) + "telo";
                        i -= 2;
                        hallado = true;
                        break;
                    }

                case "telo":
                    {
                        sPalabra = sPalabra.Substring(0, i - 1) + "melo";
                        i -= 2;
                        hallado = true;
                        break;
                    }

                case "mela":
                    {
                        sPalabra = sPalabra.Substring(0, i - 1) + "tela";
                        i -= 2;
                        hallado = true;
                        break;
                    }

                case "tela":
                    {
                        sPalabra = sPalabra.Substring(0, i - 1) + "mela";
                        i -= 2;
                        hallado = true;
                        break;
                    }
            }

            // Comprobar si es uno de los verbos conocidos
            // el infinitivo será la longitud de la palabra - 2
            // el valor de 'i' es 3 caracteres menos de la longitud total
            if (hallado)
            {
                // pero sólo tomamos la parte sin la forma infinitiva,
                // de am-arme nos quedamos con am
                sInfinitivo = sPalabra.Substring(0, i - 1);
                hallado = false;
                foreach (var tContenido1 in m_Verbos.Valores) // .Values
                {
                    if (sInfinitivo == tContenido1.ID)
                    {
                        hallado = true;
                        break;
                    }
                }
                // Si no es un verbo conocido, dejamos la palabra como estaba
                if (!hallado)
                    sPalabra = sPalabraAnt;
            }

            // Si no se ha hallado una de estas formas,
            // se comprobará si es un verbo
            if (!hallado)
            {
                // primero evaluar los verbos
                foreach (var withBlock in m_Verbos.Valores)
                {
                    hallado = false;
                    {
                        //var withBlock = tContenido1;
                        if ((" " + sPalabra).IndexOf(" " + withBlock.ID) > -1)
                        {
                            // ------------------------------------------------------
                            // Terminaciones en 'ar'
                            // ------------------------------------------------------
                            if (withBlock.Contenido == "ar")
                            {
                                // presente

                                // Para: doy -> das y viceversa,         (15/Sep/02)
                                // Aunque se supone que debería corregirse con
                                // las reglas de simplificación.
                                if (sPalabra == withBlock.ID + "oy")
                                {
                                    sPalabra = withBlock.ID + "as";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "o")
                                {
                                    sPalabra = withBlock.ID + "as";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "as")
                                {
                                    sPalabra = withBlock.ID + "o";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "aba")
                                {
                                    sPalabra = withBlock.ID + "abas";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "abas")
                                {
                                    sPalabra = withBlock.ID + "aba";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "é")
                                {
                                    sPalabra = withBlock.ID + "aste";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "aste")
                                {
                                    sPalabra = withBlock.ID + "é";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "aré")
                                {
                                    sPalabra = withBlock.ID + "arás";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "arás")
                                {
                                    sPalabra = withBlock.ID + "aré";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "aría")
                                {
                                    sPalabra = withBlock.ID + "arías";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "arías")
                                {
                                    sPalabra = withBlock.ID + "aría";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "e")
                                {
                                    sPalabra = withBlock.ID + "es";
                                    hallado = true;
                                    // No convertir de en des            (18/Sep/02)
                                    if (sPalabra == "des")
                                        hallado = false;
                                }
                                else if (sPalabra == withBlock.ID + "es")
                                {
                                    sPalabra = withBlock.ID + "e";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ara")
                                {
                                    sPalabra = withBlock.ID + "aras";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "aras")
                                {
                                    sPalabra = withBlock.ID + "ara";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ase")
                                {
                                    sPalabra = withBlock.ID + "ases";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ases")
                                {
                                    sPalabra = withBlock.ID + "ase";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "are")
                                {
                                    sPalabra = withBlock.ID + "ares";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ares")
                                {
                                    sPalabra = withBlock.ID + "are";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ándome")
                                {
                                    sPalabra = withBlock.ID + "ándote";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ándote")
                                {
                                    sPalabra = withBlock.ID + "ándome";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "me")
                                {
                                    sPalabra = withBlock.ID + "te";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "te")
                                {
                                    sPalabra = withBlock.ID + "me";
                                    hallado = true;
                                }
                            }
                            else if (withBlock.Contenido == "er" || withBlock.Contenido == "ir")
                            {
                                // presente
                                if (sPalabra == withBlock.ID + "o")
                                {
                                    sPalabra = withBlock.ID + "es";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "es")
                                {
                                    sPalabra = withBlock.ID + "o";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ía")
                                {
                                    sPalabra = withBlock.ID + "ías";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ías")
                                {
                                    sPalabra = withBlock.ID + "ía";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "í")
                                {
                                    sPalabra = withBlock.ID + "iste";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "iste")
                                {
                                    sPalabra = withBlock.ID + "í";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "eré")
                                {
                                    sPalabra = withBlock.ID + "erás";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "erás")
                                {
                                    sPalabra = withBlock.ID + "eré";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "iré")
                                {
                                    sPalabra = withBlock.ID + "irás";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "irás")
                                {
                                    sPalabra = withBlock.ID + "iré";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ería")
                                {
                                    sPalabra = withBlock.ID + "erías";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "erías")
                                {
                                    sPalabra = withBlock.ID + "ería";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "iría")
                                {
                                    sPalabra = withBlock.ID + "irías";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "irías")
                                {
                                    sPalabra = withBlock.ID + "iría";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "a")
                                {
                                    sPalabra = withBlock.ID + "as";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "as")
                                {
                                    sPalabra = withBlock.ID + "a";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "iera")
                                {
                                    sPalabra = withBlock.ID + "ieras";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ieras")
                                {
                                    sPalabra = withBlock.ID + "iera";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "iese")
                                {
                                    sPalabra = withBlock.ID + "ieses";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ieses")
                                {
                                    sPalabra = withBlock.ID + "iese";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "iere")
                                {
                                    sPalabra = withBlock.ID + "ieres";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "ieres")
                                {
                                    sPalabra = withBlock.ID + "iere";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "iéndome")
                                {
                                    sPalabra = withBlock.ID + "iéndote";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "iéndote")
                                {
                                    sPalabra = withBlock.ID + "iéndome";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "me")
                                {
                                    sPalabra = withBlock.ID + "te";
                                    hallado = true;
                                }
                                else if (sPalabra == withBlock.ID + "te")
                                {
                                    sPalabra = withBlock.ID + "me";
                                    hallado = true;
                                }
                            } // ---terminación

                            // Si se ha encontrado, salir del bucle
                            if (hallado)
                                break;
                        } // ---si está contenido en la palabra
                    }
                }
            }
        }

        // devolver la nueva palabra o la original
        return sPalabra;
    }

    private string BuscarReglas(string sPalabra, cRegla tRegla)
    {
        // Busca una palabra en tRegla,
        // devolverá la respuesta hallada o una cadena vacía

        string sRespuesta;
        int i;
        //cRespuestas tRespuestas;

        // buscar esta palabra clave en la lista
        sRespuesta = "";

        // Primero buscar en las respuestas Extras, si hay algunas
        foreach (var tRespuestas in tRegla.Extras.Valores) // .Values
        {
            if (tRespuestas.Contenido == sPalabra)
            {
                // (09/Jun/98)
                // Si la respuesta se obtiene de forma aleatoria,
                // sólo se hará para buscar la primera,
                // después se continuará secuencialmente.

                i = tRespuestas.UltimoItem + 1;
                if (tRegla.Aleatorio)
                {
                    if (tRespuestas.UltimoItem < 0)
                        // i = Int(Rnd() * tRespuestas.Count) + 2
                        i = m_rnd.Next(tRespuestas.Count);// + 2
                }
                tRespuestas.UltimoItem = i;
                // Si el siguiente item a usar es mayor que el total
                // de respuestas disponibles
                if (i >= tRespuestas.Count)
                {
                    // usar la primera respuesta y reiniciar
                    // el número del item a usar la próxima vez
                    i = 0; // 1
                    tRespuestas.UltimoItem = 0;
                }
                sRespuesta = tRespuestas.Item(i).Contenido;
            }
        }
        // Si no se ha encontrado una respuesta en los Extras
        // se comprueba si esa clave es el contenido de esta Regla
        if (sRespuesta.Length == 0)
        {
            if (tRegla.Contenido == sPalabra)
            {
                if (tRegla.Respuestas.Count > 0)
                {
                    i = tRegla.Respuestas.UltimoItem + 1;
                    tRegla.Respuestas.UltimoItem = i;
                    if (i >= tRegla.Respuestas.Count)
                    {
                        i = 0;
                        tRegla.Respuestas.UltimoItem = 0;
                    }
                    sRespuesta = tRegla.Respuestas.Item(i).Contenido;
                }
            }
        }
        // Devolver la respuesta hallada
        return sRespuesta;
    }

    private static string QuitarEspaciosExtras(string sCadena)
    {
        // Quita los espacios extras dentro de la cadena                 ( 5/Jun/98)
        return sCadena.Replace("  ", " ");
    }

    private void Entrada2Array(string sEntrada)
    {
        // Esta función convierte el string de entrada en un array
        // Aquí se comprobará si se escribe Mi o Mis y se agregará
        // a la colección de palabras a recordar

        string sPalabra;
        string sSeparador = "";
        int RecordarFrase = -1;
        string sEntradaOrig;

        // Iniciar los valores del número de palabras y la actual
        // PalabrasOrig = 0
        PalabraOrig = 0;
        sEntradaOrig = sEntrada;

        while (sEntrada.Length > 0)
        {
            sPalabra = SiguientePalabra(ref sEntrada, ref sSeparador);
            if (sPalabra.Length > 0)
            {
                // PalabrasOrig += 1
                // Se usa desde 1 hasta PalabrasOrig - 1
                // ReDim Preserve FraseOrig(PalabrasOrig)
                // FraseOrig(PalabrasOrig) = sPalabra
                FraseOrig.Add(sPalabra);
                if (sPalabra == "mi" || sPalabra == "mis")
                {
                    if (RecordarFrase == -1)
                        RecordarFrase = FraseOrig.Count - 1;// PalabrasOrig
                }
            }
        }
        // si ha mencionado mi o mis
        if (RecordarFrase > -1)
        {
            // sólo si no es la última palabra
            if (RecordarFrase < FraseOrig.Count)
            {
                // Añadirla a la colección o sustituirla por la nueva entrada
                // Sólo se usará la última letra en caso de que sea S
                // Para después usar tu(s) xxx
                if (RightN(FraseOrig[RecordarFrase], 1) == "s")
                    sSeparador = "s ";
                else
                    sSeparador = " ";

                // $Por hacer                                 (19/Jun/98)
                // Habría que comprobar que la siguiente palabra no sea
                // algo a lo que el usuario ha hecho referencia de lo
                // que Eliza dijera, por ejemplo:
                // a "mi tus" problemas no me interesan... etc.
                // Por tanto creo que se debería comprobar si es una
                // de las palabras clave para que así se pueda dirigir
                // mejor el diálogo.
                // m_colRec.Item(sSeparador & FraseOrig(RecordarFrase + 1)).Contenido = sEntradaOrig
                m_colRec.Item(sSeparador + FraseOrig[RecordarFrase]).Contenido = sEntradaOrig;
            }
        }
    }

    public void Releer()
    {
        m_Releer = true;
        Inicializar();
    }

    public cRespuestas Estadísticas()
    {
        // Devolverá una colección con los siguientes datos:
        // (realmente devuelve la cantidad)
        // Palabras usadas para sustitución
        // Verbos
        // Palabras claves
        // Sub-Claves (variantes de las claves)
        // Respuestas en palabras claves
        // Respuestas en Sub-Claves

        cRespuestas colRespuestas = new cRespuestas();
        int i = 0;
        int j = 0;
        int k = 0;
        int nMayor = 0;
        string sMayor = "";

        foreach (var tRegla in ColReglas.Values)
        {
            // sub-claves (extras)
            i += tRegla.Extras.Count;
            // respuestas
            if (tRegla.Respuestas.Count > nMayor)
            {
                nMayor = tRegla.Respuestas.Count;
                sMayor = tRegla.Contenido;
            }
            j += tRegla.Respuestas.Count;
            foreach (var tRespuestas in tRegla.Extras.Valores) // .Values
                // Respuestas en las sub-claves
                k += tRespuestas.Count;
        }

        {
            var withBlock = colRespuestas;
            withBlock.Item("Palabras usadas para simplificar").Contenido = m_colRS.Count.ToString();
            withBlock.Item("Palabras usadas para sustitución").Contenido = m_colSimp.Count.ToString();
            withBlock.Item("Verbos").Contenido = m_Verbos.Count.ToString();
            withBlock.Item("Palabras claves").Contenido = ColReglas.Count.ToString();
            withBlock.Item("Sub-Claves (variantes de las claves)").Contenido = i.ToString();
            withBlock.Item("-----").Contenido = "-----";
            withBlock.Item("Número total de palabras reconocidas").Contenido = (m_colSimp.Count + m_colRS.Count + m_Verbos.Count + ColReglas.Count + i).ToString();
            withBlock.Item("------").Contenido = "-----";
            withBlock.Item("Respuestas en palabras claves").Contenido = j.ToString();
            withBlock.Item("Respuestas en Sub-Claves").Contenido = k.ToString();
            withBlock.Item("--------").Contenido = "-----";
            withBlock.Item("Número total de Respuestas").Contenido = (k + j).ToString();
            withBlock.Item("---------").Contenido = "-----";
            withBlock.Item("Clave principal con más respuestas").Contenido = "'" + sMayor + "'";
            withBlock.Item("Número de respuestas de '" + sMayor + "'").Contenido = nMayor.ToString();
        }
        return colRespuestas;
    }

    private string BuscarEsaClave(string sPalabra)
    {
        // Comprobar si sPalabra es una clave, si es así,
        // devolver la siguiente respuesta
        cRegla tRegla;
        string sRespuesta = "";
        // Dim i As Integer

        // Comprobar primero si está en las reglas de simplificación

        if (m_colRS.ExisteItem(sPalabra))
        {
            sRespuesta = m_colRS.Item(sPalabra).Contenido;
            // si se ha encontrado, el contenido será lo que esté
            // en la lista de palabras clave
            if (sRespuesta.Length > 0)
                sPalabra = sRespuesta;
        }
        // Buscar esta palabra clave en la lista
        foreach (var tRegla1 in ColReglas.Values)
        {
            // buscar la palabra en cada una de las "claves" y subclaves
            sRespuesta = BuscarReglas(sPalabra, tRegla1);
            if (sRespuesta.Length > 0)
                break;
        }

        // Si es una clave especial, no tener en cuenta el *equal:=
        if (sRespuesta.IndexOf("{*iif", StringComparison.OrdinalIgnoreCase) > -1)
            return sRespuesta;
        // Si se usa {*base:=...}
        if (sRespuesta.IndexOf("{*base:=", StringComparison.OrdinalIgnoreCase) > -1)
            return sRespuesta;

        // Si el contenido de sRespuesta es:*equal:=xxx
        // quiere decir que se debe buscar en la clave "xxx"

        // Como ahora se pueden tener respuestas que incluyan *equal:=
        // se debe buscar respuesta sólo si esta "clave" está al
        // principio de la respuesta                                     (13/Jun/98)
        while (sRespuesta.StartsWith("*equal:=", StringComparison.OrdinalIgnoreCase))
        {
            sPalabra = sRespuesta.Substring(8).TrimStart();
            if (ColReglas.ContainsKey(sPalabra))
            {
                tRegla = ColReglas[sPalabra];
                sRespuesta = BuscarReglas(sPalabra, tRegla);
            }
            else
                // No existe como clave principal,
                // hay que buscarlo en las sub-claves
                foreach (var tRegla1 in ColReglas.Values)
                {
                    sRespuesta = BuscarReglas(sPalabra, tRegla1);
                    if (sRespuesta.Length > 0)
                        break;
                }
        }
        return sRespuesta;
    }

    private int UsarEstaRespuesta; // para no usar static en CrearRespuestaRecordando. (27/ene/23 11.18)

    private string CrearRespuestaRecordando(string sPalabra)
    {
        cRegla tRegla;
        int i;
        int j;
        string sRespuesta;
        // Static UsarEstaRespuesta As Integer
        // $Para probar usar el valor de las respuestas que tenemos,
        // en casos normales usar un valor mayor
        const int NUM_RESPUESTAS = 10;

        sRespuesta = "";
        if (ColReglas.ContainsKey(sPalabra))
            tRegla = ColReglas[sPalabra];
        else
            return sRespuesta;
        // sólo si son dos o más cosas que no ha entendido
        if (tRegla.Respuestas.UltimoItem > 2)
        {
            if (m_colRec.Count > 0)
            {
                // tomar aleatoriamente una de las cosas que
                // ha dicho el usuario
                j = 0;
                do
                {
                    j += 1;
                    i = m_rnd.Next(m_colRec.Count);
                    // Sólo usar este tema si se ha mencionado antes
                    if (m_colRec.UltimoItem != i)
                        break;
                }
                while (!(j > NUM_RESPUESTAS));

                m_colRec.UltimoItem = i;
                UsarEstaRespuesta += 1;
                if (UsarEstaRespuesta > NUM_RESPUESTAS)
                    UsarEstaRespuesta = 1;
                // El valor aleatorio sacado indicará si se usa o no
                // una respuesta "del recuerdo"
                // Esto es para que no se repitan las respuestas en el
                // mismo orden...
                j = 0;
                // le damos más "peso" al uso de este tipo de respuestas
                // de Eliza para "aparentar" más inteligencia
                if (m_rnd.Next(10) > 3)
                    j = UsarEstaRespuesta;
                // De estos valores sólo se tendrán en cuenta los aquí
                // indicados, en caso contrario,
                // usar una de las respuestas "predefinidas",
                // para ello se asigna una cadena vacia a sRespuesta
                switch (j)
                {
                    case 1:
                        {
                            sRespuesta = "Antes mencionaste tu" + m_colRec.Item(i).ID + ", hablame más de ello " + m_colRec.Item(i).ID.Substring(0, 1).Trim() + ".";
                            break;
                        }

                    case 2:
                        {
                            sRespuesta = "Me comentabas sobre tu" + m_colRec.Item(i).ID + ", ¿cómo te influye?";
                            break;
                        }

                    case 3:
                        {
                            sRespuesta = "¿Crees que la relación con tu" + m_colRec.Item(i).ID + " es el motivo de tu problema?";
                            break;
                        }

                    case 4:
                        {
                            sRespuesta = "¿Cómo crees que " + SimplificarEntrada(m_colRec.Item(i).Contenido) + " podría influir en tu comportamiento?";
                            break;
                        }

                    case 5:
                        {
                            sRespuesta = "Háblame más de tus relaciones con tu" + m_colRec.Item(i).ID;
                            break;
                        }

                    default:
                        {
                            sRespuesta = "";
                            break;
                        }
                }
            }
        }
        return sRespuesta;
    }

    /// <summary>
    /// Comprueba si ha contestado negativa o positivamente y devolver la respuesta o una cadena vacía si no tiene alguna de las consideradas.
    /// </summary>
    /// <param name="sEntrada"></param>
    /// <param name="esNegativo">Entrada para saber si se comprueban las respuestas negativas o positivas.</param>
    private static string EsRespuestaNegativaPositiva(string sEntrada, bool esNegativo)
    {
        string[] palabrasNegPos;
        if (esNegativo)
            palabrasNegPos = new[] { " no ", " no,", " nope", " nop", " nil", " negativo", " falso", " nada", " ya est", " ya vale" };
        else
            palabrasNegPos = new[] { " sí ", " si ", " sí,", " si,", " yep", " afirmativo", " positivo", " efectivamente", " así es", " asi es", " por supuesto", " ciertamente", " eso es", " vale", " ok", " o.k.", " de acuerdo", " muy bien", " ya que insistes", " claro" };

        sEntrada = " " + sEntrada + " ";
        for (var i = 0; i <= palabrasNegPos.Length - 1; i++)
        {
            if (sEntrada.IndexOf(palabrasNegPos[i], StringComparison.OrdinalIgnoreCase) > -1)
                return palabrasNegPos[i];
        }
        return "";
    }

    private static bool EsNegativoPositivo(string sEntrada, bool esNegativo)
    {
        // comprobar si en la cadena de entrada hay alguna palabra
        // que denote negación o afirmación

        var res = EsRespuestaNegativaPositiva(sEntrada, esNegativo);
        return string.IsNullOrEmpty(res);
    }

    private static bool EsNegativo(string sEntrada)
    {
        // comprobar si en la cadena de entrada hay alguna palabra
        // que denote negación

        return EsNegativoPositivo(sEntrada, esNegativo: true);
    }

    private static bool EsAfirmativo(string sEntrada)
    {
        // comprobar si en la cadena de entrada hay alguna palabra
        // que denote afirmación
        return EsNegativoPositivo(sEntrada, esNegativo: false);
    }

    // Para no usar static dentro de la función ComprobarEspeciales. (27/ene/23 11.16)
    private string restoAnt;

    private string ComprobarEspeciales(string sRespuesta, string sEntrada, string sPalabra)
    {
        // Comprobar las claves especiales de sustitución y otras
        // que puedan estar en la respuesta generada          (13/Jun/98)
        int i, j;
        // Static restoAnt As String
        // Por si quiero comprobar si hace las cosas bien.       (26/ene/23 12.26)
        // Dim sRespuestaInicial = sRespuesta
        // Dim sEntradaInicial = sEntrada
        // Dim sPalabraInicial = sPalabra

        // comprueba si la respuesta contiene caracteres especiales
        if (string.IsNullOrEmpty(sRespuesta) == false)
        {
            // Comprobar si hay que poner la hora
            // si se indica *LA_HORA* solo poner la hora         (25/ene/23 14.40)
            if (sRespuesta.Contains("*LA_HORA*", StringComparison.OrdinalIgnoreCase))
                // sRespuesta = Left$(sRespuesta, i - 1) & Format$(Now, "hh:mm") & Mid$(sRespuesta, i + Len("*HORA*"))
                sRespuesta = sRespuesta.Replace("*LA_HORA*", DateTime.Now.ToString("H"), StringComparison.OrdinalIgnoreCase);

            if (sRespuesta.Contains("*HORA*", StringComparison.OrdinalIgnoreCase))
                sRespuesta = sRespuesta.Replace("*HORA*", DateTime.Now.ToString("HH:mm"), StringComparison.OrdinalIgnoreCase);
            // Comprobar si hay que poner el día de hoy
            if (sRespuesta.Contains("*HOY*", StringComparison.OrdinalIgnoreCase))
                sRespuesta = sRespuesta.Replace("*HOY*", DateTime.Now.ToString("dddd, dd MMMM"), StringComparison.OrdinalIgnoreCase);

            // comprobar si hay que añadir el RESTO
            if (sRespuesta.IndexOf("*RESTO*", StringComparison.OrdinalIgnoreCase) > -1)
            {
                i = sEntrada.IndexOf(sPalabra, StringComparison.OrdinalIgnoreCase);
                if (i > -1)
                    sEntrada = sEntrada.Substring(i + sPalabra.Length + 1);
                // Sustituir *RESTO* por sEntrada
                i = sRespuesta.IndexOf("*RESTO*", StringComparison.OrdinalIgnoreCase);
                if (i > -1)
                {
                    sEntrada = SimplificarEntrada(sEntrada);
                    // Guardar la respuesta anterior,                    (17/Sep/02)
                    // por si se usa para asignar a la base de datos del usuario.
                    restoAnt = sEntrada.Trim();
                    sRespuesta = $"{sRespuesta.Substring(0, i)}{sEntrada}{sRespuesta.Substring(i + "*RESTO*".Length)}";
                }
            }
        }
        else
            sRespuesta = sEntrada;
        // Cambiar los *ea* por el correspondiente según el sexo
        if (sRespuesta.Contains("*ea*", StringComparison.OrdinalIgnoreCase))
        {
            sPalabra = "e";
            if (m_Sexo == eSexo.Femenino)
                sPalabra = "a";
            // Cambiar las posibles ocurrencias de *ea* por sPalabra
            sRespuesta = QuitarCaracterEx(sRespuesta, "*ea*", sPalabra);
        }
        // Cambiar los *oa* por el correspondiente según el sexo
        if (sRespuesta.Contains("*oa*", StringComparison.OrdinalIgnoreCase))
        {
            sPalabra = "o";
            if (m_Sexo == eSexo.Femenino)
                sPalabra = "a";
            // Cambiar las posibles ocurrencias de *oa* por sPalabra
            sRespuesta = QuitarCaracterEx(sRespuesta, "*oa*", sPalabra);
        }

        // Si el primer caracter es una ¿, el último debe ser ?
        if (sRespuesta.StartsWith("¿"))
        {
            if (sRespuesta.Contains('?') == false)
                sRespuesta += "?";
        }
        // Si existen dos caracteres iguales al final, dejar sólo uno
        // sólo si no es el punto, por aquello de los ...     (11/Jun/98)
        if (sRespuesta.EndsWith(".") == false)
        {
            i = sRespuesta.Length;
            var comoTermina = RightN(sRespuesta, 1);
            if (sRespuesta.EndsWith(comoTermina + comoTermina))
                sRespuesta = sRespuesta.Substring(0, i - 1);
        }

        // si se indica *mi_edad*, calcular la edad                      (18/Sep/02)
        if (sRespuesta.Contains("*mi_edad*", StringComparison.OrdinalIgnoreCase))
            sRespuesta = sRespuesta.Replace("*mi_edad*", (DateTime.Now.Year - 1998).ToString(), StringComparison.OrdinalIgnoreCase);

        // Cambiar *NOMBRE* por el nombre
        if (sRespuesta.Contains("*NOMBRE*", StringComparison.OrdinalIgnoreCase))
            // Usar siempre el nombre
            sRespuesta = sRespuesta.Replace("*NOMBRE*", Nombre, StringComparison.OrdinalIgnoreCase);

        // $Comprobar si después de un sigo de separación no hay espacio
        // Lo que se hace es añadirle el espacio, ya que posteriormente
        // se le quitarían los espacios extras
        sRespuesta = QuitarCaracterEx(sRespuesta, ",", ", ");
        // más arreglos del texto
        sRespuesta = QuitarCaracterEx(sRespuesta, " , ", ", ");

        // quitarle los dobles espacios que haya
        sRespuesta = QuitarEspaciosExtras(sRespuesta);
        // quitar los espacios de delante de la interrogación final
        sRespuesta = QuitarCaracterEx(sRespuesta, " ?", "?");
        // quitar los espacios de después de la interrogación inicial
        sRespuesta = QuitarCaracterEx(sRespuesta, "¿ ", "¿");

        // Si la respuesta contiene {*iif(
        i = sRespuesta.IndexOf("{*iif(", StringComparison.OrdinalIgnoreCase);
        sUsarPregunta = "";
        if (i > -1)
        {
            // El formato será: {*iif(condición; ES-TRUE)(ES-FALSE)}
            sUsarPregunta = sRespuesta.Substring(i);
            sRespuesta = sRespuesta.Substring(0, i);
            j = sUsarPregunta.IndexOf(";");
            if (j > -1)
            {
                i = sUsarPregunta.IndexOf("(", j);
                if (i > -1)
                {
                    sRespuestas[cNegativa] = sUsarPregunta.Substring(i + 1, sUsarPregunta.Length - i - 3);
                    sRespuestas[cAfirmativa] = sUsarPregunta.Substring(j + 1, i - j - 2);
                    sUsarPregunta = sUsarPregunta.Substring(6, j - 6);
                }
            }
        }
        // Si la respuesta contiene {*base:=
        sUsarBaseDatos = "";
        i = sRespuesta.IndexOf("{*base:=", StringComparison.OrdinalIgnoreCase);
        if (i > -1)
        {
            // El formato será: {*base:=clave_base}
            // sUsarBaseDatos contendrá la clave de la base de datos
            sUsarBaseDatos = sRespuesta.Substring(i + 8);
            sRespuesta = sRespuesta.Substring(0, i);
            // Quitarle el } del final
            i = sUsarBaseDatos.IndexOf("}");
            if (i > -1)
            {
                // si a continuación sigue un := asignar el valor indicado
                j = sUsarBaseDatos.IndexOf(":=*restoant*", StringComparison.OrdinalIgnoreCase);
                if (j > -1)
                {
                    sUsarBaseDatos = sUsarBaseDatos.Substring(0, j);
                    ValidarDatosParaBase(restoAnt);
                }
                else
                    sUsarBaseDatos = sUsarBaseDatos.Substring(0, i);
            }
        }
        // Si la respuesta incluye: *iif(*base*
        // se comprobará si el dato está en la base de datos,
        // de se así se usará lo que venga después del ;
        // en caso contrario se usará lo que esté después de )(
        // *iif(*base*signo_zodiaco;*usarbase:=signo_zodiaco*)
        // (*equal:=cual es tu signo)
        i = sRespuesta.IndexOf("*iif(*base*", StringComparison.OrdinalIgnoreCase);
        if (i > -1)
        {
            string sClave;
            string sTrue = "";
            string sFalse = "";

            sEntrada = sRespuesta.Substring(0, i);
            sClave = sRespuesta.Substring(i + 11);
            j = sClave.IndexOf(";");
            if (j > -1)
            {
                sClave = sClave.Substring(0, j);
                j = sRespuesta.IndexOf(";");
                sRespuesta = sRespuesta.Substring(j + 1);
                j = sRespuesta.IndexOf(")(");
                if (j > -1)
                {
                    sTrue = sRespuesta.Substring(0, j);
                    sFalse = sRespuesta.Substring(j + 2);
                    if (RightN(sFalse, 1) == ")")
                        sFalse = sFalse.Substring(0, sFalse.Length - 1);
                }
                if (ColBaseUser.ExisteItem(sClave))
                {
                    // Comprobar si hay que sustituir el dato
                    // *usarbase:=signo_zodiaco*
                    // Si después de la clave se
                    i = sTrue.IndexOf("*usarbase:=", StringComparison.OrdinalIgnoreCase);
                    if (i > -1)
                    {
                        j = sTrue.IndexOf("*", i + 1);
                        sClave = sTrue.Substring(i + "*usarbase:=".Length, j - (i + "*usarbase:=".Length));
                        if (ColBaseUser.ExisteItem(sClave))
                            sTrue = sTrue.Substring(0, i) + " " + ColBaseUser.Item(sClave).Contenido + " " + sTrue.Substring(j + 1);
                        else
                            sTrue = sTrue.Substring(0, i) + " " + sTrue.Substring(j + 1);
                    }
                    sRespuesta = sTrue;
                }
                else
                    sRespuesta = sFalse;
                sRespuesta = sEntrada + sRespuesta;
                i = sRespuesta.IndexOf("*equal:=", StringComparison.OrdinalIgnoreCase);
                if (i > -1)
                {
                    sRespuesta = sRespuesta.Substring(i + 8).Trim();
                    sRespuesta = BuscarEsaClave(sRespuesta);
                    // re-entrar para comprobar nuevas claves
                    if (sRespuesta.Length > 0)
                        sRespuesta = ComprobarEspeciales(sRespuesta, sRespuesta, "");
                }
            }
        }
        return sRespuesta;
    }

    public bool Iniciado { get; set; }

    private void DatosUsuario(bool AccionLeer = true)
    {
        // Leerá la base de datos de este usuario.            (14/Jun/98)
        // El formato del fichero será:
        // clave=valor
        string sFic;
        string sTmp;
        int i;
        string sClave;
        string sPath;

        sPath = System.IO.Path.Combine(AppDataPath, "Bases");
        if (System.IO.Directory.Exists(sPath) == false)
            System.IO.Directory.CreateDirectory(sPath);
        sFic = System.IO.Path.Combine(sPath, "Datos_" + Nombre + ".txt");

        if (AccionLeer)
        {
            ColBaseUser.Clear(); // = New cRespuestas
            // Para que tenga algunos datos
            ColBaseUser.Item("Nombre").Contenido = Nombre;
            ColBaseUser.Item("Sexo").Contenido = m_Sexo == eSexo.Femenino ? "Femenino" : "Masculino";
            // Leer los datos, si hay...
            if (System.IO.File.Exists(sFic))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(sFic, System.Text.Encoding.UTF8, true))
                {
                    while (!sr.EndOfStream)
                    {
                        sTmp = sr.ReadLine().Trim();
                        if (string.IsNullOrEmpty(sTmp) == false && sTmp.StartsWith(";") == false)
                        {
                            i = sTmp.IndexOf("=");
                            if (i > -1)
                            {
                                sClave = sTmp.Substring(0, i).Trim();
                                sTmp = sTmp.Substring(i + 1).Trim();
                                ColBaseUser.Item(sClave).Contenido = sTmp;
                            }
                        }
                    }
                }
            }
        }
        else
            // Guardar los datos
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sFic, false, System.Text.Encoding.UTF8))
            {
                foreach (var tContenido in ColBaseUser.Valores) // .Values
                    sw.WriteLine($"{tContenido.ID}={tContenido.Contenido}");
            }
    }

    private string ValidarDatosParaBase(string sEntrada)
    {
        // Se validará la respuesta del usuario a una pregunta para
        // añadir a la base de datos.
        int i;
        bool hallado;

        switch (sUsarBaseDatos)
        {
            case "signo_zodiaco":
                {
                    var vArray = new[] { " aries", " géminis", " geminis", " tauro", " cáncer", " cancer", " leo", " virgo", " libra", " scorpio", " escorpión", " escorpio", " escorpion", " sagitario", " capricornio", " acuario", " piscis" };
                    hallado = false;
                    for (i = 0; i <= vArray.Length - 1; i++)
                    {
                        if (sEntrada.IndexOf(vArray[i], StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            ColBaseUser.Item(sUsarBaseDatos).Contenido = vArray[i].TrimStart();
                            hallado = true;
                            break;
                        }
                    }
                    // Se puede devolver esto como respuesta
                    if (!hallado)
                        sEntrada = "Creo que no has usado un signo del zodíaco...";
                    break;
                }

            case "edad":
                {
                    i = 0;
                    if (int.TryParse(sEntrada, out i) == false)
                        sEntrada = "Por favor indica la edad con números, gracias.";
                    else if (i == 0)
                        sEntrada = "¿Acabas de nacer? :-)";
                    else if (i < 1)
                        sEntrada = "¿Es que aún no has nacido? :-)";
                    break;
                }

            default:
                {
                    // Comprobar si hay que quitar texto de la respuesta,
                    // por ejemplo, el color del pelo o los ojos: también castaños, etc.
                    ColBaseUser.Item(sUsarBaseDatos).Contenido = sEntrada;
                    break;
                }
        }
        // Guardar los datos
        DatosUsuario(false);

        return sEntrada;
    }

    ///// <summary>
    ///// El path de los datos de Eliza: LocalApplicationData\Eliza.
    ///// </summary>
    //private string AppDataPath()
    //{
    //    return AppDataPath;
    //}

    ///// <summary>
    ///// Devuelve la información de esta DLL.
    ///// </summary>
    //public static string VersionDLL()
    //{
    //    var ensamblado = typeof(cEliza).Assembly;
    //    var fvi = FileVersionInfo.GetVersionInfo(ensamblado.Location);
    //    // FileDescription en realidad muestra (o eso parece) lo mismo de ProductName
    //    var s = $"{fvi.ProductName} v{fvi.ProductVersion} ({fvi.FileVersion})" + $"{Frases.CrLf}{fvi.Comments}";

    //    return s;
    //}

    /// <summary>
    /// Devuelve los n últimos caracteres de la cadena indicada.
    /// </summary>
    /// <param name="texto"></param>
    /// <param name="n"></param>
    /// <remarks>Si n es mayor que la longitud o es menor de uno, se devuelve la cadena vacía.</remarks>
    public static string RightN(string texto, int n)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;
        var len = texto.Length;
        if (n > len || n < 1)
            return "";

        return texto.Substring(len - n, n);
    }
}
