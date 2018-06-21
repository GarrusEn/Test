using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace ZipCompress
{
    class Compressor : IZip
    {
        bool IsNotTheEnd = true;
        bool WorkISEnded = false;
        long inFilePosition = 0;
        long outFileposition = 0;
        // максимальный размер блока
        private static int MaxSize = 10240000;
        object block = new object();
        string sourceFile, targetFile;

        public Compressor(string sourceFile, string targetFile)
        {
            this.sourceFile = sourceFile;
            this.targetFile = targetFile;
        }

        public override void Operation()
        {
            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            Console.WriteLine("Компрессия: файл {0} пержимаем в файл {1}", sourceFile, targetFile);
            try
            {
                while (!WorkISEnded)
                {
                    if (IsNotTheEnd && Program.ProcessCount < Environment.ProcessorCount)
                    {
                        Program.ProcessCount++;
                        IsNotTheEnd = false;
                        Thread th = new Thread(Compress);
                        th.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                // Для отлова других исключений
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Подождём пока все треды завершат свою работу
                while (Program.ProcessCount != 0)
                    Thread.Sleep(100);
                if (stop)
                {
                    throw new ConsoleCancelException();
                }
            }
        }

        private void Compress()
        {

            DataCompressBlock data;
            FileStream fs = new FileStream(sourceFile, FileMode.Open);
            byte[] buffer;
            fs.Position = inFilePosition;
            data = new DataCompressBlock(fs.Position);

            long sizeBlock = fs.Length - fs.Position;
            if (sizeBlock < MaxSize)
            {
                buffer = new byte[sizeBlock];
                fs.Read(buffer, 0, (int)sizeBlock);
                data.setSize((int)sizeBlock);
                data.CompressData = buffer;
                float proc = ((float)fs.Position / (float)fs.Length) * 100;
                Console.Write("\rУпаковка: {0:0.0}% ", proc);
                fs.Close();
            }
            else
            {
                buffer = new byte[MaxSize];
                fs.Read(buffer, 0, MaxSize);
                data.setSize(MaxSize);
                data.CompressData = buffer;
                inFilePosition = fs.Position;
                float proc = ((float)fs.Position / (float)fs.Length) * 100;
                Console.Write("\rУпаковка: {0:0.0}% ", proc);
                if (fs.Position != fs.Length)
                {
                    fs.Close();
                    // Разблокируем доступ к файлу для других потоков
                    IsNotTheEnd = true;
                }
                else if (stop || fs.Position == fs.Length)
                {
                    WorkISEnded = true;
                    fs.Close();
                }
            }


            // Сжимаем поученный блок данных

            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream cs = new GZipStream(output, CompressionMode.Compress))
                {
                    cs.Write(data.data, 0, data.data.Length);
                }
                byte[] secondBuff;
                secondBuff = output.ToArray();
                data.CompressSize = secondBuff.Length;
                data.data = new byte[data.CompressSize];
                Array.Copy(secondBuff, data.data, data.CompressSize);
            }

            // теперь сериализуем экземпляр класса и запишем его в файл

            byte[] serializeBuff;

            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, data);
                long size = stream.Length;
                stream.Position = 0;
                stream.Read(serializeBuff = new byte[size], 0, (int)size);
            }
            lock (block)
            {
                FileStream outFile = new FileStream(targetFile, FileMode.OpenOrCreate, FileAccess.Write);
                outFile.Position = outFileposition;

                outFile.Write(serializeBuff, 0, serializeBuff.Length);
                outFileposition = outFile.Position;
                outFile.Close();
                Program.ProcessCount--;
                // Меньший блок может быть только, если он зашёл последним,
                // соответственно цикл запуска потоков можно останавливать.
                // Вызов CancelPressKey также остановит работу
                if (stop || data.OriginalSize < MaxSize)
                    WorkISEnded = true;
            }
        }
    }
}

