using System.Linq;
using System.Text;

namespace TwilioFlow2Mermaid.App
{
    public class Processor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="spreadEndingLeafs"></param>
        /// <param name="endingLeafs">some widget is really common, make it to be single ones on graph, like ending widgets or fail widgets</param>
        /// <returns></returns>
        public static string ConvertToMermaid(string file, bool spreadEndingLeafs, params string[] endingLeafs)
        {
            endingLeafs ??= Enumerable.Empty<string>().ToArray();

            try
            {
                var json = File.ReadAllText(file);

                var flow = System.Text.Json.JsonSerializer.Deserialize<Flow>(json);


                var cfg = new
                {
                    spreadTechDifficulty = true
                };

                //flow.Dump();

                var sb = new StringBuilder();
                sb.AppendLine("```mermaid");
                sb.AppendLine("graph LR");

                //flow.states.Select(x => x.name).Distinct().ToList().ForEach(n => Console.WriteLine(n));
                var dic = flow.states.Select(x => x.name).Distinct().ToList().ToDictionary<string, string, bool>(x => x, x => false);

                // flow.states.Select(x => x.name).Distinct().ToDictionary<string, bool>(x => false);

                var index = 0;
                foreach (var item in flow.states)
                {
                    index++;
                    if (dic[item.name] == true)
                        continue;
                    dic[item.name] = true;

                    var splitCountCurrent = GetTransitionCount(flow, item.name);
                    //Console.WriteLine($"{item.name}, split count: {splitCountCurrent}");
                    if (splitCountCurrent == 0)
                        continue;

                    if (item.name == "Trigger")
                        sb.AppendFormat("{0}({0}) ", item.name);
                    else
                        sb.AppendFormat("{0} ", item.name);

                    if (item.transitions != null)
                    {
                        var transitions = item.transitions.Where(t => !string.IsNullOrWhiteSpace(t.next));
                        if (transitions.Count() == 0) continue;

                        var split = transitions.Count() > 1;

                        var isMore = false;

                        foreach (var transition in transitions)
                        {
                            var evt = transition.@event;
                            var splitCount = GetTransitionCount(flow, transition.next);

                            if (isMore)
                            {
                                sb.AppendFormat("{0} ", item.name);
                            }

                            sb.Append(" --");
                            sb.Append(evt);
                            sb.Append("--> ");

                            if (spreadEndingLeafs && endingLeafs.Contains(transition.next))
                            {
                                sb.AppendFormat("{0}{1}({0}{1})", transition.next, index);
                            }
                            else
                            {
                                sb.Append(transition.next);
                                if (splitCount > 1)
                                {
                                    sb.AppendFormat("{{{0}}}", transition.next);
                                }
                                else
                                {
                                    sb.AppendFormat("({0})", transition.next);
                                }
                            }

                            sb.AppendLine();

                            isMore = true;
                        }
                    }

                    sb.AppendLine();
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
    }

    public class Transition
    {
        public string @event { get; set; } = "";
        public string next { get; set; } = "";
    }
}
