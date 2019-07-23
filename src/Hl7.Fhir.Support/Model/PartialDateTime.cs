﻿/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */


using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using System;
using System.Xml;

namespace Hl7.Fhir.Model.Primitives
{
    //TODO: Merge with FhirDateTime from Model namespace?
    public struct PartialDateTime : IComparable
    {
        private string _value;

        public static PartialDateTime Parse(string value)
        {
            try
            {
                var dummy = PrimitiveTypeConverter.ConvertTo<DateTimeOffset>(value);
            }
            catch
            {
                throw new FormatException("Partial is in an invalid format, should use ISO8601 YYYY-MM-DDThh:mm:ss+TZ notation");
            }

            return new PartialDateTime { _value = value };
        }

        public static bool TryParse(string representation, out PartialDateTime value)
        {
            try
            {
                value = Parse(representation);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        public DateTimeOffset ToUniversalTime() =>
            PrimitiveTypeConverter.ConvertTo<DateTimeOffset>(_value).ToUniversalTime();


        // overload operator <
        public static bool operator <(PartialDateTime a, PartialDateTime b) => a.ToUniversalTime() < b.ToUniversalTime();

        public static bool operator <=(PartialDateTime a, PartialDateTime b) => a.ToUniversalTime() <= b.ToUniversalTime();

        // overload operator >
        public static bool operator >(PartialDateTime a, PartialDateTime b) => a.ToUniversalTime() > b.ToUniversalTime();

        public static bool operator >=(PartialDateTime a, PartialDateTime b) => a.ToUniversalTime() >= b.ToUniversalTime();

        public static bool operator ==(PartialDateTime a, PartialDateTime b) => Equals(a, b);

        public static bool operator !=(PartialDateTime a, PartialDateTime b) => !(a == b);

        public bool IsEquivalentTo(PartialDateTime other)
        {
            if (other == null) return false;

            var len = Math.Min(10,Math.Min(_value.Length, other._value.Length));

            return String.Compare(_value, 0, other._value, 0, len) == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is PartialDateTime other)
            {

                // If we just have a date, do a straight compare
                // otherwise, we have date + time, and DateTimeOffset will work as expected,
                // correctly comparing timezones
                if (_value.Length <= 10)
                    return _value == other._value;
                else
                    return ((PartialDateTime)obj).ToUniversalTime() == this.ToUniversalTime();
            }
            else
                return false;
        }

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value;

        public static PartialDateTime Now() => FromDateTime(DateTimeOffset.Now);

        public static PartialDateTime Today() => new PartialDateTime { _value = DateTimeOffset.Now.ToString("yyyy-MM-dd") };

        public static PartialDateTime FromDateTime(DateTimeOffset dto) => new PartialDateTime { _value = PrimitiveTypeConverter.ConvertTo<string>(dto) };

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is PartialDateTime p)
            {
                if (this < p) return -1;
                if (this > p) return 1;
                return 0;
            }
            else
                throw Error.Argument(nameof(obj), "Must be a PartialDateTime");
        }
    }
}
