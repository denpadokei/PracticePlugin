using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracticePlugin.Models
{
    public class SongSpeedParameeter : IEquatable<SongSpeedParameeter>
    {
        public int Speed { get; set; }
        public float NJS { get; set; }
        public float Offset { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as SongSpeedParameeter);
        }

        public bool Equals(SongSpeedParameeter other)
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

        public static bool operator ==(SongSpeedParameeter left, SongSpeedParameeter right)
        {
            return EqualityComparer<SongSpeedParameeter>.Default.Equals(left, right);
        }

        public static bool operator !=(SongSpeedParameeter left, SongSpeedParameeter right)
        {
            return !(left == right);
        }
    }
}
