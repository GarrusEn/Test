using System;

namespace ZipCompress
{
    [Serializable]
    class DataCompressBlock
    {
        long iPos;
        int iSize, iCompress;
        public byte[] data;

        public long Position
        {
            get
            {
                return iPos;
            }
        }

        public int OriginalSize
        {
            get
            {
                return iSize;
            }
        }

        public int CompressSize
        {
            set
            {
                iCompress = value;
            }
            get
            {
                return iCompress;
            }
        }

        public DataCompressBlock(long iPos)
        {
            this.iPos = iPos;
        }
        

        public DataCompressBlock setSize(int iSize)
        {
            this.iSize = iSize;
            return this;
        }

        public byte[] CompressData
        {
            get
            {
                return data;
            }
            set
            {
                data = new byte[value.Length];
                data = value;
            }
        }
    }
}
