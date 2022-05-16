// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using TwilioFlow2Mermaid.App;

// Create some options:
var filesOption = new Option<string[]>(
        new[] { "--input", "-i", "--files" },
        description: "file(s) name option, like --input ./flow1.json flow2.json");
filesOption.AllowMultipleArgumentsPerToken = true;
filesOption.IsRequired = true;

var outputOption = new Option<string>(
        new[] { "--output", "-o" },
        description: "output dir for file name, file name only works when single file provided");

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

//optional
var singlefileStartOption = new Option<bool>(
        new[] { "--IndividualFilePerStartNode", "-ps" },
        getDefaultValue: () => true,
        "by default, if starts is given, it will generate files for each start node. ");

// Add the options to a root command:
var rootCommand = new RootCommand
{
    filesOption,
    spreadNodesOption,
    startNodesOption,
    endNodesOption,
    noSpreadsOption,
    useIdOption,
    outputOption,
    singlefileStartOption
};

rootCommand.Description = "Twilio Studio Flow to Mermaid markdown";

rootCommand.SetHandler((string[] files, string[] spreads, string[] starts, string[] ends, bool noSpreads, bool useId, string output, bool singlefile) => {
    const string dirData = "/data/";

    if (string.IsNullOrWhiteSpace(output))
        output = dirData;

    Console.WriteLine($"Found {files.Length} files");

    for (int i = 0; i < files.Length; i++)
    {
        if (Path.IsPathRooted(files[i]))
            continue;
        files[i] = Path.Combine(dirData, files[i].Replace('\\', '/'));
    }

    var settings = new FlowConvertSettings {
        NodesAsEnding = spreads,
        StartNodes = starts,
        EndNodes = ends,
        NoSpreads = noSpreads,
        UseId = useId,
        IndividualFilePerStartNode= singlefile,
    };

    foreach (var file in files)
    {
        var str = Processor.ConvertToMermaid(file, settings);

        if (str == null)
        {
            continue;
        }

        var filename = Path.GetFileNameWithoutExtension(file);
        try
        {
            var outname = Path.GetFileName(output);
            var outnameNoExt = Path.GetFileNameWithoutExtension(output);

            if (string.IsNullOrEmpty(outname))
            {
                foreach (var kvp in str)
                {
                    var file2save = Path.Combine(output, filename + "." + kvp.Key + ".md");
                    if (Flow.TriggerNodeName == kvp.Key)
                        file2save = Path.Combine(output, filename + ".md");

                    File.WriteAllText(file2save, kvp.Value);
                }
            }
            else
            {
                foreach (var kvp in str)
                {
                    var file2save = Path.Combine(outnameNoExt, filename + "." + kvp.Key + ".md");
                    if (Flow.TriggerNodeName == kvp.Key)
                        file2save = Path.Combine(output, filename + ".md");
                    File.WriteAllText(file2save, kvp.Value);
                }
            }
            Console.WriteLine($"File {filename} is saved.");
        }
        catch (global::System.Exception ex)
        {
            Console.WriteLine($"Error occurred while save content");
            Console.WriteLine(ex.ToString());
        }
    }
}, filesOption, spreadNodesOption, startNodesOption, endNodesOption, noSpreadsOption, useIdOption, outputOption, singlefileStartOption);

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