using System;
using System.Collections.Generic;

namespace PracticePlugin.Models
{
    public class SongSpeedParameter : IEquatable<SongSpeedParameter>
    {
        public int Speed { get; set; }
        public float NJS { get; set; }
        public float Offset { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as SongSpeedParameter);
        }

        public bool Equals(SongSpeedParameter other)
        {
            return !(other is null) &&
                   this.Speed == other.Speed &&
                   this.NJS == other.NJS &&
                   this.Offset == other.Offset;
        }

        public override int GetHashCode()
        {
            var hashCode = -1785421424;
            hashCode = hashCode * -1521134295 + this.Speed.GetHashCode();
            hashCode = hashCode * -1521134295 + this.NJS.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Offset.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(SongSpeedParameter left, SongSpeedParameter right)
        {
            return EqualityComparer<SongSpeedParameter>.Default.Equals(left, right);
        }

        public static bool operator !=(SongSpeedParameter left, SongSpeedParameter right)
        {
            return !(left == right);
        }
    }
}
