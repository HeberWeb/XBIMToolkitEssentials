using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace Services.ExcelSpaceReportFromIFC
{
    class ExcelReportIFC
    {
        public static void Export()
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
                    // inicializar a pasta de trabalho NPOI a partir do modelo
                    var workBook = new XSSFWorkbook(@"FileTests\template.xlsx");
                    var sheet = workBook.GetSheet("Spaces");
                    // Crie formatos numéricos agradáveis ​​com unidades. As unidades precisariam de MUITO MAIS cuidado no mundo real. 
                    // Nós apenas sabemos que nosso modelo atual tem áreas espaciais em metros quadrados e volumes espaciais em metros cúbicos
                    // Observe que os dados originais exportados do Revit estavam errados porque os volumes eram 1000 vezes maiores do que deveriam ser. 
                    // Os dados foram corrigidos usando xbim para este exemplo.

                    var areaFormat = workBook.CreateDataFormat();
                    var areaFormatId = areaFormat.GetFormat("# ##0.00 [$m²]");
                    var areaStyle = workBook.CreateCellStyle();
                    areaStyle.DataFormat = areaFormatId;

                    var volumeFormat = workBook.CreateDataFormat();
                    var volumeFormatId = volumeFormat.GetFormat("# ##0.00 [$m³]");
                    var volumeStyle = workBook.CreateCellStyle();
                    volumeStyle.DataFormat = volumeFormatId;

                    // Abra o modelo IFC. Não vamos mudar nada no modelo, então podemos deixar de fora as credenciais do editor.
                    using (var model = IfcStore.Open(file))
                    {
                        //Get all spaces in the model. 
                        //We use ToList() here to avoid multiple enumeration with Count() and foreach(){}
                        var spaces = model.Instances.OfType<IIfcSpace>().ToList();

                        // Definir o conteúdo do cabeçalho
                        sheet.GetRow(0).GetCell(0)
                            .SetCellValue($"Space Report {spaces.Count} spaces");

                        foreach (var space in spaces)
                        {
                            WriteSpaceRow(space, sheet, areaStyle, volumeStyle);
                        }
                        
                        var nameExcelFile = "";
                        do
                        {
                            nameExcelFile = EnterNameExcelFile();
                            if (!string.IsNullOrEmpty(nameExcelFile))
                            {
                                using (var stream = File.Create(Path.Combine(@"FileTests\", nameExcelFile)))
                                {
                                    workBook.Write(stream);
                                    stream.Close();
                                }
                            }

                        } while (string.IsNullOrEmpty(nameExcelFile));

                        Console.WriteLine("Concluido");

                    }
                }
                else
                {
                    Console.WriteLine("Arquivo não encontrado");
                }

                Export();
            }
        }

        private static void WriteSpaceRow(IIfcSpace space, ISheet sheet, ICellStyle areaStyle, ICellStyle volumeStyle)
        {
            var row = sheet.CreateRow(sheet.LastRowNum + 1);

            var name = space.Name;
            row.CreateCell(0).SetCellValue(name);

            var floor = GetFloor(space);
            row.CreateCell(1).SetCellValue(floor?.Name);

            var area = GetArea(space);

            if (area != null)
            {
                var cell = row.CreateCell(2);
                cell.CellStyle = areaStyle;

                // não há garantia de que seja um número se vier de uma propriedade e não de uma quantidade
                if (area.UnderlyingSystemType == typeof(double))
                    cell.SetCellValue((double)(area.Value));
                else
                    cell.SetCellValue(area.ToString());
            }

            var volume = GetVolume(space);

            if (volume != null)
            {
                var cell = row.CreateCell(3);
                cell.CellStyle = volumeStyle;

                // não há garantia de que seja um número se vier de uma propriedade e não de uma quantidade
                if (volume.UnderlyingSystemType == typeof(double))
                    cell.SetCellValue((double)(volume.Value));
                else
                    cell.SetCellValue(volume.ToString());

            }
        }

        private static IIfcBuildingStorey GetFloor(IIfcSpace space)
        {
            return
                // obtém todas as relações objetivadas que modelam a decomposição por este espaço
                space.Decomposes

                // selecione objetos decompostos (podem ser outro espaço ou andar de construção)
                .Select(x => x.RelatingObject)

                // obter apenas andares
                .OfType<IIfcBuildingStorey>()

                //get the first one
                .FirstOrDefault();
        }

        private static IIfcValue GetArea(IIfcProduct product)
        {
            // tente obter o valor das quantidades primeiro
            var area =
                // obtém todas as relações que podem definir conjuntos de propriedades e quantidades
                product.IsDefinedBy

                // Pesquise todos os conjuntos de propriedades e quantidades. 
                // Você também pode querer pesquisar em uma quantidade específica definida por nome
                .SelectMany(x => x.RelatingPropertyDefinition.PropertySetDefinitions)

                // Considere apenas conjuntos de quantidade neste caso.
                .OfType<IIfcElementQuantity>()

                // Obtenha todas as quantidades de todos os conjuntos de quantidade
                .SelectMany(x => x.Quantities)

                // Estamos interessados ​​apenas em áreas
                .OfType<IIfcQuantityArea>()

                // Vamos pegar o primeiro. Pode haver, obviamente, mais de uma propriedade na área 
                // então você pode querer verificar o nome. Mas vamos mantê-lo simples para este exemplo.
                .FirstOrDefault()?
                .AreaValue;

            if (area != null)
                return area;

            // tente obter o valor das propriedades
            return GetProperty(product, "Area");
        }

        private static IIfcValue GetVolume(IIfcProduct product)
        {
            var volume = product.IsDefinedBy
                .SelectMany(x => x.RelatingPropertyDefinition.PropertySetDefinitions)
                .OfType<IIfcElementQuantity>()
                .SelectMany(x => x.Quantities)
                .OfType<IIfcQuantityVolume>()
                .FirstOrDefault()?.VolumeValue;

            if (volume != null)
                return volume;

            return GetProperty(product, "Volume");
        }

        private static IIfcValue GetProperty(IIfcProduct product, string name)
        {
            return
                // obtém todas as relações que podem definir conjuntos de propriedades e quantidades
                product.IsDefinedBy

                // Pesquise todos os conjuntos de propriedades e quantidades. Você também pode querer pesquisar em um conjunto de propriedades específico
                .SelectMany(x => x.RelatingPropertyDefinition.PropertySetDefinitions)

                // Considere apenas conjuntos de propriedades neste caso.
                .OfType<IIfcPropertySet>()

                // Obtenha todas as propriedades de todos os conjuntos de propriedades
                .SelectMany(x => x.HasProperties)

                // permite considerar apenas propriedades de valor único. Existem também propriedades enumeradas, 
                // propriedades da tabela, propriedades de referência, propriedades complexas e outras
                .OfType<IIfcPropertySingleValue>()

                // permite tornar a comparação de nomes mais difusa. Esta pode não ser a melhor prática
                .Where(x => string.Equals(x.Name, name, System.StringComparison.OrdinalIgnoreCase) ||
                x.Name.ToString().ToLower().Contains(name.ToLower()))

                // pegue apenas o primeiro. Na realidade, você deve lidar com isso com mais cuidado.
                .FirstOrDefault()?
                .NominalValue;
        }

        private static string EnterNameExcelFile()
        {
            Console.WriteLine("Digite o nome do arquivo Excel que será criado, ou nada para cancelar o processo!");
            var nameExcelFile = Console.ReadLine();
            if (!string.IsNullOrEmpty(nameExcelFile))
            {
                if (File.Exists(Path.Combine(@"FileTests\", nameExcelFile)))
                {
                    Console.WriteLine("Este Arquivo já existe!!!!!");
                    nameExcelFile = "";
                }
            }

            return nameExcelFile;
        }
    }
}
