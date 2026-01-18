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
                Console.WriteLine("\n--- MENU ---");
                Console.WriteLine("1. Tankuj");
                Console.WriteLine("2. Zapłać");
                Console.WriteLine("3. Odjazd");
                Console.WriteLine("4. Raport ilościowy");
                Console.WriteLine("5. Raport kwotowy");
                Console.WriteLine("6. TOP 3 Klientów");
                Console.WriteLine("7. Wyjście");
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

                    case "2":
                        if (!stacja.CzyJestAktywna) { Console.WriteLine("Brak tankowania."); break; }
                        Console.WriteLine("Dokument: [P]aragon / [F]aktura?");
                        if (Console.ReadLine().ToUpper() == "F")
                        {
                            Console.Write("Nazwa: "); string nazwa = Console.ReadLine();
                            Console.Write("Adres: "); string adr = Console.ReadLine();
                            Console.Write("NIP: "); string nip = Console.ReadLine();
                            stacja.Zaplac(nazwa, adr, nip);
                        }
                        else stacja.Zaplac();
                        break;

                    case "3": stacja.Odjazd(); break;
                    case "4": stacja.RaportIlosciowy(); break;
                    case "5":
                        Console.Write("Min kwota: ");
                        var k = decimal.Parse(Console.ReadLine());
                        stacja.RaportKwotowy(k);
                        break;
                    case "6": 
                        stacja.RaportTopKlientow(); 
                        break;
                    case "7": return;
                }
            }
        }
    }
}
