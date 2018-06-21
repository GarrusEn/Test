using System;

namespace ZipCompress
{
    class Zipper
    {
        string operation;
        string sourceFile;
        string targetFile;

        IZip zip;

        public Zipper(string operation, string sourceFile, string targetFile)
        {
            this.operation = operation;
            this.sourceFile = sourceFile;
            this.targetFile = targetFile;
        }

        public void Started()
        {
            if (operation == Enum.GetName(typeof(Operations), 0))
            {
                zip = new Compressor(sourceFile, targetFile);               
            }
            else
            {
                zip = new Decompressor(sourceFile, targetFile);               
            }
            try
            {
                zip.Started();
            }
            catch (ConsoleCancelException)
            {
                // Перебросим получателю                
                throw new ConsoleCancelException();                              
            }
        }
    }
}
