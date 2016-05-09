/*
 * Created by: Silver Phoenix
 * Created: domingo, 22 de Julho de 2007
 */

using System;
using CenterSpace.Free;
using dasp.Neat.ActivationFunction;
using dasp.Utils;

namespace dasp.Neat.Experiments
{
	public class PoleBalancingExperiment: aExperiment
	{
		#region SETTINGS

		public const string GROUP = "dasp.Neat.Experiments.PoleBalancing";

		public const string PROP_STEPS = "Steps";
		public const string PROP_TIME  = "Time";
		public const string PROP_FORCE = "Force";
		public const string PROP_ANGLE_THRESHOLD = "Angle.Threshold";
		public const string PROP_GRAVITY = "Gravity";
		public const string PROP_CART_MASS = "Cart.Mass";
		public const string PROP_POLE_MASS = "Pole.Mass";
		public const string PROP_POLE_LENGTH = "Pole.Length";

		public int NumSteps = 50;
		public double TimePerStep = 0.02;
		public double MaxForce = 10;
		public double AngleThreshold = 12 * Math.PI / 180.0;

		public double Gravity = 9.8;

		private double cartMass = 1.0;
		private double poleMass = 0.1;
		private double poleLength = 1.0;
		private double totalMass;
		private double poleMassLength;

	#endregion

		#region FIELDS

		private const int NumInputs = 4;

		private const int CartPosition = 0;
		private const int CartVelocity = 1;
		private const int PoleAngle    = 2;
		private const int PoleVelocity = 3;

		private readonly int outputId;
		private readonly aActivationFunction function;

	#endregion

		#region PROPERTIES

		public double CartMass
		{
			get { return cartMass; }
			set
			{
				cartMass = value;
				totalMass = cartMass + poleMass;
			}
		}

		public double PoleMass
		{
			get { return poleMass; }
			set
			{
				poleMass = value;
				totalMass = cartMass + poleMass;
				poleMassLength = poleMass * poleLength;
			}
		}

		public double PoleLength
		{
			get { return poleLength; }
			set
			{
				poleLength = value;
				poleMassLength = poleMass * poleLength;
			}
		}

	#endregion

		#region CONSTRUCTORS

		public PoleBalancingExperiment(MersenneTwister rand, NeuronInnovation innovationManager):
			base(rand, innovationManager)
		{
			outputId = base.innovationManager.NextInnovation();
			function = new SigmoidFunction(4.9);
		}

		protected override void InitSettings(Settings settings)
		{
			settings.TryGetValue(GROUP, PROP_STEPS, ref NumSteps);
			settings.TryGetValue(GROUP, PROP_TIME, ref TimePerStep);
			settings.TryGetValue(GROUP, PROP_FORCE, ref MaxForce);
			settings.TryGetValue(GROUP, PROP_ANGLE_THRESHOLD, ref AngleThreshold);
			AngleThreshold *= Math.PI / 180.0;

			settings.TryGetValue(GROUP, PROP_GRAVITY, ref Gravity);
			settings.TryGetValue(GROUP, PROP_CART_MASS, ref cartMass);
			settings.TryGetValue(GROUP, PROP_POLE_MASS, ref poleMass);
			totalMass = cartMass + poleMass;

			settings.TryGetValue(GROUP, PROP_POLE_LENGTH, ref poleLength);
			poleMassLength = poleMass * poleLength;
		}

	#endregion

		#region METHODS

		public override NeuralNetwork Build()
		{
			NeuralNetwork nn = new NeuralNetwork(NumInputs);
			Neuron output = new Neuron(outputId, function, 1);

			for(int i = 0; i < NumInputs; i++)
			{
				output.Synapses.Add(i - NumInputs, new Synapse(i - NumInputs, random.NextDouble(-1, 1)));
			}

			nn.OutputNeurons.Add(output);

			return nn;
		}

		public override double Evaluate(NeuralNetwork nn)
		{
			nn.Clean();

			// Cart position: [-TrackLength/2; TrackLength/2] (m)
			// Cart velocity: [0.0; 1.0] (m/s)
			// Pole angle   : [-AngleThreshold; AngleThreshold] (rad)
			// Pole velocity: [0.0; 1.0] (rad/s)

			// Output = force: [-MaxForce; MaxForce] (N)

			double[] inputs = new double[NumInputs];
			inputs[CartPosition] = 0;
			inputs[CartVelocity] = 0;
			inputs[PoleAngle]    = Math.Abs(random.NextDouble() - 1.0);
			inputs[PoleVelocity] = 0;

			int step;
			for(step = 0; step < NumSteps; step++)
			{
				for(int i = 0; i < 10; i++)
				{
					nn.Execute(inputs);
				}

				TimeStep(inputs, nn.Output[0]);

				if(Math.Abs(inputs[CartPosition]) > 0.5 || Math.Abs(inputs[PoleAngle]) > 1.0)
					break;
			}

			return step == 0 ? 0.1 : step;
		}

		public void TimeStep(double[] inputs, double output)
		{
			// Cart position: [0.0; TrackLength] (m)
			// Cart velocity: [0.0; 1.0] (m/s)
			// Pole angle   : [-AngleThreshold; AngleThreshold] (rad)
			// Pole velocity: [0.0; 1.0] (rad/s)

			// Output = force: [-MaxForce; MaxForce] (N)

			double force = 2.0 * MaxForce * (output - 0.5);
			if(force == 0)
				force = random.NextBool() ? 1e-8 : -1e-8;

			double cos = Math.Cos(inputs[PoleAngle] * AngleThreshold);
			double sin = Math.Sin(inputs[PoleAngle] * AngleThreshold);

			force += poleMassLength * inputs[PoleVelocity] * inputs[PoleVelocity] * sin;
			force /= totalMass;

			double poleAccelaration = poleLength * (4.0/3.0 - poleMass * cos * cos / totalMass);
			poleAccelaration = (Gravity * sin - cos * force) / poleAccelaration;

			double cartAccelaration = force - poleMassLength * poleAccelaration * cos / totalMass;

			inputs[CartPosition] += TimePerStep * inputs[CartVelocity];
			inputs[CartVelocity] += TimePerStep * cartAccelaration;
			inputs[PoleAngle]    += TimePerStep * inputs[PoleVelocity];
			inputs[PoleVelocity] += TimePerStep * poleAccelaration;
		}

	#endregion
	}
}