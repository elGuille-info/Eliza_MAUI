// ------------------------------------------------------------------------------
// cRespuestas                                                 (22/ene/23 10.26)
// Clase colección para almacenar objetos del tipo cContenido
// 
// ©Guillermo Som (Guille), 1998-2002, 2023
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliza_gcnl;

public class cRespuestas
{
    // Lo inicio con -1 para que al incrementarlo sea 0          (24/ene/23 12.37)
    // ya que los índices empiezan por cero
    public int UltimoItem { get; set; } = -1;
    public string Contenido { get; set; }

    public cRespuestas(string contenido = "")
    {
        this.Contenido = contenido;
    }

    private readonly Dictionary<string, cContenido> m_col = new();

    // Número de elementos en la colección
    public int Count { get { return m_col.Count; } }

    public void Add(string newContenido)
    {
        // Añadirlo a la colección
        newContenido = newContenido.Trim();
        if (string.IsNullOrEmpty(newContenido) == false)
            Item(newContenido).Contenido = newContenido;
    }

    /// <summary>
    /// Limpiar el contenido de la colección.
    /// </summary>
    public void Clear() { m_col.Clear(); }

    /// <summary>
    /// El elemento con la clave indicada. Si no existe, lo crea y añade a la colección.
    /// </summary>
    /// <param name="newContenido"></param>
    public cContenido Item(string newContenido)
    {
        cContenido tContenido;

        if (m_col.ContainsKey(newContenido))
            tContenido = m_col[newContenido];
        else
        {
            // Si no existe añadirlo
            tContenido = new cContenido(newContenido);
            m_col.Add(newContenido, tContenido);
        }

        return tContenido;
    }

    /// <summary>
    /// El elemento con el índice indicado. Si no existe, se devuelve el último.
    /// </summary>
    /// <param name="newContenido">El índice en base 0: de 0 a col.count-1</param>
    public cContenido Item(int newContenido)
    {
        if (newContenido < m_col.Count)
            return m_col.ElementAt(newContenido).Value;
        else
            return m_col.ElementAt(m_col.Count - 1).Value;
    }

    public bool ExisteItem(string sContenido) { return m_col.ContainsKey(sContenido); }

    public Dictionary<string, cContenido>.ValueCollection Valores { get { return m_col.Values; } }
}
