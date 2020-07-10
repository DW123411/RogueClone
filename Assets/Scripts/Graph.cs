using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using UnityEngine;


public class Graph
{
    public class Edge : IComparable<Edge>
    {
        public int src, dest, weight;

        public int CompareTo(Edge compareEdge)
        {
            return this.weight - compareEdge.weight;
        }
    }

    public class subset
    {
        public int parent, rank;
    };

    int V, E;
    public Edge[] edge;

    public Graph(int v, int e)
    {
        V = v;
        E = e;
        edge = new Edge[E];
        for (int i = 0; i < e; i++)
            edge[i] = new Edge();
    }

    int find(subset[] subsets, int i)
    {
        if (subsets[i].parent != i)
            subsets[i].parent = find(subsets, subsets[i].parent);
        return subsets[i].parent;
    }

    void Union(subset[] subsets, int x, int y)
    {
        int xroot = find(subsets, x);
        int yroot = find(subsets, y);

        if (subsets[xroot].rank < subsets[yroot].rank)
            subsets[xroot].parent = yroot;
        else if (subsets[xroot].rank > subsets[yroot].rank)
            subsets[yroot].parent = xroot;
        else
        {
            subsets[yroot].parent = xroot;
            subsets[xroot].rank++;
        }
    }

    public int[,] KruskalMST()
    {
        Edge[] result = new Edge[V];
        int e = 0;
        int i = 0;
        for (i = 0; i < V; i++)
            result[i] = new Edge();

        Array.Sort(edge);

        subset[] subsets = new subset[V];
        for (i = 0; i < V; i++)
            subsets[i] = new subset();

        for (int v = 0; v < V; v++)
        {
            subsets[v].parent = v;
            subsets[v].rank = 0;
        }

        i = 0;

        int[,] matrix = new int[9, 9];
        for (int j = 0; j < 9; j++)
        {
            for (int k = 0; k < 9; k++)
            {
                matrix[j, k] = 0;
            }
        }

        while (e < V - 1)
        {
            Edge nextEdge = new Edge();
            nextEdge = edge[i++];

            int x = find(subsets, nextEdge.src);
            int y = find(subsets, nextEdge.dest);

            if (x != y)
            {
                result[e++] = nextEdge;
                Union(subsets, x, y);
                matrix[nextEdge.src, nextEdge.dest] = 1;
                matrix[nextEdge.dest, nextEdge.src] = 1;
            }
        }
        return matrix;
    }
}
