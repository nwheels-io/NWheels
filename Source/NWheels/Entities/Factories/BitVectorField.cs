using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil.Operands;
using Hapil.Writers;

namespace NWheels.Entities.Factories
{
    public class BitVectorField
    {
        private readonly int _length;
        private readonly Field<long> _field;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BitVectorField(ClassWriterBase classWriter, string name, int length)
        {
            if ( length < 0 || length > 64 )
            {
                throw new ArgumentOutOfRangeException("length", "Length must be 0..64");
            }

            _field = classWriter.Field<long>(name);
            _length = length;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteSetBit(MethodWriterBase methodWriter, int index)
        {
            if ( index < 0 || index >= _length )
            {
                throw new ArgumentOutOfRangeException("index");
            }

            long mask = (long)1 << index;
            _field.Assign(_field | methodWriter.Const(mask));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteClearBit(MethodWriterBase methodWriter, int index)
        {
            if ( index < 0 || index >= _length )
            {
                throw new ArgumentOutOfRangeException("index");
            }

            long mask = (long)1 << index;
            _field.Assign(_field & ~methodWriter.Const(mask));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteClearAllBits(MethodWriterBase methodWriter)
        {
            _field.Assign(0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Operand<bool> WriteNonZeroBitTest(MethodWriterBase methodWriter, int index)
        {
            long mask = (long)1 << index;
            return ((_field & mask) != methodWriter.Const<long>(0));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Operand<bool> WriteNonZeroTest(MethodWriterBase methodWriter)
        {
            return (_field != methodWriter.Const<long>(0));
        }
    }
}
