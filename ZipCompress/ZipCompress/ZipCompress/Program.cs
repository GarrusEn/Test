using System;
using System.IO;

namespace ZipCompress
{
    class ConsoleCancelException : Exception
    {
        public ConsoleCancelException()
        {}
    }

    class Program
    {
        public static int ProcessCount;
        static int Main(string[] args)
        {
            try
            {
                CheckArgs check = new CheckArgs(args[0], args[1], args[2]);
                if (check.Valid)
                {
                    Console.WriteLine("CTRL+C для отмены.");
                    Zipper zipper = new Zipper(args[0], args[1], args[2]);
                    try
                    {
                        zipper.Started();
                        return 0;
                    }
                    catch (ConsoleCancelException)
                    {
                        Console.WriteLine("Принудительная оcтановка");                        
                        File.Delete(args[2]);
                        Console.WriteLine("Целевой файл удалён");
                        // Если директория была создана программой, она её удалит
                        if (check.CreateDirectory)
                            Directory.Delete(Path.GetDirectoryName(args[2]));
                        return 1;
                    }
                }
                else
                {
                    Console.Write("Проверка: Ошибки\n");
                    Console.WriteLine(check.ErrorResult());
                }
                return 1;
            }
            // На случай, если аргументов не хватает (больше - почему бы и нет, вдруг расширяться)
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Неверное количество аргументов");
                return 1;
            }
        }
    }
}
