using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Xml.Linq;

namespace Import_from_tsv_to_DB
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string fileName;
            int TypeImport, action;

            while (true)
            {
                Console.WriteLine("\nНажмите:\n1-Для импорта\n2-Для вывода\n0-Для выхода");
                while (true)
                {
                    try
                    {
                        action = Convert.ToInt32(Console.ReadLine());
                        break;
                    }
                    catch
                    {
                        Console.WriteLine("Вы ввели не цифру!\n");
                    }
                }

                switch (action)
                {
                    case 0:
                        Environment.Exit(0);
                        break;

                    case 1:
                        Console.WriteLine("\nВведите путь до файла (без кавычек):");
                        while (true)
                        {
                            fileName = Console.ReadLine();
                            if (File.Exists(fileName))
                            {
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Вы ввели не существующий путь! Попробуйте еще раз!\n");
                            }
                        }

                        Console.WriteLine("\nВыберите тип импорта:\n1-Подразделение\n2-Сотрудник\n3-Должность");
                        while (true)
                        {
                            try
                            {
                                TypeImport = Convert.ToInt32(Console.ReadLine());
                                break;
                            }
                            catch
                            {
                                Console.WriteLine("Вы ввели некорректную команду! Попробуйте еще раз!\n");
                            }
                        }

                        Import(fileName, TypeImport);
                        break;

                    case 2:
                        Departaments();
                        Console.WriteLine("Вывести подразделение по ID?\n1-Да\n2-Нет\n3-Вывести остальные таблицы");
                        while (true)
                        {
                            int dep_by_id = Convert.ToInt32(Console.ReadLine());
                            switch (dep_by_id)
                            {
                                case 1:
                                    DepartmentByID();
                                    break;
                                case 2:
                                    break;
                                case 3:
                                    ShowOtherTables();
                                    break;
                            }
                            break;
                        }
                        break;
                            
                }
            }
        }
      
        static void Import(string fileName, int TypeImport)
        {
            switch (TypeImport)
            {
                case (1):
                    ImportDepartament(fileName);
                    break;
                case (2):
                    ImportEmployee(fileName);
                    break;
                case (3):
                    ImportJobTitle(fileName);
                    break;
                default:
                    Console.WriteLine("Вы ввели не корректную команду!");
                    break;
            }
            Departaments();
            ShowOtherTables();

        }
        public static string FirstUpperName(string str)
        {
            string[] s = str.Split(' ');

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i].Length > 1)
                    s[i] = s[i].Substring(0, 1).ToUpper() + s[i].Substring(1, s[i].Length - 1).ToLower();
                else s[i] = s[i].ToUpper();
            }
            return string.Join(" ", s);
        }
        public static string FirstUpper(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {

                str = str.Substring(0, 1).ToUpper() + str.Substring(1, str.Length - 1).ToLower();
            }
            return string.Join(" ", str);
        }
        static void ImportDepartament(string fileName)
        {
            using var bd = new BdContext();
            var list = File.ReadAllLines(fileName).Skip(1);
            var departamentsInBD = bd.Departaments.ToList();

            foreach (var item in list)
            {
                try
                {
                    if (item != "\t\t\t")
                    {
                        var value = item.Split('\t');
                        for (int i = 0; i < value.Length; i++)
                        {
                            value[i] = value[i].TrimStart(' ');
                            value[i] = value[i].TrimEnd(' ');
                            value[i] = value[i].Replace("  ", " ");
                        }
                        value[3] = value[3].Replace(" ", "");
                        value[3] = value[3].Replace("(", "");
                        value[3] = value[3].Replace(")", "");
                        value[3] = value[3].Replace("-", "");

                        value[0] = FirstUpper(value[0]);
                        Departament d = new Departament();
                        d.Name = value[0];

                        try
                        {
                            d.Managerid = (from Emmployee in bd.Employees
                                           where Emmployee.Fullname == value[2]
                                           select Emmployee.Id).Single();
                        }
                        catch
                        {
                            d.Managerid = null;
                        }
                        d.Phone = value[3];
                        try
                        {
                            d.Parentid = (from Departament in bd.Departaments
                                          where Departament.Name == value[1]
                                          select Departament.Id).Single();

                            Departament departament_update = bd.Departaments.FirstOrDefault(s => s.Name == d.Name && s.Parentid == d.Parentid);
                            Update_or_Create_departaments(d.Parentid, d.Name, d.Managerid, d.Phone, bd, departament_update);
                        }
                        catch
                        {
                            d.Parentid = null;
                            Departament departament_update = bd.Departaments.FirstOrDefault(s => s.Name == d.Name);
                            Update_or_Create_departaments(d.Parentid, d.Name, d.Managerid, d.Phone, bd, departament_update);
                        }
                        bd.SaveChanges();
                    }
                }
                catch { Console.WriteLine("stderror"); }
              
            }
        }
        static void Update_or_Create_departaments(long? parent_id, string name, long? manager_id, string phone, BdContext bd, Departament departament_update)
        {
            if (departament_update != null)
            {
                departament_update.Name = name;
                departament_update.Parentid = parent_id;
                departament_update.Managerid = manager_id;
                departament_update.Phone = phone;
                bd.Departaments.Update(departament_update);
            }
            else
            {
                Departament departament_new = new Departament
                {
                    Name = name,
                    Parentid = parent_id,
                    Managerid = manager_id,
                    Phone = phone,
                };
                bd.Departaments.Add(departament_new);
            }
        }

        static void ImportEmployee(string fileName)
        {
            using var bd = new BdContext();

            var list = File.ReadAllLines(fileName).Skip(1);
            var employeesInBD = bd.Employees.ToList();
            List<Employee> employees = new List<Employee>();
            foreach (var line in list)
            {
                try
                {
                    if (line != "\t\t\t\t\t")
                    {
                        var value = line.Split('\t');

                        for (int i = 0; i < value.Length; i++)
                        {
                            value[i] = value[i].TrimStart(' ');
                            value[i] = value[i].TrimEnd(' ');
                            value[i] = value[i].Replace("  ", " ");
                        }
                        value[0] = FirstUpper(value[0]);
                        value[1] = FirstUpperName(value[1]);
                        value[4] = FirstUpper(value[4]);
                        Employee e = new Employee();
                        try
                        {
                            e.Departament = (from Departament in bd.Departaments
                                             where Departament.Name == value[0]
                                             select Departament.Id).Single();
                        }
                        catch
                        {
                            e.Departament = null;
                        }

                        e.Fullname = value[1];
                        e.Login = value[2];
                        e.Password = value[3];
                        try
                        {
                            e.Jobtitleid = (from Jobtitle in bd.Jobtitles
                                            where Jobtitle.Name == value[4]
                                            select Jobtitle.Id).Single();
                            try
                            {
                                Employee employee_update = bd.Employees.FirstOrDefault(s => s.Fullname == e.Fullname);
                                if (employee_update != null)
                                {
                                    employee_update.Fullname = e.Fullname;
                                    employee_update.Departament = e.Departament;
                                    employee_update.Password = e.Password;
                                    employee_update.Jobtitleid = e.Jobtitleid;
                                    bd.Employees.Update(employee_update);
                                }
                                else
                                {
                                    Employee employee_new = new Employee()
                                    {
                                        Fullname = e.Fullname,
                                        Departament = e.Departament,
                                        Password = e.Password,
                                        Jobtitleid = e.Jobtitleid

                                    };
                                    bd.Employees.Add(employee_new);
                                }
                                bd.SaveChanges();
                            }
                            catch { }
                        }
                        catch
                        {
                            Console.WriteLine("stderror");
                        }
                       
                    }
                }
                catch { Console.WriteLine("stderror"); }
                
            }
        }

        static void ImportJobTitle(string fileName)
        {
            using var bd = new BdContext();

            var list = File.ReadAllLines(fileName).Skip(1);

            foreach (var line in list)
            {
                try
                {
                    var temp = line.Replace("  ", " ");
                    temp = temp.TrimStart(' ');
                    temp = temp.TrimEnd(' ');
                    if (temp.Length > 0)
                    {
                        Jobtitle jobtitle_update = bd.Jobtitles.FirstOrDefault(x => x.Name == temp);
                        if (jobtitle_update != null)
                        {
                            jobtitle_update.Name = temp;
                            bd.Jobtitles.Update(jobtitle_update);
                        }
                        else
                        {
                            Jobtitle jobtitle_new = new Jobtitle
                            {
                                Name = temp
                            };
                            bd.Jobtitles.Add(jobtitle_new);
                        }
                        bd.SaveChanges();
                    }
                }
                catch 
                {
                    Console.WriteLine("stderror");
                }
               
            }
        }

        static List<string> Recursive(BdContext bd, List<Departament> list_departaments, string f, string space)
        {
            List<string> ls = new List<string>();
            f = f + "=";
            space = space.PadLeft(f.Length-1, ' ');
            var jobtitle = "";
            var manager = "";
            try
            {
                for (var i = 0; i < list_departaments.Count; i++)
                {
                    ls.Add($"{f}{list_departaments[i].Name} ID={list_departaments[i].Id}");
                    try
                    {
                        manager = (from Employee in bd.Employees
                                   where Employee.Id == list_departaments[i].Managerid
                                   select Employee.Fullname).Single();
                        var jobid = (from Employee in bd.Employees
                                     where Employee.Id == list_departaments[i].Managerid
                                     select Employee.Jobtitleid).Single();
                        jobtitle = (from Jobtitle in bd.Jobtitles
                                    where Jobtitle.Id == jobid
                                    select Jobtitle.Name).First();

                        ls.Add($"{space}*{manager} ID={list_departaments[i].Managerid} ({jobtitle} ID={jobid})");
                    }
                    catch { }
                    try
                    {
                        var employee = (from Employee in bd.Employees
                                        where Employee.Departament == list_departaments[i].Id
                                        select Employee).ToList();
                        foreach (Employee emp in employee)
                        {
                            if (emp.Fullname == manager)
                            {
                                continue;
                            }
                            jobtitle = (from Jobtitle in bd.Jobtitles
                                        where Jobtitle.Id == emp.Jobtitleid
                                        select Jobtitle.Name).Single();
                            ls.Add($"{space}-{emp.Fullname} ID={emp.Id} ({jobtitle} ID={emp.Jobtitleid})");
                        }

                    }
                    catch { }
                    try
                    {
                        long id = list_departaments[i].Id;
                        var depart = (from Departament in bd.Departaments
                                      where Departament.Parentid == id
                                      select Departament).ToList();
                        List<Departament> list = depart;
                        ls.AddRange(Recursive(bd, list, f, space));
                    }
                    catch { }
                }
            }
            catch { }

            return ls;
        }
        static List<string> ShowDepartamentByID(BdContext bd, Departament d)
        {
            List<string> ls = new List<string>
            {
                d.Name
            };
            try
            {        
                var parent_d = (from Departament in bd.Departaments
                            where Departament.Id == d.Parentid
                            select Departament).Single();
                ls.AddRange(ShowDepartamentByID(bd, parent_d));
            
            }
            catch { }
            
            return ls;
          
        }
        static List<string> EandM(BdContext bd, Departament d, List<string> ls)
        {
            string f = "=";
            ls.Reverse();
            for (int i =0;i< ls.Count;i++)
            {
                ls[i] = ls[i].Insert(0, f);
                f = f + "=";
            }

            string space = "";
            space = space.PadLeft(ls.Count, ' ');
            var jobtitle = "";
            var manager = "";

            try
            {
                manager = (from Employee in bd.Employees
                           where Employee.Id == d.Managerid
                           select Employee.Fullname).Single();
                var jobid = (from Employee in bd.Employees
                             where Employee.Id == d.Managerid
                             select Employee.Jobtitleid).Single();
                jobtitle = (from Jobtitle in bd.Jobtitles
                            where Jobtitle.Id == jobid
                            select Jobtitle.Name).First();
                ls.Add($"{space}*{manager} ID={d.Managerid} ({jobtitle} ID={jobid})");
            }
            catch { }
            try
            {
                var employee = (from Employee in bd.Employees
                                where Employee.Departament == d.Id
                                select Employee).ToList();
                foreach (Employee emp in employee)
                {
                    if (emp.Fullname == manager)
                    {
                        continue;
                    }
                    jobtitle = (from Jobtitle in bd.Jobtitles
                                where Jobtitle.Id == emp.Jobtitleid
                                select Jobtitle.Name).Single();
                    ls.Add($"{space}-{emp.Fullname} ID={emp.Id} ({jobtitle} ID={emp.Jobtitleid})");
                }

            
            }
            catch { }
            return ls;
        }

        static void Departaments()
        {
            using var bd = new BdContext();
            var departaments = bd.Departaments.OrderBy(dep => dep.Name);
            Console.WriteLine("\nDepartaments:");

            var parent = (from Departament in bd.Departaments
                          where Departament.Parentid == null
                          select Departament).ToList();
            List<Departament> list = parent;
            string f = "";
            string space = "";
            List<string> ls = Recursive(bd, list, f, space);
            foreach (string de in ls)
            {
                Console.WriteLine(de);
            }
           
            
        }
        static void DepartmentByID()
        {
            using var bd = new BdContext();
            
            Console.WriteLine("Введите ID департамента:");
            long id_d = Convert.ToInt64(Console.ReadLine());
            try
            {
                var search_departament = (from Departament in bd.Departaments
                                          where Departament.Id == id_d
                                          select Departament).Single();
                string f = "";
                string space = "";
                List<string> lsID = ShowDepartamentByID(bd, search_departament);
                lsID = EandM(bd, search_departament, lsID);


                foreach (string de in lsID)
                {
                    Console.WriteLine(de);
                }
            }
            catch { }
        }
        static void ShowOtherTables()
        {
            using var bd = new BdContext();

            // получаем объекты из бд и выводим на консоль
            var jobtitles = bd.Jobtitles.OrderBy(s => s.Id);
            Console.WriteLine("\nJob titles:");
            foreach (Jobtitle j in jobtitles)
            {
                Console.WriteLine($"{j.Id}.{j.Name}");
            }

            var employees = bd.Employees.ToList();
            Console.WriteLine("\nEmployees");
            foreach (Employee e in employees)
            {
                var departament = "";
                var jobtitle = "";
                try
                {
                    departament = (from Departament in bd.Departaments
                                   where Departament.Id == e.Departament
                                   select Departament.Name).Single();
                }
                catch { }
                try
                {
                    jobtitle = (from Jobtitle in bd.Jobtitles
                                where Jobtitle.Id == e.Jobtitleid
                                select Jobtitle.Name).Single();
                }
                catch { }

                Console.WriteLine($"{e.Id}.{departament}(id = {e.Departament}) {e.Fullname} {e.Login} {e.Password} {jobtitle}(id = {e.Jobtitleid})");
            }
        }
    }
}
    
    

