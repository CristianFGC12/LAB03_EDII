using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAB03_EDII.Models;
using ARBOL_AVL;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace LAB03_EDII
{
    internal class Program
    {
        public static AVLTree<Ingreso> solicitante = new AVLTree<Ingreso>();
        static void Main(string[] args)
        {
            string ruta = "";
            Console.WriteLine("Ingrese la direccion de archvio");
            ruta = Console.ReadLine();
            Console.WriteLine("Escriba el directorio con las cartas");
            string ruteletters = Console.ReadLine();
            Console.WriteLine("Escriba el directorio donde guardar las cartas comprimidas");
            string rutelettercomp = Console.ReadLine();
            var reader = new StreamReader(File.OpenRead(ruta));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var value = line.Split(';');
                if (value[0] == "INSERT")
                {
                    var data = JsonConvert.DeserializeObject<Solicitante>(value[1]);
                    Solicitante trabajar = data;
                    List<string> dupli = trabajar.companies.Distinct().ToList();
                    trabajar.companies = dupli;
                    List<Compania> companias = new List<Compania>();
                    for (int i = 0; i < trabajar.companies.Count; i++)
                    {
                        Compania comp = new Compania();
                        comp.Name = trabajar.companies[i];
                        comp.Libreria.Build(comp.Name + "/" + trabajar.dpi);
                        comp.dpicod = comp.Libreria.Encode(comp.Name + "/" + trabajar.dpi);
                        companias.Add(comp);
                    }
                    Ingreso ingreso = new Ingreso();
                    ingreso.name = trabajar.name;
                    ingreso.dpi = trabajar.dpi;
                    ingreso.address = trabajar.address;
                    ingreso.dateBirth = trabajar.dateBirth;
                    ingreso.companies = companias;
                    string dpicomp = ingreso.dpi;
                    solicitante.insert(ingreso, ComparacioDPI);
                    string[] files = Directory.GetFiles(ruteletters);
                    Regex regex = new Regex(@"REC-" + dpicomp);
                    int numcart = 1;
                    foreach (string file in files)
                    {
                        Match match = regex.Match(file);
                        if (match.Success)
                        {
                            string text = System.IO.File.ReadAllText(file);
                            List<int> compress = Compress(text);
                            string rutecompress = rutelettercomp + "\\" + "compressed-REC-" + dpicomp + "-" + Convert.ToString(numcart) + ".txt";
                            string ingresocomp = JsonConvert.SerializeObject(compress);
                            File.WriteAllText(rutecompress, ingresocomp);
                            Console.WriteLine(file);
                            numcart++;
                        }
                    }

                }
                else if (value[0] == "PATCH")
                {
                    var data = JsonConvert.DeserializeObject<Solicitante>(value[1]);
                    Solicitante trabajar = data;
                    Ingreso busqueda = new Ingreso();
                    busqueda.name = trabajar.name;
                    busqueda.dpi = trabajar.dpi;
                    if (solicitante.Search(busqueda, ComparacioDPI).name == trabajar.name)
                    {
                        if (trabajar.dateBirth != null)
                        {
                            solicitante.Search(busqueda, ComparacioDPI).dateBirth = trabajar.dateBirth;
                        }
                        if (trabajar.address != null)
                        {
                            solicitante.Search(busqueda, ComparacioDPI).address = trabajar.address;
                        }
                        if (trabajar.companies != null)
                        {
                            List<string> dupli = trabajar.companies.Distinct().ToList();
                            List<Compania> sindupli = new List<Compania>();
                            for (int i = 0; i < dupli.Count; i++)
                            {
                                Compania comp = new Compania();
                                comp.Name = dupli[i];
                                comp.Libreria.Build(comp.Name + "/" + trabajar.dpi);
                                comp.dpicod = comp.Libreria.Encode(comp.Name + "/" + trabajar.dpi);
                                sindupli.Add(comp);
                            }
                            solicitante.Search(busqueda, ComparacioDPI).companies = sindupli;
                        }

                    }
                }
                else if (value[0] == "DELETE")
                {
                    var data = JsonConvert.DeserializeObject<Solicitante>(value[1]);
                    Solicitante trabajar = data;
                    Ingreso ingreso = new Ingreso();
                    ingreso.dpi = trabajar.dpi;
                    List<Ingreso> trabajo = solicitante.getAll();
                    int cant = trabajo.Count();
                    for (int i = 0; i < trabajo.Count; i++)
                    {
                        if (trabajo[i].dpi == ingreso.dpi)
                        {
                            trabajo.RemoveAt(i);
                        }
                    }
                    solicitante = new AVLTree<Ingreso>();
                    int cant2 = trabajo.Count();
                    for (int j = 0; j < trabajo.Count; j++)
                    {
                        solicitante.insert(trabajo[j], ComparacioDPI);
                    }
                }
            }
            string dpi;
            string rutesave;
            Console.WriteLine("Escriba el DPI que desea buscar");
            dpi = Console.ReadLine();
            Ingreso solicitudesearch = new Ingreso();
            Ingreso solicitudend = new Ingreso();
            solicitudesearch.dpi = dpi;
            solicitudend = solicitante.Search(solicitudesearch, ComparacioDPI);
            Console.WriteLine("Escriba donde guardar el archivo");
            rutesave = Console.ReadLine();
            List<Ingreso> solicitantelist = new List<Ingreso>();
            solicitantelist.Add(solicitudend);
            Serializacion2(solicitantelist, rutesave);
            string[] compressfiles = Directory.GetFiles(rutelettercomp);
            Console.WriteLine("Escriba el directorio donde guardar las cartas descomprimidas");
            string ruteletterdecode = Console.ReadLine();
            Regex regex2 = new Regex(@"compressed-REC-" + dpi);
            int letternum = 1;
            foreach(string cfile in compressfiles) 
            {
                Match m = regex2.Match(cfile);
                if (m.Success) 
                {
                    var reader2 = new StreamReader(File.OpenRead(cfile));
                    List<int> compress = JsonConvert.DeserializeObject<List<int>>(reader2.ReadLine());
                    string decompress = Decompress(compress);
                    string rutedecompress = ruteletterdecode + "\\" + "decompressed-REC-" + dpi + "-" + Convert.ToString(letternum) + ".txt";
                    File.WriteAllText(rutedecompress, decompress);
                    letternum++;
                }
            }
            Console.ReadKey();
        }
        public static bool ComparacioDPI(Ingreso paciente, string operador, Ingreso paciente2)
        {
            int Comparacion = string.Compare(paciente.dpi, paciente2.dpi);
            if (operador == "<")
            {
                return Comparacion < 0;
            }
            else if (operador == ">")
            {
                return Comparacion > 0;
            }
            else if (operador == "==")
            {
                return Comparacion == 0;
            }
            else return false;
        }
        public static void Serializacion2(List<Ingreso> Lista, string path)
        {
            string solictanteJson = JsonConvert.SerializeObject(Lista.ToArray(), Formatting.Indented);
            File.WriteAllText(path, solictanteJson);
        }
        public static List<int> Compress(string uncompressed)
        {
            // build the dictionary
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(((char)i).ToString(), i);

            string w = string.Empty;
            List<int> compressed = new List<int>();

            foreach (char c in uncompressed)
            {
                string wc = w + c;
                if (dictionary.ContainsKey(wc))
                {
                    w = wc;
                }
                else
                {
                    // write w to output
                    compressed.Add(dictionary[w]);
                    // wc is a new sequence; add it to the dictionary
                    dictionary.Add(wc, dictionary.Count);
                    w = c.ToString();
                }
            }

            // write remaining output if necessary
            if (!string.IsNullOrEmpty(w))
                compressed.Add(dictionary[w]);

            return compressed;
        }

        public static string Decompress(List<int> compressed)
        {
            // build the dictionary
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(i, ((char)i).ToString());

            string w = dictionary[compressed[0]];
            compressed.RemoveAt(0);
            StringBuilder decompressed = new StringBuilder(w);

            foreach (int k in compressed)
            {
                string entry = null;
                if (dictionary.ContainsKey(k))
                    entry = dictionary[k];
                else if (k == dictionary.Count)
                    entry = w + w[0];

                decompressed.Append(entry);

                // new sequence; add it to the dictionary
                dictionary.Add(dictionary.Count, w + entry[0]);

                w = entry;
            }

            return decompressed.ToString();
        }
    }
}
