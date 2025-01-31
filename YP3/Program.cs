using Microsoft.EntityFrameworkCore;
using static Programm;
using Npgsql;


class Programm
{
    static void Main()
    {
        Console.WriteLine("Введите комманду 1)");
        string Name = "";
        products product = new products { Name = "", much = 0, price = 0, date = "" };
        int chose = int.Parse(Console.ReadLine());
        switch (chose)
        {
            case 1:
                Console.WriteLine("Напишите название товара");   
                Name = Console.ReadLine();
                product = Search(Name);
                if (product.Name != "")
                {

                    Console.WriteLine("Введите количество товара");
                    product.much = int.Parse(Console.ReadLine());
                    Console.WriteLine("Введите количество цену");
                    product.price = int.Parse(Console.ReadLine());
                    Create(product);
                }
                else
                    Console.WriteLine("Уже имеется данный товар");
                break;
                case 2:
                Console.WriteLine("Напишите название товара");
                Name = Console.ReadLine();
                product = Search(Name);
                if (product != null)
                {
                    Console.WriteLine("Введите количество проданного товара");
                    product.sell = int.Parse(Console.ReadLine());
                    while(product.much <= product.sell)
                    {
                        Console.WriteLine("Продаж должно быть меньше чем на складе");
                        product.sell = int.Parse(Console.ReadLine());
                    }
                    product.much -= product.sell;
                    Console.WriteLine("Введите дату продажи ДД-ММ-ГГГГ");
                    product.date = Console.ReadLine();
                    UpdateProductInDatabase(product);
                }
                else 
                    Console.WriteLine("Ваш продукт не найден"); 
                break;
            case 3: Console.WriteLine("ОТЧЕТ:\n");
                using (ApplicationContext db = new ApplicationContext())
                {
                    // получаем объекты из бд и выводим на консоль
                    var users = db.Users.ToList();
                    foreach (products u in users)
                    {
                        Console.WriteLine($"{u.Id}.{u.Name} остаток {u.much} выручка {u.sell * u.price} последняя продажа {u.date}");
                    }
                }
                break;
        }
        void Create(products product)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Users.AddRange(product);
                db.SaveChanges();
            }
        }
       products Search(String name)
       {
            products product = new products { Name = name };
            bool SearchComplite = false;
            using (ApplicationContext db = new ApplicationContext())
            {
                var users = db.Users.ToList();
                foreach (products u in users)
                {
                    if(u.Name == name)
                    {
                        SearchComplite = true;
                        product = u;
                    }
                }
            }
            if (SearchComplite)
                return product;
            else 
                return null;
       }
    }

    static void UpdateProductInDatabase(products product)
    {
        string connectionString = "Host=localhost;Port=5432;Database=shop;Username=postgres;Password=123"; // Замените на вашу строку подключения
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            string query = "UPDATE \"Users\" SET  \"much\" = @much,  \"sell\" = @sell,  \"date\" = @date WHERE  \"Name\" = @Name"; // Замените на ваше имя таблицы и столбца

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("much", product.much);
                command.Parameters.AddWithValue("sell", product.sell);
                command.Parameters.AddWithValue("date", product.date);
                command.Parameters.AddWithValue("Name", product.Name);


            }
        }
    }
    public class products
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int much { get; set; }
        public int price { get; set; }
        public int sell { get; set; }
        public string date { get; set; }
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<products> Users { get; set; } = null!;

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=shop;Username=postgres;Password=123");
        }
    }
}