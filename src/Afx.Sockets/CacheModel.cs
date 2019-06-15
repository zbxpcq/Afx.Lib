using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Sockets.Models
{
    public class CacheModel
    {
        public int Size { get; set; }

        public int Position { get; set; }

        public byte[] Data { get; set; }

        public CacheModel()
        {
            this.Clear();
        }

        public void Clear()
        {
            this.Size = 0;
            this.Position = 0;
            this.Data = null;
        }
    }
}
