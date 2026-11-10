using System;
using UnityEngine;

namespace HexWargame.Core
{
    /// <summary>
    /// Hex ızgara sistemleri için küp (Cube) ve eksenel (Axial) koordinat yapısı.
    /// Her zaman q + r + s = 0 kuralı korunur.
    /// </summary>
    [Serializable]
    public struct HexCoordinates : IEquatable<HexCoordinates>
    {
        [SerializeField] private int q; // Sütun ekseni (Axial Q)
        [SerializeField] private int r; // Satır ekseni (Axial R)

        public int Q => q;
        public int R => r;
        public int S => -q - r; // Küp koordinatı S

        public HexCoordinates(int q, int r)
        {
            this.q = q;
            this.r = r;
        }

        public HexCoordinates(int q, int r, int s)
        {
            if (q + r + s != 0)
                throw new ArgumentException("HexCoordinates q + r + s must equal 0.");

            this.q = q;
            this.r = r;
        }

        public static HexCoordinates Zero => new HexCoordinates(0, 0);

        #region Komşuluk ve Yönler (Pointy-Topped Hex)
        
        // 6 temel yön vektörü (Küp koordinatları)
        public static readonly HexCoordinates[] Directions = new HexCoordinates[]
        {
            new HexCoordinates(1, 0),   // Doğu (E)
            new HexCoordinates(1, -1),  // Kuzeydoğu (NE)
            new HexCoordinates(0, -1),  // Kuzeybatı (NW)
            new HexCoordinates(-1, 0),  // Batı (W)
            new HexCoordinates(-1, 1),  // Güneybatı (SW)
            new HexCoordinates(0, 1)    // Güneydoğu (SE)
        };

        public HexCoordinates GetNeighbor(int directionIndex)
        {
            directionIndex = ((directionIndex % 6) + 6) % 6;
            HexCoordinates dir = Directions[directionIndex];
            return new HexCoordinates(q + dir.q, r + dir.r);
        }

        public HexCoordinates[] GetAllNeighbors()
        {
            HexCoordinates[] neighbors = new HexCoordinates[6];
            for (int i = 0; i < 6; i++)
            {
                neighbors[i] = GetNeighbor(i);
            }
            return neighbors;
        }

        #endregion

        #region Mesafe ve Çizgi (Raycasting)
        
        public static int Distance(HexCoordinates a, HexCoordinates b)
        {
            return (Math.Abs(a.Q - b.Q) + Math.Abs(a.R - b.R) + Math.Abs(a.S - b.S)) / 2;
        }

        public int DistanceTo(HexCoordinates other)
        {
            return Distance(this, other);
        }

        /// <summary>
        /// İki hex hücresi arasındaki görüş hattını (Line of Sight) hesaplar.
        /// </summary>
        public static HexCoordinates[] GetLine(HexCoordinates a, HexCoordinates b)
        {
            int n = Distance(a, b);
            if (n == 0) return new HexCoordinates[] { a };

            HexCoordinates[] results = new HexCoordinates[n + 1];
            float nudgeQ = 1e-6f;
            float nudgeR = 2e-6f;
            float nudgeS = -3e-6f;

            float aQ = a.Q + nudgeQ;
            float aR = a.R + nudgeR;
            float aS = a.S + nudgeS;

            float bQ = b.Q + nudgeQ;
            float bR = b.R + nudgeR;
            float bS = b.S + nudgeS;

            for (int i = 0; i <= n; i++)
            {
                float t = (float)i / n;
                float curQ = Mathf.Lerp(aQ, bQ, t);
                float curR = Mathf.Lerp(aR, bR, t);
                float curS = Mathf.Lerp(aS, bS, t);

                results[i] = RoundCube(curQ, curR, curS);
            }

            return results;
        }

        private static HexCoordinates RoundCube(float qf, float rf, float sf)
        {
            int qi = Mathf.RoundToInt(qf);
            int ri = Mathf.RoundToInt(rf);
            int si = Mathf.RoundToInt(sf);

            float qDiff = Mathf.Abs(qi - qf);
            float rDiff = Mathf.Abs(ri - rf);
            float sDiff = Mathf.Abs(si - sf);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                qi = -ri - si;
            }
            else if (rDiff > sDiff)
            {
                ri = -qi - si;
            }

            return new HexCoordinates(qi, ri);
        }

        #endregion

        #region Dünya Koordinatı Dönüşümü (2.5D İzometrik)

        /// <summary>
        /// Pointy-topped hex için 2.5D izometrik dünya düzlemi pozisyonu verir.
        /// </summary>
        public Vector3 ToWorldPosition(float hexRadius, float elevationStep = 0.5f, int elevation = 0)
        {
            float x = hexRadius * Mathf.Sqrt(3f) * (q + r * 0.5f);
            float z = hexRadius * 1.5f * r;
            float y = elevation * elevationStep;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Dünya pozisyonundan en yakın Hex koordinatını bulur.
        /// </summary>
        public static HexCoordinates FromWorldPosition(Vector3 worldPos, float hexRadius)
        {
            float q = (Mathf.Sqrt(3f) / 3f * worldPos.x - 1f / 3f * worldPos.z) / hexRadius;
            float r = (2f / 3f * worldPos.z) / hexRadius;
            float s = -q - r;

            return RoundCube(q, r, s);
        }

        #endregion

        #region Operator Overloads & Equatable

        public bool Equals(HexCoordinates other) => q == other.q && r == other.r;
        public override bool Equals(object obj) => obj is HexCoordinates other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(q, r);

        public static bool operator ==(HexCoordinates a, HexCoordinates b) => a.Equals(b);
        public static bool operator !=(HexCoordinates a, HexCoordinates b) => !a.Equals(b);
        public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b) => new HexCoordinates(a.q + b.q, a.r + b.r);
        public static HexCoordinates operator -(HexCoordinates a, HexCoordinates b) => new HexCoordinates(a.q - b.q, a.r - b.r);

        public override string ToString() => $"({q}, {r}, {S})";

        #endregion
    }
}
