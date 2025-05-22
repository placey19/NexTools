using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nexcide.TileKing {

    public static class MathsUtil {

        /// <summary>
        /// Calculate the normal of a triangle, assuming clockwise vertices.
        /// </summary>
        public static Vector3 CalculateNormal(Vector3 v1, Vector3 v2, Vector3 v3) {
            return Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
        }

        /// <summary>
        /// Modulo implementation that handles negative values, so the result is always positive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Mod(this int x, int m) {
            int result = (x % m);
            return (result < 0 ? (result + m) : result);
        }

        /// <summary>
        /// Shift the contents of a generic List. Positive values move right, negative move left.
        /// </summary>
        public static void Shift<T>(this List<T> list, int shiftAmount) {
            shiftAmount = shiftAmount.Mod(list.Count);

            if (shiftAmount > 0) {
                List<T> shiftedList = list.GetRange(list.Count - shiftAmount, shiftAmount);
                shiftedList.AddRange(list.GetRange(0, list.Count - shiftAmount));

                list.Clear();
                list.AddRange(shiftedList);
            }
        }
    }
}
