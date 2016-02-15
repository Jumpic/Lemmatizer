using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Denvic.Lemmatizer
{
    interface IDenvicLemma
    {
        string NormalForm { get; }
        string Kind { get; }
        string PartOfSpeech { get; }
        int Count { get; }
    }
}
