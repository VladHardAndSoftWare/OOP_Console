using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;
using System.Globalization;

namespace ConsoleApp
{
    public class Stock
    {
        public int ID { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Depth { get; set; }
    }
    public class Box : Stock
    {
        public float Weight { get; set; }
        public DateTime ProductionDate { get; set; }
        public DateTime ExpirationDate => ProductionDate.AddDays(100);

        public float Volume => Width * Height * Depth;
    }

    public class Pallet : Stock
    {
        private List<Box> boxes;
        public float Weight => Boxes.Sum(box => box.Weight) + 30;
        public DateTime ExpirationDate => Boxes.Min(box => box.ExpirationDate);
        public List<Box> Boxes
        {
            get => boxes;
            set
            {
                if (value.All(box => box.Width <= Width && box.Height <= Height))
                    boxes = value;
                else
                    throw new ArgumentException("Каждая коробка не должна превышать по размерам паллету (по ширине и длине).");
            }
        }

        public float Volume => Boxes.Sum(box => box.Volume) + Width * Height * Depth;
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var pallets = new List<Pallet>();

            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1. Использовать тестовый набор данных");
            Console.WriteLine("2. Использовать чтение из файла");
            Console.WriteLine("");
            Console.Write("Выберите действие: ");

            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    Test(pallets);
                    break;
                case "2":
                    LoadPalletsFromFile(pallets, "output.txt");
                    break;
                default:
                    Console.WriteLine("Неверный ввод. Попробуйте снова.");
                    break;
            }


            SavePalletsToFile(pallets, "output.txt");
            Output1(pallets);
            Output2(pallets);
            Console.ReadLine();
        }

        static void Test(List<Pallet> pallets)
        {

            var b1 = new Box { ID = 1, Width = 10, Height = 10, Depth = 10, Weight = 1, ProductionDate = DateTime.Now.AddDays(-40) };
            var b2 = new Box { ID = 2, Width = 5, Height = 9, Depth = 2, Weight = 2, ProductionDate = DateTime.Now.AddDays(-50) };
            var b3 = new Box { ID = 3, Width = 3, Height = 2, Depth = 9, Weight = 3, ProductionDate = DateTime.Now.AddDays(-70) };
            var b4 = new Box { ID = 4, Width = 7, Height = 6, Depth = 8, Weight = 4, ProductionDate = DateTime.Now.AddDays(-90), };
            var b5 = new Box { ID = 5, Width = 9, Height = 8, Depth = 6, Weight = 3, ProductionDate = DateTime.Now.AddDays(-76) };
            var b6 = new Box { ID = 6, Width = 10, Height = 7, Depth = 5, Weight = 2, ProductionDate = DateTime.Now.AddDays(-54) };
            var b7 = new Box { ID = 7, Width = 5, Height = 3, Depth = 6, Weight = 8, ProductionDate = DateTime.Now.AddDays(-38) };
            var b8 = new Box { ID = 8, Width = 4, Height = 7, Depth = 6, Weight = 6, ProductionDate = DateTime.Now.AddDays(-78) };
            var b9 = new Box { ID = 9, Width = 3, Height = 9, Depth = 1, Weight = 3, ProductionDate = DateTime.Now.AddDays(-91) };
            var b10 = new Box { ID = 10, Width = 2, Height = 1, Depth = 3, Weight = 9, ProductionDate = DateTime.Now.AddDays(-45) };

            var p1 = new Pallet { ID = 1, Width = 90, Height = 100, Depth = 100, Boxes = new List<Box> { b1, b2, b3, b5 } };
            var p2 = new Pallet { ID = 2, Width = 200, Height = 200, Depth = 200, Boxes = new List<Box> { b10, b9, b5 } };
            var p3 = new Pallet { ID = 3, Width = 150, Height = 150, Depth = 150, Boxes = new List<Box> { b3 } };
            var p4 = new Pallet { ID = 4, Width = 80, Height = 100, Depth = 500, Boxes = new List<Box> { b1 } };
            var p5 = new Pallet { ID = 5, Width = 300, Height = 300, Depth = 500, Boxes = new List<Box> { b8, b7, b1, b2 } };
            var p6 = new Pallet { ID = 6, Width = 450, Height = 450, Depth = 350, Boxes = new List<Box> { b8, b10, b4 } };
            var p7 = new Pallet { ID = 7, Width = 100, Height = 100, Depth = 200, Boxes = new List<Box> { b10, b7, b3 } };
            var p8 = new Pallet { ID = 8, Width = 200, Height = 200, Depth = 350, Boxes = new List<Box> { b6, b10 } };
            var p9 = new Pallet { ID = 9, Width = 350, Height = 250, Depth = 150, Boxes = new List<Box> { b6, b4, b5 } };
            var p10 = new Pallet { ID = 10, Width = 100, Height = 400, Depth = 100, Boxes = new List<Box> { b7, b4, b2, b8 } };


            pallets.Add(p1);
            pallets.Add(p2);
            pallets.Add(p3);
            pallets.Add(p4);
            pallets.Add(p5);
            pallets.Add(p6);
            pallets.Add(p7);
            pallets.Add(p8);
            pallets.Add(p9);
            pallets.Add(p10);
        }

