using System;
using System.Text.RegularExpressions;

namespace SystemUtilities.Serialization
{
    public abstract class BaseConverter : IConverter
    {
        public virtual bool ConvertToBoolean(object obj)
        {
            if (obj is Boolean) return (Boolean)obj;
            bool value = false;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    Boolean.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToBoolean((IConvertible)value);
                }
            }
            return value;
        }

        public virtual byte ConvertToByte(object obj)
        {
            if (obj is Byte) return (Byte)obj;
            byte value = Byte.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    Byte.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToByte((IConvertible)value);
                }
            }
            return value;
        }

        public virtual char ConvertToChar(object obj)
        {
            if (obj is Char) return (Char)obj;
            char value = Char.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    Char.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToChar((IConvertible)value);
                }
            }
            return value;
        }

        public virtual DateTime ConvertToDateTime(object obj)
        {
            if (obj is DateTime) return (DateTime)obj;
            DateTime value = DateTime.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    DateTime.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToDateTime((IConvertible)value);
                }
            }
            return value;
        }

        public virtual decimal ConvertToDecimal(object obj)
        {
            if (obj is Decimal) return (Decimal)obj;
            decimal value = Decimal.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    Decimal.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToDecimal((IConvertible)value);
                }
            }
            return value;
        }

        public virtual double ConvertToDouble(object obj)
        {
            if (obj is Double) return (Double)obj;
            double value = Double.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    Double.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToDouble((IConvertible)value);
                }
            }
            return value;
        }

        public virtual Guid ConvertToGuid(object obj)
        {
            if (obj is Guid) return (Guid)obj;
            Guid value = Guid.Empty;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    GuidTryParse((String)obj, out value);
                }
            }
            return value;
        }

        public virtual short ConvertToInt16(object obj)
        {
            if (obj is Int16) return (Int16)obj;
            short value = Int16.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    Int16.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToInt16((IConvertible)value);
                }
            }
            return value;
        }

        public virtual int ConvertToInt32(object obj)
        {
            if (obj is Int32) return (Int32)obj;
            int value = Int32.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    Int32.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToInt32((IConvertible)value);
                }
            }
            return value;
        }

        public virtual long ConvertToInt64(object obj)
        {
            if (obj is Int64) return (Int64)obj;
            long value = Int64.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    Int64.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToInt64((IConvertible)value);
                }
            }
            return value;
        }

        public virtual sbyte ConvertToSByte(object obj)
        {
            if (obj is SByte) return (SByte)obj;
            sbyte value = SByte.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    SByte.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToSByte((IConvertible)value);
                }
            }
            return value;
        }

        public virtual float ConvertToSingle(object obj)
        {
            if (obj is Single) return (Single)obj;
            float value = Single.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    Single.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToSingle((IConvertible)value);
                }
            }
            return value;
        }

        public virtual ushort ConvertToUInt16(object obj)
        {
            if (obj is UInt16) return (UInt16)obj;
            ushort value = UInt16.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    UInt16.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToUInt16((IConvertible)value);
                }
            }
            return value;
        }

        public virtual uint ConvertToUInt32(object obj)
        {
            if (obj is UInt32) return (UInt32)obj;
            uint value = UInt32.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    UInt32.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToUInt32((IConvertible)value);
                }
            }
            return value;
        }

        public virtual ulong ConvertToUInt64(object obj)
        {
            if (obj is UInt64) return (UInt64)obj;
            ulong value = UInt64.MinValue;
            if ((Object)obj != null)
            {
                if (obj is String)
                {
                    UInt64.TryParse((String)obj, out value);
                }
                else if (obj is IConvertible)
                {
                    value = Convert.ToUInt64((IConvertible)value);
                }
            }
            return value;
        }

        public virtual bool? ConvertToNullableBoolean(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is Boolean?) return (Boolean?)obj;
            return ConvertToBoolean(obj);
        }

        public virtual byte? ConvertToNullableByte(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is Byte?) return (Byte?)obj;
            return ConvertToByte(obj);
        }

        public virtual char? ConvertToNullableChar(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is Char?) return (Char?)obj;
            return ConvertToChar(obj);
        }

        public virtual DateTime? ConvertToNullableDateTime(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is DateTime?) return (DateTime?)obj;
            return ConvertToDateTime(obj);
        }

        public virtual decimal? ConvertToNullableDecimal(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is Decimal?) return (Decimal?)obj;
            return ConvertToDecimal(obj);
        }

        public virtual double? ConvertToNullableDouble(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is Double?) return (Double?)obj;
            return ConvertToDouble(obj);
        }

        public virtual Guid? ConvertToNullableGuid(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is Guid?) return (Guid?)obj;
            return ConvertToGuid(obj);
        }

        public virtual short? ConvertToNullableInt16(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is Int16?) return (Int16?)obj;
            return ConvertToInt16(obj);
        }

        public virtual int? ConvertToNullableInt32(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is Int32?) return (Int32?)obj;
            return ConvertToInt32(obj);
        }

        public virtual long? ConvertToNullableInt64(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is Int64?) return (Int64?)obj;
            return ConvertToInt64(obj);
        }

        public virtual sbyte? ConvertToNullableSByte(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is SByte?) return (SByte?)obj;
            return ConvertToSByte(obj);
        }

        public virtual float? ConvertToNullableSingle(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is Single?) return (Single?)obj;
            return ConvertToSingle(obj);
        }

        public virtual ushort? ConvertToNullableUInt16(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is UInt16?) return (UInt16?)obj;
            return ConvertToUInt16(obj);
        }

        public virtual uint? ConvertToNullableUInt32(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is UInt32?) return (UInt32?)obj;
            return ConvertToUInt32(obj);
        }

        public virtual ulong? ConvertToNullableUInt64(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is UInt64?) return (UInt64?)obj;
            return ConvertToUInt64(obj);
        }

        public virtual string ConvertToString(object obj)
        {
            if ((Object)obj == null) return null;
            if (obj is String) return (String)obj;
            return obj.ToString();
        }

        public virtual object ConvertTo(object obj, Type type)
        {
            if (Type.Equals(type, typeof(Object)) && (Object)obj != null)
            {
                return new Object();
            }
            if (Type.Equals(type, typeof(String)))
            {
                return ConvertToString(obj);
            }
            if (Type.Equals(type, typeof(Boolean)))
            {
                return ConvertToBoolean(obj);
            }
            if (Type.Equals(type, typeof(Boolean?)))
            {
                return ConvertToNullableBoolean(obj);
            }
            if (Type.Equals(type, typeof(Byte)))
            {
                return ConvertToByte(obj);
            }
            if (Type.Equals(type, typeof(Byte?)))
            {
                return ConvertToNullableByte(obj);
            }
            if (Type.Equals(type, typeof(Char)))
            {
                return ConvertToChar(obj);
            }
            if (Type.Equals(type, typeof(Char?)))
            {
                return ConvertToNullableChar(obj);
            }
            if (Type.Equals(type, typeof(DateTime)))
            {
                return ConvertToDateTime(obj);
            }
            if (Type.Equals(type, typeof(DateTime?)))
            {
                return ConvertToNullableDateTime(obj);
            }
            if (Type.Equals(type, typeof(Decimal)))
            {
                return ConvertToDecimal(obj);
            }
            if (Type.Equals(type, typeof(Decimal?)))
            {
                return ConvertToNullableDecimal(obj);
            }
            if (Type.Equals(type, typeof(Double)))
            {
                return ConvertToDouble(obj);
            }
            if (Type.Equals(type, typeof(Double?)))
            {
                return ConvertToNullableDouble(obj);
            }
            if (Type.Equals(type, typeof(Guid)))
            {
                return ConvertToGuid(obj);
            }
            if (Type.Equals(type, typeof(Guid?)))
            {
                return ConvertToNullableGuid(obj);
            }
            if (Type.Equals(type, typeof(Int16)))
            {
                return ConvertToInt16(obj);
            }
            if (Type.Equals(type, typeof(Int16?)))
            {
                return ConvertToNullableInt16(obj);
            }
            if (Type.Equals(type, typeof(Int32)))
            {
                return ConvertToInt32(obj);
            }
            if (Type.Equals(type, typeof(Int32?)))
            {
                return ConvertToNullableInt32(obj);
            }
            if (Type.Equals(type, typeof(Int64)))
            {
                return ConvertToInt64(obj);
            }
            if (Type.Equals(type, typeof(Int64?)))
            {
                return ConvertToNullableInt64(obj);
            }
            if (Type.Equals(type, typeof(SByte)))
            {
                return ConvertToNullableSByte(obj);
            }
            if (Type.Equals(type, typeof(SByte?)))
            {
                return ConvertToSByte(obj);
            }
            if (Type.Equals(type, typeof(Single)))
            {
                return ConvertToSingle(obj);
            }
            if (Type.Equals(type, typeof(Single?)))
            {
                return ConvertToNullableSingle(obj);
            }
            if (Type.Equals(type, typeof(UInt16)))
            {
                return ConvertToUInt16(obj);
            }
            if (Type.Equals(type, typeof(UInt16?)))
            {
                return ConvertToNullableUInt16(obj);
            }
            if (Type.Equals(type, typeof(UInt32)))
            {
                return ConvertToUInt32(obj);
            }
            if (Type.Equals(type, typeof(UInt32?)))
            {
                return ConvertToNullableUInt32(obj);
            }
            if (Type.Equals(type, typeof(UInt64)))
            {
                return ConvertToNullableUInt64(obj);
            }
            if (Type.Equals(type, typeof(UInt64?)))
            {
                return ConvertToNullableUInt64(obj);
            }
            if ((Object)obj == null)
            {
                return null;
            }
            throw new NotSupportedException(Resources.TypeConversionNotSupported);
        }

        private static bool GuidTryParse(string input, out Guid guid)
        {
            if (Regex.IsMatch(input, "^[A-Fa-f0-9]{32}$|^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$"))
            {
                guid = new Guid(input);
                return true;
            }
            else
            {
                guid = Guid.Empty;
                return false;
            }
        }
    }
}
