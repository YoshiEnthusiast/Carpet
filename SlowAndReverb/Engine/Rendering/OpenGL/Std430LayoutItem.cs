using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace SlowAndReverb
{
    public struct Std430LayoutItem
    {
        private Std430LayoutItem(int baseAlignment, int alignedOffset)
        {
            BaseAlignment = baseAlignment;
            AlignedOffset = alignedOffset;
        }

        private Std430LayoutItem(int baseAlignment)
        {
            this = new Std430LayoutItem(baseAlignment, baseAlignment);
        }

        public int BaseAlignment { get; private init; }
        public int AlignedOffset { get; private init; }

        public static Std430LayoutItem Scalar<T>() where T : struct
        {
            return new Std430LayoutItem(SizeOf<T>());
        }

        public static Std430LayoutItem Vector2()
        {
            return new Std430LayoutItem(SizeOf<Vector2>());
        }

        public static Std430LayoutItem Vector3()
        {
            return new Std430LayoutItem(SizeOf<Vector4>());
        }

        public static Std430LayoutItem Vector4()
        {
            return new Std430LayoutItem(SizeOf<Vector4>());
        }

        public static Std430LayoutItem Struct<T, TLargestMember>() 
            where T : struct
            where TLargestMember : struct
        {
            int structSize = SizeOf<T>();
            int largestMemberSize = SizeOf<TLargestMember>();

            structSize = Maths.RoundUp(structSize, largestMemberSize);

            return new Std430LayoutItem(structSize, largestMemberSize);
        }

        public static Std430LayoutItem Array<T>(int length) where T : struct
        {
            int elementSize = SizeOf<T>();
            int size = elementSize * length;

            return new Std430LayoutItem(size, elementSize);
        }

        public static Std430LayoutItem StructArray<T, TLargestMember>(int length)
            where T : struct
            where TLargestMember : struct
        {
            int structSize = SizeOf<T>();
            int largestMemberSize = SizeOf<TLargestMember>();

            structSize = Maths.RoundUp(structSize, largestMemberSize);
            int size = structSize * length;

            return new Std430LayoutItem(size, largestMemberSize);
        }

        // Matrices

        private static int SizeOf<T>() where T : struct
        {
            return Unsafe.SizeOf<T>();
        }
    }
}
