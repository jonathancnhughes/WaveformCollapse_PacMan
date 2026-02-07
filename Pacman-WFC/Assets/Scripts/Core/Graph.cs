using System.Collections.Generic;

namespace JFlex.Core
{
    public class Graph<T> where T : class
    {
        Dictionary<T, List<T>> edges = new();

        public void AddEdge(T u, T v)
        {
            if (!edges.ContainsKey(u))
            {
                edges.Add(u, new List<T>());
            }

            if (!edges[u].Contains(v))
            {
                edges[u].Add(v);
            }
        }

        public List<T> BreadthFirstSearch(T start)
        {
            List<T> visited = new();
            Queue<T> queue = new();

            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                T node = queue.Dequeue();

                if (!visited.Contains(node))
                {
                    visited.Add(node);
                    foreach (var neighbour in edges[node])
                    {
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return visited;
        }
    }
}