        static void LoadPalletsFromFile(List<Pallet> pallets, string filePath)
        {
            if (File.Exists(filePath))
            {
                CultureInfo enUS = new CultureInfo("en-US");
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    var values = line.Split(',');

                    var pallet = new Pallet
                    {
                        ID = int.Parse(values[0]),
                        Width = float.Parse(values[1]),
                        Height = float.Parse(values[2]),
                        Depth = float.Parse(values[3]),
                        Boxes = new List<Box>()
                    };

                    for (int i = 4; i < values.Length; i += 6)
                    {
                        var box = new Box
                        {
                            ID = int.Parse(values[i]),
                            Width = float.Parse(values[i + 1]),
                            Height = float.Parse(values[i + 2]),
                            Depth = float.Parse(values[i + 3]),
                            Weight = float.Parse(values[i + 4]),
                            ProductionDate = DateTime.ParseExact(values[i + 5], "M/d/yyyy h:mm:ss tt", enUS)
                        };

                        pallet.Boxes.Add(box);
                    }

                    pallets.Add(pallet);
                }
            }
        }

        static void SavePalletsToFile(List<Pallet> pallets, string filePath)
        {
            var lines = new List<string>();

            foreach (var pallet in pallets)
            {
                var palletLine = $"{pallet.ID},{pallet.Width},{pallet.Height},{pallet.Depth}";

                foreach (var box in pallet.Boxes)
                {
                    var boxLine = $"{box.ID},{box.Width},{box.Height},{box.Depth},{box.Weight},{box.ProductionDate}";
                    palletLine += $",{boxLine}";
                }

                lines.Add(palletLine);
            }

            File.WriteAllLines(filePath, lines);
        }

        static void Output1(List<Pallet> pallets)
        {
            var groupedPallets = pallets.GroupBy(pallet => pallet.ExpirationDate).OrderBy(group => group.Key);

            Console.WriteLine("Паллеты, сгруппированные по сроку годности, отсортированные по возрастанию срока годности:");
            foreach (var group in groupedPallets)
            {
                Console.WriteLine($"Срок годности: {group.Key}");

                var sortedPallets = group.OrderBy(pallet => pallet.Weight);
                foreach (var pallet in sortedPallets)
                {
                    Console.WriteLine($"ID: {pallet.ID}, Вес: {pallet.Weight}");
                }

                Console.WriteLine();
            }
        }

        static void Output2(List<Pallet> pallets)
        {
            var top3Pallets = pallets.OrderByDescending(pallet => pallet.ExpirationDate).Take(3).OrderBy(pallet => pallet.Volume);

            Console.WriteLine("Топ 3 паллеты с наибольшим сроком годности, отсортированные по возрастанию объема:");
            foreach (var pallet in top3Pallets)
            {
                Console.WriteLine($"ID: {pallet.ID}, Срок годности: {pallet.ExpirationDate}, Объем: {pallet.Volume}");
            }
        }
    }
}
