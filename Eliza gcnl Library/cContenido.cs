using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliza_gcnl;
public class cContenido
{

    /// <summary>
    /// Crear las instancias indicando siempre la clave o id de este contenido.
    /// </summary>
    /// <param name="newID"></param>
    public cContenido(string newID)
    {
        ID = newID;
        Contenido = "";
    }

    /// <summary>
    /// El contenido asociado con la clave de este contenido.
    /// </summary>
    public string Contenido { get; set; }

    /// <summary>
    /// El ID o clave de este contenido.
    /// </summary>
    public string ID { get; }
}
