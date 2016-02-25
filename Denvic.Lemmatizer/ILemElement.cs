using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Denvic.Lemmatizer
{
    /// <summary>
    /// Лемма
    /// </summary>
    interface IDenvicLemma
    {
        /// <summary>
        /// Лемма, нормальная (начальная) форма слова
        /// </summary>
        string NormalForm { get; }
        /// <summary>
        /// Описание (род, число, падеж)
        /// </summary>
        string Kind { get; }
        /// <summary>
        /// Часть речи
        /// </summary>
        string PartOfSpeech { get; }
        /// <summary>
        /// Число повторений в тексте
        /// </summary>
        int Count { get; }
    }
}
