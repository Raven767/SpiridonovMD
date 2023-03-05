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
                    //Console.WriteLine(sw.Elapsed);
                    
                }
            }
            //Console.Read();

        }
        static void Create()
        {
            command = new SQLiteCommand(connection)
            {
                CommandText = "CREATE TABLE IF NOT EXISTS [Person]([ФИО] TEXT, [ДатаРождения] TEXT, [Пол] TEXT);"
            };
            command.ExecuteNonQuery();
            Console.WriteLine("Таблица создана");
        }
        static void insert(long a)
        {
            command = new SQLiteCommand(connection);
            command.CommandText = "INSERT INTO Person (ФИО, ДатаРождения, Пол) VALUES (:name, :date, :sex)";
            try
            { 
                transaction = connection.BeginTransaction();//запускаем транзакцию
                for (int i = 0; i < a; i++)
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
                transaction.Commit(); //применяем изменения
            }
            catch
            {
                throw;
            }
        }


        static void Main(string[] args)
        {
            if (Connect("firstBase.sqlite"))
            {
                Stopwatch sw = new Stopwatch();
                foreach (var arg in args)
                {
                    switch (arg)
                    {
                        case "1":
                            Create();
                            break;

                        case "2":
                            //добавление
                            command = new SQLiteCommand(connection);
                            command.CommandText = "INSERT INTO Person (ФИО, ДатаРождения, Пол) VALUES (:name, :date, :sex)";
                            DateTime dDate;
                            if (DateTime.TryParse(args[2], out dDate))
                            {
                                //string.Format("{0:yyyy/MM/dd}", dDate);
                                if ((args[3] == "мужской") || (args[3] == "женский") || (args[3] == "Мужской") || (args[3] == "Женский"))
                                {
                                    transaction = connection.BeginTransaction();//запускаем транзакцию
                                    command.Parameters.AddWithValue("name", args[1]);
                                    command.Parameters.AddWithValue("date", args[2]);
                                    command.Parameters.AddWithValue("sex", args[3]);
                                    command.ExecuteNonQuery();
                                    transaction.Commit(); //применяем изменения
                                    View();
                                }
                                else
                                {
                                    Console.WriteLine("Неверный пол");
                                }
                               
                            }
                            else
                            {
                                Console.WriteLine("Неверная дата");
                            }
                            break;

                        case "3":
                            string sqlExpression = "SELECT ФИО, SUBSTR(ДатаРождения,1,10), Пол, (strftime('%Y', 'now') - strftime('%Y', ДатаРождения)) - (strftime('%m-%d', 'now') < strftime('%m-%d', ДатаРождения)) AS `age` FROM Person group by ФИО";
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
                            }
                            break;

                        case "4":
                            sw.Start();
                            insert(1000000);
                            sw.Stop();
                            Console.WriteLine("Время выполнения добавления 1000000 строк" + sw.Elapsed);
                            break;

                        //очитка таблицы БД
                        case "6":
                            command = new SQLiteCommand(connection)
                            {
                                CommandText = "DELETE FROM Person"
                            };
                            command.ExecuteNonQuery();
                            break;
                        case "7":
                            string SQlExpression = "SELECT count(*) FROM Person";
                            command = new SQLiteCommand(SQlExpression, connection);
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows) // если есть данные
                                {

                                    while (reader.Read())   // построчно считываем данные
                                    {
                                        var FIO = reader.GetValue(0);
                                        Console.WriteLine(FIO);
                                    }
                                }
                            }
                            command.ExecuteNonQuery();
                            break;
                            
                        case "5":
                            /*
                            command = new SQLiteCommand(connection)
                            {
                                CommandText = "CREATE INDEX IF NOT EXISTS PersonIndex ON Person (ФИО, Пол);"
                            };
                            command.ExecuteNonQuery();
                            */

                            sw.Start();
                            string SqlExpression = "SELECT  ФИО, SUBSTR(ДатаРождения,1,10), Пол FROM Person Where ФИО Like 'F%' AND Пол = 'мужской'";
                            command = new SQLiteCommand(SqlExpression, connection);
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
                                sw.Stop();
                                Console.WriteLine("Время выполнения запроса: "+sw.Elapsed);
                            }
                            Console.Read();
                            break;
                    }                  
                }
            }
        }        
    }
    
}
