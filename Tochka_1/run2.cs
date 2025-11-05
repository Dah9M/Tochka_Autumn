using System;
using System.Collections.Generic;
using System.Linq;

class Run2
{
    static List<string> Solve(List<(string, string)> edges)
    {
        var result = new List<string>();
        
        var graph = new Dictionary<string, HashSet<string>>();
        foreach (var (u, v) in edges)
        {
            if (!graph.ContainsKey(u)) graph[u] = new HashSet<string>();
            if (!graph.ContainsKey(v)) graph[v] = new HashSet<string>();
            graph[u].Add(v);
            graph[v].Add(u);
        }
        
        var gateways = graph.Keys.Where(node => char.IsUpper(node[0])).ToHashSet();
        
        string virusPosition = "a";
        
        while (true)
        {
            var distances = BFS(graph, virusPosition, gateways);
            
            if (distances.Count == 0) break;
            
            var targetGateway = distances
                .OrderBy(kv => kv.Value)
                .ThenBy(kv => kv.Key, StringComparer.Ordinal)
                .First().Key;
            
            var targetGatewayEdges = new List<string>();
            if (graph.ContainsKey(targetGateway))
            {
                foreach (var neighbor in graph[targetGateway].OrderBy(n => n, StringComparer.Ordinal))
                {
                    if (!char.IsUpper(neighbor[0]))
                    {
                        targetGatewayEdges.Add($"{targetGateway}-{neighbor}");
                    }
                }
            }
            
            if (targetGatewayEdges.Count == 0) break;
            
            var edgeToRemove = targetGatewayEdges.OrderBy(e => e, StringComparer.Ordinal).First();
            var parts = edgeToRemove.Split('-');
            var gw = parts[0];
            var node = parts[1];
            
            if (graph.ContainsKey(gw) && graph[gw].Contains(node))
            {
                graph[gw].Remove(node);
                graph[node].Remove(gw);
            }
            
            if (!graph.ContainsKey(gw) || graph[gw].Count == 0)
            {
                gateways.Remove(gw);
            }
            
            result.Add(edgeToRemove);
            
            var newDistances = BFS(graph, virusPosition, gateways);
            
            if (newDistances.Count == 0) break;
            
            var newTargetGateway = newDistances
                .OrderBy(kv => kv.Value)
                .ThenBy(kv => kv.Key, StringComparer.Ordinal)
                .First().Key;
            
            var nextNode = GetNextNode(graph, virusPosition, newTargetGateway);
            if (nextNode != null)
            {
                virusPosition = nextNode;
            }
            else
            {
                break;
            }
        }
        
        return result;
    }
    
    static Dictionary<string, int> BFS(Dictionary<string, HashSet<string>> graph, string start, HashSet<string> targets)
    {
        var distances = new Dictionary<string, int>();
        var queue = new Queue<(string node, int dist)>();
        var visited = new HashSet<string>();
        
        queue.Enqueue((start, 0));
        visited.Add(start);
        
        while (queue.Count > 0)
        {
            var (node, dist) = queue.Dequeue();
            
            if (targets.Contains(node))
            {
                distances[node] = dist;
            }
            
            if (graph.ContainsKey(node))
            {
                foreach (var neighbor in graph[node].OrderBy(x => x, StringComparer.Ordinal))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, dist + 1));
                    }
                }
            }
        }
        
        return distances;
    }
    
    static string GetNextNode(Dictionary<string, HashSet<string>> graph, string current, string target)
    {
        var queue = new Queue<(string node, List<string> path)>();
        var visited = new HashSet<string>();
        
        queue.Enqueue((current, new List<string> { current }));
        visited.Add(current);
        
        while (queue.Count > 0)
        {
            var (node, path) = queue.Dequeue();
            
            if (node == target)
            {
                return path.Count > 1 ? path[1] : null;
            }
            
            if (graph.ContainsKey(node))
            {
                foreach (var neighbor in graph[node].OrderBy(x => x, StringComparer.Ordinal))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        var newPath = new List<string>(path) { neighbor };
                        queue.Enqueue((neighbor, newPath));
                    }
                }
            }
        }
        
        return null;
    }
    
    static void Main()
    {
        var edges = new List<(string, string)>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split('-');
                if (parts.Length == 2)
                {
                    edges.Add((parts[0], parts[1]));
                }
            }
        }

        var result = Solve(edges);
        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }
    }
}