// Decompiled with JetBrains decompiler
// Type: UnityEngine.SnapAxisFilter
// Assembly: UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 066B41AD-FD1C-4A3B-9075-9D491F1499E8
// Assembly location: E:\UnityEditor\2021.3.4f1c1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll

using System;
using UnityEngine;

namespace LIBII.Light2D
{
    internal struct SnapAxisFilter : IEquatable<SnapAxisFilter>
    {
        private const SnapAxis X = SnapAxis.X;
        private const SnapAxis Y = SnapAxis.Y;
        private const SnapAxis Z = SnapAxis.Z;
        public static readonly SnapAxisFilter all = new SnapAxisFilter(SnapAxis.All);
        private SnapAxis m_Mask;

        public float x => (this.m_Mask & SnapAxis.X) == SnapAxis.X ? 1f : 0.0f;

        public float y => (this.m_Mask & SnapAxis.Y) == SnapAxis.Y ? 1f : 0.0f;

        public float z => (this.m_Mask & SnapAxis.Z) == SnapAxis.Z ? 1f : 0.0f;

        public SnapAxisFilter(Vector3 v)
        {
            this.m_Mask = SnapAxis.None;
            float num = 1E-06f;
            if ((double) Mathf.Abs(v.x) > (double) num)
                this.m_Mask |= SnapAxis.X;
            if ((double) Mathf.Abs(v.y) > (double) num)
                this.m_Mask |= SnapAxis.Y;
            if ((double) Mathf.Abs(v.z) <= (double) num)
                return;
            this.m_Mask |= SnapAxis.Z;
        }

        public SnapAxisFilter(SnapAxis axis)
        {
            this.m_Mask = SnapAxis.None;
            if ((axis & SnapAxis.X) == SnapAxis.X)
                this.m_Mask |= SnapAxis.X;
            if ((axis & SnapAxis.Y) == SnapAxis.Y)
                this.m_Mask |= SnapAxis.Y;
            if ((axis & SnapAxis.Z) != SnapAxis.Z)
                return;
            this.m_Mask |= SnapAxis.Z;
        }

        public override string ToString() =>
            string.Format("{{{0}, {1}, {2}}}", (object) this.x, (object) this.y, (object) this.z);

        public int active
        {
            get
            {
                int num = 0;
                if ((this.m_Mask & SnapAxis.X) > SnapAxis.None)
                    ++num;
                if ((this.m_Mask & SnapAxis.Y) > SnapAxis.None)
                    ++num;
                if ((this.m_Mask & SnapAxis.Z) > SnapAxis.None)
                    ++num;
                return num;
            }
        }

        public static implicit operator Vector3(SnapAxisFilter mask) => new Vector3(mask.x, mask.y, mask.z);

        public static explicit operator SnapAxisFilter(Vector3 v) => new SnapAxisFilter(v);

        public static explicit operator SnapAxis(SnapAxisFilter mask) => mask.m_Mask;

        public static SnapAxisFilter operator |(SnapAxisFilter left, SnapAxisFilter right) =>
            new SnapAxisFilter(left.m_Mask | right.m_Mask);

        public static SnapAxisFilter operator &(SnapAxisFilter left, SnapAxisFilter right) =>
            new SnapAxisFilter(left.m_Mask & right.m_Mask);

        public static SnapAxisFilter operator ^(SnapAxisFilter left, SnapAxisFilter right) =>
            new SnapAxisFilter(left.m_Mask ^ right.m_Mask);

        public static SnapAxisFilter operator ~(SnapAxisFilter left) => new SnapAxisFilter(~left.m_Mask);

        public static Vector3 operator *(SnapAxisFilter mask, float value) =>
            new Vector3(mask.x * value, mask.y * value, mask.z * value);

        public static Vector3 operator *(SnapAxisFilter mask, Vector3 right) =>
            new Vector3(mask.x * right.x, mask.y * right.y, mask.z * right.z);

        public static Vector3 operator *(Quaternion rotation, SnapAxisFilter mask)
        {
            int active = mask.active;
            if (active > 2)
                return (Vector3) mask;
            Vector3 vector3 = rotation * (Vector3) mask;
            vector3 = new Vector3(Mathf.Abs(vector3.x), Mathf.Abs(vector3.y), Mathf.Abs(vector3.z));
            return active > 1
                ? new Vector3(
                    (double) vector3.x > (double) vector3.y || (double) vector3.x > (double) vector3.z ? 1f : 0.0f,
                    (double) vector3.y > (double) vector3.x || (double) vector3.y > (double) vector3.z ? 1f : 0.0f,
                    (double) vector3.z > (double) vector3.x || (double) vector3.z > (double) vector3.y ? 1f : 0.0f)
                : new Vector3(
                    (double) vector3.x <= (double) vector3.y || (double) vector3.x <= (double) vector3.z ? 0.0f : 1f,
                    (double) vector3.y <= (double) vector3.z || (double) vector3.y <= (double) vector3.x ? 0.0f : 1f,
                    (double) vector3.z <= (double) vector3.x || (double) vector3.z <= (double) vector3.y ? 0.0f : 1f);
        }

        public static bool operator ==(SnapAxisFilter left, SnapAxisFilter right) => left.m_Mask == right.m_Mask;

        public static bool operator !=(SnapAxisFilter left, SnapAxisFilter right) => !(left == right);

        public float this[int i]
        {
            get
            {
                if (i < 0 || i > 2)
                    throw new IndexOutOfRangeException();
                return (float) (1 & (int) this.m_Mask >> i) * 1f;
            }
            set
            {
                if (i < 0 || i > 2)
                    throw new IndexOutOfRangeException();
                this.m_Mask &= (SnapAxis) ~(1 << i);
                this.m_Mask |= (SnapAxis) (((double) value > 0.0 ? 1 : 0) << i);
            }
        }

        public bool Equals(SnapAxisFilter other) => this.m_Mask == other.m_Mask;

        public override bool Equals(object obj) => obj != null && obj is SnapAxisFilter other && this.Equals(other);

        public override int GetHashCode() => this.m_Mask.GetHashCode();
    }
}