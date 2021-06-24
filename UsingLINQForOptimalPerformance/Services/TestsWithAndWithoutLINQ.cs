using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace UsingLINQForOptimalPerformance.Services
{
    class TestsWithAndWithoutLINQ
    {
        public static void SelectTypeSelection()
        {
            Console.WriteLine("Digite o nome do arquivo IFC, ou nada para sair!");
            var nameFile = Console.ReadLine();
            if (string.IsNullOrEmpty(nameFile))
            {
                return;
            }
            else
            {
                string file = Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"FileTests\", nameFile);
                if (File.Exists(file))
                {
                    string nameMethod = "";
                    do
                    {
                        Console.WriteLine("Digite 1 para testar o método SelectionWithLinq");
                        Console.WriteLine("Digite 2 para testar o método SelectionWithoutLinqIsSLOW");
                        Console.WriteLine("Digite 3 para testar os dois métodos, SelectionWithLinq e SelectionWithoutLinqIsSLOW");
                        Console.WriteLine("Digite 4 para testar o método, RunInverseSearch (Inverse attributes search)");
                        Console.WriteLine("Ou nada para sair!");

                        string valueMethod = Console.ReadLine();

                        if (string.IsNullOrEmpty(valueMethod))
                        {
                            return;
                        }
                        else
                        {
                            switch (valueMethod)
                            {
                                case "1":
                                    nameMethod = "SelectionWithLinq";
                                    break;
                                case "2":
                                    nameMethod = "SelectionWithoutLinqIsSLOW";
                                    break;
                                case "3":
                                    nameMethod = "SelectionWithoutLinqIsSLOW-SelectionWithLinq";
                                    break;
                                case "4":
                                    nameMethod = "RunInverseSearch";
                                    break;
                                default:
                                    Console.WriteLine("Valor incorreto!");
                                    break;
                            }
                        }
                    } while (string.IsNullOrEmpty(nameMethod));

                    switch (nameMethod)
                    {
                        case "SelectionWithLinq":
                            SelectionWithLinq(file);
                            break;
                        case "SelectionWithoutLinqIsSLOW":
                            SelectionWithoutLinqIsSLOW(file);
                            break;
                        case "SelectionWithoutLinqIsSLOW-SelectionWithLinq":
                            SelectionWithLinq(file);
                            SelectionWithoutLinqIsSLOW(file);
                            break;
                        case "RunInverseSearch":
                            RunInverseSearch(file);
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Arquivo não encontrado");
                }

                SelectTypeSelection();
            }
        }


        /// <summary>
        /// Este método é chamado e tratado pelo método SelectTypeSelection
        /// </summary>
        private static void SelectionWithLinq(string filePath)
        {
            var model = IfcStore.Open(filePath);
            using (var txn = model.BeginTransaction())
            {
                var requiredProducts = new IIfcProduct[0]
                    .Concat(model.Instances.OfType<IIfcWallStandardCase>())
                    .Concat(model.Instances.OfType<IIfcDoor>())
                    .Concat(model.Instances.OfType<IIfcWindow>());

                // Isso irá iterar apenas as entidades que você realmente precisa (9 neste caso)
                int i = 0;
                foreach (var product in requiredProducts)
                {
                    // Faça o que quiser aqui ...
                    i++;
                    Console.WriteLine(product.Name);
                }

                Console.WriteLine(i + " tratadas!");
                txn.Commit();
            }
        }

        /// <summary>
        /// Este método é chamado e tratado pelo método SelectTypeSelection
        /// </summary>
        private static void SelectionWithoutLinqIsSLOW(string filePath)
        {
            var model = IfcStore.Open(filePath);
            using (var txn = model.BeginTransaction())
            {
                // isso irá iterar sobre 47309 entidades em vez de apenas 9 que você precisa neste caso!
                int i = 0;
                int e = 0;
                int t = 0;
                foreach (var entity in model.Instances)
                {
                    i++;
                    e++;
                    if (entity is IIfcWallStandardCase)
                    {
                        // Você pode querer fazer algo aqui. Por favor, NÃO!
                        Console.WriteLine(((IIfcWallStandardCase)entity).Name);
                        i--;
                        t++;
                    }
                    if (entity is IIfcDoor)
                    {
                        // Você pode querer fazer algo aqui. Por favor, NÃO!
                        Console.WriteLine(((IIfcDoor)entity).Name);
                        i--;
                        t++;
                    }
                    if (entity is IIfcWindow)
                    {
                        Console.WriteLine(((IIfcWindow)entity).Name);
                        i--;
                        t++;
                    }
                }

                Console.WriteLine(i + " entity não tratada de " + e + " no total, sendo apenas " + t + " necessárias!");
                txn.Commit();
            }
        }

        /// <summary>
        /// Este método é chamado e tratado pelo método SelectTypeSelection
        /// </summary>
        private static void RunInverseSearch(string file)
        {
            var model = (IModel)IfcStore.Open(file);

            void usingInverseAttributes()
            {
                var noObjets = 0;
                var noRelations = 0;

                foreach (var obj in model.Instances.OfType<IIfcObject>())
                {
                    var relCount = obj.IsDefinedBy.Count();
                    if (relCount > 0)
                    {
                        noObjets++;
                        noRelations += noObjets;

                    }
                }

                Console.WriteLine($"usingInverseAttributes - Número de instâncias IfcObject com propriedades: {noObjets}");
            }

            void notUsingInverseAttributes()
            {
                var result = new HashSet<int>();

                foreach (var rel in model.Instances.OfType<IIfcRelDefinesByProperties>())
                {
                    foreach (var obj in rel.RelatedObjects.OfType<IIfcObject>())
                    {
                        result.Add(obj.EntityLabel);
                    }
                }

                Console.WriteLine($"notUsingInverseAttributes - Número de instâncias IfcObject com propriedades: {result.Count}");
            }

            var w = Stopwatch.StartNew();
            using (var cache = model.BeginInverseCaching())
            {
                usingInverseAttributes();
                w.Stop();

                Console.WriteLine($"Duração da tarefa COM cache inverso, usando atributos inversos: {w.ElapsedMilliseconds}ms");
            }

            using (var cache = model.BeginInverseCaching())
            {
                w.Restart();
                notUsingInverseAttributes();
                w.Stop();

                Console.WriteLine($"Duração da tarefa COM cache inverso, NÃO usando atributos inversos: {w.ElapsedMilliseconds}ms");
            }

            w.Restart();
            usingInverseAttributes();
            w.Stop();
            Console.WriteLine($"Duração da tarefa SEM cache, usando atributos inversos: {w.ElapsedMilliseconds}ms");

            w.Restart();
            notUsingInverseAttributes();
            w.Stop();
            Console.WriteLine($"Duração da tarefa SEM cache, NÃO usando atributos inversos: {w.ElapsedMilliseconds}ms");
        }
    }
}
