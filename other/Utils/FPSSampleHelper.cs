using System;
using UnityEngine;

namespace Leyoutech.Helper
{
	public class FPSSampleHelper : MonoBehaviour
	{
		public int FPS;

		private bool m_IsSampling;
		private Data m_Data;

		public void BeginSample()
		{
			m_IsSampling = true;
			m_Data.SampleCount = 0;
			m_Data.AverageDelta = 0;
			m_Data.MaximumDelta = double.MinValue;
			m_Data.MinimumDelta = double.MaxValue;
		}

		public Data EndSample()
		{
			m_IsSampling = false;
			return m_Data;
		}

		protected void Update()
		{
			if (!m_IsSampling)
			{
				return;
			}

			double delta = Time.unscaledDeltaTime;
			m_Data.AverageDelta = (m_Data.AverageDelta * m_Data.SampleCount + delta) / (m_Data.SampleCount + 1);
			m_Data.SampleCount++;
			m_Data.MaximumDelta = Math.Max(m_Data.MaximumDelta, delta);
			m_Data.MinimumDelta = Math.Min(m_Data.MinimumDelta, delta);
		}

		public struct Data
		{
			public int SampleCount;
			public double AverageDelta;
			public double MaximumDelta;
			public double MinimumDelta;
		}
	}
}