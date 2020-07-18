using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Crucis.Protocol
{
    public static class NetHelper
    {
        public static Vector3 Vec3ToPxVec3(Vec3 vec3)
        {
            Vector3 pxvec3 = Vector3.zero;
            pxvec3.x = vec3.X;
            pxvec3.y = vec3.Y;
            pxvec3.z = vec3.Z;
            return pxvec3;
        }

        public static Vec3 PxVec3ToVec3(Vector3 pxvec3)
        {
            Vec3 vec3 = new Vec3();
            vec3.X = pxvec3.x;
            vec3.Y = pxvec3.y;
            vec3.Z = pxvec3.z;
            return vec3;
        }

        public static Quaternion Vec4ToPxQuat(Vec4 vec4)
        {
            Quaternion pxquat = new Quaternion();
            pxquat.x = vec4.X;
            pxquat.y = vec4.Y;
            pxquat.z = vec4.Z;
            pxquat.w = vec4.W;
            return pxquat;
        }

        public static Vec4 PxQuatToVec4(Quaternion pxquat)
        {
            Vec4 vec4 = new Vec4();
            vec4.X = pxquat.x;
            vec4.Y = pxquat.y;
            vec4.Z = pxquat.z;
            vec4.W = pxquat.w;
            return vec4;
        }
    }

    public partial class Vec3
    {
        public static implicit operator Vector3(Vec3 vec3)
        {
            if (vec3 == null)
            {
                return Vector3.zero;
            }
            return new Vector3(vec3.X, vec3.Y, vec3.Z);
        }
        public static implicit operator Vec3(Vector3 vector3)
        {
            return new Vec3() { X = vector3.x, Y = vector3.y, Z = vector3.z };
        }
    }

    public partial class Vec4
    {
        public static implicit operator Quaternion(Vec4 vec4)
        {
            if (vec4 == null)
            {
                return Quaternion.identity;
            }
            return new Quaternion(vec4.X, vec4.Y, vec4.Z, vec4.W);
        }
        public static implicit operator Vec4(Quaternion quaternion)
        {
            return new Vec4() { X = quaternion.x, Y = quaternion.y, Z = quaternion.z, W = quaternion.w };
        }
    }
}
