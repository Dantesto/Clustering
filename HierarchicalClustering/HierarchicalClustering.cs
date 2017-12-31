using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

class HierarchicalClustering
{
	static void Main()
	{
		double[][] data = File.ReadAllLines("input.txt").Select(x => x.Split(new char[] { ';' }).Select(y => double.Parse(y)).ToArray()).ToArray();
		int numClusters = int.Parse(Console.ReadLine());
		int[] clustering = Clustering(numClusters, data);
		PrintPicture(numClusters, clustering, data);
	}
	static int[] Clustering(int numClusters, double[][] rawData) //rawData - сырые данные (не нормализованные)
	{
		double[][] data = Normalized(rawData);
		List<double[]> cMass = new List<double[]>(); //Center of mass of clusters
		List<int[]> clusters = InitClusters(data, cMass); //Набор кластеров. Каждый кластер - индексы данных из data
		int[] clustering = new int[data.Length];
		while (clusters.Count > numClusters)
			UpdateClustering(data, cMass, clusters);
		for (int i = 0; i < clusters.Count; i++)
			for (int j = 0; j < clusters[i].Length; j++)
				clustering[clusters[i][j]] = i;
		return clustering;
	}
	static double[][] Normalized(double[][] rawData) //Нормализация min-max
	{
		double[][] data = MakeMatrix(rawData.Length, rawData[0].Length);
		double[] min = new double[rawData[0].Length].Select(x => double.MaxValue).ToArray(), max = new double[rawData[0].Length].Select(x => double.MinValue).ToArray();
		for (int i = 0; i < rawData.Length; i++)
			for (int j = 0; j < rawData[0].Length; j++)
			{
				min[j] = Math.Min(min[j], rawData[i][j]);
				max[j] = Math.Max(max[j], rawData[i][j]);
			}
		for (int i = 0; i < rawData.Length; i++)
			for (int j = 0; j < rawData[0].Length; j++)
				data[i][j] = Math.Min(1, (rawData[i][j] - min[j]) / (max[j] - min[j])); //Если min[j] == max[j], тогда data[i][j] = 1
		return data;
	}
	static List<int[]> InitClusters(double[][] data, List<double[]> cMass) //Initialization Clusters - инициализация листьев дендограммы
	{
		List<int[]> clusters = new List<int[]>();
		for (int i = 0; i < data.Length; i++)
		{
			clusters.Add(new int[] { i });
			cMass.Add(new double[data[0].Length]);
			for (int j = 0; j < data[0].Length; j++)
				cMass[i][j] = data[i][j];
		}
		return clusters;
	}
	static double[][] MakeMatrix(int rows, int cols)
	{
		double[][] matrix = new double[rows][];
		for (int i = 0; i < rows; i++)
			matrix[i] = new double[cols];
		return matrix;
	}
	static double Distance(double[] tuple, double[] mean) //Расстояние (метрическое) между элементами данных. В данном случае Евклидово
	{
		double sum = 0;
		for (int i = 0; i < tuple.Length; i++)
			sum += Math.Pow(tuple[i] - mean[i], 2);
		return Math.Sqrt(sum);
	}
	static void UpdateClustering(double[][] data, List<double[]> cMass, List<int[]> clusters)
	{
		double minDist = double.MaxValue;
		int[] cheapestClusters = new int[2];
		for (int i = 0; i < clusters.Count; i++)
			for (int j = i + 1; j < clusters.Count; j++)
			{
				double dist = Distance(cMass[i], cMass[j]); //Центроидное невзвешенное расстояние
				if (dist < minDist)
				{
					minDist = dist;
					cheapestClusters = new int[] { i, j };
				}
			}
		JoinClusters(cheapestClusters[0], cheapestClusters[1], cMass, clusters);
	}
	static void JoinClusters(int idx1, int idx2, List<double[]> cMass, List<int[]> clusters)
	{
		clusters.Add(new int[clusters[idx1].Length + clusters[idx2].Length]);
		cMass.Add(new double[cMass[0].Length]);
		for (int i = 0; i < clusters[idx1].Length; i++)
			clusters[clusters.Count - 1][i] = clusters[idx1][i];
		for (int i = 0; i < clusters[idx2].Length; i++)
			clusters[clusters.Count - 1][i + clusters[idx1].Length] = clusters[idx2][i];
		for (int i = 0; i < cMass[0].Length; i++)
			cMass[cMass.Count - 1][i] = (cMass[idx1][i] * clusters[idx1].Length + cMass[idx2][i] * clusters[idx2].Length) / (clusters[idx1].Length + clusters[idx2].Length);
		clusters.RemoveAt(Math.Max(idx1, idx2));
		clusters.RemoveAt(Math.Min(idx1, idx2));
		cMass.RemoveAt(Math.Max(idx1, idx2));
		cMass.RemoveAt(Math.Min(idx1, idx2));
	}
	static void PrintPicture(int numClusters, int[] clustering, double[][] data)
	{
		Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Purple, Color.OrangeRed, Color.Brown, Color.HotPink, Color.Cyan, Color.Black, Color.Yellow };
		int[] pictureSize = new int[data[0].Length].Select(x => int.MinValue).ToArray();
		for (int i = 0; i < data.Length; i++) //Определение размера картинки
			for (int j = 0; j < data[0].Length; j++)
				pictureSize[j] = Math.Max(pictureSize[j], Convert.ToInt32(data[i][j]));
		Bitmap picture = new Bitmap(pictureSize[0] + 1, pictureSize[1] + 1);
		Graphics.FromImage(picture).Clear(Color.White);
		for (int i = 0; i < data.Length; i++)
			picture.SetPixel(Convert.ToInt32(data[i][0]), Convert.ToInt32(data[i][1]), colors[clustering[i]]);
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
	static Bitmap ColorScaling(Bitmap picture, int scale) //Масштабирование только цветных пикселей. Размер картинки остается прежним
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