﻿using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BEPUutilities2
{
    /// <summary>
    /// Provides SIMD-aware 4x4 matrix math.
    /// </summary>
    /// <remarks>
    /// All functions assume row vectors.
    /// </remarks>
    public struct Matrix
    {
        /// <summary>
        /// Row 1 of the matrix.
        /// </summary>
        public Vector4 X;
        /// <summary>
        /// Row 2 of the matrix.
        /// </summary>
        public Vector4 Y;
        /// <summary>
        /// Row 3 of the matrix.
        /// </summary>
        public Vector4 Z;
        /// <summary>
        /// Row 4 of the matrix.
        /// </summary>
        public Vector4 W;

        public static Matrix Identity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Matrix result;
                result.X = new Vector4(1, 0, 0, 0);
                result.Y = new Vector4(0, 1, 0, 0);
                result.Z = new Vector4(0, 0, 1, 0);
                result.W = new Vector4(0, 0, 0, 1);
                return result;
            }
        }



        public Vector3 Translation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return new Vector3(W.X, W.Y, W.Z);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                W = new Vector4(value, W.W);
            }
        }

        struct M
        {
            public float M11, M12, M13, M14;
            public float M21, M22, M23, M24;
            public float M31, M32, M33, M34;
            public float M41, M42, M43, M44;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static void Transpose(M* m, M* transposed)
        {
            //A weird function! Why?
            //1) Missing some helpful instructions for actual SIMD accelerated transposition.
            //2) Difficult to get SIMD types to generate competitive codegen due to lots of componentwise access.

            float m12 = m->M12;
            float m13 = m->M13;
            float m14 = m->M14;
            float m23 = m->M23;
            float m24 = m->M24;
            float m34 = m->M34;
            transposed->M11 = m->M11;
            transposed->M12 = m->M21;
            transposed->M13 = m->M31;
            transposed->M14 = m->M41;

            transposed->M21 = m12;
            transposed->M22 = m->M22;
            transposed->M23 = m->M32;
            transposed->M24 = m->M42;

            transposed->M31 = m13;
            transposed->M32 = m23;
            transposed->M33 = m->M33;
            transposed->M34 = m->M43;

            transposed->M41 = m14;
            transposed->M42 = m24;
            transposed->M43 = m34;
            transposed->M44 = m->M44;

        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Transpose(Matrix* m, Matrix* transposed)
        {
            Transpose((M*)m, (M*)transposed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transpose(ref Matrix m, out Matrix transposed)
        {
            //Not an ideal implementation. Shuffles would be handy.

            var xy = m.X.Y;
            var xz = m.X.Z;
            var xw = m.X.W;
            var yz = m.Y.Z;
            var yw = m.Y.W;
            var zw = m.Z.W;
            transposed.X = new Vector4(m.X.X, m.Y.X, m.Z.X, m.W.X);
            transposed.Y = new Vector4(xy, m.Y.Y, m.Z.Y, m.W.Y);
            transposed.Z = new Vector4(xz, yz, m.Z.Z, m.W.Z);
            transposed.W = new Vector4(xw, yw, zw, m.W.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Matrix Transpose(Matrix m)
        {
            Matrix transposed;
            Transpose(&m, &transposed);
            return transposed;
        }

        /// <summary>
        /// Transforms a vector with a transposed matrix.
        /// </summary>
        /// <param name="v">Row vector to transform.</param>
        /// <param name="m">Matrix whose transpose will be applied to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TransformTranspose(ref Vector4 v, ref Matrix m, out Vector4 result)
        {
            result = new Vector4(
                Vector4.Dot(v, m.X),
                Vector4.Dot(v, m.Y),
                Vector4.Dot(v, m.Z),
                Vector4.Dot(v, m.W));
        }

        /// <summary>
        /// Transforms a vector with a matrix.
        /// </summary>
        /// <param name="v">Row vector to transform.</param>
        /// <param name="m">Matrix to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(ref Vector4 v, ref Matrix m, out Vector4 result)
        {
            var x = new Vector4(v.X);
            var y = new Vector4(v.Y);
            var z = new Vector4(v.Z);
            var w = new Vector4(v.W);
            result = m.X * x + m.Y * y + m.Z * z + m.W * w;
        }



        /// <summary>
        /// Multiplies a matrix by another matrix.
        /// </summary>
        /// <param name="a">First matrix.</param>
        /// <param name="b">Second matrix.</param>
        /// <param name="result">Result of the matrix multiplication.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(ref Matrix a, ref Matrix b, out Matrix result)
        {
            var bX = b.X;
            var bY = b.Y;
            var bZ = b.Z;
            {
                var x = new Vector4(a.X.X);
                var y = new Vector4(a.X.Y);
                var z = new Vector4(a.X.Z);
                var w = new Vector4(a.X.W);
                result.X = (x * bX + y * bY) + (z * bZ + w * b.W);
            }

            {
                var x = new Vector4(a.Y.X);
                var y = new Vector4(a.Y.Y);
                var z = new Vector4(a.Y.Z);
                var w = new Vector4(a.Y.W);
                result.Y = (x * bX + y * bY) + (z * bZ + w * b.W);
            }

            {
                var x = new Vector4(a.Z.X);
                var y = new Vector4(a.Z.Y);
                var z = new Vector4(a.Z.Z);
                var w = new Vector4(a.Z.W);
                result.Z = (x * bX + y * bY) + (z * bZ + w * b.W);
            }

            {
                var x = new Vector4(a.W.X);
                var y = new Vector4(a.W.Y);
                var z = new Vector4(a.W.Z);
                var w = new Vector4(a.W.W);
                result.W = (x * bX + y * bY) + (z * bZ + w * b.W);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Matrix result)
        {
            //TODO: Could be better simdified.
            float xx = axis.X * axis.X;
            float yy = axis.Y * axis.Y;
            float zz = axis.Z * axis.Z;
            float xy = axis.X * axis.Y;
            float xz = axis.X * axis.Z;
            float yz = axis.Y * axis.Z;

            float sinAngle = (float)Math.Sin(angle);
            float oneMinusCosAngle = 1 - (float)Math.Cos(angle);

            result.X = new Vector4(
                1 + oneMinusCosAngle * (xx - 1),
                axis.Z * sinAngle + oneMinusCosAngle * xy,
                -axis.Y * sinAngle + oneMinusCosAngle * xz,
                0);

            result.Y = new Vector4(
                -axis.Z * sinAngle + oneMinusCosAngle * xy,
                1 + oneMinusCosAngle * (yy - 1),
                axis.X * sinAngle + oneMinusCosAngle * yz,
                0);

            result.Z = new Vector4(
                axis.Y * sinAngle + oneMinusCosAngle * xz,
                -axis.X * sinAngle + oneMinusCosAngle * yz,
                1 + oneMinusCosAngle * (zz - 1),
                0);

            result.W = new Vector4(0, 0, 0, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
        {
            Matrix result;
            CreateFromAxisAngle(ref axis, angle, out result);
            return result;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix result)
        {
            float qX2 = quaternion.X + quaternion.X;
            float qY2 = quaternion.Y + quaternion.Y;
            float qZ2 = quaternion.Z + quaternion.Z;
            float XX = qX2 * quaternion.X;
            float YY = qY2 * quaternion.Y;
            float ZZ = qZ2 * quaternion.Z;
            float XY = qX2 * quaternion.Y;
            float XZ = qX2 * quaternion.Z;
            float XW = qX2 * quaternion.W;
            float YZ = qY2 * quaternion.Z;
            float YW = qY2 * quaternion.W;
            float ZW = qZ2 * quaternion.W;

            result.X = new Vector4(
                1 - YY - ZZ,
                XY + ZW,
                XZ - YW,
                0);

            result.Y = new Vector4(
                XY - ZW,
                1 - XX - ZZ,
                YZ + XW,
                0);

            result.Z = new Vector4(
                XZ + YW,
                YZ - XW,
                1 - XX - YY,
                0);

            result.W = new Vector4(
                0,
                0,
                0,
                1);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix CreateFromQuaternion(Quaternion quaternion)
        {
            Matrix toReturn;
            CreateFromQuaternion(ref quaternion, out toReturn);
            return toReturn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            Matrix toReturn;
            Multiply(ref m1, ref m2, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Creates a right-handed perspective matrix.
        /// </summary>
        /// <param name="fieldOfView">Vertical field of view of the perspective in radians.</param>
        /// <param name="aspectRatio">Width of the viewport over the height of the viewport.</param>
        /// <param name="nearClip">Near clip plane of the perspective.</param>
        /// <param name="farClip">Far clip plane of the perspective.</param>
        /// <param name="perspective">Resulting perspective matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearClip, float farClip, out Matrix perspective)
        {
            float h = 1f / ((float)Math.Tan(fieldOfView * 0.5f));
            float w = h / aspectRatio;
            float m33 = farClip / (nearClip - farClip);
            perspective.X = new Vector4(w, 0, 0, 0);
            perspective.Y = new Vector4(0, h, 0, 0);
            perspective.Z = new Vector4(0, 0, m33, -1);
            perspective.W = new Vector4(0, 0, nearClip * m33, 0);

        }

        /// <summary>
        /// Creates a right-handed perspective matrix.
        /// </summary>
        /// <param name="verticalFieldOfView">Vertical field of view of the perspective in radians.</param>
        /// <param name="horizontalFieldOfView">Horizontal field of view of the perspective in radians.</param>
        /// <param name="nearClip">Near clip plane of the perspective.</param>
        /// <param name="farClip">Far clip plane of the perspective.</param>
        /// <returns>Resulting perspective matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix CreatePerspectiveFromFieldOfViews(float verticalFieldOfView, float horizontalFieldOfView, float nearClip, float farClip)
        {
            Matrix perspective;
            CreatePerspectiveFromFieldOfViews(verticalFieldOfView, horizontalFieldOfView, nearClip, farClip, out perspective);
            return perspective;
        }


        /// <summary>
        /// Creates a right-handed perspective matrix.
        /// </summary>
        /// <param name="verticalFieldOfView">Vertical field of view of the perspective in radians.</param>
        /// <param name="horizontalFieldOfView">Horizontal field of view of the perspective in radians.</param>
        /// <param name="nearClip">Near clip plane of the perspective.</param>
        /// <param name="farClip">Far clip plane of the perspective.</param>
        /// <param name="perspective">Resulting perspective matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreatePerspectiveFromFieldOfViews(float verticalFieldOfView, float horizontalFieldOfView, float nearClip, float farClip, out Matrix perspective)
        {
            float h = 1f / ((float)Math.Tan(verticalFieldOfView * 0.5f));
            float w = 1f / ((float)Math.Tan(horizontalFieldOfView * 0.5f));
            float m33 = farClip / (nearClip - farClip);
            perspective.X = new Vector4(w, 0, 0, 0);
            perspective.Y = new Vector4(0, h, 0, 0);
            perspective.Z = new Vector4(0, 0, m33, -1);
            perspective.W = new Vector4(0, 0, nearClip * m33, 0);
        }


        /// <summary>
        /// Creates a right-handed perspective matrix.
        /// </summary>
        /// <param name="fieldOfView">Vertical field of view of the perspective in radians.</param>
        /// <param name="aspectRatio">Width of the viewport over the height of the viewport.</param>
        /// <param name="nearClip">Near clip plane of the perspective.</param>
        /// <param name="farClip">Far clip plane of the perspective.</param>
        /// <returns>Resulting perspective matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearClip, float farClip)
        {
            Matrix toReturn;
            CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClip, farClip, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Creates a right handed orthographic projection.
        /// </summary>
        /// <param name="left">Leftmost coordinate of the projected area.</param>
        /// <param name="right">Rightmost coordinate of the projected area.</param>
        /// <param name="bottom">Bottom coordinate of the projected area.</param>
        /// <param name="top">Top coordinate of the projected area.</param>
        /// <param name="zNear">Near plane of the projection.</param>
        /// <param name="zFar">Far plane of the projection.</param>
        /// <param name="projection">The resulting orthographic projection matrix.</param>
        public static void CreateOrthographic(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix projection)
        {
            float width = right - left;
            float height = top - bottom;
            float depth = zFar - zNear;
            projection.X = new Vector4(2f / width, 0, 0, 0);
            projection.Y = new Vector4(0, 2f / height, 0, 0);
            projection.Z = new Vector4(0, 0, -1f / depth, 0);
            projection.W = new Vector4((left + right) / -width, (top + bottom) / -height, zNear / -depth, 1f);

        }


        /// <summary>
        /// Inverts the matrix.
        /// </summary>
        /// <param name="m">Matrix to invert.</param>
        /// <param name="inverted">Inverted version of the matrix.</param>
        public static void Invert(ref Matrix m, out Matrix inverted)
        {
            //TODO: This could be quite a bit faster, especially once shuffles exist... But inverting a 4x4 matrix should approximately never occur.
            float s0 = m.X.X * m.Y.Y - m.Y.X * m.X.Y;
            float s1 = m.X.X * m.Y.Z - m.Y.X * m.X.Z;
            float s2 = m.X.X * m.Y.W - m.Y.X * m.X.W;
            float s3 = m.X.Y * m.Y.Z - m.Y.Y * m.X.Z;
            float s4 = m.X.Y * m.Y.W - m.Y.Y * m.X.W;
            float s5 = m.X.Z * m.Y.W - m.Y.Z * m.X.W;

            float c5 = m.Z.Z * m.W.W - m.W.Z * m.Z.W;
            float c4 = m.Z.Y * m.W.W - m.W.Y * m.Z.W;
            float c3 = m.Z.Y * m.W.Z - m.W.Y * m.Z.Z;
            float c2 = m.Z.X * m.W.W - m.W.X * m.Z.W;
            float c1 = m.Z.X * m.W.Z - m.W.X * m.Z.Z;
            float c0 = m.Z.X * m.W.Y - m.W.X * m.Z.Y;

            float inverseDeterminant = 1.0f / (s0 * c5 - s1 * c4 + s2 * c3 + s3 * c2 - s4 * c1 + s5 * c0);

            float m11 = m.X.X;
            float m12 = m.X.Y;
            float m13 = m.X.Z;
            float m14 = m.X.W;
            float m21 = m.Y.X;
            float m22 = m.Y.Y;
            float m23 = m.Y.Z;
            float m31 = m.Z.X;
            float m32 = m.Z.Y;
            float m33 = m.Z.Z;

            float m41 = m.W.X;
            float m42 = m.W.Y;

            inverted.X = new Vector4(
                (m.Y.Y * c5 - m.Y.Z * c4 + m.Y.W * c3) * inverseDeterminant,
                (-m.X.Y * c5 + m.X.Z * c4 - m.X.W * c3) * inverseDeterminant,
                (m.W.Y * s5 - m.W.Z * s4 + m.W.W * s3) * inverseDeterminant,
                (-m.Z.Y * s5 + m.Z.Z * s4 - m.Z.W * s3) * inverseDeterminant);

            inverted.Y = new Vector4(
                (-m.Y.X * c5 + m.Y.Z * c2 - m.Y.W * c1) * inverseDeterminant,
                (m11 * c5 - m13 * c2 + m14 * c1) * inverseDeterminant,
                (-m.W.X * s5 + m.W.Z * s2 - m.W.W * s1) * inverseDeterminant,
                (m.Z.X * s5 - m.Z.Z * s2 + m.Z.W * s1) * inverseDeterminant);

            inverted.Z = new Vector4(
                (m21 * c4 - m22 * c2 + m.Y.W * c0) * inverseDeterminant,
                (-m11 * c4 + m12 * c2 - m14 * c0) * inverseDeterminant,
                (m.W.X * s4 - m.W.Y * s2 + m.W.W * s0) * inverseDeterminant,
                (-m31 * s4 + m32 * s2 - m.Z.W * s0) * inverseDeterminant);

            inverted.W = new Vector4(
                (-m21 * c3 + m22 * c1 - m23 * c0) * inverseDeterminant,
                (m11 * c3 - m12 * c1 + m13 * c0) * inverseDeterminant,
                (-m41 * s3 + m42 * s1 - m.W.Z * s0) * inverseDeterminant,
                (m31 * s3 - m32 * s1 + m33 * s0) * inverseDeterminant);
        }

        /// <summary>
        /// Inverts the matrix.
        /// </summary>
        /// <param name="m">Matrix to invert.</param>
        /// <returns>Inverted version of the matrix.</returns>
        public static Matrix Invert(ref Matrix m)
        {
            Matrix inverted;
            Invert(ref m, out inverted);
            return inverted;
        }

    }
}
