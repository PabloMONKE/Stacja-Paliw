using System;

namespace StacjaBenzynowa
{
    class Program
    {
        static void Main()
        {
            var stacja = new Dystrybutor();

            stacja.Kradziez += t => {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n!!! ALARM !!! Kradzież: {t.Kwota:C} ({t.Paliwo})");
                Console.ResetColor();
            };

            stacja.WczytajBaze();

            while (true)
            {
                Console.WriteLine("\n--- STACJA ---");
                Console.WriteLine("1. Tankuj");
                Console.WriteLine("2. Zapłać");
                Console.WriteLine("3. Odjazd");
                Console.WriteLine("4. Raport ilosciowy");
                Console.WriteLine("5. Raport kwotowy");
                Console.WriteLine("6. Wyjscie");
                Console.Write("> ");

                var wybor = Console.ReadLine();

                switch (wybor)
                {
                    case "1":
                        Console.WriteLine("Paliwo (0-Pb95, 1-Pb98, 2-Diesel, 3-LPG):");
                        var p = (RodzajPaliwa)int.Parse(Console.ReadLine());
                        
                        Console.Write("Litry: ");
                        var l = double.Parse(Console.ReadLine());
                        
                        stacja.Tankuj(p, l);
                        break;

                    case "2": stacja.Zaplac(); break;
                    case "3": stacja.Odjazd(); break;
                    case "4": stacja.RaportIlosciowy(); break;
                    case "5":
                        Console.Write("Min kwota: ");
                        var k = decimal.Parse(Console.ReadLine());
                        stacja.RaportKwotowy(k);
                        break;
                    case "6": return;
                }
            }
        }
    }
}