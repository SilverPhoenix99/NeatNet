/*
 * Created by: Silver Phoenix
 * Created: domingo, 8 de Julho de 2007
 */

using System;
using System.Collections;
using System.Collections.Generic;
using CenterSpace.Free;
using dasp.Neat.Operators;
using dasp.Utils;

namespace dasp.Neat
{
	public enum eDistribution { Uniform, Gaussian }

	public class Specie: IEnumerable<NetworkFitness>
	{
		private delegate void OffspringDistribution(
			ICollection<NeuralNetwork> children, int length, int offspring,
			aCrossoverOperator crosser, aMutationOperator mutator);

		#region class NetComparer

		private class NetComparer: IComparer<NetworkFitness>
		{
			public bool Descending;

			public NetComparer(bool descending)
			{
				Descending = descending;
			}

			public int Compare(NetworkFitness x, NetworkFitness y)
			{
				double dif = x.Fitness - y.Fitness;

				if(dif == 0)
					return 0;

				if((Descending && dif < 0) || (!Descending && dif > 0))
					return 1;

				return -1;
			}
		}

	#endregion

		#region SETTINGS

		public const string GROUP = "dasp.Neat.Specie";

		public const string PROP_SORT = "Fitness.SortDescending";
		public const string PROP_ELITE = "Elitism";
		public const string PROP_DISTRIBUTION = "Distribution";

		private readonly NetComparer comparer = new NetComparer(true);
		public double Elitism;

		private eDistribution distribution = eDistribution.Uniform;
        private OffspringDistribution distributionMethod;

	#endregion

		#region FIELDS

		public readonly int ID;
		public readonly int Age;

		private static int speciesId;

		private List<NetworkFitness> nets;
		private double totalFitness;

		private readonly MersenneTwister random;

	#endregion

		#region PROPERTIES

		public NetworkFitness Champion
		{
			get
			{
				if(nets.Count == 0)
				{
					throw new MemberAccessException("Cannot fetch the champion of an empty specie.");
				}

				if(Sorted)
				{
					return nets[0];
				}

				NetworkFitness best = nets[0];

				for(int i = 1; i < nets.Count; i++)
				{
					if(comparer.Compare(nets[i], best) < 0)
						best = nets[i];
				}

				return best;
			}
		}

		public bool SortDescending
		{
			get { return comparer.Descending; }
		}

		public bool Sorted { get; private set; }

		public double AverageFitness
		{
			get { return totalFitness / nets.Count; }
		}

		public int Count
		{
			get { return nets.Count; }
		}

		public eDistribution Distribution
		{
			get { return distribution; }
			set
			{
				if(distribution == value)
					return;

				distribution = value;
				if(distribution == eDistribution.Gaussian)
					distributionMethod = gaussianDistribution;
				else
					distributionMethod = uniformDistribution;
			}
		}

	#endregion

		#region CONSTRUCTORS

		public Specie(MersenneTwister random, int id, int age)
		{
            distributionMethod = uniformDistribution;

			InitSettings();

			ID = id;
			Age = age;
			nets = new List<NetworkFitness>();
			Sorted = false;
			this.random = random;
		}

		public Specie(MersenneTwister random, int id)
			: this(random, id, 0)
		{}

		public Specie(MersenneTwister random)
			: this(random, speciesId++, 0)
		{}

		public void InitSettings()
		{
			Settings settings = Settings.Instance;

			settings.TryGetValue(GROUP, PROP_SORT, ref comparer.Descending);
			settings.TryGetValue(GROUP, PROP_ELITE, ref Elitism);

			string prop = null;
			if(!settings.TryGetValue(GROUP, PROP_DISTRIBUTION, ref prop))
			{
				return;
			}

			if(Enum.IsDefined(typeof(eDistribution), prop))
			{
				Distribution = (eDistribution) Enum.Parse(typeof(eDistribution), prop);
			}
		}

	#endregion

		#region METHODS

		public void Add(NeuralNetwork nn, double fitness)
		{
			nets.Add(new NetworkFitness(nn, fitness));
			totalFitness += fitness;
			Sorted = false;
		}

		public void Sort()
		{
			if(Sorted || nets.Count == 0)
				return;

			LinkedList<NetworkFitness> sortedNets =
				Sorter<NetworkFitness>.Sort(nets, comparer);

			nets = new List<NetworkFitness>(sortedNets);

			Sorted = true;
		}

		public List<NeuralNetwork> Evolve(int offspring,
			aCrossoverOperator crosser, aMutationOperator mutator)
		{
			if(!Sorted)
			{
				throw new MethodAccessException("Cannot evolve unsorted specie.");
			}

			List<NeuralNetwork> children = new List<NeuralNetwork>(offspring);

			if(Elitism > 0)
			{
				int elite = (int) Elitism * offspring;
				if(elite == 0)
					elite = 1;

				offspring -= elite;

				for(int i = 0; i < elite; i++)
					children.Add(nets[i].Network.Clone());
			}

			int length = Math.Min(offspring, nets.Count);

			if(length == 0)
			{
				for(int i = 0; i < offspring; i++)
				{
					NeuralNetwork child = crosser.Crossover(nets[0], nets[0]);
					mutator.MutateInPlace(child);
					children.Add(child);
				}

				return children;
			}

			distributionMethod(children, length, offspring, crosser, mutator);

			return children;
		}

		private void gaussianDistribution(ICollection<NeuralNetwork> children, int length, int offspring,
			aCrossoverOperator crosser, aMutationOperator mutator)
		{
			double[] distributions = new double[length];
			distributions[0] = nets[0].Fitness;
			for(int i = 1; i < length; i++)
			{
				distributions[i] = distributions[i-1] + nets[i].Fitness;
			}

			for(int i = 0; i < offspring; i++)
			{
				int momIndex = gaussianPosition(distributions);
				int dadIndex = gaussianPosition(distributions);

				NeuralNetwork child = crosser.Crossover(nets[momIndex], nets[dadIndex]);
				mutator.MutateInPlace(child);
				children.Add(child);
			}
		}

		private int gaussianPosition(double[] distributions)
		{
			int min = 0;
			int max = distributions.Length - 1;

			double prob = random.NextDouble(0, distributions[max]);

			while(min < max)
			{
				int mid = min + (max - min) / 2;
				if(distributions[mid] < prob)
					min = mid + 1;
				else
					max = mid;
			}

			return Math.Min(min, distributions.Length - 1);
		}

		private void uniformDistribution(ICollection<NeuralNetwork> children, int length, int offspring,
			aCrossoverOperator crosser, aMutationOperator mutator)
		{
			for(int i = 0; i < offspring; i++)
			{
				int momIndex = random.Next(length);
				int dadIndex = random.Next(length);

				NeuralNetwork child = crosser.Crossover(nets[momIndex], nets[dadIndex]);
				mutator.MutateInPlace(child);
				children.Add(child);
			}
		}

		public IEnumerator<NetworkFitness> GetEnumerator()
		{
			return nets.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return nets.GetEnumerator();
		}

		public override string ToString()
		{
			return Sorted ?
				string.Format("Age = {0} Count = {1} Fitness = {2}", Age, nets.Count, Champion.Fitness)
				: string.Format("Age = {0} Count = {1}", Age, nets.Count);
		}

		public void PrintFitnesses(System.IO.TextWriter writer, bool writeLine)
		{
			for(int i = 0; i < nets.Count; i++)
			{
				writer.Write("{0}\t", nets[i].Fitness);
			}

			if(!writeLine)
			{
				return;
			}

			writer.WriteLine();
			writer.Flush();
		}

	#endregion
	}
}