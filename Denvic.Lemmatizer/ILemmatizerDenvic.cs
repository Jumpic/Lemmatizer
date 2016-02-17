using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Denvic.Lemmatizer
{

    interface ILemmatizerDenvic
    {
        string Version { get; }
        string GetNormalForm(string word);
        DenvicLemma GetParadigm(string word);
        Array GetNormalFormCollection(string text, bool duplicate = false);
        Array GetParadigmCollection(string text);

        void LoadDict(string RMLPath = "");
    }
}
