using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliza_gcnl;

public class Clasificar : IComparer<string>
{
    public Clasificar(bool deMayorAMenor = true)
    {
        MayorAMenor = deMayorAMenor;
    }
    private readonly bool MayorAMenor;

    public int Compare(string x, string y)
    {
        if (MayorAMenor)
            return (new CaseInsensitiveComparer()).Compare(y, x);
        else
            return (new CaseInsensitiveComparer()).Compare(x, y);
    }
}
