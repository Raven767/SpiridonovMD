using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;

namespace SpiridonoMD
{
    class Program
    {
        static SQLiteConnection connection;
        static SQLiteCommand command;
        private static SQLiteTransaction transaction;
        static Random r = new Random();

        static public bool Connect(string fileName)
        {
            //подключение
            try
            {
                connection = new SQLiteConnection("Data Source=usersdata.db;Version=3; FailIfMissing=False");
                connection.Open();
                return true;
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"Ошибка доступа к базе данных. Исключение: {ex.Message}");
                return false;
            }
        }

        static void View()
        {
            Stopwatch sw = new Stopwatch();
            string sqlExpression = "SELECT * FROM Person";
            command = new SQLiteCommand(sqlExpression, connection);
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows) // если есть данные
                {
                    transaction = connection.BeginTransaction();//запускаем транзакцию
                    while (reader.Read())   // построчно считываем данные
                    {
                        var FIO = reader.GetValue(0);
                        var Date = reader.GetValue(1);
                        var Pol = reader.GetValue(2);
                        Console.WriteLine($"{FIO} \t {Date} \t {Pol}");
                    }
                    transaction.Commit(); //применяем изменения
                    sw.Stop();
                    Console.WriteLine(sw.Elapsed);
                }
            }
            Console.Read();
        }
        static void Create(int a)
        {
            command = new SQLiteCommand(connection)
            {
                CommandText = "CREATE TABLE IF NOT EXISTS [Person]([ФИО] TEXT, [ДатаРождения] TEXT, [Пол] TEXT);"
            };
            command.ExecuteNonQuery();
            Console.WriteLine("Таблица создана");

            command = new SQLiteCommand(connection)
            {
                CommandText = "DELETE FROM [Person]"
            };
            command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO Person (ФИО, ДатаРождения, Пол) VALUES (:name, :date, :sex)";
            try
            {
                for (int i = 1; i < a; i++)
                {
                    //создание имени
                    string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
                    string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
                    string Name = "";
                    Name += consonants[r.Next(consonants.Length)].ToUpper();
                    Name += vowels[r.Next(vowels.Length)];
                    int b = 1;
                    while (b < 6)
                    {
                        Name += consonants[r.Next(consonants.Length)];
                        b++;
                        Name += vowels[r.Next(vowels.Length)];
                        b++;
                    }
                    //создание даты
                    var startDate = new DateTime(1998, 1, 1);
                    var newDate = startDate.AddDays(r.Next(366));
                    var NewYear = newDate.AddMonths(r.Next(12));
                    //выборка пола
                    string[] pol = { "мужской", "женский" };
                    string Pol = "";
                    Pol = pol[r.Next(pol.Length)];
                    //заполнение бд
                    command.Parameters.AddWithValue("name", Name);
                    command.Parameters.AddWithValue("date", NewYear);
                    command.Parameters.AddWithValue("sex", Pol);
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
        }

        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            //удаление переполения

            string param;
            if (Connect("firstBase.sqlite"))
            {

                Console.Write("Ввеедите команду: ");
                param = Console.ReadLine();
                if (param == "myApp 1")
                {
                    Create(50);
                    View();
                }
                else if (param == "myApp 2")
                {
                    Create(50);
                    //добавление
                    command.CommandText = "INSERT INTO Person (ФИО, ДатаРождения, Пол) VALUES (:name, :date, :sex)";
                    transaction = connection.BeginTransaction();//запускаем транзакцию
                    try
                    {
                        string name, date, pol;
                        Console.Write("Введите ФИО: ");
                        name = Console.ReadLine();
                        Console.Write("Введите Дату рождения: ");
                        date = Console.ReadLine();
                        Console.Write("Введите Пол: ");
                        pol = Console.ReadLine();
                        command.Parameters.AddWithValue("name", name);
                        command.Parameters.AddWithValue("date", date);
                        command.Parameters.AddWithValue("sex", pol);
                        command.ExecuteNonQuery();
                        transaction.Commit(); //применяем изменения
                        sw.Stop();
                        //Console.WriteLine(sw.Elapsed);
                        View();
                    }
                    catch
                    {
                        //transaction.Rollback(); //откатываем изменения, если произошла ошибка
                        throw;
                    }
                    sw.Restart();
                }
                else if (param == "myApp 3")
                {
                    Create(50);
                    transaction = connection.BeginTransaction();//запускаем транзакцию
                    string sqlExpression = "SELECT ФИО, ДатаРождения, Пол, (strftime('%Y', 'now') - strftime('%Y', ДатаРождения)) - (strftime('%m-%d', 'now') < strftime('%m-%d', ДатаРождения)) AS `age` FROM Person group by ФИО";
                    command = new SQLiteCommand(sqlExpression, connection);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read())   // построчно считываем данные
                            {
                                var FIO = reader.GetValue(0);
                                var Date = reader.GetValue(1);
                                var Pol = reader.GetValue(2);
                                var age = reader.GetValue(3);
                                Console.WriteLine($"{FIO} \t {Date} \t {Pol} \t {age}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Данных нет");
                        }
                        transaction.Commit(); //применяем изменения
                        sw.Stop();
                        Console.WriteLine(sw.Elapsed);
                    }
                    Console.Read();
                }
                else if (param == "myApp 4")
                {
                    Create(1000000);//очень долгий вывод данных для поверки использовал 10000
                    View();
                }
                else if (param == "myApp 5")
                {
                    Create(50);
                    transaction = connection.BeginTransaction();//запускаем транзакцию
                    string sqlExpression = "SELECT * FROM Person Where ФИО Like 'F%' AND Пол = 'мужской'";
                    command = new SQLiteCommand(sqlExpression, connection);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read())   // построчно считываем данные
                            {
                                var FIO = reader.GetValue(0);
                                var Date = reader.GetValue(1);
                                var Pol = reader.GetValue(2);
                                Console.WriteLine($"{FIO} \t {Date} \t {Pol}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Данных нет");
                        }
                        transaction.Commit(); //применяем изменения
                        sw.Stop();
                        Console.WriteLine(sw.Elapsed);
                    }
                    Console.Read();
                }
            }
        }
    }
    
}
