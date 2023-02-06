// ------------------------------------------------------------------------------
// cKeyWords                                                         (22/ene/23)
// Colección de objetos del tipo cRespuestas
// 
// ©Guillermo 'guille' Som, 1998-2002, 2023
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliza_gcnl;

public class cKeyWords
{
    private readonly Dictionary<string, cRespuestas> m_col = new();

    public cRespuestas Item(string newContenido)
    {
        cRespuestas tRespuestas;

        if (m_col.ContainsKey(newContenido))
            tRespuestas = m_col[newContenido];
        else
        {
            // Si no existe añadirlo
            tRespuestas = new cRespuestas(newContenido);
            m_col.Add(newContenido, tRespuestas);
        }
        return tRespuestas;
    }

    // Método Count de la colección
    public int Count { get { return m_col.Count; } }

    public Dictionary<string, cRespuestas>.ValueCollection Valores { get { return m_col.Values; } }
}
