using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.Interfaces;

namespace BasicModelOperations.Services
{
    class CrudExample
    {
        public static void Retrieve()
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
                        //var firstWall = model.Instances.FirstOrDefault<IfcWall>();
                        //var allWalls = model.Instances.OfType<IfcWall>();
                        //var specificWall = model.Instances.Where<IfcWall>(x => x.Name == "MUR_EXT-009");

                        // obter todas as portas no modelo (usando a interface IFC4 de IfcDoor,
                        // isso funcionará tanto para IFC2x3 quanto para IFC4)
                        var allDoors = model.Instances.OfType<IIfcDoor>();

                        // obtém apenas portas com IIfcTypeObject definido
                        var someDoors = model.Instances.Where<IIfcDoor>(x => x.IsTypedBy.Any());

                        if (allDoors.Count() > 0 && allDoors != null)
                        {

                            Random rand = new Random();
                            int skipRandom = rand.Next(0, allDoors.Count() - 1);
                            // pegue uma única porta por id
                            var id = allDoors.Skip(skipRandom).Take(1).First<IIfcDoor>().GlobalId;

                            var theDoor = model.Instances.FirstOrDefault<IIfcDoor>(x => x.GlobalId == id);
                            Console.WriteLine($"Door ID: {theDoor.GlobalId}, Name: {theDoor.Name}");

                            // obtém todas as propriedades de valor único da porta
                            var properties = theDoor.IsDefinedBy
                                .Where(x => x.RelatingPropertyDefinition is IIfcPropertySet)
                                .SelectMany(x => ((IIfcPropertySet)x.RelatingPropertyDefinition).HasProperties)
                                .OfType<IIfcPropertySingleValue>();

                            foreach (var prop in properties)
                                Console.WriteLine($"Property: {prop.Name}, Value: {prop.NominalValue}");
                        }
                        else
                        {
                            Console.WriteLine("Nada encontrado em allDoors = " + allDoors.Count());
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Arquivo não encontrado");
                }

                Retrieve();
            }
        }
    }
}
