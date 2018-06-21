using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace ZipCompress
{
    class Decompressor : IZip
    {
        bool IsNotTheEnd = true;
        bool WorkISEnded = false;
        long inFilePosition = 0;

        string sourceFile, targetFile;
        object block = new object();

        public Decompressor(string sourceFile, string targetFile)
        {
            this.sourceFile = sourceFile;
            this.targetFile = targetFile;
        }

        public override void Operation()
        {
            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            Console.WriteLine("Декомпрессия: файл {0} пержимаем в файл {1}", sourceFile, targetFile);
            try
            {
                while (!WorkISEnded)
                {
                    if (IsNotTheEnd && Program.ProcessCount < Environment.ProcessorCount)
                    {
                        Program.ProcessCount++;
                        IsNotTheEnd = false;
                        Thread th = new Thread(Decompress);
                        th.Start();
                    }
                }
            }
            catch (Exception ex)
            {
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

        private void Decompress()
        {
            DataCompressBlock data;
            FileStream fs = new FileStream(sourceFile, FileMode.Open);

            fs.Position = inFilePosition;            
            BinaryFormatter formatter = new BinaryFormatter();
            data = (DataCompressBlock)formatter.Deserialize(fs);

            float proc = ((float)fs.Position / (float)fs.Length) * 100;
            Console.Write("\rРаспаковка: {0:0.0}% ", proc);

            if (fs.Position != fs.Length)
            {
                inFilePosition = fs.Position;
                fs.Close();
                IsNotTheEnd = true;
            }else if (stop)
            {
                fs.Close();
            }
            

            // Декомпрессия блока 

            byte[] sourceBuff = new byte[data.CompressSize];
            byte[] targetBuff = new byte[data.OriginalSize];

            Array.Copy(data.data, sourceBuff, data.CompressSize);

            using (MemoryStream output = new MemoryStream(sourceBuff))
            {
                using (GZipStream ds = new GZipStream(output, CompressionMode.Decompress))
                {
                    ds.Read(targetBuff, 0, targetBuff.Length);
                }
            }

            // И запись в оригинальный файл с расстановкой блоков по своим местам
            lock (block)
            {                
                FileStream outFile = new FileStream(targetFile, FileMode.OpenOrCreate, FileAccess.Write);
                outFile.Position = data.Position;

                outFile.Write(targetBuff, 0, targetBuff.Length);
                outFile.Close();
                Program.ProcessCount--;
            }
            if (Program.ProcessCount == 0 || stop)
                // Если рабочих процессов больше нет, завершаем работу программы
                WorkISEnded = true;
        }
    }
}
