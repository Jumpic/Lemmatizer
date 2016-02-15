using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Denvic.Lemmatizer
{
    [ComVisible(true)]
    [Guid("F451E6E6-4277-45DF-AB70-BF15B928F911")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class DenvicLemma : IDenvicLemma
    {
        string _NormalForm = "";
        string _Kind = "";
        string _PartOfSpeech = "";
        int _Count = 0;

        public string NormalForm
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get { return _NormalForm; }
        }

        public int Count
        {
            get { return _Count; }
        }

        public DenvicLemma(string normalForm = "", int count = 0, string PartOfSpeech = "", string kind = "")
        {
            _NormalForm = normalForm;
            _Count = count;
            _Kind = kind;
            _PartOfSpeech = PartOfSpeech;
        }


        public string Kind
        {
            get { return _Kind; }
        }

        public string PartOfSpeech
        {
            get { return _PartOfSpeech; }
        }
    }
}
