using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace SpatialHierarchy.Services
{
    class SpatialStructureExample
    {
        public static void Show()
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
                        var proj = model.Instances.FirstOrDefault<IIfcProject>();
                        PrintHierarchy(proj, 0);
                    }
                }
                else
                {
                    Console.WriteLine("Arquivo não encontrado");
                }

                Show();
            }
        }

        private static void PrintHierarchy(IIfcObjectDefinition proj, int v)
        {
            Console.WriteLine(string.Format("{0}{1}[{2}]", GetIndent(0), proj.Name, proj.GetType().Name));
            //apenas elementos espaciais podem conter elementos de construção
            var spatialElement = proj as IIfcSpatialStructureElement;

            if (spatialElement != null)
            {
                // usando IfcRelContainedInSpatialElement para obter os elementos contidos
                var containedElements = spatialElement.ContainsElements.SelectMany(x => x.RelatedElements);
                foreach (var element in containedElements)
                {
                    Console.WriteLine(string.Format("{0}    ->{1}[{2}]", GetIndent(0), element.Name, element.GetType().Name));
                }
            }

            // usando IfcRelAggregares para obter decomposição espacial de elementos de estrutura espacial
            var relatedObjects = proj.IsDecomposedBy.SelectMany(x => x.RelatedObjects);
            foreach (var item in relatedObjects)
            {
                PrintHierarchy(item, v + 1);
            }
        }

        private static string GetIndent(int v)
        {
            var indent = "";
            for (int i = 0; i < v; i++)
            {
                indent = "  ";
            }
            return indent;
        }
    }
}
