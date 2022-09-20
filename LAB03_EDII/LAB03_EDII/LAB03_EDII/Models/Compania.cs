using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Huffman;

namespace LAB03_EDII.Models
{
    public class Compania
    {
        public string Name { get; set; }
        public byte[] dpicod { get; set; }
        [JsonIgnore]
        public HuffmanTree Libreria = new HuffmanTree();
    }
}
