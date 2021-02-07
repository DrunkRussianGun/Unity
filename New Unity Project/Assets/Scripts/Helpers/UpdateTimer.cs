using System;

namespace Helpers
{
	public class UpdateTimer
	{
		private float timer;
		private float interval;

		private int currentRepeatsCount;
		private int? repeatsCount;
		private bool checkResult;

		public UpdateTimer(float intervalInSeconds)
		{
			interval = intervalInSeconds;
		}

		public UpdateTimer(float intervalInSeconds, int repeatsCount, bool checkResultAfterRepeatsElapsed)
			: this(intervalInSeconds)
		{
			if (repeatsCount < 0)
				throw new ArgumentOutOfRangeException(nameof(repeatsCount));
			this.repeatsCount = repeatsCount;
			checkResult = checkResultAfterRepeatsElapsed;
		}

		public bool Check(float elapsedSeconds)
		{
			if (currentRepeatsCount >= repeatsCount)
				return checkResult;

			timer += elapsedSeconds;
			if (timer >= interval)
			{
				ResetTimer();
				++currentRepeatsCount;
				return true;
			}

			return false;
		}

		public void ResetTimer()
		{
			timer = 0;
		}

		public void ResetRepeatsCounter()
		{
			currentRepeatsCount = 0;
		}

		public void ResetAll()
		{
			ResetTimer();
			ResetRepeatsCounter();
		}
	}
}
