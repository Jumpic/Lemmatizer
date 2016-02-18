using LemmatizerNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;

namespace Denvic.Lemmatizer
{
    struct ParadigmStruct
    {
        public string PartOfSpeech;
        public string EndingWord;
        public string NormalForm;
        public string Kind;
    }

    [ComVisible(true)]
    [ProgId("Denvic.Lemmatizer")]
    [Guid("919D6695-7740-4FB6-B182-AC79C430BC54")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class LemmatizerDenvic : ILemmatizerDenvic
    {
        MorphLanguage lang = MorphLanguage.Russian;
        ILemmatizer lem;
        List<ParadigmStruct> rgramtab;

        public LemmatizerDenvic()
        {
            LoadDict("");
        }
        
        /// <summary>
        /// Возвращает нормальную (начальную, словарную) форму слова - Лемма в виде строки
        /// </summary>
        /// <param name="word">Искомое слово</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.BStr)]
        public string GetNormalForm([MarshalAs(UnmanagedType.BStr)] string word)
        {
            var paradigmList = lem.CreateParadigmCollectionFromForm(word, false, false);
            if (paradigmList.Count > 0)
                return paradigmList[0].Norm;
            else
                return "";
        }

        /// <summary>
        /// Русское представление метода "GetNormalForm"
        /// Возвращает нормальную (начальную, словарную) форму слова - Лемма в виде строки
        /// </summary>
        /// <param name="word">Искомое слово</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.BStr)]
        public string НачальнаяФормаСловаСтрока([MarshalAs(UnmanagedType.BStr)] string word)
        {
            return GetNormalForm(word);
        }

        /// <summary>
        /// Возвращает коллекцию нормальных форм слов (начальная, словарная) форма слова - Лемма
        /// </summary>
        /// <param name="word">Текст</param>
        /// <param name="duplicate">Если false - в коллекцию попадут только уникальные значения, по умолчанию false</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        public Array GetNormalFormCollection([MarshalAs(UnmanagedType.BStr)] string text, 
            [MarshalAs(UnmanagedType.Bool)] bool duplicate = false)
        {
            // Разбиваем текст на массив слов
            // Убираем знаки препинания, лишние пробелы и переносы строк
            //
            var arrayWords = GetWordsFromText(text);

            // Преобразуем в нормальную форму
            // Если слова нет в словаре, то оно не попадет в итоговую коллекцию
            //

            var result = arrayWords.Select( x => GetNormalForm(x)).ToArray();
            if (!duplicate)
            {
                result = result.GroupBy(x => x).Select(x => x.Key).ToArray();
            }

            return result;
        }

        /// <summary>
        /// Русское представление метода "GetNormalFormCollection"
        /// Возвращает коллекцию строк нормальных форм слов (начальная, словарная) форма слова - Лемма
        /// </summary>
        /// <param name="word">Текст</param>
        /// <param name="duplicate">Если false - в коллекцию возвратятся только не повторяющиеся значения, по умолчанию false</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        public Array КоллекцияНачальнаяФормаСловаСтрока([MarshalAs(UnmanagedType.BStr)] string word,
            [MarshalAs(UnmanagedType.Bool)] bool duplicate = false)
        {
            return GetNormalFormCollection(word, duplicate);
        }

        /// <summary>
        /// Загрузка словарей
        /// </summary>
        /// <param name="RMLPath">Путь к каталогу о словарями</param>
        public void LoadDict([MarshalAs(UnmanagedType.BStr)] string RMLPath = "")
        {
            var path = String.IsNullOrWhiteSpace(RMLPath) ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : RMLPath;
            var manager = FileManager.GetFileManager(path);
            lem = LemmatizerFactory.Create(lang);
            lem.LoadDictionariesRegistry(manager);

            rgramtab = GetGramtab(path);
        }

        /// <summary>
        /// Возвращает лемму слова, DenvicLemma
        /// </summary>
        /// <param name="word">Слово для которого требуется лемматизация</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        public DenvicLemma GetParadigm([MarshalAs(UnmanagedType.BStr)] string word)
        {
            var paradigm = GetParadigmStruct(word);
            return new DenvicLemma(paradigm.NormalForm, 1, paradigm.PartOfSpeech, paradigm.Kind);
        }

