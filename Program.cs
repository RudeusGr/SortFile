using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SortFiles;

public class Program
{
    static void Main()
    {
        Console.WriteLine("\t" + @"  _____ _ _      ____             _            ");
	    Console.WriteLine("\t" + @" |  ___(_) | ___/ ___|  ___  _ __| |_ ___ _ __ ");
	    Console.WriteLine("\t" + @" | |_  | | |/ _ \___ \ / _ \| '__| __/ _ \ '__|");
	    Console.WriteLine("\t" + @" |  _| | | |  __/___) | (_) | |  | ||  __/ |   ");
	    Console.WriteLine("\t" + @" |_|   |_|_|\___|____/ \___/|_|   \__\___|_|   ");
	    Console.WriteLine("\t" + @"                                               ");
	    Console.WriteLine("==================================================================");
	    Console.WriteLine("\tEmpezando a organizar los archivos del servidor SIVE");
        ConfigurationJson? configurationJson = ReadConfigJson();
        if (configurationJson is null ||
            configurationJson.Extensions is null ||
            configurationJson.Months is null ||
            configurationJson.Years is null ||
            configurationJson.Urls is null)
        {
            Console.WriteLine("No se encontro algun parametro del archivo de configuracion");
            return;
        }

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string pattern = GenerateRegexPatter(configurationJson.Extensions);
        ReadDir(configurationJson, configurationJson.Urls["urlGenerate"], pattern);
        ReadDir(configurationJson, configurationJson.Urls["urlReception"], pattern);

        stopwatch.Stop();
        Console.WriteLine($"\tTiempo transcurrido: {stopwatch.ElapsedMilliseconds/60000.0} minutos");
        Console.WriteLine("==================================================================");
	    Console.WriteLine("\t\tLa ventana se cerrara en un momento");
	    Console.WriteLine("\t\tgracias por usar la aplicacion");
	    Console.WriteLine("\t\tBy - Yosimar Zahid Aquino Sosa");
	    Console.WriteLine("==================================================================");
    }

    static ConfigurationJson? ReadConfigJson()
    {
        try
        {
            string jsonString = File.ReadAllText("Config.json");
            ConfigurationJson? configurationJson = JsonSerializer.Deserialize<ConfigurationJson>(jsonString);
            return configurationJson;
        }
        catch (IOException ioEx)
        {
            Console.WriteLine($"No se pudo leer un archivo\nError: {ioEx.Message}");
            return null;
        }
        catch (ArgumentNullException ArgNullEx)
        {
            Console.WriteLine($"Se esta pasando algun parametro nulo\nError: {ArgNullEx.Message}");
            return null;
        }
    }

    static string GenerateRegexPatter(IList<string> Extensions)
    {
        StringBuilder regex = new StringBuilder();
        regex.Append(@"[\w|\S|\W]+(");
        foreach (string ex in Extensions)
        {
            regex.Append(ex + "|" + ex.ToUpper());
            if (Extensions.IndexOf(ex) != Extensions.Count - 1)
            {
                regex.Append("|");
            }
        }
        regex.Append(")$");
        return regex.ToString();
    }

    static void ReadDir(ConfigurationJson configurationJson, string url, string pattern)
    {
        try
        {
            string[] pathOfFiles = Directory.GetFiles(url);

            foreach (string pathOfFile in pathOfFiles)
            {
                if (Regex.Match(pathOfFile, pattern).Value.Equals(""))
                {
                    continue;
                }
                ReLocationFile(pathOfFile, configurationJson);
            }
        }
        catch (DirectoryNotFoundException DirNotFouEx)
        {
            Console.WriteLine($"No se encontro el diectorio especificado\nError: {DirNotFouEx.Message}");
        }
    }

    static void ReLocationFile(string pathOfFiles, ConfigurationJson configurationJson)
    {
        try
        {
            FileInfo file = new FileInfo(pathOfFiles);

            string yearFile = file.CreationTime.ToString().Substring(6, 4);
            string monthFile = file.CreationTime.ToString().Substring(3, 2);

            if (configurationJson.Years.Contains(yearFile.Substring(2, 2)))
            {
                string destinationDir = file.DirectoryName + "\\" + yearFile + "\\" + configurationJson.Months[monthFile] + "\\" + file.Name.Replace(file.Extension, "");

                Directory.CreateDirectory(destinationDir);
                File.Move(file.FullName, destinationDir + "\\" + file.Name);
            }
        }
        catch (IOException ioEx)
        {
            Console.WriteLine($"Error el en movimiento del archivo.\nError: {ioEx.Message}");
        }
    }

}
