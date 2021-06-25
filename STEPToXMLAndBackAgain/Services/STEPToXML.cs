using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace STEPToXMLAndBackAgain.Services
{
    class STEPToXML
    {
        public static void Execute()
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
                    // abre o arquivo STEP21
                    using (var stepModel = IfcStore.Open(file))
                    {
                        var nameXmlFile = "";
                        do
                        {
                            nameXmlFile = EnterNameXmlFile();
                            if (!string.IsNullOrEmpty(nameXmlFile))
                            {
                                // salvar como XML
                                var pathFileSave = Path.Combine(@"FileTests\", nameXmlFile);
                                Console.WriteLine("Aguarde até o final do processo, isso pode demorar...");
                                stepModel.SaveAs(pathFileSave);

                                // abrir arquivo XML
                                using (var xmlModel = IfcStore.Open(pathFileSave))
                                {
                                    // basta dar uma olhada em que contém o mesmo número de entidades e paredes.
                                    var stepCount = stepModel.Instances.Count;
                                    var xmlCount = xmlModel.Instances.Count;

                                    var stepWallsCount = stepModel.Instances.CountOf<IIfcWall>();
                                    var xmlWallsCount = xmlModel.Instances.CountOf<IIfcWall>();

                                    Console.WriteLine($"O arquivo STEP21 tem entidades {stepCount}. O arquivo XML possui entidades {xmlCount}.");
                                    Console.WriteLine($"O arquivo STEP21 tem paredes {stepWallsCount}. O arquivo XML tem paredes {xmlWallsCount}.");

                                    try
                                    {
                                        Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe", pathFileSave);
                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine("Ocorreu um erro, o programa não encontrou o notepad++.exe");
                                    }
                                }
                            }

                        } while (string.IsNullOrEmpty(nameXmlFile));

                        Console.WriteLine("Concluido");

                    }
                }
                else
                {
                    Console.WriteLine("Arquivo não encontrado");
                }

                Execute();
            }
        }

        private static string EnterNameXmlFile()
        {
            Console.WriteLine("Digite o nome do arquivo xml que será criado, ou nada para cancelar o processo!");
            var nameXmlFile = Console.ReadLine();
            if (!string.IsNullOrEmpty(nameXmlFile))
            {
                if (File.Exists(Path.Combine(@"FileTests\", nameXmlFile)))
                {
                    Console.WriteLine("Este Arquivo já existe!!!!!");
                    nameXmlFile = "";
                }
            }

            return nameXmlFile;
        }
    }
}