        /// <summary>
        /// Возвращает лемму слова, DenvicLemma
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        public DenvicLemma НачальнаяФормаСлова([MarshalAs(UnmanagedType.BStr)] string word)
        {
            return GetParadigm(word);
        }

        /// <summary>
        /// Возвращает коллекцию неповторяющихся лемм. В коллекцию входят части речи: 
        /// 1. Глагол
        /// 2. Прилагательное
        /// 3. Существительное
        /// Остальные слова, а также те которых нет в словарях, из конечной коллекции будут исключены
        /// </summary>
        /// <param name="text">Текст</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        public Array GetParadigmCollection([MarshalAs(UnmanagedType.BStr)] string text)
        {
            // Разбиваем текст на массив слов
            // Убираем знаки препинания, лишние пробелы и переносы строк
            //
            var arrayWords = GetWordsFromText(text);

            // Преобразуем в нормальную форму
            // Если слова нет в словаре, то оно не попадет в итоговую коллекцию
            //
            var result = arrayWords.Select(x => GetParadigmStruct(x))
                        .Where(x => x.PartOfSpeech == "ИНФИНИТИВ" || x.PartOfSpeech == "С" || x.PartOfSpeech == "П").ToArray()
                        .GroupBy(lem => lem.NormalForm)
                        .Select(lem => new DenvicLemma(lem.Key, lem.Count(), lem.First().PartOfSpeech, lem.First().Kind))
                        .ToArray();

            return result;
        }

        /// <summary>
        /// Возвращает коллекцию неповторяющихся лемм глагола, прилагательного. существительного
        /// </summary>
        /// <param name="text">Текст</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        public Array КоллекцияНачальнаяФормаСлова([MarshalAs(UnmanagedType.BStr)] string text)
        {
            return GetParadigmCollection(text);
        }

        /// <summary>
        /// Версия библиотеки
        /// </summary>
        public string Version
        {
            get
            {
                var version = typeof(LemmatizerDenvic).Assembly.GetName().Version;
                return String.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            }
        }

        /// <summary>
        /// Загрузка соответсвий частей речи, окончаний и т.п.
        /// </summary>
        /// <param name="RMLPath">Путь к каталогу со словарями</param>
        /// <returns></returns>
        private List<ParadigmStruct> GetGramtab(string RMLPath)
        {
            var result = new List<ParadigmStruct>();
            var fileStrings = System.IO.File.ReadAllLines(Path.GetFullPath(RMLPath) + @"\Dicts\Morph\rgramtab.tab", Encoding.GetEncoding(1251));
            for (int i = 0; i < fileStrings.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(fileStrings[i]) && fileStrings[i].Length > 1 && fileStrings[i][1] != '/')
                {
                    var arrayString = fileStrings[i].Split(' ');
                    if (arrayString.Length > 3)
                        result.Add(new ParadigmStruct() { EndingWord = arrayString[0], PartOfSpeech = arrayString[2], Kind = arrayString[3] });
                }
            }

            return result;
        }

        /// <summary>
        /// Конвертация слова в лемму
        /// </summary>
        /// <param name="word">Слово для которого требуется лемматизация</param>
        /// <returns>ParadigmStruct</returns>
        private ParadigmStruct GetParadigmStruct(string word)
        {
            var paradigmList = lem.CreateParadigmCollectionFromForm(word, false, false);
            if (paradigmList.Count > 0)
            {
                var gram = rgramtab.Where(x => x.EndingWord == paradigmList[0].GetAncode(0)).FirstOrDefault();
                return new ParadigmStruct()
                {
                    NormalForm = paradigmList[0].Norm,
                    PartOfSpeech = gram.PartOfSpeech,
                    Kind = gram.Kind,
                    EndingWord = gram.EndingWord
                };
            }
            else
            {
                return new ParadigmStruct();
            }
        }

        private string[] GetWordsFromText(string text)
        {
            // Разбиваем текст на массив слов
            // Убираем знаки препинания, лишние пробелы и переносы строк
            //
            return Regex.Replace(text, "[-.?!)(,:\r\n]", " ").Split(' ')
                .Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
        }
    }
}
