/*
 * Created by: Silver Phoenix
 * Created: sábado, 7 de Julho de 2007
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using CenterSpace.Free;
using dasp.Neat.Experiments;
using dasp.Neat.Operators;
using dasp.Utils;

namespace dasp.Neat
{
	public class Population
	{
		#region PROTECTED CLASSES

		protected enum eDistance { Static, Dynamic }

		#region class Gene

		private class Gene
		{
			public readonly int Dest;
			public readonly int Src;
			public readonly double Weight;

			public Gene(int dest, int src, double weight)
			{
				Dest = dest;
				Src = src;
				Weight = weight;
			}
		
			public int Compare(Gene other)
			{
				if(Dest < other.Dest)
					return -1;

				if(Dest > other.Dest)
					return 1;

				if(Src < other.Src)
					return -1;

				if(Src > other.Src)
					return 1;

				return 0;
			}

			public override string ToString()
			{
				return string.Format("[{0} -> {1}]: W = {2}", Dest, Src, Weight);
			}
		}

	#endregion

		#region class SpecieComparer

		private class SpecieComparer: IComparer<Specie>
		{
			public readonly bool Descending;

			public SpecieComparer(bool descending)
			{
				Descending = descending;
			}

			public int Compare(Specie x, Specie y)
			{
				double dif = x.Champion.Fitness - y.Champion.Fitness;

				if(dif == 0)
					return 0;

				if(Descending)
					dif = -dif;

				return dif > 0 ? 1 : -1;
			}
		}

	#endregion

	#endregion

		#region SETTINGS

		public const string GROUP = "dasp.Neat.Population";

		public const string PROP_SEED       = "Random.Seed";
		public const string PROP_COUNT      = "Count";
		public const string PROP_MUTATOR    = "Operators.Mutation";
		public const string PROP_CROSSER    = "Operators.Crossover";
		public const string PROP_EXPERIMENT = "Experiment";

		public const string PROP_COEF_DISJOINT  = "Distance.Coef.Disjoint";
		public const string PROP_COEF_EXCESS    = "Distance.Coef.Excess";
		public const string PROP_COEF_WEIGHTS   = "Distance.Coef.Weights";
		public const string PROP_DIST_THRESHOLD = "Distance.Threshold";
		public const string PROP_DIST_RATIO     = "Distance.Ratio";
		public const string PROP_DIST_DYNAMIC   = "Distance.Dynamic";

		private int Seed;
		private int PopulationCount = 100;

		private static readonly Type[] param = { typeof(MersenneTwister), typeof(NeuronInnovation) };

		private ConstructorInfo MutatorClass =
			typeof(MutationOperator).GetConstructor(param);

		private ConstructorInfo CrosserClass =
			typeof(CrossoverOperator).GetConstructor(new[] { typeof(MersenneTwister) });

		private ConstructorInfo ExperimentClass =
			typeof(XORExperiment).GetConstructor(param);

		public double CoefDisjoint = 1.0;
		public double CoefExcess = 1.0;
		public double CoefWeights = 1.0;

		public bool DynamicDistance;
		public double DynamicRatio = 1.0;
		public double DistanceThreshold = 3.0;
		

	#endregion

		#region FIELDS

		public MersenneTwister Random;
		public NeuronInnovation InnovationManager;
		public aMutationOperator  Mutator;
		public aCrossoverOperator Crosser;
		public aExperiment Experiment;

		private readonly List<Specie> mSpecies;

		private double mTotalAvgFitness;

		private readonly SpecieComparer mComparer = new SpecieComparer(true);
		private bool mSorted;

	#endregion

		#region PROPERTIES

		public NetworkFitness Champion
		{
			get
			{
				if(mSorted)
					return mSpecies[0].Champion;

				NetworkFitness champion = mSpecies[0].Champion;

				for(int i = 1; i < mSpecies.Count; i++)
				{
					NetworkFitness otherChamp = mSpecies[i].Champion;
					if((mSpecies[0].SortDescending && otherChamp.Fitness > champion.Fitness)
					|| (!mSpecies[0].SortDescending && otherChamp.Fitness < champion.Fitness))
						champion = otherChamp;
				}

				return champion;
			}
		}

		public bool SortDescending
		{
			get { return mComparer.Descending; }
		}

		public List<Specie> Species
		{
			get { return mSpecies; }
		}

	#endregion

		#region CONSTRUCTORS

		public Population()
		{
			InitSettings();

			if(Seed == 0)
			{
				long time = DateTime.Now.Ticks;
				Seed = int.MaxValue & (int) (~time ^ (time >> 32));
			}

			Random = new MersenneTwister(Seed);
			Logger.Instance.Debug("Seed = {0}", Seed);
			Logger.Instance.Debug("");
			
			InnovationManager = new NeuronInnovation();

			object[] parms = new object[] {Random, InnovationManager};
			Mutator = (aMutationOperator) MutatorClass.Invoke(parms);
			Crosser = (aCrossoverOperator) CrosserClass.Invoke(new object[] {Random});
			Experiment = (aExperiment) ExperimentClass.Invoke(parms);

			mSpecies = new List<Specie>();

			if(DynamicDistance)
				initDynamicPopulation();
			else
				initStaticPopulation();
		}

	#endregion

		#region INIT METHODS

		private void InitSettings()
		{
			Settings settings = Settings.Instance;

			settings.TryGetValue(GROUP, PROP_SEED, ref Seed);
			settings.TryGetValue(GROUP, PROP_COUNT, ref PopulationCount);

			string classname = null;
			if(settings.TryGetValue(GROUP, PROP_MUTATOR, ref classname))
			{
				MutatorClass = Type.GetType(classname).GetConstructor(param);
			}

			classname = null;
			if(settings.TryGetValue(GROUP, PROP_CROSSER, ref classname))
			{
				CrosserClass = Type.GetType(classname).GetConstructor(
					new[] {typeof(MersenneTwister)});
			}

			classname = null;
			if(settings.TryGetValue(GROUP, PROP_EXPERIMENT, ref classname))
			{
				ExperimentClass = Type.GetType(classname).GetConstructor(param);
			}

			settings.TryGetValue(GROUP, PROP_COEF_DISJOINT, ref CoefDisjoint);
			settings.TryGetValue(GROUP, PROP_COEF_EXCESS, ref CoefExcess);
			settings.TryGetValue(GROUP, PROP_COEF_WEIGHTS, ref CoefWeights);
			settings.TryGetValue(GROUP, PROP_DIST_THRESHOLD, ref DistanceThreshold);
			settings.TryGetValue(GROUP, PROP_DIST_RATIO, ref DynamicRatio);
			settings.TryGetValue(GROUP, PROP_DIST_DYNAMIC, ref DynamicDistance);
		}

		private void initDynamicPopulation()
		{
			Specie specie = new Specie(Random);
			mSpecies.Add(specie);

			for(int i = 0; i < PopulationCount; i++)
			{
				NeuralNetwork nn = Experiment.Build();
				double fitness = Experiment.Evaluate(nn);
				specie.Add(nn, fitness);
			}

			specie.Sort();

			mTotalAvgFitness = specie.AverageFitness;

			NeuralNetwork champ = specie.Champion.Network;

			foreach(NetworkFitness nf in specie)
			{
				double dist = Distance(champ, nf.Network);
				if(dist > DistanceThreshold)
					DistanceThreshold = dist;
			}

			mSorted = true;
		}

		private void initStaticPopulation()
		{
			NeuralNetwork nn = Experiment.Build();
			double fitness = Experiment.Evaluate(nn);

			Specie specie = new Specie(Random) {{nn, fitness}};
			mSpecies.Add(specie);

			List<NeuralNetwork> reps = new List<NeuralNetwork> {nn};

			for(int i = 1; i < PopulationCount; i++)
			{
				nn = Experiment.Build();
				placeNetwork(nn, reps, mSpecies);
			}

			mTotalAvgFitness = 0.0;
			for(int i = 0; i < mSpecies.Count; i++)
			{
				mSpecies[i].Sort();
				mTotalAvgFitness += mSpecies[i].AverageFitness;
			}

			reps.Clear();
			sort();
		}

	#endregion

		#region PUBLIC METHODS

		public double Distance(NeuralNetwork nn1, NeuralNetwork nn2)
		{
			List<Gene> genes1 = linearizeGenes(nn1);
			List<Gene> genes2 = linearizeGenes(nn2);

			if(genes2.Count > genes1.Count)
			{
				List<Gene> temp = genes1;
				genes1 = genes2;
				genes2 = temp;
			}

			double D = 0;
			double W = 0;
			double M = 0;

			int i1, i2;
			for(i1 = 0, i2 = 0; i1 < genes1.Count && i2 < genes2.Count;)
			{
				Gene g1 = genes1[i1];
				Gene g2 = genes2[i2];
				int compare = g1.Compare(g2);

				if(compare == 0)
				{
					W += Math.Abs(g1.Weight - g2.Weight);
					M++;
					i1++;
					i2++;
				}
				else
				{
					D++;
					if(compare < 0) // => g1 < g2
						i1++;
					else // compare > 0 => g1 > g2
						i2++;
				}
			}

			double E = genes1.Count - i1 + genes2.Count - i2;
			E = (CoefDisjoint * D + CoefExcess * E) / genes1.Count;

			genes1.Clear();
			genes2.Clear();

			return E + (M == 0 ? 0 : W / M);
		}

		public void Epoch()
		{
			double oldCount = mSpecies.Count;

			killUnfitSpecies();

			int[] offsprings;
			List<NeuralNetwork> reps;

			// calculate number of offspring per specie
			// fetch representatives of current species
			genNextEpoch(out offsprings, out reps);

			// create new species for offsprings
				// distribute offsprings
				// recalculate totalFitness
				// sorting of species and population
			createOffsprings(offsprings, reps);

			reps.Clear();
	
			if(DynamicDistance)
			{
				DistanceThreshold *= DynamicRatio * mSpecies.Count / oldCount;
			}
		}

	#endregion

		#region PRIVATE METHODS

		private static List<Gene> linearizeGenes(NeuralNetwork nn)
		{
			List<Gene> genes = new List<Gene>();

			foreach(Neuron n in nn.GetNeurons())
				foreach(var s in n.Synapses)
					genes.Add(new Gene(n.ID, s.Value.Source, s.Value.Weight));

			return genes;
		}

		private void placeNetwork(NeuralNetwork nn,
			IList<NeuralNetwork> reps, IList<Specie> species)
		{
			double fitness = Experiment.Evaluate(nn);

			for(int j = 0; j < reps.Count; j++)
			{
				double dist = Distance(reps[j], nn);
				if(dist < DistanceThreshold)
				{
					species[j].Add(nn, fitness);
					return;
				}
			}

			reps.Add(nn);
			Specie specie = new Specie(Random) {{nn, fitness}};
			species.Add(specie);
		}

		private void killUnfitSpecies()
		{
			//kill bad performing specie if old or not improving
				// remove it
				// mTotalAvgFitness -= worstSpecie.AverageFitness !!!!!!!!! <- important
					// FORGETING THIS WILL DIMINISH THE POPULATION SIZE
					// (number of offprings depends on total average fitness)
					// this ensures the number of offspring is distributed
					// amongst the other species

			// TODO: killUnfitSpecies()
		}

		private void genNextEpoch(out int[] offsprings, out List<NeuralNetwork> reps)
		{
			int remainder = PopulationCount;
			offsprings = new int[mSpecies.Count];
			reps = new List<NeuralNetwork>(mSpecies.Count);

			for(int i = 0; i < mSpecies.Count; i++)
			{
				offsprings[i] = (int) (PopulationCount * mSpecies[i].AverageFitness / mTotalAvgFitness);
				remainder -= offsprings[i];
				reps.Add(mSpecies[i].Champion.Network);
			}

			offsprings[0] += remainder;
		}

		private void createOffsprings(int[] offsprings, IList<NeuralNetwork> reps)
		{
			List<NeuralNetwork> newNets = new List<NeuralNetwork>(PopulationCount);
			List<Specie> newSpecies = new List<Specie>(mSpecies.Count);
			for(int i = 0; i < mSpecies.Count; i++)
			{
				if(offsprings[i] == 0)
				{
					newSpecies.Add(new Specie(Random, mSpecies[i].ID));

					continue;
				}

				List<NeuralNetwork> children = mSpecies[i].Evolve(offsprings[i], Crosser, Mutator);
				newNets.AddRange(children);
				newSpecies.Add(new Specie(Random, mSpecies[i].ID, mSpecies[i].Age + 1));
			}

			// separate networks into species
			for(int i = 0; i < newNets.Count; i++)
			{
				placeNetwork(newNets[i], reps, newSpecies);
			}
			newNets.Clear();
			reps.Clear();
				
			// sort each specie (specie level)
			mTotalAvgFitness = 0.0;

			mSpecies.Clear();
			for(int i = 0; i < newSpecies.Count; i++)
			{
				if(newSpecies[i].Count == 0)
				{
					continue;
				}
					
				newSpecies[i].Sort();
				mSpecies.Add(newSpecies[i]);
				mTotalAvgFitness += newSpecies[i].AverageFitness;
			}
			newSpecies.Clear();

			mSorted = false;
			sort();
		}

		private void sort()
		{
			if(mSorted)
				return;

			mSorted = true;

			LinkedList<Specie> sorted = Sorter<Specie>.Sort(mSpecies, mComparer);

			mSpecies.Clear();
			mSpecies.AddRange(sorted);
			sorted.Clear();
		}

	#endregion
	}
}