using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

class DBSCAN
{
	const int cntNeighbours = 3;
	static void Main()
	{
		double[][] data = File.ReadAllLines("input.txt").Select(x => x.Split(new char[] { ';' }).Select(y => double.Parse(y)).ToArray()).ToArray();
		double maxClusterDist = double.Parse(Console.ReadLine()); //Максимальное расстояние между соседними вершинами в графе
		int[] clustering = Clustering(maxClusterDist, data);
		PrintPicture(clustering.Max(), clustering, data);
	}
	static int[] Clustering(double maxClusterDist, double[][] rawData) //rawData - сырые данные (не нормализованные)
	{
		double[][] data = rawData;
		List<int>[] graph = InitGraph(maxClusterDist, data); //graph[i] - список соседей вершины i
		bool[] used = new bool[data.Length];
		int[] clustering = new int[data.Length].Select(x => -1).ToArray();
		int numClusters = 0;
		for (int i = 0; i < data.Length; i++)
			if (!used[i] && graph[i].Count >= cntNeighbours)
			{
				UpdateClustering(i, graph, used, numClusters, clustering);
				numClusters++;
			}
		for (int i = 0; i < data.Length; i++)
			if (clustering[i] == -1)
			{
				double minDist = double.MaxValue;
				int idx = 0;
				for (int j = 0; j < data.Length; j++)
					if (clustering[j] != -1)
					{
						double dist = Distance(data[i], data[j]);
						if (dist < minDist)
						{
							minDist = dist;
							idx = j;
						}
					}
				clustering[i] = clustering[idx];
			}
		return clustering;
	}
	static List<int>[] InitGraph(double maxClusterDist, double[][] data) //Initialization Graph - инициализация графа
	{
		List<int>[] graph = new List<int>[data.Length].Select(x => new List<int>()).ToArray();
		for (int i = 0; i < data.Length; i++)
			for (int j = i + 1; j < data.Length; j++)
				if (Distance(data[i], data[j]) <= maxClusterDist)
				{
					graph[i].Add(j);
					graph[j].Add(i);
				}
		return graph;
	}
	static double Distance(double[] tuple, double[] mean) //Расстояние (метрическое) между элементами данных. В данном случае Евклидово
	{
		double sum = 0;
		for (int i = 0; i < tuple.Length; i++)
			sum += Math.Pow(tuple[i] - mean[i], 2);
		return Math.Sqrt(sum);
	}
	static void UpdateClustering(int v, List<int>[] graph, bool[] used, int numClusters, int[] clustering)
	{
		if (used[v])
			return;
		used[v] = true;
		clustering[v] = numClusters;
		foreach (int u in graph[v])
			if (graph[u].Count >= cntNeighbours)
				UpdateClustering(u, graph, used, numClusters, clustering);
			else
			{
				used[u] = true;
				clustering[u] = numClusters;
			}
	}
	static void PrintPicture(int numClusters, int[] clustering, double[][] data)
	{
		Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Purple, Color.OrangeRed, Color.Brown, Color.HotPink, Color.Cyan, Color.Black, Color.Yellow, Color.LightGray };
		int[] pictureSize = new int[data[0].Length].Select(x => int.MinValue).ToArray();
		for (int i = 0; i < data.Length; i++) //Определение размера картинки
			for (int j = 0; j < data[0].Length; j++)
				pictureSize[j] = Math.Max(pictureSize[j], Convert.ToInt32(data[i][j]));
		Bitmap picture = new Bitmap(pictureSize[0] + 1, pictureSize[1] + 1);
		Graphics.FromImage(picture).Clear(Color.White);
		for (int i = 0; i < data.Length; i++)
			picture.SetPixel(Convert.ToInt32(data[i][0]), Convert.ToInt32(data[i][1]), colors[Math.Min(10, clustering[i])]);
		picture = ColorScaling(picture, 3);
		picture = Scaling(picture, 3);
		picture.Save("picture.bmp");
	}
	static Bitmap Scaling(Bitmap picture, int scale)
	{
		Bitmap newPicture = new Bitmap(picture.Width * scale, picture.Height * scale);
		for (int i = 0; i < picture.Width; i++)
			for (int j = 0; j < picture.Height; j++)
				for (int k = i * scale; k < (i + 1) * scale; k++)
					for (int l = j * scale; l < (j + 1) * scale; l++)
						newPicture.SetPixel(k, l, picture.GetPixel(i, j));
		return newPicture;
	}
	static Bitmap ColorScaling(Bitmap picture, int scale) //Масштабирование только цветных пикселей. Размер картинки остается прежним. !!!Работает правильно только с нечетным scale
	{
		Bitmap newPicture = new Bitmap(picture);
		for (int i = 0; i < picture.Width; i++)
			for (int j = 0; j < picture.Height; j++)
				if (picture.GetPixel(i, j).Name != "ffffffff")
					for (int k = Math.Max(0, i - scale / 2); k < Math.Min(picture.Width - 1, i + scale / 2); k++)
						for (int l = Math.Max(0, j - scale / 2); l < Math.Min(picture.Height - 1, j + scale / 2); l++)
							newPicture.SetPixel(k, l, picture.GetPixel(i, j));
		return newPicture;
	}
}