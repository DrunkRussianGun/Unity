using UnityEngine;

namespace Helpers
{
	public static class LayerHelper
	{
		public static bool Contains(this LayerMask layers, int layer)
			=> (layers.value & (1 << layer)) != 0;

		public static bool In(this int layer, LayerMask layers)
			=> layers.Contains(layer);
	}
}