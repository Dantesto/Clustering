using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

class KMeans
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
		double[][] data = Normalized(rawData), means = InitMeans(numClusters, data);
		int[] clustering = new int[data.Length];
		bool changed = true, success = true;
		int maxCount = data.Length * 10, ct = 0;
		while (changed && success && ct < maxCount)
		{
			changed = UpdateClustering(data, clustering, means);
			success = UpdateMeans(data, clustering, means);
			ct++;
		}
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
	static double[][] InitMeans(int numClusters, double[][] data) //Initialization Means - инициализация средних, находящихся далеко друг от друга
	{
		double[][] means = MakeMatrix(numClusters, data[0].Length);
		List<int> used = new List<int>(); //Список индексов элементов данных, назначенных в качестве средних
		Random rnd = new Random();
		int idx = rnd.Next(0, data.Length);
		Array.Copy(data[idx], means[0], data[idx].Length);
		for (int k = 1; k < numClusters; k++)
		{
			double[] dSquared = new double[data.Length].Select(x => double.MaxValue).ToArray(); //Distance sqaured - хранит квадраты расстояний между каждым элементом data и ближайшим существующим начальным средним
			int newMean = -1;
			for (int i = 0; i < data.Length; i++)
				for (int j = 0; j < k; j++)
					dSquared[i] = Math.Min(dSquared[i], Math.Pow(Distance(data[i], means[j]), 2));
			double p = rnd.NextDouble(), sum = 0, cumulative = 0; //Начало метода колеса рулетки
			for (int i = 0; i < dSquared.Length; i++)
				sum += dSquared[i];
			int ii = 0, sanity = 0;
			while (sanity < data.Length * 2)
			{
				cumulative += dSquared[ii] / sum;
				if (cumulative >= p && !used.Contains(ii))
				{
					newMean = ii;
					used.Add(newMean);
					break;
				}
				ii++;
				if (ii >= dSquared.Length)
					ii = 0;
				sanity++;
			}
			Array.Copy(data[newMean], means[k], data[newMean].Length);
		}
		return means;
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
	static bool UpdateClustering(double[][] data, int[] clustering, double[][] means)
	{
		bool updated = false;
		for (int i = 0; i < data.Length; i++)
		{
			int cheapestCluster = 0;
			for (int j = 0; j < means.Length; j++)
				cheapestCluster = Distance(data[i], means[j]) < Distance(data[i], means[cheapestCluster]) ? j : cheapestCluster;
			if (clustering[i] != cheapestCluster)
			{
				clustering[i] = cheapestCluster;
				updated = true;
			}
		}
		return updated;
	}
	static bool UpdateMeans(double[][] data, int[] clustering, double[][] means)
	{
		means = MakeMatrix(means.Length, means[0].Length);
		int[] countData = new int[means.Length]; //Количество элементов в каждом кластере
		for (int i = 0; i < data.Length; i++)
		{
			countData[clustering[i]]++;
			for (int j = 0; j < data[0].Length; j++)
				means[clustering[i]][j] += data[i][j];
		}
		for (int i = 0; i < means.Length; i++)
		{
			if (countData[i] == 0) //Если какой-то кластер пуст
				return false;
			for (int j = 0; j < means[0].Length; j++)
				means[i][j] /= countData[i];
		}
		return true;
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