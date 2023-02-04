// ------------------------------------------------------------------------------
// cRegla                                                            (22/ene/23)
// Clase para mantener colecciones de Contenidos y KeyWords
// 
// ©Guillermo Som (Guille), 1998-2002, 2023
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliza_gcnl;

public class cRegla
{
    /// <summary>
    /// Una colección con reglas, solo se utiliza desde cEliza.
    /// </summary>
    private readonly Dictionary<string, cRegla> m_colReglas = new();

    /// <summary>
    /// Accede a un elemento de las reglas de esta clase. Si existe devuelve esa regla, si no, la añade.
    /// </summary>
    /// <param name="newContenido">La clave de la regla.</param>
    public cRegla Item(string newContenido)
    {
            cRegla tRegla;

            if (m_colReglas.ContainsKey(newContenido))
                tRegla = m_colReglas[newContenido];
            else
            {
                // Si no existe añadirlo
                tRegla = new cRegla(newContenido);
                m_colReglas.Add(newContenido, tRegla);
            }

            return tRegla;
    }

    /// <summary>
    /// Accede a la colección interna de reglas.
    /// </summary>
    public Dictionary<string, cRegla> Reglas { get { return m_colReglas; } }

    /// <summary>
    /// Si se deben tomar las respuestas de forma aleatoria.
    /// </summary>
    public bool Aleatorio { get; set; }
    /// <summary>
    /// El nivel a tener en cuenta al analizar las palabras, el más bajo es 0.
    /// </summary>
    public int Nivel { get; set; }

    private readonly cRespuestas colRespuestas = new cRespuestas();
    private readonly cKeyWords colKeyWords = new cKeyWords();

    /// <summary>
    /// La clave para esta regla.
    /// </summary>
    public string Contenido { get; set; }

    public cRegla(string contenido = "")
    {
        this.Contenido = contenido;
    }

    /// <summary>
    /// Las respuestas extras de esta regla.
    /// </summary>
    public cKeyWords Extras { get { return colKeyWords; } }

    /// <summary>
    /// Las respuestas principales de esta regla.
    /// </summary>
    public cRespuestas Respuestas { get { return colRespuestas; } }
}
