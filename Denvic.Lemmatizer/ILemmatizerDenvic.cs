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
        string GetNormalFormString(string word);
        DenvicLemma GetNormalForm(string word);
        Array GetNormalFormStringCollection(string text, bool duplicate = false);
        Array GetNormalFormCollection(string text);

        void SetDict(string RMLPath = "");
    }
}
