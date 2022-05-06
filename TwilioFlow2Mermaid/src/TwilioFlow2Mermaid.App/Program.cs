// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using TwilioFlow2Mermaid.App;

// Create some options:
var filesOption = new Option<string[]>(
        "--files",
        //getDefaultValue: () => new string[0], 
        description: "Required files name option, like --files /data/flow1.json /data/flow2.json");
filesOption.AllowMultipleArgumentsPerToken = true;
filesOption.IsRequired = true;

var spreadEndLeafsOption = new Option<bool>(
        "--spreadleafs",
        "An flag to indicates wheather processing leaf nodes spreadly, works with option --endleafs");
var endingLeafs = new Option<string[]>(
        "--endleafs",
        "required when --spreadleafs is true, case-sensitive flow widget names");
endingLeafs.AllowMultipleArgumentsPerToken = true;

// Add the options to a root command:
var rootCommand = new RootCommand
{
    filesOption,
    spreadEndLeafsOption,
    endingLeafs
};

rootCommand.Description = "Twilio Studio Flow to Mermaid markdown";

rootCommand.SetHandler((string[] files, bool spread, string[] leafs) =>
{
    const string dirData = "/data/";

    Console.WriteLine($"files: \r\n\t{string.Join("\r\n\t", files)}");
    Console.WriteLine($"spreadleafs is: {spread}");
    Console.WriteLine($"endleafs is: {string.Join(' ', leafs)}");

    for (int i = 0; i < files.Length; i++)
    {
        files[i] = Path.Combine(dirData, files[i].Replace('\\','/'));
    }

    foreach (var file in files)
    {
        var str = Processor.ConvertToMermaid(file, spread, leafs);

        //Console.WriteLine(str);

        var filename = Path.GetFileNameWithoutExtension(file);
        var file2save = Path.Combine(dirData, filename + ".md");//@$"/data/{filename}.md";
        try
        {
            File.WriteAllText(file2save, str);
            Console.WriteLine($"File {file2save} is saved.");
        }
        catch (global::System.Exception ex)
        {
            Console.WriteLine($"Error occurred while save content to '{file2save}'");
            Console.WriteLine(ex.ToString());
        }
    }
}, filesOption, spreadEndLeafsOption, endingLeafs);

// Parse the incoming args and invoke the handler
return rootCommand.Invoke(args);


/*
 * https://docs.microsoft.com/en-us/dotnet/standard/commandline/define-commands
 * https://www.npmjs.com/package/@mermaid-js/mermaid-cli
 * 
```
docker run -it -v c:\work\mermaid\:/data minlag/mermaid-cli -i /data/md-diagram.md
```

*/