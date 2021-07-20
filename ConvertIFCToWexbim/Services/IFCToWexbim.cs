using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using Xbim.Ifc;
using Xbim.ModelGeometry.Scene;

namespace ConvertIFCToWexbim.Services
{
    class IFCToWexbim
    {
        public static void Execute()
        {
            Console.WriteLine("Digite o nome do arquivo, ou nada para sair!");
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
                    using (var model = IfcStore.Open(file))
                    {
                        var context = new Xbim3DModelContext(model);
                        context.CreateContext();

                        string fileWexbim = Path.ChangeExtension(file, "wexBIM");

                        using (var fileCreate = File.Create(fileWexbim))
                        {
                            using (var fileWriter = new BinaryWriter(fileCreate))
                            {
                                model.SaveAsWexBim(fileWriter);
                                fileWriter.Close();
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Arquivo não encontrado");
                }
            }
            Execute();
        }
    }
}
