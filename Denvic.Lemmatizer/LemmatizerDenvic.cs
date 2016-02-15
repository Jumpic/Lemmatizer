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

namespace Denvic.Lemmatizer
{
    struct Lemrgramtab
    {
        public string PartOfSpeech;
        public string EndingWord;
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
        List<Lemrgramtab> rgramtab;

        public LemmatizerDenvic()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var manager = FileManager.GetFileManager(path);
            lem = LemmatizerFactory.Create(lang);
            lem.LoadDictionariesRegistry(manager);

            rgramtab = GetGramtab(path);
        }
        

        /// <summary>
        /// Возвращает нормальную (начальную, словарную) форму слова - Лемма в виде строки
        /// </summary>
        /// <param name="word">Искомое слово</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.BStr)]
        public string GetNormalFormString([MarshalAs(UnmanagedType.BStr)] string word)
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
            return GetNormalFormString(word);
        }

        /// <summary>
        /// Возвращает коллекцию нормальных форм слов (начальная, словарная) форма слова - Лемма
        /// </summary>
        /// <param name="word">Текст</param>
        /// <param name="duplicate">Если false - в коллекцию возвратятся только не повторяющиеся значения, по умолчанию false</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        public Array GetNormalFormStringCollection([MarshalAs(UnmanagedType.BStr)] string text, 
            [MarshalAs(UnmanagedType.Bool)] bool duplicate = false)
        {
            // Разбиваем текст на массив слов
            // Убираем лишние пробелы
            //
            var arrayWords = text.Split(' ')
                .Where(x => !String.IsNullOrWhiteSpace(x));

            // Преобразуем в нормальную форму
            // Если слова нет в словаре, то оно не попадет в итоговую коллекцию
            //
            //var result = arrayWords.GroupBy(x => x)
            //             .Select(y => GetNormalForm(y.Key))
            //             .ToArray();
            var result = arrayWords.Select( x => GetNormalFormString(x)).ToArray();
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
            return GetNormalFormStringCollection(word, duplicate);
        }

        /// <summary>
        /// Инициализация библиотеки
        /// </summary>
        /// <param name="RMLPath">Путь к каталогу о словарями</param>
        public void SetDict([MarshalAs(UnmanagedType.BStr)] string RMLPath = "")
        {
            var path = String.IsNullOrWhiteSpace(RMLPath) ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : RMLPath;
            var manager = FileManager.GetFileManager(path);
            //var manager = FileManager.GetFileManager(AppDomain.CurrentDomain.BaseDirectory);
            lem = LemmatizerFactory.Create(lang);
            lem.LoadDictionariesRegistry(manager);

            rgramtab = GetGramtab(path);
        }

        /// <summary>
        /// Возвращает лемму слова, DenvicLemma
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        public DenvicLemma GetNormalForm([MarshalAs(UnmanagedType.BStr)] string word)
        {
            var paradigmList = lem.CreateParadigmCollectionFromForm(word, false, false);
            if (paradigmList.Count > 0)
            {
                var gram = rgramtab.Where(x => x.EndingWord == paradigmList[0].GetAncode(0)).Take(1).FirstOrDefault();
                return new DenvicLemma(paradigmList[0].Norm, 0, gram.PartOfSpeech, gram.Kind);
            }
            else
                return new DenvicLemma();
        }

        /// <summary>
        /// Возвращает лемму слова, DenvicLemma
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        public DenvicLemma НачальнаяФормаСлова([MarshalAs(UnmanagedType.BStr)] string word)
        {
            return GetNormalForm(word);
        }

        /// <summary>
        /// Возвращает коллекцию неповторяющихся лемм глагола, прилагательного. существительного
        /// </summary>
        /// <param name="text">Текст</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        public Array GetNormalFormCollection([MarshalAs(UnmanagedType.BStr)] string text)
        {
            // Разбиваем текст на массив слов
            // Убираем лишние пробелы
            //
            var arrayWords = text.Split(' ')
                .Where(x => !String.IsNullOrWhiteSpace(x));

            // Преобразуем в нормальную форму
            // Если слова нет в словаре, то оно не попадет в итоговую коллекцию
            //
            var result = arrayWords.GroupBy(x => x)
                         .Select(x => GetNormalForm(x.Key)).Where(x => x.PartOfSpeech == "ИНФИНИТИВ" || x.PartOfSpeech == "С" || x.PartOfSpeech == "П")
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
            return GetNormalFormCollection(text);
        }

        List<Lemrgramtab> GetGramtab(string RMLPath)
        {
            var result = new List<Lemrgramtab>();
            var fileStrings = System.IO.File.ReadAllLines(Path.GetFullPath(RMLPath) + @"\Dicts\Morph\rgramtab.tab", Encoding.GetEncoding(1251));
            for (int i = 0; i < fileStrings.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(fileStrings[i]) && fileStrings[i].Length > 1 && fileStrings[i][1] != '/')
                {
                    var arrayString = fileStrings[i].Split(' ');
                    if (arrayString.Length > 3)
                        result.Add(new Lemrgramtab() { EndingWord = arrayString[0], PartOfSpeech = arrayString[2], Kind = arrayString[3] });
                }
            }

            return result;
        }

        public string Version
        {
            get
            {
                var version = typeof(LemmatizerDenvic).Assembly.GetName().Version;
                return String.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            }
        }
    }
}
