#nullable enable

using System.Collections.Generic;
using System.Linq;

public class Trie<T>
{
    private readonly Node root;
    private readonly Processor accumulator;

    public Trie() : this(x => x.ToList()) { }

    public Trie(Processor accumulator)
    {
        root = Node.CreateRoot();
        this.accumulator = accumulator;
    }

    /// <summary>
    /// Add a new complete sequence.
    /// </summary>
    /// <param name="newSequence">Complete sequence to add.</param>
    public void Add(IEnumerable<T> newSequence)
    {
        if (!newSequence.Any())
        {
            throw new System.ArgumentException("Path must have length > 0");
        }

        Node? last = null;
        var curr = root;

        foreach (var value in newSequence)
        {
            if (!curr.HasChild(value))
            {
                curr.AddChild(Node.Create(value));
            }

            last = curr;
            curr = curr.GetChild(value);
        }

        last?.AddChild(Node.CreateEnd(curr.GetValue()));
    }

    /// <summary>
    /// Gets all complete sequences that begin with the given prefix.
    /// </summary>
    /// <param name="prefix">Prefix to find completions of.</param>
    /// <returns>List of sequences which begin with 'prefix'.</returns>
    public List<List<T>> GetCompletions(IEnumerable<T> prefix)
    {
        if (!prefix.Any())
        {
            return root.GetPaths();
        }

        var curr = root;

        var path = new List<T>();

        foreach (var value in prefix)
        {
            if (!curr.HasChild(value))
            {
                return new List<List<T>>();
            }

            path.Add(value);
            curr = curr.GetChild(value);
        }

        if (curr.IsEnd())
        {
            return new List<List<T>>() { accumulator(path) };
        }

        return curr.GetPaths().ConvertAll(p => accumulator(path.Concat(p)));
    }

    /// <summary>
    /// Gives if the given sequence is complete
    /// </summary>
    /// <param name="prefix">Sequence to check.</param>
    /// <returns>If the sequence is complete</returns>
    public bool IsComplete(IEnumerable<T> prefix)
    {
        // Empty List is never complete
        if (!prefix.Any())
        {
            return false;
        }

        var curr = root;

        var path = new List<T>();

        foreach (var value in prefix)
        {
            if (!curr.HasChild(value))
            {
                return false;
            }

            path.Add(value);
            curr = curr.GetChild(value);
        }

        return curr.IsEnd();
    }

    // Inner Types

    public delegate List<T> Processor(IEnumerable<T> ret);

    public sealed class Node
    {
        private readonly T value;
        private readonly Dictionary<T, Node> children = new Dictionary<T, Node>();
        private bool isRoot;
        private bool isEnd;

        public static Node CreateRoot()
            => new Node(default!)
            {
                isRoot = true
            };

        public static Node Create(T value)
        {
            return new Node(value);
        }

        public static Node CreateEnd(T value)
            => new Node(value)
            {
                isEnd = true
            };

        private Node(T value)
        {
            this.value = value;
        }

        public T GetValue()
        {
            return value;
        }

        public void AddChild(Node child)
        {
            children[child.value] = child;
        }

        public bool HasChild(T value)
        {
            return children.ContainsKey(value);
        }

        public Node GetChild(T value)
        {
            return children[value];
        }

        public bool IsEnd()
        {
            return isEnd;
        }

        public List<List<T>> GetPaths()
        {
            return GetPaths(false);
        }

        public List<List<T>> GetPaths(bool includeThis)
        {
            var paths = new List<List<T>>();

            if (isRoot)
            {
                foreach (var child in children.Values)
                {
                    paths.AddRange(child.GetPaths());
                }
            }
            else
            {
                if (includeThis && isEnd)
                {
                    paths.Add(new List<T>() { value });
                }

                foreach (var child in children.Values)
                {
                    foreach (var path in child.GetPaths(true))
                    {
                        if (includeThis)
                        {
                            path.Insert(0, value);
                        }
                        paths.Add(path);
                    }
                }
            }

            return paths;
        }
    }
}