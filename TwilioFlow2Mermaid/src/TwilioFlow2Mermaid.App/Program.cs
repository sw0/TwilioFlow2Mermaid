// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using TwilioFlow2Mermaid.App;

// Create some options:
var filesOption = new Option<string[]>(
        "--files",
        description: "file(s) name option, like --files ./flow1.json flow2.json");
filesOption.AllowMultipleArgumentsPerToken = true;
filesOption.IsRequired = true;

var noSpreadsOption = new Option<bool>(
        "--nospreads", //new string[] { "-ns", "--nospreads" },
        getDefaultValue: () => false,
        "if true, --spreads would get ignored");

var useIdOption = new Option<bool>(
        "--useid", 
        getDefaultValue: () => false,
        "if true, use id for node instead of widget name in markdown file");

//optional
var spreadNodesOption = new Option<string[]>(
        "--spreads",
        "case-sensitive flow widget names to be treated individually by appending a <number> suffix");
spreadNodesOption.AllowMultipleArgumentsPerToken = true;

//optional
var startNodesOption = new Option<string[]>(
        "--starts",
        "specify the nodes as start nodes");
startNodesOption.AllowMultipleArgumentsPerToken = true;

//optional
var endNodesOption = new Option<string[]>(
        "--ends",
        "specify the nodes to be treated as ending nodes");
spreadNodesOption.AllowMultipleArgumentsPerToken = true;

// Add the options to a root command:
var rootCommand = new RootCommand
{
    filesOption,
    spreadNodesOption,
    startNodesOption,
    endNodesOption,
    noSpreadsOption,
    useIdOption
};

rootCommand.Description = "Twilio Studio Flow to Mermaid markdown";

rootCommand.SetHandler((string[] files, string[] spreads, string[] starts, string[] ends, bool noSpreads, bool useId) =>
{
    const string dirData = "/data/";

    Console.WriteLine($"Found {files.Length} files");

    for (int i = 0; i < files.Length; i++)
    {
        files[i] = Path.Combine(dirData, files[i].Replace('\\', '/'));
    }

    var settings = new FlowConvertSettings
    {
        NodesAsEnding = spreads,
        StartNodes = starts,
        EndNodes = ends, 
        NoSpreads = noSpreads,
        UseId = useId
    };

    foreach (var file in files)
    {
        var str = Processor.ConvertToMermaid(file, settings);

        if (str == null)
        {
            continue;
        }

        var filename = Path.GetFileNameWithoutExtension(file) + ".md";
        var file2save = Path.Combine(dirData, filename);
        try
        {
            File.WriteAllText(file2save, str);
            Console.WriteLine($"File {filename} is saved.");
        }
        catch (global::System.Exception ex)
        {
            Console.WriteLine($"Error occurred while save content to '{file2save}'");
            Console.WriteLine(ex.ToString());
        }
    }
}, filesOption, spreadNodesOption, startNodesOption, endNodesOption, noSpreadsOption, useIdOption);

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