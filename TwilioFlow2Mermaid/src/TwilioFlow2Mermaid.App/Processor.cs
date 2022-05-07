using System.Linq;
using System.Text;
using System.Text.Json;

namespace TwilioFlow2Mermaid.App
{
    public class FlowConvertSettings
    {
        public bool NoSpreads { get; set; } = false;

        public string[]? NodesAsEnding { get; set; }

        /// <summary>
        /// Optional, it would be useful that when the flow is too complicated
        /// </summary>
        public string[] StartNodes { get; set; } = new string[0];

        /// <summary>
        /// Use Id instead of text for each node in markdown, which will save file size
        /// </summary>
        public bool UseId { get; set; } = false;

        public string[] EndNodes { get; set; } = new string[0];

        public void Normalize()
        {
            StartNodes ??= Enumerable.Empty<string>().ToArray();
            NodesAsEnding ??= Enumerable.Empty<string>().ToArray();
        }
    }

    public class NodeInfo
    {
        public bool Processed { get; set; }

        public string? Id { get; set; }

        public int NextCount { get; set; }

        public int ParentCount { get; set; }
    }

    public class Processor
    {
        /// <summary>
        /// indicates whether the node with type has occurred or not. For example: C{C}, then in next time, we only render C, without {C}
        /// </summary>
        private static HashSet<string> NoteTypeOccurrence = new HashSet<string>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="spreadEndingLeafs"></param>
        /// <param name="endingLeafs">some widget is really common, make it to be single ones on graph, like ending widgets or fail widgets</param>
        /// <returns></returns>
        public static string? ConvertToMermaid(string file, FlowConvertSettings settings)
        {
            settings.Normalize();

            Flow? flow = null;

            try
            {
                var json = File.ReadAllText(file);
                flow = JsonSerializer.Deserialize<Flow>(json);

                if (flow == null)
                    return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                return null;
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("```mermaid");
                sb.AppendLine("graph LR");

                //flow.states.Select(x => x.name).Distinct().ToList().ForEach(n => Console.WriteLine(n));
                var dic = flow.states.Select(x => x.name).Distinct().ToList().ToDictionary<string, string, NodeInfo>(x => x, x => new NodeInfo());


                var allTransitions = flow.states.Where(n1 => n1.transitions != null && n1.transitions.Any())
                    .SelectMany(n1 => n1.transitions.Where(t => !string.IsNullOrWhiteSpace(t.next)));

                var index = 0;
                var numberWidth = flow.states.Count().ToString().Length;
                foreach (var item in flow.states)
                {
                    index++;
                    var node = dic[item.name];
                    node.ParentCount = allTransitions.Count(x => x.next == item.name);
                    node.NextCount = item.GetTransitionCount();
                    node.Id = "N" + index.ToString().PadLeft(numberWidth, '0');
                }

                Func<string, NodeInfo> info = (string name) => dic[name];

                // flow.states.Select(x => x.name).Distinct().ToDictionary<string, bool>(x => false);

                index = 0;
                var nodes = settings.StartNodes.Any() ?
                    flow.states!.Where(n1 => settings.StartNodes.Contains(n1.name)).ToList() : flow.states;

                foreach (var item in nodes)
                {
                    ProcessInternal(flow, dic, item, settings, sb, ref index);
                }

                sb.AppendLine("```");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return string.Empty;
            }
        }

        private static void ProcessInternal(Flow flow, Dictionary<string, NodeInfo> dic, FlowNode item, FlowConvertSettings settings, StringBuilder sb, ref int index)
        {
            var node = dic[item.name];

            if (node.Processed) return;
            node.Processed = true;

            if (node.NextCount == 0)
                return;

            if (settings.EndNodes.Contains(item.name))
                return;

            index++;

            if (item.name == "Trigger")
                sb.AppendFormat("{0}[{1}] ", settings.UseId ? node.Id : item.name, item.name);
            else
                sb.AppendFormat("{0} ", settings.UseId ? node.Id : item.name);

            if (node.NextCount > 0)
            {
                var transitions = item.transitions.Where(t => !string.IsNullOrWhiteSpace(t.next));
                if (transitions.Count() == 0) return;

                var split = transitions.Count() > 1;

                var isMore = false;

                foreach (var transition in transitions)
                {
                    var evt = transition.@event;
                    var tmpNode = dic[transition.next];

                    if (isMore)
                    {
                        sb.AppendFormat("{0} ", settings.UseId ? node.Id : item.name);
                    }

                    sb.Append(" --");
                    sb.Append(evt);
                    sb.Append("--> ");

                    if (settings.NoSpreads == false && settings.NodesAsEnding!.Contains(transition.next))
                    {
                        if (tmpNode.Processed || NoteTypeOccurrence.Contains(transition.next))
                            sb.AppendFormat("{0}:{1}", transition.next, index);
                        else
                            sb.AppendFormat("{0}:{1}({0})", settings.UseId ? tmpNode.Id : transition.next, index);
                    }
                    else if (NoteTypeOccurrence.Contains(transition.next))
                    {
                        sb.Append(settings.UseId ? tmpNode.Id : transition.next);
                    }
                    else
                    {
                        sb.Append(settings.UseId ? tmpNode.Id : transition.next);

                        if (tmpNode.NextCount > 1)
                        {
                            sb.AppendFormat("{{{0}}}", transition.next);
                        }
                        else
                        {
                            sb.AppendFormat("({0})", transition.next);
                        }
                    }

                    NoteTypeOccurrence.Add(transition.next);

                    sb.AppendLine();

                    isMore = true;
                }

                foreach (var transition in transitions)
                {
                    var child = flow.states.First(c => c.name == transition.next);
                    ProcessInternal(flow, dic, child, settings, sb, ref index);
                }
            }

            //sb.AppendLine();
        }

        private static int GetTransitionCount(Flow flow, string node)
        {
            var next = flow.states.First(n => n.name == node);
            if (next.transitions != null)
            {
                var count = next.transitions.Where(transition => !string.IsNullOrWhiteSpace(transition.next)).Count();
                return count;
            }
            return 0;
        }
    }



    class Flow
    {
        public string description { get; set; } = "";

        public List<FlowNode> states { get; set; } = new List<FlowNode>();

        public string initial_state { get; set; } = "";
    }

    class FlowNode
    {
        public string name { get; set; } = "";

        public string type { get; set; } = "";

        public List<Transition> transitions { get; set; } = new List<Transition>();

        public int GetTransitionCount()
        {
            if (this.transitions == null) return 0;
            return this.transitions.Where(t => !string.IsNullOrWhiteSpace(t.next)).Count();
        }
    }

    public class Transition
    {
        public string @event { get; set; } = "";
        public string next { get; set; } = "";
    }
}
