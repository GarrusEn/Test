using System;
using System.IO;

namespace ZipCompress
{
    enum Operations
    {
        compress,
        decompress
    };
    class CheckArgs
    {
        bool ICreateDirectory = false;
        int iError = 0;
        bool IsCheckValid = false;
        string error = "";

        public bool CreateDirectory
        {
            get
            {
                return ICreateDirectory;
            }
        }

        public CheckArgs(string operation, string sourceFilePath, string targetFilePath)
        {
            // 
            if (!isFileNameValid(sourceFilePath))
            {
                error += "Путь к исходому файлу содержит недопустимые символы.\n";
                iError = 1;
            }
            if (!isFileNameValid(targetFilePath))
            {
                error += "Путь к целевому файлу содержит недопустимые символы.\n";
                iError = 1;
            }
            if (!Enum.IsDefined(typeof(Operations), operation))
            {
                error += "Первый аргумент должен быть <compress> или <decompress>\n";
                iError = 1;
            }
            if (!File.Exists(sourceFilePath))
            {
                error += "Исходный файл не найден\n";
                iError = 1;
            }
            if (File.Exists(targetFilePath))
            {
                error += "Целевой файл уже существует\n";
                iError = 1;
            }
            else
                if (!Directory.Exists(Path.GetDirectoryName(targetFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
                ICreateDirectory = true;
            }

            // Подведём итоги проверок при помощи тернарного оператора
            IsCheckValid = iError == 1 ? false : true;
        }
        // Свойство проверки реультата валидности
        public bool Valid
        {
            get
            {
                return IsCheckValid;
            }
        }

        // Метод возвращающий все накопившиеся в результате проверки ошибки.
        public string ErrorResult()
        {
            return error;
        }

        // Проверка допустимых символов в FilePath
        private bool isFileNameValid(string fileName)
        {
            if ((fileName == null) || (fileName.IndexOfAny(Path.GetInvalidPathChars()) != -1))
                return false;
            try
            {
                var tempFileInfo = new FileInfo(fileName);
                return true;
            }
            // Затыкаем ArgumentExceprion чтобы не захламлять консоль двойной информацией
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
