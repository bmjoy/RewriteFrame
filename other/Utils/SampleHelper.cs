using System;

namespace Leyoutech.Helper
{
	public struct DoubleSampleHelper : ISampleHelper<double>
	{
		private int m_SampleCount;
		private double m_Total;
		private double m_Average;
		private double m_Maximum;
		private double m_Minimum;

		public double GetAverage()
		{
			return m_Average;
		}

		public double GetTotal()
		{
			return m_Total;
		}

		public double GetMaximum()
		{
			return m_Maximum;
		}

		public double GetMinimum()
		{
			return m_Minimum;
		}

		public void Reset()
		{
			m_SampleCount = 0;
			m_Total = 0;
			m_Average = 0;
			m_Maximum = double.MinValue;
			m_Minimum = double.MaxValue;
		}

		public void Sample(double value)
		{
			m_SampleCount++;
			m_Total += value;
			m_Average = (double)(m_Total / (float)m_SampleCount);
			m_Maximum = Math.Max(m_Maximum, value);
			m_Minimum = Math.Min(m_Minimum, value);
		}
	}

	public struct LongSampleHelper : ISampleHelper<long>
	{
		private int m_SampleCount;
		private long m_Total;
		private long m_Average;
		private long m_Maximum;
		private long m_Minimum;

		public long GetAverage()
		{
			return m_Average;
		}

		public long GetTotal()
		{
			return m_Total;
		}

		public long GetMaximum()
		{
			return m_Maximum;
		}

		public long GetMinimum()
		{
			return m_Minimum;
		}

		public void Reset()
		{
			m_SampleCount = 0;
			m_Total = 0;
			m_Average = 0;
			m_Maximum = long.MinValue;
			m_Minimum = long.MaxValue;
		}

		public void Sample(long value)
		{
			m_SampleCount++;
			m_Total += value;
			m_Average = (long)(m_Total / (float)m_SampleCount);
			m_Maximum = Math.Max(m_Maximum, value);
			m_Minimum = Math.Min(m_Minimum, value);
		}
	}

	public interface ISampleHelper<T>
	{
		void Reset();
		void Sample(T value);
		T GetTotal();
		T GetAverage();
		T GetMaximum();
		T GetMinimum();
	}
}