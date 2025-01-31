using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

class Program
{
    static void Main()
    {
        bool exit = true;
        while (exit) {
            Console.WriteLine("Введите команду (1 - Добавить Продукт, 2 - Продать товар, 3 - Отчет по продажам 4 - Выход):");
            string name = "";
            User user = new User { Name = "", QuantityInStock = 0, Price = 0 };
            int choice = int.Parse(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.WriteLine("Напишите Продукт:");
                    name = Console.ReadLine();
                    user = SearchUser(name);
                    if (user == null)
                    {
                        user = new User { Name = "", QuantityInStock = 0, Price = 0 };
                        user.Name = name;

                        Console.WriteLine("Введите количество товара:");
                        user.QuantityInStock = int.Parse(Console.ReadLine());
                        Console.WriteLine("Введите цену товара:");
                        user.Price = float.Parse(Console.ReadLine());
                        CreateUser(user);
                    }
                    else
                    {
                        Console.WriteLine("Продукт с таким именем уже существует.");
                    }
                    break;

                case 2:
                    Console.WriteLine("Напишите Продукт:");
                    name = Console.ReadLine();
                    user = SearchUser(name);
                    if (user != null)
                    {
                        Console.WriteLine("Введите количество проданного товара:");
                        int quantitySold = int.Parse(Console.ReadLine());
                        while (user.QuantityInStock < quantitySold)
                        {
                            Console.WriteLine("Продаж должно быть меньше чем на складе. Введите количество снова:");
                            quantitySold = int.Parse(Console.ReadLine());
                        }
                        user.QuantityInStock -= quantitySold;
                        UpdateUserInDatabase(user);

                        // Запрос имени покупателя
                        Console.WriteLine("Введите имя покупателя:");
                        string buyerName = Console.ReadLine();

                        // Запрос даты продажи у пользователя
                        Console.WriteLine("Введите дату продажи (в формате ГГГГ-ММ-ДД):");
                        string dateInput = Console.ReadLine();
                        DateTime saleDate;
                        while (!DateTime.TryParse(dateInput, out saleDate))
                        {
                            // Преобразуем в UTC, если дата не в UTC
                            Console.WriteLine("Неверный формат даты. Отчет не был сформирован.");
                            dateInput = Console.ReadLine();
                        }
                        // Попробуем преобразовать введённую строку в DateTime
                        // Преобразуем в UTC, если дата не в UTC
                        if (saleDate.Kind != DateTimeKind.Utc)
                        {
                            saleDate = TimeZoneInfo.ConvertTimeToUtc(saleDate);
                        }

                        // Рассчитываем сумму продажи
                        float totalSaleAmount = quantitySold * user.Price;

                        // Создание новой записи о продаже
                        CreateSale(new Sale
                        {
                            UserName = user.Name,
                            BuyerName = buyerName, // Сохраняем имя покупателя
                            QuantitySold = quantitySold,
                            SaleDate = saleDate, // Используем введённую дату
                            Sell = totalSaleAmount // Сохраняем сумму продажи
                        });

                        // Выводим сумму продажи на экран
                        Console.WriteLine($"Сумма продажи: {totalSaleAmount} (Количество: {quantitySold}, Цена за единицу: {user.Price})");
                    }

                    else
                    {
                        Console.WriteLine("Продукт не найден.");
                    }
                    break;

                case 3:
                    Console.WriteLine("ОТЧЕТ ПО ПРОДАЖАМ:\n");
                    Console.WriteLine("Введите дату начала периода (в формате ГГГГ-ММ-ДД):");
                    string startDateInput = Console.ReadLine();
                    DateTime startDate;


                    while (!DateTime.TryParse(startDateInput, out startDate))
                    {
                        // Преобразуем в UTC, если дата не в UTC
                        Console.WriteLine("Неверный формат даты. Отчет не был сформирован.");
                        startDateInput = Console.ReadLine();
                    }
                    if (startDate.Kind != DateTimeKind.Utc)
                    {
                        startDate = TimeZoneInfo.ConvertTimeToUtc(startDate);
                    }
                    Console.WriteLine("Введите дату конца периода (в формате ГГГГ-ММ-ДД):");
                    string endDateInput = Console.ReadLine();
                    DateTime endDate;


                    while (!DateTime.TryParse(endDateInput, out endDate))
                    {
                        // Преобразуем в UTC, если дата не в UTC
                        Console.WriteLine("Неверный формат даты. Отчет не был сформирован.");
                        endDateInput = Console.ReadLine();
                    }
                    if (endDate.Kind != DateTimeKind.Utc)
                    {
                        endDate = TimeZoneInfo.ConvertTimeToUtc(endDate);
                    }

                    using (ApplicationContext db = new ApplicationContext())
                    {
                        var sales = db.Shops.Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate).ToList();
                        foreach (var sale in sales)
                        {
                            Console.WriteLine($"Продукт: {sale.UserName}, Имя покупателя: {sale.BuyerName}, Количество: {sale.QuantitySold}, Сумма: {sale.Sell}, Дата: {sale.SaleDate}");
                        }
                    }
                    break;
                case 4:
                        exit = false; break;
            }
        }
    }

    static void CreateUser(User user)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            db.Users.Add(user);
            db.SaveChanges();
        }
    }

    static User SearchUser(string name)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            return db.Users.FirstOrDefault(u => u.Name == name);
        }
    }

    static void UpdateUserInDatabase(User user)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            db.Users.Update(user);
            db.SaveChanges();
        }
    }

    static void CreateSale(Sale sale)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            db.Shops.Add(sale);
            db.SaveChanges();
        }
    }

    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public float QuantityInStock { get; set; } // 'much'
        public float Price { get; set; }
    }

    public class Sale
    {
        public int Id { get; set; }
        public string UserName { get; set; } // 'name'
        public string BuyerName { get; set; } // Новое поле для имени покупателя
        public float QuantitySold { get; set; } // 'sell'
        public DateTime SaleDate { get; set; } // 'date'
        public float Sell { get; set; } // Поле для хранения суммы продажи
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Sale> Shops { get; set; } = null!; // Переименовано в Shops

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
