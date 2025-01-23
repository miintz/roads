namespace Assets.Algorithm.Kruskal
{
    public class KruskalEdge
    {
        public int StartNode { get; set; }
        public int EndNode { get; set; }
        public float Weight { get; set; }

        public KruskalEdge(int startNode, int endNode, float weight)
        {
            StartNode = startNode;
            EndNode = endNode;
            Weight = weight;
        }
    }
}
