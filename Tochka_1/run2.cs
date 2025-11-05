using System;
using System.Collections.Generic;
using System.Linq;

class Run2
{
    static bool IsGateway(string node)
    {
        return node.Any(char.IsLetter) && node == node.ToUpperInvariant();
    }

    static bool IsLowercase(string node)
    {
        return node.Any(char.IsLetter) && node == node.ToLowerInvariant();
    }

    static Dictionary<string, int> Bfs(Dictionary<string, 
        HashSet<string>> 
        graph, string start, 
        Func<string, bool> allow = null)
    {
        var dist = new Dictionary<string, int> { [start] = 0 };
        var queue = new Queue<string>();
        
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!graph.TryGetValue(current, out var neighbors)) continue;

            foreach (var v in neighbors)
            {
                if (allow != null && !allow(v)) continue;
                
                if (!dist.ContainsKey(v))
                {
                    dist[v] = dist[current] + 1;
                    queue.Enqueue(v);
                }
            }
        }

        return dist;
    }

    static (List<(string gate, string node)>, HashSet<string>) FrontierAndComponent(
        Dictionary<string, HashSet<string>> graph, string cur)
    {
        HashSet<string> components;
        if (graph.ContainsKey(cur))
        {
            var d = Bfs(graph, cur, n => IsLowercase(n));
            components = new HashSet<string>(d.Keys);
        }
        else
        {
            components = new HashSet<string>();
        }

        var frontier = new HashSet<(string, string)>();
        foreach (var current in components)
        {
            if (!graph.TryGetValue(current, out var neighbors)) continue;
            foreach (var v in neighbors)
            {
                if (IsGateway(v))
                {
                    frontier.Add((v, current));
                }
            }
        }

        var list = frontier.ToList();
        list = list.OrderBy(t => t.Item1).ThenBy(t => t.Item2).ToList();

        return (list, components);
    }

    static List<string> Solve(List<(string a, string b)> edges)
    {
        var graph = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var gates = new HashSet<string>(StringComparer.Ordinal);

        void AddEdge(string a, string b)
        {
            if (!graph.ContainsKey(a)) graph[a] = new HashSet<string>();
            if (!graph.ContainsKey(b)) graph[b] = new HashSet<string>();
            
            graph[a].Add(b);
            graph[b].Add(a);
            
            if (IsGateway(a)) gates.Add(a);
            if (IsGateway(b)) gates.Add(b);
        }

        foreach (var edge in edges)
            AddEdge(edge.a, edge.b);

        var virus = "a";
        var result = new List<string>();

        while (true)
        {
            var (frontier, components) = FrontierAndComponent(graph, virus);
            
            if (frontier.Count == 0) break;

            var candidates = frontier.Where(t => t.node == virus).ToList();
            
            if (!candidates.Any()) candidates = frontier;

            var chosen = candidates.OrderBy(t => t.gate).ThenBy(t => t.node).First();
            var G = chosen.gate;
            var u = chosen.node;

            if (graph.TryGetValue(u, out var setU)) setU.Remove(G);
            if (graph.TryGetValue(G, out var setG)) setG.Remove(u);

            result.Add($"{G}-{u}");

            var (frontAfter, _) = FrontierAndComponent(graph, virus);
            if (frontAfter.Count == 0) break;

            var distFromV = Bfs(graph, virus);
            var maybeGates = gates.Where(g => distFromV.ContainsKey(g)).ToList();
            if (!maybeGates.Any()) break;

            var minDist = maybeGates.Min(g => distFromV[g]);
            var candidateGates = maybeGates.Where(g => distFromV[g] == minDist).ToList();
            var targetGate = candidateGates.OrderBy(x => x).First();

            var distToGate = Bfs(graph, targetGate);

            int distVirus = distToGate.ContainsKey(virus) ? distToGate[virus] : int.MaxValue;
            var nextSteps = graph.ContainsKey(virus)
                ? graph[virus].Where(n => IsLowercase(n) && distToGate.GetValueOrDefault(n, int.MaxValue) == distVirus - 1).ToList()
                : new List<string>();

            if (nextSteps.Any())
            {
                virus = nextSteps.OrderBy(x => x).First();
            }
            else
            {
                break;
            }
        }

        return result;
    }

    static void Main()
    {
        var edges = new List<(string, string)>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim();
            if (line == "") continue;
            var parts = line.Split('-', 2);
            if (parts.Length == 2)
            {
                edges.Add((parts[0], parts[1]));
            }
        }

        var cuts = Solve(edges);
        foreach (var cut in cuts)
            Console.WriteLine(cut);
    }
}
